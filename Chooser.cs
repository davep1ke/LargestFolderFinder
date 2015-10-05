using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LargestFolderFinder
{

    public partial class Chooser : Form
    {
        private List<folderAndSize> allFiles = new List<folderAndSize>();
        public folderAndSize result;
        public Chooser()
        {
            allFiles.AddRange(FindLargestFolder.allFolders);
            InitializeComponent();
            listBox1.DataSource = allFiles; 
            txtSearch.Focus();
        }

        private void btngo_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                DialogResult = DialogResult.OK;
                result = (folderAndSize)listBox1.SelectedItem;
            }
            else
            {
                DialogResult = DialogResult.Abort;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length > 1)
            {
                allFiles.Clear();
                allFiles.AddRange(FindLargestFolder.allFolders);
                
                int i = allFiles.RemoveAll(filterName);
                listBox1.DataSource = null;
                listBox1.DataSource = allFiles;
            }
        }

        private  bool filterName(folderAndSize fi)
        {
            if (fi.folderPath.ToUpper().Contains(txtSearch.Text.ToUpper()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Chooser_Activated(object sender, EventArgs e)
        {
            txtSearch.Focus();
        }

        private void Chooser_Shown(object sender, EventArgs e)
        {
            txtSearch.Focus();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Up &&listBox1.SelectedIndex > 0)
            {
                listBox1.SelectedIndex = listBox1.SelectedIndex - 1;
            }
            else if (e.KeyData == Keys.Down && listBox1.SelectedIndex < listBox1.Items.Count - 1)
            {
                listBox1.SelectedIndex = listBox1.SelectedIndex + 1;
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                FileInfo fi = (FileInfo)listBox1.SelectedItem;
                System.Diagnostics.Process.Start("Explorer.exe", fi.Directory.FullName);
                
                


            }
        }

        private void btn_Open_Only_Click(object sender, EventArgs e)
        {
            folderAndSize f = (folderAndSize)listBox1.SelectedItem;
            FindLargestFolder.openFolder(f.folderPath);

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            folderAndSize f = (folderAndSize)listBox1.SelectedItem;
            FindLargestFolder.openFolder(f.folderPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo fi = (FileInfo)listBox1.SelectedItem;
            DialogResult r = MessageBox.Show("Are you sure you want to delete this file? \n\r" + fi.FullName + "\n\r\n\rNote that the file will remain in the list until you search again", "Are you sure?", MessageBoxButtons.YesNo);

            if (r == DialogResult.Yes && fi.Exists)
            {
                fi.Delete();
            }
        }

        private void listBox1_MouseHover(object sender, EventArgs e)
        {
            Point screenPosition = ListBox.MousePosition;
            Point listBoxClientAreaPosition = listBox1.PointToClient(screenPosition);
            
            int hoveredIndex = listBox1.IndexFromPoint(listBoxClientAreaPosition);

            try
            {
                folderAndSize fi = (folderAndSize)listBox1.Items[hoveredIndex];

                toolTip1.SetToolTip(listBox1, fi.folderPath);
                toolTip1.Show(fi.ToString(), this, 10000);
            }
            catch (Exception)
            {
                //eh, dont care.
            };
        }
    }
}