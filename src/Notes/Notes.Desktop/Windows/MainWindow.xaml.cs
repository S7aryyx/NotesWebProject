using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.Models;

namespace WPF.Windows
{
    public partial class MainWindow : Window
    {
        private HttpClient httpClient = new HttpClient();
        private User currentUser;

        private List<Folder> folders = new List<Folder>();
        private List<Note> notes = new List<Note>();

        private Note currentNote;
        private Folder selectedFolder;

        private bool sidebarVisible = true;
        private bool aiPanelVisible = true;
        private bool loadingEditor = false;
        private bool hasAiAccess = false;

        private const double SidebarWidth = 280;
        private const double AiPanelWidth = 280;

        public MainWindow(User user)
        {
            InitializeComponent();

            currentUser = user;
            httpClient.BaseAddress = new Uri("https://localhost:7110/"); // LocalAI.Api
            httpClient.Timeout = TimeSpan.FromMinutes(3);

            UserLoginTextBlock.Text = currentUser.Login;

            if (string.IsNullOrWhiteSpace(currentUser.Login))
            {
                UserAvatarText.Text = "U";
            }
            else
            {
                UserAvatarText.Text = currentUser.Login.Substring(0, 1).ToUpper();
            }

            NoteTypeComboBox.SelectedIndex = 0;
            UpdateEditorPlaceholders();
            UpdateActions();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            await LoadAccessStatusAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                folders = await App.NotesDataService.GetFoldersAsync(currentUser.Id);
                notes = await App.NotesDataService.GetNotesAsync(currentUser.Id);

                RenderTree();

                if (currentNote != null)
                {
                    TreeViewItem item = FindTreeItem(currentNote.Id);

                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        ClearEditor();
                    }
                }

                int activeNotesCount = notes.Count(note => !note.IsArchive);
                NoteStatusTextBlock.Text = "Папок: " + folders.Count + " · Заметок: " + activeNotesCount;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить заметки: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            UpdateActions();
        }

        private void RenderTree()
        {
            NotesTreeView.Items.Clear();

            foreach (Folder folder in folders
                .Where(folder => folder.ParentFolderId == null)
                .OrderBy(folder => folder.Title))
            {
                NotesTreeView.Items.Add(CreateFolderItem(folder));
            }

            foreach (Note note in notes
                .Where(note => note.FolderId == null && !note.IsArchive)
                .OrderBy(note => note.Title))
            {
                NotesTreeView.Items.Add(CreateNoteItem(note));
            }

            List<Note> archivedNotes = notes
                .Where(note => note.IsArchive)
                .OrderBy(note => note.Title)
                .ToList();

            TreeViewItem archiveItem = new TreeViewItem();
            archiveItem.Tag = "archive";
            archiveItem.IsExpanded = true;
            archiveItem.Header = CreateTreeHeader("▣", "Архив (" + archivedNotes.Count + ")");

            foreach (Note note in archivedNotes)
            {
                archiveItem.Items.Add(CreateNoteItem(note));
            }

            NotesTreeView.Items.Add(archiveItem);
        }

        private TreeViewItem CreateFolderItem(Folder folder)
        {
            TreeViewItem item = new TreeViewItem();
            item.Tag = folder;
            item.IsExpanded = true;
            item.Header = CreateTreeHeader("📁", folder.Title);
            item.ToolTip = folder.Title;

            foreach (Folder childFolder in folders
                .Where(child => child.ParentFolderId == folder.Id)
                .OrderBy(child => child.Title))
            {
                item.Items.Add(CreateFolderItem(childFolder));
            }

            foreach (Note note in notes
                .Where(note => note.FolderId == folder.Id && !note.IsArchive)
                .OrderBy(note => note.Title))
            {
                item.Items.Add(CreateNoteItem(note));
            }

            return item;
        }

        private TreeViewItem CreateNoteItem(Note note)
        {
            string icon = "▤";

            if (note.IsArchive)
            {
                icon = "↶";
            }
            else if (note.IsFavorite)
            {
                icon = "★";
            }

            TreeViewItem item = new TreeViewItem();
            item.Tag = note;
            item.Header = CreateTreeHeader(icon, note.Title);
            item.ToolTip = note.Title;

            return item;
        }

