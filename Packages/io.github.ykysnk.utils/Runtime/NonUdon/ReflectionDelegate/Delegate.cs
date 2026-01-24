namespace io.github.ykysnk.utils.NonUdon.ReflectionDelegate;

public delegate TField FieldGetter<out TField>();

public delegate TField FieldGetter<in TInstance, out TField>(TInstance instance);

public delegate void FieldSetter<in TField>(TField value);

public delegate void FieldSetter<in TInstance, in TField>(TInstance instance, TField value);

public delegate TProperty PropertyGetter<out TProperty>();

public delegate TProperty PropertyGetter<in TInstance, out TProperty>(TInstance instance);

public delegate void PropertySetter<in TProperty>(TProperty value);

public delegate void PropertySetter<in TInstance, in TProperty>(TInstance instance, TProperty value);

public delegate void WrapAction();

public delegate void WrapAction<in T1>(T1 arg1);

public delegate void WrapAction<in T1, in T2>(T1 arg1, T2 arg2);

public delegate void WrapAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

public delegate void WrapAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
    T5 arg5, T6 arg6);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 arg1, T2 arg2, T3 arg3,
    T4 arg4, T5 arg5, T6 arg6, T7 arg7);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 arg1, T2 arg2, T3 arg3,
    T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(T1 arg1, T2 arg2,
    T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10>(T1 arg1,
    T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13, in T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9,
    T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13, in T14, in T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8,
    T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);

public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13, in T14, in T15, in T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
    T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

public delegate TR WrapFunc<out TR>();

public delegate TR WrapFunc<in T1, out TR>(T1 arg1);

public delegate TR WrapFunc<in T1, in T2, out TR>(T1 arg1, T2 arg2);

public delegate TR WrapFunc<in T1, in T2, in T3, out TR>(T1 arg1, T2 arg2, T3 arg3);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, out TR>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
    out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
    in T13, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12, T13 arg13);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
    in T13, in T14, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12, T13 arg13, T14 arg14);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
    in T13, in T14, in T15, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12, T13 arg13, T14 arg14, T15 arg15);

public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
    in T13, in T14, in T15, in T16, out TR>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
    T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);