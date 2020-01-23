﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using tweetz.core.ViewModels;

namespace tweetz.core
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        protected override void OnSourceInitialized(EventArgs e)
        {
            ViewModel.Initialize(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.OnClosing(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}