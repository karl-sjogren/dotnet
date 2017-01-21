﻿using Microsoft.AspNetCore.Html;
using StackExchange.Profiling.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackExchange.Profiling
{
    /// <summary>
    /// Extension methods for MiniProfiler
    /// </summary>
    public static class MiniProfilerExtensions
    {
        /// <summary>
        /// Renders script tag found in "include.partial.html".
        /// </summary>
        public static HtmlString RenderIncludes(
            this MiniProfiler profiler,
            RenderPosition? position = null,
            bool? showTrivial = null,
            bool? showTimeWithChildren = null,
            int? maxTracesToShow = null,
            bool? showControls = null,
            bool? startHidden = null)
        {
            if (profiler == null) return HtmlString.Empty;

            // TODO: Figure out auth
            var authorized = true; // _options.ResultsAuthorize?.Invoke(HttpContext.Current.Request) ?? true;

            // unviewed ids are added to this list during Storage.Save, but we know we haven't 
            // seen the current one yet, so go ahead and add it to the end 
            var ids = authorized ? MiniProfiler.Settings.Storage.GetUnviewedIds(profiler.User) : new List<Guid>();
            ids.Add(profiler.Id);

            if (!MiniProfilerMiddleware.Current.Embedded.TryGetResource("include.partial.html", out string format))
            {
                return new HtmlString("<!-- Could not find 'include.partial.html' -->");
            }

            Func<bool, string> toJs = b => b ? "true" : "false";

            var sb = new StringBuilder(format);
            sb.Replace("{path}", MiniProfilerMiddleware.Current.BasePath.Value.EnsureTrailingSlash())
              .Replace("{version}", MiniProfiler.Settings.VersionHash)
              .Replace("{currentId}", profiler.Id.ToString())
              .Replace("{ids}", string.Join(",", ids.Select(guid => guid.ToString())))
              .Replace("{position}", (position ?? MiniProfiler.Settings.PopupRenderPosition).ToString().ToLower())
              .Replace("{showTrivial}", toJs(showTrivial ?? MiniProfiler.Settings.PopupShowTrivial))
              .Replace("{showChildren}", toJs(showTimeWithChildren ?? MiniProfiler.Settings.PopupShowTimeWithChildren))
              .Replace("{maxTracesToShow}", (maxTracesToShow ?? MiniProfiler.Settings.PopupMaxTracesToShow).ToString())
              .Replace("{showControls}", toJs(showControls ?? MiniProfiler.Settings.ShowControls))
              .Replace("{authorized}", toJs(authorized))
              .Replace("{toggleShortcut}", MiniProfiler.Settings.PopupToggleKeyboardShortcut)
              .Replace("{startHidden}", toJs(startHidden ?? MiniProfiler.Settings.PopupStartHidden))
              .Replace("{trivialMilliseconds}", MiniProfiler.Settings.TrivialDurationThresholdMilliseconds.ToString());
            return new HtmlString(sb.ToString());
        }
    }
}