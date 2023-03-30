using System;

namespace EurekaTrackerAutoPopper.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotShowInHelpAttribute : Attribute
    {
    }
}