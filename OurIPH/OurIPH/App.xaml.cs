using System.Windows;
using System.Windows.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;
using OurIPH.Services;

namespace OurIPH
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var isSmokeTest = e.Args.Any(arg => string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase));
            RegisterProcessExceptionHandlers();
            try
            {
                AppLogger.Startup("Application startup. Args: " + string.Join(" ", e.Args));
                if (isSmokeTest)
                {
                    RunSmokeTest();
                    return;
                }

                var window = new MainWindow();
                MainWindow = window;
                window.Show();
                AppLogger.Info("MainWindow shown.");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Startup failed.", ex);
                if (!isSmokeTest)
                {
                    MessageBox.Show(ex.ToString(), "OurIPH startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Shutdown(1);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppLogger.Error("Dispatcher unhandled exception.", e.Exception);
            MessageBox.Show(e.Exception.ToString(), "OurIPH error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Shutdown(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.Info("Application exit. Code: " + e.ApplicationExitCode);
            base.OnExit(e);
        }

        private static void RunSmokeTest()
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppLogger.Info("Smoke test: constructing MainWindow.");
            var window = new MainWindow();
            AppLogger.Info("Smoke test: MainWindow constructed successfully.");
            window.Close();
            AppLogger.Info("Smoke test: shutdown requested.");
            Current.Shutdown(0);
        }

        private static void RegisterProcessExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                AppLogger.Error("AppDomain unhandled exception. IsTerminating=" + args.IsTerminating,
                    exception ?? new Exception(args.ExceptionObject == null ? "Unknown exception" : args.ExceptionObject.ToString()));
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                AppLogger.Error("Unobserved task exception.", args.Exception);
                args.SetObserved();
            };
        }
    }
}
