
/* ========================================================================
   ENTRY: 180001000
   NAME : rnn_lpc
   SIG  : undefined rnn_lpc(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_lpc(float *param_1,float *param_2,uint param_3)

{
  float fVar1;
  float fVar2;
  uint uVar3;
  float fVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  ulonglong uVar8;
  float *pfVar9;
  ulonglong uVar10;
  longlong lVar11;
  float fVar12;
  float fVar13;
  
                    /* 0x1000  24  rnn_lpc */
  fVar13 = *param_2;
  memset(param_1,0,(longlong)(int)param_3 << 2);
  fVar4 = DAT_180010010;
  uVar3 = _DAT_180010000;
  if ((*param_2 != 0.0) || (NAN(*param_2))) {
    uVar5 = (ulonglong)param_3;
    if ((int)param_3 < 1) {
      uVar5 = 0;
    }
    uVar10 = 0;
    do {
      uVar6 = uVar10 + 1;
      uVar7 = (uVar6 >> 1) + (ulonglong)(uVar6 >> 1 == 0);
      if (uVar10 == uVar5) {
        return;
      }
      if (uVar10 == 0) {
        fVar12 = (float)((uint)(param_2[1] + 0.0) ^ uVar3) / fVar13;
        *param_1 = fVar12;
        uVar6 = 1;
      }
      else {
        if (uVar10 < 4) {
          fVar12 = 0.0;
          lVar11 = 0;
        }
        else {
          fVar12 = 0.0;
          lVar11 = 0;
          pfVar9 = param_1 + 3;
          do {
            fVar12 = *pfVar9 * param_2[uVar10 + lVar11 + -3] +
                     pfVar9[-1] * param_2[uVar10 + lVar11 + -2] +
                     pfVar9[-2] * param_2[uVar10 + lVar11 + -1] +
                     pfVar9[-3] * param_2[uVar10 + lVar11] + fVar12;
            pfVar9 = pfVar9 + 4;
            lVar11 = lVar11 + -4;
          } while (-lVar11 != (uVar10 & 0xfffffffffffffffc));
          lVar11 = -lVar11;
        }
        if ((uVar10 & 3) != 0) {
          pfVar9 = param_2 + (uVar10 - lVar11);
          uVar8 = 0;
          do {
            fVar12 = fVar12 + param_1[lVar11 + uVar8] * *pfVar9;
            uVar8 = uVar8 + 1;
            pfVar9 = pfVar9 + -1;
          } while (((uint)uVar10 & 3) != uVar8);
        }
        fVar12 = (float)((uint)(fVar12 + param_2[uVar10 + 1]) ^ uVar3) / fVar13;
        param_1[uVar10] = fVar12;
        if (uVar6 < 4) {
          uVar8 = 0;
        }
        else {
          lVar11 = uVar10 - 1;
          uVar8 = 0;
          do {
            fVar1 = param_1[uVar8];
            fVar2 = param_1[lVar11];
            param_1[uVar8] = fVar12 * fVar2 + fVar1;
            param_1[lVar11] = fVar1 * fVar12 + fVar2;
            fVar1 = param_1[uVar8 + 1];
            fVar2 = param_1[uVar10 + (uVar8 ^ 0x3ffffffffffffffe)];
            param_1[uVar8 + 1] = fVar12 * fVar2 + fVar1;
            param_1[uVar10 + (uVar8 ^ 0x3ffffffffffffffe)] = fVar1 * fVar12 + fVar2;
            uVar8 = uVar8 + 2;
            lVar11 = lVar11 + -2;
          } while ((uVar7 & 0x7ffffffffffffffe) != uVar8);
        }
        if ((uVar7 & 1) != 0) {
          fVar1 = param_1[uVar8];
          fVar2 = param_1[uVar10 + ~uVar8];
          param_1[uVar8] = fVar12 * fVar2 + fVar1;
          param_1[uVar10 + ~uVar8] = fVar1 * fVar12 + fVar2;
        }
      }
      fVar13 = fVar13 - fVar12 * fVar12 * fVar13;
      uVar10 = uVar6;
    } while (*param_2 * fVar4 <= fVar13);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800012c0
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
  
                    /* 0x12c0  3  rnn_autocorr */
  local_48 = DAT_180582000 ^ (ulonglong)auStack_df8;
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
          local_dc8[uVar9] = param_1[uVar9] * fVar14;
          local_dc8[(int)(uVar10 - 1)] = fVar14 * param_1[(int)(uVar10 - 1)];
          fVar14 = *(float *)(param_3 + 4 + uVar9 * 4);
          local_dc8[uVar9 + 1] = param_1[uVar9 + 1] * fVar14;
          local_dc8[(int)uVar6] = fVar14 * param_1[(int)uVar6];
          uVar9 = uVar9 + 2;
          uVar10 = uVar6;
        } while ((param_4 & 0x7ffffffe) != uVar9);
      }
      if ((param_4 & 1) != 0) {
        fVar14 = *(float *)(param_3 + uVar9 * 4);
        local_dc8[uVar9] = param_1[uVar9] * fVar14;
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
        uVar2 = param_5 - (int)uVar9 & 3;
        while (uVar2 != 0) {
          fVar14 = fVar14 + pfVar5[lVar13] * pfVar11[lVar13];
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
  if ((local_48 ^ (ulonglong)auStack_df8) == DAT_180582000) {
    return 0;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180001560
   NAME : rnnoise_model_from_buffer
   SIG  : undefined rnnoise_model_from_buffer(void)
   ======================================================================== */

void rnnoise_model_from_buffer(undefined8 param_1,undefined4 param_2)

{
  undefined8 *puVar1;
  
                    /* 0x1560  37  rnnoise_model_from_buffer */
  puVar1 = malloc(0x20);
  puVar1[1] = 0;
  *puVar1 = param_1;
  *(undefined4 *)(puVar1 + 2) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180001590
   NAME : rnnoise_model_from_filename
   SIG  : undefined rnnoise_model_from_filename(void)
   ======================================================================== */

void rnnoise_model_from_filename(char *param_1)

{
  FILE *pFVar1;
  longlong lVar2;
  
                    /* 0x1590  39  rnnoise_model_from_filename */
  pFVar1 = fopen(param_1,"rb");
  lVar2 = rnnoise_model_from_file(pFVar1);
  *(FILE **)(lVar2 + 0x18) = pFVar1;
  return;
}



/* ========================================================================
   ENTRY: 1800015c0
   NAME : rnnoise_model_from_file
   SIG  : undefined rnnoise_model_from_file(void)
   ======================================================================== */

undefined8 * rnnoise_model_from_file(FILE *param_1)

{
  long lVar1;
  undefined8 *_Memory;
  void *_DstBuf;
  size_t sVar2;
  
                    /* 0x15c0  38  rnnoise_model_from_file */
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
   ENTRY: 180001670
   NAME : rnnoise_model_free
   SIG  : undefined rnnoise_model_free(void)
   ======================================================================== */

void rnnoise_model_free(void *param_1)

{
                    /* 0x1670  36  rnnoise_model_free */
  if (*(FILE **)((longlong)param_1 + 0x18) != (FILE *)0x0) {
    fclose(*(FILE **)((longlong)param_1 + 0x18));
  }
  if (*(void **)((longlong)param_1 + 8) != (void *)0x0) {
    free(*(void **)((longlong)param_1 + 8));
  }
                    /* WARNING: Could not recover jumptable at 0x00018000169e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800016b0
   NAME : rnnoise_get_size
   SIG  : undefined rnnoise_get_size(void)
   ======================================================================== */

undefined8 rnnoise_get_size(void)

{
                    /* 0x16b0  34  rnnoise_get_size */
  return 0x7fb0;
}



/* ========================================================================
   ENTRY: 1800016c0
   NAME : rnnoise_get_frame_size
   SIG  : undefined rnnoise_get_frame_size(void)
   ======================================================================== */

undefined8 rnnoise_get_frame_size(void)

{
                    /* 0x16c0  33  rnnoise_get_frame_size */
  return 0x1e0;
}



/* ========================================================================
   ENTRY: 1800016d0
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
  
                    /* 0x16d0  35  rnnoise_init */
  local_18 = DAT_180582000 ^ (ulonglong)auStack_48;
  memset(param_1,0,0x7fb0);
  if (param_2 == (longlong *)0x0) {
    iVar1 = init_rnnoise(param_1,&PTR_s_conv1_weights_float_18057af60);
  }
  else {
    lVar3 = param_2[1];
    if (lVar3 == 0) {
      lVar3 = *param_2;
    }
    rnn_parse_weights(&local_20,lVar3,(int)param_2[2]);
    if (local_20 == (void *)0x0) {
      if ((local_18 ^ (ulonglong)auStack_48) == DAT_180582000) {
        return 0xffffffff;
      }
      goto LAB_180001795;
    }
    iVar1 = init_rnnoise(param_1);
    free(local_20);
  }
  uVar2 = 0xffffffff;
  if (iVar1 == 0) {
    *(undefined4 *)((longlong)param_1 + 0x280) = 0;
    uVar2 = 0;
  }
  if ((local_18 ^ (ulonglong)auStack_48) == DAT_180582000) {
    return uVar2;
  }
LAB_180001795:
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 1800017a0
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
  
                    /* 0x17a0  31  rnnoise_create */
  local_18 = DAT_180582000 ^ (ulonglong)auStack_48;
  _Memory = calloc(1,0x7fb0);
  if (param_1 == (longlong *)0x0) {
    iVar1 = init_rnnoise(_Memory,&PTR_s_conv1_weights_float_18057af60);
joined_r0x000180001843:
    if (iVar1 == 0) {
      *(undefined4 *)((longlong)_Memory + 0x280) = 0;
      if ((local_18 ^ (ulonglong)auStack_48) == DAT_180582000) {
        return _Memory;
      }
      goto LAB_180001860;
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
      goto joined_r0x000180001843;
    }
  }
  free(_Memory);
  if ((local_18 ^ (ulonglong)auStack_48) == DAT_180582000) {
    return (void *)0x0;
  }
LAB_180001860:
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180001870
   NAME : rnnoise_destroy
   SIG  : void __cdecl rnnoise_destroy(void * _Memory)
   ======================================================================== */

void __cdecl rnnoise_destroy(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x000180001870. Too many branches */
                    /* WARNING: Treating indirect jump as call */
                    /* 0x1870  32  rnnoise_destroy */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180001880
   NAME : rnn_frame_analysis
   SIG  : undefined rnn_frame_analysis(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_frame_analysis(longlong param_1,void *param_2,float *param_3,void *param_4)

{
  float fVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  ulonglong uVar4;
  int iVar5;
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  int iVar10;
  longlong lVar11;
  longlong lVar12;
  uint uVar13;
  ulonglong uVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  undefined1 auStack_4b48 [8];
  ulonglong auStack_4b40 [2];
  float fStack_4b2c;
  undefined8 local_4b28;
  ulonglong auStack_4b20 [239];
  undefined1 local_43a8 [1920];
  undefined1 local_3c28 [7632];
  undefined1 auStack_1e58 [16];
  undefined1 auStack_1e48 [16];
  undefined1 auStack_1e38 [16];
  float local_1e28 [1920];
  ulonglong local_28;
  
                    /* 0x1880  21  rnn_frame_analysis */
  local_28 = DAT_180582000 ^ (ulonglong)auStack_4b48;
  memcpy(&local_4b28,(void *)(param_1 + 0x284),0x780);
  memcpy(local_43a8,param_4,0x780);
  memcpy((void *)(param_1 + 0x284),param_4,0x780);
  lVar11 = 1;
  lVar12 = 0xefc;
  do {
    fVar15 = *(float *)(&UNK_18057e18c + lVar11 * 4);
    fVar16 = (float)(&DAT_18057e190)[lVar11];
    (&fStack_4b2c)[lVar11] = (&fStack_4b2c)[lVar11] * fVar15;
    *(float *)((longlong)auStack_4b20 + lVar12 + -8) =
         fVar15 * *(float *)((longlong)auStack_4b20 + lVar12 + -8);
    *(float *)((longlong)auStack_4b20 + lVar11 * 4 + -8) =
         *(float *)((longlong)auStack_4b20 + lVar11 * 4 + -8) * fVar16;
    *(float *)((longlong)&fStack_4b2c + lVar12) =
         fVar16 * *(float *)((longlong)&fStack_4b2c + lVar12);
    lVar11 = lVar11 + 2;
    lVar12 = lVar12 + -8;
  } while (lVar11 != 0x1e1);
  lVar11 = 6;
  do {
    uVar14 = *(ulonglong *)((longlong)auStack_4b40 + lVar11 * 4);
    uVar2 = *(ulonglong *)((longlong)auStack_4b40 + lVar11 * 4 + 8);
    uVar3 = *(ulonglong *)(&stack0xffffffffffffb4d0 + lVar11 * 4);
    uVar4 = *(ulonglong *)((longlong)auStack_4b20 + lVar11 * 4 + -8);
    auVar6._8_4_ = (int)(uVar14 >> 0x20);
    auVar6._0_8_ = uVar14 & 0xffffffff;
    auVar6._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1e58 + lVar11 * 8) = auVar6;
    auVar7._8_4_ = (int)(uVar2 >> 0x20);
    auVar7._0_8_ = uVar2 & 0xffffffff;
    auVar7._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1e48 + lVar11 * 8) = auVar7;
    auVar8._8_4_ = (int)(uVar3 >> 0x20);
    auVar8._0_8_ = uVar3 & 0xffffffff;
    auVar8._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1e38 + lVar11 * 8) = auVar8;
    auVar9._8_4_ = (int)(uVar4 >> 0x20);
    auVar9._0_8_ = uVar4 & 0xffffffff;
    auVar9._12_4_ = 0;
    *(undefined1 (*) [16])(local_1e28 + lVar11 * 2) = auVar9;
    lVar11 = lVar11 + 8;
  } while (lVar11 != 0x3c6);
  rnn_fft_c(&DAT_18057e140,local_1e28,local_3c28);
  memcpy(param_2,local_3c28,0xf08);
  local_1e28[0x1c] = 0.0;
  local_1e28[0x1d] = 0.0;
  local_1e28[0x1e] = 0.0;
  local_1e28[0x1f] = 0.0;
  local_1e28[0x18] = 0.0;
  local_1e28[0x19] = 0.0;
  local_1e28[0x1a] = 0.0;
  local_1e28[0x1b] = 0.0;
  local_1e28[0x14] = 0.0;
  local_1e28[0x15] = 0.0;
  local_1e28[0x16] = 0.0;
  local_1e28[0x17] = 0.0;
  local_1e28[0x10] = 0.0;
  local_1e28[0x11] = 0.0;
  local_1e28[0x12] = 0.0;
  local_1e28[0x13] = 0.0;
  local_1e28[0xc] = 0.0;
  local_1e28[0xd] = 0.0;
  local_1e28[0xe] = 0.0;
  local_1e28[0xf] = 0.0;
  local_1e28[8] = 0.0;
  local_1e28[9] = 0.0;
  local_1e28[10] = 0.0;
  local_1e28[0xb] = 0.0;
  local_1e28[4] = 0.0;
  local_1e28[5] = 0.0;
  local_1e28[6] = 0.0;
  local_1e28[7] = 0.0;
  local_1e28[0] = 0.0;
  local_1e28[1] = 0.0;
  local_1e28[2] = 0.0;
  local_1e28[3] = 0.0;
  local_1e28[0x20] = 0.0;
  local_1e28[0x21] = 0.0;
  lVar11 = 0;
  iVar10 = 0;
  do {
    iVar5 = (&DAT_180010024)[lVar11];
    uVar13 = iVar5 - iVar10;
    if (uVar13 != 0 && iVar10 <= iVar5) {
      fVar15 = (float)*(undefined8 *)(local_1e28 + lVar11);
      fVar16 = (float)((ulonglong)*(undefined8 *)(local_1e28 + lVar11) >> 0x20);
      uVar14 = 0;
      do {
        fVar17 = (float)(int)uVar14 / (float)(int)uVar13;
        fVar18 = *(float *)((longlong)param_2 + uVar14 * 8 + (longlong)iVar10 * 8);
        fVar1 = *(float *)((longlong)param_2 + uVar14 * 8 + (longlong)iVar10 * 8 + 4);
        fVar18 = fVar1 * fVar1 + fVar18 * fVar18;
        fVar15 = fVar15 + fVar18 * (DAT_1800100a8 - fVar17);
        fVar16 = fVar16 + fVar18 * fVar17;
        uVar14 = uVar14 + 1;
      } while (uVar13 != uVar14);
      *(ulonglong *)(local_1e28 + lVar11) = CONCAT44(fVar16,fVar15);
    }
    lVar11 = lVar11 + 1;
    iVar10 = iVar5;
  } while (lVar11 != 0x21);
  local_1e28[1] = (local_1e28[0] + local_1e28[1] + local_1e28[0] + local_1e28[1]) / DAT_1800100ac;
  local_1e28[0x20] = (local_1e28[0x20] + 0.0 + local_1e28[0x20] + 0.0) / DAT_1800100ac;
  *(ulonglong *)(param_3 + 0x18) = CONCAT44(local_1e28[0x1a],local_1e28[0x19]);
  *(ulonglong *)(param_3 + 0x1a) = CONCAT44(local_1e28[0x1c],local_1e28[0x1b]);
  *(ulonglong *)(param_3 + 0x14) = CONCAT44(local_1e28[0x16],local_1e28[0x15]);
  *(ulonglong *)(param_3 + 0x16) = CONCAT44(local_1e28[0x18],local_1e28[0x17]);
  *(ulonglong *)(param_3 + 0x10) = CONCAT44(local_1e28[0x12],local_1e28[0x11]);
  *(ulonglong *)(param_3 + 0x12) = CONCAT44(local_1e28[0x14],local_1e28[0x13]);
  *(ulonglong *)(param_3 + 0xc) = CONCAT44(local_1e28[0xe],local_1e28[0xd]);
  *(ulonglong *)(param_3 + 0xe) = CONCAT44(local_1e28[0x10],local_1e28[0xf]);
  *(ulonglong *)(param_3 + 8) = CONCAT44(local_1e28[10],local_1e28[9]);
  *(ulonglong *)(param_3 + 10) = CONCAT44(local_1e28[0xc],local_1e28[0xb]);
  *(ulonglong *)(param_3 + 4) = CONCAT44(local_1e28[6],local_1e28[5]);
  *(ulonglong *)(param_3 + 6) = CONCAT44(local_1e28[8],local_1e28[7]);
  *(ulonglong *)(param_3 + 0x1c) = CONCAT44(local_1e28[0x1e],local_1e28[0x1d]);
  *(ulonglong *)(param_3 + 0x1e) = CONCAT44(local_1e28[0x20],local_1e28[0x1f]);
  *param_3 = local_1e28[1];
  param_3[1] = local_1e28[2];
  param_3[2] = local_1e28[3];
  param_3[3] = local_1e28[4];
  if ((local_28 ^ (ulonglong)auStack_4b48) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180001bd0
   NAME : rnn_compute_frame_features
   SIG  : undefined rnn_compute_frame_features(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
rnn_compute_frame_features
          (longlong param_1,longlong param_2,void *param_3,longlong param_4,undefined8 *param_5,
          undefined8 *param_6,undefined8 *param_7,void *param_8)

{
  void *_Dst;
  undefined8 *puVar1;
  longlong lVar2;
  undefined8 *puVar3;
  float fVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  int iVar8;
  double dVar9;
  double dVar10;
  undefined1 auVar11 [16];
  double dVar12;
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  undefined1 auVar15 [16];
  double dVar16;
  undefined8 uVar17;
  undefined8 uVar18;
  undefined8 uVar19;
  double dVar20;
  int iVar21;
  longlong lVar22;
  longlong lVar23;
  uint uVar24;
  ulonglong uVar25;
  undefined4 uVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  undefined1 auStack_5988 [32];
  int *local_5968;
  undefined4 local_5960;
  undefined4 local_5958;
  int local_5944;
  void *local_5940;
  undefined1 local_5938 [1536];
  undefined1 local_5338 [1896];
  ulonglong auStack_4bd0 [2];
  undefined8 uStack_4bc0;
  float local_4bb8 [2];
  ulonglong auStack_4bb0 [479];
  undefined1 local_3cb8 [7632];
  undefined1 auStack_1ee8 [16];
  undefined1 auStack_1ed8 [16];
  undefined1 auStack_1ec8 [16];
  float local_1eb8 [1922];
  ulonglong local_b0;
  undefined8 uStack_48;
  
                    /* 0x1bd0  7  rnn_compute_frame_features */
  uStack_48 = 0x180001be6;
  local_b0 = DAT_180582000 ^ (ulonglong)auStack_5988;
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
  uVar26 = rnn_remove_doubling(local_5938,0x300,0x3c,0x3c0);
  *(int *)(param_1 + 0x478c) = local_5944;
  *(undefined4 *)(param_1 + 0x4788) = uVar26;
  if (0x6bf - local_5944 < 0x300 - local_5944) {
    iVar21 = 0x302 - local_5944;
    lVar22 = 8;
    do {
      *(undefined4 *)((longlong)&uStack_4bc0 + lVar22) =
           *(undefined4 *)((longlong)_Dst + (longlong)(iVar21 + -2) * 4);
      *(undefined4 *)((longlong)local_4bb8 + lVar22 + -4) =
           *(undefined4 *)((longlong)_Dst + (longlong)(iVar21 + -1) * 4);
      *(undefined4 *)((longlong)local_4bb8 + lVar22) =
           *(undefined4 *)((longlong)_Dst + (longlong)iVar21 * 4);
      lVar22 = lVar22 + 0xc;
      iVar21 = iVar21 + 3;
    } while (lVar22 != 0xf08);
  }
  else {
    iVar21 = 0x308 - local_5944;
    lVar22 = 0;
    do {
      puVar1 = (undefined8 *)((longlong)_Dst + (longlong)(iVar21 + -8) * 4);
      uVar17 = puVar1[1];
      puVar3 = (undefined8 *)(param_1 + 0x1198 + (longlong)(iVar21 + -8) * 4);
      uVar18 = *puVar3;
      uVar19 = puVar3[1];
      *(undefined8 *)(local_4bb8 + lVar22) = *puVar1;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4) = uVar17;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 8) = uVar18;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 0x10) = uVar19;
      puVar1 = (undefined8 *)((longlong)_Dst + (longlong)iVar21 * 4);
      uVar17 = puVar1[1];
      puVar3 = (undefined8 *)(param_1 + 0x1198 + (longlong)iVar21 * 4);
      uVar18 = *puVar3;
      uVar19 = puVar3[1];
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 0x18) = *puVar1;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 0x20) = uVar17;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 0x28) = uVar18;
      *(undefined8 *)((longlong)auStack_4bb0 + lVar22 * 4 + 0x30) = uVar19;
      lVar22 = lVar22 + 0x10;
      iVar21 = iVar21 + 0x10;
    } while (lVar22 != 0x3c0);
  }
  lVar22 = 1;
  lVar23 = 0xefc;
  do {
    fVar4 = *(float *)(&UNK_18057e18c + lVar22 * 4);
    fVar32 = (float)(&DAT_18057e190)[lVar22];
    local_4bb8[lVar22 + -1] = local_4bb8[lVar22 + -1] * fVar4;
    *(float *)((longlong)local_4bb8 + lVar23) = fVar4 * *(float *)((longlong)local_4bb8 + lVar23);
    local_4bb8[lVar22] = local_4bb8[lVar22] * fVar32;
    *(float *)((longlong)local_4bb8 + lVar23 + -4) =
         fVar32 * *(float *)((longlong)local_4bb8 + lVar23 + -4);
    lVar22 = lVar22 + 2;
    lVar23 = lVar23 + -8;
  } while (lVar22 != 0x1e1);
  lVar22 = 6;
  do {
    uVar25 = *(ulonglong *)((longlong)auStack_4bd0 + lVar22 * 4);
    uVar5 = *(ulonglong *)((longlong)auStack_4bd0 + lVar22 * 4 + 8);
    uVar6 = *(ulonglong *)((longlong)&uStack_4bc0 + lVar22 * 4);
    uVar7 = *(ulonglong *)(local_4bb8 + lVar22);
    auVar11._8_4_ = (int)(uVar25 >> 0x20);
    auVar11._0_8_ = uVar25 & 0xffffffff;
    auVar11._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1ee8 + lVar22 * 8) = auVar11;
    auVar13._8_4_ = (int)(uVar5 >> 0x20);
    auVar13._0_8_ = uVar5 & 0xffffffff;
    auVar13._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1ed8 + lVar22 * 8) = auVar13;
    auVar14._8_4_ = (int)(uVar6 >> 0x20);
    auVar14._0_8_ = uVar6 & 0xffffffff;
    auVar14._12_4_ = 0;
    *(undefined1 (*) [16])(auStack_1ec8 + lVar22 * 8) = auVar14;
    auVar15._8_4_ = (int)(uVar7 >> 0x20);
    auVar15._0_8_ = uVar7 & 0xffffffff;
    auVar15._12_4_ = 0;
    *(undefined1 (*) [16])(local_1eb8 + lVar22 * 2) = auVar15;
    lVar22 = lVar22 + 8;
  } while (lVar22 != 0x3c6);
  rnn_fft_c(&DAT_18057e140,local_1eb8,local_3cb8);
  memcpy(param_3,local_3cb8,0xf08);
  fVar32 = DAT_1800100ac;
  fVar4 = DAT_1800100a8;
  local_1eb8[0x1c] = 0.0;
  local_1eb8[0x1d] = 0.0;
  local_1eb8[0x1e] = 0.0;
  local_1eb8[0x1f] = 0.0;
  local_1eb8[0x18] = 0.0;
  local_1eb8[0x19] = 0.0;
  local_1eb8[0x1a] = 0.0;
  local_1eb8[0x1b] = 0.0;
  local_1eb8[0x14] = 0.0;
  local_1eb8[0x15] = 0.0;
  local_1eb8[0x16] = 0.0;
  local_1eb8[0x17] = 0.0;
  local_1eb8[0x10] = 0.0;
  local_1eb8[0x11] = 0.0;
  local_1eb8[0x12] = 0.0;
  local_1eb8[0x13] = 0.0;
  local_1eb8[0xc] = 0.0;
  local_1eb8[0xd] = 0.0;
  local_1eb8[0xe] = 0.0;
  local_1eb8[0xf] = 0.0;
  local_1eb8[8] = 0.0;
  local_1eb8[9] = 0.0;
  local_1eb8[10] = 0.0;
  local_1eb8[0xb] = 0.0;
  local_1eb8[4] = 0.0;
  local_1eb8[5] = 0.0;
  local_1eb8[6] = 0.0;
  local_1eb8[7] = 0.0;
  local_1eb8[0] = 0.0;
  local_1eb8[1] = 0.0;
  local_1eb8[2] = 0.0;
  local_1eb8[3] = 0.0;
  local_1eb8[0x20] = 0.0;
  lVar22 = 0;
  iVar21 = 0;
  do {
    iVar8 = (&DAT_180010024)[lVar22];
    uVar24 = iVar8 - iVar21;
    if (uVar24 != 0 && iVar21 <= iVar8) {
      fVar27 = (float)*(undefined8 *)(local_1eb8 + lVar22);
      fVar28 = (float)((ulonglong)*(undefined8 *)(local_1eb8 + lVar22) >> 0x20);
      uVar25 = 0;
      do {
        fVar29 = (float)(int)uVar25 / (float)(int)uVar24;
        fVar30 = *(float *)((longlong)param_3 + uVar25 * 8 + (longlong)iVar21 * 8);
        fVar31 = *(float *)((longlong)param_3 + uVar25 * 8 + (longlong)iVar21 * 8 + 4);
        fVar30 = fVar31 * fVar31 + fVar30 * fVar30;
        fVar27 = fVar27 + fVar30 * (DAT_1800100a8 - fVar29);
        fVar28 = fVar28 + fVar30 * fVar29;
        uVar25 = uVar25 + 1;
      } while (uVar24 != uVar25);
      *(ulonglong *)(local_1eb8 + lVar22) = CONCAT44(fVar28,fVar27);
    }
    lVar22 = lVar22 + 1;
    iVar21 = iVar8;
  } while (lVar22 != 0x21);
  fVar27 = (local_1eb8[0] + local_1eb8[1] + local_1eb8[0] + local_1eb8[1]) / DAT_1800100ac;
  fVar28 = (local_1eb8[0x20] + 0.0 + local_1eb8[0x20] + 0.0) / DAT_1800100ac;
  param_5[0xc] = CONCAT44(local_1eb8[0x1a],local_1eb8[0x19]);
  param_5[0xd] = CONCAT44(local_1eb8[0x1c],local_1eb8[0x1b]);
  param_5[10] = CONCAT44(local_1eb8[0x16],local_1eb8[0x15]);
  param_5[0xb] = CONCAT44(local_1eb8[0x18],local_1eb8[0x17]);
  param_5[8] = CONCAT44(local_1eb8[0x12],local_1eb8[0x11]);
  param_5[9] = CONCAT44(local_1eb8[0x14],local_1eb8[0x13]);
  param_5[6] = CONCAT44(local_1eb8[0xe],local_1eb8[0xd]);
  param_5[7] = CONCAT44(local_1eb8[0x10],local_1eb8[0xf]);
  param_5[4] = CONCAT44(local_1eb8[10],local_1eb8[9]);
  param_5[5] = CONCAT44(local_1eb8[0xc],local_1eb8[0xb]);
  param_5[2] = CONCAT44(local_1eb8[6],local_1eb8[5]);
  param_5[3] = CONCAT44(local_1eb8[8],local_1eb8[7]);
  param_5[0xe] = CONCAT44(local_1eb8[0x1e],local_1eb8[0x1d]);
  param_5[0xf] = CONCAT44(fVar28,local_1eb8[0x1f]);
  *param_5 = CONCAT44(local_1eb8[2],fVar27);
  param_5[1] = CONCAT44(local_1eb8[4],local_1eb8[3]);
  local_1eb8[0x1c] = 0.0;
  local_1eb8[0x1d] = 0.0;
  local_1eb8[0x1e] = 0.0;
  local_1eb8[0x1f] = 0.0;
  local_1eb8[0x18] = 0.0;
  local_1eb8[0x19] = 0.0;
  local_1eb8[0x1a] = 0.0;
  local_1eb8[0x1b] = 0.0;
  local_1eb8[0x14] = 0.0;
  local_1eb8[0x15] = 0.0;
  local_1eb8[0x16] = 0.0;
  local_1eb8[0x17] = 0.0;
  local_1eb8[0x10] = 0.0;
  local_1eb8[0x11] = 0.0;
  local_1eb8[0x12] = 0.0;
  local_1eb8[0x13] = 0.0;
  local_1eb8[0xc] = 0.0;
  local_1eb8[0xd] = 0.0;
  local_1eb8[0xe] = 0.0;
  local_1eb8[0xf] = 0.0;
  local_1eb8[8] = 0.0;
  local_1eb8[9] = 0.0;
  local_1eb8[10] = 0.0;
  local_1eb8[0xb] = 0.0;
  local_1eb8[4] = 0.0;
  local_1eb8[5] = 0.0;
  local_1eb8[6] = 0.0;
  local_1eb8[7] = 0.0;
  local_1eb8[0] = 0.0;
  local_1eb8[1] = 0.0;
  local_1eb8[2] = 0.0;
  local_1eb8[3] = 0.0;
  local_1eb8[0x20] = 0.0;
  local_1eb8[0x21] = 0.0;
  lVar22 = 0;
  iVar21 = 0;
  do {
    iVar8 = (&DAT_180010024)[lVar22];
    uVar24 = iVar8 - iVar21;
    if (uVar24 != 0 && iVar21 <= iVar8) {
      fVar27 = (float)*(undefined8 *)(local_1eb8 + lVar22);
      fVar28 = (float)((ulonglong)*(undefined8 *)(local_1eb8 + lVar22) >> 0x20);
      lVar23 = (longlong)iVar21 * 8;
      lVar2 = param_2 + 4 + (longlong)iVar21 * 8;
      uVar25 = 0;
      do {
        fVar30 = (float)(int)uVar25 / (float)(int)uVar24;
        fVar31 = *(float *)(lVar2 + uVar25 * 8) *
                 *(float *)((longlong)param_3 + uVar25 * 8 + lVar23 + 4) +
                 *(float *)(lVar2 + -4 + uVar25 * 8) *
                 *(float *)((longlong)param_3 + uVar25 * 8 + lVar23);
        fVar27 = fVar27 + fVar31 * (fVar4 - fVar30);
        fVar28 = fVar28 + fVar31 * fVar30;
        uVar25 = uVar25 + 1;
      } while (uVar24 != uVar25);
      *(ulonglong *)(local_1eb8 + lVar22) = CONCAT44(fVar28,fVar27);
    }
    lVar22 = lVar22 + 1;
    iVar21 = iVar8;
  } while (lVar22 != 0x21);
  local_1eb8[1] = (local_1eb8[0] + local_1eb8[1] + local_1eb8[0] + local_1eb8[1]) / fVar32;
  local_1eb8[0x20] = (local_1eb8[0x20] + 0.0 + local_1eb8[0x20] + 0.0) / fVar32;
  param_6[0xc] = CONCAT44(local_1eb8[0x1a],local_1eb8[0x19]);
  param_6[0xd] = CONCAT44(local_1eb8[0x1c],local_1eb8[0x1b]);
  param_6[10] = CONCAT44(local_1eb8[0x16],local_1eb8[0x15]);
  param_6[0xb] = CONCAT44(local_1eb8[0x18],local_1eb8[0x17]);
  param_6[8] = CONCAT44(local_1eb8[0x12],local_1eb8[0x11]);
  param_6[9] = CONCAT44(local_1eb8[0x14],local_1eb8[0x13]);
  param_6[6] = CONCAT44(local_1eb8[0xe],local_1eb8[0xd]);
  param_6[7] = CONCAT44(local_1eb8[0x10],local_1eb8[0xf]);
  param_6[4] = CONCAT44(local_1eb8[10],local_1eb8[9]);
  param_6[5] = CONCAT44(local_1eb8[0xc],local_1eb8[0xb]);
  param_6[2] = CONCAT44(local_1eb8[6],local_1eb8[5]);
  param_6[3] = CONCAT44(local_1eb8[8],local_1eb8[7]);
  param_6[0xe] = CONCAT44(local_1eb8[0x1e],local_1eb8[0x1d]);
  param_6[0xf] = CONCAT44(local_1eb8[0x20],local_1eb8[0x1f]);
  *param_6 = CONCAT44(local_1eb8[2],local_1eb8[1]);
  param_6[1] = CONCAT44(local_1eb8[4],local_1eb8[3]);
  dVar20 = DAT_1800100b0;
  lVar22 = 1;
  do {
    fVar4 = *(float *)((longlong)param_6 + lVar22 * 4 + -4);
    dVar9 = (double)(*(float *)(param_4 + -4 + lVar22 * 4) *
                    *(float *)((longlong)param_5 + lVar22 * 4 + -4)) + dVar20;
    if (dVar9 < 0.0) {
      dVar9 = sqrt(dVar9);
    }
    else {
      dVar9 = SQRT(dVar9);
    }
    *(float *)((longlong)param_6 + lVar22 * 4 + -4) = (float)((double)fVar4 / dVar9);
    fVar4 = *(float *)((longlong)param_6 + lVar22 * 4);
    dVar9 = (double)(*(float *)(param_4 + lVar22 * 4) * *(float *)((longlong)param_5 + lVar22 * 4))
            + dVar20;
    if (dVar9 < 0.0) {
      dVar9 = sqrt(dVar9);
    }
    else {
      dVar9 = SQRT(dVar9);
    }
    *(float *)((longlong)param_6 + lVar22 * 4) = (float)((double)fVar4 / dVar9);
    lVar22 = lVar22 + 2;
  } while (lVar22 != 0x21);
  FUN_1800024f0(param_7 + 0x10);
  *(float *)(param_7 + 0x20) = (float)((double)(local_5944 + -300) * DAT_1800100b8);
  dVar9 = DAT_1800100c8;
  fVar4 = DAT_1800100c4;
  dVar20 = DAT_1800100b8;
  fVar28 = 0.0;
  lVar22 = 0;
  fVar32 = DAT_1800100c0;
  fVar27 = DAT_1800100c0;
  do {
    dVar10 = log10((double)*(float *)(param_4 + lVar22 * 4) + dVar20);
    fVar30 = fVar32 + fVar4;
    dVar12 = (double)fVar27 + dVar9;
    dVar16 = dVar12;
    if (dVar12 <= (double)(float)dVar10) {
      dVar16 = (double)(float)dVar10;
    }
    if ((double)fVar30 <= dVar16) {
      fVar30 = (float)dVar16;
    }
    local_1eb8[lVar22] = fVar30;
    fVar27 = fVar30;
    if ((double)fVar30 < dVar12) {
      fVar27 = (float)dVar12;
    }
    if (fVar32 <= fVar30) {
      fVar32 = fVar30;
    }
    fVar28 = fVar28 + *(float *)(param_4 + lVar22 * 4);
    lVar22 = lVar22 + 1;
  } while (lVar22 != 0x20);
  if (DAT_1800100d0 <= (double)fVar28) {
    FUN_1800024f0(param_7,local_1eb8);
    *param_7 = CONCAT44((float)((ulonglong)*param_7 >> 0x20) + _UNK_1800100e4,
                        (float)*param_7 + _DAT_1800100e0);
    if ((local_b0 ^ (ulonglong)auStack_5988) == DAT_180582000) {
      return 0;
    }
  }
  else {
    memset(param_7,0,0x104);
    if ((local_b0 ^ (ulonglong)auStack_5988) == DAT_180582000) {
      return 1;
    }
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 1800024f0
   NAME : FUN_1800024f0
   SIG  : undefined FUN_1800024f0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800024f0(undefined **param_1,undefined **param_2)

{
  longlong lVar1;
  float *pfVar2;
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
  float fVar107;
  float fVar108;
  float fVar109;
  float fVar110;
  float fVar111;
  float fVar112;
  float fVar113;
  float fVar114;
  float fVar115;
  float fVar116;
  float fVar117;
  float fVar118;
  float fVar119;
  float fVar120;
  float fVar121;
  float fVar122;
  float fVar123;
  float fVar124;
  float fVar125;
  float fVar126;
  float fVar127;
  double dVar128;
  double dVar129;
  double dVar130;
  longlong lVar131;
  float fVar132;
  float fVar133;
  float fVar134;
  
  dVar130 = DAT_180010100;
  dVar129 = _UNK_1800100f8;
  dVar128 = _DAT_1800100f0;
  if ((param_1 < param_2 + 0x10 && param_2 < param_1 + 0x10) ||
     (param_1 < &PTR_DAT_18057f910 && &DAT_18057e910 < param_1 + 0x10)) {
    lVar131 = -0x20;
    do {
      *(float *)((longlong)param_1 + (lVar131 + 0x20) * 4) =
           (float)((double)(*(float *)((longlong)param_2 + 0x7c) *
                            *(float *)((longlong)&PTR_DAT_18057f910 + lVar131 * 4) +
                           *(float *)(param_2 + 0xf) * (float)(&DAT_18057f890)[lVar131] +
                           *(float *)((longlong)param_2 + 0x74) * (float)(&DAT_18057f810)[lVar131] +
                           *(float *)(param_2 + 0xe) * (float)(&DAT_18057f790)[lVar131] +
                           *(float *)((longlong)param_2 + 0x6c) * (float)(&DAT_18057f710)[lVar131] +
                           *(float *)(param_2 + 0xd) * (float)(&DAT_18057f690)[lVar131] +
                           *(float *)((longlong)param_2 + 100) * (float)(&DAT_18057f610)[lVar131] +
                           *(float *)(param_2 + 0xc) * (float)(&DAT_18057f590)[lVar131] +
                           *(float *)((longlong)param_2 + 0x5c) * (float)(&DAT_18057f510)[lVar131] +
                           *(float *)(param_2 + 0xb) * (float)(&DAT_18057f490)[lVar131] +
                           *(float *)((longlong)param_2 + 0x54) * (float)(&DAT_18057f410)[lVar131] +
                           *(float *)(param_2 + 10) * (float)(&DAT_18057f390)[lVar131] +
                           *(float *)((longlong)param_2 + 0x4c) * (float)(&DAT_18057f310)[lVar131] +
                           *(float *)(param_2 + 9) * (float)(&DAT_18057f290)[lVar131] +
                           *(float *)((longlong)param_2 + 0x44) * (float)(&DAT_18057f210)[lVar131] +
                           *(float *)(param_2 + 8) * (float)(&DAT_18057f190)[lVar131] +
                           *(float *)((longlong)param_2 + 0x3c) * (float)(&DAT_18057f110)[lVar131] +
                           *(float *)(param_2 + 7) * (float)(&DAT_18057f090)[lVar131] +
                           *(float *)((longlong)param_2 + 0x34) * (float)(&DAT_18057f010)[lVar131] +
                           *(float *)(param_2 + 6) * (float)(&DAT_18057ef90)[lVar131] +
                           *(float *)((longlong)param_2 + 0x2c) * (float)(&DAT_18057ef10)[lVar131] +
                           *(float *)(param_2 + 5) * (float)(&DAT_18057ee90)[lVar131] +
                           *(float *)((longlong)param_2 + 0x24) * (float)(&DAT_18057ee10)[lVar131] +
                           *(float *)(param_2 + 4) * (float)(&DAT_18057ed90)[lVar131] +
                           *(float *)((longlong)param_2 + 0x1c) * (float)(&DAT_18057ed10)[lVar131] +
                           *(float *)(param_2 + 3) * (float)(&DAT_18057ec90)[lVar131] +
                           *(float *)((longlong)param_2 + 0x14) * (float)(&DAT_18057ec10)[lVar131] +
                           *(float *)(param_2 + 2) * (float)(&DAT_18057eb90)[lVar131] +
                           *(float *)((longlong)param_2 + 0xc) * (float)(&DAT_18057eb10)[lVar131] +
                           *(float *)(param_2 + 1) * (float)(&DAT_18057ea90)[lVar131] +
                           *(float *)((longlong)param_2 + 4) * (float)(&DAT_18057ea10)[lVar131] +
                           *(float *)param_2 * (float)(&DAT_18057e990)[lVar131] + 0.0) * dVar130);
      lVar131 = lVar131 + 1;
    } while (lVar131 != 0);
  }
  else {
    fVar3 = *(float *)param_2;
    fVar4 = *(float *)((longlong)param_2 + 4);
    fVar5 = *(float *)(param_2 + 1);
    fVar6 = *(float *)((longlong)param_2 + 0xc);
    fVar7 = *(float *)(param_2 + 2);
    fVar8 = *(float *)((longlong)param_2 + 0x14);
    fVar9 = *(float *)(param_2 + 3);
    fVar10 = *(float *)((longlong)param_2 + 0x1c);
    fVar11 = *(float *)(param_2 + 4);
    fVar12 = *(float *)((longlong)param_2 + 0x24);
    fVar13 = *(float *)(param_2 + 5);
    fVar14 = *(float *)((longlong)param_2 + 0x2c);
    fVar15 = *(float *)(param_2 + 6);
    fVar16 = *(float *)((longlong)param_2 + 0x34);
    fVar17 = *(float *)(param_2 + 7);
    fVar18 = *(float *)((longlong)param_2 + 0x3c);
    fVar19 = *(float *)(param_2 + 8);
    fVar20 = *(float *)((longlong)param_2 + 0x44);
    fVar21 = *(float *)(param_2 + 9);
    fVar22 = *(float *)((longlong)param_2 + 0x4c);
    fVar23 = *(float *)(param_2 + 10);
    fVar24 = *(float *)((longlong)param_2 + 0x54);
    fVar25 = *(float *)(param_2 + 0xb);
    fVar26 = *(float *)((longlong)param_2 + 0x5c);
    fVar27 = *(float *)(param_2 + 0xc);
    fVar28 = *(float *)((longlong)param_2 + 100);
    fVar29 = *(float *)(param_2 + 0xd);
    fVar30 = *(float *)((longlong)param_2 + 0x6c);
    fVar31 = *(float *)(param_2 + 0xe);
    fVar32 = *(float *)((longlong)param_2 + 0x74);
    fVar33 = *(float *)(param_2 + 0xf);
    fVar34 = *(float *)((longlong)param_2 + 0x7c);
    lVar131 = -0x20;
    do {
      fVar132 = (float)(&DAT_18057e994)[lVar131] * fVar3 + _UNK_180010114;
      fVar133 = *(float *)(&UNK_18057e998 + lVar131 * 4) * fVar3 + _UNK_180010118;
      fVar134 = *(float *)(&UNK_18057e99c + lVar131 * 4) * fVar3 + _UNK_18001011c;
      fVar35 = (float)(&DAT_18057ea14)[lVar131];
      fVar36 = *(float *)(&UNK_18057ea18 + lVar131 * 4);
      fVar37 = *(float *)(&UNK_18057ea1c + lVar131 * 4);
      fVar38 = (float)(&DAT_18057ea94)[lVar131];
      fVar39 = *(float *)(&UNK_18057ea98 + lVar131 * 4);
      fVar40 = *(float *)(&UNK_18057ea9c + lVar131 * 4);
      fVar41 = (float)(&DAT_18057eb14)[lVar131];
      fVar42 = *(float *)(&UNK_18057eb18 + lVar131 * 4);
      fVar43 = *(float *)(&UNK_18057eb1c + lVar131 * 4);
      fVar44 = (float)(&DAT_18057eb94)[lVar131];
      fVar45 = *(float *)(&UNK_18057eb98 + lVar131 * 4);
      fVar46 = *(float *)(&UNK_18057eb9c + lVar131 * 4);
      lVar1 = lVar131 * 4;
      fVar47 = *(float *)(&UNK_18057ec14 + lVar1);
      fVar48 = *(float *)(&UNK_18057ec18 + lVar1);
      fVar49 = *(float *)(&UNK_18057ec1c + lVar1);
      lVar1 = lVar131 * 4;
      fVar50 = *(float *)(&UNK_18057ec94 + lVar1);
      fVar51 = *(float *)(&UNK_18057ec98 + lVar1);
      fVar52 = *(float *)(&UNK_18057ec9c + lVar1);
      lVar1 = lVar131 * 4;
      fVar53 = *(float *)(&UNK_18057ed14 + lVar1);
      fVar54 = *(float *)(&UNK_18057ed18 + lVar1);
      fVar55 = *(float *)(&UNK_18057ed1c + lVar1);
      lVar1 = lVar131 * 4;
      fVar56 = *(float *)(&UNK_18057ed94 + lVar1);
      fVar57 = *(float *)(&UNK_18057ed98 + lVar1);
      fVar58 = *(float *)(&UNK_18057ed9c + lVar1);
      lVar1 = lVar131 * 4;
      fVar59 = *(float *)(&UNK_18057ee14 + lVar1);
      fVar60 = *(float *)(&UNK_18057ee18 + lVar1);
      fVar61 = *(float *)(&UNK_18057ee1c + lVar1);
      lVar1 = lVar131 * 4;
      fVar62 = *(float *)(&UNK_18057ee94 + lVar1);
      fVar63 = *(float *)(&UNK_18057ee98 + lVar1);
      fVar64 = *(float *)(&UNK_18057ee9c + lVar1);
      lVar1 = lVar131 * 4;
      fVar65 = *(float *)(&UNK_18057ef14 + lVar1);
      fVar66 = *(float *)(&UNK_18057ef18 + lVar1);
      fVar67 = *(float *)(&UNK_18057ef1c + lVar1);
      lVar1 = lVar131 * 4;
      fVar68 = *(float *)(&UNK_18057ef94 + lVar1);
      fVar69 = *(float *)(&UNK_18057ef98 + lVar1);
      fVar70 = *(float *)(&UNK_18057ef9c + lVar1);
      lVar1 = lVar131 * 4;
      fVar71 = *(float *)(&UNK_18057f014 + lVar1);
      fVar72 = *(float *)(&UNK_18057f018 + lVar1);
      fVar73 = *(float *)(&UNK_18057f01c + lVar1);
      lVar1 = lVar131 * 4;
      fVar74 = *(float *)(&UNK_18057f094 + lVar1);
      fVar75 = *(float *)(&UNK_18057f098 + lVar1);
      fVar76 = *(float *)(&UNK_18057f09c + lVar1);
      lVar1 = lVar131 * 4;
      fVar77 = *(float *)(&UNK_18057f114 + lVar1);
      fVar78 = *(float *)(&UNK_18057f118 + lVar1);
      fVar79 = *(float *)(&UNK_18057f11c + lVar1);
      lVar1 = lVar131 * 4;
      fVar80 = *(float *)(&UNK_18057f194 + lVar1);
      fVar81 = *(float *)(&UNK_18057f198 + lVar1);
      fVar82 = *(float *)(&UNK_18057f19c + lVar1);
      lVar1 = lVar131 * 4;
      fVar83 = *(float *)(&UNK_18057f214 + lVar1);
      fVar84 = *(float *)(&UNK_18057f218 + lVar1);
      fVar85 = *(float *)(&UNK_18057f21c + lVar1);
      lVar1 = lVar131 * 4;
      fVar86 = *(float *)(&UNK_18057f294 + lVar1);
      fVar87 = *(float *)(&UNK_18057f298 + lVar1);
      fVar88 = *(float *)(&UNK_18057f29c + lVar1);
      lVar1 = lVar131 * 4;
      fVar89 = *(float *)(&UNK_18057f314 + lVar1);
      fVar90 = *(float *)(&UNK_18057f318 + lVar1);
      fVar91 = *(float *)(&UNK_18057f31c + lVar1);
      lVar1 = lVar131 * 4;
      fVar92 = *(float *)(&UNK_18057f394 + lVar1);
      fVar93 = *(float *)(&UNK_18057f398 + lVar1);
      fVar94 = *(float *)(&UNK_18057f39c + lVar1);
      lVar1 = lVar131 * 4;
      fVar95 = *(float *)(&UNK_18057f414 + lVar1);
      fVar96 = *(float *)(&UNK_18057f418 + lVar1);
      fVar97 = *(float *)(&UNK_18057f41c + lVar1);
      lVar1 = lVar131 * 4;
      fVar98 = *(float *)(&UNK_18057f494 + lVar1);
      fVar99 = *(float *)(&UNK_18057f498 + lVar1);
      fVar100 = *(float *)(&UNK_18057f49c + lVar1);
      lVar1 = lVar131 * 4;
      fVar101 = *(float *)(&UNK_18057f514 + lVar1);
      fVar102 = *(float *)(&UNK_18057f518 + lVar1);
      fVar103 = *(float *)(&UNK_18057f51c + lVar1);
      lVar1 = lVar131 * 4;
      fVar104 = *(float *)(&UNK_18057f594 + lVar1);
      fVar105 = *(float *)(&UNK_18057f598 + lVar1);
      fVar106 = *(float *)(&UNK_18057f59c + lVar1);
      lVar1 = lVar131 * 4;
      fVar107 = *(float *)(&UNK_18057f614 + lVar1);
      fVar108 = *(float *)(&UNK_18057f618 + lVar1);
      fVar109 = *(float *)(&UNK_18057f61c + lVar1);
      lVar1 = lVar131 * 4;
      fVar110 = *(float *)(&UNK_18057f694 + lVar1);
      fVar111 = *(float *)(&UNK_18057f698 + lVar1);
      fVar112 = *(float *)(&UNK_18057f69c + lVar1);
      lVar1 = lVar131 * 4;
      fVar113 = *(float *)(&UNK_18057f714 + lVar1);
      fVar114 = *(float *)(&UNK_18057f718 + lVar1);
      fVar115 = *(float *)(&UNK_18057f71c + lVar1);
      lVar1 = lVar131 * 4;
      fVar116 = *(float *)(&UNK_18057f794 + lVar1);
      fVar117 = *(float *)(&UNK_18057f798 + lVar1);
      fVar118 = *(float *)(&UNK_18057f79c + lVar1);
      lVar1 = lVar131 * 4;
      fVar119 = *(float *)(&UNK_18057f814 + lVar1);
      fVar120 = *(float *)(&UNK_18057f818 + lVar1);
      fVar121 = *(float *)(&UNK_18057f81c + lVar1);
      lVar1 = lVar131 * 4;
      fVar122 = *(float *)(&UNK_18057f894 + lVar1);
      fVar123 = *(float *)(&UNK_18057f898 + lVar1);
      fVar124 = *(float *)(&UNK_18057f89c + lVar1);
      lVar1 = lVar131 * 4;
      fVar125 = *(float *)((longlong)&PTR_DAT_18057f910 + lVar1 + 4);
      fVar126 = *(float *)((longlong)&PTR_DAT_18057f918 + lVar1);
      fVar127 = *(float *)((longlong)&PTR_DAT_18057f918 + lVar1 + 4);
      pfVar2 = (float *)((longlong)param_1 + (lVar131 + 0x20) * 4);
      *pfVar2 = (float)((double)(*(float *)((longlong)&PTR_DAT_18057f910 + lVar1) * fVar34 +
                                (float)(&DAT_18057f890)[lVar131] * fVar33 +
                                (float)(&DAT_18057f810)[lVar131] * fVar32 +
                                (float)(&DAT_18057f790)[lVar131] * fVar31 +
                                (float)(&DAT_18057f710)[lVar131] * fVar30 +
                                (float)(&DAT_18057f690)[lVar131] * fVar29 +
                                (float)(&DAT_18057f610)[lVar131] * fVar28 +
                                (float)(&DAT_18057f590)[lVar131] * fVar27 +
                                (float)(&DAT_18057f510)[lVar131] * fVar26 +
                                (float)(&DAT_18057f490)[lVar131] * fVar25 +
                                (float)(&DAT_18057f410)[lVar131] * fVar24 +
                                (float)(&DAT_18057f390)[lVar131] * fVar23 +
                                (float)(&DAT_18057f310)[lVar131] * fVar22 +
                                (float)(&DAT_18057f290)[lVar131] * fVar21 +
                                (float)(&DAT_18057f210)[lVar131] * fVar20 +
                                (float)(&DAT_18057f190)[lVar131] * fVar19 +
                                (float)(&DAT_18057f110)[lVar131] * fVar18 +
                                (float)(&DAT_18057f090)[lVar131] * fVar17 +
                                (float)(&DAT_18057f010)[lVar131] * fVar16 +
                                (float)(&DAT_18057ef90)[lVar131] * fVar15 +
                                (float)(&DAT_18057ef10)[lVar131] * fVar14 +
                                (float)(&DAT_18057ee90)[lVar131] * fVar13 +
                                (float)(&DAT_18057ee10)[lVar131] * fVar12 +
                                (float)(&DAT_18057ed90)[lVar131] * fVar11 +
                                (float)(&DAT_18057ed10)[lVar131] * fVar10 +
                                (float)(&DAT_18057ec90)[lVar131] * fVar9 +
                                (float)(&DAT_18057ec10)[lVar131] * fVar8 +
                                (float)(&DAT_18057eb90)[lVar131] * fVar7 +
                                (float)(&DAT_18057eb10)[lVar131] * fVar6 +
                                (float)(&DAT_18057ea90)[lVar131] * fVar5 +
                                (float)(&DAT_18057ea10)[lVar131] * fVar4 +
                                (float)(&DAT_18057e990)[lVar131] * fVar3 + _DAT_180010110) * dVar128
                       );
      pfVar2[1] = (float)((double)(fVar125 * fVar34 +
                                  fVar122 * fVar33 +
                                  fVar119 * fVar32 +
                                  fVar116 * fVar31 +
                                  fVar113 * fVar30 +
                                  fVar110 * fVar29 +
                                  fVar107 * fVar28 +
                                  fVar104 * fVar27 +
                                  fVar101 * fVar26 +
                                  fVar98 * fVar25 +
                                  fVar95 * fVar24 +
                                  fVar92 * fVar23 +
                                  fVar89 * fVar22 +
                                  fVar86 * fVar21 +
                                  fVar83 * fVar20 +
                                  fVar80 * fVar19 +
                                  fVar77 * fVar18 +
                                  fVar74 * fVar17 +
                                  fVar71 * fVar16 +
                                  fVar68 * fVar15 +
                                  fVar65 * fVar14 +
                                  fVar62 * fVar13 +
                                  fVar59 * fVar12 +
                                  fVar56 * fVar11 +
                                  fVar53 * fVar10 +
                                  fVar50 * fVar9 +
                                  fVar47 * fVar8 +
                                  fVar44 * fVar7 +
                                  fVar41 * fVar6 + fVar38 * fVar5 + fVar35 * fVar4 + fVar132) *
                         dVar129);
      pfVar2[2] = (float)((double)(fVar126 * fVar34 +
                                  fVar123 * fVar33 +
                                  fVar120 * fVar32 +
                                  fVar117 * fVar31 +
                                  fVar114 * fVar30 +
                                  fVar111 * fVar29 +
                                  fVar108 * fVar28 +
                                  fVar105 * fVar27 +
                                  fVar102 * fVar26 +
                                  fVar99 * fVar25 +
                                  fVar96 * fVar24 +
                                  fVar93 * fVar23 +
                                  fVar90 * fVar22 +
                                  fVar87 * fVar21 +
                                  fVar84 * fVar20 +
                                  fVar81 * fVar19 +
                                  fVar78 * fVar18 +
                                  fVar75 * fVar17 +
                                  fVar72 * fVar16 +
                                  fVar69 * fVar15 +
                                  fVar66 * fVar14 +
                                  fVar63 * fVar13 +
                                  fVar60 * fVar12 +
                                  fVar57 * fVar11 +
                                  fVar54 * fVar10 +
                                  fVar51 * fVar9 +
                                  fVar48 * fVar8 +
                                  fVar45 * fVar7 +
                                  fVar42 * fVar6 + fVar39 * fVar5 + fVar36 * fVar4 + fVar133) *
                         dVar128);
      pfVar2[3] = (float)((double)(fVar127 * fVar34 +
                                  fVar124 * fVar33 +
                                  fVar121 * fVar32 +
                                  fVar118 * fVar31 +
                                  fVar115 * fVar30 +
                                  fVar112 * fVar29 +
                                  fVar109 * fVar28 +
                                  fVar106 * fVar27 +
                                  fVar103 * fVar26 +
                                  fVar100 * fVar25 +
                                  fVar97 * fVar24 +
                                  fVar94 * fVar23 +
                                  fVar91 * fVar22 +
                                  fVar88 * fVar21 +
                                  fVar85 * fVar20 +
                                  fVar82 * fVar19 +
                                  fVar79 * fVar18 +
                                  fVar76 * fVar17 +
                                  fVar73 * fVar16 +
                                  fVar70 * fVar15 +
                                  fVar67 * fVar14 +
                                  fVar64 * fVar13 +
                                  fVar61 * fVar12 +
                                  fVar58 * fVar11 +
                                  fVar55 * fVar10 +
                                  fVar52 * fVar9 +
                                  fVar49 * fVar8 +
                                  fVar46 * fVar7 +
                                  fVar43 * fVar6 + fVar40 * fVar5 + fVar37 * fVar4 + fVar134) *
                         dVar129);
      lVar131 = lVar131 + 4;
    } while (lVar131 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002cb0
   NAME : rnn_biquad
   SIG  : undefined rnn_biquad(void)
   ======================================================================== */

void rnn_biquad(longlong param_1,float *param_2,longlong param_3,float *param_4,float *param_5,
               uint param_6)

{
  float fVar1;
  ulonglong uVar2;
  float fVar3;
  
                    /* 0x2cb0  4  rnn_biquad */
  if (0 < (int)param_6) {
    uVar2 = 0;
    do {
      fVar1 = *(float *)(param_3 + uVar2 * 4);
      fVar3 = *param_2 + fVar1;
      *param_2 = (*param_4 * fVar1 - *param_5 * fVar3) + param_2[1];
      param_2[1] = param_4[1] * fVar1 - param_5[1] * fVar3;
      *(float *)(param_1 + uVar2 * 4) = fVar3;
      uVar2 = uVar2 + 1;
    } while (param_6 != uVar2);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002d60
   NAME : rnn_pitch_filter
   SIG  : undefined rnn_pitch_filter(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void rnn_pitch_filter(ulonglong param_1,ulonglong param_2,longlong param_3,longlong param_4,
                     longlong param_5,longlong param_6)

{
  longlong lVar1;
  float *pfVar2;
  float *pfVar3;
  int iVar4;
  bool bVar5;
  bool bVar6;
  double dVar7;
  double dVar8;
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
  double dVar22;
  int iVar23;
  uint uVar24;
  ulonglong uVar25;
  longlong lVar26;
  float *pfVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  undefined8 uVar31;
  float fVar32;
  float fVar33;
  undefined1 auStack_1178 [32];
  undefined8 local_1158 [240];
  float local_9d8;
  float fStack_9cc;
  undefined8 local_9c8;
  undefined8 uStack_9c0;
  undefined8 local_9b8;
  undefined8 uStack_9b0;
  undefined8 local_9a8;
  undefined8 uStack_9a0;
  undefined8 local_998;
  undefined8 uStack_990;
  undefined8 local_988;
  undefined8 uStack_980;
  undefined8 local_978;
  undefined8 uStack_970;
  undefined8 local_968;
  undefined8 uStack_960;
  undefined8 local_958;
  undefined8 uStack_950;
  float local_948 [484];
  float local_1b8 [32];
  float local_138 [34];
  ulonglong local_b0;
  undefined8 uStack_48;
  
                    /* 0x2d60  27  rnn_pitch_filter */
  uStack_48 = 0x180002d76;
  local_b0 = DAT_180582000 ^ (ulonglong)auStack_1178;
  lVar26 = 0;
  pfVar27 = local_948;
  memset(pfVar27,0,0x784);
  memset(local_1158,0,0x784);
  dVar22 = DAT_180010120;
  dVar8 = DAT_1800100b0;
  fVar33 = DAT_1800100a8;
  do {
    fVar28 = *(float *)(param_5 + lVar26 * 4);
    fVar32 = *(float *)(param_6 + lVar26 * 4);
    fVar30 = fVar33;
    if (fVar28 <= fVar32) {
      fVar30 = (float)((double)((fVar33 - fVar32 * fVar32) * fVar28 * fVar28) /
                      ((double)((fVar33 - fVar28 * fVar28) * fVar32 * fVar32) + dVar8));
    }
    fVar28 = (float)(~-(uint)(fVar30 < 0.0) & (uint)fVar33 & -(uint)(fVar33 < fVar30));
    if (0.0 <= fVar30) {
      fVar32 = fVar30;
      if (fVar30 <= fVar33) goto LAB_180002ef9;
LAB_180002ed5:
      if (0.0 <= fVar28) goto LAB_180002edb;
LAB_180002f02:
      fVar28 = sqrtf(fVar28);
    }
    else {
      fVar32 = fVar28;
      if (fVar33 < fVar30) goto LAB_180002ed5;
LAB_180002ef9:
      fVar28 = fVar32;
      if (fVar28 < 0.0) goto LAB_180002f02;
LAB_180002edb:
      fVar28 = SQRT(fVar28);
    }
    dVar7 = (double)*(float *)(param_3 + lVar26 * 4) /
            ((double)*(float *)(param_4 + lVar26 * 4) + dVar22);
    if (dVar7 < 0.0) {
      dVar7 = sqrt(dVar7);
    }
    else {
      dVar7 = SQRT(dVar7);
    }
    local_948[lVar26 + 0x1e4] = (float)((double)fVar28 * dVar7);
    lVar26 = lVar26 + 1;
    if (lVar26 == 0x20) {
      FUN_1800033e0(pfVar27,local_948 + 0x1e4);
      bVar5 = param_1 < param_2 + 0xf08;
      bVar6 = param_2 < param_1 + 0xf08;
      lVar26 = 0;
      if (!bVar6 || !bVar5) {
        do {
          fVar33 = local_948[lVar26];
          fVar28 = local_948[lVar26 + 1];
          fVar32 = local_948[lVar26 + 2];
          fVar30 = local_948[lVar26 + 3];
          pfVar27 = (float *)(param_2 + lVar26 * 8);
          fVar29 = pfVar27[1];
          fVar9 = pfVar27[2];
          fVar10 = pfVar27[3];
          pfVar2 = (float *)(param_2 + 0x10 + lVar26 * 8);
          fVar11 = *pfVar2;
          fVar12 = pfVar2[1];
          fVar13 = pfVar2[2];
          fVar14 = pfVar2[3];
          pfVar2 = (float *)(param_1 + lVar26 * 8);
          fVar15 = pfVar2[1];
          fVar16 = pfVar2[2];
          fVar17 = pfVar2[3];
          pfVar3 = (float *)(param_1 + 0x10 + lVar26 * 8);
          fVar18 = *pfVar3;
          fVar19 = pfVar3[1];
          fVar20 = pfVar3[2];
          fVar21 = pfVar3[3];
          pfVar3 = (float *)(param_1 + lVar26 * 8);
          *pfVar3 = *pfVar27 * fVar33 + *pfVar2;
          pfVar3[1] = fVar29 * fVar33 + fVar15;
          pfVar3[2] = fVar9 * fVar28 + fVar16;
          pfVar3[3] = fVar10 * fVar28 + fVar17;
          pfVar27 = (float *)(param_1 + 0x10 + lVar26 * 8);
          *pfVar27 = fVar11 * fVar32 + fVar18;
          pfVar27[1] = fVar12 * fVar32 + fVar19;
          pfVar27[2] = fVar13 * fVar30 + fVar20;
          pfVar27[3] = fVar14 * fVar30 + fVar21;
          lVar26 = lVar26 + 4;
        } while (lVar26 != 0x1e0);
        lVar26 = 0x1e0;
        pfVar27 = local_948 + 0x1e0;
      }
      fVar33 = *pfVar27;
      uVar25 = (ulonglong)(uint)((int)lVar26 * 8);
      *(float *)(param_1 + uVar25) =
           *(float *)(param_2 + uVar25) * fVar33 + *(float *)(param_1 + uVar25);
      *(float *)(param_1 + 4 + uVar25) =
           fVar33 * *(float *)(param_2 + 4 + uVar25) + *(float *)(param_1 + 4 + uVar25);
      if (bVar6 && bVar5) {
        lVar26 = lVar26 + -0x1e0;
        do {
          fVar33 = local_948[lVar26 + 0x1e1];
          *(float *)(param_1 + 0xf08 + lVar26 * 8) =
               *(float *)(param_2 + 0xf08 + lVar26 * 8) * fVar33 +
               *(float *)(param_1 + 0xf08 + lVar26 * 8);
          *(float *)(param_1 + 0xf0c + lVar26 * 8) =
               fVar33 * *(float *)(param_2 + 0xf0c + lVar26 * 8) +
               *(float *)(param_1 + 0xf0c + lVar26 * 8);
          fVar33 = local_948[lVar26 + 0x1e2];
          *(float *)(param_1 + 0xf10 + lVar26 * 8) =
               *(float *)(param_2 + 0xf10 + lVar26 * 8) * fVar33 +
               *(float *)(param_1 + 0xf10 + lVar26 * 8);
          *(float *)(param_1 + 0xf14 + lVar26 * 8) =
               fVar33 * *(float *)(param_2 + 0xf14 + lVar26 * 8) +
               *(float *)(param_1 + 0xf14 + lVar26 * 8);
          lVar26 = lVar26 + 2;
        } while (lVar26 != 0);
      }
      local_138[0x1c] = 0.0;
      local_138[0x1d] = 0.0;
      local_138[0x1e] = 0.0;
      local_138[0x1f] = 0.0;
      local_138[0x18] = 0.0;
      local_138[0x19] = 0.0;
      local_138[0x1a] = 0.0;
      local_138[0x1b] = 0.0;
      local_138[0x14] = 0.0;
      local_138[0x15] = 0.0;
      local_138[0x16] = 0.0;
      local_138[0x17] = 0.0;
      local_138[0x10] = 0.0;
      local_138[0x11] = 0.0;
      local_138[0x12] = 0.0;
      local_138[0x13] = 0.0;
      local_138[0xc] = 0.0;
      local_138[0xd] = 0.0;
      local_138[0xe] = 0.0;
      local_138[0xf] = 0.0;
      local_138[8] = 0.0;
      local_138[9] = 0.0;
      local_138[10] = 0.0;
      local_138[0xb] = 0.0;
      local_138[4] = 0.0;
      local_138[5] = 0.0;
      local_138[6] = 0.0;
      local_138[7] = 0.0;
      local_138[0] = 0.0;
      local_138[1] = 0.0;
      local_138[2] = 0.0;
      local_138[3] = 0.0;
      local_138[0x20] = 0.0;
      local_138[0x21] = 0.0;
      lVar26 = 0;
      iVar23 = 0;
      do {
        iVar4 = (&DAT_180010024)[lVar26];
        uVar24 = iVar4 - iVar23;
        if (uVar24 != 0 && iVar23 <= iVar4) {
          uVar31 = *(undefined8 *)(local_138 + lVar26);
          lVar1 = param_1 + 4 + (longlong)iVar23 * 8;
          uVar25 = 0;
          do {
            fVar32 = (float)(int)uVar25 / (float)(int)uVar24;
            fVar33 = *(float *)(lVar1 + -4 + uVar25 * 8);
            fVar28 = *(float *)(lVar1 + uVar25 * 8);
            fVar33 = fVar28 * fVar28 + fVar33 * fVar33;
            uVar31 = CONCAT44((float)((ulonglong)uVar31 >> 0x20) + fVar33 * fVar32,
                              (float)uVar31 + fVar33 * (DAT_1800100a8 - fVar32));
            uVar25 = uVar25 + 1;
          } while (uVar24 != uVar25);
          *(undefined8 *)(local_138 + lVar26) = uVar31;
        }
        lVar26 = lVar26 + 1;
        iVar23 = iVar4;
      } while (lVar26 != 0x21);
      local_138[1] = (local_138[0] + local_138[1] + local_138[0] + local_138[1]) / DAT_1800100ac;
      local_138[0x20] = (local_138[0x20] + 0.0 + local_138[0x20] + 0.0) / DAT_1800100ac;
      local_9b8 = CONCAT44(local_138[6],local_138[5]);
      uStack_9b0 = CONCAT44(local_138[8],local_138[7]);
      local_9a8 = CONCAT44(local_138[10],local_138[9]);
      uStack_9a0 = CONCAT44(local_138[0xc],local_138[0xb]);
      local_998 = CONCAT44(local_138[0xe],local_138[0xd]);
      uStack_990 = CONCAT44(local_138[0x10],local_138[0xf]);
      local_988 = CONCAT44(local_138[0x12],local_138[0x11]);
      uStack_980 = CONCAT44(local_138[0x14],local_138[0x13]);
      local_978 = CONCAT44(local_138[0x16],local_138[0x15]);
      uStack_970 = CONCAT44(local_138[0x18],local_138[0x17]);
      local_968 = CONCAT44(local_138[0x1a],local_138[0x19]);
      uStack_960 = CONCAT44(local_138[0x1c],local_138[0x1b]);
      local_9c8 = CONCAT44(local_138[2],local_138[1]);
      uStack_9c0 = CONCAT44(local_138[4],local_138[3]);
      local_958 = CONCAT44(local_138[0x1e],local_138[0x1d]);
      uStack_950 = CONCAT44(local_138[0x20],local_138[0x1f]);
      lVar26 = 1;
      do {
        dVar8 = (double)*(float *)(param_3 + -4 + lVar26 * 4) /
                ((double)(&fStack_9cc)[lVar26] + dVar22);
        if (dVar8 < 0.0) {
          dVar8 = sqrt(dVar8);
        }
        else {
          dVar8 = SQRT(dVar8);
        }
        local_948[lVar26 + 0x203] = (float)dVar8;
        dVar8 = (double)*(float *)(param_3 + lVar26 * 4) /
                ((double)*(float *)((longlong)&local_9c8 + lVar26 * 4) + dVar22);
        if (dVar8 < 0.0) {
          dVar8 = sqrt(dVar8);
        }
        else {
          dVar8 = SQRT(dVar8);
        }
        local_138[lVar26] = (float)dVar8;
        lVar26 = lVar26 + 2;
      } while (lVar26 != 0x21);
      FUN_1800033e0(local_1158,local_138);
      lVar26 = 0;
      do {
        uVar31 = *(undefined8 *)((longlong)local_1158 + lVar26 * 4);
        fVar30 = (float)uVar31;
        fVar29 = (float)((ulonglong)uVar31 >> 0x20);
        pfVar27 = (float *)(param_1 + lVar26 * 8);
        fVar33 = pfVar27[1];
        fVar28 = pfVar27[2];
        fVar32 = pfVar27[3];
        pfVar2 = (float *)(param_1 + lVar26 * 8);
        *pfVar2 = fVar30 * *pfVar27;
        pfVar2[1] = fVar30 * fVar33;
        pfVar2[2] = fVar29 * fVar28;
        pfVar2[3] = fVar29 * fVar32;
        lVar26 = lVar26 + 2;
      } while (lVar26 != 0x1e0);
      *(ulonglong *)(param_1 + 0xf00) =
           CONCAT44(local_9d8 * (float)((ulonglong)*(undefined8 *)(param_1 + 0xf00) >> 0x20),
                    local_9d8 * (float)*(undefined8 *)(param_1 + 0xf00));
      if ((local_b0 ^ (ulonglong)auStack_1178) == DAT_180582000) {
        return;
      }
                    /* WARNING: Subroutine does not return */
      FUN_18000e470();
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 1800033e0
   NAME : FUN_1800033e0
   SIG  : undefined FUN_1800033e0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800033e0(undefined4 *param_1,undefined4 *param_2)

{
  undefined4 *puVar1;
  float *pfVar2;
  float fVar3;
  undefined4 uVar4;
  int iVar5;
  float fVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  uint uVar17;
  uint uVar18;
  uint uVar19;
  uint uVar20;
  uint uVar21;
  uint uVar22;
  float fVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  int iVar31;
  int iVar32;
  int iVar33;
  int iVar34;
  longlong lVar35;
  uint uVar36;
  int iVar37;
  ulonglong uVar38;
  ulonglong uVar39;
  ulonglong uVar40;
  float fVar41;
  float fVar42;
  uint uVar43;
  uint uVar44;
  uint uVar45;
  uint uVar46;
  undefined1 auVar47 [16];
  undefined1 auVar48 [16];
  
  memset(param_1,0,0x1e1);
  iVar34 = _UNK_18001019c;
  iVar33 = _UNK_180010198;
  iVar32 = _UNK_180010194;
  iVar31 = _DAT_180010190;
  fVar30 = _UNK_18001018c;
  fVar29 = _UNK_180010188;
  fVar28 = _UNK_180010184;
  fVar27 = DAT_180010180;
  fVar26 = _UNK_18001017c;
  fVar25 = _UNK_180010178;
  fVar24 = _UNK_180010174;
  fVar23 = _DAT_180010170;
  uVar22 = _UNK_18001016c;
  uVar21 = _UNK_180010168;
  uVar20 = _UNK_180010164;
  uVar19 = _DAT_180010160;
  uVar18 = _UNK_18001015c;
  uVar17 = _UNK_180010158;
  uVar16 = _UNK_180010154;
  uVar15 = _DAT_180010150;
  uVar14 = _UNK_18001014c;
  uVar13 = _UNK_180010148;
  uVar12 = _UNK_180010144;
  uVar11 = _DAT_180010140;
  uVar10 = _UNK_18001013c;
  uVar9 = _UNK_180010138;
  uVar8 = _UNK_180010134;
  uVar7 = _DAT_180010130;
  fVar6 = DAT_1800100a8;
  lVar35 = 1;
  iVar37 = 2;
  do {
    iVar5 = (&DAT_180010024)[lVar35];
    uVar36 = iVar5 - iVar37;
    if (uVar36 != 0 && iVar37 <= iVar5) {
      fVar41 = (float)(int)uVar36;
      uVar38 = (ulonglong)uVar36;
      puVar1 = param_1 + iVar37;
      if ((uVar36 < 4) ||
         ((puVar1 < param_2 + 0x20 && (param_2 < param_1 + (longlong)iVar37 + uVar38)))) {
        uVar39 = 0;
      }
      else {
        uVar39 = (ulonglong)(uVar36 & 0x7ffffffc);
        fVar42 = (float)param_2[lVar35 + -1];
        fVar3 = (float)param_2[lVar35];
        uVar40 = 0;
        uVar43 = uVar7;
        uVar44 = uVar8;
        uVar45 = uVar9;
        uVar46 = uVar10;
        do {
          auVar47._0_4_ =
               ((float)(uVar43 >> 0x10 | uVar19) - fVar23) + (float)(uVar43 & uVar11 | uVar15);
          auVar47._4_4_ =
               ((float)(uVar44 >> 0x10 | uVar20) - fVar24) + (float)(uVar44 & uVar12 | uVar16);
          auVar47._8_4_ =
               ((float)(uVar45 >> 0x10 | uVar21) - fVar25) + (float)(uVar45 & uVar13 | uVar17);
          auVar47._12_4_ =
               ((float)(uVar46 >> 0x10 | uVar22) - fVar26) + (float)(uVar46 & uVar14 | uVar18);
          auVar48._4_4_ = fVar41;
          auVar48._0_4_ = fVar41;
          auVar48._8_4_ = fVar41;
          auVar48._12_4_ = fVar41;
          auVar48 = divps(auVar47,auVar48);
          pfVar2 = (float *)(puVar1 + uVar40);
          *pfVar2 = auVar48._0_4_ * fVar3 + (fVar27 - auVar48._0_4_) * fVar42;
          pfVar2[1] = auVar48._4_4_ * fVar3 + (fVar28 - auVar48._4_4_) * fVar42;
          pfVar2[2] = auVar48._8_4_ * fVar3 + (fVar29 - auVar48._8_4_) * fVar42;
          pfVar2[3] = auVar48._12_4_ * fVar3 + (fVar30 - auVar48._12_4_) * fVar42;
          uVar40 = uVar40 + 4;
          uVar43 = uVar43 + iVar31;
          uVar44 = uVar44 + iVar32;
          uVar45 = uVar45 + iVar33;
          uVar46 = uVar46 + iVar34;
        } while (uVar39 != uVar40);
        if ((uVar36 & 0x7ffffffc) == uVar36) goto LAB_1800034a0;
      }
      uVar40 = uVar39;
      if ((uVar36 & 1) != 0) {
        fVar42 = (float)(int)uVar39 / fVar41;
        puVar1[uVar39] =
             (fVar6 - fVar42) * (float)param_2[lVar35 + -1] + fVar42 * (float)param_2[lVar35];
        uVar40 = uVar39 | 1;
      }
      if (uVar39 != uVar38 - 1) {
        do {
          fVar42 = (float)(int)uVar40 / fVar41;
          param_1[(longlong)iVar37 + uVar40] =
               (fVar6 - fVar42) * (float)param_2[lVar35 + -1] + fVar42 * (float)param_2[lVar35];
          fVar42 = (float)((int)uVar40 + 1) / fVar41;
          param_1[(longlong)iVar37 + uVar40 + 1] =
               (fVar6 - fVar42) * (float)param_2[lVar35 + -1] + fVar42 * (float)param_2[lVar35];
          uVar40 = uVar40 + 2;
        } while (uVar40 != uVar38);
      }
    }
LAB_1800034a0:
    lVar35 = lVar35 + 1;
    iVar37 = iVar5;
    if (lVar35 == 0x20) {
      uVar4 = *param_2;
      *param_1 = uVar4;
      param_1[1] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x164] = uVar4;
      param_1[0x165] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x166] = uVar4;
      param_1[0x167] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x168] = uVar4;
      param_1[0x169] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x16a] = uVar4;
      param_1[0x16b] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x16c] = uVar4;
      param_1[0x16d] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x16e] = uVar4;
      param_1[0x16f] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x170] = uVar4;
      param_1[0x171] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x172] = uVar4;
      param_1[0x173] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x174] = uVar4;
      param_1[0x175] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x176] = uVar4;
      param_1[0x177] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x178] = uVar4;
      param_1[0x179] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x17a] = uVar4;
      param_1[0x17b] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x17c] = uVar4;
      param_1[0x17d] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x17e] = uVar4;
      param_1[0x17f] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x180] = uVar4;
      param_1[0x181] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x182] = uVar4;
      param_1[0x183] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x184] = uVar4;
      param_1[0x185] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x186] = uVar4;
      param_1[0x187] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x188] = uVar4;
      param_1[0x189] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x18a] = uVar4;
      param_1[0x18b] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x18c] = uVar4;
      param_1[0x18d] = uVar4;
      uVar4 = param_2[0x1f];
      param_1[0x18e] = uVar4;
      param_1[399] = uVar4;
      return;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 180003890
   NAME : rnnoise_process_frame
   SIG  : undefined rnnoise_process_frame(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnnoise_process_frame(longlong param_1,longlong param_2,longlong param_3)

{
  float *pfVar1;
  float fVar2;
  undefined8 uVar3;
  double dVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  float fVar11;
  float fVar12;
  float fVar13;
  double dVar14;
  int iVar15;
  longlong lVar16;
  uint *puVar17;
  float *pfVar18;
  longlong lVar19;
  float fVar20;
  float fVar21;
  undefined1 auVar22 [16];
  float fVar23;
  undefined1 auVar24 [16];
  undefined1 auVar25 [16];
  undefined1 auVar26 [16];
  undefined1 auVar27 [16];
  undefined1 auVar28 [16];
  undefined1 auVar29 [16];
  undefined1 auVar30 [16];
  undefined1 auVar31 [16];
  float fVar32;
  double dVar33;
  undefined1 auVar34 [16];
  undefined1 auVar35 [16];
  undefined1 auVar36 [16];
  undefined1 auVar37 [16];
  undefined1 auVar38 [16];
  undefined1 auVar39 [16];
  undefined1 auVar40 [16];
  undefined1 auVar41 [16];
  undefined1 auVar42 [16];
  undefined1 auVar43 [16];
  undefined1 auVar44 [16];
  undefined1 auVar45 [16];
  undefined1 auVar46 [16];
  undefined1 auVar47 [16];
  undefined1 auVar48 [16];
  undefined1 auVar49 [16];
  double dVar50;
  undefined1 auVar51 [16];
  undefined1 auVar52 [16];
  undefined1 auVar53 [16];
  undefined1 auVar54 [16];
  undefined1 auVar55 [16];
  undefined1 auVar56 [16];
  undefined1 auVar57 [16];
  undefined1 auVar58 [16];
  undefined1 auVar59 [16];
  undefined1 auVar60 [16];
  undefined1 auVar61 [16];
  undefined1 auVar62 [16];
  undefined1 auVar63 [16];
  undefined1 auVar64 [16];
  undefined1 auVar65 [16];
  undefined1 auVar66 [16];
  undefined1 auStack_7bd8 [32];
  undefined8 *local_7bb8;
  undefined8 *local_7bb0;
  undefined1 *local_7ba8;
  float *local_7ba0;
  undefined4 local_7b8c;
  undefined8 local_7b88 [240];
  float local_7408;
  undefined1 local_73f8 [16];
  undefined1 local_73e8 [16];
  undefined1 local_73d8 [16];
  undefined1 local_73c8 [16];
  undefined1 local_73b8 [16];
  undefined1 local_73a8 [16];
  undefined1 local_7398 [16];
  undefined1 local_7388 [16];
  undefined1 local_7378 [272];
  undefined8 local_7268;
  undefined8 uStack_7260;
  undefined4 local_7258;
  undefined4 uStack_7254;
  undefined4 uStack_7250;
  undefined4 uStack_724c;
  undefined8 local_7248;
  undefined8 uStack_7240;
  undefined8 local_7238;
  undefined8 uStack_7230;
  undefined8 local_7228;
  undefined8 uStack_7220;
  undefined8 local_7218;
  undefined8 uStack_7210;
  undefined8 local_7208;
  undefined8 uStack_7200;
  undefined8 local_71f8;
  undefined8 uStack_71f0;
  undefined8 local_71e8;
  undefined8 uStack_71e0;
  undefined8 local_71d8;
  undefined8 uStack_71d0;
  undefined8 local_71c8;
  undefined8 uStack_71c0;
  undefined8 local_71b8;
  undefined8 uStack_71b0;
  undefined8 local_71a8;
  undefined8 uStack_71a0;
  undefined8 local_7198;
  undefined8 uStack_7190;
  undefined8 local_7188;
  undefined8 uStack_7180;
  undefined8 local_7178;
  undefined8 uStack_7170;
  undefined8 local_7168;
  undefined8 uStack_7160;
  undefined8 local_7158;
  undefined8 uStack_7150;
  undefined8 local_7148;
  undefined8 uStack_7140;
  undefined8 local_7138;
  undefined8 uStack_7130;
  undefined8 local_7128;
  undefined8 uStack_7120;
  undefined8 local_7118;
  undefined8 uStack_7110;
  undefined8 local_7108;
  undefined8 uStack_7100;
  undefined8 local_70f8;
  undefined8 uStack_70f0;
  float local_70e8 [480];
  undefined1 local_6968 [3856];
  undefined1 local_5a58 [3852];
  float afStack_4b4c [481];
  undefined1 local_43c8 [1920];
  float local_3c48 [1920];
  uint local_1e48 [956];
  uint local_f58 [966];
  ulonglong local_40;
  undefined8 uStack_28;
  
                    /* 0x3890  40  rnnoise_process_frame */
  uStack_28 = 0x18000389f;
  local_40 = DAT_180582000 ^ (ulonglong)auStack_7bd8;
  lVar19 = 0;
  memset(local_7b88,0,0x784);
  dVar14 = DAT_1800101a8;
  dVar4 = DAT_1800101a0;
  local_7b88[0]._0_4_ = 0x3f800000;
  local_7b8c = 0;
  fVar32 = *(float *)(param_1 + 0x4790);
  fVar23 = *(float *)(param_1 + 0x4794);
  do {
    fVar2 = *(float *)(param_3 + lVar19 * 4);
    fVar21 = fVar32 + fVar2;
    dVar33 = (double)fVar2;
    dVar50 = (double)fVar21;
    fVar32 = (float)((dVar50 * dVar4 - (dVar33 + dVar33)) + (double)fVar23);
    *(float *)(param_1 + 0x4790) = fVar32;
    fVar23 = (float)(dVar50 * dVar14 + dVar33);
    *(float *)(param_1 + 0x4794) = fVar23;
    local_70e8[lVar19] = fVar21;
    lVar19 = lVar19 + 1;
  } while (lVar19 != 0x1e0);
  local_7ba0 = local_70e8;
  local_7bb0 = &local_7268;
  local_7bb8 = &local_71e8;
  local_7ba8 = local_7378;
  iVar15 = rnn_compute_frame_features(param_1,local_5a58,local_6968,&local_7168);
  if (iVar15 == 0) {
    local_7bb0 = (undefined8 *)CONCAT44(local_7bb0._4_4_,*(undefined4 *)(param_1 + 0x280));
    local_7bb8 = (undefined8 *)local_7378;
    compute_rnn(param_1,param_1 + 0x4818,local_73f8,&local_7b8c);
    local_7bb8 = (undefined8 *)(param_1 + 0x7f30);
    lVar19 = param_1 + 0x6020;
    local_7bb0 = (undefined8 *)local_73f8;
    rnn_pitch_filter(lVar19,param_1 + 0x6f28,param_1 + 0x7e30,param_1 + 0x7eb0);
    auVar22 = _DAT_1800101d0;
    dVar33 = _UNK_1800101c8;
    dVar14 = _DAT_1800101c0;
    fVar21 = _UNK_1800101bc;
    fVar2 = _UNK_1800101b8;
    fVar23 = _UNK_1800101b4;
    fVar32 = _DAT_1800101b0;
    auVar52._0_4_ = *(float *)(param_1 + 0x4798) * _DAT_1800101b0;
    auVar52._4_4_ = *(float *)(param_1 + 0x479c) * _UNK_1800101b4;
    auVar52._8_4_ = *(float *)(param_1 + 0x47a0) * _UNK_1800101b8;
    auVar52._12_4_ = *(float *)(param_1 + 0x47a4) * _UNK_1800101bc;
    local_73f8 = maxps(local_73f8,auVar52);
    auVar51._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e38) + _DAT_1800101c0) *
         (double)local_73f8._8_4_;
    auVar51._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e38) >> 0x20) + _UNK_1800101c8) *
         (double)local_73f8._12_4_;
    auVar34._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e30) + _DAT_1800101c0) *
         (double)local_73f8._0_4_;
    auVar34._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e30) >> 0x20) + _UNK_1800101c8) *
         (double)local_73f8._4_4_;
    auVar60._0_8_ = (double)(float)local_7168 + _DAT_1800101c0;
    auVar60._8_8_ = (double)(float)((ulonglong)local_7168 >> 0x20) + _UNK_1800101c8;
    auVar35 = divpd(auVar34,auVar60);
    auVar24._0_8_ = (double)(float)uStack_7160 + _DAT_1800101c0;
    auVar24._8_8_ = (double)(float)((ulonglong)uStack_7160 >> 0x20) + _UNK_1800101c8;
    auVar52 = divpd(auVar51,auVar24);
    auVar35 = minpd(_DAT_1800101d0,auVar35);
    auVar52 = minpd(_DAT_1800101d0,auVar52);
    *(ulonglong *)(param_1 + 0x4798) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47a0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar36._0_4_ = *(float *)(param_1 + 0x47a8) * fVar32;
    auVar36._4_4_ = *(float *)(param_1 + 0x47ac) * fVar23;
    auVar36._8_4_ = *(float *)(param_1 + 0x47b0) * fVar2;
    auVar36._12_4_ = *(float *)(param_1 + 0x47b4) * fVar21;
    local_73e8 = maxps(local_73e8,auVar36);
    auVar61._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e48) + dVar14) * (double)local_73e8._8_4_;
    auVar61._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e48) >> 0x20) + dVar33) *
         (double)local_73e8._12_4_;
    auVar53._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e40) + dVar14) * (double)local_73e8._0_4_;
    auVar53._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e40) >> 0x20) + dVar33) *
         (double)local_73e8._4_4_;
    auVar37._0_8_ = (double)(float)local_7158 + dVar14;
    auVar37._8_8_ = (double)(float)((ulonglong)local_7158 >> 0x20) + dVar33;
    auVar35 = divpd(auVar53,auVar37);
    auVar25._0_8_ = (double)(float)uStack_7150 + dVar14;
    auVar25._8_8_ = (double)(float)((ulonglong)uStack_7150 >> 0x20) + dVar33;
    auVar52 = divpd(auVar61,auVar25);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47a8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47b0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar38._0_4_ = *(float *)(param_1 + 0x47b8) * fVar32;
    auVar38._4_4_ = *(float *)(param_1 + 0x47bc) * fVar23;
    auVar38._8_4_ = *(float *)(param_1 + 0x47c0) * fVar2;
    auVar38._12_4_ = *(float *)(param_1 + 0x47c4) * fVar21;
    local_73d8 = maxps(local_73d8,auVar38);
    auVar62._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e58) + dVar14) * (double)local_73d8._8_4_;
    auVar62._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e58) >> 0x20) + dVar33) *
         (double)local_73d8._12_4_;
    auVar54._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e50) + dVar14) * (double)local_73d8._0_4_;
    auVar54._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e50) >> 0x20) + dVar33) *
         (double)local_73d8._4_4_;
    auVar39._0_8_ = (double)(float)local_7148 + dVar14;
    auVar39._8_8_ = (double)(float)((ulonglong)local_7148 >> 0x20) + dVar33;
    auVar35 = divpd(auVar54,auVar39);
    auVar26._0_8_ = (double)(float)uStack_7140 + dVar14;
    auVar26._8_8_ = (double)(float)((ulonglong)uStack_7140 >> 0x20) + dVar33;
    auVar52 = divpd(auVar62,auVar26);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47b8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47c0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar40._0_4_ = *(float *)(param_1 + 0x47c8) * fVar32;
    auVar40._4_4_ = *(float *)(param_1 + 0x47cc) * fVar23;
    auVar40._8_4_ = *(float *)(param_1 + 0x47d0) * fVar2;
    auVar40._12_4_ = *(float *)(param_1 + 0x47d4) * fVar21;
    local_73c8 = maxps(local_73c8,auVar40);
    auVar63._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e68) + dVar14) * (double)local_73c8._8_4_;
    auVar63._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e68) >> 0x20) + dVar33) *
         (double)local_73c8._12_4_;
    auVar55._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e60) + dVar14) * (double)local_73c8._0_4_;
    auVar55._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e60) >> 0x20) + dVar33) *
         (double)local_73c8._4_4_;
    auVar41._0_8_ = (double)(float)local_7138 + dVar14;
    auVar41._8_8_ = (double)(float)((ulonglong)local_7138 >> 0x20) + dVar33;
    auVar35 = divpd(auVar55,auVar41);
    auVar27._0_8_ = (double)(float)uStack_7130 + dVar14;
    auVar27._8_8_ = (double)(float)((ulonglong)uStack_7130 >> 0x20) + dVar33;
    auVar52 = divpd(auVar63,auVar27);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47c8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47d0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar42._0_4_ = *(float *)(param_1 + 0x47d8) * fVar32;
    auVar42._4_4_ = *(float *)(param_1 + 0x47dc) * fVar23;
    auVar42._8_4_ = *(float *)(param_1 + 0x47e0) * fVar2;
    auVar42._12_4_ = *(float *)(param_1 + 0x47e4) * fVar21;
    local_73b8 = maxps(local_73b8,auVar42);
    auVar64._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e78) + dVar14) * (double)local_73b8._8_4_;
    auVar64._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e78) >> 0x20) + dVar33) *
         (double)local_73b8._12_4_;
    auVar56._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e70) + dVar14) * (double)local_73b8._0_4_;
    auVar56._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e70) >> 0x20) + dVar33) *
         (double)local_73b8._4_4_;
    auVar43._0_8_ = (double)(float)local_7128 + dVar14;
    auVar43._8_8_ = (double)(float)((ulonglong)local_7128 >> 0x20) + dVar33;
    auVar35 = divpd(auVar56,auVar43);
    auVar28._0_8_ = (double)(float)uStack_7120 + dVar14;
    auVar28._8_8_ = (double)(float)((ulonglong)uStack_7120 >> 0x20) + dVar33;
    auVar52 = divpd(auVar64,auVar28);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47d8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47e0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar44._0_4_ = *(float *)(param_1 + 0x47e8) * fVar32;
    auVar44._4_4_ = *(float *)(param_1 + 0x47ec) * fVar23;
    auVar44._8_4_ = *(float *)(param_1 + 0x47f0) * fVar2;
    auVar44._12_4_ = *(float *)(param_1 + 0x47f4) * fVar21;
    local_73a8 = maxps(local_73a8,auVar44);
    auVar65._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e88) + dVar14) * (double)local_73a8._8_4_;
    auVar65._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e88) >> 0x20) + dVar33) *
         (double)local_73a8._12_4_;
    auVar57._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e80) + dVar14) * (double)local_73a8._0_4_;
    auVar57._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e80) >> 0x20) + dVar33) *
         (double)local_73a8._4_4_;
    auVar45._0_8_ = (double)(float)local_7118 + dVar14;
    auVar45._8_8_ = (double)(float)((ulonglong)local_7118 >> 0x20) + dVar33;
    auVar35 = divpd(auVar57,auVar45);
    auVar29._0_8_ = (double)(float)uStack_7110 + dVar14;
    auVar29._8_8_ = (double)(float)((ulonglong)uStack_7110 >> 0x20) + dVar33;
    auVar52 = divpd(auVar65,auVar29);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47e8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x47f0) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar46._0_4_ = *(float *)(param_1 + 0x47f8) * fVar32;
    auVar46._4_4_ = *(float *)(param_1 + 0x47fc) * fVar23;
    auVar46._8_4_ = *(float *)(param_1 + 0x4800) * fVar2;
    auVar46._12_4_ = *(float *)(param_1 + 0x4804) * fVar21;
    local_7398 = maxps(local_7398,auVar46);
    auVar66._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e98) + dVar14) * (double)local_7398._8_4_;
    auVar66._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e98) >> 0x20) + dVar33) *
         (double)local_7398._12_4_;
    auVar58._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7e90) + dVar14) * (double)local_7398._0_4_;
    auVar58._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7e90) >> 0x20) + dVar33) *
         (double)local_7398._4_4_;
    auVar47._0_8_ = (double)(float)local_7108 + dVar14;
    auVar47._8_8_ = (double)(float)((ulonglong)local_7108 >> 0x20) + dVar33;
    auVar35 = divpd(auVar58,auVar47);
    auVar30._0_8_ = (double)(float)uStack_7100 + dVar14;
    auVar30._8_8_ = (double)(float)((ulonglong)uStack_7100 >> 0x20) + dVar33;
    auVar52 = divpd(auVar66,auVar30);
    auVar35 = minpd(auVar22,auVar35);
    auVar52 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x47f8) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x4800) = CONCAT44((float)auVar52._8_8_,(float)auVar52._0_8_);
    auVar48._0_4_ = *(float *)(param_1 + 0x4808) * fVar32;
    auVar48._4_4_ = *(float *)(param_1 + 0x480c) * fVar23;
    auVar48._8_4_ = *(float *)(param_1 + 0x4810) * fVar2;
    auVar48._12_4_ = *(float *)(param_1 + 0x4814) * fVar21;
    local_7388 = maxps(local_7388,auVar48);
    auVar59._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7ea8) + dVar14) * (double)local_7388._8_4_;
    auVar59._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7ea8) >> 0x20) + dVar33) *
         (double)local_7388._12_4_;
    auVar49._0_8_ =
         ((double)(float)*(undefined8 *)(param_1 + 0x7ea0) + dVar14) * (double)local_7388._0_4_;
    auVar49._8_8_ =
         ((double)(float)((ulonglong)*(undefined8 *)(param_1 + 0x7ea0) >> 0x20) + dVar33) *
         (double)local_7388._4_4_;
    dVar4 = (double)(float)((ulonglong)local_70f8 >> 0x20) + dVar33;
    auVar35._8_4_ = SUB84(dVar4,0);
    auVar35._0_8_ = (double)(float)local_70f8 + dVar14;
    auVar35._12_4_ = (int)((ulonglong)dVar4 >> 0x20);
    auVar35 = divpd(auVar49,auVar35);
    auVar31._0_8_ = (double)(float)uStack_70f0 + dVar14;
    auVar31._8_8_ = (double)(float)((ulonglong)uStack_70f0 >> 0x20) + dVar33;
    auVar52 = divpd(auVar59,auVar31);
    auVar35 = minpd(auVar22,auVar35);
    auVar22 = minpd(auVar22,auVar52);
    *(ulonglong *)(param_1 + 0x4808) = CONCAT44((float)auVar35._8_8_,(float)auVar35._0_8_);
    *(ulonglong *)(param_1 + 0x4810) = CONCAT44((float)auVar22._8_8_,(float)auVar22._0_8_);
    FUN_1800033e0(local_7b88,local_73f8);
    lVar16 = 0;
    do {
      uVar3 = *(undefined8 *)((longlong)local_7b88 + lVar16 * 4);
      pfVar18 = (float *)(lVar19 + lVar16 * 8);
      fVar32 = pfVar18[1];
      fVar23 = pfVar18[2];
      fVar2 = pfVar18[3];
      fVar20 = (float)((ulonglong)uVar3 >> 0x20);
      fVar21 = (float)uVar3;
      pfVar1 = (float *)(lVar19 + lVar16 * 8);
      *pfVar1 = fVar21 * *pfVar18;
      pfVar1[1] = fVar21 * fVar32;
      pfVar1[2] = fVar20 * fVar23;
      pfVar1[3] = fVar20 * fVar2;
      lVar16 = lVar16 + 2;
    } while (lVar16 != 0x1e0);
    *(ulonglong *)(param_1 + 0x6f20) =
         CONCAT44(local_7408 * (float)((ulonglong)*(undefined8 *)(param_1 + 0x6f20) >> 0x20),
                  local_7408 * (float)*(undefined8 *)(param_1 + 0x6f20));
  }
  memcpy(local_1e48,(void *)(param_1 + 0x6020),0xf08);
  lVar19 = 0x1e1;
  puVar17 = local_f58 + 3;
  while( true ) {
    local_1e48[lVar19 * 2] = puVar17[-1];
    local_1e48[lVar19 * 2 + 1] = *puVar17 ^ (uint)_DAT_180010000;
    if (lVar19 == 0x3bf) break;
    local_1e48[lVar19 * 2 + 2] = puVar17[-3];
    local_1e48[lVar19 * 2 + 3] = puVar17[-2] ^ (uint)_DAT_180010000;
    lVar19 = lVar19 + 2;
    puVar17 = puVar17 + -4;
  }
  rnn_fft_c(&DAT_18057e140,local_1e48,local_3c48);
  afStack_4b4c[1] = local_3c48[0] * DAT_1800101e0;
  lVar19 = 0xef0;
  pfVar18 = afStack_4b4c + 5;
  while( true ) {
    pfVar18[-3] = *(float *)((longlong)local_3c48 + lVar19 * 2 + 0x18) * DAT_1800101e0;
    pfVar18[-2] = *(float *)((longlong)local_3c48 + lVar19 * 2 + 0x10) * DAT_1800101e0;
    pfVar18[-1] = *(float *)((longlong)local_3c48 + lVar19 * 2 + 8) * DAT_1800101e0;
    if (lVar19 == 0) break;
    *pfVar18 = *(float *)((longlong)local_3c48 + lVar19 * 2) * DAT_1800101e0;
    pfVar18 = pfVar18 + 4;
    lVar19 = lVar19 + -0x10;
  }
  lVar19 = 1;
  lVar16 = 0xefc;
  do {
    fVar32 = *(float *)(&UNK_18057e18c + lVar19 * 4);
    fVar23 = (float)(&DAT_18057e190)[lVar19];
    afStack_4b4c[lVar19] = afStack_4b4c[lVar19] * fVar32;
    *(float *)((longlong)afStack_4b4c + lVar16 + 4) =
         fVar32 * *(float *)((longlong)afStack_4b4c + lVar16 + 4);
    afStack_4b4c[lVar19 + 1] = afStack_4b4c[lVar19 + 1] * fVar23;
    *(float *)((longlong)afStack_4b4c + lVar16) =
         fVar23 * *(float *)((longlong)afStack_4b4c + lVar16);
    lVar19 = lVar19 + 2;
    lVar16 = lVar16 + -8;
  } while (lVar19 != 0x1e1);
  if ((param_2 - param_1) - 0xa08U < 0x20) {
    lVar19 = 0;
    do {
      *(float *)(param_2 + lVar19 * 4) =
           afStack_4b4c[lVar19 + 1] + *(float *)(param_1 + 0xa08 + lVar19 * 4);
      *(float *)(param_2 + 4 + lVar19 * 4) =
           afStack_4b4c[lVar19 + 2] + *(float *)(param_1 + 0xa0c + lVar19 * 4);
      *(float *)(param_2 + 8 + lVar19 * 4) =
           afStack_4b4c[lVar19 + 3] + *(float *)(param_1 + 0xa10 + lVar19 * 4);
      *(float *)(param_2 + 0xc + lVar19 * 4) =
           afStack_4b4c[lVar19 + 4] + *(float *)(param_1 + 0xa14 + lVar19 * 4);
      lVar19 = lVar19 + 4;
    } while (lVar19 != 0x1e0);
  }
  else {
    lVar19 = 0;
    do {
      pfVar1 = (float *)(param_1 + 0xa08 + lVar19 * 4);
      fVar32 = pfVar1[1];
      fVar23 = pfVar1[2];
      fVar2 = pfVar1[3];
      pfVar18 = (float *)(param_1 + 0xa18 + lVar19 * 4);
      fVar21 = *pfVar18;
      fVar20 = pfVar18[1];
      fVar5 = pfVar18[2];
      fVar6 = pfVar18[3];
      fVar7 = afStack_4b4c[lVar19 + 2];
      fVar8 = afStack_4b4c[lVar19 + 3];
      fVar9 = afStack_4b4c[lVar19 + 4];
      fVar10 = afStack_4b4c[lVar19 + 5];
      fVar11 = afStack_4b4c[lVar19 + 6];
      fVar12 = afStack_4b4c[lVar19 + 7];
      fVar13 = afStack_4b4c[lVar19 + 8];
      pfVar18 = (float *)(param_2 + lVar19 * 4);
      *pfVar18 = *pfVar1 + afStack_4b4c[lVar19 + 1];
      pfVar18[1] = fVar32 + fVar7;
      pfVar18[2] = fVar23 + fVar8;
      pfVar18[3] = fVar2 + fVar9;
      pfVar18 = (float *)(param_2 + 0x10 + lVar19 * 4);
      *pfVar18 = fVar21 + fVar10;
      pfVar18[1] = fVar20 + fVar11;
      pfVar18[2] = fVar5 + fVar12;
      pfVar18[3] = fVar6 + fVar13;
      pfVar1 = (float *)(param_1 + 0xa28 + lVar19 * 4);
      fVar32 = pfVar1[1];
      fVar23 = pfVar1[2];
      fVar2 = pfVar1[3];
      pfVar18 = (float *)(param_1 + 0xa38 + lVar19 * 4);
      fVar21 = *pfVar18;
      fVar20 = pfVar18[1];
      fVar5 = pfVar18[2];
      fVar6 = pfVar18[3];
      fVar7 = afStack_4b4c[lVar19 + 10];
      fVar8 = afStack_4b4c[lVar19 + 0xb];
      fVar9 = afStack_4b4c[lVar19 + 0xc];
      fVar10 = afStack_4b4c[lVar19 + 0xd];
      fVar11 = afStack_4b4c[lVar19 + 0xe];
      fVar12 = afStack_4b4c[lVar19 + 0xf];
      fVar13 = afStack_4b4c[lVar19 + 0x10];
      pfVar18 = (float *)(param_2 + 0x20 + lVar19 * 4);
      *pfVar18 = *pfVar1 + afStack_4b4c[lVar19 + 9];
      pfVar18[1] = fVar32 + fVar7;
      pfVar18[2] = fVar23 + fVar8;
      pfVar18[3] = fVar2 + fVar9;
      pfVar18 = (float *)(param_2 + 0x30 + lVar19 * 4);
      *pfVar18 = fVar21 + fVar10;
      pfVar18[1] = fVar20 + fVar11;
      pfVar18[2] = fVar5 + fVar12;
      pfVar18[3] = fVar6 + fVar13;
      lVar19 = lVar19 + 0x10;
    } while (lVar19 != 0x1e0);
  }
  memcpy((void *)(param_1 + 0xa08),local_43c8,0x780);
  memcpy((void *)(param_1 + 0x6020),local_5a58,0xf08);
  memcpy((void *)(param_1 + 0x6f28),local_6968,0xf08);
  *(undefined8 *)(param_1 + 0x7ea0) = local_70f8;
  *(undefined8 *)(param_1 + 0x7ea8) = uStack_70f0;
  *(undefined8 *)(param_1 + 0x7e90) = local_7108;
  *(undefined8 *)(param_1 + 0x7e98) = uStack_7100;
  *(undefined8 *)(param_1 + 0x7e80) = local_7118;
  *(undefined8 *)(param_1 + 0x7e88) = uStack_7110;
  *(undefined8 *)(param_1 + 0x7e70) = local_7128;
  *(undefined8 *)(param_1 + 0x7e78) = uStack_7120;
  *(undefined8 *)(param_1 + 0x7e60) = local_7138;
  *(undefined8 *)(param_1 + 0x7e68) = uStack_7130;
  *(undefined8 *)(param_1 + 0x7e50) = local_7148;
  *(undefined8 *)(param_1 + 0x7e58) = uStack_7140;
  *(undefined8 *)(param_1 + 0x7e40) = local_7158;
  *(undefined8 *)(param_1 + 0x7e48) = uStack_7150;
  *(undefined8 *)(param_1 + 0x7e30) = local_7168;
  *(undefined8 *)(param_1 + 0x7e38) = uStack_7160;
  *(undefined8 *)(param_1 + 0x7eb0) = local_71e8;
  *(undefined8 *)(param_1 + 0x7eb8) = uStack_71e0;
  *(undefined8 *)(param_1 + 0x7f20) = local_7178;
  *(undefined8 *)(param_1 + 0x7f28) = uStack_7170;
  *(undefined8 *)(param_1 + 0x7f10) = local_7188;
  *(undefined8 *)(param_1 + 0x7f18) = uStack_7180;
  *(undefined8 *)(param_1 + 0x7f00) = local_7198;
  *(undefined8 *)(param_1 + 0x7f08) = uStack_7190;
  *(undefined8 *)(param_1 + 0x7ef0) = local_71a8;
  *(undefined8 *)(param_1 + 0x7ef8) = uStack_71a0;
  *(undefined8 *)(param_1 + 0x7ee0) = local_71b8;
  *(undefined8 *)(param_1 + 0x7ee8) = uStack_71b0;
  *(undefined8 *)(param_1 + 0x7ed0) = local_71c8;
  *(undefined8 *)(param_1 + 0x7ed8) = uStack_71c0;
  *(undefined8 *)(param_1 + 0x7ec0) = local_71d8;
  *(undefined8 *)(param_1 + 0x7ec8) = uStack_71d0;
  *(undefined8 *)(param_1 + 0x7f30) = local_7268;
  *(undefined8 *)(param_1 + 0x7f38) = uStack_7260;
  *(undefined4 *)(param_1 + 0x7f40) = local_7258;
  *(undefined4 *)(param_1 + 0x7f44) = uStack_7254;
  *(undefined4 *)(param_1 + 0x7f48) = uStack_7250;
  *(undefined4 *)(param_1 + 0x7f4c) = uStack_724c;
  *(undefined8 *)(param_1 + 0x7f50) = local_7248;
  *(undefined8 *)(param_1 + 0x7f58) = uStack_7240;
  *(undefined8 *)(param_1 + 0x7f60) = local_7238;
  *(undefined8 *)(param_1 + 0x7f68) = uStack_7230;
  *(undefined8 *)(param_1 + 0x7f70) = local_7228;
  *(undefined8 *)(param_1 + 0x7f78) = uStack_7220;
  *(undefined8 *)(param_1 + 0x7f80) = local_7218;
  *(undefined8 *)(param_1 + 0x7f88) = uStack_7210;
  *(undefined8 *)(param_1 + 0x7f90) = local_7208;
  *(undefined8 *)(param_1 + 0x7f98) = uStack_7200;
  *(undefined8 *)(param_1 + 0x7fa0) = local_71f8;
  *(undefined8 *)(param_1 + 0x7fa8) = uStack_71f0;
  if ((local_40 ^ (ulonglong)auStack_7bd8) != DAT_180582000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000e470();
  }
  return;
}



/* ========================================================================
   ENTRY: 180004360
   NAME : rnn_fft_alloc_arch_c
   SIG  : undefined rnn_fft_alloc_arch_c(void)
   ======================================================================== */

undefined8 rnn_fft_alloc_arch_c(void)

{
                    /* 0x4360  15  rnn_fft_alloc_arch_c */
  return 0;
}



/* ========================================================================
   ENTRY: 180004370
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
  
                    /* 0x4370  16  rnn_fft_alloc_twiddles */
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
  _Memory[1] = (uint)(DAT_1800100a8 / (float)(int)param_1);
  if (param_4 == (uint *)0x0) {
    pvVar6 = malloc((longlong)(int)param_1 << 3);
    *(void **)(_Memory + 0xe) = pvVar6;
    if (0 < (int)param_1) {
      dVar20 = DAT_1800101e8 / (double)(int)param_1;
      if (param_1 == 1) {
        uVar17 = 0;
      }
      else {
        uVar17 = 0;
        do {
          dVar19 = (double)(int)uVar17 * dVar20;
          dVar18 = cos(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8) = (float)dVar18;
          dVar19 = sin(dVar19);
          *(float *)((longlong)pvVar6 + uVar17 * 8 + 4) = (float)dVar19;
          dVar19 = (double)((int)uVar17 + 1) * dVar20;
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
      goto LAB_1800049c7;
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
      if (5 < iVar15) goto LAB_1800049c7;
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
    FUN_180004a20(0,pvVar6,1,puVar1);
    return _Memory;
  }
LAB_1800049c7:
  free(*(void **)(_Memory + 0xc));
  if ((int)_Memory[2] < 0) {
    free(*(void **)(_Memory + 0xe));
  }
  free(_Memory);
  return (uint *)0x0;
}



/* ========================================================================
   ENTRY: 180004a20
   NAME : FUN_180004a20
   SIG  : undefined FUN_180004a20(void)
   ======================================================================== */

void FUN_180004a20(int param_1,int *param_2,longlong param_3,ushort *param_4)

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
      FUN_180004a20(param_1,param_2,lVar7 * param_3,param_4 + 2);
      param_1 = param_1 + (short)uVar2;
      param_2 = param_2 + param_3;
      uVar5 = uVar5 - 1;
    } while (uVar5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180004b80
   NAME : rnn_fft_free
   SIG  : undefined rnn_fft_free(void)
   ======================================================================== */

void rnn_fft_free(void *param_1)

{
                    /* 0x4b80  18  rnn_fft_free */
  if (param_1 != (void *)0x0) {
    free(*(void **)((longlong)param_1 + 0x30));
    if (*(int *)((longlong)param_1 + 8) < 0) {
      free(*(void **)((longlong)param_1 + 0x38));
    }
                    /* WARNING: Could not recover jumptable at 0x000180004baf. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    free(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180004bc0
   NAME : rnn_fft_alloc
   SIG  : undefined rnn_fft_alloc(void)
   ======================================================================== */

void rnn_fft_alloc(void)

{
                    /* 0x4bc0  14  rnn_fft_alloc */
  rnn_fft_alloc_twiddles();
  return;
}



/* ========================================================================
   ENTRY: 180004be0
   NAME : rnn_fft_free_arch_c
   SIG  : undefined rnn_fft_free_arch_c(void)
   ======================================================================== */

void rnn_fft_free_arch_c(void)

{
                    /* 0x4be0  19  rnn_fft_free_arch_c */
  return;
}



/* ========================================================================
   ENTRY: 180004bf0
   NAME : rnn_fft_impl
   SIG  : undefined rnn_fft_impl(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_fft_impl(longlong param_1,undefined1 (*param_2) [16])

{
  float *pfVar1;
  float *pfVar2;
  float *pfVar3;
  float *pfVar4;
  float *pfVar5;
  float *pfVar6;
  float *pfVar7;
  float *pfVar8;
  undefined8 uVar9;
  undefined8 uVar10;
  short sVar11;
  undefined8 *puVar12;
  undefined1 auVar13 [12];
  undefined1 auVar14 [16];
  undefined1 auVar15 [16];
  undefined1 auVar16 [16];
  undefined1 auVar17 [16];
  undefined1 auVar18 [16];
  undefined1 auVar19 [16];
  undefined1 auVar20 [16];
  undefined1 auVar21 [16];
  undefined1 auVar22 [16];
  undefined1 auVar23 [16];
  undefined1 auVar24 [16];
  undefined1 auVar25 [16];
  undefined1 auVar26 [16];
  undefined1 auVar27 [16];
  undefined1 auVar28 [16];
  undefined1 auVar29 [16];
  undefined1 auVar30 [16];
  undefined1 auVar31 [16];
  undefined1 auVar32 [16];
  undefined1 auVar33 [16];
  undefined1 auVar34 [16];
  undefined1 auVar35 [16];
  undefined1 auVar36 [16];
  undefined1 auVar37 [16];
  undefined1 auVar38 [16];
  undefined1 auVar39 [16];
  undefined1 auVar40 [16];
  undefined1 auVar41 [16];
  undefined1 auVar42 [16];
  undefined1 auVar43 [16];
  undefined1 auVar44 [16];
  undefined1 auVar45 [16];
  undefined1 auVar46 [16];
  undefined1 auVar47 [16];
  undefined1 auVar48 [16];
  undefined1 auVar49 [16];
  undefined1 auVar50 [16];
  undefined1 auVar51 [16];
  undefined1 auVar52 [16];
  int iVar53;
  longlong lVar54;
  byte bVar55;
  bool bVar56;
  bool bVar57;
  uint uVar58;
  undefined1 (*pauVar59) [16];
  undefined1 *puVar60;
  undefined8 *puVar61;
  undefined1 *puVar62;
  undefined1 *puVar63;
  longlong lVar64;
  ulonglong uVar65;
  ulonglong uVar66;
  longlong lVar67;
  longlong lVar68;
  undefined1 *puVar69;
  undefined1 *puVar70;
  longlong lVar71;
  longlong lVar72;
  longlong lVar73;
  undefined1 *puVar74;
  undefined1 *puVar75;
  ulonglong uVar76;
  bool bVar77;
  int iVar98;
  float fVar110;
  undefined1 auVar78 [16];
  undefined1 auVar79 [16];
  undefined1 auVar80 [16];
  int iVar117;
  undefined1 auVar81 [16];
  undefined1 auVar82 [16];
  undefined1 auVar83 [16];
  undefined1 auVar84 [16];
  undefined1 auVar85 [16];
  undefined1 auVar86 [16];
  undefined1 auVar87 [16];
  undefined1 auVar88 [16];
  undefined1 auVar89 [16];
  undefined1 auVar90 [16];
  undefined1 auVar91 [16];
  undefined1 auVar92 [16];
  uint uVar99;
  uint uVar100;
  uint uVar101;
  uint uVar102;
  uint uVar111;
  uint uVar112;
  uint uVar118;
  uint uVar119;
  uint uVar120;
  uint uVar121;
  undefined1 auVar93 [16];
  uint uVar103;
  uint uVar104;
  uint uVar105;
  uint uVar106;
  uint uVar113;
  uint uVar114;
  uint uVar115;
  uint uVar122;
  uint uVar123;
  uint uVar124;
  undefined1 auVar94 [16];
  uint uVar107;
  uint uVar108;
  uint uVar109;
  uint uVar116;
  uint uVar125;
  uint uVar126;
  undefined1 auVar95 [16];
  undefined1 auVar96 [16];
  undefined1 auVar97 [16];
  uint uVar127;
  uint uVar131;
  uint uVar153;
  uint uVar166;
  undefined1 auVar132 [16];
  undefined1 auVar133 [16];
  undefined1 auVar134 [16];
  undefined1 auVar135 [16];
  undefined1 auVar136 [16];
  undefined1 auVar137 [16];
  undefined1 auVar138 [16];
  undefined1 auVar139 [16];
  undefined1 auVar140 [16];
  undefined1 auVar141 [16];
  undefined1 auVar142 [16];
  undefined1 auVar143 [16];
  undefined1 auVar144 [16];
  uint uVar128;
  uint uVar129;
  uint uVar130;
  uint uVar154;
  uint uVar155;
  uint uVar156;
  uint uVar157;
  uint uVar158;
  uint uVar159;
  uint uVar160;
  uint uVar167;
  uint uVar168;
  uint uVar169;
  uint uVar172;
  uint uVar173;
  uint uVar174;
  uint uVar175;
  uint uVar176;
  uint uVar177;
  uint uVar178;
  undefined1 auVar145 [16];
  uint uVar161;
  uint uVar179;
  undefined1 auVar146 [16];
  uint uVar162;
  uint uVar163;
  uint uVar170;
  uint uVar180;
  uint uVar181;
  undefined1 auVar147 [16];
  uint uVar164;
  uint uVar171;
  uint uVar182;
  undefined1 auVar148 [16];
  undefined1 auVar149 [16];
  undefined1 auVar150 [16];
  undefined1 auVar151 [16];
  float fVar165;
  undefined1 auVar152 [16];
  uint uVar183;
  uint uVar204;
  uint uVar212;
  uint uVar217;
  undefined1 auVar188 [16];
  undefined1 auVar189 [16];
  undefined1 auVar190 [16];
  undefined1 auVar191 [16];
  undefined1 auVar192 [16];
  undefined1 auVar193 [16];
  uint uVar184;
  uint uVar185;
  uint uVar186;
  undefined1 auVar194 [16];
  uint uVar205;
  uint uVar206;
  uint uVar207;
  uint uVar208;
  uint uVar213;
  uint uVar214;
  uint uVar215;
  uint uVar218;
  uint uVar219;
  uint uVar220;
  uint uVar221;
  uint uVar222;
  undefined1 auVar195 [16];
  uint uVar209;
  uint uVar223;
  undefined1 auVar196 [16];
  uint uVar187;
  uint uVar210;
  uint uVar211;
  uint uVar216;
  uint uVar224;
  uint uVar225;
  uint uVar226;
  undefined1 auVar197 [16];
  undefined1 auVar198 [16];
  undefined1 auVar199 [16];
  undefined1 auVar200 [16];
  undefined1 auVar202 [16];
  undefined1 auVar203 [16];
  int iVar227;
  int iVar229;
  int iVar245;
  int iVar251;
  undefined1 auVar230 [16];
  undefined1 auVar231 [16];
  undefined1 auVar232 [16];
  int iVar255;
  undefined1 auVar233 [16];
  undefined1 auVar234 [16];
  undefined1 auVar235 [16];
  int iVar228;
  uint uVar246;
  int iVar252;
  uint uVar256;
  undefined1 auVar236 [16];
  undefined1 auVar237 [16];
  undefined1 auVar238 [16];
  undefined1 auVar239 [16];
  undefined1 auVar240 [16];
  uint uVar247;
  uint uVar257;
  undefined1 auVar241 [16];
  uint uVar248;
  uint uVar249;
  int iVar253;
  uint uVar258;
  uint uVar259;
  undefined1 auVar242 [16];
  uint uVar250;
  uint uVar254;
  uint uVar260;
  uint uVar261;
  undefined1 auVar243 [16];
  undefined1 auVar244 [16];
  int iVar262;
  uint uVar263;
  uint uVar264;
  uint uVar265;
  float fVar266;
  uint uVar275;
  float fVar280;
  uint uVar281;
  uint uVar286;
  undefined1 auVar267 [16];
  undefined1 auVar268 [16];
  undefined1 auVar269 [16];
  undefined1 auVar270 [16];
  undefined1 auVar271 [16];
  int iVar282;
  uint uVar287;
  undefined1 auVar272 [16];
  uint uVar276;
  uint uVar277;
  uint uVar278;
  uint uVar279;
  uint uVar283;
  uint uVar284;
  uint uVar285;
  uint uVar288;
  uint uVar289;
  uint uVar290;
  uint uVar291;
  undefined1 auVar273 [16];
  undefined1 auVar274 [16];
  uint uVar292;
  uint uVar301;
  uint uVar304;
  undefined1 auVar293 [16];
  uint uVar302;
  uint uVar306;
  uint uVar307;
  uint uVar308;
  undefined1 auVar294 [16];
  uint uVar303;
  uint uVar309;
  undefined1 auVar295 [16];
  undefined1 auVar296 [16];
  undefined1 auVar297 [16];
  undefined1 auVar298 [16];
  int iVar305;
  int iVar310;
  undefined1 auVar299 [16];
  undefined1 auVar300 [16];
  int iVar311;
  uint uVar312;
  uint uVar313;
  uint uVar314;
  uint uVar315;
  uint uVar316;
  float fVar317;
  uint uVar353;
  float fVar363;
  int iVar364;
  undefined1 auVar318 [16];
  undefined1 auVar319 [16];
  undefined1 auVar320 [16];
  undefined1 auVar321 [16];
  undefined1 auVar322 [16];
  undefined1 auVar323 [16];
  undefined1 auVar324 [16];
  undefined1 auVar325 [16];
  undefined1 auVar326 [16];
  uint uVar370;
  undefined1 auVar327 [16];
  undefined1 auVar328 [16];
  undefined1 auVar329 [16];
  undefined1 auVar330 [16];
  undefined1 auVar331 [16];
  undefined1 auVar332 [16];
  undefined1 auVar333 [16];
  undefined1 auVar334 [16];
  undefined1 auVar335 [16];
  undefined1 auVar336 [16];
  uint uVar354;
  uint uVar355;
  uint uVar356;
  uint uVar371;
  uint uVar372;
  uint uVar373;
  undefined1 auVar337 [16];
  uint uVar357;
  uint uVar358;
  uint uVar359;
  uint uVar360;
  uint uVar361;
  uint uVar362;
  uint uVar365;
  uint uVar366;
  uint uVar367;
  uint uVar368;
  uint uVar369;
  uint uVar374;
  uint uVar375;
  uint uVar376;
  uint uVar377;
  uint uVar378;
  uint uVar379;
  undefined1 auVar338 [16];
  undefined1 auVar339 [16];
  undefined1 auVar340 [16];
  undefined1 auVar341 [16];
  undefined1 auVar342 [16];
  undefined1 auVar343 [16];
  undefined1 auVar344 [16];
  undefined1 auVar345 [16];
  undefined1 auVar346 [16];
  undefined1 auVar347 [16];
  undefined1 auVar348 [16];
  undefined1 auVar349 [16];
  undefined1 auVar350 [16];
  undefined1 auVar351 [16];
  undefined1 auVar352 [16];
  float fVar380;
  uint uVar381;
  uint uVar384;
  uint uVar385;
  uint uVar386;
  float fVar387;
  float fVar388;
  float fVar389;
  ulonglong uVar390;
  float fVar426;
  float fVar427;
  float fVar428;
  int iVar429;
  float fVar436;
  undefined1 auVar391 [16];
  uint uVar382;
  uint uVar416;
  uint uVar417;
  uint uVar430;
  uint uVar431;
  int iVar438;
  uint uVar439;
  uint uVar440;
  undefined1 auVar392 [16];
  undefined1 auVar393 [16];
  undefined1 auVar394 [16];
  undefined1 auVar395 [16];
  undefined1 auVar396 [16];
  undefined1 auVar397 [16];
  undefined1 auVar398 [16];
  undefined1 auVar399 [16];
  undefined1 auVar400 [16];
  undefined1 auVar401 [16];
  undefined1 auVar402 [16];
  undefined1 auVar403 [16];
  undefined1 auVar404 [16];
  undefined1 auVar405 [16];
  undefined1 auVar406 [16];
  uint uVar383;
  uint uVar418;
  uint uVar419;
  uint uVar432;
  ulonglong uVar437;
  uint uVar441;
  uint uVar442;
  undefined1 auVar407 [16];
  uint uVar420;
  uint uVar421;
  uint uVar422;
  uint uVar423;
  uint uVar424;
  uint uVar425;
  uint uVar433;
  uint uVar434;
  uint uVar435;
  uint uVar443;
  uint uVar444;
  uint uVar445;
  uint uVar446;
  uint uVar447;
  undefined1 auVar408 [16];
  undefined1 auVar409 [16];
  undefined1 auVar410 [16];
  undefined1 auVar411 [16];
  undefined1 auVar412 [16];
  undefined1 auVar413 [16];
  undefined1 auVar414 [16];
  undefined1 auVar415 [16];
  float fVar448;
  int iVar449;
  float fVar450;
  uint uVar455;
  float fVar457;
  int iVar458;
  undefined1 auVar451 [16];
  undefined1 auVar452 [16];
  uint uVar456;
  uint uVar459;
  uint uVar460;
  undefined1 auVar453 [16];
  undefined1 auVar454 [16];
  uint uVar461;
  uint uVar462;
  uint uVar463;
  uint uVar464;
  float fVar465;
  float fVar466;
  uint uVar469;
  uint uVar470;
  uint uVar471;
  float fVar472;
  float fVar473;
  uint uVar474;
  uint uVar475;
  uint uVar476;
  undefined1 auVar468 [16];
  uint uVar477;
  uint uVar478;
  uint uVar479;
  int iVar480;
  float fVar481;
  uint uVar492;
  float fVar495;
  int iVar496;
  undefined1 auVar482 [16];
  undefined1 auVar483 [16];
  undefined1 auVar484 [16];
  undefined1 auVar485 [16];
  undefined1 auVar486 [16];
  undefined1 auVar487 [16];
  undefined1 auVar488 [16];
  undefined1 auVar489 [16];
  uint uVar493;
  uint uVar497;
  undefined1 auVar490 [16];
  uint uVar494;
  uint uVar498;
  undefined1 auVar491 [16];
  int iVar499;
  float fVar500;
  uint uVar509;
  float fVar510;
  int iVar511;
  float fVar512;
  undefined1 auVar501 [16];
  undefined1 auVar502 [16];
  undefined1 auVar503 [16];
  undefined1 auVar504 [16];
  undefined1 auVar505 [16];
  undefined1 auVar506 [16];
  undefined1 auVar507 [16];
  uint uVar513;
  undefined1 auVar508 [16];
  float fVar514;
  uint uVar515;
  float fVar516;
  uint uVar527;
  float fVar528;
  uint uVar529;
  float fVar530;
  undefined1 auVar517 [16];
  undefined1 auVar518 [16];
  undefined1 auVar519 [16];
  undefined1 auVar520 [16];
  undefined1 auVar521 [16];
  undefined1 auVar522 [16];
  undefined1 auVar523 [16];
  undefined1 auVar524 [16];
  undefined1 auVar525 [16];
  uint uVar531;
  undefined1 auVar526 [16];
  float fVar532;
  float fVar533;
  float fVar534;
  uint uVar542;
  float fVar544;
  float fVar545;
  float fVar546;
  float fVar547;
  undefined1 auVar535 [16];
  undefined1 auVar536 [16];
  undefined1 auVar537 [16];
  undefined1 auVar538 [16];
  undefined1 auVar539 [16];
  uint uVar548;
  undefined1 auVar540 [16];
  uint uVar543;
  uint uVar549;
  undefined1 auVar541 [16];
  float fVar550;
  float fVar551;
  int iVar552;
  float fVar553;
  float fVar554;
  uint uVar557;
  float fVar558;
  int iVar559;
  float fVar560;
  undefined1 auVar555 [16];
  uint uVar561;
  undefined1 auVar556 [16];
  float fVar562;
  float fVar563;
  float fVar564;
  float fVar567;
  float fVar568;
  undefined1 auVar565 [16];
  undefined1 auVar566 [16];
  float fVar569;
  undefined1 auStack_688 [36];
  uint local_664;
  longlong local_660;
  undefined1 *local_658;
  int local_64c;
  undefined1 local_648 [12];
  uint uStack_63c;
  undefined8 local_638;
  float fStack_630;
  float fStack_62c;
  undefined8 local_628;
  float fStack_620;
  float fStack_61c;
  undefined1 local_618 [16];
  uint local_608;
  uint uStack_604;
  uint uStack_600;
  uint uStack_5fc;
  undefined1 (*local_5f0) [16];
  longlong local_5e8;
  undefined1 *local_5e0;
  undefined1 local_5d8 [16];
  undefined8 local_5c8;
  uint uStack_5c0;
  uint uStack_5bc;
  uint local_5b8;
  uint uStack_5b4;
  uint uStack_5b0;
  uint uStack_5ac;
  undefined1 *local_5a8;
  undefined1 (*local_5a0) [16];
  undefined1 local_598 [16];
  undefined8 local_588;
  uint uStack_580;
  uint uStack_57c;
  undefined8 local_578;
  uint uStack_570;
  uint uStack_56c;
  undefined8 local_568;
  uint uStack_560;
  uint uStack_55c;
  float local_558;
  float fStack_554;
  uint uStack_550;
  uint uStack_54c;
  longlong local_548;
  undefined1 *puStack_540;
  uint local_538;
  uint uStack_534;
  uint uStack_530;
  uint uStack_52c;
  undefined1 *local_528;
  undefined1 *puStack_520;
  undefined1 *local_518;
  longlong local_510;
  longlong local_508;
  longlong local_500;
  ulonglong local_4f8;
  longlong local_4f0;
  uint local_4e8;
  uint uStack_4e4;
  uint uStack_4e0;
  uint uStack_4dc;
  longlong local_4d8;
  undefined1 *puStack_4d0;
  longlong local_4c8;
  undefined8 uStack_4c0;
  undefined8 local_4b8;
  uint uStack_4b0;
  uint uStack_4ac;
  undefined8 local_4a8;
  uint uStack_4a0;
  uint uStack_49c;
  undefined8 local_498;
  uint uStack_490;
  uint uStack_48c;
  undefined8 local_488;
  uint uStack_480;
  uint uStack_47c;
  undefined4 local_478;
  undefined4 uStack_474;
  uint uStack_470;
  uint uStack_46c;
  float local_468;
  float fStack_464;
  uint uStack_460;
  uint uStack_45c;
  bool local_458;
  undefined3 uStack_457;
  uint uStack_454;
  uint uStack_450;
  uint uStack_44c;
  longlong local_448;
  undefined1 *puStack_440;
  undefined1 *local_438;
  longlong local_430;
  longlong local_428;
  ulonglong local_420;
  longlong local_418;
  undefined8 uStack_410;
  float local_408;
  float fStack_404;
  float fStack_400;
  float fStack_3fc;
  undefined1 local_3f8 [16];
  undefined1 local_3e8 [16];
  int local_3d8;
  uint uStack_3d4;
  int iStack_3d0;
  uint uStack_3cc;
  uint local_3c8;
  uint uStack_3c4;
  uint uStack_3c0;
  uint uStack_3bc;
  int local_3b8;
  uint uStack_3b4;
  int iStack_3b0;
  uint uStack_3ac;
  undefined1 local_3a8 [16];
  uint local_398;
  uint uStack_394;
  uint uStack_390;
  uint uStack_38c;
  int local_388;
  uint uStack_384;
  int iStack_380;
  uint uStack_37c;
  uint local_378;
  uint uStack_374;
  uint uStack_370;
  uint uStack_36c;
  uint local_368;
  uint uStack_364;
  undefined8 uStack_360;
  uint local_358;
  uint uStack_354;
  uint uStack_350;
  uint uStack_34c;
  undefined1 local_348 [16];
  undefined1 local_338 [16];
  ulonglong local_328;
  undefined1 *puStack_320;
  ulonglong local_318;
  undefined1 *puStack_310;
  undefined1 *local_308;
  undefined1 *puStack_300;
  undefined1 local_2f8;
  undefined7 uStack_2f7;
  undefined1 *puStack_2f0;
  undefined1 local_2e8;
  undefined7 uStack_2e7;
  undefined1 *puStack_2e0;
  bool local_2d8;
  undefined7 uStack_2d7;
  undefined1 *puStack_2d0;
  bool local_2c8;
  undefined7 uStack_2c7;
  undefined1 *puStack_2c0;
  longlong local_2b8;
  undefined1 *puStack_2b0;
  undefined1 *local_2a8;
  undefined1 *puStack_2a0;
  undefined1 *local_298;
  undefined1 *puStack_290;
  undefined1 *local_288;
  undefined1 *puStack_280;
  undefined1 *local_278;
  undefined1 *puStack_270;
  ulonglong local_268;
  undefined1 *puStack_260;
  longlong local_258;
  undefined8 uStack_250;
  undefined1 *local_248;
  undefined1 *puStack_240;
  longlong local_238;
  undefined1 *puStack_230;
  longlong local_228;
  undefined1 *puStack_220;
  longlong local_218;
  undefined1 *puStack_210;
  longlong local_208;
  undefined1 *puStack_200;
  longlong local_1f8;
  undefined8 uStack_1f0;
  undefined8 local_1e8;
  float fStack_1e0;
  float fStack_1dc;
  undefined8 local_1d8;
  float fStack_1d0;
  float fStack_1cc;
  undefined1 *local_1c0;
  undefined1 *local_1b8;
  undefined8 *local_1b0;
  longlong local_1a8;
  longlong local_1a0;
  undefined1 local_198 [16];
  undefined1 *local_188;
  undefined8 uStack_180;
  undefined1 local_178 [16];
  undefined1 (*local_168) [16];
  undefined8 uStack_160;
  undefined1 *local_158;
  undefined8 uStack_150;
  undefined1 *local_148;
  undefined1 *puStack_140;
  undefined1 local_138 [16];
  undefined1 *local_128;
  undefined1 *puStack_120;
  uint local_118 [10];
  ulonglong local_f0;
  undefined1 auVar201 [16];
  undefined1 auVar467 [16];
  
                    /* 0x4bf0  20  rnn_fft_impl */
  local_f0 = DAT_180582000 ^ (ulonglong)auStack_688;
  local_64c = *(int *)(param_1 + 8);
  local_118[0] = 1;
  uVar58 = 1;
  local_1a8 = -1;
  iVar53 = -1;
  do {
    uVar58 = uVar58 * (int)*(short *)(param_1 + 0x10 + local_1a8 * 4);
    sVar11 = *(short *)(param_1 + 0x12 + local_1a8 * 4);
    local_118[local_1a8 + 2] = uVar58;
    local_1a8 = local_1a8 + 1;
    iVar53 = iVar53 + 2;
  } while (sVar11 != 1);
  if (local_64c < 1) {
    local_64c = 0;
  }
  local_664 = (uint)*(short *)(param_1 + 0xc + (longlong)iVar53 * 2);
  local_1b8 = param_2[-1] + 0xc;
  local_1c0 = *param_2 + 4;
  uStack_160 = 0;
  auVar83._8_4_ = (int)param_2;
  auVar83._0_8_ = param_2;
  auVar83._12_4_ = (int)((ulonglong)param_2 >> 0x20);
  uStack_180 = 0;
  auVar190._8_4_ = (int)local_1c0;
  auVar190._0_8_ = local_1c0;
  auVar190._12_4_ = (int)((ulonglong)local_1c0 >> 0x20);
  local_1b0 = (undefined8 *)(param_2[3] + 8);
  auVar395._4_4_ = _UNK_180010004;
  auVar395._0_4_ = _DAT_180010000;
  auVar395._8_4_ = _UNK_180010008;
  auVar395._12_4_ = _UNK_18001000c;
  local_178 = auVar83 ^ auVar395;
  local_198 = auVar190 ^ auVar395;
  fVar554 = _DAT_1800101f0;
  fVar564 = _UNK_1800101f4;
  do {
    local_5e8 = local_1a8;
    uVar58 = local_664;
    uVar65 = (ulonglong)local_664;
    local_1a8 = local_5e8 + -1;
    if (local_5e8 == 0) {
      local_664 = 1;
      lVar68 = 0;
    }
    else {
      lVar68 = local_5e8 * 2;
      local_664 = (uint)*(short *)(param_1 + 0xc + (ulonglong)((int)lVar68 - 1) * 2);
    }
    bVar55 = (byte)local_64c;
    uVar304 = auVar395._0_4_;
    switch(*(undefined2 *)(param_1 + 0xc + lVar68 * 2)) {
    case 2:
      uVar304 = local_118[local_5e8];
      if (uVar58 == 1) {
        if (0 < (int)uVar304) {
          uVar58 = 0;
          pauVar59 = param_2;
          if (3 < uVar304) {
            uVar58 = uVar304 & 0x7ffffffc;
            pauVar59 = param_2 + uVar58;
            lVar68 = 0;
            do {
              fVar110 = *(float *)(param_2[1] + lVar68 + 8);
              fVar363 = *(float *)(param_2[1] + lVar68 + 0xc);
              fVar165 = *(float *)(param_2[1] + lVar68);
              fVar388 = *(float *)(param_2[1] + lVar68 + 4);
              auVar318._4_4_ =
                   *(float *)(*param_2 + lVar68 + 4) + *(float *)(*param_2 + lVar68 + 0xc);
              auVar318._0_4_ = *(float *)(*param_2 + lVar68) + *(float *)(*param_2 + lVar68 + 8);
              auVar318._8_4_ = *(float *)(*param_2 + lVar68) - *(float *)(*param_2 + lVar68 + 8);
              auVar318._12_4_ =
                   *(float *)(*param_2 + lVar68 + 4) - *(float *)(*param_2 + lVar68 + 0xc);
              auVar391._4_4_ =
                   *(float *)(param_2[3] + lVar68 + 4) + *(float *)(param_2[3] + lVar68 + 0xc);
              auVar391._0_4_ = *(float *)(param_2[3] + lVar68) + *(float *)(param_2[3] + lVar68 + 8)
              ;
              auVar391._8_4_ = *(float *)(param_2[3] + lVar68) - *(float *)(param_2[3] + lVar68 + 8)
              ;
              auVar391._12_4_ =
                   *(float *)(param_2[3] + lVar68 + 4) - *(float *)(param_2[3] + lVar68 + 0xc);
              auVar78._4_4_ =
                   *(float *)(param_2[2] + lVar68 + 4) + *(float *)(param_2[2] + lVar68 + 0xc);
              auVar78._0_4_ = *(float *)(param_2[2] + lVar68) + *(float *)(param_2[2] + lVar68 + 8);
              auVar78._8_4_ = *(float *)(param_2[2] + lVar68) - *(float *)(param_2[2] + lVar68 + 8);
              auVar78._12_4_ =
                   *(float *)(param_2[2] + lVar68 + 4) - *(float *)(param_2[2] + lVar68 + 0xc);
              *(undefined1 (*) [16])(param_2[2] + lVar68) = auVar78;
              *(undefined1 (*) [16])(param_2[3] + lVar68) = auVar391;
              *(undefined1 (*) [16])(*param_2 + lVar68) = auVar318;
              pfVar1 = (float *)(param_2[1] + lVar68);
              *pfVar1 = fVar165 + fVar110;
              pfVar1[1] = fVar388 + fVar363;
              pfVar1[2] = fVar165 - fVar110;
              pfVar1[3] = fVar388 - fVar363;
              lVar68 = lVar68 + 0x40;
            } while ((ulonglong)(uVar304 >> 2 & 0x1fffffff) << 6 != lVar68);
            if (uVar58 == uVar304) break;
          }
          iVar53 = uVar304 - uVar58;
          do {
            auVar83 = *pauVar59;
            fVar110 = auVar83._0_4_;
            fVar363 = auVar83._12_4_;
            fVar165 = auVar83._4_4_;
            auVar132._0_8_ = auVar83._8_8_;
            auVar132._8_4_ = fVar110;
            auVar132._12_4_ = fVar165;
            auVar134._4_4_ = fVar363 + fVar165;
            auVar134._0_4_ = auVar83._8_4_ + fVar110;
            auVar13._4_8_ = auVar132._8_8_;
            auVar13._0_4_ = fVar363 - fVar165;
            auVar133._0_8_ = auVar13._0_8_ << 0x20;
            auVar133._8_4_ = fVar110 - auVar83._8_4_;
            auVar133._12_4_ = fVar165 - fVar363;
            auVar134._8_8_ = auVar133._8_8_;
            *pauVar59 = auVar134;
            pauVar59 = pauVar59 + 1;
            iVar53 = iVar53 + -1;
          } while (iVar53 != 0);
        }
      }
      else {
        puVar61 = local_1b0;
        if (0 < (int)uVar304) {
          do {
            fVar110 = (float)puVar61[-2];
            fVar363 = (float)((ulonglong)puVar61[-2] >> 0x20);
            fVar165 = (fVar110 + fVar363) * _DAT_180010200;
            fVar388 = (fVar363 - fVar110) * _UNK_180010204;
            fVar110 = (float)puVar61[-3];
            fVar363 = (float)((ulonglong)puVar61[-3] >> 0x20);
            *(float *)(puVar61 + -3) = *(float *)(puVar61 + -7) - fVar110;
            *(float *)((longlong)puVar61 + -0x14) = *(float *)((longlong)puVar61 + -0x34) - fVar363;
            *(float *)(puVar61 + -2) = *(float *)(puVar61 + -6) - fVar165;
            *(float *)((longlong)puVar61 + -0xc) = *(float *)((longlong)puVar61 + -0x2c) - fVar388;
            *(float *)(puVar61 + -7) = fVar110 + *(float *)(puVar61 + -7);
            *(float *)((longlong)puVar61 + -0x34) = fVar363 + *(float *)((longlong)puVar61 + -0x34);
            *(float *)(puVar61 + -6) = fVar165 + *(float *)(puVar61 + -6);
            *(float *)((longlong)puVar61 + -0x2c) = fVar388 + *(float *)((longlong)puVar61 + -0x2c);
            fVar110 = (float)*puVar61;
            fVar363 = (float)((ulonglong)*puVar61 >> 0x20);
            fVar165 = (fVar363 - fVar110) * _DAT_180010210;
            fVar388 = (fVar110 + fVar363) * _UNK_180010214;
            fVar110 = (float)puVar61[-1];
            fVar363 = (float)((ulonglong)puVar61[-1] >> 0x20);
            auVar152._4_4_ = *(float *)((longlong)puVar61 + -0x24) - fVar110;
            auVar203._4_4_ = fVar110 + *(float *)((longlong)puVar61 + -0x24);
            auVar203._0_4_ = *(float *)(puVar61 + -5) - fVar363;
            auVar203._8_4_ = *(float *)(puVar61 + -4) - fVar165;
            auVar203._12_4_ = *(float *)((longlong)puVar61 + -0x1c) - fVar388;
            *(undefined1 (*) [16])(puVar61 + -1) = auVar203;
            auVar152._0_4_ = fVar363 + *(float *)(puVar61 + -5);
            auVar152._8_4_ = fVar165 + *(float *)(puVar61 + -4);
            auVar152._12_4_ = fVar388 + *(float *)((longlong)puVar61 + -0x1c);
            *(undefined1 (*) [16])(puVar61 + -5) = auVar152;
            uVar304 = uVar304 - 1;
            puVar61 = puVar61 + 8;
          } while (uVar304 != 0);
        }
      }
      break;
    case 3:
      uVar306 = local_118[local_5e8];
      if (0 < (longlong)(int)uVar306) {
        lVar68 = (longlong)(int)(uVar306 << (bVar55 & 0x1f));
        lVar54 = (longlong)(int)uVar58;
        lVar71 = (longlong)(int)(uVar58 * 2);
        fVar110 = *(float *)(*(longlong *)(param_1 + 0x38) + 4 + lVar68 * lVar54 * 8);
        lVar72 = 0;
        do {
          puVar61 = (undefined8 *)(*param_2 + lVar72 * (int)local_664 * 8);
          lVar73 = *(longlong *)(param_1 + 0x38);
          lVar67 = 0;
          lVar64 = lVar54;
          do {
            fVar363 = *(float *)((longlong)puVar61 + lVar54 * 8 + 4);
            fVar165 = *(float *)((longlong)puVar61 + lVar71 * 8 + 4);
            uVar9 = *(undefined8 *)(lVar73 + lVar67);
            fVar427 = (float)uVar9;
            fVar317 = (float)((ulonglong)uVar9 >> 0x20);
            fVar388 = *(float *)(puVar61 + lVar54) * fVar317 + fVar363 * fVar427;
            fVar427 = *(float *)(puVar61 + lVar54) * fVar427 +
                      (float)((uint)fVar363 ^ uVar304) * fVar317;
            uVar9 = *(undefined8 *)(lVar73 + lVar67 * 2);
            fVar317 = (float)uVar9;
            fVar481 = (float)((ulonglong)uVar9 >> 0x20);
            fVar363 = *(float *)(puVar61 + lVar71) * fVar481 + fVar165 * fVar317;
            fVar165 = *(float *)(puVar61 + lVar71) * fVar317 +
                      (float)((uint)fVar165 ^ uVar304) * fVar481;
            fVar317 = (fVar388 - fVar363) * fVar110;
            fVar481 = (fVar427 - fVar165) * fVar110;
            fVar363 = fVar363 + fVar388;
            fVar165 = fVar165 + fVar427;
            uVar9 = *puVar61;
            *(float *)(puVar61 + lVar54) = fVar165 * fVar554 + (float)uVar9;
            *(float *)((longlong)(puVar61 + lVar54) + 4) =
                 fVar363 * fVar564 + (float)((ulonglong)uVar9 >> 0x20);
            *puVar61 = CONCAT44((float)((ulonglong)*puVar61 >> 0x20) + fVar363,
                                (float)*puVar61 + fVar165);
            puVar61[lVar71] =
                 CONCAT44((float)((ulonglong)puVar61[lVar54] >> 0x20) - fVar481,
                          (float)puVar61[lVar54] + fVar317);
            puVar61[lVar54] =
                 CONCAT44(fVar481 + (float)((ulonglong)puVar61[lVar54] >> 0x20),
                          (float)puVar61[lVar54] - fVar317);
            puVar61 = puVar61 + 1;
            lVar67 = lVar67 + lVar68 * 8;
            lVar64 = lVar64 + -1;
          } while (lVar64 != 0);
          lVar72 = lVar72 + 1;
        } while (lVar72 != (int)uVar306);
      }
      break;
    case 4:
      uVar306 = local_118[local_5e8];
      if (uVar58 == 1) {
        if (0 < (int)uVar306) {
          uVar58 = 0;
          pauVar59 = param_2;
          if (3 < uVar306) {
            uVar58 = uVar306 & 0x7ffffffc;
            pauVar59 = param_2 + (ulonglong)uVar58 * 2;
            lVar68 = 0;
            do {
              pfVar1 = (float *)(param_2[5] + lVar68);
              pfVar2 = (float *)(param_2[7] + lVar68);
              pfVar8 = (float *)(*param_2 + lVar68);
              pfVar6 = (float *)(param_2[1] + lVar68);
              pfVar7 = (float *)(param_2[2] + lVar68);
              pfVar3 = (float *)(param_2[3] + lVar68);
              pfVar4 = (float *)(param_2[4] + lVar68);
              pfVar5 = (float *)(param_2[6] + lVar68);
              fVar481 = *pfVar8 - *pfVar6;
              fVar266 = *pfVar7 - *pfVar3;
              fVar280 = *pfVar4 - *pfVar1;
              fVar387 = *pfVar5 - *pfVar2;
              fVar473 = pfVar8[1] - pfVar6[1];
              fVar500 = pfVar7[1] - pfVar3[1];
              fVar510 = pfVar4[1] - pfVar1[1];
              fVar512 = pfVar5[1] - pfVar2[1];
              fVar544 = *pfVar6 + *pfVar8;
              fVar546 = *pfVar3 + *pfVar7;
              fVar550 = *pfVar1 + *pfVar4;
              fVar389 = *pfVar2 + *pfVar5;
              fVar428 = pfVar6[1] + pfVar8[1];
              fVar436 = pfVar3[1] + pfVar7[1];
              fVar448 = pfVar1[1] + pfVar4[1];
              fVar466 = pfVar2[1] + pfVar5[1];
              fVar472 = pfVar8[2] + pfVar6[2];
              fVar495 = pfVar7[2] + pfVar3[2];
              fVar380 = pfVar4[2] + pfVar1[2];
              fVar533 = pfVar5[2] + pfVar2[2];
              fVar426 = pfVar8[3] + pfVar6[3];
              fVar450 = pfVar7[3] + pfVar3[3];
              fVar457 = pfVar4[3] + pfVar1[3];
              fVar465 = pfVar5[3] + pfVar2[3];
              fVar554 = pfVar8[2] - pfVar6[2];
              fVar564 = pfVar7[2] - pfVar3[2];
              fVar110 = pfVar4[2] - pfVar1[2];
              fVar363 = pfVar5[2] - pfVar2[2];
              fVar165 = pfVar8[3] - pfVar6[3];
              fVar388 = pfVar7[3] - pfVar3[3];
              fVar427 = pfVar4[3] - pfVar1[3];
              fVar317 = pfVar5[3] - pfVar2[3];
              auVar135._4_4_ = fVar428 - fVar426;
              auVar135._0_4_ = fVar544 - fVar472;
              auVar135._8_4_ = fVar481 - fVar165;
              auVar135._12_4_ = fVar554 + fVar473;
              auVar482._4_4_ = fVar436 - fVar450;
              auVar482._0_4_ = fVar546 - fVar495;
              auVar482._8_4_ = fVar266 - fVar388;
              auVar482._12_4_ = fVar564 + fVar500;
              auVar319._4_4_ = fVar466 - fVar465;
              auVar319._0_4_ = fVar389 - fVar533;
              auVar319._8_4_ = fVar387 - fVar317;
              auVar319._12_4_ = fVar363 + fVar512;
              pfVar1 = (float *)(param_2[6] + lVar68);
              *pfVar1 = fVar533 + fVar389;
              pfVar1[1] = fVar465 + fVar466;
              pfVar1[2] = fVar387 + fVar317;
              pfVar1[3] = fVar512 - fVar363;
              *(undefined1 (*) [16])(param_2[7] + lVar68) = auVar319;
              pfVar1 = (float *)(param_2[4] + lVar68);
              *pfVar1 = fVar380 + fVar550;
              pfVar1[1] = fVar457 + fVar448;
              pfVar1[2] = fVar280 + fVar427;
              pfVar1[3] = fVar510 - fVar110;
              pfVar1 = (float *)(param_2[5] + lVar68);
              *pfVar1 = fVar550 - fVar380;
              pfVar1[1] = fVar448 - fVar457;
              pfVar1[2] = fVar280 - fVar427;
              pfVar1[3] = fVar110 + fVar510;
              pfVar1 = (float *)(param_2[2] + lVar68);
              *pfVar1 = fVar495 + fVar546;
              pfVar1[1] = fVar450 + fVar436;
              pfVar1[2] = fVar266 + fVar388;
              pfVar1[3] = fVar500 - fVar564;
              *(undefined1 (*) [16])(param_2[3] + lVar68) = auVar482;
              pfVar1 = (float *)(*param_2 + lVar68);
              *pfVar1 = fVar472 + fVar544;
              pfVar1[1] = fVar426 + fVar428;
              pfVar1[2] = fVar481 + fVar165;
              pfVar1[3] = fVar473 - fVar554;
              *(undefined1 (*) [16])(param_2[1] + lVar68) = auVar135;
              lVar68 = lVar68 + 0x80;
            } while ((ulonglong)(uVar306 >> 2 & 0x1fffffff) << 7 != lVar68);
            auVar395._4_4_ = _UNK_180010004;
            auVar395._0_4_ = _DAT_180010000;
            auVar395._8_4_ = _UNK_180010008;
            auVar395._12_4_ = _UNK_18001000c;
            fVar554 = _DAT_1800101f0;
            fVar564 = _UNK_1800101f4;
            if (uVar58 == uVar306) break;
          }
          iVar53 = uVar306 - uVar58;
          do {
            fVar110 = (float)*(undefined8 *)*pauVar59;
            fVar363 = (float)((ulonglong)*(undefined8 *)*pauVar59 >> 0x20);
            fVar165 = (float)*(undefined8 *)pauVar59[1];
            fVar280 = fVar110 - fVar165;
            fVar388 = (float)((ulonglong)*(undefined8 *)pauVar59[1] >> 0x20);
            fVar387 = fVar363 - fVar388;
            fVar110 = fVar110 + fVar165;
            fVar363 = fVar363 + fVar388;
            fVar165 = (float)*(undefined8 *)(*pauVar59 + 8);
            fVar481 = (float)*(undefined8 *)(pauVar59[1] + 8);
            fVar427 = fVar165 + fVar481;
            fVar388 = (float)((ulonglong)*(undefined8 *)(*pauVar59 + 8) >> 0x20);
            fVar266 = (float)((ulonglong)*(undefined8 *)(pauVar59[1] + 8) >> 0x20);
            fVar317 = fVar388 + fVar266;
            fVar165 = fVar165 - fVar481;
            fVar388 = fVar388 - fVar266;
            auVar79._0_4_ = fVar110 - fVar427;
            auVar79._4_4_ = fVar363 - fVar317;
            auVar79._8_4_ = fVar280 - fVar388;
            auVar230._4_4_ = fVar363 + fVar317;
            auVar230._0_4_ = fVar110 + fVar427;
            auVar230._8_4_ = fVar280 + fVar388;
            auVar230._12_4_ = fVar387 - fVar165;
            *pauVar59 = auVar230;
            auVar79._12_4_ = fVar387 + fVar165;
            pauVar59[1] = auVar79;
            pauVar59 = pauVar59 + 2;
            iVar53 = iVar53 + -1;
          } while (iVar53 != 0);
        }
      }
      else if ((0 < (int)uVar306) && (0 < (int)uVar58)) {
        lVar68 = (longlong)(int)(uVar306 << (bVar55 & 0x1f));
        lVar54 = (longlong)(int)local_664;
        puVar70 = *param_2 + (longlong)(int)(uVar58 * 3) * 8;
        puVar62 = *param_2 + (longlong)(int)(uVar58 * 2) * 8;
        puVar74 = *param_2 + (longlong)(int)uVar58 * 8;
        uVar65 = 0;
        pauVar59 = param_2;
        do {
          puVar12 = *(undefined8 **)(param_1 + 0x38);
          lVar72 = 0;
          lVar71 = 0;
          puVar61 = puVar12;
          do {
            uVar9 = *puVar61;
            puVar61 = puVar61 + lVar68 * 3;
            fVar110 = (float)*(undefined8 *)((longlong)puVar12 + lVar72);
            fVar363 = (float)((ulonglong)*(undefined8 *)((longlong)puVar12 + lVar72) >> 0x20);
            fVar165 = *(float *)(puVar74 + lVar71 * 8) * fVar363 +
                      *(float *)(puVar74 + lVar71 * 8 + 4) * fVar110;
            fVar388 = *(float *)(puVar74 + lVar71 * 8) * fVar110 +
                      (float)((uint)*(float *)(puVar74 + lVar71 * 8 + 4) ^ uVar304) * fVar363;
            uVar10 = *(undefined8 *)((longlong)puVar12 + lVar72 * 2);
            fVar363 = (float)uVar10;
            fVar427 = (float)((ulonglong)uVar10 >> 0x20);
            fVar110 = *(float *)(puVar62 + lVar71 * 8) * fVar363 +
                      fVar427 * (float)((uint)*(float *)(puVar62 + lVar71 * 8 + 4) ^ uVar304);
            fVar363 = *(float *)(puVar62 + lVar71 * 8) * fVar427 +
                      fVar363 * *(float *)(puVar62 + lVar71 * 8 + 4);
            fVar317 = (float)uVar9;
            fVar481 = (float)((ulonglong)uVar9 >> 0x20);
            fVar427 = *(float *)(puVar70 + lVar71 * 8) * fVar481 +
                      *(float *)(puVar70 + lVar71 * 8 + 4) * fVar317;
            fVar317 = *(float *)(puVar70 + lVar71 * 8) * fVar317 +
                      (float)((uint)*(float *)(puVar70 + lVar71 * 8 + 4) ^ uVar304) * fVar481;
            fVar481 = (float)*(undefined8 *)(*pauVar59 + lVar71 * 8);
            fVar266 = (float)((ulonglong)*(undefined8 *)(*pauVar59 + lVar71 * 8) >> 0x20);
            fVar280 = fVar481 - fVar110;
            fVar387 = fVar266 - fVar363;
            fVar426 = fVar165 - fVar427;
            fVar450 = fVar388 - fVar317;
            fVar110 = fVar110 + fVar481;
            fVar363 = fVar363 + fVar266;
            fVar427 = fVar427 + fVar165;
            fVar317 = fVar317 + fVar388;
            *(ulonglong *)(puVar62 + lVar71 * 8) = CONCAT44(fVar363 - fVar427,fVar110 - fVar317);
            *(ulonglong *)(*pauVar59 + lVar71 * 8) = CONCAT44(fVar427 + fVar363,fVar317 + fVar110);
            *(ulonglong *)(puVar74 + lVar71 * 8) = CONCAT44(fVar387 - fVar450,fVar280 + fVar426);
            *(ulonglong *)(puVar70 + lVar71 * 8) = CONCAT44(fVar387 + fVar450,fVar280 - fVar426);
            lVar71 = lVar71 + 1;
            lVar72 = lVar72 + lVar68 * 8;
          } while (uVar58 != (uint)lVar71);
          uVar65 = uVar65 + 1;
          pauVar59 = (undefined1 (*) [16])(*pauVar59 + lVar54 * 8);
          puVar70 = puVar70 + lVar54 * 8;
          puVar62 = puVar62 + lVar54 * 8;
          puVar74 = puVar74 + lVar54 * 8;
        } while (uVar65 != uVar306);
      }
      break;
    case 5:
      lVar68 = (longlong)(int)local_118[local_5e8];
      iVar53 = local_118[local_5e8] << (bVar55 & 0x1f);
      if ((0 < lVar68) && (0 < (int)uVar58)) {
        local_428 = (longlong)(int)uVar58;
        local_5a8 = *(undefined1 **)(param_1 + 0x38);
        local_430 = (longlong)(int)(uVar58 * 2);
        local_500 = (longlong)(int)(uVar58 * 3);
        local_508 = (longlong)(int)(uVar58 * 4);
        local_510 = (longlong)(int)local_664;
        local_4f0 = CONCAT44(local_4f0._4_4_,iVar53);
        lVar54 = (lVar68 * 8 + -8) * local_510 + uVar65 * 8;
        lVar71 = lVar54 + local_428 * 8;
        puVar62 = local_1b8 + lVar71;
        puVar69 = *param_2 + lVar71;
        lVar71 = lVar54 + local_508 * 8;
        local_1e8 = local_1b8 + lVar71;
        lVar72 = lVar54 + local_430 * 8;
        puVar70 = local_1b8 + lVar72;
        puStack_520 = *param_2 + lVar72;
        puStack_240 = local_5a8 + uVar65 * 0x10 + -0xc;
        lVar72 = lVar54 + local_500 * 8;
        puStack_300 = *param_2 + lVar71;
        puStack_310 = local_1b8 + lVar72;
        puStack_2f0 = *param_2 + lVar72;
        puStack_540 = local_5a8 + (uVar65 * 2 + -1) * 8;
        puStack_4d0 = local_5a8 + uVar65 * 0x18 + -0x14;
        puStack_440 = local_5a8 + (uVar65 * 3 + -2) * 8;
        puStack_230 = local_5a8 + uVar65 * 0x20 + -0x1c;
        puStack_210 = local_5a8 + (uVar65 * 4 + -3) * 8;
        fStack_630 = SUB84(local_1e8,0);
        fStack_62c = (float)((ulonglong)local_1e8 >> 0x20);
        local_468 = SUB84(puStack_310,0);
        fStack_464 = (float)((ulonglong)puStack_310 >> 0x20);
        uStack_460 = (uint)puStack_240;
        uStack_45c = (uint)((ulonglong)puStack_240 >> 0x20);
        local_5c8._0_4_ = (uint)puStack_230;
        local_5c8._4_4_ = (uint)((ulonglong)puStack_230 >> 0x20);
        local_348._8_8_ = puStack_540;
        local_348._0_8_ = puStack_240;
        local_438 = local_5a8 + uVar65 * 8 + -4;
        puStack_2d0 = local_5a8 + uVar65 * 8;
        uStack_360._0_4_ = (uint)puStack_2d0;
        uStack_360._4_4_ = (uint)((ulonglong)puStack_2d0 >> 0x20);
        stack0xfffffffffffff9c0 = puStack_210;
        local_648._0_8_ = puStack_230;
        local_4b8._0_4_ = (uint)puStack_520;
        local_4b8._4_4_ = (uint)((ulonglong)puStack_520 >> 0x20);
        uStack_470 = (uint)puStack_4d0;
        uStack_46c = (uint)((ulonglong)puStack_4d0 >> 0x20);
        uStack_450 = (uint)puStack_210;
        uStack_44c = (uint)((ulonglong)puStack_210 >> 0x20);
        uStack_2e7 = (undefined7)((ulonglong)puStack_210 >> 8);
        uVar461 = (uint)puStack_440;
        auVar467._8_4_ = (uint)uStack_360;
        auVar467._0_8_ = puStack_240;
        auVar467._12_4_ = uStack_360._4_4_;
        uStack_4dc = (uint)((ulonglong)puStack_440 >> 0x20);
        uStack_560 = (uint)puStack_300;
        uStack_55c = (uint)((ulonglong)puStack_300 >> 0x20);
        auVar231._8_8_ = puStack_300;
        auVar231._0_8_ = puStack_310;
        local_4a8._0_4_ = (uint)local_438;
        local_4a8._4_4_ = (uint)((ulonglong)local_438 >> 0x20);
        uStack_2f7 = (undefined7)((ulonglong)puStack_230 >> 8);
        puVar74 = local_1b8 + lVar54;
        auVar535._8_4_ = (int)puVar74;
        auVar535._0_8_ = puVar74;
        auVar535._12_4_ = (int)((ulonglong)puVar74 >> 0x20);
        uStack_480 = (uint)puStack_2f0;
        uStack_47c = (uint)((ulonglong)puStack_2f0 >> 0x20);
        auVar468._8_4_ = uVar461;
        auVar468._0_8_ = puStack_300;
        auVar468._12_4_ = uStack_4dc;
        auVar231 = auVar231 ^ auVar395;
        iVar305 = auVar231._0_4_;
        uVar204 = auVar231._4_4_;
        auVar188._4_4_ = -(uint)((int)local_178._4_4_ < (int)uVar204);
        iVar310 = auVar231._8_4_;
        uVar217 = auVar231._12_4_;
        auVar188._12_4_ = -(uint)((int)local_178._12_4_ < (int)uVar217);
        auVar232._4_4_ = -(uint)(uVar204 == local_178._4_4_);
        auVar232._12_4_ = -(uint)(uVar217 == local_178._12_4_);
        auVar232._0_4_ = auVar232._4_4_;
        auVar232._8_4_ = auVar232._12_4_;
        auVar320._4_4_ = -(uint)((int)local_178._0_4_ < iVar305);
        auVar320._0_4_ = -(uint)((int)local_178._0_4_ < iVar305);
        auVar320._8_4_ = -(uint)((int)local_178._8_4_ < iVar310);
        auVar320._12_4_ = -(uint)((int)local_178._8_4_ < iVar310);
        auVar188._0_4_ = auVar188._4_4_;
        auVar188._8_4_ = auVar188._12_4_;
        auVar80._8_8_ = puStack_520;
        auVar80._0_8_ = puVar70;
        auVar80 = auVar80 ^ auVar395;
        iVar227 = -(uint)((int)local_178._0_4_ < auVar80._0_4_);
        iVar245 = -(uint)((int)local_178._4_4_ < (int)auVar80._4_4_);
        iVar251 = -(uint)((int)local_178._8_4_ < auVar80._8_4_);
        iVar255 = -(uint)((int)local_178._12_4_ < (int)auVar80._12_4_);
        iVar98 = -(uint)(auVar80._4_4_ == local_178._4_4_);
        iVar117 = -(uint)(auVar80._12_4_ == local_178._12_4_);
        auVar321._4_4_ = iVar98;
        auVar321._0_4_ = iVar98;
        auVar321._8_4_ = iVar117;
        auVar321._12_4_ = iVar117;
        auVar81._4_4_ = iVar227;
        auVar81._0_4_ = iVar227;
        auVar81._8_4_ = iVar251;
        auVar81._12_4_ = iVar251;
        auVar82._4_4_ = iVar245;
        auVar82._0_4_ = iVar245;
        auVar82._8_4_ = iVar255;
        auVar82._12_4_ = iVar255;
        auVar83 = packssdw(auVar82 | auVar321 & auVar81,auVar188 | auVar232 & auVar320);
        auVar136._8_4_ = fStack_630;
        auVar136._0_8_ = puVar69;
        auVar136._12_4_ = fStack_62c;
        auVar136 = auVar136 ^ auVar395;
        iVar227 = -(uint)((int)local_178._0_4_ < auVar136._0_4_);
        iVar245 = -(uint)((int)local_178._4_4_ < (int)auVar136._4_4_);
        iVar251 = -(uint)((int)local_178._8_4_ < auVar136._8_4_);
        iVar255 = -(uint)((int)local_178._12_4_ < (int)auVar136._12_4_);
        iVar98 = -(uint)(auVar136._4_4_ == local_178._4_4_);
        iVar117 = -(uint)(auVar136._12_4_ == local_178._12_4_);
        auVar233._4_4_ = iVar98;
        auVar233._0_4_ = iVar98;
        auVar233._8_4_ = iVar117;
        auVar233._12_4_ = iVar117;
        auVar137._4_4_ = iVar227;
        auVar137._0_4_ = iVar227;
        auVar137._8_4_ = iVar251;
        auVar137._12_4_ = iVar251;
        auVar138._4_4_ = iVar245;
        auVar138._0_4_ = iVar245;
        auVar138._8_4_ = iVar255;
        auVar138._12_4_ = iVar255;
        puVar75 = *param_2 + lVar54;
        local_558 = SUB84(puVar69,0);
        fStack_554 = (float)((ulonglong)puVar69 >> 0x20);
        uStack_550 = (uint)puVar70;
        uStack_54c = (uint)((ulonglong)puVar70 >> 0x20);
        uStack_570 = (uint)puVar62;
        uStack_56c = (uint)((ulonglong)puVar62 >> 0x20);
        auVar234._8_8_ = puVar75;
        auVar234._0_8_ = puVar62;
        auVar234 = auVar234 ^ auVar395;
        iVar98 = -(uint)((int)local_178._0_4_ < auVar234._0_4_);
        auVar189._4_4_ = -(uint)((int)local_178._4_4_ < (int)auVar234._4_4_);
        iVar117 = -(uint)((int)local_178._8_4_ < auVar234._8_4_);
        auVar189._12_4_ = -(uint)((int)local_178._12_4_ < (int)auVar234._12_4_);
        auVar235._4_4_ = -(uint)(auVar234._4_4_ == local_178._4_4_);
        auVar235._12_4_ = -(uint)(auVar234._12_4_ == local_178._12_4_);
        auVar235._0_4_ = auVar235._4_4_;
        auVar235._8_4_ = auVar235._12_4_;
        auVar322._4_4_ = iVar98;
        auVar322._0_4_ = iVar98;
        auVar322._8_4_ = iVar117;
        auVar322._12_4_ = iVar117;
        auVar189._0_4_ = auVar189._4_4_;
        auVar189._8_4_ = auVar189._12_4_;
        auVar190 = packssdw(auVar189 | auVar235 & auVar322,auVar138 | auVar233 & auVar137);
        auVar190 = packssdw(auVar190,auVar83);
        puStack_140 = *param_2 + local_500 * 8;
        puVar62 = local_1c0 + local_508 * 8;
        local_138._8_8_ = puVar62;
        local_138._0_8_ = puStack_140;
        auVar535 = auVar535 ^ auVar395;
        local_138 = local_138 ^ auVar395;
        iVar98 = auVar535._0_4_;
        iVar228 = local_138._0_4_;
        uVar304 = auVar535._4_4_;
        uVar246 = local_138._4_4_;
        auVar84._4_4_ = -(uint)((int)uVar246 < (int)uVar304);
        iVar227 = auVar535._8_4_;
        iVar252 = local_138._8_4_;
        uVar306 = auVar535._12_4_;
        uVar256 = local_138._12_4_;
        auVar84._12_4_ = -(uint)((int)uVar256 < (int)uVar306);
        auVar139._4_4_ = -(uint)(uVar304 == uVar246);
        auVar139._12_4_ = -(uint)(uVar306 == uVar256);
        auVar139._0_4_ = auVar139._4_4_;
        auVar139._8_4_ = auVar139._12_4_;
        auVar236._4_4_ = -(uint)(iVar228 < iVar98);
        auVar236._0_4_ = -(uint)(iVar228 < iVar98);
        auVar236._8_4_ = -(uint)(iVar252 < iVar227);
        auVar236._12_4_ = -(uint)(iVar252 < iVar227);
        auVar84._0_4_ = auVar84._4_4_;
        auVar84._8_4_ = auVar84._12_4_;
        local_158 = *param_2 + local_430 * 8;
        puVar69 = local_1c0 + local_430 * 8;
        uStack_150 = 0;
        auVar140._8_8_ = puVar69;
        auVar140._0_8_ = local_158;
        auVar140 = auVar140 ^ auVar395;
        iVar117 = -(uint)(auVar140._0_4_ < iVar98);
        auVar237._4_4_ = -(uint)((int)auVar140._4_4_ < (int)uVar304);
        iVar245 = -(uint)(auVar140._8_4_ < iVar227);
        auVar237._12_4_ = -(uint)((int)auVar140._12_4_ < (int)uVar306);
        auVar141._4_4_ = -(uint)(auVar140._4_4_ == uVar304);
        auVar141._12_4_ = -(uint)(auVar140._12_4_ == uVar306);
        auVar141._0_4_ = auVar141._4_4_;
        auVar141._8_4_ = auVar141._12_4_;
        auVar323._4_4_ = iVar117;
        auVar323._0_4_ = iVar117;
        auVar323._8_4_ = iVar245;
        auVar323._12_4_ = iVar245;
        auVar237._0_4_ = auVar237._4_4_;
        auVar237._8_4_ = auVar237._12_4_;
        auVar238 = packssdw(auVar237 | auVar141 & auVar323,auVar84 | auVar139 & auVar236);
        puVar60 = local_1c0 + local_428 * 8;
        local_148 = *param_2 + local_508 * 8;
        auVar85._8_8_ = local_148;
        auVar85._0_8_ = puVar60;
        auVar85 = auVar85 ^ auVar395;
        iVar117 = -(uint)(auVar85._0_4_ < iVar98);
        iVar245 = -(uint)((int)auVar85._4_4_ < (int)uVar304);
        iVar251 = -(uint)(auVar85._8_4_ < iVar227);
        iVar255 = -(uint)((int)auVar85._12_4_ < (int)uVar306);
        auVar86._4_4_ = -(uint)(auVar85._4_4_ == uVar304);
        auVar86._12_4_ = -(uint)(auVar85._12_4_ == uVar306);
        auVar86._0_4_ = auVar86._4_4_;
        auVar86._8_4_ = auVar86._12_4_;
        auVar324._4_4_ = iVar117;
        auVar324._0_4_ = iVar117;
        auVar324._8_4_ = iVar251;
        auVar324._12_4_ = iVar251;
        auVar325._4_4_ = iVar245;
        auVar325._0_4_ = iVar245;
        auVar325._8_4_ = iVar255;
        auVar325._12_4_ = iVar255;
        puVar63 = *param_2 + local_428 * 8;
        auVar87._8_8_ = local_1c0;
        auVar87._0_8_ = puVar63;
        auVar87 = auVar87 ^ auVar395;
        iVar251 = -(uint)(auVar87._0_4_ < iVar98);
        iVar255 = -(uint)((int)auVar87._4_4_ < (int)uVar304);
        iVar429 = -(uint)(auVar87._8_4_ < iVar227);
        iVar438 = -(uint)((int)auVar87._12_4_ < (int)uVar306);
        iVar117 = -(uint)(auVar87._4_4_ == uVar304);
        iVar245 = -(uint)(auVar87._12_4_ == uVar306);
        auVar501._4_4_ = iVar117;
        auVar501._0_4_ = iVar117;
        auVar501._8_4_ = iVar245;
        auVar501._12_4_ = iVar245;
        auVar88._4_4_ = iVar251;
        auVar88._0_4_ = iVar251;
        auVar88._8_4_ = iVar429;
        auVar88._12_4_ = iVar429;
        auVar89._4_4_ = iVar255;
        auVar89._0_4_ = iVar255;
        auVar89._8_4_ = iVar438;
        auVar89._12_4_ = iVar438;
        auVar83 = packssdw(auVar89 | auVar501 & auVar88,auVar325 | auVar86 & auVar324);
        auVar83 = packssdw(auVar83,auVar238);
        puStack_120 = local_1c0 + local_500 * 8;
        auVar326._8_8_ = puStack_120;
        auVar326._0_8_ = param_2;
        auVar239._8_4_ = fStack_630;
        auVar239._0_8_ = puStack_2f0;
        auVar239._12_4_ = fStack_62c;
        auVar239 = auVar239 ^ auVar395;
        auVar326 = auVar326 ^ auVar395;
        uVar381 = -(uint)(auVar326._0_4_ < auVar239._0_4_);
        uVar416 = -(uint)(auVar326._4_4_ < auVar239._4_4_);
        uVar430 = -(uint)(auVar326._8_4_ < auVar239._8_4_);
        uVar439 = -(uint)(auVar326._12_4_ < auVar239._12_4_);
        uVar353 = -(uint)(auVar326._4_4_ == auVar239._4_4_);
        uVar370 = -(uint)(auVar326._12_4_ == auVar239._12_4_);
        auVar502._8_8_ = puStack_140;
        auVar502._0_8_ = puVar69;
        auVar565._8_8_ = puStack_120;
        auVar565._0_8_ = puVar62;
        local_248 = local_5a8 + 4;
        local_538 = (uint)local_248;
        uStack_534 = (uint)((ulonglong)local_248 >> 0x20);
        uStack_530 = (uint)puStack_140;
        uStack_52c = (uint)((ulonglong)puStack_140 >> 0x20);
        uStack_5b0 = (uint)puStack_120;
        uStack_5ac = (uint)((ulonglong)puStack_120 >> 0x20);
        auVar191._8_8_ = local_148;
        auVar191._0_8_ = puStack_120;
        auVar191 = auVar191 ^ auVar395;
        auVar240._8_4_ = uStack_480;
        auVar240._0_8_ = puVar74;
        auVar240._12_4_ = uStack_47c;
        auVar240 = auVar240 ^ auVar395;
        uVar382 = -(uint)(auVar191._0_4_ < auVar240._0_4_);
        uVar417 = -(uint)(auVar191._4_4_ < auVar240._4_4_);
        uVar431 = -(uint)(auVar191._8_4_ < auVar240._8_4_);
        uVar440 = -(uint)(auVar191._12_4_ < auVar240._12_4_);
        uVar247 = -(uint)(auVar240._4_4_ == auVar191._4_4_);
        uVar257 = -(uint)(auVar240._12_4_ == auVar191._12_4_);
        uVar281 = (uVar440 | uVar257 & uVar431) & (uVar439 | uVar370 & uVar430);
        uVar286 = (uVar440 | uVar257 & uVar431) & (uVar439 | uVar370 & uVar430);
        _local_648 = _local_648 ^ auVar395;
        iVar117 = -(uint)((int)local_178._0_4_ < local_648._0_4_);
        uVar257 = local_648._4_4_;
        auVar192._4_4_ = -(uint)((int)local_178._4_4_ < (int)uVar257);
        uVar431 = local_648._12_4_;
        iVar245 = -(uint)((int)local_178._8_4_ < local_648._8_4_);
        auVar192._12_4_ = -(uint)((int)local_178._12_4_ < (int)uVar431);
        auVar267._4_4_ = -(uint)(uVar257 == local_178._4_4_);
        auVar267._12_4_ = -(uint)(uVar431 == local_178._12_4_);
        auVar267._0_4_ = auVar267._4_4_;
        auVar267._8_4_ = auVar267._12_4_;
        auVar327._4_4_ = iVar117;
        auVar327._0_4_ = iVar117;
        auVar327._8_4_ = iVar245;
        auVar327._12_4_ = iVar245;
        auVar192._0_4_ = auVar192._4_4_;
        auVar192._8_4_ = auVar192._12_4_;
        auVar451._8_4_ = uVar461;
        auVar451._0_8_ = puStack_4d0;
        auVar451._12_4_ = uStack_4dc;
        auVar451 = auVar451 ^ auVar395;
        iVar117 = -(uint)((int)local_178._0_4_ < auVar451._0_4_);
        uVar115 = auVar451._4_4_;
        auVar268._4_4_ = -(uint)((int)local_178._4_4_ < (int)uVar115);
        uVar178 = auVar451._12_4_;
        iVar245 = -(uint)((int)local_178._8_4_ < auVar451._8_4_);
        auVar268._12_4_ = -(uint)((int)local_178._12_4_ < (int)uVar178);
        auVar328._4_4_ = -(uint)(uVar115 == local_178._4_4_);
        auVar328._12_4_ = -(uint)(uVar178 == local_178._12_4_);
        auVar328._0_4_ = auVar328._4_4_;
        auVar328._8_4_ = auVar328._12_4_;
        auVar392._4_4_ = iVar117;
        auVar392._0_4_ = iVar117;
        auVar392._8_4_ = iVar245;
        auVar392._12_4_ = iVar245;
        auVar268._0_4_ = auVar268._4_4_;
        auVar268._8_4_ = auVar268._12_4_;
        auVar238 = packssdw(auVar268 | auVar328 & auVar392,auVar192 | auVar267 & auVar327);
        auVar293._8_4_ = (uint)uStack_360;
        auVar293._0_8_ = local_438;
        auVar293._12_4_ = uStack_360._4_4_;
        auVar293 = auVar293 ^ auVar395;
        iVar117 = -(uint)((int)local_178._0_4_ < auVar293._0_4_);
        uVar370 = auVar293._4_4_;
        uVar439 = auVar293._12_4_;
        iVar245 = -(uint)((int)local_178._8_4_ < auVar293._8_4_);
        auVar329._4_4_ = -(uint)(uVar370 == local_178._4_4_);
        auVar329._12_4_ = -(uint)(uVar439 == local_178._12_4_);
        auVar329._0_4_ = auVar329._4_4_;
        auVar329._8_4_ = auVar329._12_4_;
        auVar393._4_4_ = iVar117;
        auVar393._0_4_ = iVar117;
        auVar393._8_4_ = iVar245;
        auVar393._12_4_ = iVar245;
        auVar394._4_4_ = -(uint)((int)local_178._4_4_ < (int)uVar370);
        auVar394._0_4_ = -(uint)((int)local_178._4_4_ < (int)uVar370);
        auVar394._8_4_ = -(uint)((int)local_178._12_4_ < (int)uVar439);
        auVar394._12_4_ = -(uint)((int)local_178._12_4_ < (int)uVar439);
        local_348 = local_348 ^ auVar395;
        iVar117 = -(uint)((int)local_178._0_4_ < local_348._0_4_);
        uVar430 = local_348._4_4_;
        auVar193._4_4_ = -(uint)((int)local_178._4_4_ < (int)uVar430);
        uVar440 = local_348._12_4_;
        iVar245 = -(uint)((int)local_178._8_4_ < local_348._8_4_);
        auVar193._12_4_ = -(uint)((int)local_178._12_4_ < (int)uVar440);
        auVar330._4_4_ = -(uint)(uVar430 == local_178._4_4_);
        auVar330._12_4_ = -(uint)(uVar440 == local_178._12_4_);
        auVar330._0_4_ = auVar330._4_4_;
        auVar330._8_4_ = auVar330._12_4_;
        auVar517._4_4_ = iVar117;
        auVar517._0_4_ = iVar117;
        auVar517._8_4_ = iVar245;
        auVar517._12_4_ = iVar245;
        auVar193._0_4_ = auVar193._4_4_;
        auVar193._8_4_ = auVar193._12_4_;
        auVar395 = packssdw(auVar394 | auVar329 & auVar393,auVar193 | auVar330 & auVar517);
        auVar396 = packssdw(auVar395,auVar238);
        local_4e8 = (uint)local_5a8;
        uStack_4e4 = (uint)((ulonglong)local_5a8 >> 0x20);
        auVar269._8_8_ = local_248;
        auVar269._0_8_ = local_5a8;
        auVar334._4_4_ = _UNK_180010004;
        auVar334._0_4_ = _DAT_180010000;
        auVar334._8_4_ = _UNK_180010008;
        auVar334._12_4_ = _UNK_18001000c;
        auVar269 = auVar269 ^ auVar334;
        iVar98 = -(uint)(auVar269._0_4_ < iVar98);
        uVar124 = auVar269._4_4_;
        iVar117 = -(uint)(auVar269._8_4_ < iVar227);
        uVar215 = auVar269._12_4_;
        auVar518._4_4_ = -(uint)(uVar304 == uVar124);
        auVar518._0_4_ = -(uint)(uVar304 == uVar124);
        auVar518._8_4_ = -(uint)(uVar306 == uVar215);
        auVar518._12_4_ = -(uint)(uVar306 == uVar215);
        auVar536._4_4_ = iVar98;
        auVar536._0_4_ = iVar98;
        auVar536._8_4_ = iVar117;
        auVar536._12_4_ = iVar117;
        auVar537._4_4_ = -(uint)((int)uVar124 < (int)uVar304);
        auVar537._0_4_ = -(uint)((int)uVar124 < (int)uVar304);
        auVar537._8_4_ = -(uint)((int)uVar215 < (int)uVar306);
        auVar537._12_4_ = -(uint)((int)uVar215 < (int)uVar306);
        auVar537 = auVar537 | auVar518 & auVar536;
        auVar395 = packssdw(auVar537,auVar537);
        auVar538 = packssdw(auVar395,auVar395);
        iVar98 = -(uint)((int)local_198._0_4_ < (int)(uStack_560 ^ _DAT_180010000));
        auVar90._4_4_ = -(uint)((int)local_198._4_4_ < (int)(uStack_55c ^ _UNK_180010004));
        iVar117 = -(uint)((int)local_198._8_4_ < (int)(uStack_480 ^ _UNK_180010008));
        auVar90._12_4_ = -(uint)((int)local_198._12_4_ < (int)(uStack_47c ^ _UNK_18001000c));
        auVar331._4_4_ = -(uint)((uStack_55c ^ _UNK_180010004) == local_198._4_4_);
        auVar331._12_4_ = -(uint)((uStack_47c ^ _UNK_18001000c) == local_198._12_4_);
        auVar331._0_4_ = auVar331._4_4_;
        auVar331._8_4_ = auVar331._12_4_;
        auVar397._4_4_ = iVar98;
        auVar397._0_4_ = iVar98;
        auVar397._8_4_ = iVar117;
        auVar397._12_4_ = iVar117;
        auVar90._0_4_ = auVar90._4_4_;
        auVar90._8_4_ = auVar90._12_4_;
        iVar98 = -(uint)((int)local_198._0_4_ < (int)((uint)local_4b8 ^ _DAT_180010000));
        iVar117 = -(uint)((int)local_198._4_4_ < (int)(local_4b8._4_4_ ^ _UNK_180010004));
        iVar227 = -(uint)((int)local_198._8_4_ < (int)((uint)local_468 ^ _UNK_180010008));
        iVar245 = -(uint)((int)local_198._12_4_ < (int)((uint)fStack_464 ^ _UNK_18001000c));
        auVar398._4_4_ = -(uint)((local_4b8._4_4_ ^ _UNK_180010004) == local_198._4_4_);
        auVar398._12_4_ = -(uint)(((uint)fStack_464 ^ _UNK_18001000c) == local_198._12_4_);
        auVar398._0_4_ = auVar398._4_4_;
        auVar398._8_4_ = auVar398._12_4_;
        auVar519._4_4_ = iVar98;
        auVar519._0_4_ = iVar98;
        auVar519._8_4_ = iVar227;
        auVar519._12_4_ = iVar227;
        auVar520._4_4_ = iVar117;
        auVar520._0_4_ = iVar117;
        auVar520._8_4_ = iVar245;
        auVar520._12_4_ = iVar245;
        auVar238 = packssdw(auVar520 | auVar398 & auVar519,auVar90 | auVar331 & auVar397);
        iVar98 = -(uint)((int)local_198._0_4_ < (int)((uint)fStack_630 ^ _DAT_180010000));
        auVar91._4_4_ = -(uint)((int)local_198._4_4_ < (int)((uint)fStack_62c ^ _UNK_180010004));
        iVar117 = -(uint)((int)local_198._8_4_ < (int)(uStack_550 ^ _UNK_180010008));
        auVar91._12_4_ = -(uint)((int)local_198._12_4_ < (int)(uStack_54c ^ _UNK_18001000c));
        auVar332._4_4_ = -(uint)(((uint)fStack_62c ^ _UNK_180010004) == local_198._4_4_);
        auVar332._12_4_ = -(uint)((uStack_54c ^ _UNK_18001000c) == local_198._12_4_);
        auVar332._0_4_ = auVar332._4_4_;
        auVar332._8_4_ = auVar332._12_4_;
        auVar399._4_4_ = iVar98;
        auVar399._0_4_ = iVar98;
        auVar399._8_4_ = iVar117;
        auVar399._12_4_ = iVar117;
        auVar91._0_4_ = auVar91._4_4_;
        auVar91._8_4_ = auVar91._12_4_;
        iVar98 = -(uint)((int)local_198._0_4_ < (int)(uStack_570 ^ _DAT_180010000));
        auVar333._4_4_ = -(uint)((int)local_198._4_4_ < (int)(uStack_56c ^ _UNK_180010004));
        iVar117 = -(uint)((int)local_198._8_4_ < (int)((uint)local_558 ^ _UNK_180010008));
        auVar333._12_4_ = -(uint)((int)local_198._12_4_ < (int)((uint)fStack_554 ^ _UNK_18001000c));
        auVar400._4_4_ = -(uint)((uStack_56c ^ _UNK_180010004) == local_198._4_4_);
        auVar400._12_4_ = -(uint)(((uint)fStack_554 ^ _UNK_18001000c) == local_198._12_4_);
        auVar400._0_4_ = auVar400._4_4_;
        auVar400._8_4_ = auVar400._12_4_;
        auVar483._4_4_ = iVar98;
        auVar483._0_4_ = iVar98;
        auVar483._8_4_ = iVar117;
        auVar483._12_4_ = iVar117;
        auVar333._0_4_ = auVar333._4_4_;
        auVar333._8_4_ = auVar333._12_4_;
        auVar395 = packssdw(auVar333 | auVar400 & auVar483,auVar91 | auVar332 & auVar399);
        auVar334 = packssdw(auVar395,auVar238);
        local_3c8 = (uint)puVar75;
        uStack_3c4 = (uint)((ulonglong)puVar75 >> 0x20);
        auVar406._4_4_ = _UNK_180010004;
        auVar406._0_4_ = _DAT_180010000;
        auVar406._8_4_ = _UNK_180010008;
        auVar406._12_4_ = _UNK_18001000c;
        auVar565 = auVar565 ^ auVar406;
        uVar304 = local_3c8 ^ _DAT_180010000;
        uVar306 = uStack_3c4 ^ _UNK_180010004;
        local_3c8 = local_3c8 ^ _UNK_180010008;
        uStack_3c4 = uStack_3c4 ^ _UNK_18001000c;
        iVar98 = -(uint)(auVar565._0_4_ < (int)uVar304);
        auVar401._4_4_ = -(uint)((int)auVar565._4_4_ < (int)uVar306);
        iVar117 = -(uint)(auVar565._8_4_ < (int)local_3c8);
        auVar401._12_4_ = -(uint)((int)auVar565._12_4_ < (int)uStack_3c4);
        iVar227 = -(uint)(auVar565._4_4_ == uVar306);
        iVar245 = -(uint)(auVar565._12_4_ == uStack_3c4);
        auVar484._4_4_ = iVar227;
        auVar484._0_4_ = iVar227;
        auVar484._8_4_ = iVar245;
        auVar484._12_4_ = iVar245;
        auVar521._4_4_ = iVar98;
        auVar521._0_4_ = iVar98;
        auVar521._8_4_ = iVar117;
        auVar521._12_4_ = iVar117;
        auVar401._0_4_ = auVar401._4_4_;
        auVar401._8_4_ = auVar401._12_4_;
        auVar14._4_4_ = _UNK_180010004;
        auVar14._0_4_ = _DAT_180010000;
        auVar14._8_4_ = _UNK_180010008;
        auVar14._12_4_ = _UNK_18001000c;
        auVar502 = auVar502 ^ auVar14;
        iVar98 = -(uint)(auVar502._0_4_ < (int)uVar304);
        auVar485._4_4_ = -(uint)((int)auVar502._4_4_ < (int)uVar306);
        iVar117 = -(uint)(auVar502._8_4_ < (int)local_3c8);
        auVar485._12_4_ = -(uint)((int)auVar502._12_4_ < (int)uStack_3c4);
        auVar503._4_4_ = -(uint)(auVar502._4_4_ == uVar306);
        auVar503._12_4_ = -(uint)(auVar502._12_4_ == uStack_3c4);
        auVar503._0_4_ = auVar503._4_4_;
        auVar503._8_4_ = auVar503._12_4_;
        auVar522._4_4_ = iVar98;
        auVar522._0_4_ = iVar98;
        auVar522._8_4_ = iVar117;
        auVar522._12_4_ = iVar117;
        auVar485._0_4_ = auVar485._4_4_;
        auVar485._8_4_ = auVar485._12_4_;
        auVar238 = packssdw(auVar485 | auVar503 & auVar522,auVar401 | auVar484 & auVar521);
        auVar402._8_8_ = local_158;
        auVar402._0_8_ = local_148;
        auVar15._4_4_ = _UNK_180010004;
        auVar15._0_4_ = _DAT_180010000;
        auVar15._8_4_ = _UNK_180010008;
        auVar15._12_4_ = _UNK_18001000c;
        auVar402 = auVar402 ^ auVar15;
        iVar98 = -(uint)(auVar402._0_4_ < (int)uVar304);
        auVar504._4_4_ = -(uint)((int)auVar402._4_4_ < (int)uVar306);
        iVar117 = -(uint)(auVar402._8_4_ < (int)local_3c8);
        auVar504._12_4_ = -(uint)((int)auVar402._12_4_ < (int)uStack_3c4);
        auVar403._4_4_ = -(uint)(auVar402._4_4_ == uVar306);
        auVar403._12_4_ = -(uint)(auVar402._12_4_ == uStack_3c4);
        auVar403._0_4_ = auVar403._4_4_;
        auVar403._8_4_ = auVar403._12_4_;
        auVar523._4_4_ = iVar98;
        auVar523._0_4_ = iVar98;
        auVar523._8_4_ = iVar117;
        auVar523._12_4_ = iVar117;
        auVar504._0_4_ = auVar504._4_4_;
        auVar504._8_4_ = auVar504._12_4_;
        auVar142._8_8_ = puVar60;
        auVar142._0_8_ = puVar63;
        auVar16._4_4_ = _UNK_180010004;
        auVar16._0_4_ = _DAT_180010000;
        auVar16._8_4_ = _UNK_180010008;
        auVar16._12_4_ = _UNK_18001000c;
        auVar142 = auVar142 ^ auVar16;
        iVar227 = -(uint)(auVar142._0_4_ < (int)uVar304);
        iVar245 = -(uint)((int)auVar142._4_4_ < (int)uVar306);
        iVar251 = -(uint)(auVar142._8_4_ < (int)local_3c8);
        iVar255 = -(uint)((int)auVar142._12_4_ < (int)uStack_3c4);
        iVar98 = -(uint)(auVar142._4_4_ == uVar306);
        iVar117 = -(uint)(auVar142._12_4_ == uStack_3c4);
        auVar524._4_4_ = iVar98;
        auVar524._0_4_ = iVar98;
        auVar524._8_4_ = iVar117;
        auVar524._12_4_ = iVar117;
        auVar143._4_4_ = iVar227;
        auVar143._0_4_ = iVar227;
        auVar143._8_4_ = iVar251;
        auVar143._12_4_ = iVar251;
        auVar144._4_4_ = iVar245;
        auVar144._0_4_ = iVar245;
        auVar144._8_4_ = iVar255;
        auVar144._12_4_ = iVar255;
        auVar395 = packssdw(auVar144 | auVar524 & auVar143,auVar504 | auVar403 & auVar523);
        auVar395 = packssdw(auVar395,auVar238);
        iVar98 = -(uint)((int)local_198._0_4_ < local_648._0_4_);
        auVar335._4_4_ = -(uint)((int)local_198._4_4_ < (int)uVar257);
        iVar117 = -(uint)((int)local_198._8_4_ < local_648._8_4_);
        auVar335._12_4_ = -(uint)((int)local_198._12_4_ < (int)uVar431);
        auVar404._4_4_ = -(uint)(uVar257 == local_198._4_4_);
        auVar404._12_4_ = -(uint)(uVar431 == local_198._12_4_);
        auVar404._0_4_ = auVar404._4_4_;
        auVar404._8_4_ = auVar404._12_4_;
        auVar486._4_4_ = iVar98;
        auVar486._0_4_ = iVar98;
        auVar486._8_4_ = iVar117;
        auVar486._12_4_ = iVar117;
        auVar335._0_4_ = auVar335._4_4_;
        auVar335._8_4_ = auVar335._12_4_;
        iVar98 = -(uint)((int)local_198._0_4_ < auVar451._0_4_);
        auVar405._4_4_ = -(uint)((int)local_198._4_4_ < (int)uVar115);
        iVar117 = -(uint)((int)local_198._8_4_ < auVar451._8_4_);
        auVar405._12_4_ = -(uint)((int)local_198._12_4_ < (int)uVar178);
        auVar487._4_4_ = -(uint)(uVar115 == local_198._4_4_);
        auVar487._0_4_ = -(uint)(uVar115 == local_198._4_4_);
        auVar487._8_4_ = -(uint)(uVar178 == local_198._12_4_);
        auVar487._12_4_ = -(uint)(uVar178 == local_198._12_4_);
        auVar505._4_4_ = iVar98;
        auVar505._0_4_ = iVar98;
        auVar505._8_4_ = iVar117;
        auVar505._12_4_ = iVar117;
        auVar405._0_4_ = auVar405._4_4_;
        auVar405._8_4_ = auVar405._12_4_;
        auVar406 = packssdw(auVar405 | auVar487 & auVar505,auVar335 | auVar404 & auVar486);
        iVar98 = -(uint)((int)local_198._0_4_ < auVar293._0_4_);
        auVar336._4_4_ = -(uint)((int)local_198._4_4_ < (int)uVar370);
        iVar117 = -(uint)((int)local_198._8_4_ < auVar293._8_4_);
        auVar336._12_4_ = -(uint)((int)local_198._12_4_ < (int)uVar439);
        auVar488._4_4_ = -(uint)(uVar370 == local_198._4_4_);
        auVar488._0_4_ = -(uint)(uVar370 == local_198._4_4_);
        auVar488._8_4_ = -(uint)(uVar439 == local_198._12_4_);
        auVar488._12_4_ = -(uint)(uVar439 == local_198._12_4_);
        auVar506._4_4_ = iVar98;
        auVar506._0_4_ = iVar98;
        auVar506._8_4_ = iVar117;
        auVar506._12_4_ = iVar117;
        auVar336._0_4_ = auVar336._4_4_;
        auVar336._8_4_ = auVar336._12_4_;
        iVar98 = -(uint)((int)local_198._0_4_ < local_348._0_4_);
        auVar489._4_4_ = -(uint)((int)local_198._4_4_ < (int)uVar430);
        iVar117 = -(uint)((int)local_198._8_4_ < local_348._8_4_);
        auVar489._12_4_ = -(uint)((int)local_198._12_4_ < (int)uVar440);
        auVar507._4_4_ = -(uint)(uVar430 == local_198._4_4_);
        auVar507._0_4_ = -(uint)(uVar430 == local_198._4_4_);
        auVar507._8_4_ = -(uint)(uVar440 == local_198._12_4_);
        auVar507._12_4_ = -(uint)(uVar440 == local_198._12_4_);
        auVar525._4_4_ = iVar98;
        auVar525._0_4_ = iVar98;
        auVar525._8_4_ = iVar117;
        auVar525._12_4_ = iVar117;
        auVar489._0_4_ = auVar489._4_4_;
        auVar489._8_4_ = auVar489._12_4_;
        auVar238 = packssdw(auVar336 | auVar488 & auVar506,auVar489 | auVar507 & auVar525);
        auVar406 = packssdw(auVar238,auVar406);
        iVar98 = -(uint)(auVar269._0_4_ < (int)uVar304);
        iVar117 = -(uint)(auVar269._8_4_ < (int)local_3c8);
        auVar92._4_4_ = -(uint)(uVar306 == uVar124);
        auVar92._12_4_ = -(uint)(uStack_3c4 == uVar215);
        auVar92._0_4_ = auVar92._4_4_;
        auVar92._8_4_ = auVar92._12_4_;
        auVar270._4_4_ = iVar98;
        auVar270._0_4_ = iVar98;
        auVar270._8_4_ = iVar117;
        auVar270._12_4_ = iVar117;
        auVar271._4_4_ = -(uint)((int)uVar124 < (int)uVar306);
        auVar271._0_4_ = -(uint)((int)uVar124 < (int)uVar306);
        auVar271._8_4_ = -(uint)((int)uVar215 < (int)uStack_3c4);
        auVar271._12_4_ = -(uint)((int)uVar215 < (int)uStack_3c4);
        auVar271 = auVar271 | auVar92 & auVar270;
        auVar238 = packssdw(auVar271,auVar271);
        auVar238 = packssdw(auVar238,auVar238);
        local_348 = auVar238 & auVar406 | auVar395 & auVar334 |
                    auVar538 & auVar396 | auVar83 & auVar190;
        local_588._0_4_ = (uint)puVar60;
        local_588._4_4_ = (uint)((ulonglong)puVar60 >> 0x20);
        uStack_580 = (uint)local_158;
        uStack_57c = (uint)((ulonglong)local_158 >> 0x20);
        auVar242._8_8_ = local_158;
        auVar242._0_8_ = puVar63;
        auVar241._8_8_ = local_5a8;
        auVar241._0_8_ = local_158;
        uVar492 = (uint)((ulonglong)puVar62 >> 0x20);
        local_3a8._8_8_ = puVar69;
        local_3a8._0_8_ = puVar60;
        local_608 = (uint)local_148;
        local_368 = local_608;
        uStack_604 = (uint)((ulonglong)local_148 >> 0x20);
        uStack_364 = uStack_604;
        uStack_600 = (uint)puVar69;
        uStack_5fc = (uint)((ulonglong)puVar69 >> 0x20);
        auVar452._8_8_ = local_248;
        auVar452._0_8_ = puVar69;
        local_338._8_8_ = local_248;
        local_338._0_8_ = puVar62;
        auVar194._8_4_ = local_4e8;
        auVar194._0_8_ = local_5a8;
        auVar194._12_4_ = uStack_4e4;
        uVar127 = (uint)local_558 ^ _DAT_180010000;
        uVar153 = (uint)fStack_554 ^ _UNK_180010004;
        uVar166 = (uint)local_4b8 ^ _UNK_180010008;
        uVar172 = local_4b8._4_4_ ^ _UNK_18001000c;
        uVar390 = (ulonglong)puVar63 ^ CONCAT44(_UNK_180010004,_DAT_180010000);
        uVar437 = (ulonglong)local_148 ^ CONCAT44(_UNK_18001000c,_UNK_180010008);
        uVar383 = -(uint)((int)(local_538 ^ _DAT_180010000) < (int)uVar127);
        uVar418 = -(uint)((int)(uStack_534 ^ _UNK_180010004) < (int)uVar153);
        uVar432 = -(uint)((int)(uStack_530 ^ _UNK_180010008) < (int)uVar166);
        uVar441 = -(uint)((int)(uStack_52c ^ _UNK_18001000c) < (int)uVar172);
        _local_358 = CONCAT44(uVar418,uVar383);
        _uStack_350 = CONCAT44(uVar441,uVar432);
        uVar99 = -(uint)((uStack_534 ^ _UNK_180010004) == uVar153);
        uVar118 = -(uint)((uStack_52c ^ _UNK_18001000c) == uVar172);
        uVar304 = (uint)puVar62 ^ _UNK_180010008;
        uVar306 = uVar492 ^ _UNK_18001000c;
        uVar257 = -(uint)((int)(local_4e8 ^ _DAT_180010000) < (int)uVar127);
        uVar100 = -(uint)((int)(uStack_4e4 ^ _UNK_180010004) < (int)uVar153);
        uVar111 = -(uint)((int)uVar304 < (int)uVar166);
        uVar119 = -(uint)((int)uVar306 < (int)uVar172);
        _local_378 = CONCAT44(uVar100,uVar257);
        _uStack_370 = CONCAT44(uVar119,uVar111);
        uVar301 = -(uint)((uStack_4e4 ^ _UNK_180010004) == uVar153);
        uVar307 = -(uint)(uVar306 == uVar172);
        _local_388 = CONCAT44(uVar301,-(uint)((local_4e8 ^ _DAT_180010000) == uVar127));
        _iStack_380 = CONCAT44(uVar307,-(uint)(uVar304 == uVar166));
        uVar292 = -(uint)((int)(local_538 ^ _DAT_180010000) < (int)uVar127);
        uVar302 = -(uint)((int)(uStack_534 ^ _UNK_180010004) < (int)uVar153);
        uStack_4e0 = -(uint)((int)(uStack_5b0 ^ _UNK_180010008) < (int)uVar166);
        uVar308 = -(uint)((int)(uStack_5ac ^ _UNK_18001000c) < (int)uVar172);
        uVar101 = -(uint)((uStack_534 ^ _UNK_180010004) == uVar153);
        uVar120 = -(uint)((uStack_5ac ^ _UNK_18001000c) == uVar172);
        auVar396._4_4_ = _UNK_180010004;
        auVar396._0_4_ = _DAT_180010000;
        auVar396._8_4_ = _UNK_180010008;
        auVar396._12_4_ = _UNK_18001000c;
        auVar194 = auVar194 ^ auVar396;
        uVar370 = -(uint)((int)auVar194._0_4_ < (int)uVar127);
        uVar102 = -(uint)((int)auVar194._4_4_ < (int)uVar153);
        uVar112 = -(uint)((int)auVar194._8_4_ < (int)uVar166);
        uVar121 = -(uint)((int)auVar194._12_4_ < (int)uVar172);
        _local_398 = CONCAT44(uVar102,uVar370);
        _uStack_390 = CONCAT44(uVar121,uVar112);
        local_3b8 = -(uint)(auVar194._0_4_ == uVar127);
        uStack_3b4 = -(uint)(auVar194._4_4_ == uVar153);
        iStack_3b0 = -(uint)(auVar194._8_4_ == uVar166);
        uStack_3ac = -(uint)(auVar194._12_4_ == uVar172);
        auVar93._8_4_ = local_538;
        auVar93._0_8_ = local_248;
        auVar93._12_4_ = uStack_534;
        auVar538._4_4_ = _UNK_180010004;
        auVar538._0_4_ = _DAT_180010000;
        auVar538._8_4_ = _UNK_180010008;
        auVar538._12_4_ = _UNK_18001000c;
        auVar93 = auVar93 ^ auVar538;
        uVar183 = -(uint)((int)auVar93._0_4_ < (int)uVar127);
        uStack_5b4 = -(uint)((int)auVar93._4_4_ < (int)uVar153);
        uVar212 = -(uint)((int)auVar93._8_4_ < (int)uVar166);
        uVar218 = -(uint)((int)auVar93._12_4_ < (int)uVar172);
        _local_3c8 = CONCAT44(uStack_5b4,uVar183);
        _uStack_3c0 = CONCAT44(uVar218,uVar212);
        uStack_3d4 = -(uint)(auVar93._4_4_ == uVar153);
        uStack_3cc = -(uint)(auVar93._12_4_ == uVar172);
        local_3d8 = -(uint)(auVar93._0_4_ == uVar127);
        iStack_3d0 = -(uint)(auVar93._8_4_ == uVar166);
        iVar311 = (int)uVar390;
        uVar354 = (uint)(uVar390 >> 0x20);
        iVar364 = (int)uVar437;
        uVar371 = (uint)(uVar437 >> 0x20);
        uVar462 = uStack_570 ^ _DAT_180010000;
        uVar469 = uStack_56c ^ _UNK_180010004;
        uVar474 = (uint)fStack_630 ^ _UNK_180010008;
        uVar477 = (uint)fStack_62c ^ _UNK_18001000c;
        auVar17._4_4_ = _UNK_180010004;
        auVar17._0_4_ = _DAT_180010000;
        auVar17._8_4_ = _UNK_180010008;
        auVar17._12_4_ = _UNK_18001000c;
        local_3a8 = local_3a8 ^ auVar17;
        iVar262 = local_3a8._0_4_;
        uVar275 = local_3a8._4_4_;
        iVar282 = local_3a8._8_4_;
        uVar287 = local_3a8._12_4_;
        uVar430 = -(uint)(iVar311 < (int)((uint)fStack_630 ^ _DAT_180010000));
        uVar103 = -(uint)((int)uVar354 < (int)((uint)fStack_62c ^ _UNK_180010004));
        uVar113 = -(uint)(iVar364 < (int)((uint)local_468 ^ _UNK_180010008));
        uVar122 = -(uint)((int)uVar371 < (int)((uint)fStack_464 ^ _UNK_18001000c));
        uVar154 = -(uint)(((uint)fStack_62c ^ _UNK_180010004) == uVar354);
        uVar173 = -(uint)(((uint)fStack_464 ^ _UNK_18001000c) == uVar371);
        uVar431 = -(uint)((int)(local_608 ^ _DAT_180010000) < (int)uVar462);
        uVar104 = -(uint)((int)(uStack_604 ^ _UNK_180010004) < (int)uVar469);
        uVar114 = -(uint)((int)(uStack_530 ^ _UNK_180010008) < (int)uVar474);
        uVar123 = -(uint)((int)(uStack_52c ^ _UNK_18001000c) < (int)uVar477);
        uVar155 = -(uint)((uStack_604 ^ _UNK_180010004) == uVar469);
        uVar174 = -(uint)((uStack_52c ^ _UNK_18001000c) == uVar477);
        uVar128 = -(uint)(iVar311 < (int)(uStack_550 ^ _DAT_180010000));
        uVar156 = -(uint)((int)uVar354 < (int)(uStack_54c ^ _UNK_180010004));
        uVar167 = -(uint)(iVar364 < (int)((uint)local_4a8 ^ _UNK_180010008));
        uVar175 = -(uint)((int)uVar371 < (int)(local_4a8._4_4_ ^ _UNK_18001000c));
        uVar205 = -(uint)((uStack_54c ^ _UNK_180010004) == uVar354);
        uVar219 = -(uint)((local_4a8._4_4_ ^ _UNK_18001000c) == uVar371);
        auVar18._4_4_ = _UNK_180010004;
        auVar18._0_4_ = _DAT_180010000;
        auVar18._8_4_ = _UNK_180010008;
        auVar18._12_4_ = _UNK_18001000c;
        auVar241 = auVar241 ^ auVar18;
        iVar229 = auVar241._0_4_;
        uVar248 = auVar241._4_4_;
        iVar253 = auVar241._8_4_;
        uVar258 = auVar241._12_4_;
        uVar129 = -(uint)(iVar311 < (int)((uint)local_4b8 ^ _DAT_180010000));
        uVar157 = -(uint)((int)uVar354 < (int)(local_4b8._4_4_ ^ _UNK_180010004));
        uVar168 = -(uint)(iVar364 < (int)((uint)uStack_360 ^ _UNK_180010008));
        uVar176 = -(uint)((int)uVar371 < (int)(uStack_360._4_4_ ^ _UNK_18001000c));
        uVar355 = -(uint)((local_4b8._4_4_ ^ _UNK_180010004) == uVar354);
        uVar372 = -(uint)((uStack_360._4_4_ ^ _UNK_18001000c) == uVar371);
        auVar555._8_4_ = local_538;
        auVar555._0_8_ = local_5a8;
        auVar555._12_4_ = uStack_534;
        auVar19._4_4_ = _UNK_180010004;
        auVar19._0_4_ = _DAT_180010000;
        auVar19._8_4_ = _UNK_180010008;
        auVar19._12_4_ = _UNK_18001000c;
        auVar452 = auVar452 ^ auVar19;
        iVar449 = auVar452._0_4_;
        uVar455 = auVar452._4_4_;
        iVar458 = auVar452._8_4_;
        uVar459 = auVar452._12_4_;
        uVar184 = -(uint)(iVar311 < (int)((uint)local_468 ^ _DAT_180010000));
        uVar206 = -(uint)((int)uVar354 < (int)((uint)fStack_464 ^ _UNK_180010004));
        uVar213 = -(uint)(iVar364 < (int)(uStack_460 ^ _UNK_180010008));
        uVar220 = -(uint)((int)uVar371 < (int)(uStack_45c ^ _UNK_18001000c));
        uVar356 = -(uint)(((uint)fStack_464 ^ _UNK_180010004) == uVar354);
        uVar373 = -(uint)((uStack_45c ^ _UNK_18001000c) == uVar371);
        uVar515 = uStack_530 ^ _DAT_180010000;
        uVar527 = uStack_52c ^ _UNK_180010004;
        uVar529 = local_4e8 ^ _UNK_180010008;
        uVar531 = uStack_4e4 ^ _UNK_18001000c;
        uStack_4c0._0_4_ = (uint)puStack_540;
        uVar304 = (uint)uStack_4c0;
        uStack_4c0._4_4_ = (uint)((ulonglong)puStack_540 >> 0x20);
        uVar306 = uStack_4c0._4_4_;
        uVar185 = -(uint)(iVar311 < (int)(uStack_560 ^ _DAT_180010000));
        uVar207 = -(uint)((int)uVar354 < (int)(uStack_55c ^ _UNK_180010004));
        uVar214 = -(uint)(iVar364 < (int)((uint)uStack_4c0 ^ _UNK_180010008));
        uVar221 = -(uint)((int)uVar371 < (int)(uStack_4c0._4_4_ ^ _UNK_18001000c));
        uVar419 = -(uint)((uStack_55c ^ _UNK_180010004) == uVar354);
        uVar442 = -(uint)((uStack_4c0._4_4_ ^ _UNK_18001000c) == uVar371);
        auVar20._4_4_ = _UNK_180010004;
        auVar20._0_4_ = _DAT_180010000;
        auVar20._8_4_ = _UNK_180010008;
        auVar20._12_4_ = _UNK_18001000c;
        local_338 = local_338 ^ auVar20;
        iVar480 = local_338._0_4_;
        uVar493 = local_338._4_4_;
        iVar496 = local_338._8_4_;
        uVar497 = local_338._12_4_;
        uVar390 = (ulonglong)puStack_2f0 ^ CONCAT44(_UNK_180010004,_DAT_180010000);
        uVar437 = (ulonglong)puStack_4d0 ^ CONCAT44(_UNK_18001000c,_UNK_180010008);
        uVar439 = -(uint)(iVar311 < (int)uVar390);
        uVar440 = (uint)(uVar390 >> 0x20);
        uVar105 = -(uint)((int)uVar354 < (int)uVar440);
        uVar178 = (uint)(uVar437 >> 0x20);
        uVar115 = -(uint)(iVar364 < (int)uVar437);
        uVar124 = -(uint)((int)uVar371 < (int)uVar178);
        uVar158 = -(uint)(uVar440 == uVar354);
        uVar440 = -(uint)(uVar178 == uVar371);
        uStack_4c0 = 0;
        uVar390 = (ulonglong)puStack_120 ^ CONCAT44(_UNK_180010004,_DAT_180010000);
        uVar437 = (ulonglong)local_5a8 ^ CONCAT44(_UNK_18001000c,_UNK_180010008);
        iVar499 = (int)uVar390;
        uVar509 = (uint)(uVar390 >> 0x20);
        iVar511 = (int)uVar437;
        uVar513 = (uint)(uVar437 >> 0x20);
        uVar169 = (-(uint)((int)uVar513 < (int)uVar477) |
                  -(uint)(uVar477 == uVar513) & -(uint)(iVar511 < (int)uVar474)) &
                  (uVar124 | uVar440 & uVar115);
        uVar177 = (-(uint)((int)uVar513 < (int)uVar477) |
                  -(uint)(uVar477 == uVar513) & -(uint)(iVar511 < (int)uVar474)) &
                  (uVar124 | uVar440 & uVar115);
        uVar440 = -(uint)(iVar311 < (int)((uint)local_4a8 ^ _DAT_180010000));
        uVar106 = -(uint)((int)uVar354 < (int)(local_4a8._4_4_ ^ _UNK_180010004));
        uVar115 = -(uint)(iVar364 < (int)(uVar461 ^ _UNK_180010008));
        uStack_49c = -(uint)((int)uVar371 < (int)(uStack_4dc ^ _UNK_18001000c));
        uVar159 = -(uint)((local_4a8._4_4_ ^ _UNK_180010004) == uVar354);
        uVar124 = -(uint)((uStack_4dc ^ _UNK_18001000c) == uVar371);
        uStack_4a0 = uStack_49c | uVar124 & uVar115;
        uStack_49c = uStack_49c | uVar124 & uVar115;
        auVar94._8_4_ = local_4e8;
        auVar94._0_8_ = local_248;
        auVar94._12_4_ = uStack_4e4;
        auVar21._4_4_ = _UNK_180010004;
        auVar21._0_4_ = _DAT_180010000;
        auVar21._8_4_ = _UNK_180010008;
        auVar21._12_4_ = _UNK_18001000c;
        auVar94 = auVar94 ^ auVar21;
        uVar130 = -(uint)(auVar94._0_4_ < (int)uVar462);
        uVar160 = -(uint)((int)auVar94._4_4_ < (int)uVar469);
        uVar124 = -(uint)(auVar94._8_4_ < (int)uVar474);
        uVar178 = -(uint)((int)auVar94._12_4_ < (int)uVar477);
        uVar107 = -(uint)(auVar94._4_4_ == uVar469);
        uVar115 = -(uint)(auVar94._12_4_ == uVar477);
        auVar22._4_4_ = _UNK_180010004;
        auVar22._0_4_ = _DAT_180010000;
        auVar22._8_4_ = _UNK_180010008;
        auVar22._12_4_ = _UNK_18001000c;
        auVar555 = auVar555 ^ auVar22;
        iVar552 = auVar555._0_4_;
        uVar557 = auVar555._4_4_;
        iVar559 = auVar555._8_4_;
        uVar561 = auVar555._12_4_;
        uStack_4b0 = -(uint)((int)uVar561 < (int)uVar477) |
                     -(uint)(uVar477 == uVar561) & -(uint)(iVar559 < (int)uVar474);
        uStack_4ac = -(uint)((int)uVar561 < (int)uVar477) |
                     -(uint)(uVar477 == uVar561) & -(uint)(iVar559 < (int)uVar474);
        uVar186 = -(uint)(iVar311 < (int)((uint)uStack_360 ^ _DAT_180010000));
        uVar208 = -(uint)((int)uVar354 < (int)(uStack_360._4_4_ ^ _UNK_180010004));
        uVar215 = -(uint)(iVar364 < (int)((uint)local_5c8 ^ _UNK_180010008));
        uVar222 = -(uint)((int)uVar371 < (int)(local_5c8._4_4_ ^ _UNK_18001000c));
        uVar249 = -(uint)((uStack_360._4_4_ ^ _UNK_180010004) == uVar354);
        uVar259 = -(uint)((local_5c8._4_4_ ^ _UNK_18001000c) == uVar371);
        uStack_490 = (uVar178 | uVar115 & uVar124) & (uVar222 | uVar259 & uVar215);
        uStack_48c = (uVar178 | uVar115 & uVar124) & (uVar222 | uVar259 & uVar215);
        uVar115 = -(uint)(iVar311 < (int)(uStack_460 ^ _DAT_180010000));
        uStack_454 = -(uint)((int)uVar354 < (int)(uStack_45c ^ _UNK_180010004));
        uVar178 = -(uint)(iVar364 < (int)(uStack_450 ^ _UNK_180010008));
        uVar125 = -(uint)((int)uVar371 < (int)(uStack_44c ^ _UNK_18001000c));
        uVar215 = -(uint)((uStack_45c ^ _UNK_180010004) == uVar354);
        uVar222 = -(uint)((uStack_44c ^ _UNK_18001000c) == uVar371);
        uVar124 = uStack_454 | uVar215 & uVar115;
        uStack_454 = uStack_454 | uVar215 & uVar115;
        uVar116 = uVar125 | uVar222 & uVar178;
        uVar125 = uVar125 | uVar222 & uVar178;
        auVar145._8_8_ = puStack_520;
        auVar145._0_8_ = puStack_540;
        auVar272._8_4_ = uVar304;
        auVar272._0_8_ = puStack_520;
        auVar272._12_4_ = uVar306;
        auVar23._4_4_ = _UNK_180010004;
        auVar23._0_4_ = _DAT_180010000;
        auVar23._8_4_ = _UNK_180010008;
        auVar23._12_4_ = _UNK_18001000c;
        auVar145 = auVar145 ^ auVar23;
        auVar24._4_4_ = _UNK_180010004;
        auVar24._0_4_ = _DAT_180010000;
        auVar24._8_4_ = _UNK_180010008;
        auVar24._12_4_ = _UNK_18001000c;
        auVar242 = auVar242 ^ auVar24;
        iVar98 = auVar242._0_4_;
        uVar115 = -(uint)(iVar98 < auVar145._0_4_);
        iVar117 = auVar242._4_4_;
        uVar108 = -(uint)(iVar117 < auVar145._4_4_);
        iVar227 = auVar242._8_4_;
        uVar215 = -(uint)(iVar227 < auVar145._8_4_);
        iVar245 = auVar242._12_4_;
        uVar259 = -(uint)(iVar245 < auVar145._12_4_);
        uVar161 = -(uint)(auVar145._4_4_ == iVar117);
        uVar179 = -(uint)(auVar145._12_4_ == iVar245);
        uVar463 = uStack_550 ^ _DAT_180010000;
        uVar470 = uStack_54c ^ _UNK_180010004;
        uVar475 = uStack_570 ^ _UNK_180010008;
        uVar478 = uStack_56c ^ _UNK_18001000c;
        auVar146._8_8_ = puStack_310;
        auVar146._0_8_ = puStack_4d0;
        auVar294._8_4_ = uStack_470;
        auVar294._0_8_ = puStack_310;
        auVar294._12_4_ = uStack_46c;
        auVar25._4_4_ = _UNK_180010004;
        auVar25._0_4_ = _DAT_180010000;
        auVar25._8_4_ = _UNK_180010008;
        auVar25._12_4_ = _UNK_18001000c;
        auVar146 = auVar146 ^ auVar25;
        uVar178 = -(uint)(iVar98 < auVar146._0_4_);
        uVar109 = -(uint)(iVar117 < auVar146._4_4_);
        uVar222 = -(uint)(iVar227 < auVar146._8_4_);
        uVar126 = -(uint)(iVar245 < auVar146._12_4_);
        uVar162 = -(uint)(auVar146._4_4_ == iVar117);
        uVar180 = -(uint)(auVar146._12_4_ == iVar245);
        auVar337._8_8_ = puStack_300;
        auVar337._0_8_ = puStack_440;
        auVar195._8_4_ = uVar461;
        auVar195._0_8_ = puStack_300;
        auVar195._12_4_ = uStack_4dc;
        auVar26._4_4_ = _UNK_180010004;
        auVar26._0_4_ = _DAT_180010000;
        auVar26._8_4_ = _UNK_180010008;
        auVar26._12_4_ = _UNK_18001000c;
        auVar337 = auVar337 ^ auVar26;
        uVar131 = -(uint)(iVar98 < auVar337._0_4_);
        uVar163 = -(uint)(iVar117 < auVar337._4_4_);
        uVar170 = -(uint)(iVar227 < auVar337._8_4_);
        uVar181 = -(uint)(iVar245 < auVar337._12_4_);
        uVar357 = -(uint)(auVar337._4_4_ == iVar117);
        uVar374 = -(uint)(auVar337._12_4_ == iVar245);
        auVar539._8_8_ = puStack_2f0;
        auVar539._0_8_ = puStack_230;
        auVar147._8_4_ = (uint)local_5c8;
        auVar147._0_8_ = puStack_2f0;
        auVar147._12_4_ = local_5c8._4_4_;
        auVar27._4_4_ = _UNK_180010004;
        auVar27._0_4_ = _DAT_180010000;
        auVar27._8_4_ = _UNK_180010008;
        auVar27._12_4_ = _UNK_18001000c;
        auVar539 = auVar539 ^ auVar27;
        uVar312 = -(uint)(iVar98 < auVar539._0_4_);
        uVar358 = -(uint)(iVar117 < auVar539._4_4_);
        uVar365 = -(uint)(iVar227 < auVar539._8_4_);
        uVar375 = -(uint)(iVar245 < auVar539._12_4_);
        uVar542 = -(uint)(auVar539._4_4_ == iVar117);
        uVar548 = -(uint)(auVar539._12_4_ == iVar245);
        uStack_570 = (-(uint)((int)uVar509 < (int)uVar470) |
                     -(uint)(uVar470 == uVar509) & -(uint)(iVar499 < (int)uVar463)) &
                     (uVar375 | uVar548 & uVar365) |
                     (-(uint)((int)uVar493 < (int)uVar470) |
                     -(uint)(uVar470 == uVar493) & -(uint)(iVar480 < (int)uVar463)) &
                     (uVar181 | uVar374 & uVar170) |
                     (-(uint)((int)uVar527 < (int)uVar470) |
                     -(uint)(uVar470 == uVar527) & -(uint)((int)uVar515 < (int)uVar463)) &
                     (uVar126 | uVar180 & uVar222) |
                     (-(uint)((int)uVar455 < (int)uVar470) |
                     -(uint)(uVar470 == uVar455) & -(uint)(iVar449 < (int)uVar463)) &
                     (uVar259 | uVar179 & uVar215);
        uStack_56c = (-(uint)((int)uVar509 < (int)uVar470) |
                     -(uint)(uVar470 == uVar509) & -(uint)(iVar499 < (int)uVar463)) &
                     (uVar375 | uVar548 & uVar365) |
                     (-(uint)((int)uVar493 < (int)uVar470) |
                     -(uint)(uVar470 == uVar493) & -(uint)(iVar480 < (int)uVar463)) &
                     (uVar181 | uVar374 & uVar170) |
                     (-(uint)((int)uVar527 < (int)uVar470) |
                     -(uint)(uVar470 == uVar527) & -(uint)((int)uVar515 < (int)uVar463)) &
                     (uVar126 | uVar180 & uVar222) |
                     (-(uint)((int)uVar455 < (int)uVar470) |
                     -(uint)(uVar470 == uVar455) & -(uint)(iVar449 < (int)uVar463)) &
                     (uVar259 | uVar179 & uVar215);
        auVar407._8_8_ = local_438;
        auVar407._0_8_ = puStack_210;
        auVar490._8_4_ = uStack_450;
        auVar490._0_8_ = local_438;
        auVar490._12_4_ = uStack_44c;
        auVar28._4_4_ = _UNK_180010004;
        auVar28._0_4_ = _DAT_180010000;
        auVar28._8_4_ = _UNK_180010008;
        auVar28._12_4_ = _UNK_18001000c;
        auVar407 = auVar407 ^ auVar28;
        uVar313 = -(uint)(iVar98 < auVar407._0_4_);
        uVar359 = -(uint)(iVar117 < auVar407._4_4_);
        uVar366 = -(uint)(iVar227 < auVar407._8_4_);
        uVar376 = -(uint)(iVar245 < auVar407._12_4_);
        uVar420 = -(uint)(auVar407._4_4_ == iVar117);
        uVar443 = -(uint)(auVar407._12_4_ == iVar245);
        uVar215 = (uint)local_588 ^ _DAT_180010000;
        uVar222 = local_588._4_4_ ^ _UNK_180010004;
        uStack_580 = uStack_580 ^ _UNK_180010008;
        uStack_57c = uStack_57c ^ _UNK_18001000c;
        uVar314 = -(uint)((int)uVar215 < (int)((uint)fStack_630 ^ _DAT_180010000));
        uVar360 = -(uint)((int)uVar222 < (int)((uint)fStack_62c ^ _UNK_180010004));
        uVar367 = -(uint)((int)uStack_580 < (int)((uint)uStack_360 ^ _UNK_180010008));
        uVar377 = -(uint)((int)uStack_57c < (int)(uStack_360._4_4_ ^ _UNK_18001000c));
        uVar421 = -(uint)(((uint)fStack_62c ^ _UNK_180010004) == uVar222);
        uVar444 = -(uint)((uStack_360._4_4_ ^ _UNK_18001000c) == uStack_57c);
        uVar464 = (uint)local_558 ^ _DAT_180010000;
        uVar471 = (uint)fStack_554 ^ _UNK_180010004;
        uVar476 = uStack_550 ^ _UNK_180010008;
        uVar479 = uStack_54c ^ _UNK_18001000c;
        uVar315 = -(uint)((int)(local_608 ^ _DAT_180010000) < (int)uVar464);
        uVar361 = -(uint)((int)(uStack_604 ^ _UNK_180010004) < (int)uVar471);
        uVar368 = -(uint)((int)(local_538 ^ _UNK_180010008) < (int)uVar476);
        uVar378 = -(uint)((int)(uStack_534 ^ _UNK_18001000c) < (int)uVar479);
        uVar422 = -(uint)((uStack_604 ^ _UNK_180010004) == uVar471);
        uVar445 = -(uint)((uStack_534 ^ _UNK_18001000c) == uVar479);
        uVar390 = (ulonglong)puVar70 ^ CONCAT44(_UNK_180010004,_DAT_180010000);
        uVar384 = -(uint)((int)uVar215 < (int)uVar390);
        uVar259 = (uint)(uVar390 >> 0x20);
        uVar423 = -(uint)((int)uVar222 < (int)uVar259);
        uVar433 = -(uint)((int)uStack_580 < (int)(uStack_460 ^ _UNK_180010008));
        uVar446 = -(uint)((int)uStack_57c < (int)(uStack_45c ^ _UNK_18001000c));
        uVar456 = -(uint)(uVar259 == uVar222);
        uVar460 = -(uint)((uStack_45c ^ _UNK_18001000c) == uStack_57c);
        auVar29._4_4_ = _UNK_180010004;
        auVar29._0_4_ = _DAT_180010000;
        auVar29._8_4_ = _UNK_180010008;
        auVar29._12_4_ = _UNK_18001000c;
        auVar272 = auVar272 ^ auVar29;
        uVar385 = -(uint)((int)uVar215 < auVar272._0_4_);
        uVar424 = -(uint)((int)uVar222 < (int)auVar272._4_4_);
        uVar434 = -(uint)((int)uStack_580 < auVar272._8_4_);
        uVar447 = -(uint)((int)uStack_57c < (int)auVar272._12_4_);
        uVar276 = -(uint)(auVar272._4_4_ == uVar222);
        uVar288 = -(uint)(auVar272._12_4_ == uStack_57c);
        auVar30._4_4_ = _UNK_180010004;
        auVar30._0_4_ = _DAT_180010000;
        auVar30._8_4_ = _UNK_180010008;
        auVar30._12_4_ = _UNK_18001000c;
        auVar294 = auVar294 ^ auVar30;
        uVar316 = -(uint)((int)uVar215 < auVar294._0_4_);
        uVar362 = -(uint)((int)uVar222 < (int)auVar294._4_4_);
        uVar369 = -(uint)((int)uStack_580 < auVar294._8_4_);
        uVar379 = -(uint)((int)uStack_57c < (int)auVar294._12_4_);
        uVar303 = -(uint)(auVar294._4_4_ == uVar222);
        uVar309 = -(uint)(auVar294._12_4_ == uStack_57c);
        auVar31._4_4_ = _UNK_180010004;
        auVar31._0_4_ = _DAT_180010000;
        auVar31._8_4_ = _UNK_180010008;
        auVar31._12_4_ = _UNK_18001000c;
        auVar195 = auVar195 ^ auVar31;
        uVar263 = -(uint)((int)uVar215 < auVar195._0_4_);
        uVar277 = -(uint)((int)uVar222 < (int)auVar195._4_4_);
        uVar283 = -(uint)((int)uStack_580 < auVar195._8_4_);
        uVar289 = -(uint)((int)uStack_57c < (int)auVar195._12_4_);
        uVar209 = -(uint)(auVar195._4_4_ == uVar222);
        uVar223 = -(uint)(auVar195._12_4_ == uStack_57c);
        auVar32._4_4_ = _UNK_180010004;
        auVar32._0_4_ = _DAT_180010000;
        auVar32._8_4_ = _UNK_180010008;
        auVar32._12_4_ = _UNK_18001000c;
        auVar147 = auVar147 ^ auVar32;
        uVar264 = -(uint)((int)uVar215 < auVar147._0_4_);
        uVar278 = -(uint)((int)uVar222 < (int)auVar147._4_4_);
        uVar284 = -(uint)((int)uStack_580 < auVar147._8_4_);
        uVar290 = -(uint)((int)uStack_57c < (int)auVar147._12_4_);
        uVar375 = -(uint)(auVar147._4_4_ == uVar222);
        uVar182 = -(uint)(auVar147._12_4_ == uStack_57c);
        auVar33._4_4_ = _UNK_180010004;
        auVar33._0_4_ = _DAT_180010000;
        auVar33._8_4_ = _UNK_180010008;
        auVar33._12_4_ = _UNK_18001000c;
        auVar490 = auVar490 ^ auVar33;
        uVar265 = -(uint)((int)uVar215 < auVar490._0_4_);
        uVar279 = -(uint)((int)uVar222 < (int)auVar490._4_4_);
        uVar285 = -(uint)((int)uStack_580 < auVar490._8_4_);
        uVar291 = -(uint)((int)uStack_57c < (int)auVar490._12_4_);
        uVar494 = -(uint)(auVar490._4_4_ == uVar222);
        uVar498 = -(uint)(auVar490._12_4_ == uStack_57c);
        uVar215 = -(uint)(iVar262 < (int)((uint)uStack_360 ^ _DAT_180010000));
        uVar170 = -(uint)((int)uVar275 < (int)(uStack_360._4_4_ ^ _UNK_180010004));
        uVar259 = -(uint)(iVar282 < (int)((uint)local_468 ^ _UNK_180010008));
        uVar180 = -(uint)((int)uVar287 < (int)((uint)fStack_464 ^ _UNK_18001000c));
        uVar548 = -(uint)((uStack_360._4_4_ ^ _UNK_180010004) == uVar275);
        uVar365 = -(uint)(((uint)fStack_464 ^ _UNK_18001000c) == uVar287);
        uVar222 = -(uint)(iVar262 < (int)(uStack_460 ^ _DAT_180010000));
        uVar179 = -(uint)((int)uVar275 < (int)(uStack_45c ^ _UNK_180010004));
        uVar126 = -(uint)(iVar282 < (int)(uStack_560 ^ _UNK_180010008));
        uVar181 = -(uint)((int)uVar287 < (int)(uStack_55c ^ _UNK_18001000c));
        uVar164 = -(uint)((uStack_45c ^ _UNK_180010004) == uVar275);
        uVar374 = -(uint)((uStack_55c ^ _UNK_18001000c) == uVar287);
        uVar254 = (uVar119 | uVar307 & uVar111) & (uVar181 | uVar374 & uVar126) |
                  (uVar441 | uVar118 & uVar432) & (uVar180 | uVar365 & uVar259);
        uVar260 = (uVar119 | uVar307 & uVar111) & (uVar181 | uVar374 & uVar126) |
                  (uVar441 | uVar118 & uVar432) & (uVar180 | uVar365 & uVar259);
        uVar111 = -(uint)(iVar262 < (int)(uVar304 ^ _DAT_180010000));
        uVar118 = -(uint)((int)uVar275 < (int)(uVar306 ^ _UNK_180010004));
        uVar259 = -(uint)(iVar282 < (int)(uStack_480 ^ _UNK_180010008));
        uVar126 = -(uint)((int)uVar287 < (int)(uStack_47c ^ _UNK_18001000c));
        uVar307 = -(uint)((uVar306 ^ _UNK_180010004) == uVar275);
        uVar119 = -(uint)((uStack_47c ^ _UNK_18001000c) == uVar287);
        uStack_580 = (uVar308 | uVar120 & uStack_4e0) & (uVar126 | uVar119 & uVar259);
        uStack_57c = (uVar308 | uVar120 & uStack_4e0) & (uVar126 | uVar119 & uVar259);
        uVar259 = -(uint)(iVar262 < (int)(uStack_470 ^ _DAT_180010000));
        uVar119 = -(uint)((int)uVar275 < (int)(uStack_46c ^ _UNK_180010004));
        uVar120 = -(uint)(iVar282 < (int)((uint)local_4a8 ^ _UNK_180010008));
        uVar374 = -(uint)((int)uVar287 < (int)(local_4a8._4_4_ ^ _UNK_18001000c));
        uVar180 = -(uint)((uStack_46c ^ _UNK_180010004) == uVar275);
        uVar365 = -(uint)((local_4a8._4_4_ ^ _UNK_18001000c) == uVar287);
        uVar126 = uVar119 | uVar180 & uVar259;
        uVar119 = uVar119 | uVar180 & uVar259;
        uVar181 = uVar374 | uVar365 & uVar120;
        uVar374 = uVar374 | uVar365 & uVar120;
        local_598._4_4_ = uVar102 | uStack_3b4 & uVar370;
        local_598._0_4_ = uVar102 | uStack_3b4 & uVar370;
        local_598._8_4_ = uVar121 | uStack_3ac & uVar112;
        local_598._12_4_ = uVar121 | uStack_3ac & uVar112;
        uVar370 = -(uint)(iVar262 < (int)(uVar461 ^ _DAT_180010000));
        uVar259 = -(uint)((int)uVar275 < (int)(uStack_4dc ^ _UNK_180010004));
        uVar102 = -(uint)(iVar282 < (int)((uint)uStack_360 ^ _UNK_180010008));
        uVar112 = -(uint)((int)uVar287 < (int)(uStack_360._4_4_ ^ _UNK_18001000c));
        uVar120 = -(uint)((uStack_4dc ^ _UNK_180010004) == uVar275);
        uVar121 = -(uint)((uStack_360._4_4_ ^ _UNK_18001000c) == uVar287);
        local_618._4_4_ = uVar259 | uVar120 & uVar370;
        local_618._0_4_ = uVar259 | uVar120 & uVar370;
        local_618._8_4_ = uVar112 | uVar121 & uVar102;
        local_618._12_4_ = uVar112 | uVar121 & uVar102;
        local_5b8 = uStack_5b4 | uStack_3d4 & uVar183;
        uStack_5b4 = uStack_5b4 | uStack_3d4 & uVar183;
        uVar171 = uVar218 | uStack_3cc & uVar212;
        uVar218 = uVar218 | uStack_3cc & uVar212;
        uVar370 = -(uint)(iVar262 < (int)((uint)local_5c8 ^ _DAT_180010000));
        uVar120 = -(uint)((int)uVar275 < (int)(local_5c8._4_4_ ^ _UNK_180010004));
        uVar259 = -(uint)(iVar282 < (int)(uStack_460 ^ _UNK_180010008));
        uStack_5bc = -(uint)((int)uVar287 < (int)(uStack_45c ^ _UNK_18001000c));
        uVar212 = -(uint)((local_5c8._4_4_ ^ _UNK_180010004) == uVar275);
        uVar102 = -(uint)((uStack_45c ^ _UNK_18001000c) == uVar287);
        uStack_5c0 = uStack_5bc | uVar102 & uVar259;
        uStack_5bc = uStack_5bc | uVar102 & uVar259;
        iVar98 = -(uint)(iVar262 < (int)(uStack_450 ^ _DAT_180010000));
        iVar117 = -(uint)((int)uVar275 < (int)(uStack_44c ^ _UNK_180010004));
        iVar227 = -(uint)(iVar282 < (int)(uVar304 ^ _UNK_180010008));
        iVar245 = -(uint)((int)uVar287 < (int)(uVar306 ^ _UNK_18001000c));
        auVar148._4_4_ = -(uint)((uStack_44c ^ _UNK_180010004) == uVar275);
        auVar148._12_4_ = -(uint)((uVar306 ^ _UNK_18001000c) == uVar287);
        auVar148._0_4_ = auVar148._4_4_;
        auVar148._8_4_ = auVar148._12_4_;
        auVar196._4_4_ = iVar98;
        auVar196._0_4_ = iVar98;
        auVar196._8_4_ = iVar227;
        auVar196._12_4_ = iVar227;
        auVar556._4_4_ = iVar117;
        auVar556._0_4_ = iVar117;
        auVar556._8_4_ = iVar245;
        auVar556._12_4_ = iVar245;
        uVar259 = -(uint)((int)(local_608 ^ _DAT_180010000) < (int)(uStack_550 ^ _DAT_180010000));
        uVar102 = -(uint)((int)(uStack_604 ^ _UNK_180010004) < (int)(uStack_54c ^ _UNK_180010004));
        uVar112 = -(uint)((int)(uStack_600 ^ _UNK_180010008) < (int)(uStack_470 ^ _UNK_180010008));
        uVar121 = -(uint)((int)(uStack_5fc ^ _UNK_18001000c) < (int)(uStack_46c ^ _UNK_18001000c));
        uVar441 = -(uint)((uStack_54c ^ _UNK_180010004) == (uStack_604 ^ _UNK_180010004));
        uVar224 = -(uint)((uStack_46c ^ _UNK_18001000c) == (uStack_5fc ^ _UNK_18001000c));
        uVar180 = -(uint)(iVar229 < (int)((uint)fStack_630 ^ _DAT_180010000));
        uVar365 = -(uint)((int)uVar248 < (int)((uint)fStack_62c ^ _UNK_180010004));
        uVar183 = -(uint)(iVar253 < (int)((uint)local_4b8 ^ _UNK_180010008));
        uVar432 = -(uint)((int)uVar258 < (int)(local_4b8._4_4_ ^ _UNK_18001000c));
        uVar210 = -(uint)(((uint)fStack_62c ^ _UNK_180010004) == uVar248);
        uVar225 = -(uint)((local_4b8._4_4_ ^ _UNK_18001000c) == uVar258);
        local_608 = (uVar365 | uVar210 & uVar180) & (uVar102 | uVar441 & uVar259);
        uStack_604 = (uVar365 | uVar210 & uVar180) & (uVar102 | uVar441 & uVar259);
        uVar210 = (uVar432 | uVar225 & uVar183) & (uVar121 | uVar224 & uVar112);
        uVar224 = (uVar432 | uVar225 & uVar183) & (uVar121 | uVar224 & uVar112);
        uVar386 = uStack_600 ^ _DAT_180010000;
        uVar425 = uStack_5fc ^ _UNK_180010004;
        uVar435 = (uint)puVar62 ^ _UNK_180010008;
        uVar492 = uVar492 ^ _UNK_18001000c;
        uVar259 = -(uint)((int)uVar386 < (int)(uVar461 ^ _DAT_180010000));
        uVar121 = -(uint)((int)uVar425 < (int)(uStack_4dc ^ _UNK_180010004));
        uVar102 = -(uint)((int)uVar435 < (int)(uStack_460 ^ _UNK_180010008));
        uVar112 = -(uint)((int)uVar492 < (int)(uStack_45c ^ _UNK_18001000c));
        uVar432 = -(uint)((uStack_4dc ^ _UNK_180010004) == uVar425);
        uVar180 = -(uint)((uStack_45c ^ _UNK_18001000c) == uVar492);
        fStack_630 = (float)(uVar112 | uVar180 & uVar102);
        fStack_62c = (float)(uVar112 | uVar180 & uVar102);
        auVar540._8_4_ = local_4e8;
        auVar540._0_8_ = local_248;
        auVar540._12_4_ = uStack_4e4;
        auVar149._0_8_ = auVar540._8_8_;
        auVar149._8_4_ = local_538;
        auVar149._12_4_ = uStack_534;
        auVar34._4_4_ = _UNK_180010004;
        auVar34._0_4_ = _DAT_180010000;
        auVar34._8_4_ = _UNK_180010008;
        auVar34._12_4_ = _UNK_18001000c;
        auVar540 = auVar540 ^ auVar34;
        uVar250 = local_4b8._4_4_ ^ _UNK_180010004;
        uVar261 = uStack_55c ^ _UNK_18001000c;
        uVar102 = -(uint)(auVar540._0_4_ < (int)((uint)local_4b8 ^ _DAT_180010000));
        uVar543 = auVar540._4_4_;
        uVar112 = -(uint)(auVar540._8_4_ < (int)(uStack_560 ^ _UNK_180010008));
        uVar549 = auVar540._12_4_;
        fStack_620 = (float)(-(uint)((int)uVar549 < (int)uVar261) |
                            -(uint)(uVar261 == uVar549) & uVar112);
        fStack_61c = (float)(-(uint)((int)uVar549 < (int)uVar261) |
                            -(uint)(uVar261 == uVar549) & uVar112);
        uVar112 = -(uint)((int)uVar386 < (int)((uint)local_5c8 ^ _DAT_180010000));
        uVar180 = -(uint)((int)uVar425 < (int)(local_5c8._4_4_ ^ _UNK_180010004));
        uVar365 = -(uint)((int)uVar435 < (int)(uVar304 ^ _UNK_180010008));
        uVar183 = -(uint)((int)uVar492 < (int)(uVar306 ^ _UNK_18001000c));
        uVar211 = -(uint)((local_5c8._4_4_ ^ _UNK_180010004) == uVar425);
        uVar226 = -(uint)((uVar306 ^ _UNK_18001000c) == uVar492);
        auVar35._4_4_ = _UNK_180010004;
        auVar35._0_4_ = _DAT_180010000;
        auVar35._8_4_ = _UNK_180010008;
        auVar35._12_4_ = _UNK_18001000c;
        auVar149 = auVar149 ^ auVar35;
        iVar227 = auVar149._0_4_;
        uVar187 = -(uint)(iVar227 < (int)((uint)local_4b8 ^ _DAT_180010000));
        uVar441 = auVar149._4_4_;
        iVar245 = auVar149._8_4_;
        uVar216 = -(uint)(iVar245 < (int)(uStack_560 ^ _UNK_180010008));
        uVar225 = auVar149._12_4_;
        local_648._4_4_ =
             (-(uint)((int)uVar441 < (int)uVar250) | -(uint)(uVar250 == uVar441) & uVar187) &
             (uVar180 | uVar211 & uVar112);
        local_648._0_4_ =
             (-(uint)((int)uVar441 < (int)uVar250) | -(uint)(uVar250 == uVar441) & uVar187) &
             (uVar180 | uVar211 & uVar112);
        local_648._8_4_ =
             (-(uint)((int)uVar225 < (int)uVar261) | -(uint)(uVar261 == uVar225) & uVar216) &
             (uVar183 | uVar226 & uVar365);
        uStack_63c = (-(uint)((int)uVar225 < (int)uVar261) | -(uint)(uVar261 == uVar225) & uVar216)
                     & (uVar183 | uVar226 & uVar365);
        iVar251 = -(uint)((int)uVar386 < (int)(uStack_450 ^ _DAT_180010000));
        iVar255 = -(uint)((int)uVar425 < (int)(uStack_44c ^ _UNK_180010004));
        iVar429 = -(uint)((int)uVar435 < (int)(uStack_470 ^ _UNK_180010008));
        iVar438 = -(uint)((int)uVar492 < (int)(uStack_46c ^ _UNK_18001000c));
        iVar98 = -(uint)((uStack_44c ^ _UNK_180010004) == uVar425);
        iVar117 = -(uint)((uStack_46c ^ _UNK_18001000c) == uVar492);
        auVar295._4_4_ = iVar98;
        auVar295._0_4_ = iVar98;
        auVar295._8_4_ = iVar117;
        auVar295._12_4_ = iVar117;
        auVar338._4_4_ = iVar251;
        auVar338._0_4_ = iVar251;
        auVar338._8_4_ = iVar429;
        auVar338._12_4_ = iVar429;
        auVar508._4_4_ = iVar255;
        auVar508._0_4_ = iVar255;
        auVar508._8_4_ = iVar438;
        auVar508._12_4_ = iVar438;
        iVar251 = -(uint)(iVar228 < (int)(uStack_560 ^ _DAT_180010000));
        auVar197._4_4_ = -(uint)((int)uVar246 < (int)(uStack_55c ^ _UNK_180010004));
        iVar255 = -(uint)(iVar252 < (int)(uVar461 ^ _UNK_180010008));
        auVar197._12_4_ = -(uint)((int)uVar256 < (int)(uStack_4dc ^ _UNK_18001000c));
        iVar98 = -(uint)((uStack_55c ^ _UNK_180010004) == uVar246);
        iVar117 = -(uint)((uStack_4dc ^ _UNK_18001000c) == uVar256);
        auVar296._4_4_ = iVar98;
        auVar296._0_4_ = iVar98;
        auVar296._8_4_ = iVar117;
        auVar296._12_4_ = iVar117;
        auVar339._4_4_ = iVar251;
        auVar339._0_4_ = iVar251;
        auVar339._8_4_ = iVar255;
        auVar339._12_4_ = iVar255;
        auVar197._0_4_ = auVar197._4_4_;
        auVar197._8_4_ = auVar197._12_4_;
        auVar340._4_4_ = -(uint)(uVar493 == uVar204);
        auVar340._0_4_ = -(uint)(uVar493 == uVar204);
        auVar340._8_4_ = -(uint)(uVar497 == uVar217);
        auVar340._12_4_ = -(uint)(uVar497 == uVar217);
        auVar408._4_4_ = -(uint)(iVar480 < iVar305);
        auVar408._0_4_ = -(uint)(iVar480 < iVar305);
        auVar408._8_4_ = -(uint)(iVar496 < iVar310);
        auVar408._12_4_ = -(uint)(iVar496 < iVar310);
        auVar566._4_4_ = -(uint)((int)uVar493 < (int)uVar204);
        auVar566._0_4_ = -(uint)((int)uVar493 < (int)uVar204);
        auVar566._8_4_ = -(uint)((int)uVar497 < (int)uVar217);
        auVar566._12_4_ = -(uint)((int)uVar497 < (int)uVar217);
        iVar251 = -(uint)(iVar228 < (int)(uStack_480 ^ _DAT_180010000));
        auVar198._4_4_ = -(uint)((int)uVar246 < (int)(uStack_47c ^ _UNK_180010004));
        iVar255 = -(uint)(iVar252 < (int)((uint)local_5c8 ^ _UNK_180010008));
        auVar198._12_4_ = -(uint)((int)uVar256 < (int)(local_5c8._4_4_ ^ _UNK_18001000c));
        iVar98 = -(uint)((uStack_47c ^ _UNK_180010004) == uVar246);
        iVar117 = -(uint)((local_5c8._4_4_ ^ _UNK_18001000c) == uVar256);
        auVar297._4_4_ = iVar98;
        auVar297._0_4_ = iVar98;
        auVar297._8_4_ = iVar117;
        auVar297._12_4_ = iVar117;
        auVar341._4_4_ = iVar251;
        auVar341._0_4_ = iVar251;
        auVar341._8_4_ = iVar255;
        auVar341._12_4_ = iVar255;
        auVar198._0_4_ = auVar198._4_4_;
        auVar198._8_4_ = auVar198._12_4_;
        iVar251 = -(uint)((int)(uStack_5b0 ^ _DAT_180010000) < iVar305);
        auVar298._4_4_ = -(uint)((int)(uStack_5ac ^ _UNK_180010004) < (int)uVar204);
        iVar255 = -(uint)((int)(local_4e8 ^ _UNK_180010008) < iVar310);
        auVar298._12_4_ = -(uint)((int)(uStack_4e4 ^ _UNK_18001000c) < (int)uVar217);
        iVar98 = -(uint)((uStack_5ac ^ _UNK_180010004) == uVar204);
        iVar117 = -(uint)((uStack_4e4 ^ _UNK_18001000c) == uVar217);
        auVar342._4_4_ = iVar98;
        auVar342._0_4_ = iVar98;
        auVar342._8_4_ = iVar117;
        auVar342._12_4_ = iVar117;
        auVar409._4_4_ = iVar251;
        auVar409._0_4_ = iVar251;
        auVar409._8_4_ = iVar255;
        auVar409._12_4_ = iVar255;
        auVar298._0_4_ = auVar298._4_4_;
        auVar298._8_4_ = auVar298._12_4_;
        iVar251 = -(uint)(iVar228 < (int)((uint)local_4a8 ^ _DAT_180010000));
        iVar255 = -(uint)((int)uVar246 < (int)(local_4a8._4_4_ ^ _UNK_180010004));
        iVar429 = -(uint)(iVar252 < (int)(uStack_450 ^ _UNK_180010008));
        iVar438 = -(uint)((int)uVar256 < (int)(uStack_44c ^ _UNK_18001000c));
        iVar98 = -(uint)((local_4a8._4_4_ ^ _UNK_180010004) == uVar246);
        iVar117 = -(uint)((uStack_44c ^ _UNK_18001000c) == uVar256);
        auVar343._4_4_ = iVar98;
        auVar343._0_4_ = iVar98;
        auVar343._8_4_ = iVar117;
        auVar343._12_4_ = iVar117;
        auVar410._4_4_ = iVar251;
        auVar410._0_4_ = iVar251;
        auVar410._8_4_ = iVar429;
        auVar410._12_4_ = iVar429;
        auVar411._4_4_ = iVar255;
        auVar411._0_4_ = iVar255;
        auVar411._8_4_ = iVar438;
        auVar411._12_4_ = iVar438;
        auVar344._4_4_ = -(uint)(uVar204 == uVar441);
        auVar344._0_4_ = -(uint)(uVar204 == uVar441);
        auVar344._8_4_ = -(uint)(uVar217 == uVar225);
        auVar344._12_4_ = -(uint)(uVar217 == uVar225);
        auVar453._4_4_ = -(uint)(iVar227 < iVar305);
        auVar453._0_4_ = -(uint)(iVar227 < iVar305);
        auVar453._8_4_ = -(uint)(iVar245 < iVar310);
        auVar453._12_4_ = -(uint)(iVar245 < iVar310);
        auVar454._4_4_ = -(uint)((int)uVar441 < (int)uVar204);
        auVar454._0_4_ = -(uint)((int)uVar441 < (int)uVar204);
        auVar454._8_4_ = -(uint)((int)uVar225 < (int)uVar217);
        auVar454._12_4_ = -(uint)((int)uVar225 < (int)uVar217);
        uVar365 = uStack_530 ^ _DAT_180010000;
        uVar183 = uStack_52c ^ _UNK_180010004;
        uVar187 = uStack_5b0 ^ _UNK_180010008;
        uVar204 = uStack_5ac ^ _UNK_18001000c;
        iVar251 = -(uint)((int)uVar365 < (int)((uint)uStack_360 ^ _DAT_180010000));
        iVar255 = -(uint)((int)uVar183 < (int)(uStack_360._4_4_ ^ _UNK_180010004));
        iVar305 = -(uint)((int)uVar187 < (int)((uint)local_4a8 ^ _UNK_180010008));
        iVar310 = -(uint)((int)uVar204 < (int)(local_4a8._4_4_ ^ _UNK_18001000c));
        iVar98 = -(uint)((uStack_360._4_4_ ^ _UNK_180010004) == uVar183);
        iVar117 = -(uint)((local_4a8._4_4_ ^ _UNK_18001000c) == uVar204);
        auVar345._4_4_ = iVar98;
        auVar345._0_4_ = iVar98;
        auVar345._8_4_ = iVar117;
        auVar345._12_4_ = iVar117;
        auVar412._4_4_ = iVar251;
        auVar412._0_4_ = iVar251;
        auVar412._8_4_ = iVar305;
        auVar412._12_4_ = iVar305;
        auVar526._4_4_ = iVar255;
        auVar526._0_4_ = iVar255;
        auVar526._8_4_ = iVar310;
        auVar526._12_4_ = iVar310;
        uVar112 = (uint)fStack_464 ^ _UNK_180010004;
        uVar180 = uStack_47c ^ _UNK_18001000c;
        iVar98 = -(uint)(auVar540._0_4_ < (int)((uint)local_468 ^ _DAT_180010000));
        iVar117 = -(uint)(auVar540._8_4_ < (int)(uStack_480 ^ _UNK_180010008));
        auVar346._4_4_ = -(uint)(uVar543 == uVar112);
        auVar346._0_4_ = -(uint)(uVar543 == uVar112);
        auVar346._8_4_ = -(uint)(uVar549 == uVar180);
        auVar346._12_4_ = -(uint)(uVar549 == uVar180);
        auVar413._4_4_ = iVar98;
        auVar413._0_4_ = iVar98;
        auVar413._8_4_ = iVar117;
        auVar413._12_4_ = iVar117;
        auVar491._4_4_ = -(uint)((int)uVar543 < (int)uVar112);
        auVar491._0_4_ = -(uint)((int)uVar543 < (int)uVar112);
        auVar491._8_4_ = -(uint)((int)uVar549 < (int)uVar180);
        auVar491._12_4_ = -(uint)((int)uVar549 < (int)uVar180);
        auVar491 = auVar491 | auVar346 & auVar413;
        iVar98 = -(uint)((int)uVar365 < (int)(uStack_460 ^ _DAT_180010000));
        iVar117 = -(uint)((int)uVar183 < (int)(uStack_45c ^ _UNK_180010004));
        iVar251 = -(uint)((int)uVar187 < (int)((uint)uStack_360 ^ _UNK_180010008));
        iVar255 = -(uint)((int)uVar204 < (int)(uStack_360._4_4_ ^ _UNK_18001000c));
        auVar347._4_4_ = -(uint)((uStack_45c ^ _UNK_180010004) == uVar183);
        auVar347._12_4_ = -(uint)((uStack_360._4_4_ ^ _UNK_18001000c) == uVar204);
        auVar347._0_4_ = auVar347._4_4_;
        auVar347._8_4_ = auVar347._12_4_;
        auVar414._4_4_ = iVar98;
        auVar414._0_4_ = iVar98;
        auVar414._8_4_ = iVar251;
        auVar414._12_4_ = iVar251;
        auVar541._4_4_ = iVar117;
        auVar541._0_4_ = iVar117;
        auVar541._8_4_ = iVar255;
        auVar541._12_4_ = iVar255;
        iVar98 = -(uint)(iVar227 < (int)((uint)local_468 ^ _DAT_180010000));
        auVar299._4_4_ = -(uint)((int)uVar441 < (int)uVar112);
        iVar117 = -(uint)(iVar245 < (int)(uStack_480 ^ _UNK_180010008));
        auVar299._12_4_ = -(uint)((int)uVar225 < (int)uVar180);
        auVar150._4_4_ = -(uint)(uVar112 == uVar441);
        auVar150._0_4_ = -(uint)(uVar112 == uVar441);
        auVar150._8_4_ = -(uint)(uVar180 == uVar225);
        auVar150._12_4_ = -(uint)(uVar180 == uVar225);
        auVar348._4_4_ = iVar98;
        auVar348._0_4_ = iVar98;
        auVar348._8_4_ = iVar117;
        auVar348._12_4_ = iVar117;
        auVar299._0_4_ = auVar299._4_4_;
        auVar299._8_4_ = auVar299._12_4_;
        auVar299 = auVar299 | auVar150 & auVar348;
        iVar98 = -(uint)((int)uVar365 < (int)(uVar304 ^ _DAT_180010000));
        iVar117 = -(uint)((int)uVar183 < (int)(uVar306 ^ _UNK_180010004));
        iVar227 = -(uint)((int)uVar187 < (int)(uStack_460 ^ _UNK_180010008));
        iVar245 = -(uint)((int)uVar204 < (int)(uStack_45c ^ _UNK_18001000c));
        iVar251 = -(uint)((uVar306 ^ _UNK_180010004) == uVar183);
        iVar255 = -(uint)((uStack_45c ^ _UNK_18001000c) == uVar204);
        auVar349._4_4_ = iVar251;
        auVar349._0_4_ = iVar251;
        auVar349._8_4_ = iVar255;
        auVar349._12_4_ = iVar255;
        auVar273._4_4_ = iVar98;
        auVar273._0_4_ = iVar98;
        auVar273._8_4_ = iVar227;
        auVar273._12_4_ = iVar227;
        auVar274._4_4_ = iVar117;
        auVar274._0_4_ = iVar117;
        auVar274._8_4_ = iVar245;
        auVar274._12_4_ = iVar245;
        iVar98 = -(uint)((int)uVar365 < (int)(uStack_470 ^ _DAT_180010000));
        iVar117 = -(uint)((int)uVar183 < (int)(uStack_46c ^ _UNK_180010004));
        iVar227 = -(uint)((int)uVar187 < (int)(uVar304 ^ _UNK_180010008));
        iVar245 = -(uint)((int)uVar204 < (int)(uVar306 ^ _UNK_18001000c));
        iVar251 = -(uint)((uStack_46c ^ _UNK_180010004) == uVar183);
        iVar255 = -(uint)((uVar306 ^ _UNK_18001000c) == uVar204);
        auVar350._4_4_ = iVar251;
        auVar350._0_4_ = iVar251;
        auVar350._8_4_ = iVar255;
        auVar350._12_4_ = iVar255;
        auVar243._4_4_ = iVar98;
        auVar243._0_4_ = iVar98;
        auVar243._8_4_ = iVar227;
        auVar243._12_4_ = iVar227;
        auVar244._4_4_ = iVar117;
        auVar244._0_4_ = iVar117;
        auVar244._8_4_ = iVar245;
        auVar244._12_4_ = iVar245;
        iVar98 = -(uint)((int)uVar365 < (int)(uVar461 ^ _DAT_180010000));
        iVar117 = -(uint)((int)uVar183 < (int)(uStack_4dc ^ _UNK_180010004));
        iVar227 = -(uint)((int)uVar187 < (int)(uStack_470 ^ _UNK_180010008));
        iVar245 = -(uint)((int)uVar204 < (int)(uStack_46c ^ _UNK_18001000c));
        iVar251 = -(uint)((uStack_4dc ^ _UNK_180010004) == uVar183);
        iVar255 = -(uint)((uStack_46c ^ _UNK_18001000c) == uVar204);
        auVar351._4_4_ = iVar251;
        auVar351._0_4_ = iVar251;
        auVar351._8_4_ = iVar255;
        auVar351._12_4_ = iVar255;
        auVar95._4_4_ = iVar98;
        auVar95._0_4_ = iVar98;
        auVar95._8_4_ = iVar227;
        auVar95._12_4_ = iVar227;
        auVar96._4_4_ = iVar117;
        auVar96._0_4_ = iVar117;
        auVar96._8_4_ = iVar245;
        auVar96._12_4_ = iVar245;
        iVar98 = -(uint)((int)uVar365 < (int)((uint)local_5c8 ^ _DAT_180010000));
        auVar151._4_4_ = -(uint)((int)uVar183 < (int)(local_5c8._4_4_ ^ _UNK_180010004));
        iVar117 = -(uint)((int)uVar187 < (int)(uVar461 ^ _UNK_180010008));
        auVar151._12_4_ = -(uint)((int)uVar204 < (int)(uStack_4dc ^ _UNK_18001000c));
        auVar352._4_4_ = -(uint)((local_5c8._4_4_ ^ _UNK_180010004) == uVar183);
        auVar352._12_4_ = -(uint)((uStack_4dc ^ _UNK_18001000c) == uVar204);
        auVar352._0_4_ = auVar352._4_4_;
        auVar352._8_4_ = auVar352._12_4_;
        auVar415._4_4_ = iVar98;
        auVar415._0_4_ = iVar98;
        auVar415._8_4_ = iVar117;
        auVar415._12_4_ = iVar117;
        auVar151._0_4_ = auVar151._4_4_;
        auVar151._8_4_ = auVar151._12_4_;
        auVar36._4_4_ = uVar121 | uVar432 & uVar259;
        auVar36._0_4_ = uVar121 | uVar432 & uVar259;
        auVar36._8_4_ = fStack_630;
        auVar36._12_4_ = fStack_62c;
        auVar37._4_4_ = -(uint)((int)uVar543 < (int)uVar250) | -(uint)(uVar250 == uVar543) & uVar102
        ;
        auVar37._0_4_ = -(uint)((int)uVar543 < (int)uVar250) | -(uint)(uVar250 == uVar543) & uVar102
        ;
        auVar37._8_4_ = fStack_620;
        auVar37._12_4_ = fStack_61c;
        auVar83 = auVar36 & auVar37 | _local_648;
        local_258 = (longlong)iVar53;
        lVar71 = local_258 * local_428;
        local_638 = lVar71 * 0x10;
        local_268 = uVar65 - 1;
        bVar56 = local_5a8 + local_268 * 0x10 < local_5a8 || local_268 >> 0x3c != 0;
        _local_2c8 = CONCAT71((int7)((ulonglong)puStack_2d0 >> 8),bVar56);
        auVar238._8_8_ = 0;
        auVar238._0_8_ = local_268;
        lVar54 = SUB168(auVar238 * ZEXT816(0x18),0);
        bVar77 = SUB168(auVar238 * ZEXT816(0x18),8) != 0;
        bVar57 = local_5a8 + lVar54 < local_5a8 || bVar77;
        _local_2d8 = CONCAT71((int7)((ulonglong)local_1e8 >> 8),bVar57);
        local_2e8 = local_5a8 + lVar54 + 4 < local_248 || bVar77;
        local_628 = local_268 * 0x20;
        local_2f8 = local_5a8 + local_268 * 0x20 < local_5a8 || local_268 >> 0x3b != 0;
        bVar77 = puVar62 < local_1e8 && local_148 < puStack_300;
        local_648[0] = bVar77;
        auVar97._9_7_ = 0;
        auVar97._0_9_ =
             CONCAT18(local_248 < puStack_2f0 && puStack_120 < puStack_210,
                      (ulonglong)
                      (local_248 < puStack_300 && puVar62 < puStack_2d0 ||
                      local_5a8 < puStack_300 && puVar62 < local_438)) |
             CONCAT18(local_5a8 < puStack_2f0 && puStack_120 < puStack_230,
                      (ulonglong)
                      (puStack_120 < puStack_300 && puVar62 < puStack_2f0 ||
                      local_248 < puStack_310 && puStack_140 < puStack_210));
        auVar47._4_4_ = uVar106 | uVar159 & uVar440;
        auVar47._0_4_ = uVar106 | uVar159 & uVar440;
        auVar47._8_4_ = uStack_4a0;
        auVar47._12_4_ = uStack_49c;
        auVar46._4_4_ =
             -(uint)((int)uVar557 < (int)uVar469) |
             -(uint)(uVar469 == uVar557) & -(uint)(iVar552 < (int)uVar462);
        auVar46._0_4_ =
             -(uint)((int)uVar557 < (int)uVar469) |
             -(uint)(uVar469 == uVar557) & -(uint)(iVar552 < (int)uVar462);
        auVar46._8_4_ = uStack_4b0;
        auVar46._12_4_ = uStack_4ac;
        auVar50._4_4_ =
             (-(uint)((int)uVar509 < (int)uVar469) |
             -(uint)(uVar469 == uVar509) & -(uint)(iVar499 < (int)uVar462)) &
             (uVar105 | uVar158 & uVar439);
        auVar50._0_4_ =
             (-(uint)((int)uVar509 < (int)uVar469) |
             -(uint)(uVar469 == uVar509) & -(uint)(iVar499 < (int)uVar462)) &
             (uVar105 | uVar158 & uVar439);
        auVar50._8_4_ = uVar169;
        auVar50._12_4_ = uVar177;
        auVar52._4_4_ = uStack_454;
        auVar52._0_4_ = uVar124;
        auVar52._8_4_ = uVar116;
        auVar52._12_4_ = uVar125;
        auVar48._4_4_ = (uVar160 | uVar107 & uVar130) & (uVar208 | uVar249 & uVar186);
        auVar48._0_4_ = (uVar160 | uVar107 & uVar130) & (uVar208 | uVar249 & uVar186);
        auVar48._8_4_ = uStack_490;
        auVar48._12_4_ = uStack_48c;
        local_408 = *(float *)(local_5a8 + lVar71 * 8);
        local_578 = CONCAT44((-(uint)((int)uVar513 < (int)uVar478) |
                             -(uint)(uVar478 == uVar513) & -(uint)(iVar511 < (int)uVar475)) &
                             (uVar358 | uVar542 & uVar312) |
                             (-(uint)((int)uVar497 < (int)uVar478) |
                             -(uint)(uVar478 == uVar497) & -(uint)(iVar496 < (int)uVar475)) &
                             (uVar163 | uVar357 & uVar131) |
                             (-(uint)((int)uVar531 < (int)uVar478) |
                             -(uint)(uVar478 == uVar531) & -(uint)((int)uVar529 < (int)uVar475)) &
                             (uVar109 | uVar162 & uVar178) |
                             (-(uint)((int)uVar459 < (int)uVar478) |
                             -(uint)(uVar478 == uVar459) & -(uint)(iVar458 < (int)uVar475)) &
                             (uVar108 | uVar161 & uVar115),
                             (-(uint)((int)uVar513 < (int)uVar478) |
                             -(uint)(uVar478 == uVar513) & -(uint)(iVar511 < (int)uVar475)) &
                             (uVar358 | uVar542 & uVar312) |
                             (-(uint)((int)uVar497 < (int)uVar478) |
                             -(uint)(uVar478 == uVar497) & -(uint)(iVar496 < (int)uVar475)) &
                             (uVar163 | uVar357 & uVar131) |
                             (-(uint)((int)uVar531 < (int)uVar478) |
                             -(uint)(uVar478 == uVar531) & -(uint)((int)uVar529 < (int)uVar475)) &
                             (uVar109 | uVar162 & uVar178) |
                             (-(uint)((int)uVar459 < (int)uVar478) |
                             -(uint)(uVar478 == uVar459) & -(uint)(iVar458 < (int)uVar475)) &
                             (uVar108 | uVar161 & uVar115));
        auVar42._8_4_ = uStack_570;
        auVar42._0_8_ = local_578;
        auVar42._12_4_ = uStack_56c;
        fStack_554 = *(float *)(local_5a8 + lVar71 * 8 + 4);
        auVar51._4_4_ =
             (-(uint)((int)uVar493 < (int)uVar469) |
             -(uint)(uVar469 == uVar493) & -(uint)(iVar480 < (int)uVar462)) &
             (uVar207 | uVar419 & uVar185) |
             (-(uint)((int)uVar527 < (int)uVar469) |
             -(uint)(uVar469 == uVar527) & -(uint)((int)uVar515 < (int)uVar462)) &
             (uVar206 | uVar356 & uVar184) |
             (-(uint)((int)uVar455 < (int)uVar469) |
             -(uint)(uVar469 == uVar455) & -(uint)(iVar449 < (int)uVar462)) &
             (uVar157 | uVar355 & uVar129) |
             (-(uint)((int)uVar248 < (int)uVar469) |
             -(uint)(uVar469 == uVar248) & -(uint)(iVar229 < (int)uVar462)) &
             (uVar156 | uVar205 & uVar128) |
             (uVar104 | uVar155 & uVar431) & (uVar103 | uVar154 & uVar430) |
             (-(uint)((int)uVar275 < (int)uVar469) |
             -(uint)(uVar469 == uVar275) & -(uint)(iVar262 < (int)uVar462)) &
             (-(uint)((int)uVar354 < (int)uVar153) |
             -(uint)(uVar153 == uVar354) & -(uint)(iVar311 < (int)uVar127));
        auVar51._0_4_ =
             (-(uint)((int)uVar493 < (int)uVar469) |
             -(uint)(uVar469 == uVar493) & -(uint)(iVar480 < (int)uVar462)) &
             (uVar207 | uVar419 & uVar185) |
             (-(uint)((int)uVar527 < (int)uVar469) |
             -(uint)(uVar469 == uVar527) & -(uint)((int)uVar515 < (int)uVar462)) &
             (uVar206 | uVar356 & uVar184) |
             (-(uint)((int)uVar455 < (int)uVar469) |
             -(uint)(uVar469 == uVar455) & -(uint)(iVar449 < (int)uVar462)) &
             (uVar157 | uVar355 & uVar129) |
             (-(uint)((int)uVar248 < (int)uVar469) |
             -(uint)(uVar469 == uVar248) & -(uint)(iVar229 < (int)uVar462)) &
             (uVar156 | uVar205 & uVar128) |
             (uVar104 | uVar155 & uVar431) & (uVar103 | uVar154 & uVar430) |
             (-(uint)((int)uVar275 < (int)uVar469) |
             -(uint)(uVar469 == uVar275) & -(uint)(iVar262 < (int)uVar462)) &
             (-(uint)((int)uVar354 < (int)uVar153) |
             -(uint)(uVar153 == uVar354) & -(uint)(iVar311 < (int)uVar127));
        auVar51._8_4_ =
             (-(uint)((int)uVar497 < (int)uVar477) |
             -(uint)(uVar477 == uVar497) & -(uint)(iVar496 < (int)uVar474)) &
             (uVar221 | uVar442 & uVar214) |
             (-(uint)((int)uVar531 < (int)uVar477) |
             -(uint)(uVar477 == uVar531) & -(uint)((int)uVar529 < (int)uVar474)) &
             (uVar220 | uVar373 & uVar213) |
             (-(uint)((int)uVar459 < (int)uVar477) |
             -(uint)(uVar477 == uVar459) & -(uint)(iVar458 < (int)uVar474)) &
             (uVar176 | uVar372 & uVar168) |
             (-(uint)((int)uVar258 < (int)uVar477) |
             -(uint)(uVar477 == uVar258) & -(uint)(iVar253 < (int)uVar474)) &
             (uVar175 | uVar219 & uVar167) |
             (uVar123 | uVar174 & uVar114) & (uVar122 | uVar173 & uVar113) |
             (-(uint)((int)uVar287 < (int)uVar477) |
             -(uint)(uVar477 == uVar287) & -(uint)(iVar282 < (int)uVar474)) &
             (-(uint)((int)uVar371 < (int)uVar172) |
             -(uint)(uVar172 == uVar371) & -(uint)(iVar364 < (int)uVar166));
        auVar51._12_4_ =
             (-(uint)((int)uVar497 < (int)uVar477) |
             -(uint)(uVar477 == uVar497) & -(uint)(iVar496 < (int)uVar474)) &
             (uVar221 | uVar442 & uVar214) |
             (-(uint)((int)uVar531 < (int)uVar477) |
             -(uint)(uVar477 == uVar531) & -(uint)((int)uVar529 < (int)uVar474)) &
             (uVar220 | uVar373 & uVar213) |
             (-(uint)((int)uVar459 < (int)uVar477) |
             -(uint)(uVar477 == uVar459) & -(uint)(iVar458 < (int)uVar474)) &
             (uVar176 | uVar372 & uVar168) |
             (-(uint)((int)uVar258 < (int)uVar477) |
             -(uint)(uVar477 == uVar258) & -(uint)(iVar253 < (int)uVar474)) &
             (uVar175 | uVar219 & uVar167) |
             (uVar123 | uVar174 & uVar114) & (uVar122 | uVar173 & uVar113) |
             (-(uint)((int)uVar287 < (int)uVar477) |
             -(uint)(uVar477 == uVar287) & -(uint)(iVar282 < (int)uVar474)) &
             (-(uint)((int)uVar371 < (int)uVar172) |
             -(uint)(uVar172 == uVar371) & -(uint)(iVar364 < (int)uVar166));
        fVar110 = *(float *)(local_5a8 + lVar71 * 0x10);
        auVar45._4_4_ = uVar119;
        auVar45._0_4_ = uVar126;
        auVar45._8_4_ = uVar181;
        auVar45._12_4_ = uVar374;
        local_588 = CONCAT44((uVar302 | uVar101 & uVar292) & (uVar118 | uVar307 & uVar111),
                             (uVar302 | uVar101 & uVar292) & (uVar118 | uVar307 & uVar111));
        auVar41._8_4_ = uStack_580;
        auVar41._0_8_ = local_588;
        auVar41._12_4_ = uStack_57c;
        local_558 = *(float *)(local_5a8 + local_638 + 4);
        local_568 = CONCAT44((uVar100 | uVar301 & uVar257) & (uVar179 | uVar164 & uVar222) |
                             (uVar418 | uVar99 & uVar383) & (uVar170 | uVar548 & uVar215),
                             (uVar100 | uVar301 & uVar257) & (uVar179 | uVar164 & uVar222) |
                             (uVar418 | uVar99 & uVar383) & (uVar170 | uVar548 & uVar215));
        auVar43._8_4_ = uVar254;
        auVar43._0_8_ = local_568;
        auVar43._12_4_ = uVar260;
        auVar199._1_7_ = 0;
        auVar199[0] = (int)local_664 < 0 ||
                      ((((((((SUB161(local_348 >> 0xf,0) & 1) != 0 ||
                            (SUB161(local_348 >> 0x1f,0) & 1) != 0) ||
                           (SUB161(local_348 >> 0x2f,0) & 1) != 0) ||
                          (SUB161(local_348 >> 0x3f,0) & 1) != 0) ||
                         (SUB161(local_348 >> 0x4f,0) & 1) != 0) ||
                        (SUB161(local_348 >> 0x5f,0) & 1) != 0) ||
                       (SUB161(local_348 >> 0x6f,0) & 1) != 0) || local_348[0xf] < '\0');
        auVar199[8] = bVar77;
        auVar199._9_7_ = 0;
        auVar39._4_4_ = uVar120 | uVar212 & uVar370;
        auVar39._0_4_ = uVar120 | uVar212 & uVar370;
        auVar39._8_4_ = uStack_5c0;
        auVar39._12_4_ = uStack_5bc;
        auVar40._4_4_ = uStack_5b4;
        auVar40._0_4_ = local_5b8;
        auVar40._8_4_ = uVar171;
        auVar40._12_4_ = uVar218;
        auVar38._4_4_ = uStack_604;
        auVar38._0_4_ = local_608;
        auVar38._8_4_ = uVar210;
        auVar38._12_4_ = uVar224;
        auVar44._4_4_ =
             (-(uint)((int)uVar557 < (int)uVar471) |
             -(uint)(uVar471 == uVar557) & -(uint)(iVar552 < (int)uVar464)) &
             (uVar279 | uVar494 & uVar265) |
             (-(uint)((int)uVar509 < (int)uVar471) |
             -(uint)(uVar509 == uVar471) & -(uint)(iVar499 < (int)uVar464)) &
             (uVar278 | uVar375 & uVar264) |
             (-(uint)((int)uVar493 < (int)uVar471) |
             -(uint)(uVar471 == uVar493) & -(uint)(iVar480 < (int)uVar464)) &
             (uVar277 | uVar209 & uVar263) |
             (-(uint)((int)uVar527 < (int)uVar471) |
             -(uint)(uVar527 == uVar471) & -(uint)((int)uVar515 < (int)uVar464)) &
             (uVar362 | uVar303 & uVar316) |
             (-(uint)((int)uVar455 < (int)uVar471) |
             -(uint)(uVar455 == uVar471) & -(uint)(iVar449 < (int)uVar464)) &
             (uVar424 | uVar276 & uVar385) |
             (-(uint)((int)uVar248 < (int)uVar471) |
             -(uint)(uVar471 == uVar248) & -(uint)(iVar229 < (int)uVar464)) &
             (uVar423 | uVar456 & uVar384) |
             (uVar361 | uVar422 & uVar315) & (uVar360 | uVar421 & uVar314) |
             (-(uint)((int)uVar561 < (int)uVar478) |
             -(uint)(uVar478 == uVar561) & -(uint)(iVar559 < (int)uVar475)) &
             (uVar359 | uVar420 & uVar313);
        auVar44._0_4_ =
             (-(uint)((int)uVar557 < (int)uVar471) |
             -(uint)(uVar471 == uVar557) & -(uint)(iVar552 < (int)uVar464)) &
             (uVar279 | uVar494 & uVar265) |
             (-(uint)((int)uVar509 < (int)uVar471) |
             -(uint)(uVar509 == uVar471) & -(uint)(iVar499 < (int)uVar464)) &
             (uVar278 | uVar375 & uVar264) |
             (-(uint)((int)uVar493 < (int)uVar471) |
             -(uint)(uVar471 == uVar493) & -(uint)(iVar480 < (int)uVar464)) &
             (uVar277 | uVar209 & uVar263) |
             (-(uint)((int)uVar527 < (int)uVar471) |
             -(uint)(uVar527 == uVar471) & -(uint)((int)uVar515 < (int)uVar464)) &
             (uVar362 | uVar303 & uVar316) |
             (-(uint)((int)uVar455 < (int)uVar471) |
             -(uint)(uVar455 == uVar471) & -(uint)(iVar449 < (int)uVar464)) &
             (uVar424 | uVar276 & uVar385) |
             (-(uint)((int)uVar248 < (int)uVar471) |
             -(uint)(uVar471 == uVar248) & -(uint)(iVar229 < (int)uVar464)) &
             (uVar423 | uVar456 & uVar384) |
             (uVar361 | uVar422 & uVar315) & (uVar360 | uVar421 & uVar314) |
             (-(uint)((int)uVar561 < (int)uVar478) |
             -(uint)(uVar478 == uVar561) & -(uint)(iVar559 < (int)uVar475)) &
             (uVar359 | uVar420 & uVar313);
        auVar44._8_4_ =
             (-(uint)((int)uVar561 < (int)uVar479) |
             -(uint)(uVar479 == uVar561) & -(uint)(iVar559 < (int)uVar476)) &
             (uVar291 | uVar498 & uVar285) |
             (-(uint)((int)uVar513 < (int)uVar479) |
             -(uint)(uVar513 == uVar479) & -(uint)(iVar511 < (int)uVar476)) &
             (uVar290 | uVar182 & uVar284) |
             (-(uint)((int)uVar497 < (int)uVar479) |
             -(uint)(uVar479 == uVar497) & -(uint)(iVar496 < (int)uVar476)) &
             (uVar289 | uVar223 & uVar283) |
             (-(uint)((int)uVar531 < (int)uVar479) |
             -(uint)(uVar531 == uVar479) & -(uint)((int)uVar529 < (int)uVar476)) &
             (uVar379 | uVar309 & uVar369) |
             (-(uint)((int)uVar459 < (int)uVar479) |
             -(uint)(uVar459 == uVar479) & -(uint)(iVar458 < (int)uVar476)) &
             (uVar447 | uVar288 & uVar434) |
             (-(uint)((int)uVar258 < (int)uVar479) |
             -(uint)(uVar479 == uVar258) & -(uint)(iVar253 < (int)uVar476)) &
             (uVar446 | uVar460 & uVar433) |
             (uVar378 | uVar445 & uVar368) & (uVar377 | uVar444 & uVar367) |
             (-(uint)((int)uVar557 < (int)uVar470) |
             -(uint)(uVar470 == uVar557) & -(uint)(iVar552 < (int)uVar463)) &
             (uVar376 | uVar443 & uVar366);
        auVar44._12_4_ =
             (-(uint)((int)uVar561 < (int)uVar479) |
             -(uint)(uVar479 == uVar561) & -(uint)(iVar559 < (int)uVar476)) &
             (uVar291 | uVar498 & uVar285) |
             (-(uint)((int)uVar513 < (int)uVar479) |
             -(uint)(uVar513 == uVar479) & -(uint)(iVar511 < (int)uVar476)) &
             (uVar290 | uVar182 & uVar284) |
             (-(uint)((int)uVar497 < (int)uVar479) |
             -(uint)(uVar479 == uVar497) & -(uint)(iVar496 < (int)uVar476)) &
             (uVar289 | uVar223 & uVar283) |
             (-(uint)((int)uVar531 < (int)uVar479) |
             -(uint)(uVar531 == uVar479) & -(uint)((int)uVar529 < (int)uVar476)) &
             (uVar379 | uVar309 & uVar369) |
             (-(uint)((int)uVar459 < (int)uVar479) |
             -(uint)(uVar459 == uVar479) & -(uint)(iVar458 < (int)uVar476)) &
             (uVar447 | uVar288 & uVar434) |
             (-(uint)((int)uVar258 < (int)uVar479) |
             -(uint)(uVar479 == uVar258) & -(uint)(iVar253 < (int)uVar476)) &
             (uVar446 | uVar460 & uVar433) |
             (uVar378 | uVar445 & uVar368) & (uVar377 | uVar444 & uVar367) |
             (-(uint)((int)uVar557 < (int)uVar470) |
             -(uint)(uVar470 == uVar557) & -(uint)(iVar552 < (int)uVar463)) &
             (uVar376 | uVar443 & uVar366);
        auVar49._4_4_ = (uVar417 | uVar247 & uVar382) & (uVar416 | uVar353 & uVar381);
        auVar49._0_4_ = (uVar417 | uVar247 & uVar382) & (uVar416 | uVar353 & uVar381);
        auVar49._8_4_ = uVar281;
        auVar49._12_4_ = uVar286;
        auVar395._4_4_ = _UNK_180010004;
        auVar395._0_4_ = _DAT_180010000;
        auVar395._8_4_ = _UNK_180010008;
        auVar395._12_4_ = _UNK_18001000c;
        auVar83 = auVar199 | auVar49 |
                  (auVar556 | auVar148 & auVar196) & auVar40 | auVar38 |
                  auVar39 & local_598 | local_618 & auVar40 |
                  auVar45 & local_598 | auVar41 | auVar43 | auVar44 |
                  auVar52 & auVar46 | auVar48 | auVar47 & auVar46 | auVar50 | auVar42 | auVar51 |
                  auVar97 | (auVar151 | auVar352 & auVar415) & auVar299 |
                            (auVar96 | auVar351 & auVar95) & auVar491 |
                            (auVar244 | auVar350 & auVar243) & auVar299 |
                            (auVar274 | auVar349 & auVar273) & auVar491 |
                  (auVar541 | auVar347 & auVar414) & auVar299 |
                  (auVar526 | auVar345 & auVar412) & auVar491 |
                  (auVar454 | auVar344 & auVar453) & (auVar411 | auVar343 & auVar410) |
                  (auVar298 | auVar342 & auVar409) & (auVar198 | auVar297 & auVar341) |
                  (auVar508 | auVar295 & auVar338) & auVar37 |
                  (auVar566 | auVar340 & auVar408) & (auVar197 | auVar296 & auVar339) | auVar83;
        auVar200._0_8_ = auVar83._0_8_ << 0x3f;
        auVar200._8_8_ = auVar83._8_8_ << 0x3f;
        iVar98 = movmskpd((uint)bVar77,auVar200);
        local_278 = local_5a8 + local_268 * 0x10 + 4;
        local_288 = local_5a8 + local_628 + 4;
        _local_458 = CONCAT31((int3)(uVar124 >> 8),iVar98 != 0);
        local_318 = (ulonglong)(uVar58 & 0x7ffffffc);
        local_3f8._4_4_ = fVar110;
        local_3f8._0_4_ = fVar110;
        local_3f8._8_4_ = fVar110;
        local_3f8._12_4_ = fVar110;
        uStack_550 = 0;
        uStack_54c = 0;
        auVar300._4_4_ = local_558;
        auVar300._0_4_ = fStack_554;
        auVar300._8_8_ = 0;
        local_3e8._4_4_ = local_558;
        local_3e8._0_4_ = local_558;
        local_3e8._8_4_ = local_558;
        local_3e8._12_4_ = local_558;
        local_5d8._4_4_ = fStack_554;
        local_5d8._0_4_ = fStack_554;
        local_5d8._8_4_ = fStack_554;
        local_5d8._12_4_ = fStack_554;
        local_448 = local_258 * 0x40;
        local_1f8 = local_258 * 0x60;
        uStack_1f0 = auVar467._8_8_;
        local_488 = local_258 * 0x60 + 4;
        uVar390 = local_258 * 0x20 | 4;
        local_478 = (undefined4)uVar390;
        uStack_474 = (undefined4)((ulonglong)(local_258 * 0x20) >> 0x20);
        local_208 = local_258 * 0x30;
        local_498 = local_258 * 0x30 + 4;
        local_328 = local_258 * 0x10 | 4;
        local_4a8 = local_258 * 0x48 + 4;
        local_4b8 = local_258 * 0x18 + 4;
        local_218 = (ulonglong)(uVar58 >> 2 & 0x1fffffff) << 5;
        local_228 = local_258 * 0x80;
        local_548 = local_258 * 4;
        local_418 = local_510 * 8;
        uStack_410 = auVar468._8_8_;
        local_298 = *param_2 + local_500 * 8 + 4;
        local_2a8 = *param_2 + local_508 * 8 + 4;
        local_2b8 = uVar65 * 8 + -8;
        local_518 = local_5a8 + (uVar65 - 1) * 8;
        local_4c8 = local_318 * 8;
        local_4d8 = local_448 + 4;
        uStack_250 = auVar231._8_8_;
        local_5c8 = local_258 * 3;
        uStack_460 = 0;
        uStack_45c = 0;
        pauVar59 = param_2;
        lVar54 = 0;
        puVar62 = local_148;
        puVar70 = local_158;
        puVar74 = puStack_140;
        fVar554 = _DAT_1800101f0;
        fVar564 = _UNK_1800101f4;
        local_658 = local_158;
        local_538 = uVar126;
        uStack_534 = uVar119;
        local_308 = puStack_440;
        local_1d8 = puStack_230;
        do {
          local_528 = puVar63;
          local_5e0 = puVar62;
          local_238 = lVar54;
          local_5a0 = pauVar59;
          puVar63 = *param_2 + local_238 * local_510 * 8;
          puVar62 = puVar63 + local_428 * 8;
          puVar69 = puVar63 + local_430 * 8;
          puVar60 = puVar63 + local_500 * 8;
          puVar75 = puVar63 + local_508 * 8;
          if (uVar58 < 0x1c) {
            uVar437 = 0;
LAB_180007a5b:
            lVar71 = local_548 * uVar437;
            lVar54 = uVar437 * local_5c8;
            lVar72 = 0;
            do {
              uVar304 = auVar395._0_4_;
              fVar363 = (float)*(undefined8 *)(local_5a8 + lVar71 * 2);
              fVar165 = (float)((ulonglong)*(undefined8 *)(local_5a8 + lVar71 * 2) >> 0x20);
              fVar450 = *(float *)(puVar62 + lVar72 * 8) * fVar363 +
                        fVar165 * (float)((uint)*(float *)(puVar62 + lVar72 * 8 + 4) ^ uVar304);
              fVar457 = *(float *)(puVar62 + lVar72 * 8) * fVar165 +
                        fVar363 * *(float *)(puVar62 + lVar72 * 8 + 4);
              fVar363 = (float)*(undefined8 *)(local_5a8 + lVar71 * 4);
              fVar165 = (float)((ulonglong)*(undefined8 *)(local_5a8 + lVar71 * 4) >> 0x20);
              fVar465 = *(float *)(puVar69 + lVar72 * 8) * fVar363 +
                        fVar165 * (float)((uint)*(float *)(puVar69 + lVar72 * 8 + 4) ^ uVar304);
              fVar472 = *(float *)(puVar69 + lVar72 * 8) * fVar165 +
                        fVar363 * *(float *)(puVar69 + lVar72 * 8 + 4);
              fVar363 = (float)*(undefined8 *)(local_5a8 + lVar54 * 8);
              fVar165 = (float)((ulonglong)*(undefined8 *)(local_5a8 + lVar54 * 8) >> 0x20);
              fVar481 = *(float *)(puVar60 + lVar72 * 8) * fVar363 +
                        fVar165 * (float)((uint)*(float *)(puVar60 + lVar72 * 8 + 4) ^ uVar304);
              fVar495 = *(float *)(puVar60 + lVar72 * 8) * fVar165 +
                        fVar363 * *(float *)(puVar60 + lVar72 * 8 + 4);
              fVar363 = (float)*(undefined8 *)(local_5a8 + lVar71 * 8);
              fVar165 = (float)((ulonglong)*(undefined8 *)(local_5a8 + lVar71 * 8) >> 0x20);
              fVar317 = *(float *)(puVar75 + lVar72 * 8) * fVar363 +
                        fVar165 * (float)((uint)*(float *)(puVar75 + lVar72 * 8 + 4) ^ uVar304);
              fVar363 = *(float *)(puVar75 + lVar72 * 8) * fVar165 +
                        fVar363 * *(float *)(puVar75 + lVar72 * 8 + 4);
              fVar387 = fVar450 + fVar317;
              fVar426 = fVar457 + fVar363;
              fVar266 = fVar465 + fVar481;
              fVar280 = fVar472 + fVar495;
              fVar388 = (float)*(undefined8 *)(puVar63 + lVar72 * 8);
              fVar427 = (float)((ulonglong)*(undefined8 *)(puVar63 + lVar72 * 8) >> 0x20);
              *(ulonglong *)(puVar63 + lVar72 * 8) =
                   CONCAT44(fVar280 + fVar426 + fVar427,fVar266 + fVar387 + fVar388);
              fVar450 = fVar450 - fVar317;
              fVar457 = fVar457 - fVar363;
              fVar465 = fVar465 - fVar481;
              fVar472 = fVar472 - fVar495;
              fVar363 = fVar387 * local_408 + fVar110 * fVar266 + fVar388;
              fVar165 = fVar426 * local_408 + fVar110 * fVar280 + fVar427;
              fVar317 = fVar450 * fStack_554 + local_558 * fVar465;
              fVar481 = fVar457 * fStack_554 + local_558 * fVar472;
              *(ulonglong *)(puVar62 + lVar72 * 8) = CONCAT44(fVar317 + fVar165,fVar363 - fVar481);
              *(ulonglong *)(puVar75 + lVar72 * 8) = CONCAT44(fVar165 - fVar317,fVar481 + fVar363);
              fVar388 = fVar387 * fVar110 + fVar266 * local_408 + fVar388;
              fVar427 = fVar426 * fVar110 + fVar280 * local_408 + fVar427;
              fVar363 = fVar472 * auVar300._0_4_ - fVar457 * local_558;
              fVar165 = fVar450 * auVar300._4_4_ - fVar465 * fStack_554;
              *(ulonglong *)(puVar69 + lVar72 * 8) = CONCAT44(fVar165 + fVar427,fVar363 + fVar388);
              *(ulonglong *)(puVar60 + lVar72 * 8) = CONCAT44(fVar427 - fVar165,fVar388 - fVar363);
              lVar71 = lVar71 + local_548;
              lVar72 = lVar72 + 1;
              lVar54 = lVar54 + local_5c8;
            } while (uVar65 - uVar437 != lVar72);
          }
          else {
            local_608 = (uint)puVar74;
            uStack_604 = (uint)((ulonglong)puVar74 >> 0x20);
            local_658 = puVar70;
            if (((local_288 < local_248) ||
                (((((local_278 < local_248 || (local_438 < local_248 || bVar56)) || bVar57) ||
                  (bool)local_2e8) || (bool)local_2f8) ||
                 ((local_518 < local_5a8 ||
                  local_298 + local_418 * local_238 + local_2b8 < local_298 + local_418 * local_238)
                 || ((local_268 >> 0x3d != 0 || iVar53 != 1) ||
                    local_2a8 + local_418 * local_238 + local_2b8 <
                    local_2a8 + local_418 * local_238)))) || (iVar98 != 0)) {
              uVar437 = 0;
              goto LAB_180007a5b;
            }
            puVar63 = puVar63 + local_318 * 8;
            puVar62 = puVar62 + local_318 * 8;
            puVar69 = puVar69 + local_318 * 8;
            puVar60 = puVar60 + local_318 * 8;
            puVar75 = puVar75 + local_318 * 8;
            lVar73 = 0;
            local_648._0_8_ = local_488;
            local_618._0_8_ = local_4a8;
            lVar72 = 0;
            uVar437 = local_328;
            lVar54 = local_498;
            uVar66 = uVar390;
            lVar71 = 2;
            uVar76 = uVar390;
            local_578 = local_4d8;
            local_588 = local_498;
            local_568 = local_4b8;
            do {
              local_4f0 = lVar71;
              pfVar1 = (float *)(*local_5a0 + lVar72);
              fVar554 = *pfVar1;
              fVar564 = pfVar1[1];
              fVar363 = pfVar1[2];
              fVar165 = pfVar1[3];
              pfVar1 = (float *)(local_5a0[1] + lVar72);
              fStack_1e0 = *pfVar1;
              fStack_1d0 = pfVar1[1];
              fStack_1dc = pfVar1[2];
              fStack_1cc = pfVar1[3];
              pfVar1 = (float *)(local_528 + lVar72);
              pfVar6 = (float *)(local_528 + lVar72 + 0x10);
              pfVar2 = (float *)(local_5a8 + lVar72);
              pfVar7 = (float *)(local_5a8 + lVar72 + 0x10);
              fVar466 = *pfVar1 * *pfVar2 - pfVar2[1] * pfVar1[1];
              fVar473 = pfVar1[2] * pfVar2[2] - pfVar2[3] * pfVar1[3];
              fStack_620 = *pfVar6 * *pfVar7 - pfVar7[1] * pfVar6[1];
              fStack_61c = pfVar6[2] * pfVar7[2] - pfVar7[3] * pfVar6[3];
              fVar500 = pfVar2[1] * *pfVar1 + *pfVar2 * pfVar1[1];
              fVar510 = pfVar2[3] * pfVar1[2] + pfVar2[2] * pfVar1[3];
              fVar512 = pfVar7[1] * *pfVar6 + *pfVar7 * pfVar6[1];
              fVar514 = pfVar7[3] * pfVar6[2] + pfVar7[2] * pfVar6[3];
              pfVar1 = (float *)(puVar70 + lVar72);
              pfVar2 = (float *)(puVar70 + lVar72 + 0x10);
              local_598._0_8_ = lVar54;
              local_538 = (uint)uVar66;
              uStack_534 = (uint)(uVar66 >> 0x20);
              local_5b8 = (uint)uVar437;
              uStack_5b4 = (uint)(uVar437 >> 0x20);
              fVar266 = *pfVar1 * *(float *)(local_5a8 + lVar73) -
                        *(float *)(local_5a8 + lVar73 + 4) * pfVar1[1];
              fVar280 = pfVar1[2] * *(float *)(local_5a8 + (uVar437 - 4)) -
                        *(float *)(local_5a8 + uVar437) * pfVar1[3];
              fStack_630 = *pfVar2 * *(float *)(local_5a8 + (uVar66 - 4)) -
                           *(float *)(local_5a8 + uVar66) * pfVar2[1];
              fStack_62c = pfVar2[2] * *(float *)(local_5a8 + lVar54 + -4) -
                           *(float *)(local_5a8 + lVar54) * pfVar2[3];
              fVar516 = *(float *)(local_5a8 + lVar73 + 4) * *pfVar1 +
                        *(float *)(local_5a8 + lVar73) * pfVar1[1];
              fVar528 = *(float *)(local_5a8 + uVar437) * pfVar1[2] +
                        *(float *)(local_5a8 + (uVar437 - 4)) * pfVar1[3];
              fVar530 = *(float *)(local_5a8 + uVar66) * *pfVar2 +
                        *(float *)(local_5a8 + (uVar66 - 4)) * pfVar2[1];
              fVar532 = *(float *)(local_5a8 + lVar54) * pfVar2[2] +
                        *(float *)(local_5a8 + lVar54 + -4) * pfVar2[3];
              pfVar1 = (float *)(puVar74 + lVar72);
              pfVar2 = (float *)(puVar74 + lVar72 + 0x10);
              fVar387 = *pfVar1 * *(float *)(local_5a8 + local_4f0 * 2 + -4) -
                        *(float *)(local_5a8 + local_4f0 * 2) * pfVar1[1];
              fVar426 = pfVar1[2] * *(float *)(local_5a8 + local_568 + -4) -
                        *(float *)(local_5a8 + local_568) * pfVar1[3];
              fVar450 = *pfVar2 * *(float *)(local_5a8 + local_588 + -4) -
                        *(float *)(local_5a8 + local_588) * pfVar2[1];
              fVar457 = pfVar2[2] * *(float *)(local_5a8 + local_618._0_8_ + -4) -
                        *(float *)(local_5a8 + local_618._0_8_) * pfVar2[3];
              fVar465 = *(float *)(local_5a8 + local_4f0 * 2) * *pfVar1 +
                        *(float *)(local_5a8 + local_4f0 * 2 + -4) * pfVar1[1];
              fVar472 = *(float *)(local_5a8 + local_568) * pfVar1[2] +
                        *(float *)(local_5a8 + local_568 + -4) * pfVar1[3];
              fVar495 = *(float *)(local_5a8 + local_588) * *pfVar2 +
                        *(float *)(local_5a8 + local_588 + -4) * pfVar2[1];
              fVar380 = *(float *)(local_5a8 + local_618._0_8_) * pfVar2[2] +
                        *(float *)(local_5a8 + local_618._0_8_ + -4) * pfVar2[3];
              pfVar1 = (float *)(local_5e0 + lVar72);
              pfVar2 = (float *)(local_5e0 + lVar72 + 0x10);
              fVar533 = *pfVar1 * *(float *)(local_5a8 + lVar73 * 2) -
                        *(float *)(local_5a8 + lVar73 * 2 + 4) * pfVar1[1];
              fVar544 = pfVar1[2] * *(float *)(local_5a8 + (uVar76 - 4)) -
                        *(float *)(local_5a8 + uVar76) * pfVar1[3];
              fVar546 = *pfVar2 * *(float *)(local_5a8 + local_578 + -4) -
                        *(float *)(local_5a8 + local_578) * pfVar2[1];
              fVar550 = pfVar2[2] * *(float *)(local_5a8 + local_648._0_8_ + -4) -
                        *(float *)(local_5a8 + local_648._0_8_) * pfVar2[3];
              fVar388 = *(float *)(local_5a8 + lVar73 * 2 + 4) * *pfVar1 +
                        *(float *)(local_5a8 + lVar73 * 2) * pfVar1[1];
              fVar427 = *(float *)(local_5a8 + uVar76) * pfVar1[2] +
                        *(float *)(local_5a8 + (uVar76 - 4)) * pfVar1[3];
              fVar317 = *(float *)(local_5a8 + local_578) * *pfVar2 +
                        *(float *)(local_5a8 + local_578 + -4) * pfVar2[1];
              fVar481 = *(float *)(local_5a8 + local_648._0_8_) * pfVar2[2] +
                        *(float *)(local_5a8 + local_648._0_8_ + -4) * pfVar2[3];
              fVar553 = fVar466 + fVar533;
              fVar558 = fVar473 + fVar544;
              fVar560 = fStack_620 + fVar546;
              fVar562 = fStack_61c + fVar550;
              fVar563 = fVar500 + fVar388;
              fVar567 = fVar510 + fVar427;
              fVar568 = fVar512 + fVar317;
              fVar569 = fVar514 + fVar481;
              fVar466 = fVar466 - fVar533;
              fVar473 = fVar473 - fVar544;
              local_628 = CONCAT44(fVar473,fVar466);
              fStack_620 = fStack_620 - fVar546;
              fStack_61c = fStack_61c - fVar550;
              fVar500 = fVar500 - fVar388;
              fVar510 = fVar510 - fVar427;
              fVar512 = fVar512 - fVar317;
              fVar514 = fVar514 - fVar481;
              fVar534 = fVar266 + fVar387;
              fVar545 = fVar280 + fVar426;
              fVar547 = fStack_630 + fVar450;
              fVar551 = fStack_62c + fVar457;
              fVar389 = fVar516 + fVar465;
              fVar428 = fVar528 + fVar472;
              fVar436 = fVar530 + fVar495;
              fVar448 = fVar532 + fVar380;
              fVar266 = fVar266 - fVar387;
              fVar280 = fVar280 - fVar426;
              local_638 = CONCAT44(fVar280,fVar266);
              fStack_630 = fStack_630 - fVar450;
              fStack_62c = fStack_62c - fVar457;
              fVar516 = fVar516 - fVar465;
              fVar528 = fVar528 - fVar472;
              fVar530 = fVar530 - fVar495;
              fVar532 = fVar532 - fVar380;
              local_1e8 = (undefined1 *)CONCAT44(fVar363,fVar554);
              local_1d8 = (undefined1 *)CONCAT44(fVar165,fVar564);
              pfVar1 = (float *)(local_5a0[1] + lVar72);
              *pfVar1 = fVar547 + fVar560 + fStack_1e0;
              pfVar1[1] = fVar436 + fVar568 + fStack_1d0;
              pfVar1[2] = fVar551 + fVar562 + fStack_1dc;
              pfVar1[3] = fVar448 + fVar569 + fStack_1cc;
              puVar61 = (undefined8 *)(*local_5a0 + lVar72);
              *puVar61 = CONCAT44(fVar389 + fVar563 + fVar564,fVar534 + fVar553 + fVar554);
              *(float *)(puVar61 + 1) = fVar545 + fVar558 + fVar363;
              *(float *)((longlong)puVar61 + 0xc) = fVar428 + fVar567 + fVar165;
              fVar465 = fVar553 * local_408 + fVar110 * fVar534 + fVar554;
              fVar472 = fVar558 * local_408 + fVar110 * fVar545 + fVar363;
              fVar495 = fVar560 * local_408 + fVar110 * fVar547 + fStack_1e0;
              fVar380 = fVar562 * local_408 + fVar110 * fVar551 + fStack_1dc;
              fVar533 = fVar563 * local_408 + fVar110 * fVar389 + fVar564;
              fVar544 = fVar567 * local_408 + fVar110 * fVar428 + fVar165;
              fVar546 = fVar568 * local_408 + fVar110 * fVar436 + fStack_1d0;
              fVar550 = fVar569 * local_408 + fVar110 * fVar448 + fStack_1cc;
              fVar388 = fVar500 * fStack_554 + local_558 * fVar516;
              fVar427 = fVar510 * fStack_554 + local_558 * fVar528;
              fVar317 = fVar512 * fStack_554 + local_558 * fVar530;
              fVar481 = fVar514 * fStack_554 + local_558 * fVar532;
              fVar387 = fVar466 * fStack_554 + local_558 * fVar266;
              fVar426 = fVar473 * fStack_554 + local_558 * fVar280;
              fVar450 = fStack_620 * fStack_554 + local_558 * fStack_630;
              fVar457 = fStack_61c * fStack_554 + local_558 * fStack_62c;
              pfVar1 = (float *)(local_528 + lVar72 + 0x10);
              *pfVar1 = fVar495 - fVar317;
              pfVar1[1] = fVar546 + fVar450;
              pfVar1[2] = fVar380 - fVar481;
              pfVar1[3] = fVar550 + fVar457;
              puVar61 = (undefined8 *)(local_528 + lVar72);
              *puVar61 = CONCAT44(fVar533 + fVar387,fVar465 - fVar388);
              *(float *)(puVar61 + 1) = fVar472 - fVar427;
              *(float *)((longlong)puVar61 + 0xc) = fVar544 + fVar426;
              pfVar1 = (float *)(local_5e0 + lVar72 + 0x10);
              *pfVar1 = fVar317 + fVar495;
              pfVar1[1] = fVar546 - fVar450;
              pfVar1[2] = fVar481 + fVar380;
              pfVar1[3] = fVar550 - fVar457;
              puVar61 = (undefined8 *)(local_5e0 + lVar72);
              *puVar61 = CONCAT44(fVar533 - fVar387,fVar388 + fVar465);
              *(float *)(puVar61 + 1) = fVar427 + fVar472;
              *(float *)((longlong)puVar61 + 0xc) = fVar544 - fVar426;
              fVar554 = fVar553 * fVar110 + fVar534 * local_408 + fVar554;
              fVar363 = fVar558 * fVar110 + fVar545 * local_408 + fVar363;
              fVar465 = fVar560 * fVar110 + fVar547 * local_408 + fStack_1e0;
              fVar472 = fVar562 * fVar110 + fVar551 * local_408 + fStack_1dc;
              fVar564 = fVar563 * fVar110 + fVar389 * local_408 + fVar564;
              fVar165 = fVar567 * fVar110 + fVar428 * local_408 + fVar165;
              fVar495 = fVar568 * fVar110 + fVar436 * local_408 + fStack_1d0;
              fVar380 = fVar569 * fVar110 + fVar448 * local_408 + fStack_1cc;
              fVar387 = fVar516 * fStack_554 - fVar500 * local_558;
              fVar426 = fVar528 * fStack_554 - fVar510 * local_558;
              fVar450 = fVar530 * fStack_554 - fVar512 * local_558;
              fVar457 = fVar532 * fStack_554 - fVar514 * local_558;
              fVar317 = fVar466 * local_558 - fVar266 * fStack_554;
              fVar481 = fVar473 * local_558 - fVar280 * fStack_554;
              fVar266 = fStack_620 * local_558 - fStack_630 * fStack_554;
              fVar280 = fStack_61c * local_558 - fStack_62c * fStack_554;
              fVar388 = fVar387 + fVar554;
              fVar427 = fVar426 + fVar363;
              auVar202._4_4_ = fVar427;
              auVar202._0_4_ = fVar388;
              auVar202._8_4_ = fVar427;
              auVar202._12_4_ = fVar165 + fVar481;
              auVar201._8_8_ = auVar202._8_8_;
              auVar201._4_4_ = fVar564 + fVar317;
              auVar201._0_4_ = fVar388;
              pfVar1 = (float *)(puVar70 + lVar72 + 0x10);
              *pfVar1 = fVar450 + fVar465;
              pfVar1[1] = fVar495 + fVar266;
              pfVar1[2] = fVar457 + fVar472;
              pfVar1[3] = fVar380 + fVar280;
              *(undefined1 (*) [16])(puVar70 + lVar72) = auVar201;
              pfVar1 = (float *)(puVar74 + lVar72 + 0x10);
              *pfVar1 = fVar465 - fVar450;
              pfVar1[1] = fVar495 - fVar266;
              pfVar1[2] = fVar472 - fVar457;
              pfVar1[3] = fVar380 - fVar280;
              puVar61 = (undefined8 *)(puVar74 + lVar72);
              *puVar61 = CONCAT44(fVar564 - fVar317,fVar554 - fVar387);
              *(float *)(puVar61 + 1) = fVar363 - fVar426;
              *(float *)((longlong)puVar61 + 0xc) = fVar165 - fVar481;
              lVar72 = lVar72 + 0x20;
              lVar54 = lVar54 + local_448;
              local_618._0_8_ = local_618._0_8_ + local_1f8;
              local_648._0_8_ = local_648._0_8_ + local_228;
              uVar66 = uVar66 + local_448;
              local_588 = local_588 + local_1f8;
              local_578 = local_578 + local_228;
              uVar437 = uVar437 + local_448;
              local_568 = local_568 + local_1f8;
              uVar76 = uVar76 + local_228;
              lVar73 = lVar73 + local_448;
              lVar71 = local_4f0 + local_208;
            } while (local_218 != lVar72);
            auVar395._4_4_ = _UNK_180010004;
            auVar395._0_4_ = _DAT_180010000;
            auVar395._8_4_ = _UNK_180010008;
            auVar395._12_4_ = _UNK_18001000c;
            auVar300._8_8_ = 0;
            uVar437 = local_318;
            fVar554 = _DAT_1800101f0;
            fVar564 = _UNK_1800101f4;
            local_308 = puVar63;
            if ((uVar58 & 0x7ffffffc) != uVar58) goto LAB_180007a5b;
          }
          puVar74 = puVar74 + local_510 * 8;
          puVar70 = puVar70 + local_510 * 8;
          pauVar59 = (undefined1 (*) [16])(*local_5a0 + local_418);
          lVar54 = local_238 + 1;
          puVar62 = local_5e0 + local_510 * 8;
          puVar63 = local_528 + local_510 * 8;
          uStack_600 = uVar210;
          uStack_5fc = uVar224;
          uStack_5b0 = uVar171;
          uStack_5ac = uVar218;
          uStack_560 = uVar254;
          uStack_55c = uVar260;
          uStack_530 = uVar181;
          uStack_52c = uVar374;
          local_4f8 = uVar65;
          local_4e8 = uVar292;
          uStack_4e4 = uVar302;
          uStack_4dc = uVar308;
          uStack_480 = uVar281;
          uStack_47c = uVar286;
          uStack_470 = uVar169;
          uStack_46c = uVar177;
          local_468 = fStack_554;
          fStack_464 = local_558;
          uStack_450 = uVar116;
          uStack_44c = uVar125;
          local_420 = uVar65;
          fStack_404 = local_408;
          fStack_400 = local_408;
          fStack_3fc = local_408;
          puStack_320 = puStack_520;
          puStack_2e0 = local_438;
          puStack_2c0 = puStack_310;
          puStack_2b0 = puStack_2f0;
          puStack_2a0 = local_438;
          puStack_290 = puStack_540;
          puStack_280 = puStack_2d0;
          puStack_270 = puStack_520;
          puStack_260 = puStack_300;
          puStack_220 = puStack_4d0;
          puStack_200 = local_438;
          local_1a0 = lVar68;
          local_128 = puStack_140;
          uStack_360 = local_248;
        } while (local_238 + 1 != lVar68);
      }
    }
    if (local_5e8 < 1) {
      if ((local_f0 ^ (ulonglong)auStack_688) != DAT_180582000) {
        local_660 = param_1;
        local_5f0 = param_2;
        local_188 = local_1c0;
        local_168 = param_2;
                    /* WARNING: Subroutine does not return */
        FUN_18000e470();
      }
      return;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 180008690
   NAME : rnn_fft_c
   SIG  : undefined rnn_fft_c(void)
   ======================================================================== */

void rnn_fft_c(float param_1,longlong param_2,longlong param_3)

{
  float fVar1;
  longlong lVar2;
  int *in_RCX;
  
                    /* 0x8690  17  rnn_fft_c */
  if (0 < *in_RCX) {
    param_1 = (float)in_RCX[1];
    lVar2 = 0;
    do {
      fVar1 = *(float *)(param_2 + 4 + lVar2 * 8);
      *(float *)(param_3 + (longlong)*(int *)(*(longlong *)(in_RCX + 0xc) + lVar2 * 4) * 8) =
           *(float *)(param_2 + lVar2 * 8) * param_1;
      *(float *)(param_3 + 4 + (longlong)*(int *)(*(longlong *)(in_RCX + 0xc) + lVar2 * 4) * 8) =
           fVar1 * param_1;
      lVar2 = lVar2 + 1;
    } while (lVar2 < *in_RCX);
  }
  rnn_fft_impl(param_1,param_3);
  return;
}



/* ========================================================================
   ENTRY: 1800086f0
   NAME : rnn_ifft_c
   SIG  : undefined rnn_ifft_c(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_ifft_c(int *param_1,longlong param_2,longlong param_3)

{
  uint uVar1;
  longlong lVar2;
  
                    /* 0x86f0  22  rnn_ifft_c */
  if (0 < *param_1) {
    lVar2 = 0;
    do {
      *(undefined8 *)(param_3 + (longlong)*(int *)(*(longlong *)(param_1 + 0xc) + lVar2 * 4) * 8) =
           *(undefined8 *)(param_2 + lVar2 * 8);
      uVar1 = _DAT_180010000;
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
  uVar1 = _DAT_180010000;
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
   ENTRY: 1800087b0
   NAME : rnn_compute_generic_dense
   SIG  : undefined rnn_compute_generic_dense(void)
   ======================================================================== */

void rnn_compute_generic_dense
               (longlong param_1,undefined8 param_2,undefined8 param_3,undefined4 param_4)

{
                    /* 0x87b0  9  rnn_compute_generic_dense */
  rnn_compute_linear_c();
  rnn_compute_activation_c(param_2,param_2,*(undefined4 *)(param_1 + 0x3c),param_4);
  return;
}



/* ========================================================================
   ENTRY: 1800087e0
   NAME : rnn_compute_generic_gru
   SIG  : undefined rnn_compute_generic_gru(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_compute_generic_gru(undefined8 param_1,longlong param_2,void *param_3,undefined8 param_4)

{
  longlong lVar1;
  float *pfVar2;
  float *pfVar3;
  float *pfVar4;
  float *pfVar5;
  uint uVar6;
  uint uVar7;
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
  ulonglong uVar29;
  uint uVar30;
  ulonglong uVar31;
  ulonglong uVar32;
  float *_Src;
  ulonglong uVar33;
  longlong lVar34;
  float *pfVar35;
  longlong lVar36;
  undefined1 auStack_6088 [32];
  float local_6068 [3072];
  float local_3068 [3074];
  ulonglong local_60;
  
                    /* 0x87e0  10  rnn_compute_generic_gru */
  local_60 = DAT_180582000 ^ (ulonglong)auStack_6088;
  uVar6 = *(uint *)(param_2 + 0x38);
  uVar33 = (ulonglong)(int)uVar6;
  uVar7 = uVar6 * 2;
  rnn_compute_linear_c(param_1,local_3068,param_4);
  rnn_compute_linear_c(param_2,local_6068,param_3);
  lVar36 = (longlong)(int)uVar7;
  _Src = local_3068 + lVar36;
  if ((longlong)uVar33 < 1) {
    rnn_compute_activation_c(local_3068,local_3068,uVar7,1);
  }
  else {
    uVar29 = 1;
    if (1 < (int)uVar7) {
      uVar29 = (ulonglong)uVar7;
    }
    if ((int)uVar7 < 8) {
      uVar31 = 0;
LAB_1800088e0:
      do {
        local_3068[uVar31] = local_6068[uVar31] + local_3068[uVar31];
        uVar31 = uVar31 + 1;
      } while (uVar29 != uVar31);
    }
    else {
      uVar30 = (uint)uVar29 & 0x7ffffff8;
      uVar31 = (ulonglong)uVar30;
      uVar32 = 0;
      do {
        fVar8 = local_6068[uVar32 + 1];
        fVar9 = local_6068[uVar32 + 2];
        fVar10 = local_6068[uVar32 + 3];
        fVar11 = local_6068[uVar32 + 4];
        fVar12 = local_6068[uVar32 + 5];
        fVar13 = local_6068[uVar32 + 6];
        fVar14 = local_6068[uVar32 + 7];
        local_3068[uVar32] = local_6068[uVar32] + local_3068[uVar32];
        local_3068[uVar32 + 1] = fVar8 + local_3068[uVar32 + 1];
        local_3068[uVar32 + 2] = fVar9 + local_3068[uVar32 + 2];
        local_3068[uVar32 + 3] = fVar10 + local_3068[uVar32 + 3];
        local_3068[uVar32 + 4] = fVar11 + local_3068[uVar32 + 4];
        local_3068[uVar32 + 5] = fVar12 + local_3068[uVar32 + 5];
        local_3068[uVar32 + 6] = fVar13 + local_3068[uVar32 + 6];
        local_3068[uVar32 + 7] = fVar14 + local_3068[uVar32 + 7];
        uVar32 = uVar32 + 8;
      } while (uVar31 != uVar32);
      if (uVar30 != (uint)uVar29) goto LAB_1800088e0;
    }
    rnn_compute_activation_c(local_3068,local_3068,uVar7,1);
    if (0 < (int)uVar6) {
      pfVar35 = local_3068 + uVar33;
      uVar29 = (ulonglong)uVar7;
      if ((uVar6 < 8) ||
         ((_Src < local_3068 + uVar33 * 2 && (pfVar35 < local_3068 + lVar36 + uVar33)))) {
        uVar31 = 0;
LAB_180008968:
        uVar32 = uVar31;
        if ((uVar6 & 1) != 0) {
          _Src[uVar31] = local_6068[uVar31 + uVar29] * pfVar35[uVar31] + _Src[uVar31];
          uVar32 = uVar31 | 1;
        }
        if (uVar31 != uVar33 - 1) {
          do {
            local_3068[lVar36 + uVar32] =
                 local_6068[uVar29 + uVar32] * local_3068[uVar33 + uVar32] +
                 local_3068[lVar36 + uVar32];
            local_3068[lVar36 + uVar32 + 1] =
                 local_6068[uVar29 + uVar32 + 1] * local_3068[uVar33 + uVar32 + 1] +
                 local_3068[lVar36 + uVar32 + 1];
            uVar32 = uVar32 + 2;
          } while (uVar33 != uVar32);
        }
      }
      else {
        uVar31 = (ulonglong)(uVar6 & 0x7ffffff8);
        lVar1 = lVar36 * 4;
        lVar34 = 0;
        do {
          pfVar2 = (float *)((longlong)local_6068 + lVar34 + uVar29 * 4);
          fVar8 = pfVar2[1];
          fVar9 = pfVar2[2];
          fVar10 = pfVar2[3];
          pfVar5 = (float *)((longlong)local_6068 + lVar34 + uVar29 * 4 + 0x10);
          fVar11 = *pfVar5;
          fVar12 = pfVar5[1];
          fVar13 = pfVar5[2];
          fVar14 = pfVar5[3];
          pfVar3 = (float *)((longlong)local_3068 + lVar34 + uVar33 * 4);
          fVar15 = pfVar3[1];
          fVar16 = pfVar3[2];
          fVar17 = pfVar3[3];
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + uVar33 * 4 + 0x10);
          fVar18 = *pfVar5;
          fVar19 = pfVar5[1];
          fVar20 = pfVar5[2];
          fVar21 = pfVar5[3];
          pfVar4 = (float *)((longlong)local_3068 + lVar34 + lVar1);
          fVar22 = pfVar4[1];
          fVar23 = pfVar4[2];
          fVar24 = pfVar4[3];
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + lVar1 + 0x10);
          fVar25 = *pfVar5;
          fVar26 = pfVar5[1];
          fVar27 = pfVar5[2];
          fVar28 = pfVar5[3];
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + lVar1);
          *pfVar5 = *pfVar4 + *pfVar3 * *pfVar2;
          pfVar5[1] = fVar22 + fVar15 * fVar8;
          pfVar5[2] = fVar23 + fVar16 * fVar9;
          pfVar5[3] = fVar24 + fVar17 * fVar10;
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + lVar1 + 0x10);
          *pfVar5 = fVar25 + fVar18 * fVar11;
          pfVar5[1] = fVar26 + fVar19 * fVar12;
          pfVar5[2] = fVar27 + fVar20 * fVar13;
          pfVar5[3] = fVar28 + fVar21 * fVar14;
          lVar34 = lVar34 + 0x20;
        } while ((ulonglong)(uVar6 >> 3 & 0xfffffff) << 5 != lVar34);
        if (uVar31 != uVar33) goto LAB_180008968;
      }
      rnn_compute_activation_c(_Src,_Src,uVar6,2);
      if ((int)uVar6 < 1) goto LAB_180008c49;
      if ((uVar6 < 8) || ((_Src < pfVar35 && (local_3068 < local_3068 + lVar36 + uVar33)))) {
        uVar29 = 0;
LAB_180008ae2:
        uVar31 = uVar29;
        if ((uVar6 & 1) != 0) {
          _Src[uVar29] = local_3068[uVar29] * *(float *)((longlong)param_3 + uVar29 * 4) +
                         (DAT_1800100a8 - local_3068[uVar29]) * _Src[uVar29];
          uVar31 = uVar29 | 1;
        }
        if (uVar29 != uVar33 - 1) {
          do {
            local_3068[lVar36 + uVar31] =
                 local_3068[uVar31] * *(float *)((longlong)param_3 + uVar31 * 4) +
                 (DAT_1800100a8 - local_3068[uVar31]) * local_3068[lVar36 + uVar31];
            local_3068[lVar36 + uVar31 + 1] =
                 local_3068[uVar31 + 1] * *(float *)((longlong)param_3 + uVar31 * 4 + 4) +
                 (DAT_1800100a8 - local_3068[uVar31 + 1]) * local_3068[lVar36 + uVar31 + 1];
            uVar31 = uVar31 + 2;
          } while (uVar33 != uVar31);
        }
      }
      else {
        uVar29 = (ulonglong)(uVar6 & 0x7ffffff8);
        lVar1 = lVar36 * 4;
        lVar34 = 0;
        do {
          fVar8 = *(float *)((longlong)local_3068 + lVar34 + 4);
          fVar9 = *(float *)((longlong)local_3068 + lVar34 + 8);
          fVar10 = *(float *)((longlong)local_3068 + lVar34 + 0xc);
          fVar11 = *(float *)((longlong)local_3068 + lVar34 + 0x10);
          fVar12 = *(float *)((longlong)local_3068 + lVar34 + 0x14);
          fVar13 = *(float *)((longlong)local_3068 + lVar34 + 0x18);
          fVar14 = *(float *)((longlong)local_3068 + lVar34 + 0x1c);
          pfVar35 = (float *)((longlong)param_3 + lVar34);
          fVar15 = pfVar35[1];
          fVar16 = pfVar35[2];
          fVar17 = pfVar35[3];
          pfVar5 = (float *)((longlong)param_3 + lVar34 + 0x10);
          fVar18 = *pfVar5;
          fVar19 = pfVar5[1];
          fVar20 = pfVar5[2];
          fVar21 = pfVar5[3];
          pfVar2 = (float *)((longlong)local_3068 + lVar34 + lVar1);
          fVar22 = pfVar2[1];
          fVar23 = pfVar2[2];
          fVar24 = pfVar2[3];
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + lVar1 + 0x10);
          fVar25 = *pfVar5;
          fVar26 = pfVar5[1];
          fVar27 = pfVar5[2];
          fVar28 = pfVar5[3];
          pfVar5 = (float *)((longlong)local_3068 + lVar34 + lVar1);
          *pfVar5 = *pfVar35 * *(float *)((longlong)local_3068 + lVar34) +
                    *pfVar2 * (DAT_180010180 - *(float *)((longlong)local_3068 + lVar34));
          pfVar5[1] = fVar15 * fVar8 + fVar22 * (_UNK_180010184 - fVar8);
          pfVar5[2] = fVar16 * fVar9 + fVar23 * (_UNK_180010188 - fVar9);
          pfVar5[3] = fVar17 * fVar10 + fVar24 * (_UNK_18001018c - fVar10);
          pfVar35 = (float *)((longlong)local_3068 + lVar34 + lVar1 + 0x10);
          *pfVar35 = fVar18 * fVar11 + fVar25 * (DAT_180010180 - fVar11);
          pfVar35[1] = fVar19 * fVar12 + fVar26 * (_UNK_180010184 - fVar12);
          pfVar35[2] = fVar20 * fVar13 + fVar27 * (_UNK_180010188 - fVar13);
          pfVar35[3] = fVar21 * fVar14 + fVar28 * (_UNK_18001018c - fVar14);
          lVar34 = lVar34 + 0x20;
        } while ((ulonglong)(uVar6 >> 3 & 0xfffffff) << 5 != lVar34);
        if (uVar29 != uVar33) goto LAB_180008ae2;
      }
      if (0 < (int)uVar6) {
        memcpy(param_3,_Src,uVar33 << 2);
      }
      goto LAB_180008c49;
    }
  }
  rnn_compute_activation_c(_Src,_Src,uVar6,2);
LAB_180008c49:
  if ((local_60 ^ (ulonglong)auStack_6088) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180008c90
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
  
                    /* 0x8c90  11  rnn_compute_glu */
  local_28 = DAT_180582000 ^ (ulonglong)auStack_2048;
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
  if ((local_28 ^ (ulonglong)auStack_2048) != DAT_180582000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000e470();
  }
  return;
}



/* ========================================================================
   ENTRY: 180008d70
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
  
                    /* 0x8d70  8  rnn_compute_generic_conv1d */
  local_40 = DAT_180582000 ^ (ulonglong)local_1068;
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
  if ((local_40 ^ (ulonglong)local_1068) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180008e70
   NAME : rnn_compute_activation_c
   SIG  : undefined rnn_compute_activation_c(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_compute_activation_c
               (longlong param_1,longlong param_2,ulonglong param_3,undefined4 param_4)

{
  uint *puVar1;
  float *pfVar2;
  float *pfVar3;
  undefined4 *puVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  float fVar8;
  float fVar9;
  undefined4 uVar10;
  undefined4 uVar11;
  undefined4 uVar12;
  undefined4 uVar13;
  undefined8 uVar14;
  undefined1 auVar15 [16];
  undefined1 auVar16 [16];
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
  uint uVar27;
  uint uVar28;
  uint uVar29;
  uint uVar30;
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
  float fVar61;
  float fVar62;
  uint uVar63;
  ulonglong uVar64;
  ulonglong uVar65;
  longlong lVar66;
  ulonglong uVar67;
  float fVar68;
  float fVar73;
  float fVar74;
  undefined1 auVar69 [16];
  undefined1 auVar70 [16];
  undefined1 auVar71 [16];
  undefined1 auVar72 [16];
  float fVar75;
  float fVar76;
  float fVar77;
  float fVar80;
  float fVar81;
  float fVar82;
  undefined1 auVar78 [16];
  undefined1 auVar79 [16];
  float fVar83;
  float fVar85;
  float fVar86;
  undefined1 auVar84 [16];
  float fVar87;
  float fVar88;
  float fVar89;
  float fVar94;
  float fVar96;
  undefined1 auVar90 [16];
  undefined1 auVar91 [16];
  undefined1 auVar92 [16];
  float fVar95;
  float fVar97;
  float fVar98;
  float fVar99;
  undefined1 auVar93 [16];
  float fVar100;
  float fVar101;
  float fVar102;
  float fVar103;
  undefined1 auStack_40a8 [32];
  float afStack_4088 [4098];
  ulonglong local_80;
  undefined8 uStack_8;
  
  fVar62 = _UNK_1800103ec;
  fVar61 = _UNK_1800103e8;
  fVar60 = _UNK_1800103e4;
  fVar59 = _DAT_1800103e0;
  fVar58 = _UNK_1800103dc;
  fVar57 = _UNK_1800103d8;
  fVar56 = _UNK_1800103d4;
  fVar55 = _DAT_1800103d0;
  fVar54 = _UNK_1800103cc;
  fVar53 = _UNK_1800103c8;
  fVar52 = _UNK_1800103c4;
  fVar51 = _DAT_1800103c0;
  fVar50 = _UNK_1800103bc;
  fVar49 = _UNK_1800103b8;
  fVar48 = _UNK_1800103b4;
  fVar47 = _DAT_1800103b0;
  fVar46 = _UNK_1800103ac;
  fVar45 = _UNK_1800103a8;
  fVar44 = _UNK_1800103a4;
  fVar43 = _DAT_1800103a0;
  fVar42 = _UNK_18001037c;
  fVar41 = _UNK_180010378;
  fVar40 = _UNK_180010374;
  fVar39 = DAT_180010370;
  fVar38 = _UNK_18001036c;
  fVar37 = _UNK_180010368;
  fVar36 = _UNK_180010364;
  fVar35 = _DAT_180010360;
  fVar34 = _UNK_18001035c;
  fVar33 = _UNK_180010358;
  fVar32 = _UNK_180010354;
  fVar31 = _DAT_180010350;
  fVar103 = _UNK_18001034c;
  fVar102 = _UNK_180010348;
  fVar101 = _UNK_180010344;
  fVar100 = _DAT_180010340;
  fVar99 = _UNK_18001033c;
  fVar98 = _UNK_180010338;
  fVar97 = _UNK_180010334;
  fVar96 = _DAT_180010330;
  fVar95 = _UNK_18001032c;
  fVar94 = _UNK_180010328;
  fVar89 = _UNK_180010324;
  fVar88 = _DAT_180010320;
  fVar87 = _UNK_18001031c;
  fVar86 = _UNK_180010318;
  fVar85 = _UNK_180010314;
  fVar83 = _DAT_180010310;
  uVar30 = _UNK_1800102bc;
  uVar29 = _UNK_1800102b8;
  uVar28 = _UNK_1800102b4;
  uVar27 = _DAT_1800102b0;
  fVar26 = _UNK_1800102ac;
  fVar25 = _UNK_1800102a8;
  fVar24 = _UNK_1800102a4;
  fVar23 = _DAT_1800102a0;
  fVar22 = _UNK_18001029c;
  fVar21 = _UNK_180010298;
  fVar20 = _UNK_180010294;
  fVar19 = _DAT_180010290;
  fVar18 = _UNK_18001028c;
  fVar17 = _UNK_180010288;
  fVar9 = _UNK_180010284;
  fVar8 = _DAT_180010280;
  fVar7 = _UNK_18001027c;
  fVar6 = _UNK_180010278;
  fVar77 = _UNK_180010274;
  fVar5 = _DAT_180010270;
  fVar82 = _UNK_18001026c;
  fVar81 = _UNK_180010268;
  fVar80 = _UNK_180010264;
  fVar75 = _DAT_180010260;
  auVar70 = _DAT_180010250;
  auVar71 = _DAT_180010240;
  fVar74 = _UNK_18001023c;
  fVar73 = _UNK_180010238;
  fVar76 = _UNK_180010234;
                    /* 0x8e70  5  rnn_compute_activation_c */
  fVar68 = _DAT_180010230;
  uStack_8 = 0x180008e7a;
  local_80 = DAT_180582000 ^ (ulonglong)auStack_40a8;
  uVar63 = (uint)param_3;
  switch(param_4) {
  case 1:
    uVar64 = 0;
    if (3 < (int)uVar63) {
      uVar64 = 0;
      auVar15._4_12_ = _UNK_180010184;
      auVar15._0_4_ = DAT_180010180;
      do {
        auVar71 = *(undefined1 (*) [16])(param_2 + uVar64 * 4);
        fVar68 = auVar71._0_4_;
        fVar76 = auVar71._12_4_;
        fVar73 = auVar71._4_4_;
        fVar74 = auVar71._8_4_;
        fVar75 = fVar68 * fVar68;
        fVar80 = fVar73 * fVar73;
        fVar81 = fVar74 * fVar74;
        fVar82 = fVar76 * fVar76;
        auVar90._0_4_ = (fVar75 * fVar100 + fVar31) * fVar75 + fVar35;
        auVar90._4_4_ = (fVar80 * fVar101 + fVar32) * fVar80 + fVar36;
        auVar90._8_4_ = (fVar81 * fVar102 + fVar33) * fVar81 + fVar37;
        auVar90._12_4_ = (fVar82 * fVar103 + fVar34) * fVar82 + fVar38;
        auVar71 = rcpps(auVar71,auVar90);
        auVar69._0_4_ =
             auVar71._0_4_ * ((fVar75 * fVar83 + fVar88) * fVar75 + fVar96) * fVar68 + fVar39;
        auVar69._4_4_ =
             auVar71._4_4_ * ((fVar80 * fVar85 + fVar89) * fVar80 + fVar97) * fVar73 + fVar40;
        auVar69._8_4_ =
             auVar71._8_4_ * ((fVar81 * fVar86 + fVar94) * fVar81 + fVar98) * fVar74 + fVar41;
        auVar69._12_4_ =
             auVar71._12_4_ * ((fVar82 * fVar87 + fVar95) * fVar82 + fVar99) * fVar76 + fVar42;
        auVar71 = minps(auVar15,auVar69);
        auVar71 = maxps(ZEXT816(0),auVar71);
        *(undefined1 (*) [16])(param_1 + uVar64 * 4) = auVar71;
        uVar64 = uVar64 + 4;
      } while (uVar64 < uVar63 - 3);
    }
    fVar82 = DAT_180010394;
    fVar81 = DAT_180010390;
    fVar80 = DAT_18001038c;
    fVar75 = DAT_180010388;
    fVar74 = DAT_180010384;
    fVar73 = DAT_180010380;
    fVar76 = DAT_180010370;
    fVar68 = DAT_180010180;
    if ((int)uVar64 < (int)uVar63) {
      uVar64 = uVar64 & 0xffffffff;
      do {
        fVar5 = *(float *)(param_2 + uVar64 * 4);
        fVar77 = fVar5 * fVar5;
        auVar71 = rcpss(ZEXT816(0),ZEXT416((uint)((fVar77 * fVar80 + fVar81) * fVar77 + fVar82)));
        fVar77 = auVar71._0_4_ * ((fVar77 * fVar73 + fVar74) * fVar77 + fVar75) * fVar5 + fVar76;
        fVar5 = fVar68;
        if (fVar77 <= fVar68) {
          fVar5 = fVar77;
        }
        fVar77 = 0.0;
        if (0.0 <= fVar5) {
          fVar77 = fVar5;
        }
        *(float *)(param_1 + uVar64 * 4) = fVar77;
        uVar64 = uVar64 + 1;
      } while ((param_3 & 0xffffffff) != uVar64);
    }
    break;
  case 2:
    uVar64 = 0;
    if (3 < (int)uVar63) {
      uVar64 = 0;
      auVar70._4_12_ = _UNK_180010184;
      auVar70._0_4_ = DAT_180010180;
      auVar16._4_12_ = _UNK_1800103f4;
      auVar16._0_4_ = DAT_1800103f0;
      do {
        auVar71 = *(undefined1 (*) [16])(param_2 + uVar64 * 4);
        fVar68 = auVar71._0_4_;
        fVar76 = auVar71._12_4_;
        fVar73 = auVar71._4_4_;
        fVar74 = auVar71._8_4_;
        fVar75 = fVar68 * fVar68;
        fVar80 = fVar73 * fVar73;
        fVar81 = fVar74 * fVar74;
        fVar82 = fVar76 * fVar76;
        auVar93._0_4_ = (fVar75 * fVar55 + fVar59) * fVar75 + fVar35;
        auVar93._4_4_ = (fVar80 * fVar56 + fVar60) * fVar80 + fVar36;
        auVar93._8_4_ = (fVar81 * fVar57 + fVar61) * fVar81 + fVar37;
        auVar93._12_4_ = (fVar82 * fVar58 + fVar62) * fVar82 + fVar38;
        auVar71 = rcpps(auVar71,auVar93);
        auVar72._0_4_ = auVar71._0_4_ * ((fVar75 * fVar43 + fVar47) * fVar75 + fVar51) * fVar68;
        auVar72._4_4_ = auVar71._4_4_ * ((fVar80 * fVar44 + fVar48) * fVar80 + fVar52) * fVar73;
        auVar72._8_4_ = auVar71._8_4_ * ((fVar81 * fVar45 + fVar49) * fVar81 + fVar53) * fVar74;
        auVar72._12_4_ = auVar71._12_4_ * ((fVar82 * fVar46 + fVar50) * fVar82 + fVar54) * fVar76;
        auVar71 = minps(auVar70,auVar72);
        auVar71 = maxps(auVar16,auVar71);
        *(undefined1 (*) [16])(param_1 + uVar64 * 4) = auVar71;
        uVar64 = uVar64 + 4;
      } while (uVar64 < uVar63 - 3);
    }
    fVar82 = DAT_180010410;
    fVar81 = DAT_18001040c;
    fVar80 = DAT_180010408;
    fVar75 = DAT_180010404;
    fVar74 = DAT_180010400;
    fVar73 = DAT_1800103f0;
    fVar76 = DAT_180010394;
    fVar68 = DAT_180010180;
    if ((int)uVar64 < (int)uVar63) {
      uVar64 = uVar64 & 0xffffffff;
      do {
        fVar5 = *(float *)(param_2 + uVar64 * 4);
        fVar77 = fVar5 * fVar5;
        auVar71 = rcpss(ZEXT816(0),ZEXT416((uint)((fVar77 * fVar81 + fVar82) * fVar77 + fVar76)));
        fVar77 = auVar71._0_4_ * ((fVar77 * fVar74 + fVar75) * fVar77 + fVar80) * fVar5;
        fVar5 = fVar68;
        if (fVar77 <= fVar68) {
          fVar5 = fVar77;
        }
        fVar77 = fVar73;
        if (fVar73 <= fVar5) {
          fVar77 = fVar5;
        }
        *(float *)(param_1 + uVar64 * 4) = fVar77;
        uVar64 = uVar64 + 1;
      } while ((param_3 & 0xffffffff) != uVar64);
    }
    break;
  case 3:
    if (0 < (int)uVar63) {
      if (uVar63 < 8 || (ulonglong)(param_1 - param_2) < 0x20) {
        uVar64 = 0;
      }
      else {
        uVar64 = (ulonglong)(uVar63 & 0x7ffffff8);
        lVar66 = 0;
        do {
          auVar70 = maxps(ZEXT816(0),*(undefined1 (*) [16])(param_2 + lVar66));
          auVar71 = maxps(ZEXT816(0),*(undefined1 (*) [16])(param_2 + 0x10 + lVar66));
          *(undefined1 (*) [16])(param_1 + lVar66) = auVar70;
          *(undefined1 (*) [16])(param_1 + 0x10 + lVar66) = auVar71;
          lVar66 = lVar66 + 0x20;
        } while ((ulonglong)((uint)(param_3 >> 3) & 0xfffffff) << 5 != lVar66);
        if ((uVar63 & 0x7ffffff8) == uVar63) break;
      }
      uVar67 = uVar64;
      if ((param_3 & 1) != 0) {
        fVar68 = *(float *)(param_2 + uVar64 * 4);
        fVar76 = 0.0;
        if (0.0 <= fVar68) {
          fVar76 = fVar68;
        }
        *(float *)(param_1 + uVar64 * 4) = fVar76;
        uVar67 = uVar64 | 1;
      }
      if (uVar64 != (param_3 & 0xffffffff) - 1) {
        do {
          fVar68 = *(float *)(param_2 + uVar67 * 4);
          fVar76 = 0.0;
          if (0.0 <= fVar68) {
            fVar76 = fVar68;
          }
          *(float *)(param_1 + uVar67 * 4) = fVar76;
          fVar68 = *(float *)(param_2 + 4 + uVar67 * 4);
          fVar76 = 0.0;
          if (0.0 <= fVar68) {
            fVar76 = fVar68;
          }
          *(float *)(param_1 + 4 + uVar67 * 4) = fVar76;
          uVar67 = uVar67 + 2;
        } while ((param_3 & 0xffffffff) != uVar67);
      }
    }
    break;
  case 4:
    uVar64 = 0;
    if (7 < (int)uVar63) {
      uVar64 = 0;
      do {
        pfVar2 = (float *)(param_2 + uVar64 * 4);
        pfVar3 = (float *)(param_2 + 0x10 + uVar64 * 4);
        auVar78._0_4_ = *pfVar3 * fVar68;
        auVar78._4_4_ = pfVar3[1] * fVar76;
        auVar78._8_4_ = pfVar3[2] * fVar73;
        auVar78._12_4_ = pfVar3[3] * fVar74;
        auVar92 = minps(auVar71,auVar78);
        auVar92 = maxps(auVar70,auVar92);
        fVar88 = (float)(int)(auVar92._0_4_ + fVar75);
        fVar94 = (float)(int)(auVar92._4_4_ + fVar80);
        fVar96 = (float)(int)(auVar92._8_4_ + fVar81);
        fVar98 = (float)(int)(auVar92._12_4_ + fVar82);
        fVar100 = auVar92._0_4_ - fVar88;
        fVar101 = auVar92._4_4_ - fVar94;
        fVar102 = auVar92._8_4_ - fVar96;
        fVar103 = auVar92._12_4_ - fVar98;
        auVar84._0_4_ = *pfVar2 * fVar68;
        auVar84._4_4_ = pfVar2[1] * fVar76;
        auVar84._8_4_ = pfVar2[2] * fVar73;
        auVar84._12_4_ = pfVar2[3] * fVar74;
        auVar92 = minps(auVar71,auVar84);
        auVar92 = maxps(auVar70,auVar92);
        fVar89 = (float)(int)(auVar92._0_4_ + fVar75);
        fVar95 = (float)(int)(auVar92._4_4_ + fVar80);
        fVar97 = (float)(int)(auVar92._8_4_ + fVar81);
        fVar99 = (float)(int)(auVar92._12_4_ + fVar82);
        fVar83 = auVar92._0_4_ - fVar89;
        fVar85 = auVar92._4_4_ - fVar95;
        fVar86 = auVar92._8_4_ - fVar97;
        fVar87 = auVar92._12_4_ - fVar99;
        puVar1 = (uint *)(param_1 + uVar64 * 4);
        *puVar1 = (int)fVar89 * 0x800000 +
                  (int)(((fVar83 * fVar5 + fVar8) * fVar83 + fVar19) * fVar83 + fVar23) & uVar27;
        puVar1[1] = (int)fVar95 * 0x800000 +
                    (int)(((fVar85 * fVar77 + fVar9) * fVar85 + fVar20) * fVar85 + fVar24) & uVar28;
        puVar1[2] = (int)fVar97 * 0x800000 +
                    (int)(((fVar86 * fVar6 + fVar17) * fVar86 + fVar21) * fVar86 + fVar25) & uVar29;
        puVar1[3] = (int)fVar99 * 0x800000 +
                    (int)(((fVar87 * fVar7 + fVar18) * fVar87 + fVar22) * fVar87 + fVar26) & uVar30;
        puVar1 = (uint *)(param_1 + 0x10 + uVar64 * 4);
        *puVar1 = (int)fVar88 * 0x800000 +
                  (int)(((fVar100 * fVar5 + fVar8) * fVar100 + fVar19) * fVar100 + fVar23) & uVar27;
        puVar1[1] = (int)fVar94 * 0x800000 +
                    (int)(((fVar101 * fVar77 + fVar9) * fVar101 + fVar20) * fVar101 + fVar24) &
                    uVar28;
        puVar1[2] = (int)fVar96 * 0x800000 +
                    (int)(((fVar102 * fVar6 + fVar17) * fVar102 + fVar21) * fVar102 + fVar25) &
                    uVar29;
        puVar1[3] = (int)fVar98 * 0x800000 +
                    (int)(((fVar103 * fVar7 + fVar18) * fVar103 + fVar22) * fVar103 + fVar26) &
                    uVar30;
        uVar64 = uVar64 + 8;
      } while (uVar64 < uVar63 - 7);
    }
    fVar74 = _DAT_1800102f0;
    uVar11 = s___2___2__1800102e0._0_4_;
    uVar10 = s___g>__g>_1800102d0._0_4_;
    fVar73 = DAT_1800102c0;
    uVar27 = _DAT_1800102b0;
    fVar76 = _DAT_180010270;
    fVar68 = _DAT_180010260;
    auVar70 = _DAT_180010250;
    auVar71 = _DAT_180010240;
    if ((int)uVar64 < (int)uVar63) {
      uVar64 = uVar64 & 0xffffffff;
      do {
        auVar79._0_4_ = *(float *)(param_2 + uVar64 * 4) * fVar73;
        auVar79._4_4_ = auVar79._0_4_;
        auVar79._8_4_ = auVar79._0_4_;
        auVar79._12_4_ = auVar79._0_4_;
        auVar92 = minps(auVar71,auVar79);
        auVar92 = maxps(auVar70,auVar92);
        fVar80 = (float)(int)(auVar92._0_4_ + fVar68);
        fVar75 = auVar92._0_4_ - fVar80;
        *(uint *)(param_1 + uVar64 * 4) =
             (int)fVar80 * 0x800000 +
             (int)(((fVar75 * fVar76 + (float)uVar10) * fVar75 + (float)uVar11) * fVar75 + fVar74) &
             uVar27;
        uVar64 = uVar64 + 1;
      } while ((param_3 & 0xffffffff) != uVar64);
    }
    if (0 < (int)uVar63) {
      if (uVar63 < 8) {
        fVar68 = 0.0;
        uVar64 = 0;
      }
      else {
        fVar68 = 0.0;
        uVar64 = 0;
        do {
          fVar68 = fVar68 + *(float *)(param_1 + uVar64 * 4) + *(float *)(param_1 + 4 + uVar64 * 4)
                   + *(float *)(param_1 + 8 + uVar64 * 4) + *(float *)(param_1 + 0xc + uVar64 * 4) +
                   *(float *)(param_1 + 0x10 + uVar64 * 4) + *(float *)(param_1 + 0x14 + uVar64 * 4)
                   + *(float *)(param_1 + 0x18 + uVar64 * 4) +
                   *(float *)(param_1 + 0x1c + uVar64 * 4);
          uVar64 = uVar64 + 8;
        } while ((uVar63 & 0x7ffffff8) != uVar64);
      }
      if ((ulonglong)(uVar63 & 7) != 0) {
        uVar67 = 0;
        do {
          fVar68 = fVar68 + *(float *)(param_1 + uVar64 * 4 + uVar67 * 4);
          uVar67 = uVar67 + 1;
        } while ((uVar63 & 7) != uVar67);
      }
      if (0 < (int)uVar63) {
        fVar68 = (float)(DAT_180010308 / ((double)fVar68 + _DAT_180010300));
        if (uVar63 < 8) {
          uVar64 = 0;
        }
        else {
          uVar64 = (ulonglong)(uVar63 & 0x7ffffff8);
          lVar66 = 0;
          do {
            pfVar2 = (float *)(param_1 + lVar66);
            fVar76 = pfVar2[1];
            fVar73 = pfVar2[2];
            fVar74 = pfVar2[3];
            pfVar3 = (float *)(param_1 + 0x10 + lVar66);
            fVar75 = *pfVar3;
            fVar80 = pfVar3[1];
            fVar81 = pfVar3[2];
            fVar82 = pfVar3[3];
            pfVar3 = (float *)(param_1 + lVar66);
            *pfVar3 = *pfVar2 * fVar68;
            pfVar3[1] = fVar76 * fVar68;
            pfVar3[2] = fVar73 * fVar68;
            pfVar3[3] = fVar74 * fVar68;
            pfVar2 = (float *)(param_1 + 0x10 + lVar66);
            *pfVar2 = fVar75 * fVar68;
            pfVar2[1] = fVar80 * fVar68;
            pfVar2[2] = fVar81 * fVar68;
            pfVar2[3] = fVar82 * fVar68;
            lVar66 = lVar66 + 0x20;
          } while ((ulonglong)((uint)(param_3 >> 3) & 0xfffffff) << 5 != lVar66);
          if ((uVar63 & 0x7ffffff8) == uVar63) break;
        }
        do {
          *(float *)(param_1 + uVar64 * 4) = *(float *)(param_1 + uVar64 * 4) * fVar68;
          uVar64 = uVar64 + 1;
        } while ((param_3 & 0xffffffff) != uVar64);
      }
    }
    break;
  case 5:
    uVar64 = 0;
    if (3 < (int)uVar63) {
      uVar64 = 0;
      auVar71._4_12_ = _UNK_180010184;
      auVar71._0_4_ = DAT_180010180;
      do {
        auVar70 = *(undefined1 (*) [16])(param_2 + uVar64 * 4);
        fVar68 = auVar70._0_4_;
        fVar76 = auVar70._12_4_;
        fVar73 = auVar70._4_4_;
        fVar74 = auVar70._8_4_;
        fVar75 = fVar68 * fVar68;
        fVar80 = fVar73 * fVar73;
        fVar81 = fVar74 * fVar74;
        fVar82 = fVar76 * fVar76;
        auVar91._0_4_ = (fVar75 * _DAT_180010340 + _DAT_180010350) * fVar75 + _DAT_180010360;
        auVar91._4_4_ = (fVar80 * _UNK_180010344 + _UNK_180010354) * fVar80 + _UNK_180010364;
        auVar91._8_4_ = (fVar81 * _UNK_180010348 + _UNK_180010358) * fVar81 + _UNK_180010368;
        auVar91._12_4_ = (fVar82 * _UNK_18001034c + _UNK_18001035c) * fVar82 + _UNK_18001036c;
        auVar70 = rcpps(auVar70,auVar91);
        auVar92._0_4_ =
             auVar70._0_4_ *
             ((fVar75 * _DAT_180010310 + _DAT_180010320) * fVar75 + _DAT_180010330) * fVar68 +
             DAT_180010370;
        auVar92._4_4_ =
             auVar70._4_4_ *
             ((fVar80 * _UNK_180010314 + _UNK_180010324) * fVar80 + _UNK_180010334) * fVar73 +
             _UNK_180010374;
        auVar92._8_4_ =
             auVar70._8_4_ *
             ((fVar81 * _UNK_180010318 + _UNK_180010328) * fVar81 + _UNK_180010338) * fVar74 +
             _UNK_180010378;
        auVar92._12_4_ =
             auVar70._12_4_ *
             ((fVar82 * _UNK_18001031c + _UNK_18001032c) * fVar82 + _UNK_18001033c) * fVar76 +
             _UNK_18001037c;
        auVar70 = minps(auVar71,auVar92);
        auVar70 = maxps(ZEXT816(0),auVar70);
        *(undefined1 (*) [16])(afStack_4088 + uVar64) = auVar70;
        uVar64 = uVar64 + 4;
      } while (uVar64 < uVar63 - 3);
    }
    uVar67 = param_3 & 0xffffffff;
    if ((int)uVar64 < (int)uVar63) {
      uVar64 = uVar64 & 0xffffffff;
      do {
        fVar68 = *(float *)(param_2 + uVar64 * 4);
        fVar76 = fVar68 * fVar68;
        auVar71 = rcpss(ZEXT816(0),
                        ZEXT416((uint)((fVar76 * DAT_18001038c + DAT_180010390) * fVar76 +
                                      DAT_180010394)));
        fVar76 = auVar71._0_4_ *
                 ((fVar76 * DAT_180010380 + DAT_180010384) * fVar76 + DAT_180010388) * fVar68 +
                 DAT_180010370;
        fVar68 = DAT_180010180;
        if (fVar76 <= DAT_180010180) {
          fVar68 = fVar76;
        }
        fVar76 = 0.0;
        if (0.0 <= fVar68) {
          fVar76 = fVar68;
        }
        afStack_4088[uVar64] = fVar76;
        uVar64 = uVar64 + 1;
      } while (uVar67 != uVar64);
    }
    if (0 < (int)uVar63) {
      if ((ulonglong)(param_1 - param_2) < 0x20 || uVar63 < 8) {
        uVar64 = 0;
      }
      else {
        uVar64 = (ulonglong)(uVar63 & 0x7ffffff8);
        lVar66 = 0;
        do {
          pfVar2 = (float *)(param_2 + lVar66);
          fVar68 = pfVar2[1];
          fVar76 = pfVar2[2];
          fVar73 = pfVar2[3];
          pfVar3 = (float *)(param_2 + 0x10 + lVar66);
          fVar74 = *pfVar3;
          fVar75 = pfVar3[1];
          fVar80 = pfVar3[2];
          fVar81 = pfVar3[3];
          fVar82 = *(float *)((longlong)afStack_4088 + lVar66 + 4);
          fVar5 = *(float *)((longlong)afStack_4088 + lVar66 + 8);
          fVar77 = *(float *)((longlong)afStack_4088 + lVar66 + 0xc);
          fVar6 = *(float *)((longlong)afStack_4088 + lVar66 + 0x10);
          fVar7 = *(float *)((longlong)afStack_4088 + lVar66 + 0x14);
          fVar8 = *(float *)((longlong)afStack_4088 + lVar66 + 0x18);
          fVar9 = *(float *)((longlong)afStack_4088 + lVar66 + 0x1c);
          pfVar3 = (float *)(param_1 + lVar66);
          *pfVar3 = *pfVar2 * *(float *)((longlong)afStack_4088 + lVar66);
          pfVar3[1] = fVar68 * fVar82;
          pfVar3[2] = fVar76 * fVar5;
          pfVar3[3] = fVar73 * fVar77;
          pfVar2 = (float *)(param_1 + 0x10 + lVar66);
          *pfVar2 = fVar74 * fVar6;
          pfVar2[1] = fVar75 * fVar7;
          pfVar2[2] = fVar80 * fVar8;
          pfVar2[3] = fVar81 * fVar9;
          lVar66 = lVar66 + 0x20;
        } while ((ulonglong)((uint)(param_3 >> 3) & 0xfffffff) << 5 != lVar66);
        if ((uVar63 & 0x7ffffff8) == uVar63) break;
      }
      uVar65 = uVar64;
      for (param_3 = param_3 & 3; param_3 != 0; param_3 = param_3 - 1) {
        *(float *)(param_1 + uVar65 * 4) = *(float *)(param_2 + uVar65 * 4) * afStack_4088[uVar65];
        uVar65 = uVar65 + 1;
      }
      if (uVar64 - uVar67 < 0xfffffffffffffffd) {
        do {
          *(float *)(param_1 + uVar65 * 4) = *(float *)(param_2 + uVar65 * 4) * afStack_4088[uVar65]
          ;
          *(float *)(param_1 + 4 + uVar65 * 4) =
               *(float *)(param_2 + 4 + uVar65 * 4) * afStack_4088[uVar65 + 1];
          *(float *)(param_1 + 8 + uVar65 * 4) =
               *(float *)(param_2 + 8 + uVar65 * 4) * afStack_4088[uVar65 + 2];
          *(float *)(param_1 + 0xc + uVar65 * 4) =
               *(float *)(param_2 + 0xc + uVar65 * 4) * afStack_4088[uVar65 + 3];
          uVar65 = uVar65 + 4;
        } while (uVar67 != uVar65);
      }
    }
    break;
  default:
    if (0 < (int)uVar63 && param_2 != param_1) {
      if ((ulonglong)(param_1 - param_2) < 0x20 || uVar63 < 8) {
        uVar64 = 0;
      }
      else {
        uVar64 = (ulonglong)(uVar63 & 0x7ffffff8);
        lVar66 = 0;
        do {
          uVar14 = ((undefined8 *)(param_2 + lVar66))[1];
          puVar4 = (undefined4 *)(param_2 + 0x10 + lVar66);
          uVar10 = *puVar4;
          uVar11 = puVar4[1];
          uVar12 = puVar4[2];
          uVar13 = puVar4[3];
          *(undefined8 *)(param_1 + lVar66) = *(undefined8 *)(param_2 + lVar66);
          ((undefined8 *)(param_1 + lVar66))[1] = uVar14;
          puVar4 = (undefined4 *)(param_1 + 0x10 + lVar66);
          *puVar4 = uVar10;
          puVar4[1] = uVar11;
          puVar4[2] = uVar12;
          puVar4[3] = uVar13;
          lVar66 = lVar66 + 0x20;
        } while ((ulonglong)((uint)(param_3 >> 3) & 0xfffffff) << 5 != lVar66);
        if ((uVar63 & 0x7ffffff8) == uVar63) break;
      }
      uVar65 = uVar64;
      for (uVar67 = param_3 & 3; uVar67 != 0; uVar67 = uVar67 - 1) {
        *(undefined4 *)(param_1 + uVar65 * 4) = *(undefined4 *)(param_2 + uVar65 * 4);
        uVar65 = uVar65 + 1;
      }
      if (uVar64 - (param_3 & 0xffffffff) < 0xfffffffffffffffd) {
        do {
          *(undefined4 *)(param_1 + uVar65 * 4) = *(undefined4 *)(param_2 + uVar65 * 4);
          *(undefined4 *)(param_1 + 4 + uVar65 * 4) = *(undefined4 *)(param_2 + 4 + uVar65 * 4);
          *(undefined4 *)(param_1 + 8 + uVar65 * 4) = *(undefined4 *)(param_2 + 8 + uVar65 * 4);
          *(undefined4 *)(param_1 + 0xc + uVar65 * 4) = *(undefined4 *)(param_2 + 0xc + uVar65 * 4);
          uVar65 = uVar65 + 4;
        } while ((param_3 & 0xffffffff) != uVar65);
      }
    }
  }
  if ((local_80 ^ (ulonglong)auStack_40a8) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 180009a60
   NAME : rnn_compute_linear_c
   SIG  : undefined rnn_compute_linear_c(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_compute_linear_c(undefined8 *param_1,void *param_2,longlong param_3)

{
  float *pfVar1;
  uint *puVar2;
  float *pfVar3;
  float fVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  short sVar8;
  short sVar9;
  short sVar10;
  char cVar11;
  float fVar15;
  double dVar16;
  char cVar17;
  char cVar18;
  char cVar19;
  char cVar20;
  int *piVar21;
  uint uVar22;
  ulonglong uVar23;
  float *pfVar24;
  longlong lVar25;
  void *_Dst;
  ulonglong uVar26;
  int iVar27;
  ulonglong uVar28;
  ulonglong uVar29;
  float *pfVar30;
  undefined1 (*pauVar31) [16];
  int iVar32;
  longlong lVar33;
  float *pfVar34;
  size_t _Size;
  uint *puVar35;
  ulonglong uVar36;
  void *pvVar37;
  uint uVar38;
  ulonglong uVar39;
  ulonglong uVar40;
  short sVar41;
  float fVar42;
  float fVar50;
  double dVar45;
  double dVar46;
  double dVar47;
  double dVar48;
  double dVar49;
  float fVar51;
  float fVar54;
  ulonglong uVar52;
  ulonglong uVar53;
  float fVar55;
  float fVar59;
  int iVar60;
  float fVar61;
  int iVar62;
  float fVar63;
  undefined1 auVar56 [16];
  undefined1 auVar57 [16];
  undefined1 auVar58 [16];
  int iVar64;
  int iVar65;
  int iVar66;
  int iVar67;
  int iVar68;
  float fVar69;
  float fVar71;
  float fVar72;
  float fVar73;
  undefined1 auVar70 [16];
  undefined1 auVar74 [16];
  undefined1 auVar75 [16];
  undefined1 auVar76 [16];
  undefined1 auVar77 [16];
  ushort uVar78;
  ushort uVar85;
  undefined1 auVar79 [16];
  undefined1 auVar80 [16];
  undefined1 auVar81 [16];
  undefined1 auVar82 [16];
  undefined1 auVar83 [16];
  undefined1 auVar84 [16];
  undefined1 auVar86 [16];
  undefined1 auVar87 [16];
  undefined1 auVar88 [16];
  undefined1 auVar89 [16];
  undefined1 auVar90 [16];
  undefined1 auVar91 [16];
  undefined1 auVar92 [16];
  undefined1 auVar93 [16];
  undefined1 auVar94 [16];
  undefined1 auVar95 [16];
  undefined1 auVar96 [16];
  undefined1 auStack_908 [40];
  ulonglong local_8e0;
  undefined8 *local_8d8;
  ulonglong local_8d0;
  char acStack_8c8 [4];
  undefined4 auStack_8c4 [513];
  ulonglong local_c0;
  char cVar12;
  char cVar13;
  char cVar14;
  undefined4 uVar43;
  undefined6 uVar44;
  
  cVar20 = UNK_180010453;
  cVar19 = UNK_180010452;
  cVar18 = UNK_180010451;
  cVar17 = DAT_180010450;
  uVar28 = _UNK_180010448;
  uVar23 = _DAT_180010440;
  dVar49 = _UNK_180010438;
  dVar16 = _DAT_180010430;
  fVar54 = _UNK_18001042c;
  fVar51 = _UNK_180010428;
  fVar50 = _UNK_180010424;
                    /* 0x9a60  12  rnn_compute_linear_c */
  fVar42 = _DAT_180010420;
  local_c0 = DAT_180582000 ^ (ulonglong)auStack_908;
  pvVar37 = (void *)*param_1;
  pfVar30 = (float *)param_1[3];
  uVar5 = *(uint *)(param_1 + 7);
  uVar29 = (ulonglong)uVar5;
  uVar6 = *(uint *)((longlong)param_1 + 0x3c);
  local_8e0 = (ulonglong)uVar6;
  uVar40 = (ulonglong)(int)uVar6;
  if (pfVar30 == (float *)0x0) {
    pauVar31 = (undefined1 (*) [16])param_1[2];
    if (pauVar31 != (undefined1 (*) [16])0x0) {
      puVar35 = (uint *)param_1[4];
      uVar26 = param_1[6];
      if (puVar35 == (uint *)0x0) {
        local_8d8 = param_1;
        if (0 < (int)uVar5) {
          if (uVar5 < 4) {
            uVar39 = 0;
            dVar16 = DAT_180010468;
            fVar42 = DAT_180010460;
          }
          else {
            uVar39 = (ulonglong)(uVar5 & 0x7ffffffc);
            uVar36 = 0;
            do {
              pfVar30 = (float *)(param_3 + uVar36 * 4);
              fVar55 = *pfVar30;
              fVar59 = pfVar30[1];
              fVar61 = pfVar30[3];
              dVar45 = floor((double)(pfVar30[2] * fVar51) + dVar16);
              dVar46 = floor((double)(fVar61 * fVar54) + dVar49);
              dVar47 = floor((double)(fVar55 * fVar42) + dVar16);
              dVar48 = floor((double)(fVar59 * fVar50) + dVar49);
              uVar52 = CONCAT44((int)dVar48,(int)dVar47) & uVar23;
              uVar53 = CONCAT44((int)dVar46,(int)dVar45) & uVar28;
              sVar8 = (short)uVar52;
              cVar11 = (0 < sVar8) * (sVar8 < 0x100) * (char)uVar52 - (0xff < sVar8);
              sVar8 = (short)(uVar52 >> 0x10);
              sVar41 = CONCAT11((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar52 >> 0x10) -
                                (0xff < sVar8),cVar11);
              sVar8 = (short)(uVar52 >> 0x20);
              cVar12 = (0 < sVar8) * (sVar8 < 0x100) * (char)(uVar52 >> 0x20) - (0xff < sVar8);
              sVar8 = (short)(uVar52 >> 0x30);
              uVar43 = CONCAT13((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar52 >> 0x30) -
                                (0xff < sVar8),CONCAT12(cVar12,sVar41));
              sVar8 = (short)uVar53;
              cVar13 = (0 < sVar8) * (sVar8 < 0x100) * (char)uVar53 - (0xff < sVar8);
              sVar8 = (short)(uVar53 >> 0x10);
              uVar44 = CONCAT15((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar53 >> 0x10) -
                                (0xff < sVar8),CONCAT14(cVar13,uVar43));
              sVar8 = (short)(uVar53 >> 0x20);
              cVar14 = (0 < sVar8) * (sVar8 < 0x100) * (char)(uVar53 >> 0x20) - (0xff < sVar8);
              sVar10 = (short)(uVar53 >> 0x30);
              sVar8 = (short)((uint)uVar43 >> 0x10);
              sVar9 = (short)((uint6)uVar44 >> 0x20);
              sVar10 = (short)(CONCAT17((0 < sVar10) * (sVar10 < 0x100) * (char)(uVar53 >> 0x30) -
                                        (0xff < sVar10),CONCAT16(cVar14,uVar44)) >> 0x30);
              *(uint *)(acStack_8c8 + uVar36) =
                   CONCAT13(((0 < sVar10) * (sVar10 < 0x100) * cVar14 - (0xff < sVar10)) + cVar20,
                            CONCAT12(((0 < sVar9) * (sVar9 < 0x100) * cVar13 - (0xff < sVar9)) +
                                     cVar19,CONCAT11(((0 < sVar8) * (sVar8 < 0x100) * cVar12 -
                                                     (0xff < sVar8)) + cVar18,
                                                     ((0 < sVar41) * (sVar41 < 0x100) * cVar11 -
                                                     (0xff < sVar41)) + cVar17)));
              uVar36 = uVar36 + 4;
            } while (uVar39 != uVar36);
            dVar16 = DAT_180010468;
            fVar42 = DAT_180010460;
            if ((uVar5 & 0x7ffffffc) == uVar5) goto LAB_18000a63a;
          }
          do {
            dVar49 = floor((double)(*(float *)(param_3 + uVar39 * 4) * fVar42) + dVar16);
            acStack_8c8[uVar39] = (char)(int)dVar49 + '\x7f';
            uVar39 = uVar39 + 1;
          } while (uVar29 != uVar39);
        }
LAB_18000a63a:
        auVar57 = _DAT_180010470;
        if (0 < (int)(uint)local_8e0) {
          if ((int)uVar5 < 0xd) {
            if ((int)uVar5 < 1) {
              if ((uint)local_8e0 < 9) {
                lVar33 = 0;
              }
              else {
                uVar23 = ((ulonglong)uVar6 - 1 >> 3) + 1 & 0xfffffffffffffffe;
                lVar33 = 0;
                do {
                  pfVar24 = (float *)(uVar26 + lVar33 * 4);
                  fVar42 = pfVar24[1];
                  fVar50 = pfVar24[2];
                  fVar51 = pfVar24[3];
                  pfVar30 = (float *)(uVar26 + 0x10 + lVar33 * 4);
                  fVar54 = *pfVar30;
                  fVar55 = pfVar30[1];
                  fVar59 = pfVar30[2];
                  fVar61 = pfVar30[3];
                  pfVar30 = (float *)((longlong)param_2 + lVar33 * 4);
                  *pfVar30 = *pfVar24 * 0.0;
                  pfVar30[1] = fVar42 * 0.0;
                  pfVar30[2] = fVar50 * 0.0;
                  pfVar30[3] = fVar51 * 0.0;
                  pfVar30 = (float *)((longlong)param_2 + lVar33 * 4 + 0x10);
                  *pfVar30 = fVar54 * 0.0;
                  pfVar30[1] = fVar55 * 0.0;
                  pfVar30[2] = fVar59 * 0.0;
                  pfVar30[3] = fVar61 * 0.0;
                  pfVar30 = (float *)(uVar26 + 0x20 + lVar33 * 4);
                  fVar42 = pfVar30[1];
                  fVar50 = pfVar30[2];
                  fVar51 = pfVar30[3];
                  pfVar24 = (float *)(uVar26 + 0x30 + lVar33 * 4);
                  fVar54 = *pfVar24;
                  fVar55 = pfVar24[1];
                  fVar59 = pfVar24[2];
                  fVar61 = pfVar24[3];
                  pfVar24 = (float *)((longlong)param_2 + lVar33 * 4 + 0x20);
                  *pfVar24 = *pfVar30 * 0.0;
                  pfVar24[1] = fVar42 * 0.0;
                  pfVar24[2] = fVar50 * 0.0;
                  pfVar24[3] = fVar51 * 0.0;
                  pfVar30 = (float *)((longlong)param_2 + lVar33 * 4 + 0x30);
                  *pfVar30 = fVar54 * 0.0;
                  pfVar30[1] = fVar55 * 0.0;
                  pfVar30[2] = fVar59 * 0.0;
                  pfVar30[3] = fVar61 * 0.0;
                  lVar33 = lVar33 + 0x10;
                  uVar23 = uVar23 - 2;
                } while (uVar23 != 0);
              }
              if (((ulonglong)uVar6 - 1 & 8) == 0) {
                pfVar24 = (float *)(uVar26 + lVar33 * 4);
                fVar42 = pfVar24[1];
                fVar50 = pfVar24[2];
                fVar51 = pfVar24[3];
                pfVar30 = (float *)(uVar26 + 0x10 + lVar33 * 4);
                fVar54 = *pfVar30;
                fVar55 = pfVar30[1];
                fVar59 = pfVar30[2];
                fVar61 = pfVar30[3];
                pfVar30 = (float *)((longlong)param_2 + lVar33 * 4);
                *pfVar30 = *pfVar24 * 0.0;
                pfVar30[1] = fVar42 * 0.0;
                pfVar30[2] = fVar50 * 0.0;
                pfVar30[3] = fVar51 * 0.0;
                pfVar30 = (float *)((longlong)param_2 + lVar33 * 4 + 0x10);
                *pfVar30 = fVar54 * 0.0;
                pfVar30[1] = fVar55 * 0.0;
                pfVar30[2] = fVar59 * 0.0;
                pfVar30[3] = fVar61 * 0.0;
              }
            }
            else {
              uVar23 = 0;
              do {
                iVar27 = 0;
                iVar60 = 0;
                iVar62 = 0;
                iVar64 = 0;
                uVar28 = 0;
                iVar65 = 0;
                iVar66 = 0;
                iVar67 = 0;
                iVar68 = 0;
                do {
                  uVar43 = *(undefined4 *)(acStack_8c8 + uVar28);
                  auVar86._4_4_ = uVar43;
                  auVar86._0_4_ = uVar43;
                  auVar86._8_4_ = uVar43;
                  auVar86._12_4_ = uVar43;
                  uVar78 = (ushort)uVar43;
                  auVar70._0_2_ = uVar78 >> 8;
                  uVar85 = (ushort)((uint)uVar43 >> 0x10);
                  auVar70._2_2_ = uVar85 >> 8;
                  auVar70._4_2_ = uVar78 >> 8;
                  auVar70._6_2_ = uVar85 >> 8;
                  auVar70._8_2_ = uVar78 >> 8;
                  auVar70._10_2_ = uVar85 >> 8;
                  auVar70._12_2_ = uVar78 >> 8;
                  auVar70._14_2_ = uVar85 >> 8;
                  auVar81 = psraw(pauVar31[1],8);
                  auVar79 = psllw(pauVar31[1],8);
                  auVar79 = psraw(auVar79,8);
                  auVar79 = pmaddwd(auVar79,auVar86 & auVar57);
                  auVar81 = pmaddwd(auVar81,auVar70);
                  iVar27 = iVar27 + auVar81._0_4_ + auVar79._0_4_;
                  iVar60 = iVar60 + auVar81._4_4_ + auVar79._4_4_;
                  iVar62 = iVar62 + auVar81._8_4_ + auVar79._8_4_;
                  iVar64 = iVar64 + auVar81._12_4_ + auVar79._12_4_;
                  auVar81 = psraw(*pauVar31,8);
                  auVar79 = psllw(*pauVar31,8);
                  auVar79 = psraw(auVar79,8);
                  auVar79 = pmaddwd(auVar79,auVar86 & auVar57);
                  auVar86 = pmaddwd(auVar81,auVar70);
                  iVar65 = iVar65 + auVar86._0_4_ + auVar79._0_4_;
                  iVar66 = iVar66 + auVar86._4_4_ + auVar79._4_4_;
                  iVar67 = iVar67 + auVar86._8_4_ + auVar79._8_4_;
                  iVar68 = iVar68 + auVar86._12_4_ + auVar79._12_4_;
                  pauVar31 = pauVar31 + 2;
                  uVar28 = uVar28 + 4;
                } while (uVar28 < uVar29);
                pfVar24 = (float *)(uVar26 + uVar23 * 4);
                fVar42 = pfVar24[1];
                fVar50 = pfVar24[2];
                fVar51 = pfVar24[3];
                pfVar30 = (float *)(uVar26 + 0x10 + uVar23 * 4);
                fVar54 = *pfVar30;
                fVar55 = pfVar30[1];
                fVar59 = pfVar30[2];
                fVar61 = pfVar30[3];
                pfVar30 = (float *)((longlong)param_2 + uVar23 * 4);
                *pfVar30 = *pfVar24 * (float)iVar65;
                pfVar30[1] = fVar42 * (float)iVar66;
                pfVar30[2] = fVar50 * (float)iVar67;
                pfVar30[3] = fVar51 * (float)iVar68;
                pfVar30 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
                *pfVar30 = fVar54 * (float)iVar27;
                pfVar30[1] = fVar55 * (float)iVar60;
                pfVar30[2] = fVar59 * (float)iVar62;
                pfVar30[3] = fVar61 * (float)iVar64;
                uVar23 = uVar23 + 8;
              } while (uVar23 < uVar40);
            }
          }
          else {
            uVar23 = 0;
            do {
              iVar65 = 0;
              iVar66 = 0;
              iVar67 = 0;
              iVar68 = 0;
              uVar28 = 0;
              iVar27 = 0;
              iVar60 = 0;
              iVar62 = 0;
              iVar64 = 0;
              do {
                uVar43 = *(undefined4 *)(acStack_8c8 + uVar28);
                auVar81._4_4_ = uVar43;
                auVar81._0_4_ = uVar43;
                auVar81._8_4_ = uVar43;
                auVar81._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar76._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar76._2_2_ = uVar85 >> 8;
                auVar76._4_2_ = uVar78 >> 8;
                auVar76._6_2_ = uVar85 >> 8;
                auVar76._8_2_ = uVar78 >> 8;
                auVar76._10_2_ = uVar85 >> 8;
                auVar76._12_2_ = uVar78 >> 8;
                auVar76._14_2_ = uVar85 >> 8;
                auVar86 = psraw(pauVar31[1],8);
                auVar79 = psllw(pauVar31[1],8);
                auVar79 = psraw(auVar79,8);
                auVar82 = pmaddwd(auVar79,auVar81 & auVar57);
                auVar89 = pmaddwd(auVar86,auVar76);
                auVar86 = psraw(*pauVar31,8);
                auVar79 = psllw(*pauVar31,8);
                auVar79 = psraw(auVar79,8);
                auVar77 = pmaddwd(auVar79,auVar81 & auVar57);
                auVar92 = pmaddwd(auVar86,auVar76);
                uVar43 = *(undefined4 *)((longlong)auStack_8c4 + uVar28);
                auVar79._4_4_ = uVar43;
                auVar79._0_4_ = uVar43;
                auVar79._8_4_ = uVar43;
                auVar79._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar80._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar80._2_2_ = uVar85 >> 8;
                auVar80._4_2_ = uVar78 >> 8;
                auVar80._6_2_ = uVar85 >> 8;
                auVar80._8_2_ = uVar78 >> 8;
                auVar80._10_2_ = uVar85 >> 8;
                auVar80._12_2_ = uVar78 >> 8;
                auVar80._14_2_ = uVar85 >> 8;
                auVar86 = psraw(pauVar31[3],8);
                auVar81 = psllw(pauVar31[3],8);
                auVar81 = psraw(auVar81,8);
                auVar76 = pmaddwd(auVar81,auVar79 & auVar57);
                auVar86 = pmaddwd(auVar86,auVar80);
                auVar74 = psraw(pauVar31[2],8);
                auVar81 = psllw(pauVar31[2],8);
                auVar81 = psraw(auVar81,8);
                auVar70 = pmaddwd(auVar81,auVar79 & auVar57);
                auVar83 = pmaddwd(auVar74,auVar80);
                uVar43 = *(undefined4 *)((longlong)auStack_8c4 + uVar28 + 4);
                auVar75._4_4_ = uVar43;
                auVar75._0_4_ = uVar43;
                auVar75._8_4_ = uVar43;
                auVar75._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar87._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar87._2_2_ = uVar85 >> 8;
                auVar87._4_2_ = uVar78 >> 8;
                auVar87._6_2_ = uVar85 >> 8;
                auVar87._8_2_ = uVar78 >> 8;
                auVar87._10_2_ = uVar85 >> 8;
                auVar87._12_2_ = uVar78 >> 8;
                auVar87._14_2_ = uVar85 >> 8;
                auVar79 = psraw(pauVar31[5],8);
                auVar81 = psllw(pauVar31[5],8);
                auVar81 = psraw(auVar81,8);
                auVar93 = pmaddwd(auVar81,auVar75 & auVar57);
                auVar79 = pmaddwd(auVar79,auVar87);
                auVar81 = psraw(pauVar31[4],8);
                auVar74 = psllw(pauVar31[4],8);
                auVar74 = psraw(auVar74,8);
                auVar80 = pmaddwd(auVar74,auVar75 & auVar57);
                auVar75 = pmaddwd(auVar81,auVar87);
                uVar43 = *(undefined4 *)((longlong)auStack_8c4 + uVar28 + 8);
                auVar74._4_4_ = uVar43;
                auVar74._0_4_ = uVar43;
                auVar74._8_4_ = uVar43;
                auVar74._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar90._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar90._2_2_ = uVar85 >> 8;
                auVar90._4_2_ = uVar78 >> 8;
                auVar90._6_2_ = uVar85 >> 8;
                auVar90._8_2_ = uVar78 >> 8;
                auVar90._10_2_ = uVar85 >> 8;
                auVar90._12_2_ = uVar78 >> 8;
                auVar90._14_2_ = uVar85 >> 8;
                auVar81 = psraw(pauVar31[7],8);
                auVar87 = psllw(pauVar31[7],8);
                auVar87 = psraw(auVar87,8);
                auVar87 = pmaddwd(auVar87,auVar74 & auVar57);
                auVar81 = pmaddwd(auVar81,auVar90);
                iVar65 = auVar81._0_4_ + auVar87._0_4_ +
                         auVar79._0_4_ + auVar93._0_4_ + auVar86._0_4_ +
                         auVar76._0_4_ + auVar89._0_4_ + auVar82._0_4_ + iVar65;
                iVar66 = auVar81._4_4_ + auVar87._4_4_ +
                         auVar79._4_4_ + auVar93._4_4_ + auVar86._4_4_ +
                         auVar76._4_4_ + auVar89._4_4_ + auVar82._4_4_ + iVar66;
                iVar67 = auVar81._8_4_ + auVar87._8_4_ +
                         auVar79._8_4_ + auVar93._8_4_ + auVar86._8_4_ +
                         auVar76._8_4_ + auVar89._8_4_ + auVar82._8_4_ + iVar67;
                iVar68 = auVar81._12_4_ + auVar87._12_4_ +
                         auVar79._12_4_ + auVar93._12_4_ + auVar86._12_4_ +
                         auVar76._12_4_ + auVar89._12_4_ + auVar82._12_4_ + iVar68;
                auVar79 = psraw(pauVar31[6],8);
                auVar86 = psllw(pauVar31[6],8);
                auVar86 = psraw(auVar86,8);
                auVar86 = pmaddwd(auVar86,auVar74 & auVar57);
                auVar79 = pmaddwd(auVar79,auVar90);
                iVar27 = auVar79._0_4_ + auVar86._0_4_ +
                         auVar75._0_4_ + auVar80._0_4_ + auVar83._0_4_ +
                         auVar70._0_4_ + auVar92._0_4_ + auVar77._0_4_ + iVar27;
                iVar60 = auVar79._4_4_ + auVar86._4_4_ +
                         auVar75._4_4_ + auVar80._4_4_ + auVar83._4_4_ +
                         auVar70._4_4_ + auVar92._4_4_ + auVar77._4_4_ + iVar60;
                iVar62 = auVar79._8_4_ + auVar86._8_4_ +
                         auVar75._8_4_ + auVar80._8_4_ + auVar83._8_4_ +
                         auVar70._8_4_ + auVar92._8_4_ + auVar77._8_4_ + iVar62;
                iVar64 = auVar79._12_4_ + auVar86._12_4_ +
                         auVar75._12_4_ + auVar80._12_4_ + auVar83._12_4_ +
                         auVar70._12_4_ + auVar92._12_4_ + auVar77._12_4_ + iVar64;
                pauVar31 = pauVar31 + 8;
                uVar28 = uVar28 + 0x10;
              } while (uVar28 < uVar5 - 0xc);
              if ((int)uVar28 < (int)uVar5) {
                do {
                  uVar43 = *(undefined4 *)(acStack_8c8 + uVar28);
                  auVar77._4_4_ = uVar43;
                  auVar77._0_4_ = uVar43;
                  auVar77._8_4_ = uVar43;
                  auVar77._12_4_ = uVar43;
                  uVar78 = (ushort)uVar43;
                  auVar82._0_2_ = uVar78 >> 8;
                  uVar85 = (ushort)((uint)uVar43 >> 0x10);
                  auVar82._2_2_ = uVar85 >> 8;
                  auVar82._4_2_ = uVar78 >> 8;
                  auVar82._6_2_ = uVar85 >> 8;
                  auVar82._8_2_ = uVar78 >> 8;
                  auVar82._10_2_ = uVar85 >> 8;
                  auVar82._12_2_ = uVar78 >> 8;
                  auVar82._14_2_ = uVar85 >> 8;
                  auVar79 = psraw(pauVar31[1],8);
                  auVar86 = psllw(pauVar31[1],8);
                  auVar86 = psraw(auVar86,8);
                  auVar86 = pmaddwd(auVar86,auVar77 & auVar57);
                  auVar79 = pmaddwd(auVar79,auVar82);
                  iVar65 = auVar79._0_4_ + iVar65 + auVar86._0_4_;
                  iVar66 = auVar79._4_4_ + iVar66 + auVar86._4_4_;
                  iVar67 = auVar79._8_4_ + iVar67 + auVar86._8_4_;
                  iVar68 = auVar79._12_4_ + iVar68 + auVar86._12_4_;
                  auVar79 = psraw(*pauVar31,8);
                  auVar86 = psllw(*pauVar31,8);
                  auVar86 = psraw(auVar86,8);
                  auVar86 = pmaddwd(auVar86,auVar77 & auVar57);
                  auVar79 = pmaddwd(auVar79,auVar82);
                  iVar27 = auVar79._0_4_ + iVar27 + auVar86._0_4_;
                  iVar60 = auVar79._4_4_ + iVar60 + auVar86._4_4_;
                  iVar62 = auVar79._8_4_ + iVar62 + auVar86._8_4_;
                  iVar64 = auVar79._12_4_ + iVar64 + auVar86._12_4_;
                  pauVar31 = pauVar31 + 2;
                  uVar28 = uVar28 + 4;
                } while (uVar28 < uVar29);
              }
              pfVar24 = (float *)(uVar26 + uVar23 * 4);
              fVar42 = pfVar24[1];
              fVar50 = pfVar24[2];
              fVar51 = pfVar24[3];
              pfVar30 = (float *)(uVar26 + 0x10 + uVar23 * 4);
              fVar54 = *pfVar30;
              fVar55 = pfVar30[1];
              fVar59 = pfVar30[2];
              fVar61 = pfVar30[3];
              pfVar30 = (float *)((longlong)param_2 + uVar23 * 4);
              *pfVar30 = *pfVar24 * (float)iVar27;
              pfVar30[1] = fVar42 * (float)iVar60;
              pfVar30[2] = fVar50 * (float)iVar62;
              pfVar30[3] = fVar51 * (float)iVar64;
              pfVar30 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
              *pfVar30 = fVar54 * (float)iVar65;
              pfVar30[1] = fVar55 * (float)iVar66;
              pfVar30[2] = fVar59 * (float)iVar67;
              pfVar30[3] = fVar61 * (float)iVar68;
              uVar23 = uVar23 + 8;
            } while (uVar23 < uVar40);
          }
        }
      }
      else {
        local_8d8 = param_1;
        if (0 < (int)uVar5) {
          if (uVar5 < 4) {
            uVar39 = 0;
            dVar16 = DAT_180010468;
            fVar42 = DAT_180010460;
          }
          else {
            uVar39 = (ulonglong)(uVar5 & 0x7ffffffc);
            uVar36 = 0;
            local_8d0 = uVar26;
            do {
              pfVar30 = (float *)(param_3 + uVar36 * 4);
              fVar55 = *pfVar30;
              fVar59 = pfVar30[1];
              fVar61 = pfVar30[3];
              dVar45 = floor((double)(pfVar30[2] * fVar51) + dVar16);
              dVar46 = floor((double)(fVar61 * fVar54) + dVar49);
              dVar47 = floor((double)(fVar55 * fVar42) + dVar16);
              dVar48 = floor((double)(fVar59 * fVar50) + dVar49);
              uVar26 = CONCAT44((int)dVar48,(int)dVar47) & uVar23;
              uVar52 = CONCAT44((int)dVar46,(int)dVar45) & uVar28;
              sVar8 = (short)uVar26;
              cVar11 = (0 < sVar8) * (sVar8 < 0x100) * (char)uVar26 - (0xff < sVar8);
              sVar8 = (short)(uVar26 >> 0x10);
              sVar41 = CONCAT11((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar26 >> 0x10) -
                                (0xff < sVar8),cVar11);
              sVar8 = (short)(uVar26 >> 0x20);
              cVar12 = (0 < sVar8) * (sVar8 < 0x100) * (char)(uVar26 >> 0x20) - (0xff < sVar8);
              sVar8 = (short)(uVar26 >> 0x30);
              uVar43 = CONCAT13((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar26 >> 0x30) -
                                (0xff < sVar8),CONCAT12(cVar12,sVar41));
              sVar8 = (short)uVar52;
              cVar13 = (0 < sVar8) * (sVar8 < 0x100) * (char)uVar52 - (0xff < sVar8);
              sVar8 = (short)(uVar52 >> 0x10);
              uVar44 = CONCAT15((0 < sVar8) * (sVar8 < 0x100) * (char)(uVar52 >> 0x10) -
                                (0xff < sVar8),CONCAT14(cVar13,uVar43));
              sVar8 = (short)(uVar52 >> 0x20);
              cVar14 = (0 < sVar8) * (sVar8 < 0x100) * (char)(uVar52 >> 0x20) - (0xff < sVar8);
              sVar10 = (short)(uVar52 >> 0x30);
              sVar8 = (short)((uint)uVar43 >> 0x10);
              sVar9 = (short)((uint6)uVar44 >> 0x20);
              sVar10 = (short)(CONCAT17((0 < sVar10) * (sVar10 < 0x100) * (char)(uVar52 >> 0x30) -
                                        (0xff < sVar10),CONCAT16(cVar14,uVar44)) >> 0x30);
              *(uint *)(acStack_8c8 + uVar36) =
                   CONCAT13(((0 < sVar10) * (sVar10 < 0x100) * cVar14 - (0xff < sVar10)) + cVar20,
                            CONCAT12(((0 < sVar9) * (sVar9 < 0x100) * cVar13 - (0xff < sVar9)) +
                                     cVar19,CONCAT11(((0 < sVar8) * (sVar8 < 0x100) * cVar12 -
                                                     (0xff < sVar8)) + cVar18,
                                                     ((0 < sVar41) * (sVar41 < 0x100) * cVar11 -
                                                     (0xff < sVar41)) + cVar17)));
              uVar36 = uVar36 + 4;
            } while (uVar39 != uVar36);
            uVar26 = local_8d0;
            dVar16 = DAT_180010468;
            fVar42 = DAT_180010460;
            if ((uVar5 & 0x7ffffffc) == uVar5) goto LAB_180009f2a;
          }
          do {
            dVar49 = floor((double)(*(float *)(param_3 + uVar39 * 4) * fVar42) + dVar16);
            acStack_8c8[uVar39] = (char)(int)dVar49 + '\x7f';
            uVar39 = uVar39 + 1;
          } while (uVar29 != uVar39);
        }
LAB_180009f2a:
        auVar57 = _DAT_180010470;
        if (0 < (int)local_8e0) {
          uVar23 = 0;
          do {
            uVar38 = *puVar35;
            puVar35 = puVar35 + 1;
            iVar27 = 0;
            iVar60 = 0;
            iVar62 = 0;
            iVar64 = 0;
            if ((int)uVar38 < 4) {
              iVar65 = 0;
              iVar66 = 0;
              iVar67 = 0;
              iVar68 = 0;
              if (0 < (int)uVar38) goto LAB_18000a1c0;
            }
            else {
              iVar65 = 0;
              iVar66 = 0;
              iVar67 = 0;
              iVar68 = 0;
              iVar32 = 0;
              do {
                uVar43 = *(undefined4 *)(acStack_8c8 + (int)*puVar35);
                auVar91._4_4_ = uVar43;
                auVar91._0_4_ = uVar43;
                auVar91._8_4_ = uVar43;
                auVar91._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar96._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar96._2_2_ = uVar85 >> 8;
                auVar96._4_2_ = uVar78 >> 8;
                auVar96._6_2_ = uVar85 >> 8;
                auVar96._8_2_ = uVar78 >> 8;
                auVar96._10_2_ = uVar85 >> 8;
                auVar96._12_2_ = uVar78 >> 8;
                auVar96._14_2_ = uVar85 >> 8;
                auVar86 = psraw(pauVar31[1],8);
                auVar79 = psllw(pauVar31[1],8);
                auVar79 = psraw(auVar79,8);
                auVar80 = pmaddwd(auVar79,auVar91 & auVar57);
                auVar87 = pmaddwd(auVar86,auVar96);
                auVar86 = psraw(*pauVar31,8);
                auVar79 = psllw(*pauVar31,8);
                auVar79 = psraw(auVar79,8);
                auVar75 = pmaddwd(auVar79,auVar91 & auVar57);
                auVar90 = pmaddwd(auVar86,auVar96);
                uVar43 = *(undefined4 *)(acStack_8c8 + (int)puVar35[1]);
                auVar83._4_4_ = uVar43;
                auVar83._0_4_ = uVar43;
                auVar83._8_4_ = uVar43;
                auVar83._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar94._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar94._2_2_ = uVar85 >> 8;
                auVar94._4_2_ = uVar78 >> 8;
                auVar94._6_2_ = uVar85 >> 8;
                auVar94._8_2_ = uVar78 >> 8;
                auVar94._10_2_ = uVar85 >> 8;
                auVar94._12_2_ = uVar78 >> 8;
                auVar94._14_2_ = uVar85 >> 8;
                auVar79 = psraw(pauVar31[3],8);
                auVar86 = psllw(pauVar31[3],8);
                auVar86 = psraw(auVar86,8);
                auVar74 = pmaddwd(auVar86,auVar83 & auVar57);
                auVar86 = pmaddwd(auVar79,auVar94);
                auVar81 = psraw(pauVar31[2],8);
                auVar79 = psllw(pauVar31[2],8);
                auVar79 = psraw(auVar79,8);
                auVar70 = pmaddwd(auVar79,auVar83 & auVar57);
                auVar82 = pmaddwd(auVar81,auVar94);
                uVar43 = *(undefined4 *)(acStack_8c8 + (int)puVar35[2]);
                auVar92._4_4_ = uVar43;
                auVar92._0_4_ = uVar43;
                auVar92._8_4_ = uVar43;
                auVar92._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar95._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar95._2_2_ = uVar85 >> 8;
                auVar95._4_2_ = uVar78 >> 8;
                auVar95._6_2_ = uVar85 >> 8;
                auVar95._8_2_ = uVar78 >> 8;
                auVar95._10_2_ = uVar85 >> 8;
                auVar95._12_2_ = uVar78 >> 8;
                auVar95._14_2_ = uVar85 >> 8;
                auVar79 = psraw(pauVar31[5],8);
                auVar81 = psllw(pauVar31[5],8);
                auVar81 = psraw(auVar81,8);
                auVar91 = pmaddwd(auVar81,auVar92 & auVar57);
                auVar79 = pmaddwd(auVar79,auVar95);
                auVar81 = psraw(pauVar31[4],8);
                auVar76 = psllw(pauVar31[4],8);
                auVar76 = psraw(auVar76,8);
                auVar77 = pmaddwd(auVar76,auVar92 & auVar57);
                auVar76 = pmaddwd(auVar81,auVar95);
                puVar2 = puVar35 + 3;
                puVar35 = puVar35 + 4;
                uVar43 = *(undefined4 *)(acStack_8c8 + (int)*puVar2);
                auVar89._4_4_ = uVar43;
                auVar89._0_4_ = uVar43;
                auVar89._8_4_ = uVar43;
                auVar89._12_4_ = uVar43;
                uVar78 = (ushort)uVar43;
                auVar93._0_2_ = uVar78 >> 8;
                uVar85 = (ushort)((uint)uVar43 >> 0x10);
                auVar93._2_2_ = uVar85 >> 8;
                auVar93._4_2_ = uVar78 >> 8;
                auVar93._6_2_ = uVar85 >> 8;
                auVar93._8_2_ = uVar78 >> 8;
                auVar93._10_2_ = uVar85 >> 8;
                auVar93._12_2_ = uVar78 >> 8;
                auVar93._14_2_ = uVar85 >> 8;
                auVar81 = psraw(pauVar31[7],8);
                auVar83 = psllw(pauVar31[7],8);
                auVar83 = psraw(auVar83,8);
                auVar83 = pmaddwd(auVar83,auVar89 & auVar57);
                auVar81 = pmaddwd(auVar81,auVar93);
                iVar65 = auVar81._0_4_ + auVar83._0_4_ +
                         auVar79._0_4_ + auVar91._0_4_ + auVar86._0_4_ +
                         auVar74._0_4_ + auVar87._0_4_ + auVar80._0_4_ + iVar65;
                iVar66 = auVar81._4_4_ + auVar83._4_4_ +
                         auVar79._4_4_ + auVar91._4_4_ + auVar86._4_4_ +
                         auVar74._4_4_ + auVar87._4_4_ + auVar80._4_4_ + iVar66;
                iVar67 = auVar81._8_4_ + auVar83._8_4_ +
                         auVar79._8_4_ + auVar91._8_4_ + auVar86._8_4_ +
                         auVar74._8_4_ + auVar87._8_4_ + auVar80._8_4_ + iVar67;
                iVar68 = auVar81._12_4_ + auVar83._12_4_ +
                         auVar79._12_4_ + auVar91._12_4_ + auVar86._12_4_ +
                         auVar74._12_4_ + auVar87._12_4_ + auVar80._12_4_ + iVar68;
                auVar79 = psraw(pauVar31[6],8);
                auVar86 = psllw(pauVar31[6],8);
                auVar86 = psraw(auVar86,8);
                auVar86 = pmaddwd(auVar86,auVar89 & auVar57);
                auVar79 = pmaddwd(auVar79,auVar93);
                iVar27 = auVar79._0_4_ + auVar86._0_4_ +
                         auVar76._0_4_ + auVar77._0_4_ + auVar82._0_4_ +
                         auVar70._0_4_ + auVar90._0_4_ + auVar75._0_4_ + iVar27;
                iVar60 = auVar79._4_4_ + auVar86._4_4_ +
                         auVar76._4_4_ + auVar77._4_4_ + auVar82._4_4_ +
                         auVar70._4_4_ + auVar90._4_4_ + auVar75._4_4_ + iVar60;
                iVar62 = auVar79._8_4_ + auVar86._8_4_ +
                         auVar76._8_4_ + auVar77._8_4_ + auVar82._8_4_ +
                         auVar70._8_4_ + auVar90._8_4_ + auVar75._8_4_ + iVar62;
                iVar64 = auVar79._12_4_ + auVar86._12_4_ +
                         auVar76._12_4_ + auVar77._12_4_ + auVar82._12_4_ +
                         auVar70._12_4_ + auVar90._12_4_ + auVar75._12_4_ + iVar64;
                pauVar31 = pauVar31 + 8;
                iVar32 = iVar32 + 4;
              } while (iVar32 < (int)(uVar38 - 3));
              uVar22 = uVar38 - (uVar38 & 0x7ffffffc);
              bVar7 = (int)(uVar38 & 0x7ffffffc) <= (int)uVar38;
              uVar38 = uVar22;
              if (uVar22 != 0 && bVar7) {
LAB_18000a1c0:
                do {
                  uVar22 = *puVar35;
                  puVar35 = puVar35 + 1;
                  uVar43 = *(undefined4 *)(acStack_8c8 + (int)uVar22);
                  auVar84._4_4_ = uVar43;
                  auVar84._0_4_ = uVar43;
                  auVar84._8_4_ = uVar43;
                  auVar84._12_4_ = uVar43;
                  uVar78 = (ushort)uVar43;
                  auVar88._0_2_ = uVar78 >> 8;
                  uVar85 = (ushort)((uint)uVar43 >> 0x10);
                  auVar88._2_2_ = uVar85 >> 8;
                  auVar88._4_2_ = uVar78 >> 8;
                  auVar88._6_2_ = uVar85 >> 8;
                  auVar88._8_2_ = uVar78 >> 8;
                  auVar88._10_2_ = uVar85 >> 8;
                  auVar88._12_2_ = uVar78 >> 8;
                  auVar88._14_2_ = uVar85 >> 8;
                  auVar79 = psraw(pauVar31[1],8);
                  auVar86 = psllw(pauVar31[1],8);
                  auVar86 = psraw(auVar86,8);
                  auVar86 = pmaddwd(auVar86,auVar84 & auVar57);
                  auVar79 = pmaddwd(auVar79,auVar88);
                  iVar65 = auVar79._0_4_ + iVar65 + auVar86._0_4_;
                  iVar66 = auVar79._4_4_ + iVar66 + auVar86._4_4_;
                  iVar67 = auVar79._8_4_ + iVar67 + auVar86._8_4_;
                  iVar68 = auVar79._12_4_ + iVar68 + auVar86._12_4_;
                  auVar79 = psraw(*pauVar31,8);
                  auVar86 = psllw(*pauVar31,8);
                  auVar86 = psraw(auVar86,8);
                  auVar86 = pmaddwd(auVar86,auVar84 & auVar57);
                  auVar79 = pmaddwd(auVar79,auVar88);
                  iVar27 = auVar79._0_4_ + iVar27 + auVar86._0_4_;
                  iVar60 = auVar79._4_4_ + iVar60 + auVar86._4_4_;
                  iVar62 = auVar79._8_4_ + iVar62 + auVar86._8_4_;
                  iVar64 = auVar79._12_4_ + iVar64 + auVar86._12_4_;
                  pauVar31 = pauVar31 + 2;
                  uVar38 = uVar38 - 1;
                } while (uVar38 != 0);
              }
            }
            pfVar24 = (float *)(uVar26 + uVar23 * 4);
            fVar42 = pfVar24[1];
            fVar50 = pfVar24[2];
            fVar51 = pfVar24[3];
            pfVar30 = (float *)(uVar26 + 0x10 + uVar23 * 4);
            fVar54 = *pfVar30;
            fVar55 = pfVar30[1];
            fVar59 = pfVar30[2];
            fVar61 = pfVar30[3];
            pfVar30 = (float *)((longlong)param_2 + uVar23 * 4);
            *pfVar30 = *pfVar24 * (float)iVar27;
            pfVar30[1] = fVar42 * (float)iVar60;
            pfVar30[2] = fVar50 * (float)iVar62;
            pfVar30[3] = fVar51 * (float)iVar64;
            pfVar30 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
            *pfVar30 = fVar54 * (float)iVar65;
            pfVar30[1] = fVar55 * (float)iVar66;
            pfVar30[2] = fVar59 * (float)iVar67;
            pfVar30[3] = fVar61 * (float)iVar68;
            uVar23 = uVar23 + 8;
          } while (uVar23 < local_8e0);
        }
      }
      uVar38 = (uint)local_8e0;
      pvVar37 = (void *)local_8d8[1];
      param_1 = local_8d8;
      goto LAB_18000aa73;
    }
    _Size = uVar40 << 2;
    _Dst = param_2;
LAB_18000a511:
    memset(_Dst,0,_Size);
    uVar38 = (uint)local_8e0;
  }
  else {
    piVar21 = (int *)param_1[4];
    if (piVar21 == (int *)0x0) {
      uVar23 = 0;
      if ((int)uVar6 < 0x10) {
LAB_180009cda:
        uVar38 = (uint)uVar23;
        local_8d8 = param_1;
        if ((int)(uVar6 - 7) <= (int)uVar38) goto LAB_18000a28f;
LAB_180009ce7:
        lVar33 = uVar40 - 7;
        if (0 < (int)uVar5) {
          uVar23 = (ulonglong)uVar38;
          pfVar24 = pfVar30 + uVar23 + 4;
          do {
            fVar42 = 0.0;
            fVar50 = 0.0;
            fVar51 = 0.0;
            fVar54 = 0.0;
            if (uVar5 == 1) {
              uVar28 = 0;
              auVar57 = ZEXT816(0);
            }
            else {
              uVar28 = 0;
              auVar57 = ZEXT816(0);
              pfVar34 = pfVar24;
              do {
                fVar55 = *(float *)(param_3 + uVar28 * 4);
                fVar59 = *(float *)(param_3 + 4 + uVar28 * 4);
                fVar61 = auVar57._4_4_;
                fVar63 = auVar57._8_4_;
                fVar69 = auVar57._12_4_;
                pfVar3 = pfVar34 + (uVar40 - 4);
                pfVar1 = pfVar34 + uVar40;
                auVar57._0_4_ = *pfVar3 * fVar59 + pfVar34[-4] * fVar55 + auVar57._0_4_;
                auVar57._4_4_ = pfVar3[1] * fVar59 + pfVar34[-3] * fVar55 + fVar61;
                auVar57._8_4_ = pfVar3[2] * fVar59 + pfVar34[-2] * fVar55 + fVar63;
                auVar57._12_4_ = pfVar3[3] * fVar59 + pfVar34[-1] * fVar55 + fVar69;
                fVar42 = *pfVar1 * fVar59 + *pfVar34 * fVar55 + fVar42;
                fVar50 = pfVar1[1] * fVar59 + pfVar34[1] * fVar55 + fVar50;
                fVar51 = pfVar1[2] * fVar59 + pfVar34[2] * fVar55 + fVar51;
                fVar54 = pfVar1[3] * fVar59 + pfVar34[3] * fVar55 + fVar54;
                uVar28 = uVar28 + 2;
                pfVar34 = pfVar34 + uVar40 * 2;
              } while ((uVar5 & 0x7ffffffe) != uVar28);
            }
            auVar58 = auVar57;
            if ((uVar5 & 1) != 0) {
              fVar55 = *(float *)(param_3 + uVar28 * 4);
              pfVar34 = pfVar30 + uVar23 + uVar28 * uVar40;
              pfVar1 = pfVar30 + uVar23 + uVar28 * uVar40 + 4;
              auVar58._0_4_ = auVar57._0_4_ + *pfVar34 * fVar55;
              auVar58._4_4_ = auVar57._4_4_ + pfVar34[1] * fVar55;
              auVar58._8_4_ = auVar57._8_4_ + pfVar34[2] * fVar55;
              auVar58._12_4_ = auVar57._12_4_ + pfVar34[3] * fVar55;
              fVar42 = fVar42 + *pfVar1 * fVar55;
              fVar50 = fVar50 + pfVar1[1] * fVar55;
              fVar51 = fVar51 + pfVar1[2] * fVar55;
              fVar54 = fVar54 + pfVar1[3] * fVar55;
            }
            *(undefined1 (*) [16])((longlong)param_2 + uVar23 * 4) = auVar58;
            pfVar34 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
            *pfVar34 = fVar42;
            pfVar34[1] = fVar50;
            pfVar34[2] = fVar51;
            pfVar34[3] = fVar54;
            uVar23 = uVar23 + 8;
            pfVar24 = pfVar24 + 8;
          } while ((longlong)uVar23 < (longlong)(int)lVar33);
          goto LAB_18000a28f;
        }
        local_8d0 = (ulonglong)uVar38;
        memset((void *)((longlong)param_2 + local_8d0 * 4),0,
               (ulonglong)(((int)local_8e0 - uVar38) - 8 >> 3) * 0x20 + 0x20);
        lVar25 = local_8d0 + 8;
        if ((longlong)(local_8d0 + 8) <= lVar33) {
          lVar25 = lVar33;
        }
        uVar38 = uVar38 + ((int)lVar25 + ~uVar38 & 0xfffffff8) + 8;
        uVar23 = (ulonglong)uVar38;
        if ((int)uVar38 < (int)(uVar6 - 3)) goto LAB_18000a29c;
      }
      else {
        uVar38 = uVar6 - 0xf;
        if (0 < (int)uVar5) {
          pfVar24 = pfVar30 + 0xc;
          uVar23 = 0;
          do {
            fVar42 = 0.0;
            fVar50 = 0.0;
            fVar51 = 0.0;
            fVar54 = 0.0;
            uVar28 = 0;
            fVar55 = 0.0;
            fVar59 = 0.0;
            fVar61 = 0.0;
            fVar63 = 0.0;
            fVar69 = 0.0;
            fVar71 = 0.0;
            fVar72 = 0.0;
            fVar73 = 0.0;
            pfVar34 = pfVar24;
            auVar57 = ZEXT816(0);
            do {
              fVar4 = *(float *)(param_3 + uVar28 * 4);
              fVar55 = fVar55 + pfVar34[-0xc] * fVar4;
              fVar59 = fVar59 + pfVar34[-0xb] * fVar4;
              fVar61 = fVar61 + pfVar34[-10] * fVar4;
              fVar63 = fVar63 + pfVar34[-9] * fVar4;
              fVar42 = fVar42 + pfVar34[-8] * fVar4;
              fVar50 = fVar50 + pfVar34[-7] * fVar4;
              fVar51 = fVar51 + pfVar34[-6] * fVar4;
              fVar54 = fVar54 + pfVar34[-5] * fVar4;
              fVar69 = fVar69 + pfVar34[-4] * fVar4;
              fVar71 = fVar71 + pfVar34[-3] * fVar4;
              fVar72 = fVar72 + pfVar34[-2] * fVar4;
              fVar73 = fVar73 + pfVar34[-1] * fVar4;
              auVar56._0_4_ = auVar57._0_4_ + *pfVar34 * fVar4;
              auVar56._4_4_ = auVar57._4_4_ + pfVar34[1] * fVar4;
              auVar56._8_4_ = auVar57._8_4_ + pfVar34[2] * fVar4;
              auVar56._12_4_ = auVar57._12_4_ + pfVar34[3] * fVar4;
              uVar28 = uVar28 + 1;
              pfVar34 = pfVar34 + local_8e0;
              auVar57 = auVar56;
            } while (uVar29 != uVar28);
            pfVar34 = (float *)((longlong)param_2 + uVar23 * 4);
            *pfVar34 = fVar55;
            pfVar34[1] = fVar59;
            pfVar34[2] = fVar61;
            pfVar34[3] = fVar63;
            pfVar34 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
            *pfVar34 = fVar42;
            pfVar34[1] = fVar50;
            pfVar34[2] = fVar51;
            pfVar34[3] = fVar54;
            pfVar34 = (float *)((longlong)param_2 + uVar23 * 4 + 0x20);
            *pfVar34 = fVar69;
            pfVar34[1] = fVar71;
            pfVar34[2] = fVar72;
            pfVar34[3] = fVar73;
            *(undefined1 (*) [16])((longlong)param_2 + uVar23 * 4 + 0x30) = auVar56;
            uVar23 = uVar23 + 0x10;
            pfVar24 = pfVar24 + 0x10;
          } while (uVar23 < uVar38);
          goto LAB_180009cda;
        }
        local_8d8 = param_1;
        memset(param_2,0,(ulonglong)(uVar6 - 0x10 >> 4) * 0x40 + 0x40);
        uVar22 = 0x10;
        if (0x10 < uVar38) {
          uVar22 = uVar38;
        }
        uVar38 = (uVar22 - 1 & 0xfffffff0) + 0x10;
        uVar23 = (ulonglong)uVar38;
        if ((int)uVar38 < (int)(uVar6 - 7)) goto LAB_180009ce7;
LAB_18000a28f:
        if ((int)uVar23 < (int)(uVar6 - 3)) {
LAB_18000a29c:
          uVar38 = (uint)uVar23;
          if ((int)uVar5 < 1) {
            iVar60 = (int)(uVar40 - 3);
            iVar27 = uVar38 + 4;
            if ((int)(uVar38 + 4) < iVar60) {
              iVar27 = iVar60;
            }
            memset((void *)((longlong)param_2 + (uVar23 & 0xffffffff) * 4),0,
                   (ulonglong)(~uVar38 + iVar27 >> 2) * 0x10 + 0x10);
            uVar23 = (ulonglong)(uVar38 + (~uVar38 + iVar27 & 0xfffffffc) + 4);
          }
          else {
            uVar23 = uVar23 & 0xffffffff;
            pfVar24 = pfVar30 + uVar23;
            do {
              fVar42 = 0.0;
              fVar50 = 0.0;
              fVar51 = 0.0;
              fVar54 = 0.0;
              if (uVar5 == 1) {
                uVar28 = 0;
              }
              else {
                uVar28 = 0;
                pfVar34 = pfVar24;
                do {
                  fVar55 = *(float *)(param_3 + uVar28 * 4);
                  fVar59 = *(float *)(param_3 + 4 + uVar28 * 4);
                  pfVar1 = pfVar34 + uVar40;
                  fVar42 = *pfVar1 * fVar59 + *pfVar34 * fVar55 + fVar42;
                  fVar50 = pfVar1[1] * fVar59 + pfVar34[1] * fVar55 + fVar50;
                  fVar51 = pfVar1[2] * fVar59 + pfVar34[2] * fVar55 + fVar51;
                  fVar54 = pfVar1[3] * fVar59 + pfVar34[3] * fVar55 + fVar54;
                  uVar28 = uVar28 + 2;
                  pfVar34 = pfVar34 + uVar40 * 2;
                } while ((uVar5 & 0x7ffffffe) != uVar28);
              }
              if ((uVar5 & 1) != 0) {
                fVar55 = *(float *)(param_3 + uVar28 * 4);
                pfVar34 = pfVar30 + uVar23 + uVar28 * uVar40;
                fVar42 = fVar42 + *pfVar34 * fVar55;
                fVar50 = fVar50 + pfVar34[1] * fVar55;
                fVar51 = fVar51 + pfVar34[2] * fVar55;
                fVar54 = fVar54 + pfVar34[3] * fVar55;
              }
              pfVar34 = (float *)((longlong)param_2 + uVar23 * 4);
              *pfVar34 = fVar42;
              pfVar34[1] = fVar50;
              pfVar34[2] = fVar51;
              pfVar34[3] = fVar54;
              uVar23 = uVar23 + 4;
              pfVar24 = pfVar24 + 4;
            } while ((longlong)uVar23 < (longlong)(uVar40 - 3));
          }
        }
      }
      uVar38 = (uint)local_8e0;
      uVar22 = (uint)uVar23;
      param_1 = local_8d8;
      if ((int)uVar22 < (int)uVar6) {
        if ((int)uVar5 < 1) {
          _Size = (ulonglong)(~uVar22 + uVar6) * 4 + 4;
          _Dst = (void *)((longlong)param_2 + (uVar23 & 0xffffffff) * 4);
          goto LAB_18000a511;
        }
        uVar23 = (ulonglong)(int)uVar22;
        pfVar24 = pfVar30 + uVar23;
        do {
          *(undefined4 *)((longlong)param_2 + uVar23 * 4) = 0;
          fVar42 = 0.0;
          if (uVar5 == 1) {
            uVar28 = 0;
          }
          else {
            uVar28 = 0;
            pfVar34 = pfVar24;
            do {
              fVar42 = *pfVar34 * *(float *)(param_3 + uVar28 * 4) + fVar42;
              *(float *)((longlong)param_2 + uVar23 * 4) = fVar42;
              fVar42 = pfVar34[uVar40] * *(float *)(param_3 + 4 + uVar28 * 4) + fVar42;
              *(float *)((longlong)param_2 + uVar23 * 4) = fVar42;
              uVar28 = uVar28 + 2;
              pfVar34 = pfVar34 + uVar40 * 2;
            } while ((uVar5 & 0x7ffffffe) != uVar28);
          }
          if ((uVar5 & 1) != 0) {
            *(float *)((longlong)param_2 + uVar23 * 4) =
                 pfVar30[uVar23 + uVar28 * uVar40] * *(float *)(param_3 + uVar28 * 4) + fVar42;
          }
          uVar23 = uVar23 + 1;
          pfVar24 = pfVar24 + 1;
        } while (uVar23 != uVar40);
      }
    }
    else {
      if ((int)uVar6 < 1) goto LAB_18000aba6;
      uVar23 = 0;
      do {
        iVar27 = *piVar21;
        piVar21 = piVar21 + 1;
        if (iVar27 < 1) {
          fVar55 = 0.0;
          fVar59 = 0.0;
          fVar61 = 0.0;
          fVar63 = 0.0;
          fVar42 = 0.0;
          fVar50 = 0.0;
          fVar51 = 0.0;
          fVar54 = 0.0;
        }
        else {
          fVar42 = 0.0;
          fVar50 = 0.0;
          fVar51 = 0.0;
          fVar54 = 0.0;
          fVar55 = 0.0;
          fVar59 = 0.0;
          fVar61 = 0.0;
          fVar63 = 0.0;
          do {
            lVar33 = (longlong)*piVar21;
            fVar69 = *(float *)(param_3 + lVar33 * 4);
            fVar71 = *(float *)(param_3 + 4 + lVar33 * 4);
            fVar72 = *(float *)(param_3 + 8 + lVar33 * 4);
            fVar73 = *(float *)(param_3 + 0xc + lVar33 * 4);
            auVar57 = *(undefined1 (*) [16])(pfVar30 + 0xc);
            fVar55 = pfVar30[0x18] * fVar73 +
                     pfVar30[0x10] * fVar72 + pfVar30[8] * fVar71 + *pfVar30 * fVar69 + fVar55;
            fVar59 = pfVar30[0x19] * fVar73 +
                     pfVar30[0x11] * fVar72 + pfVar30[9] * fVar71 + pfVar30[1] * fVar69 + fVar59;
            fVar61 = pfVar30[0x1a] * fVar73 +
                     pfVar30[0x12] * fVar72 + pfVar30[10] * fVar71 + pfVar30[2] * fVar69 + fVar61;
            fVar63 = pfVar30[0x1b] * fVar73 +
                     pfVar30[0x13] * fVar72 + pfVar30[0xb] * fVar71 + pfVar30[3] * fVar69 + fVar63;
            fVar42 = pfVar30[0x1c] * fVar73 +
                     pfVar30[0x14] * fVar72 + fVar71 * auVar57._0_4_ + pfVar30[4] * fVar69 + fVar42;
            fVar50 = pfVar30[0x1d] * fVar73 +
                     pfVar30[0x15] * fVar72 + fVar71 * auVar57._4_4_ + pfVar30[5] * fVar69 + fVar50;
            fVar51 = pfVar30[0x1e] * fVar73 +
                     pfVar30[0x16] * fVar72 + fVar71 * auVar57._8_4_ + pfVar30[6] * fVar69 + fVar51;
            fVar54 = pfVar30[0x1f] * fVar73 +
                     pfVar30[0x17] * fVar72 + fVar71 * auVar57._12_4_ + pfVar30[7] * fVar69 + fVar54
            ;
            pfVar30 = pfVar30 + 0x20;
            piVar21 = piVar21 + 1;
            iVar27 = iVar27 + -1;
          } while (iVar27 != 0);
        }
        pfVar24 = (float *)((longlong)param_2 + uVar23 * 4);
        *pfVar24 = fVar55;
        pfVar24[1] = fVar59;
        pfVar24[2] = fVar61;
        pfVar24[3] = fVar63;
        pfVar24 = (float *)((longlong)param_2 + uVar23 * 4 + 0x10);
        *pfVar24 = fVar42;
        pfVar24[1] = fVar50;
        pfVar24[2] = fVar51;
        pfVar24[3] = fVar54;
        uVar23 = uVar23 + 8;
        uVar38 = uVar6;
      } while (uVar23 < local_8e0);
    }
  }
LAB_18000aa73:
  if (0 < (int)uVar6 && pvVar37 != (void *)0x0) {
    if ((uVar38 < 8) ||
       (param_2 < (void *)(uVar40 * 4 + (longlong)pvVar37) &&
        pvVar37 < (void *)((longlong)param_2 + uVar40 * 4))) {
      uVar23 = 0;
    }
    else {
      uVar23 = (ulonglong)(uVar6 & 0x7ffffff8);
      lVar33 = 0;
      do {
        pfVar34 = (float *)((longlong)pvVar37 + lVar33);
        fVar42 = pfVar34[1];
        fVar50 = pfVar34[2];
        fVar51 = pfVar34[3];
        pfVar30 = (float *)((longlong)pvVar37 + lVar33 + 0x10);
        fVar54 = *pfVar30;
        fVar55 = pfVar30[1];
        fVar59 = pfVar30[2];
        fVar61 = pfVar30[3];
        pfVar30 = (float *)((longlong)param_2 + lVar33);
        fVar63 = pfVar30[1];
        fVar69 = pfVar30[2];
        fVar71 = pfVar30[3];
        pfVar24 = (float *)((longlong)param_2 + lVar33 + 0x10);
        fVar72 = *pfVar24;
        fVar73 = pfVar24[1];
        fVar4 = pfVar24[2];
        fVar15 = pfVar24[3];
        pfVar24 = (float *)((longlong)param_2 + lVar33);
        *pfVar24 = *pfVar30 + *pfVar34;
        pfVar24[1] = fVar63 + fVar42;
        pfVar24[2] = fVar69 + fVar50;
        pfVar24[3] = fVar71 + fVar51;
        pfVar30 = (float *)((longlong)param_2 + lVar33 + 0x10);
        *pfVar30 = fVar72 + fVar54;
        pfVar30[1] = fVar73 + fVar55;
        pfVar30[2] = fVar4 + fVar59;
        pfVar30[3] = fVar15 + fVar61;
        lVar33 = lVar33 + 0x20;
      } while ((ulonglong)(uVar6 >> 3 & 0xfffffff) << 5 != lVar33);
      if (uVar23 == uVar40) goto LAB_18000aba6;
    }
    uVar26 = uVar23;
    for (uVar28 = uVar40 & 3; uVar28 != 0; uVar28 = uVar28 - 1) {
      *(float *)((longlong)param_2 + uVar26 * 4) =
           *(float *)((longlong)pvVar37 + uVar26 * 4) + *(float *)((longlong)param_2 + uVar26 * 4);
      uVar26 = uVar26 + 1;
    }
    if (uVar23 - uVar40 < 0xfffffffffffffffd) {
      do {
        *(float *)((longlong)param_2 + uVar26 * 4) =
             *(float *)((longlong)pvVar37 + uVar26 * 4) + *(float *)((longlong)param_2 + uVar26 * 4)
        ;
        *(float *)((longlong)param_2 + uVar26 * 4 + 4) =
             *(float *)((longlong)pvVar37 + uVar26 * 4 + 4) +
             *(float *)((longlong)param_2 + uVar26 * 4 + 4);
        *(float *)((longlong)param_2 + uVar26 * 4 + 8) =
             *(float *)((longlong)pvVar37 + uVar26 * 4 + 8) +
             *(float *)((longlong)param_2 + uVar26 * 4 + 8);
        *(float *)((longlong)param_2 + uVar26 * 4 + 0xc) =
             *(float *)((longlong)pvVar37 + uVar26 * 4 + 0xc) +
             *(float *)((longlong)param_2 + uVar26 * 4 + 0xc);
        uVar26 = uVar26 + 4;
      } while (uVar40 != uVar26);
    }
  }
LAB_18000aba6:
  if (0 < (int)uVar5 && param_1[5] != 0) {
    uVar28 = (ulonglong)(uVar5 * 2);
    uVar40 = 0;
    uVar23 = uVar29;
    do {
      *(float *)((longlong)param_2 + uVar40 * 4) =
           *(float *)(param_1[5] + uVar40 * 4) * *(float *)(param_3 + uVar40 * 4) +
           *(float *)((longlong)param_2 + uVar40 * 4);
      *(float *)((longlong)param_2 + uVar23 * 4) =
           *(float *)(param_1[5] + uVar23 * 4) * *(float *)(param_3 + uVar40 * 4) +
           *(float *)((longlong)param_2 + uVar23 * 4);
      *(float *)((longlong)param_2 + uVar28 * 4) =
           *(float *)(param_1[5] + uVar28 * 4) * *(float *)(param_3 + uVar40 * 4) +
           *(float *)((longlong)param_2 + uVar28 * 4);
      uVar40 = uVar40 + 1;
      uVar28 = uVar28 + 1;
      uVar23 = uVar23 + 1;
    } while (uVar29 != uVar40);
  }
  if ((local_c0 ^ (ulonglong)auStack_908) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 18000aca0
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
  float *pfVar11;
  float fVar12;
  float fVar13;
  float fVar14;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  float fVar20;
  uint uVar21;
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
  longlong *plVar52;
  uint uVar53;
  int iVar54;
  int iVar55;
  int iVar56;
  int iVar57;
  longlong lVar58;
  ulonglong uVar59;
  ulonglong uVar60;
  uint uVar61;
  longlong lVar62;
  longlong lVar63;
  longlong lVar64;
  ulonglong uVar65;
  longlong lVar66;
  size_t sVar67;
  ulonglong uVar68;
  uint uVar69;
  ulonglong uVar70;
  longlong lVar71;
  void *pvVar72;
  longlong lVar73;
  int iVar74;
  int iVar75;
  void *pvVar76;
  ulonglong uVar77;
  longlong lVar78;
  bool bVar79;
  undefined1 auStack_81e8 [32];
  void *local_81c8;
  ulonglong local_81c0;
  ulonglong local_81b8;
  longlong local_81b0;
  ulonglong local_81a8;
  ulonglong local_81a0;
  void *local_8198;
  longlong local_8190;
  longlong local_8188;
  size_t local_8180;
  longlong *local_8178;
  void *local_8170;
  ulonglong local_8168;
  void *local_8160;
  ulonglong local_8158;
  ulonglong local_8150;
  ulonglong local_8148;
  longlong local_8140;
  void *local_8138;
  void *local_8130;
  void *local_8128;
  int local_811c;
  ulonglong local_8118;
  void *local_8110;
  ulonglong local_8108;
  longlong local_8100;
  size_t local_80f8;
  longlong local_80f0;
  ulonglong local_80e8;
  ulonglong local_80e0;
  ulonglong local_80d8;
  longlong local_80d0;
  ulonglong local_80c8;
  longlong local_80c0;
  ulonglong local_80b8;
  void *local_80b0;
  float local_80a8;
  float local_80a4;
  float afStack_80a0 [8192];
  ulonglong local_a0;
  undefined8 uStack_48;
  
                    /* 0xaca0  6  rnn_compute_conv2d_c */
  uStack_48 = 0x18000acb6;
  local_a0 = DAT_180582000 ^ (ulonglong)auStack_81e8;
  lVar63 = (longlong)(int)(*(int *)((longlong)param_1 + 0x1c) + (param_5 - 1)) *
           (longlong)(int)param_1[2];
  lVar64 = (longlong)(int)lVar63 * ((longlong)(int)param_1[3] + -1);
  sVar67 = lVar64 * 4;
  local_81c8 = param_2;
  memcpy(&local_80a8,param_3,sVar67);
  memcpy(&local_80a8 + lVar64,param_4,lVar63 * 4);
  memcpy(param_3,&local_80a8 + lVar63,sVar67);
  local_80f0 = *param_1;
  local_81b0 = param_1[1];
  uVar69 = *(uint *)((longlong)param_1 + 0x1c);
  local_81c0 = (ulonglong)uVar69;
  uVar53 = *(uint *)(param_1 + 3);
  uVar61 = *(uint *)((longlong)param_1 + 0x14);
  local_81a8 = (ulonglong)uVar61;
  uVar21 = *(uint *)(param_1 + 2);
  local_81b8 = (ulonglong)uVar21;
  local_8178 = param_1;
  if (uVar53 == 3 && uVar69 == 3) {
    if (0 < (int)uVar61) {
      sVar67 = (longlong)(int)param_5 << 2;
      if ((int)uVar21 < 1) {
        local_81c0 = (ulonglong)param_6;
        uVar65 = (ulonglong)(uVar61 & 7);
        if (uVar61 < 8) {
          uVar60 = 0;
        }
        else {
          lVar64 = local_81c0 * 0x20;
          lVar63 = local_81c0 * 4;
          uVar60 = 0;
          pvVar76 = local_81c8;
          do {
            memset(pvVar76,0,sVar67);
            memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
            pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
            uVar60 = uVar60 + 8;
            pvVar76 = (void *)((longlong)pvVar76 + lVar64);
          } while ((uVar61 & 0x7ffffff8) != uVar60);
        }
        if (uVar65 != 0) {
          pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_81c0 * 4);
          lVar63 = local_81c0 * 4;
          do {
            memset(pvVar76,0,sVar67);
            pvVar76 = (void *)((longlong)pvVar76 + lVar63);
            uVar65 = uVar65 - 1;
          } while (uVar65 != 0);
        }
      }
      else if ((int)param_5 < 1) {
        local_81c0 = (ulonglong)param_6;
        uVar65 = (ulonglong)(uVar61 & 7);
        if (uVar61 < 8) {
          uVar60 = 0;
        }
        else {
          lVar64 = local_81c0 * 0x20;
          lVar63 = local_81c0 * 4;
          uVar60 = 0;
          pvVar76 = local_81c8;
          do {
            memset(pvVar76,0,sVar67);
            memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
            pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
            uVar60 = uVar60 + 8;
            pvVar76 = (void *)((longlong)pvVar76 + lVar64);
          } while ((uVar61 & 0x7ffffff8) != uVar60);
        }
        if (uVar65 != 0) {
          pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_81c0 * 4);
          lVar63 = local_81c0 * 4;
          do {
            memset(pvVar76,0,sVar67);
            pvVar76 = (void *)((longlong)pvVar76 + lVar63);
            uVar65 = uVar65 - 1;
          } while (uVar65 != 0);
        }
      }
      else {
        uVar69 = param_5 + 2;
        local_81a0 = (ulonglong)param_6;
        local_8188 = local_81a0 * 4;
        local_8158 = (ulonglong)(uVar21 * 9);
        local_8140 = local_81b0 + 0x24;
        local_8160 = (void *)((longlong)local_81c8 + (ulonglong)param_5 * 4);
        local_8148 = (ulonglong)(param_5 & 0x7ffffffc);
        local_8128 = (void *)CONCAT44(local_8128._4_4_,uVar69);
        local_8168 = (ulonglong)(uVar21 * uVar69);
        local_8170 = (void *)CONCAT44(local_8170._4_4_,uVar21 * uVar69 * 2);
        uVar65 = 0;
        pvVar76 = local_81c8;
        local_8180 = sVar67;
        do {
          uVar60 = uVar65;
          local_8130 = (void *)((longlong)local_81c8 + local_8188 * uVar60);
          local_8198 = (void *)(local_8188 * uVar60 + (longlong)local_8160);
          local_8138 = (void *)(uVar60 * local_8158);
          memset((void *)((longlong)local_81c8 + uVar60 * local_81a0 * 4),0,local_8180);
          uVar53 = (int)local_81b8 * (int)uVar60;
          local_81c0 = (ulonglong)uVar53;
          lVar63 = (longlong)&local_80a4;
          uVar65 = local_8168 & 0xffffffff;
          lVar64 = (longlong)afStack_80a0;
          uVar70 = 0;
          iVar57 = (int)local_8170;
          do {
            lVar71 = (longlong)iVar57;
            lVar66 = lVar71 * 4;
            lVar73 = (longlong)(int)uVar65;
            lVar78 = lVar73 * 4;
            lVar62 = (longlong)(int)((uVar53 + (int)uVar70) * 9);
            if (param_5 < 4) {
              uVar59 = 0;
LAB_18000b110:
              do {
                *(float *)((longlong)pvVar76 + uVar59 * 4) =
                     *(float *)(local_81b0 + 0x20 + lVar62 * 4) * afStack_80a0[lVar71 + uVar59] +
                     *(float *)(local_81b0 + 0x1c + lVar62 * 4) *
                     afStack_80a0[lVar71 + uVar59 + 0xffffffffffffffff] +
                     *(float *)(local_81b0 + 0x18 + lVar62 * 4) * (&local_80a8)[lVar71 + uVar59] +
                     *(float *)(local_81b0 + 0x14 + lVar62 * 4) * afStack_80a0[lVar73 + uVar59] +
                     *(float *)(local_81b0 + 0x10 + lVar62 * 4) *
                     afStack_80a0[lVar73 + uVar59 + 0xffffffffffffffff] +
                     *(float *)(local_81b0 + 0xc + lVar62 * 4) * (&local_80a8)[lVar73 + uVar59] +
                     *(float *)(local_81b0 + 8 + lVar62 * 4) * *(float *)(lVar63 + (uVar59 + 1) * 4)
                     + *(float *)(local_81b0 + lVar62 * 4) * *(float *)(lVar63 + (uVar59 - 1) * 4) +
                       *(float *)(local_81b0 + 4 + lVar62 * 4) * *(float *)(lVar63 + uVar59 * 4) +
                     *(float *)((longlong)pvVar76 + uVar59 * 4);
                uVar59 = uVar59 + 1;
              } while (param_5 != uVar59);
            }
            else {
              lVar58 = (longlong)((int)uVar70 * 9 + (int)local_8138);
              if (local_8130 < (void *)(local_8140 + lVar58 * 4) &&
                  (void *)(local_81b0 + lVar58 * 4) < local_8198) {
                uVar59 = 0;
                goto LAB_18000b110;
              }
              fVar20 = *(float *)(local_81b0 + lVar62 * 4);
              fVar12 = *(float *)(local_81b0 + 4 + lVar62 * 4);
              fVar13 = *(float *)(local_81b0 + 8 + lVar62 * 4);
              fVar14 = *(float *)(local_81b0 + 0xc + lVar62 * 4);
              fVar15 = *(float *)(local_81b0 + 0x10 + lVar62 * 4);
              fVar16 = *(float *)(local_81b0 + 0x14 + lVar62 * 4);
              fVar17 = *(float *)(local_81b0 + 0x18 + lVar62 * 4);
              fVar18 = *(float *)(local_81b0 + 0x1c + lVar62 * 4);
              fVar19 = *(float *)(local_81b0 + 0x20 + lVar62 * 4);
              lVar58 = 0;
              do {
                pfVar4 = (float *)(lVar64 + lVar58 + -8);
                fVar22 = pfVar4[1];
                fVar23 = pfVar4[2];
                fVar24 = pfVar4[3];
                pfVar5 = (float *)(lVar64 + lVar58 + -4);
                fVar25 = pfVar5[1];
                fVar26 = pfVar5[2];
                fVar27 = pfVar5[3];
                pfVar3 = (float *)(lVar64 + lVar58);
                fVar28 = pfVar3[1];
                fVar29 = pfVar3[2];
                fVar30 = pfVar3[3];
                pfVar6 = (float *)((longlong)&local_80a8 + lVar58 + lVar78);
                fVar31 = pfVar6[1];
                fVar32 = pfVar6[2];
                fVar33 = pfVar6[3];
                pfVar7 = (float *)((longlong)afStack_80a0 + lVar58 + lVar78 + 0xfffffffffffffffcU);
                fVar34 = pfVar7[1];
                fVar35 = pfVar7[2];
                fVar36 = pfVar7[3];
                pfVar11 = (float *)((longlong)afStack_80a0 + lVar58 + lVar78);
                fVar37 = pfVar11[1];
                fVar38 = pfVar11[2];
                fVar39 = pfVar11[3];
                pfVar8 = (float *)((longlong)&local_80a8 + lVar58 + lVar66);
                fVar40 = pfVar8[1];
                fVar41 = pfVar8[2];
                fVar42 = pfVar8[3];
                pfVar9 = (float *)((longlong)afStack_80a0 + lVar58 + lVar66 + 0xfffffffffffffffcU);
                fVar43 = pfVar9[1];
                fVar44 = pfVar9[2];
                fVar45 = pfVar9[3];
                pfVar10 = (float *)((longlong)afStack_80a0 + lVar58 + lVar66);
                fVar46 = pfVar10[1];
                fVar47 = pfVar10[2];
                fVar48 = pfVar10[3];
                pfVar1 = (float *)((longlong)pvVar76 + lVar58);
                fVar49 = pfVar1[1];
                fVar50 = pfVar1[2];
                fVar51 = pfVar1[3];
                pfVar2 = (float *)((longlong)pvVar76 + lVar58);
                *pfVar2 = *pfVar1 + *pfVar10 * fVar19 +
                                    *pfVar9 * fVar18 +
                                    *pfVar8 * fVar17 +
                                    *pfVar11 * fVar16 +
                                    *pfVar7 * fVar15 +
                                    *pfVar6 * fVar14 +
                                    *pfVar3 * fVar13 + *pfVar5 * fVar12 + *pfVar4 * fVar20;
                pfVar2[1] = fVar49 + fVar46 * fVar19 +
                                     fVar43 * fVar18 +
                                     fVar40 * fVar17 +
                                     fVar37 * fVar16 +
                                     fVar34 * fVar15 +
                                     fVar31 * fVar14 +
                                     fVar28 * fVar13 + fVar25 * fVar12 + fVar22 * fVar20;
                pfVar2[2] = fVar50 + fVar47 * fVar19 +
                                     fVar44 * fVar18 +
                                     fVar41 * fVar17 +
                                     fVar38 * fVar16 +
                                     fVar35 * fVar15 +
                                     fVar32 * fVar14 +
                                     fVar29 * fVar13 + fVar26 * fVar12 + fVar23 * fVar20;
                pfVar2[3] = fVar51 + fVar48 * fVar19 +
                                     fVar45 * fVar18 +
                                     fVar42 * fVar17 +
                                     fVar39 * fVar16 +
                                     fVar36 * fVar15 +
                                     fVar33 * fVar14 +
                                     fVar30 * fVar13 + fVar27 * fVar12 + fVar24 * fVar20;
                lVar58 = lVar58 + 0x10;
              } while ((ulonglong)(param_5 >> 2 & 0x1fffffff) << 4 != lVar58);
              uVar59 = local_8148;
              if ((uint)local_8148 != param_5) goto LAB_18000b110;
            }
            uVar70 = uVar70 + 1;
            lVar64 = lVar64 + (ulonglong)uVar69 * 4;
            uVar65 = (ulonglong)(uint)((int)uVar65 + (int)local_8128);
            iVar57 = iVar57 + (int)local_8128;
            lVar63 = lVar63 + (ulonglong)uVar69 * 4;
          } while (uVar70 != local_81b8);
          pvVar76 = (void *)((longlong)pvVar76 + local_8188);
          uVar65 = uVar60 + 1;
          local_8150 = uVar60;
        } while (uVar60 + 1 != local_81a8);
      }
    }
  }
  else if (0 < (int)uVar61) {
    sVar67 = (longlong)(int)param_5 << 2;
    if ((int)uVar21 < 1) {
      local_81c0 = (ulonglong)param_6;
      uVar65 = (ulonglong)(uVar61 & 7);
      if (uVar61 < 8) {
        uVar60 = 0;
      }
      else {
        lVar64 = local_81c0 * 0x20;
        lVar63 = local_81c0 * 4;
        uVar60 = 0;
        pvVar76 = local_81c8;
        do {
          memset(pvVar76,0,sVar67);
          memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
          pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
          memset(pvVar72,0,sVar67);
          pvVar72 = (void *)((longlong)pvVar72 + lVar63);
          memset(pvVar72,0,sVar67);
          pvVar72 = (void *)((longlong)pvVar72 + lVar63);
          memset(pvVar72,0,sVar67);
          pvVar72 = (void *)((longlong)pvVar72 + lVar63);
          memset(pvVar72,0,sVar67);
          pvVar72 = (void *)((longlong)pvVar72 + lVar63);
          memset(pvVar72,0,sVar67);
          memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
          uVar60 = uVar60 + 8;
          pvVar76 = (void *)((longlong)pvVar76 + lVar64);
        } while ((uVar61 & 0x7ffffff8) != uVar60);
      }
      if (uVar65 != 0) {
        pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_81c0 * 4);
        lVar63 = local_81c0 * 4;
        do {
          memset(pvVar76,0,sVar67);
          pvVar76 = (void *)((longlong)pvVar76 + lVar63);
          uVar65 = uVar65 - 1;
        } while (uVar65 != 0);
      }
    }
    else {
      local_81a0 = (ulonglong)uVar53;
      local_8190 = (longlong)param_6;
      if ((int)uVar53 < 1) {
        uVar65 = (ulonglong)(uVar61 & 7);
        if (uVar61 < 8) {
          uVar60 = 0;
        }
        else {
          lVar64 = local_8190 * 0x20;
          lVar63 = local_8190 * 4;
          uVar60 = 0;
          pvVar76 = local_81c8;
          do {
            memset(pvVar76,0,sVar67);
            memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
            pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
            uVar60 = uVar60 + 8;
            pvVar76 = (void *)((longlong)pvVar76 + lVar64);
          } while ((uVar61 & 0x7ffffff8) != uVar60);
        }
        if (uVar65 != 0) {
          pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_8190 * 4);
          lVar63 = local_8190 * 4;
          do {
            memset(pvVar76,0,sVar67);
            pvVar76 = (void *)((longlong)pvVar76 + lVar63);
            uVar65 = uVar65 - 1;
          } while (uVar65 != 0);
        }
      }
      else if ((int)uVar69 < 1) {
        uVar65 = (ulonglong)(uVar61 & 7);
        if (uVar61 < 8) {
          uVar60 = 0;
        }
        else {
          lVar64 = local_8190 * 0x20;
          lVar63 = local_8190 * 4;
          uVar60 = 0;
          pvVar76 = local_81c8;
          do {
            memset(pvVar76,0,sVar67);
            memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
            pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
            uVar60 = uVar60 + 8;
            pvVar76 = (void *)((longlong)pvVar76 + lVar64);
          } while ((uVar61 & 0x7ffffff8) != uVar60);
        }
        if (uVar65 != 0) {
          pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_8190 * 4);
          lVar63 = local_8190 * 4;
          do {
            memset(pvVar76,0,sVar67);
            pvVar76 = (void *)((longlong)pvVar76 + lVar63);
            uVar65 = uVar65 - 1;
          } while (uVar65 != 0);
        }
      }
      else if ((int)param_5 < 1) {
        uVar65 = (ulonglong)(uVar61 & 7);
        if (uVar61 < 8) {
          uVar60 = 0;
        }
        else {
          lVar64 = local_8190 * 0x20;
          lVar63 = local_8190 * 4;
          uVar60 = 0;
          pvVar76 = local_81c8;
          do {
            memset(pvVar76,0,sVar67);
            memset((void *)((longlong)pvVar76 + lVar63),0,sVar67);
            pvVar72 = (void *)((longlong)pvVar76 + lVar63 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            pvVar72 = (void *)((longlong)pvVar72 + lVar63);
            memset(pvVar72,0,sVar67);
            memset((void *)((longlong)pvVar72 + lVar63),0,sVar67);
            uVar60 = uVar60 + 8;
            pvVar76 = (void *)((longlong)pvVar76 + lVar64);
          } while ((uVar61 & 0x7ffffff8) != uVar60);
        }
        if (uVar65 != 0) {
          pvVar76 = (void *)((longlong)local_81c8 + uVar60 * local_8190 * 4);
          lVar63 = local_8190 * 4;
          do {
            memset(pvVar76,0,sVar67);
            pvVar76 = (void *)((longlong)pvVar76 + lVar63);
            uVar65 = uVar65 - 1;
          } while (uVar65 != 0);
        }
      }
      else {
        uVar61 = (param_5 - 1) + uVar69;
        uVar60 = (ulonglong)param_5;
        uVar65 = uVar60 - 1;
        local_80e0 = (ulonglong)uVar61;
        local_8188 = CONCAT44(local_8188._4_4_,uVar61);
        local_80b8 = (ulonglong)(uVar21 * uVar61);
        local_8100 = local_8190 * 4;
        local_80e8 = (ulonglong)(uVar53 * uVar69);
        local_8108 = (ulonglong)(uVar53 * uVar69 * uVar21);
        local_80c0 = local_81b0 + local_81c0 * 4;
        local_8110 = (void *)((longlong)local_81c8 + uVar60 * 4);
        uVar69 = param_5 & 0x7ffffff8;
        lVar63 = (longlong)local_81c8 + 0x10;
        local_8130 = (void *)(uVar65 >> 0x20);
        uVar70 = 0;
        pvVar76 = local_81c8;
        local_80f8 = sVar67;
        do {
          uVar68 = local_80b8;
          uVar59 = local_80e0;
          local_80b0 = (void *)((longlong)local_81c8 + local_8100 * uVar70);
          local_8170 = (void *)(local_8100 * uVar70 + (longlong)local_8110);
          local_80d0 = uVar70 * local_8108;
          local_8138 = (void *)((longlong)local_81c8 + uVar70 * local_8190 * 4);
          local_8118 = uVar70;
          memset(local_8138,0,local_80f8);
          iVar54 = (int)local_81b8;
          local_80d8 = (ulonglong)(uint)(iVar54 * (int)local_8118);
          sVar67 = 0;
          uVar70 = 0;
          iVar57 = 1;
          do {
            local_811c = iVar57;
            local_80c8 = uVar70;
            local_8180 = sVar67;
            local_8158 = local_8180 * local_80e8 + local_80d0;
            local_8160 = (void *)(local_8180 * uVar59);
            uVar53 = (iVar54 * (int)local_8118 + (int)local_8180) * (int)local_81a0;
            local_8168 = (ulonglong)uVar53;
            uVar70 = 0;
            uVar59 = local_80c8;
            iVar57 = local_811c;
            do {
              local_8148 = uVar59;
              local_8150 = uVar70;
              iVar55 = (int)local_8150;
              lVar66 = (longlong)(iVar55 * (int)local_81c0 + (int)local_8158);
              pvVar72 = (void *)(local_81b0 + lVar66 * 4);
              local_8128 = (void *)(local_8150 * uVar68 + (longlong)local_8160);
              lVar64 = local_81b0 + (longlong)(int)((uVar53 + iVar55) * (int)local_81c0) * 4;
              bVar79 = local_80b0 < (void *)(local_80c0 + lVar66 * 4);
              local_8198 = (void *)CONCAT71((int7)((ulonglong)pvVar72 >> 8),
                                            pvVar72 < local_8170 && bVar79);
              local_8140 = CONCAT44(local_8140._4_4_,iVar57);
              uVar59 = 0;
              uVar70 = local_8148;
              iVar56 = iVar57;
              do {
                if ((param_5 < 8) ||
                   (iVar74 = (int)local_8128 + (int)uVar59,
                   (local_8130 != (void *)0x0 || pvVar72 < local_8170 && bVar79) ||
                   iVar74 + (int)uVar65 < iVar74)) {
                  uVar68 = 0;
LAB_18000b568:
                  uVar77 = uVar68;
                  if ((param_5 & 1) != 0) {
                    *(float *)((longlong)local_8138 + uVar68 * 4) =
                         *(float *)(lVar64 + uVar59 * 4) *
                         (&local_80a8)
                         [(iVar54 * iVar55 + (int)local_8180) * (int)local_8188 + (int)uVar59 +
                          (int)uVar68] + *(float *)((longlong)local_8138 + uVar68 * 4);
                    uVar77 = uVar68 | 1;
                  }
                  if (uVar68 != uVar65) {
                    lVar66 = uVar77 + 1;
                    iVar74 = (int)uVar70 + (int)uVar77;
                    iVar75 = (int)uVar77 + iVar56;
                    do {
                      *(float *)((longlong)pvVar76 + lVar66 * 4 + -4) =
                           *(float *)(lVar64 + uVar59 * 4) * (&local_80a8)[iVar74] +
                           *(float *)((longlong)pvVar76 + lVar66 * 4 + -4);
                      *(float *)((longlong)pvVar76 + lVar66 * 4) =
                           *(float *)(lVar64 + uVar59 * 4) * (&local_80a8)[iVar75] +
                           *(float *)((longlong)pvVar76 + lVar66 * 4);
                      lVar78 = lVar66 - uVar60;
                      lVar66 = lVar66 + 2;
                      iVar74 = iVar74 + 2;
                      iVar75 = iVar75 + 2;
                    } while (lVar78 != -1);
                  }
                }
                else {
                  fVar20 = *(float *)(lVar64 + uVar59 * 4);
                  uVar68 = 0;
                  do {
                    lVar66 = (longlong)((int)uVar70 + (int)uVar68);
                    fVar12 = afStack_80a0[lVar66 + 0xffffffffffffffff];
                    fVar13 = afStack_80a0[lVar66];
                    fVar14 = afStack_80a0[lVar66 + 1];
                    fVar15 = afStack_80a0[lVar66 + 2];
                    fVar16 = afStack_80a0[lVar66 + 3];
                    fVar17 = afStack_80a0[lVar66 + 4];
                    fVar18 = afStack_80a0[lVar66 + 5];
                    pfVar10 = (float *)(lVar63 + -0x10 + uVar68 * 4);
                    fVar19 = pfVar10[1];
                    fVar22 = pfVar10[2];
                    fVar23 = pfVar10[3];
                    pfVar3 = (float *)(lVar63 + uVar68 * 4);
                    fVar24 = *pfVar3;
                    fVar25 = pfVar3[1];
                    fVar26 = pfVar3[2];
                    fVar27 = pfVar3[3];
                    pfVar3 = (float *)(lVar63 + -0x10 + uVar68 * 4);
                    *pfVar3 = (&local_80a8)[lVar66] * fVar20 + *pfVar10;
                    pfVar3[1] = fVar12 * fVar20 + fVar19;
                    pfVar3[2] = fVar13 * fVar20 + fVar22;
                    pfVar3[3] = fVar14 * fVar20 + fVar23;
                    pfVar3 = (float *)(lVar63 + uVar68 * 4);
                    *pfVar3 = fVar15 * fVar20 + fVar24;
                    pfVar3[1] = fVar16 * fVar20 + fVar25;
                    pfVar3[2] = fVar17 * fVar20 + fVar26;
                    pfVar3[3] = fVar18 * fVar20 + fVar27;
                    uVar68 = uVar68 + 8;
                  } while (uVar69 != uVar68);
                  uVar68 = (ulonglong)uVar69;
                  if (uVar69 != param_5) goto LAB_18000b568;
                }
                uVar59 = uVar59 + 1;
                uVar70 = uVar70 + 1;
                iVar56 = iVar56 + 1;
              } while (uVar59 != local_81c0);
              iVar57 = iVar57 + (int)local_80b8;
              uVar70 = local_8150 + 1;
              uVar68 = local_80b8;
              uVar59 = local_8148 + local_80b8;
            } while (local_8150 + 1 != local_81a0);
            sVar67 = local_8180 + 1;
            uVar59 = local_80e0;
            uVar70 = local_80c8 + local_80e0;
            iVar57 = local_811c + (int)local_8188;
          } while ((int)(local_8180 + 1) != iVar54);
          uVar70 = local_8118 + 1;
          lVar63 = lVar63 + local_8100;
          pvVar76 = (void *)((longlong)pvVar76 + local_8100);
        } while (uVar70 != local_81a8);
      }
    }
  }
  plVar52 = local_8178;
  iVar57 = *(int *)((longlong)local_8178 + 0x14);
  if (local_80f0 == 0) {
LAB_18000bedc:
    if (iVar57 < 1) goto LAB_18000bf35;
  }
  else {
    uVar65 = (ulonglong)param_5;
    if (iVar57 < 1) goto LAB_18000bf35;
    if (0 < (int)param_5) {
      lVar64 = (longlong)param_6 * 4;
      uVar69 = param_5 & 0x7ffffff8;
      uVar60 = (ulonglong)(param_5 & 3);
      lVar63 = (longlong)local_81c8 + 0x10;
      lVar66 = 0;
      pvVar76 = local_81c8;
      do {
        uVar70 = uVar60;
        if (param_5 < 8) {
          uVar59 = 0;
          uVar68 = 0;
joined_r0x00018000be46:
          for (; uVar70 != 0; uVar70 = uVar70 - 1) {
            *(float *)((longlong)pvVar76 + uVar59 * 4) =
                 *(float *)(local_80f0 + lVar66 * 4) + *(float *)((longlong)pvVar76 + uVar59 * 4);
            uVar59 = uVar59 + 1;
          }
          if (uVar68 - uVar65 < 0xfffffffffffffffd) {
            do {
              *(float *)((longlong)pvVar76 + uVar59 * 4) =
                   *(float *)(local_80f0 + lVar66 * 4) + *(float *)((longlong)pvVar76 + uVar59 * 4);
              *(float *)((longlong)pvVar76 + uVar59 * 4 + 4) =
                   *(float *)(local_80f0 + lVar66 * 4) +
                   *(float *)((longlong)pvVar76 + uVar59 * 4 + 4);
              *(float *)((longlong)pvVar76 + uVar59 * 4 + 8) =
                   *(float *)(local_80f0 + lVar66 * 4) +
                   *(float *)((longlong)pvVar76 + uVar59 * 4 + 8);
              *(float *)((longlong)pvVar76 + uVar59 * 4 + 0xc) =
                   *(float *)(local_80f0 + lVar66 * 4) +
                   *(float *)((longlong)pvVar76 + uVar59 * 4 + 0xc);
              uVar59 = uVar59 + 4;
            } while (uVar65 != uVar59);
          }
        }
        else {
          if ((void *)(lVar64 * lVar66 + (longlong)local_81c8) <
              (void *)(local_80f0 + 4 + lVar66 * 4) &&
              (ulonglong)(lVar66 * 4 + local_80f0) <
              (longlong)local_81c8 + lVar64 * lVar66 + uVar65 * 4) {
            uVar59 = 0;
            uVar68 = 0;
            goto joined_r0x00018000be46;
          }
          fVar20 = *(float *)(local_80f0 + lVar66 * 4);
          lVar78 = 0;
          do {
            pfVar10 = (float *)(lVar63 + -0x10 + lVar78);
            fVar12 = pfVar10[1];
            fVar13 = pfVar10[2];
            fVar14 = pfVar10[3];
            pfVar3 = (float *)(lVar63 + lVar78);
            fVar15 = *pfVar3;
            fVar16 = pfVar3[1];
            fVar17 = pfVar3[2];
            fVar18 = pfVar3[3];
            pfVar3 = (float *)(lVar63 + -0x10 + lVar78);
            *pfVar3 = *pfVar10 + fVar20;
            pfVar3[1] = fVar12 + fVar20;
            pfVar3[2] = fVar13 + fVar20;
            pfVar3[3] = fVar14 + fVar20;
            pfVar3 = (float *)(lVar63 + lVar78);
            *pfVar3 = fVar15 + fVar20;
            pfVar3[1] = fVar16 + fVar20;
            pfVar3[2] = fVar17 + fVar20;
            pfVar3[3] = fVar18 + fVar20;
            lVar78 = lVar78 + 0x20;
          } while ((ulonglong)(param_5 >> 3 & 0xfffffff) << 5 != lVar78);
          uVar59 = (ulonglong)uVar69;
          uVar68 = (ulonglong)uVar69;
          if (uVar69 != param_5) goto joined_r0x00018000be46;
        }
        lVar66 = lVar66 + 1;
        iVar57 = *(int *)((longlong)local_8178 + 0x14);
        lVar63 = lVar63 + lVar64;
        pvVar76 = (void *)((longlong)pvVar76 + lVar64);
      } while (lVar66 < iVar57);
      goto LAB_18000bedc;
    }
  }
  lVar63 = 0;
  pvVar76 = local_81c8;
  do {
    rnn_compute_activation_c(pvVar76,pvVar76,param_5,param_7);
    lVar63 = lVar63 + 1;
    pvVar76 = (void *)((longlong)pvVar76 + (longlong)param_6 * 4);
  } while (lVar63 < *(int *)((longlong)plVar52 + 0x14));
LAB_18000bf35:
  if ((local_a0 ^ (ulonglong)auStack_81e8) != DAT_180582000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000e470();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000bf90
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
  
                    /* 0xbf90  25  rnn_parse_weights */
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
LAB_18000c082:
        free((void *)*param_1);
        uVar6 = 0xffffffff;
        goto LAB_18000c0a3;
      }
      iVar1 = *(int *)(param_2 + 0xc);
      iVar2 = *(int *)(param_2 + 0x10);
      if ((((iVar2 < iVar1) || (iVar5 = param_3 - 0x40, param_3 = iVar5 - iVar2, iVar5 < iVar2)) ||
          (iVar1 < 0)) || ((*(char *)(param_2 + 0x3f) != '\0' || (iVar1 == 0)))) goto LAB_18000c082;
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
LAB_18000c0a3:
  *param_1 = 0;
  return uVar6;
}



/* ========================================================================
   ENTRY: 18000c0c0
   NAME : rnn_linear_init
   SIG  : undefined rnn_linear_init(void)
   ======================================================================== */

undefined8
rnn_linear_init(longlong *param_1,longlong *param_2,char *param_3,char *param_4,char *param_5,
               char *param_6,char *param_7,char *param_8,char *param_9,int param_10,int param_11)

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
  
                    /* 0xc0c0  23  rnn_linear_init */
  param_1[4] = 0;
  param_1[5] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  *param_1 = 0;
  param_1[1] = 0;
  param_1[6] = 0;
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
    *param_1 = lVar10;
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
            param_1[1] = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000c1b7;
          }
          break;
        }
        pcVar4 = (char *)plVar7[3];
        plVar7 = plVar7 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    param_1[1] = 0;
    return 1;
  }
LAB_18000c1b7:
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
              param_1[2] = lVar10;
              if (lVar10 == 0) {
                return 1;
              }
              goto LAB_18000c360;
            }
            break;
          }
          pcVar4 = (char *)plVar6[3];
          plVar6 = plVar6 + 3;
        } while (pcVar4 != (char *)0x0);
      }
      param_1[2] = 0;
      return 1;
    }
LAB_18000c360:
    if (param_6 == (char *)0x0) goto LAB_18000c423;
    pcVar4 = (char *)*param_2;
    if (pcVar4 == (char *)0x0) goto LAB_18000c41c;
    lVar10 = 0;
    do {
      iVar2 = strcmp(pcVar4,param_6);
      if (iVar2 == 0) {
        if (*(int *)((longlong)plVar7 + 0xc) != param_11 * param_10 * 4) {
          param_1[3] = 0;
          return 1;
        }
        goto LAB_18000c3b9;
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
        if (iVar3 <= (int)uVar1) goto LAB_18000c2e1;
        puVar5 = puVar5 + 1;
        uVar8 = uVar1;
        if (0 < (int)uVar1) {
          do {
            if ((param_10 <= (int)(*puVar5 + 3)) || ((*puVar5 & 3) != 0)) goto LAB_18000c2e1;
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
LAB_18000c2e1:
      param_1[4] = 0;
      return 1;
    }
    lVar10 = plVar6[2];
    param_1[4] = lVar10;
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
              param_1[2] = lVar10;
              if (lVar10 == 0) {
                return 1;
              }
              goto LAB_18000c3ea;
            }
            break;
          }
          pcVar4 = (char *)plVar6[3];
          plVar6 = plVar6 + 3;
        } while (pcVar4 != (char *)0x0);
      }
      param_1[2] = 0;
      return 1;
    }
LAB_18000c3ea:
    if (param_6 == (char *)0x0) goto LAB_18000c423;
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
LAB_18000c3ff:
      iVar2 = strcmp(pcVar4,param_6);
      if (iVar2 != 0) goto code_r0x00018000c40f;
      if (*(int *)((longlong)plVar7 + 0xc) != iVar9 * 0x80) {
        param_1[3] = 0;
        return 1;
      }
LAB_18000c3b9:
      lVar10 = plVar7[2];
      goto LAB_18000c41f;
    }
LAB_18000c41c:
    lVar10 = 0;
  }
LAB_18000c41f:
  param_1[3] = lVar10;
LAB_18000c423:
  if (param_8 == (char *)0x0) {
LAB_18000c480:
    if (param_5 == (char *)0x0) {
LAB_18000c4ec:
      *(int *)(param_1 + 7) = param_10;
      *(int *)((longlong)param_1 + 0x3c) = param_11;
      return 0;
    }
    pcVar4 = (char *)*param_2;
    if (pcVar4 != (char *)0x0) {
      do {
        iVar2 = strcmp(pcVar4,param_9);
        if (iVar2 == 0) {
          if (*(int *)((longlong)param_2 + 0xc) == param_11 * 4) {
            lVar10 = param_2[2];
            param_1[6] = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000c4ec;
          }
          break;
        }
        pcVar4 = (char *)param_2[3];
        param_2 = param_2 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    param_1[6] = 0;
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
            param_1[5] = lVar10;
            if (lVar10 == 0) {
              return 1;
            }
            goto LAB_18000c480;
          }
          break;
        }
        pcVar4 = (char *)plVar7[3];
        plVar7 = plVar7 + 3;
      } while (pcVar4 != (char *)0x0);
    }
    param_1[5] = 0;
  }
  return 1;
code_r0x00018000c40f:
  pcVar4 = (char *)plVar7[3];
  plVar7 = plVar7 + 3;
  if (pcVar4 == (char *)0x0) goto LAB_18000c41c;
  goto LAB_18000c3ff;
}



/* ========================================================================
   ENTRY: 18000c540
   NAME : rnn_conv2d_init
   SIG  : undefined rnn_conv2d_init(void)
   ======================================================================== */

undefined4
rnn_conv2d_init(longlong *param_1,longlong *param_2,char *param_3,char *param_4,int param_5,
               int param_6,int param_7,int param_8)

{
  longlong *plVar1;
  int iVar2;
  char *pcVar3;
  undefined4 uVar4;
  longlong lVar5;
  
                    /* 0xc540  13  rnn_conv2d_init */
  *param_1 = 0;
  param_1[1] = 0;
  if (param_3 == (char *)0x0) {
LAB_18000c5ca:
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
              param_1[1] = 0;
              return 1;
            }
            lVar5 = param_2[2];
            break;
          }
          pcVar3 = (char *)param_2[3];
          param_2 = param_2 + 3;
        } while (pcVar3 != (char *)0x0);
      }
      param_1[1] = lVar5;
    }
    *(int *)(param_1 + 2) = param_5;
    *(int *)((longlong)param_1 + 0x14) = param_6;
    *(int *)(param_1 + 3) = param_7;
    *(int *)((longlong)param_1 + 0x1c) = param_8;
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
          *param_1 = lVar5;
          if (lVar5 == 0) {
            return 1;
          }
          goto LAB_18000c5ca;
        }
        pcVar3 = (char *)plVar1[3];
        plVar1 = plVar1 + 3;
      } while (pcVar3 != (char *)0x0);
    }
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000c680
   NAME : rnn_pitch_downsample
   SIG  : undefined rnn_pitch_downsample(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_pitch_downsample(longlong *param_1,float *param_2,int param_3,int param_4)

{
  float *pfVar1;
  longlong lVar2;
  longlong lVar3;
  ulonglong uVar4;
  uint uVar5;
  ulonglong uVar6;
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
  undefined1 auStack_158 [32];
  undefined4 local_138;
  uint local_130;
  undefined1 local_128 [16];
  undefined1 local_118 [16];
  undefined1 local_108 [16];
  float local_f8;
  float local_f4;
  float local_f0;
  float local_ec;
  float local_e8;
  float local_e4;
  float fStack_e0;
  float fStack_dc;
  float fStack_d8;
  ulonglong local_d0;
  
                    /* 0xc680  26  rnn_pitch_downsample */
  fVar8 = DAT_1800104e0;
  local_d0 = DAT_180582000 ^ (ulonglong)auStack_158;
  uVar5 = param_3 >> 1;
  uVar6 = (ulonglong)uVar5;
  if (1 < (int)uVar5) {
    lVar3 = 1;
    if (uVar5 != 2) {
      do {
        lVar2 = *param_1;
        param_2[lVar3] =
             ((*(float *)(lVar2 + -4 + lVar3 * 8) + *(float *)(lVar2 + 4 + lVar3 * 8)) * fVar8 +
             *(float *)(lVar2 + lVar3 * 8)) * fVar8;
        lVar2 = *param_1;
        param_2[lVar3 + 1] =
             ((*(float *)(lVar2 + 4 + lVar3 * 8) + *(float *)(lVar2 + 0xc + lVar3 * 8)) * fVar8 +
             *(float *)(lVar2 + 8 + lVar3 * 8)) * fVar8;
        lVar2 = lVar3 - (uVar6 - 1 & 0xfffffffffffffffe);
        lVar3 = lVar3 + 2;
      } while (lVar2 != -1);
    }
    if ((uVar6 - 1 & 1) != 0) {
      lVar2 = *param_1;
      param_2[lVar3] =
           ((*(float *)(lVar2 + -4 + lVar3 * 8) + *(float *)(lVar2 + 4 + lVar3 * 8)) * DAT_1800104e0
           + *(float *)(lVar2 + lVar3 * 8)) * DAT_1800104e0;
    }
  }
  fVar8 = DAT_1800104e0;
  fVar7 = (((float *)*param_1)[1] * DAT_1800104e0 + *(float *)*param_1) * DAT_1800104e0;
  *param_2 = fVar7;
  if (param_4 == 2) {
    if (1 < (int)uVar5) {
      uVar4 = 1;
      do {
        lVar3 = param_1[1];
        param_2[uVar4] =
             ((*(float *)(lVar3 + -4 + uVar4 * 8) + *(float *)(lVar3 + 4 + uVar4 * 8)) * fVar8 +
             *(float *)(lVar3 + uVar4 * 8)) * fVar8 + param_2[uVar4];
        uVar4 = uVar4 + 1;
      } while (uVar6 != uVar4);
    }
    *param_2 = (((float *)param_1[1])[1] * fVar8 + *(float *)param_1[1]) * fVar8 + fVar7;
  }
  local_138 = 4;
  local_130 = uVar5;
  rnn_autocorr(param_2,&local_e8,0,0);
  local_e8 = local_e8 * _DAT_1800104e4;
  local_e4 = _DAT_1800104f0 * local_e4 * _DAT_180010500 + local_e4;
  fStack_e0 = _UNK_1800104f4 * fStack_e0 * _UNK_180010504 + fStack_e0;
  fStack_dc = _UNK_1800104f8 * fStack_dc * _UNK_180010508 + fStack_dc;
  fStack_d8 = _UNK_1800104fc * fStack_d8 * _UNK_18001050c + fStack_d8;
  rnn_lpc(&local_f8,&local_e8,4);
  if (0 < (int)uVar5) {
    fVar16 = local_f8 * _DAT_180010510 + DAT_180010520;
    fVar14 = local_f8 * _DAT_180010510 * DAT_180010520 + local_f4 * _DAT_180010514;
    fVar8 = local_f4 * _DAT_180010514 * DAT_180010520 + local_f0 * _DAT_180010518;
    fVar7 = local_f0 * _DAT_180010518 * DAT_180010520 + local_ec * _DAT_18001051c;
    fVar15 = local_ec * _DAT_18001051c * DAT_180010520;
    if (uVar5 < 4) {
      uVar4 = 0;
      fVar9 = 0.0;
      fVar10 = 0.0;
      fVar11 = 0.0;
      fVar12 = 0.0;
      fVar13 = 0.0;
    }
    else {
      uVar4 = (ulonglong)(uVar5 & 0x7ffffffc);
      local_128 = ZEXT416((uint)fVar16);
      local_108 = ZEXT416((uint)fVar14);
      local_118 = ZEXT416((uint)fVar15);
      lVar3 = 0;
      fVar13 = 0.0;
      fVar17 = 0.0;
      fVar18 = 0.0;
      fVar19 = 0.0;
      fVar20 = 0.0;
      do {
        fVar9 = fVar13;
        pfVar1 = (float *)((longlong)param_2 + lVar3);
        fVar10 = *pfVar1;
        fVar11 = pfVar1[1];
        fVar12 = pfVar1[2];
        fVar13 = pfVar1[3];
        pfVar1 = (float *)((longlong)param_2 + lVar3);
        *pfVar1 = fVar7 * fVar18 + fVar8 * fVar19 + fVar14 * fVar20 + fVar16 * fVar9 + fVar10 +
                  fVar17 * fVar15;
        pfVar1[1] = fVar7 * fVar19 + fVar8 * fVar20 + fVar14 * fVar9 + fVar16 * fVar10 + fVar11 +
                    fVar18 * fVar15;
        pfVar1[2] = fVar7 * fVar20 + fVar8 * fVar9 + fVar14 * fVar10 + fVar16 * fVar11 + fVar12 +
                    fVar19 * fVar15;
        pfVar1[3] = fVar7 * fVar9 + fVar8 * fVar10 + fVar14 * fVar11 + fVar16 * fVar12 + fVar13 +
                    fVar20 * fVar15;
        lVar3 = lVar3 + 0x10;
        fVar17 = fVar9;
        fVar18 = fVar10;
        fVar19 = fVar11;
        fVar20 = fVar12;
      } while ((ulonglong)(uVar5 >> 2 & 0x1fffffff) << 4 != lVar3);
      if ((uVar5 & 0x7ffffffc) == uVar5) goto LAB_18000cacd;
    }
    do {
      fVar17 = param_2[uVar4];
      param_2[uVar4] =
           fVar9 * fVar15 +
           fVar7 * fVar10 + fVar8 * fVar11 + fVar14 * fVar12 + fVar16 * fVar13 + fVar17;
      uVar4 = uVar4 + 1;
      fVar9 = fVar10;
      fVar10 = fVar11;
      fVar11 = fVar12;
      fVar12 = fVar13;
      fVar13 = fVar17;
    } while (uVar6 != uVar4);
  }
LAB_18000cacd:
  if ((local_d0 ^ (ulonglong)auStack_158) != DAT_180582000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000e470();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000cb50
   NAME : rnn_pitch_xcorr
   SIG  : undefined rnn_pitch_xcorr(void)
   ======================================================================== */

void rnn_pitch_xcorr(float *param_1,longlong param_2,longlong param_3,uint param_4,uint param_5)

{
  float fVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float *pfVar5;
  int iVar6;
  uint uVar7;
  uint uVar8;
  longlong lVar9;
  ulonglong uVar10;
  ulonglong uVar11;
  float *pfVar12;
  ulonglong uVar13;
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
  
                    /* 0xcb50  29  rnn_pitch_xcorr */
  uVar10 = 0;
  if (3 < (int)param_5) {
    uVar8 = param_4 & 0x7ffffffc;
    uVar10 = 0;
    do {
      fVar16 = *(float *)(param_2 + uVar10 * 4);
      pfVar5 = (float *)(param_2 + uVar10 * 4 + 0xc);
      pfVar12 = (float *)(param_2 + 4 + uVar10 * 4);
      fVar15 = *pfVar12;
      fVar18 = pfVar12[1];
      fVar20 = 0.0;
      fVar21 = 0.0;
      fVar22 = 0.0;
      fVar23 = 0.0;
      pfVar12 = param_1;
      if ((int)param_4 < 4) {
        fVar24 = 0.0;
        uVar7 = 0;
        if (0 < (int)param_4) goto LAB_18000cc5d;
      }
      else {
        iVar6 = 0;
        fVar19 = fVar16;
        fVar14 = fVar15;
        fVar17 = fVar18;
        do {
          fVar24 = *pfVar5;
          fVar16 = pfVar5[1];
          fVar15 = pfVar5[2];
          fVar18 = pfVar5[3];
          fVar1 = *pfVar12;
          fVar2 = pfVar12[1];
          fVar3 = pfVar12[2];
          fVar4 = pfVar12[3];
          pfVar12 = pfVar12 + 4;
          pfVar5 = pfVar5 + 4;
          fVar20 = fVar4 * fVar24 + fVar17 * fVar3 + fVar2 * fVar14 + fVar19 * fVar1 + fVar20;
          fVar21 = fVar4 * fVar16 + fVar24 * fVar3 + fVar2 * fVar17 + fVar14 * fVar1 + fVar21;
          fVar22 = fVar4 * fVar15 + fVar16 * fVar3 + fVar2 * fVar24 + fVar17 * fVar1 + fVar22;
          fVar23 = fVar4 * fVar18 + fVar15 * fVar3 + fVar2 * fVar16 + fVar24 * fVar1 + fVar23;
          iVar6 = iVar6 + 4;
          fVar19 = fVar16;
          fVar14 = fVar15;
          fVar17 = fVar18;
        } while (iVar6 < (int)(param_4 - 3));
        uVar7 = uVar8;
        if ((int)uVar8 < (int)param_4) {
LAB_18000cc5d:
          fVar24 = *pfVar5;
          pfVar5 = pfVar5 + 1;
          fVar19 = *pfVar12;
          fVar20 = fVar20 + fVar16 * fVar19;
          fVar21 = fVar21 + fVar15 * fVar19;
          fVar22 = fVar22 + fVar18 * fVar19;
          fVar23 = fVar23 + fVar24 * fVar19;
          pfVar12 = pfVar12 + 1;
        }
      }
      if ((int)(uVar7 | 1) < (int)param_4) {
        fVar16 = *pfVar5;
        pfVar5 = pfVar5 + 1;
        fVar19 = *pfVar12;
        pfVar12 = pfVar12 + 1;
        fVar20 = fVar20 + fVar15 * fVar19;
        fVar21 = fVar21 + fVar18 * fVar19;
        fVar22 = fVar22 + fVar24 * fVar19;
        fVar23 = fVar23 + fVar16 * fVar19;
      }
      if ((int)(uVar7 | 2) < (int)param_4) {
        fVar15 = *pfVar12;
        fVar20 = fVar20 + fVar18 * fVar15;
        fVar21 = fVar21 + fVar24 * fVar15;
        fVar22 = fVar22 + fVar16 * fVar15;
        fVar23 = fVar23 + *pfVar5 * fVar15;
      }
      pfVar12 = (float *)(param_3 + uVar10 * 4);
      *pfVar12 = fVar20;
      pfVar12[1] = fVar21;
      pfVar12[2] = fVar22;
      pfVar12[3] = fVar23;
      uVar10 = uVar10 + 4;
    } while (uVar10 < param_5 - 3);
  }
  if ((int)(uint)uVar10 < (int)param_5) {
    uVar11 = uVar10 & 0xffffffff;
    if ((int)param_4 < 1) {
      memset((void *)(param_3 + uVar11 * 4),0,(ulonglong)(~(uint)uVar10 + param_5) * 4 + 4);
      return;
    }
    lVar9 = param_2 + uVar11 * 4 + 0xc;
    param_2 = param_2 + uVar11 * 4;
    do {
      fVar15 = 0.0;
      uVar10 = 0;
      fVar16 = 0.0;
      if (3 < param_4) {
        do {
          fVar15 = param_1[uVar10 + 3] * *(float *)(lVar9 + uVar10 * 4) +
                   param_1[uVar10 + 2] * *(float *)(lVar9 + -4 + uVar10 * 4) +
                   param_1[uVar10 + 1] * *(float *)(lVar9 + -8 + uVar10 * 4) +
                   param_1[uVar10] * *(float *)(lVar9 + -0xc + uVar10 * 4) + fVar15;
          uVar10 = uVar10 + 4;
          fVar16 = fVar15;
        } while ((param_4 & 0x7ffffffc) != uVar10);
      }
      if ((ulonglong)(param_4 & 3) != 0) {
        uVar13 = 0;
        do {
          fVar16 = fVar16 + param_1[uVar10 + uVar13] * *(float *)(param_2 + uVar10 * 4 + uVar13 * 4)
          ;
          uVar13 = uVar13 + 1;
        } while ((param_4 & 3) != uVar13);
      }
      *(float *)(param_3 + uVar11 * 4) = fVar16;
      uVar11 = uVar11 + 1;
      lVar9 = lVar9 + 4;
      param_2 = param_2 + 4;
    } while (uVar11 != param_5);
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ce40
   NAME : rnn_pitch_search
   SIG  : undefined rnn_pitch_search(void)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void rnn_pitch_search(longlong param_1,longlong param_2,int param_3,int param_4,int *param_5)

{
  undefined4 *puVar1;
  float *pfVar2;
  undefined4 *puVar3;
  float *pfVar4;
  undefined4 uVar5;
  undefined4 uVar6;
  undefined4 uVar7;
  undefined4 uVar8;
  undefined4 uVar9;
  undefined4 uVar10;
  undefined4 uVar11;
  ulonglong uVar12;
  uint uVar13;
  ulonglong uVar14;
  int iVar15;
  int iVar16;
  ulonglong uVar17;
  ulonglong uVar18;
  uint uVar19;
  uint uVar20;
  ulonglong uVar21;
  longlong lVar22;
  longlong lVar23;
  uint uVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  float fVar31;
  float fVar32;
  undefined1 local_1158 [32];
  uint local_1138;
  ulonglong local_1130;
  float local_1128 [384];
  float local_b28 [432];
  undefined4 local_468 [242];
  ulonglong local_a0;
  undefined8 uStack_48;
  
                    /* 0xce40  28  rnn_pitch_search */
  uStack_48 = 0x18000ce56;
  local_a0 = DAT_180582000 ^ (ulonglong)local_1158;
  uVar19 = param_3 >> 2;
  if (0 < (int)uVar19) {
    if (uVar19 < 9) {
      uVar14 = 0;
    }
    else {
      uVar14 = 8;
      if ((uVar19 & 7) != 0) {
        uVar14 = (ulonglong)(uVar19 & 7);
      }
      uVar14 = uVar19 - uVar14;
      uVar17 = 0;
      do {
        puVar1 = (undefined4 *)(param_1 + uVar17 * 8);
        uVar5 = puVar1[2];
        puVar3 = (undefined4 *)(param_1 + 0x10 + uVar17 * 8);
        uVar6 = *puVar3;
        uVar7 = puVar3[2];
        puVar3 = (undefined4 *)(param_1 + 0x20 + uVar17 * 8);
        uVar8 = *puVar3;
        uVar9 = puVar3[2];
        puVar3 = (undefined4 *)(param_1 + 0x30 + uVar17 * 8);
        uVar10 = *puVar3;
        uVar11 = puVar3[2];
        local_468[uVar17] = *puVar1;
        local_468[uVar17 + 1] = uVar5;
        local_468[uVar17 + 2] = uVar6;
        local_468[uVar17 + 3] = uVar7;
        local_468[uVar17 + 4] = uVar8;
        local_468[uVar17 + 5] = uVar9;
        local_468[uVar17 + 6] = uVar10;
        local_468[uVar17 + 7] = uVar11;
        uVar17 = uVar17 + 8;
      } while (uVar14 != uVar17);
    }
    do {
      local_468[uVar14] = *(undefined4 *)(param_1 + uVar14 * 8);
      uVar14 = uVar14 + 1;
    } while (uVar19 != uVar14);
  }
  uVar13 = param_4 + param_3 >> 2;
  if (0 < (int)uVar13) {
    if (uVar13 < 9) {
      uVar14 = 0;
    }
    else {
      uVar14 = 8;
      if ((uVar13 & 7) != 0) {
        uVar14 = (ulonglong)(uVar13 & 7);
      }
      uVar14 = uVar13 - uVar14;
      uVar17 = 0;
      do {
        pfVar2 = (float *)(param_2 + uVar17 * 8);
        fVar25 = pfVar2[2];
        pfVar4 = (float *)(param_2 + 0x10 + uVar17 * 8);
        fVar27 = *pfVar4;
        fVar26 = pfVar4[2];
        pfVar4 = (float *)(param_2 + 0x20 + uVar17 * 8);
        fVar29 = *pfVar4;
        fVar30 = pfVar4[2];
        pfVar4 = (float *)(param_2 + 0x30 + uVar17 * 8);
        fVar28 = *pfVar4;
        fVar31 = pfVar4[2];
        local_b28[uVar17] = *pfVar2;
        local_b28[uVar17 + 1] = fVar25;
        local_b28[uVar17 + 2] = fVar27;
        local_b28[uVar17 + 3] = fVar26;
        local_b28[uVar17 + 4] = fVar29;
        local_b28[uVar17 + 5] = fVar30;
        local_b28[uVar17 + 6] = fVar28;
        local_b28[uVar17 + 7] = fVar31;
        uVar17 = uVar17 + 8;
      } while (uVar14 != uVar17);
    }
    do {
      local_b28[uVar14] = *(float *)(param_2 + uVar14 * 8);
      uVar14 = uVar14 + 1;
    } while (uVar13 != uVar14);
  }
  uVar13 = param_4 >> 2;
  local_1138 = uVar13;
  rnn_pitch_xcorr(local_468,local_b28,local_1128);
  fVar25 = DAT_1800100a8;
  if (0 < (int)uVar19) {
    if (uVar19 < 4) {
      uVar14 = 0;
    }
    else {
      uVar14 = 0;
      do {
        fVar25 = local_b28[uVar14 + 3] * local_b28[uVar14 + 3] +
                 local_b28[uVar14 + 2] * local_b28[uVar14 + 2] +
                 local_b28[uVar14 + 1] * local_b28[uVar14 + 1] +
                 local_b28[uVar14] * local_b28[uVar14] + fVar25;
        uVar14 = uVar14 + 4;
      } while ((uVar19 & 0x7ffffffc) != uVar14);
    }
    if ((ulonglong)(uVar19 & 3) != 0) {
      uVar17 = 0;
      do {
        fVar25 = fVar25 + local_b28[uVar14 + uVar17] * local_b28[uVar14 + uVar17];
        uVar17 = uVar17 + 1;
      } while ((uVar19 & 3) != uVar17);
    }
  }
  uVar14 = 0;
  iVar16 = 0;
  if ((int)uVar13 < 1) {
    iVar15 = 2;
  }
  else {
    uVar18 = 1;
    uVar17 = 0;
    uVar21 = 0;
    fVar30 = 0.0;
    fVar26 = DAT_180010524;
    fVar29 = 0.0;
    fVar27 = DAT_180010524;
    do {
      fVar28 = fVar26;
      fVar31 = fVar29;
      if ((0.0 < local_1128[uVar21]) &&
         (fVar32 = local_1128[uVar21] * DAT_180010528, fVar32 = fVar32 * fVar32,
         fVar27 * fVar25 < fVar30 * fVar32)) {
        uVar18 = uVar21 & 0xffffffff;
        fVar30 = fVar25;
        uVar12 = uVar17;
        if (fVar26 * fVar25 < fVar29 * fVar32) {
          uVar14 = uVar21 & 0xffffffff;
          fVar31 = fVar25;
          fVar30 = fVar29;
          fVar28 = fVar32;
          uVar12 = uVar21 & 0xffffffff;
          uVar18 = uVar17;
        }
        uVar17 = uVar12;
        uVar24 = -(uint)(fVar26 * fVar25 < fVar29 * fVar32);
        fVar27 = (float)(~uVar24 & (uint)fVar32 | (uint)fVar26 & uVar24);
      }
      fVar26 = fVar25 + (local_b28[(longlong)(int)uVar19 + uVar21] *
                         local_b28[(longlong)(int)uVar19 + uVar21] -
                        local_b28[uVar21] * local_b28[uVar21]);
      fVar25 = DAT_1800100a8;
      if (DAT_1800100a8 <= fVar26) {
        fVar25 = fVar26;
      }
      uVar21 = uVar21 + 1;
      fVar26 = fVar28;
      fVar29 = fVar31;
    } while (uVar13 != uVar21);
    iVar16 = (int)uVar14 * 2;
    iVar15 = (int)uVar18 * 2;
  }
  uVar13 = param_4 >> 1;
  local_1130 = (ulonglong)uVar13;
  uVar19 = param_3 >> 1;
  if (0 < (int)uVar13) {
    lVar22 = param_2 + 0xc;
    uVar14 = 0;
    lVar23 = param_2;
    do {
      local_1128[uVar14] = 0.0;
      uVar20 = (int)uVar14 - iVar16;
      uVar24 = -uVar20;
      if ((int)uVar24 < 0) {
        uVar24 = uVar20;
      }
      if (uVar24 < 3) {
LAB_18000d23d:
        fVar25 = 0.0;
        if (0 < (int)uVar19) {
          uVar17 = 0;
          if (3 < uVar19) {
            do {
              fVar25 = *(float *)(param_1 + 0xc + uVar17 * 4) * *(float *)(lVar22 + uVar17 * 4) +
                       *(float *)(param_1 + 8 + uVar17 * 4) * *(float *)(lVar22 + -4 + uVar17 * 4) +
                       *(float *)(param_1 + 4 + uVar17 * 4) * *(float *)(lVar22 + -8 + uVar17 * 4) +
                       *(float *)(param_1 + uVar17 * 4) * *(float *)(lVar22 + -0xc + uVar17 * 4) +
                       fVar25;
              uVar17 = uVar17 + 4;
            } while ((uVar19 & 0x7ffffffc) != uVar17);
          }
          if ((ulonglong)(uVar19 & 3) != 0) {
            uVar18 = 0;
            do {
              fVar25 = fVar25 + *(float *)(param_1 + uVar17 * 4 + uVar18 * 4) *
                                *(float *)(lVar23 + uVar17 * 4 + uVar18 * 4);
              uVar18 = uVar18 + 1;
            } while ((uVar19 & 3) != uVar18);
          }
        }
        fVar27 = DAT_180010524;
        if (DAT_180010524 <= fVar25) {
          fVar27 = fVar25;
        }
        local_1128[uVar14] = fVar27;
      }
      else {
        uVar20 = (int)uVar14 - iVar15;
        uVar24 = -uVar20;
        if ((int)uVar24 < 0) {
          uVar24 = uVar20;
        }
        if (uVar24 < 3) goto LAB_18000d23d;
      }
      uVar14 = uVar14 + 1;
      lVar22 = lVar22 + 4;
      lVar23 = lVar23 + 4;
    } while (uVar14 != local_1130);
  }
  fVar25 = DAT_1800100a8;
  if (0 < (int)uVar19) {
    if (uVar19 < 4) {
      uVar14 = 0;
    }
    else {
      uVar14 = 0;
      do {
        fVar27 = *(float *)(param_2 + uVar14 * 4);
        fVar26 = *(float *)(param_2 + 4 + uVar14 * 4);
        fVar29 = *(float *)(param_2 + 8 + uVar14 * 4);
        fVar30 = *(float *)(param_2 + 0xc + uVar14 * 4);
        fVar25 = fVar30 * fVar30 + fVar29 * fVar29 + fVar26 * fVar26 + fVar27 * fVar27 + fVar25;
        uVar14 = uVar14 + 4;
      } while ((uVar19 & 0x7ffffffc) != uVar14);
    }
    if ((ulonglong)(uVar19 & 3) != 0) {
      uVar17 = 0;
      do {
        fVar27 = *(float *)(param_2 + uVar14 * 4 + uVar17 * 4);
        fVar25 = fVar25 + fVar27 * fVar27;
        uVar17 = uVar17 + 1;
      } while ((uVar19 & 3) != uVar17);
    }
  }
  if ((int)uVar13 < 1) {
    uVar14 = 0;
  }
  else {
    uVar14 = 0;
    uVar17 = 0;
    fVar30 = 0.0;
    fVar27 = 0.0;
    fVar26 = DAT_180010524;
    fVar29 = DAT_180010524;
    do {
      fVar28 = fVar27;
      if (((0.0 < local_1128[uVar17]) &&
          (fVar31 = local_1128[uVar17] * DAT_180010528, fVar31 = fVar31 * fVar31,
          fVar26 * fVar25 < fVar30 * fVar31)) &&
         (fVar26 = fVar31, fVar30 = fVar25, fVar29 * fVar25 < fVar27 * fVar31)) {
        uVar14 = uVar17 & 0xffffffff;
        fVar28 = fVar25;
        fVar26 = fVar29;
        fVar29 = fVar31;
        fVar30 = fVar27;
      }
      fVar27 = *(float *)(param_2 + (longlong)(int)uVar19 * 4 + uVar17 * 4);
      fVar31 = *(float *)(param_2 + uVar17 * 4);
      fVar27 = fVar25 + (fVar27 * fVar27 - fVar31 * fVar31);
      fVar25 = DAT_1800100a8;
      if (DAT_1800100a8 <= fVar27) {
        fVar25 = fVar27;
      }
      uVar17 = uVar17 + 1;
      fVar27 = fVar28;
    } while (local_1130 != uVar17);
    iVar16 = (int)uVar14;
    if (iVar16 < (int)(uVar13 - 1) && 0 < iVar16) {
      fVar25 = local_1128[iVar16 - 1];
      fVar27 = local_1128[uVar14 + 1];
      uVar19 = 0xffffffff;
      if (fVar27 - fVar25 <= (local_1128[uVar14] - fVar25) * _DAT_18001052c) {
        uVar19 = (uint)((local_1128[uVar14] - fVar27) * _DAT_18001052c < fVar25 - fVar27);
      }
      goto LAB_18000d4eb;
    }
  }
  uVar19 = 0;
LAB_18000d4eb:
  *param_5 = uVar19 + (int)uVar14 * 2;
  if ((local_a0 ^ (ulonglong)local_1158) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 18000d550
   NAME : rnn_remove_doubling
   SIG  : undefined rnn_remove_doubling(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
rnn_remove_doubling(longlong param_1,int param_2,ulonglong param_3,uint param_4,uint *param_5,
                   int param_6,float param_7)

{
  double dVar1;
  float fVar2;
  uint uVar3;
  int iVar4;
  uint uVar5;
  int iVar6;
  longlong lVar7;
  uint uVar8;
  uint uVar9;
  longlong lVar10;
  uint uVar11;
  longlong lVar12;
  int iVar13;
  ulonglong uVar14;
  ulonglong uVar15;
  float *pfVar16;
  longlong lVar17;
  longlong lVar18;
  double dVar19;
  double dVar20;
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
  undefined4 unaff_XMM13_Db;
  undefined1 auStack_d68 [36];
  uint local_d44;
  ulonglong local_d40;
  longlong local_d38;
  longlong local_d30;
  uint local_d24;
  float local_d20;
  int local_d1c;
  longlong local_d18;
  ulonglong local_d10;
  ulonglong local_d08;
  float *local_d00;
  float local_cf8 [770];
  ulonglong local_f0;
  
                    /* 0xd550  30  rnn_remove_doubling */
  local_f0 = DAT_180582000 ^ (ulonglong)auStack_d68;
  local_d1c = param_2 / 2;
  local_d24 = (uint)param_3;
  uVar8 = (int)param_4 / 2;
  lVar17 = (longlong)local_d1c;
  local_d18 = param_1 + lVar17 * 4;
  uVar11 = local_d1c - 1;
  if ((int)*param_5 / 2 < local_d1c) {
    uVar11 = (int)*param_5 / 2;
  }
  local_d08 = (ulonglong)uVar11;
  *param_5 = uVar11;
  lVar7 = (longlong)(int)uVar11;
  fVar28 = 0.0;
  fVar29 = 0.0;
  local_d10 = (ulonglong)uVar8;
  if (1 < (int)param_4) {
    if ((param_4 & 0x7ffffffe) == 2) {
      uVar15 = 0;
    }
    else {
      lVar10 = param_1 + lVar17 * 4;
      lVar12 = lVar10 + 4;
      lVar18 = lVar12 + lVar7 * -4;
      fVar28 = 0.0;
      fVar29 = 0.0;
      uVar15 = 0;
      do {
        fVar21 = *(float *)(lVar10 + uVar15 * 4);
        fVar25 = *(float *)(lVar12 + uVar15 * 4);
        fVar28 = fVar25 * *(float *)(lVar18 + uVar15 * 4) +
                 fVar21 * *(float *)(lVar18 + -4 + uVar15 * 4) + fVar28;
        fVar29 = fVar25 * fVar25 + fVar21 * fVar21 + fVar29;
        uVar15 = uVar15 + 2;
      } while ((uVar8 & 0x3ffffffe) != uVar15);
    }
    if ((uVar8 & 1) != 0) {
      fVar21 = *(float *)(local_d18 + uVar15 * 4);
      fVar28 = fVar28 + fVar21 * *(float *)(local_d18 + lVar7 * -4 + uVar15 * 4);
      fVar29 = fVar29 + fVar21 * fVar21;
    }
  }
  local_cf8[0] = fVar29;
  if (1 < param_2) {
    pfVar16 = (float *)(param_1 + lVar17 * 4);
    uVar15 = 1;
    fVar21 = fVar29;
    do {
      pfVar16 = pfVar16 + -1;
      fVar21 = (*pfVar16 * *pfVar16 + fVar21) - pfVar16[(int)uVar8] * pfVar16[(int)uVar8];
      fVar25 = 0.0;
      if (0.0 <= fVar21) {
        fVar25 = fVar21;
      }
      local_cf8[uVar15] = fVar25;
      uVar15 = uVar15 + 1;
    } while (local_d1c + 1 != uVar15);
  }
  iVar13 = (int)(((uint)(param_3 >> 0x1f) & 1) + local_d24) >> 1;
  fVar21 = local_cf8[lVar7];
  dVar1 = (double)fVar28;
  dVar19 = (double)(fVar29 * fVar21 + DAT_1800100a8);
  local_d44 = param_4;
  local_d38 = lVar17;
  local_d30 = param_1;
  if (dVar19 < 0.0) {
    dVar19 = sqrt(dVar19);
  }
  else {
    dVar19 = SQRT(dVar19);
  }
  fVar26 = DAT_1800104e0;
  fVar25 = DAT_1800100a8;
  uVar3 = _DAT_180010000;
  local_d20 = DAT_1800104e0 * param_7;
  local_d40 = local_d10 - 1;
  local_d00 = (float *)(param_1 + lVar17 * 4 + 4);
  lVar17 = 2;
  fVar30 = (float)(dVar1 / dVar19);
  do {
    iVar6 = (int)lVar17;
    uVar15 = (longlong)(int)(uVar11 * 2 + iVar6) / (longlong)(iVar6 * 2);
    iVar4 = (int)uVar15;
    if (iVar4 < iVar13) break;
    if (lVar17 == 2) {
      uVar9 = iVar4 + uVar11;
      if (local_d1c < (int)(iVar4 + uVar11)) {
        uVar9 = uVar11;
      }
    }
    else {
      uVar9 = (int)(*(int *)(&DAT_1800104a0 + lVar17 * 4) * uVar11 * 2 + iVar6) / (iVar6 * 2);
    }
    fVar27 = 0.0;
    lVar10 = (longlong)iVar4;
    lVar7 = (longlong)(int)uVar9;
    if (1 < (int)local_d44) {
      if (local_d40 == 0) {
        fVar22 = 0.0;
        fVar27 = 0.0;
        uVar14 = 0;
      }
      else {
        fVar22 = 0.0;
        fVar27 = 0.0;
        uVar14 = 0;
        pfVar16 = local_d00;
        do {
          fVar22 = *pfVar16 * pfVar16[-lVar7] + pfVar16[-1 - lVar7] * pfVar16[-1] + fVar22;
          fVar27 = *pfVar16 * pfVar16[-lVar10] + pfVar16[-1 - lVar10] * pfVar16[-1] + fVar27;
          uVar14 = uVar14 + 2;
          pfVar16 = pfVar16 + 2;
        } while ((uVar8 & 0x3ffffffe) != uVar14);
      }
      if ((local_d10 & 1) != 0) {
        fVar23 = *(float *)(local_d18 + uVar14 * 4);
        fVar22 = fVar22 + *(float *)(local_d18 + (longlong)(int)-uVar9 * 4 + uVar14 * 4) * fVar23;
        fVar27 = fVar27 + *(float *)(local_d18 + (longlong)-iVar4 * 4 + uVar14 * 4) * fVar23;
      }
      fVar27 = fVar27 + fVar22;
    }
    fVar22 = (local_cf8[lVar10] + local_cf8[lVar7]) * fVar26;
    dVar20 = (double)(fVar29 * fVar22 + fVar25);
    if (dVar20 < _DAT_180010540) {
      dVar20 = sqrt(dVar20);
    }
    else {
      dVar20 = SQRT(dVar20);
    }
    uVar5 = iVar4 - param_6 / 2;
    uVar9 = -uVar5;
    if ((int)uVar9 < 0) {
      uVar9 = uVar5;
    }
    fVar23 = param_7;
    if (((1 < uVar9) && (fVar23 = 0.0, uVar9 == 2)) && (iVar6 * iVar6 * 5 < (int)uVar11)) {
      fVar23 = local_d20;
    }
    fVar24 = DAT_180010530;
    fVar2 = _DAT_18001052c;
    if (iVar4 < iVar13 * 3) {
      fVar24 = DAT_180010538;
      fVar2 = _DAT_180010534;
    }
    fVar23 = (float)(dVar1 / dVar19) * fVar2 + (float)((uint)fVar23 ^ uVar3);
    if (fVar24 <= fVar23) {
      fVar24 = fVar23;
    }
    fVar27 = fVar27 * fVar26;
    fVar23 = (float)((double)fVar27 / dVar20);
    if (fVar24 < fVar23) {
      fVar28 = fVar27;
      fVar21 = fVar22;
      fVar30 = fVar23;
      unaff_XMM13_Db = 0;
      local_d08 = uVar15 & 0xffffffff;
    }
    lVar17 = lVar17 + 1;
  } while (lVar17 != 0x10);
  fVar27 = 0.0;
  fVar29 = 0.0;
  fVar26 = 0.0;
  iVar13 = (int)local_d08;
  if (1 < (int)local_d44) {
    uVar11 = (uint)local_d10;
    if (local_d40 < 3) {
      uVar15 = 0;
    }
    else {
      lVar17 = local_d30 + local_d38 * 4;
      lVar7 = lVar17 + 0xc + (longlong)(iVar13 + -1) * -4;
      uVar15 = 0;
      do {
        fVar27 = *(float *)(lVar17 + 0xc + uVar15 * 4) * *(float *)(lVar7 + uVar15 * 4) +
                 *(float *)(lVar17 + 8 + uVar15 * 4) * *(float *)(lVar7 + -4 + uVar15 * 4) +
                 *(float *)(lVar17 + 4 + uVar15 * 4) * *(float *)(lVar7 + -8 + uVar15 * 4) +
                 *(float *)(lVar17 + uVar15 * 4) * *(float *)(lVar7 + -0xc + uVar15 * 4) + fVar27;
        uVar15 = uVar15 + 4;
      } while ((uVar11 & 0x3ffffffc) != uVar15);
    }
    if ((ulonglong)(uVar11 & 3) != 0) {
      lVar17 = local_d38 * 4 + uVar15 * 4;
      uVar15 = 0;
      do {
        fVar27 = fVar27 + *(float *)(local_d30 + lVar17 + uVar15 * 4) *
                          *(float *)(lVar17 + (longlong)(iVar13 + -1) * -4 + local_d30 + uVar15 * 4)
        ;
        uVar15 = uVar15 + 1;
      } while ((uVar11 & 3) != uVar15);
    }
    if (local_d40 < 3) {
      fVar29 = 0.0;
      uVar15 = 0;
    }
    else {
      lVar17 = local_d30 + local_d38 * 4;
      lVar7 = lVar17 + 0xc + (longlong)iVar13 * -4;
      fVar29 = 0.0;
      uVar15 = 0;
      do {
        fVar29 = *(float *)(lVar17 + 0xc + uVar15 * 4) * *(float *)(lVar7 + uVar15 * 4) +
                 *(float *)(lVar17 + 8 + uVar15 * 4) * *(float *)(lVar7 + -4 + uVar15 * 4) +
                 *(float *)(lVar17 + 4 + uVar15 * 4) * *(float *)(lVar7 + -8 + uVar15 * 4) +
                 *(float *)(lVar17 + uVar15 * 4) * *(float *)(lVar7 + -0xc + uVar15 * 4) + fVar29;
        uVar15 = uVar15 + 4;
      } while ((uVar11 & 0x3ffffffc) != uVar15);
    }
    if ((ulonglong)(uVar11 & 3) != 0) {
      lVar17 = local_d38 * 4 + uVar15 * 4;
      uVar15 = 0;
      do {
        fVar29 = fVar29 + *(float *)(local_d30 + lVar17 + uVar15 * 4) *
                          *(float *)(lVar17 + (longlong)iVar13 * -4 + local_d30 + uVar15 * 4);
        uVar15 = uVar15 + 1;
      } while ((uVar11 & 3) != uVar15);
    }
    if (local_d40 < 3) {
      fVar26 = 0.0;
      uVar15 = 0;
    }
    else {
      lVar17 = local_d30 + local_d38 * 4;
      lVar7 = lVar17 + 0xc + (longlong)(iVar13 + 1) * -4;
      fVar26 = 0.0;
      uVar15 = 0;
      do {
        fVar26 = *(float *)(lVar17 + 0xc + uVar15 * 4) * *(float *)(lVar7 + uVar15 * 4) +
                 *(float *)(lVar17 + 8 + uVar15 * 4) * *(float *)(lVar7 + -4 + uVar15 * 4) +
                 *(float *)(lVar17 + 4 + uVar15 * 4) * *(float *)(lVar7 + -8 + uVar15 * 4) +
                 *(float *)(lVar17 + uVar15 * 4) * *(float *)(lVar7 + -0xc + uVar15 * 4) + fVar26;
        uVar15 = uVar15 + 4;
      } while ((uVar11 & 0x3ffffffc) != uVar15);
    }
    if ((ulonglong)(uVar11 & 3) != 0) {
      lVar17 = local_d38 * 4 + uVar15 * 4;
      uVar15 = 0;
      do {
        fVar26 = fVar26 + *(float *)(local_d30 + lVar17 + uVar15 * 4) *
                          *(float *)(lVar17 + (longlong)(iVar13 + 1) * -4 + local_d30 + uVar15 * 4);
        uVar15 = uVar15 + 1;
      } while ((uVar11 & 3) != uVar15);
    }
  }
  iVar6 = 1;
  if (fVar26 - fVar27 <= (fVar29 - fVar27) * _DAT_18001052c) {
    iVar6 = -(uint)((fVar29 - fVar26) * _DAT_18001052c < fVar27 - fVar26);
  }
  fVar29 = 0.0;
  if (0.0 <= fVar28) {
    fVar29 = fVar28;
  }
  if (fVar29 < fVar21) {
    fVar25 = fVar29 / (fVar21 + fVar25);
  }
  if (fVar25 <= fVar30) {
    fVar30 = fVar25;
  }
  uVar8 = iVar6 + iVar13 * 2;
  uVar11 = local_d24;
  if ((int)local_d24 < (int)uVar8) {
    uVar11 = uVar8;
  }
  *param_5 = uVar11;
  if ((local_f0 ^ (ulonglong)auStack_d68) != DAT_180582000) {
                    /* WARNING: Subroutine does not return */
    FUN_18000e470();
  }
  return CONCAT44(unaff_XMM13_Db,fVar30);
}



/* ========================================================================
   ENTRY: 18000de20
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
  
                    /* 0xde20  1  compute_rnn */
  local_48 = DAT_180582000 ^ (ulonglong)auStack_2888;
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
  if ((local_48 ^ (ulonglong)auStack_2888) == DAT_180582000) {
    return;
  }
                    /* WARNING: Subroutine does not return */
  FUN_18000e470();
}



/* ========================================================================
   ENTRY: 18000dfe0
   NAME : init_rnnoise
   SIG  : undefined init_rnnoise(void)
   ======================================================================== */

bool init_rnnoise(longlong param_1,undefined8 param_2)

{
  int iVar1;
  bool bVar2;
  
                    /* 0xdfe0  2  init_rnnoise */
  iVar1 = rnn_linear_init(param_1,param_2,"conv1_bias",0,0,"conv1_weights_float",0,0,0,0xc3,0x80);
  bVar2 = true;
  if (iVar1 == 0) {
    iVar1 = rnn_linear_init(param_1 + 0x40,param_2,"conv2_bias","conv2_subias","conv2_weights_int8",
                            "conv2_weights_float",0,0,"conv2_scale",0x180,0x180);
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
                  iVar1 = rnn_linear_init(param_1 + 0x200,param_2,"dense_out_bias",0,0,
                                          "dense_out_weights_float",0,0,0,0x600,0x20);
                  if (iVar1 == 0) {
                    iVar1 = rnn_linear_init(param_1 + 0x240,param_2,"vad_dense_bias",0,0,
                                            "vad_dense_weights_float",0,0,0,0x600,1);
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
   ENTRY: 18000e410
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
   ENTRY: 18000e470
   NAME : FUN_18000e470
   SIG  : noreturn undefined FUN_18000e470(void)
   ======================================================================== */

void FUN_18000e470(longlong param_1)

{
  if ((param_1 == DAT_180582000) && ((short)((ulonglong)param_1 >> 0x30) == 0)) {
    return;
  }
  FUN_18000e490(param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000e490
   NAME : FUN_18000e490
   SIG  : undefined FUN_18000e490(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000e490(void)

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
  *(undefined8 *)(puVar3 + -8) = 0x18000e4bb;
  FUN_18000e564(&DAT_180582120);
  _DAT_180582090 = *(undefined8 *)(puVar3 + 0x38);
  _DAT_1805821b8 = puVar3 + 0x40;
  _DAT_1805821a0 = *(undefined8 *)(puVar3 + 0x40);
  _DAT_180582080 = 0xc0000409;
  _DAT_180582084 = 1;
  _DAT_180582098 = 1;
  DAT_1805820a0 = 2;
  *(undefined8 *)(puVar3 + 0x20) = DAT_180582000;
  *(undefined8 *)(puVar3 + 0x28) = DAT_180582040;
  *(undefined8 *)(puVar3 + -8) = 0x18000e55d;
  DAT_180582218 = _DAT_180582090;
  __raise_securityfailure(&PTR_DAT_18057f910);
  return;
}



/* ========================================================================
   ENTRY: 18000e564
   NAME : FUN_18000e564
   SIG  : undefined FUN_18000e564(void)
   ======================================================================== */

void FUN_18000e564(PCONTEXT param_1)

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
   ENTRY: 18000e5d8
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
                    /* WARNING: Could not recover jumptable at 0x00018000e605. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  TerminateProcess(pvVar1,0xc0000409);
  return;
}



/* ========================================================================
   ENTRY: 18000e60c
   NAME : FUN_18000e60c
   SIG  : undefined FUN_18000e60c(void)
   ======================================================================== */

undefined8 FUN_18000e60c(undefined8 param_1,undefined8 param_2)

{
  bool bVar1;
  char cVar2;
  undefined1 uVar3;
  int iVar4;
  longlong *plVar5;
  
  cVar2 = FUN_18000eb74(0);
  if (cVar2 != '\0') {
    uVar3 = __scrt_acquire_startup_lock();
    bVar1 = true;
    if (DAT_180582620 != 0) {
                    /* WARNING: Subroutine does not return */
      FUN_18000edac(7);
    }
    DAT_180582620 = 1;
    cVar2 = FUN_18000ecc8();
    if (cVar2 != '\0') {
      FUN_18000eef8();
      FUN_18000ea34();
      FUN_18000ea50();
      iVar4 = _initterm_e(&DAT_18057ff08,&DAT_18057ff10);
      if ((iVar4 == 0) && (cVar2 = __scrt_dllmain_after_initialize_c(), cVar2 != '\0')) {
        _initterm(&DAT_18057fef8,&DAT_18057ff00);
        DAT_180582620 = 2;
        bVar1 = false;
      }
    }
    __scrt_release_startup_lock(uVar3);
    if (!bVar1) {
      plVar5 = (longlong *)FUN_18000ed98();
      if ((*plVar5 != 0) && (cVar2 = FUN_18000ea7c(plVar5), cVar2 != '\0')) {
        (*(code *)PTR__guard_dispatch_icall_18057fed0)(param_1,2,param_2);
      }
      DAT_1805825f0 = DAT_1805825f0 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000e724
   NAME : FUN_18000e724
   SIG  : undefined FUN_18000e724(void)
   ======================================================================== */

undefined1
FUN_18000e724(undefined1 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined1 uVar1;
  undefined1 uVar2;
  
  if (DAT_1805825f0 < 1) {
    uVar1 = 0;
  }
  else {
    DAT_1805825f0 = DAT_1805825f0 + -1;
    uVar1 = __scrt_acquire_startup_lock();
    if (DAT_180582620 != 2) {
                    /* WARNING: Subroutine does not return */
      FUN_18000edac(7);
    }
    uVar2 = uVar1;
    __scrt_dllmain_uninitialize_c();
    FUN_18000ea44();
    FUN_18000ef34();
    DAT_180582620 = 0;
    __scrt_release_startup_lock(uVar1);
    uVar1 = __scrt_uninitialize_crt(param_1,0,param_3,param_4,uVar2);
    FUN_18000ed44();
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18000e7a8
   NAME : FUN_18000e7a8
   SIG  : undefined FUN_18000e7a8(void)
   ======================================================================== */

ulonglong FUN_18000e7a8(undefined1 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  ulonglong uVar2;
  
  if (param_2 == 0) {
    uVar2 = FUN_18000e724(param_3 != 0);
    return uVar2;
  }
  if (param_2 != 1) {
    if (param_2 == 2) {
      bVar1 = FUN_18000ed58();
    }
    else {
      if (param_2 != 3) {
        return 1;
      }
      bVar1 = FUN_18000ed80();
    }
    return (ulonglong)bVar1;
  }
  uVar2 = FUN_18000e60c(param_1,param_3);
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000e7f8
   NAME : FUN_18000e7f8
   SIG  : undefined FUN_18000e7f8(void)
   ======================================================================== */

int FUN_18000e7f8(undefined8 param_1,int param_2,longlong param_3)

{
  int iVar1;
  int iVar2;
  
  if ((param_2 == 0) && (DAT_1805825f0 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (DAT_18057f920 == 0) {
      iVar2 = 1;
    }
    else {
      iVar2 = (*(code *)PTR__guard_dispatch_icall_18057fed0)();
    }
    if (iVar2 == 0) {
      return 0;
    }
    iVar2 = FUN_18000e7a8(param_1,param_2,param_3);
    if (iVar2 == 0) {
      return 0;
    }
  }
  iVar2 = FUN_18000ea10(param_1,param_2,param_3);
  if ((param_2 == 1) && (iVar2 == 0)) {
    FUN_18000ea10(param_1,0,param_3);
    FUN_18000e724(param_3 != 0);
    if (DAT_18057f920 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057fed0)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    iVar1 = FUN_18000e7a8(param_1,param_2,param_3);
    iVar2 = 0;
    if (iVar1 != 0) {
      if (DAT_18057f920 == 0) {
        iVar2 = 1;
      }
      else {
        iVar2 = (*(code *)PTR__guard_dispatch_icall_18057fed0)(param_1,param_2,param_3);
      }
    }
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 18000e920
   NAME : entry
   SIG  : undefined entry(void)
   ======================================================================== */

void entry(undefined8 param_1,int param_2,undefined8 param_3)

{
  if (param_2 == 1) {
    FUN_18000e960();
  }
  FUN_18000e7f8(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 18000e960
   NAME : FUN_18000e960
   SIG  : undefined FUN_18000e960(void)
   ======================================================================== */

void FUN_18000e960(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10;
  _FILETIME local_18 [2];
  
  if (DAT_180582000 == 0x2b992ddfa232) {
    local_res8.dwLowDateTime = 0;
    local_res8.dwHighDateTime = 0;
    GetSystemTimeAsFileTime(&local_res8);
    local_18[0] = local_res8;
    DVar1 = GetCurrentThreadId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    DVar1 = GetCurrentProcessId();
    local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
    QueryPerformanceCounter(&local_res10);
    DAT_180582000 =
         ((ulonglong)local_res10.s.LowPart << 0x20 ^
          CONCAT44(local_res10.s.HighPart,local_res10.s.LowPart) ^ (ulonglong)local_18[0] ^
         (ulonglong)local_18) & 0xffffffffffff;
    if (DAT_180582000 == 0x2b992ddfa232) {
      DAT_180582000 = 0x2b992ddfa233;
    }
  }
  DAT_180582040 = ~DAT_180582000;
  return;
}



/* ========================================================================
   ENTRY: 18000ea10
   NAME : FUN_18000ea10
   SIG  : undefined FUN_18000ea10(void)
   ======================================================================== */

undefined8 FUN_18000ea10(HMODULE param_1,int param_2)

{
  if ((param_2 == 1) && (DAT_18057f920 == 0)) {
    DisableThreadLibraryCalls(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000ea34
   NAME : FUN_18000ea34
   SIG  : undefined FUN_18000ea34(void)
   ======================================================================== */

void FUN_18000ea34(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000ea3b. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_180582600);
  return;
}



/* ========================================================================
   ENTRY: 18000ea44
   NAME : FUN_18000ea44
   SIG  : undefined FUN_18000ea44(void)
   ======================================================================== */

void FUN_18000ea44(void)

{
  __std_type_info_destroy_list(&DAT_180582600);
  return;
}



/* ========================================================================
   ENTRY: 18000ea50
   NAME : FUN_18000ea50
   SIG  : undefined FUN_18000ea50(void)
   ======================================================================== */

void FUN_18000ea50(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_18000ea6c();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_18000ea74();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 18000ea6c
   NAME : FUN_18000ea6c
   SIG  : undefined FUN_18000ea6c(void)
   ======================================================================== */

undefined * FUN_18000ea6c(void)

{
  return &DAT_180582610;
}



/* ========================================================================
   ENTRY: 18000ea74
   NAME : FUN_18000ea74
   SIG  : undefined FUN_18000ea74(void)
   ======================================================================== */

undefined * FUN_18000ea74(void)

{
  return &DAT_180582618;
}



/* ========================================================================
   ENTRY: 18000ea7c
   NAME : FUN_18000ea7c
   SIG  : undefined FUN_18000ea7c(void)
   ======================================================================== */

ulonglong FUN_18000ea7c(longlong param_1)

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
           param_1 - 0x180000000U < uVar1)) goto LAB_18000eaf2;
      }
      lVar4 = 0;
LAB_18000eaf2:
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
   ENTRY: 18000eb14
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
LAB_18000eb42:
    uVar4 = 0;
  }
  else {
    do {
      lVar3 = 0;
      LOCK();
      bVar5 = DAT_180582628 == 0;
      lVar1 = *(longlong *)((longlong)Self + 8);
      if (!bVar5) {
        lVar3 = DAT_180582628;
        lVar1 = DAT_180582628;
      }
      DAT_180582628 = lVar1;
      UNLOCK();
      if (bVar5) goto LAB_18000eb42;
    } while (*(longlong *)((longlong)Self + 8) != lVar3);
    uVar4 = 1;
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000eb50
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
    DAT_180582628 = 0;
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000eb74
   NAME : FUN_18000eb74
   SIG  : undefined FUN_18000eb74(void)
   ======================================================================== */

undefined1 FUN_18000eb74(int param_1)

{
  char cVar1;
  
  if (param_1 == 0) {
    DAT_180582630 = 1;
  }
  FUN_18000ef74();
  cVar1 = FUN_18000f220();
  if (cVar1 != '\0') {
    cVar1 = FUN_18000f220();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_18000f220(0);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000ebb0
   NAME : __scrt_uninitialize_crt
   SIG  : undefined __scrt_uninitialize_crt(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_uninitialize_crt
   
   Library: Visual Studio 2019 Release */

undefined1 __scrt_uninitialize_crt(undefined1 param_1,char param_2)

{
  if ((DAT_180582630 == '\0') || (param_2 == '\0')) {
    FUN_18000f220();
    FUN_18000f220(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000ebdc
   NAME : FUN_18000ebdc
   SIG  : undefined FUN_18000ebdc(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000ebdc(uint param_1)

{
  int iVar1;
  
  if (DAT_180582631 == '\0') {
    if (1 < param_1) {
                    /* WARNING: Subroutine does not return */
      FUN_18000edac(5);
    }
    iVar1 = __scrt_is_ucrt_dll_in_use();
    if ((iVar1 == 0) || (param_1 != 0)) {
      _DAT_180582638 = _DAT_18057fa70;
      uRam0000000180582640 = _UNK_18057fa78;
      _DAT_180582648 = 0xffffffffffffffff;
      _DAT_180582650 = _DAT_18057fa70;
      uRam0000000180582658 = _UNK_18057fa78;
      _DAT_180582660 = 0xffffffffffffffff;
    }
    else {
      iVar1 = _initialize_onexit_table(&DAT_180582638);
      if ((iVar1 != 0) || (iVar1 = _initialize_onexit_table(&DAT_180582650), iVar1 != 0)) {
        return 0;
      }
    }
    DAT_180582631 = '\x01';
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000ec68
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
    (*(code *)PTR__guard_dispatch_icall_18057fed0)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 18000ecc8
   NAME : FUN_18000ecc8
   SIG  : undefined FUN_18000ecc8(void)
   ======================================================================== */

bool FUN_18000ecc8(void)

{
  char cVar1;
  
  cVar1 = FUN_18000ebdc(0);
  return cVar1 != '\0';
}



/* ========================================================================
   ENTRY: 18000ece0
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
    uVar2 = FUN_18000f20c();
    iVar1 = _configure_narrow_argv(uVar2);
    if (iVar1 != 0) {
      return 0;
    }
    _initialize_narrow_environment();
  }
  else {
    FUN_18000ef74();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000ed14
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
    _execute_onexit_table(&DAT_180582638);
    return;
  }
  iVar1 = FUN_18000f224();
  if (iVar1 == 0) {
    _cexit();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ed44
   NAME : FUN_18000ed44
   SIG  : undefined FUN_18000ed44(void)
   ======================================================================== */

void FUN_18000ed44(void)

{
  FUN_18000f220(0);
  FUN_18000f220();
  return;
}



/* ========================================================================
   ENTRY: 18000ed58
   NAME : FUN_18000ed58
   SIG  : undefined FUN_18000ed58(void)
   ======================================================================== */

undefined1 FUN_18000ed58(void)

{
  char cVar1;
  
  cVar1 = FUN_18000f220();
  if (cVar1 != '\0') {
    cVar1 = FUN_18000f220();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_18000f220();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000ed80
   NAME : FUN_18000ed80
   SIG  : undefined FUN_18000ed80(void)
   ======================================================================== */

undefined1 FUN_18000ed80(void)

{
  FUN_18000f220();
  FUN_18000f220();
  return 1;
}



/* ========================================================================
   ENTRY: 18000ed98
   NAME : FUN_18000ed98
   SIG  : undefined FUN_18000ed98(void)
   ======================================================================== */

undefined * FUN_18000ed98(void)

{
  return &DAT_180582668;
}



/* ========================================================================
   ENTRY: 18000eda0
   NAME : FUN_18000eda0
   SIG  : undefined FUN_18000eda0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000eda0(void)

{
  _DAT_180582670 = 0;
  return;
}



/* ========================================================================
   ENTRY: 18000edac
   NAME : FUN_18000edac
   SIG  : noreturn undefined FUN_18000edac(void)
   ======================================================================== */

void FUN_18000edac(undefined4 param_1)

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
  *(undefined8 *)(puVar4 + -8) = 0x18000ede0;
  FUN_18000eda0(3);
  *(undefined8 *)(puVar4 + -8) = 0x18000edf1;
  memset(local_4d8,0,0x4d0);
  *(undefined8 *)(puVar4 + -8) = 0x18000edfb;
  RtlCaptureContext(local_4d8);
  *(undefined8 *)(puVar4 + -8) = 0x18000ee15;
  FunctionEntry = RtlLookupFunctionEntry(local_3e0,&local_res10,(PUNWIND_HISTORY_TABLE)0x0);
  if (FunctionEntry != (PRUNTIME_FUNCTION)0x0) {
    *(undefined8 *)(puVar4 + 0x38) = 0;
    *(undefined1 **)(puVar4 + 0x30) = local_res18;
    *(undefined1 **)(puVar4 + 0x28) = local_res20;
    *(undefined1 **)(puVar4 + 0x20) = local_4d8;
    *(undefined8 *)(puVar4 + -8) = 0x18000ee59;
    RtlVirtualUnwind(0,local_res10,local_3e0,FunctionEntry,*(PCONTEXT *)(puVar4 + 0x20),
                     *(PVOID **)(puVar4 + 0x28),*(PDWORD64 *)(puVar4 + 0x30),
                     *(PKNONVOLATILE_CONTEXT_POINTERS *)(puVar4 + 0x38));
  }
  local_440 = &stack0x00000008;
  *(undefined8 *)(puVar4 + -8) = 0x18000ee8b;
  memset(puVar4 + 0x50,0,0x98);
  *(undefined8 *)(puVar4 + 0x60) = unaff_retaddr;
  *(undefined4 *)(puVar4 + 0x50) = 0x40000015;
  *(undefined4 *)(puVar4 + 0x54) = 1;
  *(undefined8 *)(puVar4 + -8) = 0x18000eead;
  BVar2 = IsDebuggerPresent();
  *(undefined1 **)(puVar4 + 0x40) = puVar4 + 0x50;
  *(undefined1 **)(puVar4 + 0x48) = local_4d8;
  *(undefined8 *)(puVar4 + -8) = 0x18000eeca;
  SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)0x0);
  *(undefined8 *)(puVar4 + -8) = 0x18000eed5;
  LVar3 = UnhandledExceptionFilter((_EXCEPTION_POINTERS *)(puVar4 + 0x40));
  if ((LVar3 == 0) && (BVar2 != 1)) {
    *(undefined8 *)(puVar4 + -8) = 0x18000eee6;
    FUN_18000eda0(3);
  }
  return;
}



/* ========================================================================
   ENTRY: 18000eef8
   NAME : FUN_18000eef8
   SIG  : undefined FUN_18000eef8(void)
   ======================================================================== */

void FUN_18000eef8(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_180580b58; plVar1 < &DAT_180580b58; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057fed0)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ef34
   NAME : FUN_18000ef34
   SIG  : undefined FUN_18000ef34(void)
   ======================================================================== */

void FUN_18000ef34(void)

{
  longlong *plVar1;
  
  for (plVar1 = &DAT_180580b68; plVar1 < &DAT_180580b68; plVar1 = plVar1 + 1) {
    if (*plVar1 != 0) {
      (*(code *)PTR__guard_dispatch_icall_18057fed0)();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ef70
   NAME : _guard_check_icall
   SIG  : undefined _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
  return;
}



/* ========================================================================
   ENTRY: 18000ef74
   NAME : FUN_18000ef74
   SIG  : undefined FUN_18000ef74(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x00018000f065) */
/* WARNING: Removing unreachable block (ram,0x00018000f055) */
/* WARNING: Removing unreachable block (ram,0x00018000f030) */
/* WARNING: Removing unreachable block (ram,0x00018000efae) */
/* WARNING: Removing unreachable block (ram,0x00018000ef8c) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000ef74(void)

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
    _DAT_180582060 = 0x8000;
    _DAT_180582068 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_180582678 = DAT_180582678 | 1;
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
      DAT_180582678 = DAT_180582678 | 2;
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
  _DAT_180582058 = 1;
  DAT_18058205c = 2;
  uVar9 = DAT_180582050 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    _DAT_180582058 = 2;
    DAT_18058205c = 6;
    uVar9 = DAT_180582050 & 0xffffffffffffffee;
  }
  DAT_180582050 = uVar9;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar9 = xinuse(0);
    uVar9 = in_XCR0 & uVar9 & 0xffffffff;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar6 = (byte)uVar9, (bVar6 & 6) == 6)) {
      _DAT_180582058 = 3;
      uVar7 = DAT_180582050;
      uVar5 = DAT_18058205c | 8;
      if ((uVar8 & 0x20) != 0) {
        _DAT_180582058 = 5;
        uVar7 = DAT_180582050 & 0xfffffffffffffffd;
        uVar5 = DAT_18058205c | 0x28;
        if (((uVar8 & 0xd0030000) == 0xd0030000) && ((bVar6 & 0xe0) == 0xe0)) {
          DAT_18058205c = DAT_18058205c | 0x68;
          _DAT_180582058 = 6;
          uVar7 = DAT_180582050 & 0xffffffffffffffd9;
          uVar5 = DAT_18058205c;
        }
      }
      DAT_18058205c = uVar5;
      DAT_180582050 = uVar7;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_180582050 = DAT_180582050 & 0xfffffffffeffffff;
      }
      if (((uVar11 >> 0x13 & 1) != 0) && ((bVar6 & 0xe0) == 0xe0)) {
        _DAT_180582674 = (uint)uVar12 & 0x400ff;
        DAT_180582050 = ~((ulonglong)((uint)(uVar12 >> 0x10) & 6) | 0x1000029) & DAT_180582050;
        if (1 < (byte)_DAT_180582674) {
          DAT_180582050 = DAT_180582050 & 0xffffffffffffffbf;
        }
      }
    }
    if (((uVar11 >> 0x15 & 1) != 0) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_180582050 = DAT_180582050 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000f20c
   NAME : FUN_18000f20c
   SIG  : undefined FUN_18000f20c(void)
   ======================================================================== */

undefined8 FUN_18000f20c(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 18000f214
   NAME : __scrt_is_ucrt_dll_in_use
   SIG  : undefined __scrt_is_ucrt_dll_in_use(void)
   ======================================================================== */

/* Library Function - Single Match
    __scrt_is_ucrt_dll_in_use
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

bool __scrt_is_ucrt_dll_in_use(void)

{
  return DAT_180582070 != 0;
}



/* ========================================================================
   ENTRY: 18000f220
   NAME : FUN_18000f220
   SIG  : undefined FUN_18000f220(void)
   ======================================================================== */

undefined1 FUN_18000f220(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 18000f224
   NAME : FUN_18000f224
   SIG  : undefined FUN_18000f224(void)
   ======================================================================== */

undefined8 FUN_18000f224(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 18000f240
   NAME : _guard_dispatch_icall
   SIG  : undefined _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f240. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 18000f260
   NAME : _guard_dispatch_icall
   SIG  : undefined _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x00018000f260 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f240. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 18000f266
   NAME : FUN_18000f266
   SIG  : undefined FUN_18000f266(void)
   ======================================================================== */

void FUN_18000f266(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(undefined1 *)(param_2 + 0x40));
  return;
}



/* ========================================================================
   ENTRY: 18000f27d
   NAME : FUN_18000f27d
   SIG  : undefined FUN_18000f27d(void)
   ======================================================================== */

void FUN_18000f27d(undefined8 param_1,longlong param_2)

{
  __scrt_release_startup_lock(*(undefined1 *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 18000f296
   NAME : FUN_18000f296
   SIG  : undefined FUN_18000f296(void)
   ======================================================================== */

void FUN_18000f296(void)

{
  FUN_18000ed44();
  return;
}



/* ========================================================================
   ENTRY: 18000f2aa
   NAME : FUN_18000f2aa
   SIG  : undefined FUN_18000f2aa(void)
   ======================================================================== */

void FUN_18000f2aa(undefined8 *param_1,longlong param_2)

{
  __scrt_dllmain_exception_filter
            (*(undefined8 *)(param_2 + 0x60),*(undefined4 *)(param_2 + 0x68),
             *(undefined8 *)(param_2 + 0x70),FUN_18000e7a8,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000f2e0
   NAME : FUN_18000f2e0
   SIG  : undefined FUN_18000f2e0(void)
   ======================================================================== */

bool FUN_18000f2e0(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}



/* ========================================================================
   ENTRY: 18000f310
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f310. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 18000f320
   NAME : memcpy
   SIG  : void * __cdecl memcpy(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memcpy(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f320. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memcpy(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000f330
   NAME : memmove
   SIG  : void * __cdecl memmove(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memmove(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f330. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memmove(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000f340
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f340. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000f350
   NAME : calloc
   SIG  : void * __cdecl calloc(size_t _Count, size_t _Size)
   ======================================================================== */

void * __cdecl calloc(size_t _Count,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f350. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = calloc(_Count,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18000f360
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f360. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 18000f370
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f370. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 18000f380
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f380. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 18000f390
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f390. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 18000f3a0
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f3a0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 18000f3b0
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f3b0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 18000f3c0
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f3c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 18000f3d0
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018000f3d0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 18000f3e0
   NAME : cos
   SIG  : double __cdecl cos(double _X)
   ======================================================================== */

double __cdecl cos(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f3e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = cos(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000f3f0
   NAME : floor
   SIG  : double __cdecl floor(double _X)
   ======================================================================== */

double __cdecl floor(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f3f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = floor(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000f400
   NAME : log10
   SIG  : double __cdecl log10(double _X)
   ======================================================================== */

double __cdecl log10(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f400. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = log10(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000f410
   NAME : sin
   SIG  : double __cdecl sin(double _X)
   ======================================================================== */

double __cdecl sin(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f410. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sin(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000f420
   NAME : sqrt
   SIG  : double __cdecl sqrt(double _X)
   ======================================================================== */

double __cdecl sqrt(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f420. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sqrt(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18000f430
   NAME : sqrtf
   SIG  : float __cdecl sqrtf(float _X)
   ======================================================================== */

float __cdecl sqrtf(float _X)

{
  float fVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f430. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fVar1 = sqrtf(_X);
  return fVar1;
}



/* ========================================================================
   ENTRY: 18000f440
   NAME : strcmp
   SIG  : int __cdecl strcmp(char * _Str1, char * _Str2)
   ======================================================================== */

int __cdecl strcmp(char *_Str1,char *_Str2)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018000f440. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = strcmp(_Str1,_Str2);
  return iVar1;
}


