#!/usr/bin/python2
# encoding: utf-8

import math

class TechLevel(object):
    def __init__(self, tech, maxThrust, isp, vac, maxIgnitions, mass, cost, burnTime, minThrust=None):
        self.tech = tech
        self.maxThrust = float(maxThrust)
        if minThrust is None:
            minThrust = maxThrust / 4.0
        self.minThrust = float(minThrust)
        self.isp = isp
        self.vac = vac
        self.maxIgnitions = maxIgnitions
        self.mass = mass # value is for maxed-out thrust and ignitions
        self.cost = cost # ditto
        self.burnTime = burnTime
class EngineClass(object):
    def __init__(self, letter, propellants, **flags):
        self.letter = letter
        self.propellants = propellants
        self.tls = {}
        self.flags = flags
    def setTL(self, tl, tech, maxThrust, isp, vac, maxIgnitions, mass, cost, burnTime, minThrust=None):
        assert tl not in self.tls, tl
        self.tls[tl] = TechLevel(tech, maxThrust, isp, vac, maxIgnitions, mass, cost, burnTime, minThrust)
    def calc(self, tl, thrust, ignitions):
        assert tl in self.tls, self.tls.keys()
        TL = self.tls[tl]
        assert thrust <= TL.maxThrust, TL.maxThrust
        assert thrust >= TL.minThrust, TL.minThrust
        assert ignitions <= TL.maxIgnitions
        tf = thrust / TL.maxThrust
        mf = math.pow(tf, 0.8)
        mass = TL.mass * mf
        igf = (ignitions + 1) / (TL.maxIgnitions + 1.0)
        cf = math.pow(tf, 1.2) * math.pow(igf, 0.2)
        cost = TL.cost * cf
        #sf = math.pow(tf, 1/3.0)
        return (mass, cost)
    def upgrade(self, fromtl, totl, fromthrust, ignitions):
        fromTL = self.tls[fromtl]
        toTL = self.tls[totl]
        tf = fromthrust / fromTL.maxThrust
        ni = fromTL.maxIgnitions - ignitions
        tothrust = toTL.maxThrust * tf
        return tothrust, self.calc(totl, tothrust, toTL.maxIgnitions - ni)

### The numbers used below for the various TLs are designed to match the evolution of specific engine families.
### This sometimes makes for slightly odd trajectories, particularly the up-and-down thrust of the AJ10s.
### Before actually turning these into game configs, probably the numbers should be smoothed and homogenised.

Aggregat = EngineClass('A', {'Ethanol75': 0.5125, 'LqdOxygen': 0.4875}) # let's not faff about with HTP
Aggregat.setTL(0, 'unlockParts', 400, 203, 239, 0, 1.14, 200, 70)
Aggregat.setTL(1, 'rocketryTesting', 360, 220, 255, 0, 1.12, 900, 115) # really should be Hydyne/LOx, but who cares
Aggregat.setTL(2, 'basicRocketryRP0', 500, 216, 249, 0, 0.8, 540, 145) # NAA-75-110
Aggregat.setTL(3, 'orbitalRocketry1956', 525, 235, 265, 0, 0.8, 800, 155) # really should be Hydyne/LOx, but who cares
print Aggregat.calc(0, 311.8, 0) # A-4: .931, 150
print Aggregat.calc(1, 288.68, 0) # A-9: .931, 700
print Aggregat.calc(2, 395.9, 0) # Redstone A-6: .67, 400
print Aggregat.calc(3, 416.2, 0) # Redstone A-7: .67, 600
print
for i in xrange(4):
    print Aggregat.upgrade(0, i, 100, 0)
print

Bee = EngineClass('B', {'Aniline': 0.326832, 'Furfuryl': 0.081708, 'IRFNA-III': 0.59146}, nogimbal=True) # strictly the propellants should vary per TL, who cares
Bee.setTL(0, 'unlockParts', 10, 191, 218.36, 1, 0.01, 42, 50)
Bee.setTL(1, 'rocketryTesting', 18, 200, 235.44, 1, 0.013, 54, 65)
Bee.setTL(2, 'basicRocketryRP0', 28, 198, 231, 1, 0.015, 63, 70)
print Bee.calc(0, 7.628, 1) # WAC-Corporal: .008, 30
print Bee.calc(1, 13.7628, 1) # XASR-1: .0104, 40
print Bee.calc(2, 21.28, 1) # AJ10-27: .012, 45
print
for i in xrange(3):
    print Bee.upgrade(0, i, 2.5, 0)
