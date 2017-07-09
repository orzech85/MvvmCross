using System;
using System.Collections.Generic;
using Android.Views;

namespace MvvmCross.Droid.Views.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MvxFragmentAttribute : MvxBasePresentationAttribute
    {
        public MvxFragmentAttribute(Type activityHostViewModelType = null, int fragmentContentId = Android.Resource.Id.Content, bool addToBackStack = false)
        {
            ActivityHostViewModelType = activityHostViewModelType;
            FragmentContentId = fragmentContentId;
            AddToBackStack = addToBackStack;
        }

        /// <summary>
        /// Indicates if the fragment can be cached. True by default.
        /// </summary>
        public bool IsCacheableFragment { get; set; } = true;

        /// <summary>
        /// Content id - place where to show fragment.
        /// </summary>
        public int FragmentContentId { get; set; }

        /// <summary>
        /// Indicates if the fragment can be cached. False by default.
        /// </summary>
        public bool AddToBackStack { get; set; } = false;

        /// <summary>
        /// Fragment parent activity ViewModel Type. This activity is shown if ShowToViewModel call for Fragment is called from other activity.
        /// </summary>
        public Type ActivityHostViewModelType { get; set; }

        /// <summary>
        /// SharedElements that will be added to the transition. String may be left empty when using AppCompat
        /// </summary>
        public IDictionary<string, View> SharedElements { get; set; }

        public (int enter, int exit, int popEnter, int popExit) CustomAnimations { get; set; } = (int.MinValue, int.MinValue, int.MinValue, int.MinValue);

        public int TransitionStyle { get; set; } = int.MinValue;
    }
}