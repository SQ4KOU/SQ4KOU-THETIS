
/* ========================================================================
   ENTRY: 180001000
   NAME : spectral_adaptive_denoiser_initialize
   SIG  : uint * __fastcall spectral_adaptive_denoiser_initialize(uint param_1, uint param_2, ulonglong param_3)
   ======================================================================== */

uint * spectral_adaptive_denoiser_initialize(uint param_1,uint param_2,ulonglong param_3)

{
  undefined4 uVar1;
  uint uVar2;
  uint *puVar3;
  void *pvVar4;
  uint *puVar5;
  undefined8 *puVar6;
  uint uVar7;
  ulonglong _Count_00;
  ulonglong _Count;
  
                    /* 0x1000  96  spectral_adaptive_denoiser_initialize */
  puVar3 = calloc(1,0xa8);
  *puVar3 = param_2;
  uVar7 = (param_2 >> 1) + 1;
  _Count = (ulonglong)uVar7;
  puVar3[1] = uVar7;
  puVar3[2] = param_1;
  uVar2 = (uint)((ulonglong)param_2 / (param_3 & 0xffffffff));
  puVar3[3] = uVar2;
  *(undefined8 *)(puVar3 + 4) = DAT_180008000;
  puVar3[0x1b] = 2;
  puVar3[0x1d] = 1;
  _Count_00 = (ulonglong)param_2;
  pvVar4 = calloc(_Count_00,4);
  *(void **)(puVar3 + 0x12) = pvVar4;
  uVar1 = DAT_180008010;
  initialize_spectrum_with_value((longlong)pvVar4,param_2,DAT_180008010);
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 0xe) = pvVar4;
  initialize_spectrum_with_value((longlong)pvVar4,uVar7,uVar1);
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 0x10) = pvVar4;
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 0x18) = pvVar4;
  puVar5 = louizou_estimator_initialize(uVar7,param_1,param_2);
  *(uint **)(puVar3 + 0x26) = puVar5;
  pvVar4 = calloc(_Count_00,4);
  *(void **)(puVar3 + 0x14) = pvVar4;
  pvVar4 = calloc(_Count_00,4);
  *(void **)(puVar3 + 0x16) = pvVar4;
  puVar6 = postfilter_initialize(param_2);
  *(undefined8 **)(puVar3 + 0x24) = puVar6;
  puVar5 = spectral_smoothing_initialize(param_2,1);
  *(uint **)(puVar3 + 0x22) = puVar5;
  pvVar4 = noise_scaling_criterias_initialize(param_2,2,param_1,0);
  *(void **)(puVar3 + 0x20) = pvVar4;
  puVar6 = spectral_features_initialize(uVar7);
  *(undefined8 **)(puVar3 + 0x28) = puVar6;
  puVar6 = denoise_mixer_initialize(param_2,param_1,uVar2);
  *(undefined8 **)(puVar3 + 0x1e) = puVar6;
  return puVar3;
}



/* ========================================================================
   ENTRY: 180001170
   NAME : spectral_adaptive_denoiser_free
   SIG  : undefined __fastcall spectral_adaptive_denoiser_free(void * param_1)
   ======================================================================== */

void spectral_adaptive_denoiser_free(void *param_1)

{
                    /* 0x1170  95  spectral_adaptive_denoiser_free */
  louizou_estimator_free(*(void **)((longlong)param_1 + 0x98));
  spectral_features_free(*(undefined8 **)((longlong)param_1 + 0xa0));
  noise_scaling_criterias_free(*(void **)((longlong)param_1 + 0x80));
  spectral_smoothing_free(*(void **)((longlong)param_1 + 0x88));
  postfilter_free(*(undefined8 **)((longlong)param_1 + 0x90));
  denoise_mixer_free(*(undefined8 **)((longlong)param_1 + 0x78));
  free(*(void **)((longlong)param_1 + 0x50));
  free(*(void **)((longlong)param_1 + 0x58));
  free(*(void **)((longlong)param_1 + 0x60));
  free(*(void **)((longlong)param_1 + 0x48));
  free(*(void **)((longlong)param_1 + 0x38));
  free(*(void **)((longlong)param_1 + 0x40));
                    /* WARNING: Could not recover jumptable at 0x0001800011f5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001200
   NAME : load_adaptive_reduction_parameters
   SIG  : bool __fastcall load_adaptive_reduction_parameters(longlong param_1, undefined8 * param_2)
   ======================================================================== */

bool load_adaptive_reduction_parameters(longlong param_1,undefined8 *param_2)

{
  undefined8 uVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  
                    /* 0x1200  53  load_adaptive_reduction_parameters */
  if (param_1 != 0) {
    uVar1 = *param_2;
    uVar2 = param_2[1];
    uVar3 = *(undefined8 *)((longlong)param_2 + 0x14);
    *(undefined8 *)(param_1 + 0x24) = *(undefined8 *)((longlong)param_2 + 0xc);
    *(undefined8 *)(param_1 + 0x2c) = uVar3;
    *(undefined8 *)(param_1 + 0x18) = uVar1;
    *(undefined8 *)(param_1 + 0x20) = uVar2;
  }
  return param_1 != 0;
}



/* ========================================================================
   ENTRY: 180001220
   NAME : spectral_adaptive_denoiser_run
   SIG  : ulonglong __fastcall spectral_adaptive_denoiser_run(uint * param_1, float * param_2)
   ======================================================================== */

ulonglong spectral_adaptive_denoiser_run(uint *param_1,float *param_2)

{
  void *pvVar1;
  undefined8 unaff_RBX;
  undefined1 auStackY_78 [32];
  float local_34;
  float local_30;
  uint local_2c;
  ulonglong local_28;
  
                    /* 0x1220  97  spectral_adaptive_denoiser_run */
  local_28 = DAT_18000b000 ^ (ulonglong)auStackY_78;
  if (param_1 != (uint *)0x0 && param_2 != (float *)0x0) {
    pvVar1 = (void *)get_spectral_feature
                               (*(longlong **)(param_1 + 0x28),param_2,*param_1,param_1[0x1a]);
    louizou_estimator_run(*(uint **)(param_1 + 0x26),(longlong)pvVar1,*(void **)(param_1 + 0x18));
    local_34 = (float)param_1[5];
    local_30 = (float)param_1[4] + (float)param_1[8];
    local_2c = param_1[7];
    apply_noise_scaling_criteria
              (*(longlong *)(param_1 + 0x20),(longlong)pvVar1,*(longlong *)(param_1 + 0x18),
               *(longlong *)(param_1 + 0xe),*(longlong *)(param_1 + 0x10),&local_34);
    spectral_smoothing_run(*(longlong *)(param_1 + 0x22),(ulonglong)param_1[9],pvVar1);
    estimate_gains(param_1[1],*param_1,(longlong)pvVar1,*(longlong *)(param_1 + 0x18),
                   *(longlong *)(param_1 + 0x12),*(longlong *)(param_1 + 0xe),
                   *(longlong *)(param_1 + 0x10),param_1[0x1c]);
    postfilter_apply(*(longlong **)(param_1 + 0x24),(longlong)param_2,*(void **)(param_1 + 0x12),
                     (float)param_1[0xb]);
    local_34 = (float)param_1[6];
    local_2c = param_1[10];
    local_30 = (float)CONCAT31(local_30._1_3_,(char)param_1[0xc]);
    denoise_mixer_run(*(undefined8 **)(param_1 + 0x1e),(longlong)param_2,
                      *(longlong *)(param_1 + 0x12),&local_34);
  }
  if ((local_28 ^ (ulonglong)auStackY_78) == DAT_18000b000) {
    return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),
                    param_1 != (uint *)0x0 && param_2 != (float *)0x0) & 0xffffffff;
  }
                    /* WARNING: Subroutine does not return */
  FUN_180006af0(local_28 ^ (ulonglong)auStackY_78);
}



/* ========================================================================
   ENTRY: 180001390
   NAME : spectral_denoiser_initialize
   SIG  : uint * __fastcall spectral_denoiser_initialize(uint param_1, uint param_2, ulonglong param_3, undefined8 param_4)
   ======================================================================== */

uint * spectral_denoiser_initialize(uint param_1,uint param_2,ulonglong param_3,undefined8 param_4)

{
  undefined4 uVar1;
  uint uVar2;
  uint *puVar3;
  void *pvVar4;
  uint *puVar5;
  undefined8 *puVar6;
  uint uVar7;
  ulonglong _Count;
  
                    /* 0x1390  99  spectral_denoiser_initialize */
  puVar3 = calloc(1,0xa8);
  *puVar3 = param_2;
  uVar7 = (param_2 >> 1) + 1;
  _Count = (ulonglong)uVar7;
  puVar3[1] = uVar7;
  uVar2 = (uint)((ulonglong)param_2 / (param_3 & 0xffffffff));
  puVar3[3] = uVar2;
  puVar3[2] = param_1;
  puVar3[0xf] = 2;
  *(undefined8 *)(puVar3 + 4) = DAT_180008000;
  puVar3[0x19] = 2;
  pvVar4 = calloc((ulonglong)param_2,4);
  *(void **)(puVar3 + 6) = pvVar4;
  uVar1 = DAT_180008010;
  initialize_spectrum_with_value((longlong)pvVar4,param_2,DAT_180008010);
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 8) = pvVar4;
  initialize_spectrum_with_value((longlong)pvVar4,uVar7,uVar1);
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 10) = pvVar4;
  *(undefined8 *)(puVar3 + 0x20) = param_4;
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar3 + 0xc) = pvVar4;
  puVar5 = noise_estimation_initialize(param_2,param_4);
  *(uint **)(puVar3 + 0x1c) = puVar5;
  puVar6 = spectral_features_initialize(uVar7);
  *(undefined8 **)(puVar3 + 0x22) = puVar6;
  puVar6 = postfilter_initialize(param_2);
  *(undefined8 **)(puVar3 + 0x1e) = puVar6;
  puVar5 = spectral_smoothing_initialize(param_2,2);
  *(uint **)(puVar3 + 0x28) = puVar5;
  pvVar4 = noise_scaling_criterias_initialize(param_2,2,param_1,0);
  *(void **)(puVar3 + 0x26) = pvVar4;
  puVar6 = denoise_mixer_initialize(param_2,param_1,uVar2);
  *(undefined8 **)(puVar3 + 0x24) = puVar6;
  return puVar3;
}



/* ========================================================================
   ENTRY: 1800014e0
   NAME : spectral_denoiser_free
   SIG  : undefined __fastcall spectral_denoiser_free(void * param_1)
   ======================================================================== */

void spectral_denoiser_free(void *param_1)

{
                    /* 0x14e0  98  spectral_denoiser_free */
  noise_estimation_free(*(void **)((longlong)param_1 + 0x70));
  spectral_features_free(*(undefined8 **)((longlong)param_1 + 0x88));
  spectral_smoothing_free(*(void **)((longlong)param_1 + 0xa0));
  noise_scaling_criterias_free(*(void **)((longlong)param_1 + 0x98));
  postfilter_free(*(undefined8 **)((longlong)param_1 + 0x78));
  denoise_mixer_free(*(undefined8 **)((longlong)param_1 + 0x90));
  free(*(void **)((longlong)param_1 + 0x18));
  free(*(void **)((longlong)param_1 + 0x20));
  free(*(void **)((longlong)param_1 + 0x28));
  free(*(void **)((longlong)param_1 + 0x30));
                    /* WARNING: Could not recover jumptable at 0x000180001556. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001560
   NAME : load_reduction_parameters
   SIG  : bool __fastcall load_reduction_parameters(longlong param_1, undefined8 * param_2)
   ======================================================================== */

bool load_reduction_parameters(longlong param_1,undefined8 *param_2)

{
  undefined8 uVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  
                    /* 0x1560  54  load_reduction_parameters */
  if (param_1 != 0) {
    uVar1 = *param_2;
    uVar2 = param_2[1];
    uVar3 = param_2[3];
    *(undefined8 *)(param_1 + 0x50) = param_2[2];
    *(undefined8 *)(param_1 + 0x58) = uVar3;
    *(undefined8 *)(param_1 + 0x40) = uVar1;
    *(undefined8 *)(param_1 + 0x48) = uVar2;
  }
  return param_1 != 0;
}



/* ========================================================================
   ENTRY: 180001580
   NAME : spectral_denoiser_run
   SIG  : ulonglong __fastcall spectral_denoiser_run(uint * param_1, float * param_2)
   ======================================================================== */

ulonglong spectral_denoiser_run(uint *param_1,float *param_2)

{
  void *_Dst;
  char cVar1;
  void *pvVar2;
  void *_Src;
  undefined8 unaff_RBX;
  undefined1 auStackY_88 [32];
  float local_3c;
  float local_38;
  uint local_34;
  ulonglong local_30;
  
                    /* 0x1580  100  spectral_denoiser_run */
  local_30 = DAT_18000b000 ^ (ulonglong)auStackY_88;
  if (param_1 != (uint *)0x0 && param_2 != (float *)0x0) {
    pvVar2 = (void *)get_spectral_feature
                               (*(longlong **)(param_1 + 0x22),param_2,*param_1,param_1[0xe]);
    if (param_1[0x14] == 0) {
      cVar1 = is_noise_estimation_available(*(longlong *)(param_1 + 0x20));
      if (cVar1 != '\0') {
        _Dst = *(void **)(param_1 + 0xc);
        _Src = (void *)get_noise_profile(*(longlong *)(param_1 + 0x20));
        memcpy(_Dst,_Src,(ulonglong)param_1[1] << 2);
        local_3c = (float)param_1[5];
        local_38 = (float)param_1[4] + (float)param_1[0x12];
        local_34 = param_1[0x11];
        apply_noise_scaling_criteria
                  (*(longlong *)(param_1 + 0x26),(longlong)pvVar2,*(longlong *)(param_1 + 0xc),
                   *(longlong *)(param_1 + 8),*(longlong *)(param_1 + 10),&local_3c);
        spectral_smoothing_run
                  (*(longlong *)(param_1 + 0x28),
                   (ulonglong)CONCAT14(*(undefined1 *)((longlong)param_1 + 0x4d),param_1[0x15]),
                   pvVar2);
        estimate_gains(param_1[1],*param_1,(longlong)pvVar2,*(longlong *)(param_1 + 0xc),
                       *(longlong *)(param_1 + 6),*(longlong *)(param_1 + 8),
                       *(longlong *)(param_1 + 10),param_1[0x18]);
        postfilter_apply(*(longlong **)(param_1 + 0x1e),(longlong)param_2,*(void **)(param_1 + 6),
                         (float)param_1[0x17]);
        local_3c = (float)param_1[0x10];
        local_34 = param_1[0x16];
        local_38 = (float)CONCAT31(local_38._1_3_,(char)param_1[0x13]);
        denoise_mixer_run(*(undefined8 **)(param_1 + 0x24),(longlong)param_2,
                          *(longlong *)(param_1 + 6),&local_3c);
      }
    }
    else {
      noise_estimation_run(*(longlong *)(param_1 + 0x1c),param_1[0x14],pvVar2);
    }
  }
  if ((local_30 ^ (ulonglong)auStackY_88) == DAT_18000b000) {
    return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),
                    param_1 != (uint *)0x0 && param_2 != (float *)0x0) & 0xffffffff;
  }
                    /* WARNING: Subroutine does not return */
  FUN_180006af0(local_30 ^ (ulonglong)auStackY_88);
}



/* ========================================================================
   ENTRY: 180001720
   NAME : specbleach_adaptive_initialize
   SIG  : uint * __fastcall specbleach_adaptive_initialize(uint param_1, float param_2)
   ======================================================================== */

uint * specbleach_adaptive_initialize(uint param_1,float param_2)

{
  uint uVar1;
  uint *_Memory;
  int *piVar2;
  uint *puVar3;
  
                    /* 0x1720  81  specbleach_adaptive_initialize */
  _Memory = calloc(1,0x30);
  *_Memory = param_1;
  piVar2 = stft_processor_initialize(param_1,param_2,2,2,0x32,3,3);
  *(int **)(_Memory + 10) = piVar2;
  if (piVar2 == (int *)0x0) {
    spectral_adaptive_denoiser_free((void *)0x0);
    piVar2 = (int *)0x0;
  }
  else {
    uVar1 = get_stft_fft_size((longlong)piVar2);
    puVar3 = spectral_adaptive_denoiser_initialize(param_1,uVar1,2);
    *(uint **)(_Memory + 8) = puVar3;
    if (puVar3 != (uint *)0x0) {
      return _Memory;
    }
    spectral_adaptive_denoiser_free((void *)0x0);
  }
  stft_processor_free(piVar2);
  free(_Memory);
  return (uint *)0x0;
}



/* ========================================================================
   ENTRY: 1800017e0
   NAME : specbleach_adaptive_free
   SIG  : undefined __fastcall specbleach_adaptive_free(void * param_1)
   ======================================================================== */

void specbleach_adaptive_free(void *param_1)

