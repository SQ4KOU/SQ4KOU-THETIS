
/* ========================================================================
   ENTRY: 180001000
   NAME : rnn_lpc
   SIG  : undefined rnn_lpc(void)
   ======================================================================== */

void rnn_lpc(float *param_1,float *param_2,uint param_3)

{
  float fVar1;
  float fVar2;
  uint uVar3;
  float fVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  longlong lVar7;
  ulonglong uVar8;
  float *pfVar9;
  ulonglong uVar10;
  float fVar11;
  float fVar12;
  
                    /* 0x1000  24  rnn_lpc */
  fVar12 = *param_2;
  memset(param_1,0,(longlong)(int)param_3 << 2);
  fVar4 = DAT_18000f004;
  uVar3 = DAT_18000f000;
  if ((*param_2 != 0.0) || (NAN(*param_2))) {
    uVar6 = 0;
    do {
      uVar5 = uVar6 + 1;
      uVar8 = (uVar5 >> 1) + (ulonglong)(uVar5 >> 1 == 0);
      if (uVar6 == (~((int)param_3 >> 0x1f) & param_3)) {
        return;
      }
      if (uVar6 == 0) {
        fVar11 = (float)((uint)(param_2[1] + 0.0) ^ uVar3) / fVar12;
        *param_1 = fVar11;
        uVar5 = 1;
      }
      else {
        if (uVar6 < 4) {
          fVar11 = 0.0;
          lVar7 = 0;
        }
        else {
          fVar11 = 0.0;
          lVar7 = 0;
          pfVar9 = param_1 + 3;
          do {
            fVar11 = *pfVar9 * param_2[uVar6 + lVar7 + -3] +
                     pfVar9[-1] * param_2[uVar6 + lVar7 + -2] +
                     pfVar9[-2] * param_2[uVar6 + lVar7 + -1] +
                     pfVar9[-3] * param_2[uVar6 + lVar7] + fVar11;
            pfVar9 = pfVar9 + 4;
            lVar7 = lVar7 + -4;
          } while (-lVar7 != (uVar6 & 0xfffffffffffffffc));
          lVar7 = -lVar7;
        }
        if ((uVar6 & 3) != 0) {
          pfVar9 = param_2 + (uVar6 - lVar7);
          uVar10 = 0;
          do {
            fVar11 = param_1[lVar7 + uVar10] * *pfVar9 + fVar11;
            uVar10 = uVar10 + 1;
            pfVar9 = pfVar9 + -1;
          } while (((uint)uVar6 & 3) != uVar10);
        }
        fVar11 = (float)((uint)(fVar11 + param_2[uVar6 + 1]) ^ uVar3) / fVar12;
        param_1[uVar6] = fVar11;
        if (uVar5 < 4) {
          uVar10 = 0;
        }
        else {
          lVar7 = uVar6 - 1;
          uVar10 = 0;
          do {
            fVar1 = param_1[uVar10];
            fVar2 = param_1[lVar7];
            param_1[uVar10] = fVar11 * fVar2 + fVar1;
            param_1[lVar7] = fVar11 * fVar1 + fVar2;
            fVar1 = param_1[uVar10 + 1];
            fVar2 = param_1[uVar6 + (uVar10 ^ 0x3ffffffffffffffe)];
            param_1[uVar10 + 1] = fVar11 * fVar2 + fVar1;
            param_1[uVar6 + (uVar10 ^ 0x3ffffffffffffffe)] = fVar11 * fVar1 + fVar2;
            uVar10 = uVar10 + 2;
            lVar7 = lVar7 + -2;
          } while ((uVar8 & 0x7ffffffffffffffe) != uVar10);
        }
        if ((uVar8 & 1) != 0) {
          fVar1 = param_1[uVar10];
          fVar2 = param_1[uVar6 + ~uVar10];
          param_1[uVar10] = fVar11 * fVar2 + fVar1;
          param_1[uVar6 + ~uVar10] = fVar11 * fVar1 + fVar2;
        }
      }
      fVar12 = -(fVar11 * fVar11 * fVar12) + fVar12;
      uVar6 = uVar5;
    } while (fVar4 * *param_2 <= fVar12);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001290
   NAME : rnn_autocorr
   SIG  : undefined rnn_autocorr(void)
   ======================================================================== */

undefined8
rnn_autocorr(float *param_1,longlong param_2,longlong param_3,uint param_4,uint param_5,uint param_6
            )

{
  longlong lVar1;
  uint uVar2;
  int iVar3;
  longlong lVar4;
  float *pfVar5;
  uint uVar6;
  int iVar7;
  longlong lVar8;
  ulonglong uVar9;
  uint uVar10;
  float *pfVar11;
  longlong lVar12;
  longlong lVar13;
  float fVar14;
  undefined1 auStack_df8 [32];
  uint local_dd8;
  float local_dc8 [864];
  ulonglong local_48;
  
                    /* 0x1290  3  rnn_autocorr */
  local_48 = DAT_180580000 ^ (ulonglong)auStack_df8;
  iVar7 = param_6 - param_5;
  pfVar5 = param_1;
  if (param_4 != 0) {
    if (0 < (int)param_6) {
      memcpy(local_dc8,param_1,(ulonglong)param_6 << 2);
    }
    pfVar5 = local_dc8;
    if (0 < (int)param_4) {
      if (param_4 == 1) {
        uVar9 = 0;
      }
      else {
        uVar9 = 0;
        uVar10 = param_6;
        do {
          uVar6 = uVar10 - 2;
          fVar14 = *(float *)(param_3 + uVar9 * 4);
          local_dc8[uVar9] = fVar14 * param_1[uVar9];
          local_dc8[(int)(uVar10 - 1)] = fVar14 * param_1[(int)(uVar10 - 1)];
          fVar14 = *(float *)(param_3 + 4 + uVar9 * 4);
          local_dc8[uVar9 + 1] = fVar14 * param_1[uVar9 + 1];
          local_dc8[(int)uVar6] = fVar14 * param_1[(int)uVar6];
          uVar9 = uVar9 + 2;
          uVar10 = uVar6;
        } while ((param_4 & 0x7ffffffe) != uVar9);
      }
      if ((param_4 & 1) != 0) {
        fVar14 = *(float *)(param_3 + uVar9 * 4);
        local_dc8[uVar9] = fVar14 * param_1[uVar9];
        iVar3 = ~(uint)uVar9 + param_6;
        local_dc8[iVar3] = fVar14 * param_1[iVar3];
      }
    }
  }
  local_dd8 = param_5 + 1;
  rnn_pitch_xcorr(pfVar5,pfVar5,param_2);
  if (-1 < (int)param_5) {
    lVar4 = (longlong)iVar7;
    lVar8 = 2;
    uVar9 = 0;
    pfVar11 = pfVar5;
    uVar10 = param_5;
    do {
      fVar14 = 0.0;
      if ((longlong)uVar9 < (int)param_6 - lVar4) {
        lVar13 = (longlong)iVar7;
        uVar6 = uVar10 & 3;
        fVar14 = 0.0;
        uVar2 = param_5 - (int)uVar9 & 3;
        while (uVar2 != 0) {
          fVar14 = pfVar5[lVar13] * pfVar11[lVar13] + fVar14;
          lVar13 = lVar13 + 1;
          uVar6 = uVar6 - 1;
          uVar2 = uVar6;
        }
        if ((int)uVar9 - param_5 < 0xfffffffd) {
          lVar1 = lVar13 + lVar8;
          lVar12 = 0;
          do {
            fVar14 = pfVar5[lVar13 + lVar12 + 3] * pfVar5[lVar1 + lVar12 + 1] +
                     pfVar5[lVar13 + lVar12 + 2] * pfVar5[lVar1 + lVar12] +
                     pfVar5[lVar13 + lVar12 + 1] * pfVar5[lVar1 + lVar12 + -1] +
                     pfVar5[lVar13 + lVar12] * pfVar5[lVar1 + lVar12 + -2] + fVar14;
            lVar12 = lVar12 + 4;
          } while (param_6 - (int)lVar13 != (int)lVar12);
        }
      }
      *(float *)(param_2 + uVar9 * 4) = fVar14 + *(float *)(param_2 + uVar9 * 4);
      uVar9 = uVar9 + 1;
      iVar7 = iVar7 + 1;
      pfVar11 = pfVar11 + -1;
      uVar10 = (uint)(byte)((char)(uVar10 & 3) + 3);
      lVar8 = lVar8 + -1;
    } while (uVar9 != param_5 + 1);
  }
  if ((local_48 ^ (ulonglong)auStack_df8) == DAT_180580000) {
    return 0;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180001510
   NAME : rnnoise_model_from_buffer
   SIG  : undefined rnnoise_model_from_buffer(void)
   ======================================================================== */

void rnnoise_model_from_buffer(undefined8 param_1,undefined4 param_2)

{
  undefined8 *puVar1;
  
                    /* 0x1510  37  rnnoise_model_from_buffer */
  puVar1 = malloc(0x20);
  puVar1[1] = 0;
  *puVar1 = param_1;
  *(undefined4 *)(puVar1 + 2) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180001540
   NAME : rnnoise_model_from_filename
   SIG  : undefined rnnoise_model_from_filename(void)
   ======================================================================== */

void rnnoise_model_from_filename(char *param_1)

{
  FILE *pFVar1;
  longlong lVar2;
  
                    /* 0x1540  39  rnnoise_model_from_filename */
  pFVar1 = fopen(param_1,"rb");
  lVar2 = rnnoise_model_from_file(pFVar1);
  *(FILE **)(lVar2 + 0x18) = pFVar1;
  return;
}



/* ========================================================================
   ENTRY: 180001570
   NAME : rnnoise_model_from_file
   SIG  : undefined rnnoise_model_from_file(void)
   ======================================================================== */

undefined8 * rnnoise_model_from_file(FILE *param_1)

{
  long lVar1;
  undefined8 *_Memory;
  void *_DstBuf;
  size_t sVar2;
  
                    /* 0x1570  38  rnnoise_model_from_file */
  _Memory = malloc(0x20);
  _Memory[3] = 0;
  fseek(param_1,0,2);
  lVar1 = ftell(param_1);
  *(long *)(_Memory + 2) = lVar1;
  fseek(param_1,0,0);
  *_Memory = 0;
  _DstBuf = malloc((longlong)lVar1);
  _Memory[1] = _DstBuf;
  sVar2 = fread(_DstBuf,(longlong)lVar1,1,param_1);
  if (sVar2 != 1) {
    if (_DstBuf != (void *)0x0) {
      free(_DstBuf);
    }
    free(_Memory);
    _Memory = (undefined8 *)0x0;
  }
  return _Memory;
}



/* ========================================================================
   ENTRY: 180001620
   NAME : rnnoise_model_free
   SIG  : undefined rnnoise_model_free(void)
   ======================================================================== */

void rnnoise_model_free(void *param_1)

{
                    /* 0x1620  36  rnnoise_model_free */
  if (*(FILE **)((longlong)param_1 + 0x18) != (FILE *)0x0) {
    fclose(*(FILE **)((longlong)param_1 + 0x18));
  }
  if (*(void **)((longlong)param_1 + 8) != (void *)0x0) {
    free(*(void **)((longlong)param_1 + 8));
  }
                    /* WARNING: Could not recover jumptable at 0x00018000164e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001660
   NAME : rnnoise_get_size
   SIG  : undefined rnnoise_get_size(void)
   ======================================================================== */

undefined8 rnnoise_get_size(void)

{
                    /* 0x1660  34  rnnoise_get_size */
  return 0x7fb0;
}



/* ========================================================================
   ENTRY: 180001670
   NAME : rnnoise_get_frame_size
   SIG  : undefined rnnoise_get_frame_size(void)
   ======================================================================== */

undefined8 rnnoise_get_frame_size(void)

{
                    /* 0x1670  33  rnnoise_get_frame_size */
  return 0x1e0;
}



/* ========================================================================
   ENTRY: 180001680
   NAME : rnnoise_init
   SIG  : undefined rnnoise_init(void)
   ======================================================================== */

undefined8 rnnoise_init(void *param_1,longlong *param_2)

{
  int iVar1;
  undefined8 uVar2;
  longlong lVar3;
  undefined1 auStack_48 [40];
  void *local_20;
  ulonglong local_18;
  
                    /* 0x1680  35  rnnoise_init */
  local_18 = DAT_180580000 ^ (ulonglong)auStack_48;
  memset(param_1,0,0x7fb0);
  if (param_2 == (longlong *)0x0) {
    iVar1 = init_rnnoise(param_1,&PTR_s_conv1_weights_float_180579cb0);
  }
  else {
    lVar3 = param_2[1];
    if (lVar3 == 0) {
      lVar3 = *param_2;
    }
    rnn_parse_weights(&local_20,lVar3,(int)param_2[2]);
    if (local_20 == (void *)0x0) {
      if ((local_18 ^ (ulonglong)auStack_48) == DAT_180580000) {
        return 0xffffffff;
      }
      goto LAB_180001745;
    }
    iVar1 = init_rnnoise(param_1);
    free(local_20);
  }
  uVar2 = 0xffffffff;
  if (iVar1 == 0) {
    *(undefined4 *)((longlong)param_1 + 0x280) = 0;
    uVar2 = 0;
  }
  if ((local_18 ^ (ulonglong)auStack_48) == DAT_180580000) {
    return uVar2;
  }
LAB_180001745:
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180001750
   NAME : rnnoise_create
   SIG  : undefined rnnoise_create(void)
   ======================================================================== */

void * rnnoise_create(longlong *param_1)

{
  int iVar1;
  void *_Memory;
  longlong lVar2;
  undefined1 auStack_48 [40];
  void *local_20;
  ulonglong local_18;
  
                    /* 0x1750  31  rnnoise_create */
  local_18 = DAT_180580000 ^ (ulonglong)auStack_48;
  _Memory = calloc(1,0x7fb0);
  if (param_1 == (longlong *)0x0) {
    iVar1 = init_rnnoise(_Memory,&PTR_s_conv1_weights_float_180579cb0);
joined_r0x0001800017f3:
    if (iVar1 == 0) {
      *(undefined4 *)((longlong)_Memory + 0x280) = 0;
      if ((local_18 ^ (ulonglong)auStack_48) == DAT_180580000) {
        return _Memory;
      }
      goto LAB_180001810;
    }
  }
  else {
    lVar2 = param_1[1];
    if (lVar2 == 0) {
      lVar2 = *param_1;
    }
    rnn_parse_weights(&local_20,lVar2,(int)param_1[2]);
    if (local_20 != (void *)0x0) {
      iVar1 = init_rnnoise(_Memory);
      free(local_20);
      goto joined_r0x0001800017f3;
    }
  }
  free(_Memory);
  if ((local_18 ^ (ulonglong)auStack_48) == DAT_180580000) {
    return (void *)0x0;
  }
LAB_180001810:
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180001820
   NAME : rnnoise_destroy
   SIG  : void __cdecl rnnoise_destroy(void * _Memory)
   ======================================================================== */

void __cdecl rnnoise_destroy(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x000180001820. Too many branches */
                    /* WARNING: Treating indirect jump as call */
                    /* 0x1820  32  rnnoise_destroy */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180001830
   NAME : rnn_frame_analysis
   SIG  : undefined rnn_frame_analysis(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_frame_analysis(longlong param_1,void *param_2,undefined8 *param_3,void *param_4)

{
  longlong lVar1;
  float fVar2;
  undefined8 uVar3;
  int iVar4;
  undefined1 auVar5 [32];
  undefined1 auVar6 [32];
  longlong lVar7;
  int iVar8;
  longlong lVar9;
  uint uVar10;
  ulonglong uVar11;
  undefined1 auVar12 [12];
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  undefined1 auVar15 [16];
  undefined1 auVar16 [64];
  float fVar17;
  undefined1 auVar18 [16];
  undefined1 auVar19 [16];
  float fVar20;
  float fVar21;
  undefined1 unaff_00001384 [12];
  undefined1 auStack_4b98 [16];
  undefined1 auStack_4b88 [16];
  undefined1 auStack_4b78 [16];
  undefined1 auStack_4b68 [8];
  undefined *puStack_4b60;
  undefined1 auStack_4b58 [16];
  undefined1 auStack_4b48 [12];
  float fStack_4b3c;
  float local_4b38 [4];
  undefined1 auStack_4b28 [1904];
  undefined1 local_43b8 [1920];
  undefined1 local_3c38 [7488];
  undefined1 auStack_1ef8 [32];
  undefined1 auStack_1ed8 [32];
  undefined1 auStack_1eb8 [32];
  undefined1 auStack_1e98 [32];
  undefined1 auStack_1e78 [32];
  undefined1 auStack_1e58 [32];
  float local_1e38 [3];
  undefined8 uStack_1e2c;
  undefined8 uStack_1e24;
  undefined4 uStack_1e1c;
  float local_1e18;
  undefined8 uStack_1e14;
  undefined8 uStack_1e0c;
  undefined8 uStack_1e04;
  undefined4 uStack_1dfc;
  float local_1df8;
  undefined8 uStack_1df4;
  undefined8 uStack_1dec;
  undefined8 uStack_1de4;
  undefined4 uStack_1ddc;
  float local_1dd8;
  undefined8 uStack_1dd4;
  undefined8 uStack_1dcc;
  undefined8 uStack_1dc4;
  undefined4 uStack_1dbc;
  float local_1db8;
  undefined4 uStack_1db4;
  ulonglong local_30;
  
                    /* 0x1830  21  rnn_frame_analysis */
  local_30 = DAT_180580000 ^ (ulonglong)auStack_4b58;
  puStack_4b60 = (undefined *)0x18000187b;
  memcpy(local_4b38,(void *)(param_1 + 0x284),0x780);
  puStack_4b60 = (undefined *)0x180001891;
  memcpy(local_43b8,param_4,0x780);
  puStack_4b60 = (undefined *)0x1800018a2;
  memcpy((void *)(param_1 + 0x284),param_4,0x780);
  lVar7 = 1;
  lVar9 = 0xefc;
  do {
    fVar17 = *(float *)(&UNK_18057cedc + lVar7 * 4);
    local_4b38[lVar7 + -1] = fVar17 * local_4b38[lVar7 + -1];
    fVar21 = (float)(&DAT_18057cee0)[lVar7];
    *(float *)((longlong)local_4b38 + lVar9) = fVar17 * *(float *)((longlong)local_4b38 + lVar9);
    local_4b38[lVar7] = fVar21 * local_4b38[lVar7];
    *(float *)((longlong)local_4b38 + lVar9 + -4) =
         fVar21 * *(float *)((longlong)local_4b38 + lVar9 + -4);
    lVar7 = lVar7 + 2;
    lVar9 = lVar9 + -8;
  } while (lVar7 != 0x1e1);
  lVar7 = 0x18;
  do {
    auVar5 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b98 + lVar7 * 4));
    auVar6 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b88 + lVar7 * 4));
    *(undefined1 (*) [32])(auStack_1ed8 + lVar7 * 8) = auVar6;
    *(undefined1 (*) [32])(auStack_1ef8 + lVar7 * 8) = auVar5;
    auVar5 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b78 + lVar7 * 4));
    auVar6 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b68 + lVar7 * 4));
    *(undefined1 (*) [32])(auStack_1e98 + lVar7 * 8) = auVar6;
    *(undefined1 (*) [32])(auStack_1eb8 + lVar7 * 8) = auVar5;
    auVar5 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b58 + lVar7 * 4));
    auVar6 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b48 + lVar7 * 4));
    *(undefined1 (*) [32])(auStack_1e58 + lVar7 * 8) = auVar6;
    *(undefined1 (*) [32])(auStack_1e78 + lVar7 * 8) = auVar5;
    auVar5 = vpmovzxdq_avx2(*(undefined1 (*) [16])(local_4b38 + lVar7));
    auVar6 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4b28 + lVar7 * 4));
    *(undefined1 (*) [32])(&local_1e18 + lVar7 * 2) = auVar6;
    *(undefined1 (*) [32])(local_1e38 + lVar7 * 2) = auVar5;
    lVar7 = lVar7 + 0x20;
  } while (lVar7 != 0x3d8);
  puStack_4b60 = (undefined *)0x1800019c1;
  rnn_fft_c(&DAT_18057ce90,local_1e38,local_3c38);
  puStack_4b60 = (undefined *)0x1800019d2;
  memcpy(param_2,local_3c38,0xf08);
  auVar12 = ZEXT812(0);
  local_1dd8 = auVar12._0_4_;
  local_1e38[0] = 0.0;
  uStack_1dd4 = auVar12._4_8_;
  uStack_1dcc = 0;
  uStack_1dc4 = 0;
  uStack_1dbc = 0;
  uStack_1dec = 0;
  uStack_1de4 = 0;
  uStack_1ddc = 0;
  uStack_1e0c = 0;
  uStack_1e04 = 0;
  uStack_1dfc = 0;
  local_1e38[1] = auVar12._4_4_;
  local_1e38[1] = 0.0;
  local_1e38[2] = 0.0;
  uStack_1e2c = 0;
  uStack_1e24 = 0;
  uStack_1e1c = 0;
  local_1db8 = 0.0;
  uStack_1db4 = 0;
  lVar7 = 0;
  iVar8 = 0;
  local_1e18 = local_1e38[0];
  uStack_1e14 = uStack_1dd4;
  local_1df8 = local_1e38[0];
  uStack_1df4 = uStack_1dd4;
  local_1dd8 = local_1e38[0];
  do {
    iVar4 = (&DAT_18000f014)[lVar7];
    uVar10 = iVar4 - iVar8;
    if (uVar10 != 0 && iVar8 <= iVar4) {
      fVar17 = (float)(int)uVar10;
      auVar16 = ZEXT864(*(ulonglong *)(local_1e38 + lVar7));
      auVar14._8_8_ = 0;
      auVar14._0_8_ = *(ulonglong *)(local_1e38 + lVar7);
      lVar9 = (longlong)iVar8;
      if (uVar10 == 1) {
        uVar11 = 0;
      }
      else {
        lVar1 = lVar9 * 8;
        uVar11 = 0;
        do {
          auVar19._0_4_ = (float)(int)uVar11 / fVar17;
          auVar19._4_12_ = unaff_00001384;
          fVar21 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar1);
          fVar2 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar1 + 4);
          fVar20 = fVar2 * fVar2 + fVar21 * fVar21;
          auVar19 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar19._0_4_)),auVar19,0x10);
          auVar13._0_4_ = (float)((int)uVar11 + 1) / fVar17;
          auVar13._4_12_ = unaff_00001384;
          fVar21 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar1 + 8);
          fVar2 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar1 + 0xc);
          fVar21 = fVar2 * fVar2 + fVar21 * fVar21;
          auVar13 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar13._0_4_)),auVar13,0x10);
          auVar14._0_4_ = auVar13._0_4_ * fVar21 + auVar19._0_4_ * fVar20 + auVar16._0_4_;
          auVar14._4_4_ = auVar13._4_4_ * fVar21 + auVar19._4_4_ * fVar20 + auVar16._4_4_;
          auVar14._8_4_ = auVar13._8_4_ * fVar21 + auVar19._8_4_ * fVar20 + auVar16._8_4_;
          auVar14._12_4_ = auVar13._12_4_ * fVar21 + auVar19._12_4_ * fVar20 + auVar16._12_4_;
          auVar16 = ZEXT1664(auVar14);
          uVar11 = uVar11 + 2;
        } while (uVar11 != (uVar10 & 0x7ffffffe));
      }
      auVar15 = auVar14;
      if ((uVar10 & 1) != 0) {
        auVar18._0_4_ = (float)(int)uVar11 / fVar17;
        auVar18._4_12_ = unaff_00001384;
        fVar17 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar9 * 8);
        fVar21 = *(float *)((longlong)param_2 + uVar11 * 8 + lVar9 * 8 + 4);
        fVar17 = fVar21 * fVar21 + fVar17 * fVar17;
        auVar19 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar18._0_4_)),auVar18,0x10);
        auVar15._0_4_ = auVar19._0_4_ * fVar17 + auVar14._0_4_;
        auVar15._4_4_ = auVar19._4_4_ * fVar17 + auVar14._4_4_;
        auVar15._8_4_ = auVar19._8_4_ * fVar17 + auVar14._8_4_;
        auVar15._12_4_ = auVar19._12_4_ * fVar17 + auVar14._12_4_;
      }
      uVar3 = vmovlps_avx(auVar15);
      *(undefined8 *)(local_1e38 + lVar7) = uVar3;
    }
    lVar7 = lVar7 + 1;
    iVar8 = iVar4;
  } while (lVar7 != 0x21);
  local_1e38[1] = (local_1e38[0] + local_1e38[1] + local_1e38[0] + local_1e38[1]) / DAT_18000f09c;
  local_1db8 = (local_1db8 + 0.0 + local_1db8 + 0.0) / DAT_18000f09c;
  param_3[8] = uStack_1df4;
  param_3[9] = uStack_1dec;
  param_3[10] = uStack_1de4;
  param_3[0xb] = CONCAT44(local_1dd8,uStack_1ddc);
  param_3[4] = uStack_1e14;
  param_3[5] = uStack_1e0c;
  param_3[6] = uStack_1e04;
  param_3[7] = CONCAT44(local_1df8,uStack_1dfc);
  param_3[0xc] = uStack_1dd4;
  param_3[0xd] = uStack_1dcc;
  param_3[0xe] = uStack_1dc4;
  param_3[0xf] = CONCAT44(local_1db8,uStack_1dbc);
  *param_3 = CONCAT44(local_1e38[2],local_1e38[1]);
  param_3[1] = uStack_1e2c;
  param_3[2] = uStack_1e24;
  param_3[3] = CONCAT44(local_1e18,uStack_1e1c);
  if ((local_30 ^ (ulonglong)auStack_4b58) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  puStack_4b60 = &UNK_180001beb;
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180001bf0
   NAME : rnn_compute_frame_features
   SIG  : undefined rnn_compute_frame_features(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
rnn_compute_frame_features
          (longlong param_1,longlong param_2,void *param_3,longlong param_4,undefined8 *param_5,
          undefined8 *param_6,undefined1 (*param_7) [32],void *param_8)

{
  void *_Dst;
  undefined8 *puVar1;
  longlong lVar2;
  undefined8 *puVar3;
  float fVar4;
  undefined8 uVar5;
  int iVar6;
  undefined1 auVar7 [32];
  undefined8 uVar8;
  undefined8 uVar9;
  undefined8 uVar10;
  undefined8 uVar11;
  undefined8 uVar12;
  undefined8 uVar13;
  undefined8 uVar14;
  undefined8 uVar15;
  undefined8 uVar16;
  undefined8 uVar17;
  undefined8 uVar18;
  undefined8 uVar19;
  undefined8 uVar20;
  undefined8 uVar21;
  int iVar22;
  longlong lVar23;
  longlong lVar24;
  uint uVar25;
  ulonglong uVar26;
  undefined4 uVar27;
  double dVar28;
  double dVar29;
  undefined1 auVar30 [12];
  undefined1 auVar31 [16];
  undefined1 auVar32 [16];
  undefined1 auVar33 [32];
  undefined1 extraout_var [56];
  undefined1 auVar34 [16];
  undefined1 auVar35 [16];
  undefined1 auVar36 [16];
  undefined1 auVar37 [16];
  undefined1 auVar38 [64];
  float fVar39;
  float fVar40;
  undefined1 auVar41 [16];
  undefined1 auVar42 [16];
  undefined1 auVar43 [16];
  undefined1 auVar44 [64];
  undefined1 auVar45 [16];
  float fVar46;
  undefined1 auVar47 [16];
  float fVar48;
  double dVar49;
  undefined1 in_ZMM6 [64];
  double dVar50;
  undefined1 in_ZMM7 [64];
  float fVar51;
  undefined1 auVar52 [64];
  undefined1 auStack_5988 [32];
  int *local_5968;
  undefined4 local_5960;
  undefined4 local_5958;
  int local_5944;
  void *local_5940;
  undefined1 local_5938 [1536];
  undefined1 local_5338 [1824];
  undefined1 auStack_4c18 [16];
  undefined1 auStack_4c08 [16];
  undefined1 auStack_4bf8 [16];
  undefined1 auStack_4be8 [16];
  undefined1 auStack_4bd8 [16];
  undefined1 auStack_4bc8 [8];
  float afStack_4bc0 [2];
  float local_4bb8 [2];
  undefined8 uStack_4bb0;
  undefined8 auStack_4ba8 [22];
  undefined1 auStack_4af8 [32];
  undefined1 auStack_4ad8 [3616];
  undefined1 local_3cb8 [7488];
  undefined1 auStack_1f78 [32];
  undefined1 auStack_1f58 [32];
  undefined1 auStack_1f38 [32];
  undefined1 auStack_1f18 [32];
  undefined1 auStack_1ef8 [32];
  undefined1 auStack_1ed8 [32];
  float local_1eb8 [3];
  undefined8 uStack_1eac;
  undefined8 uStack_1ea4;
  undefined4 uStack_1e9c;
  float local_1e98;
  undefined8 uStack_1e94;
  undefined8 uStack_1e8c;
  undefined8 uStack_1e84;
  undefined4 uStack_1e7c;
  float local_1e78;
  undefined8 uStack_1e74;
  undefined8 uStack_1e6c;
  undefined8 uStack_1e64;
  undefined4 uStack_1e5c;
  float local_1e58;
  undefined8 uStack_1e54;
  undefined8 uStack_1e4c;
  undefined8 uStack_1e44;
  undefined4 uStack_1e3c;
  float local_1e38;
  undefined4 uStack_1e34;
  ulonglong local_b0;
  undefined1 local_a8 [16];
  undefined1 local_98 [16];
  undefined8 uStack_48;
  
                    /* 0x1bf0  7  rnn_compute_frame_features */
  uStack_48 = 0x180001c06;
  local_98 = in_ZMM7._0_16_;
  local_a8 = in_ZMM6._0_16_;
  local_b0 = DAT_180580000 ^ (ulonglong)auStack_5988;
  rnn_frame_analysis(param_1,param_2,param_4,param_8);
  _Dst = (void *)(param_1 + 0x1188);
  memmove(_Dst,(void *)(param_1 + 0x1908),0x1380);
  memcpy((void *)(param_1 + 0x2508),param_8,0x780);
  local_5940 = _Dst;
  rnn_pitch_downsample(&local_5940,local_5938,0x6c0,1);
  local_5968 = &local_5944;
  rnn_pitch_search(local_5338,local_5938,0x3c0,0x24c);
  local_5944 = 0x300 - local_5944;
  local_5958 = *(undefined4 *)(param_1 + 0x4788);
  local_5960 = *(undefined4 *)(param_1 + 0x478c);
  local_5968 = &local_5944;
  uVar27 = rnn_remove_doubling(local_5938,0x300,0x3c,0x3c0);
  *(int *)(param_1 + 0x478c) = local_5944;
  *(undefined4 *)(param_1 + 0x4788) = uVar27;
  if (0x6bf - local_5944 < 0x300 - local_5944) {
    iVar22 = 0x302 - local_5944;
    lVar23 = 8;
    do {
      *(undefined4 *)((longlong)afStack_4bc0 + lVar23) =
           *(undefined4 *)((longlong)_Dst + (longlong)(iVar22 + -2) * 4);
      *(undefined4 *)((longlong)local_4bb8 + lVar23 + -4) =
           *(undefined4 *)((longlong)_Dst + (longlong)(iVar22 + -1) * 4);
      *(undefined4 *)((longlong)local_4bb8 + lVar23) =
           *(undefined4 *)((longlong)_Dst + (longlong)iVar22 * 4);
      lVar23 = lVar23 + 0xc;
      iVar22 = iVar22 + 3;
    } while (lVar23 != 0xf08);
  }
  else {
    iVar22 = 800 - local_5944;
    lVar23 = 0;
    do {
      lVar24 = (longlong)(iVar22 + -0x20);
      puVar1 = (undefined8 *)((longlong)_Dst + lVar24 * 4);
      uVar5 = puVar1[1];
      uVar8 = puVar1[2];
      uVar9 = puVar1[3];
      puVar3 = (undefined8 *)(param_1 + 0x11a8 + lVar24 * 4);
      uVar10 = *puVar3;
      uVar11 = puVar3[1];
      uVar12 = puVar3[2];
      uVar13 = puVar3[3];
      puVar3 = (undefined8 *)(param_1 + 0x11c8 + lVar24 * 4);
      uVar14 = *puVar3;
      uVar15 = puVar3[1];
      uVar16 = puVar3[2];
      uVar17 = puVar3[3];
      puVar3 = (undefined8 *)(param_1 + 0x11e8 + lVar24 * 4);
      uVar18 = *puVar3;
      uVar19 = puVar3[1];
      uVar20 = puVar3[2];
      uVar21 = puVar3[3];
      lVar24 = lVar23 * 4;
      *(undefined8 *)(local_4bb8 + lVar23) = *puVar1;
      *(undefined8 *)((longlong)&uStack_4bb0 + lVar24) = uVar5;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24) = uVar8;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 8) = uVar9;
      lVar24 = lVar23 * 4;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x10) = uVar10;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x18) = uVar11;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x20) = uVar12;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x28) = uVar13;
      lVar24 = lVar23 * 4;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x30) = uVar14;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x38) = uVar15;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x40) = uVar16;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x48) = uVar17;
      lVar24 = lVar23 * 4;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x50) = uVar18;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x58) = uVar19;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x60) = uVar20;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x68) = uVar21;
      lVar24 = (longlong)iVar22;
      puVar1 = (undefined8 *)((longlong)_Dst + lVar24 * 4);
      uVar5 = puVar1[1];
      uVar8 = puVar1[2];
      uVar9 = puVar1[3];
      puVar3 = (undefined8 *)(param_1 + 0x11a8 + lVar24 * 4);
      uVar10 = *puVar3;
      uVar11 = puVar3[1];
      uVar12 = puVar3[2];
      uVar13 = puVar3[3];
      auVar33 = *(undefined1 (*) [32])(param_1 + 0x11c8 + lVar24 * 4);
      auVar7 = *(undefined1 (*) [32])(param_1 + 0x11e8 + lVar24 * 4);
      lVar24 = lVar23 * 4;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x70) = *puVar1;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x78) = uVar5;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x80) = uVar8;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x88) = uVar9;
      lVar24 = lVar23 * 4;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x90) = uVar10;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0x98) = uVar11;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0xa0) = uVar12;
      *(undefined8 *)((longlong)auStack_4ba8 + lVar24 + 0xa8) = uVar13;
      *(undefined1 (*) [32])(auStack_4af8 + lVar23 * 4) = auVar33;
      *(undefined1 (*) [32])(auStack_4ad8 + lVar23 * 4) = auVar7;
      lVar23 = lVar23 + 0x40;
      iVar22 = iVar22 + 0x40;
    } while (lVar23 != 0x3c0);
  }
  lVar23 = 1;
  lVar24 = 0xefc;
  do {
    fVar48 = *(float *)(&UNK_18057cedc + lVar23 * 4);
    local_4bb8[lVar23 + -1] = fVar48 * local_4bb8[lVar23 + -1];
    fVar51 = (float)(&DAT_18057cee0)[lVar23];
    *(float *)((longlong)local_4bb8 + lVar24) = fVar48 * *(float *)((longlong)local_4bb8 + lVar24);
    local_4bb8[lVar23] = fVar51 * local_4bb8[lVar23];
    *(float *)((longlong)local_4bb8 + lVar24 + -4) =
         fVar51 * *(float *)((longlong)local_4bb8 + lVar24 + -4);
    lVar23 = lVar23 + 2;
    lVar24 = lVar24 + -8;
  } while (lVar23 != 0x1e1);
  lVar23 = 0x18;
  do {
    auVar33 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4c18 + lVar23 * 4));
    auVar7 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4c08 + lVar23 * 4));
    *(undefined1 (*) [32])(auStack_1f58 + lVar23 * 8) = auVar7;
    *(undefined1 (*) [32])(auStack_1f78 + lVar23 * 8) = auVar33;
    auVar33 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4bf8 + lVar23 * 4));
    auVar7 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4be8 + lVar23 * 4));
    *(undefined1 (*) [32])(auStack_1f18 + lVar23 * 8) = auVar7;
    *(undefined1 (*) [32])(auStack_1f38 + lVar23 * 8) = auVar33;
    auVar33 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4bd8 + lVar23 * 4));
    auVar7 = vpmovzxdq_avx2(*(undefined1 (*) [16])(auStack_4bc8 + lVar23 * 4));
    *(undefined1 (*) [32])(auStack_1ed8 + lVar23 * 8) = auVar7;
    *(undefined1 (*) [32])(auStack_1ef8 + lVar23 * 8) = auVar33;
    auVar33 = vpmovzxdq_avx2(*(undefined1 (*) [16])(local_4bb8 + lVar23));
    auVar7 = vpmovzxdq_avx2(*(undefined1 (*) [16])((longlong)auStack_4ba8 + lVar23 * 4));
    *(undefined1 (*) [32])(&local_1e98 + lVar23 * 2) = auVar7;
    *(undefined1 (*) [32])(local_1eb8 + lVar23 * 2) = auVar33;
    lVar23 = lVar23 + 0x20;
  } while (lVar23 != 0x3d8);
  auVar44 = ZEXT1664(in_ZMM6._0_16_);
  auVar52 = ZEXT1664(in_ZMM7._0_16_);
  rnn_fft_c(&DAT_18057ce90,local_1eb8,local_3cb8);
  memcpy(param_3,local_3cb8,0xf08);
  fVar51 = DAT_18000f09c;
  fVar48 = DAT_18000f098;
  auVar30 = ZEXT812(0);
  local_1e58 = auVar30._0_4_;
  local_1eb8[0] = 0.0;
  uStack_1e54 = auVar30._4_8_;
  uStack_1e4c = 0;
  uStack_1e44 = 0;
  uStack_1e3c = 0;
  uStack_1e6c = 0;
  uStack_1e64 = 0;
  uStack_1e5c = 0;
  uStack_1e8c = 0;
  uStack_1e84 = 0;
  uStack_1e7c = 0;
  local_1eb8[1] = auVar30._4_4_;
  local_1eb8[1] = 0.0;
  local_1eb8[2] = 0.0;
  uStack_1eac = 0;
  uStack_1ea4 = 0;
  uStack_1e9c = 0;
  local_1e38 = 0.0;
  lVar23 = 0;
  iVar22 = 0;
  local_1e98 = local_1eb8[0];
  uStack_1e94 = uStack_1e54;
  local_1e78 = local_1eb8[0];
  uStack_1e74 = uStack_1e54;
  local_1e58 = local_1eb8[0];
  do {
    iVar6 = (&DAT_18000f014)[lVar23];
    uVar25 = iVar6 - iVar22;
    if (uVar25 != 0 && iVar22 <= iVar6) {
      fVar39 = (float)(int)uVar25;
      auVar30 = auVar44._4_12_;
      auVar38 = ZEXT864(*(ulonglong *)(local_1eb8 + lVar23));
      auVar35._8_8_ = 0;
      auVar35._0_8_ = *(ulonglong *)(local_1eb8 + lVar23);
      lVar24 = (longlong)iVar22;
      if (uVar25 == 1) {
        uVar26 = 0;
      }
      else {
        lVar2 = lVar24 * 8;
        uVar26 = 0;
        do {
          auVar45._0_4_ = (float)(int)uVar26 / fVar39;
          auVar45._4_12_ = auVar30;
          fVar40 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar2);
          fVar4 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar2 + 4);
          fVar46 = fVar4 * fVar4 + fVar40 * fVar40;
          auVar45 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar45._0_4_)),auVar45,0x10);
          auVar34._0_4_ = (float)((int)uVar26 + 1) / fVar39;
          auVar34._4_12_ = auVar30;
          fVar40 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar2 + 8);
          fVar4 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar2 + 0xc);
          fVar40 = fVar4 * fVar4 + fVar40 * fVar40;
          auVar34 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar34._0_4_)),auVar34,0x10);
          auVar35._0_4_ = auVar34._0_4_ * fVar40 + auVar45._0_4_ * fVar46 + auVar38._0_4_;
          auVar35._4_4_ = auVar34._4_4_ * fVar40 + auVar45._4_4_ * fVar46 + auVar38._4_4_;
          auVar35._8_4_ = auVar34._8_4_ * fVar40 + auVar45._8_4_ * fVar46 + auVar38._8_4_;
          auVar35._12_4_ = auVar34._12_4_ * fVar40 + auVar45._12_4_ * fVar46 + auVar38._12_4_;
          auVar38 = ZEXT1664(auVar35);
          uVar26 = uVar26 + 2;
        } while (uVar26 != (uVar25 & 0x7ffffffe));
      }
      auVar36 = auVar35;
      if ((uVar25 & 1) != 0) {
        auVar41._0_4_ = (float)(int)uVar26 / fVar39;
        auVar41._4_12_ = auVar30;
        fVar39 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar24 * 8);
        fVar40 = *(float *)((longlong)param_3 + uVar26 * 8 + lVar24 * 8 + 4);
        fVar39 = fVar40 * fVar40 + fVar39 * fVar39;
        auVar45 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar41._0_4_)),auVar41,0x10);
        auVar36._0_4_ = auVar45._0_4_ * fVar39 + auVar35._0_4_;
        auVar36._4_4_ = auVar45._4_4_ * fVar39 + auVar35._4_4_;
        auVar36._8_4_ = auVar45._8_4_ * fVar39 + auVar35._8_4_;
        auVar36._12_4_ = auVar45._12_4_ * fVar39 + auVar35._12_4_;
      }
      uVar5 = vmovlps_avx(auVar36);
      *(undefined8 *)(local_1eb8 + lVar23) = uVar5;
    }
    lVar23 = lVar23 + 1;
    iVar22 = iVar6;
  } while (lVar23 != 0x21);
  fVar39 = (local_1eb8[0] + local_1eb8[1] + local_1eb8[0] + local_1eb8[1]) / DAT_18000f09c;
  fVar40 = (local_1e38 + 0.0 + local_1e38 + 0.0) / DAT_18000f09c;
  param_5[8] = uStack_1e74;
  param_5[9] = uStack_1e6c;
  param_5[10] = uStack_1e64;
  param_5[0xb] = CONCAT44(local_1e58,uStack_1e5c);
  param_5[4] = uStack_1e94;
  param_5[5] = uStack_1e8c;
  param_5[6] = uStack_1e84;
  param_5[7] = CONCAT44(local_1e78,uStack_1e7c);
  param_5[0xc] = uStack_1e54;
  param_5[0xd] = uStack_1e4c;
  param_5[0xe] = uStack_1e44;
  param_5[0xf] = CONCAT44(fVar40,uStack_1e3c);
  *param_5 = CONCAT44(local_1eb8[2],fVar39);
  param_5[1] = uStack_1eac;
  param_5[2] = uStack_1ea4;
  param_5[3] = CONCAT44(local_1e98,uStack_1e9c);
  local_1e58 = 0.0;
  uStack_1e54 = 0;
  uStack_1e4c = 0;
  uStack_1e44 = 0;
  uStack_1e3c = 0;
  local_1e98 = 0.0;
  uStack_1e94 = 0;
  uStack_1e8c = 0;
  uStack_1e84 = 0;
  uStack_1e7c = 0;
  local_1eb8[1] = SUB324(ZEXT832(0),0);
  local_1eb8[1] = 0.0;
  local_1eb8[2] = SUB324(ZEXT832(0),4);
  local_1eb8[2] = 0.0;
  local_1e38 = 0.0;
  uStack_1e34 = 0;
  lVar23 = 0;
  iVar22 = 0;
  local_1eb8[0] = local_1e98;
  uStack_1eac = uStack_1e8c;
  uStack_1ea4 = uStack_1e84;
  uStack_1e9c = uStack_1e7c;
  local_1e78 = local_1e58;
  uStack_1e74 = uStack_1e54;
  uStack_1e6c = uStack_1e4c;
  uStack_1e64 = uStack_1e44;
  uStack_1e5c = uStack_1e3c;
  do {
    iVar6 = (&DAT_18000f014)[lVar23];
    uVar25 = iVar6 - iVar22;
    if (uVar25 != 0 && iVar22 <= iVar6) {
      auVar44 = ZEXT864(*(ulonglong *)(local_1eb8 + lVar23));
      lVar24 = (longlong)iVar22 * 8;
      lVar2 = param_2 + 4 + (longlong)iVar22 * 8;
      uVar26 = 0;
      do {
        auVar47._0_4_ = (float)(int)uVar26 / (float)(int)uVar25;
        auVar47._4_12_ = auVar52._4_12_;
        fVar39 = *(float *)(lVar2 + uVar26 * 8) *
                 *(float *)((longlong)param_3 + uVar26 * 8 + lVar24 + 4) +
                 *(float *)(lVar2 + -4 + uVar26 * 8) *
                 *(float *)((longlong)param_3 + uVar26 * 8 + lVar24);
        auVar35 = vinsertps_avx(ZEXT416((uint)(fVar48 - auVar47._0_4_)),auVar47,0x10);
        auVar42._0_4_ = auVar35._0_4_ * fVar39 + auVar44._0_4_;
        auVar42._4_4_ = auVar35._4_4_ * fVar39 + auVar44._4_4_;
        auVar42._8_4_ = auVar35._8_4_ * fVar39 + auVar44._8_4_;
        auVar42._12_4_ = auVar35._12_4_ * fVar39 + auVar44._12_4_;
        auVar44 = ZEXT1664(auVar42);
        uVar26 = uVar26 + 1;
      } while (uVar25 != uVar26);
      uVar5 = vmovlps_avx(auVar42);
      *(undefined8 *)(local_1eb8 + lVar23) = uVar5;
    }
    lVar23 = lVar23 + 1;
    iVar22 = iVar6;
  } while (lVar23 != 0x21);
  local_1eb8[1] = (local_1eb8[0] + local_1eb8[1] + local_1eb8[0] + local_1eb8[1]) / fVar51;
  local_1e38 = (local_1e38 + 0.0 + local_1e38 + 0.0) / fVar51;
  param_6[8] = uStack_1e74;
  param_6[9] = uStack_1e6c;
  param_6[10] = uStack_1e64;
  param_6[0xb] = CONCAT44(local_1e58,uStack_1e5c);
  param_6[4] = uStack_1e94;
  param_6[5] = uStack_1e8c;
  param_6[6] = uStack_1e84;
  param_6[7] = CONCAT44(local_1e78,uStack_1e7c);
  param_6[0xc] = uStack_1e54;
  param_6[0xd] = uStack_1e4c;
  param_6[0xe] = uStack_1e44;
  param_6[0xf] = CONCAT44(local_1e38,uStack_1e3c);
  *param_6 = CONCAT44(local_1eb8[2],local_1eb8[1]);
  param_6[1] = uStack_1eac;
  param_6[2] = uStack_1ea4;
  param_6[3] = CONCAT44(local_1e98,uStack_1e9c);
  lVar23 = 1;
  dVar50 = 0.0;
  dVar49 = DAT_18000f0a0;
  do {
    fVar48 = *(float *)((longlong)param_6 + lVar23 * 4 + -4);
    dVar28 = (double)(*(float *)(param_4 + -4 + lVar23 * 4) *
                     *(float *)((longlong)param_5 + lVar23 * 4 + -4)) + dVar49;
    if (dVar28 < dVar50) {
      auVar44 = ZEXT464((uint)fVar48);
      dVar28 = sqrt(dVar28);
      fVar48 = auVar44._0_4_;
    }
    else {
      dVar28 = SQRT(dVar28);
    }
    *(float *)((longlong)param_6 + lVar23 * 4 + -4) = (float)((double)fVar48 / dVar28);
    fVar48 = *(float *)((longlong)param_6 + lVar23 * 4);
    dVar28 = (double)(*(float *)(param_4 + lVar23 * 4) * *(float *)((longlong)param_5 + lVar23 * 4))
             + dVar49;
    if (dVar28 < dVar50) {
      auVar44 = ZEXT464((uint)fVar48);
      dVar28 = sqrt(dVar28);
      fVar48 = auVar44._0_4_;
    }
    else {
      dVar28 = SQRT(dVar28);
    }
    *(float *)((longlong)param_6 + lVar23 * 4) = (float)((double)fVar48 / dVar28);
    lVar23 = lVar23 + 2;
  } while (lVar23 != 0x21);
  FUN_1800025c0(param_7 + 4,param_6);
  *(float *)param_7[8] = (float)((double)(local_5944 + -300) * DAT_18000f0a8);
  fVar48 = 0.0;
  auVar44 = ZEXT464(DAT_18000f0b0);
  lVar23 = 0;
  auVar35 = ZEXT416(DAT_18000f0b0);
  dVar49 = DAT_18000f0a8;
  fVar51 = DAT_18000f0b4;
  dVar50 = DAT_18000f0b8;
  do {
    auVar52 = ZEXT1664(auVar35);
    dVar29 = log10(dVar49 + (double)*(float *)(param_4 + lVar23 * 4));
    fVar39 = fVar51 + auVar44._0_4_;
    auVar31 = ZEXT416((uint)fVar39);
    dVar28 = dVar50 + (double)auVar52._0_4_;
    auVar43._0_8_ = (double)(float)dVar29;
    auVar43._8_8_ = extraout_var._0_8_;
    auVar37._8_8_ = 0;
    auVar37._0_8_ = dVar28;
    auVar35 = vmaxsd_avx(auVar37,auVar43);
    if ((double)fVar39 <= auVar35._0_8_) {
      auVar31._0_4_ = (float)auVar35._0_8_;
      auVar31._4_12_ = auVar35._4_12_;
    }
    local_1eb8[lVar23] = auVar31._0_4_;
    auVar35 = auVar31;
    if ((double)auVar31._0_4_ < dVar28) {
      auVar35 = ZEXT416((uint)(float)dVar28);
    }
    auVar45 = vmaxss_avx(auVar44._0_16_,auVar31);
    auVar44 = ZEXT1664(auVar45);
    fVar48 = fVar48 + *(float *)(param_4 + lVar23 * 4);
    lVar23 = lVar23 + 1;
  } while (lVar23 != 0x20);
  if (DAT_18000f0c0 <= (double)fVar48) {
    FUN_1800025c0(param_7,local_1eb8);
    auVar32._0_4_ = (float)*(undefined8 *)*param_7 + _DAT_18000f0d0;
    auVar32._4_4_ = (float)((ulonglong)*(undefined8 *)*param_7 >> 0x20) + _UNK_18000f0d4;
    auVar32._8_4_ = _UNK_18000f0d8 + 0.0;
    auVar32._12_4_ = _UNK_18000f0dc + 0.0;
    uVar5 = vmovlps_avx(auVar32);
    *(undefined8 *)*param_7 = uVar5;
    if ((local_b0 ^ (ulonglong)auStack_5988) == DAT_180580000) {
      return 0;
    }
  }
  else {
    param_7[7] = ZEXT832(0) << 0x20;
    param_7[6] = ZEXT832(0) << 0x20;
    auVar33 = ZEXT832(0) << 0x20;
    param_7[5] = auVar33;
    param_7[4] = auVar33;
    param_7[3] = auVar33;
    param_7[2] = auVar33;
    param_7[1] = ZEXT832(0) << 0x20;
    *param_7 = ZEXT832(0) << 0x20;
    *(undefined4 *)param_7[8] = 0;
    if ((local_b0 ^ (ulonglong)auStack_5988) == DAT_180580000) {
      return 1;
    }
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 1800025c0
   NAME : FUN_1800025c0
   SIG  : undefined FUN_1800025c0(void)
   ======================================================================== */

void FUN_1800025c0(undefined **param_1,undefined **param_2)

{
  longlong lVar1;
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
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  float fVar20;
  float fVar21;
  float fVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  float fVar33;
  undefined1 auVar34 [16];
  double dVar35;
  longlong lVar36;
  undefined1 auVar37 [16];
  undefined1 auVar39 [32];
  undefined1 auVar40 [32];
  undefined1 auVar38 [32];
  
  dVar35 = DAT_18000f0e0;
  if ((param_1 < param_2 + 0x10 && param_2 < param_1 + 0x10) ||
     (param_1 < &PTR_DAT_18057e660 && &DAT_18057d660 < param_1 + 0x10)) {
    lVar36 = -0x20;
    do {
      *(float *)((longlong)param_1 + (lVar36 + 0x20) * 4) =
           (float)((double)(*(float *)((longlong)param_2 + 0x7c) *
                            *(float *)((longlong)&PTR_DAT_18057e660 + lVar36 * 4) +
                           *(float *)(param_2 + 0xf) * (float)(&DAT_18057e5e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x74) * (float)(&DAT_18057e560)[lVar36] +
                           *(float *)(param_2 + 0xe) * (float)(&DAT_18057e4e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x6c) * (float)(&DAT_18057e460)[lVar36] +
                           *(float *)(param_2 + 0xd) * (float)(&DAT_18057e3e0)[lVar36] +
                           *(float *)((longlong)param_2 + 100) * (float)(&DAT_18057e360)[lVar36] +
                           *(float *)(param_2 + 0xc) * (float)(&DAT_18057e2e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x5c) * (float)(&DAT_18057e260)[lVar36] +
                           *(float *)(param_2 + 0xb) * (float)(&DAT_18057e1e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x54) * (float)(&DAT_18057e160)[lVar36] +
                           *(float *)(param_2 + 10) * (float)(&DAT_18057e0e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x4c) * (float)(&DAT_18057e060)[lVar36] +
                           *(float *)(param_2 + 9) * (float)(&DAT_18057dfe0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x44) * (float)(&DAT_18057df60)[lVar36] +
                           *(float *)(param_2 + 8) * (float)(&DAT_18057dee0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x3c) * (float)(&DAT_18057de60)[lVar36] +
                           *(float *)(param_2 + 7) * (float)(&DAT_18057dde0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x34) * (float)(&DAT_18057dd60)[lVar36] +
                           *(float *)(param_2 + 6) * (float)(&DAT_18057dce0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x2c) * (float)(&DAT_18057dc60)[lVar36] +
                           *(float *)(param_2 + 5) * (float)(&DAT_18057dbe0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x24) * (float)(&DAT_18057db60)[lVar36] +
                           *(float *)(param_2 + 4) * (float)(&DAT_18057dae0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x1c) * (float)(&DAT_18057da60)[lVar36] +
                           *(float *)(param_2 + 3) * (float)(&DAT_18057d9e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0x14) * (float)(&DAT_18057d960)[lVar36] +
                           *(float *)(param_2 + 2) * (float)(&DAT_18057d8e0)[lVar36] +
                           *(float *)((longlong)param_2 + 0xc) * (float)(&DAT_18057d860)[lVar36] +
                           *(float *)(param_2 + 1) * (float)(&DAT_18057d7e0)[lVar36] +
                           *(float *)((longlong)param_2 + 4) * (float)(&DAT_18057d760)[lVar36] +
                           *(float *)param_2 * (float)(&DAT_18057d6e0)[lVar36] + 0.0) * dVar35);
      lVar36 = lVar36 + 1;
    } while (lVar36 != 0);
  }
  else {
    fVar2 = *(float *)param_2;
    fVar3 = *(float *)((longlong)param_2 + 4);
    fVar4 = *(float *)(param_2 + 1);
    fVar5 = *(float *)((longlong)param_2 + 0xc);
    fVar6 = *(float *)(param_2 + 2);
    fVar7 = *(float *)((longlong)param_2 + 0x14);
    fVar8 = *(float *)(param_2 + 3);
    fVar9 = *(float *)((longlong)param_2 + 0x1c);
    fVar10 = *(float *)(param_2 + 4);
    fVar11 = *(float *)((longlong)param_2 + 0x24);
    fVar12 = *(float *)(param_2 + 5);
    fVar13 = *(float *)((longlong)param_2 + 0x2c);
    fVar14 = *(float *)(param_2 + 6);
    fVar15 = *(float *)((longlong)param_2 + 0x34);
    fVar16 = *(float *)(param_2 + 7);
    fVar17 = *(float *)((longlong)param_2 + 0x3c);
    fVar18 = *(float *)(param_2 + 8);
    fVar19 = *(float *)((longlong)param_2 + 0x44);
    fVar20 = *(float *)(param_2 + 9);
    fVar21 = *(float *)((longlong)param_2 + 0x4c);
    fVar22 = *(float *)(param_2 + 10);
    fVar23 = *(float *)((longlong)param_2 + 0x54);
    fVar24 = *(float *)(param_2 + 0xb);
    fVar25 = *(float *)((longlong)param_2 + 0x5c);
    fVar26 = *(float *)(param_2 + 0xc);
    fVar27 = *(float *)((longlong)param_2 + 100);
    fVar28 = *(float *)(param_2 + 0xd);
    fVar29 = *(float *)((longlong)param_2 + 0x6c);
    fVar30 = *(float *)(param_2 + 0xe);
    fVar31 = *(float *)((longlong)param_2 + 0x74);
    fVar32 = *(float *)(param_2 + 0xf);
    fVar33 = *(float *)((longlong)param_2 + 0x7c);
    lVar36 = -0x20;
    do {
      lVar1 = lVar36 * 4;
      auVar37._0_4_ =
           fVar33 * *(float *)((longlong)&PTR_DAT_18057e660 + lVar1) +
           fVar32 * (float)(&DAT_18057e5e0)[lVar36] +
           fVar31 * (float)(&DAT_18057e560)[lVar36] +
           fVar30 * (float)(&DAT_18057e4e0)[lVar36] +
           fVar29 * (float)(&DAT_18057e460)[lVar36] +
           fVar28 * (float)(&DAT_18057e3e0)[lVar36] +
           fVar27 * (float)(&DAT_18057e360)[lVar36] +
           fVar26 * (float)(&DAT_18057e2e0)[lVar36] +
           fVar25 * (float)(&DAT_18057e260)[lVar36] +
           fVar24 * (float)(&DAT_18057e1e0)[lVar36] +
           fVar23 * (float)(&DAT_18057e160)[lVar36] +
           fVar22 * (float)(&DAT_18057e0e0)[lVar36] +
           fVar21 * (float)(&DAT_18057e060)[lVar36] +
           fVar20 * (float)(&DAT_18057dfe0)[lVar36] +
           fVar19 * (float)(&DAT_18057df60)[lVar36] +
           fVar18 * (float)(&DAT_18057dee0)[lVar36] +
           fVar17 * (float)(&DAT_18057de60)[lVar36] +
           fVar16 * (float)(&DAT_18057dde0)[lVar36] +
           fVar15 * (float)(&DAT_18057dd60)[lVar36] +
           fVar14 * (float)(&DAT_18057dce0)[lVar36] +
           fVar13 * (float)(&DAT_18057dc60)[lVar36] +
           fVar12 * (float)(&DAT_18057dbe0)[lVar36] +
           fVar11 * (float)(&DAT_18057db60)[lVar36] +
           fVar10 * (float)(&DAT_18057dae0)[lVar36] +
           fVar9 * (float)(&DAT_18057da60)[lVar36] +
           fVar8 * (float)(&DAT_18057d9e0)[lVar36] +
           fVar7 * (float)(&DAT_18057d960)[lVar36] +
           fVar6 * (float)(&DAT_18057d8e0)[lVar36] +
           fVar5 * (float)(&DAT_18057d860)[lVar36] +
           fVar4 * (float)(&DAT_18057d7e0)[lVar36] +
           fVar3 * (float)(&DAT_18057d760)[lVar36] + (float)(&DAT_18057d6e0)[lVar36] * fVar2 + 0.0;
      auVar37._4_4_ =
           fVar33 * *(float *)((longlong)&PTR_DAT_18057e660 + lVar1 + 4) +
           fVar32 * (float)(&DAT_18057e5e4)[lVar36] +
           fVar31 * (float)(&DAT_18057e564)[lVar36] +
           fVar30 * (float)(&DAT_18057e4e4)[lVar36] +
           fVar29 * (float)(&DAT_18057e464)[lVar36] +
           fVar28 * (float)(&DAT_18057e3e4)[lVar36] +
           fVar27 * (float)(&DAT_18057e364)[lVar36] +
           fVar26 * (float)(&DAT_18057e2e4)[lVar36] +
           fVar25 * (float)(&DAT_18057e264)[lVar36] +
           fVar24 * (float)(&DAT_18057e1e4)[lVar36] +
           fVar23 * (float)(&DAT_18057e164)[lVar36] +
           fVar22 * (float)(&DAT_18057e0e4)[lVar36] +
           fVar21 * (float)(&DAT_18057e064)[lVar36] +
           fVar20 * (float)(&DAT_18057dfe4)[lVar36] +
           fVar19 * (float)(&DAT_18057df64)[lVar36] +
           fVar18 * (float)(&DAT_18057dee4)[lVar36] +
           fVar17 * (float)(&DAT_18057de64)[lVar36] +
           fVar16 * (float)(&DAT_18057dde4)[lVar36] +
           fVar15 * (float)(&DAT_18057dd64)[lVar36] +
           fVar14 * (float)(&DAT_18057dce4)[lVar36] +
           fVar13 * (float)(&DAT_18057dc64)[lVar36] +
           fVar12 * (float)(&DAT_18057dbe4)[lVar36] +
           fVar11 * (float)(&DAT_18057db64)[lVar36] +
           fVar10 * (float)(&DAT_18057dae4)[lVar36] +
           fVar9 * (float)(&DAT_18057da64)[lVar36] +
           fVar8 * (float)(&DAT_18057d9e4)[lVar36] +
           fVar7 * (float)(&DAT_18057d964)[lVar36] +
           fVar6 * (float)(&DAT_18057d8e4)[lVar36] +
           fVar5 * (float)(&DAT_18057d864)[lVar36] +
           fVar4 * (float)(&DAT_18057d7e4)[lVar36] +
           fVar3 * (float)(&DAT_18057d764)[lVar36] + (float)(&DAT_18057d6e4)[lVar36] * fVar2 + 0.0;
      auVar37._8_4_ =
           fVar33 * *(float *)((longlong)&PTR_DAT_18057e668 + lVar1) +
           fVar32 * (float)(&DAT_18057e5e8)[lVar36] +
           fVar31 * (float)(&DAT_18057e568)[lVar36] +
           fVar30 * (float)(&DAT_18057e4e8)[lVar36] +
           fVar29 * (float)(&DAT_18057e468)[lVar36] +
           fVar28 * (float)(&DAT_18057e3e8)[lVar36] +
           fVar27 * (float)(&DAT_18057e368)[lVar36] +
           fVar26 * (float)(&DAT_18057e2e8)[lVar36] +
           fVar25 * (float)(&DAT_18057e268)[lVar36] +
           fVar24 * (float)(&DAT_18057e1e8)[lVar36] +
           fVar23 * (float)(&DAT_18057e168)[lVar36] +
           fVar22 * (float)(&DAT_18057e0e8)[lVar36] +
           fVar21 * (float)(&DAT_18057e068)[lVar36] +
           fVar20 * (float)(&DAT_18057dfe8)[lVar36] +
           fVar19 * (float)(&DAT_18057df68)[lVar36] +
           fVar18 * (float)(&DAT_18057dee8)[lVar36] +
           fVar17 * (float)(&DAT_18057de68)[lVar36] +
           fVar16 * (float)(&DAT_18057dde8)[lVar36] +
           fVar15 * (float)(&DAT_18057dd68)[lVar36] +
           fVar14 * (float)(&DAT_18057dce8)[lVar36] +
           fVar13 * (float)(&DAT_18057dc68)[lVar36] +
           fVar12 * (float)(&DAT_18057dbe8)[lVar36] +
           fVar11 * (float)(&DAT_18057db68)[lVar36] +
           fVar10 * (float)(&DAT_18057dae8)[lVar36] +
           fVar9 * (float)(&DAT_18057da68)[lVar36] +
           fVar8 * (float)(&DAT_18057d9e8)[lVar36] +
           fVar7 * (float)(&DAT_18057d968)[lVar36] +
           fVar6 * (float)(&DAT_18057d8e8)[lVar36] +
           fVar5 * (float)(&DAT_18057d868)[lVar36] +
           fVar4 * (float)(&DAT_18057d7e8)[lVar36] +
           fVar3 * (float)(&DAT_18057d768)[lVar36] + (float)(&DAT_18057d6e8)[lVar36] * fVar2 + 0.0;
      auVar37._12_4_ =
           fVar33 * *(float *)((longlong)&PTR_DAT_18057e668 + lVar1 + 4) +
           fVar32 * (float)(&DAT_18057e5ec)[lVar36] +
           fVar31 * (float)(&DAT_18057e56c)[lVar36] +
           fVar30 * (float)(&DAT_18057e4ec)[lVar36] +
           fVar29 * (float)(&DAT_18057e46c)[lVar36] +
           fVar28 * (float)(&DAT_18057e3ec)[lVar36] +
           fVar27 * (float)(&DAT_18057e36c)[lVar36] +
           fVar26 * (float)(&DAT_18057e2ec)[lVar36] +
           fVar25 * (float)(&DAT_18057e26c)[lVar36] +
           fVar24 * (float)(&DAT_18057e1ec)[lVar36] +
           fVar23 * (float)(&DAT_18057e16c)[lVar36] +
           fVar22 * (float)(&DAT_18057e0ec)[lVar36] +
           fVar21 * (float)(&DAT_18057e06c)[lVar36] +
           fVar20 * (float)(&DAT_18057dfec)[lVar36] +
           fVar19 * (float)(&DAT_18057df6c)[lVar36] +
           fVar18 * (float)(&DAT_18057deec)[lVar36] +
           fVar17 * (float)(&DAT_18057de6c)[lVar36] +
           fVar16 * (float)(&DAT_18057ddec)[lVar36] +
           fVar15 * (float)(&DAT_18057dd6c)[lVar36] +
           fVar14 * (float)(&DAT_18057dcec)[lVar36] +
           fVar13 * (float)(&DAT_18057dc6c)[lVar36] +
           fVar12 * (float)(&DAT_18057dbec)[lVar36] +
           fVar11 * (float)(&DAT_18057db6c)[lVar36] +
           fVar10 * (float)(&DAT_18057daec)[lVar36] +
           fVar9 * (float)(&DAT_18057da6c)[lVar36] +
           fVar8 * (float)(&DAT_18057d9ec)[lVar36] +
           fVar7 * (float)(&DAT_18057d96c)[lVar36] +
           fVar6 * (float)(&DAT_18057d8ec)[lVar36] +
           fVar5 * (float)(&DAT_18057d86c)[lVar36] +
           fVar4 * (float)(&DAT_18057d7ec)[lVar36] +
           fVar3 * (float)(&DAT_18057d76c)[lVar36] + (float)(&DAT_18057d6ec)[lVar36] * fVar2 + 0.0;
      auVar38._16_4_ =
           fVar33 * *(float *)((longlong)&DAT_18057e670 + lVar1) +
           fVar32 * (float)(&DAT_18057e5f0)[lVar36] +
           fVar31 * (float)(&DAT_18057e570)[lVar36] +
           fVar30 * (float)(&DAT_18057e4f0)[lVar36] +
           fVar29 * (float)(&DAT_18057e470)[lVar36] +
           fVar28 * (float)(&DAT_18057e3f0)[lVar36] +
           fVar27 * (float)(&DAT_18057e370)[lVar36] +
           fVar26 * (float)(&DAT_18057e2f0)[lVar36] +
           fVar25 * (float)(&DAT_18057e270)[lVar36] +
           fVar24 * (float)(&DAT_18057e1f0)[lVar36] +
           fVar23 * (float)(&DAT_18057e170)[lVar36] +
           fVar22 * (float)(&DAT_18057e0f0)[lVar36] +
           fVar21 * (float)(&DAT_18057e070)[lVar36] +
           fVar20 * (float)(&DAT_18057dff0)[lVar36] +
           fVar19 * (float)(&DAT_18057df70)[lVar36] +
           fVar18 * (float)(&DAT_18057def0)[lVar36] +
           fVar17 * (float)(&DAT_18057de70)[lVar36] +
           fVar16 * (float)(&DAT_18057ddf0)[lVar36] +
           fVar15 * (float)(&DAT_18057dd70)[lVar36] +
           fVar14 * (float)(&DAT_18057dcf0)[lVar36] +
           fVar13 * (float)(&DAT_18057dc70)[lVar36] +
           fVar12 * (float)(&DAT_18057dbf0)[lVar36] +
           fVar11 * (float)(&DAT_18057db70)[lVar36] +
           fVar10 * (float)(&DAT_18057daf0)[lVar36] +
           fVar9 * (float)(&DAT_18057da70)[lVar36] +
           fVar8 * (float)(&DAT_18057d9f0)[lVar36] +
           fVar7 * (float)(&DAT_18057d970)[lVar36] +
           fVar6 * (float)(&DAT_18057d8f0)[lVar36] +
           fVar5 * (float)(&DAT_18057d870)[lVar36] +
           fVar4 * (float)(&DAT_18057d7f0)[lVar36] +
           fVar3 * (float)(&DAT_18057d770)[lVar36] + (float)(&DAT_18057d6f0)[lVar36] * fVar2 + 0.0;
      auVar38._0_16_ = auVar37;
      auVar38._20_4_ =
           fVar33 * *(float *)((longlong)&DAT_18057e670 + lVar1 + 4) +
           fVar32 * (float)(&DAT_18057e5f4)[lVar36] +
           fVar31 * (float)(&DAT_18057e574)[lVar36] +
           fVar30 * (float)(&DAT_18057e4f4)[lVar36] +
           fVar29 * (float)(&DAT_18057e474)[lVar36] +
           fVar28 * (float)(&DAT_18057e3f4)[lVar36] +
           fVar27 * (float)(&DAT_18057e374)[lVar36] +
           fVar26 * (float)(&DAT_18057e2f4)[lVar36] +
           fVar25 * (float)(&DAT_18057e274)[lVar36] +
           fVar24 * (float)(&DAT_18057e1f4)[lVar36] +
           fVar23 * (float)(&DAT_18057e174)[lVar36] +
           fVar22 * (float)(&DAT_18057e0f4)[lVar36] +
           fVar21 * (float)(&DAT_18057e074)[lVar36] +
           fVar20 * (float)(&DAT_18057dff4)[lVar36] +
           fVar19 * (float)(&DAT_18057df74)[lVar36] +
           fVar18 * (float)(&DAT_18057def4)[lVar36] +
           fVar17 * (float)(&DAT_18057de74)[lVar36] +
           fVar16 * (float)(&DAT_18057ddf4)[lVar36] +
           fVar15 * (float)(&DAT_18057dd74)[lVar36] +
           fVar14 * (float)(&DAT_18057dcf4)[lVar36] +
           fVar13 * (float)(&DAT_18057dc74)[lVar36] +
           fVar12 * (float)(&DAT_18057dbf4)[lVar36] +
           fVar11 * (float)(&DAT_18057db74)[lVar36] +
           fVar10 * (float)(&DAT_18057daf4)[lVar36] +
           fVar9 * (float)(&DAT_18057da74)[lVar36] +
           fVar8 * (float)(&DAT_18057d9f4)[lVar36] +
           fVar7 * (float)(&DAT_18057d974)[lVar36] +
           fVar6 * (float)(&DAT_18057d8f4)[lVar36] +
           fVar5 * (float)(&DAT_18057d874)[lVar36] +
           fVar4 * (float)(&DAT_18057d7f4)[lVar36] +
           fVar3 * (float)(&DAT_18057d774)[lVar36] + (float)(&DAT_18057d6f4)[lVar36] * fVar2 + 0.0;
      auVar38._24_4_ =
           fVar33 * *(float *)(&UNK_18057e678 + lVar1) +
           fVar32 * (float)(&DAT_18057e5f8)[lVar36] +
           fVar31 * (float)(&DAT_18057e578)[lVar36] +
           fVar30 * (float)(&DAT_18057e4f8)[lVar36] +
           fVar29 * (float)(&DAT_18057e478)[lVar36] +
           fVar28 * (float)(&DAT_18057e3f8)[lVar36] +
           fVar27 * (float)(&DAT_18057e378)[lVar36] +
           fVar26 * (float)(&DAT_18057e2f8)[lVar36] +
           fVar25 * (float)(&DAT_18057e278)[lVar36] +
           fVar24 * (float)(&DAT_18057e1f8)[lVar36] +
           fVar23 * (float)(&DAT_18057e178)[lVar36] +
           fVar22 * (float)(&DAT_18057e0f8)[lVar36] +
           fVar21 * (float)(&DAT_18057e078)[lVar36] +
           fVar20 * (float)(&DAT_18057dff8)[lVar36] +
           fVar19 * (float)(&DAT_18057df78)[lVar36] +
           fVar18 * (float)(&DAT_18057def8)[lVar36] +
           fVar17 * (float)(&DAT_18057de78)[lVar36] +
           fVar16 * (float)(&DAT_18057ddf8)[lVar36] +
           fVar15 * (float)(&DAT_18057dd78)[lVar36] +
           fVar14 * (float)(&DAT_18057dcf8)[lVar36] +
           fVar13 * (float)(&DAT_18057dc78)[lVar36] +
           fVar12 * (float)(&DAT_18057dbf8)[lVar36] +
           fVar11 * (float)(&DAT_18057db78)[lVar36] +
           fVar10 * (float)(&DAT_18057daf8)[lVar36] +
           fVar9 * (float)(&DAT_18057da78)[lVar36] +
           fVar8 * (float)(&DAT_18057d9f8)[lVar36] +
           fVar7 * (float)(&DAT_18057d978)[lVar36] +
           fVar6 * (float)(&DAT_18057d8f8)[lVar36] +
           fVar5 * (float)(&DAT_18057d878)[lVar36] +
           fVar4 * (float)(&DAT_18057d7f8)[lVar36] +
           fVar3 * (float)(&DAT_18057d778)[lVar36] + (float)(&DAT_18057d6f8)[lVar36] * fVar2 + 0.0;
      auVar38._28_4_ =
           fVar33 * *(float *)(&UNK_18057e67c + lVar1) +
           fVar32 * (float)(&DAT_18057e5fc)[lVar36] +
           fVar31 * (float)(&DAT_18057e57c)[lVar36] +
           fVar30 * (float)(&DAT_18057e4fc)[lVar36] +
           fVar29 * (float)(&DAT_18057e47c)[lVar36] +
           fVar28 * (float)(&DAT_18057e3fc)[lVar36] +
           fVar27 * (float)(&DAT_18057e37c)[lVar36] +
           fVar26 * (float)(&DAT_18057e2fc)[lVar36] +
           fVar25 * (float)(&DAT_18057e27c)[lVar36] +
           fVar24 * (float)(&DAT_18057e1fc)[lVar36] +
           fVar23 * (float)(&DAT_18057e17c)[lVar36] +
           fVar22 * (float)(&DAT_18057e0fc)[lVar36] +
           fVar21 * (float)(&DAT_18057e07c)[lVar36] +
           fVar20 * (float)(&DAT_18057dffc)[lVar36] +
           fVar19 * (float)(&DAT_18057df7c)[lVar36] +
           fVar18 * (float)(&DAT_18057defc)[lVar36] +
           fVar17 * (float)(&DAT_18057de7c)[lVar36] +
           fVar16 * (float)(&DAT_18057ddfc)[lVar36] +
           fVar15 * (float)(&DAT_18057dd7c)[lVar36] +
           fVar14 * (float)(&DAT_18057dcfc)[lVar36] +
           fVar13 * (float)(&DAT_18057dc7c)[lVar36] +
           fVar12 * (float)(&DAT_18057dbfc)[lVar36] +
           fVar11 * (float)(&DAT_18057db7c)[lVar36] +
           fVar10 * (float)(&DAT_18057dafc)[lVar36] +
           fVar9 * (float)(&DAT_18057da7c)[lVar36] +
           fVar8 * (float)(&DAT_18057d9fc)[lVar36] +
           fVar7 * (float)(&DAT_18057d97c)[lVar36] +
           fVar6 * (float)(&DAT_18057d8fc)[lVar36] +
           fVar5 * (float)(&DAT_18057d87c)[lVar36] +
           fVar4 * (float)(&DAT_18057d7fc)[lVar36] +
           fVar3 * (float)(&DAT_18057d77c)[lVar36] + (float)(&DAT_18057d6fc)[lVar36] * fVar2 + 0.0;
      auVar38 = vcvtps2pd_avx(auVar38._16_16_);
      auVar40._0_8_ = auVar38._0_8_ * dVar35;
      auVar40._8_8_ = auVar38._8_8_ * dVar35;
      auVar40._16_8_ = auVar38._16_8_ * dVar35;
      auVar40._24_8_ = auVar38._24_8_ * dVar35;
      auVar34 = vcvtpd2ps_avx(auVar40);
      *(undefined1 (*) [16])((longlong)param_1 + (lVar36 + 0x24) * 4) = auVar34;
      auVar38 = vcvtps2pd_avx(auVar37);
      auVar39._0_8_ = auVar38._0_8_ * dVar35;
      auVar39._8_8_ = auVar38._8_8_ * dVar35;
      auVar39._16_8_ = auVar38._16_8_ * dVar35;
      auVar39._24_8_ = auVar38._24_8_ * dVar35;
      auVar34 = vcvtpd2ps_avx(auVar39);
      *(undefined1 (*) [16])((longlong)param_1 + (lVar36 + 0x20) * 4) = auVar34;
      lVar36 = lVar36 + 8;
    } while (lVar36 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002c90
   NAME : rnn_biquad
   SIG  : undefined rnn_biquad(void)
   ======================================================================== */

void rnn_biquad(longlong param_1,float *param_2,longlong param_3,float *param_4,float *param_5,
               uint param_6)

{
  float fVar1;
  float fVar2;
  ulonglong uVar3;
  
                    /* 0x2c90  4  rnn_biquad */
  if (0 < (int)param_6) {
    uVar3 = 0;
    do {
      fVar1 = *(float *)(param_3 + uVar3 * 4);
      fVar2 = fVar1 + *param_2;
      *param_2 = (fVar1 * *param_4 - fVar2 * *param_5) + param_2[1];
      param_2[1] = fVar1 * param_4[1] - fVar2 * param_5[1];
      *(float *)(param_1 + uVar3 * 4) = fVar2;
      uVar3 = uVar3 + 1;
    } while (param_6 != uVar3);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002d30
   NAME : rnn_pitch_filter
   SIG  : undefined rnn_pitch_filter(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_pitch_filter(ulonglong param_1,ulonglong param_2,longlong param_3,longlong param_4,
                     longlong param_5,longlong param_6)

{
  longlong lVar1;
  undefined8 uVar2;
  undefined1 auVar3 [32];
  int iVar4;
  bool bVar5;
  bool bVar6;
  undefined1 auVar7 [32];
  int iVar8;
  uint uVar9;
  ulonglong uVar10;
  longlong lVar11;
  float *_Dst;
  double dVar12;
  undefined1 auVar14 [12];
  undefined1 auVar15 [16];
  undefined1 auVar17 [16];
  double dVar13;
  undefined1 auVar16 [16];
  undefined1 auVar18 [32];
  undefined1 extraout_var [60];
  undefined1 auVar19 [16];
  undefined1 auVar20 [16];
  undefined1 auVar21 [16];
  undefined1 auVar22 [16];
  undefined1 auVar23 [32];
  float fVar24;
  undefined1 auVar25 [16];
  undefined1 auVar26 [32];
  undefined1 auVar27 [16];
  float fVar28;
  undefined1 auVar29 [32];
  float fVar30;
  undefined1 auVar31 [32];
  undefined1 auVar32 [32];
  undefined1 auVar33 [64];
  double dVar34;
  float fVar35;
  undefined1 auVar36 [64];
  undefined1 auVar37 [64];
  double dVar38;
  undefined1 unaff_00001504 [12];
  float afStack_1178 [8];
  float local_1158 [484];
  undefined8 local_9c8;
  undefined8 uStack_9c0;
  undefined8 uStack_9b8;
  undefined8 uStack_9b0;
  undefined8 local_9a8;
  undefined8 uStack_9a0;
  undefined8 uStack_998;
  undefined8 uStack_990;
  undefined8 local_988;
  undefined8 uStack_980;
  undefined8 uStack_978;
  undefined8 uStack_970;
  undefined8 local_968;
  undefined8 uStack_960;
  undefined8 uStack_958;
  undefined8 uStack_950;
  float local_948 [516];
  float local_138 [3];
  undefined8 uStack_12c;
  undefined8 uStack_124;
  undefined4 uStack_11c;
  float local_118;
  undefined8 uStack_114;
  undefined8 uStack_10c;
  undefined8 uStack_104;
  undefined4 uStack_fc;
  float local_f8;
  undefined8 uStack_f4;
  undefined8 uStack_ec;
  undefined8 uStack_e4;
  undefined4 uStack_dc;
  float local_d8;
  undefined8 uStack_d4;
  undefined8 uStack_cc;
  undefined8 uStack_c4;
  undefined4 uStack_bc;
  float local_b8;
  undefined4 uStack_b4;
  ulonglong local_b0;
  undefined8 uStack_48;
  
                    /* 0x2d30  27  rnn_pitch_filter */
  uStack_48 = 0x180002d46;
  local_b0 = DAT_180580000 ^ (ulonglong)afStack_1178;
  lVar11 = 0;
  _Dst = local_948;
  memset(_Dst,0,0x784);
  memset(afStack_1178 + 8,0,0x784);
  auVar36 = ZEXT464((uint)DAT_18000f098);
  auVar37 = ZEXT1264(ZEXT812(0));
  dVar38 = 0.0;
  dVar34 = DAT_18000f0e8;
  dVar13 = DAT_18000f0a0;
  do {
    fVar24 = *(float *)(param_5 + lVar11 * 4);
    fVar30 = *(float *)(param_6 + lVar11 * 4);
    fVar35 = auVar36._0_4_;
    auVar19 = auVar36._0_16_;
    if (fVar24 <= fVar30) {
      dVar12 = (double)(fVar24 * fVar24 * (-(fVar30 * fVar30) + fVar35)) /
               (dVar13 + (double)((-(fVar24 * fVar24) + fVar35) * fVar30 * fVar30));
      auVar19._0_4_ = (float)dVar12;
      auVar19._4_4_ = (int)((ulonglong)dVar12 >> 0x20);
      auVar19._8_8_ = 0;
    }
    fVar30 = auVar37._0_4_;
    fVar24 = auVar19._0_4_;
    auVar16 = vcmpss_avx(auVar36._0_16_,auVar19,1);
    auVar21 = vcmpss_avx(auVar19,auVar37._0_16_,1);
    auVar15._0_4_ = (float)(auVar16._0_4_ & ~auVar21._0_4_ & (uint)fVar35);
    auVar15._4_4_ = auVar16._4_4_ & ~auVar21._4_4_ & auVar36._4_4_;
    auVar15._8_4_ = auVar16._8_4_ & ~auVar21._8_4_ & auVar36._8_4_;
    auVar15._12_4_ = auVar16._12_4_ & ~auVar21._12_4_ & auVar36._12_4_;
    if (fVar30 <= fVar24) {
      if (fVar35 < fVar24) goto LAB_180002e93;
LAB_180002eab:
      auVar15 = auVar19;
      if (fVar30 <= auVar15._0_4_) goto LAB_180002e9a;
LAB_180002eb6:
      auVar33._0_4_ = sqrtf(auVar15._0_4_);
      auVar33._4_60_ = extraout_var;
      auVar16 = auVar33._0_16_;
    }
    else {
      auVar19 = auVar15;
      if (fVar24 <= fVar35) goto LAB_180002eab;
LAB_180002e93:
      if (auVar15._0_4_ < fVar30) goto LAB_180002eb6;
LAB_180002e9a:
      auVar16._0_4_ = SQRT(auVar15._0_4_);
      auVar16._4_12_ = auVar15._4_12_;
    }
    auVar33 = ZEXT1664(auVar16);
    fVar24 = auVar16._0_4_;
    dVar12 = (double)*(float *)(param_3 + lVar11 * 4) /
             ((double)*(float *)(param_4 + lVar11 * 4) + dVar34);
    if (dVar12 < dVar38) {
      dVar12 = sqrt(dVar12);
      fVar24 = auVar33._0_4_;
    }
    else {
      dVar12 = SQRT(dVar12);
    }
    local_948[lVar11 + 0x1e4] = (float)(dVar12 * (double)fVar24);
    lVar11 = lVar11 + 1;
    if (lVar11 == 0x20) {
      FUN_180003490(_Dst,local_948 + 0x1e4);
      bVar5 = param_1 < param_2 + 0xf08;
      bVar6 = param_2 < param_1 + 0xf08;
      lVar11 = 0;
      if (!bVar6 || !bVar5) {
        do {
          auVar3 = *(undefined1 (*) [32])(param_2 + lVar11 * 8);
          auVar26 = *(undefined1 (*) [32])(param_2 + 0x20 + lVar11 * 8);
          auVar18 = vshufps_avx(auVar3,auVar26,0x88);
          auVar23 = vpermpd_avx2(auVar18,0xd8);
          auVar3 = vshufps_avx(auVar3,auVar26,0xdd);
          auVar7 = vpermpd_avx2(auVar3,0xd8);
          auVar3 = *(undefined1 (*) [32])(param_1 + lVar11 * 8);
          auVar26 = *(undefined1 (*) [32])(param_1 + 0x20 + lVar11 * 8);
          auVar18 = vshufps_avx(auVar3,auVar26,0x88);
          auVar18 = vpermpd_avx2(auVar18,0xd8);
          auVar3 = vshufps_avx(auVar3,auVar26,0xdd);
          auVar3 = vpermpd_avx2(auVar3,0xd8);
          auVar31._0_4_ = local_948[lVar11] * auVar23._0_4_ + auVar18._0_4_;
          auVar31._4_4_ = local_948[lVar11 + 1] * auVar23._4_4_ + auVar18._4_4_;
          auVar31._8_4_ = local_948[lVar11 + 2] * auVar23._8_4_ + auVar18._8_4_;
          auVar31._12_4_ = local_948[lVar11 + 3] * auVar23._12_4_ + auVar18._12_4_;
          auVar31._16_4_ = local_948[lVar11 + 4] * auVar23._16_4_ + auVar18._16_4_;
          auVar31._20_4_ = local_948[lVar11 + 5] * auVar23._20_4_ + auVar18._20_4_;
          auVar31._24_4_ = local_948[lVar11 + 6] * auVar23._24_4_ + auVar18._24_4_;
          auVar31._28_4_ = local_948[lVar11 + 7] * auVar23._28_4_ + auVar18._28_4_;
          auVar26._0_4_ = local_948[lVar11] * auVar7._0_4_ + auVar3._0_4_;
          auVar26._4_4_ = local_948[lVar11 + 1] * auVar7._4_4_ + auVar3._4_4_;
          auVar26._8_4_ = local_948[lVar11 + 2] * auVar7._8_4_ + auVar3._8_4_;
          auVar26._12_4_ = local_948[lVar11 + 3] * auVar7._12_4_ + auVar3._12_4_;
          auVar26._16_4_ = local_948[lVar11 + 4] * auVar7._16_4_ + auVar3._16_4_;
          auVar26._20_4_ = local_948[lVar11 + 5] * auVar7._20_4_ + auVar3._20_4_;
          auVar26._24_4_ = local_948[lVar11 + 6] * auVar7._24_4_ + auVar3._24_4_;
          auVar26._28_4_ = local_948[lVar11 + 7] * auVar7._28_4_ + auVar3._28_4_;
          auVar3 = vunpckhps_avx(auVar31,auVar26);
          auVar26 = vunpcklps_avx(auVar31,auVar26);
          auVar18 = vperm2f128_avx(auVar26,auVar3,0x31);
          auVar3 = vperm2f128_avx(auVar26,auVar3,0x20);
          *(undefined1 (*) [32])(param_1 + lVar11 * 8) = auVar3;
          *(undefined1 (*) [32])(param_1 + 0x20 + lVar11 * 8) = auVar18;
          lVar11 = lVar11 + 8;
        } while (lVar11 != 0x1e0);
        lVar11 = 0x1e0;
        _Dst = local_948 + 0x1e0;
      }
      fVar24 = *_Dst;
      uVar10 = (ulonglong)(uint)((int)lVar11 * 8);
      *(float *)(param_1 + uVar10) =
           fVar24 * *(float *)(param_2 + uVar10) + *(float *)(param_1 + uVar10);
      *(float *)(param_1 + 4 + uVar10) =
           fVar24 * *(float *)(param_2 + 4 + uVar10) + *(float *)(param_1 + 4 + uVar10);
      if (bVar6 && bVar5) {
        lVar11 = lVar11 + -0x1e0;
        do {
          fVar24 = local_948[lVar11 + 0x1e1];
          *(float *)(param_1 + 0xf08 + lVar11 * 8) =
               fVar24 * *(float *)(param_2 + 0xf08 + lVar11 * 8) +
               *(float *)(param_1 + 0xf08 + lVar11 * 8);
          *(float *)(param_1 + 0xf0c + lVar11 * 8) =
               fVar24 * *(float *)(param_2 + 0xf0c + lVar11 * 8) +
               *(float *)(param_1 + 0xf0c + lVar11 * 8);
          fVar24 = local_948[lVar11 + 0x1e2];
          *(float *)(param_1 + 0xf10 + lVar11 * 8) =
               fVar24 * *(float *)(param_2 + 0xf10 + lVar11 * 8) +
               *(float *)(param_1 + 0xf10 + lVar11 * 8);
          *(float *)(param_1 + 0xf14 + lVar11 * 8) =
               fVar24 * *(float *)(param_2 + 0xf14 + lVar11 * 8) +
               *(float *)(param_1 + 0xf14 + lVar11 * 8);
          lVar11 = lVar11 + 2;
        } while (lVar11 != 0);
      }
      auVar14 = ZEXT812(0);
      local_d8 = auVar14._0_4_;
      local_138[0] = 0.0;
      uStack_d4 = auVar14._4_8_;
      uStack_cc = 0;
      uStack_c4 = 0;
      uStack_bc = 0;
      uStack_ec = 0;
      uStack_e4 = 0;
      uStack_dc = 0;
      uStack_10c = 0;
      uStack_104 = 0;
      uStack_fc = 0;
      local_138[1] = auVar14._4_4_;
      local_138[1] = 0.0;
      local_138[2] = 0.0;
      uStack_12c = 0;
      uStack_124 = 0;
      uStack_11c = 0;
      local_b8 = 0.0;
      uStack_b4 = 0;
      lVar11 = 0;
      iVar8 = 0;
      local_118 = local_138[0];
      uStack_114 = uStack_d4;
      local_f8 = local_138[0];
      uStack_f4 = uStack_d4;
      local_d8 = local_138[0];
      do {
        iVar4 = (&DAT_18000f014)[lVar11];
        uVar9 = iVar4 - iVar8;
        if (uVar9 != 0 && iVar8 <= iVar4) {
          fVar24 = (float)(int)uVar9;
          auVar36 = ZEXT864(*(ulonglong *)(local_138 + lVar11));
          auVar21._8_8_ = 0;
          auVar21._0_8_ = *(ulonglong *)(local_138 + lVar11);
          if (uVar9 == 1) {
            uVar10 = 0;
          }
          else {
            lVar1 = param_1 + 0xc + (longlong)iVar8 * 8;
            uVar10 = 0;
            do {
              auVar27._0_4_ = (float)(int)uVar10 / fVar24;
              auVar27._4_12_ = unaff_00001504;
              fVar30 = *(float *)(lVar1 + -0xc + uVar10 * 8);
              fVar35 = *(float *)(lVar1 + -8 + uVar10 * 8);
              fVar28 = fVar35 * fVar35 + fVar30 * fVar30;
              auVar19 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar27._0_4_)),auVar27,0x10);
              auVar20._0_4_ = (float)((int)uVar10 + 1) / fVar24;
              auVar20._4_12_ = unaff_00001504;
              fVar30 = *(float *)(lVar1 + -4 + uVar10 * 8);
              fVar35 = *(float *)(lVar1 + uVar10 * 8);
              fVar30 = fVar35 * fVar35 + fVar30 * fVar30;
              auVar16 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar20._0_4_)),auVar20,0x10);
              auVar21._0_4_ = auVar16._0_4_ * fVar30 + auVar19._0_4_ * fVar28 + auVar36._0_4_;
              auVar21._4_4_ = auVar16._4_4_ * fVar30 + auVar19._4_4_ * fVar28 + auVar36._4_4_;
              auVar21._8_4_ = auVar16._8_4_ * fVar30 + auVar19._8_4_ * fVar28 + auVar36._8_4_;
              auVar21._12_4_ = auVar16._12_4_ * fVar30 + auVar19._12_4_ * fVar28 + auVar36._12_4_;
              auVar36 = ZEXT1664(auVar21);
              uVar10 = uVar10 + 2;
            } while (uVar10 != (uVar9 & 0x7ffffffe));
          }
          auVar22 = auVar21;
          if ((uVar9 & 1) != 0) {
            lVar1 = param_1 + (longlong)iVar8 * 8;
            auVar25._0_4_ = (float)(int)uVar10 / fVar24;
            auVar25._4_12_ = unaff_00001504;
            fVar24 = *(float *)(lVar1 + uVar10 * 8);
            fVar30 = *(float *)(lVar1 + 4 + uVar10 * 8);
            fVar24 = fVar30 * fVar30 + fVar24 * fVar24;
            auVar19 = vinsertps_avx(ZEXT416((uint)(DAT_18000f098 - auVar25._0_4_)),auVar25,0x10);
            auVar22._0_4_ = auVar19._0_4_ * fVar24 + auVar21._0_4_;
            auVar22._4_4_ = auVar19._4_4_ * fVar24 + auVar21._4_4_;
            auVar22._8_4_ = auVar19._8_4_ * fVar24 + auVar21._8_4_;
            auVar22._12_4_ = auVar19._12_4_ * fVar24 + auVar21._12_4_;
          }
          uVar2 = vmovlps_avx(auVar22);
          *(undefined8 *)(local_138 + lVar11) = uVar2;
        }
        lVar11 = lVar11 + 1;
        iVar8 = iVar4;
      } while (lVar11 != 0x21);
      local_138[1] = (local_138[0] + local_138[1] + local_138[0] + local_138[1]) / DAT_18000f09c;
      local_b8 = (local_b8 + 0.0 + local_b8 + 0.0) / DAT_18000f09c;
      local_9a8 = uStack_114;
      uStack_9a0 = uStack_10c;
      uStack_998 = uStack_104;
      uStack_990 = CONCAT44(local_f8,uStack_fc);
      local_988 = uStack_f4;
      uStack_980 = uStack_ec;
      uStack_978 = uStack_e4;
      uStack_970 = CONCAT44(local_d8,uStack_dc);
      local_9c8 = CONCAT44(local_138[2],local_138[1]);
      uStack_9c0 = uStack_12c;
      uStack_9b8 = uStack_124;
      uStack_9b0 = CONCAT44(local_118,uStack_11c);
      local_968 = uStack_d4;
      uStack_960 = uStack_cc;
      uStack_958 = uStack_c4;
      uStack_950 = CONCAT44(local_b8,uStack_bc);
      lVar11 = 1;
      auVar36 = ZEXT1264(ZEXT812(0));
      do {
        dVar13 = (double)*(float *)(param_3 + -4 + lVar11 * 4) /
                 ((double)afStack_1178[lVar11 + 0x1eb] + dVar34);
        if (dVar13 < auVar36._0_8_) {
          auVar36 = ZEXT1664(auVar36._0_16_);
          dVar13 = sqrt(dVar13);
        }
        else {
          dVar13 = SQRT(dVar13);
        }
        local_948[lVar11 + 0x203] = (float)dVar13;
        dVar13 = (double)*(float *)(param_3 + lVar11 * 4) /
                 ((double)*(float *)((longlong)&local_9c8 + lVar11 * 4) + dVar34);
        if (dVar13 < auVar36._0_8_) {
          auVar36 = ZEXT1664(auVar36._0_16_);
          dVar13 = sqrt(dVar13);
        }
        else {
          dVar13 = SQRT(dVar13);
        }
        local_138[lVar11] = (float)dVar13;
        lVar11 = lVar11 + 2;
      } while (lVar11 != 0x21);
      FUN_180003490(afStack_1178 + 8,local_138);
      lVar11 = 8;
      do {
        auVar3 = *(undefined1 (*) [32])((param_1 - 0x40) + lVar11 * 8);
        auVar26 = *(undefined1 (*) [32])((param_1 - 0x20) + lVar11 * 8);
        auVar18 = *(undefined1 (*) [32])(param_1 + lVar11 * 8);
        auVar23 = *(undefined1 (*) [32])(param_1 + 0x20 + lVar11 * 8);
        auVar7 = vshufps_avx(auVar3,auVar26,0x88);
        auVar31 = vpermpd_avx2(auVar7,0xd8);
        auVar7 = vshufps_avx(auVar18,auVar23,0x88);
        auVar7 = vpermpd_avx2(auVar7,0xd8);
        auVar3 = vshufps_avx(auVar3,auVar26,0xdd);
        auVar26 = vpermpd_avx2(auVar3,0xd8);
        auVar3 = vshufps_avx(auVar18,auVar23,0xdd);
        auVar3 = vpermpd_avx2(auVar3,0xd8);
        auVar29._0_4_ = afStack_1178[lVar11] * auVar31._0_4_;
        auVar29._4_4_ = afStack_1178[lVar11 + 1] * auVar31._4_4_;
        auVar29._8_4_ = afStack_1178[lVar11 + 2] * auVar31._8_4_;
        auVar29._12_4_ = afStack_1178[lVar11 + 3] * auVar31._12_4_;
        auVar29._16_4_ = afStack_1178[lVar11 + 4] * auVar31._16_4_;
        auVar29._20_4_ = afStack_1178[lVar11 + 5] * auVar31._20_4_;
        auVar29._24_4_ = afStack_1178[lVar11 + 6] * auVar31._24_4_;
        auVar29._28_4_ = afStack_1178[lVar11 + 7] * auVar31._28_4_;
        auVar32._0_4_ = afStack_1178[lVar11 + 8] * auVar7._0_4_;
        auVar32._4_4_ = afStack_1178[lVar11 + 9] * auVar7._4_4_;
        auVar32._8_4_ = afStack_1178[lVar11 + 10] * auVar7._8_4_;
        auVar32._12_4_ = afStack_1178[lVar11 + 0xb] * auVar7._12_4_;
        auVar32._16_4_ = afStack_1178[lVar11 + 0xc] * auVar7._16_4_;
        auVar32._20_4_ = afStack_1178[lVar11 + 0xd] * auVar7._20_4_;
        auVar32._24_4_ = afStack_1178[lVar11 + 0xe] * auVar7._24_4_;
        auVar32._28_4_ = afStack_1178[lVar11 + 0xf] * auVar7._28_4_;
        auVar18._0_4_ = afStack_1178[lVar11] * auVar26._0_4_;
        auVar18._4_4_ = afStack_1178[lVar11 + 1] * auVar26._4_4_;
        auVar18._8_4_ = afStack_1178[lVar11 + 2] * auVar26._8_4_;
        auVar18._12_4_ = afStack_1178[lVar11 + 3] * auVar26._12_4_;
        auVar18._16_4_ = afStack_1178[lVar11 + 4] * auVar26._16_4_;
        auVar18._20_4_ = afStack_1178[lVar11 + 5] * auVar26._20_4_;
        auVar18._24_4_ = afStack_1178[lVar11 + 6] * auVar26._24_4_;
        auVar18._28_4_ = afStack_1178[lVar11 + 7] * auVar26._28_4_;
        auVar23._0_4_ = afStack_1178[lVar11 + 8] * auVar3._0_4_;
        auVar23._4_4_ = afStack_1178[lVar11 + 9] * auVar3._4_4_;
        auVar23._8_4_ = afStack_1178[lVar11 + 10] * auVar3._8_4_;
        auVar23._12_4_ = afStack_1178[lVar11 + 0xb] * auVar3._12_4_;
        auVar23._16_4_ = afStack_1178[lVar11 + 0xc] * auVar3._16_4_;
        auVar23._20_4_ = afStack_1178[lVar11 + 0xd] * auVar3._20_4_;
        auVar23._24_4_ = afStack_1178[lVar11 + 0xe] * auVar3._24_4_;
        auVar23._28_4_ = afStack_1178[lVar11 + 0xf] * auVar3._28_4_;
        auVar3 = vunpckhps_avx(auVar29,auVar18);
        auVar26 = vunpcklps_avx(auVar29,auVar18);
        auVar18 = vperm2f128_avx(auVar26,auVar3,0x31);
        auVar3 = vperm2f128_avx(auVar26,auVar3,0x20);
        *(undefined1 (*) [32])((param_1 - 0x40) + lVar11 * 8) = auVar3;
        *(undefined1 (*) [32])((param_1 - 0x20) + lVar11 * 8) = auVar18;
        auVar3 = vunpckhps_avx(auVar32,auVar23);
        auVar26 = vunpcklps_avx(auVar32,auVar23);
        auVar18 = vperm2f128_avx(auVar26,auVar3,0x31);
        auVar3 = vperm2f128_avx(auVar26,auVar3,0x20);
        *(undefined1 (*) [32])(param_1 + lVar11 * 8) = auVar3;
        *(undefined1 (*) [32])(param_1 + 0x20 + lVar11 * 8) = auVar18;
        lVar11 = lVar11 + 0x10;
      } while (lVar11 != 0x1e8);
      auVar17._0_4_ = local_1158[0x1e0] * (float)*(undefined8 *)(param_1 + 0xf00);
      auVar17._4_4_ =
           local_1158[0x1e0] * (float)((ulonglong)*(undefined8 *)(param_1 + 0xf00) >> 0x20);
      auVar17._8_4_ = local_1158[0x1e0] * 0.0;
      auVar17._12_4_ = local_1158[0x1e0] * 0.0;
      uVar2 = vmovlps_avx(auVar17);
      *(undefined8 *)(param_1 + 0xf00) = uVar2;
      if ((local_b0 ^ (ulonglong)afStack_1178) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
        FUN_18000db80();
      }
      return;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 180003490
   NAME : FUN_180003490
   SIG  : undefined FUN_180003490(void)
   ======================================================================== */

void FUN_180003490(undefined1 (*param_1) [32],undefined4 *param_2)

{
  undefined4 *puVar1;
  float fVar2;
  undefined4 uVar3;
  undefined1 auVar4 [16];
  int iVar5;
  undefined1 auVar6 [32];
  undefined1 auVar7 [32];
  float fVar8;
  int iVar9;
  uint uVar10;
  ulonglong uVar11;
  longlong lVar12;
  longlong lVar13;
  int iVar14;
  ulonglong uVar15;
  ulonglong uVar16;
  undefined1 auVar17 [16];
  undefined1 auVar18 [32];
  undefined1 auVar19 [32];
  undefined1 auVar20 [32];
  undefined1 auVar21 [32];
  undefined1 auVar22 [32];
  float fVar23;
  float fVar24;
  undefined1 auVar25 [32];
  undefined1 auVar26 [32];
  undefined1 auVar27 [32];
  undefined1 auVar28 [32];
  
  auVar17._0_12_ = ZEXT812(0);
  auVar17._12_4_ = 0;
  param_1[0xe] = ZEXT1632(auVar17);
  param_1[0xd] = ZEXT1632(auVar17);
  auVar18 = ZEXT1632(auVar17);
  param_1[0xc] = auVar18;
  param_1[0xb] = auVar18;
  param_1[10] = auVar18;
  param_1[9] = auVar18;
  auVar18 = ZEXT1632(auVar17);
  param_1[8] = auVar18;
  param_1[7] = auVar18;
  param_1[6] = auVar18;
  param_1[5] = auVar18;
  param_1[4] = auVar18;
  param_1[3] = auVar18;
  param_1[2] = auVar18;
  param_1[1] = auVar18;
  *param_1 = ZEXT1632(auVar17);
  param_1[0xf][0] = 0;
  iVar9 = DAT_18000f0fc;
  fVar8 = DAT_18000f098;
  lVar13 = 1;
  auVar4._8_8_ = 0;
  auVar4._0_8_ = DAT_18000f100;
  auVar18 = vpmovsxbd_avx2(auVar4);
  auVar19._4_4_ = DAT_18000f0f0;
  auVar19._0_4_ = DAT_18000f0f0;
  auVar19._8_4_ = DAT_18000f0f0;
  auVar19._12_4_ = DAT_18000f0f0;
  auVar19._16_4_ = DAT_18000f0f0;
  auVar19._20_4_ = DAT_18000f0f0;
  auVar19._24_4_ = DAT_18000f0f0;
  auVar19._28_4_ = DAT_18000f0f0;
  auVar20._4_4_ = DAT_18000f0f4;
  auVar20._0_4_ = DAT_18000f0f4;
  auVar20._8_4_ = DAT_18000f0f4;
  auVar20._12_4_ = DAT_18000f0f4;
  auVar20._16_4_ = DAT_18000f0f4;
  auVar20._20_4_ = DAT_18000f0f4;
  auVar20._24_4_ = DAT_18000f0f4;
  auVar20._28_4_ = DAT_18000f0f4;
  auVar21._4_4_ = DAT_18000f0f8;
  auVar21._0_4_ = DAT_18000f0f8;
  auVar21._8_4_ = DAT_18000f0f8;
  auVar21._12_4_ = DAT_18000f0f8;
  auVar21._16_4_ = DAT_18000f0f8;
  auVar21._20_4_ = DAT_18000f0f8;
  auVar21._24_4_ = DAT_18000f0f8;
  auVar21._28_4_ = DAT_18000f0f8;
  auVar22._4_4_ = DAT_18000f098;
  auVar22._0_4_ = DAT_18000f098;
  auVar22._8_4_ = DAT_18000f098;
  auVar22._12_4_ = DAT_18000f098;
  auVar22._16_4_ = DAT_18000f098;
  auVar22._20_4_ = DAT_18000f098;
  auVar22._24_4_ = DAT_18000f098;
  auVar22._28_4_ = DAT_18000f098;
  iVar14 = 2;
  do {
    iVar5 = (&DAT_18000f014)[lVar13];
    uVar10 = iVar5 - iVar14;
    if (uVar10 != 0 && iVar14 <= iVar5) {
      fVar23 = (float)(int)uVar10;
      lVar12 = (longlong)iVar14;
      uVar11 = (ulonglong)uVar10;
      puVar1 = (undefined4 *)(*param_1 + lVar12 * 4);
      if ((uVar10 < 8) ||
         ((puVar1 < param_2 + 0x20 && (param_2 < *param_1 + (lVar12 + uVar11) * 4)))) {
        uVar15 = 0;
      }
      else {
        uVar15 = (ulonglong)(uVar10 & 0x7ffffff8);
        auVar25._4_4_ = fVar23;
        auVar25._0_4_ = fVar23;
        auVar25._8_4_ = fVar23;
        auVar25._12_4_ = fVar23;
        auVar25._16_4_ = fVar23;
        auVar25._20_4_ = fVar23;
        auVar25._24_4_ = fVar23;
        auVar25._28_4_ = fVar23;
        fVar24 = (float)param_2[lVar13 + -1];
        fVar2 = (float)param_2[lVar13];
        uVar16 = 0;
        auVar7 = auVar18;
        do {
          auVar26 = vpsrld_avx2(auVar7,0x10);
          auVar26 = vpblendw_avx2(auVar26,auVar20,0xaa);
          auVar26 = vsubps_avx(auVar26,auVar21);
          auVar6 = vpblendw_avx2(auVar7,auVar19,0xaa);
          auVar27._0_4_ = auVar6._0_4_ + auVar26._0_4_;
          auVar27._4_4_ = auVar6._4_4_ + auVar26._4_4_;
          auVar27._8_4_ = auVar6._8_4_ + auVar26._8_4_;
          auVar27._12_4_ = auVar6._12_4_ + auVar26._12_4_;
          auVar27._16_4_ = auVar6._16_4_ + auVar26._16_4_;
          auVar27._20_4_ = auVar6._20_4_ + auVar26._20_4_;
          auVar27._24_4_ = auVar6._24_4_ + auVar26._24_4_;
          auVar27._28_4_ = auVar6._28_4_ + auVar26._28_4_;
          auVar26 = vdivps_avx(auVar27,auVar25);
          auVar6 = vsubps_avx(auVar22,auVar26);
          auVar28._0_4_ = fVar24 * auVar6._0_4_ + auVar26._0_4_ * fVar2;
          auVar28._4_4_ = fVar24 * auVar6._4_4_ + auVar26._4_4_ * fVar2;
          auVar28._8_4_ = fVar24 * auVar6._8_4_ + auVar26._8_4_ * fVar2;
          auVar28._12_4_ = fVar24 * auVar6._12_4_ + auVar26._12_4_ * fVar2;
          auVar28._16_4_ = fVar24 * auVar6._16_4_ + auVar26._16_4_ * fVar2;
          auVar28._20_4_ = fVar24 * auVar6._20_4_ + auVar26._20_4_ * fVar2;
          auVar28._24_4_ = fVar24 * auVar6._24_4_ + auVar26._24_4_ * fVar2;
          auVar28._28_4_ = fVar24 * auVar6._28_4_ + auVar26._28_4_ * fVar2;
          *(undefined1 (*) [32])(puVar1 + uVar16) = auVar28;
          uVar16 = uVar16 + 8;
          auVar26._0_4_ = auVar7._0_4_ + iVar9;
          auVar26._4_4_ = auVar7._4_4_ + iVar9;
          auVar26._8_4_ = auVar7._8_4_ + iVar9;
          auVar26._12_4_ = auVar7._12_4_ + iVar9;
          auVar26._16_4_ = auVar7._16_4_ + iVar9;
          auVar26._20_4_ = auVar7._20_4_ + iVar9;
          auVar26._24_4_ = auVar7._24_4_ + iVar9;
          auVar26._28_4_ = auVar7._28_4_ + iVar9;
          auVar7 = auVar26;
        } while (uVar15 != uVar16);
        if ((uVar10 & 0x7ffffff8) == uVar10) goto LAB_1800035b0;
      }
      uVar16 = uVar15;
      if ((uVar10 & 1) != 0) {
        fVar24 = (float)(int)uVar15 / fVar23;
        puVar1[uVar15] =
             (fVar8 - fVar24) * (float)param_2[lVar13 + -1] + fVar24 * (float)param_2[lVar13];
        uVar16 = uVar15 | 1;
      }
      if (uVar15 != uVar11 - 1) {
        do {
          fVar24 = (float)(int)uVar16 / fVar23;
          *(float *)(*param_1 + uVar16 * 4 + lVar12 * 4) =
               (fVar8 - fVar24) * (float)param_2[lVar13 + -1] + fVar24 * (float)param_2[lVar13];
          fVar24 = (float)((int)uVar16 + 1) / fVar23;
          *(float *)(*param_1 + uVar16 * 4 + lVar12 * 4 + 4) =
               (fVar8 - fVar24) * (float)param_2[lVar13 + -1] + fVar24 * (float)param_2[lVar13];
          uVar16 = uVar16 + 2;
        } while (uVar16 != uVar11);
      }
    }
LAB_1800035b0:
    lVar13 = lVar13 + 1;
    iVar14 = iVar5;
    if (lVar13 == 0x20) {
      uVar3 = *param_2;
      *(undefined4 *)*param_1 = uVar3;
      *(undefined4 *)(*param_1 + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2c] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x2c] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2c] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x2c] + 0x1c) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)param_1[0x2d] = uVar3;
      *(undefined4 *)(param_1[0x2d] + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2d] + 8) = uVar3;
      *(undefined4 *)(param_1[0x2d] + 0xc) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2d] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x2d] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2d] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x2d] + 0x1c) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)param_1[0x2e] = uVar3;
      *(undefined4 *)(param_1[0x2e] + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2e] + 8) = uVar3;
      *(undefined4 *)(param_1[0x2e] + 0xc) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2e] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x2e] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2e] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x2e] + 0x1c) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)param_1[0x2f] = uVar3;
      *(undefined4 *)(param_1[0x2f] + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2f] + 8) = uVar3;
      *(undefined4 *)(param_1[0x2f] + 0xc) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2f] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x2f] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x2f] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x2f] + 0x1c) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)param_1[0x30] = uVar3;
      *(undefined4 *)(param_1[0x30] + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x30] + 8) = uVar3;
      *(undefined4 *)(param_1[0x30] + 0xc) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x30] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x30] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x30] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x30] + 0x1c) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)param_1[0x31] = uVar3;
      *(undefined4 *)(param_1[0x31] + 4) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x31] + 8) = uVar3;
      *(undefined4 *)(param_1[0x31] + 0xc) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x31] + 0x10) = uVar3;
      *(undefined4 *)(param_1[0x31] + 0x14) = uVar3;
      uVar3 = param_2[0x1f];
      *(undefined4 *)(param_1[0x31] + 0x18) = uVar3;
      *(undefined4 *)(param_1[0x31] + 0x1c) = uVar3;
      return;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 180003930
   NAME : rnnoise_process_frame
   SIG  : undefined rnnoise_process_frame(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnnoise_process_frame(longlong param_1,float *param_2,longlong param_3)

{
  float fVar1;
  undefined8 uVar2;
  float fVar3;
  undefined1 auVar4 [16];
  undefined1 auVar5 [16];
  float fVar6;
  float fVar7;
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
  float fVar18;
  float fVar19;
  float fVar20;
  float fVar21;
  float fVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  undefined1 auVar33 [16];
  undefined1 auVar34 [16];
  undefined1 auVar35 [16];
  undefined1 auVar36 [16];
  undefined1 auVar37 [16];
  undefined1 auVar38 [16];
  double dVar39;
  double dVar40;
  double dVar41;
  int iVar42;
  uint *puVar43;
  float *pfVar44;
  longlong lVar45;
  longlong lVar46;
  undefined1 auVar47 [16];
  undefined1 auVar48 [32];
  undefined1 auVar49 [32];
  undefined1 auVar50 [32];
  undefined1 auVar51 [32];
  undefined1 auVar52 [32];
  undefined1 auVar53 [32];
  undefined1 auVar54 [32];
  undefined1 auVar55 [32];
  float fVar56;
  undefined1 auVar57 [32];
  undefined1 auVar58 [32];
  undefined1 auVar59 [32];
  undefined1 auVar60 [32];
  undefined1 auVar61 [32];
  undefined1 auVar62 [32];
  float fVar63;
  double dVar64;
  undefined1 auVar65 [32];
  undefined1 auVar66 [32];
  undefined1 auVar67 [32];
  undefined1 auVar68 [32];
  double dVar69;
  undefined1 auVar70 [32];
  undefined1 auVar71 [32];
  undefined1 auVar72 [32];
  undefined1 auVar73 [32];
  undefined1 auVar74 [32];
  undefined1 auVar75 [32];
  undefined1 auVar76 [32];
  undefined1 auStack_7be8 [32];
  undefined8 *local_7bc8;
  undefined8 *local_7bc0;
  undefined1 *local_7bb8;
  float *local_7bb0;
  undefined4 local_7b9c;
  float local_7b98 [484];
  undefined1 local_7408 [32];
  undefined1 local_73e8 [32];
  undefined1 local_73c8 [32];
  undefined1 local_73a8 [32];
  undefined1 local_7388 [272];
  undefined8 local_7278;
  undefined8 uStack_7270;
  undefined8 uStack_7268;
  undefined8 uStack_7260;
  undefined8 local_7258;
  undefined8 uStack_7250;
  undefined8 uStack_7248;
  undefined8 uStack_7240;
  undefined8 local_7238;
  undefined8 uStack_7230;
  undefined8 uStack_7228;
  undefined8 uStack_7220;
  undefined8 local_7218;
  undefined8 uStack_7210;
  undefined8 uStack_7208;
  undefined8 uStack_7200;
  undefined8 local_71f8;
  undefined8 uStack_71f0;
  undefined8 uStack_71e8;
  undefined8 uStack_71e0;
  undefined8 local_71d8;
  undefined8 uStack_71d0;
  undefined8 uStack_71c8;
  undefined8 uStack_71c0;
  undefined8 local_71b8;
  undefined8 uStack_71b0;
  undefined8 uStack_71a8;
  undefined8 uStack_71a0;
  undefined8 local_7198;
  undefined8 uStack_7190;
  undefined8 uStack_7188;
  undefined8 uStack_7180;
  undefined8 local_7178;
  undefined8 uStack_7170;
  undefined8 uStack_7168;
  undefined8 uStack_7160;
  undefined8 local_7158;
  undefined8 uStack_7150;
  undefined8 uStack_7148;
  undefined8 uStack_7140;
  undefined8 local_7138;
  undefined8 uStack_7130;
  undefined8 uStack_7128;
  undefined8 uStack_7120;
  undefined8 local_7118;
  undefined8 uStack_7110;
  undefined8 uStack_7108;
  undefined8 uStack_7100;
  float local_70f8 [480];
  undefined1 local_6978 [3856];
  undefined1 local_5a68 [3852];
  float afStack_4b5c [481];
  undefined1 local_43d8 [1892];
  float local_3c74;
  float local_3c70;
  float local_3c6c;
  float local_3c68;
  float local_3c64;
  float local_3c60;
  float local_3c5c;
  float local_3c58 [1920];
  uint local_1e58 [956];
  uint local_f68 [966];
  ulonglong local_50;
  undefined8 uStack_28;
  
                    /* 0x3930  40  rnnoise_process_frame */
  uStack_28 = 0x18000393f;
  local_50 = DAT_180580000 ^ (ulonglong)auStack_7be8;
  lVar46 = 0;
  memset(local_7b98,0,0x784);
  dVar41 = DAT_18000f118;
  dVar40 = DAT_18000f110;
  dVar39 = DAT_18000f108;
  local_7b98[0] = 1.0;
  local_7b9c = 0;
  fVar56 = *(float *)(param_1 + 0x4790);
  fVar63 = *(float *)(param_1 + 0x4794);
  do {
    fVar1 = *(float *)(param_3 + lVar46 * 4);
    fVar3 = fVar1 + fVar56;
    dVar64 = (double)fVar1;
    dVar69 = (double)fVar3;
    fVar56 = (float)(dVar64 * dVar40 + dVar69 * dVar39 + (double)fVar63);
    *(float *)(param_1 + 0x4790) = fVar56;
    fVar63 = (float)(dVar64 + dVar69 * dVar41);
    *(float *)(param_1 + 0x4794) = fVar63;
    local_70f8[lVar46] = fVar3;
    lVar46 = lVar46 + 1;
  } while (lVar46 != 0x1e0);
  local_7bb0 = local_70f8;
  local_7bc0 = &local_7278;
  local_7bc8 = &local_71f8;
  local_7bb8 = local_7388;
  iVar42 = rnn_compute_frame_features(param_1,local_5a68,local_6978,&local_7178);
  if (iVar42 == 0) {
    local_7bc0 = (undefined8 *)CONCAT44(local_7bc0._4_4_,*(undefined4 *)(param_1 + 0x280));
    local_7bc8 = (undefined8 *)local_7388;
    compute_rnn(param_1,param_1 + 0x4818,local_7408,&local_7b9c);
    local_7bc8 = (undefined8 *)(param_1 + 0x7f30);
    local_7bc0 = (undefined8 *)local_7408;
    rnn_pitch_filter(param_1 + 0x6020,param_1 + 0x6f28,param_1 + 0x7e30,param_1 + 0x7eb0);
    fVar56 = DAT_18000f120;
    dVar39 = DAT_18000f0a0;
    auVar53._0_4_ = DAT_18000f120 * *(float *)(param_1 + 0x4798);
    auVar53._4_4_ = DAT_18000f120 * *(float *)(param_1 + 0x479c);
    auVar53._8_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47a0);
    auVar53._12_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47a4);
    auVar53._16_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47a8);
    auVar53._20_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47ac);
    auVar53._24_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47b0);
    auVar53._28_4_ = DAT_18000f120 * *(float *)(param_1 + 0x47b4);
    local_7408 = vmaxps_avx(local_7408,auVar53);
    auVar50 = vcvtps2pd_avx(local_7408._0_16_);
    auVar53 = vcvtps2pd_avx(local_7408._16_16_);
    auVar48 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e40));
    auVar51 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e30));
    auVar54._0_8_ = (auVar51._0_8_ + DAT_18000f0a0) * auVar50._0_8_;
    auVar54._8_8_ = (auVar51._8_8_ + DAT_18000f0a0) * auVar50._8_8_;
    auVar54._16_8_ = (auVar51._16_8_ + DAT_18000f0a0) * auVar50._16_8_;
    auVar54._24_8_ = (auVar51._24_8_ + DAT_18000f0a0) * auVar50._24_8_;
    auVar57._0_8_ = (auVar48._0_8_ + DAT_18000f0a0) * auVar53._0_8_;
    auVar57._8_8_ = (auVar48._8_8_ + DAT_18000f0a0) * auVar53._8_8_;
    auVar57._16_8_ = (auVar48._16_8_ + DAT_18000f0a0) * auVar53._16_8_;
    auVar57._24_8_ = (auVar48._24_8_ + DAT_18000f0a0) * auVar53._24_8_;
    auVar4._8_8_ = uStack_7170;
    auVar4._0_8_ = local_7178;
    auVar50 = vcvtps2pd_avx(auVar4);
    auVar5._8_8_ = uStack_7160;
    auVar5._0_8_ = uStack_7168;
    auVar53 = vcvtps2pd_avx(auVar5);
    auVar70._0_8_ = auVar53._0_8_ + DAT_18000f0a0;
    auVar70._8_8_ = auVar53._8_8_ + DAT_18000f0a0;
    auVar70._16_8_ = auVar53._16_8_ + DAT_18000f0a0;
    auVar70._24_8_ = auVar53._24_8_ + DAT_18000f0a0;
    auVar53 = vdivpd_avx(auVar57,auVar70);
    auVar65._0_8_ = auVar50._0_8_ + DAT_18000f0a0;
    auVar65._8_8_ = auVar50._8_8_ + DAT_18000f0a0;
    auVar65._16_8_ = auVar50._16_8_ + DAT_18000f0a0;
    auVar65._24_8_ = auVar50._24_8_ + DAT_18000f0a0;
    auVar50 = vdivpd_avx(auVar54,auVar65);
    auVar55._8_8_ = DAT_18000f128;
    auVar55._0_8_ = DAT_18000f128;
    auVar55._16_8_ = DAT_18000f128;
    auVar55._24_8_ = DAT_18000f128;
    auVar53 = vminpd_avx(auVar55,auVar53);
    auVar50 = vminpd_avx(auVar55,auVar50);
    auVar4 = vcvtpd2ps_avx(auVar50);
    auVar5 = vcvtpd2ps_avx(auVar53);
    *(undefined1 (*) [16])(param_1 + 0x47a8) = auVar5;
    *(undefined1 (*) [16])(param_1 + 0x4798) = auVar4;
    auVar58._0_4_ = fVar56 * *(float *)(param_1 + 0x47b8);
    auVar58._4_4_ = fVar56 * *(float *)(param_1 + 0x47bc);
    auVar58._8_4_ = fVar56 * *(float *)(param_1 + 0x47c0);
    auVar58._12_4_ = fVar56 * *(float *)(param_1 + 0x47c4);
    auVar58._16_4_ = fVar56 * *(float *)(param_1 + 0x47c8);
    auVar58._20_4_ = fVar56 * *(float *)(param_1 + 0x47cc);
    auVar58._24_4_ = fVar56 * *(float *)(param_1 + 0x47d0);
    auVar58._28_4_ = fVar56 * *(float *)(param_1 + 0x47d4);
    local_73e8 = vmaxps_avx(local_73e8,auVar58);
    auVar50 = vcvtps2pd_avx(local_73e8._0_16_);
    auVar53 = vcvtps2pd_avx(local_73e8._16_16_);
    auVar48 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e60));
    auVar51 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e50));
    auVar66._0_8_ = (auVar51._0_8_ + dVar39) * auVar50._0_8_;
    auVar66._8_8_ = (auVar51._8_8_ + dVar39) * auVar50._8_8_;
    auVar66._16_8_ = (auVar51._16_8_ + dVar39) * auVar50._16_8_;
    auVar66._24_8_ = (auVar51._24_8_ + dVar39) * auVar50._24_8_;
    auVar59._0_8_ = (auVar48._0_8_ + dVar39) * auVar53._0_8_;
    auVar59._8_8_ = (auVar48._8_8_ + dVar39) * auVar53._8_8_;
    auVar59._16_8_ = (auVar48._16_8_ + dVar39) * auVar53._16_8_;
    auVar59._24_8_ = (auVar48._24_8_ + dVar39) * auVar53._24_8_;
    auVar33._8_8_ = uStack_7150;
    auVar33._0_8_ = local_7158;
    auVar50 = vcvtps2pd_avx(auVar33);
    auVar34._8_8_ = uStack_7140;
    auVar34._0_8_ = uStack_7148;
    auVar53 = vcvtps2pd_avx(auVar34);
    auVar75._0_8_ = auVar53._0_8_ + dVar39;
    auVar75._8_8_ = auVar53._8_8_ + dVar39;
    auVar75._16_8_ = auVar53._16_8_ + dVar39;
    auVar75._24_8_ = auVar53._24_8_ + dVar39;
    auVar53 = vdivpd_avx(auVar59,auVar75);
    auVar71._0_8_ = auVar50._0_8_ + dVar39;
    auVar71._8_8_ = auVar50._8_8_ + dVar39;
    auVar71._16_8_ = auVar50._16_8_ + dVar39;
    auVar71._24_8_ = auVar50._24_8_ + dVar39;
    auVar50 = vdivpd_avx(auVar66,auVar71);
    auVar53 = vminpd_avx(auVar55,auVar53);
    auVar50 = vminpd_avx(auVar55,auVar50);
    auVar4 = vcvtpd2ps_avx(auVar50);
    auVar5 = vcvtpd2ps_avx(auVar53);
    *(undefined1 (*) [16])(param_1 + 0x47c8) = auVar5;
    *(undefined1 (*) [16])(param_1 + 0x47b8) = auVar4;
    auVar60._0_4_ = fVar56 * *(float *)(param_1 + 0x47d8);
    auVar60._4_4_ = fVar56 * *(float *)(param_1 + 0x47dc);
    auVar60._8_4_ = fVar56 * *(float *)(param_1 + 0x47e0);
    auVar60._12_4_ = fVar56 * *(float *)(param_1 + 0x47e4);
    auVar60._16_4_ = fVar56 * *(float *)(param_1 + 0x47e8);
    auVar60._20_4_ = fVar56 * *(float *)(param_1 + 0x47ec);
    auVar60._24_4_ = fVar56 * *(float *)(param_1 + 0x47f0);
    auVar60._28_4_ = fVar56 * *(float *)(param_1 + 0x47f4);
    local_73c8 = vmaxps_avx(local_73c8,auVar60);
    auVar50 = vcvtps2pd_avx(local_73c8._0_16_);
    auVar53 = vcvtps2pd_avx(local_73c8._16_16_);
    auVar48 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e80));
    auVar51 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e70));
    auVar67._0_8_ = (auVar51._0_8_ + dVar39) * auVar50._0_8_;
    auVar67._8_8_ = (auVar51._8_8_ + dVar39) * auVar50._8_8_;
    auVar67._16_8_ = (auVar51._16_8_ + dVar39) * auVar50._16_8_;
    auVar67._24_8_ = (auVar51._24_8_ + dVar39) * auVar50._24_8_;
    auVar61._0_8_ = (auVar48._0_8_ + dVar39) * auVar53._0_8_;
    auVar61._8_8_ = (auVar48._8_8_ + dVar39) * auVar53._8_8_;
    auVar61._16_8_ = (auVar48._16_8_ + dVar39) * auVar53._16_8_;
    auVar61._24_8_ = (auVar48._24_8_ + dVar39) * auVar53._24_8_;
    auVar35._8_8_ = uStack_7130;
    auVar35._0_8_ = local_7138;
    auVar50 = vcvtps2pd_avx(auVar35);
    auVar36._8_8_ = uStack_7120;
    auVar36._0_8_ = uStack_7128;
    auVar53 = vcvtps2pd_avx(auVar36);
    auVar76._0_8_ = auVar53._0_8_ + dVar39;
    auVar76._8_8_ = auVar53._8_8_ + dVar39;
    auVar76._16_8_ = auVar53._16_8_ + dVar39;
    auVar76._24_8_ = auVar53._24_8_ + dVar39;
    auVar53 = vdivpd_avx(auVar61,auVar76);
    auVar72._0_8_ = auVar50._0_8_ + dVar39;
    auVar72._8_8_ = auVar50._8_8_ + dVar39;
    auVar72._16_8_ = auVar50._16_8_ + dVar39;
    auVar72._24_8_ = auVar50._24_8_ + dVar39;
    auVar50 = vdivpd_avx(auVar67,auVar72);
    auVar53 = vminpd_avx(auVar55,auVar53);
    auVar50 = vminpd_avx(auVar55,auVar50);
    auVar4 = vcvtpd2ps_avx(auVar50);
    auVar5 = vcvtpd2ps_avx(auVar53);
    *(undefined1 (*) [16])(param_1 + 0x47e8) = auVar5;
    *(undefined1 (*) [16])(param_1 + 0x47d8) = auVar4;
    auVar50._0_4_ = fVar56 * *(float *)(param_1 + 0x47f8);
    auVar50._4_4_ = fVar56 * *(float *)(param_1 + 0x47fc);
    auVar50._8_4_ = fVar56 * *(float *)(param_1 + 0x4800);
    auVar50._12_4_ = fVar56 * *(float *)(param_1 + 0x4804);
    auVar50._16_4_ = fVar56 * *(float *)(param_1 + 0x4808);
    auVar50._20_4_ = fVar56 * *(float *)(param_1 + 0x480c);
    auVar50._24_4_ = fVar56 * *(float *)(param_1 + 0x4810);
    auVar50._28_4_ = fVar56 * *(float *)(param_1 + 0x4814);
    local_73a8 = vmaxps_avx(local_73a8,auVar50);
    auVar50 = vcvtps2pd_avx(local_73a8._0_16_);
    auVar53 = vcvtps2pd_avx(local_73a8._16_16_);
    auVar48 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7ea0));
    auVar51 = vcvtps2pd_avx(*(undefined1 (*) [16])(param_1 + 0x7e90));
    auVar62._0_8_ = (auVar51._0_8_ + dVar39) * auVar50._0_8_;
    auVar62._8_8_ = (auVar51._8_8_ + dVar39) * auVar50._8_8_;
    auVar62._16_8_ = (auVar51._16_8_ + dVar39) * auVar50._16_8_;
    auVar62._24_8_ = (auVar51._24_8_ + dVar39) * auVar50._24_8_;
    auVar51._0_8_ = (auVar48._0_8_ + dVar39) * auVar53._0_8_;
    auVar51._8_8_ = (auVar48._8_8_ + dVar39) * auVar53._8_8_;
    auVar51._16_8_ = (auVar48._16_8_ + dVar39) * auVar53._16_8_;
    auVar51._24_8_ = (auVar48._24_8_ + dVar39) * auVar53._24_8_;
    auVar37._8_8_ = uStack_7110;
    auVar37._0_8_ = local_7118;
    auVar50 = vcvtps2pd_avx(auVar37);
    auVar38._8_8_ = uStack_7100;
    auVar38._0_8_ = uStack_7108;
    auVar53 = vcvtps2pd_avx(auVar38);
    auVar73._0_8_ = auVar53._0_8_ + dVar39;
    auVar73._8_8_ = auVar53._8_8_ + dVar39;
    auVar73._16_8_ = auVar53._16_8_ + dVar39;
    auVar73._24_8_ = auVar53._24_8_ + dVar39;
    auVar53 = vdivpd_avx(auVar51,auVar73);
    auVar48._0_8_ = auVar50._0_8_ + dVar39;
    auVar48._8_8_ = auVar50._8_8_ + dVar39;
    auVar48._16_8_ = auVar50._16_8_ + dVar39;
    auVar48._24_8_ = auVar50._24_8_ + dVar39;
    auVar50 = vdivpd_avx(auVar62,auVar48);
    auVar53 = vminpd_avx(auVar55,auVar53);
    auVar50 = vminpd_avx(auVar55,auVar50);
    auVar4 = vcvtpd2ps_avx(auVar50);
    auVar5 = vcvtpd2ps_avx(auVar53);
    *(undefined1 (*) [16])(param_1 + 0x4808) = auVar5;
    *(undefined1 (*) [16])(param_1 + 0x47f8) = auVar4;
    FUN_180003490(local_7b98,local_7408);
    lVar46 = 0;
    do {
      auVar50 = *(undefined1 (*) [32])(param_1 + 0x6020 + lVar46 * 8);
      auVar53 = *(undefined1 (*) [32])(param_1 + 0x6040 + lVar46 * 8);
      auVar48 = *(undefined1 (*) [32])(param_1 + 0x6060 + lVar46 * 8);
      auVar51 = *(undefined1 (*) [32])(param_1 + 0x6080 + lVar46 * 8);
      auVar54 = vshufps_avx(auVar50,auVar53,0x88);
      auVar55 = vpermpd_avx2(auVar54,0xd8);
      auVar54 = vshufps_avx(auVar48,auVar51,0x88);
      auVar54 = vpermpd_avx2(auVar54,0xd8);
      auVar50 = vshufps_avx(auVar50,auVar53,0xdd);
      auVar53 = vpermpd_avx2(auVar50,0xd8);
      auVar50 = vshufps_avx(auVar48,auVar51,0xdd);
      auVar50 = vpermpd_avx2(auVar50,0xd8);
      auVar68._0_4_ = local_7b98[lVar46] * auVar55._0_4_;
      auVar68._4_4_ = local_7b98[lVar46 + 1] * auVar55._4_4_;
      auVar68._8_4_ = local_7b98[lVar46 + 2] * auVar55._8_4_;
      auVar68._12_4_ = local_7b98[lVar46 + 3] * auVar55._12_4_;
      auVar68._16_4_ = local_7b98[lVar46 + 4] * auVar55._16_4_;
      auVar68._20_4_ = local_7b98[lVar46 + 5] * auVar55._20_4_;
      auVar68._24_4_ = local_7b98[lVar46 + 6] * auVar55._24_4_;
      auVar68._28_4_ = local_7b98[lVar46 + 7] * auVar55._28_4_;
      auVar74._0_4_ = local_7b98[lVar46 + 8] * auVar54._0_4_;
      auVar74._4_4_ = local_7b98[lVar46 + 9] * auVar54._4_4_;
      auVar74._8_4_ = local_7b98[lVar46 + 10] * auVar54._8_4_;
      auVar74._12_4_ = local_7b98[lVar46 + 0xb] * auVar54._12_4_;
      auVar74._16_4_ = local_7b98[lVar46 + 0xc] * auVar54._16_4_;
      auVar74._20_4_ = local_7b98[lVar46 + 0xd] * auVar54._20_4_;
      auVar74._24_4_ = local_7b98[lVar46 + 0xe] * auVar54._24_4_;
      auVar74._28_4_ = local_7b98[lVar46 + 0xf] * auVar54._28_4_;
      auVar49._0_4_ = local_7b98[lVar46] * auVar53._0_4_;
      auVar49._4_4_ = local_7b98[lVar46 + 1] * auVar53._4_4_;
      auVar49._8_4_ = local_7b98[lVar46 + 2] * auVar53._8_4_;
      auVar49._12_4_ = local_7b98[lVar46 + 3] * auVar53._12_4_;
      auVar49._16_4_ = local_7b98[lVar46 + 4] * auVar53._16_4_;
      auVar49._20_4_ = local_7b98[lVar46 + 5] * auVar53._20_4_;
      auVar49._24_4_ = local_7b98[lVar46 + 6] * auVar53._24_4_;
      auVar49._28_4_ = local_7b98[lVar46 + 7] * auVar53._28_4_;
      auVar52._0_4_ = local_7b98[lVar46 + 8] * auVar50._0_4_;
      auVar52._4_4_ = local_7b98[lVar46 + 9] * auVar50._4_4_;
      auVar52._8_4_ = local_7b98[lVar46 + 10] * auVar50._8_4_;
      auVar52._12_4_ = local_7b98[lVar46 + 0xb] * auVar50._12_4_;
      auVar52._16_4_ = local_7b98[lVar46 + 0xc] * auVar50._16_4_;
      auVar52._20_4_ = local_7b98[lVar46 + 0xd] * auVar50._20_4_;
      auVar52._24_4_ = local_7b98[lVar46 + 0xe] * auVar50._24_4_;
      auVar52._28_4_ = local_7b98[lVar46 + 0xf] * auVar50._28_4_;
      auVar50 = vunpckhps_avx(auVar68,auVar49);
      auVar53 = vunpcklps_avx(auVar68,auVar49);
      auVar48 = vperm2f128_avx(auVar53,auVar50,0x31);
      auVar50 = vperm2f128_avx(auVar53,auVar50,0x20);
      *(undefined1 (*) [32])(param_1 + 0x6020 + lVar46 * 8) = auVar50;
      *(undefined1 (*) [32])(param_1 + 0x6040 + lVar46 * 8) = auVar48;
      auVar50 = vunpckhps_avx(auVar74,auVar52);
      auVar53 = vunpcklps_avx(auVar74,auVar52);
      auVar48 = vperm2f128_avx(auVar53,auVar50,0x31);
      auVar50 = vperm2f128_avx(auVar53,auVar50,0x20);
      *(undefined1 (*) [32])(param_1 + 0x6060 + lVar46 * 8) = auVar50;
      *(undefined1 (*) [32])(param_1 + 0x6080 + lVar46 * 8) = auVar48;
      lVar46 = lVar46 + 0x10;
    } while (lVar46 != 0x1e0);
    auVar47._0_4_ = local_7b98[0x1e0] * (float)*(undefined8 *)(param_1 + 0x6f20);
    auVar47._4_4_ =
         local_7b98[0x1e0] * (float)((ulonglong)*(undefined8 *)(param_1 + 0x6f20) >> 0x20);
    auVar47._8_4_ = local_7b98[0x1e0] * 0.0;
    auVar47._12_4_ = local_7b98[0x1e0] * 0.0;
    uVar2 = vmovlps_avx(auVar47);
    *(undefined8 *)(param_1 + 0x6f20) = uVar2;
  }
  memcpy(local_1e58,(void *)(param_1 + 0x6020),0xf08);
  lVar46 = 0x1e1;
  puVar43 = local_f68 + 3;
  while( true ) {
    local_1e58[lVar46 * 2] = puVar43[-1];
    local_1e58[lVar46 * 2 + 1] = *puVar43 ^ DAT_18000f000;
    if (lVar46 == 0x3bf) break;
    local_1e58[lVar46 * 2 + 2] = puVar43[-3];
    local_1e58[lVar46 * 2 + 3] = puVar43[-2] ^ DAT_18000f000;
    lVar46 = lVar46 + 2;
    puVar43 = puVar43 + -4;
  }
  rnn_fft_c(&DAT_18057ce90,local_1e58,local_3c58);
  afStack_4b5c[1] = local_3c58[0] * DAT_18000f130;
  lVar46 = 0xee0;
  pfVar44 = afStack_4b5c + 2;
  do {
    auVar4 = vinsertps_avx(ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x18)),
                           ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x10)),0x10);
    auVar4 = vinsertps_avx(auVar4,ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 8)),0x20);
    auVar4 = vinsertps_avx(auVar4,ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2)),0x30);
    auVar5 = vinsertps_avx(ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x38)),
                           ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x30)),0x10);
    auVar5 = vinsertps_avx(auVar5,ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x28)),0x20)
    ;
    auVar5 = vinsertps_avx(auVar5,ZEXT416(*(uint *)((longlong)local_3c58 + lVar46 * 2 + 0x20)),0x30)
    ;
    *pfVar44 = auVar5._0_4_ * DAT_18000f130;
    pfVar44[1] = auVar5._4_4_ * DAT_18000f130;
    pfVar44[2] = auVar5._8_4_ * DAT_18000f130;
    pfVar44[3] = auVar5._12_4_ * DAT_18000f130;
    pfVar44[4] = auVar4._0_4_ * DAT_18000f130;
    pfVar44[5] = auVar4._4_4_ * DAT_18000f130;
    pfVar44[6] = auVar4._8_4_ * DAT_18000f130;
    pfVar44[7] = auVar4._12_4_ * DAT_18000f130;
    pfVar44 = pfVar44 + 8;
    lVar46 = lVar46 + -0x20;
  } while (lVar46 != 0);
  local_3c74 = DAT_18000f130 * local_3c58[0xe];
  local_3c70 = DAT_18000f130 * local_3c58[0xc];
  local_3c6c = DAT_18000f130 * local_3c58[10];
  local_3c68 = DAT_18000f130 * local_3c58[8];
  local_3c64 = DAT_18000f130 * local_3c58[6];
  local_3c60 = DAT_18000f130 * local_3c58[4];
  local_3c5c = DAT_18000f130 * local_3c58[2];
  lVar46 = 1;
  lVar45 = 0xefc;
  do {
    fVar56 = *(float *)(&UNK_18057cedc + lVar46 * 4);
    afStack_4b5c[lVar46] = fVar56 * afStack_4b5c[lVar46];
    fVar63 = (float)(&DAT_18057cee0)[lVar46];
    *(float *)((longlong)afStack_4b5c + lVar45 + 4) =
         fVar56 * *(float *)((longlong)afStack_4b5c + lVar45 + 4);
    afStack_4b5c[lVar46 + 1] = fVar63 * afStack_4b5c[lVar46 + 1];
    *(float *)((longlong)afStack_4b5c + lVar45) =
         fVar63 * *(float *)((longlong)afStack_4b5c + lVar45);
    lVar46 = lVar46 + 2;
    lVar45 = lVar45 + -8;
  } while (lVar46 != 0x1e1);
  if ((ulonglong)((longlong)param_2 + (-0xa08 - param_1)) < 0x80) {
    lVar46 = 0;
    do {
      param_2[lVar46] = afStack_4b5c[lVar46 + 1] + *(float *)(param_1 + 0xa08 + lVar46 * 4);
      param_2[lVar46 + 1] = afStack_4b5c[lVar46 + 2] + *(float *)(param_1 + 0xa0c + lVar46 * 4);
      param_2[lVar46 + 2] = afStack_4b5c[lVar46 + 3] + *(float *)(param_1 + 0xa10 + lVar46 * 4);
      param_2[lVar46 + 3] = afStack_4b5c[lVar46 + 4] + *(float *)(param_1 + 0xa14 + lVar46 * 4);
      lVar46 = lVar46 + 4;
    } while (lVar46 != 0x1e0);
  }
  else {
    fVar56 = *(float *)(param_1 + 0xa0c);
    fVar63 = *(float *)(param_1 + 0xa10);
    fVar1 = *(float *)(param_1 + 0xa14);
    fVar3 = *(float *)(param_1 + 0xa18);
    fVar6 = *(float *)(param_1 + 0xa1c);
    fVar7 = *(float *)(param_1 + 0xa20);
    fVar8 = *(float *)(param_1 + 0xa24);
    fVar9 = *(float *)(param_1 + 0xa28);
    fVar10 = *(float *)(param_1 + 0xa2c);
    fVar11 = *(float *)(param_1 + 0xa30);
    fVar12 = *(float *)(param_1 + 0xa34);
    fVar13 = *(float *)(param_1 + 0xa38);
    fVar14 = *(float *)(param_1 + 0xa3c);
    fVar15 = *(float *)(param_1 + 0xa40);
    fVar16 = *(float *)(param_1 + 0xa44);
    fVar17 = *(float *)(param_1 + 0xa48);
    fVar18 = *(float *)(param_1 + 0xa4c);
    fVar19 = *(float *)(param_1 + 0xa50);
    fVar20 = *(float *)(param_1 + 0xa54);
    fVar21 = *(float *)(param_1 + 0xa58);
    fVar22 = *(float *)(param_1 + 0xa5c);
    fVar23 = *(float *)(param_1 + 0xa60);
    fVar24 = *(float *)(param_1 + 0xa64);
    fVar25 = *(float *)(param_1 + 0xa68);
    fVar26 = *(float *)(param_1 + 0xa6c);
    fVar27 = *(float *)(param_1 + 0xa70);
    fVar28 = *(float *)(param_1 + 0xa74);
    fVar29 = *(float *)(param_1 + 0xa78);
    fVar30 = *(float *)(param_1 + 0xa7c);
    fVar31 = *(float *)(param_1 + 0xa80);
    fVar32 = *(float *)(param_1 + 0xa84);
    *param_2 = afStack_4b5c[1] + *(float *)(param_1 + 0xa08);
    param_2[1] = afStack_4b5c[2] + fVar56;
    param_2[2] = afStack_4b5c[3] + fVar63;
    param_2[3] = afStack_4b5c[4] + fVar1;
    param_2[4] = afStack_4b5c[5] + fVar3;
    param_2[5] = afStack_4b5c[6] + fVar6;
    param_2[6] = afStack_4b5c[7] + fVar7;
    param_2[7] = afStack_4b5c[8] + fVar8;
    param_2[8] = afStack_4b5c[9] + fVar9;
    param_2[9] = afStack_4b5c[10] + fVar10;
    param_2[10] = afStack_4b5c[0xb] + fVar11;
    param_2[0xb] = afStack_4b5c[0xc] + fVar12;
    param_2[0xc] = afStack_4b5c[0xd] + fVar13;
    param_2[0xd] = afStack_4b5c[0xe] + fVar14;
    param_2[0xe] = afStack_4b5c[0xf] + fVar15;
    param_2[0xf] = afStack_4b5c[0x10] + fVar16;
    param_2[0x10] = afStack_4b5c[0x11] + fVar17;
    param_2[0x11] = afStack_4b5c[0x12] + fVar18;
    param_2[0x12] = afStack_4b5c[0x13] + fVar19;
    param_2[0x13] = afStack_4b5c[0x14] + fVar20;
    param_2[0x14] = afStack_4b5c[0x15] + fVar21;
    param_2[0x15] = afStack_4b5c[0x16] + fVar22;
    param_2[0x16] = afStack_4b5c[0x17] + fVar23;
    param_2[0x17] = afStack_4b5c[0x18] + fVar24;
    param_2[0x18] = afStack_4b5c[0x19] + fVar25;
    param_2[0x19] = afStack_4b5c[0x1a] + fVar26;
    param_2[0x1a] = afStack_4b5c[0x1b] + fVar27;
    param_2[0x1b] = afStack_4b5c[0x1c] + fVar28;
    param_2[0x1c] = afStack_4b5c[0x1d] + fVar29;
    param_2[0x1d] = afStack_4b5c[0x1e] + fVar30;
    param_2[0x1e] = afStack_4b5c[0x1f] + fVar31;
    param_2[0x1f] = afStack_4b5c[0x20] + fVar32;
    fVar56 = *(float *)(param_1 + 0xa8c);
    fVar63 = *(float *)(param_1 + 0xa90);
    fVar1 = *(float *)(param_1 + 0xa94);
    fVar3 = *(float *)(param_1 + 0xa98);
    fVar6 = *(float *)(param_1 + 0xa9c);
    fVar7 = *(float *)(param_1 + 0xaa0);
    fVar8 = *(float *)(param_1 + 0xaa4);
    fVar9 = *(float *)(param_1 + 0xaa8);
    fVar10 = *(float *)(param_1 + 0xaac);
    fVar11 = *(float *)(param_1 + 0xab0);
    fVar12 = *(float *)(param_1 + 0xab4);
    fVar13 = *(float *)(param_1 + 0xab8);
    fVar14 = *(float *)(param_1 + 0xabc);
    fVar15 = *(float *)(param_1 + 0xac0);
    fVar16 = *(float *)(param_1 + 0xac4);
    fVar17 = *(float *)(param_1 + 0xac8);
    fVar18 = *(float *)(param_1 + 0xacc);
    fVar19 = *(float *)(param_1 + 0xad0);
    fVar20 = *(float *)(param_1 + 0xad4);
    fVar21 = *(float *)(param_1 + 0xad8);
    fVar22 = *(float *)(param_1 + 0xadc);
    fVar23 = *(float *)(param_1 + 0xae0);
    fVar24 = *(float *)(param_1 + 0xae4);
    fVar25 = *(float *)(param_1 + 0xae8);
    fVar26 = *(float *)(param_1 + 0xaec);
    fVar27 = *(float *)(param_1 + 0xaf0);
    fVar28 = *(float *)(param_1 + 0xaf4);
    fVar29 = *(float *)(param_1 + 0xaf8);
    fVar30 = *(float *)(param_1 + 0xafc);
    fVar31 = *(float *)(param_1 + 0xb00);
    fVar32 = *(float *)(param_1 + 0xb04);
    param_2[0x20] = afStack_4b5c[0x21] + *(float *)(param_1 + 0xa88);
    param_2[0x21] = afStack_4b5c[0x22] + fVar56;
    param_2[0x22] = afStack_4b5c[0x23] + fVar63;
    param_2[0x23] = afStack_4b5c[0x24] + fVar1;
    param_2[0x24] = afStack_4b5c[0x25] + fVar3;
    param_2[0x25] = afStack_4b5c[0x26] + fVar6;
    param_2[0x26] = afStack_4b5c[0x27] + fVar7;
    param_2[0x27] = afStack_4b5c[0x28] + fVar8;
    param_2[0x28] = afStack_4b5c[0x29] + fVar9;
    param_2[0x29] = afStack_4b5c[0x2a] + fVar10;
    param_2[0x2a] = afStack_4b5c[0x2b] + fVar11;
    param_2[0x2b] = afStack_4b5c[0x2c] + fVar12;
    param_2[0x2c] = afStack_4b5c[0x2d] + fVar13;
    param_2[0x2d] = afStack_4b5c[0x2e] + fVar14;
    param_2[0x2e] = afStack_4b5c[0x2f] + fVar15;
    param_2[0x2f] = afStack_4b5c[0x30] + fVar16;
    param_2[0x30] = afStack_4b5c[0x31] + fVar17;
    param_2[0x31] = afStack_4b5c[0x32] + fVar18;
    param_2[0x32] = afStack_4b5c[0x33] + fVar19;
    param_2[0x33] = afStack_4b5c[0x34] + fVar20;
    param_2[0x34] = afStack_4b5c[0x35] + fVar21;
    param_2[0x35] = afStack_4b5c[0x36] + fVar22;
    param_2[0x36] = afStack_4b5c[0x37] + fVar23;
    param_2[0x37] = afStack_4b5c[0x38] + fVar24;
    param_2[0x38] = afStack_4b5c[0x39] + fVar25;
    param_2[0x39] = afStack_4b5c[0x3a] + fVar26;
    param_2[0x3a] = afStack_4b5c[0x3b] + fVar27;
    param_2[0x3b] = afStack_4b5c[0x3c] + fVar28;
    param_2[0x3c] = afStack_4b5c[0x3d] + fVar29;
    param_2[0x3d] = afStack_4b5c[0x3e] + fVar30;
    param_2[0x3e] = afStack_4b5c[0x3f] + fVar31;
    param_2[0x3f] = afStack_4b5c[0x40] + fVar32;
    fVar56 = *(float *)(param_1 + 0xb0c);
    fVar63 = *(float *)(param_1 + 0xb10);
    fVar1 = *(float *)(param_1 + 0xb14);
    fVar3 = *(float *)(param_1 + 0xb18);
    fVar6 = *(float *)(param_1 + 0xb1c);
    fVar7 = *(float *)(param_1 + 0xb20);
    fVar8 = *(float *)(param_1 + 0xb24);
    fVar9 = *(float *)(param_1 + 0xb28);
    fVar10 = *(float *)(param_1 + 0xb2c);
    fVar11 = *(float *)(param_1 + 0xb30);
    fVar12 = *(float *)(param_1 + 0xb34);
    fVar13 = *(float *)(param_1 + 0xb38);
    fVar14 = *(float *)(param_1 + 0xb3c);
    fVar15 = *(float *)(param_1 + 0xb40);
    fVar16 = *(float *)(param_1 + 0xb44);
    fVar17 = *(float *)(param_1 + 0xb48);
    fVar18 = *(float *)(param_1 + 0xb4c);
    fVar19 = *(float *)(param_1 + 0xb50);
    fVar20 = *(float *)(param_1 + 0xb54);
    fVar21 = *(float *)(param_1 + 0xb58);
    fVar22 = *(float *)(param_1 + 0xb5c);
    fVar23 = *(float *)(param_1 + 0xb60);
    fVar24 = *(float *)(param_1 + 0xb64);
    fVar25 = *(float *)(param_1 + 0xb68);
    fVar26 = *(float *)(param_1 + 0xb6c);
    fVar27 = *(float *)(param_1 + 0xb70);
    fVar28 = *(float *)(param_1 + 0xb74);
    fVar29 = *(float *)(param_1 + 0xb78);
    fVar30 = *(float *)(param_1 + 0xb7c);
    fVar31 = *(float *)(param_1 + 0xb80);
    fVar32 = *(float *)(param_1 + 0xb84);
    param_2[0x40] = afStack_4b5c[0x41] + *(float *)(param_1 + 0xb08);
    param_2[0x41] = afStack_4b5c[0x42] + fVar56;
    param_2[0x42] = afStack_4b5c[0x43] + fVar63;
    param_2[0x43] = afStack_4b5c[0x44] + fVar1;
    param_2[0x44] = afStack_4b5c[0x45] + fVar3;
    param_2[0x45] = afStack_4b5c[0x46] + fVar6;
    param_2[0x46] = afStack_4b5c[0x47] + fVar7;
    param_2[0x47] = afStack_4b5c[0x48] + fVar8;
    param_2[0x48] = afStack_4b5c[0x49] + fVar9;
    param_2[0x49] = afStack_4b5c[0x4a] + fVar10;
    param_2[0x4a] = afStack_4b5c[0x4b] + fVar11;
    param_2[0x4b] = afStack_4b5c[0x4c] + fVar12;
    param_2[0x4c] = afStack_4b5c[0x4d] + fVar13;
    param_2[0x4d] = afStack_4b5c[0x4e] + fVar14;
    param_2[0x4e] = afStack_4b5c[0x4f] + fVar15;
    param_2[0x4f] = afStack_4b5c[0x50] + fVar16;
    param_2[0x50] = afStack_4b5c[0x51] + fVar17;
    param_2[0x51] = afStack_4b5c[0x52] + fVar18;
    param_2[0x52] = afStack_4b5c[0x53] + fVar19;
    param_2[0x53] = afStack_4b5c[0x54] + fVar20;
    param_2[0x54] = afStack_4b5c[0x55] + fVar21;
    param_2[0x55] = afStack_4b5c[0x56] + fVar22;
    param_2[0x56] = afStack_4b5c[0x57] + fVar23;
    param_2[0x57] = afStack_4b5c[0x58] + fVar24;
    param_2[0x58] = afStack_4b5c[0x59] + fVar25;
    param_2[0x59] = afStack_4b5c[0x5a] + fVar26;
    param_2[0x5a] = afStack_4b5c[0x5b] + fVar27;
    param_2[0x5b] = afStack_4b5c[0x5c] + fVar28;
    param_2[0x5c] = afStack_4b5c[0x5d] + fVar29;
    param_2[0x5d] = afStack_4b5c[0x5e] + fVar30;
    param_2[0x5e] = afStack_4b5c[0x5f] + fVar31;
    param_2[0x5f] = afStack_4b5c[0x60] + fVar32;
    fVar56 = *(float *)(param_1 + 0xb8c);
    fVar63 = *(float *)(param_1 + 0xb90);
    fVar1 = *(float *)(param_1 + 0xb94);
    fVar3 = *(float *)(param_1 + 0xb98);
    fVar6 = *(float *)(param_1 + 0xb9c);
    fVar7 = *(float *)(param_1 + 0xba0);
    fVar8 = *(float *)(param_1 + 0xba4);
    fVar9 = *(float *)(param_1 + 0xba8);
    fVar10 = *(float *)(param_1 + 0xbac);
    fVar11 = *(float *)(param_1 + 0xbb0);
    fVar12 = *(float *)(param_1 + 0xbb4);
    fVar13 = *(float *)(param_1 + 3000);
    fVar14 = *(float *)(param_1 + 0xbbc);
    fVar15 = *(float *)(param_1 + 0xbc0);
    fVar16 = *(float *)(param_1 + 0xbc4);
    fVar17 = *(float *)(param_1 + 0xbc8);
    fVar18 = *(float *)(param_1 + 0xbcc);
    fVar19 = *(float *)(param_1 + 0xbd0);
    fVar20 = *(float *)(param_1 + 0xbd4);
    fVar21 = *(float *)(param_1 + 0xbd8);
    fVar22 = *(float *)(param_1 + 0xbdc);
    fVar23 = *(float *)(param_1 + 0xbe0);
    fVar24 = *(float *)(param_1 + 0xbe4);
    fVar25 = *(float *)(param_1 + 0xbe8);
    fVar26 = *(float *)(param_1 + 0xbec);
    fVar27 = *(float *)(param_1 + 0xbf0);
    fVar28 = *(float *)(param_1 + 0xbf4);
    fVar29 = *(float *)(param_1 + 0xbf8);
    fVar30 = *(float *)(param_1 + 0xbfc);
    fVar31 = *(float *)(param_1 + 0xc00);
    fVar32 = *(float *)(param_1 + 0xc04);
    param_2[0x60] = afStack_4b5c[0x61] + *(float *)(param_1 + 0xb88);
    param_2[0x61] = afStack_4b5c[0x62] + fVar56;
    param_2[0x62] = afStack_4b5c[99] + fVar63;
    param_2[99] = afStack_4b5c[100] + fVar1;
    param_2[100] = afStack_4b5c[0x65] + fVar3;
    param_2[0x65] = afStack_4b5c[0x66] + fVar6;
    param_2[0x66] = afStack_4b5c[0x67] + fVar7;
    param_2[0x67] = afStack_4b5c[0x68] + fVar8;
    param_2[0x68] = afStack_4b5c[0x69] + fVar9;
    param_2[0x69] = afStack_4b5c[0x6a] + fVar10;
    param_2[0x6a] = afStack_4b5c[0x6b] + fVar11;
    param_2[0x6b] = afStack_4b5c[0x6c] + fVar12;
    param_2[0x6c] = afStack_4b5c[0x6d] + fVar13;
    param_2[0x6d] = afStack_4b5c[0x6e] + fVar14;
    param_2[0x6e] = afStack_4b5c[0x6f] + fVar15;
    param_2[0x6f] = afStack_4b5c[0x70] + fVar16;
    param_2[0x70] = afStack_4b5c[0x71] + fVar17;
    param_2[0x71] = afStack_4b5c[0x72] + fVar18;
    param_2[0x72] = afStack_4b5c[0x73] + fVar19;
    param_2[0x73] = afStack_4b5c[0x74] + fVar20;
    param_2[0x74] = afStack_4b5c[0x75] + fVar21;
    param_2[0x75] = afStack_4b5c[0x76] + fVar22;
    param_2[0x76] = afStack_4b5c[0x77] + fVar23;
    param_2[0x77] = afStack_4b5c[0x78] + fVar24;
    param_2[0x78] = afStack_4b5c[0x79] + fVar25;
    param_2[0x79] = afStack_4b5c[0x7a] + fVar26;
    param_2[0x7a] = afStack_4b5c[0x7b] + fVar27;
    param_2[0x7b] = afStack_4b5c[0x7c] + fVar28;
    param_2[0x7c] = afStack_4b5c[0x7d] + fVar29;
    param_2[0x7d] = afStack_4b5c[0x7e] + fVar30;
    param_2[0x7e] = afStack_4b5c[0x7f] + fVar31;
    param_2[0x7f] = afStack_4b5c[0x80] + fVar32;
    fVar56 = *(float *)(param_1 + 0xc0c);
    fVar63 = *(float *)(param_1 + 0xc10);
    fVar1 = *(float *)(param_1 + 0xc14);
    fVar3 = *(float *)(param_1 + 0xc18);
    fVar6 = *(float *)(param_1 + 0xc1c);
    fVar7 = *(float *)(param_1 + 0xc20);
    fVar8 = *(float *)(param_1 + 0xc24);
    fVar9 = *(float *)(param_1 + 0xc28);
    fVar10 = *(float *)(param_1 + 0xc2c);
    fVar11 = *(float *)(param_1 + 0xc30);
    fVar12 = *(float *)(param_1 + 0xc34);
    fVar13 = *(float *)(param_1 + 0xc38);
    fVar14 = *(float *)(param_1 + 0xc3c);
    fVar15 = *(float *)(param_1 + 0xc40);
    fVar16 = *(float *)(param_1 + 0xc44);
    fVar17 = *(float *)(param_1 + 0xc48);
    fVar18 = *(float *)(param_1 + 0xc4c);
    fVar19 = *(float *)(param_1 + 0xc50);
    fVar20 = *(float *)(param_1 + 0xc54);
    fVar21 = *(float *)(param_1 + 0xc58);
    fVar22 = *(float *)(param_1 + 0xc5c);
    fVar23 = *(float *)(param_1 + 0xc60);
    fVar24 = *(float *)(param_1 + 0xc64);
    fVar25 = *(float *)(param_1 + 0xc68);
    fVar26 = *(float *)(param_1 + 0xc6c);
    fVar27 = *(float *)(param_1 + 0xc70);
    fVar28 = *(float *)(param_1 + 0xc74);
    fVar29 = *(float *)(param_1 + 0xc78);
    fVar30 = *(float *)(param_1 + 0xc7c);
    fVar31 = *(float *)(param_1 + 0xc80);
    fVar32 = *(float *)(param_1 + 0xc84);
    param_2[0x80] = afStack_4b5c[0x81] + *(float *)(param_1 + 0xc08);
    param_2[0x81] = afStack_4b5c[0x82] + fVar56;
    param_2[0x82] = afStack_4b5c[0x83] + fVar63;
    param_2[0x83] = afStack_4b5c[0x84] + fVar1;
    param_2[0x84] = afStack_4b5c[0x85] + fVar3;
    param_2[0x85] = afStack_4b5c[0x86] + fVar6;
    param_2[0x86] = afStack_4b5c[0x87] + fVar7;
    param_2[0x87] = afStack_4b5c[0x88] + fVar8;
    param_2[0x88] = afStack_4b5c[0x89] + fVar9;
    param_2[0x89] = afStack_4b5c[0x8a] + fVar10;
    param_2[0x8a] = afStack_4b5c[0x8b] + fVar11;
    param_2[0x8b] = afStack_4b5c[0x8c] + fVar12;
    param_2[0x8c] = afStack_4b5c[0x8d] + fVar13;
    param_2[0x8d] = afStack_4b5c[0x8e] + fVar14;
    param_2[0x8e] = afStack_4b5c[0x8f] + fVar15;
    param_2[0x8f] = afStack_4b5c[0x90] + fVar16;
    param_2[0x90] = afStack_4b5c[0x91] + fVar17;
    param_2[0x91] = afStack_4b5c[0x92] + fVar18;
    param_2[0x92] = afStack_4b5c[0x93] + fVar19;
    param_2[0x93] = afStack_4b5c[0x94] + fVar20;
    param_2[0x94] = afStack_4b5c[0x95] + fVar21;
    param_2[0x95] = afStack_4b5c[0x96] + fVar22;
    param_2[0x96] = afStack_4b5c[0x97] + fVar23;
    param_2[0x97] = afStack_4b5c[0x98] + fVar24;
    param_2[0x98] = afStack_4b5c[0x99] + fVar25;
    param_2[0x99] = afStack_4b5c[0x9a] + fVar26;
    param_2[0x9a] = afStack_4b5c[0x9b] + fVar27;
    param_2[0x9b] = afStack_4b5c[0x9c] + fVar28;
    param_2[0x9c] = afStack_4b5c[0x9d] + fVar29;
    param_2[0x9d] = afStack_4b5c[0x9e] + fVar30;
    param_2[0x9e] = afStack_4b5c[0x9f] + fVar31;
    param_2[0x9f] = afStack_4b5c[0xa0] + fVar32;
    fVar56 = *(float *)(param_1 + 0xc8c);
    fVar63 = *(float *)(param_1 + 0xc90);
    fVar1 = *(float *)(param_1 + 0xc94);
    fVar3 = *(float *)(param_1 + 0xc98);
    fVar6 = *(float *)(param_1 + 0xc9c);
    fVar7 = *(float *)(param_1 + 0xca0);
    fVar8 = *(float *)(param_1 + 0xca4);
    fVar9 = *(float *)(param_1 + 0xca8);
    fVar10 = *(float *)(param_1 + 0xcac);
    fVar11 = *(float *)(param_1 + 0xcb0);
    fVar12 = *(float *)(param_1 + 0xcb4);
    fVar13 = *(float *)(param_1 + 0xcb8);
    fVar14 = *(float *)(param_1 + 0xcbc);
    fVar15 = *(float *)(param_1 + 0xcc0);
    fVar16 = *(float *)(param_1 + 0xcc4);
    fVar17 = *(float *)(param_1 + 0xcc8);
    fVar18 = *(float *)(param_1 + 0xccc);
    fVar19 = *(float *)(param_1 + 0xcd0);
    fVar20 = *(float *)(param_1 + 0xcd4);
    fVar21 = *(float *)(param_1 + 0xcd8);
    fVar22 = *(float *)(param_1 + 0xcdc);
    fVar23 = *(float *)(param_1 + 0xce0);
    fVar24 = *(float *)(param_1 + 0xce4);
    fVar25 = *(float *)(param_1 + 0xce8);
    fVar26 = *(float *)(param_1 + 0xcec);
    fVar27 = *(float *)(param_1 + 0xcf0);
    fVar28 = *(float *)(param_1 + 0xcf4);
    fVar29 = *(float *)(param_1 + 0xcf8);
    fVar30 = *(float *)(param_1 + 0xcfc);
    fVar31 = *(float *)(param_1 + 0xd00);
    fVar32 = *(float *)(param_1 + 0xd04);
    param_2[0xa0] = afStack_4b5c[0xa1] + *(float *)(param_1 + 0xc88);
    param_2[0xa1] = afStack_4b5c[0xa2] + fVar56;
    param_2[0xa2] = afStack_4b5c[0xa3] + fVar63;
    param_2[0xa3] = afStack_4b5c[0xa4] + fVar1;
    param_2[0xa4] = afStack_4b5c[0xa5] + fVar3;
    param_2[0xa5] = afStack_4b5c[0xa6] + fVar6;
    param_2[0xa6] = afStack_4b5c[0xa7] + fVar7;
    param_2[0xa7] = afStack_4b5c[0xa8] + fVar8;
    param_2[0xa8] = afStack_4b5c[0xa9] + fVar9;
    param_2[0xa9] = afStack_4b5c[0xaa] + fVar10;
    param_2[0xaa] = afStack_4b5c[0xab] + fVar11;
    param_2[0xab] = afStack_4b5c[0xac] + fVar12;
    param_2[0xac] = afStack_4b5c[0xad] + fVar13;
    param_2[0xad] = afStack_4b5c[0xae] + fVar14;
    param_2[0xae] = afStack_4b5c[0xaf] + fVar15;
    param_2[0xaf] = afStack_4b5c[0xb0] + fVar16;
    param_2[0xb0] = afStack_4b5c[0xb1] + fVar17;
    param_2[0xb1] = afStack_4b5c[0xb2] + fVar18;
    param_2[0xb2] = afStack_4b5c[0xb3] + fVar19;
    param_2[0xb3] = afStack_4b5c[0xb4] + fVar20;
    param_2[0xb4] = afStack_4b5c[0xb5] + fVar21;
    param_2[0xb5] = afStack_4b5c[0xb6] + fVar22;
    param_2[0xb6] = afStack_4b5c[0xb7] + fVar23;
    param_2[0xb7] = afStack_4b5c[0xb8] + fVar24;
    param_2[0xb8] = afStack_4b5c[0xb9] + fVar25;
    param_2[0xb9] = afStack_4b5c[0xba] + fVar26;
    param_2[0xba] = afStack_4b5c[0xbb] + fVar27;
    param_2[0xbb] = afStack_4b5c[0xbc] + fVar28;
    param_2[0xbc] = afStack_4b5c[0xbd] + fVar29;
    param_2[0xbd] = afStack_4b5c[0xbe] + fVar30;
    param_2[0xbe] = afStack_4b5c[0xbf] + fVar31;
    param_2[0xbf] = afStack_4b5c[0xc0] + fVar32;
    fVar56 = *(float *)(param_1 + 0xd0c);
    fVar63 = *(float *)(param_1 + 0xd10);
    fVar1 = *(float *)(param_1 + 0xd14);
    fVar3 = *(float *)(param_1 + 0xd18);
    fVar6 = *(float *)(param_1 + 0xd1c);
    fVar7 = *(float *)(param_1 + 0xd20);
    fVar8 = *(float *)(param_1 + 0xd24);
    fVar9 = *(float *)(param_1 + 0xd28);
    fVar10 = *(float *)(param_1 + 0xd2c);
    fVar11 = *(float *)(param_1 + 0xd30);
    fVar12 = *(float *)(param_1 + 0xd34);
    fVar13 = *(float *)(param_1 + 0xd38);
    fVar14 = *(float *)(param_1 + 0xd3c);
    fVar15 = *(float *)(param_1 + 0xd40);
    fVar16 = *(float *)(param_1 + 0xd44);
    fVar17 = *(float *)(param_1 + 0xd48);
    fVar18 = *(float *)(param_1 + 0xd4c);
    fVar19 = *(float *)(param_1 + 0xd50);
    fVar20 = *(float *)(param_1 + 0xd54);
    fVar21 = *(float *)(param_1 + 0xd58);
    fVar22 = *(float *)(param_1 + 0xd5c);
    fVar23 = *(float *)(param_1 + 0xd60);
    fVar24 = *(float *)(param_1 + 0xd64);
    fVar25 = *(float *)(param_1 + 0xd68);
    fVar26 = *(float *)(param_1 + 0xd6c);
    fVar27 = *(float *)(param_1 + 0xd70);
    fVar28 = *(float *)(param_1 + 0xd74);
    fVar29 = *(float *)(param_1 + 0xd78);
    fVar30 = *(float *)(param_1 + 0xd7c);
    fVar31 = *(float *)(param_1 + 0xd80);
    fVar32 = *(float *)(param_1 + 0xd84);
    param_2[0xc0] = afStack_4b5c[0xc1] + *(float *)(param_1 + 0xd08);
    param_2[0xc1] = afStack_4b5c[0xc2] + fVar56;
    param_2[0xc2] = afStack_4b5c[0xc3] + fVar63;
    param_2[0xc3] = afStack_4b5c[0xc4] + fVar1;
    param_2[0xc4] = afStack_4b5c[0xc5] + fVar3;
    param_2[0xc5] = afStack_4b5c[0xc6] + fVar6;
    param_2[0xc6] = afStack_4b5c[199] + fVar7;
    param_2[199] = afStack_4b5c[200] + fVar8;
    param_2[200] = afStack_4b5c[0xc9] + fVar9;
    param_2[0xc9] = afStack_4b5c[0xca] + fVar10;
    param_2[0xca] = afStack_4b5c[0xcb] + fVar11;
    param_2[0xcb] = afStack_4b5c[0xcc] + fVar12;
    param_2[0xcc] = afStack_4b5c[0xcd] + fVar13;
    param_2[0xcd] = afStack_4b5c[0xce] + fVar14;
    param_2[0xce] = afStack_4b5c[0xcf] + fVar15;
    param_2[0xcf] = afStack_4b5c[0xd0] + fVar16;
    param_2[0xd0] = afStack_4b5c[0xd1] + fVar17;
    param_2[0xd1] = afStack_4b5c[0xd2] + fVar18;
    param_2[0xd2] = afStack_4b5c[0xd3] + fVar19;
    param_2[0xd3] = afStack_4b5c[0xd4] + fVar20;
    param_2[0xd4] = afStack_4b5c[0xd5] + fVar21;
    param_2[0xd5] = afStack_4b5c[0xd6] + fVar22;
    param_2[0xd6] = afStack_4b5c[0xd7] + fVar23;
    param_2[0xd7] = afStack_4b5c[0xd8] + fVar24;
    param_2[0xd8] = afStack_4b5c[0xd9] + fVar25;
    param_2[0xd9] = afStack_4b5c[0xda] + fVar26;
    param_2[0xda] = afStack_4b5c[0xdb] + fVar27;
    param_2[0xdb] = afStack_4b5c[0xdc] + fVar28;
    param_2[0xdc] = afStack_4b5c[0xdd] + fVar29;
    param_2[0xdd] = afStack_4b5c[0xde] + fVar30;
    param_2[0xde] = afStack_4b5c[0xdf] + fVar31;
    param_2[0xdf] = afStack_4b5c[0xe0] + fVar32;
    fVar56 = *(float *)(param_1 + 0xd8c);
    fVar63 = *(float *)(param_1 + 0xd90);
    fVar1 = *(float *)(param_1 + 0xd94);
    fVar3 = *(float *)(param_1 + 0xd98);
    fVar6 = *(float *)(param_1 + 0xd9c);
    fVar7 = *(float *)(param_1 + 0xda0);
    fVar8 = *(float *)(param_1 + 0xda4);
    fVar9 = *(float *)(param_1 + 0xda8);
    fVar10 = *(float *)(param_1 + 0xdac);
    fVar11 = *(float *)(param_1 + 0xdb0);
    fVar12 = *(float *)(param_1 + 0xdb4);
    fVar13 = *(float *)(param_1 + 0xdb8);
    fVar14 = *(float *)(param_1 + 0xdbc);
    fVar15 = *(float *)(param_1 + 0xdc0);
    fVar16 = *(float *)(param_1 + 0xdc4);
    fVar17 = *(float *)(param_1 + 0xdc8);
    fVar18 = *(float *)(param_1 + 0xdcc);
    fVar19 = *(float *)(param_1 + 0xdd0);
    fVar20 = *(float *)(param_1 + 0xdd4);
    fVar21 = *(float *)(param_1 + 0xdd8);
    fVar22 = *(float *)(param_1 + 0xddc);
    fVar23 = *(float *)(param_1 + 0xde0);
    fVar24 = *(float *)(param_1 + 0xde4);
    fVar25 = *(float *)(param_1 + 0xde8);
    fVar26 = *(float *)(param_1 + 0xdec);
    fVar27 = *(float *)(param_1 + 0xdf0);
    fVar28 = *(float *)(param_1 + 0xdf4);
    fVar29 = *(float *)(param_1 + 0xdf8);
    fVar30 = *(float *)(param_1 + 0xdfc);
    fVar31 = *(float *)(param_1 + 0xe00);
    fVar32 = *(float *)(param_1 + 0xe04);
    param_2[0xe0] = afStack_4b5c[0xe1] + *(float *)(param_1 + 0xd88);
    param_2[0xe1] = afStack_4b5c[0xe2] + fVar56;
    param_2[0xe2] = afStack_4b5c[0xe3] + fVar63;
    param_2[0xe3] = afStack_4b5c[0xe4] + fVar1;
    param_2[0xe4] = afStack_4b5c[0xe5] + fVar3;
    param_2[0xe5] = afStack_4b5c[0xe6] + fVar6;
    param_2[0xe6] = afStack_4b5c[0xe7] + fVar7;
    param_2[0xe7] = afStack_4b5c[0xe8] + fVar8;
    param_2[0xe8] = afStack_4b5c[0xe9] + fVar9;
    param_2[0xe9] = afStack_4b5c[0xea] + fVar10;
    param_2[0xea] = afStack_4b5c[0xeb] + fVar11;
    param_2[0xeb] = afStack_4b5c[0xec] + fVar12;
    param_2[0xec] = afStack_4b5c[0xed] + fVar13;
    param_2[0xed] = afStack_4b5c[0xee] + fVar14;
    param_2[0xee] = afStack_4b5c[0xef] + fVar15;
    param_2[0xef] = afStack_4b5c[0xf0] + fVar16;
    param_2[0xf0] = afStack_4b5c[0xf1] + fVar17;
    param_2[0xf1] = afStack_4b5c[0xf2] + fVar18;
    param_2[0xf2] = afStack_4b5c[0xf3] + fVar19;
    param_2[0xf3] = afStack_4b5c[0xf4] + fVar20;
    param_2[0xf4] = afStack_4b5c[0xf5] + fVar21;
    param_2[0xf5] = afStack_4b5c[0xf6] + fVar22;
    param_2[0xf6] = afStack_4b5c[0xf7] + fVar23;
    param_2[0xf7] = afStack_4b5c[0xf8] + fVar24;
    param_2[0xf8] = afStack_4b5c[0xf9] + fVar25;
    param_2[0xf9] = afStack_4b5c[0xfa] + fVar26;
    param_2[0xfa] = afStack_4b5c[0xfb] + fVar27;
    param_2[0xfb] = afStack_4b5c[0xfc] + fVar28;
    param_2[0xfc] = afStack_4b5c[0xfd] + fVar29;
    param_2[0xfd] = afStack_4b5c[0xfe] + fVar30;
    param_2[0xfe] = afStack_4b5c[0xff] + fVar31;
    param_2[0xff] = afStack_4b5c[0x100] + fVar32;
    fVar56 = *(float *)(param_1 + 0xe0c);
    fVar63 = *(float *)(param_1 + 0xe10);
    fVar1 = *(float *)(param_1 + 0xe14);
    fVar3 = *(float *)(param_1 + 0xe18);
    fVar6 = *(float *)(param_1 + 0xe1c);
    fVar7 = *(float *)(param_1 + 0xe20);
    fVar8 = *(float *)(param_1 + 0xe24);
    fVar9 = *(float *)(param_1 + 0xe28);
    fVar10 = *(float *)(param_1 + 0xe2c);
    fVar11 = *(float *)(param_1 + 0xe30);
    fVar12 = *(float *)(param_1 + 0xe34);
    fVar13 = *(float *)(param_1 + 0xe38);
    fVar14 = *(float *)(param_1 + 0xe3c);
    fVar15 = *(float *)(param_1 + 0xe40);
    fVar16 = *(float *)(param_1 + 0xe44);
    fVar17 = *(float *)(param_1 + 0xe48);
    fVar18 = *(float *)(param_1 + 0xe4c);
    fVar19 = *(float *)(param_1 + 0xe50);
    fVar20 = *(float *)(param_1 + 0xe54);
    fVar21 = *(float *)(param_1 + 0xe58);
    fVar22 = *(float *)(param_1 + 0xe5c);
    fVar23 = *(float *)(param_1 + 0xe60);
    fVar24 = *(float *)(param_1 + 0xe64);
    fVar25 = *(float *)(param_1 + 0xe68);
    fVar26 = *(float *)(param_1 + 0xe6c);
    fVar27 = *(float *)(param_1 + 0xe70);
    fVar28 = *(float *)(param_1 + 0xe74);
    fVar29 = *(float *)(param_1 + 0xe78);
    fVar30 = *(float *)(param_1 + 0xe7c);
    fVar31 = *(float *)(param_1 + 0xe80);
    fVar32 = *(float *)(param_1 + 0xe84);
    param_2[0x100] = afStack_4b5c[0x101] + *(float *)(param_1 + 0xe08);
    param_2[0x101] = afStack_4b5c[0x102] + fVar56;
    param_2[0x102] = afStack_4b5c[0x103] + fVar63;
    param_2[0x103] = afStack_4b5c[0x104] + fVar1;
    param_2[0x104] = afStack_4b5c[0x105] + fVar3;
    param_2[0x105] = afStack_4b5c[0x106] + fVar6;
    param_2[0x106] = afStack_4b5c[0x107] + fVar7;
    param_2[0x107] = afStack_4b5c[0x108] + fVar8;
    param_2[0x108] = afStack_4b5c[0x109] + fVar9;
    param_2[0x109] = afStack_4b5c[0x10a] + fVar10;
    param_2[0x10a] = afStack_4b5c[0x10b] + fVar11;
    param_2[0x10b] = afStack_4b5c[0x10c] + fVar12;
    param_2[0x10c] = afStack_4b5c[0x10d] + fVar13;
    param_2[0x10d] = afStack_4b5c[0x10e] + fVar14;
    param_2[0x10e] = afStack_4b5c[0x10f] + fVar15;
    param_2[0x10f] = afStack_4b5c[0x110] + fVar16;
    param_2[0x110] = afStack_4b5c[0x111] + fVar17;
    param_2[0x111] = afStack_4b5c[0x112] + fVar18;
    param_2[0x112] = afStack_4b5c[0x113] + fVar19;
    param_2[0x113] = afStack_4b5c[0x114] + fVar20;
    param_2[0x114] = afStack_4b5c[0x115] + fVar21;
    param_2[0x115] = afStack_4b5c[0x116] + fVar22;
    param_2[0x116] = afStack_4b5c[0x117] + fVar23;
    param_2[0x117] = afStack_4b5c[0x118] + fVar24;
    param_2[0x118] = afStack_4b5c[0x119] + fVar25;
    param_2[0x119] = afStack_4b5c[0x11a] + fVar26;
    param_2[0x11a] = afStack_4b5c[0x11b] + fVar27;
    param_2[0x11b] = afStack_4b5c[0x11c] + fVar28;
    param_2[0x11c] = afStack_4b5c[0x11d] + fVar29;
    param_2[0x11d] = afStack_4b5c[0x11e] + fVar30;
    param_2[0x11e] = afStack_4b5c[0x11f] + fVar31;
    param_2[0x11f] = afStack_4b5c[0x120] + fVar32;
    fVar56 = *(float *)(param_1 + 0xe8c);
    fVar63 = *(float *)(param_1 + 0xe90);
    fVar1 = *(float *)(param_1 + 0xe94);
    fVar3 = *(float *)(param_1 + 0xe98);
    fVar6 = *(float *)(param_1 + 0xe9c);
    fVar7 = *(float *)(param_1 + 0xea0);
    fVar8 = *(float *)(param_1 + 0xea4);
    fVar9 = *(float *)(param_1 + 0xea8);
    fVar10 = *(float *)(param_1 + 0xeac);
    fVar11 = *(float *)(param_1 + 0xeb0);
    fVar12 = *(float *)(param_1 + 0xeb4);
    fVar13 = *(float *)(param_1 + 0xeb8);
    fVar14 = *(float *)(param_1 + 0xebc);
    fVar15 = *(float *)(param_1 + 0xec0);
    fVar16 = *(float *)(param_1 + 0xec4);
    fVar17 = *(float *)(param_1 + 0xec8);
    fVar18 = *(float *)(param_1 + 0xecc);
    fVar19 = *(float *)(param_1 + 0xed0);
    fVar20 = *(float *)(param_1 + 0xed4);
    fVar21 = *(float *)(param_1 + 0xed8);
    fVar22 = *(float *)(param_1 + 0xedc);
    fVar23 = *(float *)(param_1 + 0xee0);
    fVar24 = *(float *)(param_1 + 0xee4);
    fVar25 = *(float *)(param_1 + 0xee8);
    fVar26 = *(float *)(param_1 + 0xeec);
    fVar27 = *(float *)(param_1 + 0xef0);
    fVar28 = *(float *)(param_1 + 0xef4);
    fVar29 = *(float *)(param_1 + 0xef8);
    fVar30 = *(float *)(param_1 + 0xefc);
    fVar31 = *(float *)(param_1 + 0xf00);
    fVar32 = *(float *)(param_1 + 0xf04);
    param_2[0x120] = afStack_4b5c[0x121] + *(float *)(param_1 + 0xe88);
    param_2[0x121] = afStack_4b5c[0x122] + fVar56;
    param_2[0x122] = afStack_4b5c[0x123] + fVar63;
    param_2[0x123] = afStack_4b5c[0x124] + fVar1;
    param_2[0x124] = afStack_4b5c[0x125] + fVar3;
    param_2[0x125] = afStack_4b5c[0x126] + fVar6;
    param_2[0x126] = afStack_4b5c[0x127] + fVar7;
    param_2[0x127] = afStack_4b5c[0x128] + fVar8;
    param_2[0x128] = afStack_4b5c[0x129] + fVar9;
    param_2[0x129] = afStack_4b5c[0x12a] + fVar10;
    param_2[0x12a] = afStack_4b5c[299] + fVar11;
    param_2[299] = afStack_4b5c[300] + fVar12;
    param_2[300] = afStack_4b5c[0x12d] + fVar13;
    param_2[0x12d] = afStack_4b5c[0x12e] + fVar14;
    param_2[0x12e] = afStack_4b5c[0x12f] + fVar15;
    param_2[0x12f] = afStack_4b5c[0x130] + fVar16;
    param_2[0x130] = afStack_4b5c[0x131] + fVar17;
    param_2[0x131] = afStack_4b5c[0x132] + fVar18;
    param_2[0x132] = afStack_4b5c[0x133] + fVar19;
    param_2[0x133] = afStack_4b5c[0x134] + fVar20;
    param_2[0x134] = afStack_4b5c[0x135] + fVar21;
    param_2[0x135] = afStack_4b5c[0x136] + fVar22;
    param_2[0x136] = afStack_4b5c[0x137] + fVar23;
    param_2[0x137] = afStack_4b5c[0x138] + fVar24;
    param_2[0x138] = afStack_4b5c[0x139] + fVar25;
    param_2[0x139] = afStack_4b5c[0x13a] + fVar26;
    param_2[0x13a] = afStack_4b5c[0x13b] + fVar27;
    param_2[0x13b] = afStack_4b5c[0x13c] + fVar28;
    param_2[0x13c] = afStack_4b5c[0x13d] + fVar29;
    param_2[0x13d] = afStack_4b5c[0x13e] + fVar30;
    param_2[0x13e] = afStack_4b5c[0x13f] + fVar31;
    param_2[0x13f] = afStack_4b5c[0x140] + fVar32;
    fVar56 = *(float *)(param_1 + 0xf0c);
    fVar63 = *(float *)(param_1 + 0xf10);
    fVar1 = *(float *)(param_1 + 0xf14);
    fVar3 = *(float *)(param_1 + 0xf18);
    fVar6 = *(float *)(param_1 + 0xf1c);
    fVar7 = *(float *)(param_1 + 0xf20);
    fVar8 = *(float *)(param_1 + 0xf24);
    fVar9 = *(float *)(param_1 + 0xf28);
    fVar10 = *(float *)(param_1 + 0xf2c);
    fVar11 = *(float *)(param_1 + 0xf30);
    fVar12 = *(float *)(param_1 + 0xf34);
    fVar13 = *(float *)(param_1 + 0xf38);
    fVar14 = *(float *)(param_1 + 0xf3c);
    fVar15 = *(float *)(param_1 + 0xf40);
    fVar16 = *(float *)(param_1 + 0xf44);
    fVar17 = *(float *)(param_1 + 0xf48);
    fVar18 = *(float *)(param_1 + 0xf4c);
    fVar19 = *(float *)(param_1 + 0xf50);
    fVar20 = *(float *)(param_1 + 0xf54);
    fVar21 = *(float *)(param_1 + 0xf58);
    fVar22 = *(float *)(param_1 + 0xf5c);
    fVar23 = *(float *)(param_1 + 0xf60);
    fVar24 = *(float *)(param_1 + 0xf64);
    fVar25 = *(float *)(param_1 + 0xf68);
    fVar26 = *(float *)(param_1 + 0xf6c);
    fVar27 = *(float *)(param_1 + 0xf70);
    fVar28 = *(float *)(param_1 + 0xf74);
    fVar29 = *(float *)(param_1 + 0xf78);
    fVar30 = *(float *)(param_1 + 0xf7c);
    fVar31 = *(float *)(param_1 + 0xf80);
    fVar32 = *(float *)(param_1 + 0xf84);
    param_2[0x140] = afStack_4b5c[0x141] + *(float *)(param_1 + 0xf08);
    param_2[0x141] = afStack_4b5c[0x142] + fVar56;
    param_2[0x142] = afStack_4b5c[0x143] + fVar63;
    param_2[0x143] = afStack_4b5c[0x144] + fVar1;
    param_2[0x144] = afStack_4b5c[0x145] + fVar3;
    param_2[0x145] = afStack_4b5c[0x146] + fVar6;
    param_2[0x146] = afStack_4b5c[0x147] + fVar7;
    param_2[0x147] = afStack_4b5c[0x148] + fVar8;
    param_2[0x148] = afStack_4b5c[0x149] + fVar9;
    param_2[0x149] = afStack_4b5c[0x14a] + fVar10;
    param_2[0x14a] = afStack_4b5c[0x14b] + fVar11;
    param_2[0x14b] = afStack_4b5c[0x14c] + fVar12;
    param_2[0x14c] = afStack_4b5c[0x14d] + fVar13;
    param_2[0x14d] = afStack_4b5c[0x14e] + fVar14;
    param_2[0x14e] = afStack_4b5c[0x14f] + fVar15;
    param_2[0x14f] = afStack_4b5c[0x150] + fVar16;
    param_2[0x150] = afStack_4b5c[0x151] + fVar17;
    param_2[0x151] = afStack_4b5c[0x152] + fVar18;
    param_2[0x152] = afStack_4b5c[0x153] + fVar19;
    param_2[0x153] = afStack_4b5c[0x154] + fVar20;
    param_2[0x154] = afStack_4b5c[0x155] + fVar21;
    param_2[0x155] = afStack_4b5c[0x156] + fVar22;
    param_2[0x156] = afStack_4b5c[0x157] + fVar23;
    param_2[0x157] = afStack_4b5c[0x158] + fVar24;
    param_2[0x158] = afStack_4b5c[0x159] + fVar25;
    param_2[0x159] = afStack_4b5c[0x15a] + fVar26;
    param_2[0x15a] = afStack_4b5c[0x15b] + fVar27;
    param_2[0x15b] = afStack_4b5c[0x15c] + fVar28;
    param_2[0x15c] = afStack_4b5c[0x15d] + fVar29;
    param_2[0x15d] = afStack_4b5c[0x15e] + fVar30;
    param_2[0x15e] = afStack_4b5c[0x15f] + fVar31;
    param_2[0x15f] = afStack_4b5c[0x160] + fVar32;
    fVar56 = *(float *)(param_1 + 0xf8c);
    fVar63 = *(float *)(param_1 + 0xf90);
    fVar1 = *(float *)(param_1 + 0xf94);
    fVar3 = *(float *)(param_1 + 0xf98);
    fVar6 = *(float *)(param_1 + 0xf9c);
    fVar7 = *(float *)(param_1 + 4000);
    fVar8 = *(float *)(param_1 + 0xfa4);
    fVar9 = *(float *)(param_1 + 0xfa8);
    fVar10 = *(float *)(param_1 + 0xfac);
    fVar11 = *(float *)(param_1 + 0xfb0);
    fVar12 = *(float *)(param_1 + 0xfb4);
    fVar13 = *(float *)(param_1 + 0xfb8);
    fVar14 = *(float *)(param_1 + 0xfbc);
    fVar15 = *(float *)(param_1 + 0xfc0);
    fVar16 = *(float *)(param_1 + 0xfc4);
    fVar17 = *(float *)(param_1 + 0xfc8);
    fVar18 = *(float *)(param_1 + 0xfcc);
    fVar19 = *(float *)(param_1 + 0xfd0);
    fVar20 = *(float *)(param_1 + 0xfd4);
    fVar21 = *(float *)(param_1 + 0xfd8);
    fVar22 = *(float *)(param_1 + 0xfdc);
    fVar23 = *(float *)(param_1 + 0xfe0);
    fVar24 = *(float *)(param_1 + 0xfe4);
    fVar25 = *(float *)(param_1 + 0xfe8);
    fVar26 = *(float *)(param_1 + 0xfec);
    fVar27 = *(float *)(param_1 + 0xff0);
    fVar28 = *(float *)(param_1 + 0xff4);
    fVar29 = *(float *)(param_1 + 0xff8);
    fVar30 = *(float *)(param_1 + 0xffc);
    fVar31 = *(float *)(param_1 + 0x1000);
    fVar32 = *(float *)(param_1 + 0x1004);
    param_2[0x160] = afStack_4b5c[0x161] + *(float *)(param_1 + 0xf88);
    param_2[0x161] = afStack_4b5c[0x162] + fVar56;
    param_2[0x162] = afStack_4b5c[0x163] + fVar63;
    param_2[0x163] = afStack_4b5c[0x164] + fVar1;
    param_2[0x164] = afStack_4b5c[0x165] + fVar3;
    param_2[0x165] = afStack_4b5c[0x166] + fVar6;
    param_2[0x166] = afStack_4b5c[0x167] + fVar7;
    param_2[0x167] = afStack_4b5c[0x168] + fVar8;
    param_2[0x168] = afStack_4b5c[0x169] + fVar9;
    param_2[0x169] = afStack_4b5c[0x16a] + fVar10;
    param_2[0x16a] = afStack_4b5c[0x16b] + fVar11;
    param_2[0x16b] = afStack_4b5c[0x16c] + fVar12;
    param_2[0x16c] = afStack_4b5c[0x16d] + fVar13;
    param_2[0x16d] = afStack_4b5c[0x16e] + fVar14;
    param_2[0x16e] = afStack_4b5c[0x16f] + fVar15;
    param_2[0x16f] = afStack_4b5c[0x170] + fVar16;
    param_2[0x170] = afStack_4b5c[0x171] + fVar17;
    param_2[0x171] = afStack_4b5c[0x172] + fVar18;
    param_2[0x172] = afStack_4b5c[0x173] + fVar19;
    param_2[0x173] = afStack_4b5c[0x174] + fVar20;
    param_2[0x174] = afStack_4b5c[0x175] + fVar21;
    param_2[0x175] = afStack_4b5c[0x176] + fVar22;
    param_2[0x176] = afStack_4b5c[0x177] + fVar23;
    param_2[0x177] = afStack_4b5c[0x178] + fVar24;
    param_2[0x178] = afStack_4b5c[0x179] + fVar25;
    param_2[0x179] = afStack_4b5c[0x17a] + fVar26;
    param_2[0x17a] = afStack_4b5c[0x17b] + fVar27;
    param_2[0x17b] = afStack_4b5c[0x17c] + fVar28;
    param_2[0x17c] = afStack_4b5c[0x17d] + fVar29;
    param_2[0x17d] = afStack_4b5c[0x17e] + fVar30;
    param_2[0x17e] = afStack_4b5c[0x17f] + fVar31;
    param_2[0x17f] = afStack_4b5c[0x180] + fVar32;
    fVar56 = *(float *)(param_1 + 0x100c);
    fVar63 = *(float *)(param_1 + 0x1010);
    fVar1 = *(float *)(param_1 + 0x1014);
    fVar3 = *(float *)(param_1 + 0x1018);
    fVar6 = *(float *)(param_1 + 0x101c);
    fVar7 = *(float *)(param_1 + 0x1020);
    fVar8 = *(float *)(param_1 + 0x1024);
    fVar9 = *(float *)(param_1 + 0x1028);
    fVar10 = *(float *)(param_1 + 0x102c);
    fVar11 = *(float *)(param_1 + 0x1030);
    fVar12 = *(float *)(param_1 + 0x1034);
    fVar13 = *(float *)(param_1 + 0x1038);
    fVar14 = *(float *)(param_1 + 0x103c);
    fVar15 = *(float *)(param_1 + 0x1040);
    fVar16 = *(float *)(param_1 + 0x1044);
    fVar17 = *(float *)(param_1 + 0x1048);
    fVar18 = *(float *)(param_1 + 0x104c);
    fVar19 = *(float *)(param_1 + 0x1050);
    fVar20 = *(float *)(param_1 + 0x1054);
    fVar21 = *(float *)(param_1 + 0x1058);
    fVar22 = *(float *)(param_1 + 0x105c);
    fVar23 = *(float *)(param_1 + 0x1060);
    fVar24 = *(float *)(param_1 + 0x1064);
    fVar25 = *(float *)(param_1 + 0x1068);
    fVar26 = *(float *)(param_1 + 0x106c);
    fVar27 = *(float *)(param_1 + 0x1070);
    fVar28 = *(float *)(param_1 + 0x1074);
    fVar29 = *(float *)(param_1 + 0x1078);
    fVar30 = *(float *)(param_1 + 0x107c);
    fVar31 = *(float *)(param_1 + 0x1080);
    fVar32 = *(float *)(param_1 + 0x1084);
    param_2[0x180] = afStack_4b5c[0x181] + *(float *)(param_1 + 0x1008);
    param_2[0x181] = afStack_4b5c[0x182] + fVar56;
    param_2[0x182] = afStack_4b5c[0x183] + fVar63;
    param_2[0x183] = afStack_4b5c[0x184] + fVar1;
    param_2[0x184] = afStack_4b5c[0x185] + fVar3;
    param_2[0x185] = afStack_4b5c[0x186] + fVar6;
    param_2[0x186] = afStack_4b5c[0x187] + fVar7;
    param_2[0x187] = afStack_4b5c[0x188] + fVar8;
    param_2[0x188] = afStack_4b5c[0x189] + fVar9;
    param_2[0x189] = afStack_4b5c[0x18a] + fVar10;
    param_2[0x18a] = afStack_4b5c[0x18b] + fVar11;
    param_2[0x18b] = afStack_4b5c[0x18c] + fVar12;
    param_2[0x18c] = afStack_4b5c[0x18d] + fVar13;
    param_2[0x18d] = afStack_4b5c[0x18e] + fVar14;
    param_2[0x18e] = afStack_4b5c[399] + fVar15;
    param_2[399] = afStack_4b5c[400] + fVar16;
    param_2[400] = afStack_4b5c[0x191] + fVar17;
    param_2[0x191] = afStack_4b5c[0x192] + fVar18;
    param_2[0x192] = afStack_4b5c[0x193] + fVar19;
    param_2[0x193] = afStack_4b5c[0x194] + fVar20;
    param_2[0x194] = afStack_4b5c[0x195] + fVar21;
    param_2[0x195] = afStack_4b5c[0x196] + fVar22;
    param_2[0x196] = afStack_4b5c[0x197] + fVar23;
    param_2[0x197] = afStack_4b5c[0x198] + fVar24;
    param_2[0x198] = afStack_4b5c[0x199] + fVar25;
    param_2[0x199] = afStack_4b5c[0x19a] + fVar26;
    param_2[0x19a] = afStack_4b5c[0x19b] + fVar27;
    param_2[0x19b] = afStack_4b5c[0x19c] + fVar28;
    param_2[0x19c] = afStack_4b5c[0x19d] + fVar29;
    param_2[0x19d] = afStack_4b5c[0x19e] + fVar30;
    param_2[0x19e] = afStack_4b5c[0x19f] + fVar31;
    param_2[0x19f] = afStack_4b5c[0x1a0] + fVar32;
    fVar56 = *(float *)(param_1 + 0x108c);
    fVar63 = *(float *)(param_1 + 0x1090);
    fVar1 = *(float *)(param_1 + 0x1094);
    fVar3 = *(float *)(param_1 + 0x1098);
    fVar6 = *(float *)(param_1 + 0x109c);
    fVar7 = *(float *)(param_1 + 0x10a0);
    fVar8 = *(float *)(param_1 + 0x10a4);
    fVar9 = *(float *)(param_1 + 0x10a8);
    fVar10 = *(float *)(param_1 + 0x10ac);
    fVar11 = *(float *)(param_1 + 0x10b0);
    fVar12 = *(float *)(param_1 + 0x10b4);
    fVar13 = *(float *)(param_1 + 0x10b8);
    fVar14 = *(float *)(param_1 + 0x10bc);
    fVar15 = *(float *)(param_1 + 0x10c0);
    fVar16 = *(float *)(param_1 + 0x10c4);
    fVar17 = *(float *)(param_1 + 0x10c8);
    fVar18 = *(float *)(param_1 + 0x10cc);
    fVar19 = *(float *)(param_1 + 0x10d0);
    fVar20 = *(float *)(param_1 + 0x10d4);
    fVar21 = *(float *)(param_1 + 0x10d8);
    fVar22 = *(float *)(param_1 + 0x10dc);
    fVar23 = *(float *)(param_1 + 0x10e0);
    fVar24 = *(float *)(param_1 + 0x10e4);
    fVar25 = *(float *)(param_1 + 0x10e8);
    fVar26 = *(float *)(param_1 + 0x10ec);
    fVar27 = *(float *)(param_1 + 0x10f0);
    fVar28 = *(float *)(param_1 + 0x10f4);
    fVar29 = *(float *)(param_1 + 0x10f8);
    fVar30 = *(float *)(param_1 + 0x10fc);
    fVar31 = *(float *)(param_1 + 0x1100);
    fVar32 = *(float *)(param_1 + 0x1104);
    param_2[0x1a0] = afStack_4b5c[0x1a1] + *(float *)(param_1 + 0x1088);
    param_2[0x1a1] = afStack_4b5c[0x1a2] + fVar56;
    param_2[0x1a2] = afStack_4b5c[0x1a3] + fVar63;
    param_2[0x1a3] = afStack_4b5c[0x1a4] + fVar1;
    param_2[0x1a4] = afStack_4b5c[0x1a5] + fVar3;
    param_2[0x1a5] = afStack_4b5c[0x1a6] + fVar6;
    param_2[0x1a6] = afStack_4b5c[0x1a7] + fVar7;
    param_2[0x1a7] = afStack_4b5c[0x1a8] + fVar8;
    param_2[0x1a8] = afStack_4b5c[0x1a9] + fVar9;
    param_2[0x1a9] = afStack_4b5c[0x1aa] + fVar10;
    param_2[0x1aa] = afStack_4b5c[0x1ab] + fVar11;
    param_2[0x1ab] = afStack_4b5c[0x1ac] + fVar12;
    param_2[0x1ac] = afStack_4b5c[0x1ad] + fVar13;
    param_2[0x1ad] = afStack_4b5c[0x1ae] + fVar14;
    param_2[0x1ae] = afStack_4b5c[0x1af] + fVar15;
    param_2[0x1af] = afStack_4b5c[0x1b0] + fVar16;
    param_2[0x1b0] = afStack_4b5c[0x1b1] + fVar17;
    param_2[0x1b1] = afStack_4b5c[0x1b2] + fVar18;
    param_2[0x1b2] = afStack_4b5c[0x1b3] + fVar19;
    param_2[0x1b3] = afStack_4b5c[0x1b4] + fVar20;
    param_2[0x1b4] = afStack_4b5c[0x1b5] + fVar21;
    param_2[0x1b5] = afStack_4b5c[0x1b6] + fVar22;
    param_2[0x1b6] = afStack_4b5c[0x1b7] + fVar23;
    param_2[0x1b7] = afStack_4b5c[0x1b8] + fVar24;
    param_2[0x1b8] = afStack_4b5c[0x1b9] + fVar25;
    param_2[0x1b9] = afStack_4b5c[0x1ba] + fVar26;
    param_2[0x1ba] = afStack_4b5c[0x1bb] + fVar27;
    param_2[0x1bb] = afStack_4b5c[0x1bc] + fVar28;
    param_2[0x1bc] = afStack_4b5c[0x1bd] + fVar29;
    param_2[0x1bd] = afStack_4b5c[0x1be] + fVar30;
    param_2[0x1be] = afStack_4b5c[0x1bf] + fVar31;
    param_2[0x1bf] = afStack_4b5c[0x1c0] + fVar32;
    fVar56 = *(float *)(param_1 + 0x110c);
    fVar63 = *(float *)(param_1 + 0x1110);
    fVar1 = *(float *)(param_1 + 0x1114);
    fVar3 = *(float *)(param_1 + 0x1118);
    fVar6 = *(float *)(param_1 + 0x111c);
    fVar7 = *(float *)(param_1 + 0x1120);
    fVar8 = *(float *)(param_1 + 0x1124);
    fVar9 = *(float *)(param_1 + 0x1128);
    fVar10 = *(float *)(param_1 + 0x112c);
    fVar11 = *(float *)(param_1 + 0x1130);
    fVar12 = *(float *)(param_1 + 0x1134);
    fVar13 = *(float *)(param_1 + 0x1138);
    fVar14 = *(float *)(param_1 + 0x113c);
    fVar15 = *(float *)(param_1 + 0x1140);
    fVar16 = *(float *)(param_1 + 0x1144);
    fVar17 = *(float *)(param_1 + 0x1148);
    fVar18 = *(float *)(param_1 + 0x114c);
    fVar19 = *(float *)(param_1 + 0x1150);
    fVar20 = *(float *)(param_1 + 0x1154);
    fVar21 = *(float *)(param_1 + 0x1158);
    fVar22 = *(float *)(param_1 + 0x115c);
    fVar23 = *(float *)(param_1 + 0x1160);
    fVar24 = *(float *)(param_1 + 0x1164);
    fVar25 = *(float *)(param_1 + 0x1168);
    fVar26 = *(float *)(param_1 + 0x116c);
    fVar27 = *(float *)(param_1 + 0x1170);
    fVar28 = *(float *)(param_1 + 0x1174);
    fVar29 = *(float *)(param_1 + 0x1178);
    fVar30 = *(float *)(param_1 + 0x117c);
    fVar31 = *(float *)(param_1 + 0x1180);
    fVar32 = *(float *)(param_1 + 0x1184);
    param_2[0x1c0] = afStack_4b5c[0x1c1] + *(float *)(param_1 + 0x1108);
    param_2[0x1c1] = afStack_4b5c[0x1c2] + fVar56;
    param_2[0x1c2] = afStack_4b5c[0x1c3] + fVar63;
    param_2[0x1c3] = afStack_4b5c[0x1c4] + fVar1;
    param_2[0x1c4] = afStack_4b5c[0x1c5] + fVar3;
    param_2[0x1c5] = afStack_4b5c[0x1c6] + fVar6;
    param_2[0x1c6] = afStack_4b5c[0x1c7] + fVar7;
    param_2[0x1c7] = afStack_4b5c[0x1c8] + fVar8;
    param_2[0x1c8] = afStack_4b5c[0x1c9] + fVar9;
    param_2[0x1c9] = afStack_4b5c[0x1ca] + fVar10;
    param_2[0x1ca] = afStack_4b5c[0x1cb] + fVar11;
    param_2[0x1cb] = afStack_4b5c[0x1cc] + fVar12;
    param_2[0x1cc] = afStack_4b5c[0x1cd] + fVar13;
    param_2[0x1cd] = afStack_4b5c[0x1ce] + fVar14;
    param_2[0x1ce] = afStack_4b5c[0x1cf] + fVar15;
    param_2[0x1cf] = afStack_4b5c[0x1d0] + fVar16;
    param_2[0x1d0] = afStack_4b5c[0x1d1] + fVar17;
    param_2[0x1d1] = afStack_4b5c[0x1d2] + fVar18;
    param_2[0x1d2] = afStack_4b5c[0x1d3] + fVar19;
    param_2[0x1d3] = afStack_4b5c[0x1d4] + fVar20;
    param_2[0x1d4] = afStack_4b5c[0x1d5] + fVar21;
    param_2[0x1d5] = afStack_4b5c[0x1d6] + fVar22;
    param_2[0x1d6] = afStack_4b5c[0x1d7] + fVar23;
    param_2[0x1d7] = afStack_4b5c[0x1d8] + fVar24;
    param_2[0x1d8] = afStack_4b5c[0x1d9] + fVar25;
    param_2[0x1d9] = afStack_4b5c[0x1da] + fVar26;
    param_2[0x1da] = afStack_4b5c[0x1db] + fVar27;
    param_2[0x1db] = afStack_4b5c[0x1dc] + fVar28;
    param_2[0x1dc] = afStack_4b5c[0x1dd] + fVar29;
    param_2[0x1dd] = afStack_4b5c[0x1de] + fVar30;
    param_2[0x1de] = afStack_4b5c[0x1df] + fVar31;
    param_2[0x1df] = afStack_4b5c[0x1e0] + fVar32;
  }
  memcpy((void *)(param_1 + 0xa08),local_43d8,0x780);
  memcpy((void *)(param_1 + 0x6020),local_5a68,0xf08);
  memcpy((void *)(param_1 + 0x6f28),local_6978,0xf08);
  *(undefined8 *)(param_1 + 0x7e90) = local_7118;
  *(undefined8 *)(param_1 + 0x7e98) = uStack_7110;
  *(undefined8 *)(param_1 + 0x7ea0) = uStack_7108;
  *(undefined8 *)(param_1 + 0x7ea8) = uStack_7100;
  *(undefined8 *)(param_1 + 0x7e70) = local_7138;
  *(undefined8 *)(param_1 + 0x7e78) = uStack_7130;
  *(undefined8 *)(param_1 + 0x7e80) = uStack_7128;
  *(undefined8 *)(param_1 + 0x7e88) = uStack_7120;
  *(undefined8 *)(param_1 + 0x7e50) = local_7158;
  *(undefined8 *)(param_1 + 0x7e58) = uStack_7150;
  *(undefined8 *)(param_1 + 0x7e60) = uStack_7148;
  *(undefined8 *)(param_1 + 0x7e68) = uStack_7140;
  *(undefined8 *)(param_1 + 0x7e30) = local_7178;
  *(undefined8 *)(param_1 + 0x7e38) = uStack_7170;
  *(undefined8 *)(param_1 + 0x7e40) = uStack_7168;
  *(undefined8 *)(param_1 + 0x7e48) = uStack_7160;
  *(undefined8 *)(param_1 + 0x7f10) = local_7198;
  *(undefined8 *)(param_1 + 0x7f18) = uStack_7190;
  *(undefined8 *)(param_1 + 0x7f20) = uStack_7188;
  *(undefined8 *)(param_1 + 0x7f28) = uStack_7180;
  *(undefined8 *)(param_1 + 0x7ef0) = local_71b8;
  *(undefined8 *)(param_1 + 0x7ef8) = uStack_71b0;
  *(undefined8 *)(param_1 + 0x7f00) = uStack_71a8;
  *(undefined8 *)(param_1 + 0x7f08) = uStack_71a0;
  *(undefined8 *)(param_1 + 0x7ed0) = local_71d8;
  *(undefined8 *)(param_1 + 0x7ed8) = uStack_71d0;
  *(undefined8 *)(param_1 + 0x7ee0) = uStack_71c8;
  *(undefined8 *)(param_1 + 0x7ee8) = uStack_71c0;
  *(undefined8 *)(param_1 + 0x7eb0) = local_71f8;
  *(undefined8 *)(param_1 + 0x7eb8) = uStack_71f0;
  *(undefined8 *)(param_1 + 0x7ec0) = uStack_71e8;
  *(undefined8 *)(param_1 + 0x7ec8) = uStack_71e0;
  *(undefined8 *)(param_1 + 0x7f70) = local_7238;
  *(undefined8 *)(param_1 + 0x7f78) = uStack_7230;
  *(undefined8 *)(param_1 + 0x7f80) = uStack_7228;
  *(undefined8 *)(param_1 + 0x7f88) = uStack_7220;
  *(undefined8 *)(param_1 + 0x7f30) = local_7278;
  *(undefined8 *)(param_1 + 0x7f38) = uStack_7270;
  *(undefined8 *)(param_1 + 0x7f40) = uStack_7268;
  *(undefined8 *)(param_1 + 0x7f48) = uStack_7260;
  *(undefined8 *)(param_1 + 0x7f50) = local_7258;
  *(undefined8 *)(param_1 + 0x7f58) = uStack_7250;
  *(undefined8 *)(param_1 + 0x7f60) = uStack_7248;
  *(undefined8 *)(param_1 + 0x7f68) = uStack_7240;
  *(undefined8 *)(param_1 + 0x7f90) = local_7218;
  *(undefined8 *)(param_1 + 0x7f98) = uStack_7210;
  *(undefined8 *)(param_1 + 0x7fa0) = uStack_7208;
  *(undefined8 *)(param_1 + 0x7fa8) = uStack_7200;
  if ((local_50 ^ (ulonglong)auStack_7be8) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180004830
   NAME : rnn_fft_alloc_arch_c
   SIG  : undefined rnn_fft_alloc_arch_c(void)
   ======================================================================== */

undefined8 rnn_fft_alloc_arch_c(void)

{
                    /* 0x4830  15  rnn_fft_alloc_arch_c */
  return 0;
}



/* ========================================================================
   ENTRY: 180004840
   NAME : rnn_fft_alloc_twiddles
   SIG  : undefined rnn_fft_alloc_twiddles(void)
   ======================================================================== */

uint * rnn_fft_alloc_twiddles(uint param_1,uint *param_2,ulonglong *param_3,uint *param_4)

{
  uint *puVar1;
  bool bVar2;
  bool bVar3;
  uint uVar4;
  uint *_Memory;
  longlong lVar5;
  void *pvVar6;
  int iVar7;
  ulonglong uVar8;
  longlong lVar9;
  ulonglong uVar10;
  uint *puVar11;
  ulonglong uVar12;
  int iVar13;
  ulonglong uVar14;
  int iVar15;
  int iVar16;
  ulonglong uVar17;
  double dVar18;
  double dVar19;
  double dVar20;
  
                    /* 0x4840  16  rnn_fft_alloc_twiddles */
  if (param_3 == (ulonglong *)0x0) {
    _Memory = malloc(0x48);
  }
  else {
    _Memory = (uint *)0x0;
    if ((param_2 != (uint *)0x0) && (0x47 < *param_3)) {
      _Memory = param_2;
    }
    *param_3 = 0x48;
  }
  if (_Memory == (uint *)0x0) {
    return (uint *)0x0;
  }
  *_Memory = param_1;
  _Memory[1] = (uint)(DAT_18000f098 / (float)(int)param_1);
  if (param_4 == (uint *)0x0) {
    pvVar6 = malloc((longlong)(int)param_1 << 3);
    *(void **)(_Memory + 0xe) = pvVar6;
    if (0 < (int)param_1) {
      dVar20 = DAT_18000f138 / (double)(int)param_1;
      if (param_1 == 1) {
        uVar17 = 0;
      }
      else {
        uVar17 = 0;
        do {
          dVar19 = dVar20 * (double)(int)uVar17;
          dVar18 = cos(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8) = (float)dVar18;
          dVar19 = sin(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8 + 4) = (float)dVar19;
          dVar19 = dVar20 * (double)((int)uVar17 + 1);
          dVar18 = cos(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8 + 8) = (float)dVar18;
          dVar19 = sin(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8 + 0xc) = (float)dVar19;
          uVar17 = uVar17 + 2;
        } while (uVar17 != (param_1 & 0x7ffffffe));
      }
      if ((param_1 & 1) != 0) {
        dVar20 = dVar20 * (double)(int)uVar17;
        dVar19 = cos(dVar20);
        *(float *)((longlong)pvVar6 + uVar17 * 8) = (float)dVar19;
        dVar20 = sin(dVar20);
        *(float *)((longlong)pvVar6 + uVar17 * 8 + 4) = (float)dVar20;
      }
    }
    _Memory[2] = 0xffffffff;
  }
  else {
    *(undefined8 *)(_Memory + 0xe) = *(undefined8 *)(param_4 + 0xe);
    _Memory[2] = 0;
    if ((((((((*param_4 != param_1) && (_Memory[2] = 1, param_1 * 2 != *param_4)) &&
            (_Memory[2] = 2, param_1 * 4 != *param_4)) &&
           (((_Memory[2] = 3, param_1 * 8 != *param_4 && (_Memory[2] = 4, param_1 << 4 != *param_4))
            && ((_Memory[2] = 5, param_1 << 5 != *param_4 &&
                ((_Memory[2] = 6, param_1 << 6 != *param_4 &&
                 (_Memory[2] = 7, param_1 << 7 != *param_4)))))))) &&
          (_Memory[2] = 8, param_1 << 8 != *param_4)) &&
         ((((((_Memory[2] = 9, param_1 << 9 != *param_4 &&
              (_Memory[2] = 10, param_1 << 10 != *param_4)) &&
             (_Memory[2] = 0xb, param_1 << 0xb != *param_4)) &&
            ((_Memory[2] = 0xc, param_1 << 0xc != *param_4 &&
             (_Memory[2] = 0xd, param_1 << 0xd != *param_4)))) &&
           (_Memory[2] = 0xe, param_1 << 0xe != *param_4)) &&
          (((_Memory[2] = 0xf, param_1 << 0xf != *param_4 &&
            (_Memory[2] = 0x10, param_1 << 0x10 != *param_4)) &&
           (((_Memory[2] = 0x11, param_1 << 0x11 != *param_4 &&
             (((_Memory[2] = 0x12, param_1 << 0x12 != *param_4 &&
               (_Memory[2] = 0x13, param_1 << 0x13 != *param_4)) &&
              (_Memory[2] = 0x14, param_1 << 0x14 != *param_4)))) &&
            (((_Memory[2] = 0x15, param_1 << 0x15 != *param_4 &&
              (_Memory[2] = 0x16, param_1 << 0x16 != *param_4)) &&
             (_Memory[2] = 0x17, param_1 << 0x17 != *param_4)))))))))) &&
        (((_Memory[2] = 0x18, param_1 << 0x18 != *param_4 &&
          (_Memory[2] = 0x19, param_1 << 0x19 != *param_4)) &&
         ((_Memory[2] = 0x1a, param_1 << 0x1a != *param_4 &&
          (((_Memory[2] = 0x1b, param_1 << 0x1b != *param_4 &&
            (_Memory[2] = 0x1c, param_1 << 0x1c != *param_4)) &&
           (_Memory[2] = 0x1d, param_1 << 0x1d != *param_4)))))))) &&
       ((_Memory[2] = 0x1e, param_1 << 0x1e != *param_4 &&
        (_Memory[2] = 0x1f, param_1 << 0x1f != *param_4)))) {
      _Memory[2] = 0x20;
      goto LAB_180004e97;
    }
  }
  puVar1 = _Memory + 3;
  iVar15 = 4;
  iVar13 = -1;
  uVar8 = (ulonglong)param_1;
  uVar17 = 1;
  uVar14 = 0;
  bVar3 = true;
  do {
    bVar2 = bVar3;
    uVar10 = uVar14;
    uVar12 = uVar17;
    iVar7 = (int)uVar8;
    if (iVar7 % iVar15 != 0) {
      do {
        if (iVar15 == 4) {
          iVar16 = 2;
        }
        else if (iVar15 == 2) {
          iVar16 = 3;
        }
        else {
          iVar16 = iVar15 + 2;
        }
        iVar15 = iVar7;
        if (iVar16 * iVar16 <= iVar7) {
          iVar15 = iVar16;
        }
        if (32000 < iVar16) {
          iVar15 = iVar7;
        }
      } while (iVar7 % iVar15 != 0);
      if (5 < iVar15) goto LAB_180004e97;
    }
    *(short *)(puVar1 + uVar10) = (short)iVar15;
    uVar8 = (longlong)iVar7 / (longlong)iVar15 & 0xffffffff;
    if (1 < uVar10 && iVar15 == 2) {
      *(undefined2 *)(_Memory + uVar10 + 3) = 4;
      *(undefined2 *)(_Memory + 4) = 2;
    }
    iVar13 = iVar13 + 1;
    uVar17 = uVar12 + 1;
    uVar14 = uVar10 + 1;
    bVar3 = (bool)(bVar2 ^ 1);
  } while (1 < (int)((longlong)iVar7 / (longlong)iVar15));
  if (uVar10 != 0) {
    uVar17 = uVar12 >> 1 & 0x7fffffff;
    if ((uint)uVar17 < 2) {
      lVar9 = 0;
    }
    else {
      puVar11 = _Memory + 4;
      uVar14 = uVar12 >> 1 & 0x7fffffff;
      lVar9 = 0;
      do {
        uVar4 = puVar11[-1];
        *(short *)(puVar11 + -1) = (short)_Memory[(longlong)iVar13 + lVar9 + 3];
        *(short *)(_Memory + (longlong)iVar13 + lVar9 + 3) = (short)uVar4;
        uVar4 = *puVar11;
        *(short *)puVar11 = (short)_Memory[(longlong)iVar13 + lVar9 + 2];
        *(short *)(_Memory + (longlong)iVar13 + lVar9 + 2) = (short)uVar4;
        puVar11 = puVar11 + 2;
        lVar9 = lVar9 + -2;
      } while (-lVar9 != (ulonglong)((int)uVar14 + (uint)(uVar14 == 0) & 0x7ffffffe));
      lVar9 = -lVar9;
    }
    if ((uVar17 + (uVar17 == 0) & 1) != 0) {
      uVar4 = puVar1[lVar9];
      lVar5 = (int)uVar10 - lVar9;
      *(short *)(puVar1 + lVar9) = (short)puVar1[lVar5];
      *(short *)(puVar1 + lVar5) = (short)uVar4;
    }
  }
  if (uVar10 == 0) {
    lVar9 = 0;
    uVar4 = param_1;
  }
  else {
    uVar12 = uVar12 & 0xfffffffffffffffe;
    lVar9 = 0;
    uVar17 = (ulonglong)param_1;
    do {
      uVar17 = (longlong)(int)uVar17 /
               (longlong)(int)*(short *)((longlong)_Memory + lVar9 * 2 + 0xc);
      *(short *)((longlong)_Memory + lVar9 * 2 + 0xe) = (short)uVar17;
      uVar14 = (longlong)((ulonglong)(uint)((int)uVar17 >> 0x1f) << 0x20 | uVar17 & 0xffffffff) /
               (longlong)(int)*(short *)((longlong)_Memory + lVar9 * 2 + 0x10);
      uVar17 = uVar14 & 0xffffffff;
      uVar4 = (uint)uVar14;
      *(short *)((longlong)_Memory + lVar9 * 2 + 0x12) = (short)uVar17;
      lVar9 = lVar9 + 4;
      uVar12 = uVar12 - 2;
    } while (uVar12 != 0);
  }
  if (bVar2) {
    *(short *)((longlong)_Memory + lVar9 * 2 + 0xe) =
         (short)((int)uVar4 / (int)*(short *)((longlong)puVar1 + lVar9 * 2));
  }
  pvVar6 = malloc((longlong)(int)param_1 << 2);
  *(void **)(_Memory + 0xc) = pvVar6;
  if (pvVar6 != (void *)0x0) {
    FUN_180004ef0(0,pvVar6,1,puVar1);
    return _Memory;
  }
LAB_180004e97:
  free(*(void **)(_Memory + 0xc));
  if ((int)_Memory[2] < 0) {
    free(*(void **)(_Memory + 0xe));
  }
  free(_Memory);
  return (uint *)0x0;
}



/* ========================================================================
   ENTRY: 180004ef0
   NAME : FUN_180004ef0
   SIG  : undefined FUN_180004ef0(void)
   ======================================================================== */

void FUN_180004ef0(int param_1,int *param_2,longlong param_3,ushort *param_4)

{
  ushort uVar1;
  ushort uVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  int *piVar6;
  longlong lVar7;
  
  uVar1 = *param_4;
  uVar5 = (uint)(short)uVar1;
  lVar7 = (longlong)(int)uVar5;
  uVar2 = param_4[1];
  if ((short)uVar2 == 1) {
    if (0 < (short)uVar1) {
      uVar3 = uVar5 & 7;
      uVar4 = 0;
      if (7 < uVar1) {
        uVar4 = 0;
        do {
          piVar6 = param_2;
          *piVar6 = param_1 + uVar4;
          piVar6[param_3] = param_1 + uVar4 + 1;
          piVar6[param_3 * 2] = param_1 + uVar4 + 2;
          piVar6[param_3 * 3] = param_1 + uVar4 + 3;
          piVar6[param_3 * 4] = param_1 + uVar4 + 4;
          piVar6[param_3 * 5] = param_1 + uVar4 + 5;
          piVar6[param_3 * 6] = param_1 + uVar4 + 6;
          piVar6[param_3 * 7] = param_1 + uVar4 + 7;
          uVar4 = uVar4 + 8;
          param_2 = piVar6 + param_3 * 8;
        } while ((uVar5 & 0x7ff8) != uVar4);
        param_2 = piVar6 + param_3 * 8;
      }
      if (uVar3 != 0) {
        param_1 = uVar4 + param_1;
        do {
          *param_2 = param_1;
          param_1 = param_1 + 1;
          param_2 = param_2 + param_3;
          uVar3 = uVar3 - 1;
        } while (uVar3 != 0);
      }
    }
  }
  else if (0 < (short)uVar1) {
    do {
      FUN_180004ef0(param_1,param_2,lVar7 * param_3,param_4 + 2);
      param_1 = param_1 + (short)uVar2;
      param_2 = param_2 + param_3;
      uVar5 = uVar5 - 1;
    } while (uVar5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180005050
   NAME : rnn_fft_free
   SIG  : undefined rnn_fft_free(void)
   ======================================================================== */

void rnn_fft_free(void *param_1)

{
                    /* 0x5050  18  rnn_fft_free */
  if (param_1 != (void *)0x0) {
    free(*(void **)((longlong)param_1 + 0x30));
    if (*(int *)((longlong)param_1 + 8) < 0) {
      free(*(void **)((longlong)param_1 + 0x38));
    }
                    /* WARNING: Could not recover jumptable at 0x00018000507f. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    free(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180005090
   NAME : rnn_fft_alloc
   SIG  : undefined rnn_fft_alloc(void)
   ======================================================================== */

void rnn_fft_alloc(void)

{
                    /* 0x5090  14  rnn_fft_alloc */
  rnn_fft_alloc_twiddles();
  return;
}



/* ========================================================================
   ENTRY: 1800050b0
   NAME : rnn_fft_free_arch_c
   SIG  : undefined rnn_fft_free_arch_c(void)
   ======================================================================== */

void rnn_fft_free_arch_c(void)

{
                    /* 0x50b0  19  rnn_fft_free_arch_c */
  return;
}



/* ========================================================================
   ENTRY: 1800050c0
   NAME : rnn_fft_impl
   SIG  : undefined rnn_fft_impl(void)
   ======================================================================== */

void rnn_fft_impl(longlong param_1,longlong param_2)

{
  short sVar1;
  ushort uVar2;
  int iVar3;
  undefined1 auStack_6e8 [44];
  int local_6bc;
  uint local_6a4;
  longlong local_690;
  longlong local_608;
  longlong local_4c8;
  longlong local_4b0;
  longlong local_370;
  longlong local_358;
  longlong local_1d8;
  longlong lStack_1d0;
  longlong lStack_1c8;
  longlong lStack_1c0;
  longlong local_1b8;
  longlong lStack_1b0;
  longlong lStack_1a8;
  longlong lStack_1a0;
  undefined8 local_198;
  undefined8 uStack_190;
  undefined8 uStack_188;
  undefined8 uStack_180;
  undefined8 local_178;
  undefined8 uStack_170;
  undefined8 uStack_168;
  undefined8 uStack_160;
  undefined8 local_158;
  undefined8 uStack_150;
  undefined8 uStack_148;
  undefined8 uStack_140;
  undefined8 local_138;
  undefined8 uStack_130;
  undefined8 uStack_128;
  undefined8 uStack_120;
  undefined4 local_118;
  int aiStack_114 [9];
  ulonglong local_f0;
  
                    /* 0x50c0  20  rnn_fft_impl */
  local_f0 = DAT_180580000 ^ (ulonglong)auStack_6e8;
  local_6a4 = *(uint *)(param_1 + 8);
  local_118 = 1;
  iVar3 = 1;
  local_358 = -1;
  do {
    iVar3 = iVar3 * *(short *)(param_1 + 0x10 + local_358 * 4);
    sVar1 = *(short *)(param_1 + 0x12 + local_358 * 4);
    aiStack_114[local_358 + 1] = iVar3;
    local_358 = local_358 + 1;
  } while (sVar1 != 1);
  local_6a4 = ~((int)local_6a4 >> 0x1f) & local_6a4;
  local_370 = param_2 + -4;
  local_608 = param_2 + 4;
  local_138 = DAT_18000f168;
  uStack_130 = DAT_18000f168;
  uStack_128 = DAT_18000f168;
  uStack_120 = DAT_18000f168;
  local_158 = DAT_18000f170;
  uStack_150 = DAT_18000f170;
  uStack_148 = DAT_18000f170;
  uStack_140 = DAT_18000f170;
  local_178 = DAT_18000f178;
  uStack_170 = DAT_18000f178;
  uStack_168 = DAT_18000f178;
  uStack_160 = DAT_18000f178;
  local_198 = DAT_18000f180;
  uStack_190 = DAT_18000f180;
  uStack_188 = DAT_18000f180;
  uStack_180 = DAT_18000f180;
  while( true ) {
    local_690 = local_358;
    local_358 = local_690 + -1;
    if (local_690 == 0) {
      local_6bc = 1;
      uVar2 = *(ushort *)(param_1 + 0xc);
    }
    else {
      local_6bc = (int)*(short *)(param_1 + 0xc + (ulonglong)((int)local_690 * 2 - 1) * 2);
      uVar2 = *(ushort *)(param_1 + 0xc + local_690 * 4);
    }
    local_4c8 = param_2;
    local_4b0 = param_1;
    local_1d8 = local_608;
    lStack_1d0 = local_608;
    lStack_1c8 = local_608;
    lStack_1c0 = local_608;
    local_1b8 = param_2;
    lStack_1b0 = param_2;
    lStack_1a8 = param_2;
    lStack_1a0 = param_2;
    if (uVar2 - 2 < 4) break;
    if (local_690 < 1) {
      if ((local_f0 ^ (ulonglong)auStack_6e8) == DAT_180580000) {
        return;
      }
                    /* WARNING: Subroutine does not return */
      FUN_18000db80();
    }
  }
                    /* WARNING: Could not recover jumptable at 0x0001800052da. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*(code *)(&DAT_18000f188 + *(int *)(&DAT_18000f188 + (ulonglong)(uVar2 - 2) * 4)))();
  return;
}



/* ========================================================================
   ENTRY: 180007de0
   NAME : rnn_fft_c
   SIG  : undefined rnn_fft_c(void)
   ======================================================================== */

void rnn_fft_c(int *param_1,longlong param_2,longlong param_3)

{
  float fVar1;
  float fVar2;
  longlong lVar3;
  
                    /* 0x7de0  17  rnn_fft_c */
  if (0 < *param_1) {
    fVar1 = (float)param_1[1];
    lVar3 = 0;
    do {
      fVar2 = *(float *)(param_2 + 4 + lVar3 * 8);
      *(float *)(param_3 + (longlong)*(int *)(*(longlong *)(param_1 + 0xc) + lVar3 * 4) * 8) =
           fVar1 * *(float *)(param_2 + lVar3 * 8);
      *(float *)(param_3 + 4 + (longlong)*(int *)(*(longlong *)(param_1 + 0xc) + lVar3 * 4) * 8) =
           fVar1 * fVar2;
      lVar3 = lVar3 + 1;
    } while (lVar3 < *param_1);
  }
  rnn_fft_impl(param_1,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180007e30
   NAME : rnn_ifft_c
   SIG  : undefined rnn_ifft_c(void)
   ======================================================================== */

void rnn_ifft_c(int *param_1,longlong param_2,longlong param_3)

{
  uint uVar1;
  longlong lVar2;
  
                    /* 0x7e30  22  rnn_ifft_c */
  if (0 < *param_1) {
    lVar2 = 0;
    do {
      *(undefined8 *)(param_3 + (longlong)*(int *)(*(longlong *)(param_1 + 0xc) + lVar2 * 4) * 8) =
           *(undefined8 *)(param_2 + lVar2 * 8);
      uVar1 = DAT_18000f000;
      lVar2 = lVar2 + 1;
    } while (lVar2 < *param_1);
    if (0 < *param_1) {
      lVar2 = 0;
      do {
        *(uint *)(param_3 + 4 + lVar2 * 8) = *(uint *)(param_3 + 4 + lVar2 * 8) ^ uVar1;
        lVar2 = lVar2 + 1;
      } while (lVar2 < *param_1);
    }
  }
  rnn_fft_impl(param_1,param_3);
  uVar1 = DAT_18000f000;
  if (0 < *param_1) {
    lVar2 = 0;
    do {
      *(uint *)(param_3 + 4 + lVar2 * 8) = *(uint *)(param_3 + 4 + lVar2 * 8) ^ uVar1;
      lVar2 = lVar2 + 1;
    } while (lVar2 < *param_1);
  }
  return;
}



/* ========================================================================
   ENTRY: 180007ef0
   NAME : rnn_compute_generic_dense
   SIG  : undefined rnn_compute_generic_dense(void)
   ======================================================================== */

void rnn_compute_generic_dense
               (longlong param_1,undefined8 param_2,undefined8 param_3,undefined4 param_4)

{
                    /* 0x7ef0  9  rnn_compute_generic_dense */
  rnn_compute_linear_c();
  rnn_compute_activation_c(param_2,param_2,*(undefined4 *)(param_1 + 0x3c),param_4);
  return;
}



/* ========================================================================
   ENTRY: 180007f20
   NAME : rnn_compute_generic_gru
   SIG  : undefined rnn_compute_generic_gru(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_compute_generic_gru(undefined8 param_1,longlong param_2,void *param_3,undefined8 param_4)

{
  longlong lVar1;
  longlong lVar2;
  float *pfVar3;
  float *pfVar4;
  float *pfVar5;
  float *pfVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  float fVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  float fVar20;
  float fVar21;
  float fVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  float fVar33;
  float fVar34;
  undefined1 auVar10 [32];
  float fVar35;
  float fVar36;
  float fVar37;
  float fVar38;
  uint uVar39;
  undefined1 auVar40 [32];
  undefined1 auVar41 [32];
  undefined1 auVar42 [32];
  undefined1 auVar43 [32];
  uint uVar44;
  float fVar45;
  float fVar46;
  float fVar47;
  float fVar48;
  float fVar49;
  float fVar50;
  float fVar51;
  float fVar52;
  float fVar53;
  float fVar54;
  float fVar55;
  float fVar56;
  float fVar57;
  float fVar58;
  float fVar59;
  float fVar60;
  float fVar61;
  float fVar62;
  float fVar63;
  float fVar64;
  float fVar65;
  float fVar66;
  float fVar67;
  float fVar68;
  float fVar69;
  float fVar70;
  float fVar71;
  float fVar72;
  float fVar73;
  float fVar74;
  float fVar75;
  float fVar76;
  float fVar77;
  float fVar78;
  float fVar79;
  float fVar80;
  float fVar81;
  float fVar82;
  float fVar83;
  float fVar84;
  float fVar85;
  float fVar86;
  float fVar87;
  float fVar88;
  float fVar89;
  float fVar90;
  float fVar91;
  float fVar92;
  float fVar93;
  float fVar94;
  float fVar95;
  float fVar96;
  float fVar97;
  float fVar98;
  float fVar99;
  float fVar100;
  float fVar101;
  float fVar102;
  float fVar103;
  float fVar104;
  float fVar105;
  float fVar106;
  ulonglong uVar107;
  uint uVar108;
  ulonglong uVar109;
  ulonglong uVar110;
  float *_Src;
  ulonglong uVar111;
  longlong lVar112;
  longlong lVar113;
  float *pfVar114;
  longlong lVar115;
  undefined1 auVar116 [32];
  undefined1 auVar117 [32];
  undefined1 auVar118 [32];
  undefined1 auVar119 [32];
  undefined1 auStack_6098 [32];
  float local_6078 [3072];
  float local_3078 [8];
  float afStack_3058 [8];
  float afStack_3038 [8];
  float afStack_3018 [3050];
  ulonglong local_70;
  
                    /* 0x7f20  10  rnn_compute_generic_gru */
  local_70 = DAT_180580000 ^ (ulonglong)auStack_6098;
  uVar39 = *(uint *)(param_2 + 0x38);
  uVar111 = (ulonglong)(int)uVar39;
  uVar44 = uVar39 * 2;
  rnn_compute_linear_c(param_1,local_3078,param_4);
  rnn_compute_linear_c(param_2,local_6078,param_3);
  lVar115 = (longlong)(int)uVar44;
  _Src = local_3078 + lVar115;
  if ((longlong)uVar111 < 1) {
    rnn_compute_activation_c(local_3078,local_3078,uVar44,1);
  }
  else {
    uVar107 = 1;
    if (1 < (int)uVar44) {
      uVar107 = (ulonglong)uVar44;
    }
    if ((int)uVar44 < 0x20) {
      uVar109 = 0;
LAB_180008060:
      do {
        local_3078[uVar109] = local_6078[uVar109] + local_3078[uVar109];
        uVar109 = uVar109 + 1;
      } while (uVar107 != uVar109);
    }
    else {
      uVar108 = (uint)uVar107 & 0x7fffffe0;
      uVar109 = (ulonglong)uVar108;
      uVar110 = 0;
      do {
        fVar7 = local_6078[uVar110 + 1];
        fVar8 = local_6078[uVar110 + 2];
        fVar9 = local_6078[uVar110 + 3];
        fVar11 = local_6078[uVar110 + 4];
        fVar12 = local_6078[uVar110 + 5];
        fVar13 = local_6078[uVar110 + 6];
        fVar14 = local_6078[uVar110 + 7];
        auVar117._0_4_ = local_6078[uVar110 + 8] + afStack_3058[uVar110];
        auVar117._4_4_ = local_6078[uVar110 + 9] + afStack_3058[uVar110 + 1];
        auVar117._8_4_ = local_6078[uVar110 + 10] + afStack_3058[uVar110 + 2];
        auVar117._12_4_ = local_6078[uVar110 + 0xb] + afStack_3058[uVar110 + 3];
        auVar117._16_4_ = local_6078[uVar110 + 0xc] + afStack_3058[uVar110 + 4];
        auVar117._20_4_ = local_6078[uVar110 + 0xd] + afStack_3058[uVar110 + 5];
        auVar117._24_4_ = local_6078[uVar110 + 0xe] + afStack_3058[uVar110 + 6];
        auVar117._28_4_ = local_6078[uVar110 + 0xf] + afStack_3058[uVar110 + 7];
        auVar118._0_4_ = local_6078[uVar110 + 0x10] + afStack_3038[uVar110];
        auVar118._4_4_ = local_6078[uVar110 + 0x11] + afStack_3038[uVar110 + 1];
        auVar118._8_4_ = local_6078[uVar110 + 0x12] + afStack_3038[uVar110 + 2];
        auVar118._12_4_ = local_6078[uVar110 + 0x13] + afStack_3038[uVar110 + 3];
        auVar118._16_4_ = local_6078[uVar110 + 0x14] + afStack_3038[uVar110 + 4];
        auVar118._20_4_ = local_6078[uVar110 + 0x15] + afStack_3038[uVar110 + 5];
        auVar118._24_4_ = local_6078[uVar110 + 0x16] + afStack_3038[uVar110 + 6];
        auVar118._28_4_ = local_6078[uVar110 + 0x17] + afStack_3038[uVar110 + 7];
        auVar119._0_4_ = local_6078[uVar110 + 0x18] + afStack_3018[uVar110];
        auVar119._4_4_ = local_6078[uVar110 + 0x19] + afStack_3018[uVar110 + 1];
        auVar119._8_4_ = local_6078[uVar110 + 0x1a] + afStack_3018[uVar110 + 2];
        auVar119._12_4_ = local_6078[uVar110 + 0x1b] + afStack_3018[uVar110 + 3];
        auVar119._16_4_ = local_6078[uVar110 + 0x1c] + afStack_3018[uVar110 + 4];
        auVar119._20_4_ = local_6078[uVar110 + 0x1d] + afStack_3018[uVar110 + 5];
        auVar119._24_4_ = local_6078[uVar110 + 0x1e] + afStack_3018[uVar110 + 6];
        auVar119._28_4_ = local_6078[uVar110 + 0x1f] + afStack_3018[uVar110 + 7];
        local_3078[uVar110] = local_6078[uVar110] + local_3078[uVar110];
        local_3078[uVar110 + 1] = fVar7 + local_3078[uVar110 + 1];
        local_3078[uVar110 + 2] = fVar8 + local_3078[uVar110 + 2];
        local_3078[uVar110 + 3] = fVar9 + local_3078[uVar110 + 3];
        local_3078[uVar110 + 4] = fVar11 + local_3078[uVar110 + 4];
        local_3078[uVar110 + 5] = fVar12 + local_3078[uVar110 + 5];
        local_3078[uVar110 + 6] = fVar13 + local_3078[uVar110 + 6];
        local_3078[uVar110 + 7] = fVar14 + local_3078[uVar110 + 7];
        *(undefined1 (*) [32])(afStack_3058 + uVar110) = auVar117;
        *(undefined1 (*) [32])(afStack_3038 + uVar110) = auVar118;
        *(undefined1 (*) [32])(afStack_3018 + uVar110) = auVar119;
        uVar110 = uVar110 + 0x20;
      } while (uVar109 != uVar110);
      if (uVar108 != (uint)uVar107) goto LAB_180008060;
    }
    rnn_compute_activation_c(local_3078,local_3078,uVar44,1);
    if (0 < (int)uVar39) {
      pfVar114 = local_3078 + uVar111;
      uVar107 = (ulonglong)uVar44;
      if ((uVar39 < 0x20) ||
         ((_Src < local_3078 + uVar111 * 2 && (pfVar114 < local_3078 + lVar115 + uVar111)))) {
        uVar109 = 0;
LAB_1800080eb:
        uVar110 = uVar109;
        if ((uVar39 & 1) != 0) {
          _Src[uVar109] = local_6078[uVar109 + uVar107] * pfVar114[uVar109] + _Src[uVar109];
          uVar110 = uVar109 | 1;
        }
        if (uVar109 != uVar111 - 1) {
          do {
            local_3078[lVar115 + uVar110] =
                 local_6078[uVar107 + uVar110] * local_3078[uVar111 + uVar110] +
                 local_3078[lVar115 + uVar110];
            local_3078[lVar115 + uVar110 + 1] =
                 local_6078[uVar107 + uVar110 + 1] * local_3078[uVar111 + uVar110 + 1] +
                 local_3078[lVar115 + uVar110 + 1];
            uVar110 = uVar110 + 2;
          } while (uVar111 != uVar110);
        }
      }
      else {
        uVar109 = (ulonglong)(uVar39 & 0x7fffffe0);
        lVar2 = uVar107 * 4;
        lVar112 = uVar111 * 4;
        lVar1 = lVar115 * 4;
        lVar113 = 0;
        do {
          pfVar6 = (float *)((longlong)local_6078 + lVar113 + lVar2);
          fVar45 = pfVar6[1];
          fVar46 = pfVar6[2];
          fVar47 = pfVar6[3];
          fVar48 = pfVar6[4];
          fVar49 = pfVar6[5];
          fVar50 = pfVar6[6];
          fVar51 = pfVar6[7];
          pfVar3 = (float *)((longlong)local_6078 + lVar113 + lVar2 + 0x20);
          fVar52 = *pfVar3;
          fVar53 = pfVar3[1];
          fVar54 = pfVar3[2];
          fVar55 = pfVar3[3];
          fVar56 = pfVar3[4];
          fVar57 = pfVar3[5];
          fVar58 = pfVar3[6];
          fVar59 = pfVar3[7];
          pfVar3 = (float *)((longlong)local_6078 + lVar113 + lVar2 + 0x40);
          fVar60 = *pfVar3;
          fVar61 = pfVar3[1];
          fVar62 = pfVar3[2];
          fVar63 = pfVar3[3];
          fVar64 = pfVar3[4];
          fVar65 = pfVar3[5];
          fVar66 = pfVar3[6];
          fVar67 = pfVar3[7];
          pfVar3 = (float *)((longlong)local_6078 + lVar113 + lVar2 + 0x60);
          fVar68 = *pfVar3;
          fVar69 = pfVar3[1];
          fVar70 = pfVar3[2];
          fVar71 = pfVar3[3];
          fVar72 = pfVar3[4];
          fVar73 = pfVar3[5];
          fVar74 = pfVar3[6];
          fVar75 = pfVar3[7];
          pfVar4 = (float *)((longlong)local_3078 + lVar113 + lVar112);
          fVar76 = pfVar4[1];
          fVar77 = pfVar4[2];
          fVar78 = pfVar4[3];
          fVar79 = pfVar4[4];
          fVar80 = pfVar4[5];
          fVar81 = pfVar4[6];
          fVar82 = pfVar4[7];
          pfVar3 = (float *)((longlong)afStack_3058 + lVar113 + lVar112);
          fVar83 = *pfVar3;
          fVar84 = pfVar3[1];
          fVar85 = pfVar3[2];
          fVar86 = pfVar3[3];
          fVar87 = pfVar3[4];
          fVar88 = pfVar3[5];
          fVar89 = pfVar3[6];
          fVar90 = pfVar3[7];
          pfVar3 = (float *)((longlong)afStack_3038 + lVar113 + lVar112);
          fVar91 = *pfVar3;
          fVar92 = pfVar3[1];
          fVar93 = pfVar3[2];
          fVar94 = pfVar3[3];
          fVar95 = pfVar3[4];
          fVar96 = pfVar3[5];
          fVar97 = pfVar3[6];
          fVar98 = pfVar3[7];
          pfVar3 = (float *)((longlong)afStack_3018 + lVar113 + lVar112);
          fVar99 = *pfVar3;
          fVar100 = pfVar3[1];
          fVar101 = pfVar3[2];
          fVar102 = pfVar3[3];
          fVar103 = pfVar3[4];
          fVar104 = pfVar3[5];
          fVar105 = pfVar3[6];
          fVar106 = pfVar3[7];
          pfVar5 = (float *)((longlong)local_3078 + lVar113 + lVar1);
          fVar11 = pfVar5[1];
          fVar15 = pfVar5[2];
          fVar19 = pfVar5[3];
          fVar23 = pfVar5[4];
          fVar27 = pfVar5[5];
          fVar31 = pfVar5[6];
          fVar35 = pfVar5[7];
          pfVar3 = (float *)((longlong)afStack_3058 + lVar113 + lVar1);
          fVar7 = *pfVar3;
          fVar12 = pfVar3[1];
          fVar16 = pfVar3[2];
          fVar20 = pfVar3[3];
          fVar24 = pfVar3[4];
          fVar28 = pfVar3[5];
          fVar32 = pfVar3[6];
          fVar36 = pfVar3[7];
          pfVar3 = (float *)((longlong)afStack_3038 + lVar113 + lVar1);
          fVar8 = *pfVar3;
          fVar13 = pfVar3[1];
          fVar17 = pfVar3[2];
          fVar21 = pfVar3[3];
          fVar25 = pfVar3[4];
          fVar29 = pfVar3[5];
          fVar33 = pfVar3[6];
          fVar37 = pfVar3[7];
          pfVar3 = (float *)((longlong)afStack_3018 + lVar113 + lVar1);
          fVar9 = *pfVar3;
          fVar14 = pfVar3[1];
          fVar18 = pfVar3[2];
          fVar22 = pfVar3[3];
          fVar26 = pfVar3[4];
          fVar30 = pfVar3[5];
          fVar34 = pfVar3[6];
          fVar38 = pfVar3[7];
          pfVar3 = (float *)((longlong)local_3078 + lVar113 + lVar1);
          *pfVar3 = *pfVar6 * *pfVar4 + *pfVar5;
          pfVar3[1] = fVar45 * fVar76 + fVar11;
          pfVar3[2] = fVar46 * fVar77 + fVar15;
          pfVar3[3] = fVar47 * fVar78 + fVar19;
          pfVar3[4] = fVar48 * fVar79 + fVar23;
          pfVar3[5] = fVar49 * fVar80 + fVar27;
          pfVar3[6] = fVar50 * fVar81 + fVar31;
          pfVar3[7] = fVar51 * fVar82 + fVar35;
          pfVar3 = (float *)((longlong)afStack_3058 + lVar113 + lVar1);
          *pfVar3 = fVar52 * fVar83 + fVar7;
          pfVar3[1] = fVar53 * fVar84 + fVar12;
          pfVar3[2] = fVar54 * fVar85 + fVar16;
          pfVar3[3] = fVar55 * fVar86 + fVar20;
          pfVar3[4] = fVar56 * fVar87 + fVar24;
          pfVar3[5] = fVar57 * fVar88 + fVar28;
          pfVar3[6] = fVar58 * fVar89 + fVar32;
          pfVar3[7] = fVar59 * fVar90 + fVar36;
          pfVar3 = (float *)((longlong)afStack_3038 + lVar113 + lVar1);
          *pfVar3 = fVar60 * fVar91 + fVar8;
          pfVar3[1] = fVar61 * fVar92 + fVar13;
          pfVar3[2] = fVar62 * fVar93 + fVar17;
          pfVar3[3] = fVar63 * fVar94 + fVar21;
          pfVar3[4] = fVar64 * fVar95 + fVar25;
          pfVar3[5] = fVar65 * fVar96 + fVar29;
          pfVar3[6] = fVar66 * fVar97 + fVar33;
          pfVar3[7] = fVar67 * fVar98 + fVar37;
          pfVar3 = (float *)((longlong)afStack_3018 + lVar113 + lVar1);
          *pfVar3 = fVar68 * fVar99 + fVar9;
          pfVar3[1] = fVar69 * fVar100 + fVar14;
          pfVar3[2] = fVar70 * fVar101 + fVar18;
          pfVar3[3] = fVar71 * fVar102 + fVar22;
          pfVar3[4] = fVar72 * fVar103 + fVar26;
          pfVar3[5] = fVar73 * fVar104 + fVar30;
          pfVar3[6] = fVar74 * fVar105 + fVar34;
          pfVar3[7] = fVar75 * fVar106 + fVar38;
          lVar113 = lVar113 + 0x80;
        } while ((ulonglong)(uVar39 >> 5 & 0x3ffffff) << 7 != lVar113);
        if (uVar109 != uVar111) goto LAB_1800080eb;
      }
      rnn_compute_activation_c(_Src,_Src,uVar39,2);
      if ((int)uVar39 < 1) goto LAB_180008448;
      if ((uVar39 < 0x20) || ((_Src < pfVar114 && (local_3078 < local_3078 + lVar115 + uVar111)))) {
        uVar107 = 0;
LAB_1800082b9:
        uVar109 = uVar107;
        if ((uVar39 & 1) != 0) {
          _Src[uVar107] =
               local_3078[uVar107] * *(float *)((longlong)param_3 + uVar107 * 4) +
               (DAT_18000f098 - local_3078[uVar107]) * _Src[uVar107];
          uVar109 = uVar107 | 1;
        }
        if (uVar107 != uVar111 - 1) {
          do {
            local_3078[lVar115 + uVar109] =
                 local_3078[uVar109] * *(float *)((longlong)param_3 + uVar109 * 4) +
                 (DAT_18000f098 - local_3078[uVar109]) * local_3078[lVar115 + uVar109];
            local_3078[lVar115 + uVar109 + 1] =
                 local_3078[uVar109 + 1] * *(float *)((longlong)param_3 + uVar109 * 4 + 4) +
                 (DAT_18000f098 - local_3078[uVar109 + 1]) * local_3078[lVar115 + uVar109 + 1];
            uVar109 = uVar109 + 2;
          } while (uVar111 != uVar109);
        }
      }
      else {
        uVar107 = (ulonglong)(uVar39 & 0x7fffffe0);
        lVar2 = lVar115 * 4;
        lVar112 = 0;
        auVar116._4_4_ = DAT_18000f098;
        auVar116._0_4_ = DAT_18000f098;
        auVar116._8_4_ = DAT_18000f098;
        auVar116._12_4_ = DAT_18000f098;
        auVar116._16_4_ = DAT_18000f098;
        auVar116._20_4_ = DAT_18000f098;
        auVar116._24_4_ = DAT_18000f098;
        auVar116._28_4_ = DAT_18000f098;
        do {
          auVar117 = *(undefined1 (*) [32])((longlong)local_3078 + lVar112);
          auVar118 = *(undefined1 (*) [32])((longlong)afStack_3058 + lVar112);
          auVar119 = *(undefined1 (*) [32])((longlong)afStack_3038 + lVar112);
          auVar10 = *(undefined1 (*) [32])((longlong)afStack_3018 + lVar112);
          auVar40 = vsubps_avx(auVar116,auVar117);
          auVar41 = vsubps_avx(auVar116,auVar118);
          auVar42 = vsubps_avx(auVar116,auVar119);
          auVar43 = vsubps_avx(auVar116,auVar10);
          pfVar3 = (float *)((longlong)local_3078 + lVar112 + lVar2);
          fVar45 = pfVar3[1];
          fVar46 = pfVar3[2];
          fVar47 = pfVar3[3];
          fVar48 = pfVar3[4];
          fVar49 = pfVar3[5];
          fVar50 = pfVar3[6];
          fVar51 = pfVar3[7];
          pfVar114 = (float *)((longlong)afStack_3058 + lVar112 + lVar2);
          fVar52 = *pfVar114;
          fVar53 = pfVar114[1];
          fVar54 = pfVar114[2];
          fVar55 = pfVar114[3];
          fVar56 = pfVar114[4];
          fVar57 = pfVar114[5];
          fVar58 = pfVar114[6];
          fVar59 = pfVar114[7];
          pfVar114 = (float *)((longlong)afStack_3038 + lVar112 + lVar2);
          fVar60 = *pfVar114;
          fVar61 = pfVar114[1];
          fVar62 = pfVar114[2];
          fVar63 = pfVar114[3];
          fVar64 = pfVar114[4];
          fVar65 = pfVar114[5];
          fVar66 = pfVar114[6];
          fVar67 = pfVar114[7];
          pfVar114 = (float *)((longlong)afStack_3018 + lVar112 + lVar2);
          fVar68 = *pfVar114;
          fVar69 = pfVar114[1];
          fVar70 = pfVar114[2];
          fVar71 = pfVar114[3];
          fVar72 = pfVar114[4];
          fVar73 = pfVar114[5];
          fVar74 = pfVar114[6];
          fVar75 = pfVar114[7];
          pfVar114 = (float *)((longlong)param_3 + lVar112);
          fVar11 = pfVar114[1];
          fVar15 = pfVar114[2];
          fVar19 = pfVar114[3];
          fVar23 = pfVar114[4];
          fVar27 = pfVar114[5];
          fVar31 = pfVar114[6];
          fVar35 = pfVar114[7];
          pfVar6 = (float *)((longlong)param_3 + lVar112 + 0x20);
          fVar7 = *pfVar6;
          fVar12 = pfVar6[1];
          fVar16 = pfVar6[2];
          fVar20 = pfVar6[3];
          fVar24 = pfVar6[4];
          fVar28 = pfVar6[5];
          fVar32 = pfVar6[6];
          fVar36 = pfVar6[7];
          pfVar6 = (float *)((longlong)param_3 + lVar112 + 0x40);
          fVar8 = *pfVar6;
          fVar13 = pfVar6[1];
          fVar17 = pfVar6[2];
          fVar21 = pfVar6[3];
          fVar25 = pfVar6[4];
          fVar29 = pfVar6[5];
          fVar33 = pfVar6[6];
          fVar37 = pfVar6[7];
          pfVar6 = (float *)((longlong)param_3 + lVar112 + 0x60);
          fVar9 = *pfVar6;
          fVar14 = pfVar6[1];
          fVar18 = pfVar6[2];
          fVar22 = pfVar6[3];
          fVar26 = pfVar6[4];
          fVar30 = pfVar6[5];
          fVar34 = pfVar6[6];
          fVar38 = pfVar6[7];
          pfVar6 = (float *)((longlong)local_3078 + lVar112 + lVar2);
          *pfVar6 = auVar117._0_4_ * *pfVar114 + auVar40._0_4_ * *pfVar3;
          pfVar6[1] = auVar117._4_4_ * fVar11 + auVar40._4_4_ * fVar45;
          pfVar6[2] = auVar117._8_4_ * fVar15 + auVar40._8_4_ * fVar46;
          pfVar6[3] = auVar117._12_4_ * fVar19 + auVar40._12_4_ * fVar47;
          pfVar6[4] = auVar117._16_4_ * fVar23 + auVar40._16_4_ * fVar48;
          pfVar6[5] = auVar117._20_4_ * fVar27 + auVar40._20_4_ * fVar49;
          pfVar6[6] = auVar117._24_4_ * fVar31 + auVar40._24_4_ * fVar50;
          pfVar6[7] = auVar117._28_4_ * fVar35 + auVar40._28_4_ * fVar51;
          pfVar114 = (float *)((longlong)afStack_3058 + lVar112 + lVar2);
          *pfVar114 = auVar118._0_4_ * fVar7 + auVar41._0_4_ * fVar52;
          pfVar114[1] = auVar118._4_4_ * fVar12 + auVar41._4_4_ * fVar53;
          pfVar114[2] = auVar118._8_4_ * fVar16 + auVar41._8_4_ * fVar54;
          pfVar114[3] = auVar118._12_4_ * fVar20 + auVar41._12_4_ * fVar55;
          pfVar114[4] = auVar118._16_4_ * fVar24 + auVar41._16_4_ * fVar56;
          pfVar114[5] = auVar118._20_4_ * fVar28 + auVar41._20_4_ * fVar57;
          pfVar114[6] = auVar118._24_4_ * fVar32 + auVar41._24_4_ * fVar58;
          pfVar114[7] = auVar118._28_4_ * fVar36 + auVar41._28_4_ * fVar59;
          pfVar114 = (float *)((longlong)afStack_3038 + lVar112 + lVar2);
          *pfVar114 = auVar119._0_4_ * fVar8 + auVar42._0_4_ * fVar60;
          pfVar114[1] = auVar119._4_4_ * fVar13 + auVar42._4_4_ * fVar61;
          pfVar114[2] = auVar119._8_4_ * fVar17 + auVar42._8_4_ * fVar62;
          pfVar114[3] = auVar119._12_4_ * fVar21 + auVar42._12_4_ * fVar63;
          pfVar114[4] = auVar119._16_4_ * fVar25 + auVar42._16_4_ * fVar64;
          pfVar114[5] = auVar119._20_4_ * fVar29 + auVar42._20_4_ * fVar65;
          pfVar114[6] = auVar119._24_4_ * fVar33 + auVar42._24_4_ * fVar66;
          pfVar114[7] = auVar119._28_4_ * fVar37 + auVar42._28_4_ * fVar67;
          pfVar114 = (float *)((longlong)afStack_3018 + lVar112 + lVar2);
          *pfVar114 = auVar10._0_4_ * fVar9 + auVar43._0_4_ * fVar68;
          pfVar114[1] = auVar10._4_4_ * fVar14 + auVar43._4_4_ * fVar69;
          pfVar114[2] = auVar10._8_4_ * fVar18 + auVar43._8_4_ * fVar70;
          pfVar114[3] = auVar10._12_4_ * fVar22 + auVar43._12_4_ * fVar71;
          pfVar114[4] = auVar10._16_4_ * fVar26 + auVar43._16_4_ * fVar72;
          pfVar114[5] = auVar10._20_4_ * fVar30 + auVar43._20_4_ * fVar73;
          pfVar114[6] = auVar10._24_4_ * fVar34 + auVar43._24_4_ * fVar74;
          pfVar114[7] = auVar10._28_4_ * fVar38 + auVar43._28_4_ * fVar75;
          lVar112 = lVar112 + 0x80;
        } while ((ulonglong)(uVar39 >> 5 & 0x3ffffff) << 7 != lVar112);
        if (uVar107 != uVar111) goto LAB_1800082b9;
      }
      if (0 < (int)uVar39) {
        memcpy(param_3,_Src,uVar111 << 2);
      }
      goto LAB_180008448;
    }
  }
  rnn_compute_activation_c(_Src,_Src,uVar39,2);
LAB_180008448:
  if ((local_70 ^ (ulonglong)auStack_6098) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 1800084a0
   NAME : rnn_compute_glu
   SIG  : undefined rnn_compute_glu(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_compute_glu(longlong param_1,longlong param_2,longlong param_3)

{
  longlong lVar1;
  undefined1 auStack_2048 [32];
  float local_2028 [2048];
  ulonglong local_28;
  
                    /* 0x84a0  11  rnn_compute_glu */
  local_28 = DAT_180580000 ^ (ulonglong)auStack_2048;
  rnn_compute_linear_c(param_1,local_2028);
  rnn_compute_activation_c(local_2028,local_2028,*(undefined4 *)(param_1 + 0x3c),1);
  if (param_3 == param_2) {
    if (0 < *(int *)(param_1 + 0x3c)) {
      lVar1 = 0;
      do {
        *(float *)(param_2 + lVar1 * 4) = *(float *)(param_2 + lVar1 * 4) * local_2028[lVar1];
        lVar1 = lVar1 + 1;
      } while (lVar1 < *(int *)(param_1 + 0x3c));
    }
  }
  else if (0 < *(int *)(param_1 + 0x3c)) {
    lVar1 = 0;
    do {
      *(float *)(param_2 + lVar1 * 4) = *(float *)(param_3 + lVar1 * 4) * local_2028[lVar1];
      lVar1 = lVar1 + 1;
    } while (lVar1 < *(int *)(param_1 + 0x3c));
  }
  if ((local_28 ^ (ulonglong)auStack_2048) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000db80();
  }
  return;
}



/* ========================================================================
   ENTRY: 180008580
   NAME : rnn_compute_generic_conv1d
   SIG  : undefined rnn_compute_generic_conv1d(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_compute_generic_conv1d
               (longlong param_1,undefined8 param_2,void *param_3,void *param_4,int param_5,
               undefined4 param_6)

{
  int iVar1;
  longlong lVar2;
  undefined1 local_1068 [32];
  undefined1 local_1048 [4104];
  ulonglong local_40;
  
                    /* 0x8580  8  rnn_compute_generic_conv1d */
  local_40 = DAT_180580000 ^ (ulonglong)local_1068;
  iVar1 = *(int *)(param_1 + 0x38) - param_5;
  if (iVar1 == 0) {
    lVar2 = 0;
  }
  else {
    lVar2 = (longlong)iVar1;
    memcpy(local_1048,param_3,lVar2 * 4);
  }
  memcpy(local_1048 + lVar2 * 4,param_4,(longlong)param_5 * 4);
  rnn_compute_linear_c(param_1,param_2,local_1048);
  rnn_compute_activation_c(param_2,param_2,*(undefined4 *)(param_1 + 0x3c),param_6);
  iVar1 = *(int *)(param_1 + 0x38) - param_5;
  if (iVar1 != 0) {
    memcpy(param_3,local_1048 + (longlong)param_5 * 4,(longlong)iVar1 << 2);
  }
  if ((local_40 ^ (ulonglong)local_1068) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180008680
   NAME : rnn_compute_activation_c
   SIG  : undefined rnn_compute_activation_c(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_compute_activation_c
               (longlong param_1,longlong param_2,ulonglong param_3,undefined4 param_4)

{
  float *pfVar1;
  int *piVar2;
  undefined8 *puVar3;
  float *pfVar4;
  undefined8 *puVar5;
  float fVar6;
  float fVar7;
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
  float fVar18;
  float fVar19;
  float fVar20;
  float fVar21;
  float fVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  float fVar33;
  float fVar34;
  float fVar35;
  float fVar36;
  float fVar37;
  float fVar38;
  float fVar39;
  float fVar40;
  float fVar41;
  float fVar42;
  float fVar43;
  float fVar44;
  float fVar45;
  float fVar46;
  float fVar47;
  float fVar48;
  undefined8 uVar49;
  undefined8 uVar50;
  undefined8 uVar51;
  undefined8 uVar52;
  undefined8 uVar53;
  undefined8 uVar54;
  undefined8 uVar55;
  undefined8 uVar56;
  undefined8 uVar57;
  undefined8 uVar58;
  undefined8 uVar59;
  undefined8 uVar60;
  undefined8 uVar61;
  undefined8 uVar62;
  undefined8 uVar63;
  uint uVar64;
  uint uVar65;
  uint uVar66;
  ulonglong uVar67;
  ulonglong uVar68;
  longlong lVar69;
  ulonglong uVar70;
  float fVar71;
  undefined1 auVar72 [16];
  undefined1 auVar73 [16];
  undefined1 auVar74 [16];
  undefined1 auVar75 [32];
  undefined1 auVar76 [32];
  undefined1 auVar77 [32];
  undefined1 auVar78 [32];
  undefined1 auVar79 [32];
  float fVar80;
  undefined1 auVar81 [16];
  float fVar87;
  float fVar88;
  undefined1 auVar82 [32];
  undefined1 auVar83 [32];
  undefined1 auVar84 [32];
  undefined1 auVar85 [32];
  undefined1 auVar86 [32];
  undefined1 auVar89 [16];
  undefined1 auVar90 [32];
  float fVar91;
  float fVar92;
  float fVar93;
  float fVar94;
  float fVar95;
  float fVar96;
  float fVar97;
  float fVar98;
  float fVar99;
  float fVar100;
  float fVar101;
  float fVar102;
  float fVar103;
  float fVar104;
  float fVar105;
  undefined1 auVar106 [32];
  undefined1 auVar107 [32];
  undefined1 auVar108 [16];
  undefined1 auVar109 [16];
  undefined1 auVar110 [32];
  undefined1 auVar111 [32];
  undefined1 auVar112 [32];
  undefined1 auVar113 [32];
  undefined1 auVar114 [32];
  undefined1 auVar115 [32];
  undefined1 auStack_40a8 [32];
  float afStack_4088 [4098];
  ulonglong local_80;
  undefined8 uStack_8;
  
  fVar97 = DAT_18000f1ec;
  fVar105 = DAT_18000f1e8;
  fVar103 = DAT_18000f1e4;
  fVar102 = DAT_18000f1e0;
  fVar101 = DAT_18000f1dc;
  fVar100 = DAT_18000f1d8;
  fVar99 = DAT_18000f1d4;
  fVar98 = DAT_18000f1d0;
  fVar96 = DAT_18000f1cc;
  fVar88 = DAT_18000f1c8;
  fVar87 = DAT_18000f1c4;
  fVar80 = DAT_18000f1c0;
  fVar9 = DAT_18000f1b4;
  fVar8 = DAT_18000f1b0;
  fVar7 = DAT_18000f1ac;
  fVar6 = DAT_18000f1a8;
                    /* 0x8680  5  rnn_compute_activation_c */
  fVar71 = DAT_18000f19c;
  uStack_8 = 0x18000868a;
  local_80 = DAT_180580000 ^ (ulonglong)auStack_40a8;
  uVar66 = (uint)param_3;
  switch(param_4) {
  case 1:
    uVar67 = 0;
    if (7 < (int)uVar66) {
      uVar67 = 0;
      auVar82._4_4_ = DAT_18000f098;
      auVar82._0_4_ = DAT_18000f098;
      auVar82._8_4_ = DAT_18000f098;
      auVar82._12_4_ = DAT_18000f098;
      auVar82._16_4_ = DAT_18000f098;
      auVar82._20_4_ = DAT_18000f098;
      auVar82._24_4_ = DAT_18000f098;
      auVar82._28_4_ = DAT_18000f098;
      do {
        pfVar1 = (float *)(param_2 + uVar67 * 4);
        fVar71 = *pfVar1;
        fVar6 = pfVar1[1];
        fVar7 = pfVar1[2];
        fVar8 = pfVar1[3];
        fVar9 = pfVar1[4];
        fVar101 = pfVar1[5];
        fVar102 = pfVar1[6];
        fVar103 = pfVar1[7];
        fVar105 = fVar71 * fVar71;
        fVar97 = fVar6 * fVar6;
        fVar91 = fVar7 * fVar7;
        fVar92 = fVar8 * fVar8;
        fVar93 = fVar9 * fVar9;
        fVar94 = fVar101 * fVar101;
        fVar95 = fVar102 * fVar102;
        fVar104 = fVar103 * fVar103;
        auVar110._0_4_ = fVar105 * (fVar105 * fVar98 + fVar96) + fVar99;
        auVar110._4_4_ = fVar97 * (fVar97 * fVar98 + fVar96) + fVar99;
        auVar110._8_4_ = fVar91 * (fVar91 * fVar98 + fVar96) + fVar99;
        auVar110._12_4_ = fVar92 * (fVar92 * fVar98 + fVar96) + fVar99;
        auVar110._16_4_ = fVar93 * (fVar93 * fVar98 + fVar96) + fVar99;
        auVar110._20_4_ = fVar94 * (fVar94 * fVar98 + fVar96) + fVar99;
        auVar110._24_4_ = fVar95 * (fVar95 * fVar98 + fVar96) + fVar99;
        auVar110._28_4_ = fVar104 * (fVar104 * fVar98 + fVar96) + fVar99;
        auVar84 = vrcpps_avx(auVar110);
        auVar111._0_4_ =
             fVar71 * (fVar105 * (fVar105 * fVar87 + fVar80) + fVar88) * auVar84._0_4_ + fVar100;
        auVar111._4_4_ =
             fVar6 * (fVar97 * (fVar97 * fVar87 + fVar80) + fVar88) * auVar84._4_4_ + fVar100;
        auVar111._8_4_ =
             fVar7 * (fVar91 * (fVar91 * fVar87 + fVar80) + fVar88) * auVar84._8_4_ + fVar100;
        auVar111._12_4_ =
             fVar8 * (fVar92 * (fVar92 * fVar87 + fVar80) + fVar88) * auVar84._12_4_ + fVar100;
        auVar111._16_4_ =
             fVar9 * (fVar93 * (fVar93 * fVar87 + fVar80) + fVar88) * auVar84._16_4_ + fVar100;
        auVar111._20_4_ =
             fVar101 * (fVar94 * (fVar94 * fVar87 + fVar80) + fVar88) * auVar84._20_4_ + fVar100;
        auVar111._24_4_ =
             fVar102 * (fVar95 * (fVar95 * fVar87 + fVar80) + fVar88) * auVar84._24_4_ + fVar100;
        auVar111._28_4_ =
             fVar103 * (fVar104 * (fVar104 * fVar87 + fVar80) + fVar88) * auVar84._28_4_ + fVar100;
        auVar84 = vminps_avx(auVar82,auVar111);
        auVar84 = vmaxps_avx(ZEXT1632(ZEXT816(0)),auVar84);
        *(undefined1 (*) [32])(param_1 + uVar67 * 4) = auVar84;
        uVar67 = uVar67 + 8;
      } while (uVar67 < uVar66 - 7);
    }
    fVar87 = DAT_18000f1d8;
    fVar80 = DAT_18000f1d4;
    fVar9 = DAT_18000f1d0;
    fVar8 = DAT_18000f1cc;
    fVar7 = DAT_18000f1c8;
    fVar6 = DAT_18000f1c4;
    fVar71 = DAT_18000f1c0;
    uVar64 = DAT_18000f098;
    if ((int)uVar67 < (int)uVar66) {
      uVar67 = uVar67 & 0xffffffff;
      auVar89._0_12_ = ZEXT812(0);
      auVar89._12_4_ = 0;
      do {
        fVar88 = *(float *)(param_2 + uVar67 * 4);
        fVar96 = fVar88 * fVar88;
        auVar112._0_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._4_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._8_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._12_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._16_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._20_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._24_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar112._28_4_ = fVar96 * (fVar96 * fVar9 + fVar8) + fVar80;
        auVar84 = vrcpps_avx(auVar112);
        auVar108._0_4_ =
             fVar88 * (fVar96 * (fVar96 * fVar6 + fVar71) + fVar7) * auVar84._0_4_ + fVar87;
        auVar108._4_4_ =
             fVar88 * (fVar96 * (fVar96 * fVar6 + fVar71) + fVar7) * auVar84._4_4_ + fVar87;
        auVar108._8_4_ =
             fVar88 * (fVar96 * (fVar96 * fVar6 + fVar71) + fVar7) * auVar84._8_4_ + fVar87;
        auVar108._12_4_ =
             fVar88 * (fVar96 * (fVar96 * fVar6 + fVar71) + fVar7) * auVar84._12_4_ + fVar87;
        auVar81 = vminss_avx(ZEXT416(uVar64),auVar108);
        auVar81 = vmaxss_avx(auVar89,auVar81);
        *(int *)(param_1 + uVar67 * 4) = auVar81._0_4_;
        uVar67 = uVar67 + 1;
      } while ((param_3 & 0xffffffff) != uVar67);
    }
    break;
  case 2:
    uVar67 = 0;
    if (7 < (int)uVar66) {
      uVar67 = 0;
      auVar79._4_4_ = DAT_18000f098;
      auVar79._0_4_ = DAT_18000f098;
      auVar79._8_4_ = DAT_18000f098;
      auVar79._12_4_ = DAT_18000f098;
      auVar79._16_4_ = DAT_18000f098;
      auVar79._20_4_ = DAT_18000f098;
      auVar79._24_4_ = DAT_18000f098;
      auVar79._28_4_ = DAT_18000f098;
      auVar86._4_4_ = DAT_18000f1f0;
      auVar86._0_4_ = DAT_18000f1f0;
      auVar86._8_4_ = DAT_18000f1f0;
      auVar86._12_4_ = DAT_18000f1f0;
      auVar86._16_4_ = DAT_18000f1f0;
      auVar86._20_4_ = DAT_18000f1f0;
      auVar86._24_4_ = DAT_18000f1f0;
      auVar86._28_4_ = DAT_18000f1f0;
      do {
        pfVar1 = (float *)(param_2 + uVar67 * 4);
        fVar71 = *pfVar1;
        fVar6 = pfVar1[1];
        fVar7 = pfVar1[2];
        fVar8 = pfVar1[3];
        fVar9 = pfVar1[4];
        fVar80 = pfVar1[5];
        fVar87 = pfVar1[6];
        fVar88 = pfVar1[7];
        fVar96 = fVar71 * fVar71;
        fVar98 = fVar6 * fVar6;
        fVar100 = fVar7 * fVar7;
        fVar91 = fVar8 * fVar8;
        fVar92 = fVar9 * fVar9;
        fVar93 = fVar80 * fVar80;
        fVar94 = fVar87 * fVar87;
        fVar95 = fVar88 * fVar88;
        auVar106._0_4_ = fVar96 * (fVar96 * fVar97 + fVar105) + fVar99;
        auVar106._4_4_ = fVar98 * (fVar98 * fVar97 + fVar105) + fVar99;
        auVar106._8_4_ = fVar100 * (fVar100 * fVar97 + fVar105) + fVar99;
        auVar106._12_4_ = fVar91 * (fVar91 * fVar97 + fVar105) + fVar99;
        auVar106._16_4_ = fVar92 * (fVar92 * fVar97 + fVar105) + fVar99;
        auVar106._20_4_ = fVar93 * (fVar93 * fVar97 + fVar105) + fVar99;
        auVar106._24_4_ = fVar94 * (fVar94 * fVar97 + fVar105) + fVar99;
        auVar106._28_4_ = fVar95 * (fVar95 * fVar97 + fVar105) + fVar99;
        auVar84 = vrcpps_avx(auVar106);
        auVar90._0_4_ = auVar84._0_4_ * fVar71 * (fVar96 * (fVar96 * fVar102 + fVar101) + fVar103);
        auVar90._4_4_ = auVar84._4_4_ * fVar6 * (fVar98 * (fVar98 * fVar102 + fVar101) + fVar103);
        auVar90._8_4_ = auVar84._8_4_ * fVar7 * (fVar100 * (fVar100 * fVar102 + fVar101) + fVar103);
        auVar90._12_4_ = auVar84._12_4_ * fVar8 * (fVar91 * (fVar91 * fVar102 + fVar101) + fVar103);
        auVar90._16_4_ = auVar84._16_4_ * fVar9 * (fVar92 * (fVar92 * fVar102 + fVar101) + fVar103);
        auVar90._20_4_ = auVar84._20_4_ * fVar80 * (fVar93 * (fVar93 * fVar102 + fVar101) + fVar103)
        ;
        auVar90._24_4_ = auVar84._24_4_ * fVar87 * (fVar94 * (fVar94 * fVar102 + fVar101) + fVar103)
        ;
        auVar90._28_4_ = auVar84._28_4_ * fVar88 * (fVar95 * (fVar95 * fVar102 + fVar101) + fVar103)
        ;
        auVar84 = vminps_avx(auVar79,auVar90);
        auVar84 = vmaxps_avx(auVar86,auVar84);
        *(undefined1 (*) [32])(param_1 + uVar67 * 4) = auVar84;
        uVar67 = uVar67 + 8;
      } while (uVar67 < uVar66 - 7);
    }
    uVar65 = DAT_18000f1f0;
    fVar80 = DAT_18000f1ec;
    fVar9 = DAT_18000f1e8;
    fVar8 = DAT_18000f1e4;
    fVar7 = DAT_18000f1e0;
    fVar6 = DAT_18000f1dc;
    fVar71 = DAT_18000f1d4;
    uVar64 = DAT_18000f098;
    if ((int)uVar67 < (int)uVar66) {
      uVar67 = uVar67 & 0xffffffff;
      do {
        fVar87 = *(float *)(param_2 + uVar67 * 4);
        fVar88 = fVar87 * fVar87;
        auVar107._0_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._4_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._8_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._12_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._16_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._20_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._24_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar107._28_4_ = fVar88 * (fVar88 * fVar80 + fVar9) + fVar71;
        auVar84 = vrcpps_avx(auVar107);
        auVar81 = vminss_avx(ZEXT416(uVar64),
                             ZEXT416((uint)(auVar84._0_4_ *
                                           fVar87 * (fVar88 * (fVar88 * fVar7 + fVar6) + fVar8))));
        auVar81 = vmaxss_avx(ZEXT416(uVar65),auVar81);
        *(int *)(param_1 + uVar67 * 4) = auVar81._0_4_;
        uVar67 = uVar67 + 1;
      } while ((param_3 & 0xffffffff) != uVar67);
    }
    break;
  case 3:
    if (0 < (int)uVar66) {
      if (uVar66 < 0x20 || (ulonglong)(param_1 - param_2) < 0x80) {
        uVar67 = 0;
      }
      else {
        uVar67 = (ulonglong)(uVar66 & 0x7fffffe0);
        lVar69 = 0;
        auVar72._0_12_ = ZEXT812(0);
        auVar72._12_4_ = 0;
        do {
          auVar84 = vmaxps_avx(ZEXT1632(auVar72),*(undefined1 (*) [32])(param_2 + lVar69));
          auVar113 = vmaxps_avx(ZEXT1632(auVar72),*(undefined1 (*) [32])(param_2 + 0x20 + lVar69));
          auVar114 = vmaxps_avx(ZEXT1632(auVar72),*(undefined1 (*) [32])(param_2 + 0x40 + lVar69));
          auVar115 = vmaxps_avx(ZEXT1632(auVar72),*(undefined1 (*) [32])(param_2 + 0x60 + lVar69));
          *(undefined1 (*) [32])(param_1 + lVar69) = auVar84;
          *(undefined1 (*) [32])(param_1 + 0x20 + lVar69) = auVar113;
          *(undefined1 (*) [32])(param_1 + 0x40 + lVar69) = auVar114;
          *(undefined1 (*) [32])(param_1 + 0x60 + lVar69) = auVar115;
          lVar69 = lVar69 + 0x80;
        } while ((ulonglong)((uint)(param_3 >> 5) & 0x3ffffff) << 7 != lVar69);
        if ((uVar66 & 0x7fffffe0) == uVar66) break;
      }
      uVar68 = param_3 & 3;
      uVar70 = uVar67;
      if (uVar68 != 0) {
        auVar73._0_12_ = ZEXT812(0);
        auVar73._12_4_ = 0;
        do {
          auVar81 = vmaxss_avx(auVar73,ZEXT416(*(uint *)(param_2 + uVar70 * 4)));
          *(int *)(param_1 + uVar70 * 4) = auVar81._0_4_;
          uVar70 = uVar70 + 1;
          uVar68 = uVar68 - 1;
        } while (uVar68 != 0);
      }
      if (uVar67 - (param_3 & 0xffffffff) < 0xfffffffffffffffd) {
        auVar74._0_12_ = ZEXT812(0);
        auVar74._12_4_ = 0;
        do {
          auVar81 = vmaxss_avx(auVar74,ZEXT416(*(uint *)(param_2 + uVar70 * 4)));
          *(int *)(param_1 + uVar70 * 4) = auVar81._0_4_;
          auVar81 = vmaxss_avx(auVar74,ZEXT416(*(uint *)(param_2 + 4 + uVar70 * 4)));
          *(int *)(param_1 + 4 + uVar70 * 4) = auVar81._0_4_;
          auVar81 = vmaxss_avx(auVar74,ZEXT416(*(uint *)(param_2 + 8 + uVar70 * 4)));
          *(int *)(param_1 + 8 + uVar70 * 4) = auVar81._0_4_;
          auVar81 = vmaxss_avx(auVar74,ZEXT416(*(uint *)(param_2 + 0xc + uVar70 * 4)));
          *(int *)(param_1 + 0xc + uVar70 * 4) = auVar81._0_4_;
          uVar70 = uVar70 + 4;
        } while ((param_3 & 0xffffffff) != uVar70);
      }
    }
    break;
  case 4:
    uVar67 = 0;
    if (7 < (int)uVar66) {
      uVar67 = 0;
      auVar75._4_4_ = DAT_18000f1a0;
      auVar75._0_4_ = DAT_18000f1a0;
      auVar75._8_4_ = DAT_18000f1a0;
      auVar75._12_4_ = DAT_18000f1a0;
      auVar75._16_4_ = DAT_18000f1a0;
      auVar75._20_4_ = DAT_18000f1a0;
      auVar75._24_4_ = DAT_18000f1a0;
      auVar75._28_4_ = DAT_18000f1a0;
      auVar77._4_4_ = DAT_18000f1a4;
      auVar77._0_4_ = DAT_18000f1a4;
      auVar77._8_4_ = DAT_18000f1a4;
      auVar77._12_4_ = DAT_18000f1a4;
      auVar77._16_4_ = DAT_18000f1a4;
      auVar77._20_4_ = DAT_18000f1a4;
      auVar77._24_4_ = DAT_18000f1a4;
      auVar77._28_4_ = DAT_18000f1a4;
      do {
        pfVar1 = (float *)(param_2 + uVar67 * 4);
        auVar83._0_4_ = fVar71 * *pfVar1;
        auVar83._4_4_ = fVar71 * pfVar1[1];
        auVar83._8_4_ = fVar71 * pfVar1[2];
        auVar83._12_4_ = fVar71 * pfVar1[3];
        auVar83._16_4_ = fVar71 * pfVar1[4];
        auVar83._20_4_ = fVar71 * pfVar1[5];
        auVar83._24_4_ = fVar71 * pfVar1[6];
        auVar83._28_4_ = fVar71 * pfVar1[7];
        auVar84 = vminps_avx(auVar75,auVar83);
        auVar113 = vmaxps_avx(auVar77,auVar84);
        auVar114 = vroundps_avx(auVar113,1);
        auVar84 = vcvtps2dq_avx(auVar114);
        auVar113 = vsubps_avx(auVar113,auVar114);
        fVar80 = auVar113._0_4_;
        fVar87 = auVar113._4_4_;
        fVar88 = auVar113._8_4_;
        fVar96 = auVar113._12_4_;
        fVar98 = auVar113._16_4_;
        fVar99 = auVar113._20_4_;
        fVar100 = auVar113._24_4_;
        fVar101 = auVar113._28_4_;
        auVar84 = vpslld_avx2(auVar84,0x17);
        piVar2 = (int *)(param_1 + uVar67 * 4);
        *piVar2 = (int)(fVar80 * (fVar80 * (fVar80 * fVar7 + fVar6) + fVar8) + fVar9) +
                  auVar84._0_4_;
        piVar2[1] = (int)(fVar87 * (fVar87 * (fVar87 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._4_4_;
        piVar2[2] = (int)(fVar88 * (fVar88 * (fVar88 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._8_4_;
        piVar2[3] = (int)(fVar96 * (fVar96 * (fVar96 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._12_4_;
        piVar2[4] = (int)(fVar98 * (fVar98 * (fVar98 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._16_4_;
        piVar2[5] = (int)(fVar99 * (fVar99 * (fVar99 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._20_4_;
        piVar2[6] = (int)(fVar100 * (fVar100 * (fVar100 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._24_4_;
        piVar2[7] = (int)(fVar101 * (fVar101 * (fVar101 * fVar7 + fVar6) + fVar8) + fVar9) +
                    auVar84._28_4_;
        uVar67 = uVar67 + 8;
      } while (uVar67 < uVar66 - 7);
    }
    fVar9 = DAT_18000f1b4;
    fVar8 = DAT_18000f1b0;
    fVar7 = DAT_18000f1ac;
    fVar6 = DAT_18000f1a8;
    fVar71 = DAT_18000f19c;
    if ((int)uVar67 < (int)uVar66) {
      uVar67 = uVar67 & 0xffffffff;
      auVar76._4_4_ = DAT_18000f1a0;
      auVar76._0_4_ = DAT_18000f1a0;
      auVar76._8_4_ = DAT_18000f1a0;
      auVar76._12_4_ = DAT_18000f1a0;
      auVar76._16_4_ = DAT_18000f1a0;
      auVar76._20_4_ = DAT_18000f1a0;
      auVar76._24_4_ = DAT_18000f1a0;
      auVar76._28_4_ = DAT_18000f1a0;
      auVar78._4_4_ = DAT_18000f1a4;
      auVar78._0_4_ = DAT_18000f1a4;
      auVar78._8_4_ = DAT_18000f1a4;
      auVar78._12_4_ = DAT_18000f1a4;
      auVar78._16_4_ = DAT_18000f1a4;
      auVar78._20_4_ = DAT_18000f1a4;
      auVar78._24_4_ = DAT_18000f1a4;
      auVar78._28_4_ = DAT_18000f1a4;
      do {
        fVar80 = fVar71 * *(float *)(param_2 + uVar67 * 4);
        auVar85._4_4_ = fVar80;
        auVar85._0_4_ = fVar80;
        auVar85._8_4_ = fVar80;
        auVar85._12_4_ = fVar80;
        auVar85._16_4_ = fVar80;
        auVar85._20_4_ = fVar80;
        auVar85._24_4_ = fVar80;
        auVar85._28_4_ = fVar80;
        auVar84 = vminps_avx(auVar76,auVar85);
        auVar113 = vmaxps_avx(auVar78,auVar84);
        auVar114 = vroundps_avx(auVar113,1);
        auVar84 = vcvtps2dq_avx(auVar114);
        auVar113 = vsubps_avx(auVar113,auVar114);
        fVar80 = auVar113._0_4_;
        auVar81 = vpslld_avx(auVar84._0_16_,0x17);
        *(int *)(param_1 + uVar67 * 4) =
             (int)(fVar80 * (fVar80 * (fVar80 * fVar7 + fVar6) + fVar8) + fVar9) + auVar81._0_4_;
        uVar67 = uVar67 + 1;
      } while ((param_3 & 0xffffffff) != uVar67);
    }
    if (0 < (int)uVar66) {
      if (uVar66 < 8) {
        fVar71 = 0.0;
        uVar67 = 0;
      }
      else {
        fVar71 = 0.0;
        uVar67 = 0;
        do {
          fVar71 = fVar71 + *(float *)(param_1 + uVar67 * 4) + *(float *)(param_1 + 4 + uVar67 * 4)
                   + *(float *)(param_1 + 8 + uVar67 * 4) + *(float *)(param_1 + 0xc + uVar67 * 4) +
                   *(float *)(param_1 + 0x10 + uVar67 * 4) + *(float *)(param_1 + 0x14 + uVar67 * 4)
                   + *(float *)(param_1 + 0x18 + uVar67 * 4) +
                   *(float *)(param_1 + 0x1c + uVar67 * 4);
          uVar67 = uVar67 + 8;
        } while ((uVar66 & 0x7ffffff8) != uVar67);
      }
      if ((ulonglong)(uVar66 & 7) != 0) {
        uVar70 = 0;
        do {
          fVar71 = fVar71 + *(float *)(param_1 + uVar67 * 4 + uVar70 * 4);
          uVar70 = uVar70 + 1;
        } while ((uVar66 & 7) != uVar70);
      }
      if (0 < (int)uVar66) {
        fVar71 = (float)(DAT_18000f128 / ((double)fVar71 + DAT_18000f1b8));
        if (uVar66 < 0x20) {
          uVar67 = 0;
        }
        else {
          uVar67 = (ulonglong)(uVar66 & 0x7fffffe0);
          lVar69 = 0;
          do {
            pfVar1 = (float *)(param_1 + lVar69);
            fVar6 = pfVar1[1];
            fVar7 = pfVar1[2];
            fVar8 = pfVar1[3];
            fVar9 = pfVar1[4];
            fVar80 = pfVar1[5];
            fVar87 = pfVar1[6];
            fVar88 = pfVar1[7];
            pfVar4 = (float *)(param_1 + 0x20 + lVar69);
            fVar96 = *pfVar4;
            fVar98 = pfVar4[1];
            fVar99 = pfVar4[2];
            fVar100 = pfVar4[3];
            fVar101 = pfVar4[4];
            fVar102 = pfVar4[5];
            fVar103 = pfVar4[6];
            fVar105 = pfVar4[7];
            pfVar4 = (float *)(param_1 + 0x40 + lVar69);
            fVar97 = *pfVar4;
            fVar91 = pfVar4[1];
            fVar92 = pfVar4[2];
            fVar93 = pfVar4[3];
            fVar94 = pfVar4[4];
            fVar95 = pfVar4[5];
            fVar104 = pfVar4[6];
            fVar10 = pfVar4[7];
            pfVar4 = (float *)(param_1 + 0x60 + lVar69);
            fVar11 = *pfVar4;
            fVar12 = pfVar4[1];
            fVar13 = pfVar4[2];
            fVar14 = pfVar4[3];
            fVar15 = pfVar4[4];
            fVar16 = pfVar4[5];
            fVar17 = pfVar4[6];
            fVar18 = pfVar4[7];
            pfVar4 = (float *)(param_1 + lVar69);
            *pfVar4 = fVar71 * *pfVar1;
            pfVar4[1] = fVar71 * fVar6;
            pfVar4[2] = fVar71 * fVar7;
            pfVar4[3] = fVar71 * fVar8;
            pfVar4[4] = fVar71 * fVar9;
            pfVar4[5] = fVar71 * fVar80;
            pfVar4[6] = fVar71 * fVar87;
            pfVar4[7] = fVar71 * fVar88;
            pfVar1 = (float *)(param_1 + 0x20 + lVar69);
            *pfVar1 = fVar71 * fVar96;
            pfVar1[1] = fVar71 * fVar98;
            pfVar1[2] = fVar71 * fVar99;
            pfVar1[3] = fVar71 * fVar100;
            pfVar1[4] = fVar71 * fVar101;
            pfVar1[5] = fVar71 * fVar102;
            pfVar1[6] = fVar71 * fVar103;
            pfVar1[7] = fVar71 * fVar105;
            pfVar1 = (float *)(param_1 + 0x40 + lVar69);
            *pfVar1 = fVar71 * fVar97;
            pfVar1[1] = fVar71 * fVar91;
            pfVar1[2] = fVar71 * fVar92;
            pfVar1[3] = fVar71 * fVar93;
            pfVar1[4] = fVar71 * fVar94;
            pfVar1[5] = fVar71 * fVar95;
            pfVar1[6] = fVar71 * fVar104;
            pfVar1[7] = fVar71 * fVar10;
            pfVar1 = (float *)(param_1 + 0x60 + lVar69);
            *pfVar1 = fVar71 * fVar11;
            pfVar1[1] = fVar71 * fVar12;
            pfVar1[2] = fVar71 * fVar13;
            pfVar1[3] = fVar71 * fVar14;
            pfVar1[4] = fVar71 * fVar15;
            pfVar1[5] = fVar71 * fVar16;
            pfVar1[6] = fVar71 * fVar17;
            pfVar1[7] = fVar71 * fVar18;
            lVar69 = lVar69 + 0x80;
          } while ((ulonglong)((uint)(param_3 >> 5) & 0x3ffffff) << 7 != lVar69);
          if ((uVar66 & 0x7fffffe0) == uVar66) break;
        }
        do {
          *(float *)(param_1 + uVar67 * 4) = fVar71 * *(float *)(param_1 + uVar67 * 4);
          uVar67 = uVar67 + 1;
        } while ((param_3 & 0xffffffff) != uVar67);
      }
    }
    break;
  case 5:
    uVar67 = 0;
    if (7 < (int)uVar66) {
      uVar67 = 0;
      auVar84._4_4_ = DAT_18000f098;
      auVar84._0_4_ = DAT_18000f098;
      auVar84._8_4_ = DAT_18000f098;
      auVar84._12_4_ = DAT_18000f098;
      auVar84._16_4_ = DAT_18000f098;
      auVar84._20_4_ = DAT_18000f098;
      auVar84._24_4_ = DAT_18000f098;
      auVar84._28_4_ = DAT_18000f098;
      do {
        pfVar1 = (float *)(param_2 + uVar67 * 4);
        fVar71 = *pfVar1;
        fVar6 = pfVar1[1];
        fVar7 = pfVar1[2];
        fVar8 = pfVar1[3];
        fVar9 = pfVar1[4];
        fVar80 = pfVar1[5];
        fVar87 = pfVar1[6];
        fVar88 = pfVar1[7];
        fVar96 = fVar71 * fVar71;
        fVar98 = fVar6 * fVar6;
        fVar99 = fVar7 * fVar7;
        fVar100 = fVar8 * fVar8;
        fVar101 = fVar9 * fVar9;
        fVar102 = fVar80 * fVar80;
        fVar103 = fVar87 * fVar87;
        fVar105 = fVar88 * fVar88;
        auVar113._0_4_ = fVar96 * (fVar96 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._4_4_ = fVar98 * (fVar98 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._8_4_ = fVar99 * (fVar99 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._12_4_ = fVar100 * (fVar100 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._16_4_ = fVar101 * (fVar101 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._20_4_ = fVar102 * (fVar102 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._24_4_ = fVar103 * (fVar103 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113._28_4_ = fVar105 * (fVar105 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar113 = vrcpps_avx(auVar113);
        auVar114._0_4_ =
             fVar71 * (fVar96 * (fVar96 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._0_4_ + DAT_18000f1d8;
        auVar114._4_4_ =
             fVar6 * (fVar98 * (fVar98 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._4_4_ + DAT_18000f1d8;
        auVar114._8_4_ =
             fVar7 * (fVar99 * (fVar99 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._8_4_ + DAT_18000f1d8;
        auVar114._12_4_ =
             fVar8 * (fVar100 * (fVar100 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._12_4_ + DAT_18000f1d8;
        auVar114._16_4_ =
             fVar9 * (fVar101 * (fVar101 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._16_4_ + DAT_18000f1d8;
        auVar114._20_4_ =
             fVar80 * (fVar102 * (fVar102 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._20_4_ + DAT_18000f1d8;
        auVar114._24_4_ =
             fVar87 * (fVar103 * (fVar103 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._24_4_ + DAT_18000f1d8;
        auVar114._28_4_ =
             fVar88 * (fVar105 * (fVar105 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar113._28_4_ + DAT_18000f1d8;
        auVar113 = vminps_avx(auVar84,auVar114);
        auVar113 = vmaxps_avx(ZEXT1632(ZEXT816(0)),auVar113);
        *(undefined1 (*) [32])(afStack_4088 + uVar67) = auVar113;
        uVar67 = uVar67 + 8;
      } while (uVar67 < uVar66 - 7);
    }
    uVar70 = param_3 & 0xffffffff;
    if ((int)uVar67 < (int)uVar66) {
      uVar67 = uVar67 & 0xffffffff;
      auVar81._0_12_ = ZEXT812(0);
      auVar81._12_4_ = 0;
      do {
        fVar71 = *(float *)(param_2 + uVar67 * 4);
        fVar6 = fVar71 * fVar71;
        auVar115._0_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._4_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._8_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._12_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._16_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._20_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._24_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar115._28_4_ = fVar6 * (fVar6 * DAT_18000f1d0 + DAT_18000f1cc) + DAT_18000f1d4;
        auVar84 = vrcpps_avx(auVar115);
        auVar109._0_4_ =
             fVar71 * (fVar6 * (fVar6 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar84._0_4_ + DAT_18000f1d8;
        auVar109._4_4_ =
             fVar71 * (fVar6 * (fVar6 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar84._4_4_ + DAT_18000f1d8;
        auVar109._8_4_ =
             fVar71 * (fVar6 * (fVar6 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar84._8_4_ + DAT_18000f1d8;
        auVar109._12_4_ =
             fVar71 * (fVar6 * (fVar6 * DAT_18000f1c4 + DAT_18000f1c0) + DAT_18000f1c8) *
             auVar84._12_4_ + DAT_18000f1d8;
        auVar109 = vminss_avx(ZEXT416(DAT_18000f098),auVar109);
        auVar109 = vmaxss_avx(auVar81,auVar109);
        afStack_4088[uVar67] = auVar109._0_4_;
        uVar67 = uVar67 + 1;
      } while (uVar70 != uVar67);
    }
    if (0 < (int)uVar66) {
      if ((ulonglong)(param_1 - param_2) < 0x80 || uVar66 < 0x20) {
        uVar67 = 0;
      }
      else {
        uVar67 = (ulonglong)(uVar66 & 0x7fffffe0);
        lVar69 = 0;
        do {
          pfVar1 = (float *)(param_2 + lVar69);
          fVar71 = pfVar1[1];
          fVar6 = pfVar1[2];
          fVar7 = pfVar1[3];
          fVar8 = pfVar1[4];
          fVar9 = pfVar1[5];
          fVar80 = pfVar1[6];
          fVar87 = pfVar1[7];
          pfVar4 = (float *)(param_2 + 0x20 + lVar69);
          fVar88 = *pfVar4;
          fVar96 = pfVar4[1];
          fVar98 = pfVar4[2];
          fVar99 = pfVar4[3];
          fVar100 = pfVar4[4];
          fVar101 = pfVar4[5];
          fVar102 = pfVar4[6];
          fVar103 = pfVar4[7];
          pfVar4 = (float *)(param_2 + 0x40 + lVar69);
          fVar105 = *pfVar4;
          fVar97 = pfVar4[1];
          fVar91 = pfVar4[2];
          fVar92 = pfVar4[3];
          fVar93 = pfVar4[4];
          fVar94 = pfVar4[5];
          fVar95 = pfVar4[6];
          fVar104 = pfVar4[7];
          pfVar4 = (float *)(param_2 + 0x60 + lVar69);
          fVar10 = *pfVar4;
          fVar11 = pfVar4[1];
          fVar12 = pfVar4[2];
          fVar13 = pfVar4[3];
          fVar14 = pfVar4[4];
          fVar15 = pfVar4[5];
          fVar16 = pfVar4[6];
          fVar17 = pfVar4[7];
          fVar18 = *(float *)((longlong)afStack_4088 + lVar69 + 4);
          fVar19 = *(float *)((longlong)afStack_4088 + lVar69 + 8);
          fVar20 = *(float *)((longlong)afStack_4088 + lVar69 + 0xc);
          fVar21 = *(float *)((longlong)afStack_4088 + lVar69 + 0x10);
          fVar22 = *(float *)((longlong)afStack_4088 + lVar69 + 0x14);
          fVar23 = *(float *)((longlong)afStack_4088 + lVar69 + 0x18);
          fVar24 = *(float *)((longlong)afStack_4088 + lVar69 + 0x1c);
          fVar25 = *(float *)((longlong)afStack_4088 + lVar69 + 0x20);
          fVar26 = *(float *)((longlong)afStack_4088 + lVar69 + 0x24);
          fVar27 = *(float *)((longlong)afStack_4088 + lVar69 + 0x28);
          fVar28 = *(float *)((longlong)afStack_4088 + lVar69 + 0x2c);
          fVar29 = *(float *)((longlong)afStack_4088 + lVar69 + 0x30);
          fVar30 = *(float *)((longlong)afStack_4088 + lVar69 + 0x34);
          fVar31 = *(float *)((longlong)afStack_4088 + lVar69 + 0x38);
          fVar32 = *(float *)((longlong)afStack_4088 + lVar69 + 0x3c);
          fVar33 = *(float *)((longlong)afStack_4088 + lVar69 + 0x40);
          fVar34 = *(float *)((longlong)afStack_4088 + lVar69 + 0x44);
          fVar35 = *(float *)((longlong)afStack_4088 + lVar69 + 0x48);
          fVar36 = *(float *)((longlong)afStack_4088 + lVar69 + 0x4c);
          fVar37 = *(float *)((longlong)afStack_4088 + lVar69 + 0x50);
          fVar38 = *(float *)((longlong)afStack_4088 + lVar69 + 0x54);
          fVar39 = *(float *)((longlong)afStack_4088 + lVar69 + 0x58);
          fVar40 = *(float *)((longlong)afStack_4088 + lVar69 + 0x5c);
          fVar41 = *(float *)((longlong)afStack_4088 + lVar69 + 0x60);
          fVar42 = *(float *)((longlong)afStack_4088 + lVar69 + 100);
          fVar43 = *(float *)((longlong)afStack_4088 + lVar69 + 0x68);
          fVar44 = *(float *)((longlong)afStack_4088 + lVar69 + 0x6c);
          fVar45 = *(float *)((longlong)afStack_4088 + lVar69 + 0x70);
          fVar46 = *(float *)((longlong)afStack_4088 + lVar69 + 0x74);
          fVar47 = *(float *)((longlong)afStack_4088 + lVar69 + 0x78);
          fVar48 = *(float *)((longlong)afStack_4088 + lVar69 + 0x7c);
          pfVar4 = (float *)(param_1 + lVar69);
          *pfVar4 = *pfVar1 * *(float *)((longlong)afStack_4088 + lVar69);
          pfVar4[1] = fVar71 * fVar18;
          pfVar4[2] = fVar6 * fVar19;
          pfVar4[3] = fVar7 * fVar20;
          pfVar4[4] = fVar8 * fVar21;
          pfVar4[5] = fVar9 * fVar22;
          pfVar4[6] = fVar80 * fVar23;
          pfVar4[7] = fVar87 * fVar24;
          pfVar1 = (float *)(param_1 + 0x20 + lVar69);
          *pfVar1 = fVar88 * fVar25;
          pfVar1[1] = fVar96 * fVar26;
          pfVar1[2] = fVar98 * fVar27;
          pfVar1[3] = fVar99 * fVar28;
          pfVar1[4] = fVar100 * fVar29;
          pfVar1[5] = fVar101 * fVar30;
          pfVar1[6] = fVar102 * fVar31;
          pfVar1[7] = fVar103 * fVar32;
          pfVar1 = (float *)(param_1 + 0x40 + lVar69);
          *pfVar1 = fVar105 * fVar33;
          pfVar1[1] = fVar97 * fVar34;
          pfVar1[2] = fVar91 * fVar35;
          pfVar1[3] = fVar92 * fVar36;
          pfVar1[4] = fVar93 * fVar37;
          pfVar1[5] = fVar94 * fVar38;
          pfVar1[6] = fVar95 * fVar39;
          pfVar1[7] = fVar104 * fVar40;
          pfVar1 = (float *)(param_1 + 0x60 + lVar69);
          *pfVar1 = fVar10 * fVar41;
          pfVar1[1] = fVar11 * fVar42;
          pfVar1[2] = fVar12 * fVar43;
          pfVar1[3] = fVar13 * fVar44;
          pfVar1[4] = fVar14 * fVar45;
          pfVar1[5] = fVar15 * fVar46;
          pfVar1[6] = fVar16 * fVar47;
          pfVar1[7] = fVar17 * fVar48;
          lVar69 = lVar69 + 0x80;
        } while ((ulonglong)((uint)(param_3 >> 5) & 0x3ffffff) << 7 != lVar69);
        if ((uVar66 & 0x7fffffe0) == uVar66) break;
      }
      uVar68 = uVar67;
      for (param_3 = param_3 & 3; param_3 != 0; param_3 = param_3 - 1) {
        *(float *)(param_1 + uVar68 * 4) = *(float *)(param_2 + uVar68 * 4) * afStack_4088[uVar68];
        uVar68 = uVar68 + 1;
      }
      if (uVar67 - uVar70 < 0xfffffffffffffffd) {
        do {
          *(float *)(param_1 + uVar68 * 4) = *(float *)(param_2 + uVar68 * 4) * afStack_4088[uVar68]
          ;
          *(float *)(param_1 + 4 + uVar68 * 4) =
               *(float *)(param_2 + 4 + uVar68 * 4) * afStack_4088[uVar68 + 1];
          *(float *)(param_1 + 8 + uVar68 * 4) =
               *(float *)(param_2 + 8 + uVar68 * 4) * afStack_4088[uVar68 + 2];
          *(float *)(param_1 + 0xc + uVar68 * 4) =
               *(float *)(param_2 + 0xc + uVar68 * 4) * afStack_4088[uVar68 + 3];
          uVar68 = uVar68 + 4;
        } while (uVar70 != uVar68);
      }
    }
    break;
  default:
    if (0 < (int)uVar66 && param_2 != param_1) {
      if ((ulonglong)(param_1 - param_2) < 0x80 || uVar66 < 0x20) {
        uVar67 = 0;
      }
      else {
        uVar67 = (ulonglong)(uVar66 & 0x7fffffe0);
        lVar69 = 0;
        do {
          puVar3 = (undefined8 *)(param_2 + lVar69);
          uVar49 = puVar3[1];
          uVar50 = puVar3[2];
          uVar51 = puVar3[3];
          puVar5 = (undefined8 *)(param_2 + 0x20 + lVar69);
          uVar52 = *puVar5;
          uVar53 = puVar5[1];
          uVar54 = puVar5[2];
          uVar55 = puVar5[3];
          puVar5 = (undefined8 *)(param_2 + 0x40 + lVar69);
          uVar56 = *puVar5;
          uVar57 = puVar5[1];
          uVar58 = puVar5[2];
          uVar59 = puVar5[3];
          puVar5 = (undefined8 *)(param_2 + 0x60 + lVar69);
          uVar60 = *puVar5;
          uVar61 = puVar5[1];
          uVar62 = puVar5[2];
          uVar63 = puVar5[3];
          puVar5 = (undefined8 *)(param_1 + lVar69);
          *puVar5 = *puVar3;
          puVar5[1] = uVar49;
          puVar5[2] = uVar50;
          puVar5[3] = uVar51;
          puVar3 = (undefined8 *)(param_1 + 0x20 + lVar69);
          *puVar3 = uVar52;
          puVar3[1] = uVar53;
          puVar3[2] = uVar54;
          puVar3[3] = uVar55;
          puVar3 = (undefined8 *)(param_1 + 0x40 + lVar69);
          *puVar3 = uVar56;
          puVar3[1] = uVar57;
          puVar3[2] = uVar58;
          puVar3[3] = uVar59;
          puVar3 = (undefined8 *)(param_1 + 0x60 + lVar69);
          *puVar3 = uVar60;
          puVar3[1] = uVar61;
          puVar3[2] = uVar62;
          puVar3[3] = uVar63;
          lVar69 = lVar69 + 0x80;
        } while ((ulonglong)((uint)(param_3 >> 5) & 0x3ffffff) << 7 != lVar69);
        if ((uVar66 & 0x7fffffe0) == uVar66) break;
      }
      uVar68 = uVar67;
      for (uVar70 = param_3 & 3; uVar70 != 0; uVar70 = uVar70 - 1) {
        *(undefined4 *)(param_1 + uVar68 * 4) = *(undefined4 *)(param_2 + uVar68 * 4);
        uVar68 = uVar68 + 1;
      }
      if (uVar67 - (param_3 & 0xffffffff) < 0xfffffffffffffffd) {
        do {
          *(undefined4 *)(param_1 + uVar68 * 4) = *(undefined4 *)(param_2 + uVar68 * 4);
          *(undefined4 *)(param_1 + 4 + uVar68 * 4) = *(undefined4 *)(param_2 + 4 + uVar68 * 4);
          *(undefined4 *)(param_1 + 8 + uVar68 * 4) = *(undefined4 *)(param_2 + 8 + uVar68 * 4);
          *(undefined4 *)(param_1 + 0xc + uVar68 * 4) = *(undefined4 *)(param_2 + 0xc + uVar68 * 4);
          uVar68 = uVar68 + 4;
        } while ((param_3 & 0xffffffff) != uVar68);
      }
    }
  }
  if ((local_80 ^ (ulonglong)auStack_40a8) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 180009230
   NAME : rnn_compute_linear_c
   SIG  : undefined rnn_compute_linear_c(void)
   ======================================================================== */

void rnn_compute_linear_c(undefined8 *param_1,void *param_2,longlong param_3)

{
  uint *puVar1;
  float *pfVar2;
  float *pfVar3;
  float *pfVar4;
  float fVar5;
  float fVar6;
  undefined4 uVar7;
  uint uVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  float fVar20;
  float fVar21;
  float fVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  float fVar33;
  float fVar34;
  float fVar35;
  float fVar36;
  float fVar37;
  float fVar38;
  float fVar39;
  float fVar40;
  float fVar41;
  float fVar42;
  float fVar43;
  float fVar44;
  float fVar45;
  float fVar46;
  float fVar47;
  float fVar48;
  float fVar49;
  float fVar50;
  float fVar51;
  float fVar52;
  float fVar53;
  float fVar54;
  float fVar55;
  float fVar56;
  float fVar57;
  float fVar58;
  float fVar59;
  float fVar60;
  int *piVar61;
  size_t _Size;
  ulonglong uVar62;
  void *_Dst;
  float *pfVar63;
  float *pfVar64;
  void *pvVar65;
  ulonglong uVar66;
  uint uVar67;
  ulonglong uVar68;
  undefined1 (*pauVar69) [32];
  ulonglong uVar70;
  int iVar71;
  uint *puVar72;
  ulonglong uVar73;
  longlong lVar74;
  uint uVar75;
  uint uVar76;
  uint uVar77;
  longlong lVar78;
  float *pfVar79;
  float fVar80;
  undefined1 auVar81 [16];
  undefined1 auVar82 [32];
  float fVar92;
  float fVar96;
  undefined1 auVar83 [32];
  undefined1 auVar84 [32];
  undefined1 auVar85 [32];
  undefined1 auVar86 [32];
  undefined1 auVar87 [32];
  undefined1 auVar88 [32];
  float fVar90;
  float fVar91;
  float fVar93;
  float fVar94;
  float fVar95;
  undefined1 auVar89 [64];
  undefined1 auVar97 [32];
  undefined1 auVar98 [32];
  undefined1 auVar99 [32];
  undefined1 auVar100 [32];
  undefined1 auVar101 [32];
  undefined1 auVar102 [32];
  undefined1 auVar103 [32];
  undefined1 auVar104 [32];
  undefined1 auVar105 [32];
  undefined1 auVar106 [32];
  undefined1 auVar107 [32];
  undefined1 auVar108 [32];
  undefined1 auVar109 [32];
  undefined1 auVar110 [32];
  undefined1 auVar111 [32];
  undefined1 auVar112 [32];
  undefined1 auVar113 [32];
  undefined1 auVar114 [32];
  undefined1 auVar115 [32];
  undefined1 auVar116 [32];
  undefined1 auVar117 [32];
  undefined1 auVar118 [32];
  undefined1 auVar119 [32];
  undefined1 auVar120 [32];
  undefined1 auVar121 [32];
  undefined1 auStack_888 [32];
  ulonglong local_868;
  ulonglong local_860;
  void *local_858;
  longlong local_850;
  undefined4 auStack_848 [2];
  undefined4 auStack_840 [510];
  ulonglong local_48;
  
                    /* 0x9230  12  rnn_compute_linear_c */
  local_48 = DAT_180580000 ^ (ulonglong)auStack_888;
  pvVar65 = (void *)*param_1;
  pfVar79 = (float *)param_1[3];
  uVar76 = *(uint *)(param_1 + 7);
  uVar62 = (ulonglong)uVar76;
  uVar8 = *(uint *)((longlong)param_1 + 0x3c);
  local_868 = (ulonglong)uVar8;
  uVar66 = (ulonglong)(int)uVar8;
  uVar77 = uVar8;
  if (pfVar79 == (float *)0x0) {
    pauVar69 = (undefined1 (*) [32])param_1[2];
    if (pauVar69 != (undefined1 (*) [32])0x0) {
      puVar72 = (uint *)param_1[4];
      lVar74 = param_1[6];
      if (puVar72 == (uint *)0x0) {
        if (0 < (int)uVar76) {
          if (uVar76 < 9) {
            lVar78 = 0;
          }
          else {
            uVar68 = (uVar62 - 1 >> 3) + 1 & 0xfffffffffffffffe;
            lVar78 = 0;
            do {
              pfVar79 = (float *)(param_3 + lVar78 * 4);
              auVar102._0_4_ = *pfVar79 * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._4_4_ = pfVar79[1] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._8_4_ = pfVar79[2] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._12_4_ = pfVar79[3] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._16_4_ = pfVar79[4] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._20_4_ = pfVar79[5] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._24_4_ = pfVar79[6] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102._28_4_ = pfVar79[7] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102 = vcvtps2dq_avx(auVar102);
              auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
              auVar81 = vpackuswb_avx(auVar81,auVar81);
              auVar81 = vpshufd_avx(auVar81,4);
              auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
              *(undefined1 (*) [32])((longlong)auStack_848 + lVar78) = auVar102;
              pfVar79 = (float *)(param_3 + 0x20 + lVar78 * 4);
              auVar103._0_4_ = *pfVar79 * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._4_4_ = pfVar79[1] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._8_4_ = pfVar79[2] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._12_4_ = pfVar79[3] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._16_4_ = pfVar79[4] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._20_4_ = pfVar79[5] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._24_4_ = pfVar79[6] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar103._28_4_ = pfVar79[7] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102 = vcvtps2dq_avx(auVar103);
              auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
              auVar81 = vpackuswb_avx(auVar81,auVar81);
              auVar81 = vpshufd_avx(auVar81,4);
              auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
              *(undefined1 (*) [32])((longlong)auStack_840 + lVar78) = auVar102;
              lVar78 = lVar78 + 0x10;
              uVar68 = uVar68 - 2;
            } while (uVar68 != 0);
          }
          if ((uVar62 - 1 & 8) == 0) {
            pfVar79 = (float *)(param_3 + lVar78 * 4);
            auVar86._0_4_ = DAT_18000f1f4 * *pfVar79 + DAT_18000f1f4;
            auVar86._4_4_ = DAT_18000f1f4 * pfVar79[1] + DAT_18000f1f4;
            auVar86._8_4_ = DAT_18000f1f4 * pfVar79[2] + DAT_18000f1f4;
            auVar86._12_4_ = DAT_18000f1f4 * pfVar79[3] + DAT_18000f1f4;
            auVar86._16_4_ = DAT_18000f1f4 * pfVar79[4] + DAT_18000f1f4;
            auVar86._20_4_ = DAT_18000f1f4 * pfVar79[5] + DAT_18000f1f4;
            auVar86._24_4_ = DAT_18000f1f4 * pfVar79[6] + DAT_18000f1f4;
            auVar86._28_4_ = DAT_18000f1f4 * pfVar79[7] + DAT_18000f1f4;
            auVar102 = vcvtps2dq_avx(auVar86);
            auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
            auVar81 = vpackuswb_avx(auVar81,auVar81);
            auVar81 = vpshufd_avx(auVar81,4);
            auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
            *(undefined1 (*) [32])((longlong)auStack_848 + lVar78) = auVar102;
          }
        }
        if (0 < (int)uVar8) {
          if ((int)uVar76 < 0xd) {
            if ((int)uVar76 < 1) {
              uVar68 = ((ulonglong)uVar8 - 1 >> 3) + 1;
              if (uVar8 < 0x19) {
                lVar78 = 0;
              }
              else {
                uVar73 = uVar68 & 0x3ffffffffffffffc;
                lVar78 = 0;
                do {
                  pfVar79 = (float *)(lVar74 + lVar78 * 4);
                  fVar80 = pfVar79[1];
                  fVar90 = pfVar79[2];
                  fVar91 = pfVar79[3];
                  fVar92 = pfVar79[4];
                  fVar93 = pfVar79[5];
                  fVar94 = pfVar79[6];
                  fVar95 = pfVar79[7];
                  pfVar64 = (float *)((longlong)param_2 + lVar78 * 4);
                  *pfVar64 = *pfVar79 * 0.0;
                  pfVar64[1] = fVar80 * 0.0;
                  pfVar64[2] = fVar90 * 0.0;
                  pfVar64[3] = fVar91 * 0.0;
                  pfVar64[4] = fVar92 * 0.0;
                  pfVar64[5] = fVar93 * 0.0;
                  pfVar64[6] = fVar94 * 0.0;
                  pfVar64[7] = fVar95 * 0.0;
                  pfVar79 = (float *)(lVar74 + 0x20 + lVar78 * 4);
                  fVar80 = pfVar79[1];
                  fVar90 = pfVar79[2];
                  fVar91 = pfVar79[3];
                  fVar92 = pfVar79[4];
                  fVar93 = pfVar79[5];
                  fVar94 = pfVar79[6];
                  fVar95 = pfVar79[7];
                  pfVar64 = (float *)((longlong)param_2 + lVar78 * 4 + 0x20);
                  *pfVar64 = *pfVar79 * 0.0;
                  pfVar64[1] = fVar80 * 0.0;
                  pfVar64[2] = fVar90 * 0.0;
                  pfVar64[3] = fVar91 * 0.0;
                  pfVar64[4] = fVar92 * 0.0;
                  pfVar64[5] = fVar93 * 0.0;
                  pfVar64[6] = fVar94 * 0.0;
                  pfVar64[7] = fVar95 * 0.0;
                  pfVar79 = (float *)(lVar74 + 0x40 + lVar78 * 4);
                  fVar80 = pfVar79[1];
                  fVar90 = pfVar79[2];
                  fVar91 = pfVar79[3];
                  fVar92 = pfVar79[4];
                  fVar93 = pfVar79[5];
                  fVar94 = pfVar79[6];
                  fVar95 = pfVar79[7];
                  pfVar64 = (float *)((longlong)param_2 + lVar78 * 4 + 0x40);
                  *pfVar64 = *pfVar79 * 0.0;
                  pfVar64[1] = fVar80 * 0.0;
                  pfVar64[2] = fVar90 * 0.0;
                  pfVar64[3] = fVar91 * 0.0;
                  pfVar64[4] = fVar92 * 0.0;
                  pfVar64[5] = fVar93 * 0.0;
                  pfVar64[6] = fVar94 * 0.0;
                  pfVar64[7] = fVar95 * 0.0;
                  pfVar79 = (float *)(lVar74 + 0x60 + lVar78 * 4);
                  fVar80 = pfVar79[1];
                  fVar90 = pfVar79[2];
                  fVar91 = pfVar79[3];
                  fVar92 = pfVar79[4];
                  fVar93 = pfVar79[5];
                  fVar94 = pfVar79[6];
                  fVar95 = pfVar79[7];
                  pfVar64 = (float *)((longlong)param_2 + lVar78 * 4 + 0x60);
                  *pfVar64 = *pfVar79 * 0.0;
                  pfVar64[1] = fVar80 * 0.0;
                  pfVar64[2] = fVar90 * 0.0;
                  pfVar64[3] = fVar91 * 0.0;
                  pfVar64[4] = fVar92 * 0.0;
                  pfVar64[5] = fVar93 * 0.0;
                  pfVar64[6] = fVar94 * 0.0;
                  pfVar64[7] = fVar95 * 0.0;
                  lVar78 = lVar78 + 0x20;
                  uVar73 = uVar73 - 4;
                } while (uVar73 != 0);
              }
              if ((uVar68 & 3) != 0) {
                uVar73 = 0;
                do {
                  pfVar79 = (float *)(lVar74 + lVar78 * 4 + uVar73);
                  fVar80 = pfVar79[1];
                  fVar90 = pfVar79[2];
                  fVar91 = pfVar79[3];
                  fVar92 = pfVar79[4];
                  fVar93 = pfVar79[5];
                  fVar94 = pfVar79[6];
                  fVar95 = pfVar79[7];
                  pfVar64 = (float *)((longlong)param_2 + uVar73 + lVar78 * 4);
                  *pfVar64 = *pfVar79 * 0.0;
                  pfVar64[1] = fVar80 * 0.0;
                  pfVar64[2] = fVar90 * 0.0;
                  pfVar64[3] = fVar91 * 0.0;
                  pfVar64[4] = fVar92 * 0.0;
                  pfVar64[5] = fVar93 * 0.0;
                  pfVar64[6] = fVar94 * 0.0;
                  pfVar64[7] = fVar95 * 0.0;
                  uVar73 = uVar73 + 0x20;
                } while (((uint)uVar68 & 3) << 5 != uVar73);
              }
            }
            else {
              uVar68 = 0;
              auVar88._2_2_ = DAT_18000f198;
              auVar88._0_2_ = DAT_18000f198;
              auVar88._4_2_ = DAT_18000f198;
              auVar88._6_2_ = DAT_18000f198;
              auVar88._8_2_ = DAT_18000f198;
              auVar88._10_2_ = DAT_18000f198;
              auVar88._12_2_ = DAT_18000f198;
              auVar88._14_2_ = DAT_18000f198;
              auVar88._16_2_ = DAT_18000f198;
              auVar88._18_2_ = DAT_18000f198;
              auVar88._20_2_ = DAT_18000f198;
              auVar88._22_2_ = DAT_18000f198;
              auVar88._24_2_ = DAT_18000f198;
              auVar88._26_2_ = DAT_18000f198;
              auVar88._28_2_ = DAT_18000f198;
              auVar88._30_2_ = DAT_18000f198;
              do {
                auVar89 = ZEXT1664((undefined1  [16])0x0);
                auVar106 = auVar89._0_32_;
                if (uVar76 < 5) {
                  lVar78 = 0;
                }
                else {
                  lVar78 = 0;
                  uVar73 = (uVar62 - 1 >> 2) + 1 & 0xfffffffffffffffe;
                  do {
                    uVar7 = *(undefined4 *)((longlong)auStack_848 + lVar78);
                    auVar115._4_4_ = uVar7;
                    auVar115._0_4_ = uVar7;
                    auVar115._8_4_ = uVar7;
                    auVar115._12_4_ = uVar7;
                    auVar115._16_4_ = uVar7;
                    auVar115._20_4_ = uVar7;
                    auVar115._24_4_ = uVar7;
                    auVar115._28_4_ = uVar7;
                    auVar102 = vpmaddubsw_avx2(auVar115,*pauVar69);
                    auVar103 = vpmaddwd_avx2(auVar102,auVar88);
                    uVar7 = *(undefined4 *)((longlong)auStack_848 + lVar78 + 4);
                    auVar121._4_4_ = uVar7;
                    auVar121._0_4_ = uVar7;
                    auVar121._8_4_ = uVar7;
                    auVar121._12_4_ = uVar7;
                    auVar121._16_4_ = uVar7;
                    auVar121._20_4_ = uVar7;
                    auVar121._24_4_ = uVar7;
                    auVar121._28_4_ = uVar7;
                    auVar102 = vpmaddubsw_avx2(auVar121,pauVar69[1]);
                    auVar102 = vpmaddwd_avx2(auVar102,auVar88);
                    auVar106._0_4_ = auVar102._0_4_ + auVar103._0_4_ + auVar89._0_4_;
                    auVar106._4_4_ = auVar102._4_4_ + auVar103._4_4_ + auVar89._4_4_;
                    auVar106._8_4_ = auVar102._8_4_ + auVar103._8_4_ + auVar89._8_4_;
                    auVar106._12_4_ = auVar102._12_4_ + auVar103._12_4_ + auVar89._12_4_;
                    auVar106._16_4_ = auVar102._16_4_ + auVar103._16_4_ + auVar89._16_4_;
                    auVar106._20_4_ = auVar102._20_4_ + auVar103._20_4_ + auVar89._20_4_;
                    auVar106._24_4_ = auVar102._24_4_ + auVar103._24_4_ + auVar89._24_4_;
                    auVar106._28_4_ = auVar102._28_4_ + auVar103._28_4_ + auVar89._28_4_;
                    auVar89 = ZEXT3264(auVar106);
                    pauVar69 = pauVar69 + 2;
                    lVar78 = lVar78 + 8;
                    uVar73 = uVar73 - 2;
                  } while (uVar73 != 0);
                }
                auVar105 = auVar106;
                if ((uVar62 - 1 & 4) == 0) {
                  uVar7 = *(undefined4 *)((longlong)auStack_848 + lVar78);
                  auVar116._4_4_ = uVar7;
                  auVar116._0_4_ = uVar7;
                  auVar116._8_4_ = uVar7;
                  auVar116._12_4_ = uVar7;
                  auVar116._16_4_ = uVar7;
                  auVar116._20_4_ = uVar7;
                  auVar116._24_4_ = uVar7;
                  auVar116._28_4_ = uVar7;
                  auVar102 = vpmaddubsw_avx2(auVar116,*pauVar69);
                  auVar102 = vpmaddwd_avx2(auVar102,auVar88);
                  auVar105._0_4_ = auVar102._0_4_ + auVar106._0_4_;
                  auVar105._4_4_ = auVar102._4_4_ + auVar106._4_4_;
                  auVar105._8_4_ = auVar102._8_4_ + auVar106._8_4_;
                  auVar105._12_4_ = auVar102._12_4_ + auVar106._12_4_;
                  auVar105._16_4_ = auVar102._16_4_ + auVar106._16_4_;
                  auVar105._20_4_ = auVar102._20_4_ + auVar106._20_4_;
                  auVar105._24_4_ = auVar102._24_4_ + auVar106._24_4_;
                  auVar105._28_4_ = auVar102._28_4_ + auVar106._28_4_;
                  pauVar69 = pauVar69 + 1;
                }
                auVar102 = vcvtdq2ps_avx(auVar105);
                pfVar79 = (float *)(lVar74 + uVar68 * 4);
                fVar80 = pfVar79[1];
                fVar90 = pfVar79[2];
                fVar91 = pfVar79[3];
                fVar92 = pfVar79[4];
                fVar93 = pfVar79[5];
                fVar94 = pfVar79[6];
                fVar95 = pfVar79[7];
                pfVar64 = (float *)((longlong)param_2 + uVar68 * 4);
                *pfVar64 = auVar102._0_4_ * *pfVar79;
                pfVar64[1] = auVar102._4_4_ * fVar80;
                pfVar64[2] = auVar102._8_4_ * fVar90;
                pfVar64[3] = auVar102._12_4_ * fVar91;
                pfVar64[4] = auVar102._16_4_ * fVar92;
                pfVar64[5] = auVar102._20_4_ * fVar93;
                pfVar64[6] = auVar102._24_4_ * fVar94;
                pfVar64[7] = auVar102._28_4_ * fVar95;
                uVar68 = uVar68 + 8;
              } while (uVar68 < uVar66);
            }
          }
          else {
            uVar68 = 0;
            auVar87._2_2_ = DAT_18000f198;
            auVar87._0_2_ = DAT_18000f198;
            auVar87._4_2_ = DAT_18000f198;
            auVar87._6_2_ = DAT_18000f198;
            auVar87._8_2_ = DAT_18000f198;
            auVar87._10_2_ = DAT_18000f198;
            auVar87._12_2_ = DAT_18000f198;
            auVar87._14_2_ = DAT_18000f198;
            auVar87._16_2_ = DAT_18000f198;
            auVar87._18_2_ = DAT_18000f198;
            auVar87._20_2_ = DAT_18000f198;
            auVar87._22_2_ = DAT_18000f198;
            auVar87._24_2_ = DAT_18000f198;
            auVar87._26_2_ = DAT_18000f198;
            auVar87._28_2_ = DAT_18000f198;
            auVar87._30_2_ = DAT_18000f198;
            do {
              auVar89 = ZEXT1664((undefined1  [16])0x0);
              uVar73 = 0;
              do {
                uVar7 = *(undefined4 *)((longlong)auStack_848 + uVar73);
                auVar113._4_4_ = uVar7;
                auVar113._0_4_ = uVar7;
                auVar113._8_4_ = uVar7;
                auVar113._12_4_ = uVar7;
                auVar113._16_4_ = uVar7;
                auVar113._20_4_ = uVar7;
                auVar113._24_4_ = uVar7;
                auVar113._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar113,*pauVar69);
                auVar103 = vpmaddwd_avx2(auVar102,auVar87);
                uVar7 = *(undefined4 *)((longlong)auStack_848 + uVar73 + 4);
                auVar118._4_4_ = uVar7;
                auVar118._0_4_ = uVar7;
                auVar118._8_4_ = uVar7;
                auVar118._12_4_ = uVar7;
                auVar118._16_4_ = uVar7;
                auVar118._20_4_ = uVar7;
                auVar118._24_4_ = uVar7;
                auVar118._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar118,pauVar69[1]);
                auVar86 = vpmaddwd_avx2(auVar102,auVar87);
                uVar7 = *(undefined4 *)((longlong)auStack_840 + uVar73);
                auVar119._4_4_ = uVar7;
                auVar119._0_4_ = uVar7;
                auVar119._8_4_ = uVar7;
                auVar119._12_4_ = uVar7;
                auVar119._16_4_ = uVar7;
                auVar119._20_4_ = uVar7;
                auVar119._24_4_ = uVar7;
                auVar119._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar119,pauVar69[2]);
                auVar88 = vpmaddwd_avx2(auVar102,auVar87);
                uVar7 = *(undefined4 *)((longlong)auStack_840 + uVar73 + 4);
                auVar120._4_4_ = uVar7;
                auVar120._0_4_ = uVar7;
                auVar120._8_4_ = uVar7;
                auVar120._12_4_ = uVar7;
                auVar120._16_4_ = uVar7;
                auVar120._20_4_ = uVar7;
                auVar120._24_4_ = uVar7;
                auVar120._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar120,pauVar69[3]);
                auVar102 = vpmaddwd_avx2(auVar102,auVar87);
                auVar104._0_4_ =
                     auVar103._0_4_ + auVar89._0_4_ + auVar86._0_4_ + auVar88._0_4_ + auVar102._0_4_
                ;
                auVar104._4_4_ =
                     auVar103._4_4_ + auVar89._4_4_ + auVar86._4_4_ + auVar88._4_4_ + auVar102._4_4_
                ;
                auVar104._8_4_ =
                     auVar103._8_4_ + auVar89._8_4_ + auVar86._8_4_ + auVar88._8_4_ + auVar102._8_4_
                ;
                auVar104._12_4_ =
                     auVar103._12_4_ + auVar89._12_4_ + auVar86._12_4_ + auVar88._12_4_ +
                     auVar102._12_4_;
                auVar104._16_4_ =
                     auVar103._16_4_ + auVar89._16_4_ + auVar86._16_4_ + auVar88._16_4_ +
                     auVar102._16_4_;
                auVar104._20_4_ =
                     auVar103._20_4_ + auVar89._20_4_ + auVar86._20_4_ + auVar88._20_4_ +
                     auVar102._20_4_;
                auVar104._24_4_ =
                     auVar103._24_4_ + auVar89._24_4_ + auVar86._24_4_ + auVar88._24_4_ +
                     auVar102._24_4_;
                auVar104._28_4_ =
                     auVar103._28_4_ + auVar89._28_4_ + auVar86._28_4_ + auVar88._28_4_ +
                     auVar102._28_4_;
                auVar89 = ZEXT3264(auVar104);
                pauVar69 = pauVar69 + 4;
                uVar73 = uVar73 + 0x10;
              } while (uVar73 < uVar76 - 0xc);
              if ((int)uVar73 < (int)uVar76) {
                do {
                  uVar7 = *(undefined4 *)((longlong)auStack_848 + uVar73);
                  auVar114._4_4_ = uVar7;
                  auVar114._0_4_ = uVar7;
                  auVar114._8_4_ = uVar7;
                  auVar114._12_4_ = uVar7;
                  auVar114._16_4_ = uVar7;
                  auVar114._20_4_ = uVar7;
                  auVar114._24_4_ = uVar7;
                  auVar114._28_4_ = uVar7;
                  auVar102 = vpmaddubsw_avx2(auVar114,*pauVar69);
                  auVar102 = vpmaddwd_avx2(auVar102,auVar87);
                  auVar104._0_4_ = auVar102._0_4_ + auVar89._0_4_;
                  auVar104._4_4_ = auVar102._4_4_ + auVar89._4_4_;
                  auVar104._8_4_ = auVar102._8_4_ + auVar89._8_4_;
                  auVar104._12_4_ = auVar102._12_4_ + auVar89._12_4_;
                  auVar104._16_4_ = auVar102._16_4_ + auVar89._16_4_;
                  auVar104._20_4_ = auVar102._20_4_ + auVar89._20_4_;
                  auVar104._24_4_ = auVar102._24_4_ + auVar89._24_4_;
                  auVar104._28_4_ = auVar102._28_4_ + auVar89._28_4_;
                  auVar89 = ZEXT3264(auVar104);
                  pauVar69 = pauVar69 + 1;
                  uVar73 = uVar73 + 4;
                } while (uVar73 < uVar62);
              }
              auVar102 = vcvtdq2ps_avx(auVar104);
              pfVar79 = (float *)(lVar74 + uVar68 * 4);
              fVar80 = pfVar79[1];
              fVar90 = pfVar79[2];
              fVar91 = pfVar79[3];
              fVar92 = pfVar79[4];
              fVar93 = pfVar79[5];
              fVar94 = pfVar79[6];
              fVar95 = pfVar79[7];
              pfVar64 = (float *)((longlong)param_2 + uVar68 * 4);
              *pfVar64 = auVar102._0_4_ * *pfVar79;
              pfVar64[1] = auVar102._4_4_ * fVar80;
              pfVar64[2] = auVar102._8_4_ * fVar90;
              pfVar64[3] = auVar102._12_4_ * fVar91;
              pfVar64[4] = auVar102._16_4_ * fVar92;
              pfVar64[5] = auVar102._20_4_ * fVar93;
              pfVar64[6] = auVar102._24_4_ * fVar94;
              pfVar64[7] = auVar102._28_4_ * fVar95;
              uVar68 = uVar68 + 8;
            } while (uVar68 < uVar66);
          }
        }
      }
      else {
        if (0 < (int)uVar76) {
          if (uVar76 < 9) {
            lVar78 = 0;
          }
          else {
            uVar68 = (uVar62 - 1 >> 3) + 1 & 0xfffffffffffffffe;
            lVar78 = 0;
            do {
              pfVar79 = (float *)(param_3 + lVar78 * 4);
              auVar99._0_4_ = *pfVar79 * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._4_4_ = pfVar79[1] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._8_4_ = pfVar79[2] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._12_4_ = pfVar79[3] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._16_4_ = pfVar79[4] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._20_4_ = pfVar79[5] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._24_4_ = pfVar79[6] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar99._28_4_ = pfVar79[7] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102 = vcvtps2dq_avx(auVar99);
              auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
              auVar81 = vpackuswb_avx(auVar81,auVar81);
              auVar81 = vpshufd_avx(auVar81,4);
              auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
              *(undefined1 (*) [32])((longlong)auStack_848 + lVar78) = auVar102;
              pfVar79 = (float *)(param_3 + 0x20 + lVar78 * 4);
              auVar100._0_4_ = *pfVar79 * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._4_4_ = pfVar79[1] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._8_4_ = pfVar79[2] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._12_4_ = pfVar79[3] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._16_4_ = pfVar79[4] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._20_4_ = pfVar79[5] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._24_4_ = pfVar79[6] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar100._28_4_ = pfVar79[7] * DAT_18000f1f4 + DAT_18000f1f4;
              auVar102 = vcvtps2dq_avx(auVar100);
              auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
              auVar81 = vpackuswb_avx(auVar81,auVar81);
              auVar81 = vpshufd_avx(auVar81,4);
              auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
              *(undefined1 (*) [32])((longlong)auStack_840 + lVar78) = auVar102;
              lVar78 = lVar78 + 0x10;
              uVar68 = uVar68 - 2;
            } while (uVar68 != 0);
          }
          if ((uVar62 - 1 & 8) == 0) {
            pfVar79 = (float *)(param_3 + lVar78 * 4);
            auVar83._0_4_ = DAT_18000f1f4 * *pfVar79 + DAT_18000f1f4;
            auVar83._4_4_ = DAT_18000f1f4 * pfVar79[1] + DAT_18000f1f4;
            auVar83._8_4_ = DAT_18000f1f4 * pfVar79[2] + DAT_18000f1f4;
            auVar83._12_4_ = DAT_18000f1f4 * pfVar79[3] + DAT_18000f1f4;
            auVar83._16_4_ = DAT_18000f1f4 * pfVar79[4] + DAT_18000f1f4;
            auVar83._20_4_ = DAT_18000f1f4 * pfVar79[5] + DAT_18000f1f4;
            auVar83._24_4_ = DAT_18000f1f4 * pfVar79[6] + DAT_18000f1f4;
            auVar83._28_4_ = DAT_18000f1f4 * pfVar79[7] + DAT_18000f1f4;
            auVar102 = vcvtps2dq_avx(auVar83);
            auVar81 = vpackusdw_avx(auVar102._0_16_,auVar102._16_16_);
            auVar81 = vpackuswb_avx(auVar81,auVar81);
            auVar81 = vpshufd_avx(auVar81,4);
            auVar102 = vpermq_avx2(ZEXT1632(auVar81),0x54);
            *(undefined1 (*) [32])((longlong)auStack_848 + lVar78) = auVar102;
          }
        }
        if (0 < (int)uVar8) {
          uVar68 = 0;
          auVar84._2_2_ = DAT_18000f198;
          auVar84._0_2_ = DAT_18000f198;
          auVar84._4_2_ = DAT_18000f198;
          auVar84._6_2_ = DAT_18000f198;
          auVar84._8_2_ = DAT_18000f198;
          auVar84._10_2_ = DAT_18000f198;
          auVar84._12_2_ = DAT_18000f198;
          auVar84._14_2_ = DAT_18000f198;
          auVar84._16_2_ = DAT_18000f198;
          auVar84._18_2_ = DAT_18000f198;
          auVar84._20_2_ = DAT_18000f198;
          auVar84._22_2_ = DAT_18000f198;
          auVar84._24_2_ = DAT_18000f198;
          auVar84._26_2_ = DAT_18000f198;
          auVar84._28_2_ = DAT_18000f198;
          auVar84._30_2_ = DAT_18000f198;
          do {
            uVar76 = *puVar72;
            puVar72 = puVar72 + 1;
            auVar89 = ZEXT1664((undefined1  [16])0x0);
            auVar101 = auVar89._0_32_;
            if ((int)uVar76 < 4) {
              uVar67 = 0;
              if (0 < (int)uVar76) goto LAB_18000963b;
            }
            else {
              iVar71 = 0;
              do {
                uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)*puVar72);
                auVar107._4_4_ = uVar7;
                auVar107._0_4_ = uVar7;
                auVar107._8_4_ = uVar7;
                auVar107._12_4_ = uVar7;
                auVar107._16_4_ = uVar7;
                auVar107._20_4_ = uVar7;
                auVar107._24_4_ = uVar7;
                auVar107._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar107,*pauVar69);
                auVar103 = vpmaddwd_avx2(auVar102,auVar84);
                uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)puVar72[1]);
                auVar108._4_4_ = uVar7;
                auVar108._0_4_ = uVar7;
                auVar108._8_4_ = uVar7;
                auVar108._12_4_ = uVar7;
                auVar108._16_4_ = uVar7;
                auVar108._20_4_ = uVar7;
                auVar108._24_4_ = uVar7;
                auVar108._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar108,pauVar69[1]);
                auVar86 = vpmaddwd_avx2(auVar102,auVar84);
                uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)puVar72[2]);
                auVar117._4_4_ = uVar7;
                auVar117._0_4_ = uVar7;
                auVar117._8_4_ = uVar7;
                auVar117._12_4_ = uVar7;
                auVar117._16_4_ = uVar7;
                auVar117._20_4_ = uVar7;
                auVar117._24_4_ = uVar7;
                auVar117._28_4_ = uVar7;
                auVar102 = vpmaddubsw_avx2(auVar117,pauVar69[2]);
                auVar88 = vpmaddwd_avx2(auVar102,auVar84);
                uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)puVar72[3]);
                auVar109._4_4_ = uVar7;
                auVar109._0_4_ = uVar7;
                auVar109._8_4_ = uVar7;
                auVar109._12_4_ = uVar7;
                auVar109._16_4_ = uVar7;
                auVar109._20_4_ = uVar7;
                auVar109._24_4_ = uVar7;
                auVar109._28_4_ = uVar7;
                puVar72 = puVar72 + 4;
                auVar102 = vpmaddubsw_avx2(auVar109,pauVar69[3]);
                auVar102 = vpmaddwd_avx2(auVar102,auVar84);
                auVar101._0_4_ =
                     auVar103._0_4_ + auVar89._0_4_ + auVar86._0_4_ + auVar88._0_4_ + auVar102._0_4_
                ;
                auVar101._4_4_ =
                     auVar103._4_4_ + auVar89._4_4_ + auVar86._4_4_ + auVar88._4_4_ + auVar102._4_4_
                ;
                auVar101._8_4_ =
                     auVar103._8_4_ + auVar89._8_4_ + auVar86._8_4_ + auVar88._8_4_ + auVar102._8_4_
                ;
                auVar101._12_4_ =
                     auVar103._12_4_ + auVar89._12_4_ + auVar86._12_4_ + auVar88._12_4_ +
                     auVar102._12_4_;
                auVar101._16_4_ =
                     auVar103._16_4_ + auVar89._16_4_ + auVar86._16_4_ + auVar88._16_4_ +
                     auVar102._16_4_;
                auVar101._20_4_ =
                     auVar103._20_4_ + auVar89._20_4_ + auVar86._20_4_ + auVar88._20_4_ +
                     auVar102._20_4_;
                auVar101._24_4_ =
                     auVar103._24_4_ + auVar89._24_4_ + auVar86._24_4_ + auVar88._24_4_ +
                     auVar102._24_4_;
                auVar101._28_4_ =
                     auVar103._28_4_ + auVar89._28_4_ + auVar86._28_4_ + auVar88._28_4_ +
                     auVar102._28_4_;
                auVar89 = ZEXT3264(auVar101);
                pauVar69 = pauVar69 + 4;
                iVar71 = iVar71 + 4;
              } while (iVar71 < (int)(uVar76 - 3));
              uVar67 = uVar76 & 0x7ffffffc;
              if ((int)uVar67 < (int)uVar76) {
LAB_18000963b:
                uVar75 = uVar67;
                if ((uVar76 & 1) != 0) {
                  uVar75 = *puVar72;
                  puVar72 = puVar72 + 1;
                  uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)uVar75);
                  auVar110._4_4_ = uVar7;
                  auVar110._0_4_ = uVar7;
                  auVar110._8_4_ = uVar7;
                  auVar110._12_4_ = uVar7;
                  auVar110._16_4_ = uVar7;
                  auVar110._20_4_ = uVar7;
                  auVar110._24_4_ = uVar7;
                  auVar110._28_4_ = uVar7;
                  auVar102 = vpmaddubsw_avx2(auVar110,*pauVar69);
                  auVar102 = vpmaddwd_avx2(auVar102,auVar84);
                  auVar89 = ZEXT3264(CONCAT428(auVar102._28_4_ + auVar89._28_4_,
                                               CONCAT424(auVar102._24_4_ + auVar89._24_4_,
                                                         CONCAT420(auVar102._20_4_ + auVar89._20_4_,
                                                                   CONCAT416(auVar102._16_4_ +
                                                                             auVar89._16_4_,
                                                                             CONCAT412(auVar102.
                                                  _12_4_ + auVar89._12_4_,
                                                  CONCAT48(auVar102._8_4_ + auVar89._8_4_,
                                                           CONCAT44(auVar102._4_4_ + auVar89._4_4_,
                                                                    auVar102._0_4_ + auVar89._0_4_))
                                                  ))))));
                  pauVar69 = pauVar69 + 1;
                  uVar75 = uVar67 | 1;
                }
                auVar101 = auVar89._0_32_;
                if (uVar76 != (uVar67 | 1)) {
                  iVar71 = uVar76 - uVar75;
                  do {
                    uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)*puVar72);
                    auVar111._4_4_ = uVar7;
                    auVar111._0_4_ = uVar7;
                    auVar111._8_4_ = uVar7;
                    auVar111._12_4_ = uVar7;
                    auVar111._16_4_ = uVar7;
                    auVar111._20_4_ = uVar7;
                    auVar111._24_4_ = uVar7;
                    auVar111._28_4_ = uVar7;
                    auVar102 = vpmaddubsw_avx2(auVar111,*pauVar69);
                    auVar103 = vpmaddwd_avx2(auVar102,auVar84);
                    puVar1 = puVar72 + 1;
                    puVar72 = puVar72 + 2;
                    uVar7 = *(undefined4 *)((longlong)auStack_848 + (longlong)(int)*puVar1);
                    auVar112._4_4_ = uVar7;
                    auVar112._0_4_ = uVar7;
                    auVar112._8_4_ = uVar7;
                    auVar112._12_4_ = uVar7;
                    auVar112._16_4_ = uVar7;
                    auVar112._20_4_ = uVar7;
                    auVar112._24_4_ = uVar7;
                    auVar112._28_4_ = uVar7;
                    auVar102 = vpmaddubsw_avx2(auVar112,pauVar69[1]);
                    auVar102 = vpmaddwd_avx2(auVar102,auVar84);
                    auVar101._0_4_ = auVar102._0_4_ + auVar103._0_4_ + auVar89._0_4_;
                    auVar101._4_4_ = auVar102._4_4_ + auVar103._4_4_ + auVar89._4_4_;
                    auVar101._8_4_ = auVar102._8_4_ + auVar103._8_4_ + auVar89._8_4_;
                    auVar101._12_4_ = auVar102._12_4_ + auVar103._12_4_ + auVar89._12_4_;
                    auVar101._16_4_ = auVar102._16_4_ + auVar103._16_4_ + auVar89._16_4_;
                    auVar101._20_4_ = auVar102._20_4_ + auVar103._20_4_ + auVar89._20_4_;
                    auVar101._24_4_ = auVar102._24_4_ + auVar103._24_4_ + auVar89._24_4_;
                    auVar101._28_4_ = auVar102._28_4_ + auVar103._28_4_ + auVar89._28_4_;
                    auVar89 = ZEXT3264(auVar101);
                    pauVar69 = pauVar69 + 2;
                    iVar71 = iVar71 + -2;
                  } while (iVar71 != 0);
                }
              }
            }
            auVar102 = vcvtdq2ps_avx(auVar101);
            pfVar79 = (float *)(lVar74 + uVar68 * 4);
            fVar80 = pfVar79[1];
            fVar90 = pfVar79[2];
            fVar91 = pfVar79[3];
            fVar92 = pfVar79[4];
            fVar93 = pfVar79[5];
            fVar94 = pfVar79[6];
            fVar95 = pfVar79[7];
            pfVar64 = (float *)((longlong)param_2 + uVar68 * 4);
            *pfVar64 = auVar102._0_4_ * *pfVar79;
            pfVar64[1] = auVar102._4_4_ * fVar80;
            pfVar64[2] = auVar102._8_4_ * fVar90;
            pfVar64[3] = auVar102._12_4_ * fVar91;
            pfVar64[4] = auVar102._16_4_ * fVar92;
            pfVar64[5] = auVar102._20_4_ * fVar93;
            pfVar64[6] = auVar102._24_4_ * fVar94;
            pfVar64[7] = auVar102._28_4_ * fVar95;
            uVar68 = uVar68 + 8;
          } while (uVar68 < local_868);
        }
      }
      pvVar65 = (void *)param_1[1];
      goto LAB_180009e99;
    }
    _Size = uVar66 << 2;
    _Dst = param_2;
LAB_180009b56:
    memset(_Dst,0,_Size);
LAB_180009b67:
    uVar77 = (uint)local_868;
  }
  else {
    piVar61 = (int *)param_1[4];
    if (piVar61 == (int *)0x0) {
      uVar68 = 0;
      if (0xf < (int)uVar8) {
        uVar67 = uVar8 - 0xf;
        if ((int)uVar76 < 1) {
          memset(param_2,0,(ulonglong)(uVar8 - 0x10 >> 4) * 0x40 + 0x40);
          uVar77 = (uint)local_868;
          uVar75 = 0x10;
          if (0x10 < uVar67) {
            uVar75 = uVar67;
          }
          uVar68 = (ulonglong)((uVar75 - 1 & 0xfffffff0) + 0x10);
        }
        else {
          pfVar64 = pfVar79 + 8;
          uVar68 = 0;
          do {
            fVar80 = 0.0;
            fVar90 = 0.0;
            fVar91 = 0.0;
            fVar92 = 0.0;
            fVar93 = 0.0;
            fVar94 = 0.0;
            fVar95 = 0.0;
            fVar96 = 0.0;
            if (uVar76 == 1) {
              uVar73 = 0;
              auVar97 = SUB6432(ZEXT864(0),0);
              fVar80 = 0.0;
              fVar90 = 0.0;
              fVar91 = 0.0;
              fVar92 = 0.0;
              fVar93 = 0.0;
              fVar94 = 0.0;
              fVar95 = 0.0;
              fVar96 = 0.0;
            }
            else {
              uVar73 = 0;
              auVar89 = ZEXT864(0);
              pfVar63 = pfVar64;
              do {
                fVar6 = *(float *)(param_3 + uVar73 * 4);
                fVar5 = *(float *)(param_3 + 4 + uVar73 * 4);
                pfVar2 = pfVar63 + (local_868 - 8);
                fVar80 = fVar5 * *pfVar2 + fVar6 * pfVar63[-8] + fVar80;
                fVar90 = fVar5 * pfVar2[1] + fVar6 * pfVar63[-7] + fVar90;
                fVar91 = fVar5 * pfVar2[2] + fVar6 * pfVar63[-6] + fVar91;
                fVar92 = fVar5 * pfVar2[3] + fVar6 * pfVar63[-5] + fVar92;
                fVar93 = fVar5 * pfVar2[4] + fVar6 * pfVar63[-4] + fVar93;
                fVar94 = fVar5 * pfVar2[5] + fVar6 * pfVar63[-3] + fVar94;
                fVar95 = fVar5 * pfVar2[6] + fVar6 * pfVar63[-2] + fVar95;
                fVar96 = fVar5 * pfVar2[7] + fVar6 * pfVar63[-1] + fVar96;
                pfVar2 = pfVar63 + local_868;
                auVar97._0_4_ = fVar5 * *pfVar2 + fVar6 * *pfVar63 + auVar89._0_4_;
                auVar97._4_4_ = fVar5 * pfVar2[1] + fVar6 * pfVar63[1] + auVar89._4_4_;
                auVar97._8_4_ = fVar5 * pfVar2[2] + fVar6 * pfVar63[2] + auVar89._8_4_;
                auVar97._12_4_ = fVar5 * pfVar2[3] + fVar6 * pfVar63[3] + auVar89._12_4_;
                auVar97._16_4_ = fVar5 * pfVar2[4] + fVar6 * pfVar63[4] + auVar89._16_4_;
                auVar97._20_4_ = fVar5 * pfVar2[5] + fVar6 * pfVar63[5] + auVar89._20_4_;
                auVar97._24_4_ = fVar5 * pfVar2[6] + fVar6 * pfVar63[6] + auVar89._24_4_;
                auVar97._28_4_ = fVar5 * pfVar2[7] + fVar6 * pfVar63[7] + auVar89._28_4_;
                auVar89 = ZEXT3264(auVar97);
                uVar73 = uVar73 + 2;
                pfVar63 = pfVar63 + local_868 * 2;
              } while ((uVar76 & 0x7ffffffe) != uVar73);
            }
            auVar98 = auVar97;
            if ((uVar76 & 1) != 0) {
              fVar6 = *(float *)(param_3 + uVar73 * 4);
              pfVar63 = pfVar79 + uVar68 + uVar73 * local_868;
              fVar80 = fVar6 * *pfVar63 + fVar80;
              fVar90 = fVar6 * pfVar63[1] + fVar90;
              fVar91 = fVar6 * pfVar63[2] + fVar91;
              fVar92 = fVar6 * pfVar63[3] + fVar92;
              fVar93 = fVar6 * pfVar63[4] + fVar93;
              fVar94 = fVar6 * pfVar63[5] + fVar94;
              fVar95 = fVar6 * pfVar63[6] + fVar95;
              fVar96 = fVar6 * pfVar63[7] + fVar96;
              pfVar63 = pfVar79 + uVar68 + uVar73 * local_868 + 8;
              auVar98._0_4_ = fVar6 * *pfVar63 + auVar97._0_4_;
              auVar98._4_4_ = fVar6 * pfVar63[1] + auVar97._4_4_;
              auVar98._8_4_ = fVar6 * pfVar63[2] + auVar97._8_4_;
              auVar98._12_4_ = fVar6 * pfVar63[3] + auVar97._12_4_;
              auVar98._16_4_ = fVar6 * pfVar63[4] + auVar97._16_4_;
              auVar98._20_4_ = fVar6 * pfVar63[5] + auVar97._20_4_;
              auVar98._24_4_ = fVar6 * pfVar63[6] + auVar97._24_4_;
              auVar98._28_4_ = fVar6 * pfVar63[7] + auVar97._28_4_;
            }
            pfVar63 = (float *)((longlong)param_2 + uVar68 * 4);
            *pfVar63 = fVar80;
            pfVar63[1] = fVar90;
            pfVar63[2] = fVar91;
            pfVar63[3] = fVar92;
            pfVar63[4] = fVar93;
            pfVar63[5] = fVar94;
            pfVar63[6] = fVar95;
            pfVar63[7] = fVar96;
            *(undefined1 (*) [32])((longlong)param_2 + uVar68 * 4 + 0x20) = auVar98;
            uVar68 = uVar68 + 0x10;
            pfVar64 = pfVar64 + 0x10;
          } while (uVar68 < uVar67);
        }
      }
      lVar74 = uVar66 - 7;
      uVar67 = (uint)uVar68;
      local_858 = pvVar65;
      if ((int)uVar67 < (int)lVar74) {
        local_860 = uVar62;
        if ((int)uVar76 < 1) {
          memset((void *)((longlong)param_2 + (uVar68 & 0xffffffff) * 4),0,
                 (ulonglong)((uVar77 - uVar67) - 8 >> 3) * 0x20 + 0x20);
          uVar77 = (uint)local_868;
          lVar78 = (uVar68 & 0xffffffff) + 8;
          if (lVar78 <= lVar74) {
            lVar78 = lVar74;
          }
          uVar68 = (ulonglong)(uVar67 + ((int)lVar78 + ~uVar67 & 0xfffffff8) + 8);
        }
        else {
          uVar68 = uVar68 & 0xffffffff;
          pfVar64 = pfVar79 + uVar68;
          do {
            if (uVar76 < 4) {
              auVar89 = ZEXT864(0);
              uVar62 = 0;
            }
            else {
              auVar89 = ZEXT864(0);
              uVar62 = 0;
              pfVar63 = pfVar64;
              do {
                fVar80 = *(float *)(param_3 + uVar62 * 4);
                fVar90 = *(float *)(param_3 + 4 + uVar62 * 4);
                pfVar2 = pfVar63 + uVar66;
                fVar91 = *(float *)(param_3 + 8 + uVar62 * 4);
                pfVar3 = pfVar63 + uVar66 * 2;
                fVar92 = *(float *)(param_3 + 0xc + uVar62 * 4);
                pfVar4 = pfVar63 + uVar66 * 3;
                auVar89 = ZEXT3264(CONCAT428(fVar92 * pfVar4[7] +
                                             fVar91 * pfVar3[7] +
                                             fVar90 * pfVar2[7] +
                                             fVar80 * pfVar63[7] + auVar89._28_4_,
                                             CONCAT424(fVar92 * pfVar4[6] +
                                                       fVar91 * pfVar3[6] +
                                                       fVar90 * pfVar2[6] +
                                                       fVar80 * pfVar63[6] + auVar89._24_4_,
                                                       CONCAT420(fVar92 * pfVar4[5] +
                                                                 fVar91 * pfVar3[5] +
                                                                 fVar90 * pfVar2[5] +
                                                                 fVar80 * pfVar63[5] +
                                                                 auVar89._20_4_,
                                                                 CONCAT416(fVar92 * pfVar4[4] +
                                                                           fVar91 * pfVar3[4] +
                                                                           fVar90 * pfVar2[4] +
                                                                           fVar80 * pfVar63[4] +
                                                                           auVar89._16_4_,
                                                                           CONCAT412(fVar92 * pfVar4
                                                  [3] + fVar91 * pfVar3[3] +
                                                        fVar90 * pfVar2[3] +
                                                        fVar80 * pfVar63[3] + auVar89._12_4_,
                                                  CONCAT48(fVar92 * pfVar4[2] +
                                                           fVar91 * pfVar3[2] +
                                                           fVar90 * pfVar2[2] +
                                                           fVar80 * pfVar63[2] + auVar89._8_4_,
                                                           CONCAT44(fVar92 * pfVar4[1] +
                                                                    fVar91 * pfVar3[1] +
                                                                    fVar90 * pfVar2[1] +
                                                                    fVar80 * pfVar63[1] +
                                                                    auVar89._4_4_,
                                                                    fVar92 * *pfVar4 +
                                                                    fVar91 * *pfVar3 +
                                                                    fVar90 * *pfVar2 +
                                                                    fVar80 * *pfVar63 +
                                                                    auVar89._0_4_))))))));
                uVar62 = uVar62 + 4;
                pfVar63 = pfVar63 + uVar66 * 4;
              } while ((uVar76 & 0x7ffffffc) != uVar62);
            }
            auVar85 = auVar89._0_32_;
            if ((ulonglong)(uVar76 & 3) != 0) {
              pfVar63 = (float *)(uVar66 * 4 * uVar62 + (longlong)pfVar64);
              uVar73 = 0;
              do {
                fVar80 = *(float *)(param_3 + uVar62 * 4 + uVar73 * 4);
                auVar85._0_4_ = fVar80 * *pfVar63 + auVar89._0_4_;
                auVar85._4_4_ = fVar80 * pfVar63[1] + auVar89._4_4_;
                auVar85._8_4_ = fVar80 * pfVar63[2] + auVar89._8_4_;
                auVar85._12_4_ = fVar80 * pfVar63[3] + auVar89._12_4_;
                auVar85._16_4_ = fVar80 * pfVar63[4] + auVar89._16_4_;
                auVar85._20_4_ = fVar80 * pfVar63[5] + auVar89._20_4_;
                auVar85._24_4_ = fVar80 * pfVar63[6] + auVar89._24_4_;
                auVar85._28_4_ = fVar80 * pfVar63[7] + auVar89._28_4_;
                auVar89 = ZEXT3264(auVar85);
                uVar73 = uVar73 + 1;
                pfVar63 = pfVar63 + uVar66;
              } while ((uVar76 & 3) != uVar73);
            }
            *(undefined1 (*) [32])((longlong)param_2 + uVar68 * 4) = auVar85;
            uVar68 = uVar68 + 8;
            pfVar64 = pfVar64 + 8;
          } while ((longlong)uVar68 < (longlong)(int)lVar74);
          uVar77 = (uint)local_868;
        }
        uVar62 = local_860;
        if ((int)uVar68 < (int)(uVar8 - 3)) goto LAB_18000983e;
LAB_1800099e5:
        local_850 = uVar66 - 3;
        uVar76 = (uint)uVar68;
        pvVar65 = local_858;
      }
      else {
        if ((int)(uVar8 - 3) <= (int)uVar67) goto LAB_1800099e5;
LAB_18000983e:
        local_850 = uVar66 - 3;
        uVar77 = (uint)uVar62;
        uVar76 = (uint)uVar68;
        if ((int)uVar77 < 1) {
          iVar71 = uVar76 + 4;
          if ((int)(uVar76 + 4) < (int)local_850) {
            iVar71 = (int)local_850;
          }
          memset((void *)((longlong)param_2 + (uVar68 & 0xffffffff) * 4),0,
                 (ulonglong)(~uVar76 + iVar71 >> 2) * 0x10 + 0x10);
          uVar76 = uVar76 + (~uVar76 + iVar71 & 0xfffffffc) + 4;
          uVar68 = (ulonglong)uVar76;
          uVar77 = (uint)local_868;
          pvVar65 = local_858;
        }
        else {
          uVar68 = uVar68 & 0xffffffff;
          pfVar64 = pfVar79 + uVar68;
          do {
            if (uVar77 < 4) {
              auVar89 = ZEXT864(0);
              uVar73 = 0;
            }
            else {
              auVar89 = ZEXT864(0);
              uVar73 = 0;
              pfVar63 = pfVar64;
              do {
                fVar80 = *(float *)(param_3 + uVar73 * 4);
                fVar90 = *(float *)(param_3 + 4 + uVar73 * 4);
                pfVar2 = pfVar63 + uVar66;
                fVar91 = *(float *)(param_3 + 8 + uVar73 * 4);
                pfVar3 = pfVar63 + uVar66 * 2;
                fVar92 = *(float *)(param_3 + 0xc + uVar73 * 4);
                pfVar4 = pfVar63 + uVar66 * 3;
                auVar89 = ZEXT1664(CONCAT412(fVar92 * pfVar4[3] +
                                             fVar91 * pfVar3[3] +
                                             fVar90 * pfVar2[3] +
                                             fVar80 * pfVar63[3] + auVar89._12_4_,
                                             CONCAT48(fVar92 * pfVar4[2] +
                                                      fVar91 * pfVar3[2] +
                                                      fVar90 * pfVar2[2] +
                                                      fVar80 * pfVar63[2] + auVar89._8_4_,
                                                      CONCAT44(fVar92 * pfVar4[1] +
                                                               fVar91 * pfVar3[1] +
                                                               fVar90 * pfVar2[1] +
                                                               fVar80 * pfVar63[1] + auVar89._4_4_,
                                                               fVar92 * *pfVar4 +
                                                               fVar91 * *pfVar3 +
                                                               fVar90 * *pfVar2 +
                                                               fVar80 * *pfVar63 + auVar89._0_4_))))
                ;
                uVar73 = uVar73 + 4;
                pfVar63 = pfVar63 + uVar66 * 4;
              } while ((uVar77 & 0x7ffffffc) != uVar73);
            }
            auVar81 = auVar89._0_16_;
            if ((ulonglong)(uVar77 & 3) != 0) {
              pfVar63 = (float *)(uVar66 * 4 * uVar73 + (longlong)pfVar64);
              uVar70 = 0;
              do {
                fVar80 = *(float *)(param_3 + uVar73 * 4 + uVar70 * 4);
                auVar81._0_4_ = fVar80 * *pfVar63 + auVar89._0_4_;
                auVar81._4_4_ = fVar80 * pfVar63[1] + auVar89._4_4_;
                auVar81._8_4_ = fVar80 * pfVar63[2] + auVar89._8_4_;
                auVar81._12_4_ = fVar80 * pfVar63[3] + auVar89._12_4_;
                auVar89 = ZEXT1664(auVar81);
                uVar70 = uVar70 + 1;
                pfVar63 = pfVar63 + uVar66;
              } while ((uVar77 & 3) != uVar70);
            }
            *(undefined1 (*) [16])((longlong)param_2 + uVar68 * 4) = auVar81;
            uVar68 = uVar68 + 4;
            pfVar64 = pfVar64 + 4;
          } while ((longlong)uVar68 < local_850);
          uVar77 = (uint)local_868;
          uVar76 = (uint)uVar68;
          local_860 = uVar62;
          pvVar65 = local_858;
        }
      }
      local_858 = pvVar65;
      if ((int)uVar76 < (int)uVar8) {
        uVar76 = (uint)uVar62;
        if ((int)uVar76 < 1) {
          _Size = (ulonglong)(~(uint)uVar68 + uVar8) * 4 + 4;
          _Dst = (void *)((longlong)param_2 + (uVar68 & 0xffffffff) * 4);
          goto LAB_180009b56;
        }
        uVar68 = (ulonglong)(int)(uint)uVar68;
        pfVar64 = pfVar79 + uVar68;
        do {
          *(undefined4 *)((longlong)param_2 + uVar68 * 4) = 0;
          fVar80 = 0.0;
          if (uVar76 == 1) {
            uVar73 = 0;
          }
          else {
            uVar73 = 0;
            pfVar63 = pfVar64;
            do {
              fVar80 = *pfVar63 * *(float *)(param_3 + uVar73 * 4) + fVar80;
              *(float *)((longlong)param_2 + uVar68 * 4) = fVar80;
              fVar80 = pfVar63[uVar66] * *(float *)(param_3 + 4 + uVar73 * 4) + fVar80;
              *(float *)((longlong)param_2 + uVar68 * 4) = fVar80;
              uVar73 = uVar73 + 2;
              pfVar63 = pfVar63 + uVar66 * 2;
            } while ((uVar76 & 0x7ffffffe) != uVar73);
          }
          if ((uVar62 & 1) != 0) {
            *(float *)((longlong)param_2 + uVar68 * 4) =
                 pfVar79[uVar68 + uVar73 * uVar66] * *(float *)(param_3 + uVar73 * 4) + fVar80;
          }
          uVar68 = uVar68 + 1;
          pfVar64 = pfVar64 + 1;
        } while (uVar68 != uVar66);
        goto LAB_180009b67;
      }
    }
    else {
      if ((int)uVar8 < 1) goto LAB_18000a004;
      uVar68 = 0;
      do {
        iVar71 = *piVar61;
        piVar61 = piVar61 + 1;
        auVar89 = ZEXT1264(ZEXT812(0));
        auVar82 = ZEXT1232(ZEXT812(0));
        if (0 < iVar71) {
          do {
            lVar74 = (longlong)*piVar61;
            fVar80 = *(float *)(param_3 + lVar74 * 4);
            fVar90 = *(float *)(param_3 + 4 + lVar74 * 4);
            fVar91 = *(float *)(param_3 + 8 + lVar74 * 4);
            fVar92 = *(float *)(param_3 + 0xc + lVar74 * 4);
            auVar82._0_4_ =
                 fVar92 * pfVar79[0x18] +
                 fVar91 * pfVar79[0x10] + fVar90 * pfVar79[8] + fVar80 * *pfVar79 + auVar89._0_4_;
            auVar82._4_4_ =
                 fVar92 * pfVar79[0x19] +
                 fVar91 * pfVar79[0x11] + fVar90 * pfVar79[9] + fVar80 * pfVar79[1] + auVar89._4_4_;
            auVar82._8_4_ =
                 fVar92 * pfVar79[0x1a] +
                 fVar91 * pfVar79[0x12] + fVar90 * pfVar79[10] + fVar80 * pfVar79[2] + auVar89._8_4_
            ;
            auVar82._12_4_ =
                 fVar92 * pfVar79[0x1b] +
                 fVar91 * pfVar79[0x13] +
                 fVar90 * pfVar79[0xb] + fVar80 * pfVar79[3] + auVar89._12_4_;
            auVar82._16_4_ =
                 fVar92 * pfVar79[0x1c] +
                 fVar91 * pfVar79[0x14] +
                 fVar90 * pfVar79[0xc] + fVar80 * pfVar79[4] + auVar89._16_4_;
            auVar82._20_4_ =
                 fVar92 * pfVar79[0x1d] +
                 fVar91 * pfVar79[0x15] +
                 fVar90 * pfVar79[0xd] + fVar80 * pfVar79[5] + auVar89._20_4_;
            auVar82._24_4_ =
                 fVar92 * pfVar79[0x1e] +
                 fVar91 * pfVar79[0x16] +
                 fVar90 * pfVar79[0xe] + fVar80 * pfVar79[6] + auVar89._24_4_;
            auVar82._28_4_ =
                 fVar92 * pfVar79[0x1f] +
                 fVar91 * pfVar79[0x17] +
                 fVar90 * pfVar79[0xf] + fVar80 * pfVar79[7] + auVar89._28_4_;
            auVar89 = ZEXT3264(auVar82);
            pfVar79 = pfVar79 + 0x20;
            piVar61 = piVar61 + 1;
            iVar71 = iVar71 + -1;
          } while (iVar71 != 0);
        }
        *(undefined1 (*) [32])((longlong)param_2 + uVar68 * 4) = auVar82;
        uVar68 = uVar68 + 8;
      } while (uVar68 < local_868);
    }
  }
LAB_180009e99:
  if (0 < (int)uVar8 && pvVar65 != (void *)0x0) {
    if ((uVar77 < 0x20) ||
       (param_2 < (void *)(uVar66 * 4 + (longlong)pvVar65) &&
        pvVar65 < (void *)((longlong)param_2 + uVar66 * 4))) {
      uVar68 = 0;
    }
    else {
      uVar68 = (ulonglong)(uVar8 & 0x7fffffe0);
      lVar74 = 0;
      do {
        pfVar63 = (float *)((longlong)pvVar65 + lVar74);
        fVar80 = pfVar63[1];
        fVar90 = pfVar63[2];
        fVar91 = pfVar63[3];
        fVar92 = pfVar63[4];
        fVar93 = pfVar63[5];
        fVar94 = pfVar63[6];
        fVar95 = pfVar63[7];
        pfVar79 = (float *)((longlong)pvVar65 + lVar74 + 0x20);
        fVar96 = *pfVar79;
        fVar6 = pfVar79[1];
        fVar5 = pfVar79[2];
        fVar9 = pfVar79[3];
        fVar10 = pfVar79[4];
        fVar11 = pfVar79[5];
        fVar12 = pfVar79[6];
        fVar13 = pfVar79[7];
        pfVar79 = (float *)((longlong)pvVar65 + lVar74 + 0x40);
        fVar14 = *pfVar79;
        fVar15 = pfVar79[1];
        fVar16 = pfVar79[2];
        fVar17 = pfVar79[3];
        fVar18 = pfVar79[4];
        fVar19 = pfVar79[5];
        fVar20 = pfVar79[6];
        fVar21 = pfVar79[7];
        pfVar79 = (float *)((longlong)pvVar65 + lVar74 + 0x60);
        fVar22 = *pfVar79;
        fVar23 = pfVar79[1];
        fVar24 = pfVar79[2];
        fVar25 = pfVar79[3];
        fVar26 = pfVar79[4];
        fVar27 = pfVar79[5];
        fVar28 = pfVar79[6];
        fVar29 = pfVar79[7];
        pfVar79 = (float *)((longlong)param_2 + lVar74);
        fVar30 = pfVar79[1];
        fVar31 = pfVar79[2];
        fVar32 = pfVar79[3];
        fVar33 = pfVar79[4];
        fVar34 = pfVar79[5];
        fVar35 = pfVar79[6];
        fVar36 = pfVar79[7];
        pfVar64 = (float *)((longlong)param_2 + lVar74 + 0x20);
        fVar37 = *pfVar64;
        fVar38 = pfVar64[1];
        fVar39 = pfVar64[2];
        fVar40 = pfVar64[3];
        fVar41 = pfVar64[4];
        fVar42 = pfVar64[5];
        fVar43 = pfVar64[6];
        fVar44 = pfVar64[7];
        pfVar64 = (float *)((longlong)param_2 + lVar74 + 0x40);
        fVar45 = *pfVar64;
        fVar46 = pfVar64[1];
        fVar47 = pfVar64[2];
        fVar48 = pfVar64[3];
        fVar49 = pfVar64[4];
        fVar50 = pfVar64[5];
        fVar51 = pfVar64[6];
        fVar52 = pfVar64[7];
        pfVar64 = (float *)((longlong)param_2 + lVar74 + 0x60);
        fVar53 = *pfVar64;
        fVar54 = pfVar64[1];
        fVar55 = pfVar64[2];
        fVar56 = pfVar64[3];
        fVar57 = pfVar64[4];
        fVar58 = pfVar64[5];
        fVar59 = pfVar64[6];
        fVar60 = pfVar64[7];
        pfVar64 = (float *)((longlong)param_2 + lVar74);
        *pfVar64 = *pfVar63 + *pfVar79;
        pfVar64[1] = fVar80 + fVar30;
        pfVar64[2] = fVar90 + fVar31;
        pfVar64[3] = fVar91 + fVar32;
        pfVar64[4] = fVar92 + fVar33;
        pfVar64[5] = fVar93 + fVar34;
        pfVar64[6] = fVar94 + fVar35;
        pfVar64[7] = fVar95 + fVar36;
        pfVar79 = (float *)((longlong)param_2 + lVar74 + 0x20);
        *pfVar79 = fVar96 + fVar37;
        pfVar79[1] = fVar6 + fVar38;
        pfVar79[2] = fVar5 + fVar39;
        pfVar79[3] = fVar9 + fVar40;
        pfVar79[4] = fVar10 + fVar41;
        pfVar79[5] = fVar11 + fVar42;
        pfVar79[6] = fVar12 + fVar43;
        pfVar79[7] = fVar13 + fVar44;
        pfVar79 = (float *)((longlong)param_2 + lVar74 + 0x40);
        *pfVar79 = fVar14 + fVar45;
        pfVar79[1] = fVar15 + fVar46;
        pfVar79[2] = fVar16 + fVar47;
        pfVar79[3] = fVar17 + fVar48;
        pfVar79[4] = fVar18 + fVar49;
        pfVar79[5] = fVar19 + fVar50;
        pfVar79[6] = fVar20 + fVar51;
        pfVar79[7] = fVar21 + fVar52;
        pfVar79 = (float *)((longlong)param_2 + lVar74 + 0x60);
        *pfVar79 = fVar22 + fVar53;
        pfVar79[1] = fVar23 + fVar54;
        pfVar79[2] = fVar24 + fVar55;
        pfVar79[3] = fVar25 + fVar56;
        pfVar79[4] = fVar26 + fVar57;
        pfVar79[5] = fVar27 + fVar58;
        pfVar79[6] = fVar28 + fVar59;
        pfVar79[7] = fVar29 + fVar60;
        lVar74 = lVar74 + 0x80;
      } while ((ulonglong)(uVar8 >> 5 & 0x3ffffff) << 7 != lVar74);
      if (uVar68 == uVar66) goto LAB_18000a004;
    }
    uVar70 = uVar68;
    for (uVar73 = uVar66 & 3; uVar73 != 0; uVar73 = uVar73 - 1) {
      *(float *)((longlong)param_2 + uVar70 * 4) =
           *(float *)((longlong)pvVar65 + uVar70 * 4) + *(float *)((longlong)param_2 + uVar70 * 4);
      uVar70 = uVar70 + 1;
    }
    if (uVar68 - uVar66 < 0xfffffffffffffffd) {
      do {
        *(float *)((longlong)param_2 + uVar70 * 4) =
             *(float *)((longlong)pvVar65 + uVar70 * 4) + *(float *)((longlong)param_2 + uVar70 * 4)
        ;
        *(float *)((longlong)param_2 + uVar70 * 4 + 4) =
             *(float *)((longlong)pvVar65 + uVar70 * 4 + 4) +
             *(float *)((longlong)param_2 + uVar70 * 4 + 4);
        *(float *)((longlong)param_2 + uVar70 * 4 + 8) =
             *(float *)((longlong)pvVar65 + uVar70 * 4 + 8) +
             *(float *)((longlong)param_2 + uVar70 * 4 + 8);
        *(float *)((longlong)param_2 + uVar70 * 4 + 0xc) =
             *(float *)((longlong)pvVar65 + uVar70 * 4 + 0xc) +
             *(float *)((longlong)param_2 + uVar70 * 4 + 0xc);
        uVar70 = uVar70 + 4;
      } while (uVar66 != uVar70);
    }
  }
LAB_18000a004:
  if (0 < (int)uVar62 && param_1[5] != 0) {
    lVar74 = uVar62 * 2;
    uVar68 = 0;
    uVar66 = uVar62;
    do {
      *(float *)((longlong)param_2 + uVar68 * 4) =
           *(float *)(param_1[5] + uVar68 * 4) * *(float *)(param_3 + uVar68 * 4) +
           *(float *)((longlong)param_2 + uVar68 * 4);
      *(float *)((longlong)param_2 + uVar66 * 4) =
           *(float *)(param_1[5] + uVar66 * 4) * *(float *)(param_3 + uVar68 * 4) +
           *(float *)((longlong)param_2 + uVar66 * 4);
      *(float *)((longlong)param_2 + lVar74 * 4) =
           *(float *)(param_1[5] + lVar74 * 4) * *(float *)(param_3 + uVar68 * 4) +
           *(float *)((longlong)param_2 + lVar74 * 4);
      uVar68 = uVar68 + 1;
      lVar74 = lVar74 + 1;
      uVar66 = uVar66 + 1;
    } while (uVar62 != uVar68);
  }
  if ((local_48 ^ (ulonglong)auStack_888) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000db80();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a0d0
   NAME : rnn_compute_conv2d_c
   SIG  : undefined rnn_compute_conv2d_c(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Type propagation algorithm not settling */

void rnn_compute_conv2d_c
               (longlong *param_1,void *param_2,void *param_3,void *param_4,uint param_5,int param_6
               ,undefined4 param_7)

{
  float *pfVar1;
  float *pfVar2;
  float *pfVar3;
  float *pfVar4;
  float *pfVar5;
  float *pfVar6;
  float *pfVar7;
  float *pfVar8;
  float *pfVar9;
  float *pfVar10;
  float fVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  uint uVar20;
  longlong *plVar21;
  uint uVar22;
  int iVar23;
  int iVar24;
  int iVar25;
  int iVar26;
  longlong lVar27;
  ulonglong uVar28;
  uint uVar29;
  longlong lVar30;
  longlong lVar31;
  longlong lVar32;
  ulonglong uVar33;
  longlong lVar34;
  size_t sVar35;
  ulonglong uVar36;
  uint uVar37;
  ulonglong uVar38;
  longlong lVar39;
  void *pvVar40;
  longlong lVar41;
  ulonglong uVar42;
  int iVar43;
  int iVar44;
  void *pvVar45;
  ulonglong uVar46;
  longlong lVar47;
  bool bVar48;
  undefined1 auVar49 [32];
  undefined1 auVar50 [32];
  undefined1 auVar51 [32];
  undefined1 auVar52 [32];
  undefined1 auVar53 [32];
  undefined1 auVar54 [32];
  undefined1 auVar55 [32];
  undefined1 auVar56 [32];
  undefined1 in_ZMM6 [64];
  undefined1 in_ZMM7 [64];
  undefined1 in_ZMM8 [64];
  undefined1 auVar57 [32];
  undefined1 in_ZMM9 [64];
  undefined1 auStack_81d8 [32];
  void *local_81b8;
  ulonglong local_81b0;
  ulonglong local_81a8;
  longlong local_81a0;
  ulonglong local_8198;
  ulonglong local_8190;
  void *local_8188;
  longlong local_8180;
  longlong local_8178;
  size_t local_8170;
  longlong *local_8168;
  void *local_8160;
  ulonglong local_8158;
  void *local_8150;
  ulonglong local_8148;
  ulonglong local_8140;
  ulonglong local_8138;
  longlong local_8130;
  void *local_8128;
  void *local_8120;
  void *local_8118;
  int local_810c;
  ulonglong local_8108;
  void *local_8100;
  ulonglong local_80f8;
  longlong local_80f0;
  size_t local_80e8;
  longlong local_80e0;
  ulonglong local_80d8;
  ulonglong local_80d0;
  ulonglong local_80c8;
  longlong local_80c0;
  ulonglong local_80b8;
  longlong local_80b0;
  ulonglong local_80a8;
  void *local_80a0;
  float local_8098;
  float local_8094;
  float afStack_8090 [8192];
  ulonglong local_90;
  undefined1 local_88 [16];
  undefined1 local_78 [16];
  undefined1 local_68 [16];
  undefined1 local_58 [16];
  undefined8 uStack_48;
  
                    /* 0xa0d0  6  rnn_compute_conv2d_c */
  uStack_48 = 0x18000a0e6;
  local_58 = in_ZMM9._0_16_;
  local_68 = in_ZMM8._0_16_;
  local_78 = in_ZMM7._0_16_;
  local_88 = in_ZMM6._0_16_;
  local_90 = DAT_180580000 ^ (ulonglong)auStack_81d8;
  lVar31 = (longlong)(int)(*(int *)((longlong)param_1 + 0x1c) + (param_5 - 1)) *
           (longlong)(int)param_1[2];
  lVar32 = (longlong)(int)lVar31 * ((longlong)(int)param_1[3] + -1);
  sVar35 = lVar32 * 4;
  local_81b8 = param_2;
  memcpy(&local_8098,param_3,sVar35);
  memcpy(&local_8098 + lVar32,param_4,lVar31 * 4);
  memcpy(param_3,&local_8098 + lVar31,sVar35);
  local_80e0 = *param_1;
  local_81a0 = param_1[1];
  uVar37 = *(uint *)((longlong)param_1 + 0x1c);
  local_81b0 = (ulonglong)uVar37;
  uVar22 = *(uint *)(param_1 + 3);
  uVar29 = *(uint *)((longlong)param_1 + 0x14);
  local_8198 = (ulonglong)uVar29;
  uVar20 = *(uint *)(param_1 + 2);
  local_81a8 = (ulonglong)uVar20;
  local_8168 = param_1;
  if (uVar22 == 3 && uVar37 == 3) {
    if (0 < (int)uVar29) {
      sVar35 = (longlong)(int)param_5 << 2;
      if ((int)uVar20 < 1) {
        local_81b0 = (ulonglong)param_6;
        uVar33 = (ulonglong)(uVar29 & 7);
        if (uVar29 < 8) {
          uVar28 = 0;
        }
        else {
          lVar32 = local_81b0 * 0x20;
          lVar31 = local_81b0 * 4;
          uVar28 = 0;
          pvVar45 = local_81b8;
          do {
            memset(pvVar45,0,sVar35);
            memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
            pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
            uVar28 = uVar28 + 8;
            pvVar45 = (void *)((longlong)pvVar45 + lVar32);
          } while ((uVar29 & 0x7ffffff8) != uVar28);
        }
        if (uVar33 != 0) {
          pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_81b0 * 4);
          lVar31 = local_81b0 * 4;
          do {
            memset(pvVar45,0,sVar35);
            pvVar45 = (void *)((longlong)pvVar45 + lVar31);
            uVar33 = uVar33 - 1;
          } while (uVar33 != 0);
        }
      }
      else if ((int)param_5 < 1) {
        local_81b0 = (ulonglong)param_6;
        uVar33 = (ulonglong)(uVar29 & 7);
        if (uVar29 < 8) {
          uVar28 = 0;
        }
        else {
          lVar32 = local_81b0 * 0x20;
          lVar31 = local_81b0 * 4;
          uVar28 = 0;
          pvVar45 = local_81b8;
          do {
            memset(pvVar45,0,sVar35);
            memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
            pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
            uVar28 = uVar28 + 8;
            pvVar45 = (void *)((longlong)pvVar45 + lVar32);
          } while ((uVar29 & 0x7ffffff8) != uVar28);
        }
        if (uVar33 != 0) {
          pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_81b0 * 4);
          lVar31 = local_81b0 * 4;
          do {
            memset(pvVar45,0,sVar35);
            pvVar45 = (void *)((longlong)pvVar45 + lVar31);
            uVar33 = uVar33 - 1;
          } while (uVar33 != 0);
        }
      }
      else {
        uVar37 = param_5 + 2;
        local_8190 = (ulonglong)param_6;
        local_8178 = local_8190 * 4;
        local_8148 = (ulonglong)(uVar20 * 9);
        local_8130 = local_81a0 + 0x24;
        local_8150 = (void *)((longlong)local_81b8 + (ulonglong)param_5 * 4);
        local_8138 = (ulonglong)(param_5 & 0x7ffffff8);
        local_8118 = (void *)CONCAT44(local_8118._4_4_,uVar37);
        local_8158 = (ulonglong)(uVar20 * uVar37);
        local_8160 = (void *)CONCAT44(local_8160._4_4_,uVar20 * uVar37 * 2);
        uVar33 = 0;
        pvVar45 = local_81b8;
        local_8170 = sVar35;
        do {
          uVar28 = uVar33;
          local_8120 = (void *)((longlong)local_81b8 + local_8178 * uVar28);
          local_8188 = (void *)(local_8178 * uVar28 + (longlong)local_8150);
          local_8128 = (void *)(uVar28 * local_8148);
          memset((void *)((longlong)local_81b8 + uVar28 * local_8190 * 4),0,local_8170);
          uVar22 = (int)local_81a8 * (int)uVar28;
          local_81b0 = (ulonglong)uVar22;
          lVar31 = (longlong)&local_8094;
          uVar33 = local_8158 & 0xffffffff;
          lVar32 = (longlong)afStack_8090;
          uVar38 = 0;
          iVar26 = (int)local_8160;
          do {
            lVar39 = (longlong)iVar26;
            lVar34 = lVar39 * 4;
            lVar27 = (longlong)(int)uVar33;
            lVar47 = lVar27 * 4;
            lVar30 = (longlong)(int)((uVar22 + (int)uVar38) * 9);
            if (param_5 < 8) {
              uVar42 = 0;
LAB_18000a4f0:
              do {
                *(float *)((longlong)pvVar45 + uVar42 * 4) =
                     *(float *)(local_81a0 + 0x20 + lVar30 * 4) * afStack_8090[lVar39 + uVar42] +
                     *(float *)(local_81a0 + 0x1c + lVar30 * 4) *
                     afStack_8090[lVar39 + uVar42 + 0xffffffffffffffff] +
                     *(float *)(local_81a0 + 0x18 + lVar30 * 4) * (&local_8098)[lVar39 + uVar42] +
                     *(float *)(local_81a0 + 0x14 + lVar30 * 4) * afStack_8090[lVar27 + uVar42] +
                     *(float *)(local_81a0 + 0x10 + lVar30 * 4) *
                     afStack_8090[lVar27 + uVar42 + 0xffffffffffffffff] +
                     *(float *)(local_81a0 + 0xc + lVar30 * 4) * (&local_8098)[lVar27 + uVar42] +
                     *(float *)(local_81a0 + 8 + lVar30 * 4) * *(float *)(lVar31 + (uVar42 + 1) * 4)
                     + *(float *)(local_81a0 + lVar30 * 4) * *(float *)(lVar31 + (uVar42 - 1) * 4) +
                       *(float *)(local_81a0 + 4 + lVar30 * 4) * *(float *)(lVar31 + uVar42 * 4) +
                     *(float *)((longlong)pvVar45 + uVar42 * 4);
                uVar42 = uVar42 + 1;
              } while (param_5 != uVar42);
            }
            else {
              lVar41 = (longlong)((int)uVar38 * 9 + (int)local_8128);
              if (local_8120 < (void *)(local_8130 + lVar41 * 4) &&
                  (void *)(local_81a0 + lVar41 * 4) < local_8188) {
                uVar42 = 0;
                goto LAB_18000a4f0;
              }
              fVar19 = *(float *)(local_81a0 + lVar30 * 4);
              fVar11 = *(float *)(local_81a0 + 4 + lVar30 * 4);
              fVar12 = *(float *)(local_81a0 + 8 + lVar30 * 4);
              fVar13 = *(float *)(local_81a0 + 0xc + lVar30 * 4);
              fVar14 = *(float *)(local_81a0 + 0x10 + lVar30 * 4);
              fVar15 = *(float *)(local_81a0 + 0x14 + lVar30 * 4);
              fVar16 = *(float *)(local_81a0 + 0x18 + lVar30 * 4);
              fVar17 = *(float *)(local_81a0 + 0x1c + lVar30 * 4);
              fVar18 = *(float *)(local_81a0 + 0x20 + lVar30 * 4);
              lVar41 = 0;
              do {
                pfVar4 = (float *)(lVar32 + lVar41 + -4);
                pfVar5 = (float *)(lVar32 + lVar41 + -8);
                pfVar10 = (float *)(lVar32 + lVar41);
                pfVar6 = (float *)((longlong)&local_8098 + lVar41 + lVar47);
                pfVar7 = (float *)((longlong)afStack_8090 + lVar41 + lVar47 + 0xfffffffffffffffcU);
                pfVar1 = (float *)((longlong)afStack_8090 + lVar41 + lVar47);
                pfVar8 = (float *)((longlong)&local_8098 + lVar41 + lVar34);
                pfVar9 = (float *)((longlong)afStack_8090 + lVar41 + lVar34 + 0xfffffffffffffffcU);
                pfVar2 = (float *)((longlong)afStack_8090 + lVar41 + lVar34);
                pfVar3 = (float *)((longlong)pvVar45 + lVar41);
                auVar57._0_4_ =
                     fVar18 * *pfVar2 +
                     fVar17 * *pfVar9 +
                     fVar16 * *pfVar8 +
                     fVar15 * *pfVar1 +
                     fVar14 * *pfVar7 +
                     fVar13 * *pfVar6 + fVar12 * *pfVar10 + fVar19 * *pfVar5 + fVar11 * *pfVar4 +
                     *pfVar3;
                auVar57._4_4_ =
                     fVar18 * pfVar2[1] +
                     fVar17 * pfVar9[1] +
                     fVar16 * pfVar8[1] +
                     fVar15 * pfVar1[1] +
                     fVar14 * pfVar7[1] +
                     fVar13 * pfVar6[1] +
                     fVar12 * pfVar10[1] + fVar19 * pfVar5[1] + fVar11 * pfVar4[1] + pfVar3[1];
                auVar57._8_4_ =
                     fVar18 * pfVar2[2] +
                     fVar17 * pfVar9[2] +
                     fVar16 * pfVar8[2] +
                     fVar15 * pfVar1[2] +
                     fVar14 * pfVar7[2] +
                     fVar13 * pfVar6[2] +
                     fVar12 * pfVar10[2] + fVar19 * pfVar5[2] + fVar11 * pfVar4[2] + pfVar3[2];
                auVar57._12_4_ =
                     fVar18 * pfVar2[3] +
                     fVar17 * pfVar9[3] +
                     fVar16 * pfVar8[3] +
                     fVar15 * pfVar1[3] +
                     fVar14 * pfVar7[3] +
                     fVar13 * pfVar6[3] +
                     fVar12 * pfVar10[3] + fVar19 * pfVar5[3] + fVar11 * pfVar4[3] + pfVar3[3];
                auVar57._16_4_ =
                     fVar18 * pfVar2[4] +
                     fVar17 * pfVar9[4] +
                     fVar16 * pfVar8[4] +
                     fVar15 * pfVar1[4] +
                     fVar14 * pfVar7[4] +
                     fVar13 * pfVar6[4] +
                     fVar12 * pfVar10[4] + fVar19 * pfVar5[4] + fVar11 * pfVar4[4] + pfVar3[4];
                auVar57._20_4_ =
                     fVar18 * pfVar2[5] +
                     fVar17 * pfVar9[5] +
                     fVar16 * pfVar8[5] +
                     fVar15 * pfVar1[5] +
                     fVar14 * pfVar7[5] +
                     fVar13 * pfVar6[5] +
                     fVar12 * pfVar10[5] + fVar19 * pfVar5[5] + fVar11 * pfVar4[5] + pfVar3[5];
                auVar57._24_4_ =
                     fVar18 * pfVar2[6] +
                     fVar17 * pfVar9[6] +
                     fVar16 * pfVar8[6] +
                     fVar15 * pfVar1[6] +
                     fVar14 * pfVar7[6] +
                     fVar13 * pfVar6[6] +
                     fVar12 * pfVar10[6] + fVar19 * pfVar5[6] + fVar11 * pfVar4[6] + pfVar3[6];
                auVar57._28_4_ =
                     fVar18 * pfVar2[7] +
                     fVar17 * pfVar9[7] +
                     fVar16 * pfVar8[7] +
                     fVar15 * pfVar1[7] +
                     fVar14 * pfVar7[7] +
                     fVar13 * pfVar6[7] +
                     fVar12 * pfVar10[7] + fVar19 * pfVar5[7] + fVar11 * pfVar4[7] + pfVar3[7];
                *(undefined1 (*) [32])((longlong)pvVar45 + lVar41) = auVar57;
                lVar41 = lVar41 + 0x20;
              } while ((ulonglong)(param_5 >> 3 & 0xfffffff) << 5 != lVar41);
              uVar42 = local_8138;
              if ((uint)local_8138 != param_5) goto LAB_18000a4f0;
            }
            uVar38 = uVar38 + 1;
            lVar32 = lVar32 + (ulonglong)uVar37 * 4;
            uVar33 = (ulonglong)(uint)((int)uVar33 + (int)local_8118);
            iVar26 = iVar26 + (int)local_8118;
            lVar31 = lVar31 + (ulonglong)uVar37 * 4;
          } while (uVar38 != local_81a8);
          pvVar45 = (void *)((longlong)pvVar45 + local_8178);
          uVar33 = uVar28 + 1;
          local_8140 = uVar28;
        } while (uVar28 + 1 != local_8198);
      }
    }
  }
  else if (0 < (int)uVar29) {
    sVar35 = (longlong)(int)param_5 << 2;
    if ((int)uVar20 < 1) {
      local_81b0 = (ulonglong)param_6;
      uVar33 = (ulonglong)(uVar29 & 7);
      if (uVar29 < 8) {
        uVar28 = 0;
      }
      else {
        lVar32 = local_81b0 * 0x20;
        lVar31 = local_81b0 * 4;
        uVar28 = 0;
        pvVar45 = local_81b8;
        do {
          memset(pvVar45,0,sVar35);
          memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
          pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
          memset(pvVar40,0,sVar35);
          pvVar40 = (void *)((longlong)pvVar40 + lVar31);
          memset(pvVar40,0,sVar35);
          pvVar40 = (void *)((longlong)pvVar40 + lVar31);
          memset(pvVar40,0,sVar35);
          pvVar40 = (void *)((longlong)pvVar40 + lVar31);
          memset(pvVar40,0,sVar35);
          pvVar40 = (void *)((longlong)pvVar40 + lVar31);
          memset(pvVar40,0,sVar35);
          memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
          uVar28 = uVar28 + 8;
          pvVar45 = (void *)((longlong)pvVar45 + lVar32);
        } while ((uVar29 & 0x7ffffff8) != uVar28);
      }
      if (uVar33 != 0) {
        pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_81b0 * 4);
        lVar31 = local_81b0 * 4;
        do {
          memset(pvVar45,0,sVar35);
          pvVar45 = (void *)((longlong)pvVar45 + lVar31);
          uVar33 = uVar33 - 1;
        } while (uVar33 != 0);
      }
    }
    else {
      local_8190 = (ulonglong)uVar22;
      local_8180 = (longlong)param_6;
      if ((int)uVar22 < 1) {
        uVar33 = (ulonglong)(uVar29 & 7);
        if (uVar29 < 8) {
          uVar28 = 0;
        }
        else {
          lVar32 = local_8180 * 0x20;
          lVar31 = local_8180 * 4;
          uVar28 = 0;
          pvVar45 = local_81b8;
          do {
            memset(pvVar45,0,sVar35);
            memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
            pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
            uVar28 = uVar28 + 8;
            pvVar45 = (void *)((longlong)pvVar45 + lVar32);
          } while ((uVar29 & 0x7ffffff8) != uVar28);
        }
        if (uVar33 != 0) {
          pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_8180 * 4);
          lVar31 = local_8180 * 4;
          do {
            memset(pvVar45,0,sVar35);
            pvVar45 = (void *)((longlong)pvVar45 + lVar31);
            uVar33 = uVar33 - 1;
          } while (uVar33 != 0);
        }
      }
      else if ((int)uVar37 < 1) {
        uVar33 = (ulonglong)(uVar29 & 7);
        if (uVar29 < 8) {
          uVar28 = 0;
        }
        else {
          lVar32 = local_8180 * 0x20;
          lVar31 = local_8180 * 4;
          uVar28 = 0;
          pvVar45 = local_81b8;
          do {
            memset(pvVar45,0,sVar35);
            memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
            pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
            uVar28 = uVar28 + 8;
            pvVar45 = (void *)((longlong)pvVar45 + lVar32);
          } while ((uVar29 & 0x7ffffff8) != uVar28);
        }
        if (uVar33 != 0) {
          pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_8180 * 4);
          lVar31 = local_8180 * 4;
          do {
            memset(pvVar45,0,sVar35);
            pvVar45 = (void *)((longlong)pvVar45 + lVar31);
            uVar33 = uVar33 - 1;
          } while (uVar33 != 0);
        }
      }
      else if ((int)param_5 < 1) {
        uVar33 = (ulonglong)(uVar29 & 7);
        if (uVar29 < 8) {
          uVar28 = 0;
        }
        else {
          lVar32 = local_8180 * 0x20;
          lVar31 = local_8180 * 4;
          uVar28 = 0;
          pvVar45 = local_81b8;
          do {
            memset(pvVar45,0,sVar35);
            memset((void *)((longlong)pvVar45 + lVar31),0,sVar35);
            pvVar40 = (void *)((longlong)pvVar45 + lVar31 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            pvVar40 = (void *)((longlong)pvVar40 + lVar31);
            memset(pvVar40,0,sVar35);
            memset((void *)((longlong)pvVar40 + lVar31),0,sVar35);
            uVar28 = uVar28 + 8;
            pvVar45 = (void *)((longlong)pvVar45 + lVar32);
          } while ((uVar29 & 0x7ffffff8) != uVar28);
        }
        if (uVar33 != 0) {
          pvVar45 = (void *)((longlong)local_81b8 + uVar28 * local_8180 * 4);
          lVar31 = local_8180 * 4;
          do {
            memset(pvVar45,0,sVar35);
            pvVar45 = (void *)((longlong)pvVar45 + lVar31);
            uVar33 = uVar33 - 1;
          } while (uVar33 != 0);
        }
      }
      else {
        uVar29 = (param_5 - 1) + uVar37;
        uVar28 = (ulonglong)param_5;
        uVar33 = uVar28 - 1;
        local_80d0 = (ulonglong)uVar29;
        local_8178 = CONCAT44(local_8178._4_4_,uVar29);
        local_80a8 = (ulonglong)(uVar20 * uVar29);
        local_80f0 = local_8180 * 4;
        local_80d8 = (ulonglong)(uVar22 * uVar37);
        local_80f8 = (ulonglong)(uVar22 * uVar37 * uVar20);
        local_80b0 = local_81a0 + local_81b0 * 4;
        local_8100 = (void *)((longlong)local_81b8 + uVar28 * 4);
        uVar37 = param_5 & 0x7fffffe0;
        lVar31 = (longlong)local_81b8 + 0x60;
        local_8120 = (void *)(uVar33 >> 0x20);
        uVar38 = 0;
        pvVar45 = local_81b8;
        local_80e8 = sVar35;
        do {
          uVar36 = local_80a8;
          uVar42 = local_80d0;
          local_80a0 = (void *)((longlong)local_81b8 + local_80f0 * uVar38);
          local_8160 = (void *)(local_80f0 * uVar38 + (longlong)local_8100);
          local_80c0 = uVar38 * local_80f8;
          local_8128 = (void *)((longlong)local_81b8 + uVar38 * local_8180 * 4);
          local_8108 = uVar38;
          memset(local_8128,0,local_80e8);
          iVar23 = (int)local_81a8;
          local_80c8 = (ulonglong)(uint)(iVar23 * (int)local_8108);
          sVar35 = 0;
          uVar38 = 0;
          iVar26 = 1;
          do {
            local_810c = iVar26;
            local_80b8 = uVar38;
            local_8170 = sVar35;
            local_8148 = local_8170 * local_80d8 + local_80c0;
            local_8150 = (void *)(local_8170 * uVar42);
            uVar22 = (iVar23 * (int)local_8108 + (int)local_8170) * (int)local_8190;
            local_8158 = (ulonglong)uVar22;
            uVar38 = 0;
            uVar42 = local_80b8;
            iVar26 = local_810c;
            do {
              local_8138 = uVar42;
              local_8140 = uVar38;
              iVar24 = (int)local_8140;
              lVar34 = (longlong)(iVar24 * (int)local_81b0 + (int)local_8148);
              pvVar40 = (void *)(local_81a0 + lVar34 * 4);
              local_8118 = (void *)(local_8140 * uVar36 + (longlong)local_8150);
              lVar32 = local_81a0 + (longlong)(int)((uVar22 + iVar24) * (int)local_81b0) * 4;
              bVar48 = local_80a0 < (void *)(local_80b0 + lVar34 * 4);
              local_8188 = (void *)CONCAT71((int7)((ulonglong)pvVar40 >> 8),
                                            pvVar40 < local_8160 && bVar48);
              local_8130 = CONCAT44(local_8130._4_4_,iVar26);
              uVar42 = 0;
              uVar38 = local_8138;
              iVar25 = iVar26;
              do {
                if ((param_5 < 0x20) ||
                   (iVar43 = (int)local_8118 + (int)uVar42,
                   (local_8120 != (void *)0x0 || pvVar40 < local_8160 && bVar48) ||
                   iVar43 + (int)uVar33 < iVar43)) {
                  uVar36 = 0;
LAB_18000a928:
                  uVar46 = uVar36;
                  if ((param_5 & 1) != 0) {
                    *(float *)((longlong)local_8128 + uVar36 * 4) =
                         *(float *)(lVar32 + uVar42 * 4) *
                         (&local_8098)
                         [(iVar23 * iVar24 + (int)local_8170) * (int)local_8178 + (int)uVar42 +
                          (int)uVar36] + *(float *)((longlong)local_8128 + uVar36 * 4);
                    uVar46 = uVar36 | 1;
                  }
                  if (uVar36 != uVar33) {
                    lVar34 = uVar46 + 1;
                    iVar43 = (int)uVar38 + (int)uVar46;
                    iVar44 = (int)uVar46 + iVar25;
                    do {
                      *(float *)((longlong)pvVar45 + lVar34 * 4 + -4) =
                           *(float *)(lVar32 + uVar42 * 4) * (&local_8098)[iVar43] +
                           *(float *)((longlong)pvVar45 + lVar34 * 4 + -4);
                      *(float *)((longlong)pvVar45 + lVar34 * 4) =
                           *(float *)(lVar32 + uVar42 * 4) * (&local_8098)[iVar44] +
                           *(float *)((longlong)pvVar45 + lVar34 * 4);
                      lVar47 = lVar34 - uVar28;
                      lVar34 = lVar34 + 2;
                      iVar43 = iVar43 + 2;
                      iVar44 = iVar44 + 2;
                    } while (lVar47 != -1);
                  }
                }
                else {
                  fVar19 = *(float *)(lVar32 + uVar42 * 4);
                  uVar36 = 0;
                  do {
                    lVar34 = (longlong)((int)uVar38 + (int)uVar36);
                    pfVar10 = (float *)(lVar31 + -0x60 + uVar36 * 4);
                    auVar49._0_4_ = fVar19 * (&local_8098)[lVar34] + *pfVar10;
                    auVar49._4_4_ = fVar19 * afStack_8090[lVar34 + 0xffffffffffffffff] + pfVar10[1];
                    auVar49._8_4_ = fVar19 * afStack_8090[lVar34] + pfVar10[2];
                    auVar49._12_4_ = fVar19 * afStack_8090[lVar34 + 1] + pfVar10[3];
                    auVar49._16_4_ = fVar19 * afStack_8090[lVar34 + 2] + pfVar10[4];
                    auVar49._20_4_ = fVar19 * afStack_8090[lVar34 + 3] + pfVar10[5];
                    auVar49._24_4_ = fVar19 * afStack_8090[lVar34 + 4] + pfVar10[6];
                    auVar49._28_4_ = fVar19 * afStack_8090[lVar34 + 5] + pfVar10[7];
                    pfVar10 = (float *)(lVar31 + -0x40 + uVar36 * 4);
                    auVar51._0_4_ = fVar19 * afStack_8090[lVar34 + 6] + *pfVar10;
                    auVar51._4_4_ = fVar19 * afStack_8090[lVar34 + 7] + pfVar10[1];
                    auVar51._8_4_ = fVar19 * afStack_8090[lVar34 + 8] + pfVar10[2];
                    auVar51._12_4_ = fVar19 * afStack_8090[lVar34 + 9] + pfVar10[3];
                    auVar51._16_4_ = fVar19 * afStack_8090[lVar34 + 10] + pfVar10[4];
                    auVar51._20_4_ = fVar19 * afStack_8090[lVar34 + 0xb] + pfVar10[5];
                    auVar51._24_4_ = fVar19 * afStack_8090[lVar34 + 0xc] + pfVar10[6];
                    auVar51._28_4_ = fVar19 * afStack_8090[lVar34 + 0xd] + pfVar10[7];
                    pfVar10 = (float *)(lVar31 + -0x20 + uVar36 * 4);
                    auVar53._0_4_ = fVar19 * afStack_8090[lVar34 + 0xe] + *pfVar10;
                    auVar53._4_4_ = fVar19 * afStack_8090[lVar34 + 0xf] + pfVar10[1];
                    auVar53._8_4_ = fVar19 * afStack_8090[lVar34 + 0x10] + pfVar10[2];
                    auVar53._12_4_ = fVar19 * afStack_8090[lVar34 + 0x11] + pfVar10[3];
                    auVar53._16_4_ = fVar19 * afStack_8090[lVar34 + 0x12] + pfVar10[4];
                    auVar53._20_4_ = fVar19 * afStack_8090[lVar34 + 0x13] + pfVar10[5];
                    auVar53._24_4_ = fVar19 * afStack_8090[lVar34 + 0x14] + pfVar10[6];
                    auVar53._28_4_ = fVar19 * afStack_8090[lVar34 + 0x15] + pfVar10[7];
                    pfVar10 = (float *)(lVar31 + uVar36 * 4);
                    auVar55._0_4_ = fVar19 * afStack_8090[lVar34 + 0x16] + *pfVar10;
                    auVar55._4_4_ = fVar19 * afStack_8090[lVar34 + 0x17] + pfVar10[1];
                    auVar55._8_4_ = fVar19 * afStack_8090[lVar34 + 0x18] + pfVar10[2];
                    auVar55._12_4_ = fVar19 * afStack_8090[lVar34 + 0x19] + pfVar10[3];
                    auVar55._16_4_ = fVar19 * afStack_8090[lVar34 + 0x1a] + pfVar10[4];
                    auVar55._20_4_ = fVar19 * afStack_8090[lVar34 + 0x1b] + pfVar10[5];
                    auVar55._24_4_ = fVar19 * afStack_8090[lVar34 + 0x1c] + pfVar10[6];
                    auVar55._28_4_ = fVar19 * afStack_8090[lVar34 + 0x1d] + pfVar10[7];
                    *(undefined1 (*) [32])(lVar31 + -0x60 + uVar36 * 4) = auVar49;
                    *(undefined1 (*) [32])(lVar31 + -0x40 + uVar36 * 4) = auVar51;
                    *(undefined1 (*) [32])(lVar31 + -0x20 + uVar36 * 4) = auVar53;
                    *(undefined1 (*) [32])(lVar31 + uVar36 * 4) = auVar55;
                    uVar36 = uVar36 + 0x20;
                  } while (uVar37 != uVar36);
                  uVar36 = (ulonglong)uVar37;
                  if (uVar37 != param_5) goto LAB_18000a928;
                }
                uVar42 = uVar42 + 1;
                uVar38 = uVar38 + 1;
                iVar25 = iVar25 + 1;
              } while (uVar42 != local_81b0);
              iVar26 = iVar26 + (int)local_80a8;
              uVar38 = local_8140 + 1;
              uVar36 = local_80a8;
              uVar42 = local_8138 + local_80a8;
            } while (local_8140 + 1 != local_8190);
            sVar35 = local_8170 + 1;
            uVar42 = local_80d0;
            uVar38 = local_80b8 + local_80d0;
            iVar26 = local_810c + (int)local_8178;
          } while ((int)(local_8170 + 1) != iVar23);
          uVar38 = local_8108 + 1;
          lVar31 = lVar31 + local_80f0;
          pvVar45 = (void *)((longlong)pvVar45 + local_80f0);
        } while (uVar38 != local_8198);
      }
    }
  }
  plVar21 = local_8168;
  iVar26 = *(int *)((longlong)local_8168 + 0x14);
  if (local_80e0 == 0) {
LAB_18000b2dc:
    if (iVar26 < 1) goto LAB_18000b338;
  }
  else {
    uVar33 = (ulonglong)param_5;
    if (iVar26 < 1) goto LAB_18000b338;
    if (0 < (int)param_5) {
      lVar32 = (longlong)param_6 * 4;
      uVar37 = param_5 & 0x7fffffe0;
      uVar28 = (ulonglong)(param_5 & 3);
      lVar31 = (longlong)local_81b8 + 0x60;
      lVar34 = 0;
      pvVar45 = local_81b8;
      do {
        uVar38 = uVar28;
        if (param_5 < 0x20) {
          uVar42 = 0;
          uVar36 = 0;
joined_r0x00018000b246:
          for (; uVar38 != 0; uVar38 = uVar38 - 1) {
            *(float *)((longlong)pvVar45 + uVar42 * 4) =
                 *(float *)(local_80e0 + lVar34 * 4) + *(float *)((longlong)pvVar45 + uVar42 * 4);
            uVar42 = uVar42 + 1;
          }
          if (uVar36 - uVar33 < 0xfffffffffffffffd) {
            do {
              *(float *)((longlong)pvVar45 + uVar42 * 4) =
                   *(float *)(local_80e0 + lVar34 * 4) + *(float *)((longlong)pvVar45 + uVar42 * 4);
              *(float *)((longlong)pvVar45 + uVar42 * 4 + 4) =
                   *(float *)(local_80e0 + lVar34 * 4) +
                   *(float *)((longlong)pvVar45 + uVar42 * 4 + 4);
              *(float *)((longlong)pvVar45 + uVar42 * 4 + 8) =
                   *(float *)(local_80e0 + lVar34 * 4) +
                   *(float *)((longlong)pvVar45 + uVar42 * 4 + 8);
              *(float *)((longlong)pvVar45 + uVar42 * 4 + 0xc) =
                   *(float *)(local_80e0 + lVar34 * 4) +
                   *(float *)((longlong)pvVar45 + uVar42 * 4 + 0xc);
              uVar42 = uVar42 + 4;
            } while (uVar33 != uVar42);
          }
        }
        else {
          if ((void *)(lVar32 * lVar34 + (longlong)local_81b8) <
              (void *)(local_80e0 + 4 + lVar34 * 4) &&
              (ulonglong)(lVar34 * 4 + local_80e0) <
              (longlong)local_81b8 + lVar32 * lVar34 + uVar33 * 4) {
            uVar42 = 0;
            uVar36 = 0;
            goto joined_r0x00018000b246;
          }
          fVar19 = *(float *)(local_80e0 + lVar34 * 4);
          lVar47 = 0;
          do {
            pfVar10 = (float *)(lVar31 + -0x60 + lVar47);
            auVar50._0_4_ = fVar19 + *pfVar10;
            auVar50._4_4_ = fVar19 + pfVar10[1];
            auVar50._8_4_ = fVar19 + pfVar10[2];
            auVar50._12_4_ = fVar19 + pfVar10[3];
            auVar50._16_4_ = fVar19 + pfVar10[4];
            auVar50._20_4_ = fVar19 + pfVar10[5];
            auVar50._24_4_ = fVar19 + pfVar10[6];
            auVar50._28_4_ = fVar19 + pfVar10[7];
            pfVar10 = (float *)(lVar31 + -0x40 + lVar47);
            auVar52._0_4_ = fVar19 + *pfVar10;
            auVar52._4_4_ = fVar19 + pfVar10[1];
            auVar52._8_4_ = fVar19 + pfVar10[2];
            auVar52._12_4_ = fVar19 + pfVar10[3];
            auVar52._16_4_ = fVar19 + pfVar10[4];
            auVar52._20_4_ = fVar19 + pfVar10[5];
            auVar52._24_4_ = fVar19 + pfVar10[6];
            auVar52._28_4_ = fVar19 + pfVar10[7];
            pfVar10 = (float *)(lVar31 + -0x20 + lVar47);
            auVar54._0_4_ = fVar19 + *pfVar10;
            auVar54._4_4_ = fVar19 + pfVar10[1];
            auVar54._8_4_ = fVar19 + pfVar10[2];
            auVar54._12_4_ = fVar19 + pfVar10[3];
            auVar54._16_4_ = fVar19 + pfVar10[4];
            auVar54._20_4_ = fVar19 + pfVar10[5];
            auVar54._24_4_ = fVar19 + pfVar10[6];
            auVar54._28_4_ = fVar19 + pfVar10[7];
            pfVar10 = (float *)(lVar31 + lVar47);
            auVar56._0_4_ = fVar19 + *pfVar10;
            auVar56._4_4_ = fVar19 + pfVar10[1];
            auVar56._8_4_ = fVar19 + pfVar10[2];
            auVar56._12_4_ = fVar19 + pfVar10[3];
            auVar56._16_4_ = fVar19 + pfVar10[4];
            auVar56._20_4_ = fVar19 + pfVar10[5];
            auVar56._24_4_ = fVar19 + pfVar10[6];
            auVar56._28_4_ = fVar19 + pfVar10[7];
            *(undefined1 (*) [32])(lVar31 + -0x60 + lVar47) = auVar50;
            *(undefined1 (*) [32])(lVar31 + -0x40 + lVar47) = auVar52;
            *(undefined1 (*) [32])(lVar31 + -0x20 + lVar47) = auVar54;
            *(undefined1 (*) [32])(lVar31 + lVar47) = auVar56;
            lVar47 = lVar47 + 0x80;
          } while ((ulonglong)(param_5 >> 5 & 0x3ffffff) << 7 != lVar47);
          uVar42 = (ulonglong)uVar37;
          uVar36 = (ulonglong)uVar37;
          if (uVar37 != param_5) goto joined_r0x00018000b246;
        }
        lVar34 = lVar34 + 1;
        iVar26 = *(int *)((longlong)local_8168 + 0x14);
        lVar31 = lVar31 + lVar32;
        pvVar45 = (void *)((longlong)pvVar45 + lVar32);
      } while (lVar34 < iVar26);
      goto LAB_18000b2dc;
    }
  }
  lVar31 = 0;
  pvVar45 = local_81b8;
  do {
    rnn_compute_activation_c(pvVar45,pvVar45,param_5,param_7);
    lVar31 = lVar31 + 1;
    pvVar45 = (void *)((longlong)pvVar45 + (longlong)param_6 * 4);
  } while (lVar31 < *(int *)((longlong)plVar21 + 0x14));
LAB_18000b338:
  if ((local_90 ^ (ulonglong)auStack_81d8) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000db80();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b390
   NAME : rnn_parse_weights
   SIG  : undefined rnn_parse_weights(void)
   ======================================================================== */

uint rnn_parse_weights(undefined8 *param_1,longlong param_2,uint param_3)

{
  int iVar1;
  int iVar2;
  undefined4 uVar3;
  void *pvVar4;
  int iVar5;
  uint uVar6;
  longlong lVar7;
  int iVar8;
  
                    /* 0xb390  25  rnn_parse_weights */
  pvVar4 = calloc(0x1e0,1);
  *param_1 = pvVar4;
  if ((int)param_3 < 1) {
    uVar6 = 0;
  }
  else {
    iVar8 = 0x14;
    lVar7 = 0x10;
    uVar6 = 0;
    do {
      if (param_3 < 0x40) {
LAB_18000b482:
        free((void *)*param_1);
        uVar6 = 0xffffffff;
        goto LAB_18000b4a3;
      }
      iVar1 = *(int *)(param_2 + 0xc);
      iVar2 = *(int *)(param_2 + 0x10);
      if ((((iVar2 < iVar1) || (iVar5 = param_3 - 0x40, param_3 = iVar5 - iVar2, iVar5 < iVar2)) ||
          (iVar1 < 0)) || ((*(char *)(param_2 + 0x3f) != '\0' || (iVar1 == 0)))) goto LAB_18000b482;
      uVar3 = *(undefined4 *)(param_2 + 8);
      uVar6 = uVar6 + 1;
      pvVar4 = (void *)*param_1;
      if (iVar8 <= (int)uVar6) {
        iVar8 = (iVar8 * 3) / 2;
        pvVar4 = realloc(pvVar4,(longlong)iVar8 * 0x18);
        *param_1 = pvVar4;
      }
      *(longlong *)((longlong)pvVar4 + lVar7 + -0x10) = param_2 + 0x14;
      *(undefined4 *)((longlong)pvVar4 + lVar7 + -8) = uVar3;
      *(int *)((longlong)pvVar4 + lVar7 + -4) = iVar1;
      *(longlong *)((longlong)pvVar4 + lVar7) = param_2 + 0x40;
      lVar7 = lVar7 + 0x18;
      param_2 = iVar2 + param_2 + 0x40;
    } while (0 < (int)param_3);
    pvVar4 = (void *)*param_1;
  }
  param_1 = (undefined8 *)((longlong)pvVar4 + (ulonglong)uVar6 * 0x18);
LAB_18000b4a3:
  *param_1 = 0;
  return uVar6;
}



/* ========================================================================
   ENTRY: 18000b4c0
   NAME : rnn_linear_init
   SIG  : undefined rnn_linear_init(void)
   ======================================================================== */

undefined8
rnn_linear_init(undefined1 (*param_1) [32],longlong *param_2,char *param_3,char *param_4,
               char *param_5,char *param_6,char *param_7,char *param_8,char *param_9,int param_10,
               int param_11)

{
  uint uVar1;
  int iVar2;
  int iVar3;
  char *pcVar4;
  uint *puVar5;
  longlong *plVar6;
  longlong *plVar7;
  uint uVar8;
  int iVar9;
  longlong lVar10;
  
                    /* 0xb4c0  23  rnn_linear_init */
  *(undefined1 (*) [32])(*param_1 + 0x18) = ZEXT1232(ZEXT812(0));
  *param_1 = ZEXT1232(ZEXT812(0));
  if (param_3 != (char *)0x0) {
    pcVar4 = (char *)*param_2;
    if (pcVar4 == (char *)0x0) {
      return 1;
    }
    plVar7 = param_2;
    while( true ) {
      iVar2 = strcmp(pcVar4,param_3);
      if (iVar2 == 0) break;
      pcVar4 = (char *)plVar7[3];
      plVar7 = plVar7 + 3;
      if (pcVar4 == (char *)0x0) {
        return 1;
      }
    }
    if (*(int *)((longlong)plVar7 + 0xc) != param_11 * 4) {
      return 1;
    }
    lVar10 = plVar7[2];
    *(longlong *)*param_1 = lVar10;
    if (lVar10 == 0) {
      return 1;
    }
  }
  if (param_4 != (char *)0x0) {
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
      plVar7 = param_2;
      do {
        iVar2 = strcmp(pcVar4,param_4);
        if (iVar2 == 0) {
          if (*(int *)((longlong)plVar7 + 0xc) == param_11 * 4) {
            lVar10 = plVar7[2];
            *(longlong *)(*param_1 + 8) = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000b5ba;
          }
          break;
        }
        pcVar4 = (char *)plVar7[3];
        plVar7 = plVar7 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    *(undefined8 *)(*param_1 + 8) = 0;
    return 1;
  }
LAB_18000b5ba:
  plVar7 = param_2;
  if (param_7 == (char *)0x0) {
    if (param_5 != (char *)0x0) {
      pcVar4 = (char *)*param_2;
      if (pcVar4 != (char *)0x0) {
        plVar6 = param_2;
        do {
          iVar2 = strcmp(pcVar4,param_5);
          if (iVar2 == 0) {
            if (*(int *)((longlong)plVar6 + 0xc) == param_11 * param_10) {
              lVar10 = plVar6[2];
              *(longlong *)(*param_1 + 0x10) = lVar10;
              if (lVar10 == 0) {
                return 1;
              }
              goto LAB_18000b786;
            }
            break;
          }
          pcVar4 = (char *)plVar6[3];
          plVar6 = plVar6 + 3;
        } while (pcVar4 != (char *)0x0);
      }
      *(undefined8 *)(*param_1 + 0x10) = 0;
      return 1;
    }
LAB_18000b786:
    if (param_6 == (char *)0x0) goto LAB_18000b84f;
    pcVar4 = (char *)*param_2;
    if (pcVar4 == (char *)0x0) goto LAB_18000b848;
    lVar10 = 0;
    do {
      iVar2 = strcmp(pcVar4,param_6);
      if (iVar2 == 0) {
        if (*(int *)((longlong)plVar7 + 0xc) != param_11 * param_10 * 4) {
          *(undefined8 *)(*param_1 + 0x18) = 0;
          return 1;
        }
        goto LAB_18000b7e2;
      }
      pcVar4 = (char *)plVar7[3];
      plVar7 = plVar7 + 3;
    } while (pcVar4 != (char *)0x0);
  }
  else {
    pcVar4 = (char *)*param_2;
    plVar6 = param_2;
    while ((pcVar4 != (char *)0x0 && (iVar2 = strcmp(pcVar4,param_7), iVar2 != 0))) {
      pcVar4 = (char *)plVar6[3];
      plVar6 = plVar6 + 3;
    }
    iVar3 = *(int *)((longlong)plVar6 + 0xc) >> 2;
    iVar2 = param_11;
    if (iVar3 < 1) {
      iVar9 = 0;
    }
    else {
      puVar5 = (uint *)plVar6[2];
      iVar9 = 0;
      do {
        uVar1 = *puVar5;
        if (iVar3 <= (int)uVar1) goto LAB_18000b704;
        puVar5 = puVar5 + 1;
        uVar8 = uVar1;
        if (0 < (int)uVar1) {
          do {
            if ((param_10 <= (int)(*puVar5 + 3)) || ((*puVar5 & 3) != 0)) goto LAB_18000b704;
            puVar5 = puVar5 + 1;
            uVar8 = uVar8 - 1;
          } while (uVar8 != 0);
        }
        iVar9 = iVar9 + uVar1;
        iVar2 = iVar2 + -8;
        iVar3 = iVar3 + ~uVar1;
      } while (0 < iVar3);
    }
    if (iVar2 != 0) {
LAB_18000b704:
      *(undefined8 *)param_1[1] = 0;
      return 1;
    }
    lVar10 = plVar6[2];
    *(longlong *)param_1[1] = lVar10;
    if (lVar10 == 0) {
      return 1;
    }
    if (param_5 != (char *)0x0) {
      pcVar4 = (char *)*param_2;
      if (pcVar4 != (char *)0x0) {
        plVar6 = param_2;
        do {
          iVar2 = strcmp(pcVar4,param_5);
          if (iVar2 == 0) {
            if (*(int *)((longlong)plVar6 + 0xc) == iVar9 << 5) {
              lVar10 = plVar6[2];
              *(longlong *)(*param_1 + 0x10) = lVar10;
              if (lVar10 == 0) {
                return 1;
              }
              goto LAB_18000b813;
            }
            break;
          }
          pcVar4 = (char *)plVar6[3];
          plVar6 = plVar6 + 3;
        } while (pcVar4 != (char *)0x0);
      }
      *(undefined8 *)(*param_1 + 0x10) = 0;
      return 1;
    }
LAB_18000b813:
    if (param_6 == (char *)0x0) goto LAB_18000b84f;
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
LAB_18000b828:
      iVar2 = strcmp(pcVar4,param_6);
      if (iVar2 != 0) goto code_r0x00018000b83b;
      if (*(int *)((longlong)plVar7 + 0xc) != iVar9 * 0x80) {
        *(undefined8 *)(*param_1 + 0x18) = 0;
        return 1;
      }
LAB_18000b7e2:
      lVar10 = plVar7[2];
      goto LAB_18000b84b;
    }
LAB_18000b848:
    lVar10 = 0;
  }
LAB_18000b84b:
  *(longlong *)(*param_1 + 0x18) = lVar10;
LAB_18000b84f:
  if (param_8 == (char *)0x0) {
LAB_18000b8b3:
    if (param_5 == (char *)0x0) {
LAB_18000b922:
      *(int *)(param_1[1] + 0x18) = param_10;
      *(int *)(param_1[1] + 0x1c) = param_11;
      return 0;
    }
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
      do {
        iVar2 = strcmp(pcVar4,param_9);
        if (iVar2 == 0) {
          if (*(int *)((longlong)param_2 + 0xc) == param_11 * 4) {
            lVar10 = param_2[2];
            *(longlong *)(param_1[1] + 0x10) = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000b922;
          }
          break;
        }
        pcVar4 = (char *)param_2[3];
        param_2 = param_2 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    *(undefined8 *)(param_1[1] + 0x10) = 0;
  }
  else {
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
      plVar7 = param_2;
      do {
        iVar2 = strcmp(pcVar4,param_8);
        if (iVar2 == 0) {
          if (*(int *)((longlong)plVar7 + 0xc) == param_11 * 4) {
            lVar10 = plVar7[2];
            *(longlong *)(param_1[1] + 8) = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000b8b3;
          }
          break;
        }
        pcVar4 = (char *)plVar7[3];
        plVar7 = plVar7 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    *(undefined8 *)(param_1[1] + 8) = 0;
  }
  return 1;
code_r0x00018000b83b:
  pcVar4 = (char *)plVar7[3];
  plVar7 = plVar7 + 3;
  if (pcVar4 == (char *)0x0) goto LAB_18000b848;
  goto LAB_18000b828;
}



/* ========================================================================
   ENTRY: 18000b970
   NAME : rnn_conv2d_init
   SIG  : undefined rnn_conv2d_init(void)
   ======================================================================== */

undefined4
rnn_conv2d_init(undefined1 (*param_1) [16],longlong *param_2,char *param_3,char *param_4,int param_5
               ,int param_6,int param_7,int param_8)

{
  longlong *plVar1;
  int iVar2;
  char *pcVar3;
  undefined4 uVar4;
  longlong lVar5;
  undefined1 auVar6 [16];
  
                    /* 0xb970  13  rnn_conv2d_init */
  auVar6._0_12_ = ZEXT812(0);
  auVar6._12_4_ = 0;
  *param_1 = auVar6;
  if (param_3 == (char *)0x0) {
LAB_18000b9fa:
    if (param_4 != (char *)0x0) {
      pcVar3 = (char *)*param_2;
      if (pcVar3 == (char *)0x0) {
        lVar5 = 0;
      }
      else {
        lVar5 = 0;
        do {
          iVar2 = strcmp(pcVar3,param_4);
          if (iVar2 == 0) {
            if (*(int *)((longlong)param_2 + 0xc) != param_7 * param_8 * param_5 * param_6 * 4) {
              *(undefined8 *)(*param_1 + 8) = 0;
              return 1;
            }
            lVar5 = param_2[2];
            break;
          }
          pcVar3 = (char *)param_2[3];
          param_2 = param_2 + 3;
        } while (pcVar3 != (char *)0x0);
      }
      *(longlong *)(*param_1 + 8) = lVar5;
    }
    *(int *)param_1[1] = param_5;
    *(int *)(param_1[1] + 4) = param_6;
    *(int *)(param_1[1] + 8) = param_7;
    *(int *)(param_1[1] + 0xc) = param_8;
    uVar4 = 0;
  }
  else {
    pcVar3 = (char *)*param_2;
    uVar4 = 1;
    if (pcVar3 != (char *)0x0) {
      plVar1 = param_2;
      do {
        iVar2 = strcmp(pcVar3,param_3);
        if (iVar2 == 0) {
          if (*(int *)((longlong)plVar1 + 0xc) != param_6 * 4) {
            return 1;
          }
          lVar5 = plVar1[2];
          *(longlong *)*param_1 = lVar5;
          if (lVar5 == 0) {
            return 1;
          }
          goto LAB_18000b9fa;
        }
        pcVar3 = (char *)plVar1[3];
        plVar1 = plVar1 + 3;
      } while (pcVar3 != (char *)0x0);
    }
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000bab0
   NAME : rnn_pitch_downsample
   SIG  : undefined rnn_pitch_downsample(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_pitch_downsample(longlong *param_1,float *param_2,int param_3,int param_4)

{
  float *pfVar1;
  float fVar2;
  undefined1 auVar3 [32];
  undefined1 auVar4 [32];
  longlong lVar5;
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [32];
  undefined1 auVar10 [32];
  undefined1 auVar11 [32];
  undefined1 auVar12 [32];
  undefined1 auVar13 [32];
  undefined1 auVar14 [32];
  undefined1 auVar15 [32];
  undefined1 auVar16 [32];
  undefined1 auVar17 [32];
  undefined1 auVar18 [32];
  longlong lVar19;
  ulonglong uVar20;
  uint uVar21;
  ulonglong uVar22;
  float fVar23;
  float fVar24;
  undefined1 auVar25 [16];
  undefined1 auVar26 [64];
  undefined1 auVar27 [64];
  float fVar28;
  float fVar29;
  float fVar30;
  undefined1 auVar31 [64];
  undefined1 auVar32 [64];
  undefined1 auVar33 [64];
  undefined1 auStack_1b8 [32];
  undefined4 local_198;
  uint local_190;
  undefined1 local_188 [16];
  undefined1 local_178 [16];
  float local_168;
  undefined4 uStack_164;
  undefined4 uStack_160;
  undefined4 uStack_15c;
  float local_158;
  undefined4 uStack_154;
  undefined4 uStack_150;
  undefined4 uStack_14c;
  float local_148;
  undefined4 uStack_144;
  undefined4 uStack_140;
  undefined4 uStack_13c;
  float local_138;
  float fStack_134;
  float fStack_130;
  float fStack_12c;
  float fStack_128;
  float fStack_124;
  float fStack_120;
  float fStack_11c;
  float local_118;
  float fStack_114;
  float fStack_110;
  float fStack_10c;
  float fStack_108;
  float fStack_104;
  float fStack_100;
  float fStack_fc;
  float local_f8;
  float local_f4;
  float local_f0;
  float local_ec;
  float local_e8;
  undefined1 local_e4 [16];
  ulonglong local_d0;
  
                    /* 0xbab0  26  rnn_pitch_downsample */
  fVar23 = DAT_18000f1d8;
  local_d0 = DAT_180580000 ^ (ulonglong)auStack_1b8;
  uVar21 = param_3 >> 1;
  uVar22 = (ulonglong)uVar21;
  if (1 < (int)uVar21) {
    lVar19 = 1;
    if (uVar21 != 2) {
      do {
        lVar5 = *param_1;
        param_2[lVar19] =
             (fVar23 * (*(float *)(lVar5 + -4 + lVar19 * 8) + *(float *)(lVar5 + 4 + lVar19 * 8)) +
             *(float *)(lVar5 + lVar19 * 8)) * fVar23;
        lVar5 = *param_1;
        param_2[lVar19 + 1] =
             (fVar23 * (*(float *)(lVar5 + 4 + lVar19 * 8) + *(float *)(lVar5 + 0xc + lVar19 * 8)) +
             *(float *)(lVar5 + 8 + lVar19 * 8)) * fVar23;
        lVar5 = lVar19 - (uVar22 - 1 & 0xfffffffffffffffe);
        lVar19 = lVar19 + 2;
      } while (lVar5 != -1);
    }
    if ((uVar22 - 1 & 1) != 0) {
      lVar5 = *param_1;
      param_2[lVar19] =
           (DAT_18000f1d8 *
            (*(float *)(lVar5 + -4 + lVar19 * 8) + *(float *)(lVar5 + 4 + lVar19 * 8)) +
           *(float *)(lVar5 + lVar19 * 8)) * DAT_18000f1d8;
    }
  }
  fVar24 = DAT_18000f1d8;
  fVar23 = (DAT_18000f1d8 * ((float *)*param_1)[1] + *(float *)*param_1) * DAT_18000f1d8;
  *param_2 = fVar23;
  if (param_4 == 2) {
    if (1 < (int)uVar21) {
      lVar19 = 1;
      if (uVar21 != 2) {
        do {
          lVar5 = param_1[1];
          param_2[lVar19] =
               fVar24 * (fVar24 * (*(float *)(lVar5 + -4 + lVar19 * 8) +
                                  *(float *)(lVar5 + 4 + lVar19 * 8)) +
                        *(float *)(lVar5 + lVar19 * 8)) + param_2[lVar19];
          lVar5 = param_1[1];
          param_2[lVar19 + 1] =
               fVar24 * (fVar24 * (*(float *)(lVar5 + 4 + lVar19 * 8) +
                                  *(float *)(lVar5 + 0xc + lVar19 * 8)) +
                        *(float *)(lVar5 + 8 + lVar19 * 8)) + param_2[lVar19 + 1];
          lVar5 = lVar19 - (uVar22 - 1 & 0xfffffffffffffffe);
          lVar19 = lVar19 + 2;
        } while (lVar5 != -1);
      }
      if ((uVar22 - 1 & 1) != 0) {
        lVar5 = param_1[1];
        param_2[lVar19] =
             fVar24 * (fVar24 * (*(float *)(lVar5 + -4 + lVar19 * 8) +
                                *(float *)(lVar5 + 4 + lVar19 * 8)) + *(float *)(lVar5 + lVar19 * 8)
                      ) + param_2[lVar19];
      }
    }
    *param_2 = fVar24 * (fVar24 * ((float *)param_1[1])[1] + *(float *)param_1[1]) + fVar23;
  }
  local_198 = 4;
  local_190 = uVar21;
  rnn_autocorr(param_2,&local_e8,0,0);
  local_e8 = local_e8 * DAT_18000f250;
  fVar23 = local_e4._4_4_;
  fVar24 = local_e4._8_4_;
  fVar28 = local_e4._12_4_;
  local_e4._0_4_ = -((float)DAT_18000f260 * local_e4._0_4_ * (float)DAT_18000f260) + local_e4._0_4_;
  local_e4._4_4_ = -(DAT_18000f260._4_4_ * fVar23 * DAT_18000f260._4_4_) + fVar23;
  local_e4._8_4_ = -(DAT_18000f260._8_4_ * fVar24 * DAT_18000f260._8_4_) + fVar24;
  local_e4._12_4_ = -(DAT_18000f260._12_4_ * fVar28 * DAT_18000f260._12_4_) + fVar28;
  rnn_lpc(&local_f8,&local_e8,4);
  if (0 < (int)uVar21) {
    fVar23 = local_f8 * DAT_18000f270 + DAT_18000f280;
    fVar28 = DAT_18000f280 * local_f8 * DAT_18000f270 + local_f4 * DAT_18000f274;
    fVar29 = DAT_18000f280 * local_f4 * DAT_18000f274 + local_f0 * DAT_18000f278;
    fVar30 = DAT_18000f280 * local_f0 * DAT_18000f278 + local_ec * DAT_18000f27c;
    fVar24 = local_ec * DAT_18000f27c * DAT_18000f280;
    if (uVar21 < 0x10) {
      auVar26 = ZEXT1264(ZEXT812(0));
      auVar27 = ZEXT1264(ZEXT812(0));
      uVar20 = 0;
    }
    else {
      uVar20 = (ulonglong)(uVar21 & 0x7ffffff0);
      local_178 = ZEXT416((uint)fVar23);
      uStack_144 = 0;
      uStack_140 = 0;
      uStack_13c = 0;
      uStack_154 = 0;
      uStack_150 = 0;
      uStack_14c = 0;
      uStack_164 = 0;
      uStack_160 = 0;
      uStack_15c = 0;
      local_188 = ZEXT416((uint)fVar24);
      auVar26 = ZEXT1264(ZEXT812(0));
      lVar19 = 0;
      auVar31 = ZEXT1264(ZEXT812(0));
      auVar33 = ZEXT1264(ZEXT812(0));
      auVar32 = ZEXT1264(ZEXT812(0));
      auVar27 = ZEXT1264(ZEXT812(0));
      do {
        auVar3 = *(undefined1 (*) [32])((longlong)param_2 + lVar19);
        auVar11 = vperm2f128_avx(auVar27._0_32_,auVar3,0x21);
        auVar12 = vperm2f128_avx(auVar26._0_32_,auVar27._0_32_,0x31);
        auVar4 = *(undefined1 (*) [32])((longlong)param_2 + lVar19 + 0x20);
        auVar27 = ZEXT3264(auVar4);
        auVar9 = vshufps_avx(auVar11,auVar3,3);
        auVar9 = vshufps_avx(auVar9,auVar3,0x98);
        auVar13 = vperm2f128_avx(auVar3,auVar4,0x21);
        auVar10 = vshufps_avx(auVar13,auVar4,3);
        auVar10 = vshufps_avx(auVar10,auVar4,0x98);
        auVar14 = vperm2f128_avx(auVar32._0_32_,auVar9,0x21);
        auVar11 = vshufps_avx(auVar14,auVar11,0xff);
        auVar11 = vshufps_avx(auVar11,auVar3,0x48);
        auVar15 = vperm2f128_avx(auVar9,auVar10,0x21);
        auVar13 = vshufps_avx(auVar15,auVar13,0xff);
        auVar16 = vperm2f128_avx(auVar33._0_32_,auVar11,0x21);
        auVar13 = vshufps_avx(auVar13,auVar4,0x48);
        auVar33 = ZEXT3264(auVar13);
        auVar14 = vshufps_avx(auVar16,auVar14,0xff);
        auVar14 = vshufps_avx(auVar14,auVar11,0x98);
        auVar17 = vperm2f128_avx(auVar11,auVar13,0x21);
        auVar15 = vshufps_avx(auVar17,auVar15,0xff);
        auVar18 = vperm2f128_avx(auVar31._0_32_,auVar14,0x21);
        auVar15 = vshufps_avx(auVar15,auVar13,0x98);
        auVar31 = ZEXT3264(auVar15);
        auVar16 = vshufps_avx(auVar18,auVar16,0xff);
        auVar12 = vshufps_avx(auVar12,auVar18,0xff);
        auVar18 = vperm2f128_avx(auVar14,auVar15,0x21);
        auVar17 = vshufps_avx(auVar18,auVar17,0xff);
        auVar18 = vshufps_avx(auVar3,auVar18,0xff);
        auVar16 = vshufps_avx(auVar16,auVar14,0x98);
        auVar12 = vshufps_avx(auVar12,auVar16,0x98);
        auVar17 = vshufps_avx(auVar17,auVar15,0x98);
        auVar18 = vshufps_avx(auVar18,auVar17,0x98);
        pfVar1 = (float *)((longlong)param_2 + lVar19);
        *pfVar1 = fVar24 * auVar12._0_4_ +
                  fVar30 * auVar16._0_4_ +
                  fVar29 * auVar14._0_4_ +
                  fVar28 * auVar11._0_4_ + fVar23 * auVar9._0_4_ + auVar3._0_4_;
        pfVar1[1] = fVar24 * auVar12._4_4_ +
                    fVar30 * auVar16._4_4_ +
                    fVar29 * auVar14._4_4_ +
                    fVar28 * auVar11._4_4_ + fVar23 * auVar9._4_4_ + auVar3._4_4_;
        pfVar1[2] = fVar24 * auVar12._8_4_ +
                    fVar30 * auVar16._8_4_ +
                    fVar29 * auVar14._8_4_ +
                    fVar28 * auVar11._8_4_ + fVar23 * auVar9._8_4_ + auVar3._8_4_;
        pfVar1[3] = fVar24 * auVar12._12_4_ +
                    fVar30 * auVar16._12_4_ +
                    fVar29 * auVar14._12_4_ +
                    fVar28 * auVar11._12_4_ + fVar23 * auVar9._12_4_ + auVar3._12_4_;
        pfVar1[4] = fVar24 * auVar12._16_4_ +
                    fVar30 * auVar16._16_4_ +
                    fVar29 * auVar14._16_4_ +
                    fVar28 * auVar11._16_4_ + fVar23 * auVar9._16_4_ + auVar3._16_4_;
        pfVar1[5] = fVar24 * auVar12._20_4_ +
                    fVar30 * auVar16._20_4_ +
                    fVar29 * auVar14._20_4_ +
                    fVar28 * auVar11._20_4_ + fVar23 * auVar9._20_4_ + auVar3._20_4_;
        pfVar1[6] = fVar24 * auVar12._24_4_ +
                    fVar30 * auVar16._24_4_ +
                    fVar29 * auVar14._24_4_ +
                    fVar28 * auVar11._24_4_ + fVar23 * auVar9._24_4_ + auVar3._24_4_;
        pfVar1[7] = fVar24 * auVar12._28_4_ +
                    fVar30 * auVar16._28_4_ +
                    fVar29 * auVar14._28_4_ +
                    fVar28 * auVar11._28_4_ + fVar23 * auVar9._28_4_ + auVar3._28_4_;
        pfVar1 = (float *)((longlong)param_2 + lVar19 + 0x20);
        *pfVar1 = fVar24 * auVar18._0_4_ +
                  fVar30 * auVar17._0_4_ +
                  fVar29 * auVar15._0_4_ +
                  fVar28 * auVar13._0_4_ + fVar23 * auVar10._0_4_ + auVar4._0_4_;
        pfVar1[1] = fVar24 * auVar18._4_4_ +
                    fVar30 * auVar17._4_4_ +
                    fVar29 * auVar15._4_4_ +
                    fVar28 * auVar13._4_4_ + fVar23 * auVar10._4_4_ + auVar4._4_4_;
        pfVar1[2] = fVar24 * auVar18._8_4_ +
                    fVar30 * auVar17._8_4_ +
                    fVar29 * auVar15._8_4_ +
                    fVar28 * auVar13._8_4_ + fVar23 * auVar10._8_4_ + auVar4._8_4_;
        pfVar1[3] = fVar24 * auVar18._12_4_ +
                    fVar30 * auVar17._12_4_ +
                    fVar29 * auVar15._12_4_ +
                    fVar28 * auVar13._12_4_ + fVar23 * auVar10._12_4_ + auVar4._12_4_;
        pfVar1[4] = fVar24 * auVar18._16_4_ +
                    fVar30 * auVar17._16_4_ +
                    fVar29 * auVar15._16_4_ +
                    fVar28 * auVar13._16_4_ + fVar23 * auVar10._16_4_ + auVar4._16_4_;
        pfVar1[5] = fVar24 * auVar18._20_4_ +
                    fVar30 * auVar17._20_4_ +
                    fVar29 * auVar15._20_4_ +
                    fVar28 * auVar13._20_4_ + fVar23 * auVar10._20_4_ + auVar4._20_4_;
        pfVar1[6] = fVar24 * auVar18._24_4_ +
                    fVar30 * auVar17._24_4_ +
                    fVar29 * auVar15._24_4_ +
                    fVar28 * auVar13._24_4_ + fVar23 * auVar10._24_4_ + auVar4._24_4_;
        pfVar1[7] = fVar24 * auVar18._28_4_ +
                    fVar30 * auVar17._28_4_ +
                    fVar29 * auVar15._28_4_ +
                    fVar28 * auVar13._28_4_ + fVar23 * auVar10._28_4_ + auVar4._28_4_;
        auVar32 = ZEXT3264(auVar10);
        lVar19 = lVar19 + 0x40;
        auVar26 = ZEXT3264(auVar17);
      } while ((ulonglong)(uVar21 >> 4 & 0x7ffffff) << 6 != lVar19);
      local_168 = fVar30;
      local_158 = fVar29;
      local_148 = fVar28;
      local_138 = fVar28;
      fStack_134 = fVar28;
      fStack_130 = fVar28;
      fStack_12c = fVar28;
      fStack_128 = fVar28;
      fStack_124 = fVar28;
      fStack_120 = fVar28;
      fStack_11c = fVar28;
      local_118 = fVar23;
      fStack_114 = fVar23;
      fStack_110 = fVar23;
      fStack_10c = fVar23;
      fStack_108 = fVar23;
      fStack_104 = fVar23;
      fStack_100 = fVar23;
      fStack_fc = fVar23;
      if ((uVar21 & 0x7ffffff0) == uVar21) goto LAB_18000bfd8;
      auVar7 = vshufps_avx(auVar4._0_16_,auVar4._0_16_,0xff);
      auVar27 = ZEXT1664(auVar7);
      auVar7 = vshufps_avx(auVar4._16_16_,auVar4._16_16_,0xb1);
      auVar26 = ZEXT1664(auVar7);
    }
    do {
      fVar2 = param_2[uVar20];
      auVar25 = auVar26._0_16_;
      auVar7 = vshufpd_avx(auVar25,auVar25,1);
      auVar8 = vshufps_avx(auVar25,auVar25,0xff);
      auVar6 = vmovshdup_avx(auVar25);
      param_2[uVar20] =
           fVar24 * auVar27._0_4_ +
           fVar30 * auVar6._0_4_ +
           fVar29 * auVar26._0_4_ + fVar28 * auVar8._0_4_ + fVar23 * auVar7._0_4_ + fVar2;
      uVar20 = uVar20 + 1;
      auVar7 = vshufps_avx(auVar25,auVar25,0xa3);
      auVar7 = vinsertps_avx(auVar7,ZEXT416((uint)fVar2),0x20);
      auVar26 = ZEXT1664(auVar7);
      auVar27 = ZEXT1664(auVar6);
    } while (uVar22 != uVar20);
  }
LAB_18000bfd8:
  if ((local_d0 ^ (ulonglong)auStack_1b8) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000db80();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000c060
   NAME : rnn_pitch_xcorr
   SIG  : undefined rnn_pitch_xcorr(void)
   ======================================================================== */

void rnn_pitch_xcorr(float *param_1,longlong param_2,longlong param_3,uint param_4,uint param_5)

{
  uint uVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  uint uVar6;
  undefined8 uVar7;
  undefined1 auVar8 [16];
  longlong lVar9;
  longlong lVar10;
  uint uVar11;
  ulonglong uVar12;
  ulonglong uVar13;
  int iVar14;
  float *pfVar15;
  uint *puVar16;
  longlong lVar17;
  ulonglong uVar18;
  float fVar19;
  float fVar21;
  float fVar22;
  float fVar23;
  undefined1 auVar20 [16];
  undefined1 auVar24 [16];
  undefined1 auVar25 [16];
  
                    /* 0xc060  29  rnn_pitch_xcorr */
  uVar12 = 0;
  if (3 < (int)param_5) {
    uVar13 = (ulonglong)(param_5 - 3);
    if ((int)param_4 < 4) {
      uVar12 = 0;
      auVar20._0_12_ = ZEXT812(0);
      auVar20._12_4_ = 0;
      if ((int)param_4 < 1) {
        do {
          fVar19 = 0.0;
          fVar21 = 0.0;
          fVar22 = 0.0;
          fVar23 = 0.0;
          if (1 < (int)param_4) {
            fVar23 = *param_1;
            auVar25._8_8_ = 0;
            auVar25._0_8_ = *(ulonglong *)(param_2 + 4 + uVar12 * 4);
            auVar25 = vshufps_avx(auVar25,ZEXT416(*(uint *)(param_2 + 0xc + uVar12 * 4)),0x14);
            fVar19 = auVar25._0_4_ * fVar23 + 0.0;
            fVar21 = auVar25._4_4_ * fVar23 + 0.0;
            fVar22 = auVar25._8_4_ * fVar23 + 0.0;
            fVar23 = auVar25._12_4_ * fVar23 + 0.0;
            if (param_4 == 3) {
              fVar2 = param_1[1];
              auVar24 = vinsertps_avx(auVar20,auVar25,0x6b);
              auVar25 = vshufps_avx(auVar24,auVar25,0xf2);
              auVar25 = vinsertps_avx(auVar25,ZEXT416(*(uint *)(param_2 + 0x10 + uVar12 * 4)),0x30);
              fVar19 = fVar2 * auVar25._0_4_ + fVar19;
              fVar21 = fVar2 * auVar25._4_4_ + fVar21;
              fVar22 = fVar2 * auVar25._8_4_ + fVar22;
              fVar23 = fVar2 * auVar25._12_4_ + fVar23;
            }
          }
          pfVar15 = (float *)(param_3 + uVar12 * 4);
          *pfVar15 = fVar19;
          pfVar15[1] = fVar21;
          pfVar15[2] = fVar22;
          pfVar15[3] = fVar23;
          uVar12 = uVar12 + 4;
        } while (uVar12 < uVar13);
      }
      else {
        do {
          auVar20 = *(undefined1 (*) [16])(param_2 + uVar12 * 4);
          fVar19 = *param_1;
          fVar21 = auVar20._0_4_ * fVar19 + 0.0;
          fVar22 = auVar20._4_4_ * fVar19 + 0.0;
          fVar23 = auVar20._8_4_ * fVar19 + 0.0;
          fVar19 = auVar20._12_4_ * fVar19 + 0.0;
          if (1 < (int)param_4) {
            fVar2 = param_1[1];
            auVar25 = vshufps_avx(auVar20,auVar20,0xf9);
            auVar24 = ZEXT416(*(uint *)(param_2 + 0x10 + uVar12 * 4));
            auVar25 = vinsertps_avx(auVar25,auVar24,0x30);
            fVar21 = fVar2 * auVar25._0_4_ + fVar21;
            fVar22 = fVar2 * auVar25._4_4_ + fVar22;
            fVar23 = fVar2 * auVar25._8_4_ + fVar23;
            fVar19 = fVar2 * auVar25._12_4_ + fVar19;
            if (param_4 == 3) {
              fVar2 = param_1[2];
              auVar20 = vshufpd_avx(auVar20,auVar24,1);
              auVar20 = vinsertps_avx(auVar20,ZEXT416(*(uint *)(param_2 + 0x14 + uVar12 * 4)),0x30);
              fVar21 = fVar2 * auVar20._0_4_ + fVar21;
              fVar22 = fVar2 * auVar20._4_4_ + fVar22;
              fVar23 = fVar2 * auVar20._8_4_ + fVar23;
              fVar19 = fVar2 * auVar20._12_4_ + fVar19;
            }
          }
          pfVar15 = (float *)(param_3 + uVar12 * 4);
          *pfVar15 = fVar21;
          pfVar15[1] = fVar22;
          pfVar15[2] = fVar23;
          pfVar15[3] = fVar19;
          uVar12 = uVar12 + 4;
        } while (uVar12 < uVar13);
      }
    }
    else {
      uVar11 = param_4 & 0x7ffffffc;
      uVar12 = 0;
      lVar17 = param_2;
      do {
        uVar1 = *(uint *)(param_2 + uVar12 * 4);
        auVar24._8_8_ = 0;
        auVar24._0_8_ = *(ulonglong *)(param_2 + 4 + uVar12 * 4);
        fVar19 = 0.0;
        fVar21 = 0.0;
        fVar22 = 0.0;
        fVar23 = 0.0;
        iVar14 = 0;
        lVar10 = 0;
        do {
          lVar9 = lVar10;
          auVar20 = *(undefined1 (*) [16])(lVar17 + 0xc + lVar9);
          fVar2 = *(float *)((longlong)param_1 + lVar9);
          auVar25 = vmovlhps_avx(ZEXT416(uVar1),auVar24);
          auVar25 = vshufps_avx(auVar25,auVar24,0xd8);
          auVar25 = vinsertps_avx(auVar25,auVar20,0x30);
          fVar3 = *(float *)((longlong)param_1 + lVar9 + 4);
          auVar24 = vmovlhps_avx(auVar24,auVar20);
          fVar4 = *(float *)((longlong)param_1 + lVar9 + 8);
          auVar8 = vshufps_avx(auVar24,auVar20,0x99);
          fVar5 = *(float *)((longlong)param_1 + lVar9 + 0xc);
          fVar19 = auVar20._0_4_ * fVar5 +
                   fVar4 * auVar8._0_4_ + auVar24._0_4_ * fVar3 + fVar2 * auVar25._0_4_ + fVar19;
          fVar21 = auVar20._4_4_ * fVar5 +
                   fVar4 * auVar8._4_4_ + auVar24._4_4_ * fVar3 + fVar2 * auVar25._4_4_ + fVar21;
          fVar22 = auVar20._8_4_ * fVar5 +
                   fVar4 * auVar8._8_4_ + auVar24._8_4_ * fVar3 + fVar2 * auVar25._8_4_ + fVar22;
          fVar23 = auVar20._12_4_ * fVar5 +
                   fVar4 * auVar8._12_4_ + auVar24._12_4_ * fVar3 + fVar2 * auVar25._12_4_ + fVar23;
          iVar14 = iVar14 + 4;
          uVar1 = *(uint *)(lVar17 + 0x10 + lVar9);
          uVar7 = *(undefined8 *)(lVar17 + 0x14 + lVar9);
          auVar24._8_8_ = uVar7;
          auVar24._0_8_ = uVar7;
          lVar10 = lVar9 + 0x10;
        } while (iVar14 < (int)(param_4 - 3));
        if (uVar11 == param_4) {
          puVar16 = (uint *)(lVar17 + lVar10 + 0xc);
          auVar25 = auVar20;
        }
        else {
          puVar16 = (uint *)(lVar17 + 0x10 + lVar10);
          uVar6 = *(uint *)(lVar17 + 0xc + lVar10);
          fVar2 = *(float *)((longlong)param_1 + lVar10);
          auVar25 = vshufps_avx(auVar20,auVar20,0xf9);
          auVar25 = vinsertps_avx(auVar25,ZEXT416(uVar6),0x30);
          fVar19 = fVar2 * auVar25._0_4_ + fVar19;
          fVar21 = fVar2 * auVar25._4_4_ + fVar21;
          fVar22 = fVar2 * auVar25._8_4_ + fVar22;
          fVar23 = fVar2 * auVar25._12_4_ + fVar23;
          auVar25 = ZEXT416(uVar6);
          lVar10 = lVar9 + 0x14;
        }
        pfVar15 = (float *)((longlong)param_1 + lVar10);
        if (uVar11 + 1 < param_4) {
          uVar1 = *puVar16;
          puVar16 = puVar16 + 1;
          fVar2 = *pfVar15;
          pfVar15 = pfVar15 + 1;
          auVar24 = vshufpd_avx(auVar20,auVar25,1);
          auVar24 = vinsertps_avx(auVar24,ZEXT416(uVar1),0x30);
          fVar19 = fVar2 * auVar24._0_4_ + fVar19;
          fVar21 = fVar2 * auVar24._4_4_ + fVar21;
          fVar22 = fVar2 * auVar24._8_4_ + fVar22;
          fVar23 = fVar2 * auVar24._12_4_ + fVar23;
        }
        if (uVar11 + 2 < param_4) {
          fVar2 = *pfVar15;
          auVar20 = vshufps_avx(auVar20,auVar20,0xff);
          auVar20 = vinsertps_avx(auVar20,auVar25,0x10);
          auVar20 = vinsertps_avx(auVar20,ZEXT416(uVar1),0x20);
          auVar20 = vinsertps_avx(auVar20,ZEXT416(*puVar16),0x30);
          fVar19 = fVar2 * auVar20._0_4_ + fVar19;
          fVar21 = fVar2 * auVar20._4_4_ + fVar21;
          fVar22 = fVar2 * auVar20._8_4_ + fVar22;
          fVar23 = fVar2 * auVar20._12_4_ + fVar23;
        }
        pfVar15 = (float *)(param_3 + uVar12 * 4);
        *pfVar15 = fVar19;
        pfVar15[1] = fVar21;
        pfVar15[2] = fVar22;
        pfVar15[3] = fVar23;
        uVar12 = uVar12 + 4;
        lVar17 = lVar17 + 0x10;
      } while (uVar12 < uVar13);
    }
  }
  if ((int)(uint)uVar12 < (int)param_5) {
    uVar13 = uVar12 & 0xffffffff;
    if ((int)param_4 < 1) {
      memset((void *)(param_3 + uVar13 * 4),0,(ulonglong)(~(uint)uVar12 + param_5) * 4 + 4);
      return;
    }
    lVar17 = param_2 + uVar13 * 4 + 0xc;
    param_2 = param_2 + uVar13 * 4;
    do {
      fVar19 = 0.0;
      uVar12 = 0;
      if (3 < param_4) {
        do {
          fVar19 = param_1[uVar12 + 3] * *(float *)(lVar17 + uVar12 * 4) +
                   param_1[uVar12 + 2] * *(float *)(lVar17 + -4 + uVar12 * 4) +
                   param_1[uVar12 + 1] * *(float *)(lVar17 + -8 + uVar12 * 4) +
                   param_1[uVar12] * *(float *)(lVar17 + -0xc + uVar12 * 4) + fVar19;
          uVar12 = uVar12 + 4;
        } while ((param_4 & 0x7ffffffc) != uVar12);
      }
      if ((ulonglong)(param_4 & 3) != 0) {
        uVar18 = 0;
        do {
          fVar19 = param_1[uVar12 + uVar18] * *(float *)(param_2 + uVar12 * 4 + uVar18 * 4) + fVar19
          ;
          uVar18 = uVar18 + 1;
        } while ((param_4 & 3) != uVar18);
      }
      *(float *)(param_3 + uVar13 * 4) = fVar19;
      uVar13 = uVar13 + 1;
      lVar17 = lVar17 + 4;
      param_2 = param_2 + 4;
    } while (uVar13 != param_5);
  }
  return;
}



/* ========================================================================
   ENTRY: 18000c3f0
   NAME : rnn_pitch_search
   SIG  : undefined rnn_pitch_search(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_pitch_search(longlong param_1,longlong param_2,int param_3,int param_4,int *param_5)

{
  float fVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  undefined1 auVar6 [32];
  undefined1 auVar7 [32];
  undefined1 auVar8 [32];
  undefined1 auVar9 [32];
  uint uVar10;
  ulonglong uVar11;
  int iVar12;
  int iVar13;
  ulonglong uVar14;
  uint uVar15;
  uint uVar16;
  uint uVar17;
  ulonglong uVar18;
  ulonglong uVar19;
  ulonglong uVar20;
  longlong lVar21;
  longlong lVar22;
  float fVar23;
  float fVar24;
  undefined1 auVar25 [64];
  undefined1 auVar26 [16];
  undefined1 auVar27 [64];
  float fVar28;
  undefined1 auVar29 [16];
  undefined1 auVar30 [64];
  undefined1 auVar31 [64];
  undefined1 auVar32 [64];
  undefined1 local_1158 [32];
  uint local_1138;
  ulonglong local_1130;
  float local_1128 [384];
  float local_b28 [8];
  undefined1 auStack_b08 [32];
  undefined1 auStack_ae8 [32];
  undefined1 auStack_ac8 [1632];
  undefined4 local_468 [8];
  undefined1 auStack_448 [32];
  undefined1 auStack_428 [32];
  undefined1 auStack_408 [872];
  ulonglong local_a0;
  undefined8 uStack_48;
  
                    /* 0xc3f0  28  rnn_pitch_search */
  uStack_48 = 0x18000c406;
  local_a0 = DAT_180580000 ^ (ulonglong)local_1158;
  uVar15 = param_3 >> 2;
  if (0 < (int)uVar15) {
    if (uVar15 < 0x21) {
      uVar11 = 0;
    }
    else {
      uVar11 = 0x20;
      if ((uVar15 & 0x1f) != 0) {
        uVar11 = (ulonglong)(uVar15 & 0x1f);
      }
      uVar11 = uVar15 - uVar11;
      uVar14 = 0;
      do {
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_1 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_1 + 0x20 + uVar14 * 8),0x88);
        auVar8 = vpermpd_avx2(auVar6,0xd8);
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_1 + 0x40 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_1 + 0x60 + uVar14 * 8),0x88);
        auVar9 = vpermpd_avx2(auVar6,0xd8);
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_1 + 0x80 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_1 + 0xa0 + uVar14 * 8),0x88);
        auVar7 = vshufps_avx(*(undefined1 (*) [32])(param_1 + 0xc0 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_1 + 0xe0 + uVar14 * 8),0x88);
        auVar6 = vpermpd_avx2(auVar6,0xd8);
        auVar7 = vpermpd_avx2(auVar7,0xd8);
        *(undefined1 (*) [32])(local_468 + uVar14) = auVar8;
        *(undefined1 (*) [32])(auStack_448 + uVar14 * 4) = auVar9;
        *(undefined1 (*) [32])(auStack_428 + uVar14 * 4) = auVar6;
        *(undefined1 (*) [32])(auStack_408 + uVar14 * 4) = auVar7;
        uVar14 = uVar14 + 0x20;
      } while (uVar11 != uVar14);
    }
    do {
      local_468[uVar11] = *(undefined4 *)(param_1 + uVar11 * 8);
      uVar11 = uVar11 + 1;
    } while (uVar15 != uVar11);
  }
  uVar10 = param_4 + param_3 >> 2;
  if (0 < (int)uVar10) {
    if (uVar10 < 0x21) {
      uVar11 = 0;
    }
    else {
      uVar11 = 0x20;
      if ((uVar10 & 0x1f) != 0) {
        uVar11 = (ulonglong)(uVar10 & 0x1f);
      }
      uVar11 = uVar10 - uVar11;
      uVar14 = 0;
      do {
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_2 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_2 + 0x20 + uVar14 * 8),0x88);
        auVar8 = vpermpd_avx2(auVar6,0xd8);
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_2 + 0x40 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_2 + 0x60 + uVar14 * 8),0x88);
        auVar9 = vpermpd_avx2(auVar6,0xd8);
        auVar6 = vshufps_avx(*(undefined1 (*) [32])(param_2 + 0x80 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_2 + 0xa0 + uVar14 * 8),0x88);
        auVar7 = vshufps_avx(*(undefined1 (*) [32])(param_2 + 0xc0 + uVar14 * 8),
                             *(undefined1 (*) [32])(param_2 + 0xe0 + uVar14 * 8),0x88);
        auVar6 = vpermpd_avx2(auVar6,0xd8);
        auVar7 = vpermpd_avx2(auVar7,0xd8);
        *(undefined1 (*) [32])(local_b28 + uVar14) = auVar8;
        *(undefined1 (*) [32])(auStack_b08 + uVar14 * 4) = auVar9;
        *(undefined1 (*) [32])(auStack_ae8 + uVar14 * 4) = auVar6;
        *(undefined1 (*) [32])(auStack_ac8 + uVar14 * 4) = auVar7;
        uVar14 = uVar14 + 0x20;
      } while (uVar11 != uVar14);
    }
    do {
      local_b28[uVar11] = *(float *)(param_2 + uVar11 * 8);
      uVar11 = uVar11 + 1;
    } while (uVar10 != uVar11);
  }
  uVar10 = param_4 >> 2;
  local_1138 = uVar10;
  rnn_pitch_xcorr(local_468,local_b28,local_1128);
  if ((int)uVar15 < 1) {
    auVar25 = ZEXT464(DAT_18000f098);
  }
  else {
    if (uVar15 < 8) {
      auVar25 = ZEXT464(DAT_18000f098);
      uVar11 = 0;
    }
    else {
      auVar25 = ZEXT464(DAT_18000f098);
      uVar11 = 0;
      do {
        auVar25 = ZEXT464((uint)(*(float *)(auStack_b08 + uVar11 * 4 + -4) *
                                 *(float *)(auStack_b08 + uVar11 * 4 + -4) +
                                local_b28[uVar11 + 6] * local_b28[uVar11 + 6] +
                                local_b28[uVar11 + 5] * local_b28[uVar11 + 5] +
                                local_b28[uVar11 + 4] * local_b28[uVar11 + 4] +
                                local_b28[uVar11 + 3] * local_b28[uVar11 + 3] +
                                local_b28[uVar11 + 2] * local_b28[uVar11 + 2] +
                                local_b28[uVar11 + 1] * local_b28[uVar11 + 1] +
                                local_b28[uVar11] * local_b28[uVar11] + auVar25._0_4_));
        uVar11 = uVar11 + 8;
      } while ((uVar15 & 0x7ffffff8) != uVar11);
    }
    if ((ulonglong)(uVar15 & 7) != 0) {
      uVar14 = 0;
      do {
        auVar25._0_4_ = local_b28[uVar11 + uVar14] * local_b28[uVar11 + uVar14] + auVar25._0_4_;
        auVar25 = ZEXT1664(auVar25._0_16_);
        uVar14 = uVar14 + 1;
      } while ((uVar15 & 7) != uVar14);
    }
  }
  uVar11 = 0;
  iVar13 = 0;
  if ((int)uVar10 < 1) {
    iVar12 = 2;
  }
  else {
    auVar27 = ZEXT464((uint)DAT_18000f1f0);
    uVar14 = 1;
    uVar20 = 0;
    auVar30 = ZEXT1264(ZEXT812(0));
    auVar31 = ZEXT1264(ZEXT812(0));
    auVar32 = ZEXT464((uint)DAT_18000f1f0);
    uVar18 = 0;
    do {
      fVar23 = auVar25._0_4_;
      uVar19 = uVar18;
      if ((0.0 < local_1128[uVar20]) &&
         (fVar28 = local_1128[uVar20] * DAT_18000f284, fVar28 = fVar28 * fVar28,
         auVar32._0_4_ * fVar23 < fVar28 * auVar31._0_4_)) {
        fVar4 = fVar28 * auVar30._0_4_;
        fVar24 = auVar27._0_4_ * fVar23;
        uVar14 = uVar20 & 0xffffffff;
        if (fVar24 < fVar4) {
          uVar11 = uVar20 & 0xffffffff;
          uVar19 = uVar20 & 0xffffffff;
          uVar14 = uVar18;
        }
        auVar26 = auVar25._0_16_;
        auVar29 = auVar27._0_16_;
        if (fVar24 < fVar4) {
          auVar29 = auVar30._0_16_;
          auVar30 = ZEXT1664(auVar26);
          auVar26 = auVar29;
          auVar29 = ZEXT416((uint)fVar28);
        }
        auVar31 = ZEXT1664(auVar26);
        auVar26 = vcmpss_avx(ZEXT416((uint)fVar24),ZEXT416((uint)fVar4),1);
        auVar26 = vblendvps_avx(ZEXT416((uint)fVar28),auVar27._0_16_,auVar26);
        auVar32 = ZEXT1664(auVar26);
        auVar27 = ZEXT1664(auVar29);
      }
      auVar26 = vmaxss_avx(ZEXT416(DAT_18000f098),
                           ZEXT416((uint)((local_b28[(longlong)(int)uVar15 + uVar20] *
                                           local_b28[(longlong)(int)uVar15 + uVar20] -
                                          local_b28[uVar20] * local_b28[uVar20]) + fVar23)));
      auVar25 = ZEXT1664(auVar26);
      uVar20 = uVar20 + 1;
      uVar18 = uVar19;
    } while (uVar10 != uVar20);
    iVar13 = (int)uVar11 * 2;
    iVar12 = (int)uVar14 * 2;
  }
  uVar10 = param_4 >> 1;
  local_1130 = (ulonglong)uVar10;
  uVar15 = param_3 >> 1;
  if (0 < (int)uVar10) {
    lVar21 = param_2 + 0xc;
    uVar11 = 0;
    lVar22 = param_2;
    do {
      local_1128[uVar11] = 0.0;
      uVar17 = (int)uVar11 - iVar13;
      uVar16 = -uVar17;
      if ((int)uVar16 < 0) {
        uVar16 = uVar17;
      }
      if (uVar16 < 3) {
LAB_18000c8aa:
        auVar26._0_12_ = ZEXT812(0);
        auVar26._12_4_ = 0;
        auVar25 = ZEXT1664(auVar26);
        if (0 < (int)uVar15) {
          uVar14 = 0;
          if (3 < uVar15) {
            do {
              auVar25 = ZEXT464((uint)(*(float *)(param_1 + 0xc + uVar14 * 4) *
                                       *(float *)(lVar21 + uVar14 * 4) +
                                      *(float *)(param_1 + 8 + uVar14 * 4) *
                                      *(float *)(lVar21 + -4 + uVar14 * 4) +
                                      *(float *)(param_1 + 4 + uVar14 * 4) *
                                      *(float *)(lVar21 + -8 + uVar14 * 4) +
                                      *(float *)(param_1 + uVar14 * 4) *
                                      *(float *)(lVar21 + -0xc + uVar14 * 4) + auVar25._0_4_));
              uVar14 = uVar14 + 4;
            } while ((uVar15 & 0x7ffffffc) != uVar14);
          }
          auVar26 = auVar25._0_16_;
          if ((ulonglong)(uVar15 & 3) != 0) {
            uVar18 = 0;
            do {
              auVar27._4_60_ = auVar25._4_60_;
              auVar27._0_4_ =
                   *(float *)(param_1 + uVar14 * 4 + uVar18 * 4) *
                   *(float *)(lVar22 + uVar14 * 4 + uVar18 * 4) + auVar25._0_4_;
              auVar26 = auVar27._0_16_;
              auVar25 = ZEXT1664(auVar26);
              uVar18 = uVar18 + 1;
            } while ((uVar15 & 3) != uVar18);
          }
        }
        auVar26 = vmaxss_avx(ZEXT416((uint)DAT_18000f1f0),auVar26);
        local_1128[uVar11] = auVar26._0_4_;
      }
      else {
        uVar17 = (int)uVar11 - iVar12;
        uVar16 = -uVar17;
        if ((int)uVar16 < 0) {
          uVar16 = uVar17;
        }
        if (uVar16 < 3) goto LAB_18000c8aa;
      }
      uVar11 = uVar11 + 1;
      lVar21 = lVar21 + 4;
      lVar22 = lVar22 + 4;
    } while (uVar11 != local_1130);
  }
  if ((int)uVar15 < 1) {
    auVar25 = ZEXT464(DAT_18000f098);
  }
  else {
    if (uVar15 < 8) {
      auVar25 = ZEXT464(DAT_18000f098);
      uVar11 = 0;
    }
    else {
      auVar25 = ZEXT464(DAT_18000f098);
      uVar11 = 0;
      do {
        fVar23 = *(float *)(param_2 + uVar11 * 4);
        fVar28 = *(float *)(param_2 + 4 + uVar11 * 4);
        fVar4 = *(float *)(param_2 + 8 + uVar11 * 4);
        fVar24 = *(float *)(param_2 + 0xc + uVar11 * 4);
        fVar5 = *(float *)(param_2 + 0x10 + uVar11 * 4);
        fVar1 = *(float *)(param_2 + 0x14 + uVar11 * 4);
        fVar2 = *(float *)(param_2 + 0x18 + uVar11 * 4);
        fVar3 = *(float *)(param_2 + 0x1c + uVar11 * 4);
        auVar25 = ZEXT464((uint)(fVar3 * fVar3 +
                                fVar2 * fVar2 +
                                fVar1 * fVar1 +
                                fVar5 * fVar5 +
                                fVar24 * fVar24 +
                                fVar4 * fVar4 + fVar28 * fVar28 + fVar23 * fVar23 + auVar25._0_4_));
        uVar11 = uVar11 + 8;
      } while ((uVar15 & 0x7ffffff8) != uVar11);
    }
    if ((ulonglong)(uVar15 & 7) != 0) {
      uVar14 = 0;
      do {
        fVar23 = *(float *)(param_2 + uVar11 * 4 + uVar14 * 4);
        auVar30._4_60_ = auVar25._4_60_;
        auVar30._0_4_ = fVar23 * fVar23 + auVar25._0_4_;
        auVar25 = ZEXT1664(auVar30._0_16_);
        uVar14 = uVar14 + 1;
      } while ((uVar15 & 7) != uVar14);
    }
  }
  if ((int)uVar10 < 1) {
    uVar11 = 0;
  }
  else {
    uVar11 = 0;
    uVar14 = 0;
    auVar27 = ZEXT1264(ZEXT812(0));
    auVar30 = ZEXT1264(ZEXT812(0));
    fVar23 = DAT_18000f1f0;
    fVar28 = DAT_18000f1f0;
    do {
      fVar24 = auVar25._0_4_;
      fVar4 = fVar23;
      if ((0.0 < local_1128[uVar14]) &&
         (fVar5 = local_1128[uVar14] * DAT_18000f284, fVar5 = fVar5 * fVar5,
         fVar28 * fVar24 < fVar5 * auVar30._0_4_)) {
        if (fVar5 * auVar27._0_4_ <= fVar23 * fVar24) {
          auVar30 = ZEXT1664(auVar25._0_16_);
          fVar28 = fVar5;
        }
        else {
          uVar11 = uVar14 & 0xffffffff;
          auVar30 = ZEXT1664(auVar27._0_16_);
          auVar27 = ZEXT1664(auVar25._0_16_);
          fVar4 = fVar5;
          fVar28 = fVar23;
        }
      }
      fVar23 = fVar4;
      fVar4 = *(float *)(param_2 + (longlong)(int)uVar15 * 4 + uVar14 * 4);
      fVar5 = *(float *)(param_2 + uVar14 * 4);
      auVar26 = vmaxss_avx(ZEXT416(DAT_18000f098),
                           ZEXT416((uint)((fVar4 * fVar4 - fVar5 * fVar5) + fVar24)));
      auVar25 = ZEXT1664(auVar26);
      uVar14 = uVar14 + 1;
    } while (local_1130 != uVar14);
    iVar13 = (int)uVar11;
    if (iVar13 < (int)(uVar10 - 1) && 0 < iVar13) {
      fVar23 = local_1128[iVar13 - 1];
      fVar28 = local_1128[uVar11 + 1];
      uVar15 = 0xffffffff;
      if (fVar28 - fVar23 <= (local_1128[uVar11] - fVar23) * DAT_18000f288) {
        uVar15 = (uint)((local_1128[uVar11] - fVar28) * DAT_18000f288 < fVar23 - fVar28);
      }
      goto LAB_18000cb49;
    }
  }
  uVar15 = 0;
LAB_18000cb49:
  *param_5 = uVar15 + (int)uVar11 * 2;
  if ((local_a0 ^ (ulonglong)local_1158) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 18000cbb0
   NAME : rnn_remove_doubling
   SIG  : undefined rnn_remove_doubling(void)
   ======================================================================== */

undefined8
rnn_remove_doubling(longlong param_1,uint param_2,uint param_3,int param_4,uint *param_5,int param_6
                   ,float param_7)

{
  longlong lVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  uint uVar5;
  int iVar6;
  longlong lVar7;
  uint uVar8;
  uint uVar9;
  longlong lVar10;
  ulonglong uVar11;
  float *pfVar12;
  uint uVar13;
  ulonglong uVar14;
  longlong lVar15;
  ulonglong uVar16;
  longlong lVar17;
  float fVar18;
  float fVar19;
  float fVar20;
  double dVar21;
  undefined1 auVar22 [16];
  undefined1 auVar23 [16];
  undefined1 auVar24 [16];
  uint uVar29;
  undefined1 auVar25 [16];
  uint uVar30;
  undefined1 auVar26 [16];
  undefined1 auVar27 [16];
  undefined1 auVar28 [64];
  undefined1 auVar31 [64];
  undefined8 uVar33;
  undefined1 auVar32 [64];
  double dVar34;
  undefined1 auVar35 [16];
  undefined1 auVar36 [64];
  undefined1 auVar37 [64];
  uint uVar38;
  uint uVar39;
  double dVar40;
  undefined1 auVar41 [64];
  undefined1 auStack_d78 [44];
  uint local_d4c;
  float local_d48;
  int local_d44;
  ulonglong local_d40;
  ulonglong local_d38;
  uint local_d30;
  float local_d2c;
  longlong local_d28;
  longlong local_d20;
  longlong local_d18;
  ulonglong local_d10;
  ulonglong local_d08;
  float *local_d00;
  float local_cf8 [770];
  ulonglong local_f0;
  
                    /* 0xcbb0  30  rnn_remove_doubling */
  local_f0 = DAT_180580000 ^ (ulonglong)auStack_d78;
  local_d4c = (int)param_2 / 2;
  uVar8 = param_4 / 2;
  lVar17 = (longlong)(int)local_d4c;
  uVar13 = local_d4c - 1;
  if ((int)*param_5 / 2 < (int)local_d4c) {
    uVar13 = (int)*param_5 / 2;
  }
  local_d38 = (ulonglong)uVar13;
  *param_5 = uVar13;
  lVar7 = (longlong)(int)uVar13;
  auVar37 = ZEXT1264(ZEXT812(0));
  uVar11 = (ulonglong)uVar8;
  auVar32 = ZEXT1264(ZEXT812(0));
  if (1 < param_4) {
    if (uVar11 - 1 < 3) {
      uVar14 = 0;
    }
    else {
      lVar10 = param_1 + lVar17 * 4;
      lVar15 = lVar10 + 0xc + lVar7 * -4;
      uVar14 = 0;
      do {
        fVar20 = *(float *)(lVar10 + uVar14 * 4);
        fVar18 = *(float *)(lVar10 + 4 + uVar14 * 4);
        auVar28._0_4_ = *(float *)(lVar10 + 8 + uVar14 * 4);
        auVar31._0_4_ = *(float *)(lVar10 + 0xc + uVar14 * 4);
        auVar37 = ZEXT1664(CONCAT124(auVar37._4_12_,
                                     auVar31._0_4_ * auVar31._0_4_ +
                                     auVar28._0_4_ * auVar28._0_4_ +
                                     fVar18 * fVar18 + fVar20 * fVar20 + auVar37._0_4_));
        auVar32 = ZEXT1664(CONCAT124(auVar32._4_12_,
                                     auVar31._0_4_ * *(float *)(lVar15 + uVar14 * 4) +
                                     auVar28._0_4_ * *(float *)(lVar15 + -4 + uVar14 * 4) +
                                     fVar18 * *(float *)(lVar15 + -8 + uVar14 * 4) +
                                     fVar20 * *(float *)(lVar15 + -0xc + uVar14 * 4) + auVar32._0_4_
                                    ));
        uVar14 = uVar14 + 4;
      } while ((uVar8 & 0x3ffffffc) != uVar14);
    }
    if ((ulonglong)(uVar8 & 3) != 0) {
      lVar10 = lVar17 * 4 + uVar14 * 4;
      uVar14 = 0;
      do {
        fVar20 = *(float *)(param_1 + lVar10 + uVar14 * 4);
        auVar37._0_4_ = fVar20 * fVar20 + auVar37._0_4_;
        auVar37 = ZEXT1664(auVar37._0_16_);
        auVar32._0_4_ =
             fVar20 * *(float *)(lVar10 + lVar7 * -4 + param_1 + uVar14 * 4) + auVar32._0_4_;
        auVar32 = ZEXT1664(auVar32._0_16_);
        uVar14 = uVar14 + 1;
      } while ((uVar8 & 3) != uVar14);
    }
  }
  local_d18 = param_1 + lVar17 * 4;
  fVar20 = auVar37._0_4_;
  local_cf8[0] = fVar20;
  if (1 < (int)param_2) {
    lVar10 = (longlong)(int)uVar8;
    lVar15 = 1;
    auVar22 = auVar37._0_16_;
    fVar18 = fVar20;
    if ((param_2 & 0x7ffffffe) != 2) {
      pfVar12 = (float *)(param_1 + lVar17 * 4 + -4);
      auVar25._0_12_ = ZEXT812(0);
      auVar25._12_4_ = 0;
      do {
        fVar18 = -(pfVar12[lVar10] * pfVar12[lVar10]) + *pfVar12 * *pfVar12 + auVar22._0_4_;
        auVar22 = vmaxss_avx(auVar25,ZEXT416((uint)fVar18));
        local_cf8[lVar15] = auVar22._0_4_;
        fVar18 = -(pfVar12[lVar10 + -1] * pfVar12[lVar10 + -1]) + pfVar12[-1] * pfVar12[-1] + fVar18
        ;
        auVar22 = ZEXT416((uint)fVar18);
        auVar23 = vmaxss_avx(auVar25,auVar22);
        local_cf8[lVar15 + 1] = auVar23._0_4_;
        pfVar12 = pfVar12 + -2;
        lVar1 = lVar15 - (ulonglong)(local_d4c & 0x3ffffffe);
        lVar15 = lVar15 + 2;
      } while (lVar1 != -1);
    }
    if ((local_d4c & 1) != 0) {
      auVar28._0_4_ = *(float *)(local_d18 + lVar15 * -4);
      auVar31._0_4_ = *(float *)(local_d18 + (lVar10 - lVar15) * 4);
      auVar22._0_12_ = ZEXT812(0);
      auVar22._12_4_ = 0;
      auVar22 = vmaxss_avx(auVar22,ZEXT416((uint)(-(auVar31._0_4_ * auVar31._0_4_) +
                                                 auVar28._0_4_ * auVar28._0_4_ + fVar18)));
      local_cf8[lVar15] = auVar22._0_4_;
    }
  }
  uVar13 = 0;
  uVar38 = 0;
  uVar39 = 0;
  fVar18 = local_cf8[lVar7];
  dVar34 = (double)auVar32._0_4_;
  uVar33 = auVar32._8_8_;
  dVar21 = (double)(fVar20 * fVar18 + DAT_18000f098);
  dVar40 = 0.0;
  local_d44 = param_4;
  local_d30 = param_3;
  if (dVar21 < 0.0) {
    dVar21 = sqrt(dVar21);
  }
  else {
    dVar21 = SQRT(dVar21);
  }
  auVar23._0_8_ = dVar34 / dVar21;
  auVar23._8_8_ = uVar33;
  local_d48 = (float)auVar23._0_8_;
  auVar41 = ZEXT1664(CONCAT124(auVar23._4_12_,local_d48));
  iVar2 = (int)local_d38 * 2;
  local_d2c = param_7 * DAT_18000f1d8;
  local_d40 = uVar11 - 1;
  local_d00 = (float *)(param_1 + lVar17 * 4 + 0xc);
  lVar7 = 2;
  auVar36 = ZEXT464((uint)DAT_18000f098);
  local_d10 = local_d38;
  fVar20 = DAT_18000f1d8;
  local_d28 = lVar17;
  local_d20 = param_1;
  local_d08 = uVar11;
  do {
    auVar35 = auVar36._0_16_;
    auVar22 = auVar32._0_16_;
    iVar6 = (int)lVar7;
    uVar11 = (longlong)(iVar2 + iVar6) / (longlong)(iVar6 * 2);
    iVar3 = (int)uVar11;
    if (iVar3 < (int)param_3 / 2) break;
    if (lVar7 == 2) {
      iVar4 = iVar3 + (int)local_d38;
      if ((int)local_d4c < iVar4) {
        iVar4 = (int)local_d38;
      }
    }
    else {
      iVar4 = (*(int *)(&DAT_18000f210 + lVar7 * 4) * iVar2 + iVar6) / (iVar6 * 2);
    }
    auVar28._0_4_ = 0.0;
    lVar10 = (longlong)iVar3;
    lVar17 = (longlong)iVar4;
    if (1 < local_d44) {
      if (local_d40 < 3) {
        auVar31._0_4_ = 0.0;
        uVar14 = 0;
        auVar28._0_4_ = 0.0;
      }
      else {
        auVar31._0_4_ = 0.0;
        uVar14 = 0;
        auVar28._0_4_ = 0.0;
        pfVar12 = local_d00;
        do {
          auVar28._0_4_ =
               *pfVar12 * pfVar12[-lVar10] +
               pfVar12[-1] * pfVar12[-1 - lVar10] +
               pfVar12[-2] * pfVar12[-2 - lVar10] +
               pfVar12[-3] * pfVar12[-3 - lVar10] + auVar28._0_4_;
          auVar31._0_4_ =
               *pfVar12 * pfVar12[-lVar17] +
               pfVar12[-1] * pfVar12[-1 - lVar17] +
               pfVar12[-2] * pfVar12[-2 - lVar17] +
               pfVar12[-3] * pfVar12[-3 - lVar17] + auVar31._0_4_;
          uVar14 = uVar14 + 4;
          pfVar12 = pfVar12 + 4;
        } while ((uVar8 & 0x3ffffffc) != uVar14);
      }
      if ((ulonglong)(uVar8 & 3) != 0) {
        uVar16 = 0;
        do {
          fVar19 = *(float *)(uVar14 * 4 + local_d18 + uVar16 * 4);
          auVar28._0_4_ =
               fVar19 * *(float *)(uVar14 * 4 + lVar10 * -4 + local_d18 + uVar16 * 4) +
               auVar28._0_4_;
          auVar31._0_4_ =
               fVar19 * *(float *)(uVar14 * 4 + lVar17 * -4 + local_d18 + uVar16 * 4) +
               auVar31._0_4_;
          uVar16 = uVar16 + 1;
        } while ((uVar8 & 3) != uVar16);
      }
      auVar28._0_4_ = auVar28._0_4_ + auVar31._0_4_;
    }
    auVar31._0_4_ = fVar20 * (local_cf8[lVar10] + local_cf8[lVar17]);
    dVar21 = (double)(auVar37._0_4_ * auVar31._0_4_ + auVar36._0_4_);
    if (dVar21 < dVar40) {
      dVar21 = sqrt(dVar21);
    }
    else {
      dVar21 = SQRT(dVar21);
    }
    auVar35 = auVar36._0_16_;
    uVar5 = iVar3 - param_6 / 2;
    uVar9 = -uVar5;
    if ((int)uVar9 < 0) {
      uVar9 = uVar5;
    }
    fVar19 = param_7;
    uVar5 = uVar13;
    uVar29 = uVar38;
    uVar30 = uVar39;
    if (1 < uVar9) {
      fVar19 = 0.0;
      uVar5 = 0;
      uVar29 = 0;
      uVar30 = 0;
      if (uVar9 == 2) {
        uVar5 = 0;
        uVar29 = 0;
        uVar30 = 0;
        fVar19 = local_d2c;
        if ((int)local_d38 <= iVar6 * iVar6 * 5) {
          fVar19 = 0.0;
          uVar5 = 0;
          uVar29 = 0;
          uVar30 = 0;
        }
      }
    }
    auVar26._0_4_ = (float)((uint)fVar19 ^ DAT_18000f000);
    auVar26._4_4_ = uVar5 ^ DAT_18000f000;
    auVar26._8_4_ = uVar29 ^ DAT_18000f000;
    auVar26._12_4_ = uVar30 ^ DAT_18000f000;
    fVar19 = DAT_18000f288;
    uVar9 = DAT_18000f28c;
    if (iVar3 < ((int)param_3 / 2) * 3) {
      fVar19 = DAT_18000f290;
      uVar9 = DAT_18000f294;
    }
    auVar27._0_4_ = auVar41._0_4_ * fVar19 + auVar26._0_4_;
    auVar27._4_12_ = auVar26._4_12_;
    auVar22 = vmaxss_avx(ZEXT416(uVar9),auVar27);
    fVar19 = (float)((double)(auVar28._0_4_ * fVar20) / dVar21);
    if (auVar22._0_4_ < fVar19) {
      auVar32 = ZEXT464((uint)(auVar28._0_4_ * fVar20));
      fVar18 = auVar31._0_4_;
      local_d48 = fVar19;
      local_d10 = uVar11 & 0xffffffff;
    }
    auVar22 = auVar32._0_16_;
    lVar7 = lVar7 + 1;
  } while (lVar7 != 0x10);
  fVar20 = 0.0;
  auVar28._0_4_ = 0.0;
  auVar31._0_4_ = 0.0;
  iVar2 = (int)local_d10;
  if (1 < local_d44) {
    uVar13 = (uint)local_d08;
    if (local_d40 < 3) {
      uVar11 = 0;
    }
    else {
      lVar17 = local_d20 + local_d28 * 4;
      lVar7 = lVar17 + 0xc + (longlong)(iVar2 + -1) * -4;
      uVar11 = 0;
      do {
        fVar20 = *(float *)(lVar17 + 0xc + uVar11 * 4) * *(float *)(lVar7 + uVar11 * 4) +
                 *(float *)(lVar17 + 8 + uVar11 * 4) * *(float *)(lVar7 + -4 + uVar11 * 4) +
                 *(float *)(lVar17 + 4 + uVar11 * 4) * *(float *)(lVar7 + -8 + uVar11 * 4) +
                 *(float *)(lVar17 + uVar11 * 4) * *(float *)(lVar7 + -0xc + uVar11 * 4) + fVar20;
        uVar11 = uVar11 + 4;
      } while ((uVar13 & 0x3ffffffc) != uVar11);
    }
    if ((ulonglong)(uVar13 & 3) != 0) {
      lVar17 = local_d28 * 4 + uVar11 * 4;
      uVar11 = 0;
      do {
        fVar20 = *(float *)(local_d20 + lVar17 + uVar11 * 4) *
                 *(float *)(lVar17 + (longlong)(iVar2 + -1) * -4 + local_d20 + uVar11 * 4) + fVar20;
        uVar11 = uVar11 + 1;
      } while ((uVar13 & 3) != uVar11);
    }
    if (local_d40 < 3) {
      auVar32 = ZEXT864(0);
      uVar11 = 0;
    }
    else {
      lVar17 = local_d20 + local_d28 * 4;
      lVar7 = lVar17 + 0xc + (longlong)iVar2 * -4;
      auVar32 = ZEXT864(0);
      uVar11 = 0;
      do {
        auVar32 = ZEXT464((uint)(*(float *)(lVar17 + 0xc + uVar11 * 4) *
                                 *(float *)(lVar7 + uVar11 * 4) +
                                *(float *)(lVar17 + 8 + uVar11 * 4) *
                                *(float *)(lVar7 + -4 + uVar11 * 4) +
                                *(float *)(lVar17 + 4 + uVar11 * 4) *
                                *(float *)(lVar7 + -8 + uVar11 * 4) +
                                *(float *)(lVar17 + uVar11 * 4) *
                                *(float *)(lVar7 + -0xc + uVar11 * 4) + auVar32._0_4_));
        uVar11 = uVar11 + 4;
      } while ((uVar13 & 0x3ffffffc) != uVar11);
    }
    auVar28._0_4_ = auVar32._0_4_;
    if ((ulonglong)(uVar13 & 3) != 0) {
      lVar17 = local_d28 * 4 + uVar11 * 4;
      uVar11 = 0;
      do {
        auVar28._4_60_ = auVar32._4_60_;
        auVar28._0_4_ =
             *(float *)(local_d20 + lVar17 + uVar11 * 4) *
             *(float *)(lVar17 + (longlong)iVar2 * -4 + local_d20 + uVar11 * 4) + auVar32._0_4_;
        auVar32 = ZEXT1664(auVar28._0_16_);
        uVar11 = uVar11 + 1;
      } while ((uVar13 & 3) != uVar11);
    }
    if (local_d40 < 3) {
      auVar32 = ZEXT864(0);
      uVar11 = 0;
    }
    else {
      lVar17 = local_d20 + local_d28 * 4;
      lVar7 = lVar17 + 0xc + (longlong)(iVar2 + 1) * -4;
      auVar32 = ZEXT864(0);
      uVar11 = 0;
      do {
        auVar32 = ZEXT464((uint)(*(float *)(lVar17 + 0xc + uVar11 * 4) *
                                 *(float *)(lVar7 + uVar11 * 4) +
                                *(float *)(lVar17 + 8 + uVar11 * 4) *
                                *(float *)(lVar7 + -4 + uVar11 * 4) +
                                *(float *)(lVar17 + 4 + uVar11 * 4) *
                                *(float *)(lVar7 + -8 + uVar11 * 4) +
                                *(float *)(lVar17 + uVar11 * 4) *
                                *(float *)(lVar7 + -0xc + uVar11 * 4) + auVar32._0_4_));
        uVar11 = uVar11 + 4;
      } while ((uVar13 & 0x3ffffffc) != uVar11);
    }
    auVar31._0_4_ = auVar32._0_4_;
    if ((ulonglong)(uVar13 & 3) != 0) {
      lVar17 = local_d28 * 4 + uVar11 * 4;
      uVar11 = 0;
      do {
        auVar31._4_60_ = auVar32._4_60_;
        auVar31._0_4_ =
             *(float *)(local_d20 + lVar17 + uVar11 * 4) *
             *(float *)(lVar17 + (longlong)(iVar2 + 1) * -4 + local_d20 + uVar11 * 4) +
             auVar32._0_4_;
        auVar32 = ZEXT1664(auVar31._0_16_);
        uVar11 = uVar11 + 1;
      } while ((uVar13 & 3) != uVar11);
    }
  }
  iVar6 = 1;
  if (auVar31._0_4_ - fVar20 <= (auVar28._0_4_ - fVar20) * DAT_18000f288) {
    iVar6 = -(uint)((auVar28._0_4_ - auVar31._0_4_) * DAT_18000f288 < fVar20 - auVar31._0_4_);
  }
  auVar24._0_12_ = ZEXT812(0);
  auVar24._12_4_ = 0;
  auVar22 = vmaxss_avx(auVar24,auVar22);
  if (auVar22._0_4_ < fVar18) {
    auVar35._0_4_ = auVar22._0_4_ / (fVar18 + auVar35._0_4_);
    auVar35._4_12_ = auVar22._4_12_;
  }
  auVar22 = vminss_avx(ZEXT416((uint)local_d48),auVar35);
  uVar8 = iVar6 + iVar2 * 2;
  uVar13 = local_d30;
  if ((int)local_d30 < (int)uVar8) {
    uVar13 = uVar8;
  }
  *param_5 = uVar13;
  if ((local_f0 ^ (ulonglong)auStack_d78) != DAT_180580000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000db80();
  }
  return auVar22._0_8_;
}



/* ========================================================================
   ENTRY: 18000d530
   NAME : compute_rnn
   SIG  : undefined compute_rnn(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void compute_rnn(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4,
                undefined8 param_5,undefined4 param_6)

{
  void *_Src;
  void *_Src_00;
  undefined1 auStack_2888 [32];
  undefined4 local_2868;
  undefined4 local_2860;
  undefined4 local_2858;
  undefined1 local_2848 [1536];
  undefined1 local_2248 [1536];
  undefined1 local_1c48 [1536];
  undefined1 local_1648 [1536];
  undefined1 local_1048 [4096];
  ulonglong local_48;
  
                    /* 0xd530  1  compute_rnn */
  local_48 = DAT_180580000 ^ (ulonglong)auStack_2888;
  local_2858 = param_6;
  local_2860 = 2;
  local_2868 = 0x41;
  rnn_compute_generic_conv1d(param_1,local_1048,param_2,param_5);
  local_2858 = param_6;
  local_2860 = 2;
  local_2868 = 0x80;
  rnn_compute_generic_conv1d(param_1 + 0x40,local_2848,param_2 + 0x208,local_1048);
  _Src = (void *)(param_2 + 0x608);
  local_2868 = param_6;
  rnn_compute_generic_gru(param_1 + 0x80,param_1 + 0xc0,_Src,local_2848);
  _Src_00 = (void *)(param_2 + 0xc08);
  local_2868 = param_6;
  rnn_compute_generic_gru(param_1 + 0x100,param_1 + 0x140,_Src_00,_Src);
  local_2868 = param_6;
  rnn_compute_generic_gru(param_1 + 0x180,param_1 + 0x1c0,(void *)(param_2 + 0x1208),_Src_00);
  memcpy(local_2248,_Src,0x600);
  memcpy(local_1c48,_Src_00,0x600);
  memcpy(local_1648,(void *)(param_2 + 0x1208),0x600);
  local_2868 = param_6;
  rnn_compute_generic_dense(param_1 + 0x200,param_3,local_2848,1);
  local_2868 = param_6;
  rnn_compute_generic_dense(param_1 + 0x240,param_4,local_2848,1);
  if ((local_48 ^ (ulonglong)auStack_2888) == DAT_180580000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000db80();
}



/* ========================================================================
   ENTRY: 18000d6f0
   NAME : init_rnnoise
   SIG  : undefined init_rnnoise(void)
   ======================================================================== */

bool init_rnnoise(longlong param_1,undefined8 param_2)

{
  int iVar1;
  bool bVar2;
  undefined1 auVar3 [16];
  undefined1 auVar4 [16];
  undefined1 auVar5 [64];
  
  auVar3._0_12_ = ZEXT812(0);
  auVar3._12_4_ = 0;
                    /* 0xd6f0  2  init_rnnoise */
  auVar5 = ZEXT1664(auVar3);
  iVar1 = rnn_linear_init(param_1,param_2,"conv1_bias",0,0,"conv1_weights_float",auVar3,0,0xc3,0x80)
  ;
  bVar2 = true;
  if (iVar1 == 0) {
    iVar1 = rnn_linear_init(param_1 + 0x40,param_2,"conv2_bias","conv2_subias","conv2_weights_int8",
                            "conv2_weights_float",auVar5._0_16_,"conv2_scale",0x180,0x180);
    if (iVar1 == 0) {
      iVar1 = rnn_linear_init(param_1 + 0x80,param_2,"gru1_input_bias","gru1_input_subias",
                              "gru1_input_weights_int8","gru1_input_weights_float",
                              "gru1_input_weights_idx",0,"gru1_input_scale",0x180,0x480);
      if (iVar1 == 0) {
        iVar1 = rnn_linear_init(param_1 + 0xc0,param_2,"gru1_recurrent_bias","gru1_recurrent_subias"
                                ,"gru1_recurrent_weights_int8","gru1_recurrent_weights_float",
                                "gru1_recurrent_weights_idx","gru1_recurrent_weights_diag",
                                "gru1_recurrent_scale",0x180,0x480);
        if (iVar1 == 0) {
          iVar1 = rnn_linear_init(param_1 + 0x100,param_2,"gru2_input_bias","gru2_input_subias",
                                  "gru2_input_weights_int8","gru2_input_weights_float",
                                  "gru2_input_weights_idx",0,"gru2_input_scale",0x180,0x480);
          if (iVar1 == 0) {
            iVar1 = rnn_linear_init(param_1 + 0x140,param_2,"gru2_recurrent_bias",
                                    "gru2_recurrent_subias","gru2_recurrent_weights_int8",
                                    "gru2_recurrent_weights_float","gru2_recurrent_weights_idx",
                                    "gru2_recurrent_weights_diag","gru2_recurrent_scale",0x180,0x480
                                   );
            if (iVar1 == 0) {
              iVar1 = rnn_linear_init(param_1 + 0x180,param_2,"gru3_input_bias","gru3_input_subias",
                                      "gru3_input_weights_int8","gru3_input_weights_float",
                                      "gru3_input_weights_idx",0,"gru3_input_scale",0x180,0x480);
              if (iVar1 == 0) {
                iVar1 = rnn_linear_init(param_1 + 0x1c0,param_2,"gru3_recurrent_bias",
                                        "gru3_recurrent_subias","gru3_recurrent_weights_int8",
                                        "gru3_recurrent_weights_float","gru3_recurrent_weights_idx",
                                        "gru3_recurrent_weights_diag","gru3_recurrent_scale",0x180,
                                        0x480);
                if (iVar1 == 0) {
                  auVar4._0_12_ = ZEXT812(0);
                  auVar4._12_4_ = 0;
                  auVar5 = ZEXT1664(auVar4);
                  iVar1 = rnn_linear_init(param_1 + 0x200,param_2,"dense_out_bias",0,0,
                                          "dense_out_weights_float",auVar4,0,0x600,0x20);
                  if (iVar1 == 0) {
                    iVar1 = rnn_linear_init(param_1 + 0x240,param_2,"vad_dense_bias",0,0,
                                            "vad_dense_weights_float",auVar5._0_16_,0,0x600,1);
                    bVar2 = iVar1 != 0;
                  }
                }
              }
            }
          }
        }
      }
    }
  }
  return bVar2;
}



/* ========================================================================
   ENTRY: 18000db20
   NAME : __chkstk
   SIG  : undefined __chkstk(void)
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
   ENTRY: 18000db80
   NAME : FUN_18000db80
   SIG  : noreturn undefined FUN_18000db80(void)
   ======================================================================== */

void FUN_18000db80(longlong param_1)

{
  if ((param_1 == DAT_180580000) && ((short)((ulonglong)param_1 >> 0x30) == 0)) {
    return;
  }
  FUN_18000dba0(param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000dba0
   NAME : FUN_18000dba0
   SIG  : undefined FUN_18000dba0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000dba0(void)

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
  *(undefined8 *)(puVar3 + -8) = 0x18000dbcb;
  FUN_18000dc74(&DAT_180580120);
  _DAT_180580090 = *(undefined8 *)(puVar3 + 0x38);
  _DAT_1805801b8 = puVar3 + 0x40;
  _DAT_1805801a0 = *(undefined8 *)(puVar3 + 0x40);
  _DAT_180580080 = 0xc0000409;
  _DAT_180580084 = 1;
  _DAT_180580098 = 1;
  DAT_1805800a0 = 2;
  *(undefined8 *)(puVar3 + 0x20) = DAT_180580000;
  *(undefined8 *)(puVar3 + 0x28) = DAT_180580040;
  *(undefined8 *)(puVar3 + -8) = 0x18000dc6d;
  DAT_180580218 = _DAT_180580090;
  __raise_securityfailure(&PTR_DAT_18057e660);
  return;
}



/* ========================================================================
   ENTRY: 18000dc74
   NAME : FUN_18000dc74
   SIG  : undefined FUN_18000dc74(void)
   ======================================================================== */

void FUN_18000dc74(PCONTEXT param_1)

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
   ENTRY: 18000dce8
   NAME : __raise_securityfailure
   SIG  : undefined __raise_securityfailure(void)
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
                    /* WARNING: Could not recover jumptable at 0x00018000dd15. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  TerminateProcess(pvVar1,0xc0000409);
  return;
}



/* ========================================================================
   ENTRY: 18000dd1c
   NAME : FUN_18000dd1c
   SIG  : undefined FUN_18000dd1c(void)
   ======================================================================== */

undefined8 FUN_18000dd1c(undefined8 param_1,undefined8 param_2)

{
  bool bVar1;
  char cVar2;
  undefined1 uVar3;
  int iVar4;
  longlong *plVar5;
  
  cVar2 = FUN_18000e284(0);
  if (cVar2 != '\0') {
    uVar3 = __scrt_acquire_startup_lock();
    bVar1 = true;
    if (DAT_180580620 != 0) {
                    /* WARNING: Subroutine does not return */
      FUN_18000e4bc(7);
    }
    DAT_180580620 = 1;
    cVar2 = FUN_18000e3d8();
    if (cVar2 != '\0') {
      FUN_18000e608();
      FUN_18000e144();
      FUN_18000e160();
      iVar4 = _initterm_e(&DAT_18057ec58,&DAT_18057ec60);
      if ((iVar4 == 0) && (cVar2 = __scrt_dllmain_after_initialize_c(), cVar2 != '\0')) {
        _initterm(&DAT_18057ec48,&DAT_18057ec50);
        DAT_180580620 = 2;
        bVar1 = false;
      }
    }
    __scrt_release_startup_lock(uVar3);
    if (!bVar1) {
      plVar5 = (longlong *)FUN_18000e4a8();
      if ((*plVar5 != 0) && (cVar2 = FUN_18000e18c(plVar5), cVar2 != '\0')) {
        (*(code *)PTR__guard_dispatch_icall_18057ec20)(param_1,2,param_2);
      }
      DAT_1805805f0 = DAT_1805805f0 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000de34
   NAME : FUN_18000de34
   SIG  : undefined FUN_18000de34(void)
   ======================================================================== */

undefined1
FUN_18000de34(undefined1 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined1 uVar1;
  undefined1 uVar2;
  
  if (DAT_1805805f0 < 1) {
    uVar1 = 0;
  }
  else {
    DAT_1805805f0 = DAT_1805805f0 + -1;
    uVar1 = __scrt_acquire_startup_lock();
    if (DAT_180580620 != 2) {
                    /* WARNING: Subroutine does not return */
      FUN_18000e4bc(7);
    }
    uVar2 = uVar1;
    __scrt_dllmain_uninitialize_c();
    FUN_18000e154();
    FUN_18000e644();
    DAT_180580620 = 0;
    __scrt_release_startup_lock(uVar1);
    uVar1 = __scrt_uninitialize_crt(param_1,0,param_3,param_4,uVar2);
    FUN_18000e454();
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18000deb8
   NAME : FUN_18000deb8
   SIG  : undefined FUN_18000deb8(void)
   ======================================================================== */

ulonglong FUN_18000deb8(undefined1 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  ulonglong uVar2;
  
  if (param_2 == 0) {
    uVar2 = FUN_18000de34(param_3 != 0);
    return uVar2;
  }
  if (param_2 != 1) {
    if (param_2 == 2) {
      bVar1 = FUN_18000e468();
    }
    else {
      if (param_2 != 3) {
        return 1;
      }
      bVar1 = FUN_18000e490();
    }
    return (ulonglong)bVar1;
  }
  uVar2 = FUN_18000dd1c(param_1,param_3);
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000df08
   NAME : FUN_18000df08
   SIG  : undefined FUN_18000df08(void)
   ======================================================================== */

int FUN_18000df08(undefined8 param_1,int param_2,longlong param_3)

{
  int iVar1;
  int iVar2;
  
  if ((param_2 == 0) && (DAT_1805805f0 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (DAT_18057e670 == 0) {
      iVar2 = 1;
    }
    else {
      iVar2 = (*(code *)PTR__guard_dispatch_icall_18057ec20)();
    }
    if (iVar2 == 0) {
      return 0;
    }
    iVar2 = FUN_18000deb8(param_1,param_2,param_3);
    if (iVar2 == 0) {
      return 0;
    }
  }
  iVar2 = FUN_18000e120(param_1,param_2,param_3);
  if ((param_2 == 1) && (iVar2 == 0)) {
    FUN_18000e120(param_1,0,param_3);
    FUN_18000de34(param_3 != 0);
    if (DAT_18057e670 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057ec20)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    iVar1 = FUN_18000deb8(param_1,param_2,param_3);
    iVar2 = 0;
    if (iVar1 != 0) {
      if (DAT_18057e670 == 0) {
        iVar2 = 1;
      }
      else {
        iVar2 = (*(code *)PTR__guard_dispatch_icall_18057ec20)(param_1,param_2,param_3);
      }
    }
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 18000e030
   NAME : entry
   SIG  : undefined entry(void)
   ======================================================================== */

void entry(undefined8 param_1,int param_2,undefined8 param_3)

{
  if (param_2 == 1) {
    FUN_18000e070();
  }
  FUN_18000df08(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 18000e070
   NAME : FUN_18000e070
   SIG  : undefined FUN_18000e070(void)
   ======================================================================== */

void FUN_18000e070(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10;
  _FILETIME local_18 [2];
  
  if (DAT_180580000 == 0x2b992ddfa232) {
    local_res8.dwLowDateTime = 0;
    local_res8.dwHighDateTime = 0;
    GetSystemTimeAsFileTime(&local_res8);
    local_18[0] = local_res8;
    DVar1 = GetCurrentThreadId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    DVar1 = GetCurrentProcessId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    QueryPerformanceCounter(&local_res10);
    DAT_180580000 =
         ((ulonglong)local_res10.s.LowPart << 0x20 ^
          CONCAT44(local_res10.s.HighPart,local_res10.s.LowPart) ^ (ulonglong)local_18[0] ^
         (ulonglong)local_18) & 0xffffffffffff;
    if (DAT_180580000 == 0x2b992ddfa232) {
      DAT_180580000 = 0x2b992ddfa233;
    }
  }
  DAT_180580040 = ~DAT_180580000;
  return;
}



/* ========================================================================
   ENTRY: 18000e120
   NAME : FUN_18000e120
   SIG  : undefined FUN_18000e120(void)
   ======================================================================== */

undefined8 FUN_18000e120(HMODULE param_1,int param_2)

{
  if ((param_2 == 1) && (DAT_18057e670 == 0)) {
    DisableThreadLibraryCalls(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000e144
   NAME : FUN_18000e144
   SIG  : undefined FUN_18000e144(void)
   ======================================================================== */

void FUN_18000e144(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000e14b. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_180580600);
  return;
}



/* ========================================================================
   ENTRY: 18000e154
   NAME : FUN_18000e154
   SIG  : undefined FUN_18000e154(void)
   ======================================================================== */

void FUN_18000e154(void)

{
  __std_type_info_destroy_list(&DAT_180580600);
  return;
}



/* ========================================================================
   ENTRY: 18000e160
   NAME : FUN_18000e160
   SIG  : undefined FUN_18000e160(void)
   ======================================================================== */

void FUN_18000e160(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_18000e17c();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_18000e184();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 18000e17c
   NAME : FUN_18000e17c
   SIG  : undefined FUN_18000e17c(void)
   ======================================================================== */

undefined * FUN_18000e17c(void)

{
  return &DAT_180580610;
}



/* ========================================================================
   ENTRY: 18000e184
   NAME : FUN_18000e184
   SIG  : undefined FUN_18000e184(void)
   ======================================================================== */

undefined * FUN_18000e184(void)

{
  return &DAT_180580618;
}



/* ========================================================================
   ENTRY: 18000e18c
   NAME : FUN_18000e18c
   SIG  : undefined FUN_18000e18c(void)
   ======================================================================== */

ulonglong FUN_18000e18c(longlong param_1)

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
           param_1 - 0x180000000U < uVar1)) goto LAB_18000e202;
      }
      lVar4 = 0;
LAB_18000e202:
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
   ENTRY: 18000e224
   NAME : __scrt_acquire_startup_lock
   SIG  : undefined __scrt_acquire_startup_lock(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_acquire_startup_lock
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

undefined8 __scrt_acquire_startup_lock(void)

{
  longlong lVar1;
  int iVar2;
  longlong lVar3;
  undefined8 uVar4;
  bool bVar5;
  
  iVar2 = __scrt_is_ucrt_dll_in_use();
  if (iVar2 == 0) {
LAB_18000e252:
    uVar4 = 0;
  }
  else {
    do {
      lVar3 = 0;
      LOCK();
      bVar5 = DAT_180580628 == 0;
      lVar1 = *(longlong *)((longlong)Self + 8);
      if (!bVar5) {
        lVar3 = DAT_180580628;
        lVar1 = DAT_180580628;
      }
      DAT_180580628 = lVar1;
      UNLOCK();
      if (bVar5) goto LAB_18000e252;
    } while (*(longlong *)((longlong)Self + 8) != lVar3);
    uVar4 = 1;
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000e260
   NAME : __scrt_release_startup_lock
   SIG  : undefined __scrt_release_startup_lock(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_release_startup_lock
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_release_startup_lock(char param_1)

{
  int iVar1;
  
  iVar1 = __scrt_is_ucrt_dll_in_use();
  if ((iVar1 != 0) && (param_1 == '\0')) {
    LOCK();
    DAT_180580628 = 0;
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e284
   NAME : FUN_18000e284
   SIG  : undefined FUN_18000e284(void)
   ======================================================================== */

undefined1 FUN_18000e284(int param_1)

{
  char cVar1;
  
  if (param_1 == 0) {
    DAT_180580630 = 1;
  }
  FUN_18000e684();
  cVar1 = FUN_18000e930();
  if (cVar1 != '\0') {
    cVar1 = FUN_18000e930();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_18000e930(0);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000e2c0
   NAME : __scrt_uninitialize_crt
   SIG  : undefined __scrt_uninitialize_crt(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_uninitialize_crt
   
   Library: Visual Studio 2019 Release */

undefined1 __scrt_uninitialize_crt(undefined1 param_1,char param_2)

{
  if ((DAT_180580630 == '\0') || (param_2 == '\0')) {
    FUN_18000e930();
    FUN_18000e930(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000e2ec
   NAME : FUN_18000e2ec
   SIG  : undefined FUN_18000e2ec(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000e2ec(uint param_1)

{
  int iVar1;
  
  if (DAT_180580631 == '\0') {
    if (1 < param_1) {
                    /* WARNING: Subroutine does not return */
      FUN_18000e4bc(5);
    }
    iVar1 = __scrt_is_ucrt_dll_in_use();
    if ((iVar1 == 0) || (param_1 != 0)) {
      _DAT_180580638 = _DAT_18057e7c0;
      uRam0000000180580640 = _UNK_18057e7c8;
      _DAT_180580648 = 0xffffffffffffffff;
      _DAT_180580650 = _DAT_18057e7c0;
      uRam0000000180580658 = _UNK_18057e7c8;
      _DAT_180580660 = 0xffffffffffffffff;
    }
    else {
      iVar1 = _initialize_onexit_table(&DAT_180580638);
      if ((iVar1 != 0) || (iVar1 = _initialize_onexit_table(&DAT_180580650), iVar1 != 0)) {
        return 0;
      }
    }
    DAT_180580631 = '\x01';
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000e378
   NAME : __scrt_dllmain_exception_filter
   SIG  : undefined __scrt_dllmain_exception_filter(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_exception_filter
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_dllmain_exception_filter
               (undefined8 param_1,int param_2,undefined8 param_3,undefined8 param_4,
               undefined4 param_5,undefined8 param_6)

{
  int iVar1;
  
  iVar1 = __scrt_is_ucrt_dll_in_use();
  if ((iVar1 == 0) && (param_2 == 1)) {
    (*(code *)PTR__guard_dispatch_icall_18057ec20)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 18000e3d8
   NAME : FUN_18000e3d8
   SIG  : undefined FUN_18000e3d8(void)
   ======================================================================== */

bool FUN_18000e3d8(void)

{
  char cVar1;
  
  cVar1 = FUN_18000e2ec(0);
  return cVar1 != '\0';
}



/* ========================================================================
   ENTRY: 18000e3f0
   NAME : __scrt_dllmain_after_initialize_c
   SIG  : undefined __scrt_dllmain_after_initialize_c(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_after_initialize_c
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

undefined4 __scrt_dllmain_after_initialize_c(void)

{
  int iVar1;
  undefined4 uVar2;
  
  iVar1 = __scrt_is_ucrt_dll_in_use();
  if (iVar1 == 0) {
    uVar2 = FUN_18000e91c();
    iVar1 = _configure_narrow_argv(uVar2);
    if (iVar1 != 0) {
      return 0;
    }
    _initialize_narrow_environment();
  }
  else {
    FUN_18000e684();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000e424
   NAME : __scrt_dllmain_uninitialize_c
   SIG  : undefined __scrt_dllmain_uninitialize_c(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_dllmain_uninitialize_c
   
   Libraries: Visual Studio 2015 Release, Visual Studio 2017 Release, Visual Studio 2019 Release */

void __scrt_dllmain_uninitialize_c(void)

{
  int iVar1;
  
  iVar1 = __scrt_is_ucrt_dll_in_use();
  if (iVar1 != 0) {
    _execute_onexit_table(&DAT_180580638);
    return;
  }
  iVar1 = FUN_18000e934();
  if (iVar1 == 0) {
    _cexit();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e454
   NAME : FUN_18000e454
   SIG  : undefined FUN_18000e454(void)
   ======================================================================== */

void FUN_18000e454(void)

{
  FUN_18000e930(0);
  FUN_18000e930();
  return;
}



/* ========================================================================
   ENTRY: 18000e468
   NAME : FUN_18000e468
   SIG  : undefined FUN_18000e468(void)
   ======================================================================== */

undefined1 FUN_18000e468(void)

{
  char cVar1;
  
  cVar1 = FUN_18000e930();
  if (cVar1 != '\0') {
    cVar1 = FUN_18000e930();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_18000e930();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000e490
   NAME : FUN_18000e490
   SIG  : undefined FUN_18000e490(void)
   ======================================================================== */

undefined1 FUN_18000e490(void)

{
  FUN_18000e930();
  FUN_18000e930();
  return 1;
}



/* ========================================================================
   ENTRY: 18000e4a8
   NAME : FUN_18000e4a8
   SIG  : undefined FUN_18000e4a8(void)
   ======================================================================== */

undefined * FUN_18000e4a8(void)

{
  return &DAT_180580668;
}



/* ========================================================================
   ENTRY: 18000e4b0
   NAME : FUN_18000e4b0
   SIG  : undefined FUN_18000e4b0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000e4b0(void)

{
  _DAT_180580670 = 0;
  return;
}



/* ========================================================================
   ENTRY: 18000e4bc
   NAME : FUN_18000e4bc
   SIG  : noreturn undefined FUN_18000e4bc(void)
   ======================================================================== */

void FUN_18000e4bc(undefined4 param_1)

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
  *(undefined8 *)(puVar4 + -8) = 0x18000e4f0;
  FUN_18000e4b0(3);
  *(undefined8 *)(puVar4 + -8) = 0x18000e501;
  memset(local_4d8,0,0x4d0);
  *(undefined8 *)(puVar4 + -8) = 0x18000e50b;
  RtlCaptureContext(local_4d8);
  *(undefined8 *)(puVar4 + -8) = 0x18000e525;
  FunctionEntry = RtlLookupFunctionEntry(local_3e0,&local_res10,(PUNWIND_HISTORY_TABLE)0x0);
  if (FunctionEntry != (PRUNTIME_FUNCTION)0x0) {
    *(undefined8 *)(puVar4 + 0x38) = 0;
    *(undefined1 **)(puVar4 + 0x30) = local_res18;
    *(undefined1 **)(puVar4 + 0x28) = local_res20;
    *(undefined1 **)(puVar4 + 0x20) = local_4d8;
    *(undefined8 *)(puVar4 + -8) = 0x18000e569;
    RtlVirtualUnwind(0,local_res10,local_3e0,FunctionEntry,*(PCONTEXT *)(puVar4 + 0x20),
                     *(PVOID **)(puVar4 + 0x28),*(PDWORD64 *)(puVar4 + 0x30),
                     *(PKNONVOLATILE_CONTEXT_POINTERS *)(puVar4 + 0x38));
  }
  local_440 = &stack0x00000008;
  *(undefined8 *)(puVar4 + -8) = 0x18000e59b;
  memset(puVar4 + 0x50,0,0x98);
  *(undefined8 *)(puVar4 + 0x60) = unaff_retaddr;
  *(undefined4 *)(puVar4 + 0x50) = 0x40000015;
  *(undefined4 *)(puVar4 + 0x54) = 1;
  *(undefined8 *)(puVar4 + -8) = 0x18000e5bd;
  BVar2 = IsDebuggerPresent();
  *(undefined1 **)(puVar4 + 0x40) = puVar4 + 0x50;
  *(undefined1 **)(puVar4 + 0x48) = local_4d8;
  *(undefined8 *)(puVar4 + -8) = 0x18000e5da;
  SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)0x0);
  *(undefined8 *)(puVar4 + -8) = 0x18000e5e5;
  LVar3 = UnhandledExceptionFilter((_EXCEPTION_POINTERS *)(puVar4 + 0x40));
  if ((LVar3 == 0) && (BVar2 != 1)) {
    *(undefined8 *)(puVar4 + -8) = 0x18000e5f6;
    FUN_18000e4b0(3);
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e608
   NAME : FUN_18000e608
   SIG  : undefined FUN_18000e608(void)
   ======================================================================== */

void FUN_18000e608(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_18057f890; plVar1 < &DAT_18057f890; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057ec20)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e644
   NAME : FUN_18000e644
   SIG  : undefined FUN_18000e644(void)
   ======================================================================== */

void FUN_18000e644(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_18057f8a0; plVar1 < &DAT_18057f8a0; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057ec20)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e680
   NAME : _guard_check_icall
   SIG  : undefined _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
  return;
}



/* ========================================================================
   ENTRY: 18000e684
   NAME : FUN_18000e684
   SIG  : undefined FUN_18000e684(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x00018000e775) */
/* WARNING: Removing unreachable block (ram,0x00018000e765) */
/* WARNING: Removing unreachable block (ram,0x00018000e740) */
/* WARNING: Removing unreachable block (ram,0x00018000e6be) */
/* WARNING: Removing unreachable block (ram,0x00018000e69c) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000e684(void)

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
    _DAT_180580060 = 0x8000;
    _DAT_180580068 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_180580678 = DAT_180580678 | 1;
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
      DAT_180580678 = DAT_180580678 | 2;
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
  _DAT_180580058 = 1;
  DAT_18058005c = 2;
  uVar9 = DAT_180580050 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    _DAT_180580058 = 2;
    DAT_18058005c = 6;
    uVar9 = DAT_180580050 & 0xffffffffffffffee;
  }
  DAT_180580050 = uVar9;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar9 = xinuse(0);
    uVar9 = in_XCR0 & uVar9 & 0xffffffff;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar6 = (byte)uVar9, (bVar6 & 6) == 6)) {
      _DAT_180580058 = 3;
      uVar7 = DAT_180580050;
      uVar5 = DAT_18058005c | 8;
      if ((uVar8 & 0x20) != 0) {
        _DAT_180580058 = 5;
        uVar7 = DAT_180580050 & 0xfffffffffffffffd;
        uVar5 = DAT_18058005c | 0x28;
        if (((uVar8 & 0xd0030000) == 0xd0030000) && ((bVar6 & 0xe0) == 0xe0)) {
          DAT_18058005c = DAT_18058005c | 0x68;
          _DAT_180580058 = 6;
          uVar7 = DAT_180580050 & 0xffffffffffffffd9;
          uVar5 = DAT_18058005c;
        }
      }
      DAT_18058005c = uVar5;
      DAT_180580050 = uVar7;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_180580050 = DAT_180580050 & 0xfffffffffeffffff;
      }
      if (((uVar11 >> 0x13 & 1) != 0) && ((bVar6 & 0xe0) == 0xe0)) {
        _DAT_180580674 = (uint)uVar12 & 0x400ff;
        DAT_180580050 = ~((ulonglong)((uint)(uVar12 >> 0x10) & 6) | 0x1000029) & DAT_180580050;
        if (1 < (byte)_DAT_180580674) {
          DAT_180580050 = DAT_180580050 & 0xffffffffffffffbf;
        }
      }
    }
    if (((uVar11 >> 0x15 & 1) != 0) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_180580050 = DAT_180580050 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000e91c
   NAME : FUN_18000e91c
   SIG  : undefined FUN_18000e91c(void)
   ======================================================================== */

undefined8 FUN_18000e91c(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 18000e924
   NAME : __scrt_is_ucrt_dll_in_use
   SIG  : undefined __scrt_is_ucrt_dll_in_use(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_is_ucrt_dll_in_use
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

bool __scrt_is_ucrt_dll_in_use(void)

{
  return DAT_180580070 != 0;
}



/* ========================================================================
   ENTRY: 18000e930
   NAME : FUN_18000e930
   SIG  : undefined FUN_18000e930(void)
   ======================================================================== */

undefined1 FUN_18000e930(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 18000e934
   NAME : FUN_18000e934
   SIG  : undefined FUN_18000e934(void)
   ======================================================================== */

undefined8 FUN_18000e934(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 18000e950
   NAME : _guard_dispatch_icall
   SIG  : undefined _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x00018000e950. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 18000e970
   NAME : _guard_dispatch_icall
   SIG  : undefined _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x00018000e970 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x00018000e950. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 18000e976
   NAME : FUN_18000e976
   SIG  : undefined FUN_18000e976(void)
   ======================================================================== */

void FUN_18000e976(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(undefined1 *)(param_2 + 0x40));
  return;
}



/* ========================================================================
   ENTRY: 18000e98d
   NAME : FUN_18000e98d
   SIG  : undefined FUN_18000e98d(void)
   ======================================================================== */

void FUN_18000e98d(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(undefined1 *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 18000e9a6
   NAME : FUN_18000e9a6
   SIG  : undefined FUN_18000e9a6(void)
   ======================================================================== */

void FUN_18000e9a6(void)

{
  FUN_18000e454();
  return;
}



/* ========================================================================
   ENTRY: 18000e9ba
   NAME : FUN_18000e9ba
   SIG  : undefined FUN_18000e9ba(void)
   ======================================================================== */

void FUN_18000e9ba(undefined8 *param_1,longlong param_2)

{
  __scrt_dllmain_exception_filter
            (*(undefined8 *)(param_2 + 0x60),*(undefined4 *)(param_2 + 0x68),
             *(undefined8 *)(param_2 + 0x70),FUN_18000deb8,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000e9f0
   NAME : FUN_18000e9f0
   SIG  : undefined FUN_18000e9f0(void)
   ======================================================================== */

bool FUN_18000e9f0(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}



/* ========================================================================
   ENTRY: 18000ea20
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ea20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 18000ea30
   NAME : memcpy
   SIG  : void * __cdecl memcpy(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memcpy(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000ea30. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memcpy(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000ea40
   NAME : memmove
   SIG  : void * __cdecl memmove(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memmove(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000ea40. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memmove(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000ea50
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000ea50. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000ea60
   NAME : calloc
   SIG  : void * __cdecl calloc(size_t _Count, size_t _Size)
   ======================================================================== */

void * __cdecl calloc(size_t _Count,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000ea60. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = calloc(_Count,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000ea70
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ea70. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 18000ea80
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ea80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 18000ea90
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ea90. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 18000eaa0
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000eaa0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 18000eab0
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000eab0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 18000eac0
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000eac0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 18000ead0
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ead0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 18000eae0
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000eae0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 18000eaf0
   NAME : cos
   SIG  : double __cdecl cos(double _X)
   ======================================================================== */

double __cdecl cos(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eaf0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = cos(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000eb00
   NAME : log10
   SIG  : double __cdecl log10(double _X)
   ======================================================================== */

double __cdecl log10(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eb00. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = log10(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000eb10
   NAME : sin
   SIG  : double __cdecl sin(double _X)
   ======================================================================== */

double __cdecl sin(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eb10. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sin(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000eb20
   NAME : sqrt
   SIG  : double __cdecl sqrt(double _X)
   ======================================================================== */

double __cdecl sqrt(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eb20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sqrt(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000eb30
   NAME : sqrtf
   SIG  : float __cdecl sqrtf(float _X)
   ======================================================================== */

float __cdecl sqrtf(float _X)

{
  float fVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eb30. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fVar1 = sqrtf(_X);
  return fVar1;
}



/* ========================================================================
   ENTRY: 18000eb40
   NAME : strcmp
   SIG  : int __cdecl strcmp(char * _Str1, char * _Str2)
   ======================================================================== */

int __cdecl strcmp(char *_Str1,char *_Str2)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000eb40. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = strcmp(_Str1,_Str2);
  return iVar1;
}


