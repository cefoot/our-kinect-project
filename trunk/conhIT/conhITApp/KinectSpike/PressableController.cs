using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace De.DataExperts.conhITApp
{
    public class PressableController : IKinectPressableController, IDisposable
    {
        private WeakReference element;
        private KinectRegion kinectRegion;
        private PressableModel inputModel;
        //private CompositeDisposable eventSubscriptions;

        public PressableController(FrameworkElement element, PressableModel model, KinectRegion kinectRegion)
        {
            this.element = new WeakReference(element);
            this.kinectRegion = kinectRegion;
            this.inputModel = model;

            if (this.inputModel == null)
                return;

            this.inputModel.Holding += (sender, args) => Debug.WriteLine("Holding:"+args.Position);
            this.inputModel.Tapped += (sender, args) => Debug.WriteLine("Tapped:" + args.Position);
            this.inputModel.PressingStarted += (sender, args) => Debug.WriteLine("Pressing Started:" + args.Position);
            //this.inputModel.PressingUpdated += (sender, args) => Debug.WriteLine("Pressing Updated:" + args.Position);
            this.inputModel.PressingCompleted += (sender, args) => Debug.WriteLine("Pressing Completed:" + args.Position);

            //this.eventSubscriptions = new CompositeDisposable
            //{

            //    this.inputModel.PressingStartedObservable()
            //                   .Subscribe(_ => VisualStateManager.GoToState(this.Control, "Focused", true)),

            //    this.inputModel.HoldingObservable()
            //                   .Subscribe(_ => Debug.WriteLine(string.Format("Holding: {0}, ", DateTime.Now))),

            //    this.inputModel.PressingUpdatedObservable()
            //                   .Subscribe(_ => Debug.WriteLine(string.Format("PressingUpdated: {0}, ", DateTime.Now))),

            //    this.inputModel.PressingCompletedObservable()
            //                   .Subscribe(_ => VisualStateManager.GoToState(this.Control, "Unfocused", true)),

            //    this.inputModel.TappedObservable()
            //                   .Subscribe(_ => Debug.WriteLine(string.Format("Tapped: {0}, ", DateTime.Now))),
            //};
        }

        public PressableModel PressableInputModel
        {
            get { return this.inputModel; }
        }

        public FrameworkElement Element
        {
            get { return (FrameworkElement)this.element.Target; }
        }

        internal Control Control { get { return (Control)this.Element; } }

        public void Dispose()
        {
            //Events entfernen
            this.element = (WeakReference)null;
        }
    }
}
