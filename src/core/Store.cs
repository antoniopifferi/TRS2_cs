using Microsoft.VisualBasic;
using System.IO;
using System.Windows;

namespace TRS2
{
    public static class Store
    {
        private static byte[] header = new byte[Konst.HEAD_LEN];
        private static string datafilepath = string.Empty;
        private static FileStream? datafileobj;

        public static void Init()
        {
            var file = Set.File;
            datafilepath = Path.Combine(file.Fdir,$"{file.Prefix}{file.Tag:D4}.{file.Fext.TrimStart('.')}");

            try
            {
                if (File.Exists(datafilepath))
                {
                    var answer = MessageBox.Show($"File already exists:\n{datafilepath}\n\nOverwrite?","TRS2",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
                    if (answer == MessageBoxResult.Cancel) return;
                    if (answer == MessageBoxResult.No)
                    {
                        var name = Interaction.InputBox("Enter the new filename without directory or extension:","TRS2",file.Prefix);
                        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || Path.HasExtension(name)) return;
                        datafilepath = Path.Combine(file.Fdir,$"{name}.{file.Fext.TrimStart('.')}");
                    }
                }

                CompileHead();
                datafileobj = new FileStream(datafilepath,FileMode.Create,FileAccess.Write,FileShare.Read);
                datafileobj.Write(header);
            }
            catch
            {
                MessageBox.Show($"Error in File = {datafilepath}","TRS2",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public static void Save() { }

        public static void Close()
        {
            datafileobj?.Close();
            datafileobj = null;
        }

        private static void CompileHead()
        {
            Array.Clear(header);
            BitConverter.GetBytes((Int16)(-Konst.HEAD_VER_DUE)).CopyTo(header, 0);
            BitConverter.GetBytes((Int16)Konst.HEAD_VER_GEN).CopyTo(header, 2);
            BitConverter.GetBytes((Int32)Konst.HEAD_IS_SUB).CopyTo(header, 4);
            BitConverter.GetBytes((Int32)Konst.HEAD_VER_SUB_UNO).CopyTo(header, 8);
        }
    }
}
