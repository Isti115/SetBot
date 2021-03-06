﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace SetBot
{
    public partial class MainForm : Form
    {
        public bool settingArea = false;
        public Rectangle area = new Rectangle();

        Bitmap bmp;

        public Point cardStart;
        public Size cardSize;
        public Size gapSize;

        public bool cardsMeasured;

        public Card[,] cardArray;

        public List<int[]> SETs = new List<int[]>();
        public List<string> SETStrings = new List<string>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cardsMeasured = false;
        }

        private void setAreaButton_Click(object sender, EventArgs e)
        {
            if (!settingArea)
            {
                area.X = Cursor.Position.X;
                area.Y = Cursor.Position.Y;

                settingArea = true;
                setAreaButton.BackColor = SystemColors.ControlDark;
            }

            else
            {
                area.Width = Cursor.Position.X - area.X;
                area.Height = Cursor.Position.Y - area.Y;

                settingArea = false;
                setAreaButton.BackColor = SystemColors.Control;
            }
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            bmp = new Bitmap(area.Width, area.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(area.X, area.Y, 0, 0, area.Size);
        }

        private void findCardsButton_Click(object sender, EventArgs e)
        {
            if (cardsMeasured)
            {
                return;
            }

            int currentX, currentY;

            #region X axis

            currentX = 50;
            currentY = 0;

            while (!isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentY++;
            }

            while (isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentX--;
            }
            currentX++;

            cardStart.X = currentX;

            while (isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentX++;
            }

            cardSize.Width = (currentX - 1) - cardStart.X;

            while (!isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentX++;
            }

            gapSize.Width = (currentX - 1) - (cardStart.X + cardSize.Width);

            #endregion

            #region Y axis

            currentX = 0;
            currentY = 50;

            while (!isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentX++;
            }

            while (isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentY--;
            }
            currentY++;

            cardStart.Y = currentY;

            while (isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentY++;
            }

            cardSize.Height = (currentY - 1) - cardStart.Y;

            while (!isWhite(bmp.GetPixel(currentX, currentY)))
            {
                currentY++;
            }

            gapSize.Height = (currentY - 1) - (cardStart.Y + cardSize.Height);

            #endregion

            cardsMeasured = true;
        }

        private void analyzeButton_Click(object sender, EventArgs e)
        {
            cardArray = new Card[4, 4];

            for (int i = 0; i < 4 * 4; i++)
            {
                Card currentCard = new Card();

                Point currentCardStart = new Point(
                      cardStart.X + (cardSize.Width + gapSize.Width) * (i % 4),
                      cardStart.Y + (cardSize.Height + gapSize.Height) * (i / 4)
                      );

                int currentX, currentY;
                currentX = currentCardStart.X;
                currentY = currentCardStart.Y;

                if (isWhite(bmp.GetPixel(currentX, currentY)))
                {
                    int center, topWidth, bottomWidth;

                    #region color

                    for (int j = 0; j < 3; j++)
                    {
                        currentX = currentCardStart.X;
                        currentY++;

                        while (isWhite(bmp.GetPixel(currentX, currentY)))
                        {
                            currentX++;
                            if (currentX - currentCardStart.X > cardSize.Width)
                            {
                                currentX = currentCardStart.X;
                                currentY++;
                            }
                        }
                    }

                    currentCard.color = getCardColor(bmp.GetPixel(currentX, currentY));

                    #endregion

                    center = currentX;

                    while (!isWhite(bmp.GetPixel(currentX, currentY)))
                    {
                        currentX++;
                    }

                    topWidth = currentX - center;
                    center = (center + currentX) / 2;

                    #region count

                    int count = 1;

                    while (currentX - currentCardStart.X < cardSize.Width)
                    {
                        if (!isWhite(bmp.GetPixel(currentX, currentY)))
                        {
                            while (!isWhite(bmp.GetPixel(currentX, currentY)))
                            {
                                currentX++;
                            }

                            count++;
                        }

                        currentX++;
                    }

                    currentCard.count = (CardCount)count;

                    #endregion

                    #region fill

                    currentX = center;
                    count = 0;

                    int bottomY = -1;

                    while (currentY - currentCardStart.Y < cardSize.Height)
                    {
                        if (!isWhite(bmp.GetPixel(currentX, currentY)))
                        {
                            while (!isWhite(bmp.GetPixel(currentX, currentY)))
                            {
                                currentY++;
                            }

                            bottomY = currentY - 2;
                            count++;
                        }

                        currentY++;
                    }

                    currentCard.fill = getCardFill(count);

                    #endregion

                    #region type

                    currentY = bottomY;

                    while (!isWhite(bmp.GetPixel(currentX, currentY)))
                    {
                        currentX--;
                    }
                    currentX++;

                    bottomWidth = currentX;

                    while (!isWhite(bmp.GetPixel(currentX, currentY)))
                    {
                        currentX++;
                    }
                    currentX--;

                    bottomWidth = currentX - bottomWidth;

                    currentCard.type = getCardType(topWidth, bottomWidth);

                    #endregion
                }

                else
                {
                    currentCard = null;
                }

                cardArray[i % 4, i / 4] = currentCard;
            }
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            string output = "";

            for (int i = 0; i < 4 * 4; i++)
            {
                output += cardArray[i % 4, i / 4].ToString() + "|";
                if (i % 4 == 3)
                {
                    output += System.Environment.NewLine;
                }
            }

            outputLabel.Text = output;
        }

        private void drawCardBoundsButton_Click(object sender, EventArgs e)
        {
            Graphics graphics = Graphics.FromImage(bmp);

            for (int i = 0; i < 4 * 4; i++)
            {
                graphics.DrawRectangle(Pens.Black,
                    cardStart.X + (cardSize.Width + gapSize.Width) * (i % 4),
                    cardStart.Y + (cardSize.Height + gapSize.Height) * (i / 4),
                    cardSize.Width, cardSize.Height);
            }
        }

        private void saveImageButton_Click(object sender, EventArgs e)
        {
            bmp.Save(@"C:\Users\isti\Desktop\test.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public bool isWhite(Color color)
        {
            int whiteTreshold = 165;
            return color.R > whiteTreshold && color.G > whiteTreshold && color.B > whiteTreshold;
        }

        public CardColor getCardColor(Color color)
        {
            if (color.R > color.G && color.R > color.B)
            {
                return CardColor.Red;
            }

            else if (color.G > color.R && color.G > color.B)
            {
                return CardColor.Green;
            }

            else if (color.B > color.R && color.B > color.G)
            {
                return CardColor.Blue;
            }

            else
            {
                throw new ColorNotRecognizedException();
            }
        }

        public class ColorNotRecognizedException : Exception { }

        public CardFill getCardFill(int count)
        {
            if (count == 1)
            {
                return CardFill.Filled;
            }

            else if (count == 2)
            {
                return CardFill.Empty;
            }

            else if (count > 2)
            {
                return CardFill.Striped;
            }

            else
            {
                throw new FillNotRecognisedException();
            }
        }

        public class FillNotRecognisedException : Exception { }

        public CardType getCardType(int topWidth, int bottomWidth)
        {
            int separator = 35;

            if (topWidth < separator && bottomWidth < separator)
            {
                return CardType.Oval;
            }

            else if (topWidth < separator && bottomWidth > separator)
            {
                return CardType.Triangle;
            }

            else if (topWidth > separator && bottomWidth > separator)
            {
                return CardType.Rectangle;
            }

            else
            {
                throw new TypeNotRecognisedException();
            }
        }

        public class TypeNotRecognisedException : Exception { }

        public class Card
        {
            public CardCount count;
            public CardColor color;
            public CardType type;
            public CardFill fill;

            public override string ToString()
            {
                return ((int)count).ToString() + color.ToString()[0] + type.ToString()[0] + fill.ToString()[0];
            }
        }

        public enum CardCount { One = 1, Two = 2, Three = 3 };
        public enum CardColor { Red, Green, Blue };
        public enum CardType { Rectangle, Oval, Triangle };
        public enum CardFill { Empty, Striped, Filled };

        // Here comes the part with SETs

        private void findSetButton_Click(object sender, EventArgs e)
        {
            SETs.Clear();
            SETStrings.Clear();

            for (int i = 0; i < 4 * 4; i++)
            {
                for (int j = i + 1; j < 4 * 4; j++)
                {
                    for (int k = j + 1; k < 4 * 4; k++)
                    {
                        if (isSET(
                            cardArray[i % 4, i / 4],
                            cardArray[j % 4, j / 4],
                            cardArray[k % 4, k / 4]
                            ))
                        {
                            //MessageBox.Show(makeSETString(i, j, k));
                            SETs.Add(new int[] { i, j, k });
                            SETStrings.Add(makeSETString(i, j, k));
                        }
                    }
                }
            }
        }

        public bool isSET(params Card[] cards)
        {
            bool result = true;

            result = result && cards[0] != null && cards[1] != null && cards[2] != null;

            result = result && (
               (cards[0].count == cards[1].count && cards[1].count == cards[2].count && cards[2].count == cards[0].count)
               ||
               (cards[0].count != cards[1].count && cards[1].count != cards[2].count && cards[2].count != cards[0].count)
               );

            result = result && (
               (cards[0].color == cards[1].color && cards[1].color == cards[2].color && cards[2].color == cards[0].color)
               ||
               (cards[0].color != cards[1].color && cards[1].color != cards[2].color && cards[2].color != cards[0].color)
               );

            result = result && (
               (cards[0].fill == cards[1].fill && cards[1].fill == cards[2].fill && cards[2].fill == cards[0].fill)
               ||
               (cards[0].fill != cards[1].fill && cards[1].fill != cards[2].fill && cards[2].fill != cards[0].fill)
               );

            result = result && (
               (cards[0].type == cards[1].type && cards[1].type == cards[2].type && cards[2].type == cards[0].type)
               ||
               (cards[0].type != cards[1].type && cards[1].type != cards[2].type && cards[2].type != cards[0].type)
               );

            return result;
        }

        public string makeSETString(params int[] cards)
        {
            string output = "|";

            for (int i = 0; i < 4 * 4; i++)
            {
                if (cards.Contains(i))
                {
                    output += "x|";
                }

                else
                {
                    output += " |";
                }

                if (i % 4 == 3)
                {
                    output += System.Environment.NewLine + "|";
                }
            }

            output = output.Substring(0, output.Length - 2);

            return output;
        }

        private void outputLabel_Click(object sender, EventArgs e)
        {
            outputLabel.Text = SETStrings[(SETStrings.IndexOf(outputLabel.Text) + 1) % SETStrings.Count];
        }

        private void CFAFOButton_Click(object sender, EventArgs e)
        {
            do
            {
                captureButton_Click(sender, e);
                findCardsButton_Click(sender, e);
                analyzeButton_Click(sender, e);
                findSetButton_Click(sender, e);

                if (SETs.Count == 0)
                {
                    automaticCheckBox.Checked = false;
                    return;
                }

                outputLabel_Click(sender, e);
                takeSetButton_Click(sender, e);

                new System.Threading.ManualResetEvent(false).WaitOne(150);
            } while (automaticCheckBox.Checked);
        }

        private void takeSetButton_Click(object sender, EventArgs e)
        {
            Point startPosition = Cursor.Position;

            for (int i = 0; i < 3; i++)
            {
                mouseClick(
                    area.X + cardStart.X + (cardSize.Width + gapSize.Width) * (SETs[0][i] % 4) + (cardSize.Width / 2),
                    area.Y + cardStart.Y + (cardSize.Height + gapSize.Height) * (SETs[0][i] / 4) + (cardSize.Height / 2)
                    );

                System.Threading.Thread.Sleep(100);
            }

            Cursor.Position = startPosition;
        }

        // copy-pasted mouse click
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);
        public void mouseClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            mouse_event(0x0002, 0, 0, 0, 0);//make left button down
            mouse_event(0x0004, 0, 0, 0, 0);//make left button up
        }
    }
}
