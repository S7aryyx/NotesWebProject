using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.Models;

namespace WPF.Windows;

public partial class MainWindow : Window
{
    private readonly User _currentUser;
    private List<Folder> _folders = new();
    private List<Note> _notes = new();
    private Note? _currentNote;
    private Folder? _selectedFolder;
    private bool _sidebarVisible = true;
    private bool _loadingEditor;

    public MainWindow(User user)
    {
        InitializeComponent();
        _currentUser = user;
        UserLoginTextBlock.Text = user.Login;
        UserAvatarText.Text = string.IsNullOrWhiteSpace(user.Login) ? "U" : user.Login[..1].ToUpper();
        NoteTypeComboBox.SelectedIndex = 0;
        UpdateEditorPlaceholders();
        Loaded += async (_, _) => await LoadDataAsync();
        UpdateActions();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _folders = await App.NotesDataService.GetFoldersAsync(_currentUser.Id);
            _notes = await App.NotesDataService.GetNotesAsync(_currentUser.Id);
            RenderTree();
            if (_currentNote != null)
            {
                TreeViewItem? item = FindTreeItem(_currentNote.Id);
                if (item != null) item.IsSelected = true;
                else ClearEditor();
            }
            NoteStatusTextBlock.Text = $"Папок: {_folders.Count} · Заметок: {_notes.Count(n => !n.IsArchive)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить заметки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        UpdateActions();
    }

    private void RenderTree()
    {
        NotesTreeView.Items.Clear();
        foreach (Folder folder in _folders.Where(f => f.ParentFolderId == null).OrderBy(f => f.Title))
            NotesTreeView.Items.Add(CreateFolderItem(folder));
        foreach (Note note in _notes.Where(n => n.FolderId == null && !n.IsArchive).OrderBy(n => n.Title))
            NotesTreeView.Items.Add(CreateNoteItem(note));

        List<Note> archivedNotes = _notes.Where(n => n.IsArchive).OrderBy(n => n.Title).ToList();
        TreeViewItem archiveItem = new() { Tag = "archive", IsExpanded = true, Header = CreateTreeHeader("▣", $"Архив ({archivedNotes.Count})") };
        foreach (Note note in archivedNotes) archiveItem.Items.Add(CreateNoteItem(note));
        NotesTreeView.Items.Add(archiveItem);
    }

    private TreeViewItem CreateFolderItem(Folder folder)
    {
        TreeViewItem item = new() { Tag = folder, IsExpanded = true, Header = CreateTreeHeader("📁", folder.Title), ToolTip = folder.Title };
        foreach (Folder child in _folders.Where(f => f.ParentFolderId == folder.Id).OrderBy(f => f.Title))
            item.Items.Add(CreateFolderItem(child));
        foreach (Note note in _notes.Where(n => n.FolderId == folder.Id && !n.IsArchive).OrderBy(n => n.Title))
            item.Items.Add(CreateNoteItem(note));
        return item;
    }

    private TreeViewItem CreateNoteItem(Note note)
    {
        string icon = note.IsArchive ? "↶" : note.IsFavorite ? "★" : "▤";
        TreeViewItem item = new() { Tag = note, Header = CreateTreeHeader(icon, note.Title), ToolTip = note.Title };
        return item;
    }

