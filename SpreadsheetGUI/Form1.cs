

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using System.Text.RegularExpressions;


namespace SpreadsheetGUI
{
    
    public partial class SpreadsheetForm : Form
    {
       
        Spreadsheet sheet;
       
        int row, col;

        
        public SpreadsheetForm()
        {
            InitializeComponent();
            sheet = new Spreadsheet(a => Regex.IsMatch(a, "^[a-zA-Z][0-9]+$"), a => a.ToLower(), "default");
            constructorHelper();
        }


        private void constructorHelper()
        {
            int zt=0;
            row = zt;
            col = zt;
            string str="a1";
            textBoxCellName.Text = str;
            editBox.Text = sheet.GetCellValue(str).ToString();
            spreadsheetPanel.SetSelection(zt, zt);
        }

       
        private void spreadsheetPanel1_SelectionChanged(SS.SpreadsheetPanel sender)
        {
           
            string value = editBox.Text;

            foreach (string s in sheet.SetContentsOfCell(calcCell(col, row), value))
            {
                char temp='a';
                int inttemp=1;
                int zs=0;
                sender.SetValue(s[zs] - temp, int.Parse(s.Substring(inttemp)) - inttemp, sheet.GetCellValue(s).ToString());
            }

            
            sender.GetSelection(out col, out row);
            editBox.Text = sheet.GetCellString(calcCell(col, row));
            textBoxCellName.Text = calcCell(col, row);
            textBoxValue.Text = sheet.GetCellValue(calcCell(col, row)).ToString();

           
            editBox.Focus();
            editBox.SelectAll();

        }

      
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys masked = keyData & ~Keys.Modifiers;
            if (editBox.SelectionLength == editBox.Text.Length)
            {
                
                switch (masked)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                        arrowKeyHelper(keyData);
                        return true;
                }
            }
            switch (keyData)
            {
                case Keys.Enter: 
                    spreadsheetPanel.SetSelection(col, row + 1);
                    return true;
                case Keys.Tab:
                    if(editBox.SelectionLength == editBox.Text.Length)
                    {
                        editBox.Select(editBox.Text.Length, 0);
                    }
                    else
                    {
                        editBox.Select(0, editBox.Text.Length);
                    }
                    return true;
                case Keys.Control | Keys.S: 
                    saveToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.O: 
                    openToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.N:
                    newToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.E:
                    closeToolStripMenuItem_Click(null, null);
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

      
        private void arrowKeyHelper(Keys key)
        {
           
            bool isCmd = key.HasFlag(Keys.Control);
            bool isShift = key.HasFlag(Keys.Shift);
            key = key & ~Keys.Modifiers;
           
            int tempint=1;
            int dx = tempint * Convert.ToInt32(key == Keys.Right) - tempint * Convert.ToInt32(key == Keys.Left);
            int dy = tempint * Convert.ToInt32(key == Keys.Down) - tempint * Convert.ToInt32(key == Keys.Up);
            if (isCmd)
            {
                copyHelper(isShift, dx, dy);
            }
            spreadsheetPanel.SetSelection(col + dx, row + dy);
        }

      
        private void copyHelper(bool isShift, int dx, int dy)
        {
            if (col + dx >= 0 && col + dx < 26 && row + dy >= 0 && row + dy < 100) 
            {
                string val = sheet.GetCellString(calcCell(col, row)); 
                if (val.Length > 0 && !isShift && val[0] == '=') 
                {
                    string newString = "";
                    foreach (string s in Regex.Split(val, "(=|\\+|-|/|\\*|\\)|\\()")) 
                    {
                        string temp = s.Trim();
                        if (Regex.IsMatch(temp, "^[a-zA-Z][0-9]+")) 
                        {
                            temp = calcCell(temp[0] - 'a' + dx, int.Parse(temp.Substring(1)) - 1 + dy);
                            newString += temp;
                        }
                        else
                        {
                            newString += s; 
                        }
                    }
                    val = newString;
                }
                sheet.SetContentsOfCell(calcCell(col + dx, row + dy), val);
                spreadsheetPanel.SetValue(col + dx, row + dy, sheet.GetCellValue(calcCell(col + dx, row + dy)).ToString());
            }
        }

      
        private void SpreadsheetForm_Shown(object sender, EventArgs e)
        {
            editBox.Focus();
        }

      
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sheet.Changed)
            {
                DialogResult dialogResult = MessageBox.Show("There are unsaved changes, are you sure you want to close?", "Unsaved Changes", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }
            Close();
        }

       
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = "";
                using (SaveFileDialog saveFileDialog = new SaveFileDialog()) //open dialog
                {
                    saveFileDialog.Filter = "spreadsheet files (*.sprd)|*.sprd|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                       
                        path = saveFileDialog.FileName;
                    }
                    else
                    { 
                        return;
                    }
                }
                sheet.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("something wrong when saving files: " + ex.Message); // if error, display
            }
        }

       
        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = "";
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) // open file dialog
                {
                    openFileDialog.Filter = "spreadsheet files (*.sprd)|*.sprd|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        
                        path = openFileDialog.FileName;
                    }
                    else 
                    {
                        return;
                    }
                }
                if (sheet.Changed)
                {
                    DialogResult dialogResult = MessageBox.Show("unsaved changes", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                }
               
                sheet = new Spreadsheet(path, a => Regex.IsMatch(a, "^[a-zA-Z][0-9]+$"), a => a.ToLower(), sheet.GetSavedVersion(path));
                constructorHelper();
               
                foreach (string s in sheet.GetNamesOfAllNonemptyCells())
                {
                    char tempchar='a';
                    int tempint=1;
                    spreadsheetPanel.SetValue(((char)s[0] - tempchar), int.Parse(s.Substring(tempint)) - tempint, sheet.GetCellValue(s).ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("something wrong when open file:: " + ex.Message); // if error, notify
            }
        }

        
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SpreadsheetContext.getAppContext().RunForm(new SpreadsheetForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("something wrong when open file: " + ex.Message);
            }
        }

       
        private string calcCell(int c, int r) => (char)('a' + c) + (r + 1).ToString();
    }
}
