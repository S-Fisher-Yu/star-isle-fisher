using System;
using System.Drawing;
using System.Windows.Forms;

namespace CelestayFishing
{
    // Native controls retain keyboard navigation, focus cues and accessibility.
    sealed class HarborTheme : IDisposable
    {
        public static readonly Color Ink=Color.FromArgb(66,53,39);
        public static readonly Color Muted=Color.FromArgb(111,96,73);
        public static readonly Color Paper=Color.FromArgb(255,248,227);
        public static readonly Color Edge=Color.FromArgb(130,98,59);
        public static readonly Color Sea=Color.FromArgb(43,103,102);
        public readonly Font Body=new Font("Microsoft YaHei UI",10);
        public readonly Font Small=new Font("Microsoft YaHei UI",9);
        public readonly Font Heading=new Font("SimSun",27,FontStyle.Bold);
        public readonly Font Number=new Font("Consolas",23,FontStyle.Bold);
        public readonly Font Mono=new Font("Consolas",10,FontStyle.Bold);

        public Label Label(Control parent,string text,int x,int y,int width,int height,Font font) {
            var label=new Label { Text=text,Location=new Point(x,y),Size=new Size(width,height),
                Font=font,ForeColor=Ink,BackColor=Color.Transparent,UseMnemonic=false };
            parent.Controls.Add(label); return label;
        }
        public Button Button(Control parent,string text,int x,int y,int width,int height,Action action,bool primary) {
            var button=new Button { Text=text,Location=new Point(x,y),Size=new Size(width,height),
                Font=Body,FlatStyle=FlatStyle.Flat,UseVisualStyleBackColor=false,Cursor=Cursors.Hand,
                BackColor=primary?Sea:Paper,ForeColor=primary?Color.White:Ink,AccessibleName=text };
            button.FlatAppearance.BorderColor=primary?Color.FromArgb(29,72,73):Edge;
            button.FlatAppearance.BorderSize=2;
            button.FlatAppearance.MouseOverBackColor=primary?Color.FromArgb(54,121,118):Color.FromArgb(247,231,193);
            button.FlatAppearance.MouseDownBackColor=primary?Color.FromArgb(33,83,83):Color.FromArgb(234,210,163);
            button.EnabledChanged+=delegate {
                button.BackColor=!button.Enabled?Color.FromArgb(225,218,199):primary?Sea:Paper;
            };
            button.Click+=delegate { action(); };
            parent.Controls.Add(button); return button;
        }
        public void Dispose() { Body.Dispose(); Small.Dispose(); Heading.Dispose(); Number.Dispose(); Mono.Dispose(); }
    }

    sealed class HarborPanel : Panel
    {
        public HarborPanel() { DoubleBuffered=true; BackColor=HarborTheme.Paper; }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            float unit=DeviceDpi/96f;
            using(var pen=new Pen(HarborTheme.Edge,2*unit))
                e.Graphics.DrawRectangle(pen,unit,unit,Width-3*unit,Height-3*unit);
            using(var pen=new Pen(Color.FromArgb(222,197,151),unit))
                e.Graphics.DrawRectangle(pen,5*unit,5*unit,Width-11*unit,Height-11*unit);
        }
    }
}
