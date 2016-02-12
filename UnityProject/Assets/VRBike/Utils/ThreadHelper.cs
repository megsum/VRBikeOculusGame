using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace MedRoad.Utils
{
    /// <summary>
    /// This helper class allows actions and coroutines from other threads to be queued and then
    /// performed on Unity's main thread.
    /// </summary>
    public class ThreadHelper : MonoBehaviourSingleton<ThreadHelper>
    {
        /// <summary>
        /// The main thread that Unity is running on.
        /// </summary>
        private static Thread mainThread = null;

        /// <summary>
        /// Get the main thread on Awake.
        /// </summary>
        protected override void Awake2()
        {
            mainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Perform queued actions and start queued coroutines on the main thread.
        /// </summary>
        private void Update()
        {
            this.PerformQueuedActions();
            this.StartQueuedCoroutines();
        }

        /// <summary>
        /// Checks if the singleton has been set (i.e., that a ThreadHelper instance exists) and
        /// prints a Unity error message if it doesn't.
        /// </summary>
        /// <param name="utilityName">The name of the class, method, etc. that wants to use
        /// ThreadHelper. This will be included in the error message if no ThreadHelper instance
        /// exists.</param>
        /// <returns><c>True</c> if a ThreadHelper instance exists, <c>false</c> otherwise.
        /// </returns>
        public static bool SingletonCheck(string utilityName)
        {
            if (ThreadHelper.Singleton == null)
            {
                Debug.LogErrorFormat("[ThreadHelper] {0} wants to use ThreadHelper but no instance exists! Add a ThreadHelper instance to your scene.", utilityName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get if the current thread you are running on
        /// </summary>
        /// <returns></returns>
        public static bool IsOnMainThread()
        {
            if (ThreadHelper.Singleton == null)
            {
                Debug.LogError("[ThreadHelper] Cannot run IsOnMainThread() because no instance exists! Add a ThreadHelper instance to your scene.");
                return false;
            }

            return mainThread.Equals(Thread.CurrentThread);
        }

        /// <summary>
        /// A queue of coroutines to be started on the main thread.
        /// </summary>
        private Queue<IEnumerator> queuedCoroutines = new Queue<IEnumerator>(10);

        /// <summary>
        /// A queue of actions to be performed on the main thread.
        /// </summary>
        private Queue<Action> queuedActions = new Queue<Action>(10);

        /// <summary>
        /// Called on the main thread to perform queued actions.
        /// </summary>
        private void PerformQueuedActions()
        {
            lock (this.queuedActions)
            {
                while (this.queuedActions.Count > 0)
                    this.queuedActions.Dequeue().Invoke();
            }
        }

        /// <summary>
        /// Called on the main thread to start queued coroutines.
        /// </summary>
        private void StartQueuedCoroutines()
        {
            lock (this.queuedCoroutines)
            {
                while (this.queuedCoroutines.Count > 0)
                    StartCoroutine(this.queuedCoroutines.Dequeue());
            }
        }

        /// <summary>
        /// Perform the given action on the main thread. If this method is called from the main
        /// thread, it will be performed immediately. If called from a different thread, it will
        /// be queued and ran during the next Unity update cycle.
        /// </summary>
        /// <param name="action">The Action to perform.</param>
        public void PerformActionOnMainThread(Action action)
        {
            if (IsOnMainThread())
                action();
            else
                lock (this.queuedActions)
                    this.queuedActions.Enqueue(action);
        }

        /// <summary>
        /// Start the given coroutine on the main thread. If this method is called from the main
        /// thread, it will be started immediately. If called from a different thread, it will
        /// be queued and started during the next Unity update cycle.
        /// </summary>
        /// <param name="coroutine">The Couroutine to start.</param>
        public void StartCoroutineOnMainThread(IEnumerator coroutine)
        {
            if (IsOnMainThread())
                StartCoroutine(coroutine);
            else
                lock (this.queuedCoroutines)
                    this.queuedCoroutines.Enqueue(coroutine);
        }

    }
}
