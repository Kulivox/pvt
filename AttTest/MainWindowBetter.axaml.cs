using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private readonly DispatcherTimer _eventLoopTimer;

        private BaseAppState? _appState;
        
        private TimerConstants? _constants;

        private object _lock = new object();

        private bool _gameEnded = false;
        
        private string? _pathToConstants;

        private List<(int, string)>? _results;

        private DateTime? _endOfTest;

        public MainWindowBetter()
        {
            _eventLoopTimer = new DispatcherTimer() {Interval = TimeSpan.FromMilliseconds(1)};
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

        private void Tick(object? sender, EventArgs args)
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
                    var tb = this.FindControl<TextBlock>("RoundResult");
                    tb.Text = "End";
                    try
                    {
                        EndTest();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    return;
                }
            
                _appState?.HandleTimerTick();
            }
            
        }

        private void KeyPress(object? sender, KeyEventArgs args)
        {
            _appState?.HandleKeyPress();
        }
        
        private void OnClicked(object? sender, PointerPressedEventArgs e)
        {
            _appState?.HandleKeyPress();
        }
        

        public void HideFocusPoint()
        {
            this.FindControl<Ellipse>("FocusPoint").IsVisible = false;
        }

        public void ShowFocusPoint()
        {
            this.FindControl<Ellipse>("FocusPoint").IsVisible = true;
        }

        private void DisplayAndSaveResult(string result, bool success)
        {
            var tb = this.FindControl<TextBlock>("RoundResult");
            _results?.Add((_appState.RoundId, result));

            var color = Colors.Green;
            if (!success)
            {
                color = Colors.Red;
            }
            else
            {
                result += " ms";
            }
            
            tb.Text = result;
            tb.Foreground = new SolidColorBrush(color);
            tb.IsVisible = true;
        }

        public void ShowFailure(string failureText)
        {
            DisplayAndSaveResult(failureText, false);
        }

        public void ShowSuccess(string success)
        {
            DisplayAndSaveResult(success, true);
        }

        public void HideResult()
        {
            this.FindControl<TextBlock>("RoundResult").IsVisible = false;
        }

        public void EndTest()
        {
            
            _eventLoopTimer.Stop();

            _appState = null;
            this.KeyDown -= KeyPress;
            ShowRoundResults();
            this.FindControl<Grid>("TestGrid").IsVisible = false;
            this.FindControl<Grid>("ResultScreen").IsVisible = true;


            var fName = this.FindControl<TextBox>("NameInput").Text.Trim() + "_"
                                                                           + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

            var sb = new StringBuilder("roundId,result\n");
            foreach (var (r, res) in _results)
            {
                sb.Append($"{r},{res}\n");
            }

            File.WriteAllText(fName, sb.ToString());

            
        }

        private void ShowRoundResults()
        {
            var plot = this.FindControl<AvaPlot>("RoundPlot");
            var dataX = new List<double>();
            var dataY = new List<double>();

            var misses = 0;
            var earlyStarts = 0;
            var averageResponseTime = 0;

            foreach (var (roundNum, roundResult) in _results)
            {
                if (int.TryParse(roundResult, out var result))
                {
                    dataX.Add(roundNum);
                    dataY.Add(result);

                    averageResponseTime += result;
                }
                else
                {
                    if (roundResult == "Too fast")
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