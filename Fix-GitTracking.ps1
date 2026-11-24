# Run from the repository root in PowerShell.
$ErrorActionPreference = "Stop"

$root = git rev-parse --show-toplevel 2>$null
if (-not $root) {
    throw "Текущая папка не является Git-репозиторием. Откройте терминал в корне репозитория."
}
Set-Location $root

$ignore = @'
# Visual Studio
.vs/
*.suo
*.user
*.userosscache
*.sln.docstates
*.VC.db
*.VC.VC.opendb

# .NET build output
**/bin/
**/obj/
[Bb]uild/
[Dd]ebug/
[Rr]elease/
x64/
x86/

# IDE and temporary files
.idea/
.vscode/
*.cache
*.tmp
*.log

# OS
.DS_Store
Thumbs.db
'@

Set-Content -Path ".gitignore" -Value $ignore -Encoding utf8

$tracked = @(git ls-files)
$generated = @($tracked | Where-Object {
    $_ -match '(^|/)(\.vs|bin|obj)(/|$)' -or
    $_ -match '\.(suo|user|userosscache|sln\.docstates|cache|pdb)$'
})

if ($generated.Count -gt 0) {
    git rm --cached --ignore-unmatch -- $generated
}

git add -- .gitignore
Write-Host ""
Write-Host "Готово. Локальные файлы не удалялись." -ForegroundColor Green
Write-Host "Исключено из Git: $($generated.Count) служебных файлов."
Write-Host "Проверьте Git Changes. Ваши исходники не затрагивались."
