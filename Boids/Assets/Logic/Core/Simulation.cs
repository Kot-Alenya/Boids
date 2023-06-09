﻿using BoidSimulation.Data;
using UnityEngine.Jobs;
using Unity.Jobs;
using System;

namespace BoidSimulation
{
    public class Simulation : IDisposable
    {
        private SimulationLoop _simulationLoop;
        private SimulationData _simulationData;
        private BoidsRenderer _spriteRenderer;

        public Simulation(SimulationLoop simulationLoop, SimulationData simulationData)
        {
            _simulationLoop = simulationLoop;
            _simulationData = simulationData;
            _spriteRenderer = new(_simulationData.BoidsData.BoidSprite,
                _simulationData.BoidsData.BoidMaterial, _simulationData.AreaSize);

            simulationLoop.OnPhysicsUpdate += AccelerationUpdate;
            simulationLoop.OnPhysicsUpdate += MoveUpdate;
            simulationLoop.OnGraphicsUpdate += RendererUpdate;
            simulationLoop.OnDispose += Dispose;
        }

        private void AccelerationUpdate(float deltaTime)
        {
            var accelerationJob = new AccelerationJob()
            {
                Positions = _simulationData.BoidsData.Positions,
                Velocities = _simulationData.BoidsData.Velocities,
                Accelerations = _simulationData.BoidsData.Accelerations,
                AvoidanceDistance = _simulationData.AvoidanceDistance,
                SightDistance = _simulationData.SightDistance,
                CohesionFactor = _simulationData.CohesionFactor,
                SeparationFactor = _simulationData.SeparationFactor,
                AlignmentFactor = _simulationData.AlignmentFactor,
            };

            accelerationJob.Schedule(_simulationData.BoidsData.GetInstanceCount(), 0).Complete();
        }

        private void MoveUpdate(float deltaTime)
        {
            var moveJob = new MoveJob()
            {
                Accelerations = _simulationData.BoidsData.Accelerations,
                Positions = _simulationData.BoidsData.Positions,
                Rotations = _simulationData.BoidsData.Rotations,
                Velocities = _simulationData.BoidsData.Velocities,
                AreaSize = _simulationData.AreaSize,
                BorderSightDistance = _simulationData.BorderSightDistance,
                BorderAvoidanceFactor = _simulationData.BorderAvoidanceFactor,
                MaximumVelocity = _simulationData.MaximumVelocity,
                MinimumVelocity = _simulationData.MinimumVelocity,
            };

            moveJob.Schedule(_simulationData.BoidsData.GetInstanceCount(), 0).Complete();
        }

        private void RendererUpdate(float deltaTime)
        {
            _spriteRenderer.Render(
                _simulationData.BoidsData.Positions,
                _simulationData.BoidsData.Rotations,
                _simulationData.BoidsData.GetInstanceCount());
        }

        public void Dispose()
        {
            _simulationLoop.OnPhysicsUpdate -= AccelerationUpdate;
            _simulationLoop.OnPhysicsUpdate -= MoveUpdate;
            _simulationLoop.OnGraphicsUpdate -= RendererUpdate;
            _simulationLoop.OnDispose -= Dispose;
        }
    }
}