        private static Grid CreateTreeHeader(string icon, string title)
        {
            Grid header = new Grid();
            header.MinWidth = 0;

            ColumnDefinition iconColumn = new ColumnDefinition();
            iconColumn.Width = new GridLength(22);

            ColumnDefinition titleColumn = new ColumnDefinition();
            titleColumn.Width = new GridLength(1, GridUnitType.Star);

            header.ColumnDefinitions.Add(iconColumn);
            header.ColumnDefinitions.Add(titleColumn);

            TextBlock iconText = new TextBlock();
            iconText.Text = icon;
            iconText.Foreground = new SolidColorBrush(Color.FromRgb(160, 157, 190));
            iconText.VerticalAlignment = VerticalAlignment.Center;
            iconText.FontSize = 12;

            TextBlock nameText = new TextBlock();
            nameText.Text = title;
            nameText.Foreground = new SolidColorBrush(Color.FromRgb(217, 218, 224));
            nameText.VerticalAlignment = VerticalAlignment.Center;
            nameText.TextTrimming = TextTrimming.CharacterEllipsis;
            nameText.MaxWidth = 200;

            Grid.SetColumn(nameText, 1);

            header.Children.Add(iconText);
            header.Children.Add(nameText);

            return header;
        }

        private TreeViewItem FindTreeItem(Guid id)
        {
            foreach (object value in NotesTreeView.Items)
            {
                TreeViewItem item = value as TreeViewItem;

                if (item != null)
                {
                    TreeViewItem foundItem = FindTreeItem(item, id);

                    if (foundItem != null)
                    {
                        return foundItem;
                    }
                }
            }

            return null;
        }

        private static TreeViewItem FindTreeItem(TreeViewItem item, Guid id)
        {
            Note note = item.Tag as Note;
            Folder folder = item.Tag as Folder;

            if (note != null && note.Id == id)
            {
                return item;
            }

            if (folder != null && folder.Id == id)
            {
                return item;
            }

            foreach (object value in item.Items)
            {
                TreeViewItem childItem = value as TreeViewItem;

                if (childItem != null)
                {
                    TreeViewItem foundItem = FindTreeItem(childItem, id);

                    if (foundItem != null)
                    {
                        return foundItem;
                    }
                }
            }

            return null;
        }

        private void NotesTreeView_PreviewMouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;

            while (source != null && !(source is TreeViewItem) && source != NotesTreeView)
            {
                source = VisualTreeHelper.GetParent(source);
            }

            if (source is TreeViewItem || source != NotesTreeView)
            {
                return;
            }

            foreach (object value in NotesTreeView.Items)
            {
                TreeViewItem item = value as TreeViewItem;

                if (item != null)
                {
                    ClearTreeSelection(item);
                }
            }

