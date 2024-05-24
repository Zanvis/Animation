using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimationPL2
{
    public partial class Form1 : Form
    {
        private List<Ball> apballs = new List<Ball>();
        private Random aprandom = new Random();
        int apinterval = 5, apnumber = 12;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = apinterval;
            for (int i = 0; i < apnumber; i++)
            {
                apballs.Add(new Ball(pictureBox1, aprandom));
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Interval = apinterval;
            timer1.Start();

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckAndResolveCollisions();
            foreach (var apball in apballs)
            {
                apball.Move();
                apball.UpdateParticles();
            }

            pictureBox1.Invalidate();
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            foreach (var apball in apballs)
            {
                apball.Draw(e.Graphics);
                apball.DrawParticles(e.Graphics);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (var apball in apballs)
            {
                apball.Reset();
            }
            pictureBox1.Invalidate();
        }

        private void btnLosuj_Click(object sender, EventArgs e)
        {
            int apcurrentCount = apballs.Count;
            int apdesiredCount = apnumber;

            if (apcurrentCount < apdesiredCount)
            {
                for (int i = apcurrentCount; i < apdesiredCount; i++)
                {
                    apballs.Add(new Ball(pictureBox1, aprandom));
                }
            }
            else if (apcurrentCount > apdesiredCount)
            {
                apballs = apballs.Take(apdesiredCount).ToList();
            }
            foreach (var apball in apballs)
            {
                apball.RandomizePosition();
            }
            pictureBox1.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value > 5)
                apinterval = trackBar1.Value * 3;
            else if (trackBar1.Value <= 5)
                apinterval = trackBar1.Value;
            if (apinterval < 1)
                apinterval = 1;
            timer1.Interval = apinterval;
        }

        private void btnStart_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Play the animation", btnStart);
        }

        private void btnStop_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Stop the animation", btnStop);
        }

        private void btnReset_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Move back to the initial position of the balls", btnReset);
        }

        private void btnLosuj_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Draw new ball positions", btnLosuj);
        }
        private void CheckAndResolveCollisions()
        {
            for (int i = 0; i < apballs.Count; i++)
            {
                for (int j = i + 1; j < apballs.Count; j++)
                {
                    Ball apball1 = apballs[i];
                    Ball apball2 = apballs[j];
                    float apdx = apball2.apX - apball1.apX;
                    float apdy = apball2.apY - apball1.apY;
                    float apdistance = (float)Math.Sqrt(apdx * apdx + apdy * apdy);
                    if (apdistance < (apball1.apSize / 2) + (apball2.apSize / 2))
                    {
                        float aptempVx = apball1.apVelocityX;
                        float aptempVy = apball1.apVelocityY;
                        apball1.apVelocityX = apball2.apVelocityX;
                        apball1.apVelocityY = apball2.apVelocityY;
                        apball2.apVelocityX = aptempVx;
                        apball2.apVelocityY = aptempVy;
                        apball1.GenerateExplosion(aprandom);
                        apball2.GenerateExplosion(aprandom);
                    }
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            apnumber = (int)numericUpDown1.Value;
        }
    }
    public class Ball
    {
        public float apX, apY, apVelocityX, apVelocityY, apSize;
        public Color apColor;
        private PictureBox appictureBox;
        private List<PointF> aptrail = new List<PointF>();
        private const int apTrailLength = 12;
        private Random aprandom;
        private int apinitialX, apinitialY;
        private float apinitialVelocityX, apinitialVelocityY;
        private List<Particle> explosionParticles = new List<Particle>();

        public Ball(PictureBox appb, Random aprandom)
        {
            appictureBox = appb;
            this.aprandom = aprandom;
            Initialize();
        }

        public void Initialize()
        {
            int apminEdgeDistance = 10;
            apSize = aprandom.Next(12, 25);

            apinitialX = aprandom.Next((int)apSize + apminEdgeDistance, appictureBox.Width - (int)apSize - apminEdgeDistance);
            apinitialY = aprandom.Next((int)apSize + apminEdgeDistance, appictureBox.Height - (int)apSize - apminEdgeDistance);

            do
            {
                apinitialVelocityX = (float)(aprandom.NextDouble() * 6 - 2);
            } while (Math.Abs(apinitialVelocityX) < 0.5);

            do
            {
                apinitialVelocityY = (float)(aprandom.NextDouble() * 6 - 2);
            } while (Math.Abs(apinitialVelocityY) < 0.5);

            apX = apinitialX;
            apY = apinitialY;
            apVelocityX = apinitialVelocityX;
            apVelocityY = apinitialVelocityY;

            apColor = Color.FromArgb(aprandom.Next(256), aprandom.Next(256), aprandom.Next(256));
        }

        public void Move()
        {
            apX += apVelocityX;
            apY += apVelocityY;

            if (apX <= 0 || apX >= appictureBox.Width - apSize) apVelocityX = -apVelocityX;
            if (apY <= 0 || apY >= appictureBox.Height - apSize) apVelocityY = -apVelocityY;

            aptrail.Insert(0, new PointF(apX, apY));
            if (aptrail.Count > apTrailLength) aptrail.RemoveAt(aptrail.Count - 1);
        }

        public void Draw(Graphics g)
        {
            using (Brush apbrush = new SolidBrush(apColor))
            {
                g.FillEllipse(apbrush, apX, apY, apSize, apSize);
            }

            for (int i = 0; i < aptrail.Count; i++)
            {
                float apalpha = 1 - (float)i / apTrailLength;
                using (Brush apbrush = new SolidBrush(Color.FromArgb((int)(apalpha * 255), apColor)))
                {
                    float apsize = apSize * apalpha;
                    g.FillEllipse(apbrush, aptrail[i].X + (apSize - apsize) / 2, aptrail[i].Y + (apSize - apsize) / 2, apsize, apsize);
                }
            }
        }
        public void Reset()
        {
            apX = apinitialX;
            apY = apinitialY;
            apVelocityX = apinitialVelocityX;
            apVelocityY = apinitialVelocityY;
        }
        public void RandomizePosition()
        {
            int apminEdgeDistance = 10;
            apX = aprandom.Next((int)apSize + apminEdgeDistance, appictureBox.Width - (int)apSize - apminEdgeDistance);
            apY = aprandom.Next((int)apSize + apminEdgeDistance, appictureBox.Height - (int)apSize - apminEdgeDistance);

            apColor = Color.FromArgb(aprandom.Next(256), aprandom.Next(256), aprandom.Next(256));
        }
        public void GenerateExplosion(Random random)
        {
            for (int i = 0; i < 20; i++)
            {
                explosionParticles.Add(new Particle(this.apX + this.apSize / 2, this.apY + this.apSize / 2, random));
            }
        }
        public void UpdateParticles()
        {
            explosionParticles.ForEach(p => p.Update());
            explosionParticles = explosionParticles.Where(p => p.IsAlive()).ToList();
        }

        public void DrawParticles(Graphics g)
        {
            explosionParticles.ForEach(p => p.Draw(g));
        }
    }
    public class Particle
    {
        public float X, Y, VelocityX, VelocityY, Size;
        public Color Color;
        private float lifespan = 1.0f;

        public Particle(float x, float y, Random random)
        {
            X = x;
            Y = y;
            VelocityX = (float)(random.NextDouble() * 4 - 2);
            VelocityY = (float)(random.NextDouble() * 4 - 2);
            Size = random.Next(2, 5);
            Color = Color.FromArgb(255, random.Next(200, 256), random.Next(100, 256), random.Next(50, 200));
        }

        public bool IsAlive() => lifespan > 0;

        public void Update()
        {
            X += VelocityX;
            Y += VelocityY;
            lifespan -= 0.05f;
        }

        public void Draw(Graphics g)
        {
            if (lifespan <= 0) return;
            using (Brush brush = new SolidBrush(Color.FromArgb((int)(lifespan * 255), Color)))
            {
                g.FillEllipse(brush, X - Size / 2, Y - Size / 2, Size, Size);
            }
        }
    }
}
