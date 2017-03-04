using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Required to support the core dispatcher and the accelerometer

using Windows.UI.Core;
using Windows.Devices.Sensors;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace SensorTest
{

    public sealed partial class MainPage : Page
    {
        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        // This event handler writes the current accelerometer reading to
        // the three acceleration text blocks on the app' s main page.

        double maxX = 0;
        double maxY = 0;
        //double maxZ = -1;
        double minX = 0;
        double minY = 0;
        double minZ = -1;
        double X = 0;
        double Y = 0;
        //double Z = 0;
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                X = reading.AccelerationX;
                Y = reading.AccelerationY;
                //Z = reading.AccelerationZ;

                if (X > maxX)
                    maxX = X;
                else if (X < minX)
                    minX = X;

                if (Y > maxY)
                    maxY = Y;
                else if (Y < minY)
                    minY = Y;

                //if (Z > maxZ)
                //    maxZ = Z;
                //else if (Z < minZ)
                //    minZ = Z;

                txtXAxis.Text = String.Format("{0,5:0.00}, {1,5:0.00}, {2,5:0.00}", X, minX, maxX);
                txtYAxis.Text = String.Format("{0,5:0.00}, {1,5:0.00}, {2,5:0.00}", Y, minY, maxY);
                //txtZAxis.Text = String.Format("{0,5:0.00}, {1,5:0.00}, {2,5:0.00}", Z, minZ, maxZ);

                DrawDotByPosition((X + Y) / 2, Colors.Green);
                //DrawDotByPosition(Y, Colors.Green);
                //DrawDotByPosition(Z, Colors.Blue);
            });
        }

        public MainPage()
        {
            this.InitializeComponent();

            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }

            // so numbers will start slowly
            for (int i = 0; i < 100; i++)
                list.Add(0);

            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        List<long> list = new List<long>();
        int notMovingSecs = 0;
        private void Timer_Tick(object sender, object e)
        {
            var diffX = Math.Abs(maxX - minX);
            var diffY = Math.Abs(maxY - minY);
            //var diffZ = Math.Abs(maxZ - minZ);
            if (diffX > 0.03 || diffY > 0.03)
            {
                double maxPoints = 2000;

                maxPoints -= diffX * 1000;
                maxPoints -= diffY * 1000;
                //maxPoints -= diffZ * 1000;

                list.Add(Convert.ToInt64(maxPoints));
                var avg = list.Average();

                minX = X;
                minY = Y;
                //minZ = Z;
                maxX = X;
                maxY = Y;
                //maxZ = Z;


                var last = Convert.ToDouble(outputTB.Text);
                var current = Math.Round(avg, 1);
                outputTB.Text = current.ToString(); //+ (last - current).ToString();
            }
            else
            {
                //outputTB.Text = "none";
            }        
            

        }
        double lastX = 0;

        public void DrawDotByPosition(double y, Color color)
        {
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;
            var midY = canvas.Height / 2;

            var dot = new Ellipse();
            dot.Fill = new SolidColorBrush(color);
            dot.Height = 1;
            dot.Width = 1;

            var yPoint = y < 0 ? midY + midY * y : midY - midY * y;

            //foreach (var item in canvas.Children)
            //    if (item.GetType() == typeof(Ellipse))
            //        if (((Ellipse)item).Margin.Top == lastX)
            //            canvas.Children.Remove(item);

            if (lastX > width)
            {
                lastX = 0;
                foreach (var item in canvas.Children)
                    if(item.GetType() == typeof(Ellipse))
                        canvas.Children.Remove(item);
            }

            dot.Margin = new Thickness(lastX, yPoint, 0, 0);

            canvas.Children.Add(dot);

            lastX += 0.1;
        }

    }
}