using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace myFeed.Extensions
{
    /// <summary>
    /// Contains usefull methods.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Logs data if debug mode is enabled.
        /// </summary>
        /// <param name="data">Data object</param>
        public static void Log(object data)
        {
#if DEBUG
            Debug.WriteLine(data);
#endif
        }

        /// <summary>
        /// Fades target object in using lightweight animations.
        /// </summary>
        /// <param name="o">Target</param>
        /// <param name="duration">Duration</param>
        public static void FadeIn(this UIElement o, int duration = 300)
        {
            var story = new Storyboard();
            var daukf = new DoubleAnimationUsingKeyFrames();
            daukf.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 0, 0, 0), Value = 0 });
            daukf.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 0, 0, 300), Value = 1 });
            Storyboard.SetTarget(daukf, o);
            Storyboard.SetTargetProperty(daukf, "(UIElement.Opacity)");
            story.Children.Add(daukf);
            story.Begin();
        }

        /// <summary>
        /// Capitalizes the string.
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>1-caps string</returns>
        public static string Capitalize(this string str)
        {
            var lower = str.ToLowerInvariant();
            var array = lower.ToCharArray();
            if (lower.Length <= 0) return string.Empty;
            array[0] = char.ToUpperInvariant(array[0]);
            return new string(array);
        }

        /// <summary>
        /// Tries to execute a task and returns default value onFail.
        /// </summary>
        /// <typeparam name="TResult">Generic typedef</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns></returns>
        public static async Task<TResult> TryAsync<TResult>(Task<TResult> task, TResult defaultResult)
        {
            try { return await task; } catch (Exception) { return defaultResult; }
        }

        /// <summary>
        /// Converts hex value to color object.
        /// </summary>
        /// <param name="hex">Hex string #ffffffff</param>
        /// <returns>Color object</returns>
        public static Color GetColorFromHex(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            var a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            var r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            var g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            var b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }
    }
}