            currentNote = null;
            selectedFolder = null;
            ClearEditor();
        }

        private static void ClearTreeSelection(TreeViewItem item)
        {
            item.IsSelected = false;

            foreach (object value in item.Items)
            {
                TreeViewItem child = value as TreeViewItem;

                if (child != null)
                {
                    ClearTreeSelection(child);
                }
            }
        }

        private void NoteTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEditorPlaceholders();
        }

        private void NoteContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEditorPlaceholders();
        }

        private void UpdateEditorPlaceholders()
        {
            if (NoteTitlePlaceholder != null)
            {
                if (string.IsNullOrEmpty(NoteTitleTextBox.Text))
                {
                    NoteTitlePlaceholder.Visibility = Visibility.Visible;
                }
                else
                {
                    NoteTitlePlaceholder.Visibility = Visibility.Collapsed;
                }
            }

            if (NoteContentPlaceholder != null)
            {
                if (string.IsNullOrEmpty(NoteContentTextBox.Text))
                {
                    NoteContentPlaceholder.Visibility = Visibility.Visible;
                }
                else
                {
                    NoteContentPlaceholder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void NotesTreeView_SelectedItemChanged(
            object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = e.NewValue as TreeViewItem;

            if (item == null)
            {
                return;
            }

            string view = item.Tag as string;

            if (view == "archive")
            {
                currentNote = null;
                selectedFolder = null;
                loadingEditor = true;

                NoteTitleHeader.Text = "Архив";
                NoteTitleTextBox.Text = "";
                NoteContentTextBox.Text = "";
                NoteTypeComboBox.SelectedIndex = 0;
                NoteStatusTextBlock.Text = "Архивные заметки. Выберите заметку, чтобы восстановить её.";

                loadingEditor = false;
            }
            else
            {
                Note note = item.Tag as Note;
                Folder folder = item.Tag as Folder;

                if (note != null)
                {
                    currentNote = note;
                    selectedFolder = folders.FirstOrDefault(value => value.Id == note.FolderId);
                    ShowNote(note);
                }
                else if (folder != null)
                {
                    currentNote = null;
                    selectedFolder = folder;
                    loadingEditor = true;

                    NoteTitleHeader.Text = folder.Title;
                    NoteTitleTextBox.Text = "";
                    NoteContentTextBox.Text = "";
                    NoteTypeComboBox.SelectedIndex = 0;
                    NoteStatusTextBlock.Text = "Папка выбрана. Новая заметка будет создана внутри неё.";

                    loadingEditor = false;
                }
            }

            UpdateActions();
        }

        private void ShowNote(Note note)
        {
            loadingEditor = true;

            NoteTitleHeader.Text = note.Title;
            NoteTitleTextBox.Text = note.Title;
            NoteContentTextBox.Text = note.Content;

            SelectNoteType(note.NoteType);

            if (note.IsArchive)
            {
                NoteStatusTextBlock.Text = "Заметка в архиве";
            }
            else
            {
                NoteStatusTextBlock.Text = "Изменено: " + note.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            }

            if (note.IsFavorite)
            {
                FavoriteButton.Content = "★";
                FavoriteButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 209, 102));
            }
            else
            {
                FavoriteButton.Content = "☆";
                FavoriteButton.Foreground = new SolidColorBrush(Color.FromRgb(180, 182, 192));
            }

            if (note.IsArchive)
            {
                ArchiveButton.Content = "Восстановить";
            }
            else
            {
                ArchiveButton.Content = "Архив";
            }

            loadingEditor = false;
        }

        private void SelectNoteType(string type)
        {
            string value = "personal";

            if (!string.IsNullOrWhiteSpace(type))
            {
                value = type.ToLowerInvariant();
            }

            foreach (ComboBoxItem item in NoteTypeComboBox.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == value)
                {
                    NoteTypeComboBox.SelectedItem = item;
                    return;
                }
            }

            NoteTypeComboBox.SelectedIndex = 0;
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            currentNote = null;
            selectedFolder = null;

            TreeViewItem selectedItem = NotesTreeView.SelectedItem as TreeViewItem;

            if (selectedItem != null)
            {
                Folder folder = selectedItem.Tag as Folder;
                Note note = selectedItem.Tag as Note;

                if (folder != null)
                {
                    selectedFolder = folder;
                }
                else if (note != null)
                {
                    selectedFolder = folders.FirstOrDefault(value => value.Id == note.FolderId);
                }
            }

            loadingEditor = true;

            NoteTitleHeader.Text = "Новая заметка";
            NoteTitleTextBox.Text = "";
            NoteContentTextBox.Text = "";
            NoteTypeComboBox.SelectedIndex = 0;

            if (selectedFolder == null)
            {
                NoteStatusTextBlock.Text = "Новая заметка · Без папки";
            }
            else
            {
                NoteStatusTextBlock.Text = "Новая заметка · " + selectedFolder.Title;
            }

            FavoriteButton.Content = "☆";
            FavoriteButton.Foreground = new SolidColorBrush(Color.FromRgb(180, 182, 192));
            ArchiveButton.Content = "Архив";

            loadingEditor = false;

            UpdateActions();
            NoteTitleTextBox.Focus();
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string title = PromptFolderName();

            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            Folder parentFolder = null;
            TreeViewItem selectedItem = NotesTreeView.SelectedItem as TreeViewItem;

            if (selectedItem != null)
            {
                parentFolder = selectedItem.Tag as Folder;
            }

            try
            {
                Folder folder = await App.NotesDataService.CreateFolderAsync(
                    currentUser.Id,
                    title.Trim(),
                    parentFolder == null ? (Guid?)null : parentFolder.Id);

                await LoadDataAsync();

                TreeViewItem createdItem = FindTreeItem(folder.Id);

                if (createdItem != null)
                {
                    createdItem.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось создать папку: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string PromptFolderName()
        {
            Window dialog = new Window();
            dialog.Title = "Новая папка";
            dialog.Width = 360;
            dialog.Height = 165;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.Background = new SolidColorBrush(Color.FromRgb(24, 26, 32));
            dialog.Foreground = Brushes.White;

            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(16);

            TextBox input = new TextBox();
            input.Height = 32;
            input.Padding = new Thickness(8);
            input.Background = new SolidColorBrush(Color.FromRgb(37, 39, 46));
            input.Foreground = Brushes.White;
            input.BorderBrush = new SolidColorBrush(Color.FromRgb(58, 60, 70));

            StackPanel buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            buttons.Margin = new Thickness(0, 12, 0, 0);

            Button cancel = new Button();
            cancel.Content = "Отмена";
            cancel.Width = 85;
            cancel.Height = 32;
            cancel.Margin = new Thickness(0, 0, 8, 0);
            cancel.IsCancel = true;

            Button create = new Button();
            create.Content = "Создать";
            create.Width = 85;
            create.Height = 32;
            create.IsDefault = true;
            create.Background = new SolidColorBrush(Color.FromRgb(108, 92, 231));
            create.Foreground = Brushes.White;

            create.Click += delegate
            {
                if (!string.IsNullOrWhiteSpace(input.Text))
                {
                    dialog.DialogResult = true;
                }
                else
                {
                    MessageBox.Show(dialog, "Введите название папки.");
                }
            };

            buttons.Children.Add(cancel);
            buttons.Children.Add(create);

            TreeViewItem selectedItem = NotesTreeView.SelectedItem as TreeViewItem;
            Folder selectedParent = null;

            if (selectedItem != null)
            {
                selectedParent = selectedItem.Tag as Folder;
            }

            TextBlock hint = new TextBlock();
            hint.Margin = new Thickness(0, 0, 0, 7);
            hint.Foreground = new SolidColorBrush(Color.FromRgb(180, 182, 192));

            if (selectedParent != null)
            {
                hint.Text = "Вложенная папка в «" + selectedParent.Title + "»";
            }
            else
            {
                hint.Text = "Главная папка";
            }

            panel.Children.Add(hint);
            panel.Children.Add(input);
            panel.Children.Add(buttons);

            dialog.Content = panel;
            dialog.Loaded += delegate { input.Focus(); };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                return input.Text;
            }

            return null;
        }

        private async void SaveNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string title = NoteTitleTextBox.Text.Trim();

            if (title.Length == 0)
            {
                MessageBox.Show(
                    "Введите название заметки.",
                    "Заметка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NoteTitleTextBox.Focus();
                return;
            }

            string noteType = "personal";
            ComboBoxItem selectedType = NoteTypeComboBox.SelectedItem as ComboBoxItem;

            if (selectedType != null && selectedType.Tag != null)
            {
                noteType = selectedType.Tag.ToString();
            }

            try
            {
                if (currentNote == null)
                {
                    Note newNote = new Note();
                    newNote.OwnerId = currentUser.Id;

                    if (selectedFolder != null)
                    {
                        newNote.FolderId = selectedFolder.Id;
                    }

                    newNote.Title = title;
                    newNote.Content = NoteContentTextBox.Text;
                    newNote.NoteType = noteType;

                    currentNote = await App.NotesDataService.CreateNoteAsync(newNote);
                }
                else
                {
                    currentNote.Title = title;
                    currentNote.Content = NoteContentTextBox.Text;
                    currentNote.NoteType = noteType;

                    await App.NotesDataService.UpdateNoteAsync(currentNote);
                }

                await LoadDataAsync();

                TreeViewItem savedItem = FindTreeItem(currentNote.Id);

                if (savedItem != null)
                {
                    savedItem.IsSelected = true;
                }

                NoteStatusTextBlock.Text = "Сохранено";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить заметку: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentNote == null)
            {
                return;
            }

            try
            {
                currentNote.IsFavorite = !currentNote.IsFavorite;

                await App.NotesDataService.SetNoteStateAsync(
                    currentUser.Id,
                    currentNote.Id,
                    "is_favorite",
                    currentNote.IsFavorite);

                await LoadDataAsync();

                TreeViewItem item = FindTreeItem(currentNote.Id);

                if (item != null)
                {
                    item.IsSelected = true;
                }

                ShowNote(currentNote);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось изменить избранное: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentNote == null)
            {
                return;
            }

            try
            {
                currentNote.IsArchive = !currentNote.IsArchive;

                await App.NotesDataService.SetNoteStateAsync(
                    currentUser.Id,
                    currentNote.Id,
                    "is_archive",
                    currentNote.IsArchive);

                if (currentNote.IsArchive)
                {
                    currentNote = null;
                    ClearEditor();
                }

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось изменить архив: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentNote != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Удалить заметку «" + currentNote.Title + "»?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    await App.NotesDataService.DeleteNoteAsync(currentUser.Id, currentNote.Id);

                    currentNote = null;
                    ClearEditor();

                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось удалить заметку: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                return;
            }

            if (selectedFolder == null)
            {
                return;
            }

            string message = "Удалить папку «" + selectedFolder.Title
                + "» и все вложенные папки и заметки внутри неё? Это действие нельзя отменить.";

            MessageBoxResult folderResult = MessageBox.Show(
                message,
                "Удаление папки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (folderResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Guid deletedId = selectedFolder.Id;

                await App.NotesDataService.DeleteFolderTreeAsync(currentUser.Id, deletedId);

                selectedFolder = null;
                ClearEditor();

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить папку: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearEditor()
        {
            selectedFolder = null;
            loadingEditor = true;

            NoteTitleHeader.Text = "Мои заметки";
            NoteTitleTextBox.Text = "";
            NoteContentTextBox.Text = "";
            NoteTypeComboBox.SelectedIndex = 0;
            NoteStatusTextBlock.Text = "Выберите заметку или создайте новую";

            loadingEditor = false;

            UpdateActions();
        }

        private void UpdateActions()
        {
            bool isNoteSelected = currentNote != null;

            FavoriteButton.IsEnabled = isNoteSelected;
            ArchiveButton.IsEnabled = isNoteSelected;
            DeleteButton.IsEnabled = isNoteSelected || selectedFolder != null;

            if (selectedFolder != null && !isNoteSelected)
            {
                DeleteButton.Content = "Удалить папку";
            }
            else
            {
                DeleteButton.Content = "Удалить";
            }
        }

        private void NoteTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingEditor && currentNote != null)
            {
                NoteStatusTextBlock.Text = "Есть несохранённые изменения";
            }
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            sidebarVisible = !sidebarVisible;

            if (sidebarVisible)
            {
                SidebarColumn.Width = new GridLength(SidebarWidth);
                DividerColumn.Width = new GridLength(1);
                SidebarBorder.Visibility = Visibility.Visible;
                SidebarDivider.Visibility = Visibility.Visible;
                HideSidebarButton.Visibility = Visibility.Visible;
                ShowSidebarButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarColumn.Width = new GridLength(0);
                DividerColumn.Width = new GridLength(0);
                SidebarBorder.Visibility = Visibility.Collapsed;
                SidebarDivider.Visibility = Visibility.Collapsed;
                HideSidebarButton.Visibility = Visibility.Collapsed;
                ShowSidebarButton.Visibility = Visibility.Visible;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Выйти из аккаунта?",
                "Выход",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        // Открывает и скрывает правую панель ИИ-анализа.
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (aiPanelVisible)
            {
                aiPanelVisible = false;
                RightBarColumn.Width = new GridLength(0);
                DividerColumn2.Width = new GridLength(0);
                AI_Column.Visibility = Visibility.Collapsed;
                AIDivider.Visibility = Visibility.Collapsed;
                ShowAIButton.Visibility = Visibility.Visible;
            }
            else
            {
                aiPanelVisible = true;
                RightBarColumn.Width = new GridLength(AiPanelWidth);
                DividerColumn2.Width = new GridLength(1);
                AI_Column.Visibility = Visibility.Visible;
                AIDivider.Visibility = Visibility.Visible;
                ShowAIButton.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadAccessStatusAsync()
        {
            try
            {
                using HttpResponseMessage response = await App.ApiClient.GetAsync(
                    "api/billing/status");

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Не удалось получить статус доступа.");

                using JsonDocument document = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                JsonElement root = document.RootElement;
                hasAiAccess = root.GetProperty("hasAiAccess").GetBoolean();
                string plan = root.GetProperty("plan").GetString() ?? "none";

                AiAccessStatusTextBlock.Text = plan switch
                {
                    "lifetime" => "Постоянный доступ активен",
                    "monthly" => "Подписка активна",
                    _ => "ИИ-анализ заблокирован: требуется оплата"
                };

                GenerateButton.IsEnabled = hasAiAccess;
                if (!hasAiAccess)
                    ResponseTextBox.Text = "Для использования ИИ-анализа оформите подписку или единовременный доступ.";
            }
            catch (Exception ex)
            {
                hasAiAccess = false;
                GenerateButton.IsEnabled = false;
                AiAccessStatusTextBlock.Text = "Не удалось проверить доступ";
                ResponseTextBox.Text = ex.Message;
            }
        }

        private async void RefreshAccessButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAccessStatusAsync();
        }

        private async void BuyAccessButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult choice = MessageBox.Show(
                "Да — ежемесячный доступ за 349 ₽ на 30 дней.\nНет — единовременный доступ за 4 999 ₽.",
                "Выберите тариф",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Cancel)
                return;

            string plan = choice == MessageBoxResult.Yes ? "monthly" : "lifetime";

            try
            {
                using HttpResponseMessage response = await App.ApiClient.PostAsJsonAsync(
                    "api/billing/create",
                    new { plan });

                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(body);

                using JsonDocument document = JsonDocument.Parse(body);
                string url = document.RootElement.GetProperty("confirmationUrl").GetString() ?? "";
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? paymentUri))
                    throw new InvalidOperationException("Платёжная ссылка не получена.");

                Process.Start(new ProcessStartInfo(paymentUri.ToString()) { UseShellExecute = true });
                MessageBox.Show(
                    "Завершите оплату в браузере. После подтверждения нажмите «Обновить статус» или перезапустите окно.",
                    "Оплата",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось создать платёж", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GenerateButton_Click(object sender,RoutedEventArgs e)
        {
            if (!hasAiAccess)
            {
                ResponseTextBox.Text = "ИИ-анализ доступен только при активной подписке или единовременном доступе.";
                return;
            }

            GenerateButton.IsEnabled = false;
            ResponseTextBox.Text = "Выполняется анализ...";

            try
            {
                string title = NoteTitleTextBox.Text.Trim();
                string content = NoteContentTextBox.Text;
                string type = GetSelectedNoteType();

                if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
                {
                    ResponseTextBox.Text = "Сначала выберите заметку.";
                    return;
                }

                ChatRequest request = new ChatRequest
                {
                    Message =
                        "Название заметки: " + title +
                        "\nТип заметки: " + type +
                        "\n\nСодержимое заметки:\n" + content,

                    Prompt = "analyse"
                };

                using HttpResponseMessage tokenResponse = await App.ApiClient.GetAsync("api/billing/ai-token");
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    ResponseTextBox.Text = "Не удалось подтвердить право на ИИ-анализ. Проверьте подписку.";
                    await LoadAccessStatusAsync();
                    return;
                }

                using JsonDocument tokenDocument = JsonDocument.Parse(
                    await tokenResponse.Content.ReadAsStringAsync());
                string accessToken = tokenDocument.RootElement.GetProperty("token").GetString() ?? "";

                using var aiRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat");
                aiRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                aiRequest.Content = JsonContent.Create(request);

                using HttpResponseMessage response = await httpClient.SendAsync(aiRequest);
                string answer = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ResponseTextBox.Text = "Ошибка API: " + answer;
                    return;
                }

                if (string.IsNullOrWhiteSpace(answer))
                {
                    ResponseTextBox.Text = "ИИ не вернула ответ.";
                    return;
                }

                ResponseTextBox.Text = RemoveMarkdownSymbols(answer);
            }
            catch (Exception ex)
            {
                ResponseTextBox.Text = "Не удалось получить анализ: " + ex.Message;
            }
            finally
            {
                GenerateButton.IsEnabled = hasAiAccess;
            }
        }

        private string GetSelectedNoteType()
        {
            ComboBoxItem selectedType = NoteTypeComboBox.SelectedItem as ComboBoxItem;

            if (selectedType != null && selectedType.Content != null)
            {
                return selectedType.Content.ToString();
            }

            return "Личное";
        }

        // Убираем основные обозначения Markdown (** , __ , ~~ , ` , # , > , - , * , +) из текста
        private string RemoveMarkdownSymbols(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Заголовки: ## Заголовок -> Заголовок.
            text = Regex.Replace(text, @"(?m)^\s{0,3}#{1,6}\s*", "");

            // Обычные списки: - пункт или * пункт -> • пункт.
            text = Regex.Replace(text, @"(?m)^\s*[-*+]\s+", "• ");
            text = Regex.Replace(text, @"(?m)^\s*\d+[.)]\s+", "• ");

            // Убираем выделение, зачёркивание, код и блоки кода.
            text = text.Replace("**", "");
            text = text.Replace("__", "");
            text = text.Replace("~~", "");
            text = text.Replace("`", "");
            text = Regex.Replace(text, @"(?m)^\s*>\s?", "");
            text = Regex.Replace(text, @"(?m)^\s*[-*_]{3,}\s*$", "");

            return text.Trim();
        }

        public class ChatRequest
        {
            public string Message { get; set; } = "";
            public string Prompt { get; set; } = "analyse";
        }
    }
}