{
                    /* 0x17e0  79  specbleach_adaptive_free */
  spectral_adaptive_denoiser_free(*(void **)((longlong)param_1 + 0x20));
  stft_processor_free(*(void **)((longlong)param_1 + 0x28));
                    /* WARNING: Could not recover jumptable at 0x000180001802. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001810
   NAME : specbleach_adaptive_get_latency
   SIG  : undefined __fastcall specbleach_adaptive_get_latency(longlong param_1)
   ======================================================================== */

void specbleach_adaptive_get_latency(longlong param_1)

{
                    /* 0x1810  80  specbleach_adaptive_get_latency
                       0x1810  88  specbleach_get_noise_profile_size */
  get_noise_profile_size(*(undefined4 **)(param_1 + 0x28));
  return;
}



/* ========================================================================
   ENTRY: 180001820
   NAME : specbleach_adaptive_process
   SIG  : ulonglong __fastcall specbleach_adaptive_process(longlong param_1, uint param_2, longlong param_3, longlong param_4)
   ======================================================================== */

ulonglong specbleach_adaptive_process
                    (longlong param_1,uint param_2,longlong param_3,longlong param_4)

{
  undefined8 unaff_RBX;
  
                    /* 0x1820  83  specbleach_adaptive_process */
  if ((param_4 != 0 && param_3 != 0) && (param_2 != 0 && param_1 != 0)) {
    stft_processor_run(*(longlong *)(param_1 + 0x28),param_2,param_3,param_4,
                       spectral_adaptive_denoiser_run,*(undefined8 *)(param_1 + 0x20));
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),
                  (param_4 != 0 && param_3 != 0) && (param_2 != 0 && param_1 != 0)) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 180001870
   NAME : specbleach_adaptive_load_parameters
   SIG  : undefined8 __fastcall specbleach_adaptive_load_parameters(longlong param_1, undefined1 * param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 specbleach_adaptive_load_parameters(longlong param_1,undefined1 *param_2)

{
  float fVar1;
  undefined1 uVar2;
  undefined4 uVar3;
  float fVar4;
  ulonglong uVar5;
  uint7 extraout_var;
  undefined4 uVar6;
  undefined4 uVar7;
  float fVar8;
  undefined4 uVar9;
  undefined1 auStack_b8 [32];
  undefined4 local_98;
  undefined4 local_94;
  undefined4 uStack_90;
  undefined4 local_8c;
  undefined4 uStack_88;
  undefined4 local_84;
  undefined1 local_80;
  undefined2 local_7f;
  undefined1 local_7d;
  ulonglong local_70;
  
                    /* 0x1870  82  specbleach_adaptive_load_parameters */
  uVar5 = DAT_18000b000 ^ (ulonglong)auStack_b8;
  local_70 = uVar5;
  if (param_1 != 0) {
    uVar6 = from_db_to_coefficient((float)(*(uint *)(param_2 + 4) ^ _DAT_180008020));
    uVar3 = *(undefined4 *)(param_2 + 0x10);
    uVar7 = from_db_to_coefficient(*(float *)(param_2 + 0x14));
    fVar4 = DAT_180008030;
    fVar8 = remap_percentage_log_like_unity(*(float *)(param_2 + 8) / DAT_180008030);
    fVar1 = *(float *)(param_2 + 0xc);
    uVar9 = from_db_to_coefficient(*(float *)(param_2 + 0x18));
    uVar2 = *param_2;
    *(undefined4 *)(param_1 + 4) = uVar6;
    *(undefined4 *)(param_1 + 8) = uVar3;
    *(undefined4 *)(param_1 + 0xc) = uVar7;
    *(float *)(param_1 + 0x10) = fVar8;
    *(float *)(param_1 + 0x14) = fVar1 / fVar4;
    *(undefined4 *)(param_1 + 0x18) = uVar9;
    *(undefined1 *)(param_1 + 0x1c) = uVar2;
    local_98 = *(undefined4 *)(param_1 + 4);
    local_94 = *(undefined4 *)(param_1 + 8);
    uStack_88 = (undefined4)((ulonglong)*(undefined8 *)(param_1 + 0x10) >> 0x20);
    local_84 = *(undefined4 *)(param_1 + 0x18);
    local_80 = *(undefined1 *)(param_1 + 0x1c);
    local_7f = *(undefined2 *)(param_1 + 0x1d);
    local_7d = *(undefined1 *)(param_1 + 0x1f);
    uStack_90 = (undefined4)*(undefined8 *)(param_1 + 0xc);
    local_8c = (undefined4)((ulonglong)*(undefined8 *)(param_1 + 0xc) >> 0x20);
    load_adaptive_reduction_parameters(*(longlong *)(param_1 + 0x20),(undefined8 *)&local_98);
    uVar5 = (ulonglong)extraout_var << 8;
  }
  if ((local_70 ^ (ulonglong)auStack_b8) == DAT_18000b000) {
    return CONCAT71((int7)(uVar5 >> 8),param_1 != 0);
  }
                    /* WARNING: Subroutine does not return */
  FUN_180006af0(local_70 ^ (ulonglong)auStack_b8);
}



/* ========================================================================
   ENTRY: 1800019e0
   NAME : specbleach_initialize
   SIG  : uint * __fastcall specbleach_initialize(uint param_1, float param_2)
   ======================================================================== */

uint * specbleach_initialize(uint param_1,float param_2)

{
  uint uVar1;
  uint uVar2;
  uint *_Memory;
  int *piVar3;
  uint *puVar4;
  uint *puVar5;
  
                    /* 0x19e0  89  specbleach_initialize */
  _Memory = calloc(1,0x40);
  *_Memory = param_1;
  piVar3 = stft_processor_initialize(param_1,param_2,4,2,0x32,0,0);
  *(int **)(_Memory + 0xe) = piVar3;
  if (piVar3 == (int *)0x0) {
    noise_profile_free((void *)0x0);
    spectral_denoiser_free((void *)0x0);
    piVar3 = (int *)0x0;
  }
  else {
    uVar1 = get_stft_fft_size((longlong)piVar3);
    uVar2 = get_stft_real_spectrum_size((longlong)piVar3);
    puVar4 = noise_profile_initialize(uVar2);
    *(uint **)(_Memory + 10) = puVar4;
    if (puVar4 == (uint *)0x0) {
      puVar4 = (uint *)0x0;
    }
    else {
      puVar5 = spectral_denoiser_initialize(param_1,uVar1,4,puVar4);
      *(uint **)(_Memory + 0xc) = puVar5;
      if (puVar5 != (uint *)0x0) {
        return _Memory;
      }
    }
    noise_profile_free(puVar4);
    spectral_denoiser_free((void *)0x0);
  }
  stft_processor_free(piVar3);
  free(_Memory);
  return (uint *)0x0;
}



/* ========================================================================
   ENTRY: 180001ad0
   NAME : specbleach_free
   SIG  : undefined __fastcall specbleach_free(void * param_1)
   ======================================================================== */

void specbleach_free(void *param_1)

{
                    /* 0x1ad0  84  specbleach_free */
  noise_profile_free(*(void **)((longlong)param_1 + 0x28));
  spectral_denoiser_free(*(void **)((longlong)param_1 + 0x30));
  stft_processor_free(*(void **)((longlong)param_1 + 0x38));
                    /* WARNING: Could not recover jumptable at 0x000180001afb. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001b10
   NAME : specbleach_get_latency
   SIG  : undefined __fastcall specbleach_get_latency(longlong param_1)
   ======================================================================== */

void specbleach_get_latency(longlong param_1)

{
                    /* 0x1b10  85  specbleach_get_latency */
  get_noise_profile_size(*(undefined4 **)(param_1 + 0x38));
  return;
}



/* ========================================================================
   ENTRY: 180001b20
   NAME : specbleach_process
   SIG  : ulonglong __fastcall specbleach_process(longlong param_1, uint param_2, longlong param_3, longlong param_4)
   ======================================================================== */

ulonglong specbleach_process(longlong param_1,uint param_2,longlong param_3,longlong param_4)

{
  undefined8 unaff_RBX;
  
                    /* 0x1b20  93  specbleach_process */
  if ((param_4 != 0 && param_3 != 0) && (param_2 != 0 && param_1 != 0)) {
    stft_processor_run(*(longlong *)(param_1 + 0x38),param_2,param_3,param_4,spectral_denoiser_run,
                       *(undefined8 *)(param_1 + 0x30));
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),
                  (param_4 != 0 && param_3 != 0) && (param_2 != 0 && param_1 != 0)) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 180001b70
   NAME : specbleach_get_noise_profile_blocks_averaged
   SIG  : undefined __fastcall specbleach_get_noise_profile_blocks_averaged(longlong param_1)
   ======================================================================== */

void specbleach_get_noise_profile_blocks_averaged(longlong param_1)

{
                    /* 0x1b70  87  specbleach_get_noise_profile_blocks_averaged */
  get_noise_profile_blocks_averaged(*(longlong *)(param_1 + 0x28));
  return;
}



/* ========================================================================
   ENTRY: 180001b80
   NAME : specbleach_get_noise_profile
   SIG  : undefined __fastcall specbleach_get_noise_profile(longlong param_1)
   ======================================================================== */

void specbleach_get_noise_profile(longlong param_1)

{
                    /* 0x1b80  86  specbleach_get_noise_profile */
  get_noise_profile(*(longlong *)(param_1 + 0x28));
  return;
}



/* ========================================================================
   ENTRY: 180001b90
   NAME : specbleach_load_noise_profile
   SIG  : undefined8 __fastcall specbleach_load_noise_profile(longlong param_1, void * param_2, uint param_3, uint param_4)
   ======================================================================== */

undefined8 specbleach_load_noise_profile(longlong param_1,void *param_2,uint param_3,uint param_4)

{
  uint uVar1;
  undefined8 uVar2;
  
                    /* 0x1b90  90  specbleach_load_noise_profile */
  if ((param_2 != (void *)0x0 && param_1 != 0) &&
     (uVar1 = get_noise_profile_size(*(undefined4 **)(param_1 + 0x28)), uVar1 == param_3)) {
    uVar2 = set_noise_profile(*(uint **)(param_1 + 0x28),param_2,param_3,param_4);
    return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180001bf0
   NAME : specbleach_reset_noise_profile
   SIG  : bool __fastcall specbleach_reset_noise_profile(longlong param_1)
   ======================================================================== */

bool specbleach_reset_noise_profile(longlong param_1)

{
                    /* 0x1bf0  94  specbleach_reset_noise_profile */
  if (param_1 != 0) {
    reset_noise_profile(*(uint **)(param_1 + 0x28));
  }
  return param_1 != 0;
}



/* ========================================================================
   ENTRY: 180001c20
   NAME : specbleach_noise_profile_available
   SIG  : undefined __fastcall specbleach_noise_profile_available(longlong param_1)
   ======================================================================== */

void specbleach_noise_profile_available(longlong param_1)

{
                    /* 0x1c20  92  specbleach_noise_profile_available */
  is_noise_estimation_available(*(longlong *)(param_1 + 0x28));
  return;
}



/* ========================================================================
   ENTRY: 180001c30
   NAME : specbleach_load_parameters
   SIG  : undefined8 __fastcall specbleach_load_parameters(longlong param_1, undefined4 * param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 specbleach_load_parameters(longlong param_1,undefined4 *param_2)

{
  float fVar1;
  undefined1 uVar2;
  undefined1 uVar3;
  undefined4 uVar4;
  undefined4 uVar5;
  float fVar6;
  ulonglong uVar7;
  uint7 extraout_var;
  undefined4 uVar8;
  undefined4 uVar9;
  float fVar10;
  undefined4 uVar11;
  undefined1 auStack_d8 [32];
  undefined4 local_b8;
  undefined4 local_b4;
  undefined4 local_b0;
  undefined1 local_ac;
  undefined1 local_ab;
  undefined2 local_aa;
  undefined4 local_a8;
  undefined8 local_a4;
  undefined4 local_9c;
  ulonglong local_90;
  
                    /* 0x1c30  91  specbleach_load_parameters */
  uVar7 = DAT_18000b000 ^ (ulonglong)auStack_d8;
  local_90 = uVar7;
  if (param_1 != 0) {
    uVar8 = from_db_to_coefficient((float)(param_2[2] ^ _DAT_180008020));
    uVar4 = param_2[6];
    uVar9 = from_db_to_coefficient((float)param_2[7]);
    fVar6 = DAT_180008030;
    uVar2 = *(undefined1 *)(param_2 + 1);
    uVar3 = *(undefined1 *)(param_2 + 4);
    uVar5 = *param_2;
    fVar10 = remap_percentage_log_like_unity((float)param_2[3] / DAT_180008030);
    fVar1 = (float)param_2[5];
    uVar11 = from_db_to_coefficient((float)param_2[8]);
    *(undefined4 *)(param_1 + 4) = uVar8;
    *(undefined4 *)(param_1 + 8) = uVar4;
    *(undefined4 *)(param_1 + 0xc) = uVar9;
    *(undefined1 *)(param_1 + 0x10) = uVar2;
    *(undefined1 *)(param_1 + 0x11) = uVar3;
    *(undefined4 *)(param_1 + 0x14) = uVar5;
    *(float *)(param_1 + 0x18) = fVar10;
    *(float *)(param_1 + 0x1c) = fVar1 / fVar6;
    *(undefined4 *)(param_1 + 0x20) = uVar11;
    local_b8 = *(undefined4 *)(param_1 + 4);
    local_b4 = *(undefined4 *)(param_1 + 8);
    local_b0 = *(undefined4 *)(param_1 + 0xc);
    local_ac = *(undefined1 *)(param_1 + 0x10);
    local_ab = *(undefined1 *)(param_1 + 0x11);
    local_aa = *(undefined2 *)(param_1 + 0x12);
    local_a8 = *(undefined4 *)(param_1 + 0x14);
    local_a4 = *(undefined8 *)(param_1 + 0x18);
    local_9c = *(undefined4 *)(param_1 + 0x20);
    load_reduction_parameters(*(longlong *)(param_1 + 0x30),(undefined8 *)&local_b8);
    uVar7 = (ulonglong)extraout_var << 8;
  }
  if ((local_90 ^ (ulonglong)auStack_d8) == DAT_18000b000) {
    return CONCAT71((int7)(uVar7 >> 8),param_1 != 0);
  }
                    /* WARNING: Subroutine does not return */
  FUN_180006af0(local_90 ^ (ulonglong)auStack_d8);
}



/* ========================================================================
   ENTRY: 180001dc0
   NAME : estimate_gains
   SIG  : undefined __fastcall estimate_gains(uint param_1, uint param_2, longlong param_3, longlong param_4, longlong param_5, longlong param_6, longlong param_7, int param_8)
   ======================================================================== */

void estimate_gains(uint param_1,uint param_2,longlong param_3,longlong param_4,longlong param_5,
                   longlong param_6,longlong param_7,int param_8)

{
  float *pfVar1;
  float *pfVar2;
  float *pfVar3;
  float fVar4;
  float fVar5;
  ulonglong uVar6;
  undefined1 auVar7 [16];
  float fVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  ulonglong uVar18;
  longlong lVar19;
  uint uVar20;
  ulonglong uVar21;
  ulonglong uVar22;
  float fVar23;
  undefined4 extraout_XMM0_Db;
  undefined8 extraout_XMM0_Qb;
  undefined1 auVar24 [16];
  float fVar25;
  
  fVar4 = DAT_180008038;
  fVar9 = DAT_180008034;
                    /* 0x1dc0  15  estimate_gains */
  fVar8 = DAT_180008010;
  if (param_8 != 0) {
    if (param_8 == 2) {
      if (param_1 < 2) {
        return;
      }
      lVar19 = 0;
      do {
        param_2 = param_2 - 1;
        fVar25 = *(float *)(param_3 + 4 + lVar19);
        fVar23 = fVar8;
        if (fVar9 < fVar25) {
          fVar25 = *(float *)(param_4 + 4 + lVar19) / fVar25;
          fVar25 = fVar25 * fVar25;
          fVar23 = *(float *)(param_6 + 4 + lVar19);
          fVar5 = *(float *)(param_7 + 4 + lVar19);
          uVar20 = -(uint)(fVar25 < fVar8 / (fVar23 + fVar5));
          auVar24._0_4_ =
               powf((float)(uVar20 & (uint)(fVar8 - fVar23 * fVar25) |
                           ~uVar20 & (uint)(fVar5 * fVar25)),fVar4);
          auVar24._4_4_ = extraout_XMM0_Db;
          auVar24._8_8_ = extraout_XMM0_Qb;
          if (auVar24._0_4_ <= 0.0) {
            auVar7._12_4_ = 0;
            auVar7._0_12_ = auVar24._4_12_;
            auVar24 = auVar7 << 0x20;
          }
          fVar23 = auVar24._0_4_;
        }
        *(float *)(param_5 + 4 + lVar19) = fVar23;
        *(float *)(param_5 + (ulonglong)param_2 * 4) = fVar23;
        lVar19 = lVar19 + 4;
      } while ((ulonglong)param_1 * 4 + -4 != lVar19);
      return;
    }
    if (param_8 != 1) {
      return;
    }
    if (param_1 < 2) {
      return;
    }
    uVar6 = (ulonglong)param_1;
    uVar18 = 1;
    if ((8 < param_1) &&
       (param_6 + uVar6 * 4 <= param_4 + 4U || param_4 + uVar6 * 4 <= param_6 + 4U)) {
      uVar21 = uVar6 - 1 & 0xfffffffffffffff8;
      uVar18 = uVar21 + 1;
      uVar22 = 0;
      do {
        pfVar1 = (float *)(param_6 + 4 + uVar22 * 4);
        fVar8 = pfVar1[1];
        fVar9 = pfVar1[2];
        fVar4 = pfVar1[3];
        pfVar2 = (float *)(param_6 + 0x14 + uVar22 * 4);
        fVar25 = *pfVar2;
        fVar23 = pfVar2[1];
        fVar5 = pfVar2[2];
        fVar10 = pfVar2[3];
        pfVar2 = (float *)(param_4 + 4 + uVar22 * 4);
        fVar11 = pfVar2[1];
        fVar12 = pfVar2[2];
        fVar13 = pfVar2[3];
        pfVar3 = (float *)(param_4 + 0x14 + uVar22 * 4);
        fVar14 = *pfVar3;
        fVar15 = pfVar3[1];
        fVar16 = pfVar3[2];
        fVar17 = pfVar3[3];
        pfVar3 = (float *)(param_4 + 4 + uVar22 * 4);
        *pfVar3 = *pfVar2 * *pfVar1;
        pfVar3[1] = fVar11 * fVar8;
        pfVar3[2] = fVar12 * fVar9;
        pfVar3[3] = fVar13 * fVar4;
        pfVar1 = (float *)(param_4 + 0x14 + uVar22 * 4);
        *pfVar1 = fVar14 * fVar25;
        pfVar1[1] = fVar15 * fVar23;
        pfVar1[2] = fVar16 * fVar5;
        pfVar1[3] = fVar17 * fVar10;
        uVar22 = uVar22 + 8;
      } while (uVar21 != uVar22);
      if (uVar6 - 1 == uVar21) goto LAB_180001f42;
    }
    uVar20 = param_1 - (int)uVar18 & 3;
    uVar22 = (ulonglong)uVar20;
    uVar21 = uVar18;
    if (uVar20 != 0) {
      do {
        *(float *)(param_4 + uVar21 * 4) =
             *(float *)(param_6 + uVar21 * 4) * *(float *)(param_4 + uVar21 * 4);
        uVar21 = uVar21 + 1;
        uVar22 = uVar22 - 1;
      } while (uVar22 != 0);
    }
    if (uVar18 - uVar6 < 0xfffffffffffffffd) {
      do {
        *(float *)(param_4 + uVar21 * 4) =
             *(float *)(param_6 + uVar21 * 4) * *(float *)(param_4 + uVar21 * 4);
        *(float *)(param_4 + 4 + uVar21 * 4) =
             *(float *)(param_6 + 4 + uVar21 * 4) * *(float *)(param_4 + 4 + uVar21 * 4);
        *(float *)(param_4 + 8 + uVar21 * 4) =
             *(float *)(param_6 + 8 + uVar21 * 4) * *(float *)(param_4 + 8 + uVar21 * 4);
        *(float *)(param_4 + 0xc + uVar21 * 4) =
             *(float *)(param_6 + 0xc + uVar21 * 4) * *(float *)(param_4 + 0xc + uVar21 * 4);
        uVar21 = uVar21 + 4;
      } while (uVar6 != uVar21);
    }
LAB_180001f42:
    fVar9 = DAT_180008034;
    fVar8 = DAT_180008010;
    uVar18 = 1;
    do {
      param_2 = param_2 - 1;
      fVar4 = *(float *)(param_4 + uVar18 * 4);
      fVar25 = fVar8;
      if (fVar9 < fVar4) {
        fVar25 = (float)(~-(uint)(*(float *)(param_3 + uVar18 * 4) < fVar4) & (uint)fVar8);
      }
      *(float *)(param_5 + uVar18 * 4) = fVar25;
      *(float *)(param_5 + (ulonglong)param_2 * 4) = fVar25;
      uVar18 = uVar18 + 1;
    } while (uVar6 != uVar18);
    return;
  }
  if (param_1 < 2) {
    return;
  }
  uVar6 = (ulonglong)param_1;
  uVar18 = 1;
  if ((8 < param_1) && (param_6 + uVar6 * 4 <= param_4 + 4U || param_4 + uVar6 * 4 <= param_6 + 4U))
  {
    uVar21 = uVar6 - 1 & 0xfffffffffffffff8;
    uVar18 = uVar21 + 1;
    uVar22 = 0;
    do {
      pfVar1 = (float *)(param_6 + 4 + uVar22 * 4);
      fVar8 = pfVar1[1];
      fVar9 = pfVar1[2];
      fVar4 = pfVar1[3];
      pfVar2 = (float *)(param_6 + 0x14 + uVar22 * 4);
      fVar25 = *pfVar2;
      fVar23 = pfVar2[1];
      fVar5 = pfVar2[2];
      fVar10 = pfVar2[3];
      pfVar2 = (float *)(param_4 + 4 + uVar22 * 4);
      fVar11 = pfVar2[1];
      fVar12 = pfVar2[2];
      fVar13 = pfVar2[3];
      pfVar3 = (float *)(param_4 + 0x14 + uVar22 * 4);
      fVar14 = *pfVar3;
      fVar15 = pfVar3[1];
      fVar16 = pfVar3[2];
      fVar17 = pfVar3[3];
      pfVar3 = (float *)(param_4 + 4 + uVar22 * 4);
      *pfVar3 = *pfVar2 * *pfVar1;
      pfVar3[1] = fVar11 * fVar8;
      pfVar3[2] = fVar12 * fVar9;
      pfVar3[3] = fVar13 * fVar4;
      pfVar1 = (float *)(param_4 + 0x14 + uVar22 * 4);
      *pfVar1 = fVar14 * fVar25;
      pfVar1[1] = fVar15 * fVar23;
      pfVar1[2] = fVar16 * fVar5;
      pfVar1[3] = fVar17 * fVar10;
      uVar22 = uVar22 + 8;
    } while (uVar21 != uVar22);
    if (uVar6 - 1 == uVar21) goto LAB_1800021a2;
  }
  uVar20 = param_1 - (int)uVar18 & 3;
  uVar22 = (ulonglong)uVar20;
  uVar21 = uVar18;
  if (uVar20 != 0) {
    do {
      *(float *)(param_4 + uVar21 * 4) =
           *(float *)(param_6 + uVar21 * 4) * *(float *)(param_4 + uVar21 * 4);
      uVar21 = uVar21 + 1;
      uVar22 = uVar22 - 1;
    } while (uVar22 != 0);
  }
  if (uVar18 - uVar6 < 0xfffffffffffffffd) {
    do {
      *(float *)(param_4 + uVar21 * 4) =
           *(float *)(param_6 + uVar21 * 4) * *(float *)(param_4 + uVar21 * 4);
      *(float *)(param_4 + 4 + uVar21 * 4) =
           *(float *)(param_6 + 4 + uVar21 * 4) * *(float *)(param_4 + 4 + uVar21 * 4);
      *(float *)(param_4 + 8 + uVar21 * 4) =
           *(float *)(param_6 + 8 + uVar21 * 4) * *(float *)(param_4 + 8 + uVar21 * 4);
      *(float *)(param_4 + 0xc + uVar21 * 4) =
           *(float *)(param_6 + 0xc + uVar21 * 4) * *(float *)(param_4 + 0xc + uVar21 * 4);
      uVar21 = uVar21 + 4;
    } while (uVar6 != uVar21);
  }
LAB_1800021a2:
  fVar9 = DAT_180008034;
  fVar8 = DAT_180008010;
  uVar18 = 1;
  do {
    param_2 = param_2 - 1;
    fVar4 = *(float *)(param_4 + uVar18 * 4);
    fVar25 = fVar8;
    if (fVar9 < fVar4) {
      fVar23 = *(float *)(param_3 + uVar18 * 4);
      fVar25 = 0.0;
      if (fVar4 < fVar23) {
        fVar25 = (fVar23 - fVar4) / fVar23;
      }
    }
    *(float *)(param_5 + uVar18 * 4) = fVar25;
    *(float *)(param_5 + (ulonglong)param_2 * 4) = fVar25;
    uVar18 = uVar18 + 1;
  } while (uVar6 != uVar18);
  return;
}



/* ========================================================================
   ENTRY: 180002230
   NAME : louizou_estimator_initialize
   SIG  : uint * __fastcall louizou_estimator_initialize(uint param_1, uint param_2, uint param_3)
   ======================================================================== */

uint * louizou_estimator_initialize(uint param_1,uint param_2,uint param_3)

{
  undefined4 uVar1;
  uint *puVar2;
  void *pvVar3;
  void *pvVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  undefined8 *puVar8;
  ulonglong _Count;
  undefined4 uVar9;
  undefined4 uVar10;
  
                    /* 0x2230  56  louizou_estimator_initialize */
  puVar2 = calloc(1,0x38);
  *puVar2 = param_1;
  _Count = (ulonglong)param_1;
  pvVar3 = calloc(_Count,4);
  *(void **)(puVar2 + 6) = pvVar3;
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar2 + 10) = pvVar4;
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar2 + 0xc) = pvVar4;
  pvVar4 = calloc(_Count,4);
  *(void **)(puVar2 + 8) = pvVar4;
  uVar5 = freq_to_fft_bin(DAT_18000803c,param_2,param_3);
  uVar5 = uVar5 & 0xffffffff;
  uVar6 = freq_to_fft_bin(DAT_180008040,param_2,param_3);
  uVar1 = DAT_180008048;
  uVar9 = DAT_180008044;
  if (param_1 != 0) {
    uVar6 = uVar6 & 0xffffffff;
    if (param_1 == 1) {
      uVar7 = 0;
    }
    else {
      uVar7 = 0;
      do {
        uVar10 = uVar1;
        if (uVar5 < uVar7) {
          if (uVar7 < uVar6) {
            uVar10 = uVar9;
          }
LAB_180002370:
          *(undefined4 *)((longlong)pvVar3 + uVar7 * 4) = uVar10;
        }
        else {
          *(undefined4 *)((longlong)pvVar3 + uVar7 * 4) = 0x40000000;
          if (uVar6 <= uVar7) goto LAB_180002370;
        }
        if (uVar7 < uVar5) {
          *(undefined4 *)((longlong)pvVar3 + uVar7 * 4 + 4) = 0x40000000;
          if (uVar6 <= uVar7 + 1) {
LAB_180002330:
            uVar10 = uVar1;
            goto LAB_180002333;
          }
        }
        else {
          uVar10 = uVar9;
          if (uVar6 <= uVar7 + 1) goto LAB_180002330;
LAB_180002333:
          *(undefined4 *)((longlong)pvVar3 + uVar7 * 4 + 4) = uVar10;
        }
        uVar7 = uVar7 + 2;
      } while ((param_1 & 0xfffffffe) != uVar7);
    }
    if ((param_1 & 1) != 0) {
      if (uVar5 < uVar7) {
        uVar9 = DAT_180008048;
        if (uVar7 < uVar6) {
          uVar9 = DAT_180008044;
        }
      }
      else {
        *(undefined4 *)((longlong)pvVar3 + uVar7 * 4) = 0x40000000;
        uVar9 = DAT_180008048;
        if (uVar7 < uVar6) goto LAB_1800023b5;
      }
      *(undefined4 *)((longlong)pvVar3 + uVar7 * 4) = uVar9;
    }
  }
LAB_1800023b5:
  puVar8 = calloc(1,0x18);
  pvVar3 = calloc(_Count,4);
  *puVar8 = pvVar3;
  pvVar3 = calloc(_Count,4);
  puVar8[1] = pvVar3;
  pvVar4 = calloc(_Count,4);
  puVar8[2] = pvVar4;
  uVar9 = DAT_180008034;
  initialize_spectrum_with_value((longlong)pvVar3,param_1,DAT_180008034);
  *(undefined8 **)(puVar2 + 2) = puVar8;
  puVar8 = calloc(1,0x18);
  pvVar3 = calloc(_Count,4);
  *puVar8 = pvVar3;
  pvVar3 = calloc(_Count,4);
  puVar8[1] = pvVar3;
  pvVar4 = calloc(_Count,4);
  puVar8[2] = pvVar4;
  initialize_spectrum_with_value((longlong)pvVar3,param_1,uVar9);
  *(undefined8 **)(puVar2 + 4) = puVar8;
  puVar2[1] = 0;
  return puVar2;
}



/* ========================================================================
   ENTRY: 180002480
   NAME : louizou_estimator_free
   SIG  : undefined __fastcall louizou_estimator_free(void * param_1)
   ======================================================================== */

void louizou_estimator_free(void *param_1)

{
  undefined8 *puVar1;
  
                    /* 0x2480  55  louizou_estimator_free */
  free(*(void **)((longlong)param_1 + 0x18));
  free(*(void **)((longlong)param_1 + 0x28));
  free(*(void **)((longlong)param_1 + 0x30));
  free(*(void **)((longlong)param_1 + 0x20));
  puVar1 = *(undefined8 **)((longlong)param_1 + 8);
  free((void *)*puVar1);
  free((void *)puVar1[1]);
  free((void *)puVar1[2]);
  free(puVar1);
  puVar1 = *(undefined8 **)((longlong)param_1 + 0x10);
  free((void *)*puVar1);
  free((void *)puVar1[1]);
  free((void *)puVar1[2]);
  free(puVar1);
                    /* WARNING: Could not recover jumptable at 0x0001800024ea. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800024f0
   NAME : louizou_estimator_run
   SIG  : ulonglong __fastcall louizou_estimator_run(uint * param_1, longlong param_2, void * param_3)
   ======================================================================== */

ulonglong louizou_estimator_run(uint *param_1,longlong param_2,void *param_3)

{
  float *pfVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  undefined8 unaff_RBX;
  ulonglong uVar12;
  ulonglong uVar13;
  float fVar14;
  float fVar15;
  undefined8 uVar16;
  
  fVar11 = DAT_18000806c;
  fVar10 = DAT_180008068;
  fVar9 = DAT_180008064;
  fVar8 = DAT_180008060;
  fVar7 = DAT_18000805c;
  fVar6 = DAT_180008058;
  fVar5 = DAT_180008054;
  fVar4 = DAT_180008050;
  fVar3 = DAT_18000804c;
                    /* 0x24f0  57  louizou_estimator_run */
  fVar2 = DAT_180008010;
  if (param_3 != (void *)0x0 && (param_2 != 0 && param_1 != (uint *)0x0)) {
    uVar12 = (ulonglong)*param_1;
    if (1 < uVar12) {
      uVar13 = 1;
      do {
        *(float *)(**(longlong **)(param_1 + 2) + uVar13 * 4) =
             *(float *)(**(longlong **)(param_1 + 4) + uVar13 * 4) * fVar4 +
             *(float *)(param_2 + uVar13 * 4) * fVar3;
        fVar14 = *(float *)((*(longlong **)(param_1 + 4))[1] + uVar13 * 4);
        fVar15 = *(float *)(**(longlong **)(param_1 + 2) + uVar13 * 4);
        if (fVar14 < fVar15) {
          fVar15 = fVar14 * fVar7 +
                   (*(float *)(**(longlong **)(param_1 + 4) + uVar13 * 4) * fVar5 + fVar15) * fVar6;
        }
        *(float *)((*(longlong **)(param_1 + 2))[1] + uVar13 * 4) = fVar15;
        uVar16 = sanitize_denormal((ulonglong)
                                   (uint)(*(float *)(**(longlong **)(param_1 + 2) + uVar13 * 4) /
                                         *(float *)((*(longlong **)(param_1 + 2))[1] + uVar13 * 4)))
        ;
        fVar14 = (float)uVar16;
        param_1[1] = (uint)fVar14;
        pfVar1 = (float *)(*(longlong *)(param_1 + 6) + uVar13 * 4);
        *(uint *)(*(longlong *)(param_1 + 0xc) + uVar13 * 4) =
             (uint)(*pfVar1 <= fVar14 && fVar14 != *pfVar1);
        *(float *)(*(longlong *)(*(longlong *)(param_1 + 2) + 0x10) + uVar13 * 4) =
             *(float *)(*(longlong *)(*(longlong *)(param_1 + 4) + 0x10) + uVar13 * 4) * fVar9 +
             (float)*(uint *)(*(longlong *)(param_1 + 0xc) + uVar13 * 4) * fVar8;
        *(float *)(*(longlong *)(param_1 + 10) + uVar13 * 4) =
             *(float *)(*(longlong *)(*(longlong *)(param_1 + 2) + 0x10) + uVar13 * 4) * fVar10 +
             fVar11;
        fVar14 = *(float *)(*(longlong *)(param_1 + 10) + uVar13 * 4);
        *(float *)((longlong)param_3 + uVar13 * 4) =
             fVar14 * *(float *)(*(longlong *)(param_1 + 8) + uVar13 * 4) +
             (fVar2 - fVar14) * *(float *)(param_2 + uVar13 * 4);
        uVar13 = uVar13 + 1;
        uVar12 = (ulonglong)*param_1;
      } while (uVar13 < uVar12);
    }
    memcpy(*(void **)(param_1 + 8),param_3,uVar12 << 2);
    memcpy(*(void **)(*(longlong *)(param_1 + 4) + 8),*(void **)(*(longlong *)(param_1 + 2) + 8),
           (ulonglong)*param_1 << 2);
    memcpy((void *)**(undefined8 **)(param_1 + 4),(void *)**(undefined8 **)(param_1 + 2),
           (ulonglong)*param_1 << 2);
    memcpy(*(void **)(*(longlong *)(param_1 + 4) + 0x10),
           *(void **)(*(longlong *)(param_1 + 2) + 0x10),(ulonglong)*param_1 << 2);
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),
                  param_3 != (void *)0x0 && (param_2 != 0 && param_1 != (uint *)0x0)) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 1800027f0
   NAME : noise_estimation_initialize
   SIG  : uint * __fastcall noise_estimation_initialize(uint param_1, undefined8 param_2)
   ======================================================================== */

uint * noise_estimation_initialize(uint param_1,undefined8 param_2)

{
  uint *puVar1;
  uint *puVar2;
  uint uVar3;
  
                    /* 0x27f0  65  noise_estimation_initialize */
  puVar1 = calloc(1,0x18);
  *puVar1 = param_1;
  uVar3 = (param_1 >> 1) + 1;
  puVar1[1] = uVar3;
  *(undefined8 *)(puVar1 + 4) = param_2;
  puVar2 = spectral_trailing_buffer_initialize(uVar3,5);
  *(uint **)(puVar1 + 2) = puVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180002840
   NAME : noise_estimation_free
   SIG  : undefined __fastcall noise_estimation_free(void * param_1)
   ======================================================================== */

void noise_estimation_free(void *param_1)

{
                    /* 0x2840  64  noise_estimation_free */
  noise_profile_free(*(void **)((longlong)param_1 + 8));
                    /* WARNING: Could not recover jumptable at 0x000180002859. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180002860
   NAME : noise_estimation_run
   SIG  : ulonglong __fastcall noise_estimation_run(longlong param_1, int param_2, void * param_3)
   ======================================================================== */

ulonglong noise_estimation_run(longlong param_1,int param_2,void *param_3)

{
  uint uVar1;
  uint uVar2;
  ulonglong uVar3;
  longlong lVar4;
  undefined8 unaff_RBX;
  
                    /* 0x2860  66  noise_estimation_run */
  if (param_3 == (void *)0x0 || param_1 == 0) goto LAB_18000292b;
  uVar3 = get_noise_profile(*(longlong *)(param_1 + 0x10));
  if (param_2 == 3) {
    max_spectrum(uVar3,(ulonglong)param_3,*(uint *)(param_1 + 4));
  }
  else {
    if (param_2 != 2) {
      if (param_2 == 1) {
        uVar2 = *(uint *)(param_1 + 4);
        uVar1 = get_noise_profile_blocks_averaged(*(longlong *)(param_1 + 0x10));
        get_rolling_mean_spectrum(uVar3,(longlong)param_3,uVar1,uVar2);
        increment_blocks_averaged(*(longlong *)(param_1 + 0x10));
      }
      goto LAB_18000292b;
    }
    spectral_trailing_buffer_push_back(*(uint **)(param_1 + 8),param_3);
    uVar2 = get_noise_profile_size(*(undefined4 **)(param_1 + 8));
    uVar1 = get_noise_profile_blocks_averaged(*(longlong *)(param_1 + 8));
    lVar4 = get_noise_profile(*(longlong *)(param_1 + 8));
    uVar3 = get_rolling_median_spectrum(uVar3,lVar4,(ulonglong)uVar1,uVar2);
    if ((char)uVar3 == '\0') goto LAB_18000292b;
  }
  set_noise_profile_available(*(longlong *)(param_1 + 0x10));
LAB_18000292b:
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_3 != (void *)0x0 && param_1 != 0) &
         0xffffffff;
}



/* ========================================================================
   ENTRY: 180002940
   NAME : noise_profile_initialize
   SIG  : uint * __fastcall noise_profile_initialize(uint param_1)
   ======================================================================== */

uint * noise_profile_initialize(uint param_1)

{
  uint *puVar1;
  void *pvVar2;
  
                    /* 0x2940  68  noise_profile_initialize */
  puVar1 = calloc(1,0x18);
  *puVar1 = param_1;
  pvVar2 = calloc((ulonglong)param_1,4);
  *(void **)(puVar1 + 2) = pvVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180002980
   NAME : noise_profile_free
   SIG  : undefined __fastcall noise_profile_free(void * param_1)
   ======================================================================== */

void noise_profile_free(void *param_1)

{
                    /* 0x2980  67  noise_profile_free
                       0x2980  107  spectral_trailing_buffer_free */
  free(*(void **)((longlong)param_1 + 8));
                    /* WARNING: Could not recover jumptable at 0x0001800029a2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800029b0
   NAME : is_noise_estimation_available
   SIG  : undefined1 __fastcall is_noise_estimation_available(longlong param_1)
   ======================================================================== */

undefined1 is_noise_estimation_available(longlong param_1)

{
                    /* 0x29b0  52  is_noise_estimation_available */
  return *(undefined1 *)(param_1 + 0x10);
}



/* ========================================================================
   ENTRY: 1800029c0
   NAME : get_noise_profile
   SIG  : undefined8 __fastcall get_noise_profile(longlong param_1)
   ======================================================================== */

undefined8 get_noise_profile(longlong param_1)

{
                    /* 0x29c0  34  get_noise_profile
                       0x29c0  38  get_phase_spectrum
                       0x29c0  48  get_trailing_spectral_buffer */
  return *(undefined8 *)(param_1 + 8);
}



/* ========================================================================
   ENTRY: 1800029d0
   NAME : get_noise_profile_size
   SIG  : undefined4 __fastcall get_noise_profile_size(undefined4 * param_1)
   ======================================================================== */

undefined4 get_noise_profile_size(undefined4 *param_1)

{
                    /* 0x29d0  36  get_noise_profile_size
                       0x29d0  44  get_spectrum_size
                       0x29d0  46  get_stft_latency */
  return *param_1;
}



/* ========================================================================
   ENTRY: 1800029e0
   NAME : get_noise_profile_blocks_averaged
   SIG  : undefined4 __fastcall get_noise_profile_blocks_averaged(longlong param_1)
   ======================================================================== */

undefined4 get_noise_profile_blocks_averaged(longlong param_1)

{
                    /* 0x29e0  35  get_noise_profile_blocks_averaged
                       0x29e0  43  get_spectrum_buffer_size */
  return *(undefined4 *)(param_1 + 4);
}



/* ========================================================================
   ENTRY: 1800029f0
   NAME : set_noise_profile_available
   SIG  : undefined __fastcall set_noise_profile_available(longlong param_1)
   ======================================================================== */

void set_noise_profile_available(longlong param_1)

{
                    /* 0x29f0  78  set_noise_profile_available */
  *(undefined1 *)(param_1 + 0x10) = 1;
  return;
}



/* ========================================================================
   ENTRY: 180002a00
   NAME : set_noise_profile
   SIG  : undefined8 __fastcall set_noise_profile(uint * param_1, void * param_2, uint param_3, uint param_4)
   ======================================================================== */

undefined8 set_noise_profile(uint *param_1,void *param_2,uint param_3,uint param_4)

{
  void *pvVar1;
  undefined8 uVar2;
  
                    /* 0x2a00  77  set_noise_profile */
  if ((param_2 == (void *)0x0 || param_1 == (uint *)0x0) || (*param_1 != param_3)) {
    uVar2 = 0;
  }
  else {
    pvVar1 = memcpy(*(void **)(param_1 + 2),param_2,(ulonglong)param_3 << 2);
    *param_1 = param_3;
    param_1[1] = param_4;
    *(undefined1 *)(param_1 + 4) = 1;
    uVar2 = CONCAT71((int7)((ulonglong)pvVar1 >> 8),1);
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180002a60
   NAME : increment_blocks_averaged
   SIG  : bool __fastcall increment_blocks_averaged(longlong param_1)
   ======================================================================== */

bool increment_blocks_averaged(longlong param_1)

{
  uint uVar1;
  
                    /* 0x2a60  49  increment_blocks_averaged */
  if (((param_1 != 0) &&
      (uVar1 = *(int *)(param_1 + 4) + 1, *(uint *)(param_1 + 4) = uVar1, 5 < uVar1)) &&
     (*(char *)(param_1 + 0x10) == '\0')) {
    *(undefined1 *)(param_1 + 0x10) = 1;
  }
  return param_1 != 0;
}



/* ========================================================================
   ENTRY: 180002a90
   NAME : reset_noise_profile
   SIG  : bool __fastcall reset_noise_profile(uint * param_1)
   ======================================================================== */

bool reset_noise_profile(uint *param_1)

{
                    /* 0x2a90  75  reset_noise_profile */
  if (param_1 != (uint *)0x0) {
    initialize_spectrum_with_value(*(longlong *)(param_1 + 2),*param_1,0);
    param_1[1] = 0;
    *(undefined1 *)(param_1 + 4) = 0;
  }
  return param_1 != (uint *)0x0;
}



/* ========================================================================
   ENTRY: 180002ad0
   NAME : postfilter_initialize
   SIG  : undefined8 * __fastcall postfilter_initialize(uint param_1)
   ======================================================================== */

undefined8 * postfilter_initialize(uint param_1)

{
  undefined8 *puVar1;
  undefined8 *puVar2;
  void *pvVar3;
  
                    /* 0x2ad0  73  postfilter_initialize */
  puVar1 = calloc(1,0x30);
  *(uint *)(puVar1 + 4) = param_1;
  *(uint *)((longlong)puVar1 + 0x24) = (param_1 >> 1) + 1;
  *(undefined1 *)(puVar1 + 5) = 1;
  *(undefined4 *)((longlong)puVar1 + 0x2c) = 0x41a00000;
  puVar2 = fft_transform_initialize_bins(param_1);
  *puVar1 = puVar2;
  puVar2 = fft_transform_initialize_bins(param_1);
  puVar1[1] = puVar2;
  pvVar3 = calloc((ulonglong)param_1,4);
  puVar1[3] = pvVar3;
  pvVar3 = calloc((ulonglong)param_1,4);
  puVar1[2] = pvVar3;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180002b50
   NAME : postfilter_free
   SIG  : undefined __fastcall postfilter_free(undefined8 * param_1)
   ======================================================================== */

void postfilter_free(undefined8 *param_1)

{
                    /* 0x2b50  72  postfilter_free */
  fft_transform_free((undefined8 *)*param_1);
  fft_transform_free((undefined8 *)param_1[1]);
  free((void *)param_1[2]);
  free((void *)param_1[3]);
                    /* WARNING: Could not recover jumptable at 0x000180002b89. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180002b90
   NAME : postfilter_apply
   SIG  : ulonglong __fastcall postfilter_apply(longlong * param_1, longlong param_2, void * param_3, float param_4)
   ======================================================================== */

ulonglong postfilter_apply(longlong *param_1,longlong param_2,void *param_3,float param_4)

{
  float fVar1;
  char cVar2;
  void *_Src;
  ulonglong uVar3;
  longlong lVar4;
  undefined8 unaff_RBX;
  uint uVar5;
  ulonglong uVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  
                    /* 0x2b90  71  postfilter_apply */
  if (param_3 != (void *)0x0 && param_2 != 0) {
    memcpy((void *)param_1[3],param_3,(ulonglong)*(uint *)(param_1 + 4) << 2);
    fVar8 = DAT_180008010;
    lVar4 = param_1[3];
    uVar5 = *(uint *)((longlong)param_1 + 0x24);
    fVar7 = DAT_180008070;
    if (uVar5 != 0) {
      if (uVar5 == 1) {
        fVar7 = 0.0;
        fVar9 = 0.0;
        uVar3 = 0;
      }
      else {
        fVar7 = 0.0;
        fVar9 = 0.0;
        uVar3 = 0;
        do {
          fVar1 = *(float *)(param_2 + uVar3 * 4);
          fVar11 = *(float *)(param_2 + 4 + uVar3 * 4);
          fVar12 = *(float *)(lVar4 + uVar3 * 4) * fVar1;
          fVar10 = *(float *)(lVar4 + 4 + uVar3 * 4) * fVar11;
          fVar7 = fVar11 * fVar11 + fVar1 * fVar1 + fVar7;
          fVar9 = fVar10 * fVar10 + fVar12 * fVar12 + fVar9;
          uVar3 = uVar3 + 2;
        } while ((uVar5 & 0xfffffffe) != uVar3);
      }
      if ((uVar5 & 1) != 0) {
        fVar1 = *(float *)(param_2 + uVar3 * 4);
        fVar11 = *(float *)(lVar4 + uVar3 * 4) * fVar1;
        fVar7 = fVar7 + fVar1 * fVar1;
        fVar9 = fVar9 + fVar11 * fVar11;
      }
      fVar7 = fVar9 / fVar7;
    }
    fVar7 = (float)(~-(uint)(fVar7 < param_4) & (uint)DAT_180008010 |
                   (uint)fVar7 & -(uint)(fVar7 < param_4));
    if ((fVar7 != DAT_180008010) || (NAN(fVar7) || NAN(DAT_180008010))) {
      fVar7 = (float)roundf((DAT_180008010 - fVar7 / param_4) * *(float *)((longlong)param_1 + 0x2c)
                           );
      fVar8 = fVar7 + fVar7 + fVar8;
    }
    if (uVar5 != 0) {
      uVar3 = 0;
      fVar7 = DAT_180008010 / fVar8;
      do {
        fVar9 = 0.0;
        if ((float)(uVar3 & 0xffffffff) < fVar8) {
          fVar9 = fVar7;
        }
        *(float *)(param_1[2] + uVar3 * 4) = fVar9;
        uVar3 = uVar3 + 1;
      } while (uVar3 < *(uint *)((longlong)param_1 + 0x24));
      lVar4 = param_1[3];
    }
    fft_load_input_samples(*param_1,lVar4);
    fft_load_input_samples(param_1[1],param_1[2]);
    compute_forward_fft((undefined8 *)*param_1);
    compute_forward_fft((undefined8 *)param_1[1]);
    if ((int)param_1[4] != 0) {
      uVar3 = 0;
      do {
        lVar4 = get_fft_output_buffer(param_1[1]);
        fVar8 = *(float *)(lVar4 + uVar3 * 4);
        lVar4 = get_fft_output_buffer(*param_1);
        *(float *)(lVar4 + uVar3 * 4) = fVar8 * *(float *)(lVar4 + uVar3 * 4);
        uVar3 = uVar3 + 1;
      } while (uVar3 < *(uint *)(param_1 + 4));
    }
    compute_backward_fft(*param_1);
    if ((int)param_1[4] == 0) {
      uVar3 = 0;
      uVar5 = 0;
      _Src = (void *)param_1[3];
      cVar2 = (char)param_1[5];
    }
    else {
      uVar6 = 0;
      do {
        lVar4 = get_fft_input_buffer(*param_1);
        *(float *)(param_1[3] + uVar6 * 4) =
             *(float *)(lVar4 + uVar6 * 4) / (float)*(uint *)(param_1 + 4);
        uVar6 = uVar6 + 1;
        uVar5 = *(uint *)(param_1 + 4);
        uVar3 = (ulonglong)uVar5;
      } while (uVar6 < uVar3);
      _Src = (void *)param_1[3];
      cVar2 = (char)param_1[5];
    }
    if (cVar2 == '\0') {
      memcpy(param_3,_Src,uVar3 << 2);
    }
    else {
      min_spectrum((ulonglong)param_3,(ulonglong)_Src,uVar5);
    }
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_3 != (void *)0x0 && param_2 != 0) &
         0xffffffff;
}



/* ========================================================================
   ENTRY: 180002e10
   NAME : spectral_whitening_initialize
   SIG  : undefined8 * __fastcall spectral_whitening_initialize(uint param_1, uint param_2, uint param_3)
   ======================================================================== */

undefined8 * spectral_whitening_initialize(uint param_1,uint param_2,uint param_3)

{
  undefined8 *puVar1;
  void *pvVar2;
  float fVar3;
  
                    /* 0x2e10  111  spectral_whitening_initialize */
  puVar1 = calloc(1,0x28);
  *(uint *)(puVar1 + 3) = param_1;
  *(uint *)((longlong)puVar1 + 0x1c) = param_2;
  *(uint *)(puVar1 + 4) = param_3;
  pvVar2 = calloc((ulonglong)param_1,4);
  puVar1[1] = pvVar2;
  pvVar2 = calloc((ulonglong)param_1,4);
  fVar3 = (float)param_2 * DAT_18000803c;
  *puVar1 = pvVar2;
  fVar3 = expf(DAT_180008074 / (fVar3 / (float)param_3));
  *(float *)(puVar1 + 2) = fVar3;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180002eb0
   NAME : spectral_whitening_free
   SIG  : undefined __fastcall spectral_whitening_free(undefined8 * param_1)
   ======================================================================== */

void spectral_whitening_free(undefined8 *param_1)

{
                    /* 0x2eb0  110  spectral_whitening_free */
  free((void *)param_1[1]);
  free((void *)*param_1);
                    /* WARNING: Could not recover jumptable at 0x000180002ed7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180002ee0
   NAME : spectral_whitening_run
   SIG  : undefined __fastcall spectral_whitening_run(longlong * param_1, float param_2, longlong param_3)
   ======================================================================== */

void spectral_whitening_run(longlong *param_1,float param_2,longlong param_3)

{
  ulonglong uVar1;
  ulonglong uVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  
                    /* 0x2ee0  112  spectral_whitening_run */
  if ((0.0 <= param_2 && (param_3 != 0 && param_1 != (longlong *)0x0)) &&
     (*(int *)((longlong)param_1 + 0x14) = *(int *)((longlong)param_1 + 0x14) + 1,
     fVar3 = DAT_180008078, 1 < *(uint *)(param_1 + 3))) {
    uVar1 = 1;
    do {
      fVar4 = *(float *)(param_3 + uVar1 * 4);
      if (fVar4 <= fVar3) {
        fVar4 = fVar3;
      }
      if (1 < *(uint *)((longlong)param_1 + 0x14)) {
        fVar5 = *(float *)(*param_1 + uVar1 * 4) * *(float *)(param_1 + 2);
        fVar6 = fVar5;
        if (fVar5 <= fVar4) {
          fVar6 = fVar4;
        }
        fVar4 = (float)(~-(uint)NAN(fVar4) & (uint)fVar6 | -(uint)NAN(fVar4) & (uint)fVar5);
      }
      *(float *)(*param_1 + uVar1 * 4) = fVar4;
      fVar4 = DAT_180008034;
      uVar1 = uVar1 + 1;
      uVar2 = (ulonglong)*(uint *)(param_1 + 3);
    } while (uVar1 < uVar2);
    if (1 < *(uint *)(param_1 + 3)) {
      fVar3 = DAT_180008010 - param_2;
      uVar1 = 1;
      do {
        fVar6 = *(float *)(param_3 + uVar1 * 4);
        if (fVar4 < fVar6) {
          *(float *)(param_1[1] + uVar1 * 4) = fVar6 / *(float *)(*param_1 + uVar1 * 4);
          *(float *)(param_3 + uVar1 * 4) =
               *(float *)(param_3 + uVar1 * 4) * fVar3 +
               *(float *)(param_1[1] + uVar1 * 4) * param_2;
          uVar2 = (ulonglong)*(uint *)(param_1 + 3);
        }
        uVar1 = uVar1 + 1;
      } while (uVar1 < uVar2);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180002ff0
   NAME : absolute_hearing_thresholds_initialize
   SIG  : undefined8 * __fastcall absolute_hearing_thresholds_initialize(uint param_1, uint param_2, int param_3)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x00018000313f) */

undefined8 * absolute_hearing_thresholds_initialize(uint param_1,uint param_2,int param_3)

{
  double dVar1;
  float _Y;
  double dVar2;
  double dVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  float _Y_00;
  float fVar8;
  undefined8 *puVar9;
  undefined8 *puVar10;
  void *pvVar11;
  void *pvVar12;
  void *pvVar13;
  void *pvVar14;
  longlong *plVar15;
  longlong lVar16;
  float *pfVar17;
  ulonglong uVar18;
  uint uVar19;
  ulonglong uVar20;
  uint uVar21;
  bool bVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  
                    /* 0x2ff0  2  absolute_hearing_thresholds_initialize */
  puVar9 = calloc(1,0x50);
  *(uint *)((longlong)puVar9 + 0x34) = param_2;
  uVar21 = param_2 >> 1;
  uVar19 = uVar21 + 1;
  *(uint *)(puVar9 + 7) = uVar19;
  *(uint *)((longlong)puVar9 + 0x3c) = param_1;
  *(int *)(puVar9 + 6) = param_3;
  puVar9[8] = DAT_180008080;
  *(undefined4 *)(puVar9 + 9) = 0x42b40000;
  puVar10 = fft_transform_initialize_bins(param_2);
  puVar9[5] = puVar10;
  pvVar11 = calloc((ulonglong)uVar19,4);
  puVar9[2] = pvVar11;
  pvVar12 = calloc((ulonglong)uVar19,4);
  puVar9[3] = pvVar12;
  uVar18 = (ulonglong)param_2;
  pvVar13 = calloc(uVar18,4);
  *puVar9 = pvVar13;
  pvVar14 = calloc(uVar18,4);
  puVar9[1] = pvVar14;
  plVar15 = spectral_features_initialize(uVar19);
  puVar9[4] = plVar15;
  dVar3 = DAT_180008098;
  dVar2 = DAT_180008090;
  if (param_2 == 0) {
    get_fft_window((longlong)pvVar14,0,3);
    goto LAB_180003270;
  }
  dVar1 = (double)param_1;
  if (param_2 == 1) {
    uVar20 = 0;
LAB_1800031fd:
    fVar24 = sinf((float)(((double)(uVar20 & 0xffffffff) * DAT_180008090 * DAT_180008098) / dVar1));
    *(float *)((longlong)pvVar13 + uVar20 * 4) = fVar24;
  }
  else {
    uVar20 = 0;
    do {
      fVar24 = sinf((float)(((double)(uVar20 & 0xffffffff) * dVar2 * dVar3) / dVar1));
      *(float *)((longlong)pvVar13 + uVar20 * 4) = fVar24;
      fVar24 = sinf((float)(((double)((int)uVar20 + 1) * dVar2 * dVar3) / dVar1));
      *(float *)((longlong)pvVar13 + uVar20 * 4 + 4) = fVar24;
      uVar20 = uVar20 + 2;
    } while (uVar20 != (param_2 & 0xfffffffe));
    if ((param_2 & 1) != 0) goto LAB_1800031fd;
  }
  get_fft_window((longlong)pvVar14,param_2,3);
  uVar20 = 0;
  do {
    fVar24 = *(float *)((longlong)pvVar13 + uVar20 * 4);
    fVar25 = *(float *)((longlong)pvVar14 + uVar20 * 4);
    lVar16 = get_fft_input_buffer((longlong)puVar10);
    *(float *)(lVar16 + uVar20 * 4) = fVar24 * fVar25;
    uVar20 = uVar20 + 1;
  } while (uVar18 != uVar20);
LAB_180003270:
  compute_forward_fft(puVar10);
  pfVar17 = (float *)get_fft_output_buffer((longlong)puVar10);
  lVar16 = get_spectral_feature(plVar15,pfVar17,param_2,param_3);
  fVar25 = DAT_1800080a4;
  fVar24 = DAT_1800080a0;
  if (1 < param_2) {
    uVar18 = 0;
    do {
      fVar23 = log10f(*(float *)(lVar16 + 4 + uVar18 * 4));
      *(float *)((longlong)pvVar11 + uVar18 * 4 + 4) = fVar23 * fVar24 + fVar25;
      fVar8 = DAT_1800080bc;
      _Y_00 = DAT_1800080b8;
      fVar7 = DAT_1800080b4;
      fVar6 = DAT_1800080b0;
      fVar5 = DAT_1800080ac;
      fVar4 = DAT_1800080a8;
      _Y = DAT_180008054;
      fVar23 = DAT_18000803c;
      uVar18 = uVar18 + 1;
    } while (uVar21 != uVar18);
    uVar18 = 1;
    do {
      fVar24 = fft_bin_to_freq((uint)uVar18,param_1,param_2);
      fVar24 = fVar24 / fVar23;
      fVar25 = powf(fVar24,_Y);
      fVar26 = expf((fVar24 + fVar4) * (fVar24 + fVar4) * fVar5);
      fVar24 = powf(fVar24,_Y_00);
      *(float *)((longlong)pvVar12 + uVar18 * 4) = fVar24 * fVar8 + fVar25 * fVar7 + fVar26 * fVar6;
      bVar22 = uVar18 != uVar21;
      uVar18 = uVar18 + 1;
    } while (bVar22);
  }
  return puVar9;
}



/* ========================================================================
   ENTRY: 180003430
   NAME : absolute_hearing_thresholds_free
   SIG  : undefined __fastcall absolute_hearing_thresholds_free(undefined8 * param_1)
   ======================================================================== */

void absolute_hearing_thresholds_free(undefined8 *param_1)

{
                    /* 0x3430  1  absolute_hearing_thresholds_free */
  fft_transform_free((undefined8 *)param_1[5]);
  spectral_features_free((undefined8 *)param_1[4]);
  free((void *)*param_1);
  free((void *)param_1[1]);
  free((void *)param_1[2]);
  free((void *)param_1[3]);
                    /* WARNING: Could not recover jumptable at 0x000180003475. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180003480
   NAME : apply_thresholds_as_floor
   SIG  : undefined __fastcall apply_thresholds_as_floor(longlong param_1, longlong param_2)
   ======================================================================== */

void apply_thresholds_as_floor(longlong param_1,longlong param_2)

{
  float fVar1;
  ulonglong uVar2;
  float fVar3;
  float fVar4;
  
                    /* 0x3480  4  apply_thresholds_as_floor */
  if ((param_2 != 0 && param_1 != 0) && (1 < *(uint *)(param_1 + 0x38))) {
    uVar2 = 1;
    do {
      fVar3 = *(float *)(param_2 + uVar2 * 4) +
              *(float *)(*(longlong *)(param_1 + 0x10) + uVar2 * 4);
      fVar1 = *(float *)(*(longlong *)(param_1 + 0x18) + uVar2 * 4);
      fVar4 = fVar1;
      if (fVar1 <= fVar3) {
        fVar4 = fVar3;
      }
      *(uint *)(param_2 + uVar2 * 4) =
           ~-(uint)NAN(fVar3) & (uint)fVar4 | -(uint)NAN(fVar3) & (uint)fVar1;
      uVar2 = uVar2 + 1;
    } while (uVar2 < *(uint *)(param_1 + 0x38));
  }
  return;
}



/* ========================================================================
   ENTRY: 1800034f0
   NAME : critical_bands_initialize
   SIG  : longlong * __fastcall critical_bands_initialize(uint param_1, uint param_2, undefined4 param_3)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

longlong * critical_bands_initialize(uint param_1,uint param_2,undefined4 param_3)

{
  uint uVar1;
  int iVar2;
  longlong *plVar3;
  undefined4 *puVar4;
  undefined4 *puVar5;
  longlong lVar6;
  uint uVar7;
  longlong *plVar8;
  ulonglong uVar9;
  uint uVar10;
  uint uVar11;
  ulonglong _Count;
  bool bVar12;
  bool bVar13;
  float fVar14;
  float *local_48;
  
                    /* 0x34f0  10  critical_bands_initialize */
  plVar3 = calloc(1,0x38);
  *(uint *)(plVar3 + 3) = param_2;
  uVar10 = (param_2 >> 1) + 1;
  *(uint *)((longlong)plVar3 + 0x1c) = uVar10;
  *(uint *)(plVar3 + 4) = param_1;
  *(undefined4 *)(plVar3 + 5) = param_3;
  uVar11 = 0;
  switch(param_3) {
  case 0:
    fVar14 = (float)param_1 * DAT_180008038;
    uVar11 = 2;
    if (fVar14 <= _DAT_1800082dc) {
      uVar11 = (uint)(_DAT_1800082d8 < fVar14);
    }
    uVar1 = 3;
    if (fVar14 <= _DAT_180008228) {
      uVar1 = uVar11;
    }
    uVar11 = 4;
    if (fVar14 <= _DAT_1800082e0) {
      uVar11 = uVar1;
    }
    uVar1 = 5;
    if (fVar14 <= _DAT_1800082e4) {
      uVar1 = uVar11;
    }
    uVar11 = 6;
    if (fVar14 <= _DAT_1800082e8) {
      uVar11 = uVar1;
    }
    uVar1 = 7;
    if (fVar14 <= _DAT_1800082ec) {
      uVar1 = uVar11;
    }
    uVar11 = 8;
    if (fVar14 <= _DAT_1800082f0) {
      uVar11 = uVar1;
    }
    uVar1 = 9;
    if (fVar14 <= _DAT_1800082f4) {
      uVar1 = uVar11;
    }
    uVar11 = 10;
    if (fVar14 <= _DAT_1800082f8) {
      uVar11 = uVar1;
    }
    uVar1 = 0xb;
    if (fVar14 <= _DAT_1800082fc) {
      uVar1 = uVar11;
    }
    uVar11 = 0xc;
    if (fVar14 <= _DAT_180008240) {
      uVar11 = uVar1;
    }
    uVar1 = 0xd;
    if (fVar14 <= _DAT_180008300) {
      uVar1 = uVar11;
    }
    uVar11 = 0xe;
    if (fVar14 <= _DAT_180008304) {
      uVar11 = uVar1;
    }
    uVar1 = 0xf;
    if (fVar14 <= _DAT_180008308) {
      uVar1 = uVar11;
    }
    uVar11 = 0x10;
    if (fVar14 <= _DAT_18000830c) {
      uVar11 = uVar1;
    }
    uVar1 = 0x11;
    if (fVar14 <= _DAT_180008310) {
      uVar1 = uVar11;
    }
    uVar11 = 0x12;
    if (fVar14 <= _DAT_180008314) {
      uVar11 = uVar1;
    }
    uVar1 = 0x13;
    if (fVar14 <= _DAT_180008318) {
      uVar1 = uVar11;
    }
    uVar11 = 0x14;
    if (fVar14 <= _DAT_18000831c) {
      uVar11 = uVar1;
    }
    uVar1 = 0x15;
    if (fVar14 <= _DAT_180008320) {
      uVar1 = uVar11;
    }
    local_48 = (float *)&DAT_1800080c0;
    plVar3[2] = (longlong)&DAT_1800080c0;
    uVar7 = 0x16;
    if (fVar14 <= _DAT_180008268) {
      uVar7 = uVar1;
    }
    bVar13 = fVar14 == _DAT_180008324;
    bVar12 = fVar14 < _DAT_180008324;
    uVar11 = 0x17;
    goto LAB_180003802;
  case 1:
    fVar14 = (float)param_1 * DAT_180008038;
    uVar11 = 2;
    if (fVar14 <= _DAT_180008274) {
      uVar11 = (uint)(_DAT_180008270 < fVar14);
    }
    uVar1 = 3;
    if (fVar14 <= DAT_18000803c) {
      uVar1 = uVar11;
    }
    uVar11 = 4;
    if (fVar14 <= _DAT_180008278) {
      uVar11 = uVar1;
    }
    uVar1 = 5;
    if (fVar14 <= _DAT_18000827c) {
      uVar1 = uVar11;
    }
    uVar11 = 6;
    if (fVar14 <= _DAT_180008280) {
      uVar11 = uVar1;
    }
    uVar1 = 7;
    if (fVar14 <= _DAT_180008240) {
      uVar1 = uVar11;
    }
    uVar11 = 8;
    if (fVar14 <= _DAT_180008284) {
      uVar11 = uVar1;
    }
    uVar1 = 9;
    if (fVar14 <= _DAT_180008288) {
      uVar1 = uVar11;
    }
    uVar11 = 10;
    if (fVar14 <= _DAT_18000828c) {
      uVar11 = uVar1;
    }
    uVar1 = 0xb;
    if (fVar14 <= DAT_180008040) {
      uVar1 = uVar11;
    }
    uVar11 = 0xc;
    if (fVar14 <= _DAT_180008290) {
      uVar11 = uVar1;
    }
    uVar1 = 0xd;
    if (fVar14 <= _DAT_180008294) {
      uVar1 = uVar11;
    }
    uVar11 = 0xe;
    if (fVar14 <= _DAT_180008298) {
      uVar11 = uVar1;
    }
    uVar1 = 0xf;
    if (fVar14 <= _DAT_180008250) {
      uVar1 = uVar11;
    }
    uVar11 = 0x10;
    if (fVar14 <= _DAT_18000829c) {
      uVar11 = uVar1;
    }
    uVar1 = 0x11;
    if (fVar14 <= _DAT_1800082a0) {
      uVar1 = uVar11;
    }
    uVar11 = 0x12;
    if (fVar14 <= _DAT_1800082a4) {
      uVar11 = uVar1;
    }
    uVar1 = 0x13;
    if (fVar14 <= _DAT_1800082a8) {
      uVar1 = uVar11;
    }
    uVar11 = 0x14;
    if (fVar14 <= _DAT_1800082ac) {
      uVar11 = uVar1;
    }
    uVar1 = 0x15;
    if (fVar14 <= _DAT_1800082b0) {
      uVar1 = uVar11;
    }
    uVar11 = 0x16;
    if (fVar14 <= _DAT_1800082b4) {
      uVar11 = uVar1;
    }
    uVar1 = 0x17;
    if (fVar14 <= _DAT_1800082b8) {
      uVar1 = uVar11;
    }
    uVar11 = 0x18;
    if (fVar14 <= _DAT_1800082bc) {
      uVar11 = uVar1;
    }
    uVar1 = 0x19;
    if (fVar14 <= _DAT_1800082c0) {
      uVar1 = uVar11;
    }
    uVar11 = 0x1a;
    if (fVar14 <= _DAT_1800082c4) {
      uVar11 = uVar1;
    }
    uVar1 = 0x1b;
    if (fVar14 <= _DAT_1800082c8) {
      uVar1 = uVar11;
    }
    uVar11 = 0x1c;
    if (fVar14 <= _DAT_1800082cc) {
      uVar11 = uVar1;
    }
    uVar1 = 0x1d;
    if (fVar14 <= _DAT_1800082d0) {
      uVar1 = uVar11;
    }
    uVar11 = 0x1e;
    if (fVar14 <= _DAT_1800082d4) {
      uVar11 = uVar1;
    }
    local_48 = (float *)&DAT_180008120;
    plVar3[2] = (longlong)&DAT_180008120;
    uVar1 = 0x1f;
    if (fVar14 <= _DAT_180008260) {
      uVar1 = uVar11;
    }
    uVar11 = 0x20;
    if (fVar14 <= 0.0) {
      uVar11 = uVar1;
    }
    *(uint *)((longlong)plVar3 + 0x24) = uVar11;
    break;
  case 2:
    fVar14 = (float)param_1 * DAT_180008038;
    uVar11 = 2;
    if (fVar14 <= _DAT_18000822c) {
      uVar11 = (uint)(_DAT_180008228 < fVar14);
    }
    uVar1 = 3;
    if (fVar14 <= _DAT_180008230) {
      uVar1 = uVar11;
    }
    uVar11 = 4;
    if (fVar14 <= DAT_18000803c) {
      uVar11 = uVar1;
    }
    uVar1 = 5;
    if (fVar14 <= _DAT_180008234) {
      uVar1 = uVar11;
    }
    uVar11 = 6;
    if (fVar14 <= _DAT_180008238) {
      uVar11 = uVar1;
    }
    uVar1 = 7;
    if (fVar14 <= _DAT_18000823c) {
      uVar1 = uVar11;
    }
    uVar11 = 8;
    if (fVar14 <= _DAT_180008240) {
      uVar11 = uVar1;
    }
    uVar1 = 9;
    if (fVar14 <= _DAT_180008244) {
      uVar1 = uVar11;
    }
    uVar11 = 10;
    if (fVar14 <= _DAT_180008248) {
      uVar11 = uVar1;
    }
    uVar1 = 0xb;
    if (fVar14 <= _DAT_18000824c) {
      uVar1 = uVar11;
    }
    uVar11 = 0xc;
    if (fVar14 <= _DAT_180008250) {
      uVar11 = uVar1;
    }
    uVar1 = 0xd;
    if (fVar14 <= _DAT_180008254) {
      uVar1 = uVar11;
    }
    uVar11 = 0xe;
    if (fVar14 <= _DAT_180008258) {
      uVar11 = uVar1;
    }
    uVar1 = 0xf;
    if (fVar14 <= _DAT_18000825c) {
      uVar1 = uVar11;
    }
    uVar11 = 0x10;
    if (fVar14 <= _DAT_180008260) {
      uVar11 = uVar1;
    }
    uVar1 = 0x11;
    if (fVar14 <= _DAT_180008264) {
      uVar1 = uVar11;
    }
    local_48 = (float *)&DAT_1800081b0;
    plVar3[2] = (longlong)&DAT_1800081b0;
    uVar7 = 0x12;
    if (fVar14 <= _DAT_180008268) {
      uVar7 = uVar1;
    }
    bVar13 = fVar14 == _DAT_18000826c;
    bVar12 = fVar14 < _DAT_18000826c;
    uVar11 = 0x13;
LAB_180003802:
    if (bVar12 || bVar13) {
      uVar11 = uVar7;
    }
    *(uint *)((longlong)plVar3 + 0x24) = uVar11;
    break;
  case 3:
    local_48 = (float *)&DAT_180008200;
    plVar3[2] = (longlong)&DAT_180008200;
    break;
  default:
    local_48 = (float *)0x0;
  }
  _Count = (ulonglong)uVar11;
  puVar4 = calloc(_Count,4);
  *plVar3 = (longlong)puVar4;
  puVar5 = calloc(_Count,4);
  plVar3[1] = (longlong)puVar5;
  if (uVar11 != 0) {
    lVar6 = freq_to_fft_bin(*local_48,param_1,uVar10);
    *puVar5 = (int)lVar6;
    *puVar4 = (int)lVar6;
    if (uVar11 != 1) {
      uVar9 = 1;
      do {
        lVar6 = freq_to_fft_bin(local_48[uVar9],param_1,uVar10);
        iVar2 = (int)lVar6;
        if (uVar11 - 1 == uVar9) {
          puVar4[uVar9] = uVar10;
          iVar2 = uVar10 - puVar4[uVar9 - 1];
          plVar8 = plVar3 + 1;
        }
        else {
          puVar5[uVar9] = iVar2 - puVar4[uVar9 - 1];
          plVar8 = plVar3;
        }
        *(int *)(*plVar8 + uVar9 * 4) = iVar2;
        uVar9 = uVar9 + 1;
      } while (uVar9 < _Count);
    }
  }
  return plVar3;
}



/* ========================================================================
   ENTRY: 180003b10
   NAME : critical_bands_free
   SIG  : undefined __fastcall critical_bands_free(undefined8 * param_1)
   ======================================================================== */

void critical_bands_free(undefined8 *param_1)

{
                    /* 0x3b10  9  critical_bands_free
                       0x3b10  121  stft_window_free */
  free((void *)*param_1);
  free((void *)param_1[1]);
                    /* WARNING: Could not recover jumptable at 0x000180003b37. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180003b40
   NAME : compute_critical_bands_spectrum
   SIG  : bool __fastcall compute_critical_bands_spectrum(longlong * param_1, longlong param_2, longlong param_3)
   ======================================================================== */

bool compute_critical_bands_spectrum(longlong *param_1,longlong param_2,longlong param_3)

{
  uint uVar1;
  ulonglong uVar2;
  uint uVar3;
  ulonglong uVar4;
  uint uVar5;
  float fVar6;
  
                    /* 0x3b40  6  compute_critical_bands_spectrum */
  if (param_2 != 0) {
    uVar3 = *(uint *)((longlong)param_1 + 0x24);
    if (uVar3 != 0) {
      uVar2 = 0;
      do {
        uVar1 = *(uint *)(*param_1 + uVar2 * 4);
        uVar5 = uVar1 - *(int *)(param_1[1] + uVar2 * 4);
        *(ulonglong *)((longlong)param_1 + 0x2c) = CONCAT44(uVar1,uVar5);
        if (uVar5 < uVar1) {
          uVar4 = (ulonglong)uVar5;
          fVar6 = *(float *)(param_3 + uVar2 * 4);
          do {
            fVar6 = fVar6 + *(float *)(param_2 + uVar4 * 4);
            *(float *)(param_3 + uVar2 * 4) = fVar6;
            uVar4 = uVar4 + 1;
          } while (uVar4 < *(uint *)(param_1 + 6));
          uVar3 = *(uint *)((longlong)param_1 + 0x24);
        }
        uVar2 = uVar2 + 1;
      } while (uVar2 < uVar3);
    }
  }
  return param_2 != 0;
}



/* ========================================================================
   ENTRY: 180003bd0
   NAME : get_band_indexes
   SIG  : undefined8 __fastcall get_band_indexes(longlong * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 get_band_indexes(longlong *param_1,ulonglong param_2)

{
  int iVar1;
  
                    /* 0x3bd0  24  get_band_indexes */
  iVar1 = *(int *)(*param_1 + (param_2 & 0xffffffff) * 4);
  return CONCAT44(iVar1,iVar1 - *(int *)(param_1[1] + (param_2 & 0xffffffff) * 4));
}



/* ========================================================================
   ENTRY: 180003bf0
   NAME : get_number_of_critical_bands
   SIG  : undefined4 __fastcall get_number_of_critical_bands(longlong param_1)
   ======================================================================== */

undefined4 get_number_of_critical_bands(longlong param_1)

{
                    /* 0x3bf0  37  get_number_of_critical_bands */
  return *(undefined4 *)(param_1 + 0x24);
}



/* ========================================================================
   ENTRY: 180003c00
   NAME : masking_estimation_initialize
   SIG  : uint * __fastcall masking_estimation_initialize(uint param_1, uint param_2, int param_3)
   ======================================================================== */

uint * masking_estimation_initialize(uint param_1,uint param_2,int param_3)

{
  ulonglong uVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  float _X;
  uint uVar7;
  uint *puVar8;
  longlong *plVar9;
  void *pvVar10;
  void *pvVar11;
  void *pvVar12;
  void *pvVar13;
  undefined8 *puVar14;
  uint uVar15;
  ulonglong _Count;
  uint uVar16;
  ulonglong uVar17;
  longlong lVar18;
  float fVar19;
  
                    /* 0x3c00  59  masking_estimation_initialize */
  puVar8 = calloc(1,0x60);
  *puVar8 = param_1;
  puVar8[1] = (param_1 >> 1) + 1;
  puVar8[2] = param_2;
  plVar9 = critical_bands_initialize(param_2,param_1,2);
  *(longlong **)(puVar8 + 6) = plVar9;
  uVar7 = get_number_of_critical_bands((longlong)plVar9);
  _Count = (ulonglong)uVar7;
  puVar8[3] = uVar7;
  pvVar10 = calloc(_Count * _Count,4);
  *(void **)(puVar8 + 10) = pvVar10;
  pvVar11 = calloc(_Count,4);
  *(void **)(puVar8 + 0xc) = pvVar11;
  pvVar12 = calloc(_Count,4);
  *(void **)(puVar8 + 0xe) = pvVar12;
  pvVar13 = calloc(_Count,4);
  *(void **)(puVar8 + 0x10) = pvVar13;
  pvVar13 = calloc(_Count,4);
  *(void **)(puVar8 + 0x12) = pvVar13;
  pvVar13 = calloc(_Count,4);
  *(void **)(puVar8 + 0x14) = pvVar13;
  pvVar13 = calloc(_Count,4);
  *(void **)(puVar8 + 0x16) = pvVar13;
  puVar14 = absolute_hearing_thresholds_initialize(param_2,param_1,param_3);
  *(undefined8 **)(puVar8 + 4) = puVar14;
  _X = DAT_180008348;
  fVar6 = DAT_180008344;
  fVar5 = DAT_180008340;
  fVar4 = DAT_18000833c;
  fVar3 = DAT_180008338;
  fVar2 = DAT_180008010;
  if (uVar7 != 0) {
    lVar18 = 0;
    uVar15 = 0;
    do {
      uVar17 = 0;
      uVar16 = uVar15;
      do {
        uVar1 = uVar17 + 1;
        fVar19 = (float)uVar16 + fVar3;
        fVar19 = powf(_X,(SQRT(fVar19 * fVar19 + fVar2) * fVar6 + fVar19 * fVar4 + fVar5) / _X);
        *(float *)((longlong)pvVar10 + (ulonglong)(uint)((int)uVar17 + (int)lVar18) * 4) = fVar19;
        uVar16 = uVar16 - 1;
        uVar17 = uVar1;
      } while (_Count != uVar1);
      uVar15 = uVar15 + 1;
      lVar18 = lVar18 + _Count;
    } while (uVar15 != uVar7);
  }
  initialize_spectrum_with_value((longlong)pvVar11,uVar7,DAT_180008010);
  direct_matrix_to_vector_spectral_convolution
            ((longlong)pvVar10,(longlong)pvVar11,(longlong)pvVar12,uVar7);
  return puVar8;
}



/* ========================================================================
   ENTRY: 180003e50
   NAME : masking_estimation_free
   SIG  : undefined __fastcall masking_estimation_free(void * param_1)
   ======================================================================== */

void masking_estimation_free(void *param_1)

{
                    /* 0x3e50  58  masking_estimation_free */
  absolute_hearing_thresholds_free(*(undefined8 **)((longlong)param_1 + 0x10));
  critical_bands_free(*(undefined8 **)((longlong)param_1 + 0x18));
  free(*(void **)((longlong)param_1 + 0x28));
  free(*(void **)((longlong)param_1 + 0x30));
  free(*(void **)((longlong)param_1 + 0x38));
  free(*(void **)((longlong)param_1 + 0x40));
  free(*(void **)((longlong)param_1 + 0x48));
  free(*(void **)((longlong)param_1 + 0x50));
  free(*(void **)((longlong)param_1 + 0x58));
                    /* WARNING: Could not recover jumptable at 0x000180003ea8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180003eb0
   NAME : compute_masking_thresholds
   SIG  : ulonglong __fastcall compute_masking_thresholds(longlong param_1, longlong param_2, longlong param_3)
   ======================================================================== */

ulonglong compute_masking_thresholds(longlong param_1,longlong param_2,longlong param_3)

{
  float fVar1;
  float fVar2;
  float _X;
  float fVar3;
  float fVar4;
  float fVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  undefined8 unaff_RBX;
  ulonglong uVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  
                    /* 0x3eb0  8  compute_masking_thresholds */
  if (param_3 != 0 && (param_2 != 0 && param_1 != 0)) {
    compute_critical_bands_spectrum
              (*(longlong **)(param_1 + 0x18),param_2,*(longlong *)(param_1 + 0x58));
    direct_matrix_to_vector_spectral_convolution
              (*(longlong *)(param_1 + 0x28),*(longlong *)(param_1 + 0x58),
               *(longlong *)(param_1 + 0x50),*(uint *)(param_1 + 0xc));
    fVar5 = DAT_180008354;
    fVar4 = DAT_180008350;
    fVar3 = DAT_18000834c;
    _X = DAT_180008348;
    fVar2 = DAT_1800080a0;
    fVar1 = DAT_180008010;
    if (*(int *)(param_1 + 0xc) != 0) {
      uVar8 = 0;
      do {
        uVar6 = get_band_indexes(*(longlong **)(param_1 + 0x18),uVar8 & 0xffffffff);
        *(ulonglong *)(param_1 + 0x20) = uVar6;
        uVar7 = uVar6 >> 0x20;
        fVar11 = 0.0;
        fVar10 = 0.0;
        if ((uint)uVar6 < (uint)(uVar6 >> 0x20)) {
          uVar6 = uVar6 & 0xffffffff;
          do {
            fVar9 = *(float *)(param_2 + uVar6 * 4);
            fVar10 = fVar10 + fVar9;
            fVar9 = log10f(fVar9);
            fVar11 = fVar11 + fVar9;
            uVar6 = uVar6 + 1;
            uVar7 = (ulonglong)*(uint *)(param_1 + 0x24);
          } while (uVar6 < uVar7);
          uVar6 = (ulonglong)*(uint *)(param_1 + 0x20);
        }
        fVar9 = (float)uVar7 - (float)(uVar6 & 0xffffffff);
        fVar10 = log10f(fVar10 / fVar9);
        uVar6 = uVar8 + 1;
        fVar10 = ((fVar11 / fVar9) * _X - fVar10) / fVar3;
        if (fVar1 <= fVar10) {
          fVar10 = fVar1;
        }
        *(float *)(*(longlong *)(param_1 + 0x48) + uVar8 * 4) =
             ((float)(uVar6 & 0xffffffff) + fVar4) * fVar10 + (fVar1 - fVar10) * fVar5;
        fVar10 = log10f(*(float *)(*(longlong *)(param_1 + 0x50) + uVar8 * 4));
        fVar10 = powf(_X,*(float *)(*(longlong *)(param_1 + 0x48) + uVar8 * 4) / fVar2 + fVar10);
        fVar11 = log10f(*(float *)(*(longlong *)(param_1 + 0x38) + uVar8 * 4));
        *(float *)(*(longlong *)(param_1 + 0x40) + uVar8 * 4) = fVar11 * fVar2 + fVar10;
        uVar7 = get_band_indexes(*(longlong **)(param_1 + 0x18),uVar8 & 0xffffffff);
        *(ulonglong *)(param_1 + 0x20) = uVar7;
        if ((uint)uVar7 < (uint)(uVar7 >> 0x20)) {
          uVar7 = uVar7 & 0xffffffff;
          do {
            *(undefined4 *)(param_3 + uVar7 * 4) =
                 *(undefined4 *)(*(longlong *)(param_1 + 0x40) + uVar8 * 4);
            uVar7 = uVar7 + 1;
          } while (uVar7 < *(uint *)(param_1 + 0x24));
        }
        uVar8 = uVar6;
      } while (uVar6 < *(uint *)(param_1 + 0xc));
    }
    apply_thresholds_as_floor(*(longlong *)(param_1 + 0x10),param_3);
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_3 != 0 && (param_2 != 0 && param_1 != 0))
         & 0xffffffff;
}



/* ========================================================================
   ENTRY: 180004160
   NAME : noise_scaling_criterias_initialize
   SIG  : void * __fastcall noise_scaling_criterias_initialize(uint param_1, undefined4 param_2, uint param_3, int param_4)
   ======================================================================== */

void * noise_scaling_criterias_initialize(uint param_1,undefined4 param_2,uint param_3,int param_4)

{
  uint uVar1;
  void *pvVar2;
  longlong *plVar3;
  uint *puVar4;
  void *pvVar5;
  uint uVar6;
  
                    /* 0x4160  70  noise_scaling_criterias_initialize */
  pvVar2 = calloc(1,0x68);
  *(uint *)((longlong)pvVar2 + 4) = param_1;
  uVar6 = (param_1 >> 1) + 1;
  *(uint *)((longlong)pvVar2 + 8) = uVar6;
  *(undefined4 *)((longlong)pvVar2 + 0x30) = param_2;
  *(uint *)((longlong)pvVar2 + 0xc) = param_3;
  *(int *)((longlong)pvVar2 + 0x10) = param_4;
  *(undefined8 *)((longlong)pvVar2 + 0x1c) = DAT_180008360;
  plVar3 = critical_bands_initialize(param_3,param_1,param_2);
  *(longlong **)((longlong)pvVar2 + 0x60) = plVar3;
  puVar4 = masking_estimation_initialize(param_1,param_3,param_4);
  *(uint **)((longlong)pvVar2 + 0x58) = puVar4;
  uVar1 = get_number_of_critical_bands((longlong)plVar3);
  *(uint *)((longlong)pvVar2 + 0x14) = uVar1;
  pvVar5 = calloc((ulonglong)uVar1,4);
  *(void **)((longlong)pvVar2 + 0x48) = pvVar5;
  pvVar5 = calloc((ulonglong)uVar1,4);
  *(void **)((longlong)pvVar2 + 0x50) = pvVar5;
  pvVar5 = calloc((ulonglong)uVar6,4);
  *(void **)((longlong)pvVar2 + 0x38) = pvVar5;
  pvVar5 = calloc((ulonglong)uVar6,4);
  *(void **)((longlong)pvVar2 + 0x40) = pvVar5;
  return pvVar2;
}



/* ========================================================================
   ENTRY: 180004240
   NAME : noise_scaling_criterias_free
   SIG  : undefined __fastcall noise_scaling_criterias_free(void * param_1)
   ======================================================================== */

void noise_scaling_criterias_free(void *param_1)

{
                    /* 0x4240  69  noise_scaling_criterias_free */
  critical_bands_free(*(undefined8 **)((longlong)param_1 + 0x60));
  masking_estimation_free(*(void **)((longlong)param_1 + 0x58));
  free(*(void **)((longlong)param_1 + 0x40));
  free(*(void **)((longlong)param_1 + 0x38));
  free(*(void **)((longlong)param_1 + 0x48));
  free(*(void **)((longlong)param_1 + 0x50));
                    /* WARNING: Could not recover jumptable at 0x000180004286. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180004290
   NAME : apply_noise_scaling_criteria
   SIG  : ulonglong __fastcall apply_noise_scaling_criteria(longlong param_1, longlong param_2, longlong param_3, longlong param_4, longlong param_5, float * param_6)
   ======================================================================== */

ulonglong apply_noise_scaling_criteria
                    (longlong param_1,longlong param_2,longlong param_3,longlong param_4,
                    longlong param_5,float *param_6)

{
  uint uVar1;
  ulonglong uVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  undefined8 unaff_RBX;
  longlong lVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  
                    /* 0x4290  3  apply_noise_scaling_criteria */
  if (param_3 != 0 && param_2 != 0) {
    fVar8 = param_6[2];
    if (fVar8 == 2.8026e-45) {
      fVar8 = *param_6;
      fVar13 = param_6[1];
      if (1 < *(uint *)(param_1 + 8)) {
        uVar7 = 1;
        do {
          fVar14 = *(float *)(param_2 + uVar7 * 4) - *(float *)(param_3 + uVar7 * 4);
          if (fVar14 <= 0.0) {
            fVar14 = 0.0;
          }
          *(float *)(*(longlong *)(param_1 + 0x40) + uVar7 * 4) = fVar14;
          uVar7 = uVar7 + 1;
        } while (uVar7 < *(uint *)(param_1 + 8));
      }
      compute_masking_thresholds
                (*(longlong *)(param_1 + 0x58),*(longlong *)(param_1 + 0x40),
                 *(longlong *)(param_1 + 0x38));
      fVar11 = (float)max_spectral_value(*(longlong *)(param_1 + 0x38),*(uint *)(param_1 + 8));
      fVar12 = (float)min_spectral_value(*(longlong *)(param_1 + 0x38),*(uint *)(param_1 + 8));
      fVar14 = DAT_180008010;
      if (1 < *(uint *)(param_1 + 8)) {
        uVar7 = 1;
        do {
          fVar15 = *(float *)(*(longlong *)(param_1 + 0x38) + uVar7 * 4);
          if ((fVar15 != fVar11) || (NAN(fVar15) || NAN(fVar11))) {
            if ((fVar15 != fVar12) || (NAN(fVar15) || NAN(fVar12))) {
              fVar15 = (fVar15 - fVar12) / (fVar11 - fVar12);
              *(float *)(param_4 + uVar7 * 4) =
                   (fVar14 - fVar15) * fVar13 + *(float *)(param_1 + 0x20) * fVar15;
              fVar15 = (fVar14 - fVar15) * fVar8 + fVar15 * *(float *)(param_1 + 0x24);
            }
            else {
              *(float *)(param_4 + uVar7 * 4) = fVar13;
              fVar15 = fVar8;
            }
          }
          else {
            *(undefined4 *)(param_4 + uVar7 * 4) = *(undefined4 *)(param_1 + 0x20);
            fVar15 = *(float *)(param_1 + 0x24);
          }
          *(float *)(param_5 + uVar7 * 4) = fVar15;
          uVar7 = uVar7 + 1;
        } while (uVar7 < *(uint *)(param_1 + 8));
      }
    }
    else if (fVar8 == 1.4013e-45) {
      fVar8 = param_6[1];
      compute_critical_bands_spectrum
                (*(longlong **)(param_1 + 0x60),param_3,*(longlong *)(param_1 + 0x48));
      compute_critical_bands_spectrum
                (*(longlong **)(param_1 + 0x60),param_2,*(longlong *)(param_1 + 0x50));
      fVar12 = DAT_180008374;
      fVar11 = DAT_180008370;
      fVar14 = DAT_180008348;
      fVar13 = DAT_180008010;
      if (*(int *)(param_1 + 0x14) != 0) {
        uVar7 = 0;
        fVar15 = DAT_180008010;
        do {
          uVar3 = get_band_indexes(*(longlong **)(param_1 + 0x60),uVar7 & 0xffffffff);
          *(undefined8 *)(param_1 + 0x28) = uVar3;
          fVar9 = log10f(*(float *)(*(longlong *)(param_1 + 0x50) + uVar7 * 4) /
                         *(float *)(*(longlong *)(param_1 + 0x48) + uVar7 * 4));
          fVar9 = fVar9 * fVar14;
          if ((fVar9 < *(float *)(param_1 + 0x18)) || (*(float *)(param_1 + 0x1c) < fVar9)) {
            fVar10 = fVar8;
            if ((0.0 <= fVar9) && (fVar10 = fVar15, fVar12 < fVar9)) {
              fVar10 = fVar13;
            }
            uVar4 = (ulonglong)*(uint *)(param_1 + 0x28);
            if (*(uint *)(param_1 + 0x28) < *(uint *)(param_1 + 0x2c)) goto LAB_180004430;
          }
          else {
            fVar10 = fVar9 * fVar11 + fVar8;
            uVar4 = (ulonglong)*(uint *)(param_1 + 0x28);
            if (*(uint *)(param_1 + 0x28) < *(uint *)(param_1 + 0x2c)) {
LAB_180004430:
              do {
                *(float *)(param_4 + uVar4 * 4) = fVar10;
                uVar4 = uVar4 + 1;
              } while (uVar4 < *(uint *)(param_1 + 0x2c));
            }
          }
          uVar7 = uVar7 + 1;
          fVar15 = fVar10;
        } while (uVar7 < *(uint *)(param_1 + 0x14));
      }
    }
    else if (fVar8 == 0.0) {
      uVar7 = (ulonglong)*(uint *)(param_1 + 8);
      fVar8 = DAT_180008070;
      if (1 < uVar7) {
        uVar4 = (ulonglong)((uint)(uVar7 - 1) & 3);
        if (uVar7 - 2 < 3) {
          fVar13 = 0.0;
          lVar5 = 1;
          fVar8 = 0.0;
        }
        else {
          fVar13 = 0.0;
          fVar8 = 0.0;
          uVar2 = 0;
          do {
            uVar6 = uVar2;
            fVar8 = fVar8 + *(float *)(param_2 + 4 + uVar6 * 4) +
                    *(float *)(param_2 + 8 + uVar6 * 4) + *(float *)(param_2 + 0xc + uVar6 * 4) +
                    *(float *)(param_2 + 0x10 + uVar6 * 4);
            fVar13 = fVar13 + *(float *)(param_3 + 4 + uVar6 * 4) +
                     *(float *)(param_3 + 8 + uVar6 * 4) + *(float *)(param_3 + 0xc + uVar6 * 4) +
                     *(float *)(param_3 + 0x10 + uVar6 * 4);
            uVar2 = uVar6 + 4;
          } while ((uVar7 - 1 & 0xfffffffffffffffc) != uVar6 + 4);
          lVar5 = uVar6 + 5;
        }
        if (uVar4 != 0) {
          uVar7 = 0;
          do {
            fVar8 = fVar8 + *(float *)(param_2 + lVar5 * 4 + uVar7 * 4);
            fVar13 = fVar13 + *(float *)(param_3 + lVar5 * 4 + uVar7 * 4);
            uVar7 = uVar7 + 1;
          } while (uVar4 != uVar7);
        }
        fVar8 = fVar8 / fVar13;
      }
      fVar13 = param_6[1];
      fVar8 = log10f(fVar8);
      fVar8 = fVar8 * DAT_180008348;
      if ((fVar8 < *(float *)(param_1 + 0x18)) || (*(float *)(param_1 + 0x1c) < fVar8)) {
        if (0.0 <= fVar8) {
          fVar13 = DAT_180008010;
        }
        uVar1 = *(uint *)(param_1 + 8);
      }
      else {
        fVar13 = fVar13 + fVar8 * DAT_180008370;
        uVar1 = *(uint *)(param_1 + 8);
      }
      if (1 < uVar1) {
        uVar7 = 1;
        do {
          *(float *)(param_4 + uVar7 * 4) = fVar13;
          uVar7 = uVar7 + 1;
        } while (uVar7 < *(uint *)(param_1 + 8));
      }
    }
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_3 != 0 && param_2 != 0) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 1800046d0
   NAME : spectral_smoothing_initialize
   SIG  : uint * __fastcall spectral_smoothing_initialize(uint param_1, uint param_2)
   ======================================================================== */

uint * spectral_smoothing_initialize(uint param_1,uint param_2)

{
  uint *puVar1;
  void *pvVar2;
  uint *puVar3;
  uint uVar4;
  ulonglong _Count;
  
                    /* 0x46d0  105  spectral_smoothing_initialize */
  puVar1 = calloc(1,0x38);
  *puVar1 = param_1;
  uVar4 = (param_1 >> 1) + 1;
  _Count = (ulonglong)uVar4;
  puVar1[1] = uVar4;
  puVar1[4] = param_2;
  pvVar2 = calloc(_Count,4);
  *(void **)(puVar1 + 6) = pvVar2;
  pvVar2 = calloc(_Count,4);
  *(void **)(puVar1 + 8) = pvVar2;
  pvVar2 = calloc(_Count,4);
  *(void **)(puVar1 + 10) = pvVar2;
  puVar3 = transient_detector_initialize(param_1);
  *(uint **)(puVar1 + 0xc) = puVar3;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180004750
   NAME : spectral_smoothing_free
   SIG  : undefined __fastcall spectral_smoothing_free(void * param_1)
   ======================================================================== */

void spectral_smoothing_free(void *param_1)

{
                    /* 0x4750  104  spectral_smoothing_free */
  transient_detector_free(*(void **)((longlong)param_1 + 0x30));
  free(*(void **)((longlong)param_1 + 0x18));
  free(*(void **)((longlong)param_1 + 0x20));
  free(*(void **)((longlong)param_1 + 0x28));
                    /* WARNING: Could not recover jumptable at 0x000180004787. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180004790
   NAME : spectral_smoothing_run
   SIG  : ulonglong __fastcall spectral_smoothing_run(longlong param_1, ulonglong param_2, void * param_3)
   ======================================================================== */

ulonglong spectral_smoothing_run(longlong param_1,ulonglong param_2,void *param_3)

{
  float fVar1;
  float fVar2;
  bool bVar3;
  ulonglong uVar4;
  float fVar5;
  undefined8 unaff_RBX;
  uint uVar6;
  float fVar7;
  
                    /* 0x4790  106  spectral_smoothing_run */
  if (param_3 != (void *)0x0 && param_1 != 0) {
    fVar5 = (float)param_2;
    memcpy(*(void **)(param_1 + 0x20),param_3,(ulonglong)*(uint *)(param_1 + 4) << 2);
    if (*(int *)(param_1 + 0x10) == 1) {
      uVar6 = *(uint *)(param_1 + 4);
      if (1 < uVar6) {
        fVar7 = DAT_180008010 - fVar5;
        uVar4 = 1;
        do {
          fVar1 = *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4);
          fVar2 = *(float *)(*(longlong *)(param_1 + 0x28) + uVar4 * 4);
          if (fVar2 < fVar1) {
            *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4) = fVar2 * fVar5 + fVar1 * fVar7;
            uVar6 = *(uint *)(param_1 + 4);
          }
          uVar4 = uVar4 + 1;
        } while (uVar4 < uVar6);
      }
    }
    else if (*(int *)(param_1 + 0x10) == 2) {
      if ((param_2 >> 0x20 & 1) == 0) {
        uVar6 = *(uint *)(param_1 + 4);
        if (1 < uVar6) {
          fVar7 = DAT_180008010 - fVar5;
          uVar4 = 1;
          do {
            fVar1 = *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4);
            fVar2 = *(float *)(*(longlong *)(param_1 + 0x28) + uVar4 * 4);
            if (fVar2 < fVar1) {
              *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4) = fVar2 * fVar5 + fVar1 * fVar7;
              uVar6 = *(uint *)(param_1 + 4);
            }
            uVar4 = uVar4 + 1;
          } while (uVar4 < uVar6);
        }
      }
      else {
        bVar3 = transient_detector_run(*(longlong *)(param_1 + 0x30),param_3);
        uVar6 = *(uint *)(param_1 + 4);
        if (1 < uVar6 && !bVar3) {
          fVar7 = DAT_180008010 - fVar5;
          uVar4 = 1;
          do {
            fVar1 = *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4);
            fVar2 = *(float *)(*(longlong *)(param_1 + 0x28) + uVar4 * 4);
            if (fVar2 < fVar1) {
              *(float *)(*(longlong *)(param_1 + 0x20) + uVar4 * 4) = fVar2 * fVar5 + fVar1 * fVar7;
              uVar6 = *(uint *)(param_1 + 4);
            }
            uVar4 = uVar4 + 1;
          } while (uVar4 < uVar6);
        }
      }
    }
    else {
      uVar6 = *(uint *)(param_1 + 4);
    }
    memcpy(*(void **)(param_1 + 0x28),*(void **)(param_1 + 0x20),(ulonglong)uVar6 << 2);
    memcpy(param_3,*(void **)(param_1 + 0x20),(ulonglong)*(uint *)(param_1 + 4) << 2);
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_3 != (void *)0x0 && param_1 != 0) &
         0xffffffff;
}



/* ========================================================================
   ENTRY: 180004970
   NAME : transient_detector_initialize
   SIG  : uint * __fastcall transient_detector_initialize(uint param_1)
   ======================================================================== */

uint * transient_detector_initialize(uint param_1)

{
  uint *puVar1;
  void *pvVar2;
  uint uVar3;
  
                    /* 0x4970  124  transient_detector_initialize */
  puVar1 = calloc(1,0x20);
  *puVar1 = param_1;
  uVar3 = (param_1 >> 1) + 1;
  puVar1[1] = uVar3;
  pvVar2 = calloc((ulonglong)uVar3,4);
  *(void **)(puVar1 + 6) = pvVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 1800049c0
   NAME : transient_detector_free
   SIG  : undefined __fastcall transient_detector_free(void * param_1)
   ======================================================================== */

void transient_detector_free(void *param_1)

{
                    /* 0x49c0  123  transient_detector_free */
  free(*(void **)((longlong)param_1 + 0x18));
                    /* WARNING: Could not recover jumptable at 0x0001800049e2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800049f0
   NAME : transient_detector_run
   SIG  : bool __fastcall transient_detector_run(longlong param_1, void * param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

bool transient_detector_run(longlong param_1,void *param_2)

{
  uint uVar1;
  float fVar2;
  float fVar3;
  
                    /* 0x49f0  125  transient_detector_run */
  fVar2 = spectral_flux((longlong)param_2,*(longlong *)(param_1 + 0x18),*(uint *)(param_1 + 4));
  uVar1 = *(int *)(param_1 + 0x10) + 1;
  *(uint *)(param_1 + 0x10) = uVar1;
  fVar3 = fVar2;
  if (1 < uVar1) {
    fVar3 = (fVar2 - *(float *)(param_1 + 8)) / (float)uVar1 + *(float *)(param_1 + 8);
  }
  *(float *)(param_1 + 8) = fVar3;
  fVar3 = fVar3 * _DAT_180008378;
  memcpy(*(void **)(param_1 + 0x18),param_2,(ulonglong)*(uint *)(param_1 + 4) << 2);
  return fVar3 < fVar2;
}



/* ========================================================================
   ENTRY: 180004a80
   NAME : fft_transform_initialize
   SIG  : undefined8 * __fastcall fft_transform_initialize(uint param_1, int param_2, int param_3)
   ======================================================================== */

undefined8 * fft_transform_initialize(uint param_1,int param_2,int param_3)

{
  uint uVar1;
  undefined8 *puVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  
                    /* 0x4a80  20  fft_transform_initialize */
  puVar2 = calloc(1,0x38);
  *(int *)(puVar2 + 4) = param_2;
  *(int *)(puVar2 + 3) = param_3;
  *(uint *)((longlong)puVar2 + 0x14) = param_1;
  if (param_2 == 0) {
    uVar1 = get_next_power_two(param_1);
    *(uint *)((longlong)puVar2 + 0x24) = uVar1 - param_1;
  }
  else {
    uVar1 = param_1;
    if (param_2 == 1) {
      *(int *)((longlong)puVar2 + 0x24) = param_3;
      uVar1 = param_3 + param_1;
    }
    uVar1 = get_next_divisible_two(uVar1);
  }
  *(uint *)(puVar2 + 2) = uVar1;
  *(uint *)((longlong)puVar2 + 0x1c) = (uVar1 >> 1) - (param_1 >> 1);
  uVar3 = fftwf_malloc((ulonglong)uVar1 << 2);
  puVar2[5] = uVar3;
  uVar4 = fftwf_malloc((ulonglong)uVar1 << 2);
  puVar2[6] = uVar4;
  uVar5 = fftwf_plan_r2r_1d(uVar1,uVar3,uVar4,0xffffffff,0x40);
  *puVar2 = uVar5;
  uVar3 = fftwf_plan_r2r_1d(uVar1,uVar4,uVar3,1,0x40);
  puVar2[1] = uVar3;
  return puVar2;
}



/* ========================================================================
   ENTRY: 180004b60
   NAME : fft_transform_initialize_bins
   SIG  : undefined8 * __fastcall fft_transform_initialize_bins(uint param_1)
   ======================================================================== */

undefined8 * fft_transform_initialize_bins(uint param_1)

{
  undefined8 *puVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  
                    /* 0x4b60  21  fft_transform_initialize_bins */
  puVar1 = calloc(1,0x38);
  *(uint *)(puVar1 + 2) = param_1;
  *(uint *)((longlong)puVar1 + 0x14) = param_1;
  uVar2 = fftwf_malloc((ulonglong)param_1 << 2);
  puVar1[5] = uVar2;
  uVar3 = fftwf_malloc((ulonglong)param_1 << 2);
  puVar1[6] = uVar3;
  uVar4 = fftwf_plan_r2r_1d(param_1,uVar2,uVar3,0xffffffff,0x40);
  *puVar1 = uVar4;
  uVar2 = fftwf_plan_r2r_1d(param_1,uVar3,uVar2,1,0x40);
  puVar1[1] = uVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180004c00
   NAME : fft_transform_free
   SIG  : undefined __fastcall fft_transform_free(undefined8 * param_1)
   ======================================================================== */

void fft_transform_free(undefined8 *param_1)

{
                    /* 0x4c00  19  fft_transform_free */
  fftwf_free(param_1[5]);
  fftwf_free(param_1[6]);
  fftwf_destroy_plan(*param_1);
  fftwf_destroy_plan(param_1[1]);
                    /* WARNING: Could not recover jumptable at 0x000180004c33. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180004c40
   NAME : get_fft_size
   SIG  : undefined4 __fastcall get_fft_size(longlong param_1)
   ======================================================================== */

undefined4 get_fft_size(longlong param_1)

{
                    /* 0x4c40  28  get_fft_size */
  return *(undefined4 *)(param_1 + 0x10);
}



/* ========================================================================
   ENTRY: 180004c50
   NAME : get_fft_real_spectrum_size
   SIG  : int __fastcall get_fft_real_spectrum_size(longlong param_1)
   ======================================================================== */

int get_fft_real_spectrum_size(longlong param_1)

{
                    /* 0x4c50  27  get_fft_real_spectrum_size */
  return (*(uint *)(param_1 + 0x10) >> 1) + 1;
}



/* ========================================================================
   ENTRY: 180004c60
   NAME : fft_load_input_samples
   SIG  : undefined __fastcall fft_load_input_samples(longlong param_1, longlong param_2)
   ======================================================================== */

void fft_load_input_samples(longlong param_1,longlong param_2)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
                    /* 0x4c60  18  fft_load_input_samples */
  if (param_2 != 0 && param_1 != 0) {
    uVar1 = *(uint *)(param_1 + 0x1c);
    uVar2 = (ulonglong)uVar1;
    uVar3 = uVar2;
    if (uVar1 < *(int *)(param_1 + 0x14) + uVar1) {
      do {
        *(undefined4 *)(*(longlong *)(param_1 + 0x28) + uVar3 * 4) =
             *(undefined4 *)(param_2 + (ulonglong)(uint)((int)uVar3 - (int)uVar2) * 4);
        uVar3 = uVar3 + 1;
        uVar2 = (ulonglong)*(uint *)(param_1 + 0x1c);
      } while (uVar3 < *(int *)(param_1 + 0x14) + *(uint *)(param_1 + 0x1c));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180004cc0
   NAME : fft_get_output_samples
   SIG  : undefined __fastcall fft_get_output_samples(longlong param_1, longlong param_2)
   ======================================================================== */

void fft_get_output_samples(longlong param_1,longlong param_2)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
                    /* 0x4cc0  17  fft_get_output_samples */
  if (param_2 != 0 && param_1 != 0) {
    uVar1 = *(uint *)(param_1 + 0x1c);
    uVar2 = (ulonglong)uVar1;
    uVar3 = uVar2;
    if (uVar1 < *(int *)(param_1 + 0x14) + uVar1) {
      do {
        *(undefined4 *)(param_2 + (ulonglong)(uint)((int)uVar3 - (int)uVar2) * 4) =
             *(undefined4 *)(*(longlong *)(param_1 + 0x28) + uVar3 * 4);
        uVar3 = uVar3 + 1;
        uVar2 = (ulonglong)*(uint *)(param_1 + 0x1c);
      } while (uVar3 < *(int *)(param_1 + 0x14) + *(uint *)(param_1 + 0x1c));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180004d20
   NAME : compute_forward_fft
   SIG  : bool __fastcall compute_forward_fft(undefined8 * param_1)
   ======================================================================== */

bool compute_forward_fft(undefined8 *param_1)

{
                    /* 0x4d20  7  compute_forward_fft */
  if (param_1 != (undefined8 *)0x0) {
    fftwf_execute(*param_1);
  }
  return param_1 != (undefined8 *)0x0;
}



/* ========================================================================
   ENTRY: 180004d50
   NAME : compute_backward_fft
   SIG  : bool __fastcall compute_backward_fft(longlong param_1)
   ======================================================================== */

bool compute_backward_fft(longlong param_1)

{
                    /* 0x4d50  5  compute_backward_fft */
  if (param_1 != 0) {
    fftwf_execute(*(undefined8 *)(param_1 + 8));
  }
  return param_1 != 0;
}



/* ========================================================================
   ENTRY: 180004d80
   NAME : get_fft_input_buffer
   SIG  : undefined8 __fastcall get_fft_input_buffer(longlong param_1)
   ======================================================================== */

undefined8 get_fft_input_buffer(longlong param_1)

{
                    /* 0x4d80  25  get_fft_input_buffer */
  return *(undefined8 *)(param_1 + 0x28);
}



/* ========================================================================
   ENTRY: 180004d90
   NAME : get_fft_output_buffer
   SIG  : undefined8 __fastcall get_fft_output_buffer(longlong param_1)
   ======================================================================== */

undefined8 get_fft_output_buffer(longlong param_1)

{
                    /* 0x4d90  26  get_fft_output_buffer */
  return *(undefined8 *)(param_1 + 0x30);
}



/* ========================================================================
   ENTRY: 180004da0
   NAME : stft_buffer_initialize
   SIG  : undefined4 * __fastcall stft_buffer_initialize(uint param_1, undefined4 param_2, undefined4 param_3)
   ======================================================================== */

undefined4 * stft_buffer_initialize(uint param_1,undefined4 param_2,undefined4 param_3)

{
  undefined4 *puVar1;
  void *pvVar2;
  
                    /* 0x4da0  116  stft_buffer_initialize */
  puVar1 = calloc(1,0x20);
  puVar1[2] = param_1;
  puVar1[1] = param_2;
  puVar1[3] = param_3;
  *puVar1 = param_2;
  pvVar2 = calloc((ulonglong)param_1,4);
  *(void **)(puVar1 + 4) = pvVar2;
  pvVar2 = calloc((ulonglong)param_1,4);
  *(void **)(puVar1 + 6) = pvVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180004e10
   NAME : stft_buffer_free
   SIG  : undefined __fastcall stft_buffer_free(void * param_1)
   ======================================================================== */

void stft_buffer_free(void *param_1)

{
                    /* 0x4e10  115  stft_buffer_free */
  free(*(void **)((longlong)param_1 + 0x10));
  free(*(void **)((longlong)param_1 + 0x18));
                    /* WARNING: Could not recover jumptable at 0x000180004e38. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180004e40
   NAME : is_buffer_full
   SIG  : undefined4 __fastcall is_buffer_full(int * param_1)
   ======================================================================== */

undefined4 is_buffer_full(int *param_1)

{
                    /* 0x4e40  51  is_buffer_full */
  return CONCAT31((int3)((uint)*param_1 >> 8),*param_1 == param_1[2]);
}



/* ========================================================================
   ENTRY: 180004e50
   NAME : stft_buffer_fill
   SIG  : undefined4 __fastcall stft_buffer_fill(uint * param_1, undefined4 param_2)
   ======================================================================== */

undefined4 stft_buffer_fill(uint *param_1,undefined4 param_2)

{
  undefined4 uVar1;
  uint uVar2;
  
                    /* 0x4e50  114  stft_buffer_fill */
  *(undefined4 *)(*(longlong *)(param_1 + 4) + (ulonglong)*param_1 * 4) = param_2;
  uVar2 = *param_1;
  uVar1 = *(undefined4 *)(*(longlong *)(param_1 + 6) + (ulonglong)(uVar2 - param_1[1]) * 4);
  if (uVar2 < param_1[2]) {
    *param_1 = uVar2 + 1;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004e80
   NAME : stft_buffer_advance_block
   SIG  : bool __fastcall stft_buffer_advance_block(uint * param_1, void * param_2)
   ======================================================================== */

bool stft_buffer_advance_block(uint *param_1,void *param_2)

{
                    /* 0x4e80  113  stft_buffer_advance_block */
  if (param_2 != (void *)0x0) {
    *param_1 = param_1[1];
    memmove(*(void **)(param_1 + 4),
            (void *)((longlong)*(void **)(param_1 + 4) + (ulonglong)param_1[3] * 4),
            (ulonglong)param_1[1] << 2);
    memcpy(*(void **)(param_1 + 6),param_2,(ulonglong)param_1[3] << 2);
  }
  return param_2 != (void *)0x0;
}



/* ========================================================================
   ENTRY: 180004ed0
   NAME : get_full_buffer_block
   SIG  : undefined8 __fastcall get_full_buffer_block(longlong param_1)
   ======================================================================== */

undefined8 get_full_buffer_block(longlong param_1)

{
                    /* 0x4ed0  30  get_full_buffer_block
                       0x4ed0  31  get_magnitude_spectrum */
  return *(undefined8 *)(param_1 + 0x10);
}



/* ========================================================================
   ENTRY: 180004ee0
   NAME : stft_processor_initialize
   SIG  : int * __fastcall stft_processor_initialize(uint param_1, float param_2, uint param_3, int param_4, int param_5, uint param_6, uint param_7)
   ======================================================================== */

int * stft_processor_initialize
                (uint param_1,float param_2,uint param_3,int param_4,int param_5,uint param_6,
                uint param_7)

{
  uint uVar1;
  int iVar2;
  int *piVar3;
  undefined8 *puVar4;
  void *pvVar5;
  undefined4 *puVar6;
  uint uVar7;
  ulonglong uVar8;
  
                    /* 0x4ee0  118  stft_processor_initialize */
  piVar3 = calloc(1,0x40);
  uVar8 = (ulonglong)((float)param_1 * (param_2 / DAT_18000803c));
  uVar7 = (uint)uVar8;
  piVar3[4] = uVar7;
  puVar4 = fft_transform_initialize(uVar7,param_4,param_5);
  *(undefined8 **)(piVar3 + 10) = puVar4;
  uVar1 = get_fft_size((longlong)puVar4);
  piVar3[3] = uVar1;
  piVar3[2] = param_3;
  iVar2 = (int)((uVar8 & 0xffffffff) / (ulonglong)param_3);
  piVar3[1] = iVar2;
  *piVar3 = uVar7 - iVar2;
  pvVar5 = calloc((ulonglong)(uVar7 * 2),4);
  *(void **)(piVar3 + 6) = pvVar5;
  pvVar5 = calloc(uVar8 & 0xffffffff,4);
  *(void **)(piVar3 + 8) = pvVar5;
  puVar6 = stft_buffer_initialize(uVar7,uVar7 - iVar2,iVar2);
  *(undefined4 **)(piVar3 + 0xc) = puVar6;
  puVar4 = stft_window_initialize(uVar1,param_3,param_6,param_7);
  *(undefined8 **)(piVar3 + 0xe) = puVar4;
  return piVar3;
}



/* ========================================================================
   ENTRY: 180004fe0
   NAME : stft_processor_free
   SIG  : undefined __fastcall stft_processor_free(void * param_1)
   ======================================================================== */

void stft_processor_free(void *param_1)

{
                    /* 0x4fe0  117  stft_processor_free */
  stft_buffer_free(*(void **)((longlong)param_1 + 0x30));
  critical_bands_free(*(undefined8 **)((longlong)param_1 + 0x38));
  fft_transform_free(*(undefined8 **)((longlong)param_1 + 0x28));
  free(*(void **)((longlong)param_1 + 0x18));
  free(*(void **)((longlong)param_1 + 0x20));
                    /* WARNING: Could not recover jumptable at 0x000180005023. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180005030
   NAME : stft_processor_run
   SIG  : ulonglong __fastcall stft_processor_run(longlong param_1, uint param_2, longlong param_3, longlong param_4, undefined * param_5, undefined8 param_6)
   ======================================================================== */

ulonglong stft_processor_run(longlong param_1,uint param_2,longlong param_3,longlong param_4,
                            undefined *param_5,undefined8 param_6)

{
  longlong lVar1;
  undefined8 uVar2;
  ulonglong uVar3;
  undefined8 unaff_RBP;
  ulonglong uVar4;
  undefined4 uVar5;
  
                    /* 0x5030  119  stft_processor_run */
  if ((param_2 != 0 && param_4 != 0) && (param_3 != 0 && param_1 != 0)) {
    uVar4 = 0;
    do {
      uVar5 = stft_buffer_fill(*(uint **)(param_1 + 0x30),*(undefined4 *)(param_3 + uVar4 * 4));
      *(undefined4 *)(param_4 + uVar4 * 4) = uVar5;
      uVar5 = is_buffer_full(*(int **)(param_1 + 0x30));
      if ((char)uVar5 != '\0') {
        lVar1 = get_full_buffer_block(*(longlong *)(param_1 + 0x30));
        fft_load_input_samples(*(longlong *)(param_1 + 0x28),lVar1);
        lVar1 = get_fft_input_buffer(*(longlong *)(param_1 + 0x28));
        stft_window_apply(*(longlong **)(param_1 + 0x38),lVar1,1);
        compute_forward_fft(*(undefined8 **)(param_1 + 0x28));
        uVar2 = get_fft_output_buffer(*(longlong *)(param_1 + 0x28));
        (*(code *)param_5)(param_6,uVar2);
        compute_backward_fft(*(longlong *)(param_1 + 0x28));
        lVar1 = get_fft_input_buffer(*(longlong *)(param_1 + 0x28));
        stft_window_apply(*(longlong **)(param_1 + 0x38),lVar1,2);
        fft_get_output_samples(*(longlong *)(param_1 + 0x28),*(longlong *)(param_1 + 0x20));
        if (*(int *)(param_1 + 0x10) != 0) {
          uVar3 = 0;
          do {
            *(float *)(*(longlong *)(param_1 + 0x18) + uVar3 * 4) =
                 *(float *)(*(longlong *)(param_1 + 0x20) + uVar3 * 4) +
                 *(float *)(*(longlong *)(param_1 + 0x18) + uVar3 * 4);
            uVar3 = uVar3 + 1;
          } while (uVar3 < *(uint *)(param_1 + 0x10));
        }
        stft_buffer_advance_block(*(uint **)(param_1 + 0x30),*(void **)(param_1 + 0x18));
        memmove(*(void **)(param_1 + 0x18),
                (void *)((longlong)*(void **)(param_1 + 0x18) +
                        (ulonglong)*(uint *)(param_1 + 4) * 4),
                (ulonglong)*(uint *)(param_1 + 0x10) << 2);
      }
      uVar4 = uVar4 + 1;
    } while (uVar4 != param_2);
  }
  return CONCAT71((int7)((ulonglong)unaff_RBP >> 8),
                  (param_2 != 0 && param_4 != 0) && (param_3 != 0 && param_1 != 0)) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 1800051b0
   NAME : get_stft_fft_size
   SIG  : undefined4 __fastcall get_stft_fft_size(longlong param_1)
   ======================================================================== */

undefined4 get_stft_fft_size(longlong param_1)

{
                    /* 0x51b0  45  get_stft_fft_size */
  return *(undefined4 *)(param_1 + 0xc);
}



/* ========================================================================
   ENTRY: 1800051c0
   NAME : get_stft_real_spectrum_size
   SIG  : undefined __fastcall get_stft_real_spectrum_size(longlong param_1)
   ======================================================================== */

void get_stft_real_spectrum_size(longlong param_1)

{
                    /* 0x51c0  47  get_stft_real_spectrum_size */
  get_fft_real_spectrum_size(*(longlong *)(param_1 + 0x28));
  return;
}



/* ========================================================================
   ENTRY: 1800051d0
   NAME : stft_window_initialize
   SIG  : undefined8 * __fastcall stft_window_initialize(uint param_1, uint param_2, uint param_3, uint param_4)
   ======================================================================== */

undefined8 * stft_window_initialize(uint param_1,uint param_2,uint param_3,uint param_4)

{
  undefined8 *puVar1;
  void *pvVar2;
  void *pvVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  float fVar6;
  
                    /* 0x51d0  122  stft_window_initialize */
  puVar1 = calloc(1,0x18);
  *(uint *)(puVar1 + 2) = param_1;
  pvVar2 = calloc((ulonglong)param_1,4);
  *puVar1 = pvVar2;
  pvVar3 = calloc((ulonglong)param_1,4);
  puVar1[1] = pvVar3;
  get_fft_window((longlong)pvVar2,param_1,param_3);
  get_fft_window((longlong)pvVar3,param_1,param_4);
  fVar6 = 0.0;
  if (1 < param_2) {
    if (param_1 == 0) {
      fVar6 = 0.0;
    }
    else {
      if (param_1 < 4) {
        fVar6 = 0.0;
        uVar4 = 0;
      }
      else {
        fVar6 = 0.0;
        uVar4 = 0;
        do {
          fVar6 = *(float *)((longlong)pvVar2 + uVar4 * 4 + 0xc) *
                  *(float *)((longlong)pvVar3 + uVar4 * 4 + 0xc) +
                  *(float *)((longlong)pvVar2 + uVar4 * 4 + 8) *
                  *(float *)((longlong)pvVar3 + uVar4 * 4 + 8) +
                  *(float *)((longlong)pvVar2 + uVar4 * 4 + 4) *
                  *(float *)((longlong)pvVar3 + uVar4 * 4 + 4) +
                  *(float *)((longlong)pvVar2 + uVar4 * 4) *
                  *(float *)((longlong)pvVar3 + uVar4 * 4) + fVar6;
          uVar4 = uVar4 + 4;
        } while ((param_1 & 0xfffffffc) != uVar4);
      }
      if ((ulonglong)(param_1 & 3) != 0) {
        uVar5 = 0;
        do {
          fVar6 = fVar6 + *(float *)((longlong)pvVar2 + uVar5 * 4 + uVar4 * 4) *
                          *(float *)((longlong)pvVar3 + uVar5 * 4 + uVar4 * 4);
          uVar5 = uVar5 + 1;
        } while ((param_1 & 3) != uVar5);
      }
    }
    fVar6 = (float)param_2 * fVar6;
  }
  *(float *)((longlong)puVar1 + 0x14) = fVar6;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180005320
   NAME : stft_window_apply
   SIG  : undefined __fastcall stft_window_apply(longlong * param_1, longlong param_2, int param_3)
   ======================================================================== */

void stft_window_apply(longlong *param_1,longlong param_2,int param_3)

{
  ulonglong uVar1;
  
                    /* 0x5320  120  stft_window_apply */
  if ((param_2 != 0 && param_1 != (longlong *)0x0) && ((int)param_1[2] != 0)) {
    if (param_3 == 1) {
      uVar1 = 0;
      do {
        *(float *)(param_2 + uVar1 * 4) =
             *(float *)(*param_1 + uVar1 * 4) * *(float *)(param_2 + uVar1 * 4);
        uVar1 = uVar1 + 1;
      } while (uVar1 < *(uint *)(param_1 + 2));
    }
    else if (param_3 == 2) {
      uVar1 = 0;
      do {
        *(float *)(param_2 + uVar1 * 4) =
             (*(float *)(param_1[1] + uVar1 * 4) / *(float *)((longlong)param_1 + 0x14)) *
             *(float *)(param_2 + uVar1 * 4);
        uVar1 = uVar1 + 1;
      } while (uVar1 < *(uint *)(param_1 + 2));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800053b0
   NAME : denoise_mixer_initialize
   SIG  : undefined8 * __fastcall denoise_mixer_initialize(uint param_1, uint param_2, uint param_3)
   ======================================================================== */

undefined8 * denoise_mixer_initialize(uint param_1,uint param_2,uint param_3)

{
  undefined8 *puVar1;
  void *pvVar2;
  undefined8 *puVar3;
  
                    /* 0x53b0  12  denoise_mixer_initialize */
  puVar1 = calloc(1,0x28);
  *(uint *)(puVar1 + 3) = param_1;
  *(uint *)((longlong)puVar1 + 0x1c) = (param_1 >> 1) + 1;
  *(uint *)(puVar1 + 4) = param_2;
  *(uint *)((longlong)puVar1 + 0x24) = param_3;
  pvVar2 = calloc((ulonglong)param_1,4);
  puVar1[1] = pvVar2;
  pvVar2 = calloc((ulonglong)param_1,4);
  puVar1[2] = pvVar2;
  puVar3 = spectral_whitening_initialize(param_1,param_2,param_3);
  *puVar1 = puVar3;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180005440
   NAME : denoise_mixer_free
   SIG  : undefined __fastcall denoise_mixer_free(undefined8 * param_1)
   ======================================================================== */

void denoise_mixer_free(undefined8 *param_1)

{
                    /* 0x5440  11  denoise_mixer_free */
  spectral_whitening_free((undefined8 *)*param_1);
  free((void *)param_1[1]);
  free((void *)param_1[2]);
                    /* WARNING: Could not recover jumptable at 0x000180005470. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180005480
   NAME : denoise_mixer_run
   SIG  : bool __fastcall denoise_mixer_run(undefined8 * param_1, longlong param_2, longlong param_3, float * param_4)
   ======================================================================== */

bool denoise_mixer_run(undefined8 *param_1,longlong param_2,longlong param_3,float *param_4)

{
  ulonglong uVar1;
  
                    /* 0x5480  13  denoise_mixer_run */
  if (param_3 != 0 && param_2 != 0) {
    if (1 < *(uint *)(param_1 + 3)) {
      uVar1 = 1;
      do {
        *(float *)(param_1[2] + uVar1 * 4) =
             *(float *)(param_2 + uVar1 * 4) * *(float *)(param_3 + uVar1 * 4);
        uVar1 = uVar1 + 1;
      } while (uVar1 < *(uint *)(param_1 + 3));
      if (1 < *(uint *)(param_1 + 3)) {
        uVar1 = 1;
        do {
          *(float *)(param_1[1] + uVar1 * 4) =
               *(float *)(param_2 + uVar1 * 4) - *(float *)(param_1[2] + uVar1 * 4);
          uVar1 = uVar1 + 1;
        } while (uVar1 < *(uint *)(param_1 + 3));
      }
    }
    if (0.0 < param_4[2]) {
      spectral_whitening_run((longlong *)*param_1,param_4[2],param_1[1]);
    }
    if (*(char *)(param_4 + 1) == '\0') {
      if (1 < *(uint *)(param_1 + 3)) {
        uVar1 = 1;
        do {
          *(float *)(param_2 + uVar1 * 4) =
               *(float *)(param_1[1] + uVar1 * 4) * *param_4 + *(float *)(param_1[2] + uVar1 * 4);
          uVar1 = uVar1 + 1;
        } while (uVar1 < *(uint *)(param_1 + 3));
      }
    }
    else if (1 < *(uint *)(param_1 + 3)) {
      uVar1 = 1;
      do {
        *(undefined4 *)(param_2 + uVar1 * 4) = *(undefined4 *)(param_1[1] + uVar1 * 4);
        uVar1 = uVar1 + 1;
      } while (uVar1 < *(uint *)(param_1 + 3));
    }
  }
  return param_3 != 0 && param_2 != 0;
}



/* ========================================================================
   ENTRY: 1800055c0
   NAME : sanitize_denormal
   SIG  : undefined8 __fastcall sanitize_denormal(undefined8 param_1)
   ======================================================================== */

undefined8 sanitize_denormal(undefined8 param_1)

{
                    /* 0x55c0  76  sanitize_denormal */
  if (((float)param_1 != 0.0) && ((int)ABS((float)param_1) - 0x800000U < 0x7f000000)) {
    return param_1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800055f0
   NAME : from_db_to_coefficient
   SIG  : undefined __fastcall from_db_to_coefficient(float param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void from_db_to_coefficient(float param_1)

{
                    /* 0x55f0  23  from_db_to_coefficient */
                    /* WARNING: Could not recover jumptable at 0x000180005600. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  expf((param_1 / DAT_180008348) * _DAT_18000837c);
  return;
}



/* ========================================================================
   ENTRY: 180005610
   NAME : remap_percentage_log_like_unity
   SIG  : float __fastcall remap_percentage_log_like_unity(float param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

float remap_percentage_log_like_unity(float param_1)

{
  float fVar1;
  
                    /* 0x5610  74  remap_percentage_log_like_unity */
  fVar1 = expf(param_1 * _DAT_180008380);
  return DAT_180008010 - fVar1;
}



/* ========================================================================
   ENTRY: 180005640
   NAME : get_next_divisible_two
   SIG  : uint __fastcall get_next_divisible_two(int param_1)
   ======================================================================== */

uint get_next_divisible_two(int param_1)

{
  uint uVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  
                    /* 0x5640  32  get_next_divisible_two */
  uVar1 = param_1 - (param_1 >> 0x1f) & 0xfffffffe;
  uVar3 = (uVar1 + (uint)(0 < param_1) * 4) - 2;
  uVar5 = -(param_1 % 2);
  if ((int)uVar5 < 0) {
    uVar5 = param_1 % 2;
  }
  uVar2 = param_1 - uVar3;
  uVar4 = -uVar2;
  if ((int)uVar4 < 0) {
    uVar4 = uVar2;
  }
  if (uVar4 <= uVar5) {
    uVar1 = uVar3;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180005680
   NAME : get_next_power_two
   SIG  : int __fastcall get_next_power_two(int param_1)
   ======================================================================== */

int get_next_power_two(int param_1)

{
  float fVar1;
  
                    /* 0x5680  33  get_next_power_two */
  fVar1 = (float)log2f((float)param_1);
  ceilf(fVar1);
  exp2f();
  fVar1 = (float)roundf();
  return (int)fVar1;
}



/* ========================================================================
   ENTRY: 1800056b0
   NAME : spectral_features_initialize
   SIG  : undefined8 * __fastcall spectral_features_initialize(uint param_1)
   ======================================================================== */

undefined8 * spectral_features_initialize(uint param_1)

{
  undefined8 *puVar1;
  void *pvVar2;
  ulonglong _Count;
  
                    /* 0x56b0  102  spectral_features_initialize */
  puVar1 = calloc(1,0x20);
  *(uint *)(puVar1 + 3) = param_1;
  _Count = (ulonglong)param_1;
  pvVar2 = calloc(_Count,4);
  *puVar1 = pvVar2;
  pvVar2 = calloc(_Count,4);
  puVar1[1] = pvVar2;
  pvVar2 = calloc(_Count,4);
  puVar1[2] = pvVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180005710
   NAME : spectral_features_free
   SIG  : undefined __fastcall spectral_features_free(undefined8 * param_1)
   ======================================================================== */

void spectral_features_free(undefined8 *param_1)

{
                    /* 0x5710  101  spectral_features_free */
  free((void *)*param_1);
  free((void *)param_1[1]);
  free((void *)param_1[2]);
                    /* WARNING: Could not recover jumptable at 0x00018000573d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180005740
   NAME : get_power_spectrum
   SIG  : undefined8 __fastcall get_power_spectrum(undefined8 * param_1)
   ======================================================================== */

undefined8 get_power_spectrum(undefined8 *param_1)

{
                    /* 0x5740  39  get_power_spectrum */
  return *param_1;
}



/* ========================================================================
   ENTRY: 180005750
   NAME : get_spectral_feature
   SIG  : longlong __fastcall get_spectral_feature(longlong * param_1, float * param_2, uint param_3, int param_4)
   ======================================================================== */

longlong get_spectral_feature(longlong *param_1,float *param_2,uint param_3,int param_4)

{
  longlong lVar1;
  ulonglong uVar2;
  longlong *plVar3;
  float fVar4;
  
                    /* 0x5750  42  get_spectral_feature */
  if (param_3 == 0 || (param_2 == (float *)0x0 || param_1 == (longlong *)0x0)) {
LAB_180005778:
    lVar1 = 0;
  }
  else {
    if (param_4 == 2) {
      fVar4 = atan2f(*param_2,0.0);
      *(float *)param_1[1] = fVar4;
      plVar3 = param_1 + 1;
      if (1 < *(uint *)(param_1 + 3)) {
        uVar2 = 1;
        do {
          param_3 = param_3 - 1;
          fVar4 = atan2f(param_2[uVar2],param_2[param_3]);
          *(float *)(param_1[1] + uVar2 * 4) = fVar4;
          uVar2 = uVar2 + 1;
        } while (uVar2 < *(uint *)(param_1 + 3));
      }
    }
    else if (param_4 == 1) {
      *(float *)param_1[2] = *param_2;
      plVar3 = param_1 + 2;
      if (1 < *(uint *)(param_1 + 3)) {
        uVar2 = 1;
        do {
          param_3 = param_3 - 1;
          *(float *)(param_1[2] + uVar2 * 4) =
               SQRT(param_2[uVar2] * param_2[uVar2] + param_2[param_3] * param_2[param_3]);
          uVar2 = uVar2 + 1;
        } while (uVar2 < *(uint *)(param_1 + 3));
      }
    }
    else {
      if (param_4 != 0) goto LAB_180005778;
      *(float *)*param_1 = *param_2 * *param_2;
      plVar3 = param_1;
      if (1 < *(uint *)(param_1 + 3)) {
        uVar2 = 1;
        do {
          param_3 = param_3 - 1;
          *(float *)(*param_1 + uVar2 * 4) =
               param_2[uVar2] * param_2[uVar2] + param_2[param_3] * param_2[param_3];
          uVar2 = uVar2 + 1;
        } while (uVar2 < *(uint *)(param_1 + 3));
      }
    }
    lVar1 = *plVar3;
  }
  return lVar1;
}



/* ========================================================================
   ENTRY: 1800058d0
   NAME : spectral_trailing_buffer_initialize
   SIG  : uint * __fastcall spectral_trailing_buffer_initialize(uint param_1, uint param_2)
   ======================================================================== */

uint * spectral_trailing_buffer_initialize(uint param_1,uint param_2)

{
  uint *puVar1;
  void *pvVar2;
  
                    /* 0x58d0  108  spectral_trailing_buffer_initialize */
  puVar1 = calloc(1,0x10);
  *puVar1 = param_1;
  puVar1[1] = param_2;
  pvVar2 = calloc((ulonglong)param_2 * (ulonglong)param_1,4);
  *(void **)(puVar1 + 2) = pvVar2;
  return puVar1;
}



/* ========================================================================
   ENTRY: 180005920
   NAME : spectral_trailing_buffer_push_back
   SIG  : bool __fastcall spectral_trailing_buffer_push_back(uint * param_1, void * param_2)
   ======================================================================== */

bool spectral_trailing_buffer_push_back(uint *param_1,void *param_2)

{
                    /* 0x5920  109  spectral_trailing_buffer_push_back */
  if (param_2 != (void *)0x0) {
    memmove(*(void **)(param_1 + 2),
            (void *)((longlong)*(void **)(param_1 + 2) + (ulonglong)*param_1 * 4),
            (ulonglong)(param_1[1] - 1) * (ulonglong)*param_1 * 4);
    memcpy((void *)((ulonglong)(param_1[1] - 1) * (ulonglong)*param_1 * 4 +
                   *(longlong *)(param_1 + 2)),param_2,(ulonglong)*param_1 << 2);
  }
  return param_2 != (void *)0x0;
}



/* ========================================================================
   ENTRY: 180005980
   NAME : get_fft_window
   SIG  : ulonglong __fastcall get_fft_window(longlong param_1, uint param_2, uint param_3)
   ======================================================================== */

ulonglong get_fft_window(longlong param_1,uint param_2,uint param_3)

{
  double dVar1;
  double dVar2;
  double dVar3;
  float fVar4;
  double dVar5;
  float fVar6;
  float fVar7;
  undefined8 unaff_RBX;
  ulonglong uVar8;
  ulonglong uVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  undefined4 extraout_XMM0_Db;
  undefined4 extraout_XMM0_Db_00;
  undefined4 extraout_XMM0_Db_01;
  undefined8 uVar13;
  undefined4 extraout_XMM0_Db_02;
  float fVar14;
  
  fVar7 = DAT_1800083b0;
  fVar11 = DAT_1800083ac;
  fVar6 = DAT_1800083a8;
  dVar5 = DAT_1800083a0;
  fVar10 = DAT_18000839c;
  fVar4 = DAT_180008398;
  dVar3 = DAT_180008390;
  dVar1 = DAT_180008388;
  dVar2 = DAT_180008090;
                    /* 0x5980  29  get_fft_window */
  fVar12 = DAT_180008038;
  if ((param_2 != 0 && param_1 != 0) && (param_3 < 4)) {
    uVar8 = (ulonglong)param_2;
    fVar14 = (float)uVar8;
    switch(param_3) {
    case 0:
      uVar9 = 0;
      do {
        fVar10 = cosf((float)((double)((float)(uVar9 & 0xffffffff) / fVar14) * dVar2));
        uVar13 = sanitize_denormal(CONCAT44(extraout_XMM0_Db,fVar10 * fVar4 + fVar12));
        *(int *)(param_1 + uVar9 * 4) = (int)uVar13;
        uVar9 = uVar9 + 1;
      } while (uVar8 != uVar9);
      break;
    case 1:
      uVar9 = 0;
      do {
        fVar12 = cosf((float)((double)((float)(uVar9 & 0xffffffff) / fVar14) * dVar2));
        uVar13 = sanitize_denormal(CONCAT44(extraout_XMM0_Db_02,fVar12 * fVar11 + fVar7));
        *(int *)(param_1 + uVar9 * 4) = (int)uVar13;
        uVar9 = uVar9 + 1;
      } while (uVar8 != uVar9);
      break;
    case 2:
      uVar9 = 0;
      do {
        dVar1 = (double)((float)(uVar9 & 0xffffffff) / fVar14);
        fVar12 = cosf((float)(dVar1 * dVar2));
        fVar11 = cosf((float)(dVar1 * dVar5));
        uVar13 = sanitize_denormal(CONCAT44(extraout_XMM0_Db_00,
                                            fVar11 * fVar6 + fVar12 * fVar4 + fVar10));
        *(int *)(param_1 + uVar9 * 4) = (int)uVar13;
        uVar9 = uVar9 + 1;
      } while (uVar8 != uVar9);
      break;
    case 3:
      uVar9 = 0;
      do {
        fVar12 = sinf((float)((double)((float)(uVar9 & 0xffffffff) / fVar14) * dVar1));
        fVar12 = sinf((float)((double)(fVar12 * fVar12) * dVar3));
        uVar13 = sanitize_denormal(CONCAT44(extraout_XMM0_Db_01,fVar12));
        *(int *)(param_1 + uVar9 * 4) = (int)uVar13;
        uVar9 = uVar9 + 1;
      } while (uVar8 != uVar9);
    }
  }
  return CONCAT71((int7)((ulonglong)unaff_RBX >> 8),param_2 != 0 && param_1 != 0) & 0xffffffff;
}



/* ========================================================================
   ENTRY: 180005c30
   NAME : initialize_spectrum_with_value
   SIG  : undefined __fastcall initialize_spectrum_with_value(longlong param_1, uint param_2, undefined4 param_3)
   ======================================================================== */

void initialize_spectrum_with_value(longlong param_1,uint param_2,undefined4 param_3)

{
  undefined4 *puVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
                    /* 0x5c30  50  initialize_spectrum_with_value */
  if (param_2 != 0 && param_1 != 0) {
    if (param_2 < 8) {
      uVar2 = 0;
    }
    else {
      uVar2 = (ulonglong)(param_2 & 0xfffffff8);
      uVar3 = 0;
      do {
        puVar1 = (undefined4 *)(param_1 + uVar3);
        *puVar1 = param_3;
        puVar1[1] = param_3;
        puVar1[2] = param_3;
        puVar1[3] = param_3;
        puVar1 = (undefined4 *)(param_1 + 0x10 + uVar3);
        *puVar1 = param_3;
        puVar1[1] = param_3;
        puVar1[2] = param_3;
        puVar1[3] = param_3;
        uVar3 = uVar3 + 0x20;
      } while (((ulonglong)param_2 * 4 & 0xffffffffffffffe0) != uVar3);
      if ((param_2 & 0xfffffff8) == param_2) {
        return;
      }
    }
    do {
      *(undefined4 *)(param_1 + uVar2 * 4) = param_3;
      uVar2 = uVar2 + 1;
    } while (param_2 != uVar2);
  }
  return;
}



/* ========================================================================
   ENTRY: 180005ca0
   NAME : max_spectral_value
   SIG  : undefined __fastcall max_spectral_value(longlong param_1, uint param_2)
   ======================================================================== */

void max_spectral_value(longlong param_1,uint param_2)

{
  float fVar1;
  ulonglong uVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  
                    /* 0x5ca0  60  max_spectral_value */
  if (param_2 != 0 && param_1 != 0) {
    fVar3 = *(float *)(param_1 + 4);
    if ((2 < param_2) && (param_2 != 3)) {
      uVar2 = 2;
      do {
        fVar4 = *(float *)(param_1 + uVar2 * 4);
        fVar1 = *(float *)(param_1 + 4 + uVar2 * 4);
        fVar5 = fVar3;
        if (fVar3 <= fVar4) {
          fVar5 = fVar4;
        }
        fVar4 = (float)(-(uint)NAN(fVar4) & (uint)fVar3 | ~-(uint)NAN(fVar4) & (uint)fVar5);
        fVar3 = fVar4;
        if (fVar4 <= fVar1) {
          fVar3 = fVar1;
        }
        fVar3 = (float)(~-(uint)NAN(fVar1) & (uint)fVar3 | -(uint)NAN(fVar1) & (uint)fVar4);
        uVar2 = uVar2 + 2;
      } while ((param_2 & 0xfffffffe) != uVar2);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180005d50
   NAME : min_spectral_value
   SIG  : undefined __fastcall min_spectral_value(longlong param_1, uint param_2)
   ======================================================================== */

void min_spectral_value(longlong param_1,uint param_2)

{
  float fVar1;
  ulonglong uVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  
                    /* 0x5d50  62  min_spectral_value */
  if (param_2 != 0 && param_1 != 0) {
    fVar3 = *(float *)(param_1 + 4);
    if ((2 < param_2) && (param_2 != 3)) {
      uVar2 = 2;
      do {
        fVar4 = *(float *)(param_1 + uVar2 * 4);
        fVar1 = *(float *)(param_1 + 4 + uVar2 * 4);
        fVar5 = fVar3;
        if (fVar4 <= fVar3) {
          fVar5 = fVar4;
        }
        fVar4 = (float)(-(uint)NAN(fVar4) & (uint)fVar3 | ~-(uint)NAN(fVar4) & (uint)fVar5);
        fVar3 = fVar4;
        if (fVar1 <= fVar4) {
          fVar3 = fVar1;
        }
        fVar3 = (float)(~-(uint)NAN(fVar1) & (uint)fVar3 | -(uint)NAN(fVar1) & (uint)fVar4);
        uVar2 = uVar2 + 2;
      } while ((param_2 & 0xfffffffe) != uVar2);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180005e00
   NAME : min_spectrum
   SIG  : undefined __fastcall min_spectrum(ulonglong param_1, ulonglong param_2, uint param_3)
   ======================================================================== */

void min_spectrum(ulonglong param_1,ulonglong param_2,uint param_3)

{
  uint *puVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  ulonglong uVar10;
  ulonglong uVar11;
  ulonglong uVar12;
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  
                    /* 0x5e00  63  min_spectrum */
  if (param_3 != 0 && (param_2 != 0 && param_1 != 0)) {
    uVar11 = (ulonglong)param_3;
    if ((param_3 < 8) || (param_1 < param_2 + uVar11 * 4 && param_2 < param_1 + uVar11 * 4)) {
      uVar10 = 0;
    }
    else {
      uVar10 = (ulonglong)(param_3 & 0xfffffff8);
      uVar12 = 0;
      do {
        auVar6 = *(undefined1 (*) [16])(param_1 + uVar12);
        auVar7 = *(undefined1 (*) [16])(param_1 + 0x10 + uVar12);
        auVar8 = *(undefined1 (*) [16])(param_2 + uVar12);
        auVar9 = *(undefined1 (*) [16])(param_2 + 0x10 + uVar12);
        auVar14 = minps(auVar8,auVar6);
        auVar13 = minps(auVar9,auVar7);
        puVar1 = (uint *)(param_1 + uVar12);
        *puVar1 = ~-(uint)NAN(auVar6._0_4_) & auVar14._0_4_ |
                  auVar8._0_4_ & -(uint)NAN(auVar6._0_4_);
        puVar1[1] = ~-(uint)NAN(auVar6._4_4_) & auVar14._4_4_ |
                    auVar8._4_4_ & -(uint)NAN(auVar6._4_4_);
        puVar1[2] = ~-(uint)NAN(auVar6._8_4_) & auVar14._8_4_ |
                    auVar8._8_4_ & -(uint)NAN(auVar6._8_4_);
        puVar1[3] = ~-(uint)NAN(auVar6._12_4_) & auVar14._12_4_ |
                    auVar8._12_4_ & -(uint)NAN(auVar6._12_4_);
        puVar1 = (uint *)(param_1 + 0x10 + uVar12);
        *puVar1 = ~-(uint)NAN(auVar7._0_4_) & auVar13._0_4_ |
                  auVar9._0_4_ & -(uint)NAN(auVar7._0_4_);
        puVar1[1] = ~-(uint)NAN(auVar7._4_4_) & auVar13._4_4_ |
                    auVar9._4_4_ & -(uint)NAN(auVar7._4_4_);
        puVar1[2] = ~-(uint)NAN(auVar7._8_4_) & auVar13._8_4_ |
                    auVar9._8_4_ & -(uint)NAN(auVar7._8_4_);
        puVar1[3] = ~-(uint)NAN(auVar7._12_4_) & auVar13._12_4_ |
                    auVar9._12_4_ & -(uint)NAN(auVar7._12_4_);
        uVar12 = uVar12 + 0x20;
      } while ((uVar11 * 4 & 0xffffffffffffffe0) != uVar12);
      if ((param_3 & 0xfffffff8) == param_3) {
        return;
      }
    }
    uVar12 = uVar10;
    if ((param_3 & 1) != 0) {
      fVar2 = *(float *)(param_1 + uVar10 * 4);
      fVar3 = *(float *)(param_2 + uVar10 * 4);
      fVar4 = fVar3;
      if (fVar2 <= fVar3) {
        fVar4 = fVar2;
      }
      *(uint *)(param_1 + uVar10 * 4) =
           ~-(uint)NAN(fVar2) & (uint)fVar4 | -(uint)NAN(fVar2) & (uint)fVar3;
      uVar12 = uVar10 | 1;
    }
    if (uVar10 != uVar11 - 1) {
      do {
        fVar2 = *(float *)(param_2 + uVar12 * 4);
        fVar3 = *(float *)(param_1 + uVar12 * 4);
        fVar4 = *(float *)(param_1 + 4 + uVar12 * 4);
        fVar5 = fVar2;
        if (fVar3 <= fVar2) {
          fVar5 = fVar3;
        }
        *(uint *)(param_1 + uVar12 * 4) =
             ~-(uint)NAN(fVar3) & (uint)fVar5 | -(uint)NAN(fVar3) & (uint)fVar2;
        fVar2 = *(float *)(param_2 + 4 + uVar12 * 4);
        fVar3 = fVar2;
        if (fVar4 <= fVar2) {
          fVar3 = fVar4;
        }
        *(uint *)(param_1 + 4 + uVar12 * 4) =
             ~-(uint)NAN(fVar4) & (uint)fVar3 | -(uint)NAN(fVar4) & (uint)fVar2;
        uVar12 = uVar12 + 2;
      } while (uVar11 != uVar12);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180005f80
   NAME : max_spectrum
   SIG  : undefined __fastcall max_spectrum(ulonglong param_1, ulonglong param_2, uint param_3)
   ======================================================================== */

void max_spectrum(ulonglong param_1,ulonglong param_2,uint param_3)

{
  uint *puVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  ulonglong uVar10;
  ulonglong uVar11;
  ulonglong uVar12;
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  
                    /* 0x5f80  61  max_spectrum */
  if (param_3 != 0 && (param_2 != 0 && param_1 != 0)) {
    uVar11 = (ulonglong)param_3;
    if ((param_3 < 8) || (param_1 < param_2 + uVar11 * 4 && param_2 < param_1 + uVar11 * 4)) {
      uVar10 = 0;
    }
    else {
      uVar10 = (ulonglong)(param_3 & 0xfffffff8);
      uVar12 = 0;
      do {
        auVar6 = *(undefined1 (*) [16])(param_1 + uVar12);
        auVar7 = *(undefined1 (*) [16])(param_1 + 0x10 + uVar12);
        auVar8 = *(undefined1 (*) [16])(param_2 + uVar12);
        auVar9 = *(undefined1 (*) [16])(param_2 + 0x10 + uVar12);
        auVar14 = maxps(auVar8,auVar6);
        auVar13 = maxps(auVar9,auVar7);
        puVar1 = (uint *)(param_1 + uVar12);
        *puVar1 = ~-(uint)NAN(auVar6._0_4_) & auVar14._0_4_ |
                  auVar8._0_4_ & -(uint)NAN(auVar6._0_4_);
        puVar1[1] = ~-(uint)NAN(auVar6._4_4_) & auVar14._4_4_ |
                    auVar8._4_4_ & -(uint)NAN(auVar6._4_4_);
        puVar1[2] = ~-(uint)NAN(auVar6._8_4_) & auVar14._8_4_ |
                    auVar8._8_4_ & -(uint)NAN(auVar6._8_4_);
        puVar1[3] = ~-(uint)NAN(auVar6._12_4_) & auVar14._12_4_ |
                    auVar8._12_4_ & -(uint)NAN(auVar6._12_4_);
        puVar1 = (uint *)(param_1 + 0x10 + uVar12);
        *puVar1 = ~-(uint)NAN(auVar7._0_4_) & auVar13._0_4_ |
                  auVar9._0_4_ & -(uint)NAN(auVar7._0_4_);
        puVar1[1] = ~-(uint)NAN(auVar7._4_4_) & auVar13._4_4_ |
                    auVar9._4_4_ & -(uint)NAN(auVar7._4_4_);
        puVar1[2] = ~-(uint)NAN(auVar7._8_4_) & auVar13._8_4_ |
                    auVar9._8_4_ & -(uint)NAN(auVar7._8_4_);
        puVar1[3] = ~-(uint)NAN(auVar7._12_4_) & auVar13._12_4_ |
                    auVar9._12_4_ & -(uint)NAN(auVar7._12_4_);
        uVar12 = uVar12 + 0x20;
      } while ((uVar11 * 4 & 0xffffffffffffffe0) != uVar12);
      if ((param_3 & 0xfffffff8) == param_3) {
        return;
      }
    }
    uVar12 = uVar10;
    if ((param_3 & 1) != 0) {
      fVar2 = *(float *)(param_1 + uVar10 * 4);
      fVar3 = *(float *)(param_2 + uVar10 * 4);
      fVar4 = fVar3;
      if (fVar3 <= fVar2) {
        fVar4 = fVar2;
      }
      *(uint *)(param_1 + uVar10 * 4) =
           ~-(uint)NAN(fVar2) & (uint)fVar4 | -(uint)NAN(fVar2) & (uint)fVar3;
      uVar12 = uVar10 | 1;
    }
    if (uVar10 != uVar11 - 1) {
      do {
        fVar2 = *(float *)(param_2 + uVar12 * 4);
        fVar3 = *(float *)(param_1 + uVar12 * 4);
        fVar4 = *(float *)(param_1 + 4 + uVar12 * 4);
        fVar5 = fVar2;
        if (fVar2 <= fVar3) {
          fVar5 = fVar3;
        }
        *(uint *)(param_1 + uVar12 * 4) =
             ~-(uint)NAN(fVar3) & (uint)fVar5 | -(uint)NAN(fVar3) & (uint)fVar2;
        fVar2 = *(float *)(param_2 + 4 + uVar12 * 4);
        fVar3 = fVar2;
        if (fVar2 <= fVar4) {
          fVar3 = fVar4;
        }
        *(uint *)(param_1 + 4 + uVar12 * 4) =
             ~-(uint)NAN(fVar4) & (uint)fVar3 | -(uint)NAN(fVar4) & (uint)fVar2;
        uVar12 = uVar12 + 2;
      } while (uVar11 != uVar12);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180006100
   NAME : direct_matrix_to_vector_spectral_convolution
   SIG  : undefined __fastcall direct_matrix_to_vector_spectral_convolution(longlong param_1, longlong param_2, longlong param_3, uint param_4)
   ======================================================================== */

void direct_matrix_to_vector_spectral_convolution
               (longlong param_1,longlong param_2,longlong param_3,uint param_4)

{
  ulonglong uVar1;
  uint uVar2;
  ulonglong uVar3;
  uint uVar4;
  float fVar5;
  
                    /* 0x6100  14  direct_matrix_to_vector_spectral_convolution */
  if ((param_4 != 0 && param_3 != 0) && (param_2 != 0 && param_1 != 0)) {
    uVar2 = 1;
    uVar3 = 0;
    do {
      *(undefined4 *)(param_3 + uVar3 * 4) = 0;
      fVar5 = 0.0;
      if (param_4 == 1) {
        uVar1 = 0;
      }
      else {
        uVar1 = 0;
        uVar4 = uVar2;
        do {
          fVar5 = *(float *)(param_1 + (ulonglong)(uVar4 - 1) * 4) * *(float *)(param_2 + uVar1 * 4)
                  + fVar5;
          *(float *)(param_3 + uVar3 * 4) = fVar5;
          fVar5 = *(float *)(param_1 + (ulonglong)uVar4 * 4) * *(float *)(param_2 + 4 + uVar1 * 4) +
                  fVar5;
          *(float *)(param_3 + uVar3 * 4) = fVar5;
          uVar1 = uVar1 + 2;
          uVar4 = uVar4 + 2;
        } while ((param_4 & 0xfffffffe) != uVar1);
      }
      if ((param_4 & 1) != 0) {
        *(float *)(param_3 + uVar3 * 4) =
             *(float *)(param_1 + (ulonglong)((int)uVar3 * param_4 + (int)uVar1) * 4) *
             *(float *)(param_2 + uVar1 * 4) + fVar5;
      }
      uVar3 = uVar3 + 1;
      uVar2 = uVar2 + param_4;
    } while (uVar3 != param_4);
  }
  return;
}



/* ========================================================================
   ENTRY: 180006210
   NAME : fft_bin_to_freq
   SIG  : float __fastcall fft_bin_to_freq(uint param_1, uint param_2, uint param_3)
   ======================================================================== */

float fft_bin_to_freq(uint param_1,uint param_2,uint param_3)

{
                    /* 0x6210  16  fft_bin_to_freq */
  return ((float)param_2 / (float)param_3) * (float)param_1;
}



/* ========================================================================
   ENTRY: 180006230
   NAME : freq_to_fft_bin
   SIG  : longlong __fastcall freq_to_fft_bin(float param_1, uint param_2, uint param_3)
   ======================================================================== */

longlong freq_to_fft_bin(float param_1,uint param_2,uint param_3)

{
                    /* 0x6230  22  freq_to_fft_bin */
  return (longlong)(param_1 / (((float)param_2 / (float)param_3) * DAT_180008038));
}



/* ========================================================================
   ENTRY: 180006260
   NAME : spectral_flux
   SIG  : float __fastcall spectral_flux(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

float spectral_flux(longlong param_1,longlong param_2,uint param_3)

{
  uint uVar1;
  ulonglong uVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  float fVar8;
  
  uVar1 = _DAT_1800083c0;
                    /* 0x6260  103  spectral_flux */
  fVar7 = DAT_180008038;
  fVar6 = 0.0;
  if (param_3 == 0 || (param_2 == 0 || param_1 == 0)) {
    return 0.0;
  }
  if (param_3 == 1) {
    uVar2 = 0;
  }
  else {
    uVar2 = 0;
    do {
      fVar3 = *(float *)(param_1 + uVar2 * 4);
      if (fVar3 < 0.0) {
        fVar3 = sqrtf(fVar3);
        fVar4 = *(float *)(param_2 + uVar2 * 4);
        if (fVar4 < 0.0) goto LAB_18000637a;
LAB_180006391:
        fVar4 = SQRT(fVar4);
      }
      else {
        fVar3 = SQRT(fVar3);
        fVar4 = *(float *)(param_2 + uVar2 * 4);
        if (0.0 <= fVar4) goto LAB_180006391;
LAB_18000637a:
        fVar4 = sqrtf(fVar4);
      }
      fVar8 = *(float *)(param_1 + 4 + uVar2 * 4);
      if (fVar8 < 0.0) {
        fVar8 = sqrtf(fVar8);
        fVar5 = *(float *)(param_2 + 4 + uVar2 * 4);
        if (0.0 <= fVar5) goto LAB_1800063e6;
LAB_180006330:
        fVar5 = sqrtf(fVar5);
      }
      else {
        fVar8 = SQRT(fVar8);
        fVar5 = *(float *)(param_2 + 4 + uVar2 * 4);
        if (fVar5 < 0.0) goto LAB_180006330;
LAB_1800063e6:
        fVar5 = SQRT(fVar5);
      }
      fVar6 = fVar6 + ((float)((uint)(fVar3 - fVar4) & uVar1) + (fVar3 - fVar4)) * fVar7 +
              ((float)((uint)(fVar8 - fVar5) & uVar1) + (fVar8 - fVar5)) * fVar7;
      uVar2 = uVar2 + 2;
    } while ((param_3 & 0xfffffffe) != uVar2);
  }
  if ((param_3 & 1) != 0) {
    fVar7 = *(float *)(param_1 + uVar2 * 4);
    if (fVar7 < 0.0) {
      fVar7 = sqrtf(fVar7);
      fVar3 = *(float *)(param_2 + uVar2 * 4);
    }
    else {
      fVar7 = SQRT(fVar7);
      fVar3 = *(float *)(param_2 + uVar2 * 4);
    }
    if (0.0 <= fVar3) {
      fVar3 = SQRT(fVar3);
    }
    else {
      fVar3 = sqrtf(fVar3);
    }
    return fVar6 + ((float)(_DAT_1800083c0 & (uint)(fVar7 - fVar3)) + (fVar7 - fVar3)) *
                   DAT_180008038;
  }
  return fVar6;
}



/* ========================================================================
   ENTRY: 180006460
   NAME : get_rolling_mean_spectrum
   SIG  : bool __fastcall get_rolling_mean_spectrum(longlong param_1, longlong param_2, uint param_3, uint param_4)
   ======================================================================== */

bool get_rolling_mean_spectrum(longlong param_1,longlong param_2,uint param_3,uint param_4)

{
  undefined8 *puVar1;
  undefined8 *puVar2;
  float *pfVar3;
  float *pfVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  undefined8 uVar8;
  undefined8 uVar9;
  undefined8 uVar10;
  uint uVar11;
  ulonglong uVar12;
  ulonglong uVar13;
  ulonglong uVar14;
  ulonglong uVar15;
  ulonglong uVar16;
  float fVar17;
  undefined1 auVar18 [16];
  undefined1 auVar19 [16];
  
                    /* 0x6460  40  get_rolling_mean_spectrum */
  if (1 < param_4 && (param_2 != 0 && param_1 != 0)) {
    uVar15 = (ulonglong)param_4;
    uVar16 = uVar15 - 1;
    if (param_3 < 2) {
      uVar14 = 1;
      if (0x1f < (ulonglong)(param_1 - param_2) && 8 < param_4) {
        uVar12 = uVar16 & 0xfffffffffffffff8;
        uVar14 = uVar12 + 1;
        uVar13 = 0;
        do {
          puVar1 = (undefined8 *)(param_2 + 4 + uVar13 * 4);
          uVar8 = puVar1[1];
          puVar2 = (undefined8 *)(param_2 + 0x14 + uVar13 * 4);
          uVar9 = *puVar2;
          uVar10 = puVar2[1];
          puVar2 = (undefined8 *)(param_1 + 4 + uVar13 * 4);
          *puVar2 = *puVar1;
          puVar2[1] = uVar8;
          puVar1 = (undefined8 *)(param_1 + 0x14 + uVar13 * 4);
          *puVar1 = uVar9;
          puVar1[1] = uVar10;
          uVar13 = uVar13 + 8;
        } while (uVar12 != uVar13);
        if (uVar16 == uVar12) goto LAB_180006672;
      }
      uVar11 = param_4 - (int)uVar14 & 3;
      uVar12 = (ulonglong)uVar11;
      uVar16 = uVar14;
      if (uVar11 != 0) {
        do {
          *(undefined4 *)(param_1 + uVar16 * 4) = *(undefined4 *)(param_2 + uVar16 * 4);
          uVar16 = uVar16 + 1;
          uVar12 = uVar12 - 1;
        } while (uVar12 != 0);
      }
      if (uVar14 - uVar15 < 0xfffffffffffffffd) {
        do {
          *(undefined4 *)(param_1 + uVar16 * 4) = *(undefined4 *)(param_2 + uVar16 * 4);
          *(undefined4 *)(param_1 + 4 + uVar16 * 4) = *(undefined4 *)(param_2 + 4 + uVar16 * 4);
          *(undefined4 *)(param_1 + 8 + uVar16 * 4) = *(undefined4 *)(param_2 + 8 + uVar16 * 4);
          *(undefined4 *)(param_1 + 0xc + uVar16 * 4) = *(undefined4 *)(param_2 + 0xc + uVar16 * 4);
          uVar16 = uVar16 + 4;
        } while (uVar15 != uVar16);
      }
    }
    else {
      fVar17 = (float)param_3;
      uVar14 = 1;
      if ((4 < param_4) &&
         (param_2 + uVar15 * 4 <= param_1 + 4U || param_1 + uVar15 * 4 <= param_2 + 4U)) {
        uVar12 = uVar16 & 0xfffffffffffffffc;
        uVar14 = uVar12 + 1;
        uVar13 = 0;
        do {
          pfVar3 = (float *)(param_2 + 4 + uVar13 * 4);
          pfVar4 = (float *)(param_1 + 4 + uVar13 * 4);
          fVar5 = pfVar4[1];
          fVar6 = pfVar4[2];
          fVar7 = pfVar4[3];
          auVar18._0_4_ = *pfVar3 - *pfVar4;
          auVar18._4_4_ = pfVar3[1] - fVar5;
          auVar18._8_4_ = pfVar3[2] - fVar6;
          auVar18._12_4_ = pfVar3[3] - fVar7;
          auVar19._4_4_ = fVar17;
          auVar19._0_4_ = fVar17;
          auVar19._8_4_ = fVar17;
          auVar19._12_4_ = fVar17;
          auVar19 = divps(auVar18,auVar19);
          pfVar3 = (float *)(param_1 + 4 + uVar13 * 4);
          *pfVar3 = auVar19._0_4_ + *pfVar4;
          pfVar3[1] = auVar19._4_4_ + fVar5;
          pfVar3[2] = auVar19._8_4_ + fVar6;
          pfVar3[3] = auVar19._12_4_ + fVar7;
          uVar13 = uVar13 + 4;
        } while (uVar12 != uVar13);
        if (uVar16 == uVar12) goto LAB_180006672;
      }
      uVar12 = uVar14;
      if ((param_4 & 1) == 0) {
        fVar5 = *(float *)(param_1 + uVar14 * 4);
        *(float *)(param_1 + uVar14 * 4) =
             (*(float *)(param_2 + uVar14 * 4) - fVar5) / fVar17 + fVar5;
        uVar12 = uVar14 + 1;
      }
      if (uVar14 != uVar16) {
        do {
          fVar5 = *(float *)(param_1 + uVar12 * 4);
          fVar6 = *(float *)(param_1 + 4 + uVar12 * 4);
          *(float *)(param_1 + uVar12 * 4) =
               (*(float *)(param_2 + uVar12 * 4) - fVar5) / fVar17 + fVar5;
          *(float *)(param_1 + 4 + uVar12 * 4) =
               (*(float *)(param_2 + 4 + uVar12 * 4) - fVar6) / fVar17 + fVar6;
          uVar12 = uVar12 + 2;
        } while (uVar15 != uVar12);
      }
    }
  }
LAB_180006672:
  return (param_2 != 0 && param_1 != 0) && param_4 != 0;
}



/* ========================================================================
   ENTRY: 180006680
   NAME : get_rolling_median_spectrum
   SIG  : ulonglong __fastcall get_rolling_median_spectrum(longlong param_1, longlong param_2, ulonglong param_3, uint param_4)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

ulonglong get_rolling_median_spectrum
                    (longlong param_1,longlong param_2,ulonglong param_3,uint param_4)

{
  float fVar1;
  longlong lVar2;
  size_t sVar3;
  bool bVar4;
  uint uVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  float *pfVar8;
  ulonglong uVar9;
  uint uVar10;
  float fVar11;
  undefined8 auStack_e0 [4];
  undefined8 uStack_c0;
  float afStack_b8 [2];
  undefined8 local_b0;
  undefined1 *local_a8;
  ulonglong local_a0;
  ulonglong local_98;
  ulonglong local_90;
  ulonglong local_88;
  ulonglong local_80;
  size_t local_78;
  longlong local_70;
  byte local_61;
  ulonglong local_60;
  undefined1 local_48 [8];
  
                    /* 0x6680  41  get_rolling_median_spectrum */
  fVar1 = DAT_180008038;
  local_60 = DAT_18000b000 ^ (ulonglong)local_48;
  bVar4 = param_4 != 0 && (param_2 != 0 && param_1 != 0);
  uVar9 = CONCAT71((int7)(local_60 >> 8),bVar4);
  pfVar8 = afStack_b8;
  local_70 = param_1;
  if (param_4 != 0 && (param_2 != 0 && param_1 != 0)) {
    uVar10 = (uint)param_3;
    local_78 = param_3 & 0xffffffff;
    uStack_c0 = 0x1800066ee;
    lVar2 = -(local_78 * 4 + 0xf & 0xfffffffffffffff0);
    local_a8 = (undefined1 *)afStack_b8;
    local_61 = bVar4;
    if (1 < param_4) {
      local_98 = param_3 >> 1 & 0x7fffffff;
      local_80 = (ulonglong)(uVar10 - 1 >> 1);
      local_90 = (ulonglong)param_4;
      if (uVar10 == 0) {
        uVar9 = 1;
        local_a8 = (undefined1 *)afStack_b8;
        do {
          sVar3 = local_78;
          *(undefined8 *)((longlong)auStack_e0 + lVar2) = 0x180006763;
          qsort((void *)((longlong)afStack_b8 + lVar2),sVar3,4,(_PtFuncCompare *)&LAB_180006a70);
          fVar11 = (*(float *)((longlong)afStack_b8 + local_80 * 4 + lVar2) +
                   *(float *)((longlong)afStack_b8 + local_98 * 4 + lVar2)) * fVar1;
          pfVar8 = (float *)(local_70 + uVar9 * 4);
          if (*pfVar8 <= fVar11 && fVar11 != *pfVar8) {
            *(float *)(local_70 + uVar9 * 4) = fVar11;
          }
          uVar9 = uVar9 + 1;
        } while (local_90 != uVar9);
      }
      else {
        local_a0 = param_3;
        if ((param_3 & 1) == 0) {
          local_b0 = (ulonglong)(uVar10 & 0xfffffffc);
          uVar9 = 1;
          local_a8 = (undefined1 *)afStack_b8;
          do {
            uVar7 = local_b0;
            if ((uint)local_a0 < 4) {
              uVar6 = 0;
            }
            else {
              uVar6 = 0;
              uVar5 = (uint)uVar9;
              do {
                *(undefined4 *)((longlong)afStack_b8 + uVar6 * 4 + lVar2) =
                     *(undefined4 *)(param_2 + (ulonglong)uVar5 * 4);
                *(undefined4 *)((longlong)afStack_b8 + uVar6 * 4 + lVar2 + 4) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 + uVar5) * 4);
                *(undefined4 *)((longlong)&local_b0 + uVar6 * 4 + lVar2) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 * 2 + uVar5) * 4);
                *(undefined4 *)((longlong)&local_b0 + uVar6 * 4 + lVar2 + 4) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 * 3 + uVar5) * 4);
                uVar6 = uVar6 + 4;
                uVar5 = uVar5 + param_4 * 4;
              } while (uVar7 != uVar6);
            }
            if ((ulonglong)(uVar10 & 2) != 0) {
              uVar5 = (int)uVar6 * param_4 + (uint)uVar9;
              uVar7 = 0;
              do {
                *(undefined4 *)((longlong)afStack_b8 + uVar7 * 4 + uVar6 * 4 + lVar2) =
                     *(undefined4 *)(param_2 + (ulonglong)uVar5 * 4);
                uVar7 = uVar7 + 1;
                uVar5 = uVar5 + param_4;
              } while ((uVar10 & 2) != uVar7);
            }
            sVar3 = local_78;
            local_88 = uVar9;
            *(undefined8 *)((longlong)auStack_e0 + lVar2) = 0x1800068bc;
            qsort((void *)((longlong)afStack_b8 + lVar2),sVar3,4,(_PtFuncCompare *)&LAB_180006a70);
            fVar11 = (*(float *)((longlong)afStack_b8 + local_80 * 4 + lVar2) +
                     *(float *)((longlong)afStack_b8 + local_98 * 4 + lVar2)) * fVar1;
            pfVar8 = (float *)(local_70 + local_88 * 4);
            if (*pfVar8 <= fVar11 && fVar11 != *pfVar8) {
              *(float *)(local_70 + local_88 * 4) = fVar11;
            }
            uVar9 = local_88 + 1;
          } while (uVar9 != local_90);
        }
        else {
          local_88 = (ulonglong)(uVar10 & 0xfffffffc);
          uVar9 = 1;
          local_a8 = (undefined1 *)afStack_b8;
          do {
            uVar7 = local_88;
            if ((uint)local_a0 < 4) {
              uVar6 = 0;
            }
            else {
              uVar6 = 0;
              uVar5 = (uint)uVar9;
              do {
                *(undefined4 *)((longlong)afStack_b8 + uVar6 * 4 + lVar2) =
                     *(undefined4 *)(param_2 + (ulonglong)uVar5 * 4);
                *(undefined4 *)((longlong)afStack_b8 + uVar6 * 4 + lVar2 + 4) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 + uVar5) * 4);
                *(undefined4 *)((longlong)&local_b0 + uVar6 * 4 + lVar2) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 * 2 + uVar5) * 4);
                *(undefined4 *)((longlong)&local_b0 + uVar6 * 4 + lVar2 + 4) =
                     *(undefined4 *)(param_2 + (ulonglong)(param_4 * 3 + uVar5) * 4);
                uVar6 = uVar6 + 4;
                uVar5 = uVar5 + param_4 * 4;
              } while (uVar7 != uVar6);
            }
            uVar5 = (int)uVar6 * param_4 + (uint)uVar9;
            uVar7 = 0;
            local_80 = uVar9;
            do {
              *(undefined4 *)((longlong)afStack_b8 + uVar7 * 4 + uVar6 * 4 + lVar2) =
                   *(undefined4 *)(param_2 + (ulonglong)uVar5 * 4);
              sVar3 = local_78;
              uVar7 = uVar7 + 1;
              uVar5 = uVar5 + param_4;
            } while ((uVar10 & 3) != uVar7);
            *(undefined8 *)((longlong)auStack_e0 + lVar2) = 0x1800069f8;
            qsort((void *)((longlong)afStack_b8 + lVar2),sVar3,4,(_PtFuncCompare *)&LAB_180006a70);
            fVar1 = *(float *)((longlong)afStack_b8 + local_98 * 4 + lVar2);
            pfVar8 = (float *)(local_70 + local_80 * 4);
            if (*pfVar8 <= fVar1 && fVar1 != *pfVar8) {
              *(float *)(local_70 + local_80 * 4) = fVar1;
            }
            uVar9 = local_80 + 1;
          } while (uVar9 != local_90);
        }
      }
    }
    uVar9 = (ulonglong)local_61;
    pfVar8 = (float *)local_a8;
  }
  uVar7 = local_60 ^ (ulonglong)local_48;
  if (uVar7 == DAT_18000b000) {
    return uVar9;
  }
                    /* WARNING: Subroutine does not return */
  *(undefined **)((longlong)pfVar8 + -0x28) = &UNK_180006a5e;
  FUN_180006af0(uVar7);
}



/* ========================================================================
   ENTRY: 180006a90
   NAME : __chkstk
   SIG  : undefined __fastcall __chkstk(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* Library Function - Single Match
    __chkstk
   
   Libraries: Visual Studio 2005, Visual Studio 2008, Visual Studio 2010, Visual Studio 2012 */

void __chkstk(void)

{
  undefined1 *in_RAX;
  undefined1 *puVar1;
  undefined1 *puVar2;
  undefined1 local_res8 [32];
  
  puVar1 = local_res8 + -(longlong)in_RAX;
  if (local_res8 < in_RAX) {
    puVar1 = (undefined1 *)0x0;
  }
  if (puVar1 < StackLimit) {
    puVar2 = StackLimit;
    do {
      puVar2 = puVar2 + -0x1000;
      *puVar2 = 0;
    } while ((undefined1 *)((ulonglong)puVar1 & 0xfffffffffffff000) != puVar2);
  }
  return;
}



/* ========================================================================
   ENTRY: 180006af0
   NAME : FUN_180006af0
   SIG  : noreturn undefined __fastcall FUN_180006af0(longlong param_1)
   ======================================================================== */

void FUN_180006af0(longlong param_1)

{
  if ((param_1 == DAT_18000b000) && ((short)((ulonglong)param_1 >> 0x30) == 0)) {
    return;
  }
  FUN_180006b10();
  return;
}



/* ========================================================================
   ENTRY: 180006b10
   NAME : FUN_180006b10
   SIG  : undefined __fastcall FUN_180006b10(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180006b10(void)

{
  code *pcVar1;
  BOOL BVar2;
  undefined1 *puVar3;
  undefined1 auStack_38 [8];
  undefined1 auStack_30 [48];
  
  puVar3 = auStack_38;
  BVar2 = IsProcessorFeaturePresent(0x17);
  if (BVar2 != 0) {
    pcVar1 = (code *)swi(0x29);
    (*pcVar1)(2);
    puVar3 = auStack_30;
  }
  *(undefined8 *)(puVar3 + -8) = 0x180006b3b;
  FUN_180006be4((PCONTEXT)&DAT_18000b120);
  _DAT_18000b090 = *(undefined8 *)(puVar3 + 0x38);
  _DAT_18000b1b8 = puVar3 + 0x40;
  _DAT_18000b1a0 = *(undefined8 *)(puVar3 + 0x40);
  _DAT_18000b080 = 0xc0000409;
  _DAT_18000b084 = 1;
  _DAT_18000b098 = 1;
  DAT_18000b0a0 = 2;
  *(undefined8 *)(puVar3 + 0x20) = DAT_18000b000;
  *(undefined8 *)(puVar3 + 0x28) = DAT_18000b040;
  *(undefined8 *)(puVar3 + -8) = 0x180006bdd;
  DAT_18000b218 = _DAT_18000b090;
  __raise_securityfailure((_EXCEPTION_POINTERS *)&PTR_DAT_1800083e0);
  return;
}



/* ========================================================================
   ENTRY: 180006be4
   NAME : FUN_180006be4
   SIG  : undefined __fastcall FUN_180006be4(PCONTEXT param_1)
   ======================================================================== */

void FUN_180006be4(PCONTEXT param_1)

{
  DWORD64 ControlPc;
  PRUNTIME_FUNCTION FunctionEntry;
  int iVar1;
  DWORD64 local_res8;
  ulonglong local_res10;
  PVOID local_res18 [2];
  
  RtlCaptureContext();
  ControlPc = param_1->Rip;
  iVar1 = 0;
  do {
    FunctionEntry = RtlLookupFunctionEntry(ControlPc,&local_res8,(PUNWIND_HISTORY_TABLE)0x0);
    if (FunctionEntry == (PRUNTIME_FUNCTION)0x0) {
      return;
    }
    RtlVirtualUnwind(0,local_res8,ControlPc,FunctionEntry,param_1,local_res18,&local_res10,
                     (PKNONVOLATILE_CONTEXT_POINTERS)0x0);
    iVar1 = iVar1 + 1;
  } while (iVar1 < 2);
  return;
}



/* ========================================================================
   ENTRY: 180006c58
   NAME : __raise_securityfailure
   SIG  : undefined __fastcall __raise_securityfailure(_EXCEPTION_POINTERS * param_1)
   ======================================================================== */

/* Library Function - Single Match
    __raise_securityfailure
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

void __raise_securityfailure(_EXCEPTION_POINTERS *param_1)

{
  HANDLE pvVar1;
  
  SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)0x0);
  UnhandledExceptionFilter(param_1);
  pvVar1 = GetCurrentProcess();
                    /* WARNING: Could not recover jumptable at 0x000180006c85. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  TerminateProcess(pvVar1,0xc0000409);
  return;
}



/* ========================================================================
   ENTRY: 180006c8c
   NAME : FUN_180006c8c
   SIG  : undefined8 __fastcall FUN_180006c8c(undefined8 param_1, undefined8 param_2)
   ======================================================================== */

undefined8 FUN_180006c8c(undefined8 param_1,undefined8 param_2)

{
  bool bVar1;
  bool bVar2;
  int iVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  longlong *plVar6;
  ulonglong uVar7;
  
  uVar4 = FUN_1800071f4(0);
  if ((char)uVar4 != '\0') {
    uVar4 = __scrt_acquire_startup_lock();
    bVar1 = true;
    if (DAT_18000b620 != 0) {
                    /* WARNING: Subroutine does not return */
      FUN_18000742c(7);
    }
    DAT_18000b620 = 1;
    bVar2 = FUN_180007348();
    if (bVar2) {
      FUN_180007578();
      FUN_1800070b4();
      FUN_1800070d0();
      iVar3 = _initterm_e(&DAT_180008598,&DAT_1800085a0);
      if ((iVar3 == 0) && (uVar5 = __scrt_dllmain_after_initialize_c(), (char)uVar5 != '\0')) {
        _initterm(&DAT_180008588,&DAT_180008590);
        DAT_18000b620 = 2;
        bVar1 = false;
      }
    }
    __scrt_release_startup_lock((char)uVar4);
    if (!bVar1) {
      plVar6 = (longlong *)FUN_180007418();
      if ((*plVar6 != 0) && (uVar7 = FUN_1800070fc((longlong)plVar6), (char)uVar7 != '\0')) {
        (*(code *)PTR__guard_dispatch_icall_180008560)(param_1,2,param_2);
      }
      DAT_18000b5f0 = DAT_18000b5f0 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180006da4
   NAME : FUN_180006da4
   SIG  : undefined1 __fastcall FUN_180006da4(undefined8 param_1)
   ======================================================================== */

undefined1 FUN_180006da4(undefined8 param_1)

{
  undefined1 uVar1;
  undefined8 uVar2;
  undefined7 uVar3;
  
  uVar1 = (undefined1)param_1;
  if (DAT_18000b5f0 < 1) {
    uVar1 = 0;
  }
  else {
    DAT_18000b5f0 = DAT_18000b5f0 + -1;
    uVar2 = __scrt_acquire_startup_lock();
    if (DAT_18000b620 != 2) {
                    /* WARNING: Subroutine does not return */
      FUN_18000742c(7);
    }
    __scrt_dllmain_uninitialize_c();
    FUN_1800070c4();
    FUN_1800075b4();
    DAT_18000b620 = 0;
    uVar3 = (undefined7)((ulonglong)param_1 >> 8);
    __scrt_release_startup_lock((char)uVar2);
    uVar1 = __scrt_uninitialize_crt(CONCAT71(uVar3,uVar1),'\0');
    FUN_1800073c4();
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006e28
   NAME : FUN_180006e28
   SIG  : ulonglong __fastcall FUN_180006e28(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_180006e28(undefined8 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  undefined1 uVar2;
  ulonglong uVar3;
  undefined7 extraout_var;
  
  if (param_2 == 0) {
    uVar2 = FUN_180006da4(CONCAT71((int7)((ulonglong)param_1 >> 8),param_3 != 0));
    return CONCAT71(extraout_var,uVar2);
  }
  if (param_2 != 1) {
    if (param_2 == 2) {
      bVar1 = FUN_1800073d8();
    }
    else {
      if (param_2 != 3) {
        return 1;
      }
      bVar1 = FUN_180007400();
    }
    return (ulonglong)bVar1;
  }
  uVar3 = FUN_180006c8c(param_1,param_3);
  return uVar3;
}



/* ========================================================================
   ENTRY: 180006e78
   NAME : FUN_180006e78
   SIG  : int __fastcall FUN_180006e78(HMODULE param_1, int param_2, longlong param_3)
   ======================================================================== */

int FUN_180006e78(HMODULE param_1,int param_2,longlong param_3)

{
  ulonglong uVar1;
  undefined8 uVar2;
  HMODULE pHVar3;
  int iVar4;
  
  if ((param_2 == 0) && (DAT_18000b5f0 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (DAT_1800083f0 == 0) {
      iVar4 = 1;
    }
    else {
      iVar4 = (*(code *)PTR__guard_dispatch_icall_180008560)();
    }
    if (iVar4 == 0) {
      return 0;
    }
    uVar1 = FUN_180006e28(param_1,param_2,param_3);
    if ((int)uVar1 == 0) {
      return 0;
    }
  }
  uVar2 = FUN_180007090(param_1,param_2);
  iVar4 = (int)uVar2;
  if ((param_2 == 1) && (iVar4 == 0)) {
    pHVar3 = param_1;
    FUN_180007090(param_1,0);
    FUN_180006da4(CONCAT71((int7)((ulonglong)pHVar3 >> 8),param_3 != 0));
    if (DAT_1800083f0 != 0) {
      (*(code *)PTR__guard_dispatch_icall_180008560)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    uVar1 = FUN_180006e28(param_1,param_2,param_3);
    iVar4 = 0;
    if ((int)uVar1 != 0) {
      if (DAT_1800083f0 == 0) {
        iVar4 = 1;
      }
      else {
        iVar4 = (*(code *)PTR__guard_dispatch_icall_180008560)(param_1,param_2,param_3);
      }
    }
  }
  return iVar4;
}



/* ========================================================================
   ENTRY: 180006fa0
   NAME : entry
   SIG  : undefined __fastcall entry(HMODULE param_1, int param_2, longlong param_3)
   ======================================================================== */

void entry(HMODULE param_1,int param_2,longlong param_3)

{
  if (param_2 == 1) {
    FUN_180006fe0();
  }
  FUN_180006e78(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180006fe0
   NAME : FUN_180006fe0
   SIG  : undefined __fastcall FUN_180006fe0(void)
   ======================================================================== */

void FUN_180006fe0(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10;
  _FILETIME local_18 [2];
  
  if (DAT_18000b000 == 0x2b992ddfa232) {
    local_res8.dwLowDateTime = 0;
    local_res8.dwHighDateTime = 0;
    GetSystemTimeAsFileTime(&local_res8);
    local_18[0] = local_res8;
    DVar1 = GetCurrentThreadId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    DVar1 = GetCurrentProcessId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    QueryPerformanceCounter(&local_res10);
    DAT_18000b000 =
         ((ulonglong)local_res10.s.LowPart << 0x20 ^
          CONCAT44(local_res10.s.HighPart,local_res10.s.LowPart) ^ (ulonglong)local_18[0] ^
         (ulonglong)local_18) & 0xffffffffffff;
    if (DAT_18000b000 == 0x2b992ddfa232) {
      DAT_18000b000 = 0x2b992ddfa233;
    }
  }
  DAT_18000b040 = ~DAT_18000b000;
  return;
}



/* ========================================================================
   ENTRY: 180007090
   NAME : FUN_180007090
   SIG  : undefined8 __fastcall FUN_180007090(HMODULE param_1, int param_2)
   ======================================================================== */

undefined8 FUN_180007090(HMODULE param_1,int param_2)

{
  if ((param_2 == 1) && (DAT_1800083f0 == 0)) {
    DisableThreadLibraryCalls(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 1800070b4
   NAME : FUN_1800070b4
   SIG  : undefined __fastcall FUN_1800070b4(void)
   ======================================================================== */

void FUN_1800070b4(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800070bb. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_18000b600);
  return;
}



/* ========================================================================
   ENTRY: 1800070c4
   NAME : FUN_1800070c4
   SIG  : undefined __fastcall FUN_1800070c4(void)
   ======================================================================== */

void FUN_1800070c4(void)

{
  __std_type_info_destroy_list(&DAT_18000b600);
  return;
}



/* ========================================================================
   ENTRY: 1800070d0
   NAME : FUN_1800070d0
   SIG  : undefined __fastcall FUN_1800070d0(void)
   ======================================================================== */

void FUN_1800070d0(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_1800070ec();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_1800070f4();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 1800070ec
   NAME : FUN_1800070ec
   SIG  : undefined * __fastcall FUN_1800070ec(void)
   ======================================================================== */

undefined * FUN_1800070ec(void)

{
  return &DAT_18000b610;
}



/* ========================================================================
   ENTRY: 1800070f4
   NAME : FUN_1800070f4
   SIG  : undefined * __fastcall FUN_1800070f4(void)
   ======================================================================== */

undefined * FUN_1800070f4(void)

{
  return &DAT_18000b618;
}



/* ========================================================================
   ENTRY: 1800070fc
   NAME : FUN_1800070fc
   SIG  : ulonglong __fastcall FUN_1800070fc(longlong param_1)
   ======================================================================== */

ulonglong FUN_1800070fc(longlong param_1)

{
  ulonglong uVar1;
  uint7 uVar2;
  longlong lVar3;
  longlong lVar4;
  
  uVar1 = 0x5a4d;
  if (IMAGE_DOS_HEADER_180000000.e_magic == (char  [2])0x5a4d) {
    lVar3 = (longlong)(int)IMAGE_DOS_HEADER_180000000.e_lfanew;
    if ((*(int *)(lVar3 + 0x180000000) == 0x4550) &&
       (uVar1 = 0x20b,
       *(short *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar3 + -4) == 0x20b)) {
      lVar4 = (ulonglong)*(ushort *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar3 + -8) +
              0x18 + lVar3 + 0x180000000;
      uVar1 = (ulonglong)*(ushort *)(IMAGE_DOS_HEADER_180000000.e_magic + lVar3 + 6);
      lVar3 = lVar4 + uVar1 * 0x28;
      for (; lVar4 != lVar3; lVar4 = lVar4 + 0x28) {
        if (((ulonglong)*(uint *)(lVar4 + 0xc) <= param_1 - 0x180000000U) &&
           (uVar1 = (ulonglong)(*(int *)(lVar4 + 8) + *(uint *)(lVar4 + 0xc)),
           param_1 - 0x180000000U < uVar1)) goto LAB_180007172;
      }
      lVar4 = 0;
LAB_180007172:
      if (lVar4 == 0) {
        return uVar1 & 0xffffffffffffff00;
      }
      uVar2 = (uint7)(uVar1 >> 8);
      if (*(int *)(lVar4 + 0x24) < 0) {
        return (ulonglong)uVar2 << 8;
      }
      return CONCAT71(uVar2,1);
    }
  }
  return uVar1 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 180007194
   NAME : __scrt_acquire_startup_lock
   SIG  : undefined8 __fastcall __scrt_acquire_startup_lock(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_acquire_startup_lock
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

ulonglong __scrt_acquire_startup_lock(void)

{
  ulonglong uVar1;
  bool bVar2;
  undefined7 extraout_var;
  ulonglong uVar3;
  
  bVar2 = __scrt_is_ucrt_dll_in_use();
  uVar3 = CONCAT71(extraout_var,bVar2);
  if ((int)uVar3 == 0) {
LAB_1800071c2:
    uVar3 = uVar3 & 0xffffffffffffff00;
  }
  else {
    do {
      uVar3 = 0;
      LOCK();
      bVar2 = DAT_18000b628 == 0;
      uVar1 = *(ulonglong *)((longlong)Self + 8);
      if (!bVar2) {
        uVar3 = DAT_18000b628;
        uVar1 = DAT_18000b628;
      }
      DAT_18000b628 = uVar1;
      UNLOCK();
      if (bVar2) goto LAB_1800071c2;
    } while (*(ulonglong *)((longlong)Self + 8) != uVar3);
    uVar3 = CONCAT71((int7)(uVar3 >> 8),1);
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 1800071d0
   NAME : __scrt_release_startup_lock
   SIG  : undefined __fastcall __scrt_release_startup_lock(char param_1)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_release_startup_lock
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_release_startup_lock(char param_1)

{
  bool bVar1;
  undefined3 extraout_var;
  
  bVar1 = __scrt_is_ucrt_dll_in_use();
  if ((CONCAT31(extraout_var,bVar1) != 0) && (param_1 == '\0')) {
    LOCK();
    DAT_18000b628 = 0;
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 1800071f4
   NAME : FUN_1800071f4
   SIG  : undefined8 __fastcall FUN_1800071f4(int param_1)
   ======================================================================== */

longlong FUN_1800071f4(int param_1)

{
  char cVar1;
  uint7 extraout_var;
  uint7 uVar2;
  undefined7 extraout_var_00;
  uint7 extraout_var_01;
  
  if (param_1 == 0) {
    DAT_18000b630 = 1;
  }
  FUN_1800075f4();
  cVar1 = FUN_1800078a0();
  uVar2 = extraout_var;
  if (cVar1 != '\0') {
    cVar1 = FUN_1800078a0();
    if (cVar1 != '\0') {
      return CONCAT71(extraout_var_00,1);
    }
    FUN_1800078a0();
    uVar2 = extraout_var_01;
  }
  return (ulonglong)uVar2 << 8;
}



/* ========================================================================
   ENTRY: 180007230
   NAME : __scrt_uninitialize_crt
   SIG  : undefined1 __fastcall __scrt_uninitialize_crt(undefined8 param_1, char param_2)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_uninitialize_crt
   
   Library: Visual Studio 2019 Release */

undefined1 __scrt_uninitialize_crt(undefined8 param_1,char param_2)

{
  if ((DAT_18000b630 == '\0') || (param_2 == '\0')) {
    FUN_1800078a0();
    FUN_1800078a0();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000725c
   NAME : FUN_18000725c
   SIG  : undefined8 __fastcall FUN_18000725c(uint param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000725c(uint param_1)

{
  bool bVar1;
  ulonglong in_RAX;
  undefined7 extraout_var;
  
  if (DAT_18000b631 == '\0') {
    if (1 < param_1) {
                    /* WARNING: Subroutine does not return */
      FUN_18000742c(5);
    }
    bVar1 = __scrt_is_ucrt_dll_in_use();
    if (((int)CONCAT71(extraout_var,bVar1) == 0) || (param_1 != 0)) {
      in_RAX = 0xffffffffffffffff;
      _DAT_18000b638 = _DAT_180008540;
      uRam000000018000b640 = _UNK_180008548;
      _DAT_18000b648 = 0xffffffffffffffff;
      _DAT_18000b650 = _DAT_180008540;
      uRam000000018000b658 = _UNK_180008548;
      _DAT_18000b660 = 0xffffffffffffffff;
    }
    else {
      in_RAX = _initialize_onexit_table(&DAT_18000b638);
      if (((int)in_RAX != 0) ||
         (in_RAX = _initialize_onexit_table(&DAT_18000b650), (int)in_RAX != 0)) {
        return in_RAX & 0xffffffffffffff00;
      }
    }
    DAT_18000b631 = '\x01';
  }
  return CONCAT71((int7)(in_RAX >> 8),1);
}



/* ========================================================================
   ENTRY: 1800072e8
   NAME : __scrt_dllmain_exception_filter
   SIG  : undefined __fastcall __scrt_dllmain_exception_filter(undefined8 param_1, int param_2, undefined8 param_3, undefined * param_4, undefined4 param_5, undefined8 param_6)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_exception_filter
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_dllmain_exception_filter
               (undefined8 param_1,int param_2,undefined8 param_3,undefined *param_4,
               undefined4 param_5,undefined8 param_6)

{
  bool bVar1;
  undefined7 extraout_var;
  
  bVar1 = __scrt_is_ucrt_dll_in_use();
  if (((int)CONCAT71(extraout_var,bVar1) == 0) && (param_2 == 1)) {
    (*(code *)PTR__guard_dispatch_icall_180008560)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 180007348
   NAME : FUN_180007348
   SIG  : bool __fastcall FUN_180007348(void)
   ======================================================================== */

bool FUN_180007348(void)

{
  undefined8 uVar1;
  
  uVar1 = FUN_18000725c(0);
  return (char)uVar1 != '\0';
}



/* ========================================================================
   ENTRY: 180007360
   NAME : __scrt_dllmain_after_initialize_c
   SIG  : undefined8 __fastcall __scrt_dllmain_after_initialize_c(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_after_initialize_c
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

undefined8 __scrt_dllmain_after_initialize_c(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  ulonglong uVar3;
  
  bVar1 = __scrt_is_ucrt_dll_in_use();
  if ((int)CONCAT71(extraout_var,bVar1) == 0) {
    uVar3 = FUN_18000788c();
    uVar3 = _configure_narrow_argv(uVar3 & 0xffffffff);
    if ((int)uVar3 != 0) {
      return uVar3 & 0xffffffffffffff00;
    }
    uVar2 = _initialize_narrow_environment();
  }
  else {
    uVar2 = FUN_1800075f4();
  }
  return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
}



/* ========================================================================
   ENTRY: 180007394
   NAME : __scrt_dllmain_uninitialize_c
   SIG  : undefined __fastcall __scrt_dllmain_uninitialize_c(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_uninitialize_c
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_dllmain_uninitialize_c(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  
  bVar1 = __scrt_is_ucrt_dll_in_use();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    _execute_onexit_table(&DAT_18000b638);
    return;
  }
  uVar2 = FUN_1800078a4();
  if ((int)uVar2 == 0) {
    _cexit();
  }
  return;
}



/* ========================================================================
   ENTRY: 1800073c4
   NAME : FUN_1800073c4
   SIG  : undefined __fastcall FUN_1800073c4(void)
   ======================================================================== */

void FUN_1800073c4(void)

{
  FUN_1800078a0();
  FUN_1800078a0();
  return;
}



/* ========================================================================
   ENTRY: 1800073d8
   NAME : FUN_1800073d8
   SIG  : undefined1 __fastcall FUN_1800073d8(void)
   ======================================================================== */

undefined1 FUN_1800073d8(void)

{
  char cVar1;
  
  cVar1 = FUN_1800078a0();
  if (cVar1 != '\0') {
    cVar1 = FUN_1800078a0();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_1800078a0();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180007400
   NAME : FUN_180007400
   SIG  : undefined1 __fastcall FUN_180007400(void)
   ======================================================================== */

undefined1 FUN_180007400(void)

{
  FUN_1800078a0();
  FUN_1800078a0();
  return 1;
}



/* ========================================================================
   ENTRY: 180007418
   NAME : FUN_180007418
   SIG  : undefined * __fastcall FUN_180007418(void)
   ======================================================================== */

undefined * FUN_180007418(void)

{
  return &DAT_18000b668;
}



/* ========================================================================
   ENTRY: 180007420
   NAME : FUN_180007420
   SIG  : undefined __fastcall FUN_180007420(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180007420(void)

{
  _DAT_18000b670 = 0;
  return;
}



/* ========================================================================
   ENTRY: 18000742c
   NAME : FUN_18000742c
   SIG  : noreturn undefined __fastcall FUN_18000742c(undefined4 param_1)
   ======================================================================== */

void FUN_18000742c(undefined4 param_1)

{
  code *pcVar1;
  BOOL BVar2;
  LONG LVar3;
  PRUNTIME_FUNCTION FunctionEntry;
  undefined1 *puVar4;
  undefined8 unaff_retaddr;
  DWORD64 local_res10;
  undefined1 local_res18 [8];
  undefined1 local_res20 [8];
  undefined1 auStack_5c8 [8];
  undefined1 auStack_5c0 [232];
  undefined1 local_4d8 [152];
  undefined1 *local_440;
  DWORD64 local_3e0;
  
  puVar4 = auStack_5c8;
  BVar2 = IsProcessorFeaturePresent(0x17);
  if (BVar2 != 0) {
    pcVar1 = (code *)swi(0x29);
    (*pcVar1)(param_1);
    puVar4 = auStack_5c0;
  }
  *(undefined8 *)(puVar4 + -8) = 0x180007460;
  FUN_180007420();
  *(undefined8 *)(puVar4 + -8) = 0x180007471;
  memset(local_4d8,0,0x4d0);
  *(undefined8 *)(puVar4 + -8) = 0x18000747b;
  RtlCaptureContext(local_4d8);
  *(undefined8 *)(puVar4 + -8) = 0x180007495;
  FunctionEntry = RtlLookupFunctionEntry(local_3e0,&local_res10,(PUNWIND_HISTORY_TABLE)0x0);
  if (FunctionEntry != (PRUNTIME_FUNCTION)0x0) {
    *(undefined8 *)(puVar4 + 0x38) = 0;
    *(undefined1 **)(puVar4 + 0x30) = local_res18;
    *(undefined1 **)(puVar4 + 0x28) = local_res20;
    *(undefined1 **)(puVar4 + 0x20) = local_4d8;
    *(undefined8 *)(puVar4 + -8) = 0x1800074d9;
    RtlVirtualUnwind(0,local_res10,local_3e0,FunctionEntry,*(PCONTEXT *)(puVar4 + 0x20),
                     *(PVOID **)(puVar4 + 0x28),*(PDWORD64 *)(puVar4 + 0x30),
                     *(PKNONVOLATILE_CONTEXT_POINTERS *)(puVar4 + 0x38));
  }
  local_440 = &stack0x00000008;
  *(undefined8 *)(puVar4 + -8) = 0x18000750b;
  memset(puVar4 + 0x50,0,0x98);
  *(undefined8 *)(puVar4 + 0x60) = unaff_retaddr;
  *(undefined4 *)(puVar4 + 0x50) = 0x40000015;
  *(undefined4 *)(puVar4 + 0x54) = 1;
  *(undefined8 *)(puVar4 + -8) = 0x18000752d;
  BVar2 = IsDebuggerPresent();
  *(undefined1 **)(puVar4 + 0x40) = puVar4 + 0x50;
  *(undefined1 **)(puVar4 + 0x48) = local_4d8;
  *(undefined8 *)(puVar4 + -8) = 0x18000754a;
  SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)0x0);
  *(undefined8 *)(puVar4 + -8) = 0x180007555;
  LVar3 = UnhandledExceptionFilter((_EXCEPTION_POINTERS *)(puVar4 + 0x40));
  if ((LVar3 == 0) && (BVar2 != 1)) {
    *(undefined8 *)(puVar4 + -8) = 0x180007566;
    FUN_180007420();
  }
  return;
}



/* ========================================================================
   ENTRY: 180007578
   NAME : FUN_180007578
   SIG  : undefined __fastcall FUN_180007578(void)
   ======================================================================== */

void FUN_180007578(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_180009ed0; plVar1 < &DAT_180009ed0; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_180008560)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800075b4
   NAME : FUN_1800075b4
   SIG  : undefined __fastcall FUN_1800075b4(void)
   ======================================================================== */

void FUN_1800075b4(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_180009ee0; plVar1 < &DAT_180009ee0; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_180008560)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800075f0
   NAME : _guard_check_icall
   SIG  : undefined __fastcall _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
  return;
}



/* ========================================================================
   ENTRY: 1800075f4
   NAME : FUN_1800075f4
   SIG  : undefined8 __fastcall FUN_1800075f4(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x0001800076e5) */
/* WARNING: Removing unreachable block (ram,0x0001800076d5) */
/* WARNING: Removing unreachable block (ram,0x0001800076b0) */
/* WARNING: Removing unreachable block (ram,0x00018000762e) */
/* WARNING: Removing unreachable block (ram,0x00018000760c) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_1800075f4(void)

{
  int *piVar1;
  uint *puVar2;
  int *piVar3;
  longlong lVar4;
  uint uVar5;
  byte bVar6;
  ulonglong uVar7;
  uint uVar8;
  ulonglong uVar9;
  uint uVar10;
  uint uVar11;
  ulonglong uVar12;
  ulonglong in_XCR0;
  
  piVar1 = (int *)cpuid_basic_info(0);
  puVar2 = (uint *)cpuid_Version_info(1);
  uVar5 = puVar2[3];
  if ((piVar1[2] == 0x49656e69 && piVar1[3] == 0x6c65746e) && piVar1[1] == 0x756e6547) {
    uVar8 = *puVar2 & 0xfff3ff0;
    _DAT_18000b060 = 0x8000;
    _DAT_18000b068 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_18000b678 = DAT_18000b678 | 1;
    }
  }
  uVar8 = 0;
  uVar10 = 0;
  uVar11 = 0;
  uVar12 = 0;
  if (6 < *piVar1) {
    piVar3 = (int *)cpuid_Extended_Feature_Enumeration_info(7);
    uVar8 = piVar3[1];
    uVar10 = piVar3[2];
    if ((uVar8 >> 9 & 1) != 0) {
      DAT_18000b678 = DAT_18000b678 | 2;
    }
    if (0 < *piVar3) {
      lVar4 = cpuid_Extended_Feature_Enumeration_info(7);
      uVar11 = *(uint *)(lVar4 + 8);
    }
    if (0x23 < *piVar1) {
      lVar4 = cpuid(0x24);
      uVar12 = (ulonglong)*(uint *)(lVar4 + 4);
    }
  }
  _DAT_18000b058 = 1;
  DAT_18000b05c = 2;
  uVar9 = DAT_18000b050 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    _DAT_18000b058 = 2;
    DAT_18000b05c = 6;
    uVar9 = DAT_18000b050 & 0xffffffffffffffee;
  }
  DAT_18000b050 = uVar9;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar9 = xinuse(0);
    uVar9 = in_XCR0 & uVar9 & 0xffffffff;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar6 = (byte)uVar9, (bVar6 & 6) == 6)) {
      _DAT_18000b058 = 3;
      uVar7 = DAT_18000b050;
      uVar5 = DAT_18000b05c | 8;
      if ((uVar8 & 0x20) != 0) {
        _DAT_18000b058 = 5;
        uVar7 = DAT_18000b050 & 0xfffffffffffffffd;
        uVar5 = DAT_18000b05c | 0x28;
        if (((uVar8 & 0xd0030000) == 0xd0030000) && ((bVar6 & 0xe0) == 0xe0)) {
          DAT_18000b05c = DAT_18000b05c | 0x68;
          _DAT_18000b058 = 6;
          uVar7 = DAT_18000b050 & 0xffffffffffffffd9;
          uVar5 = DAT_18000b05c;
        }
      }
      DAT_18000b05c = uVar5;
      DAT_18000b050 = uVar7;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_18000b050 = DAT_18000b050 & 0xfffffffffeffffff;
      }
      if (((uVar11 >> 0x13 & 1) != 0) && ((bVar6 & 0xe0) == 0xe0)) {
        _DAT_18000b674 = (uint)uVar12 & 0x400ff;
        DAT_18000b050 = ~((ulonglong)((uint)(uVar12 >> 0x10) & 6) | 0x1000029) & DAT_18000b050;
        if (1 < (byte)_DAT_18000b674) {
          DAT_18000b050 = DAT_18000b050 & 0xffffffffffffffbf;
        }
      }
    }
    if (((uVar11 >> 0x15 & 1) != 0) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_18000b050 = DAT_18000b050 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000788c
   NAME : FUN_18000788c
   SIG  : undefined8 __fastcall FUN_18000788c(void)
   ======================================================================== */

undefined8 FUN_18000788c(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180007894
   NAME : __scrt_is_ucrt_dll_in_use
   SIG  : bool __fastcall __scrt_is_ucrt_dll_in_use(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_is_ucrt_dll_in_use
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

bool __scrt_is_ucrt_dll_in_use(void)

{
  return DAT_18000b070 != 0;
}



/* ========================================================================
   ENTRY: 1800078a0
   NAME : FUN_1800078a0
   SIG  : undefined1 __fastcall FUN_1800078a0(void)
   ======================================================================== */

undefined1 FUN_1800078a0(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 1800078a4
   NAME : FUN_1800078a4
   SIG  : undefined8 __fastcall FUN_1800078a4(void)
   ======================================================================== */

undefined8 FUN_1800078a4(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 1800078c0
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x0001800078c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 1800078e0
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x0001800078e0 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x0001800078c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 1800078e6
   NAME : FUN_1800078e6
   SIG  : undefined __fastcall FUN_1800078e6(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_1800078e6(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(char *)(param_2 + 0x40));
  return;
}



/* ========================================================================
   ENTRY: 1800078fd
   NAME : FUN_1800078fd
   SIG  : undefined __fastcall FUN_1800078fd(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_1800078fd(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(char *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 180007916
   NAME : FUN_180007916
   SIG  : undefined __fastcall FUN_180007916(void)
   ======================================================================== */

void FUN_180007916(void)

{
  FUN_1800073c4();
  return;
}



/* ========================================================================
   ENTRY: 18000792a
   NAME : FUN_18000792a
   SIG  : undefined __fastcall FUN_18000792a(undefined8 * param_1, longlong param_2)
   ======================================================================== */

void FUN_18000792a(undefined8 *param_1,longlong param_2)

{
  __scrt_dllmain_exception_filter
            (*(undefined8 *)(param_2 + 0x60),*(int *)(param_2 + 0x68),
             *(undefined8 *)(param_2 + 0x70),FUN_180006e28,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180007960
   NAME : FUN_180007960
   SIG  : bool __fastcall FUN_180007960(undefined8 * param_1)
   ======================================================================== */

bool FUN_180007960(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}



/* ========================================================================
   ENTRY: 180007980
   NAME : fftwf_malloc
   SIG  : undefined fftwf_malloc(void)
   ======================================================================== */

void fftwf_malloc(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007980. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fftwf_malloc();
  return;
}



/* ========================================================================
   ENTRY: 180007990
   NAME : fftwf_plan_r2r_1d
   SIG  : undefined fftwf_plan_r2r_1d(void)
   ======================================================================== */

void fftwf_plan_r2r_1d(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007990. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fftwf_plan_r2r_1d();
  return;
}



/* ========================================================================
   ENTRY: 1800079a0
   NAME : fftwf_free
   SIG  : undefined fftwf_free(void)
   ======================================================================== */

void fftwf_free(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800079a0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fftwf_free();
  return;
}



/* ========================================================================
   ENTRY: 1800079b0
   NAME : fftwf_destroy_plan
   SIG  : undefined fftwf_destroy_plan(void)
   ======================================================================== */

void fftwf_destroy_plan(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800079b0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fftwf_destroy_plan();
  return;
}



/* ========================================================================
   ENTRY: 1800079c0
   NAME : fftwf_execute
   SIG  : undefined fftwf_execute(void)
   ======================================================================== */

void fftwf_execute(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800079c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fftwf_execute();
  return;
}



/* ========================================================================
   ENTRY: 1800079e0
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800079e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 1800079f0
   NAME : memcpy
   SIG  : void * __cdecl memcpy(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memcpy(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800079f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memcpy(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180007a00
   NAME : memmove
   SIG  : void * __cdecl memmove(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memmove(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180007a00. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memmove(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180007a10
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180007a10. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180007a20
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 180007a30
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a30. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 180007a40
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a40. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180007a50
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a50. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 180007a60
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a60. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180007a70
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a70. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 180007a80
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 180007a90
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007a90. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 180007aa0
   NAME : ceilf
   SIG  : float __cdecl ceilf(float _X)
   ======================================================================== */

float __cdecl ceilf(float _X)

{
  float fVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180007aa0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fVar1 = ceilf(_X);
  return fVar1;
}



/* ========================================================================
   ENTRY: 180007ab0
   NAME : exp2f
   SIG  : undefined exp2f(void)
   ======================================================================== */

void exp2f(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007ab0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  exp2f();
  return;
}



/* ========================================================================
   ENTRY: 180007ac0
   NAME : roundf
   SIG  : undefined roundf(void)
   ======================================================================== */

void roundf(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180007ac0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  roundf();
  return;
}


