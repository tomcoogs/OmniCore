﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Mobile.Droid.Platform
{
    public class CoreApplicationLogger : ICoreApplicationLogger
    {
        public const string TAG = "OmniCore";
        public void Debug(string message)
        {
            Log.Debug(TAG, message);
        }

        public void Error(string message)
        {
            Log.Error(TAG, message);
        }

        public void Error(string message, Exception e)
        {
            Log.Error(TAG, FormatExceptionMessage(message, e));
        }

        public void Information(string message)
        {
            Log.Info(TAG, message);
        }

        public void Verbose(string message)
        {
            Log.Verbose(TAG, message);
        }

        public void Warning(string message)
        {
            Log.Warn(TAG, message);
        }

        public void Warning(string message, Exception e)
        {
            Log.Warn(TAG, FormatExceptionMessage(message,e));
        }

        private string FormatExceptionMessage(string message, Exception e)
        {
            var errMessage = new StringBuilder();
            errMessage.AppendLine(message);
            errMessage.AppendLine($"Exception: {e}");
            while (true)
            {
                errMessage.AppendLine($"Message: {e.Message}");
                errMessage.AppendLine($"Requestor: {e.Source}");
                errMessage.AppendLine($"Trace: {e.StackTrace}");
                e = e.InnerException;
                if (e != null)
                {
                    errMessage.AppendLine($"Inner Exception: {e}");
                }
                else
                    break;
            }
            return errMessage.ToString();
        }
    }
}