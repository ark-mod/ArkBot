﻿// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.Wpf;
using System;
using System.Windows;
using System.Windows.Interactivity;

namespace ArkBot.Modules.Application.Browser.Behaviours
{
    public class HoverLinkBehaviour : Behavior<ChromiumWebBrowser>
    {
        // Using a DependencyProperty as the backing store for HoverLink. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverLinkProperty = DependencyProperty.Register("HoverLink", typeof(string), typeof(HoverLinkBehaviour), new PropertyMetadata(string.Empty));

        public string HoverLink
        {
            get { return (string)GetValue(HoverLinkProperty); }
            set { SetValue(HoverLinkProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.StatusMessage += OnStatusMessageChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.StatusMessage -= OnStatusMessageChanged;
        }

        private void OnStatusMessageChanged(object sender, CefSharp.StatusMessageEventArgs e)
        {
            var chromiumWebBrowser = sender as ChromiumWebBrowser;
            chromiumWebBrowser.Dispatcher.BeginInvoke((Action)(() => HoverLink = e.Value));
        }
    }
}