    private static Grid CreateTreeHeader(string icon, string title)
    {
        Grid header = new() { MinWidth = 0 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        TextBlock iconText = new() { Text = icon, Foreground = new SolidColorBrush(Color.FromRgb(160, 157, 190)), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
        TextBlock nameText = new() { Text = title, Foreground = new SolidColorBrush(Color.FromRgb(217, 218, 224)), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 200 };
        Grid.SetColumn(nameText, 1);
        header.Children.Add(iconText);
        header.Children.Add(nameText);
        return header;
    }

    private TreeViewItem? FindTreeItem(Guid id)
    {
        foreach (object value in NotesTreeView.Items)
        {
            if (value is TreeViewItem item && FindTreeItem(item, id) is TreeViewItem found) return found;
        }
        return null;
    }

    private static TreeViewItem? FindTreeItem(TreeViewItem item, Guid id)
    {
        if (item.Tag is Note note && note.Id == id || item.Tag is Folder folder && folder.Id == id) return item;
        foreach (object child in item.Items)
            if (child is TreeViewItem childItem && FindTreeItem(childItem, id) is TreeViewItem found) return found;
        return null;
    }

    private void NotesTreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DependencyObject? source = e.OriginalSource as DependencyObject;
        while (source != null && source is not TreeViewItem && source != NotesTreeView)
            source = VisualTreeHelper.GetParent(source);
        if (source is TreeViewItem || source != NotesTreeView) return;
        foreach (object value in NotesTreeView.Items)
            if (value is TreeViewItem item) ClearTreeSelection(item);
        _currentNote = null;
        _selectedFolder = null;
        ClearEditor();
    }

    private static void ClearTreeSelection(TreeViewItem item)
    {
        item.IsSelected = false;
        foreach (object value in item.Items)
            if (value is TreeViewItem child) ClearTreeSelection(child);
    }

    private void NoteTitleTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateEditorPlaceholders();
    private void NoteContentTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateEditorPlaceholders();

    private void UpdateEditorPlaceholders()
    {
        if (NoteTitlePlaceholder != null) NoteTitlePlaceholder.Visibility = string.IsNullOrEmpty(NoteTitleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        if (NoteContentPlaceholder != null) NoteContentPlaceholder.Visibility = string.IsNullOrEmpty(NoteContentTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void NotesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem item) return;
        if (item.Tag is string view && view == "archive")
        {
            _currentNote = null;
            _selectedFolder = null;
            _loadingEditor = true;
            NoteTitleHeader.Text = "Архив";
            NoteTitleTextBox.Text = string.Empty;
            NoteContentTextBox.Text = string.Empty;
            NoteTypeComboBox.SelectedIndex = 0;
            NoteStatusTextBlock.Text = "Архивные заметки. Выберите заметку, чтобы восстановить её.";
            _loadingEditor = false;
        }
        else if (item.Tag is Note note)
        {
            _currentNote = note;
            _selectedFolder = _folders.FirstOrDefault(f => f.Id == note.FolderId);
            ShowNote(note);
        }
        else if (item.Tag is Folder folder)
        {
            _currentNote = null;
            _selectedFolder = folder;
            _loadingEditor = true;
            NoteTitleHeader.Text = folder.Title;
            NoteTitleTextBox.Text = string.Empty;
            NoteContentTextBox.Text = string.Empty;
            NoteTypeComboBox.SelectedIndex = 0;
            NoteStatusTextBlock.Text = "Папка выбрана. Новая заметка будет создана внутри неё.";
            _loadingEditor = false;
        }
        UpdateActions();
    }

    private void ShowNote(Note note)
    {
        _loadingEditor = true;
        NoteTitleHeader.Text = note.Title;
        NoteTitleTextBox.Text = note.Title;
        NoteContentTextBox.Text = note.Content;
        SelectNoteType(note.NoteType);
        NoteStatusTextBlock.Text = note.IsArchive ? "Заметка в архиве" : $"Изменено: {note.UpdatedAt.ToLocalTime():dd.MM.yyyy HH:mm}";
        FavoriteButton.Content = note.IsFavorite ? "★" : "☆";
        FavoriteButton.Foreground = new SolidColorBrush(note.IsFavorite ? Color.FromRgb(255, 209, 102) : Color.FromRgb(180, 182, 192));
        ArchiveButton.Content = note.IsArchive ? "Восстановить" : "Архив";
        _loadingEditor = false;
    }

    private void SelectNoteType(string? type)
    {
        string value = string.IsNullOrWhiteSpace(type) ? "personal" : type.ToLowerInvariant();
        foreach (ComboBoxItem item in NoteTypeComboBox.Items)
            if ((string)item.Tag == value) { NoteTypeComboBox.SelectedItem = item; return; }
        NoteTypeComboBox.SelectedIndex = 0;
    }

    private void NewNoteButton_Click(object sender, RoutedEventArgs e)
    {
        _currentNote = null;
        _selectedFolder = NotesTreeView.SelectedItem is TreeViewItem item
            ? item.Tag is Folder folder ? folder : item.Tag is Note selectedNote ? _folders.FirstOrDefault(f => f.Id == selectedNote.FolderId) : null
            : null;
        _loadingEditor = true;
        NoteTitleHeader.Text = "Новая заметка";
        NoteTitleTextBox.Text = string.Empty;
        NoteContentTextBox.Text = string.Empty;
        NoteTypeComboBox.SelectedIndex = 0;
        NoteStatusTextBlock.Text = _selectedFolder == null ? "Новая заметка · Без папки" : $"Новая заметка · {_selectedFolder.Title}";
        FavoriteButton.Content = "☆";
        FavoriteButton.Foreground = new SolidColorBrush(Color.FromRgb(180, 182, 192));
        ArchiveButton.Content = "Архив";
        _loadingEditor = false;
        UpdateActions();
        NoteTitleTextBox.Focus();
    }

    private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
    {
        string? title = PromptFolderName();
        if (string.IsNullOrWhiteSpace(title)) return;
        Folder? parent = NotesTreeView.SelectedItem is TreeViewItem item ? item.Tag as Folder : null;
        try
        {
            Folder folder = await App.NotesDataService.CreateFolderAsync(_currentUser.Id, title.Trim(), parent?.Id);
            await LoadDataAsync();
            TreeViewItem? createdItem = FindTreeItem(folder.Id);
            if (createdItem != null) createdItem.IsSelected = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось создать папку: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? PromptFolderName()
    {
        Window dialog = new() { Title = "Новая папка", Width = 360, Height = 165, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, ResizeMode = ResizeMode.NoResize, Background = new SolidColorBrush(Color.FromRgb(24, 26, 32)), Foreground = Brushes.White };
        StackPanel panel = new() { Margin = new Thickness(16) };
        TextBox input = new() { Height = 32, Padding = new Thickness(8), Background = new SolidColorBrush(Color.FromRgb(37, 39, 46)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(58, 60, 70)) };
        StackPanel buttons = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
        Button cancel = new() { Content = "Отмена", Width = 85, Height = 32, Margin = new Thickness(0, 0, 8, 0), IsCancel = true };
        Button create = new() { Content = "Создать", Width = 85, Height = 32, IsDefault = true, Background = new SolidColorBrush(Color.FromRgb(108, 92, 231)), Foreground = Brushes.White };
        create.Click += (_, _) => { if (!string.IsNullOrWhiteSpace(input.Text)) dialog.DialogResult = true; else MessageBox.Show(dialog, "Введите название папки."); };
        buttons.Children.Add(cancel);
        buttons.Children.Add(create);
        panel.Children.Add(new TextBlock { Text = NotesTreeView.SelectedItem is TreeViewItem selected && selected.Tag is Folder parent ? $"Вложенная папка в «{parent.Title}»" : "Главная папка", Margin = new Thickness(0, 0, 0, 7), Foreground = new SolidColorBrush(Color.FromRgb(180, 182, 192)) });
        panel.Children.Add(input);
        panel.Children.Add(buttons);
        dialog.Content = panel;
        dialog.Loaded += (_, _) => input.Focus();
        return dialog.ShowDialog() == true ? input.Text : null;
    }

    private async void SaveNoteButton_Click(object sender, RoutedEventArgs e)
    {
        string title = NoteTitleTextBox.Text.Trim();
        if (title.Length == 0) { MessageBox.Show("Введите название заметки.", "Заметка", MessageBoxButton.OK, MessageBoxImage.Information); NoteTitleTextBox.Focus(); return; }
        string noteType = NoteTypeComboBox.SelectedItem is ComboBoxItem selected ? (string)selected.Tag : "personal";
        try
        {
            if (_currentNote == null)
            {
                _currentNote = new Note { OwnerId = _currentUser.Id, FolderId = _selectedFolder?.Id, Title = title, Content = NoteContentTextBox.Text, NoteType = noteType };
                _currentNote = await App.NotesDataService.CreateNoteAsync(_currentNote);
            }
            else
            {
                _currentNote.Title = title;
                _currentNote.Content = NoteContentTextBox.Text;
                _currentNote.NoteType = noteType;
                await App.NotesDataService.UpdateNoteAsync(_currentNote);
            }
            await LoadDataAsync();
            TreeViewItem? savedItem = FindTreeItem(_currentNote.Id);
            if (savedItem != null) savedItem.IsSelected = true;
            NoteStatusTextBlock.Text = "Сохранено";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось сохранить заметку: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentNote == null) return;
        try
        {
            _currentNote.IsFavorite = !_currentNote.IsFavorite;
            await App.NotesDataService.SetNoteStateAsync(_currentUser.Id, _currentNote.Id, "is_favorite", _currentNote.IsFavorite);
            await LoadDataAsync();
            TreeViewItem? item = FindTreeItem(_currentNote.Id);
            if (item != null) item.IsSelected = true;
            ShowNote(_currentNote);
        }
        catch (Exception ex) { MessageBox.Show("Не удалось изменить избранное: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void ArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentNote == null) return;
        try
        {
            _currentNote.IsArchive = !_currentNote.IsArchive;
            await App.NotesDataService.SetNoteStateAsync(_currentUser.Id, _currentNote.Id, "is_archive", _currentNote.IsArchive);
            if (_currentNote.IsArchive) { _currentNote = null; ClearEditor(); }
            await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show("Не удалось изменить архив: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentNote != null)
        {
            if (MessageBox.Show($"Удалить заметку «{_currentNote.Title}»?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                await App.NotesDataService.DeleteNoteAsync(_currentUser.Id, _currentNote.Id);
                _currentNote = null;
                ClearEditor();
                await LoadDataAsync();
            }
            catch (Exception ex) { MessageBox.Show("Не удалось удалить заметку: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            return;
        }
        if (_selectedFolder == null) return;
        string message = $"Удалить папку «{_selectedFolder.Title}» и все вложенные папки и заметки внутри неё? Это действие нельзя отменить.";
        if (MessageBox.Show(message, "Удаление папки", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            Guid deletedId = _selectedFolder.Id;
            await App.NotesDataService.DeleteFolderTreeAsync(_currentUser.Id, deletedId);
            _selectedFolder = null;
            ClearEditor();
            await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show("Не удалось удалить папку: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void ClearEditor()
    {
        _selectedFolder = null;
        _loadingEditor = true;
        NoteTitleHeader.Text = "Мои заметки";
        NoteTitleTextBox.Text = string.Empty;
        NoteContentTextBox.Text = string.Empty;
        NoteTypeComboBox.SelectedIndex = 0;
        NoteStatusTextBlock.Text = "Выберите заметку или создайте новую";
        _loadingEditor = false;
        UpdateActions();
    }

    private void UpdateActions()
    {
        bool isNote = _currentNote != null;
        FavoriteButton.IsEnabled = isNote;
        ArchiveButton.IsEnabled = isNote;
        DeleteButton.IsEnabled = isNote || _selectedFolder != null;
        DeleteButton.Content = _selectedFolder != null && !isNote ? "Удалить папку" : "Удалить";
    }

    private void NoteTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loadingEditor && _currentNote != null) NoteStatusTextBlock.Text = "Есть несохранённые изменения";
    }

    private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
    {
        _sidebarVisible = !_sidebarVisible;
        SidebarColumn.Width = new GridLength(_sidebarVisible ? 280 : 0);
        DividerColumn.Width = new GridLength(_sidebarVisible ? 1 : 0);
        SidebarBorder.Visibility = _sidebarVisible ? Visibility.Visible : Visibility.Collapsed;
        SidebarDivider.Visibility = _sidebarVisible ? Visibility.Visible : Visibility.Collapsed;
        HideSidebarButton.Visibility = _sidebarVisible ? Visibility.Visible : Visibility.Collapsed;
        ShowSidebarButton.Visibility = _sidebarVisible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Выйти из аккаунта?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        new LoginWindow().Show();
        Close();
    }
}
