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
    public class ManipulatableController : IKinectManipulatableController, IDisposable
    {
        private WeakReference<KinectMenuItem> element;
        private KinectRegion kinectRegion;
        private ManipulatableModel inputModel;
        //private CompositeDisposable eventSubscriptions;

        public ManipulatableController(KinectMenuItem element, ManipulatableModel model, KinectRegion kinectRegion)
        {
            this.element = new WeakReference<KinectMenuItem>(element);
            this.kinectRegion = kinectRegion;
            this.inputModel = model;

            if (this.inputModel == null)
                return;

            this.inputModel.ManipulationStarted += inputModel_ManipulationStarted;
        }

        void inputModel_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            Item.Clicked();
        }

        public ManipulatableModel ManipulatableInputModel
        {
            get { return this.inputModel; }
        }

        public void Dispose()
        {
            //Events entfernen
            this.inputModel.ManipulationStarted -= inputModel_ManipulationStarted;
            this.element = (WeakReference<KinectMenuItem>)null;
        }

        public FrameworkElement Element
        {
            get { return Item; }
        }

        public KinectMenuItem Item
        {
            get
            {
                KinectMenuItem itm;
                return element.TryGetTarget(out itm) ? itm : null;
            }
        }
    }
}
