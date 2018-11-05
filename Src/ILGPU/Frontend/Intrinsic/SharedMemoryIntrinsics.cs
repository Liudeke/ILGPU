﻿// -----------------------------------------------------------------------------
//                                    ILGPU
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: SharedMemoryIntrinsics.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details
// -----------------------------------------------------------------------------

using ILGPU.IR;
using ILGPU.IR.Values;
using System;

namespace ILGPU.Frontend.Intrinsic
{
    enum SharedMemoryIntrinsicKind
    {
        AllocateElement,
        Allocate,
    }

    /// <summary>
    /// Marks shared-memory methods that are builtin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    sealed class SharedMemoryIntrinsicAttribute : IntrinsicAttribute
    {
        public SharedMemoryIntrinsicAttribute(SharedMemoryIntrinsicKind intrinsicKind)
        {
            IntrinsicKind = intrinsicKind;
        }

        public override IntrinsicType Type => IntrinsicType.SharedMemory;

        /// <summary>
        /// Returns the assigned intrinsic kind.
        /// </summary>
        public SharedMemoryIntrinsicKind IntrinsicKind { get; }
    }

    partial class Intrinsics
    {
        /// <summary>
        /// Handles view operations.
        /// </summary>
        /// <param name="context">The current invocation context.</param>
        /// <param name="attribute">The intrinsic attribute.</param>
        /// <returns>The resulting value.</returns>
        private static ValueReference HandleSharedMemoryOperation(
            in InvocationContext context,
            SharedMemoryIntrinsicAttribute attribute)
        {
            var builder = context.Builder;

            var genericArgs = context.GetMethodGenericArguments();
            var allocationType = genericArgs[0];

            Value length;
            switch (attribute.IntrinsicKind)
            {
                case SharedMemoryIntrinsicKind.AllocateElement:
                    length = builder.CreatePrimitiveValue(1);
                    break;
                case SharedMemoryIntrinsicKind.Allocate:
                    length = context.Arguments[TopLevelFunction.ParametersOffset];
                    break;
                default:
                    throw new NotSupportedException("Invalid shared memory operation");
            }

            var alloca = context.Builder.CreateAlloca(
                context.PopMemory(),
                length,
                context.Builder.CreateType(allocationType),
                MemoryAddressSpace.Shared);
            context.PushMemory(alloca);

            if (attribute.IntrinsicKind == SharedMemoryIntrinsicKind.AllocateElement)
                return alloca;
            return builder.CreateNewView(alloca, length);
        }
    }
}
