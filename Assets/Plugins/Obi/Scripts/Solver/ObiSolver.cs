/**
\mainpage Obi documentation
 
Introduction:
------------- 

Obi is a position-based dynamics framework for unity. It enables the simulation of cloth, ropes and fluid in realtime, complete with two-way
rigidbody interaction.
 
Features:
-------------------

- Particles can be pinned both in local space and to rigidbodies (kinematic or not).
- Realistic wind forces.
- Rigidbodies react to particle dynamics, and particles reach to each other and to rigidbodies too.
- Easy prefab instantiation, particle-based actors can be translated, scaled and rotated.
- Simulation can be warm-started in the editor, then all simulation state gets serialized with the object. This means
  your prefabs can be stored at any point in the simulation, and they will resume it when instantiated.
- Custom editor tools.

*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;

namespace Obi
{

/**
 * An ObiSolver component simulates particles and their interactions using the Oni unified physics library.
 * Several kinds of constraint types and their parameters are exposed, and several Obi components can
 * be used to feed particles and constraints to the solver.
 */
[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Solver")]
[DisallowMultipleComponent]
public sealed class ObiSolver : MonoBehaviour
{

	public enum SimulationOrder{
		FixedUpdate,
		AfterFixedUpdate,
		LateUpdate
	}

	public class ObiCollisionEventArgs : EventArgs{

		public Oni.Contact[] contacts;	/**< collision contacts.*/

		public ObiCollisionEventArgs(Oni.Contact[] contacts){
			this.contacts = contacts;
		}
	}

	public class ObiFluidEventArgs : EventArgs{

		public int[] indices;			/**< fluid particle indices.*/
		public Vector4[] vorticities;
		public float[] densities;

		public ObiFluidEventArgs(int[] indices, 
								 Vector4[] vorticities,
								 float[] densities){
			this.indices = indices;
			this.vorticities = vorticities;
			this.densities = densities;
		}
	}

	public const int MAX_NEIGHBOURS = 92;
	public const int CONSTRAINT_GROUPS = 11;

	public event EventHandler OnFrameBegin;
	public event EventHandler OnStepBegin;
	public event EventHandler OnFixedParticlesUpdated;
	public event EventHandler OnStepEnd;
	public event EventHandler OnBeforePositionInterpolation;
	public event EventHandler OnBeforeActorsFrameEnd;
	public event EventHandler OnFrameEnd;
	public event EventHandler<ObiCollisionEventArgs> OnCollision;
	public event EventHandler<ObiFluidEventArgs> OnFluidUpdated;
	
	public int maxParticles = 5000;

	[HideInInspector] [NonSerialized] public bool simulate = true;

	[Tooltip("If enabled, will force the solver to keep simulating even when not visible from any camera.")]
	public bool simulateWhenInvisible = true; 			/**< Whether to keep simulating the cloth when its not visible by any camera.*/

	[Tooltip("If enabled, the solver object transform will be used as the frame of reference for all actors using this solver, instead of the world's frame.")]
	public bool simulateInLocalSpace = false;

	[Tooltip("Determines when will the solver update particles.")]
	public SimulationOrder simulationOrder = SimulationOrder.FixedUpdate;

	public ObiColliderGroup colliderGroup;
	public Oni.SolverParameters parameters = new Oni.SolverParameters(Oni.SolverParameters.Interpolation.None,
	                                                                  new Vector4(0,-9.81f,0,0));

	[HideInInspector] [NonSerialized] public List<ObiActor> actors = new List<ObiActor>();
	[HideInInspector] [NonSerialized] public HashSet<int> allocatedParticles;
	[HideInInspector] [NonSerialized] public HashSet<int> activeParticles;

	[HideInInspector] [NonSerialized] public int[] materialIndices;
	[HideInInspector] [NonSerialized] public int[] fluidMaterialIndices;

	private List<ObiEmitterMaterial> emitterMaterials = new List<ObiEmitterMaterial>();
	private List<ObiCollisionMaterial> collisionMaterials = new List<ObiCollisionMaterial>();

	[HideInInspector] [NonSerialized] public Vector4[] renderablePositions;	/**< renderable particle positions.*/

	// constraint groups:
	[HideInInspector] public int[] constraintsOrder;
	
	// constraint parameters:
	public Oni.ConstraintParameters distanceConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters bendingConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters particleCollisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters collisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters skinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters volumeConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters tetherConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters pinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters stitchConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,2);
	public Oni.ConstraintParameters densityConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,2);

	private IntPtr oniSolver;

	private ObiCollisionMaterial defaultMaterial;
	private ObiEmitterMaterial defaultFluidMaterial;
	private UnityEngine.Bounds bounds = new UnityEngine.Bounds();
	private Matrix4x4 lastTransform;
 
 	private bool initialized = false;
	private bool isVisible = true;
	private float smoothDelta = 0.02f;
	private int renderablePositionsClients = 0;		/** counter for the amount of actors that need renderable positions.*/

	public IntPtr OniSolver
	{
		get{return oniSolver;}
	}

	public UnityEngine.Bounds Bounds
	{
		get{return bounds;}
	}

	public Matrix4x4 LastTransform
	{
		get{return lastTransform;}
	}

	public bool IsVisible
	{
		get{return isVisible;}
	}

	public bool IsUpdating{
		get{return (initialized && simulate && (simulateWhenInvisible || IsVisible));}
	}

	public void RequireRenderablePositions(){
		renderablePositionsClients++;
	}

	public void RelinquishRenderablePositions(){
		if (renderablePositionsClients > 0)
			renderablePositionsClients--;
	}

	void Start(){
		if (colliderGroup != null)
			Oni.SetColliderGroup(oniSolver,colliderGroup.oniColliderGroup);
	}

	void Awake(){

		lastTransform = transform.localToWorldMatrix;

		if (Application.isPlaying) //only during game.
			Initialize();
	}

	void OnDestroy(){
		if (Application.isPlaying) //only during game.
			Teardown();
	}

	void OnEnable(){
		if (!Application.isPlaying) //only in editor.
			Initialize();
		StartCoroutine("RunLateFixedUpdate");
		ObiArbiter.RegisterSolver(this);
	}
	
	void OnDisable(){
		if (!Application.isPlaying) //only in editor.
			Teardown();
		StopCoroutine("RunLateFixedUpdate");
		ObiArbiter.UnregisterSolver(this);
	}
	
	public void Initialize(){

		// Tear everything down first:
		Teardown();
			
		try{

			// Create a default material:
			defaultMaterial = ScriptableObject.CreateInstance<ObiCollisionMaterial>();
			defaultMaterial.hideFlags = HideFlags.HideAndDontSave;

			defaultFluidMaterial = ScriptableObject.CreateInstance<ObiEmitterMaterialFluid>();
			defaultFluidMaterial.hideFlags = HideFlags.HideAndDontSave;
	
			// Create the Oni solver:
			oniSolver = Oni.CreateSolver(maxParticles,MAX_NEIGHBOURS);
			
			actors = new List<ObiActor>();
			allocatedParticles = new HashSet<int>();
			activeParticles = new HashSet<int>();
			materialIndices = new int[maxParticles];
			fluidMaterialIndices = new int[maxParticles];
			renderablePositions = new Vector4[maxParticles];
			
			// Initialize materials:
			UpdateSolverMaterials();
			UpdateEmitterMaterials();
			
			// Initialize parameters:
			UpdateParameters();
			
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = true;
		};

	}

	private void Teardown(){
	
		if (!initialized) return;
		
		try{

			while (actors.Count > 0){
				actors[actors.Count-1].RemoveFromSolver(null);
			}
				
			Oni.DestroySolver(oniSolver);
			oniSolver = IntPtr.Zero;
			
			GameObject.DestroyImmediate(defaultMaterial);
			GameObject.DestroyImmediate(defaultFluidMaterial);
		
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = false;
		}
	}

	/**
	 * Adds a new transform to the solver and returns its ID.
	 */
	public int SetActor(int ID, ObiActor actor)
	{

		// Add the transform, as its new.
		if (ID < 0 || ID >= actors.Count){
	
			int index = actors.Count;

            // Use the free slot to insert the transform:
			actors.Add(actor);

			// Update materials, in case the actor has a new one.
			UpdateSolverMaterials();
			UpdateEmitterMaterials();

			// Return the transform index as its ID
			return index;

		}
		// The transform is already there.
		else{

			actors[ID] = actor;
			UpdateSolverMaterials();
			UpdateEmitterMaterials();
			return ID;

		}

	}

	/**
 	 * Removes an actor from the solver and returns its ID.
	 */
	public void RemoveActor(int ID){
		
		if (ID < 0 || ID >= actors.Count) return;

		// Update actor ID for affected actors:
		for (int i = ID+1; i < actors.Count; i++){
			actors[i].actorID--;
		}

		actors.RemoveAt(ID); 

		// Update materials, in case the actor had one.
		UpdateSolverMaterials();
		UpdateEmitterMaterials();
	}

	/**
	 * Reserves a certain amount of particles and returns their indices in the 
	 * solver arrays.
	 */
	public int[] AllocateParticles(int numParticles){

		if (allocatedParticles == null)
			return null;

		int[] allocated = new int[numParticles];
		int allocatedCount = 0;

		for (int i = 0; i < maxParticles && allocatedCount < numParticles; i++){
			if (!allocatedParticles.Contains(i)){
				allocated[allocatedCount] = i;
				allocatedCount++;
			}
		}

		// could not allocate enough particles.
		if (allocatedCount < numParticles){
			return null; 
		}
   
        // allocation was successful:
		allocatedParticles.UnionWith(allocated);
		activeParticles.UnionWith(allocated);
		UpdateActiveParticles();          
		return allocated;

	}

	/**
	 * Frees a list of particles.
	 */
	public void FreeParticles(int[] indices){
		
		if (allocatedParticles == null || indices == null)
			return;
		
		allocatedParticles.ExceptWith(indices);
		activeParticles.ExceptWith(indices);

		UpdateActiveParticles(); 
		
	}

	/**
	 * Updates solver parameters, sending them to the Oni library.
	 */
	public void UpdateParameters(){

		Oni.SetSolverParameters(oniSolver,ref parameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Distance,ref distanceConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Bending,ref bendingConstraintParameters);
	
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.ParticleCollision,ref particleCollisionConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Collision,ref collisionConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Density,ref densityConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Skin,ref skinConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Volume,ref volumeConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Tether,ref tetherConstraintParameters);
	
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Pin,ref pinConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Stitch,ref stitchConstraintParameters);

		// Lazy initialization of constraints order.
		if (constraintsOrder == null || constraintsOrder.Length != CONSTRAINT_GROUPS)
		 	constraintsOrder = Enumerable.Range(0, CONSTRAINT_GROUPS).ToArray();

		Oni.SetConstraintsOrder(oniSolver,constraintsOrder);
    }

	/**
	 * Updates the active particles array.
	 */
	public void UpdateActiveParticles(){

		// Get allocated particles and remove the inactive ones:
		int[] activeArray = new int[activeParticles.Count];
		activeParticles.CopyTo(activeArray);
		Oni.SetActiveParticles(oniSolver,activeArray,activeArray.Length);

	}

	public void UpdateEmitterMaterials(){

		// reset the emitter material list:
		emitterMaterials = new List<ObiEmitterMaterial>(){defaultFluidMaterial};

		// Setup all materials used by particle actors:
		foreach (ObiActor actor in actors){
			
			ObiEmitter em = actor as ObiEmitter;
				if (em == null) continue;

			int materialIndex = 0;

			if (em.EmitterMaterial != null){

				materialIndex = emitterMaterials.IndexOf(em.EmitterMaterial);

				// if the material has not been considered before:
				if (materialIndex < 0){

					materialIndex = emitterMaterials.Count;
					emitterMaterials.Add(em.EmitterMaterial);
			
					//keep an eye on material changes:
					em.EmitterMaterial.OnChangesMade += emitterMaterial_OnChangesMade;
				}
			}
			
			// Update material index for all actor particles:
			for(int i = 0; i < actor.particleIndices.Length; i++){
				fluidMaterialIndices[actor.particleIndices[i]] = materialIndex;
			}
		}

		Oni.SetFluidMaterialIndices(oniSolver,fluidMaterialIndices,fluidMaterialIndices.Length,0);
		Oni.FluidMaterial[] mArray = emitterMaterials.ConvertAll<Oni.FluidMaterial>(a => a.GetEquivalentOniMaterial(parameters.mode)).ToArray();
		Oni.SetFluidMaterials(oniSolver,mArray,mArray.Length,0);
	}

	private void emitterMaterial_OnChangesMade (object sender, ObiEmitterMaterial.MaterialChangeEventArgs e)
	{
		ObiEmitterMaterial material = sender as ObiEmitterMaterial; 
		int index = emitterMaterials.IndexOf(material);
		if (index >= 0){
			Oni.SetFluidMaterials(oniSolver,new Oni.FluidMaterial[]{material.GetEquivalentOniMaterial(parameters.mode)},1,index);
		}
	}

	public void UpdateSolverMaterials(){

		// reset the collision material list:
		collisionMaterials = new List<ObiCollisionMaterial>(){defaultMaterial};

		// Setup all materials used by particle actors:
		foreach (ObiActor actor in actors){
			
			int materialIndex = 0;

			if (actor.material != null){

				materialIndex = collisionMaterials.IndexOf(actor.material);

				// if the material has not been considered before:
				if (materialIndex < 0){
					materialIndex = collisionMaterials.Count;
					collisionMaterials.Add(actor.material);
				}

			}

			// Update material index for all actor particles:
			for(int i = 0; i < actor.particleIndices.Length; i++){
				materialIndices[actor.particleIndices[i]] = materialIndex;
			}
		}

		// Setup all materials used by colliders: //TODO: 2d colliders too!
		if (colliderGroup != null){
			foreach (Collider c in colliderGroup.colliders){
			
				if (c == null) continue;

				ObiCollider oc = c.GetComponent<ObiCollider>();
	
				if (oc == null) continue;
					
				oc.materialIndex = 0; // Colliders with no ObiCollider component should use the default material.
				
				if (oc.material == null) continue;

				oc.materialIndex = collisionMaterials.IndexOf(oc.material);
	
				// if the material has not been considered before:
				if (oc.materialIndex < 0){
					oc.materialIndex = collisionMaterials.Count;
					collisionMaterials.Add(oc.material);
				}

			}
		}

		Oni.SetMaterialIndices(oniSolver,materialIndices,materialIndices.Length,0);
		Oni.CollisionMaterial[] mArray = collisionMaterials.ConvertAll<Oni.CollisionMaterial>(a => a.GetEquivalentOniMaterial()).ToArray();
		Oni.SetCollisionMaterials(oniSolver,mArray,mArray.Length,0);
	}

	public void AccumulateSimulationTime(float dt){
		Oni.AddSimulationTime(oniSolver,dt);
	}

	public void ResetSimulationTime(){
		Oni.ResetSimulationTime(oniSolver);
	}

	public void SimulateStep(float stepTime){

		Oni.ClearDiffuseParticles(oniSolver);

		if (OnStepBegin != null)
			OnStepBegin(this,null);

		foreach(ObiActor actor in actors)
            actor.OnSolverStepBegin();

		// Update Oni skeletal mesh skinning after updating animators:
		Oni.UpdateSkeletalAnimation(oniSolver);

		// Trigger event right after actors have fixed their particles in OnSolverStepBegin.
		if (OnFixedParticlesUpdated != null)
			OnFixedParticlesUpdated(this,null);

		// Update all collider and rigidbody information, so that the solver works with up-to-date stuff.
		// This is only actually done for the first solver that calls it this step.
		if (colliderGroup != null)
			colliderGroup.UpdateBodiesInfo(this); 

		ObiArbiter.FrameStart();

		// Update the solver (this is internally split in tasks so multiple solvers can be updated in parallel)
		Oni.UpdateSolver(oniSolver, stepTime); 

		// Wait here for all other solvers to finish, if we are the last solver to call this.
		ObiArbiter.WaitForAllSolvers();

	} 

	public void EndFrame(float frameDelta){

		foreach(ObiActor actor in actors)
            actor.OnSolverPreInterpolation();

		if (OnBeforePositionInterpolation != null)
			OnBeforePositionInterpolation(this,null);

		Oni.ApplyPositionInterpolation(oniSolver, frameDelta);

		// if we need to get renderable positions back from the solver:
		if (renderablePositionsClients > 0)
		{
			Oni.GetRenderableParticlePositions(oniSolver, renderablePositions, renderablePositions.Length,0);

			// convert positions to world space if they are expressed in solver space:
			if (simulateInLocalSpace){
				Matrix4x4 l2wTransform = transform.localToWorldMatrix;
				for (int i = 0; i < renderablePositions.Length; ++i){
					renderablePositions[i] = l2wTransform.MultiplyPoint3x4(renderablePositions[i]);
				}
			}
		}

		// Trigger fluid update:
		TriggerFluidUpdateEvents();

		if (OnBeforeActorsFrameEnd != null)
			OnBeforeActorsFrameEnd(this,null);

		CheckVisibility();
		
		foreach(ObiActor actor in actors)
            actor.OnSolverFrameEnd();

	}

	private void TriggerFluidUpdateEvents(){

		int numFluidParticles = Oni.GetConstraintCount(oniSolver,(int)Oni.ConstraintType.Density);
		
		if (numFluidParticles > 0 && OnFluidUpdated != null){

			int[] indices = new int[numFluidParticles];
			Vector4[] vorticities = new Vector4[maxParticles];
			float[] densities = new float[maxParticles];

			Oni.GetActiveConstraintIndices(oniSolver,indices,numFluidParticles,(int)Oni.ConstraintType.Density);
			Oni.GetParticleVorticities(oniSolver,vorticities,maxParticles,0);
			Oni.GetParticleDensities(oniSolver,densities,maxParticles,0);

			OnFluidUpdated(this,new ObiFluidEventArgs(indices,vorticities,densities));
		}
	}

	private void TriggerCollisionEvents(){
	
		int numCollisions = Oni.GetConstraintCount(oniSolver,(int)Oni.ConstraintType.Collision);

		if (OnCollision != null){

			Oni.Contact[] contacts = new Oni.Contact[numCollisions];

			if (numCollisions > 0)
			{
				Oni.GetCollisionContacts(oniSolver,contacts,numCollisions);
			}
	
			OnCollision(this,new ObiCollisionEventArgs(contacts));

		}
	}

	private bool AreBoundsValid(Bounds bounds){
		return !(float.IsNaN(bounds.center.x) || float.IsInfinity(bounds.center.x) ||
			     float.IsNaN(bounds.center.y) || float.IsInfinity(bounds.center.y) ||
			     float.IsNaN(bounds.center.z) || float.IsInfinity(bounds.center.z));
	}

	/**
	 * Checks if any particle in the solver is visible from at least one camera. If so, sets isVisible to true, false otherwise.
	 */
	private void CheckVisibility(){

		Vector3 min = Vector3.zero, max = Vector3.zero;
		Oni.GetBounds(oniSolver,ref min, ref max);
		bounds.SetMinMax(min,max);

		isVisible = false;

		if (AreBoundsValid(bounds)){

			Bounds wsBounds = simulateInLocalSpace ? bounds.Transform(transform.localToWorldMatrix) : bounds;

			foreach (Camera cam in Camera.allCameras){
	        	Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
	       		if (GeometryUtility.TestPlanesAABB(planes, wsBounds)){
					isVisible = true;
					return;
				}
			}
		}
	}
    
    void Update(){

		if (!Application.isPlaying)
			return;

		if (OnFrameBegin != null)
			OnFrameBegin(this,null);

		foreach(ObiActor actor in actors)
            actor.OnSolverFrameBegin();

		if (IsUpdating && simulationOrder != SimulationOrder.LateUpdate){
			AccumulateSimulationTime(Time.deltaTime);
		}

	}

	IEnumerator RunLateFixedUpdate() {
         while (true) {

             yield return new WaitForFixedUpdate();

			 if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.AfterFixedUpdate)
             	SimulateStep(Time.fixedDeltaTime);
         }
     }

    void FixedUpdate()
    {
		if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.FixedUpdate)
			SimulateStep(Time.fixedDeltaTime);
    }

	public void AllSolversStepEnd()
	{
		// Apply modified rigidbody velocities and torques back:
		// This is only actually done for the first solver that calls it at the end of this step.
		if (colliderGroup != null)
			colliderGroup.UpdateVelocities(); 

		// Trigger solver events:
		TriggerCollisionEvents();
	
		foreach(ObiActor actor in actors)
       	 	actor.OnSolverStepEnd();

		if (OnStepEnd != null)
			OnStepEnd(this,null);

		lastTransform = transform.localToWorldMatrix;
	}

	private void LateUpdate(){

		if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.LateUpdate){
			 smoothDelta = Mathf.Lerp(Time.deltaTime,smoothDelta,0.95f);
			 AccumulateSimulationTime(smoothDelta);
             SimulateStep(smoothDelta);
		}

		if (!Application.isPlaying)
			return;
   
		EndFrame (simulationOrder == SimulationOrder.LateUpdate ? smoothDelta : Time.fixedDeltaTime);

		if (OnFrameEnd != null)
			OnFrameEnd(this,null);
	}

}

}
