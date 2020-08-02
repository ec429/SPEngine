Simple Procedural Engines
=========================

SPEngine is a mod for the Squad game "Kerbal Space Program".
Like the name implies, it allows the player to design procedurally-created
 engines, using a simple and understandable model.

SPEngine is currently in beta; the engine configs aren't all complete, though
 we're only missing a couple and the basic functionality works.

SPEngine is developed by Edward Cree, aka 'soundnfury'.

SPEngine is licensed under the MIT License.  Its source code can be found at
 <https://github.com/ec429/SPEngine>.

How does it work?
-----------------

There are a bunch of "Families", like A-class and B-class (Aggregats and Bees,
 respectively).  Each Family has a series of TechLevels, with a techRequired,
 a maxThrust, a maxIgnitions, numbers for Isps, mass, and cost, propellants,
 ignitor_resources, minThrottle, flags for Ullage and Pressure-fed, an
 entryCost (possibly including some ECMs), and a toolCost.
You first have to pay a TechLevel's entryCost to unlock it (and you have to
 unlock them in order).  When you create a Design within a given family, you
 set its Thrust and Ignitions, as well as choosing a TechLevel you've
 unlocked.  These scale the mass and cost:
    mass *= pow(Thrust/maxThrust, 0.8);
    cost *= pow(Thrust/maxThrust, 1.2)
          * pow((Ignitions+1)/(maxIgnitions+1), 0.2);
 The toolCost is also scaled like the cost.
When untooled, a Design's cost is multiplied by 1,000 (which is basically
 infinity).  Untooled designs are for experimenting with stuff in the VAB.
 When you actually want something you can launch, you tool it.
Later, you develop some new tech, and you want to use the new TechLevel.
 After unlocking it (paying its entryCost), you _could_ just create a new
 Design from scratch.  But that's expensive, because you have to pay the
 toolCost all over again.
If you instead Upgrade an existing design, the Thrust scales linearly with the
 maxThrust (i.e. if the TechLevels were 100kN and 120kN, and your old Design
 was 50kN, the upgraded Design will be 60kN), the Ignitions are additive (if
 the TechLevels had 3 and 5 ignitions, your old design had 2, you get 4), and
 the toolCost for the new Design is reduced by 80% of the vanilla toolCost of
 the old Design.
So it's much cheaper to make a series of engines following the defined upgrade
 path, than to make a whole new engine each time you get new tech.

In a race, developing the right order of engine designs might be the most
 important thing to separate you.  As an example, if you want your very early
 SRs to build faster for that Kármán line first, you can make smaller Bees
 (cheaper, so fewer BP), but then you have a smaller AJ10-27 for your upper
 stages, so you have to have more in the cluster on your first orbital shot,
 and Agathorn eats you.

There are a few parts with a 'chamber multiplier' (8-chamber X-class based on
 Gamma 8, twin-chamber W-class based on Gamma 2, twin-chamber T-class based on
 LR87).  These multiply the thrust, mass and cost by the number of chambers in
 the part, but unfortunately do not yet alter their reliability to account for
 the clustering, so are not properly balanced.  In all other respects they are
 equivalent to a cluster of the corresponding single-chamber parts, and exist
 only for aesthetic purposes.

What are its dependencies?
--------------------------

The plugin depends directly on RealFuels.  The part configs generally work by
 cloning engines from Realism Overhaul; in some cases additional part mods are
 needed:
 * A-class (A-4): Taerobee
 * D-class (AJ10): SXT
 * E-class (LR91) alternate model: FASA
 * G-class (Agena): VenStockRevamp
 * H-class (SSME): RealEngines
 * J-class (J-2): SXT
 * K-class alternate model (H-1): FASA
 * M-class (M-1): FASA
 * N-class (NK-15): RealEngines
 * O-class (RD-0210): RealEngines
 * P-class (Proton): RealEngines
 * X-class (Gamma) alternate model: RealEngines
 * Y-class (NK-15V): RealEngines
Alternatively, ROEngines should provide a complete set.
Also, TestLite is recommended (but not required), as increased burn-time
 limits and improved reliability are often the biggest upgrades from one tech
 level to the next.  Alternatively, TestFlight is also supported.
And of course you need ModuleManager.

How do I install it?
--------------------

Assuming you've installed all the dependencies (see above), just copy the
 GameData/SPEngine folder into your KSP install's GameData/.

Where do I report bugs?
-----------------------

Open an issue on the project's GitHub page,
	https://github.com/ec429/SPEngine/issues
Sometimes the developer will be on IRC as soundnfury on irc.esper.net, or on
 the KSP-RO Discord server under the same nickname.
