CUDAfy .NET SDK V1.25 - General Purpose GPU programming for .NET
Copyright 2011-13 Hybrid DSP Systems
www.hybriddsp.com

Description
-----------
CUDAfy.NET is a .NET 4.0 library that allows writing of NVIDIA CUDA and (Intel/AMD/NVIDIA/Altera, etc) OpenCL applications from with .NET. There are no separate CUDA cu files or complex set-up procedures to launch GPU device functions. It follows the CUDA programming model and any knowledge gained from tutorials or books on CUDA can be easily transferred to CUDAfy, only in a clean .NET fashion.

Licenses
--------
The LGPL v2.1 License applies to CUDAfy .NET. If you wish to modify the code then changes should be re-submitted to Hybrid DSP.  If you wish to incorporate Cudafy.NET into your own application instead of redistributing the dll's then please consider a commerical license. Visit www.hybriddsp.com. This will also provide you with priority support and contribute to on-going development.

The MIT license applies to ILSpy, NRefactory and ICSharpCode.Decompiler (Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop team).
Mono.Cecil also uses the MIT license (Copyright JB Evain).
CUDA.NET is a free for use license (Copyright Company for Advanced Supercomputing Solutions Ltd)
The BSD v2 License applies to SimpleBarrier. Author: Thomas W. Christopher

The MIT license applies to Cristi Potlog's Controls for .NET used in installer.


Installation Instructions
-------------------------

Download and install in default locations the CUDA Toolkit 5.5 from http://developer.nvidia.com/cuda-toolkit-sdk

Demo Visual Studo 2010 projects are provided.  
Cudafy by Example is based on the book CUDA By Example by Sanders and Kandrot and is copyright NVIDIA. Reading this book is 
highly recommended.

CudafyModuleViewer can be used to examine Cudafy modules (*.cdfy)


Releases
--------
V1.25 02-08-13
	Fix: Typo in xml comment of GThread.
	Fix: Use of eCudafyAddressSpace attribute on parameters (needed for OpenCL).
	Fix: Shared memory arrays of structs not translated correct for OpenCL.
	Fix: Cudafying reported by some users to appear to freeze when calling nvcc. 
	Chg: Make HostAllocate for OpenCL 4096 byte aligned.
	Chg: OpenCL Linux so name
	Add: LinuxDebug configuration to Cloo.VS2010.csproj
	Add: More flexible Cudafy method using CompileProperties (testing to be 		finalized).
	Add: Dynamic Parallelism support (alpha).
	Add: Ability to GetDevice based on architecture
	Add: Binary (cubin) support.
	Chg: Cudafy.Host.Unit tests.
	Chg: Update cudafybyexample
	Chg: If no arch specified fall back on CudafyModes.Lanaguage prop
	Add: CUDA 5.5 (required for RT API)
	Add: New *.cdfy files (not backward compatible)
	Add: Clone method to CudafyModule to allow easier loading of same module to 		multiple GPUs.
	Add: Support GetDeviceProperties advanced for both current and previous CUDA 		release.
	Add: .NET 3.5 support.
	Fix: Delete generated code.
	Add: CUDA 5.5 support.

V1.22 07-05-13
	Fix: Handle non-public members
	Add: OpenCL as an architecture
	Add: Support for Infinite and NaN

V1.21 Alpha - 10-04-13
	Fix: Handing of structs in OpenCL
	Fix: Make GMath.PI and GMath.E const
	Fix: Formatting of floating points

V1.20 Beta - 02-03-13
	Add: OpenCL support - target AMD GPUs and Intel CPUs
	Add: Integer intrinsics
	Add: Can now specify a null in list of types to cudafy.
	Fix: GMath.E and GMath.PI did not translate to floating point versions
	Fix: Unicode handling for CUDA when writing a string on device
	Fix: Nested structs and structs referenced within a struct broken

V1.12 - 08-11-12
	Fix: CudafyTranslator support for 2.1 and 3.5 architectures.
	Fix: Some device properties incorrect for CUDA 5.0.
	Fix: CudafyHost.GetDeviceProperties failed if device was not already instantiated.
	Add: Strongly typed launches (thanks to P. Geerkens).
	Add: GetArchitecture method to GPGPU. 

V1.11 - 30-10-12
	Add: Support for CUDA 5 Production Release.
	Fix: Compilation could enter infinite loop for some versions of CUDA.
	Fix: warpSize not properly translated.

V1.10 - 26-08-12
	Add: Support for CUDA 5 RC (required if using Maths libraries).
	Fix: Lock method when multi-threading enabled could dead-lock.
	Add: Architecture sm_35.
	Add: Support for context switching.
	Fix: Translation of PI and E must be done using InvariantCulture.
	Add: tcc driver property (HighPerformanceDriver).
	Add: GetDevice always sets the current context to the device context that was got.
	Add: Device to device copies.
	Add: Async on device copies.
	Fix: Device properties were not always correctly returned.


V1.9 - 23-04-12
	Add: New copy to device and copy from device overloads.
        Add: CUDA 4.2 support (Maths libraries must use CUDA 4.2).
	Add: Architectures sm_21 and sm_30 (Kepler).

