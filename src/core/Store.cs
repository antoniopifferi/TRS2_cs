using Microsoft.VisualBasic;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;

namespace TRS2 {
    public static class Store {
        private static byte[] header = new byte[Konst.HEAD_LEN];
        public static string DataPath = string.Empty; // full path to data file
        private static FileStream? datafileobj;

        public static void Init() {
            var store = Set.Store;
            DataPath = Path.Combine(store.Fdir,$"{store.Prefix}{store.Tag:D4}.{store.Fext.TrimStart('.')}");
            try {
                if (File.Exists(DataPath)) {
                    var answer = MessageBox.Show($"File already exists:\n{DataPath}\n\nOverwrite?","TRS2",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
                    if (answer == MessageBoxResult.Cancel) return;
                    if (answer == MessageBoxResult.No) {
                        var name = Interaction.InputBox("Enter the new filename without directory or extension:","TRS2", $"{store.Prefix}{store.Tag:D4}");
                        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || Path.HasExtension(name)) return;
                        DataPath = Path.Combine(store.Fdir,$"{name}.{store.Fext.TrimStart('.')}");
                        }
                    }
                CompileHead();
                datafileobj = new FileStream(DataPath,FileMode.Create,FileAccess.Write,FileShare.Read);
                datafileobj.Write(header);
                }
            catch {
                MessageBox.Show($"Error in File = {DataPath}","TRS2",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            Display.Text("Initialised File: " + DataPath);
            }

        public static void Save() {
            var data = Run.data;
            var file = datafileobj;
            if (data is null || file is null) return;
            file.Write(MemoryMarshal.AsBytes(data.Archive.AsSpan()));
            }

        public static void Close() {
            datafileobj?.Close();
            datafileobj = null;
            }

        private static void CompileHead() {
            Array.Clear(header);
            BitConverter.GetBytes((Int16)(-Konst.HEAD_VER_DUE)).CopyTo(header, 0);
            BitConverter.GetBytes((Int16)Konst.HEAD_VER_GEN).CopyTo(header, 2);
            BitConverter.GetBytes((Int32)Konst.HEAD_IS_SUB).CopyTo(header, 4);
            BitConverter.GetBytes((Int32)Konst.HEAD_VER_SUB_UNO).CopyTo(header, 8);
            }
        }
    }
