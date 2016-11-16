using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using WpfMvvmLib;

namespace WebcamTesterWPF
{
    public class MainViewModel : ViewModelBase
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        private VideoCaptureDevice _device;

        public MainViewModel()
        {
            Webcams = new ObservableCollection<FilterInfo>();
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevices)
            {
                Webcams.Add(videoDevice);
            }
            if (Webcams.Any())
            {
                SelectedWebcam = Webcams[0];
                OnPropertyChanged("SelectedWebcam");
            }

            PlayCmd = new RelayCommand(CanPlay, Play);
            StopCmd = new RelayCommand(CanStop, Stop);
            ClosingCmd = new RelayCommand(o => true, Closing);
        }

        private void Closing(object obj)
        {
            StopPreview();
        }

        private void StopPreview()
        {
            if (_device != null)
            {
                _device.SignalToStop();
                _device.WaitForStop();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Stop(object obj)
        {
            StopPreview();
        }

        private bool CanStop(object arg)
        {
            return _device != null && _device.IsRunning;
        }

        private void Play(object o)
        {
            _device = new VideoCaptureDevice(SelectedWebcam.MonikerString);
            _device.NewFrame += (sender, args) =>
            {
                var bitmap = args.Frame;
                if (bitmap != null)
                {
                    if (Application.Current != null)
                    {
                        var dispatcher = Application.Current.Dispatcher;
                        if (dispatcher != null)
                        {
                            dispatcher.Invoke(() =>
                            {
                                ImgPreview = ToBitmapSource(bitmap);
                                OnPropertyChanged("ImgPreview");
                            });
                        }
                    }
                }
            };
            _device.Start();
        }

        private static BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        private bool CanPlay(object o)
        {
            return SelectedWebcam != null && (_device == null || !_device.IsRunning);
        }

        public RelayCommand PlayCmd { get; set; }
        public RelayCommand ClosingCmd { get; set; }
        public RelayCommand StopCmd { get; set; }
        public ImageSource ImgPreview { get; set; }
        public FilterInfo SelectedWebcam { get; set; }
        public ObservableCollection<FilterInfo> Webcams { get; set; }
    }
}