print

Delta = EngineClass('D', {'UDMH': 0.4281, 'IRFNA-III': 0.5719}) # again, the propellants should vary per TL but who cares
Delta.setTL(0, 'orbitalRocketry1956', 42, 240, 271, 1, 0.1, 200, 115)
Delta.setTL(1, 'orbitalRocketry1958', 41, 238, 267, 1, 0.095, 176, 150)
Delta.setTL(2, 'orbitalRocketry1959', 41.5, 240, 270, 1, 0.095, 196, 150)
Delta.setTL(3, 'orbitalRocketry1960', 44, 240, 270, 1, 0.098, 204, 150)
Delta.setTL(4, 'orbitalRocketry1961', 45, 215, 278, 4, 0.11, 336, 300)
Delta.setTL(5, 'orbitalRocketry1964', 45.5, 100, 311, 4, 0.134, 405, None)
Delta.setTL(6, 'orbitalRocketry1972', 54, 215, 315, 16, 0.121, 472, None)
Delta.setTL(7, 'orbitalRocketry1986', 56, 215, 319.2, 16, 0.122, 540, None)
print Delta.calc(0, 33.8, 1) # AJ10-37: .084, 150
print Delta.calc(1, 33.0, 1) # AJ10-42: .08, 135
print Delta.calc(2, 33.4, 1) # AJ10-101A: .08, 151
print Delta.calc(3, 34.25, 1) # AJ10-142: .08, 151
print Delta.calc(4, 35.1, 4) # AJ10-104: .09, 250
print Delta.calc(5, 35.585, 4) # AJ10-138: .11, 300
print Delta.calc(6, 42.3, 16) # AJ10-118F: .1, 350
print Delta.calc(7, 43.7, 16) # AJ10-118K: .1, 400
print
for i in xrange(8):
    print Delta.upgrade(0, i, 10.5, 1)
print

Kerolox = EngineClass('K', {'Kerosene': 0.3929, 'LqdOxygen': 0.6071})
Kerolox.setTL(0, 'orbitalRocketry1956', 800, 246, 280, 0, 1.14, 424, 165)
Kerolox.setTL(1, 'orbitalRocketry1958', 1000, 248, 282, 0, 1.132, 420, 165)
Kerolox.setTL(2, 'orbitalRocketry1959', 1024, 245, 284, 0, 1.168, 462, 165)
Kerolox.setTL(3, 'orbitalRocketry1960', 1125, 248.3, 286.2, 0, 1.225, 480, 165)
Kerolox.setTL(4, 'orbitalRocketry1961', 1200, 245, 289, 0, 0.77, 268, 150)
Kerolox.setTL(5, 'orbitalRocketry1965', 1280, 265, 296, 0, 1.175, 275, 180)
Kerolox.setTL(6, 'orbitalRocketry1972', 1275, 264, 295, 0, 1.225, 260, 240)
Kerolox.setTL(7, 'orbitalRocketry1986', 1320, 255, 302, 0, 1.306, 328, 285)
Kerolox.setTL(8, 'orbitalRocketry2004', 640, 267, 304.8, 1, 0.79, 260, 300)
Kerolox.setTL(9, 'orbitalRocketry2009', 960, 282, 311, 4, 0.59, 248, 350)
Kerolox.setTL(10,'orbitalRocketry2014', 1200, 288.5, 311, 4, 0.584, 244, 350)
print Kerolox.calc(0, 600.5, 0) # S-3: .907, 300
print Kerolox.calc(1, 758.7, 0) # S-3D: .907, 301
print Kerolox.calc(2, 774, 0) # LR79-NA-9: .93421, 330
print Kerolox.calc(3, 850, 0) # LR79-NA-11: .97956, 340
print Kerolox.calc(4, 947, 0) # H-1-SaturnI: .6353, 200
print Kerolox.calc(5, 1030.2, 0) # H-1-SaturnIB: .988, 210
print Kerolox.calc(6, 1027, 0) # RS-27: 1.027, 201
print Kerolox.calc(7, 1054, 0) # RS-27A: 1.091, 250
print Kerolox.calc(8, 482.63, 1) # Merlin1C: .63, 185
print Kerolox.calc(9, 722, 4) # Merlin1D: .47, 175
print Kerolox.calc(10, 914.12, 4) # Merlin1D++: .47, 175
print
for i in xrange(11):
    print Kerolox.upgrade(0, i, 200, 0)
print
