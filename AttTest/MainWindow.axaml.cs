using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Path = System.IO.Path;

namespace AttTest
{
    public partial class MainWindow : Window
    {
        private TimerConstants _constants;
        private string? _pathToConstants;
        
        private readonly DispatcherTimer _introDispatch = new DispatcherTimer();
        private readonly DispatcherTimer _untilVisibleTimer = new DispatcherTimer();
        private readonly DispatcherTimer _visibleTimer = new DispatcherTimer();
        private readonly DispatcherTimer _gameLengthTimer = new DispatcherTimer();
        private readonly Stopwatch _reactionStopWatch = new Stopwatch();

        private int _round = 0;
        private List<(int, string)> _results = new List<(int, string)>(); 
        public MainWindow()
        {
            _constants = new TimerConstants()
            {
                FocusPointVisibilityLength = 355,
                IntroLengthSeconds = 2,
                RoundLengthMinSeconds = 2,
                RoundLengthMaxSeconds = 5,
                TestLengthSeconds = 20
            };

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void FilePathButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var files = openFileDialog.ShowAsync(this).Result;

            if (files is null)
            {
                return;
            }
            
            _pathToConstants = files[0];
            this.FindControl<Button>("FilePathButton").Content = Path.GetFileName(_pathToConstants);
        }

        private void RunButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var nameField = this.FindControl<TextBox>("NameInput");
            if (string.IsNullOrWhiteSpace(nameField.Text) ||
                nameField.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                this.FindControl<TextBlock>("NameError").Text = "Name contains invalid characters or is empty";
                return;
            }

            if (string.IsNullOrWhiteSpace(_pathToConstants))
            {
                this.FindControl<TextBlock>("NameError").Text = "No configuration loaded";
                return;
            }

            var constants = TimerConstantsProvider.GetTimerConstants(_pathToConstants);
            if (constants is null)
            {
                this.FindControl<TextBlock>("NameError").Text = "Invalid format of configuration file";
                return;
            }
            _constants = constants;
            
            
            this.FindControl<Grid>("MainMenu").IsVisible = false;
            this.FindControl<Grid>("TestGrid").IsVisible = true;

            _introDispatch.Interval = TimeSpan.FromSeconds(_constants.IntroLengthSeconds);
            _introDispatch.Tick += IntroText_Tick;
            _introDispatch.Start();
        }
        private void IntroText_Tick(object? sender, EventArgs e)
        {
            _introDispatch.Stop();
            this.FindControl<TextBlock>("InfoText").IsVisible = false;
            _gameLengthTimer.Interval = TimeSpan.FromSeconds(_constants.TestLengthSeconds);
            _gameLengthTimer.Tick += EndGame;
            _gameLengthTimer.Start();
            StartRound();
        }

        private void EndGame(object? sender, EventArgs e)
        {
            _gameLengthTimer.Stop();
            _gameLengthTimer.Tick -= EndGame;
            
            _untilVisibleTimer.Stop();
            _visibleTimer.Stop();
            _untilVisibleTimer.Tick -= ShowFocusPoint_Tick;
            _visibleTimer.Tick -= MissedRound_Tick;
            KeyDown -= ReactToFocusPoint_KeyDown;

            var fName = this.FindControl<TextBox>("NameInput").Text.Trim() + "_"
                                                                    + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

            var sb = new StringBuilder("roundId,result\n");
            foreach (var (r, res) in _results)
            {
                sb.Append($"{r},{res}\n");
            }

            File.WriteAllText(fName, sb.ToString());

            _results = new List<(int, string)>();
            _round = 0;
                
            this.FindControl<Grid>("TestGrid").IsVisible = false;
            this.FindControl<Grid>("ResultScreen").IsVisible = true;
            
        }
        
        private void StartRound()
        {
            _round += 1;
            
            this.FindControl<Ellipse>("FocusPoint").IsVisible = false;
            _untilVisibleTimer.Interval = TimeSpan
                .FromSeconds(
                    new Random().NextDouble()
                    * (_constants.RoundLengthMaxSeconds - _constants.RoundLengthMinSeconds)
                    + _constants.RoundLengthMinSeconds);
            _untilVisibleTimer.Tick += ShowFocusPoint_Tick;
            _untilVisibleTimer.Start();
        }

        private void ShowFocusPoint_Tick(object? sender, EventArgs e)
        {
            _untilVisibleTimer.Stop();
            _untilVisibleTimer.Tick -= ShowFocusPoint_Tick;
            
            this.FindControl<Ellipse>("FocusPoint").IsVisible = true;

            _visibleTimer.Interval = TimeSpan.FromMilliseconds(_constants.FocusPointVisibilityLength);
            _visibleTimer.Tick += MissedRound_Tick;
            
            _reactionStopWatch.Start();
            this.KeyDown += ReactToFocusPoint_KeyDown;
            _visibleTimer.Start();
            

        }

        private void ReactToFocusPoint_KeyDown(object? sender, KeyEventArgs e)
        {
            _reactionStopWatch.Stop();
            _visibleTimer.Stop();
            _visibleTimer.Tick -= MissedRound_Tick;
            this.KeyDown -= ReactToFocusPoint_KeyDown;
            
            var tb = this.FindControl<TextBlock>("RoundResult");
            
            tb.Text = _reactionStopWatch.Elapsed.Milliseconds.ToString();
            _results.Add((_round, _reactionStopWatch.ElapsedMilliseconds.ToString()));
            tb.Foreground = new SolidColorBrush(Colors.Green);
            
            _reactionStopWatch.Reset();
            StartRound();
        }

        private void MissedRound_Tick(object? sender, EventArgs e)
        {
            _visibleTimer.Stop();
            _reactionStopWatch.Stop();
            
            this.KeyDown -= ReactToFocusPoint_KeyDown;
            
            var tb = this.FindControl<TextBlock>("RoundResult");
            tb.Text = "Missed round";
            _results.Add((_round, "miss"));
            tb.Foreground = new SolidColorBrush(Colors.Red);
            
            _reactionStopWatch.Reset();
            _visibleTimer.Tick -= MissedRound_Tick;
            StartRound();
        }
        
    }
}