V1.8 - 15-02-12
	Fix: All relevant CUDA.NET functions now use SizeT.
	Fix: CUSPARSE wrapper and unit tests.
	Fix: GMathUnitTests could access uninitialised memory.
	Add: Disallow passing arrays by ref.
	Add: Support for CUDA 4.1 Maths libraries (4.0 support dropped).
	Fix: Dummy struct with parameterless constructor.
	Fix: Check in CudafyTranslator caused cudafycl to fail on some structs.
	Add: GThread Ballot, SyncThreadsCount, Any, All.
	Add: Examples for SyncThreadCount and Ballot.
	Add: Debug.WriteLine and Console.WriteLine support.
	Add: Debug.Assert support.
	Add: New CUDA 4.1 error codes.	
	Fix: CudafyLanguageException inherits from CudafyException.
	Fix: GPGPU destructor should not set IsDisposed flag and should not be removed from CudafyHost store.	
	Add: Allow CudafyIgnore to be placed on struct constructors.	

V1.7 - 13-12-11
	Fix: Compiler options for specific CUDA version not handled correctly.
	Fix: Context switching to use SetCurrentContext and GetCurrentContext.

V1.6 - 09-12-11
	Chg: Use DefaultEncoding for generated CUDA C files.
	Add: Basic support for Unicode strings.
	Fix: In unit test TestASUMInVectorStep, N should be divided by 2. 
	Fix: Wrong code generated for 2D indexing when an indexer is a statement (e.g. [x+1,y+1]).
	Fix: CUDARuntime class now explicitly supports 64-bit only (cudart64_40_17.dll). Support for 32-bit can be given by 	manually altering DLL_NAME in source file. 
	Fix: Checksum check should also ensure that a suitable PTX exists. 
	Add: SPARSE Matrix library Level 1,2,3 for real numbers.
	Add: Conjugate Gradient solver.
	Add: Biconjugate gradient stabilized method (BiCGSTAB) for sparse linear system solver.
	Add: cudafy command line tool (translates and embeds CudafyModule in .NET assembly)
	Add: cudafycl unit tests.
	Fix: StringTests unit test did not run if CudafyModule was unchanged after Deserialize.
	Fix: Struct methods that return void were not translated correctly, resulting in nvcc compile error.
	Add: Deserialize CudafyModule from Stream.
	Add: SuppressWindow property in CudafyModule (stops command windows showing)
	Add: Embedded cudafy module support in CudafyModule and helper extensions for Assembly class. 	
	Add: Transparently handle checked/unchecked expressions - allows VB projects to run without Remove integer overflow 	checks option being enabled. 

V1.5 - 25-10-11
	Fix: Destination offset was set to source offset value in CopyOnHost.
	Fix: CudaGPU Free methods did not correctly switch context.
	Fix: In generated CUDA C code do not print @ sign infront of reserved words.
	Fix: Emulator now behaves like CudaGPU for illegal multithreaded use.
	Add: IsMultithreadingEnabled property.
	Add: Smart copy - easy asynchronous copying to and from GPU. Call EnableSmartCopy then use overloads of 		CopyXXXDeviceAsync().

V1.4 - 11-07-11
	Add: GenerateDebug flag to CudafyTranslator and CudafyModule - allows Parallel NSight Debugging.
	Add: Ability to pass GThread between device functions.
	Add: Max and Min support to Math and GMath.
	Fix: Math.Abs translation.
	Fix: Int64 and UInt64 not translated correctly.
	Fix: Catch kernels without any members in TryVerifyChecksums and VerifyChecksums.
	Add: Enum support (not flags).
	Fix: 32-bit and 64-bit cross compiler options.
	Add: Constant memory offset and size when transferring from host to device.
	Fix: Create default constructor for structs.
	Add: New icon to CudafyModuleViewer.
	Add: CudafyIgnore attribute to allow ignoring of properties and methods of structs.
	Add: Support for .NET pointers ('fixed' statements).
	Add: Support for fixed pointers within structs.
	

V1.3 - 21-06-11
	Change	: Separate ILSpy and Mono.Cecil dlls removed - functionality placed in Cudafy.Translator
	Fix	: DeviceProperties struct was incorrect layout results in some parameters being filled wrongly

