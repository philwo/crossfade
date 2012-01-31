using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.AudioVideoPlayback;

namespace Media_DirectX
{
    public class Video_w : UserControl
    {
        OpenFileDialog openFileDialog;
        Video DXvideo = null;
        Panel VideoPanel;
        int Volume = 0;
        bool mute = false;
        double duration = 0;


       
         
        public Video_w()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            Initialize();
            CreatePanel();
        }

    void Initialize()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Video Dateien (Windows Media(*.wmv)|*.wmv|*.avi)|*.avi|Alle Dateien (*.*)|*.*";
            this.openFileDialog.Title = "Video auswählen";
        }

        void CreatePanel()
        {
            if (VideoPanel != null)
                this.Controls.Remove(this.VideoPanel);

            VideoPanel = null;
            VideoPanel = new Panel();
            this.VideoPanel.Dock = DockStyle.Fill;
            this.VideoPanel.Location = new Point(0, 0);
            this.VideoPanel.Name = "VideoPanel";
            this.VideoPanel.Size = new Size(320, 240);
            VideoPanel.AutoSize = true;
            this.VideoPanel.TabIndex = 1;
            this.VideoPanel.MouseDown += new MouseEventHandler(this.panel1_MouseDown);
            this.Controls.Add(this.VideoPanel);
        }



        void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (DXvideo == null || DXvideo != null && !DXvideo.Playing)
                    OpenVideo();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (DXvideo != null && DXvideo.Playing)
                    DXvideo.Stop();
                OpenVideo();
            }
        }


        void OpenVideo() //öffnet neues Video
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                   DXvideo = new Video(openFileDialog.FileName, false);
                   DXvideo.Owner = VideoPanel;
                   DXvideo.Ending += new EventHandler(video_Ending);
                   DXvideo.Play();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void playVideo() //startet gestopptes video oder pausiert es
        {
            if (DXvideo != null)
            {
                if (DXvideo.Paused == true)
                {
                    DXvideo.Play();
                }
                else
                {
                    DXvideo.Pause();
                }
            }
        }
       public void stopVideo()  //stoppt das Video
        {   
            if (DXvideo != null)
            DXvideo.Stop();
        }
        //gibt die Länge in Sekunden an
        public double length()
        {
            if (DXvideo != null)
            {
                this.duration = DXvideo.Duration;
                return this.duration;
            }
            else
                return 0;
        }
        //gibt die aktuelle Position an
        public double getposition()
        {
            if (DXvideo != null)
            {
                return DXvideo.CurrentPosition;
            }
            else
                return 0;
        }
        //setzt die aktuelle position
        public void setposition(double position)
        {
            if (DXvideo != null)
            {
                DXvideo.CurrentPosition = position;
            }
        }
        public void setVolume(int Volumeneu)
        {
            if (DXvideo != null)
            {
                this.Volume = Volumeneu;
                DXvideo.Audio.Volume = this.Volume;
            }
        }

        public float getVolume()
        {
            if (DXvideo != null)
            {
                this.Volume = DXvideo.Audio.Volume;
                return this.Volume;
            }
            else
                return 0;
        }

        public void muting()
        {
            if (DXvideo != null)
            {
                if (this.mute == false)
                {
                    this.mute = true;
                    DXvideo.Audio.Volume = 0;
                }
                if (this.mute == true)
                {
                    this.mute = false;
                    DXvideo.Audio.Volume = this.Volume;
                }
            }
        }

        public void fullscreen()
        {
            if (DXvideo != null)
            {
                DXvideo.Fullscreen = true;
            }
        }

       void video_Ending(object sender, EventArgs e)
        {
            OpenVideo();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Video_w
            // 
            this.Name = "Video_w";
            this.Load += new System.EventHandler(this.Video_w_Load);
            this.ResumeLayout(false);

        }

        private void Video_w_Load(object sender, EventArgs e)
        {

        }
    }

}
