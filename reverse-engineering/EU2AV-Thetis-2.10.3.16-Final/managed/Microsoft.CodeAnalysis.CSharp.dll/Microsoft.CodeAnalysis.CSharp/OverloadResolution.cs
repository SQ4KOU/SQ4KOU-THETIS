using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class OverloadResolution
{
	internal static class BinopEasyOut
	{
		private const BinaryOperatorKind ERR = BinaryOperatorKind.Error;

		private const BinaryOperatorKind OBJ = BinaryOperatorKind.Object;

		private const BinaryOperatorKind STR = BinaryOperatorKind.String;

		private const BinaryOperatorKind OSC = BinaryOperatorKind.ObjectAndString;

		private const BinaryOperatorKind SOC = BinaryOperatorKind.StringAndObject;

		private const BinaryOperatorKind INT = BinaryOperatorKind.Int;

		private const BinaryOperatorKind UIN = BinaryOperatorKind.UInt;

		private const BinaryOperatorKind LNG = BinaryOperatorKind.Long;

		private const BinaryOperatorKind ULG = BinaryOperatorKind.ULong;

		private const BinaryOperatorKind NIN = BinaryOperatorKind.NInt;

		private const BinaryOperatorKind NUI = BinaryOperatorKind.NUInt;

		private const BinaryOperatorKind FLT = BinaryOperatorKind.Float;

		private const BinaryOperatorKind DBL = BinaryOperatorKind.Double;

		private const BinaryOperatorKind DEC = BinaryOperatorKind.Decimal;

		private const BinaryOperatorKind BOL = BinaryOperatorKind.Bool;

		private const BinaryOperatorKind LIN = BinaryOperatorKind.Int | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LUN = BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LLG = BinaryOperatorKind.Long | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LUL = BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LNI = BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LNU = BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LFL = BinaryOperatorKind.Float | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LDB = BinaryOperatorKind.Double | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LDC = BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LBL = BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted;

		private static readonly BinaryOperatorKind[,] s_arithmetic = new BinaryOperatorKind[32, 32]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_addition = new BinaryOperatorKind[32, 32]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.String,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_shift = new BinaryOperatorKind[32, 32]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			}
		};

		private static readonly BinaryOperatorKind[,] s_equality = new BinaryOperatorKind[32, 32]
		{
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.String,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_logical = new BinaryOperatorKind[32, 32]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.NUInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			}
		};

		private static readonly BinaryOperatorKind[][,] s_opkind = new BinaryOperatorKind[17][,]
		{
			s_arithmetic, s_addition, s_arithmetic, s_arithmetic, s_arithmetic, s_shift, s_shift, s_equality, s_equality, s_arithmetic,
			s_arithmetic, s_arithmetic, s_arithmetic, s_logical, s_logical, s_logical, s_shift
		};

		public static BinaryOperatorKind OpKind(BinaryOperatorKind kind, TypeSymbol left, TypeSymbol right)
		{
			int num = left.TypeToIndex();
			if (num < 0)
			{
				return BinaryOperatorKind.Error;
			}
			int num2 = right.TypeToIndex();
			if (num2 < 0)
			{
				return BinaryOperatorKind.Error;
			}
			BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Error;
			if (!kind.IsLogical() || (num == 15 && num2 == 15))
			{
				binaryOperatorKind = s_opkind[kind.OperatorIndex()][num, num2];
			}
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				return binaryOperatorKind | kind;
			}
			return binaryOperatorKind;
		}
	}

	private enum LiftingResult
	{
		NotLifted,
		LiftOperandsAndResult,
		LiftOperandsButNotResult
	}

	internal static class UnopEasyOut
	{
		private const UnaryOperatorKind ERR = UnaryOperatorKind.Error;

		private const UnaryOperatorKind BOL = UnaryOperatorKind.Bool;

		private const UnaryOperatorKind CHR = UnaryOperatorKind.Char;

		private const UnaryOperatorKind I08 = UnaryOperatorKind.SByte;

		private const UnaryOperatorKind U08 = UnaryOperatorKind.Byte;

		private const UnaryOperatorKind I16 = UnaryOperatorKind.Short;

		private const UnaryOperatorKind U16 = UnaryOperatorKind.UShort;

		private const UnaryOperatorKind I32 = UnaryOperatorKind.Int;

		private const UnaryOperatorKind U32 = UnaryOperatorKind.UInt;

		private const UnaryOperatorKind I64 = UnaryOperatorKind.Long;

		private const UnaryOperatorKind U64 = UnaryOperatorKind.ULong;

		private const UnaryOperatorKind NIN = UnaryOperatorKind.NInt;

		private const UnaryOperatorKind NUI = UnaryOperatorKind.NUInt;

		private const UnaryOperatorKind R32 = UnaryOperatorKind.Float;

		private const UnaryOperatorKind R64 = UnaryOperatorKind.Double;

		private const UnaryOperatorKind DEC = UnaryOperatorKind.Decimal;

		private const UnaryOperatorKind LBOL = UnaryOperatorKind.Bool | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LCHR = UnaryOperatorKind.Char | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI08 = UnaryOperatorKind.SByte | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU08 = UnaryOperatorKind.Byte | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI16 = UnaryOperatorKind.Short | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU16 = UnaryOperatorKind.UShort | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI32 = UnaryOperatorKind.Int | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU32 = UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI64 = UnaryOperatorKind.Long | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU64 = UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LNI = UnaryOperatorKind.NInt | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LNU = UnaryOperatorKind.NUInt | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LR32 = UnaryOperatorKind.Float | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LR64 = UnaryOperatorKind.Double | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LDEC = UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted;

		private static readonly UnaryOperatorKind[] s_increment = new UnaryOperatorKind[32]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Char,
			UnaryOperatorKind.SByte,
			UnaryOperatorKind.Short,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Byte,
			UnaryOperatorKind.UShort,
			UnaryOperatorKind.UInt,
			UnaryOperatorKind.ULong,
			UnaryOperatorKind.NInt,
			UnaryOperatorKind.NUInt,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Char | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.SByte | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Short | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Byte | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UShort | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.NInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.NUInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_plus = new UnaryOperatorKind[32]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.UInt,
			UnaryOperatorKind.ULong,
			UnaryOperatorKind.NInt,
			UnaryOperatorKind.NUInt,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.NInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.NUInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_minus = new UnaryOperatorKind[32]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.NInt,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.NInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_logicalNegation;

		private static readonly UnaryOperatorKind[] s_bitwiseComplement;

		private static readonly UnaryOperatorKind[][] s_opkind;

		public static UnaryOperatorKind OpKind(UnaryOperatorKind kind, TypeSymbol operand)
		{
			int num = operand.TypeToIndex();
			if (num < 0)
			{
				return UnaryOperatorKind.Error;
			}
			int num2 = kind.OperatorIndex();
			UnaryOperatorKind unaryOperatorKind = ((num2 < s_opkind.Length) ? s_opkind[num2][num] : UnaryOperatorKind.Error);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				return unaryOperatorKind | kind;
			}
			return unaryOperatorKind;
		}

		static UnopEasyOut()
		{
			UnaryOperatorKind[] array = new UnaryOperatorKind[32];
			array[2] = UnaryOperatorKind.Bool;
			array[17] = UnaryOperatorKind.Bool | UnaryOperatorKind.Lifted;
			s_logicalNegation = array;
			s_bitwiseComplement = new UnaryOperatorKind[32]
			{
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Long,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.UInt,
				UnaryOperatorKind.ULong,
				UnaryOperatorKind.NInt,
				UnaryOperatorKind.NUInt,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.NInt | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.NUInt | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error
			};
			s_opkind = new UnaryOperatorKind[8][] { s_increment, s_increment, s_increment, s_increment, s_plus, s_minus, s_logicalNegation, s_bitwiseComplement };
		}
	}

	internal class PairedExtensionOperatorSignatureComparer : IEqualityComparer<MethodSymbol>
	{
		public static readonly PairedExtensionOperatorSignatureComparer Instance = new PairedExtensionOperatorSignatureComparer();

		private PairedExtensionOperatorSignatureComparer()
		{
		}

		public bool Equals(MethodSymbol? x, MethodSymbol? y)
		{
			if ((object)x.OriginalDefinition.ContainingType.ContainingType != x.OriginalDefinition.ContainingType.ContainingType)
			{
				return false;
			}
			NamedTypeSymbol containingType = x.OriginalDefinition.ContainingType;
			string? extensionGroupingName = ((SourceNamedTypeSymbol)containingType).ExtensionGroupingName;
			NamedTypeSymbol containingType2 = y.OriginalDefinition.ContainingType;
			string extensionGroupingName2 = ((SourceNamedTypeSymbol)containingType2).ExtensionGroupingName;
			if (!extensionGroupingName.Equals(extensionGroupingName2))
			{
				return false;
			}
			return SourceMemberContainerTypeSymbol.DoOperatorsPair(x.OriginalDefinition.AsMember(Normalize(containingType)), y.OriginalDefinition.AsMember(Normalize(containingType2)));
		}

		private static NamedTypeSymbol Normalize(NamedTypeSymbol extension)
		{
			if (extension.Arity != 0)
			{
				extension = extension.Construct(IndexedTypeParameterSymbol.Take(extension.Arity));
			}
			return extension;
		}

		public int GetHashCode(MethodSymbol op)
		{
			EqualityComparer<Symbol> allIgnoreOptions = Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.AllIgnoreOptions;
			int hashCode = allIgnoreOptions.GetHashCode(op.OriginalDefinition.ContainingType.ContainingType);
			NamedTypeSymbol containingType = op.OriginalDefinition.ContainingType;
			string extensionGroupingName = ((SourceNamedTypeSymbol)containingType).ExtensionGroupingName;
			hashCode = Hash.Combine(hashCode, extensionGroupingName.GetHashCode());
			foreach (ParameterSymbol parameter in op.OriginalDefinition.AsMember(Normalize(containingType)).Parameters)
			{
				hashCode = Hash.Combine(hashCode, allIgnoreOptions.GetHashCode(parameter.Type));
			}
			return hashCode;
		}
	}

	[Flags]
	public enum Options : ushort
	{
		None = 0,
		IsMethodGroupConversion = 1,
		AllowRefOmittedArguments = 2,
		InferWithDynamic = 4,
		IgnoreNormalFormIfHasValidParamsParameter = 8,
		IsFunctionPointerResolution = 0x10,
		IsExtensionMethodResolution = 0x20,
		DynamicResolution = 0x40,
		DynamicConvertsToAnything = 0x80,
		DisallowExpandedNonArrayParams = 0x100,
		InferringUniqueMethodGroupSignature = 0x200,
		DisallowExpandedForm = 0x400
	}

	private class ReturnStatements : BoundTreeWalker
	{
		private readonly ArrayBuilder<BoundReturnStatement> _returns;

		public ReturnStatements(ArrayBuilder<BoundReturnStatement> returns)
		{
			_returns = returns;
		}

		public override BoundNode Visit(BoundNode node)
		{
			if (!(node is BoundExpression))
			{
				return base.Visit(node);
			}
			return null;
		}

		protected override BoundNode VisitExpressionOrPatternWithoutStackGuard(BoundNode node)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/OverloadResolution/OverloadResolution.cs", 3432);
		}

		public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			return null;
		}

		public override BoundNode VisitReturnStatement(BoundReturnStatement node)
		{
			_returns.Add(node);
			return null;
		}
	}

	private readonly struct EffectiveParameters
	{
		internal readonly ImmutableArray<TypeWithAnnotations> ParameterTypes;

		internal readonly ImmutableArray<RefKind> ParameterRefKinds;

		internal readonly int FirstParamsElementIndex;

		internal EffectiveParameters(ImmutableArray<TypeWithAnnotations> types, ImmutableArray<RefKind> refKinds, int firstParamsElementIndex)
		{
			ParameterTypes = types;
			ParameterRefKinds = refKinds;
			FirstParamsElementIndex = firstParamsElementIndex;
		}
	}

	private readonly struct ParameterMap(int[] parameters, int length)
	{
		private readonly int[] _parameters = parameters;

		private readonly int _length = length;

		public bool IsTrivial => _parameters == null;

		public int Length => _length;

		public int this[int argument]
		{
			get
			{
				if (_parameters != null)
				{
					return _parameters[argument];
				}
				return argument;
			}
		}

		public ImmutableArray<int> ToImmutableArray()
		{
			return _parameters.AsImmutableOrNull();
		}
	}

	private readonly Binder _binder;

	private bool? _strict;

	private static readonly ObjectPool<PooledHashSet<Symbol>> s_HiddenSymbolsSetPool = PooledHashSet<Symbol>.CreatePool(Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.AllIgnoreOptions);

	private const int BetterConversionTargetRecursionLimit = 100;

	private CSharpCompilation Compilation => _binder.Compilation;

	private Conversions Conversions => _binder.Conversions;

	private bool Strict
	{
		get
		{
			if (_strict.HasValue)
			{
				return _strict.Value;
			}
			bool featureStrictEnabled = _binder.Compilation.FeatureStrictEnabled;
			_strict = featureStrictEnabled;
			return featureStrictEnabled;
		}
	}

	private void BinaryOperatorEasyOut(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol type = left.Type;
		if ((object)type == null)
		{
			return;
		}
		TypeSymbol type2 = right.Type;
		if ((object)type2 != null && !PossiblyUnusualConstantOperation(left, right))
		{
			BinaryOperatorKind binaryOperatorKind = BinopEasyOut.OpKind(kind, type, type2);
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				BinaryOperatorSignature signature = Compilation.BuiltInOperators.GetSignature(binaryOperatorKind);
				Conversion leftConversion = ConversionsBase.FastClassifyConversion(type, signature.LeftType);
				Conversion rightConversion = ConversionsBase.FastClassifyConversion(type2, signature.RightType);
				result.Results.Add(BinaryOperatorAnalysisResult.Applicable(signature, leftConversion, rightConversion));
			}
		}
	}

	private static bool PossiblyUnusualConstantOperation(BoundExpression left, BoundExpression right)
	{
		if (left.ConstantValueOpt == null && right.ConstantValueOpt == null)
		{
			return false;
		}
		if (left.Type.SpecialType != right.Type.SpecialType)
		{
			return true;
		}
		if (left.Type.SpecialType == SpecialType.System_Int32 || left.Type.SpecialType == SpecialType.System_Boolean || left.Type.SpecialType == SpecialType.System_String)
		{
			return false;
		}
		return true;
	}

	public void BinaryOperatorOverloadResolution(BinaryOperatorKind kind, bool isChecked, string name1, string name2Opt, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		BinaryOperatorOverloadResolution_EasyOut(kind, left, right, result);
		if (result.Results.Count <= 0)
		{
			BinaryOperatorOverloadResolution_NoEasyOut(kind, isChecked, name1, name2Opt, left, right, result, ref useSiteInfo);
		}
	}

	internal void BinaryOperatorOverloadResolution_EasyOut(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result)
	{
		BinaryOperatorKind kind2 = kind & ~BinaryOperatorKind.Logical;
		BinaryOperatorEasyOut(kind2, left, right, result);
	}

	internal void BinaryOperatorOverloadResolution_NoEasyOut(BinaryOperatorKind kind, bool isChecked, string name1, string name2Opt, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeSymbol typeSymbol = left.Type?.StrippedType();
		TypeSymbol typeSymbol2 = right.Type?.StrippedType();
		bool flag = typeSymbol?.IsInterfaceType() ?? false;
		bool flag2 = typeSymbol2?.IsInterfaceType() ?? false;
		bool flag3 = false;
		if ((object)typeSymbol != null && !flag)
		{
			flag3 = GetUserDefinedOperators(kind, isChecked, name1, name2Opt, typeSymbol, left, right, result.Results, ref useSiteInfo);
			if (!flag3)
			{
				result.Results.Clear();
			}
		}
		bool flag4 = kind.IsShift();
		if (!flag4 && (object)typeSymbol2 != null && !flag2 && !typeSymbol2.Equals(typeSymbol))
		{
			ArrayBuilder<BinaryOperatorAnalysisResult> instance = ArrayBuilder<BinaryOperatorAnalysisResult>.GetInstance();
			if (GetUserDefinedOperators(kind, isChecked, name1, name2Opt, typeSymbol2, left, right, instance, ref useSiteInfo))
			{
				flag3 = true;
				AddDistinctOperators(result.Results, instance);
			}
			instance.Free();
		}
		if (!flag3)
		{
			result.Results.Clear();
			PooledDictionary<TypeSymbol, bool> instance2 = PooledDictionary<TypeSymbol, bool>.GetInstance();
			TypeSymbol typeSymbol3;
			TypeSymbol typeSymbol4;
			bool sourceIsInterface;
			bool sourceIsInterface2;
			if (!flag4 && ((object)typeSymbol == null || (!(typeSymbol is TypeParameterSymbol) && typeSymbol2 is TypeParameterSymbol)))
			{
				typeSymbol3 = typeSymbol2;
				typeSymbol4 = typeSymbol;
				sourceIsInterface = flag2;
				sourceIsInterface2 = flag;
			}
			else
			{
				typeSymbol3 = typeSymbol;
				typeSymbol4 = typeSymbol2;
				sourceIsInterface = flag;
				sourceIsInterface2 = flag2;
			}
			flag3 = GetUserDefinedBinaryOperatorsFromInterfaces(kind, isChecked, name1, name2Opt, typeSymbol3, sourceIsInterface, left, right, ref useSiteInfo, instance2, result.Results);
			if (!flag3)
			{
				result.Results.Clear();
			}
			if (!flag4 && (object)typeSymbol4 != null && !typeSymbol4.Equals(typeSymbol3))
			{
				ArrayBuilder<BinaryOperatorAnalysisResult> instance3 = ArrayBuilder<BinaryOperatorAnalysisResult>.GetInstance();
				if (GetUserDefinedBinaryOperatorsFromInterfaces(kind, isChecked, name1, name2Opt, typeSymbol4, sourceIsInterface2, left, right, ref useSiteInfo, instance2, instance3))
				{
					flag3 = true;
					AddDistinctOperators(result.Results, instance3);
				}
				instance3.Free();
			}
			instance2.Free();
		}
		if (!flag3)
		{
			result.Results.Clear();
			GetAllBuiltInOperators(kind, isChecked, left, right, result.Results, ref useSiteInfo);
		}
		BinaryOperatorOverloadResolution(left, right, result, ref useSiteInfo);
	}

	private bool GetUserDefinedBinaryOperatorsFromInterfaces(BinaryOperatorKind kind, bool isChecked, string name1, string name2Opt, TypeSymbol operatorSourceOpt, bool sourceIsInterface, BoundExpression left, BoundExpression right, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Dictionary<TypeSymbol, bool> lookedInInterfaces, ArrayBuilder<BinaryOperatorAnalysisResult> candidates)
	{
		if ((object)operatorSourceOpt == null)
		{
			return false;
		}
		bool flag = false;
		ImmutableArray<NamedTypeSymbol> immutableArray = default(ImmutableArray<NamedTypeSymbol>);
		TypeSymbol constrainedToTypeOpt = null;
		if (sourceIsInterface)
		{
			if (!lookedInInterfaces.TryGetValue(operatorSourceOpt, out var _))
			{
				ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
				GetUserDefinedBinaryOperatorsFromType(constrainedToTypeOpt, (NamedTypeSymbol)operatorSourceOpt, kind, name1, name2Opt, instance);
				flag = CandidateOperators(isChecked, instance, left, right, candidates, ref useSiteInfo);
				instance.Free();
				lookedInInterfaces.Add(operatorSourceOpt, flag);
				if (!flag)
				{
					candidates.Clear();
					immutableArray = operatorSourceOpt.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
				}
			}
		}
		else if (operatorSourceOpt.IsTypeParameter())
		{
			immutableArray = ((TypeParameterSymbol)operatorSourceOpt).AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
			constrainedToTypeOpt = operatorSourceOpt;
		}
		if (!immutableArray.IsDefaultOrEmpty)
		{
			ArrayBuilder<BinaryOperatorSignature> instance2 = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
			ArrayBuilder<BinaryOperatorAnalysisResult> instance3 = ArrayBuilder<BinaryOperatorAnalysisResult>.GetInstance();
			PooledHashSet<NamedTypeSymbol> instance4 = PooledHashSet<NamedTypeSymbol>.GetInstance();
			foreach (NamedTypeSymbol item in immutableArray)
			{
				if (!item.IsInterface || instance4.Contains(item))
				{
					continue;
				}
				if (lookedInInterfaces.TryGetValue(item, out var value2))
				{
					if (value2)
					{
						instance4.AddAll(item.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo));
					}
					continue;
				}
				instance2.Clear();
				instance3.Clear();
				GetUserDefinedBinaryOperatorsFromType(constrainedToTypeOpt, item, kind, name1, name2Opt, instance2);
				value2 = CandidateOperators(isChecked, instance2, left, right, instance3, ref useSiteInfo);
				lookedInInterfaces.Add(item, value2);
				if (value2)
				{
					flag = true;
					candidates.AddRange(instance3);
					instance4.AddAll(item.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo));
				}
			}
			instance2.Free();
			instance3.Free();
			instance4.Free();
		}
		return flag;
	}

	private void AddDelegateOperation(BinaryOperatorKind kind, TypeSymbol delegateType, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Delegate, delegateType, delegateType, Compilation.GetSpecialType(SpecialType.System_Boolean)));
			break;
		default:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Delegate, delegateType, delegateType, delegateType));
			break;
		}
	}

	private void GetDelegateOperations(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> operators, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.Division:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.LeftShift:
		case BinaryOperatorKind.RightShift:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
		case BinaryOperatorKind.UnsignedRightShift:
		case BinaryOperatorKind.LogicalAnd:
		case BinaryOperatorKind.LogicalOr:
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		case BinaryOperatorKind.Addition:
		case BinaryOperatorKind.Subtraction:
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		{
			TypeSymbol type = left.Type;
			bool flag = type?.IsDelegateType() ?? false;
			TypeSymbol type2 = right.Type;
			bool flag2 = type2?.IsDelegateType() ?? false;
			if (!flag && !flag2)
			{
				BinaryOperatorKind binaryOperatorKind = kind.Operator();
				if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
				{
					TypeSymbol specialType = _binder.Compilation.GetSpecialType(SpecialType.System_Delegate);
					specialType.AddUseSiteInfo(ref useSiteInfo);
					if (Conversions.ClassifyImplicitConversionFromExpression(left, specialType, ref useSiteInfo).IsValid && Conversions.ClassifyImplicitConversionFromExpression(right, specialType, ref useSiteInfo).IsValid)
					{
						AddDelegateOperation(kind, specialType, operators);
					}
				}
			}
			else if (flag & flag2)
			{
				AddDelegateOperation(kind, type, operators);
				if (!((kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual) ? ConversionsBase.HasIdentityConversion(type, type2) : type.Equals(type2)))
				{
					AddDelegateOperation(kind, type2, operators);
				}
			}
			else
			{
				TypeSymbol delegateType = (flag ? type : type2);
				BoundExpression boundExpression = (flag ? right : left);
				if ((kind != BinaryOperatorKind.Equal && kind != BinaryOperatorKind.NotEqual) || boundExpression.Kind != BoundKind.UnboundLambda)
				{
					AddDelegateOperation(kind, delegateType, operators);
				}
			}
			break;
		}
		}
	}

	private void GetEnumOperation(BinaryOperatorKind kind, TypeSymbol enumType, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		if (!enumType.IsValidEnumType())
		{
			return;
		}
		NamedTypeSymbol enumUnderlyingType = enumType.GetEnumUnderlyingType();
		NamedTypeSymbol orCreateNullableType = Compilation.GetOrCreateNullableType(enumType);
		NamedTypeSymbol orCreateNullableType2 = Compilation.GetOrCreateNullableType(enumUnderlyingType);
		switch (kind)
		{
		case BinaryOperatorKind.Addition:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingAddition, enumType, enumUnderlyingType, enumType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UnderlyingAndEnumAddition, enumUnderlyingType, enumType, enumType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingAddition, orCreateNullableType, orCreateNullableType2, orCreateNullableType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedUnderlyingAndEnumAddition, orCreateNullableType2, orCreateNullableType, orCreateNullableType));
			break;
		case BinaryOperatorKind.Subtraction:
		{
			if (Strict)
			{
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumSubtraction, enumType, enumType, enumUnderlyingType));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingSubtraction, enumType, enumUnderlyingType, enumType));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumSubtraction, orCreateNullableType, orCreateNullableType, orCreateNullableType2));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingSubtraction, orCreateNullableType, orCreateNullableType2, orCreateNullableType));
				break;
			}
			bool flag = TypeSymbol.Equals(right.Type?.StrippedType(), enumUnderlyingType, TypeCompareKind.ConsiderEverything);
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumSubtraction, enumType, enumType, enumUnderlyingType)
			{
				Priority = 2
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingSubtraction, enumType, enumUnderlyingType, enumType)
			{
				Priority = (flag ? 1 : 3)
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumSubtraction, orCreateNullableType, orCreateNullableType, orCreateNullableType2)
			{
				Priority = 12
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingSubtraction, orCreateNullableType, orCreateNullableType2, orCreateNullableType)
			{
				Priority = (flag ? 11 : 13)
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UnderlyingAndEnumSubtraction, enumUnderlyingType, enumType, enumType)
			{
				Priority = 4
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedUnderlyingAndEnumSubtraction, orCreateNullableType2, orCreateNullableType, orCreateNullableType)
			{
				Priority = 14
			});
			break;
		}
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		{
			NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Boolean);
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Enum, enumType, enumType, specialType));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Lifted | BinaryOperatorKind.Enum, orCreateNullableType, orCreateNullableType, specialType));
			break;
		}
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Enum, enumType, enumType, enumType));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Lifted | BinaryOperatorKind.Enum, orCreateNullableType, orCreateNullableType, orCreateNullableType));
			break;
		}
	}

	private void GetPointerArithmeticOperators(BinaryOperatorKind kind, PointerTypeSymbol pointerType, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Addition:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndIntAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndUIntAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndLongAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndULongAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.IntAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UIntAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LongAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.ULongAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType, pointerType));
			break;
		case BinaryOperatorKind.Subtraction:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndIntSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndUIntSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndLongSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndULongSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerSubtraction, pointerType, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64)));
			break;
		}
	}

	private void GetPointerComparisonOperators(BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		{
			PointerTypeSymbol pointerTypeSymbol = new PointerTypeSymbol(TypeWithAnnotations.Create(Compilation.GetSpecialType(SpecialType.System_Void)));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Pointer, pointerTypeSymbol, pointerTypeSymbol, Compilation.GetSpecialType(SpecialType.System_Boolean)));
			break;
		}
		}
	}

	private void GetEnumOperations(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> results)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.Division:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.LeftShift:
		case BinaryOperatorKind.RightShift:
		case BinaryOperatorKind.UnsignedRightShift:
		case BinaryOperatorKind.LogicalAnd:
		case BinaryOperatorKind.LogicalOr:
			return;
		}
		TypeSymbol typeSymbol = left.Type;
		if ((object)typeSymbol != null)
		{
			typeSymbol = typeSymbol.StrippedType();
		}
		TypeSymbol typeSymbol2 = right.Type;
		if ((object)typeSymbol2 != null)
		{
			typeSymbol2 = typeSymbol2.StrippedType();
		}
		bool flag;
		switch (kind)
		{
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			flag = false;
			break;
		case BinaryOperatorKind.Addition:
			flag = true;
			break;
		case BinaryOperatorKind.Subtraction:
			flag = true;
			break;
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			flag = true;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
		if ((object)typeSymbol != null)
		{
			GetEnumOperation(kind, typeSymbol, right, results);
		}
		if ((object)typeSymbol2 != null && ((object)typeSymbol == null || !(flag ? ConversionsBase.HasIdentityConversion(typeSymbol2, typeSymbol) : typeSymbol2.Equals(typeSymbol))))
		{
			GetEnumOperation(kind, typeSymbol2, right, results);
		}
	}

	private void GetPointerOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> results)
	{
		PointerTypeSymbol pointerTypeSymbol = left.Type as PointerTypeSymbol;
		PointerTypeSymbol pointerTypeSymbol2 = right.Type as PointerTypeSymbol;
		if ((object)pointerTypeSymbol != null)
		{
			GetPointerArithmeticOperators(kind, pointerTypeSymbol, results);
		}
		if ((object)pointerTypeSymbol2 != null && ((object)pointerTypeSymbol == null || !ConversionsBase.HasIdentityConversion(pointerTypeSymbol2, pointerTypeSymbol)))
		{
			GetPointerArithmeticOperators(kind, pointerTypeSymbol2, results);
		}
		if ((object)pointerTypeSymbol != null || (object)pointerTypeSymbol2 != null || left.Type is FunctionPointerTypeSymbol || right.Type is FunctionPointerTypeSymbol)
		{
			GetPointerComparisonOperators(kind, results);
		}
	}

	private void GetAllBuiltInOperators(BinaryOperatorKind kind, bool isChecked, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		kind = kind.OperatorWithLogical();
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		if ((kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual) && useOnlyReferenceEquality(Conversions, left, right, ref useSiteInfo))
		{
			GetReferenceEquality(kind, instance);
			if ((left.Type is TypeParameterSymbol { AllowsRefLikeType: not false } && right.IsLiteralNull()) || (right.Type is TypeParameterSymbol { AllowsRefLikeType: not false } && left.IsLiteralNull()))
			{
				BinaryOperatorSignature signature = instance[0];
				Conversion leftConversion = getOperandConversionForAllowByRefLikeNullCheck(isChecked, left, signature.LeftType, ref useSiteInfo);
				Conversion rightConversion = getOperandConversionForAllowByRefLikeNullCheck(isChecked, right, signature.RightType, ref useSiteInfo);
				results.Add(BinaryOperatorAnalysisResult.Applicable(signature, leftConversion, rightConversion));
				instance.Free();
				return;
			}
		}
		else
		{
			Compilation.BuiltInOperators.GetSimpleBuiltInOperators(kind, instance, !left.Type.IsNativeIntegerOrNullableThereof() && !right.Type.IsNativeIntegerOrNullableThereof());
			GetDelegateOperations(kind, left, right, instance, ref useSiteInfo);
			GetEnumOperations(kind, left, right, instance);
			GetPointerOperators(kind, left, right, instance);
			if (kind.Operator() == BinaryOperatorKind.Addition && isUtf8ByteRepresentation(left) && isUtf8ByteRepresentation(right))
			{
				Compilation.BuiltInOperators.GetUtf8ConcatenationBuiltInOperator(left.Type, instance);
			}
		}
		CandidateOperators(isChecked, instance, left, right, results, ref useSiteInfo);
		instance.Free();
		Conversion getOperandConversionForAllowByRefLikeNullCheck(bool isChecked2, BoundExpression operand, TypeSymbol objectType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (!(operand.Type is TypeParameterSymbol { AllowsRefLikeType: not false }))
			{
				return Conversions.ClassifyConversionFromExpression(operand, objectType, isChecked2, ref useSiteInfo2);
			}
			return Conversion.Boxing;
		}
		static bool isUtf8ByteRepresentation(BoundExpression value)
		{
			if (value is BoundUtf8String || value is BoundBinaryOperator { OperatorKind: BinaryOperatorKind.Utf8Addition })
			{
				return true;
			}
			return false;
		}
		static bool useOnlyReferenceEquality(Conversions conversions, BoundExpression boundExpression, BoundExpression boundExpression2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (BuiltInOperators.IsValidObjectEquality(conversions, boundExpression.Type, boundExpression.IsLiteralNull(), leftIsDefault: false, boundExpression2.Type, boundExpression2.IsLiteralNull(), rightIsDefault: false, ref useSiteInfo2) && ((object)boundExpression.Type == null || (!boundExpression.Type.IsDelegateType() && boundExpression.Type.SpecialType != SpecialType.System_String && boundExpression.Type.SpecialType != SpecialType.System_Delegate)))
			{
				if ((object)boundExpression2.Type != null)
				{
					if (!boundExpression2.Type.IsDelegateType() && boundExpression2.Type.SpecialType != SpecialType.System_String)
					{
						return boundExpression2.Type.SpecialType != SpecialType.System_Delegate;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	private void GetReferenceEquality(BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Object);
		operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Object, specialType, specialType, Compilation.GetSpecialType(SpecialType.System_Boolean)));
	}

	private bool CandidateOperators(bool isChecked, ArrayBuilder<BinaryOperatorSignature> operators, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool result = false;
		foreach (BinaryOperatorSignature @operator in operators)
		{
			Conversion leftConversion = Conversions.ClassifyConversionFromExpression(left, @operator.LeftType, isChecked, ref useSiteInfo);
			Conversion rightConversion = Conversions.ClassifyConversionFromExpression(right, @operator.RightType, isChecked, ref useSiteInfo);
			if (leftConversion.IsImplicit && rightConversion.IsImplicit)
			{
				results.Add(BinaryOperatorAnalysisResult.Applicable(@operator, leftConversion, rightConversion));
				result = true;
			}
			else
			{
				results.Add(BinaryOperatorAnalysisResult.Inapplicable(@operator, leftConversion, rightConversion));
			}
		}
		return result;
	}

	private static void AddDistinctOperators(ArrayBuilder<BinaryOperatorAnalysisResult> result, ArrayBuilder<BinaryOperatorAnalysisResult> additionalOperators)
	{
		int count = result.Count;
		foreach (BinaryOperatorAnalysisResult additionalOperator in additionalOperators)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				BinaryOperatorSignature signature = result[i].Signature;
				if (additionalOperator.Signature.Kind == signature.Kind && equalsIgnoringNullable(additionalOperator.Signature.ReturnType, signature.ReturnType) && equalsIgnoringNullableAndDynamic(additionalOperator.Signature.LeftType, signature.LeftType) && equalsIgnoringNullableAndDynamic(additionalOperator.Signature.RightType, signature.RightType) && equalsIgnoringNullableAndDynamic(additionalOperator.Signature.Method.ContainingType, signature.Method.ContainingType))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				result.Add(additionalOperator);
			}
		}
		static bool equalsIgnoringNullable(TypeSymbol a, TypeSymbol b)
		{
			return a.Equals(b, TypeCompareKind.AllNullableIgnoreOptions);
		}
		static bool equalsIgnoringNullableAndDynamic(TypeSymbol a, TypeSymbol b)
		{
			return a.Equals(b, TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.IgnoreDynamic);
		}
	}

	private bool GetUserDefinedOperators(BinaryOperatorKind kind, bool isChecked, string name1, string name2Opt, TypeSymbol type0, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)type0 == null || OperatorFacts.DefinitelyHasNoUserDefinedOperators(type0))
		{
			return false;
		}
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		bool result = false;
		NamedTypeSymbol namedTypeSymbol = type0 as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = type0.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		if ((object)namedTypeSymbol == null && type0.IsTypeParameter())
		{
			namedTypeSymbol = ((TypeParameterSymbol)type0).EffectiveBaseClass(ref useSiteInfo);
		}
		while ((object)namedTypeSymbol != null)
		{
			instance.Clear();
			GetUserDefinedBinaryOperatorsFromType(null, namedTypeSymbol, kind, name1, name2Opt, instance);
			results.Clear();
			if (CandidateOperators(isChecked, instance, left, right, results, ref useSiteInfo))
			{
				result = true;
				break;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		instance.Free();
		return result;
	}

	internal static void GetStaticUserDefinedBinaryOperatorMethodNames(BinaryOperatorKind kind, bool isChecked, out string name1, out string? name2Opt)
	{
		name1 = OperatorFacts.BinaryOperatorNameFromOperatorKind(kind, isChecked);
		if (isChecked && SyntaxFacts.IsCheckedOperator(name1))
		{
			name2Opt = OperatorFacts.BinaryOperatorNameFromOperatorKind(kind, isChecked: false);
		}
		else
		{
			name2Opt = null;
		}
	}

	private void GetUserDefinedBinaryOperatorsFromType(TypeSymbol constrainedToTypeOpt, NamedTypeSymbol type, BinaryOperatorKind kind, string name1, string? name2Opt, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		GetDeclaredUserDefinedBinaryOperators(constrainedToTypeOpt, type, kind, name1, operators);
		if (name2Opt != null)
		{
			ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
			GetDeclaredUserDefinedBinaryOperators(constrainedToTypeOpt, type, kind, name2Opt, instance);
			if (operators.Count != 0)
			{
				for (int num = instance.Count - 1; num >= 0; num--)
				{
					foreach (BinaryOperatorSignature @operator in operators)
					{
						if (SourceMemberContainerTypeSymbol.DoOperatorsPair(@operator.Method, instance[num].Method))
						{
							instance.RemoveAt(num);
							break;
						}
					}
				}
			}
			operators.AddRange(instance);
			instance.Free();
		}
		AddLiftedUserDefinedBinaryOperators(constrainedToTypeOpt, kind, operators);
	}

	private static void GetDeclaredUserDefinedBinaryOperators(TypeSymbol? constrainedToTypeOpt, NamedTypeSymbol type, BinaryOperatorKind kind, string name, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		type.AddOperators(name, instance);
		GetDeclaredUserDefinedBinaryOperators(constrainedToTypeOpt, instance, kind, name, operators);
		instance.Free();
	}

	private static void GetDeclaredUserDefinedBinaryOperators(TypeSymbol? constrainedToTypeOpt, ArrayBuilder<MethodSymbol> typeOperators, BinaryOperatorKind kind, string name, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		foreach (MethodSymbol typeOperator in typeOperators)
		{
			if (!(typeOperator.Name != name) && typeOperator.IsStatic && typeOperator.ParameterCount == 2 && !typeOperator.ReturnsVoid)
			{
				TypeSymbol parameterType = typeOperator.GetParameterType(0);
				TypeSymbol parameterType2 = typeOperator.GetParameterType(1);
				TypeSymbol returnType = typeOperator.ReturnType;
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | kind, parameterType, parameterType2, returnType, typeOperator, constrainedToTypeOpt));
			}
		}
	}

	private void AddLiftedUserDefinedBinaryOperators(TypeSymbol? constrainedToTypeOpt, BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		for (int num = operators.Count - 1; num >= 0; num--)
		{
			MethodSymbol method = operators[num].Method;
			TypeSymbol parameterType = method.GetParameterType(0);
			TypeSymbol parameterType2 = method.GetParameterType(1);
			TypeSymbol returnType = method.ReturnType;
			switch (UserDefinedBinaryOperatorCanBeLifted(parameterType, parameterType2, returnType, kind))
			{
			case LiftingResult.LiftOperandsAndResult:
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | BinaryOperatorKind.Lifted | kind, MakeNullable(parameterType), MakeNullable(parameterType2), MakeNullable(returnType), method, constrainedToTypeOpt));
				break;
			case LiftingResult.LiftOperandsButNotResult:
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | BinaryOperatorKind.Lifted | kind, MakeNullable(parameterType), MakeNullable(parameterType2), returnType, method, constrainedToTypeOpt));
				break;
			}
		}
	}

	private static LiftingResult UserDefinedBinaryOperatorCanBeLifted(TypeSymbol left, TypeSymbol right, TypeSymbol result, BinaryOperatorKind kind)
	{
		if (!left.IsValidNullableTypeArgument() || !right.IsValidNullableTypeArgument())
		{
			return LiftingResult.NotLifted;
		}
		if (kind <= BinaryOperatorKind.GreaterThan)
		{
			if (kind != BinaryOperatorKind.Equal && kind != BinaryOperatorKind.NotEqual)
			{
				if (kind != BinaryOperatorKind.GreaterThan)
				{
					goto IL_0067;
				}
			}
			else if (!TypeSymbol.Equals(left, right, TypeCompareKind.ConsiderEverything))
			{
				return LiftingResult.NotLifted;
			}
		}
		else if (kind != BinaryOperatorKind.LessThan && kind != BinaryOperatorKind.GreaterThanOrEqual && kind != BinaryOperatorKind.LessThanOrEqual)
		{
			goto IL_0067;
		}
		if (result.SpecialType != SpecialType.System_Boolean)
		{
			return LiftingResult.NotLifted;
		}
		return LiftingResult.LiftOperandsButNotResult;
		IL_0067:
		if (!result.IsValidNullableTypeArgument())
		{
			return LiftingResult.NotLifted;
		}
		return LiftingResult.LiftOperandsAndResult;
	}

	private void BinaryOperatorOverloadResolution(BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (result.SingleValid())
		{
			return;
		}
		ArrayBuilder<BinaryOperatorAnalysisResult> results = result.Results;
		RemoveLowerPriorityMembers<BinaryOperatorAnalysisResult, MethodSymbol>(results);
		int theBestCandidateIndex = GetTheBestCandidateIndex(left, right, results, ref useSiteInfo);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].Kind != OperatorAnalysisResultKind.Inapplicable && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		for (int j = 1; j < results.Count; j++)
		{
			if (results[j].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int k = 0; k < j; k++)
			{
				if (results[k].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[j].Signature, results[k].Signature, left, right, ref useSiteInfo))
					{
					case BetterResult.Left:
						results[k] = results[k].Worse();
						break;
					case BetterResult.Right:
						results[j] = results[j].Worse();
						break;
					}
				}
			}
		}
	}

	private int GetTheBestCandidateIndex(BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> candidates, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		int num = -1;
		for (int i = 0; i < candidates.Count; i++)
		{
			if (candidates[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			switch (BetterOperator(candidates[num].Signature, candidates[i].Signature, left, right, ref useSiteInfo))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (candidates[j].Kind != OperatorAnalysisResultKind.Inapplicable && BetterOperator(candidates[num].Signature, candidates[j].Signature, left, right, ref useSiteInfo) != BetterResult.Left)
			{
				return -1;
			}
		}
		return num;
	}

	private BetterResult BetterOperator(BinaryOperatorSignature op1, BinaryOperatorSignature op2, BoundExpression left, BoundExpression right, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (op1.Priority.HasValue && op1.Priority.GetValueOrDefault() != op2.Priority.GetValueOrDefault())
		{
			if (op1.Priority.GetValueOrDefault() >= op2.Priority.GetValueOrDefault())
			{
				return BetterResult.Right;
			}
			return BetterResult.Left;
		}
		BetterResult betterResult = BetterConversionFromExpression(left, op1.LeftType, op2.LeftType, ref useSiteInfo);
		BetterResult betterResult2 = BetterConversionFromExpression(right, op1.RightType, op2.RightType, ref useSiteInfo);
		if ((betterResult == BetterResult.Left && betterResult2 != BetterResult.Right) || (betterResult != BetterResult.Right && betterResult2 == BetterResult.Left))
		{
			return BetterResult.Left;
		}
		if ((betterResult == BetterResult.Right && betterResult2 != BetterResult.Left) || (betterResult != BetterResult.Left && betterResult2 == BetterResult.Right))
		{
			return BetterResult.Right;
		}
		if (ConversionsBase.HasIdentityConversion(op1.LeftType, op2.LeftType) && ConversionsBase.HasIdentityConversion(op1.RightType, op2.RightType))
		{
			int? num = op1.Method?.GetMemberArityIncludingExtension();
			if ((!num.HasValue || num.GetValueOrDefault() == 0) ? true : false)
			{
				MethodSymbol method = op2.Method;
				if ((object)method != null && method.GetMemberArityIncludingExtension() > 0)
				{
					return BetterResult.Left;
				}
			}
			else
			{
				num = op2.Method?.GetMemberArityIncludingExtension();
				if ((!num.HasValue || num.GetValueOrDefault() == 0) ? true : false)
				{
					return BetterResult.Right;
				}
			}
			BetterResult betterResult3 = MoreSpecificOperator(op1, op2, ref useSiteInfo);
			if (betterResult3 == BetterResult.Left || betterResult3 == BetterResult.Right)
			{
				return betterResult3;
			}
			bool flag = op1.Kind.IsLifted();
			bool flag2 = op2.Kind.IsLifted();
			if (flag && !flag2)
			{
				return BetterResult.Right;
			}
			if (!flag & flag2)
			{
				return BetterResult.Left;
			}
		}
		BetterResult betterResult4 = ((op1.LeftRefKind != RefKind.None || op2.LeftRefKind != RefKind.In) ? ((op2.LeftRefKind == RefKind.None && op1.LeftRefKind == RefKind.In) ? BetterResult.Right : BetterResult.Neither) : BetterResult.Left);
		if (op1.RightRefKind == RefKind.None && op2.RightRefKind == RefKind.In)
		{
			if (betterResult4 == BetterResult.Right)
			{
				return BetterResult.Neither;
			}
			betterResult4 = BetterResult.Left;
		}
		else if (op2.RightRefKind == RefKind.None && op1.RightRefKind == RefKind.In)
		{
			if (betterResult4 == BetterResult.Left)
			{
				return BetterResult.Neither;
			}
			betterResult4 = BetterResult.Right;
		}
		return betterResult4;
	}

	private BetterResult MoreSpecificOperator(BinaryOperatorSignature op1, BinaryOperatorSignature op2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeSymbol typeSymbol;
		TypeSymbol typeSymbol2;
		if ((object)op1.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters = op1.Method.OriginalDefinition.GetParameters();
			typeSymbol = parameters[0].Type;
			typeSymbol2 = parameters[1].Type;
			if (op1.Kind.IsLifted())
			{
				typeSymbol = MakeNullable(typeSymbol);
				typeSymbol2 = MakeNullable(typeSymbol2);
			}
		}
		else
		{
			typeSymbol = op1.LeftType;
			typeSymbol2 = op1.RightType;
		}
		TypeSymbol typeSymbol3;
		TypeSymbol typeSymbol4;
		if ((object)op2.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters2 = op2.Method.OriginalDefinition.GetParameters();
			typeSymbol3 = parameters2[0].Type;
			typeSymbol4 = parameters2[1].Type;
			if (op2.Kind.IsLifted())
			{
				typeSymbol3 = MakeNullable(typeSymbol3);
				typeSymbol4 = MakeNullable(typeSymbol4);
			}
		}
		else
		{
			typeSymbol3 = op2.LeftType;
			typeSymbol4 = op2.RightType;
		}
		using TemporaryArray<TypeSymbol> array = TemporaryArray<TypeSymbol>.Empty;
		using TemporaryArray<TypeSymbol> array2 = TemporaryArray<TypeSymbol>.Empty;
		array.Add(typeSymbol);
		array.Add(typeSymbol2);
		array2.Add(typeSymbol3);
		array2.Add(typeSymbol4);
		return MoreSpecificType(ref TemporaryArrayExtensions.AsRef(in array), ref TemporaryArrayExtensions.AsRef(in array2), ref useSiteInfo);
	}

	[Conditional("DEBUG")]
	private static void AssertNotChecked(BinaryOperatorKind kind)
	{
	}

	public bool BinaryOperatorExtensionOverloadResolutionInSingleScope(ArrayBuilder<Symbol> extensionCandidatesInSingleScope, BinaryOperatorKind kind, bool isChecked, string name1, string? name2Opt, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		OverloadResolution overloadResolution = this;
		BinaryOperatorKind kind2 = kind;
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		getDeclaredUserDefinedBinaryOperatorsInScope(extensionCandidatesInSingleScope, kind2, name1, name2Opt, instance);
		TypeSymbol? type = left.Type;
		if ((object)type == null || !type.IsNullableType())
		{
			TypeSymbol? type2 = right.Type;
			if ((object)type2 == null || !type2.IsNullableType())
			{
				goto IL_005f;
			}
		}
		AddLiftedUserDefinedBinaryOperators(null, kind2, instance);
		goto IL_005f;
		IL_005f:
		inferTypeArgumentsAndRemoveInapplicableToReceiverType(kind2, left, right, instance, ref useSiteInfo);
		bool result2 = false;
		if (!instance.IsEmpty)
		{
			ArrayBuilder<BinaryOperatorAnalysisResult> results = result.Results;
			results.Clear();
			if (CandidateOperators(isChecked, instance, left, right, results, ref useSiteInfo))
			{
				BinaryOperatorOverloadResolution(left, right, result, ref useSiteInfo);
				result2 = true;
			}
		}
		instance.Free();
		return result2;
		static void getDeclaredUserDefinedBinaryOperators(ArrayBuilder<Symbol> candidates, BinaryOperatorKind kind3, string name2, ArrayBuilder<BinaryOperatorSignature> operators)
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			NamedTypeSymbol.AddOperators(instance2, candidates);
			GetDeclaredUserDefinedBinaryOperators(null, instance2, kind3, name2, operators);
			instance2.Free();
		}
		static void getDeclaredUserDefinedBinaryOperatorsInScope(ArrayBuilder<Symbol> extensionCandidatesInSingleScope2, BinaryOperatorKind kind3, string name2, string? text, ArrayBuilder<BinaryOperatorSignature> operators)
		{
			getDeclaredUserDefinedBinaryOperators(extensionCandidatesInSingleScope2, kind3, name2, operators);
			if (text != null)
			{
				if (!operators.IsEmpty)
				{
					HashSet<MethodSymbol> hashSet = new HashSet<MethodSymbol>(PairedExtensionOperatorSignatureComparer.Instance);
					hashSet.AddRange(operators.Select((BinaryOperatorSignature op) => op.Method));
					ArrayBuilder<BinaryOperatorSignature> instance2 = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
					getDeclaredUserDefinedBinaryOperators(extensionCandidatesInSingleScope2, kind3, text, instance2);
					foreach (BinaryOperatorSignature item in instance2)
					{
						if (!hashSet.Contains(item.Method))
						{
							operators.Add(item);
						}
					}
					instance2.Free();
				}
				else
				{
					getDeclaredUserDefinedBinaryOperators(extensionCandidatesInSingleScope2, kind3, text, operators);
				}
			}
		}
		void inferTypeArgumentsAndRemoveInapplicableToReceiverType(BinaryOperatorKind binaryOperatorKind, BoundExpression boundExpression, BoundExpression boundExpression2, ArrayBuilder<BinaryOperatorSignature> operators, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			for (int num = operators.Count - 1; num >= 0; num--)
			{
				BinaryOperatorSignature candidate = operators[num];
				MethodSymbol method = candidate.Method;
				NamedTypeSymbol containingType = method.ContainingType;
				if (containingType.Arity == 0)
				{
					if (isApplicableToReceiver(in candidate, boundExpression, boundExpression2, ref useSiteInfo2))
					{
						continue;
					}
				}
				else
				{
					MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, Conversions, containingType.TypeParameters, containingType, ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[2]
					{
						TypeWithAnnotations.Create(candidate.LeftType),
						TypeWithAnnotations.Create(candidate.RightType)
					}), method.ParameterRefKinds, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2] { boundExpression, boundExpression2 }), ref useSiteInfo2);
					if (methodTypeInferenceResult.Success)
					{
						containingType = containingType.Construct(methodTypeInferenceResult.InferredTypeArguments);
						method = method.AsMember(containingType);
						if (!FailsConstraintChecks(method, out var constraintFailureDiagnosticsOpt, CompoundUseSiteInfo<AssemblySymbol>.Discarded))
						{
							TypeSymbol parameterType = method.GetParameterType(0);
							TypeSymbol parameterType2 = method.GetParameterType(1);
							TypeSymbol returnType = method.ReturnType;
							BinaryOperatorSignature candidate2;
							if (candidate.Kind.IsLifted())
							{
								LiftingResult liftingResult = UserDefinedBinaryOperatorCanBeLifted(parameterType, parameterType2, returnType, binaryOperatorKind);
								candidate2 = new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | BinaryOperatorKind.Lifted | binaryOperatorKind, MakeNullable(parameterType), MakeNullable(parameterType2), (liftingResult == LiftingResult.LiftOperandsButNotResult) ? returnType : MakeNullable(returnType), method, null);
							}
							else
							{
								candidate2 = new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | binaryOperatorKind, parameterType, parameterType2, returnType, method, null);
							}
							if (isApplicableToReceiver(in candidate2, boundExpression, boundExpression2, ref useSiteInfo2))
							{
								operators[num] = candidate2;
								continue;
							}
						}
						constraintFailureDiagnosticsOpt?.Free();
					}
				}
				operators.RemoveAt(num);
			}
		}
		bool isApplicableToReceiver(in BinaryOperatorSignature candidate, BoundExpression boundExpression, BoundExpression boundExpression2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if ((object)boundExpression.Type != null && parameterMatchesReceiver(in candidate, 0) && isOperandApplicableToReceiver(in candidate, boundExpression, ref useSiteInfo2))
			{
				return true;
			}
			if (!kind2.IsShift() && (object)boundExpression2.Type != null && parameterMatchesReceiver(in candidate, 1) && isOperandApplicableToReceiver(in candidate, boundExpression2, ref useSiteInfo2))
			{
				return true;
			}
			return false;
		}
		bool isOperandApplicableToReceiver(in BinaryOperatorSignature candidate, BoundExpression operand, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (candidate.Kind.IsLifted() && operand.Type.IsNullableType())
			{
				if (!candidate.Method.ContainingType.ExtensionParameter.Type.IsValidNullableTypeArgument() || !Conversions.ConvertExtensionMethodThisArg(MakeNullable(candidate.Method.ContainingType.ExtensionParameter.Type), operand.Type, ref useSiteInfo2, isMethodGroupConversion: false).Exists)
				{
					return false;
				}
			}
			else if (!Conversions.ConvertExtensionMethodThisArg(candidate.Method.ContainingType.ExtensionParameter.Type, operand.Type, ref useSiteInfo2, isMethodGroupConversion: false).Exists)
			{
				return false;
			}
			return true;
		}
		static bool parameterMatchesReceiver(in BinaryOperatorSignature candidate, int paramIndex)
		{
			MethodSymbol originalDefinition = candidate.Method.OriginalDefinition;
			return SourceUserDefinedOperatorSymbolBase.ExtensionOperatorParameterTypeMatchesExtendedType(extendedType: originalDefinition.ContainingType.ExtensionParameter.Type, type: originalDefinition.Parameters[paramIndex].Type);
		}
	}

	private void UnaryOperatorEasyOut(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol type = operand.Type;
		if ((object)type != null)
		{
			UnaryOperatorKind unaryOperatorKind = UnopEasyOut.OpKind(kind, type);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				UnaryOperatorSignature signature = Compilation.BuiltInOperators.GetSignature(unaryOperatorKind);
				Conversion? conversion = ConversionsBase.FastClassifyConversion(type, signature.OperandType);
				result.Results.Add(UnaryOperatorAnalysisResult.Applicable(signature, conversion.Value));
			}
		}
	}

	private NamedTypeSymbol MakeNullable(TypeSymbol type)
	{
		return Compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(type);
	}

	public void UnaryOperatorOverloadResolution(UnaryOperatorKind kind, bool isChecked, string name1, string name2Opt, BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		UnaryOperatorEasyOut(kind, operand, result);
		if (result.Results.Count <= 0)
		{
			if (!GetUserDefinedOperators(kind, isChecked, name1, name2Opt, operand, result.Results, ref useSiteInfo))
			{
				result.Results.Clear();
				GetAllBuiltInOperators(kind, isChecked, operand, result.Results, ref useSiteInfo);
			}
			UnaryOperatorOverloadResolution(operand, result, ref useSiteInfo);
		}
	}

	public bool UnaryOperatorExtensionOverloadResolutionInSingleScope(ArrayBuilder<Symbol> extensionCandidatesInSingleScope, UnaryOperatorKind kind, bool isChecked, string name1, string? name2Opt, BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		getDeclaredUserDefinedUnaryOperatorsInScope(extensionCandidatesInSingleScope, kind, name1, name2Opt, instance);
		if (operand.Type.IsNullableType())
		{
			AddLiftedUserDefinedUnaryOperators(null, kind, instance);
		}
		inferTypeArgumentsAndRemoveInapplicableToReceiverType(kind, operand, instance, ref useSiteInfo);
		bool result2 = false;
		if (!instance.IsEmpty)
		{
			ArrayBuilder<UnaryOperatorAnalysisResult> results = result.Results;
			results.Clear();
			if (CandidateOperators(isChecked, instance, operand, results, ref useSiteInfo))
			{
				UnaryOperatorOverloadResolution(operand, result, ref useSiteInfo);
				result2 = true;
			}
		}
		instance.Free();
		return result2;
		static void getDeclaredUserDefinedUnaryOperators(ArrayBuilder<Symbol> candidates, UnaryOperatorKind kind2, string name2, ArrayBuilder<UnaryOperatorSignature> operators)
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			NamedTypeSymbol.AddOperators(instance2, candidates);
			GetDeclaredUserDefinedUnaryOperators(null, instance2, kind2, name2, operators);
			instance2.Free();
		}
		static void getDeclaredUserDefinedUnaryOperatorsInScope(ArrayBuilder<Symbol> extensionCandidatesInSingleScope2, UnaryOperatorKind kind2, string name2, string? text, ArrayBuilder<UnaryOperatorSignature> operators)
		{
			getDeclaredUserDefinedUnaryOperators(extensionCandidatesInSingleScope2, kind2, name2, operators);
			if (text != null)
			{
				if (!operators.IsEmpty)
				{
					HashSet<MethodSymbol> hashSet = new HashSet<MethodSymbol>(PairedExtensionOperatorSignatureComparer.Instance);
					hashSet.AddRange(operators.Select((UnaryOperatorSignature op) => op.Method));
					ArrayBuilder<UnaryOperatorSignature> instance2 = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
					getDeclaredUserDefinedUnaryOperators(extensionCandidatesInSingleScope2, kind2, text, instance2);
					foreach (UnaryOperatorSignature item in instance2)
					{
						if (!hashSet.Contains(item.Method))
						{
							operators.Add(item);
						}
					}
					instance2.Free();
				}
				else
				{
					getDeclaredUserDefinedUnaryOperators(extensionCandidatesInSingleScope2, kind2, text, operators);
				}
			}
		}
		void inferTypeArgumentsAndRemoveInapplicableToReceiverType(UnaryOperatorKind unaryOperatorKind, BoundExpression boundExpression, ArrayBuilder<UnaryOperatorSignature> operators, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			for (int num = operators.Count - 1; num >= 0; num--)
			{
				UnaryOperatorSignature candidate = operators[num];
				MethodSymbol method = candidate.Method;
				NamedTypeSymbol containingType = method.ContainingType;
				if (containingType.Arity == 0)
				{
					if (isApplicableToReceiver(in candidate, boundExpression, ref useSiteInfo2))
					{
						continue;
					}
				}
				else
				{
					MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, Conversions, containingType.TypeParameters, containingType, ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { TypeWithAnnotations.Create(candidate.OperandType) }), method.ParameterRefKinds, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundExpression }), ref useSiteInfo2);
					if (methodTypeInferenceResult.Success)
					{
						containingType = containingType.Construct(methodTypeInferenceResult.InferredTypeArguments);
						method = method.AsMember(containingType);
						if (!FailsConstraintChecks(method, out var constraintFailureDiagnosticsOpt, CompoundUseSiteInfo<AssemblySymbol>.Discarded))
						{
							TypeSymbol parameterType = method.GetParameterType(0);
							TypeSymbol returnType = method.ReturnType;
							UnaryOperatorSignature candidate2 = ((!candidate.Kind.IsLifted()) ? new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | unaryOperatorKind, parameterType, returnType, method, null) : new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | UnaryOperatorKind.Lifted | unaryOperatorKind, MakeNullable(parameterType), MakeNullable(returnType), method, null));
							if (isApplicableToReceiver(in candidate2, boundExpression, ref useSiteInfo2))
							{
								operators[num] = candidate2;
								continue;
							}
						}
						constraintFailureDiagnosticsOpt?.Free();
					}
				}
				operators.RemoveAt(num);
			}
		}
		bool isApplicableToReceiver(in UnaryOperatorSignature candidate, BoundExpression boundExpression, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (candidate.Kind.IsLifted())
			{
				if (!candidate.Method.ContainingType.ExtensionParameter.Type.IsValidNullableTypeArgument() || !Conversions.ConvertExtensionMethodThisArg(MakeNullable(candidate.Method.ContainingType.ExtensionParameter.Type), boundExpression.Type, ref useSiteInfo2, isMethodGroupConversion: false).Exists)
				{
					return false;
				}
			}
			else if (!Conversions.ConvertExtensionMethodThisArg(candidate.Method.ContainingType.ExtensionParameter.Type, boundExpression.Type, ref useSiteInfo2, isMethodGroupConversion: false).Exists)
			{
				return false;
			}
			return true;
		}
	}

	internal void UnaryOperatorOverloadResolution(BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (result.SingleValid())
		{
			return;
		}
		ArrayBuilder<UnaryOperatorAnalysisResult> results = result.Results;
		RemoveLowerPriorityMembers<UnaryOperatorAnalysisResult, MethodSymbol>(results);
		int theBestCandidateIndex = GetTheBestCandidateIndex(operand, results, ref useSiteInfo);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].Kind != OperatorAnalysisResultKind.Inapplicable && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		for (int j = 1; j < results.Count; j++)
		{
			if (results[j].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int k = 0; k < j; k++)
			{
				if (results[k].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[j].Signature, results[k].Signature, operand, ref useSiteInfo))
					{
					case BetterResult.Left:
						results[k] = results[k].Worse();
						break;
					case BetterResult.Right:
						results[j] = results[j].Worse();
						break;
					}
				}
			}
		}
	}

	private int GetTheBestCandidateIndex(BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> candidates, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		int num = -1;
		for (int i = 0; i < candidates.Count; i++)
		{
			if (candidates[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			switch (BetterOperator(candidates[num].Signature, candidates[i].Signature, operand, ref useSiteInfo))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (candidates[j].Kind != OperatorAnalysisResultKind.Inapplicable && BetterOperator(candidates[num].Signature, candidates[j].Signature, operand, ref useSiteInfo) != BetterResult.Left)
			{
				return -1;
			}
		}
		return num;
	}

	private BetterResult BetterOperator(UnaryOperatorSignature op1, UnaryOperatorSignature op2, BoundExpression operand, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		BetterResult betterResult = BetterConversionFromExpression(operand, op1.OperandType, op2.OperandType, ref useSiteInfo);
		if (betterResult == BetterResult.Left || betterResult == BetterResult.Right)
		{
			return betterResult;
		}
		if (ConversionsBase.HasIdentityConversion(op1.OperandType, op2.OperandType))
		{
			int? num = op1.Method?.GetMemberArityIncludingExtension();
			if ((!num.HasValue || num.GetValueOrDefault() == 0) ? true : false)
			{
				MethodSymbol method = op2.Method;
				if ((object)method != null && method.GetMemberArityIncludingExtension() > 0)
				{
					return BetterResult.Left;
				}
			}
			else
			{
				num = op2.Method?.GetMemberArityIncludingExtension();
				if ((!num.HasValue || num.GetValueOrDefault() == 0) ? true : false)
				{
					return BetterResult.Right;
				}
			}
			bool flag = op1.Kind.IsLifted();
			bool flag2 = op2.Kind.IsLifted();
			if (flag && !flag2)
			{
				return BetterResult.Right;
			}
			if (!flag & flag2)
			{
				return BetterResult.Left;
			}
		}
		if (op1.RefKind == RefKind.None && op2.RefKind == RefKind.In)
		{
			return BetterResult.Left;
		}
		if (op2.RefKind == RefKind.None && op1.RefKind == RefKind.In)
		{
			return BetterResult.Right;
		}
		return BetterResult.Neither;
	}

	private void GetAllBuiltInOperators(UnaryOperatorKind kind, bool isChecked, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		Compilation.BuiltInOperators.GetSimpleBuiltInOperators(kind, instance, !operand.Type.IsNativeIntegerOrNullableThereof());
		GetEnumOperations(kind, operand, instance);
		UnaryOperatorSignature? pointerOperation = GetPointerOperation(kind, operand);
		if (pointerOperation.HasValue)
		{
			instance.Add(pointerOperation.Value);
		}
		CandidateOperators(isChecked, instance, operand, results, ref useSiteInfo);
		instance.Free();
	}

	private bool CandidateOperators(bool isChecked, ArrayBuilder<UnaryOperatorSignature> operators, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool result = false;
		foreach (UnaryOperatorSignature @operator in operators)
		{
			Conversion conversion = Conversions.ClassifyConversionFromExpression(operand, @operator.OperandType, isChecked, ref useSiteInfo);
			if (conversion.IsImplicit)
			{
				result = true;
				results.Add(UnaryOperatorAnalysisResult.Applicable(@operator, conversion));
			}
			else
			{
				results.Add(UnaryOperatorAnalysisResult.Inapplicable(@operator, conversion));
			}
		}
		return result;
	}

	private void GetEnumOperations(UnaryOperatorKind kind, BoundExpression operand, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		TypeSymbol type = operand.Type;
		if ((object)type == null)
		{
			return;
		}
		type = type.StrippedType();
		if (type.IsValidEnumType())
		{
			NamedTypeSymbol orCreateNullableType = Compilation.GetOrCreateNullableType(type);
			switch (kind)
			{
			case UnaryOperatorKind.PostfixIncrement:
			case UnaryOperatorKind.PostfixDecrement:
			case UnaryOperatorKind.PrefixIncrement:
			case UnaryOperatorKind.PrefixDecrement:
			case UnaryOperatorKind.BitwiseComplement:
				operators.Add(new UnaryOperatorSignature(kind | UnaryOperatorKind.Enum, type, type));
				operators.Add(new UnaryOperatorSignature(kind | UnaryOperatorKind.Lifted | UnaryOperatorKind.Enum, orCreateNullableType, orCreateNullableType));
				break;
			}
		}
	}

	private static UnaryOperatorSignature? GetPointerOperation(UnaryOperatorKind kind, BoundExpression operand)
	{
		if (!(operand.Type is PointerTypeSymbol pointerTypeSymbol))
		{
			return null;
		}
		UnaryOperatorSignature? result = null;
		switch (kind)
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixIncrement:
		case UnaryOperatorKind.PrefixDecrement:
			result = new UnaryOperatorSignature(kind | UnaryOperatorKind.Pointer, pointerTypeSymbol, pointerTypeSymbol);
			break;
		}
		return result;
	}

	private bool GetUserDefinedOperators(UnaryOperatorKind kind, bool isChecked, string name1, string name2Opt, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)operand.Type == null)
		{
			return false;
		}
		return GetUserDefinedOperators(operand.Type.StrippedType(), kind, isChecked, name1, name2Opt, operand, results, ref useSiteInfo);
	}

	internal bool GetUserDefinedOperators(TypeSymbol declaringTypeOrTypeParameter, UnaryOperatorKind kind, bool isChecked, string name1, string name2Opt, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeSymbol constrainedToTypeOpt = declaringTypeOrTypeParameter as TypeParameterSymbol;
		if (OperatorFacts.DefinitelyHasNoUserDefinedOperators(declaringTypeOrTypeParameter))
		{
			return false;
		}
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		bool flag = false;
		NamedTypeSymbol namedTypeSymbol = declaringTypeOrTypeParameter as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = declaringTypeOrTypeParameter.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		if ((object)namedTypeSymbol == null && declaringTypeOrTypeParameter.IsTypeParameter())
		{
			namedTypeSymbol = ((TypeParameterSymbol)declaringTypeOrTypeParameter).EffectiveBaseClass(ref useSiteInfo);
		}
		while ((object)namedTypeSymbol != null)
		{
			instance.Clear();
			GetUserDefinedUnaryOperatorsFromType(constrainedToTypeOpt, namedTypeSymbol, kind, name1, name2Opt, instance);
			results.Clear();
			if (CandidateOperators(isChecked, instance, operand, results, ref useSiteInfo))
			{
				flag = true;
				break;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		if (!flag)
		{
			ImmutableArray<NamedTypeSymbol> immutableArray = default(ImmutableArray<NamedTypeSymbol>);
			if (declaringTypeOrTypeParameter.IsInterfaceType())
			{
				immutableArray = declaringTypeOrTypeParameter.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
			}
			else if (declaringTypeOrTypeParameter.IsTypeParameter())
			{
				immutableArray = ((TypeParameterSymbol)declaringTypeOrTypeParameter).AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
			}
			if (!immutableArray.IsDefaultOrEmpty)
			{
				PooledHashSet<NamedTypeSymbol> instance2 = PooledHashSet<NamedTypeSymbol>.GetInstance();
				ArrayBuilder<UnaryOperatorAnalysisResult> instance3 = ArrayBuilder<UnaryOperatorAnalysisResult>.GetInstance();
				results.Clear();
				foreach (NamedTypeSymbol item in immutableArray)
				{
					if (item.IsInterface && !instance2.Contains(item))
					{
						instance.Clear();
						instance3.Clear();
						GetUserDefinedUnaryOperatorsFromType(constrainedToTypeOpt, item, kind, name1, name2Opt, instance);
						if (CandidateOperators(isChecked, instance, operand, instance3, ref useSiteInfo))
						{
							flag = true;
							results.AddRange(instance3);
							instance2.AddAll(item.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo));
						}
					}
				}
				instance2.Free();
				instance3.Free();
			}
		}
		instance.Free();
		return flag;
	}

	internal static void GetStaticUserDefinedUnaryOperatorMethodNames(UnaryOperatorKind kind, bool isChecked, out string name1, out string? name2Opt)
	{
		name1 = OperatorFacts.UnaryOperatorNameFromOperatorKind(kind, isChecked);
		if (isChecked && SyntaxFacts.IsCheckedOperator(name1))
		{
			name2Opt = OperatorFacts.UnaryOperatorNameFromOperatorKind(kind, isChecked: false);
		}
		else
		{
			name2Opt = null;
		}
	}

	private void GetUserDefinedUnaryOperatorsFromType(TypeSymbol constrainedToTypeOpt, NamedTypeSymbol type, UnaryOperatorKind kind, string name1, string? name2Opt, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		GetDeclaredUserDefinedUnaryOperators(constrainedToTypeOpt, type, kind, name1, operators);
		if (name2Opt != null)
		{
			ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
			GetDeclaredUserDefinedUnaryOperators(constrainedToTypeOpt, type, kind, name2Opt, instance);
			if (operators.Count != 0)
			{
				for (int num = instance.Count - 1; num >= 0; num--)
				{
					foreach (UnaryOperatorSignature @operator in operators)
					{
						if (SourceMemberContainerTypeSymbol.DoOperatorsPair(@operator.Method, instance[num].Method))
						{
							instance.RemoveAt(num);
							break;
						}
					}
				}
			}
			operators.AddRange(instance);
			instance.Free();
		}
		AddLiftedUserDefinedUnaryOperators(constrainedToTypeOpt, kind, operators);
	}

	private static void GetDeclaredUserDefinedUnaryOperators(TypeSymbol? constrainedToTypeOpt, NamedTypeSymbol type, UnaryOperatorKind kind, string name, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		type.AddOperators(name, instance);
		GetDeclaredUserDefinedUnaryOperators(constrainedToTypeOpt, instance, kind, name, operators);
		instance.Free();
	}

	private static void GetDeclaredUserDefinedUnaryOperators(TypeSymbol? constrainedToTypeOpt, IEnumerable<MethodSymbol> typeOperators, UnaryOperatorKind kind, string name, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		foreach (MethodSymbol typeOperator in typeOperators)
		{
			if (!(typeOperator.Name != name) && typeOperator.IsStatic && typeOperator.ParameterCount == 1 && !typeOperator.ReturnsVoid)
			{
				TypeSymbol parameterType = typeOperator.GetParameterType(0);
				TypeSymbol returnType = typeOperator.ReturnType;
				operators.Add(new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | kind, parameterType, returnType, typeOperator, constrainedToTypeOpt));
			}
		}
	}

	private void AddLiftedUserDefinedUnaryOperators(TypeSymbol? constrainedToTypeOpt, UnaryOperatorKind kind, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixIncrement:
		case UnaryOperatorKind.PrefixDecrement:
		case UnaryOperatorKind.UnaryPlus:
		case UnaryOperatorKind.UnaryMinus:
		case UnaryOperatorKind.LogicalNegation:
		case UnaryOperatorKind.BitwiseComplement:
		{
			for (int num = operators.Count - 1; num >= 0; num--)
			{
				MethodSymbol method = operators[num].Method;
				TypeSymbol parameterType = method.GetParameterType(0);
				TypeSymbol returnType = method.ReturnType;
				if (parameterType.IsValidNullableTypeArgument() && returnType.IsValidNullableTypeArgument())
				{
					operators.Add(new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | UnaryOperatorKind.Lifted | kind, MakeNullable(parameterType), MakeNullable(returnType), method, constrainedToTypeOpt));
				}
			}
			break;
		}
		}
	}

	public OverloadResolution(Binder binder)
	{
		_binder = binder;
	}

	private static bool AnyValidResult<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.IsValid)
			{
				return true;
			}
		}
		return false;
	}

	private static bool SingleValidResult<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		bool flag = false;
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.IsValid)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
			}
		}
		return flag;
	}

	public void ObjectCreationOverloadResolution(ImmutableArray<MethodSymbol> constructors, AnalyzedArguments arguments, OverloadResolutionResult<MethodSymbol> result, bool dynamicResolution, bool isEarlyAttributeBinding, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<MemberResolutionResult<MethodSymbol>> resultsBuilder = result.ResultsBuilder;
		PerformObjectCreationOverloadResolution(resultsBuilder, constructors, arguments, completeResults: false, dynamicResolution, isEarlyAttributeBinding, ref useSiteInfo);
		if (!OverloadResolutionResultIsValid(resultsBuilder, arguments.HasDynamicArgument))
		{
			result.Clear();
			PerformObjectCreationOverloadResolution(resultsBuilder, constructors, arguments, completeResults: true, dynamicResolution, isEarlyAttributeBinding, ref useSiteInfo);
		}
	}

	public void MethodInvocationOverloadResolution(ArrayBuilder<MethodSymbol> methods, ArrayBuilder<TypeWithAnnotations> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, OverloadResolutionResult<MethodSymbol> result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Options options, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null, in CallingConventionInfo callingConventionInfo = default(CallingConventionInfo))
	{
		MethodOrPropertyOverloadResolution(methods, typeArguments, receiver, arguments, result, ref useSiteInfo, options, returnRefKind, returnType, in callingConventionInfo);
	}

	public void PropertyOverloadResolution(ArrayBuilder<PropertySymbol> indexers, BoundExpression receiverOpt, AnalyzedArguments arguments, OverloadResolutionResult<PropertySymbol> result, bool allowRefOmittedArguments, bool dynamicResolution, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		MethodOrPropertyOverloadResolution(indexers, instance, receiverOpt, arguments, result, ref useSiteInfo, (Options)((allowRefOmittedArguments ? 2 : 0) | (dynamicResolution ? 64 : 0)), RefKind.None, null, default(CallingConventionInfo));
		instance.Free();
	}

	internal void MethodOrPropertyOverloadResolution<TMember>(ArrayBuilder<TMember> members, ArrayBuilder<TypeWithAnnotations> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, OverloadResolutionResult<TMember> result, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Options options, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null, in CallingConventionInfo callingConventionInfo = default(CallingConventionInfo)) where TMember : Symbol
	{
		ArrayBuilder<MemberResolutionResult<TMember>> resultsBuilder = result.ResultsBuilder;
		bool checkOverriddenOrHidden = (options & Options.IsExtensionMethodResolution) == 0 || !members.All((TMember m) => m.ContainingSymbol is NamedTypeSymbol { BaseTypeNoUseSiteDiagnostics: { } baseTypeNoUseSiteDiagnostics } && baseTypeNoUseSiteDiagnostics.SpecialType == SpecialType.System_Object);
		PerformMemberOverloadResolution(resultsBuilder, members, typeArguments, receiver, arguments, completeResults: false, returnRefKind, returnType, in callingConventionInfo, ref useSiteInfo, options, checkOverriddenOrHidden);
		if (!OverloadResolutionResultIsValid(resultsBuilder, arguments.HasDynamicArgument))
		{
			result.Clear();
			PerformMemberOverloadResolution(resultsBuilder, members, typeArguments, receiver, arguments, completeResults: true, returnRefKind, returnType, in callingConventionInfo, ref useSiteInfo, options, checkOverriddenOrHidden);
		}
	}

	internal bool FilterMethodsForUniqueSignature(ArrayBuilder<MethodSymbol> methods, out bool useParams)
	{
		useParams = false;
		if (methods.Count == 0)
		{
			return true;
		}
		OverloadResolutionResult<MethodSymbol> instance = OverloadResolutionResult<MethodSymbol>.GetInstance();
		ArrayBuilder<MemberResolutionResult<MethodSymbol>> resultsBuilder = instance.ResultsBuilder;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		ArrayBuilder<TypeWithAnnotations> instance2 = ArrayBuilder<TypeWithAnnotations>.GetInstance(0);
		AnalyzedArguments instance3 = AnalyzedArguments.GetInstance();
		ArrayBuilder<MethodSymbol> arrayBuilder = methods;
		if (methods.Any((MethodSymbol m) => (object)m.ReducedFrom != null))
		{
			arrayBuilder = ArrayBuilder<MethodSymbol>.GetInstance(methods.Count);
			foreach (MethodSymbol method in methods)
			{
				arrayBuilder.Add(method.ReducedFrom ?? method);
			}
		}
		PerformMemberOverloadResolutionStart(resultsBuilder, arrayBuilder, instance2, instance3, completeResults: false, ref useSiteInfo, Options.IgnoreNormalFormIfHasValidParamsParameter | Options.InferringUniqueMethodGroupSignature, checkOverriddenOrHidden: true);
		instance3.Free();
		instance2.Free();
		bool flag = resultsBuilder.Any((MemberResolutionResult<MethodSymbol> r) => r.Resolution == MemberResolutionKind.ApplicableInExpandedForm);
		if (flag && resultsBuilder.Any((MemberResolutionResult<MethodSymbol> r) => r.Resolution == MemberResolutionKind.ApplicableInNormalForm))
		{
			if (arrayBuilder != methods)
			{
				arrayBuilder.Free();
			}
			instance.Free();
			return false;
		}
		if (arrayBuilder == methods)
		{
			ImmutableArray<MethodSymbol> allApplicableMembers = instance.GetAllApplicableMembers();
			if (allApplicableMembers.Length != methods.Count)
			{
				methods.Clear();
				methods.AddRange(allApplicableMembers);
			}
		}
		else
		{
			ArrayBuilder<MethodSymbol> instance4 = ArrayBuilder<MethodSymbol>.GetInstance(methods.Count);
			foreach (MemberResolutionResult<MethodSymbol> item in resultsBuilder)
			{
				if (item.Result.IsApplicable)
				{
					int index = arrayBuilder.IndexOf(item.Member);
					instance4.Add(methods[index]);
				}
			}
			if (instance4.Count != methods.Count)
			{
				methods.Clear();
				methods.AddRange(instance4);
			}
			instance4.Free();
			arrayBuilder.Free();
		}
		instance.Free();
		useParams = flag;
		return true;
	}

	private static bool OverloadResolutionResultIsValid<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, bool hasDynamicArgument) where TMember : Symbol
	{
		if (hasDynamicArgument)
		{
			foreach (MemberResolutionResult<TMember> result in results)
			{
				if (result.Result.IsApplicable)
				{
					return true;
				}
			}
			return false;
		}
		return SingleValidResult(results);
	}

	private void PerformMemberOverloadResolutionStart<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, ArrayBuilder<TypeWithAnnotations> typeArguments, AnalyzedArguments arguments, bool completeResults, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Options options, bool checkOverriddenOrHidden) where TMember : Symbol
	{
		Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt = null;
		if (checkOverriddenOrHidden && members.Count > 50)
		{
			containingTypeMapOpt = PartitionMembersByContainingType(members);
		}
		for (int i = 0; i < members.Count; i++)
		{
			AddMemberToCandidateSet(members[i], results, members, typeArguments, arguments, completeResults, containingTypeMapOpt, ref useSiteInfo, options, checkOverriddenOrHidden);
		}
		ClearContainingTypeMap(ref containingTypeMapOpt);
		RemoveInaccessibleTypeArguments(results, ref useSiteInfo);
		if (checkOverriddenOrHidden)
		{
			if ((options & Options.DynamicResolution) != Options.None || (options & Options.InferringUniqueMethodGroupSignature) != Options.None)
			{
				RemoveHiddenMembers(results);
			}
			else
			{
				RemoveLessDerivedMembers(results, ref useSiteInfo);
			}
		}
	}

	private void PerformMemberOverloadResolution<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, ArrayBuilder<TypeWithAnnotations> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, bool completeResults, RefKind returnRefKind, TypeSymbol returnType, in CallingConventionInfo callingConventionInfo, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Options options, bool checkOverriddenOrHidden) where TMember : Symbol
	{
		PerformMemberOverloadResolutionStart(results, members, typeArguments, arguments, completeResults, ref useSiteInfo, options, checkOverriddenOrHidden);
		if (Compilation.LanguageVersion.AllowImprovedOverloadCandidates())
		{
			RemoveStaticInstanceMismatches(results, receiver);
			RemoveConstraintViolations(results, new CompoundUseSiteInfo<AssemblySymbol>(useSiteInfo));
			if ((options & Options.IsMethodGroupConversion) != Options.None)
			{
				RemoveDelegateConversionsWithWrongReturnType(results, ref useSiteInfo, returnRefKind, returnType, (options & Options.IsFunctionPointerResolution) != 0);
			}
		}
		if ((options & Options.IsFunctionPointerResolution) != Options.None)
		{
			RemoveCallingConventionMismatches(results, in callingConventionInfo);
			RemoveMethodsNotDeclaredStatic(results);
		}
		ReportUseSiteInfo(results, ref useSiteInfo);
		if (AnyValidResult(results) && (options & Options.DynamicResolution) == 0)
		{
			RemoveLowerPriorityMembers<MemberResolutionResult<TMember>, TMember>(results);
			RemoveWorseMembers(results, arguments, ref useSiteInfo);
		}
	}

	private static void RemoveHiddenMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		PooledHashSet<Symbol> pooledHashSet = null;
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (!memberResolutionResult.Result.IsValid)
			{
				continue;
			}
			foreach (Symbol hiddenMember in getHiddenMembers(memberResolutionResult.LeastOverriddenMember.ConstructedFrom()))
			{
				if (pooledHashSet == null)
				{
					pooledHashSet = s_HiddenSymbolsSetPool.Allocate();
				}
				pooledHashSet.Add(hiddenMember);
			}
		}
		if (pooledHashSet != null)
		{
			for (int j = 0; j < results.Count; j++)
			{
				MemberResolutionResult<TMember> memberResolutionResult2 = results[j];
				if (memberResolutionResult2.Result.IsValid && pooledHashSet.Contains(memberResolutionResult2.Member.ConstructedFrom()))
				{
					results[j] = memberResolutionResult2.WithResult(MemberAnalysisResult.LessDerived());
				}
			}
		}
		pooledHashSet?.Free();
		static ImmutableArray<Symbol> getHiddenMembers(Symbol member)
		{
			if (member is MethodSymbol methodSymbol)
			{
				return methodSymbol.OverriddenOrHiddenMembers.HiddenMembers;
			}
			if (member is PropertySymbol propertySymbol)
			{
				return propertySymbol.OverriddenOrHiddenMembers.HiddenMembers;
			}
			if (member is EventSymbol eventSymbol)
			{
				return eventSymbol.OverriddenOrHiddenMembers.HiddenMembers;
			}
			return ImmutableArray<Symbol>.Empty;
		}
	}

	internal void FunctionPointerOverloadResolution(ArrayBuilder<FunctionPointerMethodSymbol> funcPtrBuilder, AnalyzedArguments analyzedArguments, OverloadResolutionResult<FunctionPointerMethodSymbol> overloadResolutionResult, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		AddMemberToCandidateSet(funcPtrBuilder[0], overloadResolutionResult.ResultsBuilder, funcPtrBuilder, instance, analyzedArguments, completeResults: true, null, ref useSiteInfo, Options.None);
		ReportUseSiteInfo(overloadResolutionResult.ResultsBuilder, ref useSiteInfo);
	}

	private void RemoveStaticInstanceMismatches<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, BoundExpression receiverOpt) where TMember : Symbol
	{
		if (!Binder.IsTypeOrValueExpression(receiverOpt))
		{
			bool flag = Binder.WasImplicitReceiver(receiverOpt);
			bool flag2 = !_binder.HasThis(!flag, out var inStaticContext) | inStaticContext;
			if (!flag || flag2)
			{
				bool requireStatic = (flag & flag2) || Binder.IsMemberAccessedThroughType(receiverOpt);
				RemoveStaticInstanceMismatches(results, requireStatic);
			}
		}
	}

	private static void RemoveStaticInstanceMismatches<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, bool requireStatic) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			TMember member = memberResolutionResult.Member;
			if (!(member is MethodSymbol { IsExtensionMethod: not false }) && memberResolutionResult.Result.IsValid && member.RequiresInstanceReceiver() == requireStatic)
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.StaticInstanceMismatch());
			}
		}
	}

	private static void RemoveMethodsNotDeclaredStatic<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			TMember member = memberResolutionResult.Member;
			if (memberResolutionResult.Result.IsValid && !member.IsStatic)
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.StaticInstanceMismatch());
			}
		}
	}

	private void RemoveConstraintViolations<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, CompoundUseSiteInfo<AssemblySymbol> template) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			TMember member = memberResolutionResult.Member;
			if ((memberResolutionResult.Result.IsValid || memberResolutionResult.Result.Kind == MemberResolutionKind.ConstructedParameterFailedConstraintCheck) && FailsConstraintChecks(member, out var constraintFailureDiagnosticsOpt, template))
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.ConstraintFailure(constraintFailureDiagnosticsOpt.ToImmutableAndFree()));
			}
		}
	}

	private void RemoveCallingConventionMismatches<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, in CallingConventionInfo expectedConvention) where TMember : Symbol
	{
		if (typeof(TMember) != typeof(MethodSymbol) || _binder.InAttributeArgument || (_binder.Flags & BinderFlags.InContextualAttributeBinder) != BinderFlags.None)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> result = results[i];
			MethodSymbol methodSymbol = (MethodSymbol)(object)result.Member;
			if (!result.Result.IsValid)
			{
				continue;
			}
			UnmanagedCallersOnlyAttributeData unmanagedCallersOnlyAttributeData = methodSymbol.GetUnmanagedCallersOnlyAttributeData(forceComplete: true);
			Microsoft.Cci.CallingConvention callingConvention;
			ImmutableHashSet<INamedTypeSymbolInternal> immutableHashSet;
			ImmutableHashSet<INamedTypeSymbolInternal> callingConventionTypes;
			if (unmanagedCallersOnlyAttributeData == null)
			{
				callingConvention = methodSymbol.CallingConvention;
				immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
			}
			else
			{
				callingConventionTypes = unmanagedCallersOnlyAttributeData.CallingConventionTypes;
				int count = callingConventionTypes.Count;
				if (count != 0)
				{
					if (count != 1)
					{
						goto IL_013b;
					}
					switch (callingConventionTypes.Single().Name)
					{
					case "CallConvCdecl":
						break;
					case "CallConvStdcall":
						goto IL_0117;
					case "CallConvThiscall":
						goto IL_0123;
					case "CallConvFastcall":
						goto IL_012f;
					default:
						goto IL_013b;
					}
					callingConvention = Microsoft.Cci.CallingConvention.CDecl;
					immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
				}
				else
				{
					callingConvention = Microsoft.Cci.CallingConvention.Unmanaged;
					immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
				}
			}
			goto IL_0143;
			IL_013b:
			callingConvention = Microsoft.Cci.CallingConvention.Unmanaged;
			immutableHashSet = callingConventionTypes;
			goto IL_0143;
			IL_012f:
			callingConvention = Microsoft.Cci.CallingConvention.FastCall;
			immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
			goto IL_0143;
			IL_0123:
			callingConvention = Microsoft.Cci.CallingConvention.ThisCall;
			immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
			goto IL_0143;
			IL_0117:
			callingConvention = Microsoft.Cci.CallingConvention.Standard;
			immutableHashSet = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
			goto IL_0143;
			IL_0143:
			if (callingConvention.HasUnknownCallingConventionAttributeBits() || !callingConvention.IsCallingConvention(expectedConvention.CallKind))
			{
				results[i] = makeWrongCallingConvention(result);
			}
			else
			{
				if (!expectedConvention.CallKind.IsCallingConvention(Microsoft.Cci.CallingConvention.Unmanaged))
				{
					continue;
				}
				if (expectedConvention.UnmanagedCallingConventionTypes.Count != immutableHashSet.Count)
				{
					results[i] = makeWrongCallingConvention(result);
					continue;
				}
				foreach (CustomModifier unmanagedCallingConventionType in expectedConvention.UnmanagedCallingConventionTypes)
				{
					if (!immutableHashSet.Contains(((CSharpCustomModifier)unmanagedCallingConventionType).ModifierSymbol))
					{
						results[i] = makeWrongCallingConvention(result);
						break;
					}
				}
			}
		}
		static MemberResolutionResult<TMember> makeWrongCallingConvention(MemberResolutionResult<TMember> memberResolutionResult)
		{
			return memberResolutionResult.WithResult(MemberAnalysisResult.WrongCallingConvention());
		}
	}

	private bool FailsConstraintChecks<TMember>(TMember member, out ArrayBuilder<TypeParameterDiagnosticInfo> constraintFailureDiagnosticsOpt, CompoundUseSiteInfo<AssemblySymbol> template) where TMember : Symbol
	{
		if (member.GetMemberArityIncludingExtension() == 0 || (object)member.OriginalDefinition == member)
		{
			constraintFailureDiagnosticsOpt = null;
			return false;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		ConstraintsHelper.CheckConstraintsArgs args = new ConstraintsHelper.CheckConstraintsArgs(Compilation, Conversions, includeNullability: false, NoLocation.Singleton, null, template);
		bool flag = true;
		if (member is MethodSymbol method)
		{
			flag = ConstraintsHelper.CheckMethodConstraints(method, in args, instance, null, ref useSiteDiagnosticsBuilder);
		}
		else if (member.IsExtensionBlockMember())
		{
			NamedTypeSymbol containingType = member.ContainingType;
			if ((object)containingType != null && ConstraintsHelper.RequiresChecking(containingType))
			{
				flag = containingType.CheckConstraints(in args, containingType.TypeSubstitution, containingType.TypeParameters, containingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, instance, null, ref useSiteDiagnosticsBuilder);
			}
		}
		if (!flag)
		{
			if (useSiteDiagnosticsBuilder != null)
			{
				instance.AddRange(useSiteDiagnosticsBuilder);
				useSiteDiagnosticsBuilder.Free();
			}
			constraintFailureDiagnosticsOpt = instance;
			return true;
		}
		instance.Free();
		useSiteDiagnosticsBuilder?.Free();
		constraintFailureDiagnosticsOpt = null;
		return false;
	}

	private void RemoveDelegateConversionsWithWrongReturnType<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, RefKind? returnRefKind, TypeSymbol returnType, bool isFunctionPointerConversion) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (!memberResolutionResult.Result.IsValid)
			{
				continue;
			}
			MethodSymbol methodSymbol = (MethodSymbol)(object)memberResolutionResult.Member;
			bool flag;
			if ((object)returnType == null || methodSymbol.ReturnType.Equals(returnType, TypeCompareKind.AllIgnoreOptions))
			{
				flag = true;
			}
			else if (returnRefKind == RefKind.None)
			{
				flag = Conversions.HasIdentityOrImplicitReferenceConversion(methodSymbol.ReturnType, returnType, ref useSiteInfo);
				if (!flag & isFunctionPointerConversion)
				{
					flag = ConversionsBase.HasImplicitPointerToVoidConversion(methodSymbol.ReturnType, returnType) || Conversions.HasImplicitPointerConversion(methodSymbol.ReturnType, returnType, ref useSiteInfo);
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.WrongReturnType());
			}
			else if (methodSymbol.RefKind != returnRefKind)
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.WrongRefKind());
			}
		}
	}

	private static Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> PartitionMembersByContainingType<TMember>(ArrayBuilder<TMember> members) where TMember : Symbol
	{
		Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> dictionary = new Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>>();
		for (int i = 0; i < members.Count; i++)
		{
			TMember val = members[i];
			NamedTypeSymbol containingType = val.ContainingType;
			if (!dictionary.TryGetValue(containingType, out var value))
			{
				value = (dictionary[containingType] = ArrayBuilder<TMember>.GetInstance());
			}
			value.Add(val);
		}
		return dictionary;
	}

	private static void ClearContainingTypeMap<TMember>(ref Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt) where TMember : Symbol
	{
		if (containingTypeMapOpt == null)
		{
			return;
		}
		foreach (ArrayBuilder<TMember> value in containingTypeMapOpt.Values)
		{
			value.Free();
		}
		containingTypeMapOpt = null;
	}

	private void AddConstructorToCandidateSet(MethodSymbol constructor, ArrayBuilder<MemberResolutionResult<MethodSymbol>> results, AnalyzedArguments arguments, bool completeResults, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (constructor.HasUnsupportedMetadata)
		{
			if (completeResults)
			{
				results.Add(new MemberResolutionResult<MethodSymbol>(constructor, constructor, MemberAnalysisResult.UnsupportedMetadata(), hasTypeArgumentInferredFromFunctionType: false));
			}
			return;
		}
		MemberAnalysisResult memberAnalysisResult = IsConstructorApplicableInNormalForm(constructor, arguments, completeResults, ref useSiteInfo);
		MemberAnalysisResult result = memberAnalysisResult;
		if (!memberAnalysisResult.IsValid && IsValidParams(_binder, constructor, disallowExpandedNonArrayParams: false, out var definitionElementType))
		{
			MemberAnalysisResult memberAnalysisResult2 = IsConstructorApplicableInExpandedForm(constructor, arguments, definitionElementType, completeResults, ref useSiteInfo);
			if (memberAnalysisResult2.IsValid | completeResults)
			{
				result = memberAnalysisResult2;
			}
		}
		if ((result.IsValid | completeResults) || result.HasUseSiteDiagnosticToReportFor(constructor))
		{
			results.Add(new MemberResolutionResult<MethodSymbol>(constructor, constructor, result, hasTypeArgumentInferredFromFunctionType: false));
		}
	}

	private MemberAnalysisResult IsConstructorApplicableInNormalForm(MethodSymbol constructor, AnalyzedArguments arguments, bool completeResults, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(constructor, arguments, isMethodGroupConversion: false, expanded: false);
		if (!argAnalysis.IsValid)
		{
			return MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis);
		}
		if (constructor.HasUseSiteError)
		{
			return MemberAnalysisResult.UseSiteError();
		}
		EffectiveParameters effectiveParametersInNormalForm = GetEffectiveParametersInNormalForm(constructor, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, Options.None, _binder, out var _);
		return IsApplicable(constructor, effectiveParametersInNormalForm, default(TypeWithAnnotations), isExpanded: false, arguments, argAnalysis.ArgsToParamsOpt, constructor.IsVararg, hasAnyRefOmittedArgument: false, ignoreOpenTypes: false, completeResults, dynamicConvertsToAnything: false, isMethodGroupConversion: false, ref useSiteInfo);
	}

	private MemberAnalysisResult IsConstructorApplicableInExpandedForm(MethodSymbol constructor, AnalyzedArguments arguments, TypeWithAnnotations definitionParamsElementType, bool completeResults, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(constructor, arguments, isMethodGroupConversion: false, expanded: true);
		if (!argAnalysis.IsValid)
		{
			return MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis);
		}
		if (constructor.HasUseSiteError)
		{
			return MemberAnalysisResult.UseSiteError();
		}
		EffectiveParameters effectiveParametersInExpandedForm = GetEffectiveParametersInExpandedForm(constructor, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, Options.None);
		return IsApplicable(constructor, effectiveParametersInExpandedForm, definitionParamsElementType, isExpanded: true, arguments, argAnalysis.ArgsToParamsOpt, isVararg: false, hasAnyRefOmittedArgument: false, ignoreOpenTypes: false, completeResults, dynamicConvertsToAnything: false, isMethodGroupConversion: false, ref useSiteInfo);
	}

	private void AddMemberToCandidateSet<TMember>(TMember member, ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, ArrayBuilder<TypeWithAnnotations> typeArguments, AnalyzedArguments arguments, bool completeResults, Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, Options options, bool checkOverriddenOrHidden = true) where TMember : Symbol
	{
		if (checkOverriddenOrHidden && members.Count >= 2)
		{
			if (containingTypeMapOpt == null)
			{
				if (MemberGroupContainsMoreDerivedOverride(members, member, checkOverrideContainingType: true, ref useSiteInfo) || MemberGroupHidesByName(members, member, ref useSiteInfo))
				{
					return;
				}
			}
			else if (containingTypeMapOpt.Count != 1)
			{
				NamedTypeSymbol containingType = member.ContainingType;
				foreach (KeyValuePair<NamedTypeSymbol, ArrayBuilder<TMember>> item2 in containingTypeMapOpt)
				{
					if (item2.Key.IsDerivedFrom(containingType, TypeCompareKind.ConsiderEverything, ref useSiteInfo))
					{
						ArrayBuilder<TMember> value = item2.Value;
						if (MemberGroupContainsMoreDerivedOverride(value, member, checkOverrideContainingType: false, ref useSiteInfo) || MemberGroupHidesByName(value, member, ref useSiteInfo))
						{
							return;
						}
					}
				}
			}
		}
		TMember val = (TMember)member.GetLeastOverriddenMember(_binder.ContainingType);
		if ((options & Options.InferringUniqueMethodGroupSignature) == 0 && member.HasUnsupportedMetadata)
		{
			if (completeResults)
			{
				results.Add(new MemberResolutionResult<TMember>(member, val, MemberAnalysisResult.UnsupportedMetadata(), hasTypeArgumentInferredFromFunctionType: false));
			}
			return;
		}
		bool disallowExpandedNonArrayParams = (options & Options.DisallowExpandedNonArrayParams) != 0;
		bool flag = (options & Options.IgnoreNormalFormIfHasValidParamsParameter) != Options.None && IsValidParams(_binder, val, disallowExpandedNonArrayParams, out var _);
		MemberResolutionResult<TMember> memberResolutionResult = (flag ? default(MemberResolutionResult<TMember>) : IsMemberApplicableInNormalForm(member, val, typeArguments, arguments, options, completeResults, ref useSiteInfo));
		MemberResolutionResult<TMember> item = memberResolutionResult;
		if (!memberResolutionResult.Result.IsValid && (options & (Options.IsMethodGroupConversion | Options.DisallowExpandedForm)) == 0 && IsValidParams(_binder, val, disallowExpandedNonArrayParams, out var definitionElementType2))
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = IsMemberApplicableInExpandedForm(member, val, typeArguments, arguments, definitionElementType2, options, completeResults, (options & Options.DynamicConvertsToAnything) != 0, (options & Options.IsMethodGroupConversion) != 0, ref useSiteInfo);
			if (flag || PreferExpandedFormOverNormalForm(memberResolutionResult, memberResolutionResult2))
			{
				item = memberResolutionResult2;
			}
		}
		if ((item.Result.IsValid | completeResults) || item.HasUseSiteDiagnosticToReport)
		{
			results.Add(item);
		}
		else
		{
			item.Member.AddUseSiteInfo(ref useSiteInfo, addDiagnostics: false);
		}
	}

	private static bool PreferExpandedFormOverNormalForm<TMember>(MemberResolutionResult<TMember> normalResult, MemberResolutionResult<TMember> expandedResult) where TMember : Symbol
	{
		if (expandedResult.IsValid)
		{
			return true;
		}
		switch (normalResult.Result.Kind)
		{
		case MemberResolutionKind.NoCorrespondingParameter:
		case MemberResolutionKind.RequiredParameterMissing:
			switch (expandedResult.Result.Kind)
			{
			case MemberResolutionKind.NoCorrespondingNamedParameter:
			case MemberResolutionKind.DuplicateNamedArgument:
			case MemberResolutionKind.NameUsedForPositional:
			case MemberResolutionKind.BadNonTrailingNamedArgument:
			case MemberResolutionKind.UseSiteError:
			case MemberResolutionKind.BadArgumentConversion:
			case MemberResolutionKind.TypeInferenceFailed:
			case MemberResolutionKind.TypeInferenceExtensionInstanceArgument:
			case MemberResolutionKind.ConstructedParameterFailedConstraintCheck:
				return true;
			}
			break;
		case MemberResolutionKind.BadArgumentConversion:
			if (expandedResult.Result.Kind == MemberResolutionKind.BadArgumentConversion && expandedResult.Result.ParamsElementTypeOpt.HasType && (object)expandedResult.Result.ParamsElementTypeOpt.Type != ErrorTypeSymbol.EmptyParamsCollectionElementTypeSentinel && haveBadArgumentForLastParameter(normalResult))
			{
				return true;
			}
			break;
		}
		return false;
		static bool haveBadArgumentForLastParameter(MemberResolutionResult<TMember> result)
		{
			int parameterCount = result.Member.GetParameterCount();
			foreach (int item in result.Result.BadArgumentsOpt.TrueBits())
			{
				if (parameterCount == result.Result.ParameterFromArgument(item) + 1)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsValidParams(Binder binder, Symbol member, bool disallowExpandedNonArrayParams, out TypeWithAnnotations definitionElementType)
	{
		if (member.GetIsVararg())
		{
			definitionElementType = default(TypeWithAnnotations);
			return false;
		}
		if (member.GetParameterCount() == 0)
		{
			definitionElementType = default(TypeWithAnnotations);
			return false;
		}
		ParameterSymbol parameterSymbol = member.GetParameters().Last();
		if ((parameterSymbol.IsParamsArray && parameterSymbol.Type.IsSZArray()) || (parameterSymbol.IsParamsCollection && !parameterSymbol.Type.IsSZArray() && !disallowExpandedNonArrayParams && (binder.Compilation.LanguageVersion > LanguageVersion.CSharp12 || member.ContainingModule == binder.Compilation.SourceModule)))
		{
			return TryInferParamsCollectionIterationType(binder, parameterSymbol.OriginalDefinition.Type, out definitionElementType);
		}
		definitionElementType = default(TypeWithAnnotations);
		return false;
	}

	public static bool TryInferParamsCollectionIterationType(Binder binder, TypeSymbol type, out TypeWithAnnotations elementType)
	{
		if (binder.Flags.HasFlag(BinderFlags.AttributeArgument) && !type.IsSZArray())
		{
			elementType = default(TypeWithAnnotations);
			return false;
		}
		CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(binder.Compilation, type, out elementType);
		switch (collectionExpressionTypeKind)
		{
		case CollectionExpressionTypeKind.None:
			return false;
		case CollectionExpressionTypeKind.CollectionBuilder:
		case CollectionExpressionTypeKind.ImplementsIEnumerable:
		{
			SyntaxNode root = CSharpSyntaxTree.Dummy.GetRoot();
			binder.TryGetCollectionIterationType(root, type, out elementType);
			if ((object)elementType.Type == null)
			{
				return false;
			}
			if (collectionExpressionTypeKind == CollectionExpressionTypeKind.ImplementsIEnumerable)
			{
				if (!binder.HasCollectionExpressionApplicableConstructor(root, type, out MethodSymbol _, out bool _, BindingDiagnosticBag.Discarded))
				{
					return false;
				}
				if (!binder.HasCollectionExpressionApplicableAddMethod(root, type, out ImmutableArray<MethodSymbol> _, BindingDiagnosticBag.Discarded))
				{
					return false;
				}
			}
			break;
		}
		}
		return true;
	}

	private static bool IsMoreDerivedOverride(Symbol member, Symbol moreDerivedOverride, bool checkOverrideContainingType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!moreDerivedOverride.IsOverride || (checkOverrideContainingType && !moreDerivedOverride.ContainingType.IsDerivedFrom(member.ContainingType, TypeCompareKind.ConsiderEverything, ref useSiteInfo)) || !MemberSignatureComparer.SloppyOverrideComparer.Equals(member, moreDerivedOverride))
		{
			return false;
		}
		return moreDerivedOverride.GetLeastOverriddenMember(null).OriginalDefinition == member.GetLeastOverriddenMember(null).OriginalDefinition;
	}

	private static bool MemberGroupContainsMoreDerivedOverride<TMember>(ArrayBuilder<TMember> members, TMember member, bool checkOverrideContainingType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		if (!member.IsVirtual && !member.IsAbstract && !member.IsOverride)
		{
			return false;
		}
		if (!member.ContainingType.IsClassType())
		{
			return false;
		}
		for (int i = 0; i < members.Count; i++)
		{
			if (IsMoreDerivedOverride(member, members[i], checkOverrideContainingType, ref useSiteInfo))
			{
				return true;
			}
		}
		return false;
	}

	private static bool MemberGroupHidesByName<TMember>(ArrayBuilder<TMember> members, TMember member, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		NamedTypeSymbol containingType = member.ContainingType;
		foreach (TMember member2 in members)
		{
			NamedTypeSymbol containingType2 = member2.ContainingType;
			if (HidesByName(member2) && containingType2.IsDerivedFrom(containingType, TypeCompareKind.ConsiderEverything, ref useSiteInfo))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HidesByName(Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).HidesBaseMethodsByName, 
			SymbolKind.Property => ((PropertySymbol)member).HidesBasePropertiesByName, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	private void RemoveInaccessibleTypeArguments<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid && (!typeArgumentsAccessible(memberResolutionResult.Member.GetMemberTypeArgumentsNoUseSiteDiagnostics(), ref useSiteInfo) || (memberResolutionResult.Member.IsExtensionBlockMember() && !typeArgumentsAccessible(memberResolutionResult.Member.ContainingType.GetMemberTypeArgumentsNoUseSiteDiagnostics(), ref useSiteInfo))))
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.InaccessibleTypeArgument());
			}
		}
		bool typeArgumentsAccessible(ImmutableArray<TypeSymbol> typeArguments, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			foreach (TypeSymbol item in typeArguments)
			{
				if (!_binder.IsAccessible(item, ref useSiteInfo2))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static void RemoveLessDerivedMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		RemoveAllInterfaceMembers(results);
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if ((memberResolutionResult.Result.IsValid || memberResolutionResult.HasUseSiteDiagnosticToReport) && IsLessDerivedThanAny(i, memberResolutionResult.LeastOverriddenMember.ContainingType, results, ref useSiteInfo))
			{
				results[i] = memberResolutionResult.WithResult(MemberAnalysisResult.LessDerived());
			}
		}
	}

	private static bool IsLessDerivedThanAny<TMember>(int index, TypeSymbol type, ArrayBuilder<MemberResolutionResult<TMember>> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			if (i == index)
			{
				continue;
			}
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid)
			{
				NamedTypeSymbol containingType = memberResolutionResult.LeastOverriddenMember.ContainingType;
				if (type.SpecialType == SpecialType.System_Object && containingType.SpecialType != SpecialType.System_Object)
				{
					return true;
				}
				if (containingType.IsInterfaceType() && type.IsInterfaceType() && containingType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo).Contains((NamedTypeSymbol)type))
				{
					return true;
				}
				if (containingType.IsClassType() && type.IsClassType() && containingType.IsDerivedFrom(type, TypeCompareKind.ConsiderEverything, ref useSiteInfo))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void RemoveAllInterfaceMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		bool flag = false;
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid)
			{
				NamedTypeSymbol containingType = memberResolutionResult.LeastOverriddenMember.ContainingType;
				if (containingType.IsClassType() && containingType.GetSpecialTypeSafe() != SpecialType.System_Object)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < results.Count; j++)
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = results[j];
			if (memberResolutionResult2.Result.IsValid && memberResolutionResult2.Member.ContainingType.IsInterfaceType())
			{
				results[j] = memberResolutionResult2.WithResult(MemberAnalysisResult.LessDerived());
			}
		}
	}

	private void PerformObjectCreationOverloadResolution(ArrayBuilder<MemberResolutionResult<MethodSymbol>> results, ImmutableArray<MethodSymbol> constructors, AnalyzedArguments arguments, bool completeResults, bool dynamicResolution, bool isEarlyAttributeBinding, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		foreach (MethodSymbol item in constructors)
		{
			AddConstructorToCandidateSet(item, results, arguments, completeResults, ref useSiteInfo);
		}
		ReportUseSiteInfo(results, ref useSiteInfo);
		if (!dynamicResolution)
		{
			if (!isEarlyAttributeBinding)
			{
				RemoveLowerPriorityMembers<MemberResolutionResult<MethodSymbol>, MethodSymbol>(results);
			}
			RemoveWorseMembers(results, arguments, ref useSiteInfo);
		}
	}

	private static void ReportUseSiteInfo<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		foreach (MemberResolutionResult<TMember> result in results)
		{
			result.Member.AddUseSiteInfo(ref useSiteInfo, result.HasUseSiteDiagnosticToReport);
		}
	}

	private int GetTheBestCandidateIndex<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		int num = -1;
		for (int i = 0; i < results.Count; i++)
		{
			if (!results[i].IsValid)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			if (results[num].Member == results[i].Member)
			{
				num = -1;
				continue;
			}
			switch (BetterFunctionMember(results[num], results[i], arguments.Arguments, ref useSiteInfo))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (results[j].IsValid)
			{
				if (results[num].Member == results[j].Member)
				{
					return -1;
				}
				if (BetterFunctionMember(results[num], results[j], arguments.Arguments, ref useSiteInfo) != BetterResult.Left)
				{
					return -1;
				}
			}
		}
		return num;
	}

	private void RemoveLowerPriorityMembers<TMemberResolution, TMember>(ArrayBuilder<TMemberResolution> results) where TMemberResolution : IMemberResolutionResultWithPriority<TMember> where TMember : Symbol
	{
		if (!Compilation.IsFeatureEnabled(MessageID.IDS_FeatureOverloadResolutionPriority) || results.Count < 2 || results.All(delegate(TMemberResolution r)
		{
			int? num = r.MemberWithPriority?.GetOverloadResolutionPriority();
			return (!num.HasValue || num.GetValueOrDefault() == 0) ? true : false;
		}))
		{
			return;
		}
		bool flag = false;
		PooledDictionary<NamedTypeSymbol, OneOrMany<TMemberResolution>> instance = PooledDictionary<NamedTypeSymbol, OneOrMany<TMemberResolution>>.GetInstance();
		ArrayBuilder<TMemberResolution> instance2 = ArrayBuilder<TMemberResolution>.GetInstance();
		foreach (TMemberResolution result in results)
		{
			TMember memberWithPriority = result.MemberWithPriority;
			if (!result.IsApplicable)
			{
				instance2.Add(result);
				continue;
			}
			NamedTypeSymbol key = (memberWithPriority.IsExtensionBlockMember() ? memberWithPriority.ContainingType.ContainingType : memberWithPriority.ContainingType);
			if (instance.TryGetValue(key, out var value))
			{
				int overloadResolutionPriority = value.First().MemberWithPriority.GetOverloadResolutionPriority();
				int overloadResolutionPriority2 = memberWithPriority.GetOverloadResolutionPriority();
				if (overloadResolutionPriority2 > overloadResolutionPriority)
				{
					flag = true;
					instance[key] = OneOrMany.Create(result);
				}
				else if (overloadResolutionPriority2 == overloadResolutionPriority)
				{
					instance[key] = value.Add(result);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				instance.Add(key, OneOrMany.Create(result));
			}
		}
		if (!flag)
		{
			instance.Free();
			instance2.Free();
			return;
		}
		results.Clear();
		foreach (var (_, items) in instance)
		{
			results.AddRange(items);
		}
		results.AddRange(instance2);
		instance.Free();
		instance2.Free();
	}

	private void RemoveWorseMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		if (SingleValidResult(results))
		{
			return;
		}
		int theBestCandidateIndex = GetTheBestCandidateIndex(results, arguments, ref useSiteInfo);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].IsValid && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(results.Count, 0);
		int num = 0;
		int index = -1;
		for (int j = 0; j < results.Count; j++)
		{
			MemberResolutionResult<TMember> m = results[j];
			if (!m.IsValid || instance[j] == 1)
			{
				continue;
			}
			for (int k = 0; k < results.Count; k++)
			{
				MemberResolutionResult<TMember> m2 = results[k];
				if (m2.IsValid && j != k && !(m.Member == m2.Member))
				{
					switch (BetterFunctionMember(m, m2, arguments.Arguments, ref useSiteInfo))
					{
					case BetterResult.Left:
						instance[k] = 1;
						continue;
					case BetterResult.Right:
						break;
					default:
						continue;
					}
					instance[j] = 1;
					break;
				}
			}
			if (instance[j] == 0)
			{
				instance[j] = 2;
				num++;
				index = j;
			}
		}
		switch (num)
		{
		case 0:
		{
			for (int n = 0; n < instance.Count; n++)
			{
				if (instance[n] == 1)
				{
					results[n] = results[n].Worse();
				}
			}
			break;
		}
		case 1:
		{
			for (int num2 = 0; num2 < instance.Count; num2++)
			{
				if (instance[num2] == 1)
				{
					results[num2] = ((BetterFunctionMember(results[index], results[num2], arguments.Arguments, ref useSiteInfo) == BetterResult.Left) ? results[num2].Worst() : results[num2].Worse());
				}
			}
			results[index] = results[index].Worse();
			break;
		}
		default:
		{
			for (int l = 0; l < instance.Count; l++)
			{
				if (instance[l] == 1)
				{
					results[l] = results[l].Worst();
				}
				else if (instance[l] == 2)
				{
					results[l] = results[l].Worse();
				}
			}
			break;
		}
		}
		instance.Free();
	}

	private BetterResult BetterFunctionMember<TMember>(MemberResolutionResult<TMember> m1, MemberResolutionResult<TMember> m2, ArrayBuilder<BoundExpression> arguments, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		bool num = RequiredFunctionType(m1);
		bool flag = RequiredFunctionType(m2);
		if (!num)
		{
			if (flag)
			{
				return BetterResult.Left;
			}
		}
		else if (!flag)
		{
			return BetterResult.Right;
		}
		bool hasAnyRefOmittedArgument = m1.Result.HasAnyRefOmittedArgument;
		bool hasAnyRefOmittedArgument2 = m2.Result.HasAnyRefOmittedArgument;
		if (hasAnyRefOmittedArgument != hasAnyRefOmittedArgument2)
		{
			if (!hasAnyRefOmittedArgument)
			{
				return BetterResult.Left;
			}
			return BetterResult.Right;
		}
		return BetterFunctionMember(m1, m2, arguments, hasAnyRefOmittedArgument, ref useSiteInfo);
	}

	private BetterResult BetterFunctionMember<TMember>(MemberResolutionResult<TMember> m1, MemberResolutionResult<TMember> m2, ArrayBuilder<BoundExpression> arguments, bool considerRefKinds, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		BetterResult betterResult = BetterResult.Neither;
		bool flag = false;
		bool flag2 = false;
		ImmutableArray<ParameterSymbol> parameters = m1.LeastOverriddenMember.GetParameters();
		ImmutableArray<ParameterSymbol> parameters2 = m2.LeastOverriddenMember.GetParameters();
		bool flag3 = true;
		int i;
		for (i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Kind == BoundKind.ArgListOperator)
			{
				continue;
			}
			TypeSymbol typeSymbol = getParameterTypeAndRefKind(i, m1.Result, parameters, m1.Result.ParamsElementTypeOpt, m1.LeastOverriddenMember, out var parameterRefKind);
			TypeSymbol typeSymbol2 = getParameterTypeAndRefKind(i, m2.Result, parameters2, m2.Result.ParamsElementTypeOpt, m2.LeastOverriddenMember, out var parameterRefKind2);
			BetterResult betterResult2 = BetterConversionFromExpression(arguments[i], typeSymbol, m1.Result.ConversionForArg(i), parameterRefKind, typeSymbol2, m2.Result.ConversionForArg(i), parameterRefKind2, considerRefKinds, ref useSiteInfo, out var okToDowngradeToNeither);
			TypeSymbol source = typeSymbol;
			TypeSymbol destination = typeSymbol2;
			if (!_binder.InAttributeArgument)
			{
				source = typeSymbol.NormalizeTaskTypes(Compilation);
				destination = typeSymbol2.NormalizeTaskTypes(Compilation);
			}
			if (betterResult2 == BetterResult.Neither)
			{
				if (flag3 && Conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteInfo).Kind != ConversionKind.Identity)
				{
					flag3 = false;
				}
				continue;
			}
			if (Conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteInfo).Kind != ConversionKind.Identity)
			{
				flag3 = false;
			}
			if (betterResult == BetterResult.Neither)
			{
				if (!(flag2 & okToDowngradeToNeither))
				{
					betterResult = betterResult2;
					flag = okToDowngradeToNeither;
				}
			}
			else if (betterResult != betterResult2)
			{
				if (flag)
				{
					if (okToDowngradeToNeither)
					{
						betterResult = BetterResult.Neither;
						flag = false;
						flag2 = true;
					}
					else
					{
						betterResult = betterResult2;
						flag = false;
					}
				}
				else if (!okToDowngradeToNeither)
				{
					betterResult = BetterResult.Neither;
					break;
				}
			}
			else
			{
				flag &= okToDowngradeToNeither;
			}
		}
		if (betterResult != BetterResult.Neither)
		{
			return betterResult;
		}
		GetParameterCounts(m1, arguments, out var declaredParameterCount, out var parametersUsedIncludingExpansionAndOptional);
		GetParameterCounts(m2, arguments, out var declaredParameterCount2, out var parametersUsedIncludingExpansionAndOptional2);
		RefKind parameterRefKind3;
		if (flag3 && parametersUsedIncludingExpansionAndOptional == parametersUsedIncludingExpansionAndOptional2)
		{
			for (i++; i < arguments.Count; i++)
			{
				if (arguments[i].Kind != BoundKind.ArgListOperator)
				{
					TypeSymbol typeSymbol3 = getParameterTypeAndRefKind(i, m1.Result, parameters, m1.Result.ParamsElementTypeOpt, m1.LeastOverriddenMember, out parameterRefKind3);
					TypeSymbol typeSymbol4 = getParameterTypeAndRefKind(i, m2.Result, parameters2, m2.Result.ParamsElementTypeOpt, m2.LeastOverriddenMember, out parameterRefKind3);
					TypeSymbol source2 = typeSymbol3;
					TypeSymbol destination2 = typeSymbol4;
					if (!_binder.InAttributeArgument)
					{
						source2 = typeSymbol3.NormalizeTaskTypes(Compilation);
						destination2 = typeSymbol4.NormalizeTaskTypes(Compilation);
					}
					if (Conversions.ClassifyImplicitConversionFromType(source2, destination2, ref useSiteInfo).Kind != ConversionKind.Identity)
					{
						flag3 = false;
						break;
					}
				}
			}
		}
		if (!flag3 || parametersUsedIncludingExpansionAndOptional != parametersUsedIncludingExpansionAndOptional2)
		{
			if (parametersUsedIncludingExpansionAndOptional != parametersUsedIncludingExpansionAndOptional2)
			{
				if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
				{
					if (m2.Result.Kind != MemberResolutionKind.ApplicableInExpandedForm)
					{
						return BetterResult.Right;
					}
				}
				else if (m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
				{
					return BetterResult.Left;
				}
				if (parametersUsedIncludingExpansionAndOptional == arguments.Count)
				{
					return BetterResult.Left;
				}
				if (parametersUsedIncludingExpansionAndOptional2 == arguments.Count)
				{
					return BetterResult.Right;
				}
			}
			return preferValOverInOrRefInterpolatedHandlerParameters(arguments, m1, parameters, m2, parameters2);
		}
		if (m1.Member.GetMemberArityIncludingExtension() == 0)
		{
			if (m2.Member.GetMemberArityIncludingExtension() > 0)
			{
				return BetterResult.Left;
			}
		}
		else if (m2.Member.GetMemberArityIncludingExtension() == 0)
		{
			return BetterResult.Right;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInNormalForm && m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			return BetterResult.Left;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm && m2.Result.Kind == MemberResolutionKind.ApplicableInNormalForm)
		{
			return BetterResult.Right;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm && m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			if (declaredParameterCount > declaredParameterCount2)
			{
				return BetterResult.Left;
			}
			if (declaredParameterCount < declaredParameterCount2)
			{
				return BetterResult.Right;
			}
		}
		bool flag4 = m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm || declaredParameterCount == arguments.Count;
		bool flag5 = m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm || declaredParameterCount2 == arguments.Count;
		if (flag4 && !flag5)
		{
			return BetterResult.Left;
		}
		if (!flag4 & flag5)
		{
			return BetterResult.Right;
		}
		using (TemporaryArray<TypeSymbol> array = TemporaryArray<TypeSymbol>.Empty)
		{
			using TemporaryArray<TypeSymbol> array2 = TemporaryArray<TypeSymbol>.Empty;
			ImmutableArray<ParameterSymbol> parameters3 = m1.LeastOverriddenMember.OriginalDefinition.GetParameters();
			ImmutableArray<ParameterSymbol> parameters4 = m2.LeastOverriddenMember.OriginalDefinition.GetParameters();
			for (i = 0; i < arguments.Count; i++)
			{
				if (arguments[i].Kind != BoundKind.ArgListOperator)
				{
					array.Add(getParameterTypeAndRefKind(i, m1.Result, parameters3, m1.Result.DefinitionParamsElementTypeOpt, (TMember)m1.LeastOverriddenMember.OriginalDefinition, out parameterRefKind3));
					array2.Add(getParameterTypeAndRefKind(i, m2.Result, parameters4, m2.Result.DefinitionParamsElementTypeOpt, (TMember)m2.LeastOverriddenMember.OriginalDefinition, out parameterRefKind3));
				}
			}
			betterResult = MoreSpecificType(ref TemporaryArrayExtensions.AsRef(in array), ref TemporaryArrayExtensions.AsRef(in array2), ref useSiteInfo);
			if (betterResult != BetterResult.Neither)
			{
				return betterResult;
			}
		}
		if (m1.Member.ContainingType.TypeKind == TypeKind.Submission && m2.Member.ContainingType.TypeKind == TypeKind.Submission)
		{
			CSharpCompilation declaringCompilation = m1.Member.DeclaringCompilation;
			CSharpCompilation declaringCompilation2 = m2.Member.DeclaringCompilation;
			int submissionSlotIndex = declaringCompilation.GetSubmissionSlotIndex();
			int submissionSlotIndex2 = declaringCompilation2.GetSubmissionSlotIndex();
			if (submissionSlotIndex > submissionSlotIndex2)
			{
				return BetterResult.Left;
			}
			if (submissionSlotIndex < submissionSlotIndex2)
			{
				return BetterResult.Right;
			}
		}
		int num = m1.LeastOverriddenMember.CustomModifierCount();
		int num2 = m2.LeastOverriddenMember.CustomModifierCount();
		if (num != num2)
		{
			if (num >= num2)
			{
				return BetterResult.Right;
			}
			return BetterResult.Left;
		}
		betterResult = preferValOverInOrRefInterpolatedHandlerParameters(arguments, m1, parameters, m2, parameters2);
		if (betterResult != BetterResult.Neither)
		{
			return betterResult;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm && m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			ParameterSymbol parameterSymbol = parameters[parameters.Length - 1];
			ParameterSymbol parameterSymbol2 = parameters2[parameters2.Length - 1];
			for (i = 0; i < arguments.Count; i++)
			{
				ParameterSymbol parameterSymbol3 = getParameterOrExtensionParameter(i, m1.Result, parameters, m1.LeastOverriddenMember);
				ParameterSymbol parameterSymbol4 = getParameterOrExtensionParameter(i, m2.Result, parameters2, m2.LeastOverriddenMember);
				if ((object)parameterSymbol3 == parameterSymbol != ((object)parameterSymbol4 == parameterSymbol2))
				{
					break;
				}
			}
			if (i == arguments.Count)
			{
				TypeSymbol type = parameterSymbol.Type;
				TypeSymbol type2 = parameterSymbol2.Type;
				if (!ConversionsBase.HasIdentityConversion(type, type2))
				{
					BetterResult betterResult3 = BetterParamsCollectionType(type, type2, ref useSiteInfo);
					if (betterResult3 != BetterResult.Neither)
					{
						return betterResult3;
					}
				}
			}
		}
		return BetterResult.Neither;
		static ParameterSymbol getParameterOrExtensionParameter(int argIndex, MemberAnalysisResult result, ImmutableArray<ParameterSymbol> immutableArray, TMember member)
		{
			int num3 = result.ParameterFromArgument(argIndex);
			if (member.IsExtensionBlockMember())
			{
				if (num3 == 0)
				{
					return member.ContainingType.ExtensionParameter;
				}
				num3--;
			}
			return immutableArray[num3];
		}
		static TypeSymbol getParameterTypeAndRefKind(int argIndex, MemberAnalysisResult memberResolutionResult, ImmutableArray<ParameterSymbol> parameters5, TypeWithAnnotations paramsElementTypeOpt, TMember member, out RefKind reference)
		{
			ParameterSymbol parameterSymbol5 = getParameterOrExtensionParameter(argIndex, memberResolutionResult, parameters5, member);
			reference = GetParameterBetternessRefKind(parameterSymbol5, member);
			TypeSymbol type3 = parameterSymbol5.Type;
			if (memberResolutionResult.Kind == MemberResolutionKind.ApplicableInExpandedForm)
			{
				if ((object)parameterSymbol5 == parameters5[parameters5.Length - 1])
				{
					return paramsElementTypeOpt.Type;
				}
			}
			return type3;
		}
		static bool isAcceptableRefMismatch(RefKind refKind, bool isInterpolatedStringHandlerConversion)
		{
			switch (refKind)
			{
			case RefKind.In:
			case RefKind.RefReadOnlyParameter:
				return true;
			case RefKind.Ref:
				if (isInterpolatedStringHandlerConversion)
				{
					return true;
				}
				break;
			}
			return false;
		}
		static BetterResult preferValOverInOrRefInterpolatedHandlerParameters(ArrayBuilder<BoundExpression> arrayBuilder, MemberResolutionResult<TMember> memberResolutionResult, ImmutableArray<ParameterSymbol> parameters5, MemberResolutionResult<TMember> memberResolutionResult2, ImmutableArray<ParameterSymbol> parameters6)
		{
			BetterResult betterResult4 = BetterResult.Neither;
			for (int j = 0; j < arrayBuilder.Count; j++)
			{
				if (arrayBuilder[j].Kind != BoundKind.ArgListOperator)
				{
					ParameterSymbol parameter = getParameterOrExtensionParameter(j, memberResolutionResult.Result, parameters5, memberResolutionResult.Member);
					ParameterSymbol parameter2 = getParameterOrExtensionParameter(j, memberResolutionResult2.Result, parameters6, memberResolutionResult2.Member);
					bool isInterpolatedStringHandlerConversion = false;
					if (memberResolutionResult.IsValid && memberResolutionResult2.IsValid)
					{
						Conversion conversion = memberResolutionResult.Result.ConversionForArg(j);
						Conversion conversion2 = memberResolutionResult2.Result.ConversionForArg(j);
						isInterpolatedStringHandlerConversion = conversion.IsInterpolatedStringHandler && conversion2.IsInterpolatedStringHandler;
					}
					RefKind parameterBetternessRefKind = GetParameterBetternessRefKind(parameter, memberResolutionResult.Member);
					RefKind parameterBetternessRefKind2 = GetParameterBetternessRefKind(parameter2, memberResolutionResult2.Member);
					if (parameterBetternessRefKind == RefKind.None && isAcceptableRefMismatch(parameterBetternessRefKind2, isInterpolatedStringHandlerConversion))
					{
						if (betterResult4 == BetterResult.Right)
						{
							return BetterResult.Neither;
						}
						betterResult4 = BetterResult.Left;
					}
					else if (parameterBetternessRefKind2 == RefKind.None && isAcceptableRefMismatch(parameterBetternessRefKind, isInterpolatedStringHandlerConversion))
					{
						if (betterResult4 == BetterResult.Left)
						{
							return BetterResult.Neither;
						}
						betterResult4 = BetterResult.Right;
					}
				}
			}
			return betterResult4;
		}
	}

	private static RefKind GetParameterBetternessRefKind<TMember>(ParameterSymbol parameter, TMember member) where TMember : Symbol
	{
		if ((object)parameter != null && parameter.ContainingSymbol is NamedTypeSymbol { IsExtension: not false } namedTypeSymbol)
		{
			ParameterSymbol extensionParameter = namedTypeSymbol.ExtensionParameter;
			if (member.IsStatic && (object)parameter == extensionParameter)
			{
				return RefKind.None;
			}
		}
		return parameter.RefKind;
	}

	private static bool RequiredFunctionType<TMember>(MemberResolutionResult<TMember> m) where TMember : Symbol
	{
		if (m.HasTypeArgumentInferredFromFunctionType)
		{
			return true;
		}
		ImmutableArray<Conversion> conversionsOpt = m.Result.ConversionsOpt;
		if (conversionsOpt.IsDefault)
		{
			return false;
		}
		return conversionsOpt.Any((Conversion c) => c.Kind == ConversionKind.FunctionType);
	}

	private static void GetParameterCounts<TMember>(MemberResolutionResult<TMember> m, ArrayBuilder<BoundExpression> arguments, out int declaredParameterCount, out int parametersUsedIncludingExpansionAndOptional) where TMember : Symbol
	{
		declaredParameterCount = m.Member.GetParameterCount() + (m.Member.IsExtensionBlockMember() ? 1 : 0);
		if (m.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			if (arguments.Count < declaredParameterCount)
			{
				ImmutableArray<int> argsToParamsOpt = m.Result.ArgsToParamsOpt;
				if (argsToParamsOpt.IsDefaultOrEmpty || !argsToParamsOpt.Contains(declaredParameterCount - 1))
				{
					parametersUsedIncludingExpansionAndOptional = declaredParameterCount - 1;
				}
				else
				{
					parametersUsedIncludingExpansionAndOptional = declaredParameterCount;
				}
			}
			else
			{
				parametersUsedIncludingExpansionAndOptional = arguments.Count;
			}
		}
		else
		{
			parametersUsedIncludingExpansionAndOptional = declaredParameterCount;
		}
	}

	private static BetterResult MoreSpecificType(ref TemporaryArray<TypeSymbol> t1, ref TemporaryArray<TypeSymbol> t2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		BetterResult betterResult = BetterResult.Neither;
		for (int i = 0; i < t1.Count; i++)
		{
			BetterResult betterResult2 = MoreSpecificType(t1[i], t2[i], ref useSiteInfo);
			if (betterResult2 != BetterResult.Neither)
			{
				if (betterResult == BetterResult.Neither)
				{
					betterResult = betterResult2;
				}
				else if (betterResult != betterResult2)
				{
					return BetterResult.Neither;
				}
			}
		}
		return betterResult;
	}

	private static BetterResult MoreSpecificType(TypeSymbol t1, TypeSymbol t2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool flag = t1.IsTypeParameter();
		bool flag2 = t2.IsTypeParameter();
		if (flag && !flag2)
		{
			return BetterResult.Right;
		}
		if (!flag & flag2)
		{
			return BetterResult.Left;
		}
		if (flag & flag2)
		{
			return BetterResult.Neither;
		}
		if (t1.IsArray())
		{
			ArrayTypeSymbol obj = (ArrayTypeSymbol)t1;
			return MoreSpecificType(t2: ((ArrayTypeSymbol)t2).ElementType, t1: obj.ElementType, useSiteInfo: ref useSiteInfo);
		}
		if (t1.TypeKind == TypeKind.Pointer)
		{
			PointerTypeSymbol obj2 = (PointerTypeSymbol)t1;
			return MoreSpecificType(t2: ((PointerTypeSymbol)t2).PointedAtType, t1: obj2.PointedAtType, useSiteInfo: ref useSiteInfo);
		}
		if (t1.IsDynamic() || t2.IsDynamic())
		{
			return BetterResult.Neither;
		}
		NamedTypeSymbol namedTypeSymbol = t1 as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = t2 as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			return BetterResult.Neither;
		}
		using TemporaryArray<TypeSymbol> array = TemporaryArray<TypeSymbol>.Empty;
		using TemporaryArray<TypeSymbol> array2 = TemporaryArray<TypeSymbol>.Empty;
		namedTypeSymbol.GetAllTypeArguments(ref TemporaryArrayExtensions.AsRef(in array), ref useSiteInfo);
		namedTypeSymbol2.GetAllTypeArguments(ref TemporaryArrayExtensions.AsRef(in array2), ref useSiteInfo);
		return MoreSpecificType(ref TemporaryArrayExtensions.AsRef(in array), ref TemporaryArrayExtensions.AsRef(in array2), ref useSiteInfo);
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, TypeSymbol t2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool okToDowngradeToNeither;
		return BetterConversionFromExpression(node, t1, Conversions.ClassifyImplicitConversionFromExpression(node, t1, ref useSiteInfo), t2, Conversions.ClassifyImplicitConversionFromExpression(node, t2, ref useSiteInfo), ref useSiteInfo, out okToDowngradeToNeither);
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, Conversion conv1, RefKind refKind1, TypeSymbol t2, Conversion conv2, RefKind refKind2, bool considerRefKinds, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool okToDowngradeToNeither)
	{
		okToDowngradeToNeither = false;
		if (considerRefKinds)
		{
			if (refKind1 != refKind2)
			{
				if (refKind1 == RefKind.None)
				{
					if (conv1.Kind != ConversionKind.Identity)
					{
						return BetterResult.Neither;
					}
					return BetterResult.Left;
				}
				if (conv2.Kind != ConversionKind.Identity)
				{
					return BetterResult.Neither;
				}
				return BetterResult.Right;
			}
			if (refKind1 == RefKind.Ref)
			{
				return BetterResult.Neither;
			}
		}
		return BetterConversionFromExpression(node, t1, conv1, t2, conv2, ref useSiteInfo, out okToDowngradeToNeither);
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, Conversion conv1, TypeSymbol t2, Conversion conv2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool okToDowngradeToNeither)
	{
		okToDowngradeToNeither = false;
		if (ConversionsBase.HasIdentityConversion(t1, t2))
		{
			return BetterResult.Neither;
		}
		UnboundLambda unboundLambda = node as UnboundLambda;
		BoundKind kind = node.Kind;
		if (kind == BoundKind.OutVariablePendingInference || kind == BoundKind.OutDeconstructVarPendingInference || (kind == BoundKind.DiscardExpression && !node.HasExpressionType()))
		{
			okToDowngradeToNeither = false;
			return BetterResult.Neither;
		}
		bool flag = _binder.Compilation.IsFeatureEnabled(MessageID.IDS_FeatureImprovedInterpolatedStrings);
		bool flag2;
		if (flag)
		{
			if (node is BoundUnconvertedInterpolatedString boundUnconvertedInterpolatedString)
			{
				if ((object)boundUnconvertedInterpolatedString.ConstantValueOpt == null)
				{
					goto IL_0092;
				}
			}
			else if (node is BoundBinaryOperator { IsUnconvertedInterpolatedStringAddition: not false, ConstantValueOpt: null })
			{
				goto IL_0092;
			}
			flag2 = false;
			goto IL_009a;
		}
		goto IL_009d;
		IL_009d:
		ConversionKind kind3;
		if (flag)
		{
			ConversionKind kind2 = conv1.Kind;
			kind3 = conv2.Kind;
			if (kind2 == ConversionKind.InterpolatedStringHandler)
			{
				if (kind3 == ConversionKind.InterpolatedStringHandler)
				{
					return BetterResult.Neither;
				}
				return BetterResult.Left;
			}
			if (kind3 == ConversionKind.InterpolatedStringHandler)
			{
				return BetterResult.Right;
			}
		}
		ConversionKind kind4 = conv1.Kind;
		kind3 = conv2.Kind;
		if (kind4 == ConversionKind.FunctionType)
		{
			if (kind3 != ConversionKind.FunctionType)
			{
				return BetterResult.Right;
			}
		}
		else if (kind3 == ConversionKind.FunctionType)
		{
			return BetterResult.Left;
		}
		bool num = ExpressionMatchExactly(node, t1, ref useSiteInfo);
		bool flag3 = ExpressionMatchExactly(node, t2, ref useSiteInfo);
		if (num)
		{
			if (!flag3)
			{
				okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Left, unboundLambda, t1, t2, ref useSiteInfo, fromTypeAnalysis: false);
				return BetterResult.Left;
			}
		}
		else if (flag3)
		{
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Right, unboundLambda, t1, t2, ref useSiteInfo, fromTypeAnalysis: false);
			return BetterResult.Right;
		}
		if (!conv1.IsConditionalExpression && conv2.IsConditionalExpression)
		{
			return BetterResult.Left;
		}
		if (!conv2.IsConditionalExpression && conv1.IsConditionalExpression)
		{
			return BetterResult.Right;
		}
		if (conv1.Kind == ConversionKind.CollectionExpression && conv2.Kind == ConversionKind.CollectionExpression)
		{
			return BetterCollectionExpressionConversion((BoundUnconvertedCollectionExpression)node, t1, conv1, t2, conv2, ref useSiteInfo);
		}
		ConversionKind kind5 = conv1.Kind;
		kind3 = conv2.Kind;
		if (kind5 == ConversionKind.ImplicitSpan)
		{
			if (kind3 != ConversionKind.ImplicitSpan)
			{
				return BetterResult.Left;
			}
		}
		else if (kind3 == ConversionKind.ImplicitSpan)
		{
			return BetterResult.Right;
		}
		return BetterConversionTarget(node, t1, conv1, t2, conv2, ref useSiteInfo, out okToDowngradeToNeither);
		IL_009a:
		flag = flag2;
		goto IL_009d;
		IL_0092:
		flag2 = true;
		goto IL_009a;
	}

	private BetterResult BetterCollectionExpressionConversion(BoundUnconvertedCollectionExpression collectionExpression, TypeSymbol t1, Conversion conv1, TypeSymbol t2, Conversion conv2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		CollectionExpressionTypeKind collectionExpressionTypeKind = conv1.GetCollectionExpressionTypeKind(out TypeSymbol elementType, out MethodSymbol constructor, out bool isExpanded);
		CollectionExpressionTypeKind collectionExpressionTypeKind2 = conv2.GetCollectionExpressionTypeKind(out TypeSymbol elementType2, out constructor, out isExpanded);
		if (Compilation.LanguageVersion < LanguageVersion.CSharp13)
		{
			if (IsBetterCollectionExpressionConversion_CSharp12(t1, collectionExpressionTypeKind, elementType, t2, collectionExpressionTypeKind2, elementType2, ref useSiteInfo))
			{
				return BetterResult.Left;
			}
			if (IsBetterCollectionExpressionConversion_CSharp12(t2, collectionExpressionTypeKind2, elementType2, t1, collectionExpressionTypeKind, elementType, ref useSiteInfo))
			{
				return BetterResult.Right;
			}
			return BetterResult.Neither;
		}
		return BetterCollectionExpressionConversion(collectionExpression.Elements, t1, collectionExpressionTypeKind, elementType, conv1.UnderlyingConversions, t2, collectionExpressionTypeKind2, elementType2, conv2.UnderlyingConversions, ref useSiteInfo);
	}

	private BetterResult BetterCollectionExpressionConversion(ImmutableArray<BoundNode> collectionExpressionElements, TypeSymbol t1, CollectionExpressionTypeKind kind1, TypeSymbol elementType1, ImmutableArray<Conversion> underlyingElementConversions1, TypeSymbol t2, CollectionExpressionTypeKind kind2, TypeSymbol elementType2, ImmutableArray<Conversion> underlyingElementConversions2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool flag = (uint)(kind1 - 2) <= 1u;
		bool flag2 = flag;
		flag = (uint)(kind2 - 2) <= 1u;
		bool flag3 = flag;
		if (!flag2 && !flag3)
		{
			bool isImplicit = Conversions.ClassifyImplicitConversionFromType(t1, t2, ref useSiteInfo).IsImplicit;
			bool isImplicit2 = Conversions.ClassifyImplicitConversionFromType(t2, t1, ref useSiteInfo).IsImplicit;
			if (isImplicit)
			{
				if (!isImplicit2)
				{
					return BetterResult.Left;
				}
			}
			else if (isImplicit2)
			{
				return BetterResult.Right;
			}
		}
		if (!ConversionsBase.HasIdentityConversion(elementType1, elementType2))
		{
			BetterResult betterResult = BetterResult.Neither;
			for (int i = 0; i < underlyingElementConversions1.Length; i++)
			{
				BoundNode boundNode = collectionExpressionElements[i];
				Conversion conv = underlyingElementConversions1[i];
				Conversion conv2 = underlyingElementConversions2[i];
				BetterResult betterResult2 = ((!(boundNode is BoundCollectionExpressionSpreadElement node)) ? BetterConversionFromExpression((BoundExpression)boundNode, elementType1, conv, elementType2, conv2, ref useSiteInfo, out flag) : BetterConversionTarget(node, elementType1, conv, elementType2, conv2, ref useSiteInfo, out flag));
				if (betterResult2 == BetterResult.Neither)
				{
					continue;
				}
				if (betterResult != BetterResult.Neither)
				{
					if (betterResult != betterResult2)
					{
						return BetterResult.Neither;
					}
				}
				else
				{
					betterResult = betterResult2;
				}
			}
			return betterResult;
		}
		CollectionExpressionTypeKind item = default(CollectionExpressionTypeKind);
		TypeSymbol elementType3;
		if (flag2 | flag3)
		{
			(CollectionExpressionTypeKind, CollectionExpressionTypeKind) tuple = (kind1, kind2);
			var (collectionExpressionTypeKind, _) = tuple;
			int num;
			if (collectionExpressionTypeKind != CollectionExpressionTypeKind.Span)
			{
				if (collectionExpressionTypeKind != CollectionExpressionTypeKind.ReadOnlySpan)
				{
					item = tuple.Item2;
					if (item != CollectionExpressionTypeKind.ReadOnlySpan)
					{
						goto IL_0164;
					}
					goto IL_0189;
				}
				item = tuple.Item2;
				if (item == CollectionExpressionTypeKind.Span)
				{
					goto IL_0187;
				}
				num = 0;
			}
			else
			{
				num = 1;
			}
			if (!IsSZArrayOrArrayInterface(t2, out elementType3))
			{
				if (num == 0)
				{
					if (item == CollectionExpressionTypeKind.ReadOnlySpan)
					{
						goto IL_0189;
					}
					goto IL_0195;
				}
				if (num == 1)
				{
					item = tuple.Item2;
					if (item != CollectionExpressionTypeKind.ReadOnlySpan)
					{
						goto IL_0164;
					}
					goto IL_0193;
				}
			}
			goto IL_0187;
		}
		goto IL_0195;
		IL_0195:
		return BetterResult.Neither;
		IL_0187:
		return BetterResult.Left;
		IL_0164:
		if (item == CollectionExpressionTypeKind.Span)
		{
			goto IL_0189;
		}
		goto IL_0195;
		IL_0189:
		if (IsSZArrayOrArrayInterface(t1, out elementType3))
		{
			goto IL_0193;
		}
		goto IL_0195;
		IL_0193:
		return BetterResult.Right;
	}

	private bool IsBetterCollectionExpressionConversion_CSharp12(TypeSymbol t1, CollectionExpressionTypeKind kind1, TypeSymbol elementType1, TypeSymbol t2, CollectionExpressionTypeKind kind2, TypeSymbol elementType2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (kind1 == CollectionExpressionTypeKind.ReadOnlySpan && kind2 == CollectionExpressionTypeKind.Span)
		{
			return hasImplicitConversion(elementType1, elementType2, ref useSiteInfo);
		}
		if ((uint)(kind1 - 2) <= 1u)
		{
			if (IsSZArrayOrArrayInterface(t2, out elementType2))
			{
				return hasImplicitConversion(elementType1, elementType2, ref useSiteInfo);
			}
			return false;
		}
		bool flag = (uint)(kind2 - 2) <= 1u;
		if (!flag && hasImplicitConversion(t1, t2, ref useSiteInfo))
		{
			return true;
		}
		return false;
		bool hasImplicitConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			return Conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteInfo2).IsImplicit;
		}
	}

	private BetterResult BetterParamsCollectionType(TypeSymbol t1, TypeSymbol t2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(Compilation, t1, out var elementType);
		CollectionExpressionTypeKind collectionExpressionTypeKind2 = ConversionsBase.GetCollectionExpressionTypeKind(Compilation, t2, out var elementType2);
		if ((uint)(collectionExpressionTypeKind - 4) <= 1u)
		{
			_binder.TryGetCollectionIterationType(CSharpSyntaxTree.Dummy.GetRoot(), t1, out elementType);
		}
		if ((uint)(collectionExpressionTypeKind2 - 4) <= 1u)
		{
			_binder.TryGetCollectionIterationType(CSharpSyntaxTree.Dummy.GetRoot(), t2, out elementType2);
		}
		return BetterCollectionExpressionConversion(ImmutableArray<BoundNode>.Empty, t1, collectionExpressionTypeKind, elementType.Type, ImmutableArray<Conversion>.Empty, t2, collectionExpressionTypeKind2, elementType2.Type, ImmutableArray<Conversion>.Empty, ref useSiteInfo);
	}

	private static bool IsSZArrayOrArrayInterface(TypeSymbol type, out TypeSymbol elementType)
	{
		if (type is ArrayTypeSymbol { IsSZArray: not false } arrayTypeSymbol)
		{
			elementType = arrayTypeSymbol.ElementType;
			return true;
		}
		if (type.IsArrayInterface(out var typeArgument))
		{
			elementType = typeArgument.Type;
			return true;
		}
		elementType = null;
		return false;
	}

	private bool ExpressionMatchExactly(BoundExpression node, TypeSymbol t, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)node.Type != null && ConversionsBase.HasIdentityConversion(node.Type, t))
		{
			return true;
		}
		if (node.Kind == BoundKind.TupleLiteral)
		{
			return ExpressionMatchExactly((BoundTupleLiteral)node, t, ref useSiteInfo);
		}
		NamedTypeSymbol delegateType;
		MethodSymbol delegateInvokeMethod;
		TypeSymbol typeSymbol;
		if (node.Kind == BoundKind.UnboundLambda && (object)(delegateType = t.GetDelegateType()) != null && (object)(delegateInvokeMethod = delegateType.DelegateInvokeMethod) != null && !(typeSymbol = delegateInvokeMethod.ReturnType).IsVoidType())
		{
			BoundLambda boundLambda = ((UnboundLambda)node).BindForReturnTypeInference(delegateType);
			TypeWithAnnotations inferredReturnType = boundLambda.GetInferredReturnType(ref useSiteInfo, out var _);
			if (inferredReturnType.HasType && ConversionsBase.HasIdentityConversion(inferredReturnType.Type, typeSymbol))
			{
				return true;
			}
			if (boundLambda.Symbol.IsAsync)
			{
				typeSymbol = ((!typeSymbol.OriginalDefinition.IsGenericTaskType(Compilation)) ? null : ((NamedTypeSymbol)typeSymbol).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type);
			}
			if ((object)typeSymbol != null)
			{
				int length = boundLambda.Body.Statements.Length;
				if (length != 0)
				{
					if (length == 1 && boundLambda.Body.Statements[0].Kind == BoundKind.ReturnStatement)
					{
						BoundReturnStatement boundReturnStatement = (BoundReturnStatement)boundLambda.Body.Statements[0];
						if (boundReturnStatement.ExpressionOpt != null && ExpressionMatchExactly(boundReturnStatement.ExpressionOpt, typeSymbol, ref useSiteInfo))
						{
							return true;
						}
					}
					else
					{
						ArrayBuilder<BoundReturnStatement> instance = ArrayBuilder<BoundReturnStatement>.GetInstance();
						new ReturnStatements(instance).Visit(boundLambda.Body);
						bool flag = false;
						foreach (BoundReturnStatement item in instance)
						{
							if (item.ExpressionOpt == null || !ExpressionMatchExactly(item.ExpressionOpt, typeSymbol, ref useSiteInfo))
							{
								flag = false;
								break;
							}
							flag = true;
						}
						instance.Free();
						if (flag)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private bool ExpressionMatchExactly(BoundTupleLiteral tupleSource, TypeSymbol targetType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (targetType.Kind != SymbolKind.NamedType)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)targetType;
		ImmutableArray<BoundExpression> arguments = tupleSource.Arguments;
		if (!namedTypeSymbol.IsTupleTypeOfCardinality(arguments.Length))
		{
			return false;
		}
		ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = namedTypeSymbol.TupleElementTypesWithAnnotations;
		for (int i = 0; i < arguments.Length; i++)
		{
			if (!ExpressionMatchExactly(arguments[i], tupleElementTypesWithAnnotations[i].Type, ref useSiteInfo))
			{
				return false;
			}
		}
		return true;
	}

	private BetterResult BetterConversionTargetCore(TypeSymbol type1, TypeSymbol type2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, int betterConversionTargetRecursionLimit)
	{
		if (betterConversionTargetRecursionLimit < 0)
		{
			return BetterResult.Neither;
		}
		bool okToDowngradeToNeither;
		return BetterConversionTargetCore(null, type1, default(Conversion), type2, default(Conversion), ref useSiteInfo, out okToDowngradeToNeither, betterConversionTargetRecursionLimit - 1);
	}

	private BetterResult BetterConversionTarget(BoundNode node, TypeSymbol type1, Conversion conv1, TypeSymbol type2, Conversion conv2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool okToDowngradeToNeither)
	{
		return BetterConversionTargetCore(node, type1, conv1, type2, conv2, ref useSiteInfo, out okToDowngradeToNeither, 100);
	}

	private BetterResult BetterConversionTargetCore(BoundNode node, TypeSymbol type1, Conversion conv1, TypeSymbol type2, Conversion conv2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool okToDowngradeToNeither, int betterConversionTargetRecursionLimit)
	{
		okToDowngradeToNeither = false;
		if (ConversionsBase.HasIdentityConversion(type1, type2))
		{
			return BetterResult.Neither;
		}
		if (Compilation.IsFeatureEnabled(MessageID.IDS_FeatureFirstClassSpan))
		{
			if (isBetterSpanConversionTarget(type1, type2))
			{
				return BetterResult.Left;
			}
			if (isBetterSpanConversionTarget(type2, type1))
			{
				return BetterResult.Right;
			}
			if ((type1.IsSpan() || type1.IsReadOnlySpan()) && (type2.IsSpan() || type2.IsReadOnlySpan()) && (!type1.IsReadOnlySpan() || !type2.IsReadOnlySpan()))
			{
				return BetterResult.Neither;
			}
		}
		bool isImplicit = Conversions.ClassifyImplicitConversionFromType(type1, type2, ref useSiteInfo).IsImplicit;
		bool isImplicit2 = Conversions.ClassifyImplicitConversionFromType(type2, type1, ref useSiteInfo).IsImplicit;
		UnboundLambda unboundLambda = node as UnboundLambda;
		if (isImplicit)
		{
			if (isImplicit2)
			{
				return BetterResult.Neither;
			}
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Left, unboundLambda, type1, type2, ref useSiteInfo, fromTypeAnalysis: true);
			return BetterResult.Left;
		}
		if (isImplicit2)
		{
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Right, unboundLambda, type1, type2, ref useSiteInfo, fromTypeAnalysis: true);
			return BetterResult.Right;
		}
		bool num = type1.OriginalDefinition.IsGenericTaskType(Compilation);
		bool flag = type2.OriginalDefinition.IsGenericTaskType(Compilation);
		if (num)
		{
			if (flag)
			{
				return BetterConversionTargetCore(((NamedTypeSymbol)type1).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type, ((NamedTypeSymbol)type2).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type, ref useSiteInfo, betterConversionTargetRecursionLimit);
			}
			return BetterResult.Neither;
		}
		if (flag)
		{
			return BetterResult.Neither;
		}
		NamedTypeSymbol delegateType;
		if ((object)(delegateType = type1.GetDelegateType()) != null)
		{
			NamedTypeSymbol delegateType2;
			if ((object)(delegateType2 = type2.GetDelegateType()) != null)
			{
				MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
				MethodSymbol delegateInvokeMethod2 = delegateType2.DelegateInvokeMethod;
				if ((object)delegateInvokeMethod != null && (object)delegateInvokeMethod2 != null)
				{
					TypeSymbol returnType = delegateInvokeMethod.ReturnType;
					TypeSymbol returnType2 = delegateInvokeMethod2.ReturnType;
					BetterResult betterResult = BetterResult.Neither;
					if (!returnType.IsVoidType())
					{
						if (returnType2.IsVoidType())
						{
							betterResult = BetterResult.Left;
						}
					}
					else if (!returnType2.IsVoidType())
					{
						betterResult = BetterResult.Right;
					}
					if (betterResult == BetterResult.Neither)
					{
						betterResult = BetterConversionTargetCore(returnType, returnType2, ref useSiteInfo, betterConversionTargetRecursionLimit);
					}
					if (node != null && node.Kind == BoundKind.MethodGroup)
					{
						BoundMethodGroup node2 = (BoundMethodGroup)node;
						switch (betterResult)
						{
						case BetterResult.Left:
							if (IsMethodGroupConversionIncompatibleWithDelegate(node2, delegateType, conv1))
							{
								return BetterResult.Neither;
							}
							break;
						case BetterResult.Right:
							if (IsMethodGroupConversionIncompatibleWithDelegate(node2, delegateType2, conv2))
							{
								return BetterResult.Neither;
							}
							break;
						}
					}
					return betterResult;
				}
			}
			return BetterResult.Neither;
		}
		if ((object)type2.GetDelegateType() != null)
		{
			return BetterResult.Neither;
		}
		if (IsSignedIntegralType(type1))
		{
			if (IsUnsignedIntegralType(type2))
			{
				return BetterResult.Left;
			}
		}
		else if (IsUnsignedIntegralType(type1) && IsSignedIntegralType(type2))
		{
			return BetterResult.Right;
		}
		return BetterResult.Neither;
		static bool isBetterSpanConversionTarget(TypeSymbol typeSymbol, TypeSymbol typeSymbol2)
		{
			if (typeSymbol.IsReadOnlySpan() && typeSymbol2.IsSpan())
			{
				TypeSymbol type3 = ((NamedTypeSymbol)typeSymbol).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type;
				TypeSymbol type4 = ((NamedTypeSymbol)typeSymbol2).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type;
				if (ConversionsBase.HasIdentityConversion(type3, type4))
				{
					return true;
				}
			}
			return false;
		}
	}

	private bool IsMethodGroupConversionIncompatibleWithDelegate(BoundMethodGroup node, NamedTypeSymbol delegateType, Conversion conv)
	{
		if (conv.IsMethodGroup)
		{
			return !_binder.MethodIsCompatibleWithDelegateOrFunctionPointer(node.ReceiverOpt, conv.IsExtensionMethod, conv.Method, delegateType, Location.None, BindingDiagnosticBag.Discarded);
		}
		return false;
	}

	private bool CanDowngradeConversionFromLambdaToNeither(BetterResult currentResult, UnboundLambda lambda, TypeSymbol type1, TypeSymbol type2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool fromTypeAnalysis)
	{
		NamedTypeSymbol delegateType;
		NamedTypeSymbol delegateType2;
		if ((object)(delegateType = type1.GetDelegateType()) != null && (object)(delegateType2 = type2.GetDelegateType()) != null)
		{
			MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
			MethodSymbol delegateInvokeMethod2 = delegateType2.DelegateInvokeMethod;
			if ((object)delegateInvokeMethod != null && (object)delegateInvokeMethod2 != null)
			{
				if (!IdenticalParameters(delegateInvokeMethod.Parameters, delegateInvokeMethod2.Parameters))
				{
					return true;
				}
				TypeSymbol returnType = delegateInvokeMethod.ReturnType;
				TypeSymbol returnType2 = delegateInvokeMethod2.ReturnType;
				if (returnType.IsVoidType())
				{
					if (returnType2.IsVoidType())
					{
						return true;
					}
					return false;
				}
				if (returnType2.IsVoidType())
				{
					return false;
				}
				if (ConversionsBase.HasIdentityConversion(returnType, returnType2))
				{
					return true;
				}
				if (!lambda.InferReturnType(Conversions, delegateType, ref useSiteInfo, out var _).HasType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IdenticalParameters(ImmutableArray<ParameterSymbol> p1, ImmutableArray<ParameterSymbol> p2)
	{
		if (p1.IsDefault || p2.IsDefault)
		{
			return false;
		}
		if (p1.Length != p2.Length)
		{
			return false;
		}
		for (int i = 0; i < p1.Length; i++)
		{
			ParameterSymbol parameterSymbol = p1[i];
			ParameterSymbol parameterSymbol2 = p2[i];
			if (parameterSymbol.RefKind != parameterSymbol2.RefKind)
			{
				return false;
			}
			if (!ConversionsBase.HasIdentityConversion(parameterSymbol.Type, parameterSymbol2.Type))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsSignedIntegralType(TypeSymbol type)
	{
		if ((object)type != null && type.IsNullableType())
		{
			type = type.GetNullableUnderlyingType();
		}
		switch (type.GetSpecialTypeSafe())
		{
		case SpecialType.System_IntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_SByte;
		case SpecialType.System_SByte:
		case SpecialType.System_Int16:
		case SpecialType.System_Int32:
		case SpecialType.System_Int64:
			return true;
		}
		return false;
	}

	private static bool IsUnsignedIntegralType(TypeSymbol type)
	{
		if ((object)type != null && type.IsNullableType())
		{
			type = type.GetNullableUnderlyingType();
		}
		switch (type.GetSpecialTypeSafe())
		{
		case SpecialType.System_UIntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Byte;
		case SpecialType.System_Byte:
		case SpecialType.System_UInt16:
		case SpecialType.System_UInt32:
		case SpecialType.System_UInt64:
			return true;
		}
		return false;
	}

	internal static void GetEffectiveParameterTypes(Symbol member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, bool isMethodGroupConversion, bool allowRefOmittedArguments, Binder binder, bool expanded, out ImmutableArray<TypeWithAnnotations> parameterTypes, out ImmutableArray<RefKind> parameterRefKinds)
	{
		Options options = (Options)((isMethodGroupConversion ? 1 : 0) | (allowRefOmittedArguments ? 2 : 0));
		EffectiveParameters effectiveParameters = (expanded ? GetEffectiveParametersInExpandedForm(member, argumentCount, argToParamMap, argumentRefKinds, options, binder, out var hasAnyRefOmittedArgument) : GetEffectiveParametersInNormalForm(member, argumentCount, argToParamMap, argumentRefKinds, options, binder, out hasAnyRefOmittedArgument));
		parameterTypes = effectiveParameters.ParameterTypes;
		parameterRefKinds = effectiveParameters.ParameterRefKinds;
	}

	private static EffectiveParameters GetEffectiveParametersInNormalForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, Options options, Binder binder, out bool hasAnyRefOmittedArgument) where TMember : Symbol
	{
		hasAnyRefOmittedArgument = false;
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = member.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
		int num = parametersIncludingExtensionParameter.Length + (member.GetIsVararg() ? 1 : 0);
		if (argumentCount == num && argToParamMap.IsDefaultOrEmpty)
		{
			bool flag = !member.GetParameterRefKinds().IsDefaultOrEmpty;
			bool flag2 = member.IsExtensionBlockMember();
			if (flag2)
			{
				flag |= member.ContainingType.ExtensionParameter.RefKind != RefKind.None;
			}
			if (!flag)
			{
				return new EffectiveParameters(flag2 ? GetParameterTypesIncludingReceiver(member) : member.GetParameterTypes(), default(ImmutableArray<RefKind>), -1);
			}
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		ArrayBuilder<RefKind> arrayBuilder = null;
		bool flag3 = argumentRefKinds.Any();
		for (int i = 0; i < argumentCount; i++)
		{
			int num2 = (argToParamMap.IsDefault ? i : argToParamMap[i]);
			if (num2 >= parametersIncludingExtensionParameter.Length)
			{
				continue;
			}
			ParameterSymbol parameterSymbol = parametersIncludingExtensionParameter[num2];
			instance.Add(parameterSymbol.TypeWithAnnotations);
			RefKind argRefKind = (flag3 ? argumentRefKinds[i] : RefKind.None);
			RefKind effectiveParameterRefKind = GetEffectiveParameterRefKind(parameterSymbol, argRefKind, options, binder, ref hasAnyRefOmittedArgument);
			if (arrayBuilder == null)
			{
				if (effectiveParameterRefKind != RefKind.None)
				{
					arrayBuilder = ArrayBuilder<RefKind>.GetInstance(i, RefKind.None);
					arrayBuilder.Add(effectiveParameterRefKind);
				}
			}
			else
			{
				arrayBuilder.Add(effectiveParameterRefKind);
			}
		}
		ImmutableArray<RefKind> refKinds = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<RefKind>);
		return new EffectiveParameters(instance.ToImmutableAndFree(), refKinds, -1);
	}

	private static RefKind GetEffectiveParameterRefKind(ParameterSymbol parameter, RefKind argRefKind, Options options, Binder binder, ref bool hasAnyRefOmittedArgument)
	{
		RefKind refKind = parameter.RefKind;
		if ((options & Options.IsMethodGroupConversion) == 0)
		{
			if (refKind == RefKind.In)
			{
				switch (argRefKind)
				{
				case RefKind.None:
					return RefKind.None;
				case RefKind.Ref:
					if (binder.Compilation.IsFeatureEnabled(MessageID.IDS_FeatureRefReadonlyParameters))
					{
						return RefKind.Ref;
					}
					break;
				}
			}
			else
			{
				bool flag = refKind == RefKind.RefReadOnlyParameter;
				if (flag)
				{
					bool flag2 = ((argRefKind <= RefKind.Ref || argRefKind == RefKind.In) ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					return argRefKind;
				}
			}
		}
		else if (AreRefsCompatibleForMethodConversion(refKind, argRefKind, binder.Compilation))
		{
			return argRefKind;
		}
		if ((options & Options.AllowRefOmittedArguments) != Options.None && refKind == RefKind.Ref && argRefKind == RefKind.None && !binder.InAttributeArgument)
		{
			hasAnyRefOmittedArgument = true;
			return RefKind.None;
		}
		return refKind;
	}

	internal static bool AreRefsCompatibleForMethodConversion(RefKind candidateMethodParameterRefKind, RefKind delegateParameterRefKind, CSharpCompilation compilation)
	{
		if (candidateMethodParameterRefKind == delegateParameterRefKind)
		{
			return true;
		}
		if (candidateMethodParameterRefKind != RefKind.In)
		{
			if (candidateMethodParameterRefKind == RefKind.RefReadOnlyParameter && (delegateParameterRefKind == RefKind.Ref || delegateParameterRefKind == RefKind.In))
			{
				goto IL_001c;
			}
		}
		else if (delegateParameterRefKind == RefKind.RefReadOnlyParameter)
		{
			goto IL_001c;
		}
		bool flag = false;
		goto IL_0022;
		IL_0022:
		if (flag)
		{
			return true;
		}
		if (compilation.IsFeatureEnabled(MessageID.IDS_FeatureRefReadonlyParameters) && candidateMethodParameterRefKind == RefKind.In && delegateParameterRefKind == RefKind.Ref)
		{
			return true;
		}
		return false;
		IL_001c:
		flag = true;
		goto IL_0022;
	}

	private EffectiveParameters GetEffectiveParametersInExpandedForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, Options options) where TMember : Symbol
	{
		bool hasAnyRefOmittedArgument;
		return GetEffectiveParametersInExpandedForm(member, argumentCount, argToParamMap, argumentRefKinds, options, _binder, out hasAnyRefOmittedArgument);
	}

	private static EffectiveParameters GetEffectiveParametersInExpandedForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, Options options, Binder binder, out bool hasAnyRefOmittedArgument) where TMember : Symbol
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance();
		bool flag = false;
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = member.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
		bool flag2 = argumentRefKinds.Any();
		hasAnyRefOmittedArgument = false;
		TypeWithAnnotations elementType = default(TypeWithAnnotations);
		int firstParamsElementIndex = -1;
		for (int i = 0; i < argumentCount; i++)
		{
			int num = (argToParamMap.IsDefault ? i : argToParamMap[i]);
			ParameterSymbol parameterSymbol = parametersIncludingExtensionParameter[num];
			TypeWithAnnotations typeWithAnnotations = parameterSymbol.TypeWithAnnotations;
			if (num == parametersIncludingExtensionParameter.Length - 1)
			{
				if (!elementType.HasType)
				{
					firstParamsElementIndex = instance.Count;
					TryInferParamsCollectionIterationType(binder, typeWithAnnotations.Type, out elementType);
				}
				instance.Add(elementType);
			}
			else
			{
				instance.Add(typeWithAnnotations);
			}
			RefKind argRefKind = (flag2 ? argumentRefKinds[i] : RefKind.None);
			RefKind effectiveParameterRefKind = GetEffectiveParameterRefKind(parameterSymbol, argRefKind, options, binder, ref hasAnyRefOmittedArgument);
			instance2.Add(effectiveParameterRefKind);
			if (effectiveParameterRefKind != RefKind.None)
			{
				flag = true;
			}
		}
		ImmutableArray<RefKind> refKinds = (flag ? instance2.ToImmutable() : default(ImmutableArray<RefKind>));
		instance2.Free();
		return new EffectiveParameters(instance.ToImmutableAndFree(), refKinds, firstParamsElementIndex);
	}

	private MemberResolutionResult<TMember> IsMemberApplicableInNormalForm<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeWithAnnotations> typeArguments, AnalyzedArguments arguments, Options options, bool completeResults, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		ArgumentAnalysisResult argAnalysis = (((options & Options.InferringUniqueMethodGroupSignature) != Options.None) ? ArgumentAnalysisResult.NormalForm(default(ImmutableArray<int>)) : AnalyzeArguments(member, arguments, (options & Options.IsMethodGroupConversion) != 0, expanded: false));
		if (!argAnalysis.IsValid)
		{
			ArgumentAnalysisResultKind kind = argAnalysis.Kind;
			if ((kind != ArgumentAnalysisResultKind.NoCorrespondingParameter && kind - 4 > ArgumentAnalysisResultKind.Expanded) || !completeResults)
			{
				return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis), hasTypeArgumentInferredFromFunctionType: false);
			}
		}
		if ((options & Options.InferringUniqueMethodGroupSignature) == 0 && member.HasUseSiteError)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.UseSiteError(), hasTypeArgumentInferredFromFunctionType: false);
		}
		TMember constructedFrom = GetConstructedFrom(leastOverriddenMember);
		EffectiveParameters effectiveParametersInNormalForm = GetEffectiveParametersInNormalForm(constructedFrom, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, options, _binder, out var hasAnyRefOmittedArgument);
		MemberResolutionResult<TMember> result = IsApplicable(member, constructedFrom, typeArguments, arguments, options, effectiveParametersInNormalForm, default(TypeWithAnnotations), isExpanded: false, argAnalysis.ArgsToParamsOpt, hasAnyRefOmittedArgument, (options & Options.InferWithDynamic) != 0, completeResults, (options & Options.DynamicConvertsToAnything) != 0, (options & Options.IsMethodGroupConversion) != 0, ref useSiteInfo);
		if (completeResults && !argAnalysis.IsValid)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis), hasTypeArgumentInferredFromFunctionType: false);
		}
		return result;
	}

	private MemberResolutionResult<TMember> IsMemberApplicableInExpandedForm<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeWithAnnotations> typeArguments, AnalyzedArguments arguments, TypeWithAnnotations definitionParamsElementType, Options options, bool completeResults, bool dynamicConvertsToAnything, bool isMethodGroupConversion, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		ArgumentAnalysisResult argAnalysis = (((options & Options.InferringUniqueMethodGroupSignature) != Options.None) ? ArgumentAnalysisResult.ExpandedForm(default(ImmutableArray<int>)) : AnalyzeArguments(member, arguments, isMethodGroupConversion: false, expanded: true));
		if (!argAnalysis.IsValid)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis), hasTypeArgumentInferredFromFunctionType: false);
		}
		if ((options & Options.InferringUniqueMethodGroupSignature) == 0 && member.HasUseSiteError)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.UseSiteError(), hasTypeArgumentInferredFromFunctionType: false);
		}
		TMember constructedFrom = GetConstructedFrom(leastOverriddenMember);
		EffectiveParameters effectiveParametersInExpandedForm = GetEffectiveParametersInExpandedForm(constructedFrom, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, options & Options.AllowRefOmittedArguments, _binder, out var hasAnyRefOmittedArgument);
		return IsApplicable(member, constructedFrom, typeArguments, arguments, options, effectiveParametersInExpandedForm, definitionParamsElementType, isExpanded: true, argAnalysis.ArgsToParamsOpt, hasAnyRefOmittedArgument, inferWithDynamic: false, completeResults, dynamicConvertsToAnything, isMethodGroupConversion, ref useSiteInfo);
	}

	private MemberResolutionResult<TMember> IsApplicable<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeWithAnnotations> typeArgumentsBuilder, AnalyzedArguments arguments, Options options, EffectiveParameters constructedFromEffectiveParameters, TypeWithAnnotations definitionParamsElementTypeOpt, bool isExpanded, ImmutableArray<int> argsToParamsMap, bool hasAnyRefOmittedArgument, bool inferWithDynamic, bool completeResults, bool dynamicConvertsToAnything, bool isMethodGroupConversion, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		bool hasTypeArgumentsInferredFromFunctionType = false;
		bool ignoreOpenTypes;
		EffectiveParameters parameters;
		if ((options & Options.InferringUniqueMethodGroupSignature) == 0 && member.GetMemberArityIncludingExtension() > 0)
		{
			bool flag = member.IsExtensionBlockMember();
			ImmutableArray<TypeWithAnnotations> immutableArray;
			if (typeArgumentsBuilder.Count == 0 && arguments.HasDynamicArgument && !inferWithDynamic)
			{
				ignoreOpenTypes = true;
				immutableArray = getAllTypeArguments(member, flag);
			}
			else
			{
				if (typeArgumentsBuilder.Count > 0)
				{
					immutableArray = typeArgumentsBuilder.ToImmutable();
				}
				else
				{
					ImmutableArray<TypeParameterSymbol> typeParametersIncludingExtension = leastOverriddenMember.GetTypeParametersIncludingExtension();
					immutableArray = InferMethodTypeArguments(member, typeParametersIncludingExtension, arguments, constructedFromEffectiveParameters, out hasTypeArgumentsInferredFromFunctionType, out var error, ref useSiteInfo);
					if (immutableArray.IsDefault)
					{
						return new MemberResolutionResult<TMember>(member, leastOverriddenMember, error, hasTypeArgumentInferredFromFunctionType: false);
					}
				}
				member = member.ConstructIncludingExtension(immutableArray);
				leastOverriddenMember = GetConstructedFrom(leastOverriddenMember).ConstructIncludingExtension(immutableArray);
				ImmutableArray<TypeWithAnnotations> immutableArray2 = (flag ? GetParameterTypesIncludingReceiver(leastOverriddenMember) : leastOverriddenMember.GetParameterTypes());
				for (int i = 0; i < immutableArray2.Length; i++)
				{
					if (!immutableArray2[i].Type.CheckAllConstraints(Compilation, Conversions))
					{
						return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ConstructedParameterFailedConstraintsCheck(i), hasTypeArgumentsInferredFromFunctionType);
					}
				}
				ignoreOpenTypes = false;
			}
			TypeMap typeMap = new TypeMap((flag ? leastOverriddenMember.OriginalDefinition : leastOverriddenMember).GetTypeParametersIncludingExtension(), immutableArray, allowAlpha: true);
			parameters = new EffectiveParameters(typeMap.SubstituteTypes(constructedFromEffectiveParameters.ParameterTypes), constructedFromEffectiveParameters.ParameterRefKinds, constructedFromEffectiveParameters.FirstParamsElementIndex);
		}
		else
		{
			parameters = constructedFromEffectiveParameters;
			ignoreOpenTypes = false;
		}
		MemberAnalysisResult result = IsApplicable(member, parameters, definitionParamsElementTypeOpt, isExpanded, arguments, argsToParamsMap, member.GetIsVararg(), hasAnyRefOmittedArgument, ignoreOpenTypes, completeResults, dynamicConvertsToAnything, isMethodGroupConversion, ref useSiteInfo);
		return new MemberResolutionResult<TMember>(member, leastOverriddenMember, result, hasTypeArgumentsInferredFromFunctionType);
		static ImmutableArray<TypeWithAnnotations> getAllTypeArguments(TMember val, bool isExtensionBlockMember)
		{
			if (val is MethodSymbol methodSymbol)
			{
				if (!isExtensionBlockMember)
				{
					return methodSymbol.TypeArgumentsWithAnnotations;
				}
				return methodSymbol.ContainingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Concat(methodSymbol.TypeArgumentsWithAnnotations);
			}
			if (val is PropertySymbol propertySymbol)
			{
				return propertySymbol.ContainingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
			}
			throw ExceptionUtilities.UnexpectedValue(val);
		}
	}

	private ImmutableArray<TypeWithAnnotations> InferMethodTypeArguments<TMember>(TMember member, ImmutableArray<TypeParameterSymbol> originalTypeParameters, AnalyzedArguments arguments, EffectiveParameters originalEffectiveParameters, out bool hasTypeArgumentsInferredFromFunctionType, out MemberAnalysisResult error, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo) where TMember : Symbol
	{
		ImmutableArray<BoundExpression> arguments2 = arguments.Arguments.ToImmutable();
		Dictionary<TypeParameterSymbol, int> ordinals = member.MakeAdjustedTypeParameterOrdinalsIfNeeded(originalTypeParameters);
		MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, _binder.Conversions, originalTypeParameters, member.ContainingType, originalEffectiveParameters.ParameterTypes, originalEffectiveParameters.ParameterRefKinds, arguments2, ref useSiteInfo, null, ordinals);
		if (methodTypeInferenceResult.Success)
		{
			hasTypeArgumentsInferredFromFunctionType = methodTypeInferenceResult.HasTypeArgumentInferredFromFunctionType;
			error = default(MemberAnalysisResult);
			return methodTypeInferenceResult.InferredTypeArguments;
		}
		if (arguments.IncludesReceiverAsArgument)
		{
			bool flag;
			if (member.IsExtensionBlockMember())
			{
				if (member.ContainingType.Arity > 0)
				{
					ImmutableArray<TypeWithAnnotations> immutableArray = MethodTypeInferrer.InferTypeArgumentsFromReceiverType(member.ContainingType, arguments2[0], _binder.Compilation, _binder.Conversions, ref useSiteInfo);
					flag = !immutableArray.IsDefault && !immutableArray.Any((TypeWithAnnotations t) => !t.HasType);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = MethodTypeInferrer.CanInferTypeArgumentsFromFirstArgument(_binder.Compilation, _binder.Conversions, (MethodSymbol)(object)member, arguments2, ref useSiteInfo, out var _);
			}
			if (!flag)
			{
				hasTypeArgumentsInferredFromFunctionType = false;
				error = MemberAnalysisResult.TypeInferenceExtensionInstanceArgumentFailed();
				return default(ImmutableArray<TypeWithAnnotations>);
			}
		}
		hasTypeArgumentsInferredFromFunctionType = false;
		error = MemberAnalysisResult.TypeInferenceFailed();
		return default(ImmutableArray<TypeWithAnnotations>);
	}

	private MemberAnalysisResult IsApplicable(Symbol candidate, EffectiveParameters parameters, TypeWithAnnotations definitionParamsElementTypeOpt, bool isExpanded, AnalyzedArguments arguments, ImmutableArray<int> argsToParameters, bool isVararg, bool hasAnyRefOmittedArgument, bool ignoreOpenTypes, bool completeResults, bool dynamicConvertsToAnything, bool isMethodGroupConversion, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeWithAnnotations typeWithAnnotations = ((!isExpanded) ? default(TypeWithAnnotations) : ((parameters.FirstParamsElementIndex != -1) ? parameters.ParameterTypes[parameters.FirstParamsElementIndex] : TypeWithAnnotations.Create(ErrorTypeSymbol.EmptyParamsCollectionElementTypeSentinel)));
		int num = parameters.ParameterTypes.Length + (isVararg ? 1 : 0);
		if (arguments.Arguments.Count < num)
		{
			num = arguments.Arguments.Count;
		}
		ArrayBuilder<Conversion> arrayBuilder = null;
		BitVector badArguments = default(BitVector);
		for (int i = 0; i < num; i++)
		{
			BoundExpression boundExpression = arguments.Argument(i);
			Conversion conversion;
			if (isVararg && i == num - 1)
			{
				if (boundExpression.Kind == BoundKind.ArgListOperator)
				{
					conversion = Conversion.Identity;
				}
				else
				{
					if (badArguments.IsNull)
					{
						badArguments = BitVector.Create(i + 1);
					}
					badArguments[i] = true;
					conversion = Conversion.NoConversion;
				}
			}
			else
			{
				RefKind refKind = arguments.RefKind(i);
				RefKind refKind2 = ((!parameters.ParameterRefKinds.IsDefault) ? parameters.ParameterRefKinds[i] : RefKind.None);
				bool flag = arguments.IsExtensionMethodReceiverArgument(i);
				if (flag && refKind == RefKind.None && refKind2 == RefKind.Ref)
				{
					refKind = refKind2;
				}
				bool hasInterpolatedStringRefMismatch = false;
				bool flag2 = !_binder.InParameterDefaultValue && !_binder.InAttributeArgument;
				if (flag2)
				{
					bool flag3 = ((boundExpression is BoundUnconvertedInterpolatedString || boundExpression is BoundBinaryOperator { IsUnconvertedInterpolatedStringAddition: not false }) ? true : false);
					flag2 = flag3;
				}
				if (flag2 && refKind2 == RefKind.Ref && parameters.ParameterTypes[i].Type is NamedTypeSymbol { IsInterpolatedStringHandlerType: not false, IsValueType: not false })
				{
					hasInterpolatedStringRefMismatch = true;
					refKind = refKind2;
				}
				conversion = CheckArgumentForApplicability(candidate, boundExpression, refKind, parameters.ParameterTypes[i].Type, refKind2, ignoreOpenTypes, ref useSiteInfo, flag, hasInterpolatedStringRefMismatch, dynamicConvertsToAnything, isMethodGroupConversion);
				if (flag && !conversion.IsDynamic && !ConversionsBase.IsValidExtensionMethodThisArgConversion(conversion))
				{
					return MemberAnalysisResult.BadArgumentConversions(argsToParameters, MemberAnalysisResult.CreateBadArgumentsWithPosition(i), ImmutableArray.Create(conversion), definitionParamsElementTypeOpt, typeWithAnnotations);
				}
				if (!conversion.Exists)
				{
					if (badArguments.IsNull)
					{
						badArguments = BitVector.Create(i + 1);
					}
					badArguments[i] = true;
				}
			}
			if (arrayBuilder != null)
			{
				arrayBuilder.Add(conversion);
			}
			else if (!conversion.IsIdentity)
			{
				arrayBuilder = ArrayBuilder<Conversion>.GetInstance(num);
				arrayBuilder.AddMany(Conversion.Identity, i);
				arrayBuilder.Add(conversion);
			}
			if (!badArguments.IsNull && !completeResults)
			{
				break;
			}
		}
		ImmutableArray<Conversion> conversions = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<Conversion>);
		if (!badArguments.IsNull)
		{
			return MemberAnalysisResult.BadArgumentConversions(argsToParameters, badArguments, conversions, definitionParamsElementTypeOpt, typeWithAnnotations);
		}
		if (isExpanded)
		{
			return MemberAnalysisResult.ExpandedForm(argsToParameters, conversions, hasAnyRefOmittedArgument, definitionParamsElementTypeOpt, typeWithAnnotations);
		}
		return MemberAnalysisResult.NormalForm(argsToParameters, conversions, hasAnyRefOmittedArgument);
	}

	private Conversion CheckArgumentForApplicability(Symbol candidate, BoundExpression argument, RefKind argRefKind, TypeSymbol parameterType, RefKind parRefKind, bool ignoreOpenTypes, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forExtensionMethodThisArg, bool hasInterpolatedStringRefMismatch, bool dynamicConvertsToAnything, bool isMethodGroupConversion)
	{
		if (argRefKind != parRefKind)
		{
			return Conversion.NoConversion;
		}
		if (ignoreOpenTypes && TypeContainsTypeParameterFromContainer(candidate, parameterType))
		{
			return Conversion.ImplicitDynamic;
		}
		TypeSymbol type = argument.Type;
		if (argument.Kind == BoundKind.OutVariablePendingInference || argument.Kind == BoundKind.OutDeconstructVarPendingInference || (argument.Kind == BoundKind.DiscardExpression && (object)type == null))
		{
			return Conversion.Identity;
		}
		if ((argRefKind == RefKind.None) | hasInterpolatedStringRefMismatch)
		{
			Conversion result = (forExtensionMethodThisArg ? Conversions.ClassifyImplicitExtensionMethodThisArgConversion(argument, argument.Type, parameterType, ref useSiteInfo, isMethodGroupConversion) : ((!dynamicConvertsToAnything || !argument.Type.IsDynamic()) ? Conversions.ClassifyImplicitConversionFromExpression(argument, parameterType, ref useSiteInfo) : Conversion.ImplicitDynamic));
			if (hasInterpolatedStringRefMismatch && !result.IsInterpolatedStringHandler)
			{
				return Conversion.NoConversion;
			}
			return result;
		}
		if ((object)type != null && ConversionsBase.HasIdentityConversion(type, parameterType))
		{
			return Conversion.Identity;
		}
		return Conversion.NoConversion;
	}

	private static bool TypeContainsTypeParameterFromContainer(Symbol container, TypeSymbol parameterType)
	{
		if (parameterType.ContainsTypeParameter(container))
		{
			return true;
		}
		if (container.IsExtensionBlockMember())
		{
			return parameterType.ContainsTypeParameter(container.ContainingType);
		}
		return false;
	}

	private static TMember GetConstructedFrom<TMember>(TMember member) where TMember : Symbol
	{
		return member.Kind switch
		{
			SymbolKind.Property => member, 
			SymbolKind.Method => (TMember)(Symbol)(member as MethodSymbol).ConstructedFrom, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	private static ImmutableArray<TypeWithAnnotations> GetParameterTypesIncludingReceiver(Symbol symbol)
	{
		TypeWithAnnotations typeWithAnnotations = symbol.ContainingType.ExtensionParameter.TypeWithAnnotations;
		ImmutableArray<TypeWithAnnotations> parameterTypes = symbol.GetParameterTypes();
		int num = 0;
		TypeWithAnnotations[] array = new TypeWithAnnotations[1 + parameterTypes.Length];
		array[num] = typeWithAnnotations;
		num++;
		ReadOnlySpan<TypeWithAnnotations> readOnlySpan = parameterTypes.AsSpan();
		readOnlySpan.CopyTo(new Span<TypeWithAnnotations>(array).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	private static ArgumentAnalysisResult AnalyzeArguments(Symbol symbol, AnalyzedArguments arguments, bool isMethodGroupConversion, bool expanded)
	{
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = symbol.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
		bool isVararg = symbol.GetIsVararg();
		if (!expanded && arguments.Names.Count == 0)
		{
			return AnalyzeArgumentsForNormalFormNoNamedArguments(parametersIncludingExtensionParameter, arguments, isMethodGroupConversion, isVararg);
		}
		int count = arguments.Arguments.Count;
		int[] array = null;
		int? num = null;
		bool? flag = null;
		bool seenNamedParams = false;
		bool seenOutOfPositionNamedArgument = false;
		for (int i = 0; i < count; i++)
		{
			int num2 = CorrespondsToAnyParameter(parametersIncludingExtensionParameter, expanded, arguments, i, isVararg, out var isNamedArgument, ref seenNamedParams, ref seenOutOfPositionNamedArgument) ?? (-1);
			if (num2 == -1 && !num.HasValue)
			{
				num = i;
				flag = isNamedArgument;
			}
			if (num2 != i && array == null)
			{
				array = new int[count];
				for (int j = 0; j < i; j++)
				{
					array[j] = j;
				}
			}
			if (array != null)
			{
				array[i] = num2;
			}
		}
		ParameterMap argsToParameters = new ParameterMap(array, count);
		int? num3 = CheckForBadNonTrailingNamedArgument(arguments, argsToParameters);
		if (num3.HasValue)
		{
			return ArgumentAnalysisResult.BadNonTrailingNamedArgument(num3.Value);
		}
		if (num.HasValue)
		{
			if (flag.Value)
			{
				return ArgumentAnalysisResult.NoCorrespondingNamedParameter(num.Value);
			}
			return ArgumentAnalysisResult.NoCorrespondingParameter(num.Value);
		}
		int? num4 = NameUsedForPositional(arguments, argsToParameters);
		if (num4.HasValue)
		{
			return ArgumentAnalysisResult.NameUsedForPositional(num4.Value);
		}
		int? num5 = CheckForMissingRequiredParameter(argsToParameters, parametersIncludingExtensionParameter, isMethodGroupConversion, expanded);
		if (num5.HasValue)
		{
			return ArgumentAnalysisResult.RequiredParameterMissing(num5.Value);
		}
		if ((arguments.Names.Any() && arguments.Names.Last().HasValue) & isVararg)
		{
			return ArgumentAnalysisResult.RequiredParameterMissing(parametersIncludingExtensionParameter.Length);
		}
		int? num6 = CheckForDuplicateNamedArgument(arguments);
		if (num6.HasValue)
		{
			return ArgumentAnalysisResult.DuplicateNamedArgument(num6.Value);
		}
		if (!expanded)
		{
			return ArgumentAnalysisResult.NormalForm(argsToParameters.ToImmutableArray());
		}
		return ArgumentAnalysisResult.ExpandedForm(argsToParameters.ToImmutableArray());
	}

	private static int? CheckForBadNonTrailingNamedArgument(AnalyzedArguments arguments, ParameterMap argsToParameters)
	{
		if (argsToParameters.IsTrivial)
		{
			return null;
		}
		int num = -1;
		int count = arguments.Arguments.Count;
		for (int i = 0; i < count; i++)
		{
			int num2 = argsToParameters[i];
			if (num2 != -1 && num2 != i && arguments.Name(i) != null)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			for (int j = num + 1; j < count; j++)
			{
				if (arguments.Name(j) == null)
				{
					return num;
				}
			}
		}
		return null;
	}

	private static int? CorrespondsToAnyParameter(ImmutableArray<ParameterSymbol> memberParameters, bool expanded, AnalyzedArguments arguments, int argumentPosition, bool isVararg, out bool isNamedArgument, ref bool seenNamedParams, ref bool seenOutOfPositionNamedArgument)
	{
		isNamedArgument = arguments.Names.Count > argumentPosition && arguments.Names[argumentPosition].HasValue;
		if (!isNamedArgument)
		{
			if (seenNamedParams)
			{
				return null;
			}
			if (seenOutOfPositionNamedArgument)
			{
				return null;
			}
			int num = memberParameters.Length + (isVararg ? 1 : 0);
			if (argumentPosition >= num)
			{
				if (!expanded)
				{
					return null;
				}
				return num - 1;
			}
			return argumentPosition;
		}
		string item = arguments.Names[argumentPosition].GetValueOrDefault().Name;
		for (int i = 0; i < memberParameters.Length; i++)
		{
			if (memberParameters[i].Name == item)
			{
				if (expanded && i == memberParameters.Length - 1)
				{
					seenNamedParams = true;
				}
				if (i != argumentPosition)
				{
					seenOutOfPositionNamedArgument = true;
				}
				return i;
			}
		}
		return null;
	}

	private static ArgumentAnalysisResult AnalyzeArgumentsForNormalFormNoNamedArguments(ImmutableArray<ParameterSymbol> parameters, AnalyzedArguments arguments, bool isMethodGroupConversion, bool isVararg)
	{
		int num = parameters.Length + (isVararg ? 1 : 0);
		int count = arguments.Arguments.Count;
		if (count < num)
		{
			for (int i = count; i < num; i++)
			{
				if (parameters.Length == i || !CanBeOptional(parameters[i], isMethodGroupConversion))
				{
					return ArgumentAnalysisResult.RequiredParameterMissing(i);
				}
			}
		}
		else if (num < count)
		{
			return ArgumentAnalysisResult.NoCorrespondingParameter(num);
		}
		return ArgumentAnalysisResult.NormalForm(default(ImmutableArray<int>));
	}

	private static bool CanBeOptional(ParameterSymbol parameter, bool isMethodGroupConversion)
	{
		if (!isMethodGroupConversion)
		{
			return parameter.IsOptional;
		}
		return false;
	}

	private static int? NameUsedForPositional(AnalyzedArguments arguments, ParameterMap argsToParameters)
	{
		if (argsToParameters.IsTrivial)
		{
			return null;
		}
		for (int i = 0; i < argsToParameters.Length; i++)
		{
			if (arguments.Name(i) == null)
			{
				continue;
			}
			for (int j = 0; j < i; j++)
			{
				if (arguments.Name(j) == null && argsToParameters[i] == argsToParameters[j])
				{
					return i;
				}
			}
		}
		return null;
	}

	private static int? CheckForMissingRequiredParameter(ParameterMap argsToParameters, ImmutableArray<ParameterSymbol> parameters, bool isMethodGroupConversion, bool expanded)
	{
		int num = (expanded ? (parameters.Length - 1) : parameters.Length);
		if (argsToParameters.IsTrivial && num <= argsToParameters.Length)
		{
			return null;
		}
		for (int i = 0; i < num; i++)
		{
			if (CanBeOptional(parameters[i], isMethodGroupConversion))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < argsToParameters.Length; j++)
			{
				flag = argsToParameters[j] == i;
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return null;
	}

	private static int? CheckForDuplicateNamedArgument(AnalyzedArguments arguments)
	{
		if (arguments.Names.IsEmpty)
		{
			return null;
		}
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		for (int i = 0; i < arguments.Names.Count; i++)
		{
			string text = arguments.Name(i);
			if (text != null && !instance.Add(text))
			{
				instance.Free();
				return i;
			}
		}
		instance.Free();
		return null;
	}
}
