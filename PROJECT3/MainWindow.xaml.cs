using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PROJECT3
{
    public class SongInfo
    {
        public SongInfo()
        {
            Performer = "Unknown artist";
            Album = "Unknown Album";
        }
        string temp1;
        public string Path
        {
            get { return temp1; }
            set
            {
                temp1 = value;

            }
        }
        string temp2;
        public string Name
        {
            get
            {
                return temp2;
            }
            set
            {
                temp2 = value;

            }
        }
        private string temp3;
        public string Performer
        {
            get
            {
                return temp3;
            }
            set
            {
                temp3 = value;

            }
        }
        string temp4;
        public string Album
        {
            get
            {
                return temp4;
            }
            set
            {
                temp4 = value;

            }
        }
        string temp5;
        public string Title
        {
            get
            {
                return temp5;
            }
            set
            {
                temp5 = value;

            }
        }
        string temp6;
        public String Duration
        {
            get
            {
                return temp6;
            }
            set
            {
                temp6 = value;

            }
        }

    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String listName = "";
        int isRepeat = 1; // 1 : ko lap | 2: lap 1bai | 3 : Lap tat  ca
        bool isRandom = false;
        int index;
        bool isDragging = false;
        bool isPlaying = false;
        bool isSelected = false;
        BindingList<SongInfo> _playlist = new BindingList<SongInfo>();
        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer;
        int _lastIndex = -1;
        private IKeyboardMouseEvents m_GlobalHook;
        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
            volumnController.Value = volumnController.Maximum / 2;
            //
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playList.ItemsSource = _playlist;
            try
            {
                StreamReader sr = new StreamReader("lastPlaying");
                int lastSongIndex = int.Parse(sr.ReadLine());
                if (lastSongIndex!=-1)
                {
                    loadSongList("lastlist");
                    isSelected = true;
                    index = lastSongIndex;
                    PlaySelectedIndex(index);
                }
                sr.Close();
            }
            catch(Exception ex)
            {

            }
            try{
                m_GlobalHook = Hook.GlobalEvents();
                m_GlobalHook.KeyUp += KeyUp_hook;
            }
            catch(Exception ex)
            {
                MessageBoxError("Một số tập tin bị thiếu. Vui lòng cài đặt lại chương trình");
            }
        }
        bool isEmpty()
        {
            if (_playlist.Count() == 0)
            {
                return true;
            }
            return false;
        }
        private void _player_MediaEnded(object sender, EventArgs e)
        {

            if (isRepeat == 1) // repeat 1 bai
            {
                txtEllapsedTime.Text = "00:00:00";
                sliderProgress.Value = 0;
                _player.Stop();
                _timer.Stop();
                playBtn.Visibility = Visibility.Visible;
                return;
            }
            else if (isRepeat == 3)
            {
                if (isRandom)
                {
                    int temp = index;
                    do
                    {
                        Random random = new Random();
                        index = random.Next(0, _playlist.Count);
                    } while (temp == index);
                }
                else
                {

                    if (index == _playlist.Count - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            _timer.Stop();
            _timer.Start();
            PlaySelectedIndex(index);
            _player.Open(new Uri(_playlist[index].Path, UriKind.Absolute));
            _player.Play();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source != null)
            {
                System.Windows.Duration duration = _player.NaturalDuration;
                txtEllapsedTime.Text = _player.Position.ToString(@"hh\:mm\:ss"); ;
                TimeSpan ts;
                if (duration.HasTimeSpan)
                {
                    ts = _player.NaturalDuration.TimeSpan;
                    double total = ts.TotalSeconds;
                    TimeSpan now = _player.Position;
                    double current = now.TotalSeconds;
                    double percent = ((current * sliderProgress.Maximum / total));
                    if (!isDragging)
                    {

                        sliderProgress.Value = percent;
                    }
                }

            }
        }
        private void browserBtn_Click(object sender, RoutedEventArgs e)
        {

            var screen = new Microsoft.Win32.OpenFileDialog();

            screen.Filter = "Audio file (*.MP3;*.WMA;*.AAC;*.WAV;*.FLAC)|*.MP3;*.WMA;*.AAC;*.WAV;*.FLAC|Video file (*.MP4;*.3GP ;*.WMV ;*.AVI;*.FLV )|*.MP4;*.3GP ;*.WMV ;*.AVI;*.FLV ";
            screen.FilterIndex = 1;
            if (screen.ShowDialog() == true)
            {
                var info = new FileInfo(screen.FileName);
                var song = new SongInfo();

                //gan gia tri song
                song.Name = info.Name;
                song.Path = info.FullName;
                TagLib.File tagFile = TagLib.File.Create(song.Path);
                if (tagFile.Tag.Performers.Length > 0)
                    song.Performer = tagFile.Tag.Performers[0];
                if (tagFile.Tag.Album != null)
                    song.Album = tagFile.Tag.Album;
                song.Title = tagFile.Tag.Title;
                song.Duration = tagFile.Properties.Duration.ToString(@"hh\:mm\:ss");
                _playlist.Add(song);
            }
        }

        private void sliderProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void playList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {
            isSelected = true;
            if (playList.SelectedIndex >= 0)
            {
                _lastIndex = playList.SelectedIndex;
                PlaySelectedIndex(_lastIndex);
            }
        }

        private void PlaySelectedIndex(int i)
        {
            index = i;
            if (i < _playlist.Count)
            {
                txtEndTime.Text = _playlist[i].Duration;
                if (_playlist[i].Title == null)
                    txtSongTitle.Text = _playlist[i].Name;
                else txtSongTitle.Text = _playlist[i].Title;
            }

        }
        //Hàm click play, gọi xử lý play
        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            playSong();
        }
        //Hàm xử lý play
        private void playSong()
        {
            playBtn.Visibility = Visibility.Hidden;

            if (isPlaying == false || isSelected)
            {
                if (_playlist.Count == 0)
                {
                    MessageBoxError("Không có tập tin trong danh sách phát");
                    playBtn.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    if (playList.SelectedIndex >= 0)
                    {
                        index = playList.SelectedIndex;
                    }
                    else
                    {
                        PlaySelectedIndex(index);
                    }
                }
                _player.Open(new Uri(_playlist[index].Path, UriKind.Absolute));
            }

            _player.Play();
            isSelected = false;
            _timer.Start();
        }

        //Hàm click pause, gọi xử lý pause
        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            pauseSong();
        }
        //Hàm xử lý pause
        private void pauseSong()
        {
            playBtn.Visibility = Visibility.Visible;
            _player.Pause();
            _timer.Stop();
            isPlaying = true;
        }
        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            stopSong();
        }
        private void stopSong()
        {
            txtEllapsedTime.Text = "00:00:00";
            sliderProgress.Value = 0;
            _player.Stop();
            _timer.Stop();
            playBtn.Visibility = Visibility.Visible;
        }
        private void shuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleBtn.IsChecked == true)
            {
                isRandom = true;
                MyMessageBox("Đã bật chế độ phát ngẫu nhiên !");
            }
            else
            {
                isRandom = false;
                MyMessageBox("Đã tắt chế độ phát ngẫu nhiên !");
            }

        }
        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            // 1 : ko lap | 2: lap 1bai | 3 : Lap tat  ca
            isRepeat++;
            if (isRepeat == 4)
            {
                isRepeat = 1;
            }
            if (isRepeat == 2)
            {
                noneRepeat.Visibility = Visibility.Hidden;
                MyMessageBox("Lặp lại bài hát hiện tại !");
                //singleRepeat.Visibility = Visibility.Visible;
            }
            if (isRepeat == 3)
            {
                singleRepeat.Visibility = Visibility.Hidden;
                MyMessageBox("Lặp lại tất cả danh sách !");
            }
            if (isRepeat == 1)
            {
                noneRepeat.Visibility = Visibility.Visible;
                singleRepeat.Visibility = Visibility.Visible;
                MyMessageBox("Không lặp lại !");
            }

        }
        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new  OpenFileDialog();
            string appBaseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            openFileDlg.InitialDirectory = appBaseDir;
            openFileDlg.Filter = "Playlist (*.list*)|*.list*";
            openFileDlg.FilterIndex = 1;
            if (openFileDlg.ShowDialog()==true)
            {
                loadSongList(openFileDlg.FileName);
                MyMessageBox("Đã load Playlist !!");
                listName = openFileDlg.SafeFileName.Substring(0, openFileDlg.SafeFileName.Length - 5);
            }

        }
        private void loadSongList(string pathName)
        {
            _playlist.Clear();
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(pathName);
                line = sr.ReadLine();
                do
                {
                    var info = new FileInfo(line);
                    var song = new SongInfo();

                    //gan gia tri song
                    song.Name = info.Name;
                    song.Path = info.FullName;
                    TagLib.File tagFile = TagLib.File.Create(song.Path);
                    if (tagFile.Tag.Performers.Length > 0)
                        song.Performer = tagFile.Tag.Performers[0];
                    if (tagFile.Tag.Album != null)
                        song.Album = tagFile.Tag.Album;
                    song.Title = tagFile.Tag.Title;
                    song.Duration = tagFile.Properties.Duration.ToString(@"hh\:mm\:ss");
                    _playlist.Add(song);
                    line = sr.ReadLine();
                } while (line != null);
                sr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
        private void savebtn_Click(object sender, RoutedEventArgs e)
        {
            saveSongList();

        }
        private void saveSongList()
        {
            if (_playlist.Count==0)
            {
                MessageBoxError("Playlist rỗng");
                return;
            }
            try
            {
                var screen = new EditWindow(listName);
                if (screen.ShowDialog() == true)
                {

                    StreamWriter sw = new StreamWriter(screen.newPlaylist +".list");
                    for (int i = 0; i < _playlist.Count(); i++)
                    {
                        sw.WriteLine(_playlist[i].Path);
                    }

                    sw.Close();
                    listName= screen.newPlaylist;
                    MyMessageBox("Đã lưu Playlist !!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
        private void deleteSong_Click(object sender, RoutedEventArgs e)
        {
            MyMessageBox("OK");
            //_playlist.Remove(_playlist[playList.SelectedIndex]);
        }
        private void deleteMulti_Click(object sender, RoutedEventArgs e)
        {
            List<SongInfo> temp = new List<SongInfo>();
            foreach (SongInfo item1 in playList.SelectedItems)
            {
                temp.Add(item1);
            }
            foreach (SongInfo item2 in temp)
            {
                _playlist.Remove(item2);
            }

            playList.ItemsSource = _playlist;
        }
        //
        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            previousSong();
        }
        private void previousSong()
        {
            if (isEmpty())
            {
                return;
            }
            playBtn.Visibility = Visibility.Hidden;
            if (isRandom)
            {
                int temp = index;
                do
                {
                    Random random = new Random();
                    index = random.Next(0, _playlist.Count);
                } while (temp == index);
            }
            else
            {
                if (index == 0)
                {
                    index = _playlist.Count - 1;
                }
                else
                {
                    index--;
                }
            }
            _timer.Stop();
            _timer.Start();
            PlaySelectedIndex(index);
            _player.Open(new Uri(_playlist[index].Path, UriKind.Absolute));
            _player.Play();

        }
        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            nextSong();
        }
        private void nextSong()
        {
            if (isEmpty())
            {
                return;
            }
            playBtn.Visibility = Visibility.Hidden;
            if (isRandom)
            {
                int temp = index;
                do
                {
                    Random random = new Random();
                    index = random.Next(0, _playlist.Count);
                } while (temp == index);
            }
            else
            {
                if (index == _playlist.Count - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
            }
            _timer.Stop();
            _timer.Start();
            PlaySelectedIndex(index);
            _player.Open(new Uri(_playlist[index].Path, UriKind.Absolute));
            _player.Play();
        }
        private void KeyUp_hook(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Shift && (e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                nextSong();
            }
            if (e.Shift && (e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                previousSong();
            }
            // Chua kiem tra,  nhung hoat dong OK
            if (e.Shift && (e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                playSong();
            }
            if (e.Control && (e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                pauseSong();
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
            try
            {

                m_GlobalHook.KeyUp -= KeyUp_hook;
                m_GlobalHook.Dispose();
                StreamWriter sw = new StreamWriter("lastPlaying");
                if (_playlist.Count==0)
                {
                    index = -1;
                }
                    sw.WriteLine(index.ToString());
                sw.Close();
                        StreamWriter sq = new StreamWriter("lastlist");
                        for (int i = 0; i < _playlist.Count(); i++)
                        {
                            sq.WriteLine(_playlist[i].Path);
                        }

                        sq.Close();
            }
            catch(Exception ex)
            {

            }

        }

        private void volumnDown_Click(object sender, RoutedEventArgs e)
        {
            if (_player.Volume > 0)
            {
                _player.Volume -= 0.1;
            }
            else
            {
                _player.Volume = 0;
            }
            volumnController.Value = _player.Volume * 6;
        }

        private void volumeUp_Click(object sender, RoutedEventArgs e)
        {
            if (_player.Volume < 1)
            {
                _player.Volume += 0.1;
            }
            else
            {
                _player.Volume = 1;
            }
            volumnController.Value = _player.Volume * 6;
        }
        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            isDragging = false;
            double target = sliderProgress.Value;
            double total = _player.NaturalDuration.TimeSpan.TotalSeconds;
            double current = target * total / sliderProgress.Maximum;
            TimeSpan ts = TimeSpan.FromSeconds(current);
            _player.Pause();
            _player.Position = ts;
            _player.Play();

        }
        private void volumnController_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = volumnController.Value / sliderProgress.Maximum;

        }

        public void MyMessageBox(string messageBoxText)
        {
            string caption = "Notification";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    var index = listPlaylist.SelectedIndex;

        //    if (index == -1)
        //    {
        //        MessageBoxError("Error. Can not delete !!!");
        //        return;
        //    }
        //    _listPlaylist.RemoveAt(index);
        //}
        private void MessageBoxError(string messageBoxText)
        {
            string caption = "Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }
    }
}
