// Copyright © 2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp;

namespace ArkBot.Modules.Application.Browser.EventArgs
{
    public class OnPluginCrashedEventArgs : BaseRequestEventArgs
    {
        public OnPluginCrashedEventArgs(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath) : base(chromiumWebBrowser, browser)
        {
            PluginPath = pluginPath;
        }

        public string PluginPath { get; private set; }
    }
}