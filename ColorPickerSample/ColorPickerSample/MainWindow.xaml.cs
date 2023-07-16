using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.Runtime.InteropServices;

namespace ColorPickerSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public WriteableBitmap Bitmap { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int width = 300;
            int height = 200;
            Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            DataContext = this;

            Bitmap.Lock();
            IntPtr backBuffer = Bitmap.BackBuffer;
            int stride = Bitmap.BackBufferStride;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r, g, b;

                    double normalizedX = (double)x / (width - 1);
                    double normalizedY = (double)y / (height - 1);

                    HSVToRGB(normalizedX, normalizedY, 1, out r, out g, out b);

                    int pixelOffset = y * stride + x * 4;
                    Marshal.WriteByte(backBuffer, pixelOffset + 0, b);
                    Marshal.WriteByte(backBuffer, pixelOffset + 1, g);
                    Marshal.WriteByte(backBuffer, pixelOffset + 2, r);
                    Marshal.WriteByte(backBuffer, pixelOffset + 3, 0xFF);
                }
            }

            Bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            Bitmap.Unlock();
            Canvas.SetLeft(thumb, 50);
            Canvas.SetTop(thumb, 50);
            var thumbPosition = new Point(50, 50);
            GetAreaColor(thumbPosition);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            double newLeft = Canvas.GetLeft(thumb) + e.HorizontalChange;
            double newTop = Canvas.GetTop(thumb) + e.VerticalChange;
            double canvasRight = canvas.ActualWidth - thumb.ActualWidth;
            double canvasBottom = canvas.ActualHeight - thumb.ActualHeight;

            if (newLeft < 0)
                newLeft = 0;
            else if (newLeft > canvasRight)
                newLeft = canvasRight;

            if (newTop < 0)
                newTop = 0;
            else if (newTop > canvasBottom)
                newTop = canvasBottom;

            Canvas.SetLeft(thumb, newLeft);
            Canvas.SetTop(thumb, newTop);

            GetAreaColor();
        }


        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvasPosition = e.GetPosition(canvas);
            double newLeft = canvasPosition.X - thumb.ActualWidth / 2;
            double newTop = canvasPosition.Y - thumb.ActualHeight / 2;

            double canvasRight = canvas.ActualWidth - thumb.ActualWidth;
            double canvasBottom = canvas.ActualHeight - thumb.ActualHeight;

            if (newLeft < 0)
                newLeft = 0;
            else if (newLeft > canvasRight)
                newLeft = canvasRight;

            if (newTop < 0)
                newTop = 0;
            else if (newTop > canvasBottom)
                newTop = canvasBottom;

            Canvas.SetLeft(thumb, newLeft);
            Canvas.SetTop(thumb, newTop);
            var thumbPosition = e.GetPosition(canvas);
            GetAreaColor(thumbPosition);
        }

        void GetAreaColor(Point? thumbPosition = null)
        {
            thumbPosition = thumbPosition == null ? thumbPosition = thumb.TranslatePoint(new Point(thumb.ActualWidth / 2, thumb.ActualHeight / 2), canvas) : thumbPosition;
            int xCoordinate = (int)thumbPosition?.X;
            int yCoordinate = (int)thumbPosition?.Y;

            if (xCoordinate >= 0 && xCoordinate < Bitmap.PixelWidth && yCoordinate >= 0 && yCoordinate < Bitmap.PixelHeight)
            {
                int stride = Bitmap.PixelWidth * (Bitmap.Format.BitsPerPixel / 8);
                byte[] pixels = new byte[Bitmap.PixelHeight * stride];
                Bitmap.CopyPixels(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight), pixels, stride, 0);
                int pixelIndex = (yCoordinate * stride) + (xCoordinate * (Bitmap.Format.BitsPerPixel / 8));
                Color color = Color.FromArgb(pixels[pixelIndex + 3], pixels[pixelIndex + 2], pixels[pixelIndex + 1], pixels[pixelIndex]);
                MyBtn.Background = new SolidColorBrush(color);
            }
        }

        private static void HSVToRGB(double h, double s, double v, out byte r, out byte g, out byte b)
        {
            if (s == 0)
            {
                r = g = b = (byte)(v * 255);
            }
            else
            {
                double hue = h * 6.0;
                int i = (int)Math.Floor(hue);
                double f = hue - i;
                double p = v * (1.0 - s);
                double q = v * (1.0 - (s * f));
                double t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = (byte)(v * 255);
                        g = (byte)(t * 255);
                        b = (byte)(p * 255);
                        break;
                    case 1:
                        r = (byte)(q * 255);
                        g = (byte)(v * 255);
                        b = (byte)(p * 255);
                        break;
                    case 2:
                        r = (byte)(p * 255);
                        g = (byte)(v * 255);
                        b = (byte)(t * 255);
                        break;
                    case 3:
                        r = (byte)(p * 255);
                        g = (byte)(q * 255);
                        b = (byte)(v * 255);
                        break;
                    case 4:
                        r = (byte)(t * 255);
                        g = (byte)(p * 255);
                        b = (byte)(v * 255);
                        break;
                    default:
                        r = (byte)(v * 255);
                        g = (byte)(p * 255);
                        b = (byte)(q * 255);
                        break;
                }
            }
        }
    }
}
