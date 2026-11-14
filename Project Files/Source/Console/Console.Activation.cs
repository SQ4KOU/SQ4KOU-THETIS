//  Console.Activation.cs
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
    public partial class Console
    {
        // Activation is not required in this source build.
        // All previously restricted DSP features remain available.
        private void loadActivationFeatures()
        {
            ActivationCode.Unlocked = ActivationFeature.SectionDiversity | ActivationFeature.SoftEdges;
        }

        // Called before POWER is switched on; never block the radio.
        private bool ensureActivated()
        {
            loadActivationFeatures();
            return true;
        }

        // No activation prompt or obsolete activation menu.
        private void initActivationMenu() { }
    }
}
