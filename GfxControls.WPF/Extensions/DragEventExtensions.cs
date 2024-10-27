using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GfxControls.Extensions
{
    public static class DragEventExtensions
    {
        public static DragEventArgs Create(
            IDataObject? data,
            DragDropKeyStates keyState,
            DragDropEffects effect,
            DependencyObject target,
            Point dropPoint)
        {
#if NET8_0_OR_GREATER
            // Call the internal constructor using UnsafeAccessor
            DragEventArgs args = PrivateCtor(data, keyState, effect, target, dropPoint);
            return args;

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            extern static DragEventArgs PrivateCtor(
                IDataObject? data,
                DragDropKeyStates keyState,
                DragDropEffects effect,
                DependencyObject target,
                Point dropPoint);

#else
            Type type = typeof(DragEventArgs);

            Type[] parameterTypes = 
                [
                typeof(IDataObject),
                typeof(DragDropKeyStates), 
                typeof(DragDropEffects),
                typeof(DependencyObject), 
                typeof(Point)
                ];

            ConstructorInfo? ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);

            if (ctor == null)
            {
                throw new NullReferenceException("Failed to get DragEventArgs .ctor");
            }

            return (DragEventArgs)ctor.Invoke(new object[] { data, keyState, effect, target, dropPoint });
#endif
        }

        public static DragEventArgs Create(
            IDataObject? data,
            DragDropKeyStates keyState,
            DragDropEffects effect,
            DependencyObject target,
            int x,
            int y)
        {
            return Create(data, keyState, effect, target, new Point(x, y));
        }
    }
}
