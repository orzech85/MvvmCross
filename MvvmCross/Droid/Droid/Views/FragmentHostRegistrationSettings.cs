using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.App;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;

namespace MvvmCross.Droid.Views
{
    public class FragmentHostRegistrationSettings
    {
        private readonly IEnumerable<Assembly> _assembliesToLookup;
        private readonly IMvxViewModelTypeFinder _viewModelTypeFinder;

        private readonly Dictionary<Type, IList<MvxBasePresentationAttribute>> _fragmentTypeToMvxFragmentAttributeMap;
        private Dictionary<Type, Type> _viewModelToFragmentTypeMap;

        private bool isInitialized;

        public FragmentHostRegistrationSettings(IEnumerable<Assembly> assembliesToLookup)
        {
            _assembliesToLookup = assembliesToLookup;
            _viewModelTypeFinder = Mvx.Resolve<IMvxViewModelTypeFinder>();
            _fragmentTypeToMvxFragmentAttributeMap = new Dictionary<Type, IList<MvxBasePresentationAttribute>>();
        }

        private void InitializeIfNeeded()
        {
            lock (this)
            {
                if (isInitialized)
                    return;

                isInitialized = true;

                var typesWithMvxFragmentAttribute =
                    _assembliesToLookup
                        .SelectMany(x => x.DefinedTypes)
                        .Select(x => x.AsType())
                        .Where(x => x.HasBasePresentationAttribute())
                        .ToList();

                foreach (var typeWithMvxFragmentAttribute in typesWithMvxFragmentAttribute)
                {
                    if (!_fragmentTypeToMvxFragmentAttributeMap.ContainsKey(typeWithMvxFragmentAttribute))
                        _fragmentTypeToMvxFragmentAttributeMap.Add(typeWithMvxFragmentAttribute, new List<MvxBasePresentationAttribute>());

                    foreach (var mvxAttribute in typeWithMvxFragmentAttribute.GetBasePresentationAttributes())
                        _fragmentTypeToMvxFragmentAttributeMap[typeWithMvxFragmentAttribute].Add(mvxAttribute);
                }

                _viewModelToFragmentTypeMap =
                    typesWithMvxFragmentAttribute.ToDictionary(GetAssociatedViewModelType, fragmentType => fragmentType);
            }
        }

        private Type GetAssociatedViewModelType(Type fromFragmentType)
        {
            Type viewModelType = _viewModelTypeFinder.FindTypeOrNull(fromFragmentType);

            return viewModelType ?? fromFragmentType.GetBasePresentationAttributes().First().ViewModelType;
        }

        public virtual bool IsTypeRegisteredAsFragment(Type viewModelType)
        {
            InitializeIfNeeded();

            return _viewModelToFragmentTypeMap.ContainsKey(viewModelType);
        }

        public virtual bool IsActualHostValid(Type forViewModelType)
        {
            InitializeIfNeeded();

            var activityViewModelType = GetCurrentActivityViewModelType();

            // for example: MvxSplashScreen usually does not have associated ViewModel
            // it is for sure not valid host - and it can not be used with Fragment Presenter.
            if (activityViewModelType == typeof(MvxNullViewModel))
                return false;

            return
                GetMvxFragmentAssociatedAttributes(forViewModelType).OfType<MvxFragmentAttribute>()
                    .Any(x => x.ActivityHostViewModelType == activityViewModelType);
        }

        private Type GetCurrentActivityViewModelType()
        {
            Activity currentActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            Type currentActivityType = currentActivity.GetType();

            var activityViewModelType = _viewModelTypeFinder.FindTypeOrNull(currentActivityType);
            return activityViewModelType;
        }

        public virtual Type GetFragmentHostViewModelType(Type forViewModelType)
        {
            InitializeIfNeeded();

            var associatedMvxFragmentAttributes = GetMvxFragmentAssociatedAttributes(forViewModelType).ToList();
            return associatedMvxFragmentAttributes.OfType<MvxFragmentAttribute>().First().ActivityHostViewModelType;
        }

        public virtual Type GetFragmentTypeAssociatedWith(Type viewModelType)
        {
            InitializeIfNeeded();

            return _viewModelToFragmentTypeMap[viewModelType];
        }

        private IList<MvxBasePresentationAttribute> GetMvxFragmentAssociatedAttributes(Type withFragmentViewModelType)
        {
            var fragmentTypeAssociatedWithViewModel = GetFragmentTypeAssociatedWith(withFragmentViewModelType);
            return _fragmentTypeToMvxFragmentAttributeMap[fragmentTypeAssociatedWithViewModel];
        }

        public virtual MvxBasePresentationAttribute GetAttributesForFragment(Type fragmentViewModelType)
        {
            InitializeIfNeeded();

            var currentActivityViewModelType = GetCurrentActivityViewModelType();
            Activity currentActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var fragmentAttributes = GetMvxFragmentAssociatedAttributes(fragmentViewModelType);

            return fragmentAttributes.First();
        }

        public virtual MvxBasePresentationAttribute GetMvxFragmentAttributeAssociatedWithCurrentHost(Type fragmentViewModelType)
        {
            InitializeIfNeeded();

            var currentActivityViewModelType = GetCurrentActivityViewModelType();
            Activity currentActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var fragmentAttributes = GetMvxFragmentAssociatedAttributes(fragmentViewModelType).OfType<MvxFragmentAttribute>()
                .Where(x => x.ActivityHostViewModelType == currentActivityViewModelType);
            MvxBasePresentationAttribute attribute = fragmentAttributes.FirstOrDefault();

            if (fragmentAttributes.Count() > 1)
            {
                foreach (var item in fragmentAttributes.OfType<MvxFragmentAttribute>())
                {
                    if (currentActivity.FindViewById(item.FragmentContentId) != null)
                    {
                        attribute = item;
                        break;
                    }
                }
            }

            return attribute;
        }
    }
}