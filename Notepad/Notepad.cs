using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Notepad
{
    public partial class Notepad : Form
    {
        Font currentFont = new Font("Microsoft Sans Serif", 14);
        Color currentColor = Color.Black;
        List<Note> notes = new List<Note>();

        string notesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NotepadApp", "notes.json");

        public Notepad()
        {
            InitializeComponent();
            saveFileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            openFileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            fontDialog.ShowColor = true;

            string appDataPath = Path.GetDirectoryName(notesFilePath);
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            LoadNotesFromJson();
            if(notes.Count == 0)
                CreateNewNote("Hello, World!");
        }

        private void Notepad_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveNotesToJson();
        }

        private int GetNextAvailableNoteNumber()
        {
            int maxNumber = 0;
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                string title = tabPage.Text;
                if (title.StartsWith("Новый "))
                {
                    string numberPart = title.Substring("Новый ".Length);
                    if (int.TryParse(numberPart, out int number))
                    {
                        maxNumber = Math.Max(maxNumber, number);
                    }
                }
            }
            return maxNumber + 1;
        }

        private void CreateNewNote(string text = null)
        {
            Note note = new Note();

            TabPage newTabPage = new TabPage();
            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.TextChanged += textBox_TextChanged;
            newTabPage.Controls.Add(textBox);

            tabControl.TabPages.Add(newTabPage);
            tabControl.SelectedTab = newTabPage;

            int num = GetNextAvailableNoteNumber();
            newTabPage.Text = $"Новый {num}";
            note.Title = newTabPage.Text;
            if(text != null)
            {
                textBox.Text = text;
                note.Text = text;
            }

            notes.Add(note);
        }

        private RichTextBox CreateNewRichTextBox(string text, bool rtf = false)
        {
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            if (rtf)
            {
                richTextBox.Rtf = text;
            }
            else
            {
                richTextBox.Text = text;
            }
            richTextBox.TextChanged += textBox_TextChanged;
            return richTextBox;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TabPage curTabPage = tabControl.SelectedTab;
            if(curTabPage != null)
            {
                RichTextBox currentRichTextBox = curTabPage.Controls[0] as RichTextBox;

                if (curTabPage != null)
                {
                    int index = tabControl.TabPages.IndexOf(curTabPage);
                    if (index >= 0 && index < notes.Count)
                    {
                        notes[index].Text = (curTabPage.Controls[0] as RichTextBox).Text;
                        notes[index].RtfText = currentRichTextBox.Text;
                        notes[index].IsModified = true;
                    }
                }
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewNote();
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                int selectedIndex = tabControl.SelectedIndex;
                tabControl.TabPages.RemoveAt(selectedIndex);

                if (selectedIndex >= 0 && selectedIndex < notes.Count)
                {
                    notes.RemoveAt(selectedIndex);
                }

                if (tabControl.TabPages.Count > 0)
                {
                    tabControl.SelectedIndex = Math.Min(selectedIndex, tabControl.TabPages.Count - 1);
                }
            }
        }

        #region "Файл"
        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewNote();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "Проводник";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    string fileContent = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

                    Note newNote = new Note();
                    newNote.Title = Path.GetFileName(filePath);
                    newNote.Text = fileContent;
                    newNote.FilePath = filePath;

                    notes.Add(newNote);

                    TabPage newTabPage = new TabPage();
                    newTabPage.Text = newNote.Title;
                    RichTextBox richTextBox = CreateNewRichTextBox(fileContent);

                    newTabPage.Controls.Add(richTextBox);
                    tabControl.Controls.Add(newTabPage);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void SaveText(Encoding encoding)
        {
            saveFileDialog.Title = "Сохранить файл";

            TabPage curTabPage = tabControl.SelectedTab;
            if (curTabPage != null)
            {
                var text = curTabPage.Controls[0].Text;

                if (notes.Count > tabControl.SelectedIndex && !string.IsNullOrEmpty(notes[tabControl.SelectedIndex].FilePath))
                {
                    saveFileDialog.FileName = Path.GetFileName(notes[tabControl.SelectedIndex].FilePath);
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(notes[tabControl.SelectedIndex].FilePath);
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, text, encoding);

                        int selectedIndex = tabControl.SelectedIndex;
                        if (selectedIndex >= 0 && selectedIndex < notes.Count)
                        {
                            notes[selectedIndex].FilePath = saveFileDialog.FileName;
                            notes[selectedIndex].IsModified = false;
                        }

                        MessageBox.Show("Файл успешно сохранен.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveText(Encoding.UTF8);
        }
        private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveText(Encoding.UTF8);
        }
        private void uTF32ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveText(Encoding.UTF32);
        }
        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveText(Encoding.ASCII);
        }
        #endregion

        #region "Шрифт"
        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage curTabPage = tabControl.SelectedTab;
            if (curTabPage != null)
            {
                RichTextBox textBox = (RichTextBox)curTabPage.Controls[0];
                fontDialog.Font = textBox.SelectionLength > 0 ? textBox.SelectionFont : currentFont;

                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox.SelectionFont = fontDialog.Font;
                    textBox.SelectionColor = fontDialog.Color;
                }
            }
        }
        #endregion

        #region "Правка"
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage curTabPage = tabControl.SelectedTab;
            if (curTabPage != null)
            {
                RichTextBox textBox = curTabPage.Controls[0] as RichTextBox;
                if (!textBox.SelectedText.Equals(null))
                {
                    textBox.Cut();
                }
                
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage curTabPage = tabControl.SelectedTab;
            if (curTabPage != null)
            {
                RichTextBox textBox = curTabPage.Controls[0] as RichTextBox;
                if (!textBox.SelectedText.Equals(null))
                {
                    textBox.Copy();
                }

            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage curTabPage = tabControl.SelectedTab;
            if (curTabPage != null)
            {
                RichTextBox textBox = curTabPage.Controls[0] as RichTextBox;
                if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                {
                    textBox.Paste();
                }

            }
        }


        #endregion

        #region "Печать"
        private void printToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            printDialog.ShowDialog();
        }
        #endregion

        #region "Json"
        private void SaveNotesToJson()
        {
            try
            {
                List<Note> notesToSave = new List<Note>();
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    RichTextBox richTextBox = tabPage.Controls[0] as RichTextBox;
                    if (richTextBox != null)
                    {
                        int index = tabControl.TabPages.IndexOf(tabPage);
                        if (index >= 0 && index < notes.Count)
                        {
                            notes[index].RtfText = richTextBox.Rtf;
                            notesToSave.Add(notes[index]);
                        }
                    }
                }
                string json = JsonConvert.SerializeObject(notesToSave, Formatting.Indented);
                File.WriteAllText(notesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заметок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNotesFromJson()
        {
            if (File.Exists(notesFilePath))
            {
                try
                {
                    string json = File.ReadAllText(notesFilePath);
                    List<Note> loadedNotes = JsonConvert.DeserializeObject<List<Note>>(json);

                    if (loadedNotes != null)
                    {
                        notes.Clear();
                        tabControl.TabPages.Clear();

                        foreach (Note note in loadedNotes)
                        {
                            TabPage newTabPage = new TabPage();
                            RichTextBox richTextBox = CreateNewRichTextBox(note.RtfText, true);
                            newTabPage.Controls.Add(richTextBox);

                            newTabPage.Text = note.Title;
                            tabControl.TabPages.Add(newTabPage);
                            notes.Add(note);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заметок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

    }
}
