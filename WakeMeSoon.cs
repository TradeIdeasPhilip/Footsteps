using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Footsteps
{
    /// <summary>
    /// This is used to consolidate multiple requests in the GUI thread.  Imagine that one piece of
    /// code says to change the background color of your button.  And another says to change the
    /// foreground color.  And a third one says to change the text.  Naively you might redraw the
    /// button 3 times, once for each call.  Another approach would be to make the caller first
    /// call StartTransaction() and then Commit() at the end.  That's often very inconvenient for
    /// the caller.  Instead, use WakeMeSoon.  Any time someone changes the state of your button, store
    /// the new state (e.g. ForegroundColor = Color.Red) but don't call Paint() right away.  Instead
    /// call WakeMeSoon.RequestWakeUp() each time you make a change.  The callback will eventually
    /// be called.  Call Paint() in the callback.
    /// 
    /// The previous example has one issue.  Windows provides Invalidate() for this exact purpose.
    /// If you want to call Paint() soon, possibly merging multiple calls, you should call
    /// Invalidate().  But Invalidate() only works with Paint().  WakeMeSoon() calls anything you
    /// want.
    /// 
    /// You can call RequestWakeUp() from any thread.  The callback will always come back in the 
    /// GUI thread.
    /// 
    /// If you call this from the GUI thread, you will not get a callback until the GUI event loop
    /// finishes processing the current event.  It might take longer.  In general, the busier the
    /// GUI thread is, the more events get consolidated.
    /// 
    /// This was inspired by WakeMeSoon in the Delphi software.  That was a little more complicated to
    /// write because Delphi didn't have anything like BeginInvoke() built it.  It does have something
    /// like Invoke(), but I had to create BeginInvoke() myself.  Otherwise the Delphi version was
    /// almost identical to this.
    /// 
    /// The bulk of this file was actually copied from a C# WinForms version of the same thing.
    /// The only difference is that WPF uses a Dispatcher where WinForms uses Control.BeginInvoke().
    /// </summary>
    public class WakeMeSoon
    {
        private readonly Dispatcher _dispatcher;

        public WakeMeSoon(Dispatcher dispatcher = null)
        {
            if (null == dispatcher)
                _dispatcher = Dispatcher.CurrentDispatcher;
            else
                _dispatcher = dispatcher;
        }

        /// <summary>
        /// This will be called in the GUI thread.  After a call to RequestWakeUp(), you are guaranteed
        /// to eventually get a one of these callbacks.  However, multiple calls to RequestWakeUp()
        /// might be consolidated to a single call of this event.
        /// 
        /// This can change at any time, including changing in multiple threads.  However, this typically
        /// is only set once, right after we construct the object.
        /// </summary>
        public event Action OnWakeUp;

        /// <summary>
        /// None of the methods for System.Threading.Interlocked understand the bool type.  So I created
        /// my own using int.
        /// </summary>
        private const int TRUE = 1;

        /// <summary>
        /// None of the methods for System.Threading.Interlocked understand the bool type.  So I created
        /// my own using int.
        /// </summary>
        private const int FALSE = 0;

        /// <summary>
        /// This is how we consolidate multiple requests.  This variable says TRUE if we've already called
        /// BeginInvoke() recently.  
        /// </summary>
        private int _requestInProgress = FALSE;

        public void RequestWakeUp()
        {
            int alreadInProgress = System.Threading.Interlocked.Exchange(ref _requestInProgress, TRUE);
            // At this instant RequestInProgress will be true.  (I mean "instant" literally, as it could
            // be changed back by another thread at any time.)  If RequestInProgress was false, and
            // N threads all call this at once, exactly one thread should see alreadInProgress == false,
            // and the others will all see alreadInProgress == true.  If RequestInProgress was initially
            // true, this will return true.
            if (alreadInProgress == TRUE)
                //  This wakeup request is not required because one is already pending.
                return;
            // Note that we always call BeginInvoke().  Most of our code uses BeginInvokeIfRequired().
            // That would just immediately call the code if we are already in the GUI thread.  We want
            // just the opposite.  We always want to submit this to the event queue.
            _dispatcher.BeginInvoke((Action)delegate
            {
                _requestInProgress = FALSE;
                System.Threading.Thread.MemoryBarrier();
                // Any requests to RequestWakeUp() made after this time will cause another callback.

                // Make a copy.  This code is thread safe.  It's possibly soeone could change the value
                // of OnWakeUp between our test if it is null and our attempt to call it.
                Action onWakeup = OnWakeUp;
                if (null != onWakeup)
                    OnWakeUp();
            });
        }

    }
}
