using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Client.Store.Common
{
    public class CustomDialog
    {
        private Popup _popup;
        private TaskCompletionSource<ICommand> _taskCompletionSource;
        private StackPanel _buttonPanel;

        /// <summary>
        /// Initializes a new instance of the CustomDialog class to display an untitled
        /// dialog that can be used to show your user custom content.
        /// </summary>
        /// <param name="content">The content displayed to the user.</param>
        protected CustomDialog(object content)
        {
            Title = string.Empty;
            Commands = new List<Tuple<string, ICommand>>();
            commands = new List<CommandWrapper>();
            CancelCommandIndex = Int32.MaxValue;
            DefaultCommandIndex = Int32.MaxValue;
            HeaderBrush = new SolidColorBrush(Colors.White);
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the CustomDialog class to display a titled
        /// dialog that can be used to show your user custom content.
        /// </summary>
        /// <param name="content">The content displayed to the user.</param>
        /// <param name="title">The title you want displayed on the dialog.</param>
        protected CustomDialog(object content, string title)
            : this(content)
        {
            if (string.IsNullOrEmpty(title) == false)
            {
                Title = title;
            }
        }

        /// <Summary>
        /// Gets or sets the index of the command you want to use as the default. This
        /// is the command that fires by default when users press the ENTER key.
        /// </Summary>
        /// <Returns>The index of the default command.</Returns>
        public uint DefaultCommandIndex { get; set; }

        /// <Summary>
        /// Gets or sets the index of the command you want to use as the cancel command.
        /// This is the command that fires when users press the ESC key.
        /// </Summary>
        /// <Returns>The index of the cancel command.</Returns>
        public uint CancelCommandIndex { get; set; }

        /// <Summary>
        /// Gets the set of commands that appear in the command bar of the message dialog.
        /// </Summary>
        /// <Returns>The commands.</Returns>
        public IList<Tuple<string, ICommand>> Commands { get; private set; }

        private IList<CommandWrapper> commands;

        /// <Summary>
        /// Gets or sets the content to be displayed to the user.
        /// </Summary>
        /// <Returns>The message to be displayed to the user.</Returns>
        public object Content { get; set; }

        /// <Summary>
        /// Gets or sets the title to display on the dialog, if any.
        /// </Summary>
        /// <Returns>
        /// The title you want to display on the dialog. If the title is not set, this
        /// will return an empty string.
        /// </Returns>
        public string Title { get; set; }

        public Brush HeaderBrush { get; set; }

        /// <Summary>
        /// Begins an asynchronous operation showing a dialog.
        /// </Summary>
        /// <Returns>
        /// An object that represents the asynchronous operation. For more on the async
        /// pattern, see Asynchronous programming in the Windows Runtime.
        /// </Returns>
        public IAsyncOperation<ICommand> ShowAsync()
        {
            _popup = new Popup { Child = CreateDialog() };
            if (_popup.Child != null)
            {
                SubscribeEvents();
                _popup.IsOpen = true;
            }
            return AsyncInfo.Run(WaitForInput);
        }

        public static async Task ShowDialog(object content, string title, Brush headerBrush, UInt32 cancelIndex, params Tuple<string, ICommand>[] commands)
        {
            await ShowDialog(content, title, headerBrush, cancelIndex, commands as IEnumerable<Tuple<string, ICommand>>);
        }

        public static async Task ShowDialog(object content, string title, Brush headerBrush, UInt32 cancelIndex, IEnumerable<Tuple<string, ICommand>> commands)
        {
            var d = new CustomDialog(content, title);
            d.HeaderBrush = headerBrush;
            d.CancelCommandIndex = cancelIndex;
            if (commands != null)
            {
                foreach (var item in commands)
                {
                    d.Commands.Add(item);
                    d.commands.Add(new CommandWrapper(item.Item2));
                }
            }
            await d.ShowAsync();
        }

        private async Task<ICommand> WaitForInput(CancellationToken token)
        {
            _taskCompletionSource = new TaskCompletionSource<ICommand>();

            token.Register(OnCanceled);

            return await _taskCompletionSource.Task;
        }

        private UIElement CreateDialog()
        {
            var content = Window.Current.Content as FrameworkElement;
            if (content == null)
            {
                // The dialog is being shown before content has been created for the window
                Window.Current.Activated += OnWindowActivated;
                return null;
            }
            //Style subHeaderTextStyle = Application.Current.Resources["LightSubheaderTextStyle"] as Style;
            //Style buttonStyle = Application.Current.Resources["LightButtonStyle"] as Style;

            double width = Window.Current.Bounds.Width;
            double height = Window.Current.Bounds.Height;
            var root = new Grid { Width = width, Height = height };
            var overlay = new Grid { Background = new SolidColorBrush(Colors.Black), Opacity = 0.2D };
            root.Children.Add(overlay);

            root.RequestedTheme = App.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;

            var dialogPanel = new Dialogs.DialogTemplate();

            dialogPanel.HeaderBackground = HeaderBrush;
            dialogPanel.Title = Title;

            dialogPanel.DialogContent = Content;

            _buttonPanel = dialogPanel.ButtonPanel;

            if (commands.Count == 0)
            {
                Button button = new Button();
                //button.Style = buttonStyle;
                button.Content = "Close";
                button.MinWidth = 92;
                button.Margin = new Thickness(20, 20, 0, 20);
                button.Click += (okSender, okArgs) => CloseDialog(null);
                _buttonPanel.Children.Add(button);
            }
            else
            {
                System.Diagnostics.Debug.Assert(Commands.Count == commands.Count, "Commands und commands in der Klasse Custom Dialogs sollten gleichgroß sein.");
                for (int i = 0; i < Commands.Count; i++)
                {
                    CommandWrapper currentCommand = commands[i];
                    Button button = new Button();
                    //button.Style = buttonStyle;
                    button.Content = Commands[i].Item1;
                    button.Command = currentCommand;
                    button.Margin = new Thickness(20, 20, 0, 20);
                    button.MinWidth = 92;

                    button.Click += (okSender, okArgs) => CloseDialog(currentCommand);
                    _buttonPanel.Children.Add(button);
                }
            }

            root.Children.Add(dialogPanel);

            return root;
        }

        // adjust for different view states
        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (_popup.IsOpen == false) return;

            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            child.Width = e.Size.Width;
            child.Height = e.Size.Height;
        }

        // Adjust the name/password textboxes for the virtual keyuboard
        private void OnInputShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            var transform = _buttonPanel.TransformToVisual(child);
            var topLeft = transform.TransformPoint(new Point(0, 0));

            // Need to be able to view the entire textblock (plus a little more)
            var buffer = 20;
            if ((topLeft.Y - buffer) > sender.OccludedRect.Top)
            {
                var margin = topLeft.Y - sender.OccludedRect.Top;
                margin -= buffer;
                child.Margin = new Thickness(0, -margin, 0, 0);
            }
        }

        private void OnInputHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            child.Margin = new Thickness(0);
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Only respond to Esc if there is a cancel index
            if ((e.Key == VirtualKey.Escape) && (CancelCommandIndex < commands.Count))
            {
                OnCanceled();
            }

            // Only respond to Enter if there is a cancel index
            if ((e.Key == VirtualKey.Enter) && (DefaultCommandIndex < commands.Count))
            {
                var com = commands[(int)DefaultCommandIndex];
                if (com.CanExecute(null))
                    com.Execute(null);
                CloseDialog(com);
            }
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            Window.Current.Activated -= OnWindowActivated;
            SubscribeEvents();
            _popup.Child = CreateDialog();
            _popup.IsOpen = true;
        }

        private void OnCanceled()
        {
            UnsubscribeEvents();

            CommandWrapper command = null;
            if (CancelCommandIndex < commands.Count)
            {
                command = commands[(int)CancelCommandIndex];
            }
            if (command.CanExecute(null))
                command.Execute(null);
            CloseDialog(command);
        }

        private async void CloseDialog(CommandWrapper command)
        {
            UnsubscribeEvents();

            if (command != null)
                await command.ProlongWaiter;

            _popup.IsOpen = false;
            _taskCompletionSource.SetResult(command);
        }

        private void SubscribeEvents()
        {
            Window.Current.SizeChanged += OnWindowSizeChanged;
            Window.Current.Content.KeyDown += OnKeyDown;

            var input = InputPane.GetForCurrentView();
            input.Showing += OnInputShowing;
            input.Hiding += OnInputHiding;
        }

        private void UnsubscribeEvents()
        {
            Window.Current.SizeChanged -= OnWindowSizeChanged;
            Window.Current.Content.KeyDown -= OnKeyDown;

            var input = InputPane.GetForCurrentView();
            input.Showing -= OnInputShowing;
            input.Hiding -= OnInputHiding;
        }

        private class CommandWrapper : ICommand
        {
            private ICommand command;

            /// <summary>
            /// Ein Task welcher abläuft sobald das Command fertig bearbeitet wurde.
            /// </summary>
            /// <remarks>
            /// Kann erst aufgerufen werden nachdem Execute aufgerufen wurde. Wenn execute mehrfach aufgerufen wurde, bezieht sich der Task auf den Letzten Aufruf.
            /// </remarks>
            public Task ProlongWaiter
            {
                get
                {
                    return waiter.Task;
                }
            }

            private TaskCompletionSource<bool> waiter = new TaskCompletionSource<bool>();

            public bool CanExecute(object parameter)
            {
                return command.CanExecute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { command.CanExecuteChanged += value; }
                remove { command.CanExecuteChanged -= value; }
            }

            public async void Execute(object parameter)
            {
                var prolonged = command as ProlongedCommand;
                if (prolonged != null)
                {
                    await prolonged.Execute(parameter);
                    waiter.SetResult(true);
                }
                else
                {
                    command.Execute(parameter);
                    waiter.SetResult(true);
                }
            }

            public CommandWrapper(ICommand command)
            {
                this.command = command;
            }
        }
    }
}