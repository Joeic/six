

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace SS
{


  

    public delegate void SelectionChangedHandler(SpreadsheetPanel sender);



   
    public partial class SpreadsheetPanel : UserControl
    {
        
        public event SelectionChangedHandler SelectionChanged;



      
        private HScrollBar hScroll;
        private VScrollBar vScroll;
        private DrawingPanel drawingPanel;


      
        private const int DATA_COL_WIDTH = 81;
        private const int DATA_ROW_HEIGHT = 22;
        private const int LABEL_COL_WIDTH = 31;
        private const int LABEL_ROW_HEIGHT = 31;
        private const int PADDING = 2;
        private const int SCROLLBAR_WIDTH = 21;
        private const int COL_COUNT = 26;
        private const int ROW_COUNT = 99;


      

        public SpreadsheetPanel()
        {

            InitializeComponent();

          
            drawingPanel = new DrawingPanel(this);
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.AutoScroll = false;

          
            vScroll = new VScrollBar();
            vScroll.SmallChange = 1;
            vScroll.Maximum = ROW_COUNT;

      
            hScroll = new HScrollBar();
            hScroll.SmallChange = 1;
            hScroll.Maximum = COL_COUNT;

           
            Controls.Add(drawingPanel);
            Controls.Add(vScroll);
            Controls.Add(hScroll);

           
            hScroll.Scroll += drawingPanel.HandleHScroll;
            vScroll.Scroll += drawingPanel.HandleVScroll;

        }

       

        public void Clear()
        {
            drawingPanel.Clear();
        }



        public bool SetValue(int c, int r, string v)
        {
            return drawingPanel.SetValue(c, r, v);
        }


     

        public bool GetValue(int c, int r, out string v)
        {
            return drawingPanel.GetValue(c, r, out v);
        }



        public bool SetSelection(int c, int r)
        {
            return drawingPanel.SetSelection(c, r);
        }


       

        public void GetSelection(out int c, out int r)
        {
            drawingPanel.GetSelection(out c, out r);
        }


     
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (FindForm() == null )
            {
                drawingPanel.Size = new Size(Width - SCROLLBAR_WIDTH, Height - SCROLLBAR_WIDTH);
                vScroll.Location = new Point(Width - SCROLLBAR_WIDTH, 0);
                vScroll.Size = new Size(SCROLLBAR_WIDTH, Height - SCROLLBAR_WIDTH);
                vScroll.LargeChange = (Height - SCROLLBAR_WIDTH) / DATA_ROW_HEIGHT;
                hScroll.Location = new Point(0, Height - SCROLLBAR_WIDTH);
                hScroll.Size = new Size(Width - SCROLLBAR_WIDTH, SCROLLBAR_WIDTH);
                hScroll.LargeChange = (Width - SCROLLBAR_WIDTH) / DATA_COL_WIDTH;
                return;
            }

            if(FindForm().WindowState != FormWindowState.Minimized)
             {
                drawingPanel.Size = new Size(Width - SCROLLBAR_WIDTH, Height - SCROLLBAR_WIDTH);
                vScroll.Location = new Point(Width - SCROLLBAR_WIDTH, 0);
                vScroll.Size = new Size(SCROLLBAR_WIDTH, Height - SCROLLBAR_WIDTH);
                vScroll.LargeChange = (Height - SCROLLBAR_WIDTH) / DATA_ROW_HEIGHT;
                hScroll.Location = new Point(0, Height - SCROLLBAR_WIDTH);
                hScroll.Size = new Size(Width - SCROLLBAR_WIDTH, SCROLLBAR_WIDTH);
                hScroll.LargeChange = (Width - SCROLLBAR_WIDTH) / DATA_COL_WIDTH;
                return;
            }
        }




        private class Address
        {

            public int Col { get; set; }
            public int Row { get; set; }

            public Address(int c, int r)
            {
                Col = c;
                Row = r;
            }

            public override int GetHashCode()
            {
                return Col.GetHashCode() ^ Row.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if ((obj == null) || !(obj is Address))
                {
                    return false;
                }
                Address a = (Address)obj;
                return Col == a.Col && Row == a.Row;
            }

        }


     

        private class DrawingPanel : Panel
        {
         
            private int _selectedCol;
            private int _selectedRow;

           
            private int _firstColumn = 0;
            private int _firstRow = 0;

          
            private Dictionary<Address, String> _values;

           
            private SpreadsheetPanel _ssp;


            public DrawingPanel(SpreadsheetPanel ss)
            {
                DoubleBuffered = true;
                _values = new Dictionary<Address, String>();
                _ssp = ss;
            }


            private bool InvalidAddress(int col, int row)
            {
                return col < 0 || row < 0 || col >= COL_COUNT || row >= ROW_COUNT;
            }


            public void Clear()
            {
                _values.Clear();
                Invalidate();
            }


            public bool SetValue(int col, int row, string c)
            {
                if (InvalidAddress(col, row))
                {
                    return false;
                }

                Address a = new Address(col, row);
                if (c == null || c == "")
                {
                    _values.Remove(a);
                }
                else
                {
                    _values[a] = c;
                }
                Invalidate();
                return true;
            }


            public bool GetValue(int col, int row, out string c)
            {
                if (InvalidAddress(col, row))
                {
                    c = null;
                    return false;
                }
                if (!_values.TryGetValue(new Address(col, row), out c))
                {
                    c = "";
                }
                return true;
            }


            public bool SetSelection(int col, int row)
            {
                if (InvalidAddress(col, row))
                {
                    return false;
                }
                _selectedCol = col;
                _selectedRow = row;
                if (_ssp.SelectionChanged != null)
                {
                    _ssp.SelectionChanged(_ssp);
                }
                Invalidate();
                return true;
            }


            public void GetSelection(out int col, out int row)
            {
                col = _selectedCol;
                row = _selectedRow;
            }


            public void HandleHScroll(Object sender, ScrollEventArgs args)
            {
                _firstColumn = args.NewValue;
                Invalidate();
            }

            public void HandleVScroll(Object sender, ScrollEventArgs args)
            {
                _firstRow = args.NewValue;
                Invalidate();
            }


            protected override void OnPaint(PaintEventArgs e)
            {

                Region clip = new Region(e.ClipRectangle);
                e.Graphics.Clip = clip;

                e.Graphics.FillRectangle(
                    new SolidBrush(Color.White),
                    LABEL_COL_WIDTH,
                    LABEL_ROW_HEIGHT,
                    (COL_COUNT - _firstColumn) * DATA_COL_WIDTH,
                    (ROW_COUNT - _firstRow) * DATA_ROW_HEIGHT);

                Brush brush = new SolidBrush(Color.Gray);
                Pen pen = new Pen(brush);
                Font regularFont = Font;
                Font boldFont = new Font(regularFont, FontStyle.Bold);

                int bottom = LABEL_ROW_HEIGHT + (ROW_COUNT - _firstRow) * DATA_ROW_HEIGHT;
                e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, bottom));
                for (int x = 0; x <= (COL_COUNT - _firstColumn); x++)
                {
                    e.Graphics.DrawLine(
                        pen,
                        new Point(LABEL_COL_WIDTH + x * DATA_COL_WIDTH, 0),
                        new Point(LABEL_COL_WIDTH + x * DATA_COL_WIDTH, bottom));
                }

                for (int x = 0; x < COL_COUNT - _firstColumn; x++)
                {
                    Font f = (_selectedCol - _firstColumn == x) ? boldFont : Font;
                    DrawColumnLabel(e.Graphics, x, f);
                }

                int right = LABEL_COL_WIDTH + (COL_COUNT - _firstColumn) * DATA_COL_WIDTH;
                e.Graphics.DrawLine(pen, new Point(0, 0), new Point(right, 0));
                for (int y = 0; y <= ROW_COUNT - _firstRow; y++)
                {
                    e.Graphics.DrawLine(
                        pen,
                        new Point(0, LABEL_ROW_HEIGHT + y * DATA_ROW_HEIGHT),
                        new Point(right, LABEL_ROW_HEIGHT + y * DATA_ROW_HEIGHT));
                }

                for (int y = 0; y < (ROW_COUNT - _firstRow); y++)
                {
                    Font f = (_selectedRow - _firstRow == y) ? boldFont : Font;
                    DrawRowLabel(e.Graphics, y, f);
                }

                if ((_selectedCol - _firstColumn >= 0) && (_selectedRow - _firstRow >= 0))
                {
                    e.Graphics.DrawRectangle(
                        pen,
                        new Rectangle(LABEL_COL_WIDTH + (_selectedCol - _firstColumn) * DATA_COL_WIDTH + 1,
                                      LABEL_ROW_HEIGHT + (_selectedRow - _firstRow) * DATA_ROW_HEIGHT + 1,
                                      DATA_COL_WIDTH - 2,
                                      DATA_ROW_HEIGHT - 2));
                }

                foreach (KeyValuePair<Address, String> address in _values)
                {
                    String text = address.Value;
                    int x = address.Key.Col - _firstColumn;
                    int y = address.Key.Row - _firstRow;
                    float height = e.Graphics.MeasureString(text, regularFont).Height;
                    float width = e.Graphics.MeasureString(text, regularFont).Width;
                    if (x >= 0 && y >= 0)
                    {
                        Region cellClip = new Region(new Rectangle(LABEL_COL_WIDTH + x * DATA_COL_WIDTH + PADDING,
                                                                   LABEL_ROW_HEIGHT + y * DATA_ROW_HEIGHT,
                                                                   DATA_COL_WIDTH - 2 * PADDING,
                                                                   DATA_ROW_HEIGHT));
                        cellClip.Intersect(clip);
                        e.Graphics.Clip = cellClip;
                        e.Graphics.DrawString(
                            text,
                            regularFont,
                            brush,
                            LABEL_COL_WIDTH + x * DATA_COL_WIDTH + PADDING,
                            LABEL_ROW_HEIGHT + y * DATA_ROW_HEIGHT + (DATA_ROW_HEIGHT - height) / 2);
                    }
                }


            }


            private void DrawColumnLabel(Graphics g, int x, Font f)
            {
                String label = ((char)('A' + x + _firstColumn)).ToString();
                float height = g.MeasureString(label, f).Height;
                float width = g.MeasureString(label, f).Width;
                g.DrawString(
                      label,
                      f,
                      new SolidBrush(Color.Black),
                      LABEL_COL_WIDTH + x * DATA_COL_WIDTH + (DATA_COL_WIDTH - width) / 2,
                      (LABEL_ROW_HEIGHT - height) / 2);
            }


          
            private void DrawRowLabel(Graphics g, int y, Font f)
            {
                String label = (y + 1 + _firstRow).ToString();
                float height = g.MeasureString(label, f).Height;
                float width = g.MeasureString(label, f).Width;
                g.DrawString(
                    label,
                    f,
                    new SolidBrush(Color.Black),
                    LABEL_COL_WIDTH - width - PADDING,
                    LABEL_ROW_HEIGHT + y * DATA_ROW_HEIGHT + (DATA_ROW_HEIGHT - height) / 2);
            }


           

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnClick(e);
                int x = (e.X - LABEL_COL_WIDTH) / DATA_COL_WIDTH;
                int y = (e.Y - LABEL_ROW_HEIGHT) / DATA_ROW_HEIGHT;
                if (e.X > LABEL_COL_WIDTH && e.Y > LABEL_ROW_HEIGHT && (x + _firstColumn < COL_COUNT) && (y + _firstRow < ROW_COUNT))
                {
                    _selectedCol = x + _firstColumn;
                    _selectedRow = y + _firstRow;
                    if (_ssp.SelectionChanged != null)
                    {
                        _ssp.SelectionChanged(_ssp);
                    }
                }
                Invalidate();
            }

        }

    }
}
