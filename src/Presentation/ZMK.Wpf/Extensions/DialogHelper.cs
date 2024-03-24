namespace ZMK.Wpf.Extensions;

public static class DialogHelper
{
    public static string? ShowSaveXlsxFileDialog()
    {
        const string Filter = "Excel Files (*.xlsx)|*.xlsx";
        const string Title = "Выберите файл:";
        var fileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = Filter,
            Title = Title,
            RestoreDirectory = true,
        };

        fileDialog.ShowDialog();
        return fileDialog.FileName;
    }
}