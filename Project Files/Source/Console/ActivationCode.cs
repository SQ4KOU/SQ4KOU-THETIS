//  ActivationCode.cs
//
//  Copyright (C) 2026 Daniel Poznanski, 3Z9AM
//
//  This program is free software; you can redistribute it and/or
//  modify it under the terms of the GNU General Public License
//  as published by the Free Software Foundation; either version 2
//  of the License, or (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

namespace Thetis
{
    [System.Flags]
    internal enum ActivationFeature
    {
        None = 0,
        SectionDiversity = 1,
        SoftEdges = 2,
    }

    // This GPL build enables every previously activation-gated feature.
    // Keep this compatibility API: DSP and UI call ActivationCode.Has(...).
    internal static class ActivationCode
    {
        private const ActivationFeature All = ActivationFeature.SectionDiversity | ActivationFeature.SoftEdges;

        public static ActivationFeature Unlocked { get; set; } = All;
        public static bool Has(ActivationFeature features) => (Unlocked & features) == features;
    }
}