V1.2 - 15-06-11
	Change	: CudafyByExample code now uses dynamic run-time to launch functions
	Add	: Extension methods to IntPtr to allow reading and writing of managed array
	Add	: Default parameter to delete generated code set to false
	Fix	: Dynamic launcher with zero arguments must set stream id to -1
	Add	: CUBLAS 4.0 Wrapper for Level 1 (beta)
	Add	: CUFFT 4.0 Wrapper (beta)
	Add	: CURAND 4.0 host and kernel Wrapper (beta)
	Add	: Auto resolving of 32-bit or 64-bit CUBLAS, CURAND, CUFFT and CUDART (no need to copy and rename dll's)
	Fix	: Emulated FFT destructor could cause crash
	Add	: Casting one array to another in terms of offset, size, rank, type (not fully supported for emulation)
	Fix	: Handling of the zero stream
	Add	: Additional overloads to the CudafyTranslator.Cudafy method


V1.1 - 23-05-11
	Add	: CUDA 4.0 RC Support
	Add	: CudafyTranslator (built on ILSpy)
	Removed	: .NET Reflector requirements
	Add	: CUBLAS Wrapper (Alpha)
	Add	: CUFFT Wrapper
	Change	: CudafyByExample and CudafyExamples updated to use CudafyTranslator.
	Add	: Char (Unicode) + String copy to/from device support
	Add	: Multiple kernel module support
	Add	: Casting between 1D, 2D and 3D arrays
	Add	: Foreach support
	Change	: Support Any CPU
	Add	: Dynamic Launcher (DLR)
 

V1.0 - 30-03-11
	Add	: 64-bit support to CUDA.NET and for cudafy module compilation.
	Add	: Support multiple platform targets.
	Add	: CudafyReflectorWrapper to enable cudafying from directly within user application.
	Add	: Multithreading support (call GPGPU.EnableMultithreading()).
	Add	: Overloaded Cudafy, Serialize and Deserialize methods that use name of calling type.
	Change	: Simpilify design of GPGPU, CudaGPU and EmulatedGPU.
	Add	: GPGPU supports Dispose.
	Add	: CudafyHost.CreateGPU method.
	Add	: WarpSize property to GThread.
	Change	: CudafyByExample and CudafyExamples updated to support CudafyReflectorWrapper use.
	

V0.3 - 17-03-11	
	Fix	: .NET Reflector Add-in could put output files in wrong folder when calling nvcc.exe
	Fix	: Cudafy Module Viewer - resizing window reveals bad anchoring of sub components.
	Fix	: CUDA.NET XML documentation was missing.
	Change	: Make x86 default configuration for example solutions.
	Change	: All examples except hist_gpu_shmem_atomics are pre-compiled for compute capability 1.1.
	Add	: Check for compute capability 1.2 or higher in hist_gpu_shmem_atomics. 
	Add	: Link in documents folder to CUDAfy web page.

V0.3 - 13-03-11	
	Add	: Support for .NET Reflector version 7


Known Issues and Limitations
----------------------------

Large numbers of threads per block can result in very slow emulation - for debugging on CPU temporarily minimize this.

Out parameters are not supported - instead pass an array of length 1.

The following constructs are not currently supported for translation:

	Try-Catch-Finally
	Events
	Delegates
	Pinned types
	Optional modifiers
	Volatile
	Modifiers
	Function pointers
	Properties
	Abstract methods
	Virtual methods
	Params attribute
	Jagged arrays
	Generic parameter constraints
	Default expressions
	Type of typed reference expressions
	Value of typed reference expressions
	Typed reference create expressions
	Method Of expressions
	Base reference expressions
	Try cast expressions (as)
	Can cast expressions (is)
	Null coalescing expressions
	Delegate create expressions
	Property indexer expressions
	Event reference expressions
	Delegate invoke expressions
	Stack allocate expressions
	Lambda expressions
	Query expressions
	Snippet expressions
	Memory copy statements
	Memory initialize statements
	Debug break statements
	Lock statements
	Using statements
	Fixed statements
	Non variable declarations in a block expression
	Attach event statements
	Remove event statements
	Resources
	Arrays of more than 3 dimensions
	Enumerations
	Interfaces
	Nested types
	Operator overloading
	Overloaded Methods
	Classes (individual methods, constants and blittable structs are)

Furthermore only blittable and value types are supported. For example:
	Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Single, Double

For convenience the following complex types are used.  They are directly equivalent to the CUDA types:
	ComplexF, ComplexD (cuFloatComplex and cuDoubleComplex)

Most Math methods are supported. Some .NET Math methods only support doubles. 
If you are using floats then this means an unnecessary cast will be inserted in the generated CUDA code. 
In these cases use GMath.  

Array: Length, GetLength and Rank members are supported.

Be sure to read the output of the NVIDIA compiler since the CUDAfy language will not catch all illegal
settings.

Targeting OpenCL is more restrictive than CUDA. Constant strings, methods in structs,
the Cudafy.Maths libraries, etc are not supported. Consult the unit tests and CudafyExamples
on cudafy.codeplex.com for more information on what is and is not supported.



Acknowledgements
----------------

Hybrid DSP Systems would like to acknowledge the following:

GASS for CUDA.NET (http://www.hoopoe-cloud.com/Solutions/CUDA.NET/Default.aspx)

CUDA, NSight and 'CUDA By Example' (Sanders and Kandrot) are copyright NVIDIA Corporation (http://www.nvidia.com)

Daniel Grunwald and SharpDevelop for ILSpy (http://wiki.sharpdevelop.net/DanielGrunwald.ashx)

JB Evian for Mono.Cecil (http://www.mono-project.com/Cecil)

Tamas Szalay for Managed FFTW Wrapper - Copyright 2006 - Tamas Szalay (http://www.sdss.jhu.edu/~tamas/bytes/fftwcsharp.html)

Thomas W. Christopher for SimpleBarrier
