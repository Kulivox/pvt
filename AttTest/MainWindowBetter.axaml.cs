using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AttTest.AppStates;
using AttTest.AppStates.RelatedInterfaces;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using HarfBuzzSharp;
using ScottPlot.Avalonia;

namespace AttTest
{
    public partial class MainWindowBetter : Window, IAttTestWindow
    {

        private BaseAppState? _appState;
        
        private TimerConstants? _constants;

        private object _lock = new object();

        private bool _gameEnded = false;
        
        private string? _pathToConstants;

        private List<(int, string, string)>? _results;

        private List<(string, string, string)> _focusPointsAndPresses;
        
        private string _fname = String.Empty;

        private DateTime? StartOfTest { get; set; }

        private DateTime? _endOfTest;

        public MainWindowBetter()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void UpdateState(BaseAppState newState)
        {
            _appState = newState;
        }

        private void EventLoop()
        {
            StartOfTest = DateTime.Now;
            _endOfTest = DateTime.Now.AddSeconds(_constants.TestLengthSeconds);

            Task.Factory.StartNew(() =>
            {
                while (!_gameEnded)
                {
                    Tick();
                }
            });

        }
        
        private void Tick()
        {
            lock (_lock)
            {
                if (_gameEnded)
                {
                    return;
                }
                
                var now = DateTime.Now;

                if (now > _endOfTest)
                {
                    _gameEnded = true;
                    EndTest();
                    
                    return;
                }
            
                _appState?.HandleTimerTick(now);
            }
            
        }

        private void KeyPress(object? sender, KeyEventArgs args)
        {
            lock (_lock)
            {
                _appState?.HandleKeyPress(DateTime.Now);
            }   
        }
        
        

        public void HideFocusPoint()
        {
            Dispatcher.UIThread.Post(() => this.FindControl<Ellipse>("FocusPoint").IsVisible = false);
        }

        public void ShowFocusPoint()
        {
            Dispatcher.UIThread.Post(() => this.FindControl<Ellipse>("FocusPoint").IsVisible = true);
            
        }

        private void DisplayAndSaveResult(string result, string note, bool success)
        {
            _results?.Add((_appState.RoundId, result, note));
            
            Dispatcher.UIThread.Post(() =>
            {
                var tb = this.FindControl<TextBlock>("RoundResult");

                var color = Colors.Green;
                if (!success)
                {
                    color = Colors.Red;
                    tb.Text = note;

                }
                else
                {
                    result += " ms";
                    tb.Text = result;

                }
            
                tb.Foreground = new SolidColorBrush(color);
                tb.IsVisible = true;
            });
            
        }

        public void ShowFailure(string failureText, string note)
        {
            DisplayAndSaveResult(failureText, note, false);
        }

        public void ShowSuccess(string success, string note)
        {
            DisplayAndSaveResult(success, note, true);
        }

        public void SaveKeyPressTime(DateTime focus, DateTime keyPress, string type)
        {
            var relFocus = focus - StartOfTest;
            var relKeyPress = keyPress - StartOfTest;
            
            _focusPointsAndPresses.Add((relFocus.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), relKeyPress.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), type));
        }

        public void HideResult()
        {
            Dispatcher.UIThread.Post(() => this.FindControl<TextBlock>("RoundResult").IsVisible = false);
            
        }

        public void EndTest()
        {
            
            _appState = null;
            this.KeyDown -= KeyPress;
            
            Dispatcher.UIThread.Post(() =>
            {
                ShowRoundResults();
                this.FindControl<Grid>("TestGrid").IsVisible = false;
                this.FindControl<Grid>("ResultScreen").IsVisible = true;
            });
            
            
            var sb = new StringBuilder("roundId;result;addInfo\n");
            foreach (var (r, res, note) in _results)
            {
                sb.Append($"{r};{res};{note}\n");
            }

            File.WriteAllText(_fname, sb.ToString());

            sb.Clear();

            sb.Append("stimulus;keyPress;type\n");
            foreach (var (st, kp, type) in _focusPointsAndPresses)
            {
                sb.Append($"{st};{kp};{type}\n");
            }
            File.WriteAllText("time_table_" +  _fname, sb.ToString());
            
        }

        private void ShowRoundResults()
        {
            var plot = this.FindControl<AvaPlot>("RoundPlot");
            var dataX = new List<double>();
            var dataY = new List<double>();

            var misses = 0;
            var earlyStarts = 0;
            var averageResponseTime = 0;

            foreach (var (roundNum, roundResult, note) in _results)
            {
                if (int.TryParse(roundResult, out var result) && note == "")
                {
                    dataX.Add(roundNum);
                    dataY.Add(result);

                    averageResponseTime += result;
                }
                else
                {
                    if (note == "Too fast")
                    {
                        earlyStarts += 1;
                    }
                    else
                    {
                        misses += 1;
                    }
                }
            }
            
            if (dataX.Count > 0 && dataX.Count == dataY.Count)
            {
                plot.Plot.AddScatter(dataX.ToArray(), dataY.ToArray());
                plot.Plot.Title("Rounds and results");
                plot.Plot.XLabel("Round Id");
                plot.Plot.YLabel("Response time");
                plot.Refresh();
            }
            
            averageResponseTime =  dataX.Count != 0 ? averageResponseTime / dataX.Count : 0;
                
            this.FindControl<TextBlock>("AverageResponse").Text = $"Average response time: {averageResponseTime} ms";
            this.FindControl<TextBlock>("Misses").Text =$"Misses: {misses}";
            this.FindControl<TextBlock>("EarlyStarts").Text = $"Early starts: {earlyStarts}";
        }

        public void HideTestDescription()
        {
            this.FindControl<TextBlock>("InfoText").IsVisible = false;
        }
        
        
    }
}