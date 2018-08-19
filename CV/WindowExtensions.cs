namespace CV
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using Microsoft.Windows.Shell;

    /// <summary> Extension methods for <see cref="Window"/>. </summary>
    public static class WindowExtensions
    {
        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.RegisterAttached("ShowIcon", typeof(bool), typeof(WindowExtensions), new FrameworkPropertyMetadata(true, new PropertyChangedCallback((d, e) => RemoveIcon(d as Window))));

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.RegisterAttached("TitleTemplate", typeof(DataTemplate), typeof(WindowExtensions), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleTemplateSelectorProperty =
            DependencyProperty.RegisterAttached("TitleTemplateSelector", typeof(DataTemplateSelector), typeof(WindowExtensions), new PropertyMetadata(null));

        public static readonly DependencyProperty UseSystemCommandsProperty =
                    DependencyProperty.RegisterAttached("UseSystemCommands", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(false, UseSystemCommandsChanged));

        const int GwlExstyle = -20;

        const int SwpFramechanged = 0x0020;

        const int SwpNomove = 0x0002;

        const int SwpNosize = 0x0001;

        const int SwpNozorder = 0x0004;

        const int WsExDlgmodalframe = 0x0001;

        /// <summary> 
        /// Attaches command bindings for the SystemCommands, eg. Minimize, Close, Maximize, etc.
        /// </summary>
        /// <param name="window"> Target window. </param>
        /// <returns> Target window to allow chained calls. </returns>
        public static Window AttachSystemCommands(this Window window)
        {
            window.CommandBindings.AddRange(new[]
            {
                new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow),
                new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow),
                new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow),
                new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow),
                new CommandBinding(SystemCommands.ShowSystemMenuCommand, OnShowSystemMenu),
            });

            return window;
        }

        public static void DetachSystemCommands(this Window window)
        {
            foreach (var commandBinding in window.CommandBindings.Cast<CommandBinding>().ToArray())
            {
                if (commandBinding.Command == SystemCommands.MinimizeWindowCommand ||
                    commandBinding.Command == SystemCommands.MaximizeWindowCommand ||
                    commandBinding.Command == SystemCommands.RestoreWindowCommand ||
                    commandBinding.Command == SystemCommands.CloseWindowCommand ||
                    commandBinding.Command == SystemCommands.ShowSystemMenuCommand)
                    window.CommandBindings.Remove(commandBinding);
            }
        }

        public static bool GetShowIcon(UIElement element) => (bool)element.GetValue(ShowIconProperty);

        public static DataTemplate GetTitleTemplate(DependencyObject obj) => (DataTemplate)obj.GetValue(TitleTemplateProperty);

        public static DataTemplateSelector GetTitleTemplateSelector(DependencyObject obj) => (DataTemplateSelector)obj.GetValue(TitleTemplateSelectorProperty);

        public static bool GetUseSystemCommands(DependencyObject obj) => (bool)obj.GetValue(UseSystemCommandsProperty);

        /// <summary> Removes the icon from a WPF Window. </summary>
        /// <param name="window"> Target window. </param>
        public static void RemoveIcon(this Window window)
        {
            if (window is null) return;

            window.SourceInitialized += (s, e) =>
            {
                // Get this window's handle
                var hwnd = new WindowInteropHelper(window).Handle;

                // Change the extended window style to not show a window icon
                int extendedStyle = GetWindowLong(hwnd, GwlExstyle);
                SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExDlgmodalframe);

                // Update the window's non-client area to reflect the changes
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNozorder | SwpFramechanged);
            };
        }

        public static void SetShowIcon(UIElement element, bool value) => element.SetValue(ShowIconProperty, value);

        public static void SetTitleTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(TitleTemplateProperty, value);

        public static void SetTitleTemplateSelector(DependencyObject obj, DataTemplateSelector value) => obj.SetValue(TitleTemplateSelectorProperty, value);

        public static void SetUseSystemCommands(DependencyObject obj, bool value) => obj.SetValue(UseSystemCommandsProperty, value);

        /// <summary> Show the system menu for the given window. </summary>
        /// <param name="window"> Target window. </param>
        public static void ShowSystemMenu(this Window window)
        {
            if (PresentationSource.FromVisual(window) is PresentationSource source)
            {
                // Divide by DPI ratio because Screen coordinates are absolute and not device-independent
                var coords = window.PointToScreen(new Point(1, 32));
                var dpiX = source.CompositionTarget.TransformToDevice.M11;
                var dpiY = source.CompositionTarget.TransformToDevice.M22;
                coords.X = coords.X / dpiX;
                coords.Y = coords.Y / dpiY;

                SystemCommands.ShowSystemMenu(window, coords);
            }
        }

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        static void OnCloseWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(sender as Window);

        static void OnMaximizeWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MaximizeWindow(sender as Window);

        static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(sender as Window);

        static void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(sender as Window);

        static void OnShowSystemMenu(object sender, ExecutedRoutedEventArgs e) => ShowSystemMenu(sender as Window);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        static void UseSystemCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (e.NewValue is bool value && value)
                {
                    AttachSystemCommands(window);
                }
                else
                {
                    DetachSystemCommands(window);
                }
            }
        }
    }
}