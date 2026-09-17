using Modern.Forms;
using Rayman2Lib;
using System.Diagnostics;
namespace BNKReader
{
    public partial class BNMReaderMainForm : Form
    {
        public BNMReaderMainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.AllowMultiple = true;
            if (openFileDialog1.ShowDialog(this).Result == DialogResult.OK)
            {
                string dir = "";
                foreach (var fileName in openFileDialog1.FileNames)
                {
                    if (File.Exists(fileName))
                    {
                        BNMFile bnm = new BNMFile(File.ReadAllBytes(fileName));

                        var fileInfo = new FileInfo(fileName);
                        dir = Path.Combine(fileInfo.DirectoryName, fileInfo.Name.Replace(".bnm", ""));

                        Directory.CreateDirectory(dir);

                        int file = 0;
                        foreach (var soundFile in bnm.soundFiles)
                        {
                            var filename = Path.Combine(dir, (addIndexCheckBox.Checked ? file++ + "_" : "") + soundFile.name);

                            if (File.Exists(filename))
                                File.Delete(filename);
                            soundFile.Save(File.Create(filename));
                        }
                    }
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(dir);
                startInfo.UseShellExecute = true;

                Process.Start(startInfo);
            }
        }

        private void repackButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select Original .bnm Template";
            if (openFileDialog1.ShowDialog(this).Result != DialogResult.OK) return;

            string bnmTemplatePath = openFileDialog1.FileName;
            folderBrowserDialog1.Title = "Select folder containing extracted .wav files";
            if (folderBrowserDialog1.ShowDialog(this).Result != DialogResult.OK) return;

            string wavFolder = folderBrowserDialog1.SelectedPath;
            saveFileDialog1.Title = "Select output destination for repacked bnm";
            saveFileDialog1.FileName = Path.GetFileName(bnmTemplatePath).Replace(".bnm", "_repacked.bnm");
            if (saveFileDialog1.ShowDialog(this).Result != DialogResult.OK) return;

            string outputPath = saveFileDialog1.FileName;

            try
            {
                BNMFile bnm = new BNMFile(File.ReadAllBytes(bnmTemplatePath));
                var replacements = new Dictionary<string, string>();
                string[] filesInFolder = Directory.GetFiles(wavFolder, "*.wav");

                foreach (var sound in bnm.soundFiles)
                {
                    string exactMatch = Path.Combine(wavFolder, sound.name);
                    if (File.Exists(exactMatch))
                    {
                        replacements[sound.name] = exactMatch;
                    } else
                    {
                        var indexedMatch = filesInFolder.FirstOrDefault(f =>
                            Path.GetFileName(f).Contains("_" + sound.name));

                        if (indexedMatch != null)
                        {
                            replacements[sound.name] = indexedMatch;
                        }
                    }
                }

                byte[] resultData = bnm.SaveBNM(replacements);
                File.WriteAllBytes(outputPath, resultData);

                new MessageBoxForm($"Successfully repacked {replacements.Count} sounds to {outputPath}", "Success").ShowDialog(this);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                new MessageBoxForm($"Error during repacking: {ex.Message}", "Error").ShowDialog(this);
            }
        }

    }
}
