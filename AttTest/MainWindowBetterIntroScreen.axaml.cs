using System;
using System.Collections.Generic;
using System.IO;
using AttTest.AppStates;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AttTest
{
    public partial class MainWindowBetter
    {
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
            
            nameField.IsEnabled = false;
            this.FindControl<Grid>("TestGrid").Focus();
            this.FindControl<Grid>("MainMenu").IsVisible = false;
            this.FindControl<Grid>("TestGrid").IsVisible = true;

            this.KeyDown += StartTest;
        }
        
        

        private void StartTest(object? sender, EventArgs args)
        {
            HideTestDescription();
            HideFocusPoint();
            this.KeyDown -= StartTest;
            _results = new List<(int, string, string)>();
            _focusPointsAndPresses = new List<(string, string, string)>();

            if (_constants is null)
            {
                throw new NullReferenceException();
            }
            
            
            _appState = new FocusNotVisible(this, _constants, 0);
            
            this.FindControl<Grid>("TestGrid").Focus();
            this.KeyDown += KeyPress;
           _fname = this.FindControl<TextBox>("NameInput").Text.Trim() + "_"
                                                                           + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            EventLoop();
        }

        
    }
}