
/* ========================================================================
   ENTRY: 180001000
   NAME : FUN_180001000
   SIG  : undefined8 * __fastcall FUN_180001000(int param_1, undefined8 param_2, undefined8 param_3)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 * FUN_180001000(int param_1,undefined8 param_2,undefined8 param_3)

{
  int iVar1;
  int iVar2;
  int iVar3;
  uint uVar4;
  undefined8 *puVar5;
  int iVar6;
  longlong lVar7;
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  undefined1 in_XMM2 [16];
  undefined1 auVar10 [16];
  
  puVar5 = (undefined8 *)FUN_180020230(param_1 << 4);
  if (puVar5 == (undefined8 *)0x0) {
    return (undefined8 *)0x0;
  }
  puVar5[1] = puVar5;
  *puVar5 = param_2;
  iVar3 = _UNK_180023534;
  iVar2 = _DAT_180023530;
  if (1 < param_1) {
    iVar6 = 1;
    if ((3 < param_1 - 1U) && (1 < DAT_18002b540)) {
      uVar4 = param_1 - 1U & 0x80000003;
      if ((int)uVar4 < 0) {
        uVar4 = (uVar4 - 1 | 0xfffffffc) + 1;
      }
      do {
        iVar1 = iVar6 + 3;
        auVar8._4_4_ = iVar3 + iVar6 + 1;
        auVar8._0_4_ = iVar2 + iVar6 + 1;
        lVar7 = (longlong)iVar6;
        auVar8._8_8_ = 0;
        iVar6 = iVar6 + 4;
        puVar5[lVar7 * 2 + 1] = 0;
        puVar5[lVar7 * 2 + 3] = 0;
        puVar5[lVar7 * 2 + 5] = 0;
        puVar5[lVar7 * 2 + 7] = 0;
        auVar8 = pmovsxdq(in_XMM2,auVar8);
        puVar5[lVar7 * 2] = puVar5 + auVar8._0_8_ * 2;
        auVar10._8_8_ = 0;
        auVar10._0_8_ = puVar5 + auVar8._8_8_ * 2;
        puVar5[lVar7 * 2 + 2] = puVar5 + auVar8._8_8_ * 2;
        auVar9._4_4_ = iVar3 + iVar1;
        auVar9._0_4_ = iVar2 + iVar1;
        auVar9._8_8_ = 0;
        auVar8 = pmovsxdq(auVar10,auVar9);
        puVar5[lVar7 * 2 + 4] = puVar5 + auVar8._0_8_ * 2;
        in_XMM2._8_8_ = 0;
        in_XMM2._0_8_ = puVar5 + auVar8._8_8_ * 2;
        puVar5[lVar7 * 2 + 6] = puVar5 + auVar8._8_8_ * 2;
      } while (iVar6 < (int)(param_1 - uVar4));
      if (param_1 <= iVar6) goto LAB_18000118f;
    }
    do {
      lVar7 = (longlong)iVar6;
      iVar6 = iVar6 + 1;
      puVar5[lVar7 * 2 + 1] = 0;
      puVar5[lVar7 * 2] = puVar5 + (longlong)iVar6 * 2;
    } while (iVar6 < param_1);
  }
LAB_18000118f:
  puVar5[((longlong)param_1 + -1) * 2] = param_3;
  return puVar5;
}



/* ========================================================================
   ENTRY: 1800011b0
   NAME : FUN_1800011b0
   SIG  : undefined4 * __fastcall FUN_1800011b0(void)
   ======================================================================== */

undefined4 * FUN_1800011b0(void)

{
  undefined8 *puVar1;
  undefined4 *puVar2;
  
  puVar1 = FUN_180001000(0x10,0,0);
  if (puVar1 != (undefined8 *)0x0) {
    puVar2 = (undefined4 *)FUN_180020230(0x20);
    if (puVar2 != (undefined4 *)0x0) {
      *puVar2 = 0x10;
      *(undefined8 **)(puVar2 + 2) = puVar1;
      *(undefined8 **)(puVar2 + 4) = puVar1 + 2;
      *(undefined8 *)(puVar2 + 6) = 0;
      return puVar2;
    }
    FUN_180020240((longlong)puVar1);
  }
  return (undefined4 *)0x0;
}



/* ========================================================================
   ENTRY: 180001230
   NAME : FUN_180001230
   SIG  : undefined __fastcall FUN_180001230(longlong param_1)
   ======================================================================== */

void FUN_180001230(longlong param_1)

{
  longlong *plVar1;
  undefined8 *puVar2;
  
  puVar2 = *(undefined8 **)(param_1 + 8);
  while (puVar2 != (undefined8 *)0x0) {
    plVar1 = puVar2 + 1;
    puVar2 = (undefined8 *)*puVar2;
    FUN_180020240(*plVar1);
  }
  FUN_180020240(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001280
   NAME : FUN_180001280
   SIG  : undefined __fastcall FUN_180001280(longlong param_1)
   ======================================================================== */

void FUN_180001280(longlong param_1)

{
  longlong *plVar1;
  longlong *plVar2;
  
  plVar1 = *(longlong **)(param_1 + 0x18);
  if (*(longlong **)(param_1 + 0x18) != (longlong *)0x0) {
    do {
      plVar2 = plVar1;
      FUN_180020240(plVar2[1]);
      plVar2[1] = 0;
      plVar1 = (longlong *)*plVar2;
    } while ((longlong *)*plVar2 != (longlong *)0x0);
    if (plVar2 != (longlong *)0x0) {
      *plVar2 = *(longlong *)(param_1 + 0x10);
      *(undefined8 *)(param_1 + 0x10) = *(undefined8 *)(param_1 + 0x18);
      *(undefined8 *)(param_1 + 0x18) = 0;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800012e0
   NAME : FUN_1800012e0
   SIG  : longlong __fastcall FUN_1800012e0(int * param_1, int param_2)
   ======================================================================== */

longlong FUN_1800012e0(int *param_1,int param_2)

{
  undefined8 *puVar1;
  longlong lVar2;
  
  if ((*(longlong *)(param_1 + 4) == 0) &&
     (puVar1 = FUN_180001000(*param_1,*(undefined8 *)(param_1 + 2),0), puVar1 != (undefined8 *)0x0))
  {
    *param_1 = *param_1 << 1;
    *(undefined8 **)(param_1 + 2) = puVar1;
    *(undefined8 **)(param_1 + 4) = puVar1 + 2;
  }
  if ((*(longlong *)(param_1 + 4) != 0) && (lVar2 = FUN_180020230(param_2), lVar2 != 0)) {
    puVar1 = *(undefined8 **)(param_1 + 4);
    *(undefined8 *)(param_1 + 4) = *puVar1;
    puVar1[1] = lVar2;
    *puVar1 = *(undefined8 *)(param_1 + 6);
    *(undefined8 **)(param_1 + 6) = puVar1;
    return lVar2;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180001360
   NAME : FUN_180001360
   SIG  : undefined __fastcall FUN_180001360(longlong param_1, longlong param_2)
   ======================================================================== */

void FUN_180001360(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  longlong *plVar2;
  longlong *plVar3;
  
  if (param_2 == 0) {
    return;
  }
  plVar1 = *(longlong **)(param_1 + 0x18);
  plVar3 = (longlong *)0x0;
  if (*(longlong **)(param_1 + 0x18) != (longlong *)0x0) {
    while (plVar2 = plVar1, plVar1 = (longlong *)*plVar2, plVar2[1] != param_2) {
      plVar3 = plVar2;
      if (plVar1 == (longlong *)0x0) {
        FUN_180020240(param_2);
        return;
      }
    }
    if (plVar3 == (longlong *)0x0) {
      *(longlong **)(param_1 + 0x18) = plVar1;
    }
    else {
      *plVar3 = (longlong)plVar1;
    }
    plVar2[1] = 0;
    *plVar2 = *(longlong *)(param_1 + 0x10);
    *(longlong **)(param_1 + 0x10) = plVar2;
  }
  FUN_180020240(param_2);
  return;
}



/* ========================================================================
   ENTRY: 180001510
   NAME : FUN_180001510
   SIG  : undefined __fastcall FUN_180001510(undefined2 * param_1, int param_2, double * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001510(undefined2 *param_1,int param_2,double *param_3,int param_4,int param_5,
                  int *param_6)

{
  double dVar1;
  double dVar2;
  float fVar3;
  
  dVar2 = DAT_180023570;
  if (param_5 != 0) {
    do {
      fVar3 = FUN_1800039d0(param_6);
      dVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (short)(int)(dVar1 * dVar2 + (double)fVar3);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001610
   NAME : FUN_180001610
   SIG  : undefined __fastcall FUN_180001610(undefined2 * param_1, int param_2, double * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001610(undefined2 *param_1,int param_2,double *param_3,int param_4,int param_5,
                  int *param_6)

{
  double dVar1;
  undefined2 uVar2;
  int iVar3;
  float fVar4;
  
  dVar1 = DAT_180023570;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar4 = FUN_1800039d0(param_6);
      iVar3 = (int)(*param_3 * dVar1 + (double)fVar4);
      if (iVar3 < -0x8000) {
        uVar2 = 0x8000;
      }
      else {
        uVar2 = (undefined2)iVar3;
        if (0x7fff < iVar3) {
          uVar2 = 0x7fff;
        }
      }
      *param_1 = uVar2;
      param_3 = param_3 + param_4;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800017d0
   NAME : FUN_1800017d0
   SIG  : undefined __fastcall FUN_1800017d0(int * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_1800017d0(int *param_1,int param_2,float *param_3,int param_4,int param_5,int *param_6)

{
  float fVar1;
  double dVar2;
  float fVar3;
  
  dVar2 = DAT_180023588;
  if (param_5 != 0) {
    do {
      fVar3 = FUN_1800039d0(param_6);
      fVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (int)((double)fVar1 * dVar2 + (double)fVar3);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800018d0
   NAME : FUN_1800018d0
   SIG  : undefined __fastcall FUN_1800018d0(int * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_1800018d0(int *param_1,int param_2,float *param_3,int param_4,int param_5,int *param_6)

{
  double dVar1;
  double dVar2;
  double dVar3;
  float fVar4;
  double dVar5;
  double dVar6;
  
  dVar3 = DAT_1800235a8;
  dVar2 = DAT_180023590;
  dVar1 = DAT_180023588;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar4 = FUN_1800039d0(param_6);
      dVar6 = (double)*param_3 * dVar1 + (double)fVar4;
      dVar5 = dVar3;
      if ((dVar3 <= dVar6) && (dVar5 = dVar2, dVar6 <= dVar2)) {
        dVar5 = dVar6;
      }
      param_3 = param_3 + param_4;
      *param_1 = (int)dVar5;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001a20
   NAME : FUN_180001a20
   SIG  : undefined __fastcall FUN_180001a20(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001a20(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  double dVar2;
  int iVar3;
  float fVar4;
  
  dVar2 = DAT_180023588;
  if (param_5 != 0) {
    do {
      fVar4 = FUN_1800039d0(param_6);
      fVar1 = *param_3;
      param_3 = param_3 + param_4;
      iVar3 = (int)((double)fVar1 * dVar2 + (double)fVar4);
      *param_1 = (char)((uint)iVar3 >> 8);
      param_1[1] = (char)((uint)iVar3 >> 0x10);
      param_1[2] = (char)((uint)iVar3 >> 0x18);
      param_1 = param_1 + param_2 * 3;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001b50
   NAME : FUN_180001b50
   SIG  : undefined __fastcall FUN_180001b50(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001b50(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  double dVar1;
  double dVar2;
  double dVar3;
  int iVar4;
  float fVar5;
  double dVar6;
  double dVar7;
  
  dVar3 = DAT_1800235a8;
  dVar2 = DAT_180023590;
  dVar1 = DAT_180023588;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar5 = FUN_1800039d0(param_6);
      dVar7 = (double)*param_3 * dVar1 + (double)fVar5;
      dVar6 = dVar3;
      if ((dVar3 <= dVar7) && (dVar6 = dVar2, dVar7 <= dVar2)) {
        dVar6 = dVar7;
      }
      iVar4 = (int)dVar6;
      param_3 = param_3 + param_4;
      *param_1 = (char)((uint)iVar4 >> 8);
      param_1[1] = (char)((uint)iVar4 >> 0x10);
      param_1[2] = (char)((uint)iVar4 >> 0x18);
      param_1 = param_1 + param_2 * 3;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001c80
   NAME : FUN_180001c80
   SIG  : undefined __fastcall FUN_180001c80(undefined2 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001c80(undefined2 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  float fVar2;
  float fVar3;
  
  fVar2 = DAT_1800235a0;
  if (param_5 != 0) {
    do {
      fVar3 = FUN_1800039d0(param_6);
      fVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (short)(int)(fVar1 * fVar2 + fVar3);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001d80
   NAME : FUN_180001d80
   SIG  : undefined __fastcall FUN_180001d80(undefined2 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001d80(undefined2 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  undefined2 uVar2;
  int iVar3;
  float fVar4;
  
  fVar1 = DAT_1800235a0;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar4 = FUN_1800039d0(param_6);
      iVar3 = (int)(*param_3 * fVar1 + fVar4);
      if (iVar3 < -0x8000) {
        uVar2 = 0x8000;
      }
      else {
        uVar2 = (undefined2)iVar3;
        if (0x7fff < iVar3) {
          uVar2 = 0x7fff;
        }
      }
      *param_1 = uVar2;
      param_3 = param_3 + param_4;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001e70
   NAME : FUN_180001e70
   SIG  : undefined __fastcall FUN_180001e70(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001e70(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  float fVar2;
  float fVar3;
  
  fVar2 = DAT_180023598;
  if (param_5 != 0) {
    do {
      fVar3 = FUN_1800039d0(param_6);
      fVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (char)(int)(fVar1 * fVar2 + fVar3);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001f70
   NAME : FUN_180001f70
   SIG  : undefined __fastcall FUN_180001f70(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180001f70(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  undefined1 uVar2;
  int iVar3;
  float fVar4;
  
  fVar1 = DAT_180023598;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar4 = FUN_1800039d0(param_6);
      iVar3 = (int)(*param_3 * fVar1 + fVar4);
      if (iVar3 < -0x80) {
        uVar2 = 0x80;
      }
      else {
        uVar2 = (undefined1)iVar3;
        if (0x7f < iVar3) {
          uVar2 = 0x7f;
        }
      }
      *param_1 = uVar2;
      param_3 = param_3 + param_4;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002060
   NAME : FUN_180002060
   SIG  : undefined __fastcall FUN_180002060(char * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002060(char *param_1,int param_2,float *param_3,int param_4,int param_5,int *param_6)

{
  float fVar1;
  float fVar2;
  float fVar3;
  
  fVar2 = DAT_180023598;
  if (param_5 != 0) {
    do {
      fVar3 = FUN_1800039d0(param_6);
      fVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (char)(int)(fVar1 * fVar2 + fVar3) + -0x80;
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800020f0
   NAME : FUN_1800020f0
   SIG  : undefined __fastcall FUN_1800020f0(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5)
   ======================================================================== */

void FUN_1800020f0(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5)

{
  float fVar1;
  int iVar2;
  
  fVar1 = DAT_1800235b0;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      iVar2 = 0x80 - (int)(*param_3 * fVar1);
      if (iVar2 < 0) {
        iVar2 = 0;
      }
      else if (0xff < iVar2) {
        iVar2 = 0xff;
      }
      *param_1 = (char)iVar2;
      param_3 = param_3 + param_4;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002160
   NAME : FUN_180002160
   SIG  : undefined __fastcall FUN_180002160(undefined1 * param_1, int param_2, float * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002160(undefined1 *param_1,int param_2,float *param_3,int param_4,int param_5,
                  int *param_6)

{
  float fVar1;
  int iVar2;
  float fVar3;
  
  fVar1 = DAT_180023598;
  if (param_5 != 0) {
    do {
      param_5 = param_5 + -1;
      fVar3 = FUN_1800039d0(param_6);
      iVar2 = (int)(*param_3 * fVar1 + fVar3) + 0x80;
      if (iVar2 < 0) {
        iVar2 = 0;
      }
      else if (0xff < iVar2) {
        iVar2 = 0xff;
      }
      *param_1 = (char)iVar2;
      param_3 = param_3 + param_4;
      param_1 = param_1 + param_2;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002300
   NAME : _guard_check_icall
   SIG  : undefined __fastcall _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
                    /* 0x2300  52  PaUtil_InitializeX86PlainConverters */
  return;
}



/* ========================================================================
   ENTRY: 180002350
   NAME : FUN_180002350
   SIG  : undefined __fastcall FUN_180002350(undefined2 * param_1, int param_2, int * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002350(undefined2 *param_1,int param_2,int *param_3,int param_4,int param_5,int *param_6
                  )

{
  int iVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      iVar2 = FUN_1800039a0(param_6);
      iVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (short)((iVar1 >> 1) + iVar2 >> 0xf);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002400
   NAME : FUN_180002400
   SIG  : undefined __fastcall FUN_180002400(undefined1 * param_1, int param_2, int * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002400(undefined1 *param_1,int param_2,int *param_3,int param_4,int param_5,int *param_6
                  )

{
  int iVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      iVar2 = FUN_1800039a0(param_6);
      iVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (char)((iVar1 >> 1) + iVar2 * 0x100 >> 0x17);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800024b0
   NAME : FUN_1800024b0
   SIG  : undefined __fastcall FUN_1800024b0(char * param_1, int param_2, int * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_1800024b0(char *param_1,int param_2,int *param_3,int param_4,int param_5,int *param_6)

{
  int iVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      iVar2 = FUN_1800039a0(param_6);
      iVar1 = *param_3;
      param_3 = param_3 + param_4;
      *param_1 = (char)((iVar1 >> 1) + iVar2 * 0x100 >> 0x17) + -0x80;
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800026a0
   NAME : FUN_1800026a0
   SIG  : undefined __fastcall FUN_1800026a0(undefined2 * param_1, int param_2, uint3 * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_1800026a0(undefined2 *param_1,int param_2,uint3 *param_3,int param_4,int param_5,
                  int *param_6)

{
  uint3 uVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      uVar1 = *param_3;
      iVar2 = FUN_1800039a0(param_6);
      param_3 = (uint3 *)((longlong)param_3 + (longlong)(param_4 * 3));
      *param_1 = (short)(iVar2 + ((int)((uint)uVar1 << 8) >> 1) >> 0xf);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002780
   NAME : FUN_180002780
   SIG  : undefined __fastcall FUN_180002780(undefined1 * param_1, int param_2, uint3 * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002780(undefined1 *param_1,int param_2,uint3 *param_3,int param_4,int param_5,
                  int *param_6)

{
  uint3 uVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      uVar1 = *param_3;
      iVar2 = FUN_1800039a0(param_6);
      param_3 = (uint3 *)((longlong)param_3 + (longlong)(param_4 * 3));
      *param_1 = (char)(iVar2 * 0x100 + ((int)((uint)uVar1 << 8) >> 1) >> 0x17);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002860
   NAME : FUN_180002860
   SIG  : undefined __fastcall FUN_180002860(char * param_1, int param_2, uint3 * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002860(char *param_1,int param_2,uint3 *param_3,int param_4,int param_5,int *param_6)

{
  uint3 uVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      uVar1 = *param_3;
      iVar2 = FUN_1800039a0(param_6);
      param_3 = (uint3 *)((longlong)param_3 + (longlong)(param_4 * 3));
      *param_1 = (char)(iVar2 * 0x100 + ((int)((uint)uVar1 << 8) >> 1) >> 0x17) + -0x80;
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002a50
   NAME : FUN_180002a50
   SIG  : undefined __fastcall FUN_180002a50(undefined1 * param_1, int param_2, short * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002a50(undefined1 *param_1,int param_2,short *param_3,int param_4,int param_5,
                  int *param_6)

{
  short sVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      sVar1 = *param_3;
      iVar2 = FUN_1800039a0(param_6);
      param_3 = param_3 + param_4;
      *param_1 = (char)(iVar2 * 0x100 + (((int)sVar1 << 0x10) >> 1) >> 0x17);
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180002b20
   NAME : FUN_180002b20
   SIG  : undefined __fastcall FUN_180002b20(char * param_1, int param_2, short * param_3, int param_4, int param_5, int * param_6)
   ======================================================================== */

void FUN_180002b20(char *param_1,int param_2,short *param_3,int param_4,int param_5,int *param_6)

{
  short sVar1;
  int iVar2;
  
  if (param_5 != 0) {
    do {
      sVar1 = *param_3;
      iVar2 = FUN_1800039a0(param_6);
      param_3 = param_3 + param_4;
      *param_1 = (char)(iVar2 * 0x100 + (((int)sVar1 << 0x10) >> 1) >> 0x17) + -0x80;
      param_1 = param_1 + param_2;
      param_5 = param_5 + -1;
    } while (param_5 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800030c0
   NAME : FUN_1800030c0
   SIG  : uint __fastcall FUN_1800030c0(uint param_1, uint param_2)
   ======================================================================== */

uint FUN_1800030c0(uint param_1,uint param_2)

{
  uint uVar1;
  uint uVar2;
  uint uVar3;
  bool bVar4;
  
  uVar3 = param_2 & 0x7fffffff;
  if ((param_2 & param_1 & 0x7fffffff) == 0) {
    uVar1 = uVar3;
    if (uVar3 != 1) {
      do {
        uVar2 = uVar1 >> 1;
        if ((uVar2 & param_1) != 0) {
          if (1 < uVar1) {
            return uVar2;
          }
          break;
        }
        bVar4 = 1 < uVar1;
        uVar1 = uVar2;
      } while (bVar4);
    }
    while (uVar3 = uVar3 * 2, (param_1 & uVar3 & 0x7fffffff) == 0) {
      if (uVar3 == 0x10000) {
        return 0xffffd8f6;
      }
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180003120
   NAME : FUN_180003120
   SIG  : undefined * __fastcall FUN_180003120(uint param_1, uint param_2, uint param_3)
   ======================================================================== */

undefined * FUN_180003120(uint param_1,uint param_2,uint param_3)

{
  undefined *puVar1;
  
  switch(param_1 & 0x7fffffff) {
  case 1:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b248;
    case 2:
      return PTR_LAB_18002b000;
    case 4:
      return PTR_LAB_18002b008;
    case 8:
      return PTR_LAB_18002b010;
    case 0x10:
      return PTR_LAB_18002b018;
    case 0x20:
      return PTR_LAB_18002b038;
    case 0x40:
      return PTR_LAB_18002b040;
    }
    break;
  case 2:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b048;
    case 2:
switchD_1800032e9_caseD_4:
      return PTR_LAB_18002b240;
    case 4:
      if ((param_3 & 1) != 0) {
        puVar1 = PTR_FUN_18002b058;
        if ((param_3 & 2) != 0) {
          puVar1 = PTR_LAB_18002b050;
        }
        return puVar1;
      }
      puVar1 = PTR_FUN_18002b068;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b060;
      }
      return puVar1;
    case 8:
      if ((param_3 & 1) != 0) {
        puVar1 = PTR_FUN_18002b078;
        if ((param_3 & 2) != 0) {
          puVar1 = PTR_LAB_18002b070;
        }
        return puVar1;
      }
      puVar1 = PTR_FUN_18002b088;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b080;
      }
      return puVar1;
    case 0x10:
      if ((param_3 & 1) != 0) {
        puVar1 = PTR_FUN_18002b098;
        if ((param_3 & 2) != 0) {
          puVar1 = PTR_LAB_18002b090;
        }
        return puVar1;
      }
      puVar1 = PTR_FUN_18002b0a8;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b0a0;
      }
      return puVar1;
    case 0x20:
      if ((param_3 & 1) != 0) {
        puVar1 = PTR_FUN_18002b0b8;
        if ((param_3 & 2) != 0) {
          puVar1 = PTR_LAB_18002b0b0;
        }
        return puVar1;
      }
      puVar1 = PTR_FUN_18002b0c8;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b0c0;
      }
      return puVar1;
    case 0x40:
      if ((param_3 & 1) != 0) {
        puVar1 = PTR_FUN_18002b0d8;
        if ((param_3 & 2) != 0) {
          puVar1 = PTR_LAB_18002b0d0;
        }
        return puVar1;
      }
      puVar1 = PTR_FUN_18002b0e8;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_FUN_18002b0e0;
      }
      return puVar1;
    }
    break;
  case 4:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b0f0;
    case 2:
      return PTR_LAB_18002b0f8;
    case 4:
      goto switchD_1800032e9_caseD_4;
    case 8:
      puVar1 = PTR__guard_check_icall_18002b108;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b100;
      }
      return puVar1;
    case 0x10:
      puVar1 = PTR_FUN_18002b118;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b110;
      }
      return puVar1;
    case 0x20:
      puVar1 = PTR_FUN_18002b128;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b120;
      }
      return puVar1;
    case 0x40:
      puVar1 = PTR_FUN_18002b138;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b130;
      }
      return puVar1;
    }
    break;
  case 8:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b140;
    case 2:
      return PTR_LAB_18002b148;
    case 4:
      return PTR_LAB_18002b150;
    case 8:
      return PTR_LAB_18002b238;
    case 0x10:
      puVar1 = PTR_FUN_18002b160;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b158;
      }
      return puVar1;
    case 0x20:
      puVar1 = PTR_FUN_18002b170;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b168;
      }
      return puVar1;
    case 0x40:
      puVar1 = PTR_FUN_18002b180;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b178;
      }
      return puVar1;
    }
    break;
  case 0x10:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b188;
    case 2:
      return PTR_LAB_18002b190;
    case 4:
      return PTR_LAB_18002b198;
    case 8:
      return PTR_LAB_18002b1a0;
    case 0x10:
      return PTR_LAB_18002b230;
    case 0x20:
      puVar1 = PTR_FUN_18002b1b0;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b1a8;
      }
      return puVar1;
    case 0x40:
      puVar1 = PTR_FUN_18002b1c0;
      if ((param_3 & 2) != 0) {
        puVar1 = PTR_LAB_18002b1b8;
      }
      return puVar1;
    }
    break;
  case 0x20:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b1c8;
    case 2:
      return PTR_LAB_18002b1d0;
    case 4:
      return PTR_LAB_18002b1d8;
    case 8:
      return PTR_LAB_18002b1e0;
    case 0x10:
      return PTR_LAB_18002b1e8;
    case 0x20:
switchD_1800034bd_caseD_40:
      return PTR_LAB_18002b228;
    case 0x40:
      return PTR_LAB_18002b1f0;
    }
    break;
  case 0x40:
    switch(param_2 & 0x7fffffff) {
    case 1:
      return PTR_LAB_18002b1f8;
    case 2:
      return PTR_LAB_18002b200;
    case 4:
      return PTR_LAB_18002b208;
    case 8:
      return PTR_LAB_18002b210;
    case 0x10:
      return PTR_LAB_18002b218;
    case 0x20:
      return PTR_LAB_18002b220;
    case 0x40:
      goto switchD_1800034bd_caseD_40;
    }
  }
  return (undefined *)0x0;
}



/* ========================================================================
   ENTRY: 180003800
   NAME : FUN_180003800
   SIG  : undefined * __fastcall FUN_180003800(uint param_1)
   ======================================================================== */

undefined * FUN_180003800(uint param_1)

{
  switch(param_1 & 0x7fffffff) {
  case 1:
    return PTR_LAB_18002b278;
  case 2:
  case 4:
    return PTR_LAB_18002b270;
  default:
    return (undefined *)0x0;
  case 8:
    return PTR_LAB_18002b268;
  case 0x10:
    return PTR_LAB_18002b260;
  case 0x20:
    return PTR_LAB_18002b258;
  case 0x40:
    return PTR_LAB_18002b250;
  }
}



/* ========================================================================
   ENTRY: 1800038c0
   NAME : FUN_1800038c0
   SIG  : undefined __fastcall FUN_1800038c0(longlong param_1)
   ======================================================================== */

void FUN_1800038c0(longlong param_1)

{
  double dVar1;
  
  dVar1 = FUN_180020250();
  *(double *)(param_1 + 8) = dVar1;
  return;
}



/* ========================================================================
   ENTRY: 1800038e0
   NAME : FUN_1800038e0
   SIG  : undefined __fastcall FUN_1800038e0(double * param_1, uint param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800038e0(double *param_1,uint param_2)

{
  double dVar1;
  
  if (param_2 != 0) {
    dVar1 = FUN_180020250();
    param_1[2] = ((dVar1 - param_1[1]) / ((double)param_2 * *param_1)) * _DAT_1800235b8 +
                 param_1[2] * _DAT_1800235c0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180003940
   NAME : FUN_180003940
   SIG  : undefined8 __fastcall FUN_180003940(longlong param_1)
   ======================================================================== */

undefined8 FUN_180003940(longlong param_1)

{
  return *(undefined8 *)(param_1 + 0x10);
}



/* ========================================================================
   ENTRY: 180003950
   NAME : FUN_180003950
   SIG  : undefined __fastcall FUN_180003950(double * param_1, double param_2)
   ======================================================================== */

void FUN_180003950(double *param_1,double param_2)

{
  double dVar1;
  
  dVar1 = DAT_1800235c8 / param_2;
  param_1[2] = 0.0;
  *param_1 = dVar1;
  return;
}



/* ========================================================================
   ENTRY: 180003970
   NAME : FUN_180003970
   SIG  : undefined __fastcall FUN_180003970(longlong param_1)
   ======================================================================== */

void FUN_180003970(longlong param_1)

{
  *(undefined8 *)(param_1 + 0x10) = 0;
  return;
}



/* ========================================================================
   ENTRY: 180003980
   NAME : PaUtil_SetDebugPrintFunction
   SIG  : undefined __fastcall PaUtil_SetDebugPrintFunction(undefined8 param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void PaUtil_SetDebugPrintFunction(undefined8 param_1)

{
                    /* 0x3980  55  PaUtil_SetDebugPrintFunction */
  _DAT_18002b690 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180003990
   NAME : FUN_180003990
   SIG  : undefined * __fastcall FUN_180003990(void)
   ======================================================================== */

undefined * FUN_180003990(void)

{
  return &DAT_18002bc68;
}



/* ========================================================================
   ENTRY: 1800039a0
   NAME : FUN_1800039a0
   SIG  : int __fastcall FUN_1800039a0(int * param_1)
   ======================================================================== */

int FUN_1800039a0(int *param_1)

{
  int iVar1;
  int iVar2;
  
  iVar2 = param_1[1] * 0xbb38435 + 0x3619636b;
  iVar1 = param_1[2] * 0xbb38435 + 0x3619636b;
  param_1[1] = iVar2;
  param_1[2] = iVar1;
  iVar2 = (iVar2 >> 0x12) + (iVar1 >> 0x12);
  iVar1 = *param_1;
  *param_1 = iVar2;
  return iVar2 - iVar1;
}



/* ========================================================================
   ENTRY: 1800039d0
   NAME : FUN_1800039d0
   SIG  : float __fastcall FUN_1800039d0(int * param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

float FUN_1800039d0(int *param_1)

{
  int iVar1;
  int iVar2;
  
  iVar2 = param_1[1] * 0xbb38435 + 0x3619636b;
  iVar1 = param_1[2] * 0xbb38435 + 0x3619636b;
  param_1[1] = iVar2;
  param_1[2] = iVar1;
  iVar2 = (iVar1 >> 0x12) + (iVar2 >> 0x12);
  iVar1 = *param_1;
  *param_1 = iVar2;
  return (float)(iVar2 - iVar1) * _DAT_1800235d0;
}



/* ========================================================================
   ENTRY: 180003a10
   NAME : FUN_180003a10
   SIG  : undefined __fastcall FUN_180003a10(undefined4 * param_1)
   ======================================================================== */

void FUN_180003a10(undefined4 *param_1)

{
  *param_1 = 0;
  param_1[1] = 0x56ce;
  param_1[2] = 0x54c563;
  return;
}



/* ========================================================================
   ENTRY: 180003a30
   NAME : FUN_180003a30
   SIG  : undefined __fastcall FUN_180003a30(longlong param_1)
   ======================================================================== */

void FUN_180003a30(longlong param_1)

{
  *(longlong *)(param_1 + 8) = DAT_18002baa8;
  DAT_18002baa8 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180003a50
   NAME : FUN_180003a50
   SIG  : undefined __fastcall FUN_180003a50(void)
   ======================================================================== */

void FUN_180003a50(void)

{
  while (DAT_18002baa8 != (int *)0x0) {
    Pa_CloseStream(DAT_18002baa8);
  }
  return;
}



/* ========================================================================
   ENTRY: 180003a80
   NAME : FUN_180003a80
   SIG  : undefined __fastcall FUN_180003a80(void)
   ======================================================================== */

void FUN_180003a80(void)

{
  undefined *puVar1;
  int iVar2;
  
  iVar2 = 0;
  puVar1 = PTR_FUN_18002b500;
  while (puVar1 != (undefined *)0x0) {
    iVar2 = iVar2 + 1;
    puVar1 = (&PTR_FUN_18002b500)[iVar2];
  }
  return;
}



/* ========================================================================
   ENTRY: 180003aa0
   NAME : FUN_180003aa0
   SIG  : int __fastcall FUN_180003aa0(int param_1, int * param_2)
   ======================================================================== */

int FUN_180003aa0(int param_1,int *param_2)

{
  int iVar1;
  int iVar2;
  
  if (((DAT_18002bac0 != 0) && (-1 < param_1)) && (iVar2 = 0, 0 < DAT_18002bab8)) {
    do {
      iVar1 = *(int *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar2 * 8) + 0x18);
      if (param_1 < iVar1) {
        if (DAT_18002bab8 <= iVar2) {
          return -1;
        }
        if (param_2 != (int *)0x0) {
          *param_2 = param_1;
          return iVar2;
        }
        return iVar2;
      }
      param_1 = param_1 - iVar1;
      iVar2 = iVar2 + 1;
    } while (iVar2 < DAT_18002bab8);
  }
  return -1;
}



/* ========================================================================
   ENTRY: 180003b00
   NAME : FUN_180003b00
   SIG  : int __fastcall FUN_180003b00(void)
   ======================================================================== */

int FUN_180003b00(void)

{
  int *piVar1;
  int iVar2;
  int iVar3;
  longlong lVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  uint uVar7;
  ulonglong uVar8;
  
  iVar2 = FUN_180003a80();
  lVar4 = FUN_180020230(iVar2 * 8);
  DAT_18002bab0 = lVar4;
  if (lVar4 != 0) {
    uVar5 = 0;
    DAT_18002babc = 0xffffffff;
    DAT_18002bac8 = 0;
    DAT_18002bab8 = 0;
    uVar6 = uVar5;
    uVar8 = uVar5;
    if (0 < iVar2) {
      do {
        *(undefined8 *)(lVar4 + (longlong)(int)uVar5 * 8) = 0;
        iVar3 = (*(code *)(&PTR_FUN_18002b500)[uVar8])();
        lVar4 = DAT_18002bab0;
        if (iVar3 != 0) goto LAB_180003b34;
        uVar5 = (ulonglong)(int)DAT_18002bab8;
        piVar1 = *(int **)(DAT_18002bab0 + uVar5 * 8);
        if (piVar1 != (int *)0x0) {
          if ((DAT_18002babc == 0xffffffff) && ((piVar1[7] != -1 || (piVar1[8] != -1)))) {
            DAT_18002babc = DAT_18002bab8;
          }
          iVar3 = (int)uVar6;
          *piVar1 = iVar3;
          if (piVar1[7] != -1) {
            piVar1[7] = piVar1[7] + iVar3;
          }
          if (piVar1[8] != -1) {
            piVar1[8] = piVar1[8] + iVar3;
          }
          uVar6 = (ulonglong)(uint)(iVar3 + piVar1[6]);
          DAT_18002bac8 = DAT_18002bac8 + piVar1[6];
          DAT_18002bab8 = DAT_18002bab8 + 1;
          uVar5 = (ulonglong)DAT_18002bab8;
        }
        uVar7 = (int)uVar8 + 1;
        uVar8 = (ulonglong)uVar7;
      } while ((int)uVar7 < iVar2);
      if (DAT_18002babc != 0xffffffff) {
        return 0;
      }
    }
    DAT_18002babc = 0;
    return 0;
  }
  iVar3 = -0x2708;
LAB_180003b34:
  FUN_180004a90();
  return iVar3;
}



/* ========================================================================
   ENTRY: 180003c20
   NAME : FUN_180003c20
   SIG  : undefined8 __fastcall FUN_180003c20(int * param_1, int param_2, int * param_3)
   ======================================================================== */

undefined8 FUN_180003c20(int *param_1,int param_2,int *param_3)

{
  int iVar1;
  
  iVar1 = param_2 - *param_3;
  if ((-1 < iVar1) && (iVar1 < param_3[6])) {
    *param_1 = iVar1;
    return 0;
  }
  return 0xffffd8f4;
}



/* ========================================================================
   ENTRY: 180003c40
   NAME : FUN_180003c40
   SIG  : undefined8 __fastcall FUN_180003c40(longlong * param_1, int param_2)
   ======================================================================== */

undefined8 FUN_180003c40(longlong *param_1,int param_2)

{
  longlong lVar1;
  int iVar2;
  
  if (DAT_18002bac0 == 0) {
    return 0xffffd8f0;
  }
  iVar2 = 0;
  if (0 < DAT_18002bab8) {
    do {
      lVar1 = *(longlong *)(DAT_18002bab0 + (longlong)iVar2 * 8);
      if (*(int *)(lVar1 + 0xc) == param_2) {
        *param_1 = lVar1;
        return 0;
      }
      iVar2 = iVar2 + 1;
    } while (iVar2 < DAT_18002bab8);
  }
  return 0xffffd905;
}



/* ========================================================================
   ENTRY: 180003c90
   NAME : FUN_180003c90
   SIG  : undefined __fastcall FUN_180003c90(undefined4 param_1, undefined4 param_2, char * param_3)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180003c90(undefined4 param_1,undefined4 param_2,char *param_3)

{
  _DAT_18002b2a0 = param_1;
  _DAT_18002b2a4 = param_2;
  strncpy(&DAT_18002b6a0,param_3,0x400);
  return;
}



/* ========================================================================
   ENTRY: 180003cc0
   NAME : FUN_180003cc0
   SIG  : undefined8 __fastcall FUN_180003cc0(int * param_1)
   ======================================================================== */

undefined8 FUN_180003cc0(int *param_1)

{
  undefined8 uVar1;
  
  if (DAT_18002bac0 == 0) {
    return 0xffffd8f0;
  }
  if (param_1 != (int *)0x0) {
    uVar1 = 0;
    if (*param_1 != 0x18273645) {
      uVar1 = 0xffffd8fc;
    }
    return uVar1;
  }
  return 0xffffd8fc;
}



/* ========================================================================
   ENTRY: 180003cf0
   NAME : Pa_AbortStream
   SIG  : undefined8 __fastcall Pa_AbortStream(int * param_1)
   ======================================================================== */

undefined8 Pa_AbortStream(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x3cf0  23  Pa_AbortStream */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180003d1c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x18))(param_1);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180003d30
   NAME : Pa_CloseStream
   SIG  : ulonglong __fastcall Pa_CloseStream(int * param_1)
   ======================================================================== */

ulonglong Pa_CloseStream(int *param_1)

{
  undefined8 *puVar1;
  undefined8 uVar2;
  ulonglong uVar3;
  uint uVar4;
  
                    /* 0x3d30  19  Pa_CloseStream */
  uVar2 = FUN_180003cc0(param_1);
  uVar4 = (uint)uVar2;
  FUN_180004a10((longlong)param_1);
  if (uVar4 != 0) {
    return (ulonglong)uVar4;
  }
  puVar1 = *(undefined8 **)(param_1 + 4);
  uVar3 = (*(code *)puVar1[4])();
  if ((int)uVar3 != 1) {
    if ((int)uVar3 != 0) {
      return uVar3;
    }
    uVar3 = (*(code *)puVar1[3])(param_1);
    if ((int)uVar3 != 0) {
      return uVar3;
    }
  }
  uVar3 = (*(code *)*puVar1)(param_1);
  return uVar3;
}



/* ========================================================================
   ENTRY: 180003d90
   NAME : Pa_GetDefaultHostApi
   SIG  : int __fastcall Pa_GetDefaultHostApi(void)
   ======================================================================== */

int Pa_GetDefaultHostApi(void)

{
  int iVar1;
  
                    /* 0x3d90  7  Pa_GetDefaultHostApi */
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  if ((DAT_18002babc < 0) || (iVar1 = DAT_18002babc, DAT_18002bab8 <= DAT_18002babc)) {
    iVar1 = -0x2702;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 180003dc0
   NAME : Pa_GetDefaultInputDevice
   SIG  : undefined4 __fastcall Pa_GetDefaultInputDevice(void)
   ======================================================================== */

undefined4 Pa_GetDefaultInputDevice(void)

{
  int iVar1;
  
                    /* 0x3dc0  13  Pa_GetDefaultInputDevice */
  iVar1 = Pa_GetDefaultHostApi();
  if (iVar1 < 0) {
    return 0xffffffff;
  }
  return *(undefined4 *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x1c);
}



/* ========================================================================
   ENTRY: 180003df0
   NAME : Pa_GetDefaultOutputDevice
   SIG  : undefined4 __fastcall Pa_GetDefaultOutputDevice(void)
   ======================================================================== */

undefined4 Pa_GetDefaultOutputDevice(void)

{
  int iVar1;
  
                    /* 0x3df0  14  Pa_GetDefaultOutputDevice */
  iVar1 = Pa_GetDefaultHostApi();
  if (iVar1 < 0) {
    return 0xffffffff;
  }
  return *(undefined4 *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x20);
}



/* ========================================================================
   ENTRY: 180003e20
   NAME : Pa_GetDeviceCount
   SIG  : undefined4 __fastcall Pa_GetDeviceCount(void)
   ======================================================================== */

undefined4 Pa_GetDeviceCount(void)

{
  undefined4 uVar1;
  
                    /* 0x3e20  12  Pa_GetDeviceCount */
  uVar1 = DAT_18002bac8;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180003e40
   NAME : Pa_GetDeviceInfo
   SIG  : undefined8 __fastcall Pa_GetDeviceInfo(int param_1)
   ======================================================================== */

undefined8 Pa_GetDeviceInfo(int param_1)

{
  int iVar1;
  int local_res10 [6];
  
                    /* 0x3e40  15  Pa_GetDeviceInfo */
  iVar1 = FUN_180003aa0(param_1,local_res10);
  if (iVar1 < 0) {
    return 0;
  }
  return *(undefined8 *)
          (*(longlong *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x28) +
          (longlong)local_res10[0] * 8);
}



/* ========================================================================
   ENTRY: 180003e80
   NAME : Pa_GetErrorText
   SIG  : char * __fastcall Pa_GetErrorText(int param_1)
   ======================================================================== */

char * Pa_GetErrorText(int param_1)

{
                    /* 0x3e80  3  Pa_GetErrorText */
  if (0 < param_1) {
    return "Invalid error code (value greater than zero)";
  }
  if (param_1 == 0) {
    return "Success";
  }
  switch(param_1) {
  case -10000:
    return "PortAudio not initialized";
  case -9999:
    return "Unanticipated host error";
  case -0x270e:
    return "Invalid number of channels";
  case -0x270d:
    return "Invalid sample rate";
  case -0x270c:
    return "Invalid device";
  case -0x270b:
    return "Invalid flag";
  case -0x270a:
    return "Sample format not supported";
  case -0x2709:
    return "Illegal combination of I/O devices";
  case -0x2708:
    return "Insufficient memory";
  case -0x2707:
    return "Buffer too big";
  case -0x2706:
    return "Buffer too small";
  case -0x2705:
    return "No callback routine specified";
  case -0x2704:
    return "Invalid stream pointer";
  case -0x2703:
    return "Wait timed out";
  case -0x2702:
    return "Internal PortAudio error";
  case -0x2701:
    return "Device unavailable";
  case -0x2700:
    return "Incompatible host API specific stream info";
  case -0x26ff:
    return "Stream is stopped";
  case -0x26fe:
    return "Stream is not stopped";
  case -0x26fd:
    return "Input overflowed";
  case -0x26fc:
    return "Output underflowed";
  case -0x26fb:
    return "Host API not found";
  case -0x26fa:
    return "Invalid host API";
  case -0x26f9:
    return "Can\'t read from a callback stream";
  case -0x26f8:
    return "Can\'t write to a callback stream";
  case -0x26f7:
    return "Can\'t read from an output only stream";
  case -0x26f6:
    return "Can\'t write to an input only stream";
  case -0x26f5:
    return "Incompatible stream host API";
  case -0x26f4:
    return "Bad buffer pointer";
  case -0x26f3:
    return "PortAudio can not be initialized recursively";
  default:
    return "Invalid error code";
  }
}



/* ========================================================================
   ENTRY: 180004040
   NAME : Pa_GetHostApiCount
   SIG  : undefined4 __fastcall Pa_GetHostApiCount(void)
   ======================================================================== */

undefined4 Pa_GetHostApiCount(void)

{
  undefined4 uVar1;
  
                    /* 0x4040  6  Pa_GetHostApiCount */
  uVar1 = DAT_18002bab8;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004060
   NAME : Pa_GetHostApiInfo
   SIG  : longlong __fastcall Pa_GetHostApiInfo(int param_1)
   ======================================================================== */

longlong Pa_GetHostApiInfo(int param_1)

{
                    /* 0x4060  8  Pa_GetHostApiInfo */
  if (((DAT_18002bac0 != 0) && (-1 < param_1)) && (param_1 < DAT_18002bab8)) {
    return *(longlong *)(DAT_18002bab0 + (longlong)param_1 * 8) + 8;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180004090
   NAME : Pa_GetLastHostErrorInfo
   SIG  : undefined * __fastcall Pa_GetLastHostErrorInfo(void)
   ======================================================================== */

undefined * Pa_GetLastHostErrorInfo(void)

{
                    /* 0x4090  11  Pa_GetLastHostErrorInfo */
  return &DAT_18002b2a0;
}



/* ========================================================================
   ENTRY: 1800040a0
   NAME : Pa_GetSampleSize
   SIG  : undefined8 __fastcall Pa_GetSampleSize(uint param_1)
   ======================================================================== */

undefined8 Pa_GetSampleSize(uint param_1)

{
                    /* 0x40a0  33  Pa_GetSampleSize */
  switch(param_1 & 0x7fffffff) {
  case 1:
    return 8;
  case 2:
  case 4:
    return 4;
  default:
    return 0xffffd8f6;
  case 8:
    return 3;
  case 0x10:
    return 2;
  case 0x20:
  case 0x40:
    return 1;
  }
}



/* ========================================================================
   ENTRY: 180004150
   NAME : Pa_GetStreamCpuLoad
   SIG  : undefined8 __fastcall Pa_GetStreamCpuLoad(int * param_1)
   ======================================================================== */

undefined8 Pa_GetStreamCpuLoad(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x4150  28  Pa_GetStreamCpuLoad */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004177. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x38))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004180
   NAME : Pa_GetStreamInfo
   SIG  : int * __fastcall Pa_GetStreamInfo(int * param_1)
   ======================================================================== */

int * Pa_GetStreamInfo(int *param_1)

{
  undefined8 uVar1;
  int *piVar2;
  
                    /* 0x4180  26  Pa_GetStreamInfo */
  uVar1 = FUN_180003cc0(param_1);
  piVar2 = (int *)0x0;
  if ((int)uVar1 == 0) {
    piVar2 = param_1 + 0xc;
  }
  return piVar2;
}



/* ========================================================================
   ENTRY: 1800041b0
   NAME : Pa_GetStreamReadAvailable
   SIG  : undefined8 __fastcall Pa_GetStreamReadAvailable(int * param_1)
   ======================================================================== */

undefined8 Pa_GetStreamReadAvailable(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x41b0  31  Pa_GetStreamReadAvailable */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800041d6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x50))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800041e0
   NAME : Pa_GetStreamTime
   SIG  : undefined8 __fastcall Pa_GetStreamTime(int * param_1)
   ======================================================================== */

undefined8 Pa_GetStreamTime(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x41e0  27  Pa_GetStreamTime */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004207. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x30))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004210
   NAME : Pa_GetStreamWriteAvailable
   SIG  : undefined8 __fastcall Pa_GetStreamWriteAvailable(int * param_1)
   ======================================================================== */

undefined8 Pa_GetStreamWriteAvailable(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x4210  32  Pa_GetStreamWriteAvailable */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004236. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x58))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004240
   NAME : Pa_GetVersion
   SIG  : undefined8 __fastcall Pa_GetVersion(void)
   ======================================================================== */

undefined8 Pa_GetVersion(void)

{
                    /* 0x4240  1  Pa_GetVersion */
  return 0x130700;
}



/* ========================================================================
   ENTRY: 180004250
   NAME : Pa_GetVersionText
   SIG  : char * __fastcall Pa_GetVersionText(void)
   ======================================================================== */

char * Pa_GetVersionText(void)

{
                    /* 0x4250  2  Pa_GetVersionText */
  return "PortAudio V19.7.0-devel, revision unknown";
}



/* ========================================================================
   ENTRY: 180004260
   NAME : Pa_HostApiDeviceIndexToDeviceIndex
   SIG  : int __fastcall Pa_HostApiDeviceIndexToDeviceIndex(int param_1, int param_2)
   ======================================================================== */

int Pa_HostApiDeviceIndexToDeviceIndex(int param_1,int param_2)

{
  int *piVar1;
  
                    /* 0x4260  10  Pa_HostApiDeviceIndexToDeviceIndex */
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  if ((-1 < param_1) && (param_1 < DAT_18002bab8)) {
    if ((-1 < param_2) &&
       (piVar1 = *(int **)(DAT_18002bab0 + (longlong)param_1 * 8), param_2 < piVar1[6])) {
      return *piVar1 + param_2;
    }
    return -0x270c;
  }
  return -0x26fa;
}



/* ========================================================================
   ENTRY: 1800042b0
   NAME : Pa_HostApiTypeIdToHostApiIndex
   SIG  : int __fastcall Pa_HostApiTypeIdToHostApiIndex(int param_1)
   ======================================================================== */

int Pa_HostApiTypeIdToHostApiIndex(int param_1)

{
  int iVar1;
  
                    /* 0x42b0  9  Pa_HostApiTypeIdToHostApiIndex */
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  iVar1 = 0;
  if (0 < DAT_18002bab8) {
    do {
      if (*(int *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0xc) == param_1) {
        return iVar1;
      }
      iVar1 = iVar1 + 1;
    } while (iVar1 < DAT_18002bab8);
  }
  return -0x26fb;
}



/* ========================================================================
   ENTRY: 180004300
   NAME : Pa_Initialize
   SIG  : int __fastcall Pa_Initialize(void)
   ======================================================================== */

int Pa_Initialize(void)

{
  int iVar1;
  
                    /* 0x4300  4  Pa_Initialize */
  if (DAT_18002bac0 != 0) {
    DAT_18002bac0 = DAT_18002bac0 + 1;
    return 0;
  }
  if (DAT_18002bac4 != 0) {
    return -0x26f3;
  }
  DAT_18002bac4 = 1;
  FUN_1800202a0();
  iVar1 = FUN_180003b00();
  if (iVar1 == 0) {
    DAT_18002bac0 = DAT_18002bac0 + 1;
  }
  DAT_18002bac4 = 0;
  return iVar1;
}



/* ========================================================================
   ENTRY: 180004360
   NAME : Pa_IsFormatSupported
   SIG  : undefined8 __fastcall Pa_IsFormatSupported(int * param_1, int * param_2, double param_3)
   ======================================================================== */

undefined8 Pa_IsFormatSupported(int *param_1,int *param_2,double param_3)

{
  undefined8 uVar1;
  int *piVar2;
  int *piVar3;
  int local_res20 [2];
  int local_68 [2];
  longlong local_60;
  int local_58;
  int local_54;
  int local_50;
  undefined8 local_48;
  undefined8 local_40;
  int local_38;
  int local_34;
  int local_30;
  undefined8 local_28;
  undefined8 local_20;
  
                    /* 0x4360  16  Pa_IsFormatSupported */
  local_60 = 0;
  local_res20[0] = -1;
  local_68[0] = -1;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  else {
    uVar1 = FUN_180004b00(param_1,param_2,param_3,0,0,0,&local_60,local_res20,local_68);
    if ((int)uVar1 == 0) {
      if (param_1 == (int *)0x0) {
        piVar2 = (int *)0x0;
      }
      else {
        piVar2 = &local_58;
        local_48 = *(undefined8 *)(param_1 + 4);
        local_58 = local_res20[0];
        local_54 = param_1[1];
        local_50 = param_1[2];
        local_40 = *(undefined8 *)(param_1 + 6);
      }
      if (param_2 == (int *)0x0) {
        piVar3 = (int *)0x0;
      }
      else {
        piVar3 = &local_38;
        local_28 = *(undefined8 *)(param_2 + 4);
        local_38 = local_68[0];
        local_34 = param_2[1];
        local_30 = param_2[2];
        local_20 = *(undefined8 *)(param_2 + 6);
      }
      uVar1 = (**(code **)(local_60 + 0x40))(local_60,piVar2,piVar3,SUB84(param_3,0));
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800044a0
   NAME : Pa_IsStreamActive
   SIG  : undefined __fastcall Pa_IsStreamActive(int * param_1)
   ======================================================================== */

void Pa_IsStreamActive(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x44a0  25  Pa_IsStreamActive */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800044be. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(*(longlong *)(param_1 + 4) + 0x28))(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800044d0
   NAME : Pa_IsStreamStopped
   SIG  : undefined __fastcall Pa_IsStreamStopped(int * param_1)
   ======================================================================== */

void Pa_IsStreamStopped(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x44d0  24  Pa_IsStreamStopped */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800044ee. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180004500
   NAME : Pa_OpenDefaultStream
   SIG  : ulonglong __fastcall Pa_OpenDefaultStream(longlong * param_1, int param_2, int param_3, undefined4 param_4, double param_5, int param_6, longlong param_7, undefined8 param_8)
   ======================================================================== */

ulonglong Pa_OpenDefaultStream
                    (longlong *param_1,int param_2,int param_3,undefined4 param_4,double param_5,
                    int param_6,longlong param_7,undefined8 param_8)

{
  longlong lVar1;
  ulonglong uVar2;
  int *piVar3;
  int *piVar4;
  int local_68;
  int local_64;
  undefined4 local_60;
  undefined8 local_58;
  undefined8 local_50;
  int local_48;
  int local_44;
  undefined4 local_40;
  undefined8 local_38;
  undefined8 local_30;
  
                    /* 0x4500  18  Pa_OpenDefaultStream */
  if (param_2 < 1) {
    piVar4 = (int *)0x0;
  }
  else {
    local_68 = Pa_GetDefaultInputDevice();
    if (local_68 == -1) {
      return 0xffffd8ff;
    }
    local_64 = param_2;
    local_60 = param_4;
    lVar1 = Pa_GetDeviceInfo(local_68);
    piVar4 = &local_68;
    local_50 = 0;
    local_58 = *(undefined8 *)(lVar1 + 0x30);
  }
  piVar3 = (int *)0x0;
  if (0 < param_3) {
    local_48 = Pa_GetDefaultOutputDevice();
    if (local_48 == -1) {
      return 0xffffd8ff;
    }
    local_44 = param_3;
    local_40 = param_4;
    lVar1 = Pa_GetDeviceInfo(local_48);
    local_30 = 0;
    piVar3 = &local_48;
    local_38 = *(undefined8 *)(lVar1 + 0x38);
  }
  uVar2 = Pa_OpenStream(param_1,piVar4,piVar3,param_5,param_6,0,param_7,param_8);
  return uVar2;
}



/* ========================================================================
   ENTRY: 1800045f0
   NAME : Pa_OpenStream
   SIG  : ulonglong __fastcall Pa_OpenStream(longlong * param_1, int * param_2, int * param_3, double param_4, int param_5, uint param_6, longlong param_7, undefined8 param_8)
   ======================================================================== */

ulonglong Pa_OpenStream(longlong *param_1,int *param_2,int *param_3,double param_4,int param_5,
                       uint param_6,longlong param_7,undefined8 param_8)

{
  uint uVar1;
  ulonglong uVar2;
  int *piVar3;
  int *piVar4;
  int local_78;
  int local_74;
  longlong local_70;
  int local_68;
  int local_64;
  int local_60;
  undefined8 local_58;
  undefined8 local_50;
  int local_48;
  int local_44;
  int local_40;
  undefined8 local_38;
  undefined8 local_30;
  
                    /* 0x45f0  17  Pa_OpenStream */
  local_70 = 0;
  local_78 = -1;
  local_74 = -1;
  if (DAT_18002bac0 == 0) {
    uVar2 = 0xffffd8f0;
  }
  else if (param_1 == (longlong *)0x0) {
    uVar2 = 0xffffd8fc;
  }
  else {
    uVar2 = FUN_180004b00(param_2,param_3,param_4,param_5,param_6,param_7,&local_70,&local_78,
                          &local_74);
    if ((int)uVar2 == 0) {
      if (param_2 == (int *)0x0) {
        piVar3 = (int *)0x0;
      }
      else {
        piVar3 = &local_68;
        local_58 = *(undefined8 *)(param_2 + 4);
        local_68 = local_78;
        local_64 = param_2[1];
        local_60 = param_2[2];
        local_50 = *(undefined8 *)(param_2 + 6);
      }
      if (param_3 == (int *)0x0) {
        piVar4 = (int *)0x0;
      }
      else {
        piVar4 = &local_48;
        local_38 = *(undefined8 *)(param_3 + 4);
        local_48 = local_74;
        local_44 = param_3[1];
        local_40 = param_3[2];
        local_30 = *(undefined8 *)(param_3 + 6);
      }
      uVar1 = (**(code **)(local_70 + 0x38))
                        (local_70,param_1,piVar3,piVar4,param_4,param_5,param_6,param_7,param_8);
      uVar2 = (ulonglong)uVar1;
      if (uVar1 == 0) {
        FUN_180003a30(*param_1);
      }
      uVar2 = uVar2 & 0xffffffff;
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 1800047b0
   NAME : Pa_ReadStream
   SIG  : undefined8 __fastcall Pa_ReadStream(int * param_1, longlong param_2, int param_3)
   ======================================================================== */

undefined8 Pa_ReadStream(int *param_1,longlong param_2,int param_3)

{
  undefined8 uVar1;
  
                    /* 0x47b0  29  Pa_ReadStream */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    if (param_3 == 0) {
      return uVar1;
    }
    if (param_2 == 0) {
      return 0xffffd90c;
    }
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180004829. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x40))(param_1,param_2,param_3);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004850
   NAME : Pa_SetStreamFinishedCallback
   SIG  : undefined8 __fastcall Pa_SetStreamFinishedCallback(int * param_1, undefined8 param_2)
   ======================================================================== */

undefined8 Pa_SetStreamFinishedCallback(int *param_1,undefined8 param_2)

{
  undefined8 uVar1;
  
                    /* 0x4850  20  Pa_SetStreamFinishedCallback */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
      return 0xffffd902;
    }
    if ((int)uVar1 == 1) {
      *(undefined8 *)(param_1 + 8) = param_2;
      uVar1 = 0;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800048a0
   NAME : Pa_StartStream
   SIG  : undefined8 __fastcall Pa_StartStream(int * param_1)
   ======================================================================== */

undefined8 Pa_StartStream(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x48a0  21  Pa_StartStream */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
      return 0xffffd902;
    }
    if ((int)uVar1 == 1) {
                    /* WARNING: Could not recover jumptable at 0x0001800048dc. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 8))(param_1);
      return uVar1;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800048f0
   NAME : Pa_StopStream
   SIG  : undefined8 __fastcall Pa_StopStream(int * param_1)
   ======================================================================== */

undefined8 Pa_StopStream(int *param_1)

{
  undefined8 uVar1;
  
                    /* 0x48f0  22  Pa_StopStream */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x00018000491c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x10))(param_1);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004930
   NAME : Pa_Terminate
   SIG  : undefined8 __fastcall Pa_Terminate(void)
   ======================================================================== */

undefined8 Pa_Terminate(void)

{
                    /* 0x4930  5  Pa_Terminate */
  if (DAT_18002bac0 != 0) {
    if (DAT_18002bac0 == 1) {
      FUN_180003a50();
      FUN_180004a90();
    }
    DAT_18002bac0 = DAT_18002bac0 + -1;
    return 0;
  }
  return 0xffffd8f0;
}



/* ========================================================================
   ENTRY: 180004970
   NAME : Pa_WriteStream
   SIG  : undefined8 __fastcall Pa_WriteStream(int * param_1, longlong param_2, int param_3)
   ======================================================================== */

undefined8 Pa_WriteStream(int *param_1,longlong param_2,int param_3)

{
  undefined8 uVar1;
  
                    /* 0x4970  30  Pa_WriteStream */
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    if (param_3 == 0) {
      return uVar1;
    }
    if (param_2 == 0) {
      return 0xffffd90c;
    }
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800049e9. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x48))(param_1,param_2,param_3);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180004a10
   NAME : FUN_180004a10
   SIG  : undefined __fastcall FUN_180004a10(longlong param_1)
   ======================================================================== */

void FUN_180004a10(longlong param_1)

{
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  
  lVar1 = DAT_18002baa8;
  lVar3 = 0;
  if (DAT_18002baa8 != 0) {
    while (lVar2 = lVar1, lVar1 = *(longlong *)(lVar2 + 8), lVar2 != param_1) {
      lVar3 = lVar2;
      if (lVar1 == 0) {
        return;
      }
    }
    if (lVar3 == 0) {
      DAT_18002baa8 = lVar1;
      return;
    }
    *(longlong *)(lVar3 + 8) = lVar1;
  }
  return;
}



/* ========================================================================
   ENTRY: 180004a50
   NAME : FUN_180004a50
   SIG  : undefined8 __fastcall FUN_180004a50(uint param_1)
   ======================================================================== */

undefined8 FUN_180004a50(uint param_1)

{
  uint uVar1;
  bool bVar2;
  
  uVar1 = param_1 & 0x7fffffff;
  if (uVar1 < 0x11) {
    if (uVar1 == 0x10) {
      return 1;
    }
    if (uVar1 == 1) {
      return 1;
    }
    if (uVar1 == 2) {
      return 1;
    }
    if (uVar1 == 4) {
      return 1;
    }
    bVar2 = uVar1 == 8;
  }
  else {
    if (uVar1 == 0x20) {
      return 1;
    }
    if (uVar1 == 0x40) {
      return 1;
    }
    bVar2 = uVar1 == 0x10000;
  }
  if (bVar2) {
    return 1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180004a90
   NAME : FUN_180004a90
   SIG  : undefined __fastcall FUN_180004a90(void)
   ======================================================================== */

void FUN_180004a90(void)

{
  longlong lVar1;
  
  while (0 < DAT_18002bab8) {
    DAT_18002bab8 = DAT_18002bab8 + -1;
    lVar1 = *(longlong *)(DAT_18002bab0 + (longlong)DAT_18002bab8 * 8);
    (**(code **)(lVar1 + 0x30))(lVar1);
  }
  DAT_18002bab8 = 0;
  DAT_18002babc = 0;
  DAT_18002bac8 = 0;
  if (DAT_18002bab0 != 0) {
    FUN_180020240(DAT_18002bab0);
  }
  DAT_18002bab0 = 0;
  return;
}



/* ========================================================================
   ENTRY: 180004b00
   NAME : FUN_180004b00
   SIG  : undefined8 __fastcall FUN_180004b00(int * param_1, int * param_2, double param_3, int param_4, uint param_5, longlong param_6, longlong * param_7, int * param_8, int * param_9)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
FUN_180004b00(int *param_1,int *param_2,double param_3,int param_4,uint param_5,longlong param_6,
             longlong *param_7,int *param_8,int *param_9)

{
  int iVar1;
  int iVar2;
  undefined8 uVar3;
  longlong lVar4;
  longlong lVar5;
  
  lVar5 = DAT_18002bab0;
  if (param_1 == (int *)0x0) {
    if (param_2 == (int *)0x0) {
      return 0xffffd8f4;
    }
    iVar2 = -1;
    *param_8 = -1;
  }
  else {
    iVar2 = *param_1;
    if (iVar2 == -2) {
      if (*(longlong *)(param_1 + 6) == 0) {
        return 0xffffd8f4;
      }
      iVar2 = Pa_HostApiTypeIdToHostApiIndex(*(int *)(*(longlong *)(param_1 + 6) + 4));
      if (iVar2 == -1) {
        return 0xffffd8f4;
      }
      *param_8 = -2;
      *param_7 = *(longlong *)(lVar5 + (longlong)iVar2 * 8);
    }
    else {
      if (iVar2 < 0) {
        return 0xffffd8f4;
      }
      if (DAT_18002bac8 <= iVar2) {
        return 0xffffd8f4;
      }
      iVar2 = FUN_180003aa0(iVar2,param_8);
      if (iVar2 < 0) {
        return 0xffffd8fe;
      }
      lVar4 = *(longlong *)(lVar5 + (longlong)iVar2 * 8);
      *param_7 = lVar4;
      if (param_1[1] < 1) {
        return 0xffffd8f2;
      }
      uVar3 = FUN_180004a50(param_1[2]);
      if ((int)uVar3 == 0) {
        return 0xffffd8f6;
      }
      if ((*(longlong *)(param_1 + 6) != 0) &&
         (*(int *)(*(longlong *)(param_1 + 6) + 4) != *(int *)(lVar4 + 0xc))) {
        return 0xffffd900;
      }
    }
    if (param_2 == (int *)0x0) {
      *param_9 = -1;
      goto LAB_180004c5f;
    }
  }
  iVar1 = *param_2;
  if (iVar1 == -2) {
    if (*(longlong *)(param_2 + 6) == 0) {
      return 0xffffd8f4;
    }
    iVar1 = Pa_HostApiTypeIdToHostApiIndex(*(int *)(*(longlong *)(param_2 + 6) + 4));
    if (iVar1 == -1) {
      return 0xffffd8f4;
    }
    *param_9 = -2;
    *param_7 = *(longlong *)(lVar5 + (longlong)iVar1 * 8);
  }
  else {
    if (iVar1 < 0) {
      return 0xffffd8f4;
    }
    if (DAT_18002bac8 <= iVar1) {
      return 0xffffd8f4;
    }
    iVar1 = FUN_180003aa0(iVar1,param_9);
    lVar4 = (longlong)iVar1;
    if (iVar1 < 0) {
      return 0xffffd8fe;
    }
    lVar5 = *(longlong *)(lVar5 + lVar4 * 8);
    *param_7 = lVar5;
    if (param_2[1] < 1) {
      return 0xffffd8f2;
    }
    uVar3 = FUN_180004a50(param_2[2]);
    iVar1 = (int)lVar4;
    if ((int)uVar3 == 0) {
      return 0xffffd8f6;
    }
    if ((*(longlong *)(param_2 + 6) != 0) &&
       (*(int *)(*(longlong *)(param_2 + 6) + 4) != *(int *)(lVar5 + 0xc))) {
      return 0xffffd900;
    }
  }
  if ((param_1 != (int *)0x0) && (iVar2 != iVar1)) {
    return 0xffffd8f7;
  }
LAB_180004c5f:
  if ((param_3 < DAT_1800239c8) || (_DAT_1800239d0 < param_3)) {
    return 0xffffd8f3;
  }
  if (((param_5 & 0xfff0) == 0) &&
     (((param_5 & 4) == 0 ||
      ((((param_6 != 0 && (param_1 != (int *)0x0)) && (param_2 != (int *)0x0)) && (param_4 == 0)))))
     ) {
    return 0;
  }
  return 0xffffd8f5;
}



/* ========================================================================
   ENTRY: 180004d50
   NAME : FUN_180004d50
   SIG  : int __fastcall FUN_180004d50(uint * param_1, int * param_2, longlong param_3, int param_4)
   ======================================================================== */

int FUN_180004d50(uint *param_1,int *param_2,longlong param_3,int param_4)

{
  int iVar1;
  uint uVar2;
  int iVar3;
  longlong lVar4;
  longlong lVar5;
  ulonglong uVar6;
  uint uVar7;
  ulonglong uVar8;
  uint uVar9;
  uint uVar10;
  uint local_res8;
  int local_48;
  longlong local_40;
  
  local_48 = 0;
  do {
    uVar6 = 0;
    uVar9 = param_1[0x1e];
    uVar7 = *param_1;
    uVar10 = param_1[9];
    lVar5 = *(longlong *)(param_1 + 0x1a);
    uVar2 = param_1[7];
    iVar1 = uVar7 - uVar9;
    if (uVar9 + param_4 <= uVar7) {
      iVar1 = param_4;
    }
    uVar9 = uVar10 * uVar9;
    if (param_1[10] == 0) {
      uVar10 = uVar10 * uVar7;
      local_res8 = 1;
      uVar8 = uVar6;
      if (uVar2 != 0) {
        do {
          uVar7 = (int)uVar8 + 1;
          *(ulonglong *)(*(longlong *)(param_1 + 0x1c) + uVar8 * 8) =
               (ulonglong)((int)uVar8 * param_1[9] * *param_1) + *(longlong *)(param_1 + 0x1a);
          uVar2 = param_1[7];
          uVar8 = (ulonglong)uVar7;
        } while (uVar7 < uVar2);
      }
      local_40 = *(longlong *)(param_1 + 0x1c);
    }
    else {
      uVar9 = uVar9 * uVar2;
      local_res8 = uVar2;
      local_40 = lVar5;
    }
    if (uVar2 != 0) {
      lVar5 = (ulonglong)uVar9 + lVar5;
      do {
        lVar4 = uVar6 * 0x10;
        (**(code **)(param_1 + 0xc))
                  (lVar5,local_res8,*(undefined8 *)(lVar4 + param_3),
                   *(undefined4 *)(lVar4 + 8 + param_3),iVar1,param_1 + 0x38);
        lVar5 = lVar5 + (ulonglong)uVar10;
        uVar9 = (int)uVar6 + 1;
        uVar6 = (ulonglong)uVar9;
        *(longlong *)(lVar4 + param_3) =
             *(longlong *)(lVar4 + param_3) +
             (ulonglong)(iVar1 * param_1[8] * *(int *)(lVar4 + 8 + param_3));
      } while (uVar9 < param_1[7]);
    }
    uVar9 = param_1[0x1e];
    param_1[0x1e] = uVar9 + iVar1;
    if (uVar9 + iVar1 == *param_1) {
      if (*param_2 == 0) {
        *(undefined8 *)(*(longlong *)(param_1 + 0x26) + 0x10) = 0;
        iVar3 = (**(code **)(param_1 + 0x3e))
                          (local_40,0,*param_1,*(undefined8 *)(param_1 + 0x26),param_1[0x28],
                           *(undefined8 *)(param_1 + 0x40));
        *param_2 = iVar3;
        **(double **)(param_1 + 0x26) =
             (double)*param_1 * *(double *)(param_1 + 0x3c) + **(double **)(param_1 + 0x26);
      }
      param_1[0x1e] = 0;
    }
    local_48 = local_48 + iVar1;
    param_4 = param_4 - iVar1;
  } while (param_4 != 0);
  return local_48;
}



/* ========================================================================
   ENTRY: 180004f40
   NAME : FUN_180004f40
   SIG  : int __fastcall FUN_180004f40(uint * param_1, int * param_2, longlong param_3, uint param_4)
   ======================================================================== */

int FUN_180004f40(uint *param_1,int *param_2,longlong param_3,uint param_4)

{
  int iVar1;
  uint uVar2;
  undefined8 uVar3;
  longlong lVar4;
  longlong lVar5;
  ulonglong uVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  int local_res8;
  
  local_res8 = 0;
  do {
    if ((param_1[0x24] == 0) && (*param_2 == 0)) {
      if (param_1[0x13] == 0) {
        uVar6 = 0;
        if (param_1[0x10] != 0) {
          do {
            *(ulonglong *)(*(longlong *)(param_1 + 0x22) + uVar6 * 8) =
                 (ulonglong)(param_1[0x12] * *param_1 * (int)uVar6) + *(longlong *)(param_1 + 0x20);
            uVar2 = (int)uVar6 + 1;
            uVar6 = (ulonglong)uVar2;
          } while (uVar2 < param_1[0x10]);
        }
        uVar3 = *(undefined8 *)(param_1 + 0x22);
      }
      else {
        uVar3 = *(undefined8 *)(param_1 + 0x20);
      }
      **(undefined8 **)(param_1 + 0x26) = 0;
      iVar1 = (**(code **)(param_1 + 0x3e))
                        (0,uVar3,*param_1,*(undefined8 *)(param_1 + 0x26),param_1[0x28],
                         *(undefined8 *)(param_1 + 0x40));
      *param_2 = iVar1;
      if (iVar1 != 2) {
        *(double *)(*(longlong *)(param_1 + 0x26) + 0x10) =
             (double)*param_1 * *(double *)(param_1 + 0x3c) +
             *(double *)(*(longlong *)(param_1 + 0x26) + 0x10);
        param_1[0x24] = *param_1;
      }
    }
    uVar2 = param_1[0x24];
    uVar9 = param_4;
    if (uVar2 == 0) {
      uVar6 = 0;
      if (param_1[0x10] != 0) {
        do {
          lVar5 = uVar6 * 0x10;
          (**(code **)(param_1 + 0x16))
                    (*(undefined8 *)(param_3 + lVar5),*(undefined4 *)(param_3 + 8 + lVar5));
          uVar2 = (int)uVar6 + 1;
          uVar6 = (ulonglong)uVar2;
          *(longlong *)(param_3 + lVar5) =
               *(longlong *)(param_3 + lVar5) +
               (ulonglong)(*(int *)(param_3 + 8 + lVar5) * param_1[0x11] * param_4);
        } while (uVar2 < param_1[0x10]);
      }
    }
    else {
      uVar7 = param_1[0x12];
      uVar9 = uVar2;
      if (param_4 <= uVar2) {
        uVar9 = param_4;
      }
      uVar2 = (*param_1 - uVar2) * uVar7;
      if (param_1[0x13] == 0) {
        uVar8 = 1;
        uVar7 = uVar7 * *param_1;
      }
      else {
        uVar8 = param_1[0x10];
        uVar2 = uVar2 * uVar8;
      }
      uVar6 = (ulonglong)uVar7;
      if (param_1[0x10] != 0) {
        uVar7 = 0;
        lVar5 = (ulonglong)uVar2 + *(longlong *)(param_1 + 0x20);
        do {
          lVar4 = (ulonglong)uVar7 * 0x10;
          (**(code **)(param_1 + 0x14))
                    (*(undefined8 *)(param_3 + lVar4),*(undefined4 *)(param_3 + 8 + lVar4),lVar5,
                     uVar8,uVar9,param_1 + 0x38,uVar6);
          uVar7 = uVar7 + 1;
          lVar5 = lVar5 + uVar6;
          *(longlong *)(param_3 + lVar4) =
               *(longlong *)(param_3 + lVar4) +
               (ulonglong)(*(int *)(param_3 + 8 + lVar4) * param_1[0x11] * uVar9);
        } while (uVar7 < param_1[0x10]);
      }
      param_1[0x24] = param_1[0x24] - uVar9;
    }
    local_res8 = local_res8 + uVar9;
    param_4 = param_4 - uVar9;
  } while (param_4 != 0);
  return local_res8;
}



/* ========================================================================
   ENTRY: 180005180
   NAME : FUN_180005180
   SIG  : int __fastcall FUN_180005180(uint * param_1, int * param_2, int param_3)
   ======================================================================== */

int FUN_180005180(uint *param_1,int *param_2,int param_3)

{
  uint uVar1;
  int iVar2;
  undefined8 uVar3;
  uint uVar4;
  longlong lVar5;
  longlong lVar6;
  uint uVar7;
  ulonglong uVar8;
  uint uVar9;
  uint uVar10;
  undefined8 uVar11;
  uint uVar12;
  int iVar13;
  longlong lVar14;
  uint uVar15;
  int local_res8;
  uint local_res18;
  
  iVar13 = 0;
  uVar15 = param_1[0x2b] + param_1[0x2a];
  local_res8 = 0;
  if (param_3 == 0) {
    uVar4 = *param_1 - 1;
  }
  else {
    uVar4 = 0;
  }
  FUN_1800055c0((int *)param_1);
  local_res18 = uVar15;
  do {
    if (uVar15 <= uVar4) {
      return iVar13;
    }
    if ((param_1[0x24] == 0) && (*param_2 != 0)) {
      uVar1 = param_1[0x31];
      if (uVar1 != 0) {
        lVar14 = *(longlong *)(param_1 + 0x34);
        uVar8 = 0;
        if (param_1[0x10] != 0) {
          do {
            lVar5 = uVar8 * 0x10;
            (**(code **)(param_1 + 0x16))
                      (*(undefined8 *)(lVar14 + lVar5),*(undefined4 *)(lVar14 + 8 + lVar5));
            uVar7 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar7;
            *(longlong *)(lVar14 + lVar5) =
                 *(longlong *)(lVar14 + lVar5) +
                 (ulonglong)(*(int *)(lVar14 + 8 + lVar5) * param_1[0x11] * uVar1);
          } while (uVar7 < param_1[0x10]);
        }
        param_1[0x31] = 0;
      }
      uVar1 = param_1[0x32];
      if (uVar1 != 0) {
        lVar14 = *(longlong *)(param_1 + 0x36);
        uVar8 = 0;
        if (param_1[0x10] != 0) {
          do {
            lVar5 = uVar8 * 0x10;
            (**(code **)(param_1 + 0x16))
                      (*(undefined8 *)(lVar14 + lVar5),*(undefined4 *)(lVar14 + 8 + lVar5));
            uVar7 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar7;
            *(longlong *)(lVar14 + lVar5) =
                 *(longlong *)(lVar14 + lVar5) +
                 (ulonglong)(uVar1 * *(int *)(lVar14 + 8 + lVar5) * param_1[0x11]);
          } while (uVar7 < param_1[0x10]);
        }
        param_1[0x32] = 0;
      }
    }
    uVar1 = param_1[0x1e];
    uVar7 = *param_1;
    if (uVar1 < uVar7) {
      do {
        uVar10 = param_1[0x2b];
        uVar12 = param_1[0x2a];
        if (uVar10 + uVar12 == 0) break;
        uVar9 = uVar7 - uVar1;
        if (uVar12 == 0) {
          lVar14 = *(longlong *)(param_1 + 0x2e);
          if (uVar10 < uVar9) {
            uVar9 = uVar10;
          }
        }
        else {
          lVar14 = *(longlong *)(param_1 + 0x2c);
          if (uVar12 < uVar9) {
            uVar9 = uVar12;
          }
        }
        uVar10 = param_1[9];
        uVar1 = uVar10 * uVar1;
        if (param_1[10] == 0) {
          uVar12 = 1;
          uVar10 = uVar10 * uVar7;
        }
        else {
          uVar12 = param_1[7];
          uVar1 = uVar1 * uVar12;
        }
        if (param_1[7] != 0) {
          uVar8 = 0;
          lVar5 = (ulonglong)uVar1 + *(longlong *)(param_1 + 0x1a);
          do {
            lVar6 = uVar8 * 0x10;
            (**(code **)(param_1 + 0xc))
                      (lVar5,uVar12,*(undefined8 *)(lVar14 + lVar6),
                       *(undefined4 *)(lVar14 + 8 + lVar6),uVar9,param_1 + 0x38);
            uVar1 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar1;
            lVar5 = lVar5 + (ulonglong)uVar10;
            *(longlong *)(lVar14 + lVar6) =
                 *(longlong *)(lVar14 + lVar6) +
                 (ulonglong)(*(int *)(lVar14 + 8 + lVar6) * param_1[8] * uVar9);
            uVar15 = local_res18;
            iVar13 = local_res8;
          } while (uVar1 < param_1[7]);
        }
        if (param_1[0x2a] == 0) {
          param_1[0x2b] = param_1[0x2b] - uVar9;
        }
        else {
          param_1[0x2a] = param_1[0x2a] - uVar9;
        }
        param_1[0x1e] = param_1[0x1e] + uVar9;
        uVar15 = uVar15 - uVar9;
        uVar1 = param_1[0x1e];
        iVar13 = iVar13 + uVar9;
        uVar7 = *param_1;
        local_res8 = iVar13;
        local_res18 = uVar15;
      } while (uVar1 < uVar7);
    }
    if ((uVar1 == uVar7) && (param_1[0x24] == 0)) {
      if (*param_2 == 0) {
        if (param_1[10] == 0) {
          uVar8 = 0;
          if (param_1[7] != 0) {
            do {
              *(ulonglong *)(*(longlong *)(param_1 + 0x1c) + uVar8 * 8) =
                   (ulonglong)((int)uVar8 * param_1[9] * *param_1) + *(longlong *)(param_1 + 0x1a);
              uVar1 = (int)uVar8 + 1;
              uVar8 = (ulonglong)uVar1;
            } while (uVar1 < param_1[7]);
            uVar7 = *param_1;
          }
          uVar11 = *(undefined8 *)(param_1 + 0x1c);
        }
        else {
          uVar11 = *(undefined8 *)(param_1 + 0x1a);
        }
        if (param_1[0x13] == 0) {
          uVar8 = 0;
          if (param_1[0x10] != 0) {
            do {
              *(ulonglong *)(*(longlong *)(param_1 + 0x22) + uVar8 * 8) =
                   (ulonglong)((int)uVar8 * param_1[0x12] * *param_1) +
                   *(longlong *)(param_1 + 0x20);
              uVar1 = (int)uVar8 + 1;
              uVar8 = (ulonglong)uVar1;
            } while (uVar1 < param_1[0x10]);
            uVar7 = *param_1;
          }
          uVar3 = *(undefined8 *)(param_1 + 0x22);
        }
        else {
          uVar3 = *(undefined8 *)(param_1 + 0x20);
        }
        iVar2 = (**(code **)(param_1 + 0x3e))
                          (uVar11,uVar3,uVar7,*(undefined8 *)(param_1 + 0x26),param_1[0x28],
                           *(undefined8 *)(param_1 + 0x40));
        *param_2 = iVar2;
        **(double **)(param_1 + 0x26) =
             (double)*param_1 * *(double *)(param_1 + 0x3c) + **(double **)(param_1 + 0x26);
        *(double *)(*(longlong *)(param_1 + 0x26) + 0x10) =
             (double)*param_1 * *(double *)(param_1 + 0x3c) +
             *(double *)(*(longlong *)(param_1 + 0x26) + 0x10);
        param_1[0x1e] = 0;
        if (*param_2 == 2) {
          param_1[0x24] = 0;
        }
        else {
          param_1[0x24] = *param_1;
        }
      }
      else {
        param_1[0x1e] = 0;
      }
    }
    FUN_1800055c0((int *)param_1);
  } while( true );
}



/* ========================================================================
   ENTRY: 180005570
   NAME : FUN_180005570
   SIG  : uint __fastcall FUN_180005570(uint param_1, uint param_2)
   ======================================================================== */

uint FUN_180005570(uint param_1,uint param_2)

{
  uint uVar1;
  ulonglong uVar2;
  uint uVar3;
  uint uVar4;
  
  uVar2 = FUN_180005740(param_1,param_2);
  uVar1 = 0;
  for (uVar4 = param_1; uVar4 < (uint)uVar2; uVar4 = uVar4 + param_1) {
    uVar3 = uVar4 % param_2;
    if (uVar4 % param_2 < uVar1) {
      uVar3 = uVar1;
    }
    uVar1 = uVar3;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800055c0
   NAME : FUN_1800055c0
   SIG  : undefined __fastcall FUN_1800055c0(int * param_1)
   ======================================================================== */

void FUN_1800055c0(int *param_1)

{
  uint *puVar1;
  uint uVar2;
  uint uVar3;
  ulonglong uVar4;
  uint uVar5;
  longlong lVar6;
  uint uVar7;
  int iVar8;
  longlong lVar9;
  longlong lVar10;
  
  uVar3 = param_1[0x24];
  uVar7 = uVar3;
  while( true ) {
    if (uVar7 == 0) {
      return;
    }
    uVar7 = param_1[0x32];
    uVar2 = param_1[0x31];
    if (uVar7 + uVar2 == 0) break;
    uVar5 = uVar3;
    if (uVar2 == 0) {
      lVar9 = *(longlong *)(param_1 + 0x36);
      if (uVar7 < uVar3) {
        uVar5 = uVar7;
      }
    }
    else {
      lVar9 = *(longlong *)(param_1 + 0x34);
      if (uVar2 < uVar3) {
        uVar5 = uVar2;
      }
    }
    uVar7 = param_1[0x12];
    uVar3 = (*param_1 - uVar3) * uVar7;
    if (param_1[0x13] == 0) {
      iVar8 = 1;
      uVar7 = uVar7 * *param_1;
    }
    else {
      iVar8 = param_1[0x10];
      uVar3 = uVar3 * iVar8;
    }
    if (param_1[0x10] != 0) {
      uVar4 = 0;
      lVar10 = (ulonglong)uVar3 + *(longlong *)(param_1 + 0x20);
      do {
        lVar6 = uVar4 * 0x10;
        (**(code **)(param_1 + 0x14))
                  (*(undefined8 *)(lVar9 + lVar6),*(undefined4 *)(lVar9 + 8 + lVar6),lVar10,iVar8,
                   uVar5,param_1 + 0x38);
        uVar3 = (int)uVar4 + 1;
        uVar4 = (ulonglong)uVar3;
        lVar10 = lVar10 + (ulonglong)uVar7;
        *(longlong *)(lVar9 + lVar6) =
             *(longlong *)(lVar9 + lVar6) +
             (ulonglong)(*(int *)(lVar9 + 8 + lVar6) * param_1[0x11] * uVar5);
      } while (uVar3 < (uint)param_1[0x10]);
    }
    if (param_1[0x31] == 0) {
      param_1[0x32] = param_1[0x32] - uVar5;
    }
    else {
      param_1[0x31] = param_1[0x31] - uVar5;
    }
    puVar1 = (uint *)(param_1 + 0x24);
    *puVar1 = *puVar1 - uVar5;
    uVar7 = *puVar1;
    uVar3 = param_1[0x24];
  }
  return;
}



/* ========================================================================
   ENTRY: 180005720
   NAME : FUN_180005720
   SIG  : undefined __fastcall FUN_180005720(uint param_1, uint param_2)
   ======================================================================== */

void FUN_180005720(uint param_1,uint param_2)

{
  uint uVar1;
  
  while (uVar1 = param_2, uVar1 != 0) {
    param_2 = param_1 % uVar1;
    param_1 = uVar1;
  }
  return;
}



/* ========================================================================
   ENTRY: 180005740
   NAME : FUN_180005740
   SIG  : ulonglong __fastcall FUN_180005740(uint param_1, uint param_2)
   ======================================================================== */

ulonglong FUN_180005740(uint param_1,uint param_2)

{
  uint uVar1;
  
  uVar1 = FUN_180005720(param_1,param_2);
  return (ulonglong)(param_1 * param_2) / (ulonglong)uVar1;
}



/* ========================================================================
   ENTRY: 180005770
   NAME : FUN_180005770
   SIG  : int __fastcall FUN_180005770(longlong param_1, int * param_2, ulonglong * param_3, ulonglong * param_4, uint param_5)
   ======================================================================== */

int FUN_180005770(longlong param_1,int *param_2,ulonglong *param_3,ulonglong *param_4,uint param_5)

{
  bool bVar1;
  bool bVar2;
  int iVar3;
  uint uVar4;
  uint uVar5;
  ulonglong uVar6;
  uint uVar7;
  ulonglong uVar8;
  uint uVar9;
  uint *puVar10;
  uint uVar11;
  ulonglong uVar12;
  longlong lVar13;
  uint local_50;
  int local_4c;
  ulonglong local_48;
  
  uVar11 = param_5;
  if (*param_2 == 0) {
    local_4c = 0;
    bVar2 = false;
    bVar1 = false;
    do {
      uVar6 = 0;
      uVar5 = *(uint *)(param_1 + 0x1c);
      uVar7 = *(uint *)(param_1 + 0x18);
      if (uVar11 <= *(uint *)(param_1 + 0x18)) {
        uVar7 = uVar11;
      }
      local_48 = uVar6;
      if (uVar5 != 0) {
        uVar12 = *(ulonglong *)(param_1 + 0x68);
        uVar4 = *(uint *)(param_1 + 0x24);
        if (*(int *)(param_1 + 0x28) == 0) {
          uVar4 = uVar4 * uVar7;
          local_50 = 1;
          if (((*(int *)(param_1 + 0x14) == 0) || (*(int *)(param_1 + 0xa4) != 0)) ||
             (**(longlong **)(param_1 + 0xb0) == 0)) {
            uVar8 = uVar6;
            if (uVar5 != 0) {
              do {
                uVar11 = (int)uVar8 + 1;
                *(ulonglong *)(*(longlong *)(param_1 + 0x70) + uVar8 * 8) =
                     (ulonglong)((int)uVar8 * *(int *)(param_1 + 0x24) * uVar7) +
                     *(longlong *)(param_1 + 0x68);
                uVar5 = *(uint *)(param_1 + 0x1c);
                uVar8 = (ulonglong)uVar11;
              } while (uVar11 < uVar5);
            }
          }
          else {
            uVar8 = uVar6;
            if (uVar5 != 0) {
              do {
                uVar11 = (int)uVar8 + 1;
                *(ulonglong *)(*(longlong *)(param_1 + 0x70) + uVar8 * 8) = param_3[uVar8 * 2];
                uVar5 = *(uint *)(param_1 + 0x1c);
                uVar8 = (ulonglong)uVar11;
              } while (uVar11 < uVar5);
            }
            bVar1 = true;
          }
          local_48 = *(ulonglong *)(param_1 + 0x70);
        }
        else {
          local_50 = uVar5;
          local_48 = uVar12;
          if (((*(int *)(param_1 + 0x14) != 0) && (*(int *)(param_1 + 0xa4) != 0)) &&
             ((**(longlong **)(param_1 + 0xb0) != 0 && (uVar5 == (uint)param_3[1])))) {
            uVar12 = *param_3;
            bVar1 = true;
            local_48 = uVar12;
          }
        }
        if (**(longlong **)(param_1 + 0xb0) == 0) {
          uVar11 = param_5;
          if (uVar5 != 0) {
            do {
              (**(code **)(param_1 + 0x38))(uVar12,local_50,uVar7);
              uVar12 = uVar12 + uVar4;
              uVar5 = (int)uVar6 + 1;
              uVar6 = (ulonglong)uVar5;
            } while (uVar5 < *(uint *)(param_1 + 0x1c));
          }
        }
        else if (bVar1) {
          uVar11 = param_5;
          if (uVar5 != 0) {
            do {
              uVar5 = (int)uVar6 + 1;
              param_3[uVar6 * 2] =
                   param_3[uVar6 * 2] +
                   (ulonglong)((int)param_3[uVar6 * 2 + 1] * *(int *)(param_1 + 0x20) * uVar7);
              uVar6 = (ulonglong)uVar5;
            } while (uVar5 < *(uint *)(param_1 + 0x1c));
          }
        }
        else {
          uVar11 = param_5;
          if (uVar5 != 0) {
            do {
              (**(code **)(param_1 + 0x30))
                        (uVar12,local_50,param_3[uVar6 * 2],(int)param_3[uVar6 * 2 + 1],uVar7,
                         param_1 + 0xe0);
              uVar12 = uVar12 + uVar4;
              uVar5 = (int)uVar6 + 1;
              param_3[uVar6 * 2] =
                   param_3[uVar6 * 2] +
                   (ulonglong)(uVar7 * *(int *)(param_1 + 0x20) * (int)param_3[uVar6 * 2 + 1]);
              uVar6 = (ulonglong)uVar5;
            } while (uVar5 < *(uint *)(param_1 + 0x1c));
          }
        }
      }
      uVar6 = 0;
      puVar10 = (uint *)(param_1 + 0x40);
      uVar5 = *puVar10;
      uVar12 = uVar6;
      if (uVar5 != 0) {
        if (*(int *)(param_1 + 0x4c) == 0) {
          if ((*(int *)(param_1 + 0x10) == 0) || (*(int *)(param_1 + 0xc0) != 0)) {
            if (uVar5 != 0) {
              do {
                uVar5 = (int)uVar12 + 1;
                *(ulonglong *)(*(longlong *)(param_1 + 0x88) + uVar12 * 8) =
                     (ulonglong)((int)uVar12 * *(int *)(param_1 + 0x48) * uVar7) +
                     *(longlong *)(param_1 + 0x80);
                uVar12 = (ulonglong)uVar5;
              } while (uVar5 < *puVar10);
            }
          }
          else {
            if (uVar5 != 0) {
              do {
                uVar5 = (int)uVar12 + 1;
                *(ulonglong *)(*(longlong *)(param_1 + 0x88) + uVar12 * 8) = param_4[uVar12 * 2];
                uVar12 = (ulonglong)uVar5;
              } while (uVar5 < *puVar10);
            }
            bVar2 = true;
          }
          uVar12 = *(ulonglong *)(param_1 + 0x88);
        }
        else if (((*(int *)(param_1 + 0x10) == 0) || (*(int *)(param_1 + 0xc0) == 0)) ||
                (uVar5 != (uint)param_4[1])) {
          uVar12 = *(ulonglong *)(param_1 + 0x80);
        }
        else {
          bVar2 = true;
          uVar12 = *param_4;
        }
      }
      iVar3 = (**(code **)(param_1 + 0xf8))
                        (local_48,uVar12,uVar7,*(undefined8 *)(param_1 + 0x98),
                         *(undefined4 *)(param_1 + 0xa0),*(undefined8 *)(param_1 + 0x100));
      *param_2 = iVar3;
      if (iVar3 != 2) {
        **(double **)(param_1 + 0x98) =
             (double)uVar7 * *(double *)(param_1 + 0xf0) + **(double **)(param_1 + 0x98);
        *(double *)(*(longlong *)(param_1 + 0x98) + 0x10) =
             (double)uVar7 * *(double *)(param_1 + 0xf0) +
             *(double *)(*(longlong *)(param_1 + 0x98) + 0x10);
        uVar5 = *puVar10;
        if ((uVar5 != 0) && (**(longlong **)(param_1 + 0xd0) != 0)) {
          if (bVar2) {
            if (uVar5 != 0) {
              do {
                uVar5 = (int)uVar6 + 1;
                param_4[uVar6 * 2] =
                     param_4[uVar6 * 2] +
                     (ulonglong)(uVar7 * (int)param_4[uVar6 * 2 + 1] * *(int *)(param_1 + 0x44));
                uVar6 = (ulonglong)uVar5;
              } while (uVar5 < *puVar10);
            }
          }
          else {
            lVar13 = *(longlong *)(param_1 + 0x80);
            uVar4 = *(uint *)(param_1 + 0x48);
            uVar9 = uVar5;
            if (*(int *)(param_1 + 0x4c) == 0) {
              uVar4 = uVar4 * uVar7;
              uVar9 = 1;
            }
            uVar11 = param_5;
            if (uVar5 != 0) {
              do {
                (**(code **)(param_1 + 0x50))
                          (param_4[uVar6 * 2],(int)param_4[uVar6 * 2 + 1],lVar13,uVar9,uVar7,
                           param_1 + 0xe0);
                lVar13 = lVar13 + (ulonglong)uVar4;
                uVar5 = (int)uVar6 + 1;
                param_4[uVar6 * 2] =
                     param_4[uVar6 * 2] +
                     (ulonglong)(uVar7 * (int)param_4[uVar6 * 2 + 1] * *(int *)(param_1 + 0x44));
                uVar6 = (ulonglong)uVar5;
              } while (uVar5 < *(uint *)(param_1 + 0x40));
            }
          }
        }
        local_4c = local_4c + uVar7;
        uVar11 = uVar11 - uVar7;
        param_5 = uVar11;
      }
      if (uVar11 == 0) {
        return local_4c;
      }
    } while (*param_2 == 0);
  }
  else {
    local_4c = 0;
    if (param_5 == 0) {
      return 0;
    }
  }
  uVar7 = 0;
  uVar5 = *(uint *)(param_1 + 0x40);
  if (((uVar5 != 0) && (**(longlong **)(param_1 + 0xd0) != 0)) && (uVar5 != 0)) {
    do {
      uVar6 = (ulonglong)uVar7;
      (**(code **)(param_1 + 0x58))(param_4[uVar6 * 2],(int)param_4[uVar6 * 2 + 1],uVar11);
      uVar7 = uVar7 + 1;
      param_4[uVar6 * 2] =
           param_4[uVar6 * 2] +
           (ulonglong)(uVar11 * (int)param_4[uVar6 * 2 + 1] * *(int *)(param_1 + 0x44));
    } while (uVar7 < *(uint *)(param_1 + 0x40));
  }
  return uVar11 + local_4c;
}



/* ========================================================================
   ENTRY: 180005ce0
   NAME : FUN_180005ce0
   SIG  : undefined __fastcall FUN_180005ce0(longlong param_1, double * param_2, undefined4 param_3)
   ======================================================================== */

void FUN_180005ce0(longlong param_1,double *param_2,undefined4 param_3)

{
  *(double **)(param_1 + 0x98) = param_2;
  *param_2 = *param_2 - (double)*(uint *)(param_1 + 0x78) * *(double *)(param_1 + 0xf0);
  *(double *)(*(longlong *)(param_1 + 0x98) + 0x10) =
       (double)*(uint *)(param_1 + 0x90) * *(double *)(param_1 + 0xf0) +
       *(double *)(*(longlong *)(param_1 + 0x98) + 0x10);
  *(undefined4 *)(param_1 + 0xa0) = param_3;
  *(undefined4 *)(param_1 + 0xac) = 0;
  *(undefined4 *)(param_1 + 200) = 0;
  return;
}



/* ========================================================================
   ENTRY: 180005d50
   NAME : FUN_180005d50
   SIG  : uint __fastcall FUN_180005d50(longlong param_1, longlong * param_2, uint param_3)
   ======================================================================== */

uint FUN_180005d50(longlong param_1,longlong *param_2,uint param_3)

{
  int iVar1;
  uint uVar2;
  longlong lVar3;
  longlong lVar4;
  longlong lVar5;
  uint uVar6;
  uint uVar7;
  ulonglong uVar8;
  longlong lVar9;
  
  lVar3 = *(longlong *)(param_1 + 0xb0);
  lVar9 = *param_2;
  iVar1 = *(int *)(param_1 + 0x1c);
  if (*(uint *)(param_1 + 0xa8) < param_3) {
    param_3 = *(uint *)(param_1 + 0xa8);
  }
  uVar8 = 0;
  if (*(int *)(param_1 + 0x28) == 0) {
    if (iVar1 == 0) {
      *(int *)(param_1 + 0xa8) = *(int *)(param_1 + 0xa8) - param_3;
    }
    else {
      do {
        lVar5 = *(longlong *)(lVar9 + uVar8 * 8);
        lVar4 = uVar8 * 0x10;
        (**(code **)(param_1 + 0x30))
                  (lVar5,1,*(undefined8 *)(lVar3 + lVar4),*(undefined4 *)(lVar3 + 8 + lVar4),param_3
                   ,param_1 + 0xe0);
        *(ulonglong *)(lVar9 + uVar8 * 8) = (ulonglong)(param_3 * *(int *)(param_1 + 0x24)) + lVar5;
        uVar7 = (int)uVar8 + 1;
        uVar8 = (ulonglong)uVar7;
        *(longlong *)(lVar3 + lVar4) =
             *(longlong *)(lVar3 + lVar4) +
             (ulonglong)(*(int *)(lVar3 + 8 + lVar4) * *(int *)(param_1 + 0x20) * param_3);
      } while (uVar7 < *(uint *)(param_1 + 0x1c));
      *(int *)(param_1 + 0xa8) = *(int *)(param_1 + 0xa8) - param_3;
    }
  }
  else {
    if (iVar1 == 0) {
      uVar7 = 0;
    }
    else {
      uVar2 = *(uint *)(param_1 + 0x24);
      do {
        lVar5 = uVar8 * 0x10;
        (**(code **)(param_1 + 0x30))
                  (lVar9,iVar1,*(undefined8 *)(lVar3 + lVar5),*(undefined4 *)(lVar3 + 8 + lVar5),
                   param_3,param_1 + 0xe0);
        uVar6 = (int)uVar8 + 1;
        uVar8 = (ulonglong)uVar6;
        lVar9 = lVar9 + (ulonglong)uVar2;
        *(longlong *)(lVar3 + lVar5) =
             *(longlong *)(lVar3 + lVar5) +
             (ulonglong)(*(int *)(lVar3 + 8 + lVar5) * *(int *)(param_1 + 0x20) * param_3);
        uVar7 = *(uint *)(param_1 + 0x1c);
      } while (uVar6 < uVar7);
    }
    *param_2 = *param_2 + (ulonglong)(uVar7 * *(int *)(param_1 + 0x24) * param_3);
    *(int *)(param_1 + 0xa8) = *(int *)(param_1 + 0xa8) - param_3;
  }
  return param_3;
}



/* ========================================================================
   ENTRY: 180005eb0
   NAME : FUN_180005eb0
   SIG  : uint __fastcall FUN_180005eb0(longlong param_1, longlong * param_2, uint param_3)
   ======================================================================== */

uint FUN_180005eb0(longlong param_1,longlong *param_2,uint param_3)

{
  int iVar1;
  uint uVar2;
  longlong lVar3;
  longlong lVar4;
  longlong lVar5;
  uint uVar6;
  uint uVar7;
  ulonglong uVar8;
  longlong lVar9;
  
  lVar3 = *(longlong *)(param_1 + 0xd0);
  lVar9 = *param_2;
  iVar1 = *(int *)(param_1 + 0x40);
  if (*(uint *)(param_1 + 0xc4) < param_3) {
    param_3 = *(uint *)(param_1 + 0xc4);
  }
  uVar8 = 0;
  if (*(int *)(param_1 + 0x4c) == 0) {
    if (iVar1 == 0) {
      *(int *)(param_1 + 0xc4) = *(int *)(param_1 + 0xc4) + param_3;
    }
    else {
      do {
        lVar5 = *(longlong *)(lVar9 + uVar8 * 8);
        lVar4 = uVar8 * 0x10;
        (**(code **)(param_1 + 0x50))
                  (*(undefined8 *)(lVar3 + lVar4),*(undefined4 *)(lVar3 + 8 + lVar4),lVar5,1,param_3
                   ,param_1 + 0xe0);
        *(ulonglong *)(lVar9 + uVar8 * 8) = (ulonglong)(param_3 * *(int *)(param_1 + 0x48)) + lVar5;
        uVar7 = (int)uVar8 + 1;
        uVar8 = (ulonglong)uVar7;
        *(longlong *)(lVar3 + lVar4) =
             *(longlong *)(lVar3 + lVar4) +
             (ulonglong)(*(int *)(param_1 + 0x44) * *(int *)(lVar3 + 8 + lVar4) * param_3);
      } while (uVar7 < *(uint *)(param_1 + 0x40));
      *(int *)(param_1 + 0xc4) = *(int *)(param_1 + 0xc4) + param_3;
    }
  }
  else {
    if (iVar1 == 0) {
      uVar7 = 0;
    }
    else {
      uVar2 = *(uint *)(param_1 + 0x48);
      do {
        lVar5 = uVar8 * 0x10;
        (**(code **)(param_1 + 0x50))
                  (*(undefined8 *)(lVar3 + lVar5),*(undefined4 *)(lVar3 + 8 + lVar5),lVar9,iVar1,
                   param_3,param_1 + 0xe0);
        uVar6 = (int)uVar8 + 1;
        uVar8 = (ulonglong)uVar6;
        lVar9 = lVar9 + (ulonglong)uVar2;
        *(longlong *)(lVar3 + lVar5) =
             *(longlong *)(lVar3 + lVar5) +
             (ulonglong)(*(int *)(param_1 + 0x44) * *(int *)(lVar3 + 8 + lVar5) * param_3);
        uVar7 = *(uint *)(param_1 + 0x40);
      } while (uVar6 < uVar7);
    }
    *param_2 = *param_2 + (ulonglong)(uVar7 * *(int *)(param_1 + 0x48) * param_3);
    *(int *)(param_1 + 0xc4) = *(int *)(param_1 + 0xc4) + param_3;
  }
  return param_3;
}



/* ========================================================================
   ENTRY: 180006010
   NAME : FUN_180006010
   SIG  : int __fastcall FUN_180006010(uint * param_1, int * param_2)
   ======================================================================== */

int FUN_180006010(uint *param_1,int *param_2)

{
  uint uVar1;
  int iVar2;
  int iVar3;
  uint *puVar4;
  longlong lVar5;
  ulonglong *puVar6;
  longlong lVar7;
  uint uVar8;
  int local_res8;
  uint local_38 [4];
  
  uVar8 = param_1[7];
  if (param_1[3] == 0) {
    if (uVar8 == 0) {
      iVar2 = FUN_180004f40(param_1,param_2,*(longlong *)(param_1 + 0x34),param_1[0x31]);
      if (param_1[0x32] != 0) {
        iVar3 = FUN_180004f40(param_1,param_2,*(longlong *)(param_1 + 0x36),param_1[0x32]);
        iVar2 = iVar3 + iVar2;
      }
    }
    else {
      if (param_1[0x10] != 0) {
        iVar2 = FUN_180005180(param_1,param_2,(uint)(param_1[2] != 3));
        return iVar2;
      }
      iVar2 = FUN_180004d50(param_1,param_2,*(longlong *)(param_1 + 0x2c),param_1[0x2a]);
      if (param_1[0x2b] != 0) {
        iVar3 = FUN_180004d50(param_1,param_2,*(longlong *)(param_1 + 0x2e),param_1[0x2b]);
        return iVar3 + iVar2;
      }
    }
    return iVar2;
  }
  if ((uVar8 != 0) && (param_1[0x10] != 0)) {
    uVar8 = param_1[0x32] + param_1[0x31];
    local_res8 = 0;
    do {
      puVar6 = *(ulonglong **)(param_1 + 0x2c);
      if (*puVar6 == 0) {
        local_38[0] = uVar8;
        puVar4 = local_38;
        puVar6 = (ulonglong *)0x0;
      }
      else if (param_1[0x2a] == 0) {
        puVar6 = *(ulonglong **)(param_1 + 0x2e);
        puVar4 = param_1 + 0x2b;
      }
      else {
        puVar4 = param_1 + 0x2a;
      }
      lVar5 = 0xc4;
      if (param_1[0x31] == 0) {
        lVar5 = 200;
      }
      lVar7 = 0xd0;
      uVar1 = *puVar4;
      if (*(uint *)((longlong)param_1 + lVar5) <= *puVar4) {
        uVar1 = *(uint *)((longlong)param_1 + lVar5);
      }
      if (param_1[0x31] == 0) {
        lVar7 = 0xd8;
      }
      iVar2 = FUN_180005770((longlong)param_1,param_2,puVar6,
                            *(ulonglong **)(lVar7 + (longlong)param_1),uVar1);
      *puVar4 = *puVar4 - iVar2;
      *(int *)((longlong)param_1 + lVar5) = *(int *)((longlong)param_1 + lVar5) - iVar2;
      local_res8 = local_res8 + iVar2;
      uVar8 = uVar8 - iVar2;
    } while (uVar8 != 0);
    return local_res8;
  }
  lVar5 = 0xa8;
  if (uVar8 == 0) {
    lVar5 = 0xc4;
  }
  iVar2 = FUN_180005770((longlong)param_1,param_2,*(ulonglong **)(param_1 + 0x2c),
                        *(ulonglong **)(param_1 + 0x34),*(uint *)(lVar5 + (longlong)param_1));
  lVar5 = 0xac;
  if (param_1[7] == 0) {
    lVar5 = 200;
  }
  if (*(uint *)((longlong)param_1 + lVar5) != 0) {
    iVar3 = FUN_180005770((longlong)param_1,param_2,*(ulonglong **)(param_1 + 0x2e),
                          *(ulonglong **)(param_1 + 0x36),*(uint *)((longlong)param_1 + lVar5));
    return iVar3 + iVar2;
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 180006270
   NAME : FUN_180006270
   SIG  : undefined4 __fastcall FUN_180006270(longlong param_1)
   ======================================================================== */

undefined4 FUN_180006270(longlong param_1)

{
  return *(undefined4 *)(param_1 + 0x60);
}



/* ========================================================================
   ENTRY: 180006280
   NAME : FUN_180006280
   SIG  : undefined4 __fastcall FUN_180006280(longlong param_1)
   ======================================================================== */

undefined4 FUN_180006280(longlong param_1)

{
  return *(undefined4 *)(param_1 + 100);
}



/* ========================================================================
   ENTRY: 180006290
   NAME : FUN_180006290
   SIG  : ulonglong __fastcall FUN_180006290(uint * param_1, uint param_2, uint param_3, uint param_4, uint param_5, uint param_6, uint param_7, double param_8, uint param_9, uint param_10, uint param_11, uint param_12, longlong param_13, undefined8 param_14)
   ======================================================================== */

ulonglong FUN_180006290(uint *param_1,uint param_2,uint param_3,uint param_4,uint param_5,
                       uint param_6,uint param_7,double param_8,uint param_9,uint param_10,
                       uint param_11,uint param_12,longlong param_13,undefined8 param_14)

{
  uint uVar1;
  undefined8 uVar2;
  undefined *puVar3;
  longlong lVar4;
  ulonglong unaff_RDI;
  uint uVar5;
  double dVar6;
  
  if ((param_9 & 4) != 0) {
    if (param_13 == 0) {
      return unaff_RDI;
    }
    if ((int)param_2 < 1) {
      return unaff_RDI;
    }
    if ((int)param_5 < 1) {
      return unaff_RDI;
    }
    if (param_10 != 0) {
      return unaff_RDI;
    }
  }
  *param_1 = param_10;
  param_1[0x1a] = 0;
  param_1[0x1b] = 0;
  param_1[0x1c] = 0;
  param_1[0x1d] = 0;
  param_1[0x20] = 0;
  param_1[0x21] = 0;
  param_1[0x22] = 0;
  param_1[0x23] = 0;
  param_1[7] = param_2;
  param_1[0x10] = param_5;
  param_1[1] = param_11;
  param_1[2] = param_12;
  param_1[0x2e] = 0;
  param_1[0x2f] = 0;
  param_1[0x2c] = 0;
  param_1[0x2d] = 0;
  param_1[0x36] = 0;
  param_1[0x37] = 0;
  param_1[0x34] = 0;
  param_1[0x35] = 0;
  if (param_10 == 0) {
    param_1[3] = 1;
    param_1[0x18] = 0;
    param_1[0x19] = 0;
    if (1 < param_12) {
      param_11 = 0x400;
    }
    param_1[6] = param_11;
LAB_18000638b:
    param_1[0x1e] = param_1[0x18];
    param_1[0x24] = param_1[0x19];
    if (0 < (int)param_2) goto LAB_1800063a2;
  }
  else {
    param_1[6] = param_10;
    if ((param_12 == 0) && (param_11 % param_10 == 0)) {
      param_1[3] = 1;
LAB_180006387:
      param_1[0x18] = 0;
      param_1[0x19] = 0;
      goto LAB_18000638b;
    }
    param_1[3] = 0;
    if (((int)param_2 < 1) || ((int)param_5 < 1)) goto LAB_180006387;
    if (param_12 == 0) {
      uVar1 = FUN_180005570(param_11,param_10);
      if (param_11 < param_10) {
        param_10 = 0;
        uVar5 = uVar1;
      }
      else {
        uVar5 = 0;
        param_10 = uVar1;
      }
    }
    else {
      uVar5 = 0;
    }
    param_1[0x18] = uVar5;
    param_1[0x19] = param_10;
    param_1[0x1e] = param_1[0x18];
    param_1[0x24] = param_10;
LAB_1800063a2:
    uVar2 = Pa_GetSampleSize(param_4);
    if ((int)(uint)uVar2 < 1) goto LAB_18000663e;
    param_1[8] = (uint)uVar2;
    uVar2 = Pa_GetSampleSize(param_3);
    if ((int)(uint)uVar2 < 1) goto LAB_18000663e;
    param_1[9] = (uint)uVar2;
    uVar5 = param_9;
    if ((((param_9 & 2) == 0) && ((param_4 & 4) != 0)) && ((param_3 & 8) != 0)) {
      uVar5 = param_9 | 2;
    }
    puVar3 = FUN_180003120(param_4,param_3,uVar5);
    *(undefined **)(param_1 + 0xc) = puVar3;
    puVar3 = FUN_180003800(param_3);
    *(undefined **)(param_1 + 0xe) = puVar3;
    param_1[10] = ~param_3 >> 0x1f;
    param_1[0x29] = ~param_4 >> 0x1f;
    param_1[5] = (uint)((param_3 & 0x7fffffff) == (param_4 & 0x7fffffff));
    lVar4 = FUN_180020230(param_1[9] * param_1[6] * param_2);
    *(longlong *)(param_1 + 0x1a) = lVar4;
    if (lVar4 == 0) goto LAB_18000663e;
    if ((int)param_3 < 0) {
      lVar4 = FUN_180020230(param_2 * 8);
      *(longlong *)(param_1 + 0x1c) = lVar4;
      if (lVar4 == 0) goto LAB_18000663e;
    }
    lVar4 = FUN_180020230(param_2 << 5);
    *(longlong *)(param_1 + 0x2c) = lVar4;
    if (lVar4 == 0) goto LAB_18000663e;
    *(longlong *)(param_1 + 0x2e) = (longlong)(int)param_2 * 0x10 + lVar4;
  }
  if ((int)param_5 < 1) {
LAB_18000658d:
    FUN_180003a10(param_1 + 0x38);
    dVar6 = DAT_1800235c8 / param_8;
    *(longlong *)(param_1 + 0x3e) = param_13;
    *(undefined8 *)(param_1 + 0x40) = param_14;
    *(double *)(param_1 + 0x3c) = dVar6;
    return unaff_RDI;
  }
  uVar2 = Pa_GetSampleSize(param_7);
  if (0 < (int)(uint)uVar2) {
    param_1[0x11] = (uint)uVar2;
    uVar2 = Pa_GetSampleSize(param_6);
    if (0 < (int)(uint)uVar2) {
      param_1[0x12] = (uint)uVar2;
      puVar3 = FUN_180003120(param_6,param_7,param_9);
      *(undefined **)(param_1 + 0x14) = puVar3;
      puVar3 = FUN_180003800(param_7);
      *(undefined **)(param_1 + 0x16) = puVar3;
      param_1[0x13] = ~param_6 >> 0x1f;
      param_1[0x30] = ~param_7 >> 0x1f;
      param_1[4] = (uint)((param_6 & 0x7fffffff) == (param_7 & 0x7fffffff));
      lVar4 = FUN_180020230(param_1[0x12] * param_1[6] * param_5);
      *(longlong *)(param_1 + 0x20) = lVar4;
      if (lVar4 != 0) {
        if ((int)param_6 < 0) {
          lVar4 = FUN_180020230(param_5 * 8);
          *(longlong *)(param_1 + 0x22) = lVar4;
          if (lVar4 == 0) goto LAB_18000663e;
        }
        lVar4 = FUN_180020230(param_5 << 5);
        *(longlong *)(param_1 + 0x34) = lVar4;
        if (lVar4 != 0) {
          *(longlong *)(param_1 + 0x36) = (longlong)(int)param_5 * 0x10 + lVar4;
          goto LAB_18000658d;
        }
      }
    }
  }
LAB_18000663e:
  if (*(longlong *)(param_1 + 0x1a) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x1a));
  }
  if (*(longlong *)(param_1 + 0x1c) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x1c));
  }
  if (*(longlong *)(param_1 + 0x2c) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x2c));
  }
  if (*(longlong *)(param_1 + 0x20) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x20));
  }
  if (*(longlong *)(param_1 + 0x22) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x22));
  }
  if (*(longlong *)(param_1 + 0x34) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x34));
  }
  return unaff_RDI;
}



/* ========================================================================
   ENTRY: 1800066b0
   NAME : FUN_1800066b0
   SIG  : bool __fastcall FUN_1800066b0(longlong param_1)
   ======================================================================== */

bool FUN_1800066b0(longlong param_1)

{
  return *(int *)(param_1 + 0x90) == 0;
}



/* ========================================================================
   ENTRY: 1800066c0
   NAME : FUN_1800066c0
   SIG  : undefined __fastcall FUN_1800066c0(longlong param_1)
   ======================================================================== */

void FUN_1800066c0(longlong param_1)

{
  *(int *)(param_1 + 0x78) = *(int *)(param_1 + 0x60);
  *(undefined4 *)(param_1 + 0x90) = *(undefined4 *)(param_1 + 100);
  if (*(int *)(param_1 + 0x60) != 0) {
    memset(*(void **)(param_1 + 0x68),0,
           (ulonglong)
           (uint)(*(int *)(param_1 + 0x24) * *(int *)(param_1 + 0x1c) * *(int *)(param_1 + 0x18)));
  }
  if (*(int *)(param_1 + 0x90) != 0) {
    memset(*(void **)(param_1 + 0x80),0,
           (ulonglong)
           (uint)(*(int *)(param_1 + 0x48) * *(int *)(param_1 + 0x40) * *(int *)(param_1 + 0x18)));
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180006750
   NAME : FUN_180006750
   SIG  : undefined __fastcall FUN_180006750(longlong param_1, undefined4 param_2)
   ======================================================================== */

void FUN_180006750(longlong param_1,undefined4 param_2)

{
  *(undefined4 *)(param_1 + 0xac) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180006760
   NAME : FUN_180006760
   SIG  : undefined __fastcall FUN_180006760(longlong param_1, int param_2, longlong param_3, uint param_4)
   ======================================================================== */

void FUN_180006760(longlong param_1,int param_2,longlong param_3,uint param_4)

{
  uint uVar1;
  uint uVar2;
  
  if (param_4 == 0) {
    param_4 = *(uint *)(param_1 + 0x1c);
  }
  uVar2 = 0;
  if (param_4 != 0) {
    do {
      uVar1 = uVar2 + param_2;
      uVar2 = uVar2 + 1;
      *(longlong *)(*(longlong *)(param_1 + 0xb8) + (ulonglong)uVar1 * 0x10) = param_3;
      param_3 = param_3 + (ulonglong)*(uint *)(param_1 + 0x20);
      *(uint *)(*(longlong *)(param_1 + 0xb8) + 8 + (ulonglong)uVar1 * 0x10) = param_4;
    } while (uVar2 < param_4);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800067b0
   NAME : FUN_1800067b0
   SIG  : undefined __fastcall FUN_1800067b0(longlong param_1, int param_2, longlong param_3, uint param_4)
   ======================================================================== */

void FUN_1800067b0(longlong param_1,int param_2,longlong param_3,uint param_4)

{
  uint uVar1;
  
  if (param_4 == 0) {
    param_4 = *(uint *)(param_1 + 0x40);
  }
  uVar1 = 0;
  if (param_4 != 0) {
    do {
      FUN_180006810(param_1,uVar1 + param_2,param_3,param_4);
      uVar1 = uVar1 + 1;
      param_3 = param_3 + (ulonglong)*(uint *)(param_1 + 0x44);
    } while (uVar1 < param_4);
  }
  return;
}



/* ========================================================================
   ENTRY: 180006810
   NAME : FUN_180006810
   SIG  : undefined __fastcall FUN_180006810(longlong param_1, uint param_2, undefined8 param_3, undefined4 param_4)
   ======================================================================== */

void FUN_180006810(longlong param_1,uint param_2,undefined8 param_3,undefined4 param_4)

{
  *(undefined8 *)(*(longlong *)(param_1 + 0xd8) + (ulonglong)param_2 * 0x10) = param_3;
  *(undefined4 *)(*(longlong *)(param_1 + 0xd8) + 8 + (ulonglong)param_2 * 0x10) = param_4;
  return;
}



/* ========================================================================
   ENTRY: 180006830
   NAME : FUN_180006830
   SIG  : undefined __fastcall FUN_180006830(longlong param_1, undefined4 param_2)
   ======================================================================== */

void FUN_180006830(longlong param_1,undefined4 param_2)

{
  *(undefined4 *)(param_1 + 200) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180006840
   NAME : FUN_180006840
   SIG  : undefined __fastcall FUN_180006840(longlong param_1, uint param_2, undefined8 param_3, undefined4 param_4)
   ======================================================================== */

void FUN_180006840(longlong param_1,uint param_2,undefined8 param_3,undefined4 param_4)

{
  *(undefined8 *)(*(longlong *)(param_1 + 0xb0) + (ulonglong)param_2 * 0x10) = param_3;
  *(undefined4 *)(*(longlong *)(param_1 + 0xb0) + 8 + (ulonglong)param_2 * 0x10) = param_4;
  return;
}



/* ========================================================================
   ENTRY: 180006860
   NAME : FUN_180006860
   SIG  : undefined __fastcall FUN_180006860(longlong param_1, int param_2)
   ======================================================================== */

void FUN_180006860(longlong param_1,int param_2)

{
  if (param_2 == 0) {
    *(undefined4 *)(param_1 + 0xa8) = *(undefined4 *)(param_1 + 4);
    return;
  }
  *(int *)(param_1 + 0xa8) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180006880
   NAME : FUN_180006880
   SIG  : undefined __fastcall FUN_180006880(longlong param_1, int param_2, longlong param_3, uint param_4)
   ======================================================================== */

void FUN_180006880(longlong param_1,int param_2,longlong param_3,uint param_4)

{
  uint uVar1;
  uint uVar2;
  
  if (param_4 == 0) {
    param_4 = *(uint *)(param_1 + 0x1c);
  }
  uVar2 = 0;
  if (param_4 != 0) {
    do {
      uVar1 = uVar2 + param_2;
      uVar2 = uVar2 + 1;
      *(longlong *)(*(longlong *)(param_1 + 0xb0) + (ulonglong)uVar1 * 0x10) = param_3;
      param_3 = param_3 + (ulonglong)*(uint *)(param_1 + 0x20);
      *(uint *)(*(longlong *)(param_1 + 0xb0) + 8 + (ulonglong)uVar1 * 0x10) = param_4;
    } while (uVar2 < param_4);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800068d0
   NAME : FUN_1800068d0
   SIG  : undefined __fastcall FUN_1800068d0(longlong param_1, int param_2, longlong param_3, uint param_4)
   ======================================================================== */

void FUN_1800068d0(longlong param_1,int param_2,longlong param_3,uint param_4)

{
  uint uVar1;
  
  if (param_4 == 0) {
    param_4 = *(uint *)(param_1 + 0x40);
  }
  uVar1 = 0;
  if (param_4 != 0) {
    do {
      FUN_180006980(param_1,uVar1 + param_2,param_3,param_4);
      uVar1 = uVar1 + 1;
      param_3 = param_3 + (ulonglong)*(uint *)(param_1 + 0x44);
    } while (uVar1 < param_4);
  }
  return;
}



/* ========================================================================
   ENTRY: 180006930
   NAME : FUN_180006930
   SIG  : undefined __fastcall FUN_180006930(longlong param_1)
   ======================================================================== */

void FUN_180006930(longlong param_1)

{
  **(undefined8 **)(param_1 + 0xb0) = 0;
  return;
}



/* ========================================================================
   ENTRY: 180006940
   NAME : FUN_180006940
   SIG  : undefined __fastcall FUN_180006940(longlong param_1, uint param_2, undefined8 param_3)
   ======================================================================== */

void FUN_180006940(longlong param_1,uint param_2,undefined8 param_3)

{
  *(undefined8 *)(*(longlong *)(param_1 + 0xb0) + (ulonglong)param_2 * 0x10) = param_3;
  *(undefined4 *)(*(longlong *)(param_1 + 0xb0) + 8 + (ulonglong)param_2 * 0x10) = 1;
  return;
}



/* ========================================================================
   ENTRY: 180006970
   NAME : FUN_180006970
   SIG  : undefined __fastcall FUN_180006970(longlong param_1, uint param_2, undefined8 param_3)
   ======================================================================== */

void FUN_180006970(longlong param_1,uint param_2,undefined8 param_3)

{
  FUN_180006980(param_1,param_2,param_3,1);
  return;
}



/* ========================================================================
   ENTRY: 180006980
   NAME : FUN_180006980
   SIG  : undefined __fastcall FUN_180006980(longlong param_1, uint param_2, undefined8 param_3, undefined4 param_4)
   ======================================================================== */

void FUN_180006980(longlong param_1,uint param_2,undefined8 param_3,undefined4 param_4)

{
  *(undefined8 *)(*(longlong *)(param_1 + 0xd0) + (ulonglong)param_2 * 0x10) = param_3;
  *(undefined4 *)(*(longlong *)(param_1 + 0xd0) + 8 + (ulonglong)param_2 * 0x10) = param_4;
  return;
}



/* ========================================================================
   ENTRY: 1800069a0
   NAME : FUN_1800069a0
   SIG  : undefined __fastcall FUN_1800069a0(longlong param_1, int param_2)
   ======================================================================== */

void FUN_1800069a0(longlong param_1,int param_2)

{
  if (param_2 == 0) {
    *(undefined4 *)(param_1 + 0xc4) = *(undefined4 *)(param_1 + 4);
    return;
  }
  *(int *)(param_1 + 0xc4) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 1800069c0
   NAME : FUN_1800069c0
   SIG  : undefined __fastcall FUN_1800069c0(longlong param_1)
   ======================================================================== */

void FUN_1800069c0(longlong param_1)

{
  if (*(longlong *)(param_1 + 0x68) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x68));
  }
  if (*(longlong *)(param_1 + 0x70) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x70));
  }
  if (*(longlong *)(param_1 + 0xb0) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0xb0));
  }
  if (*(longlong *)(param_1 + 0x80) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x80));
  }
  if (*(longlong *)(param_1 + 0x88) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0x88));
  }
  if (*(longlong *)(param_1 + 0xd0) != 0) {
    FUN_180020240(*(longlong *)(param_1 + 0xd0));
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180006a40
   NAME : FUN_180006a40
   SIG  : uint __fastcall FUN_180006a40(longlong param_1, uint param_2)
   ======================================================================== */

uint FUN_180006a40(longlong param_1,uint param_2)

{
  longlong lVar1;
  uint uVar2;
  longlong lVar3;
  
  lVar1 = *(longlong *)(param_1 + 0xd0);
  if (*(uint *)(param_1 + 0xc4) < param_2) {
    param_2 = *(uint *)(param_1 + 0xc4);
  }
  uVar2 = 0;
  if (*(int *)(param_1 + 0x40) == 0) {
    *(int *)(param_1 + 0xc4) = *(int *)(param_1 + 0xc4) + param_2;
    return param_2;
  }
  do {
    lVar3 = (ulonglong)uVar2 * 0x10;
    (**(code **)(param_1 + 0x58))
              (*(undefined8 *)(lVar1 + lVar3),*(undefined4 *)(lVar1 + 8 + lVar3),param_2);
    uVar2 = uVar2 + 1;
    *(longlong *)(lVar1 + lVar3) =
         *(longlong *)(lVar1 + lVar3) +
         (ulonglong)(param_2 * *(int *)(lVar1 + 8 + lVar3) * *(int *)(param_1 + 0x44));
  } while (uVar2 < *(uint *)(param_1 + 0x40));
  *(int *)(param_1 + 0xc4) = *(int *)(param_1 + 0xc4) + param_2;
  return param_2;
}



/* ========================================================================
   ENTRY: 180006ad0
   NAME : FUN_180006ad0
   SIG  : undefined __fastcall FUN_180006ad0(longlong param_1, int param_2)
   ======================================================================== */

void FUN_180006ad0(longlong param_1,int param_2)

{
  *(uint *)(param_1 + 8) = *(int *)(param_1 + 8) + param_2 & *(uint *)(param_1 + 0xc);
  return;
}



/* ========================================================================
   ENTRY: 180006ae0
   NAME : FUN_180006ae0
   SIG  : undefined __fastcall FUN_180006ae0(longlong param_1, int param_2)
   ======================================================================== */

void FUN_180006ae0(longlong param_1,int param_2)

{
  *(uint *)(param_1 + 4) = *(int *)(param_1 + 4) + param_2 & *(uint *)(param_1 + 0xc);
  return;
}



/* ========================================================================
   ENTRY: 180006af0
   NAME : FUN_180006af0
   SIG  : undefined __fastcall FUN_180006af0(longlong param_1)
   ======================================================================== */

void FUN_180006af0(longlong param_1)

{
  *(undefined4 *)(param_1 + 8) = 0;
  *(undefined4 *)(param_1 + 4) = 0;
  return;
}



/* ========================================================================
   ENTRY: 180006b00
   NAME : FUN_180006b00
   SIG  : uint __fastcall FUN_180006b00(longlong param_1)
   ======================================================================== */

uint FUN_180006b00(longlong param_1)

{
  return *(int *)(param_1 + 4) - *(int *)(param_1 + 8) & *(uint *)(param_1 + 0xc);
}



/* ========================================================================
   ENTRY: 180006b10
   NAME : FUN_180006b10
   SIG  : uint __fastcall FUN_180006b10(int * param_1, uint param_2, longlong * param_3, uint * param_4, undefined8 * param_5, int * param_6)
   ======================================================================== */

uint FUN_180006b10(int *param_1,uint param_2,longlong *param_3,uint *param_4,undefined8 *param_5,
                  int *param_6)

{
  uint uVar1;
  int iVar2;
  undefined8 uVar3;
  
  uVar1 = FUN_180006b00((longlong)param_1);
  iVar2 = *param_1;
  if ((int)uVar1 < (int)param_2) {
    param_2 = uVar1;
  }
  uVar1 = param_1[2] & param_1[4];
  *param_3 = (longlong)(int)(uVar1 * param_1[5]) + *(longlong *)(param_1 + 6);
  if (iVar2 < (int)(uVar1 + param_2)) {
    uVar1 = iVar2 - uVar1;
    *param_4 = uVar1;
    iVar2 = param_2 - uVar1;
    uVar3 = *(undefined8 *)(param_1 + 6);
  }
  else {
    iVar2 = 0;
    *param_4 = param_2;
    uVar3 = 0;
  }
  *param_5 = uVar3;
  *param_6 = iVar2;
  return param_2;
}



/* ========================================================================
   ENTRY: 180006ba0
   NAME : FUN_180006ba0
   SIG  : int __fastcall FUN_180006ba0(int * param_1)
   ======================================================================== */

int FUN_180006ba0(int *param_1)

{
  int iVar1;
  uint uVar2;
  
  iVar1 = *param_1;
  uVar2 = FUN_180006b00((longlong)param_1);
  return iVar1 - uVar2;
}



/* ========================================================================
   ENTRY: 180006bc0
   NAME : FUN_180006bc0
   SIG  : int __fastcall FUN_180006bc0(int * param_1, int param_2, longlong * param_3, int * param_4, undefined8 * param_5, int * param_6)
   ======================================================================== */

int FUN_180006bc0(int *param_1,int param_2,longlong *param_3,int *param_4,undefined8 *param_5,
                 int *param_6)

{
  int iVar1;
  undefined8 uVar2;
  int iVar3;
  uint uVar4;
  
  iVar1 = FUN_180006ba0(param_1);
  iVar3 = *param_1;
  if (iVar1 < param_2) {
    param_2 = iVar1;
  }
  uVar4 = param_1[1] & param_1[4];
  *param_3 = (longlong)(int)(uVar4 * param_1[5]) + *(longlong *)(param_1 + 6);
  if (iVar3 < (int)(uVar4 + param_2)) {
    iVar3 = iVar3 - uVar4;
    *param_4 = iVar3;
    iVar3 = param_2 - iVar3;
    uVar2 = *(undefined8 *)(param_1 + 6);
  }
  else {
    iVar3 = 0;
    *param_4 = param_2;
    uVar2 = 0;
  }
  *param_5 = uVar2;
  *param_6 = iVar3;
  return param_2;
}



/* ========================================================================
   ENTRY: 180006c50
   NAME : FUN_180006c50
   SIG  : undefined8 __fastcall FUN_180006c50(uint * param_1, uint param_2, uint param_3, undefined8 param_4)
   ======================================================================== */

undefined8 FUN_180006c50(uint *param_1,uint param_2,uint param_3,undefined8 param_4)

{
  if ((param_3 & param_3 - 1) != 0) {
    return 0xffffffff;
  }
  *param_1 = param_3;
  *(undefined8 *)(param_1 + 6) = param_4;
  FUN_180006af0((longlong)param_1);
  param_1[4] = param_3 - 1;
  param_1[3] = param_3 * 2 - 1;
  param_1[5] = param_2;
  return 0;
}



/* ========================================================================
   ENTRY: 180006ca0
   NAME : FUN_180006ca0
   SIG  : uint __fastcall FUN_180006ca0(int * param_1, void * param_2, uint param_3)
   ======================================================================== */

uint FUN_180006ca0(int *param_1,void *param_2,uint param_3)

{
  uint uVar1;
  uint local_res20 [2];
  int local_28 [2];
  void *local_20;
  void *local_18 [2];
  
  uVar1 = FUN_180006b10(param_1,param_3,(longlong *)&local_20,local_res20,local_18,local_28);
  memcpy(param_2,local_20,(longlong)(int)(param_1[5] * local_res20[0]));
  if (0 < local_28[0]) {
    memcpy((void *)((longlong)(int)(param_1[5] * local_res20[0]) + (longlong)param_2),local_18[0],
           (longlong)(param_1[5] * local_28[0]));
  }
  FUN_180006ad0((longlong)param_1,uVar1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006d40
   NAME : FUN_180006d40
   SIG  : int __fastcall FUN_180006d40(int * param_1, void * param_2, int param_3)
   ======================================================================== */

int FUN_180006d40(int *param_1,void *param_2,int param_3)

{
  int iVar1;
  int local_res20 [2];
  int local_28 [2];
  void *local_20;
  void *local_18 [2];
  
  iVar1 = FUN_180006bc0(param_1,param_3,(longlong *)&local_20,local_res20,local_18,local_28);
  memcpy(local_20,param_2,(longlong)(param_1[5] * local_res20[0]));
  if (0 < local_28[0]) {
    memcpy(local_18[0],(void *)((longlong)(param_1[5] * local_res20[0]) + (longlong)param_2),
           (longlong)(param_1[5] * local_28[0]));
  }
  FUN_180006ae0((longlong)param_1,iVar1);
  return iVar1;
}



/* ========================================================================
   ENTRY: 180006e10
   NAME : FUN_180006e10
   SIG  : undefined __fastcall FUN_180006e10(undefined8 * param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4, undefined8 param_5, undefined8 param_6, undefined8 param_7, undefined8 param_8, undefined8 param_9, undefined8 param_10, undefined8 param_11, undefined8 param_12, undefined8 param_13)
   ======================================================================== */

void FUN_180006e10(undefined8 *param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4,
                  undefined8 param_5,undefined8 param_6,undefined8 param_7,undefined8 param_8,
                  undefined8 param_9,undefined8 param_10,undefined8 param_11,undefined8 param_12,
                  undefined8 param_13)

{
  param_1[3] = param_5;
  param_1[4] = param_6;
  param_1[5] = param_7;
  param_1[6] = param_8;
  param_1[7] = param_9;
  param_1[8] = param_10;
  param_1[9] = param_11;
  param_1[10] = param_12;
  param_1[0xb] = param_13;
  *param_1 = param_2;
  param_1[1] = param_3;
  param_1[2] = param_4;
  return;
}



/* ========================================================================
   ENTRY: 180006e70
   NAME : FUN_180006e70
   SIG  : undefined __fastcall FUN_180006e70(undefined4 * param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

void FUN_180006e70(undefined4 *param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  *param_1 = 0x18273645;
  *(undefined8 *)(param_1 + 2) = 0;
  *(undefined8 *)(param_1 + 8) = 0;
  *(undefined8 *)(param_1 + 0xe) = 0;
  *(undefined8 *)(param_1 + 0x10) = 0;
  *(undefined8 *)(param_1 + 0x12) = 0;
  *(undefined8 *)(param_1 + 4) = param_2;
  *(undefined8 *)(param_1 + 6) = param_3;
  *(undefined8 *)(param_1 + 10) = param_4;
  return;
}



/* ========================================================================
   ENTRY: 180006ea0
   NAME : FUN_180006ea0
   SIG  : undefined __fastcall FUN_180006ea0(undefined4 * param_1)
   ======================================================================== */

void FUN_180006ea0(undefined4 *param_1)

{
  *param_1 = 0;
  return;
}



/* ========================================================================
   ENTRY: 180006eb0
   NAME : FUN_180006eb0
   SIG  : undefined8 __fastcall FUN_180006eb0(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180006eb0(undefined4 param_1)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180006ec8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x60))(DAT_18002bad0,param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006ed0
   NAME : FUN_180006ed0
   SIG  : undefined8 __fastcall FUN_180006ed0(void)
   ======================================================================== */

undefined8 FUN_180006ed0(void)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180006ee5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0xa8))();
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006ef0
   NAME : FUN_180006ef0
   SIG  : undefined8 __fastcall FUN_180006ef0(longlong param_1, int param_2, undefined4 param_3, undefined8 param_4)
   ======================================================================== */

undefined8 FUN_180006ef0(longlong param_1,int param_2,undefined4 param_3,undefined8 param_4)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 != (longlong *)0x0) {
    uVar1 = (**(code **)(*DAT_18002bad0 + 0x98))(DAT_18002bad0,param_1,param_2,param_3,param_4);
    return uVar1;
  }
  if (0 < param_2) {
    do {
      *(undefined8 *)(param_1 + 0x10) = 0;
      *(undefined8 *)(param_1 + 8) = 0;
      param_1 = param_1 + 0x18;
      param_2 = param_2 + -1;
    } while (param_2 != 0);
  }
  return 0xfffffc18;
}



/* ========================================================================
   ENTRY: 180006f50
   NAME : FUN_180006f50
   SIG  : undefined8 __fastcall FUN_180006f50(void)
   ======================================================================== */

undefined8 FUN_180006f50(void)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180006f65. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0xa0))();
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006f70
   NAME : FUN_180006f70
   SIG  : undefined __fastcall FUN_180006f70(void)
   ======================================================================== */

void FUN_180006f70(void)

{
  if (DAT_18002bad0 != 0) {
    FUN_180007450(DAT_18002bad8);
  }
  DAT_18002bad0 = 0;
  return;
}



/* ========================================================================
   ENTRY: 180006fa0
   NAME : FUN_180006fa0
   SIG  : undefined8 __fastcall FUN_180006fa0(undefined4 * param_1, undefined4 * param_2, undefined4 * param_3, undefined4 * param_4)
   ======================================================================== */

undefined8
FUN_180006fa0(undefined4 *param_1,undefined4 *param_2,undefined4 *param_3,undefined4 *param_4)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    *param_4 = 0;
    *param_3 = 0;
    *param_2 = 0;
    *param_1 = 0;
    return 0xfffffc18;
  }
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x58))(DAT_18002bad0,param_1,param_2,param_3,param_4);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180006ff0
   NAME : FUN_180006ff0
   SIG  : undefined8 __fastcall FUN_180006ff0(longlong param_1)
   ======================================================================== */

undefined8 FUN_180006ff0(longlong param_1)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    *(undefined4 *)(param_1 + 0x14) = DAT_180023a00;
    *(undefined1 *)(param_1 + 0x18) = DAT_180023a04;
    *(undefined4 *)(param_1 + 0xc) = 0xffffffff;
    *(undefined4 *)(param_1 + 0x10) = 0;
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007029. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x90))(DAT_18002bad0,param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007030
   NAME : FUN_180007030
   SIG  : undefined8 __fastcall FUN_180007030(undefined4 * param_1, undefined4 * param_2)
   ======================================================================== */

undefined8 FUN_180007030(undefined4 *param_1,undefined4 *param_2)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    *param_2 = 0;
    *param_1 = 0;
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007055. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x48))(DAT_18002bad0,param_1,param_2);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007060
   NAME : FUN_180007060
   SIG  : undefined8 __fastcall FUN_180007060(undefined4 * param_1, undefined4 * param_2)
   ======================================================================== */

undefined8 FUN_180007060(undefined4 *param_1,undefined4 *param_2)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    *param_2 = 0;
    *param_1 = 0;
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007085. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x50))(DAT_18002bad0,param_1,param_2);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007090
   NAME : FUN_180007090
   SIG  : undefined8 __fastcall FUN_180007090(undefined8 param_1, undefined8 param_2)
   ======================================================================== */

undefined8 FUN_180007090(undefined8 param_1,undefined8 param_2)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800070ae. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x88))(DAT_18002bad0,param_1,param_2);
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800070c0
   NAME : FUN_1800070c0
   SIG  : undefined8 __fastcall FUN_1800070c0(undefined8 param_1)
   ======================================================================== */

undefined8 FUN_1800070c0(undefined8 param_1)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800070d8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x68))(DAT_18002bad0,param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800070e0
   NAME : FUN_1800070e0
   SIG  : undefined8 __fastcall FUN_1800070e0(longlong param_1)
   ======================================================================== */

undefined8 FUN_1800070e0(longlong param_1)

{
  undefined8 uVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  int iVar4;
  undefined4 uVar5;
  
  uVar5 = s_p_ANo_ASIO_Driver_1800239d5._11_4_;
  *(undefined8 *)(param_1 + 8) = s_p_ANo_ASIO_Driver_1800239d5._3_8_;
  *(undefined4 *)(param_1 + 0x10) = uVar5;
  *(undefined2 *)(param_1 + 0x14) = s_p_ANo_ASIO_Driver_1800239d5._15_2_;
  *(char *)(param_1 + 0x16) = s_p_ANo_ASIO_Driver_1800239d5[0x11];
  *(undefined4 *)(param_1 + 4) = 0;
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
  iVar4 = (**(code **)(*DAT_18002bad0 + 0x18))(DAT_18002bad0,*(undefined8 *)(param_1 + 0xa4));
  uVar3 = s_No_ASIO_Driver_Error_1800239e8._12_4_;
  uVar2 = s_No_ASIO_Driver_Error_1800239e8._8_4_;
  uVar5 = s_No_ASIO_Driver_Error_1800239e8._4_4_;
  if (iVar4 == 0) {
    (**(code **)(*DAT_18002bad0 + 0x30))(DAT_18002bad0,param_1 + 0x28);
    DAT_18002bad0 = (longlong *)0x0;
    return 0xfffffc18;
  }
  uVar1 = CONCAT53(s_No_ASIO_Driver_Error_1800239e8._16_5_,s_No_ASIO_Driver_Error_1800239e8._13_3_);
  *(undefined4 *)(param_1 + 0x28) = s_No_ASIO_Driver_Error_1800239e8._0_4_;
  *(undefined4 *)(param_1 + 0x2c) = uVar5;
  *(undefined4 *)(param_1 + 0x30) = uVar2;
  *(undefined4 *)(param_1 + 0x34) = uVar3;
  *(undefined8 *)(param_1 + 0x35) = uVar1;
  (**(code **)(*DAT_18002bad0 + 0x20))(DAT_18002bad0,param_1 + 8);
  uVar5 = (**(code **)(*DAT_18002bad0 + 0x28))();
  *(undefined4 *)(param_1 + 4) = uVar5;
  return 0;
}



/* ========================================================================
   ENTRY: 1800071e0
   NAME : FUN_1800071e0
   SIG  : undefined8 __fastcall FUN_1800071e0(void)
   ======================================================================== */

undefined8 FUN_1800071e0(void)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800071f5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0xb8))();
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007200
   NAME : FUN_180007200
   SIG  : undefined8 __fastcall FUN_180007200(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180007200(undefined4 param_1)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007218. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x70))(DAT_18002bad0,param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007220
   NAME : FUN_180007220
   SIG  : undefined8 __fastcall FUN_180007220(void)
   ======================================================================== */

undefined8 FUN_180007220(void)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007235. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x38))();
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007240
   NAME : FUN_180007240
   SIG  : undefined8 __fastcall FUN_180007240(void)
   ======================================================================== */

undefined8 FUN_180007240(void)

{
  undefined8 uVar1;
  
  if (DAT_18002bad0 == (longlong *)0x0) {
    return 0xfffffc18;
  }
                    /* WARNING: Could not recover jumptable at 0x000180007255. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*DAT_18002bad0 + 0x40))();
  return uVar1;
}



/* ========================================================================
   ENTRY: 180007260
   NAME : FUN_180007260
   SIG  : longlong * __fastcall FUN_180007260(longlong * param_1)
   ======================================================================== */

longlong * FUN_180007260(longlong *param_1)

{
  FUN_180007480(param_1);
  *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
  return param_1;
}



/* ========================================================================
   ENTRY: 180007280
   NAME : thunk_FUN_180007580
   SIG  : undefined __fastcall thunk_FUN_180007580(undefined8 * param_1)
   ======================================================================== */

void thunk_FUN_180007580(undefined8 *param_1)

{
  if (*(int *)(param_1 + 1) != 0) {
    FUN_180007750((void *)*param_1);
                    /* WARNING: Could not recover jumptable at 0x000180007597. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    CoUninitialize();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180007290
   NAME : FUN_180007290
   SIG  : bool __fastcall FUN_180007290(undefined8 * param_1, char * param_2)
   ======================================================================== */

bool FUN_180007290(undefined8 *param_1,char *param_2)

{
  undefined8 uVar1;
  
  if (-1 < *(int *)((longlong)param_1 + 0x14)) {
    uVar1 = FUN_1800075f0(param_1,*(int *)((longlong)param_1 + 0x14),param_2,0x20);
    return (int)uVar1 == 0;
  }
  *param_2 = '\0';
  return false;
}



/* ========================================================================
   ENTRY: 1800072c0
   NAME : FUN_1800072c0
   SIG  : int __fastcall FUN_1800072c0(undefined8 * param_1, longlong param_2, int param_3)
   ======================================================================== */

int FUN_1800072c0(undefined8 *param_1,longlong param_2,int param_3)

{
  int iVar1;
  int iVar2;
  
  iVar2 = 0;
  iVar1 = FUN_1800076a0((longlong)param_1);
  if (0 < iVar1) {
    do {
      if (param_3 <= iVar2) break;
      FUN_1800075f0(param_1,iVar2,*(char **)(param_2 + (longlong)iVar2 * 8),0x20);
      iVar2 = iVar2 + 1;
      iVar1 = FUN_1800076a0((longlong)param_1);
    } while (iVar2 < iVar1);
  }
  iVar1 = FUN_1800076a0((longlong)param_1);
  if (iVar1 < param_3) {
    iVar1 = FUN_1800076a0((longlong)param_1);
    return iVar1;
  }
  return param_3;
}



/* ========================================================================
   ENTRY: 180007340
   NAME : FUN_180007340
   SIG  : ulonglong __fastcall FUN_180007340(undefined8 * param_1, char * param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_180007340(undefined8 *param_1,char *param_2)

{
  int iVar1;
  HRESULT HVar2;
  undefined4 extraout_var;
  undefined8 uVar3;
  undefined4 extraout_var_00;
  undefined4 extraout_var_01;
  undefined4 extraout_var_02;
  ulonglong uVar4;
  int iVar5;
  undefined1 auStack_b8 [32];
  char local_98 [64];
  char local_58 [64];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_b8;
  iVar5 = 0;
  iVar1 = FUN_1800076a0((longlong)param_1);
  uVar4 = CONCAT44(extraout_var,iVar1);
  if (0 < iVar1) {
    do {
      uVar3 = FUN_1800075f0(param_1,iVar5,local_58,0x20);
      if (((int)uVar3 == 0) && (iVar1 = strcmp(param_2,local_58), iVar1 == 0)) {
        local_98[0] = '\0';
        FUN_180007290(param_1,local_98);
        FUN_180007450(param_1);
        HVar2 = FUN_1800076b0(param_1,iVar5,(LPVOID *)&DAT_18002bad0);
        uVar4 = CONCAT44(extraout_var_01,HVar2);
        if (HVar2 == 0) {
          *(int *)((longlong)param_1 + 0x14) = iVar5;
          return CONCAT71((int7)(uVar4 >> 8),1);
        }
        DAT_18002bad0 = 0;
        if (local_98[0] != '\0') {
          iVar1 = strcmp(local_58,local_98);
          uVar4 = CONCAT44(extraout_var_02,iVar1);
          if (iVar1 != 0) {
            uVar4 = FUN_180007340(param_1,local_98);
          }
        }
        break;
      }
      iVar5 = iVar5 + 1;
      iVar1 = FUN_1800076a0((longlong)param_1);
      uVar4 = CONCAT44(extraout_var_00,iVar1);
    } while (iVar5 < iVar1);
  }
  return uVar4 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 180007450
   NAME : FUN_180007450
   SIG  : undefined __fastcall FUN_180007450(undefined8 * param_1)
   ======================================================================== */

void FUN_180007450(undefined8 *param_1)

{
  if (*(int *)((longlong)param_1 + 0x14) != -1) {
    FUN_1800075b0(param_1,*(int *)((longlong)param_1 + 0x14));
    *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
    return;
  }
  *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
  return;
}



/* ========================================================================
   ENTRY: 180007480
   NAME : FUN_180007480
   SIG  : longlong * __fastcall FUN_180007480(longlong * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

longlong * FUN_180007480(longlong *param_1)

{
  LSTATUS LVar1;
  int *piVar2;
  longlong lVar3;
  int iVar4;
  DWORD dwIndex;
  undefined1 auStack_c8 [32];
  HKEY local_a8 [2];
  BYTE local_98 [128];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_c8;
  dwIndex = 0;
  *(undefined4 *)(param_1 + 1) = 0;
  *param_1 = 0;
  local_a8[0] = (HKEY)0x0;
  LVar1 = RegOpenKeyA((HKEY)0xffffffff80000002,"software\\asio",local_a8);
  if (LVar1 == 0) {
    while( true ) {
      LVar1 = RegEnumKeyA(local_a8[0],dwIndex,(LPSTR)local_98,0x80);
      if (LVar1 != 0) break;
      dwIndex = dwIndex + 1;
      piVar2 = FUN_1800079d0(local_a8[0],local_98,0,(int *)*param_1);
      *param_1 = (longlong)piVar2;
    }
  }
  if (local_a8[0] != (HKEY)0x0) {
    RegCloseKey(local_a8[0]);
  }
  lVar3 = *param_1;
  if (lVar3 != 0) {
    iVar4 = (int)param_1[1];
    do {
      iVar4 = iVar4 + 1;
      *(int *)(param_1 + 1) = iVar4;
      lVar3 = *(longlong *)(lVar3 + 0x2a0);
    } while (lVar3 != 0);
  }
  if ((int)param_1[1] != 0) {
    CoInitialize((LPVOID)0x0);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 180007580
   NAME : FUN_180007580
   SIG  : undefined __fastcall FUN_180007580(undefined8 * param_1)
   ======================================================================== */

void FUN_180007580(undefined8 *param_1)

{
  if (*(int *)(param_1 + 1) != 0) {
    FUN_180007750((void *)*param_1);
                    /* WARNING: Could not recover jumptable at 0x000180007597. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    CoUninitialize();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800075b0
   NAME : FUN_1800075b0
   SIG  : undefined8 __fastcall FUN_1800075b0(undefined8 * param_1, int param_2)
   ======================================================================== */

undefined8 FUN_1800075b0(undefined8 *param_1,int param_2)

{
  int *piVar1;
  
  piVar1 = FUN_1800079b0(param_2,(int *)*param_1);
  if ((piVar1 != (int *)0x0) && (*(longlong **)(piVar1 + 0xa6) != (longlong *)0x0)) {
    (**(code **)(**(longlong **)(piVar1 + 0xa6) + 0x10))();
    piVar1[0xa6] = 0;
    piVar1[0xa7] = 0;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800075f0
   NAME : FUN_1800075f0
   SIG  : undefined8 __fastcall FUN_1800075f0(undefined8 * param_1, int param_2, char * param_3, uint param_4)
   ======================================================================== */

undefined8 FUN_1800075f0(undefined8 *param_1,int param_2,char *param_3,uint param_4)

{
  int *piVar1;
  size_t sVar2;
  
  if (param_3 == (char *)0x0) {
    return 0xffffec77;
  }
  piVar1 = FUN_1800079b0(param_2,(int *)*param_1);
  if (piVar1 != (int *)0x0) {
    piVar1 = piVar1 + 0x85;
    sVar2 = strlen((char *)piVar1);
    if (sVar2 < param_4) {
      strcpy(param_3,(char *)piVar1);
      return 0;
    }
    memcpy(param_3,piVar1,(longlong)(int)param_4 - 4);
    builtin_strncpy(param_3 + (longlong)(int)param_4 + -4,"...",4);
    return 0;
  }
  return 0xffffec75;
}



/* ========================================================================
   ENTRY: 1800076a0
   NAME : FUN_1800076a0
   SIG  : undefined4 __fastcall FUN_1800076a0(longlong param_1)
   ======================================================================== */

undefined4 FUN_1800076a0(longlong param_1)

{
  return *(undefined4 *)(param_1 + 8);
}



/* ========================================================================
   ENTRY: 1800076b0
   NAME : FUN_1800076b0
   SIG  : HRESULT __fastcall FUN_1800076b0(undefined8 * param_1, int param_2, LPVOID * param_3)
   ======================================================================== */

HRESULT FUN_1800076b0(undefined8 *param_1,int param_2,LPVOID *param_3)

{
  HRESULT HVar1;
  int *piVar2;
  
  if (param_3 == (LPVOID *)0x0) {
    return -0x1389;
  }
  piVar2 = FUN_1800079b0(param_2,(int *)*param_1);
  if (piVar2 == (int *)0x0) {
    HVar1 = -0x138b;
  }
  else {
    if (*(longlong *)(piVar2 + 0xa6) != 0) {
      return -0x138a;
    }
    HVar1 = CoCreateInstance((IID *)(piVar2 + 1),(LPUNKNOWN)0x0,1,(IID *)(piVar2 + 1),param_3);
    if (HVar1 == 0) {
      *(LPVOID *)(piVar2 + 0xa6) = *param_3;
      return 0;
    }
  }
  return HVar1;
}



/* ========================================================================
   ENTRY: 180007750
   NAME : FUN_180007750
   SIG  : undefined __fastcall FUN_180007750(void * param_1)
   ======================================================================== */

void FUN_180007750(void *param_1)

{
  if (param_1 != (void *)0x0) {
    FUN_180007750(*(void **)((longlong)param_1 + 0x2a0));
    if (*(longlong **)((longlong)param_1 + 0x298) != (longlong *)0x0) {
      (**(code **)(**(longlong **)((longlong)param_1 + 0x298) + 0x10))();
    }
    free(param_1);
  }
  return;
}



/* ========================================================================
   ENTRY: 180007790
   NAME : FUN_180007790
   SIG  : undefined4 __fastcall FUN_180007790(char * param_1, LPBYTE param_2, DWORD param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined4 FUN_180007790(char *param_1,LPBYTE param_2,DWORD param_3)

{
  bool bVar1;
  undefined4 uVar2;
  LSTATUS LVar3;
  int iVar4;
  HFILE HVar5;
  size_t sVar6;
  undefined4 uVar7;
  DWORD dwIndex;
  undefined1 auStackY_328 [32];
  DWORD local_2f8 [2];
  HKEY local_2f0;
  HKEY local_2e8;
  HKEY local_2e0;
  _OFSTRUCT local_2d8;
  CHAR local_248 [512];
  ulonglong local_48;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStackY_328;
  uVar7 = 0xffffffff;
  sVar6 = strlen(param_1);
  CharLowerBuffA(param_1,(DWORD)sVar6);
  LVar3 = RegOpenKeyA((HKEY)0xffffffff80000000,"clsid",&local_2f0);
  if (LVar3 == 0) {
    bVar1 = false;
    dwIndex = 0;
    do {
      do {
        if ((bVar1) || (LVar3 = RegEnumKeyA(local_2f0,dwIndex,local_248,0x200), LVar3 != 0))
        goto LAB_180007968;
        dwIndex = dwIndex + 1;
        sVar6 = strlen(local_248);
        CharLowerBuffA(local_248,(DWORD)sVar6);
        iVar4 = strcmp(local_248,param_1);
      } while (iVar4 != 0);
      LVar3 = RegOpenKeyExA(local_2f0,local_248,0,0x20019,&local_2e0);
      if (LVar3 == 0) {
        LVar3 = RegOpenKeyExA(local_2e0,"InprocServer32",0,0x20019,&local_2e8);
        if (LVar3 == 0) {
          local_2f8[1] = 1;
          local_2f8[0] = param_3;
          LVar3 = RegQueryValueExA(local_2e8,(LPCSTR)0x0,(LPDWORD)0x0,local_2f8 + 1,param_2,
                                   local_2f8);
          uVar2 = uVar7;
          if (LVar3 == 0) {
            local_2d8.cBytes = 0x88;
            local_2d8.szPathName[0x69] = '\0';
            local_2d8.szPathName[0x6a] = '\0';
            local_2d8.szPathName[0x6b] = '\0';
            local_2d8.szPathName[0x6c] = '\0';
            local_2d8.szPathName[0x6d] = '\0';
            local_2d8.szPathName[0x6e] = '\0';
            local_2d8.szPathName[0x6f] = '\0';
            local_2d8.szPathName[0x70] = '\0';
            local_2d8.szPathName[0x71] = '\0';
            local_2d8.szPathName[0x72] = '\0';
            local_2d8.szPathName[0x73] = '\0';
            local_2d8.szPathName[0x74] = '\0';
            local_2d8.szPathName[0x75] = '\0';
            local_2d8.szPathName[0x76] = '\0';
            local_2d8.szPathName[0x77] = '\0';
            local_2d8.szPathName[0x78] = '\0';
            local_2d8.szPathName[0x79] = '\0';
            local_2d8.szPathName[0x7a] = '\0';
            local_2d8.szPathName[0x7b] = '\0';
            local_2d8.szPathName[0x7c] = '\0';
            local_2d8.szPathName[0x7d] = '\0';
            local_2d8.szPathName[0x7e] = '\0';
            local_2d8.szPathName[0x7f] = '\0';
            local_2d8.fFixedDisk = '\0';
            local_2d8.nErrCode = 0;
            local_2d8.Reserved1 = 0;
            local_2d8.Reserved2 = 0;
            local_2d8.szPathName[0] = '\0';
            local_2d8.szPathName[1] = '\0';
            local_2d8.szPathName[2] = '\0';
            local_2d8.szPathName[3] = '\0';
            local_2d8.szPathName[4] = '\0';
            local_2d8.szPathName[5] = '\0';
            local_2d8.szPathName[6] = '\0';
            local_2d8.szPathName[7] = '\0';
            local_2d8.szPathName[8] = '\0';
            local_2d8.szPathName[9] = '\0';
            local_2d8.szPathName[10] = '\0';
            local_2d8.szPathName[0xb] = '\0';
            local_2d8.szPathName[0xc] = '\0';
            local_2d8.szPathName[0xd] = '\0';
            local_2d8.szPathName[0xe] = '\0';
            local_2d8.szPathName[0xf] = '\0';
            local_2d8.szPathName[0x10] = '\0';
            local_2d8.szPathName[0x11] = '\0';
            local_2d8.szPathName[0x12] = '\0';
            local_2d8.szPathName[0x13] = '\0';
            local_2d8.szPathName[0x14] = '\0';
            local_2d8.szPathName[0x15] = '\0';
            local_2d8.szPathName[0x16] = '\0';
            local_2d8.szPathName[0x17] = '\0';
            local_2d8.szPathName[0x18] = '\0';
            local_2d8.szPathName[0x19] = '\0';
            local_2d8.szPathName[0x1a] = '\0';
            local_2d8.szPathName[0x1b] = '\0';
            local_2d8.szPathName[0x1c] = '\0';
            local_2d8.szPathName[0x1d] = '\0';
            local_2d8.szPathName[0x1e] = '\0';
            local_2d8.szPathName[0x1f] = '\0';
            local_2d8.szPathName[0x20] = '\0';
            local_2d8.szPathName[0x21] = '\0';
            local_2d8.szPathName[0x22] = '\0';
            local_2d8.szPathName[0x23] = '\0';
            local_2d8.szPathName[0x24] = '\0';
            local_2d8.szPathName[0x25] = '\0';
            local_2d8.szPathName[0x26] = '\0';
            local_2d8.szPathName[0x27] = '\0';
            local_2d8.szPathName[0x28] = '\0';
            local_2d8.szPathName[0x29] = '\0';
            local_2d8.szPathName[0x2a] = '\0';
            local_2d8.szPathName[0x2b] = '\0';
            local_2d8.szPathName[0x2c] = '\0';
            local_2d8.szPathName[0x2d] = '\0';
            local_2d8.szPathName[0x2e] = '\0';
            local_2d8.szPathName[0x2f] = '\0';
            local_2d8.szPathName[0x30] = '\0';
            local_2d8.szPathName[0x31] = '\0';
            local_2d8.szPathName[0x32] = '\0';
            local_2d8.szPathName[0x33] = '\0';
            local_2d8.szPathName[0x34] = '\0';
            local_2d8.szPathName[0x35] = '\0';
            local_2d8.szPathName[0x36] = '\0';
            local_2d8.szPathName[0x37] = '\0';
            local_2d8.szPathName[0x38] = '\0';
            local_2d8.szPathName[0x39] = '\0';
            local_2d8.szPathName[0x3a] = '\0';
            local_2d8.szPathName[0x3b] = '\0';
            local_2d8.szPathName[0x3c] = '\0';
            local_2d8.szPathName[0x3d] = '\0';
            local_2d8.szPathName[0x3e] = '\0';
            local_2d8.szPathName[0x3f] = '\0';
            local_2d8.szPathName[0x40] = '\0';
            local_2d8.szPathName[0x41] = '\0';
            local_2d8.szPathName[0x42] = '\0';
            local_2d8.szPathName[0x43] = '\0';
            local_2d8.szPathName[0x44] = '\0';
            local_2d8.szPathName[0x45] = '\0';
            local_2d8.szPathName[0x46] = '\0';
            local_2d8.szPathName[0x47] = '\0';
            local_2d8.szPathName[0x48] = '\0';
            local_2d8.szPathName[0x49] = '\0';
            local_2d8.szPathName[0x4a] = '\0';
            local_2d8.szPathName[0x4b] = '\0';
            local_2d8.szPathName[0x4c] = '\0';
            local_2d8.szPathName[0x4d] = '\0';
            local_2d8.szPathName[0x4e] = '\0';
            local_2d8.szPathName[0x4f] = '\0';
            local_2d8.szPathName[0x50] = '\0';
            local_2d8.szPathName[0x51] = '\0';
            local_2d8.szPathName[0x52] = '\0';
            local_2d8.szPathName[0x53] = '\0';
            local_2d8.szPathName[0x54] = '\0';
            local_2d8.szPathName[0x55] = '\0';
            local_2d8.szPathName[0x56] = '\0';
            local_2d8.szPathName[0x57] = '\0';
            local_2d8.szPathName[0x58] = '\0';
            local_2d8.szPathName[0x59] = '\0';
            local_2d8.szPathName[0x5a] = '\0';
            local_2d8.szPathName[0x5b] = '\0';
            local_2d8.szPathName[0x5c] = '\0';
            local_2d8.szPathName[0x5d] = '\0';
            local_2d8.szPathName[0x5e] = '\0';
            local_2d8.szPathName[0x5f] = '\0';
            local_2d8.szPathName[0x60] = '\0';
            local_2d8.szPathName[0x61] = '\0';
            local_2d8.szPathName[0x62] = '\0';
            local_2d8.szPathName[99] = '\0';
            local_2d8.szPathName[100] = '\0';
            local_2d8.szPathName[0x65] = '\0';
            local_2d8.szPathName[0x66] = '\0';
            local_2d8.szPathName[0x67] = '\0';
            local_2d8.szPathName[0x68] = '\0';
            HVar5 = OpenFile((LPCSTR)param_2,&local_2d8,0x4000);
            uVar2 = 0;
            if (HVar5 == 0) {
              uVar2 = uVar7;
            }
          }
          uVar7 = uVar2;
          RegCloseKey(local_2e8);
        }
        RegCloseKey(local_2e0);
      }
      bVar1 = true;
    } while (LVar3 == 0);
LAB_180007968:
    RegCloseKey(local_2f0);
  }
  return uVar7;
}



/* ========================================================================
   ENTRY: 1800079b0
   NAME : FUN_1800079b0
   SIG  : int * __fastcall FUN_1800079b0(int param_1, int * param_2)
   ======================================================================== */

int * FUN_1800079b0(int param_1,int *param_2)

{
  while( true ) {
    if (param_2 == (int *)0x0) {
      return (int *)0x0;
    }
    if (*param_2 == param_1) break;
    param_2 = *(int **)(param_2 + 0xa8);
  }
  return param_2;
}



/* ========================================================================
   ENTRY: 1800079d0
   NAME : FUN_1800079d0
   SIG  : int * __fastcall FUN_1800079d0(HKEY param_1, BYTE * param_2, int param_3, int * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int * FUN_1800079d0(HKEY param_1,BYTE *param_2,int param_3,int *param_4)

{
  LSTATUS LVar1;
  int iVar2;
  HRESULT HVar3;
  int *piVar4;
  BYTE *_Source;
  undefined1 auStackY_448 [32];
  DWORD local_418 [2];
  HKEY local_410;
  CLSID local_408;
  BYTE local_3f8 [256];
  WCHAR local_2f8 [104];
  BYTE local_228 [512];
  ulonglong local_28;
  
  local_28 = DAT_18002b580 ^ (ulonglong)auStackY_448;
  if (param_4 == (int *)0x0) {
    LVar1 = RegOpenKeyExA(param_1,(LPCSTR)param_2,0,0x20019,&local_410);
    if (LVar1 == 0) {
      local_418[1] = 1;
      local_418[0] = 0x100;
      LVar1 = RegQueryValueExA(local_410,"clsid",(LPDWORD)0x0,local_418 + 1,local_3f8,local_418);
      if (LVar1 == 0) {
        iVar2 = FUN_180007790((char *)local_3f8,local_228,0x200);
        if (iVar2 == 0) {
          param_4 = (int *)thunk_FUN_180021630(0x2a8);
          if (param_4 != (int *)0x0) {
            memset(param_4 + 1,0,0x2a4);
            *param_4 = param_3;
            MultiByteToWideChar(0,0,(LPCSTR)local_3f8,-1,local_2f8,100);
            HVar3 = CLSIDFromString(local_2f8,&local_408);
            if (HVar3 == 0) {
              param_4[1] = local_408.Data1;
              param_4[2] = local_408._4_4_;
              param_4[3] = local_408.Data4._0_4_;
              param_4[4] = local_408.Data4._4_4_;
            }
            local_418[1] = 1;
            local_418[0] = 0x100;
            LVar1 = RegQueryValueExA(local_410,"description",(LPDWORD)0x0,local_418 + 1,local_3f8,
                                     local_418);
            _Source = local_3f8;
            if (LVar1 != 0) {
              _Source = param_2;
            }
            strcpy((char *)(param_4 + 0x85),(char *)_Source);
          }
        }
      }
      RegCloseKey(local_410);
    }
  }
  else {
    piVar4 = FUN_1800079d0(param_1,param_2,param_3 + 1,*(int **)(param_4 + 0xa8));
    *(int **)(param_4 + 0xa8) = piVar4;
  }
  return param_4;
}



/* ========================================================================
   ENTRY: 180007b90
   NAME : FUN_180007b90
   SIG  : undefined __fastcall FUN_180007b90(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180007b90(int param_1)

{
  undefined8 uVar1;
  undefined4 uVar2;
  undefined1 auStack_d8 [32];
  undefined8 local_b8;
  undefined8 uStack_b0;
  undefined8 local_a8;
  undefined8 uStack_a0;
  undefined8 local_98;
  undefined8 uStack_90;
  undefined8 local_88;
  undefined8 uStack_80;
  undefined8 local_78;
  undefined8 uStack_70;
  undefined8 local_68;
  undefined8 uStack_60;
  undefined8 local_58;
  undefined8 uStack_50;
  undefined8 local_48;
  undefined8 uStack_40;
  undefined8 local_38;
  undefined8 uStack_30;
  undefined4 local_28;
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_d8;
  local_b8 = 0;
  uStack_b0 = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  local_98 = 0;
  uStack_90 = 0;
  local_88 = 0;
  uStack_80 = 0;
  local_78 = 0;
  uStack_70 = 0;
  local_68 = 0;
  uStack_60 = 0;
  local_28 = 0;
  local_58 = 0;
  uStack_50 = 0;
  local_48 = 0;
  uStack_40 = 0;
  local_38 = 0;
  uStack_30 = 0;
  uVar1 = FUN_180007090(&local_98,&uStack_a0);
  uVar2 = (undefined4)local_88;
  if ((int)uVar1 == 0) {
    uVar2 = 3;
  }
  local_88 = CONCAT44(local_88._4_4_,uVar2);
  FUN_180007c40((longlong)&local_b8,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180007c40
   NAME : FUN_180007c40
   SIG  : undefined8 __fastcall FUN_180007c40(longlong param_1, int param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180007c40(longlong param_1,int param_2)

{
  int *piVar1;
  int iVar2;
  double dVar3;
  double dVar4;
  bool bVar5;
  undefined7 extraout_var;
  uint uVar6;
  uint uVar7;
  longlong lVar8;
  uint uVar9;
  uint uVar10;
  undefined1 auStack_98 [32];
  uint local_78 [2];
  double local_70;
  double local_68;
  double local_60;
  ulonglong local_58;
  
  local_58 = DAT_18002b580 ^ (ulonglong)auStack_98;
  lVar8 = (longlong)param_2;
  if (DAT_18002bae8 != 0) {
    LOCK();
    piVar1 = (int *)(DAT_18002bae8 + 0x210);
    *piVar1 = *piVar1 + 1;
    dVar4 = _DAT_180023cc0;
    dVar3 = DAT_180023ca8;
    UNLOCK();
    if (*piVar1 == 0) {
      uVar9 = 0;
      uVar10 = uVar9;
      do {
        if ((int)uVar10 < 1) {
          if (*(char *)(DAT_18002bae8 + 0x20c) == '\0') {
            FUN_1800038c0(DAT_18002bae8 + 0x50);
            local_68 = ((double)*(uint *)(param_1 + 0x18) * dVar4 +
                       (double)*(uint *)(param_1 + 0x1c)) * dVar3;
            local_70 = local_68 -
                       (double)*(int *)(DAT_18002bae8 + 400) / *(double *)(DAT_18002bae8 + 0x48);
            local_60 = (double)*(int *)(DAT_18002bae8 + 0x194) / *(double *)(DAT_18002bae8 + 0x48) +
                       local_68;
            if ((*(longlong *)(DAT_18002bae8 + 0x1d0) != 0) &&
               (uVar6 = uVar9, 0 < *(int *)(DAT_18002bae8 + 0x198))) {
              do {
                (**(code **)(DAT_18002bae8 + 0x1d0))
                          (*(undefined8 *)
                            (*(longlong *)(DAT_18002bae8 + 0x1b0 + lVar8 * 8) +
                            (longlong)(int)uVar6 * 8),*(undefined4 *)(DAT_18002bae8 + 0x1d8),
                           *(undefined4 *)(DAT_18002bae8 + 0x178));
                uVar6 = uVar6 + 1;
              } while ((int)uVar6 < *(int *)(DAT_18002bae8 + 0x198));
            }
            FUN_180005ce0(DAT_18002bae8 + 0x68,&local_70,*(undefined4 *)(DAT_18002bae8 + 0x218));
            *(undefined4 *)(DAT_18002bae8 + 0x218) = 0;
            FUN_180006860(DAT_18002bae8 + 0x68,0);
            uVar6 = uVar9;
            if (0 < *(int *)(DAT_18002bae8 + 0x198)) {
              do {
                FUN_180006940(DAT_18002bae8 + 0x68,uVar6,
                              *(undefined8 *)
                               (*(longlong *)(DAT_18002bae8 + 0x1b0 + lVar8 * 8) +
                               (longlong)(int)uVar6 * 8));
                uVar6 = uVar6 + 1;
              } while ((int)uVar6 < *(int *)(DAT_18002bae8 + 0x198));
            }
            FUN_1800069a0(DAT_18002bae8 + 0x68,0);
            uVar6 = uVar9;
            if (0 < *(int *)(DAT_18002bae8 + 0x19c)) {
              do {
                FUN_180006970(DAT_18002bae8 + 0x68,uVar6,
                              *(undefined8 *)
                               (*(longlong *)(DAT_18002bae8 + 0x1c0 + lVar8 * 8) +
                               (longlong)(int)uVar6 * 8));
                uVar6 = uVar6 + 1;
              } while ((int)uVar6 < *(int *)(DAT_18002bae8 + 0x19c));
            }
            local_78[0] = (uint)(*(char *)(DAT_18002bae8 + 0x1ec) != '\0');
            uVar6 = FUN_180006010((uint *)(DAT_18002bae8 + 0x68),(int *)local_78);
            if ((*(longlong *)(DAT_18002bae8 + 0x1e0) != 0) &&
               (uVar7 = uVar9, 0 < *(int *)(DAT_18002bae8 + 0x19c))) {
              do {
                (**(code **)(DAT_18002bae8 + 0x1e0))
                          (*(undefined8 *)
                            (*(longlong *)(DAT_18002bae8 + 0x1c0 + lVar8 * 8) +
                            (longlong)(int)uVar7 * 8),*(undefined4 *)(DAT_18002bae8 + 0x1e8),
                           *(undefined4 *)(DAT_18002bae8 + 0x178));
                uVar7 = uVar7 + 1;
              } while ((int)uVar7 < *(int *)(DAT_18002bae8 + 0x19c));
            }
            FUN_1800038e0((double *)(DAT_18002bae8 + 0x50),uVar6);
            if (*(char *)(DAT_18002bae8 + 0x1a0) != '\0') {
              FUN_1800071e0();
            }
            if (local_78[0] != 0) {
              if (local_78[0] == 2) {
                *(undefined4 *)(DAT_18002bae8 + 0x208) = 0;
                if (*(code **)(DAT_18002bae8 + 0x20) != (code *)0x0) {
                  (**(code **)(DAT_18002bae8 + 0x20))(*(undefined8 *)(DAT_18002bae8 + 0x28));
                }
                *(undefined1 *)(DAT_18002bae8 + 0x200) = 1;
                SetEvent(*(HANDLE *)(DAT_18002bae8 + 0x1f8));
                *(undefined1 *)(DAT_18002bae8 + 0x20c) = 1;
              }
              else {
                *(undefined1 *)(DAT_18002bae8 + 0x1ec) = 1;
                bVar5 = FUN_1800066b0(DAT_18002bae8 + 0x68);
                if ((int)CONCAT71(extraout_var,bVar5) != 0) {
                  *(undefined1 *)(DAT_18002bae8 + 0x20c) = 1;
                  *(undefined4 *)(DAT_18002bae8 + 0x1f0) = 0;
                }
              }
            }
          }
          else {
            FUN_18000aa10(DAT_18002bae8,param_2);
            if (*(char *)(DAT_18002bae8 + 0x1a0) != '\0') {
              FUN_1800071e0();
            }
            if (((*(char *)(DAT_18002bae8 + 0x1ec) != '\0') && (*(int *)(DAT_18002bae8 + 0x1f0) < 2)
                ) && (*(int *)(DAT_18002bae8 + 0x1f0) = *(int *)(DAT_18002bae8 + 0x1f0) + 1,
                     *(int *)(DAT_18002bae8 + 0x1f0) == 2)) {
              *(undefined4 *)(DAT_18002bae8 + 0x208) = 0;
              if (*(code **)(DAT_18002bae8 + 0x20) != (code *)0x0) {
                (**(code **)(DAT_18002bae8 + 0x20))(*(undefined8 *)(DAT_18002bae8 + 0x28));
              }
              *(undefined1 *)(DAT_18002bae8 + 0x200) = 1;
              SetEvent(*(HANDLE *)(DAT_18002bae8 + 0x1f8));
            }
          }
        }
        else {
          if (0 < *(int *)(DAT_18002bae8 + 0x198)) {
            *(uint *)(DAT_18002bae8 + 0x218) = *(uint *)(DAT_18002bae8 + 0x218) | 2;
          }
          if (0 < *(int *)(DAT_18002bae8 + 0x19c)) {
            *(uint *)(DAT_18002bae8 + 0x218) = *(uint *)(DAT_18002bae8 + 0x218) | 4;
          }
        }
        uVar10 = uVar10 + 1;
        LOCK();
        piVar1 = (int *)(DAT_18002bae8 + 0x210);
        iVar2 = *piVar1;
        *piVar1 = *piVar1 + -1;
        UNLOCK();
      } while (-1 < iVar2 + -1);
    }
    else {
      *(int *)(DAT_18002bae8 + 0x214) = *(int *)(DAT_18002bae8 + 0x214) + 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180008180
   NAME : FUN_180008180
   SIG  : undefined8 __fastcall FUN_180008180(longlong param_1)
   ======================================================================== */

undefined8 FUN_180008180(longlong param_1)

{
  int iVar1;
  undefined8 uVar2;
  char *pcVar3;
  
  *(undefined1 *)(param_1 + 0x20c) = 1;
  uVar2 = FUN_180007240();
  iVar1 = (int)uVar2;
  if (iVar1 == 0) {
    uVar2 = 0;
    FUN_1800086a0(param_1);
  }
  else {
    uVar2 = 0xffffd8f1;
    pcVar3 = FUN_180009ad0(iVar1);
    FUN_180003c90(3,iVar1,pcVar3);
  }
  *(undefined4 *)(param_1 + 0x204) = 1;
  *(undefined4 *)(param_1 + 0x208) = 0;
  if ((*(char *)(param_1 + 0x200) == '\0') && (*(code **)(param_1 + 0x20) != (code *)0x0)) {
    (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180008210
   NAME : FUN_180008210
   SIG  : undefined8 __fastcall FUN_180008210(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180008210(undefined4 param_1)

{
  switch(param_1) {
  case 0:
  case 0x10:
    return 0x10;
  case 1:
  case 0x11:
    return 8;
  case 2:
  case 8:
  case 9:
  case 10:
  case 0xb:
  case 0x12:
  case 0x18:
  case 0x19:
  case 0x1a:
  case 0x1b:
    return 4;
  case 3:
  case 4:
  case 0x13:
  case 0x14:
    return 2;
  default:
    return 0x10000;
  }
}



/* ========================================================================
   ENTRY: 180008290
   NAME : FUN_180008290
   SIG  : undefined8 __fastcall FUN_180008290(void * param_1, void * param_2, uint param_3, undefined8 param_4, uint param_5, longlong * param_6)
   ======================================================================== */

undefined8
FUN_180008290(void *param_1,void *param_2,uint param_3,undefined8 param_4,uint param_5,
             longlong *param_6)

{
  longlong lVar1;
  int *piVar2;
  uint uVar3;
  uint uVar4;
  int iVar5;
  
  lVar1 = *param_6;
  piVar2 = *(int **)(lVar1 + 0x220);
  if (*(int *)(lVar1 + 0x19c) == 0) goto LAB_180008369;
  if ((param_5 & 0xffffd904) != 0) {
    piVar2[0x66] = 1;
  }
  uVar3 = FUN_180006b00((longlong)(piVar2 + 0xe));
  uVar4 = param_3;
  if ((int)uVar3 < (int)param_3) {
    piVar2[0x66] = 1;
    (**(code **)(piVar2 + 0x3a))(param_2,1,param_3 * piVar2[0x34]);
    if (*piVar2 != 0) {
      uVar4 = FUN_180006b00((longlong)(piVar2 + 0xe));
      if ((int)uVar4 < (int)param_3) {
        uVar4 = FUN_180006b00((longlong)(piVar2 + 0xe));
        goto LAB_180008337;
      }
    }
  }
  else {
LAB_180008337:
    FUN_180006ca0(piVar2 + 0xe,param_2,uVar4);
  }
  if (piVar2[3] != 0) {
    iVar5 = FUN_180006ba0(piVar2 + 0xe);
    if (piVar2[1] <= iVar5) {
      piVar2[3] = 0;
      piVar2[1] = 0;
      SetEvent(*(HANDLE *)(piVar2 + 6));
    }
  }
LAB_180008369:
  if (*(int *)(lVar1 + 0x198) != 0) {
    if ((param_5 & 0xffffd903) != 0) {
      piVar2[0x67] = 1;
    }
    iVar5 = FUN_180006ba0(piVar2 + 0x16);
    if (iVar5 < (int)param_3) {
      piVar2[0x67] = 1;
      FUN_180006ad0((longlong)(piVar2 + 0x16),param_3);
    }
    FUN_180006d40(piVar2 + 0x16,param_1,param_3);
    if (piVar2[4] != 0) {
      uVar4 = FUN_180006b00((longlong)(piVar2 + 0x16));
      if (piVar2[2] <= (int)uVar4) {
        piVar2[4] = 0;
        piVar2[2] = 0;
        SetEvent(*(HANDLE *)(piVar2 + 8));
      }
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180008400
   NAME : FUN_180008400
   SIG  : undefined8 __fastcall FUN_180008400(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180008400(undefined4 param_1)

{
  switch(param_1) {
  case 0:
  case 0x10:
    return 2;
  case 1:
  case 0x11:
    return 3;
  case 2:
  case 3:
  case 8:
  case 9:
  case 10:
  case 0xb:
  case 0x12:
  case 0x13:
  case 0x18:
  case 0x19:
  case 0x1a:
  case 0x1b:
    return 4;
  case 4:
  case 0x14:
    return 8;
  default:
    return 0;
  }
}



/* ========================================================================
   ENTRY: 180008470
   NAME : FUN_180008470
   SIG  : undefined8 __fastcall FUN_180008470(undefined4 * param_1)
   ======================================================================== */

undefined8 FUN_180008470(undefined4 *param_1)

{
  FUN_1800069c0((longlong)(param_1 + 0x1a));
  FUN_180006ea0(param_1);
  *(undefined4 *)(*(longlong *)(param_1 + 0x5c) + 0x128) = 0xffffffff;
  CloseHandle(*(HANDLE *)(param_1 + 0x7e));
  if (*(longlong *)(param_1 + 0x88) != 0) {
    FUN_1800069c0(*(longlong *)(param_1 + 0x88) + 0x90);
    if (param_1[0x66] != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(param_1 + 0x88) + 0x30));
      FUN_180020240(*(longlong *)(*(longlong *)(param_1 + 0x88) + 0x88));
      CloseHandle(*(HANDLE *)(*(longlong *)(param_1 + 0x88) + 0x20));
    }
    if (param_1[0x67] != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(param_1 + 0x88) + 0x28));
      FUN_180020240(*(longlong *)(*(longlong *)(param_1 + 0x88) + 0x80));
      CloseHandle(*(HANDLE *)(*(longlong *)(param_1 + 0x88) + 0x18));
    }
    FUN_180020240(*(longlong *)(param_1 + 0x88));
  }
  FUN_180020240(*(longlong *)(param_1 + 0x60));
  FUN_180020240(*(longlong *)(param_1 + 0x62));
  FUN_180020240(*(longlong *)(param_1 + 0x6a));
  FUN_180020240((longlong)param_1);
  FUN_180006f50();
  thunk_FUN_180006f70();
  DAT_18002bae8 = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 1800086a0
   NAME : FUN_1800086a0
   SIG  : undefined __fastcall FUN_1800086a0(longlong param_1)
   ======================================================================== */

void FUN_1800086a0(longlong param_1)

{
  int iVar1;
  int iVar2;
  
  iVar1 = *(int *)(param_1 + 0x210);
  for (iVar2 = 2000; (iVar1 != -1 && (0 < iVar2)); iVar2 = iVar2 + -1) {
    Sleep(1);
    iVar1 = *(int *)(param_1 + 0x210);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800086f0
   NAME : FUN_1800086f0
   SIG  : longlong * __fastcall FUN_1800086f0(longlong param_1, int * param_2, int param_3)
   ======================================================================== */

longlong * FUN_1800086f0(longlong param_1,int *param_2,int param_3)

{
  uint uVar1;
  longlong *plVar2;
  longlong lVar3;
  ulonglong uVar4;
  
  plVar2 = (longlong *)FUN_1800012e0(param_2,param_3 * 8);
  if (plVar2 != (longlong *)0x0) {
    lVar3 = FUN_1800012e0(param_2,param_3 << 5);
    *plVar2 = lVar3;
    if (lVar3 != 0) {
      uVar4 = 0;
      if (0 < param_3) {
        do {
          plVar2[uVar4] = (longlong)((int)uVar4 << 5) + *plVar2;
          uVar1 = (int)uVar4 + 1;
          uVar4 = (ulonglong)uVar1;
        } while ((int)uVar1 < param_3);
      }
      FUN_1800072c0(*(undefined8 **)(param_1 + 0x118),(longlong)plVar2,param_3);
    }
  }
  return plVar2;
}



/* ========================================================================
   ENTRY: 1800087a0
   NAME : FUN_1800087a0
   SIG  : double __fastcall FUN_1800087a0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

double FUN_1800087a0(void)

{
  DWORD DVar1;
  
  DVar1 = timeGetTime();
  return (double)DVar1 * _DAT_180023cb0;
}



/* ========================================================================
   ENTRY: 1800087e0
   NAME : FUN_1800087e0
   SIG  : ulonglong __fastcall FUN_1800087e0(longlong param_1, char * param_2, undefined8 param_3, longlong param_4, longlong param_5)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_1800087e0(longlong param_1,char *param_2,undefined8 param_3,longlong param_4,
                       longlong param_5)

{
  undefined1 auVar1 [16];
  double dVar2;
  int iVar3;
  undefined8 uVar4;
  longlong lVar5;
  char *pcVar6;
  uint uVar7;
  int iVar8;
  ulonglong uVar10;
  ulonglong unaff_RSI;
  double dVar11;
  double dVar12;
  undefined1 auStack_1068 [32];
  undefined4 local_1048 [43];
  undefined4 local_f9c;
  undefined4 local_f98;
  undefined4 local_f94;
  int local_f90;
  int local_f8c;
  undefined4 local_f88;
  ulonglong local_48;
  ulonglong uVar9;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStack_1068;
  uVar10 = 0;
  *(undefined8 *)(param_5 + 0x58) = 0;
  uVar4 = FUN_180008ba0(param_1,param_2,local_1048,*(undefined8 *)(param_1 + 0x120));
  if ((int)uVar4 != 0) {
    return unaff_RSI;
  }
  *(undefined4 *)(param_4 + 0x14) = local_f9c;
  *(undefined4 *)(param_4 + 0x18) = local_f98;
  *(undefined8 *)(param_4 + 0x40) = 0;
  uVar9 = uVar10;
  do {
    uVar4 = FUN_180006eb0((int)*(undefined8 *)(&DAT_18002b2d0 + uVar9 * 8));
    if (((int)uVar4 != -0x3e3) && ((int)uVar4 != -1000)) {
      *(undefined8 *)(param_4 + 0x40) = *(undefined8 *)(&DAT_18002b2d0 + uVar9 * 8);
      dVar12 = (double)local_f8c / *(double *)(param_4 + 0x40);
      dVar11 = (double)local_f90 / *(double *)(param_4 + 0x40);
      dVar2 = dVar12;
      if (dVar12 <= dVar11) {
        dVar2 = dVar11;
      }
      goto LAB_1800088a7;
    }
    uVar7 = (int)uVar9 + 1;
    uVar9 = (ulonglong)uVar7;
  } while ((int)uVar7 < 0xd);
  dVar12 = 0.0;
  dVar2 = 0.0;
LAB_1800088a7:
  *(double *)(param_4 + 0x20) = dVar12;
  *(double *)(param_4 + 0x28) = dVar12;
  auVar1._8_4_ = SUB84(dVar2,0);
  auVar1._0_8_ = dVar2;
  auVar1._12_4_ = (int)((ulonglong)dVar2 >> 0x20);
  *(undefined1 (*) [16])(param_4 + 0x30) = auVar1;
  *(undefined4 *)(param_5 + 0x48) = local_f94;
  *(int *)(param_5 + 0x4c) = local_f90;
  *(undefined4 *)(param_5 + 0x54) = local_f88;
  *(int *)(param_5 + 0x50) = local_f8c;
  lVar5 = FUN_1800012e0(*(int **)(param_1 + 0x108),
                        (*(int *)(param_4 + 0x18) + *(int *)(param_4 + 0x14)) * 0x34);
  *(longlong *)(param_5 + 0x58) = lVar5;
  if (lVar5 != 0) {
    uVar9 = uVar10;
    if (0 < *(int *)(param_4 + 0x14)) {
      do {
        iVar8 = (int)uVar9;
        lVar5 = (longlong)iVar8 * 0x34;
        *(int *)(lVar5 + *(longlong *)(param_5 + 0x58)) = iVar8;
        *(undefined4 *)(*(longlong *)(param_5 + 0x58) + 4 + lVar5) = 1;
        uVar4 = FUN_180006ff0(lVar5 + *(longlong *)(param_5 + 0x58));
        iVar3 = (int)uVar4;
        if (iVar3 != 0) goto LAB_1800089eb;
        uVar9 = (ulonglong)(iVar8 + 1U);
      } while ((int)(iVar8 + 1U) < *(int *)(param_4 + 0x14));
    }
    if (0 < *(int *)(param_4 + 0x18)) {
      do {
        iVar8 = (int)uVar10;
        lVar5 = (longlong)(*(int *)(param_4 + 0x14) + iVar8) * 0x34;
        *(int *)(lVar5 + *(longlong *)(param_5 + 0x58)) = iVar8;
        *(undefined4 *)(*(longlong *)(param_5 + 0x58) + 4 + lVar5) = 0;
        uVar4 = FUN_180006ff0(lVar5 + *(longlong *)(param_5 + 0x58));
        iVar3 = (int)uVar4;
        if (iVar3 != 0) goto LAB_1800089eb;
        uVar10 = (ulonglong)(iVar8 + 1U);
      } while ((int)(iVar8 + 1U) < *(int *)(param_4 + 0x18));
    }
    thunk_FUN_180006f70();
    return unaff_RSI;
  }
LAB_180008a08:
  thunk_FUN_180006f70();
  if (*(longlong *)(param_5 + 0x58) != 0) {
    FUN_180001360(*(longlong *)(param_1 + 0x108),*(longlong *)(param_5 + 0x58));
    *(undefined8 *)(param_5 + 0x58) = 0;
  }
  return unaff_RSI;
LAB_1800089eb:
  pcVar6 = FUN_180009ad0(iVar3);
  FUN_180003c90(3,iVar3,pcVar6);
  goto LAB_180008a08;
}



/* ========================================================================
   ENTRY: 180008a30
   NAME : FUN_180008a30
   SIG  : ulonglong __fastcall FUN_180008a30(longlong param_1, int * param_2, int * param_3, undefined4 param_4)
   ======================================================================== */

ulonglong FUN_180008a30(longlong param_1,int *param_2,int *param_3,undefined4 param_4)

{
  int iVar1;
  ulonglong uVar2;
  undefined8 uVar3;
  uint uVar4;
  int iVar5;
  int iVar6;
  int local_res8;
  
  if (param_2 == (int *)0x0) {
    iVar5 = 0;
LAB_180008a80:
    if (param_3 == (int *)0x0) {
      iVar6 = 0;
    }
    else {
      if ((param_3[2] & 0x10000U) != 0) goto LAB_180008a8f;
      local_res8 = *param_3;
      if (local_res8 == -2) {
        return 0xffffd8f4;
      }
      iVar6 = param_3[1];
    }
    iVar1 = *(int *)(param_1 + 0x128);
    if ((iVar1 == -1) || (iVar1 == local_res8)) {
      if ((iVar1 != -1) ||
         (uVar2 = FUN_180008ba0(param_1,*(char **)(*(longlong *)
                                                    (*(longlong *)(param_1 + 0x28) +
                                                    (longlong)local_res8 * 8) + 8),
                                (undefined4 *)(param_1 + 300),*(undefined8 *)(param_1 + 0x120)),
         (int)uVar2 == 0)) {
        uVar4 = 0;
        if ((iVar5 < 1) || (iVar5 <= *(int *)(param_1 + 0x1d8))) {
          if ((iVar6 == 0) || (iVar6 <= *(int *)(param_1 + 0x1dc))) {
            uVar3 = FUN_180006eb0(param_4);
            if (((int)uVar3 == -0x3e3) || ((int)uVar3 == -1000)) {
              uVar4 = 0xffffd8f3;
            }
          }
          else {
            uVar4 = 0xffffd8f2;
          }
        }
        else {
          uVar4 = 0xffffd8f2;
        }
        if (*(int *)(param_1 + 0x128) == -1) {
          thunk_FUN_180006f70();
        }
        uVar2 = (ulonglong)uVar4;
        if (uVar4 == 0) {
          uVar2 = 0;
        }
      }
    }
    else {
      uVar2 = 0xffffd8ff;
    }
  }
  else {
    if ((param_3 != (int *)0x0) && (*param_2 != *param_3)) {
      return 0xffffd8f7;
    }
    if ((param_2[2] & 0x10000U) == 0) {
      local_res8 = *param_2;
      if (local_res8 == -2) {
        return 0xffffd8f4;
      }
      iVar5 = param_2[1];
      goto LAB_180008a80;
    }
LAB_180008a8f:
    uVar2 = 0xffffd8f6;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180008ba0
   NAME : FUN_180008ba0
   SIG  : undefined8 __fastcall FUN_180008ba0(longlong param_1, char * param_2, undefined4 * param_3, undefined8 param_4)
   ======================================================================== */

undefined8 FUN_180008ba0(longlong param_1,char *param_2,undefined4 *param_3,undefined8 param_4)

{
  int iVar1;
  ulonglong uVar2;
  undefined8 uVar3;
  char *pcVar4;
  undefined4 uVar5;
  
  uVar2 = FUN_180007340(*(undefined8 **)(param_1 + 0x118),param_2);
  if ((char)uVar2 == '\0') {
    FUN_180003c90(3,0,"Failed to load ASIO driver");
    return 0xffffd8f1;
  }
  memset(param_3 + 1,0,0xa0);
  *param_3 = 2;
  *(undefined8 *)(param_3 + 0x29) = param_4;
  uVar3 = FUN_1800070e0((longlong)param_3);
  iVar1 = (int)uVar3;
  if (iVar1 != 0) {
    pcVar4 = FUN_180009ad0(iVar1);
    FUN_180003c90(3,iVar1,pcVar4);
    return 0xffffd8f1;
  }
  uVar3 = FUN_180007030(param_3 + 0x2b,param_3 + 0x2c);
  iVar1 = (int)uVar3;
  if (iVar1 == 0) {
    uVar3 = FUN_180006fa0(param_3 + 0x2d,param_3 + 0x2e,param_3 + 0x2f,param_3 + 0x30);
    iVar1 = (int)uVar3;
    if (iVar1 == 0) {
      uVar3 = FUN_1800071e0();
      *(bool *)(param_3 + 0x31) = (int)uVar3 == 0;
      return 0;
    }
  }
  uVar5 = 3;
  pcVar4 = FUN_180009ad0(iVar1);
  FUN_180003c90(uVar5,iVar1,pcVar4);
  FUN_180006f70();
  return 0xffffd8f1;
}



/* ========================================================================
   ENTRY: 180008cd0
   NAME : FUN_180008cd0
   SIG  : int __fastcall FUN_180008cd0(int param_1)
   ======================================================================== */

int FUN_180008cd0(int param_1)

{
  uint uVar1;
  
  uVar1 = param_1 - 1U >> 1 | param_1 - 1U;
  uVar1 = uVar1 >> 2 | uVar1;
  uVar1 = uVar1 >> 4 | uVar1;
  uVar1 = uVar1 >> 8 | uVar1;
  return (uVar1 >> 0x10 | uVar1) + 1;
}



/* ========================================================================
   ENTRY: 180008d00
   NAME : FUN_180008d00
   SIG  : ulonglong __fastcall FUN_180008d00(longlong param_1, undefined8 * param_2, int * param_3, int * param_4, double param_5, uint param_6, uint param_7, undefined * param_8, undefined8 * param_9)
   ======================================================================== */

ulonglong FUN_180008d00(longlong param_1,undefined8 *param_2,int *param_3,int *param_4,
                       double param_5,uint param_6,uint param_7,undefined *param_8,
                       undefined8 *param_9)

{
  longlong *plVar1;
  longlong *plVar2;
  bool bVar3;
  undefined8 *puVar4;
  DWORD DVar5;
  uint uVar6;
  uint extraout_EAX;
  uint extraout_EAX_00;
  int iVar7;
  uint extraout_EAX_01;
  ulonglong uVar8;
  ulonglong uVar9;
  undefined4 *puVar10;
  HANDLE pvVar11;
  longlong lVar12;
  undefined8 uVar13;
  char *pcVar14;
  longlong lVar15;
  int iVar16;
  uint uVar17;
  undefined *puVar18;
  undefined4 uVar19;
  undefined *puVar20;
  uint uVar21;
  int iVar22;
  uint uVar23;
  uint uVar24;
  double dVar25;
  uint local_94;
  uint local_90;
  uint local_8c;
  int local_88;
  undefined8 local_80;
  undefined8 *local_78;
  int local_70;
  int local_6c;
  int local_68;
  int local_64;
  undefined8 local_60;
  ulonglong local_58;
  undefined8 local_50;
  
  puVar18 = (undefined *)0x0;
  local_78 = (undefined8 *)0x0;
  uVar23 = 0;
  local_80 = (undefined *)0x0;
  if (*(int *)(param_1 + 0x128) != -1) {
    return 0xffffd8ff;
  }
  dVar25 = DAT_180023560;
  if (param_3 == (int *)0x0) {
    local_90 = 0;
    local_58 = local_58 & 0xffffffff00000000;
    puVar20 = puVar18;
    local_88 = (int)param_8;
  }
  else {
    if ((param_4 != (int *)0x0) && (*param_3 != *param_4)) {
      return 0xffffd8f7;
    }
    local_88 = *param_3;
    if (local_88 == -2) {
      return 0xffffd8f4;
    }
    uVar8 = FUN_18000a790((longlong)param_3,*(int **)(param_3 + 6),
                          *(int *)(*(longlong *)
                                    (*(longlong *)(param_1 + 0x28) + (longlong)local_88 * 8) + 0x14)
                          ,(longlong *)&local_78);
    if ((int)uVar8 != 0) {
      return uVar8;
    }
    local_90 = param_3[2];
    local_58 = (ulonglong)(param_5 * *(double *)(param_3 + 4) + dVar25);
    puVar20 = (undefined *)(ulonglong)(uint)param_3[1];
  }
  puVar4 = local_78;
  if (param_4 == (int *)0x0) {
    local_94 = 0;
    local_50 = (ulonglong)local_50._4_4_ << 0x20;
  }
  else {
    local_88 = *param_4;
    if (local_88 == -2) {
      return 0xffffd8f4;
    }
    uVar8 = FUN_18000a790((longlong)param_4,*(int **)(param_4 + 6),
                          *(int *)(*(longlong *)
                                    (*(longlong *)(param_1 + 0x28) + (longlong)local_88 * 8) + 0x18)
                          ,&local_80);
    if ((int)uVar8 != 0) {
      return uVar8;
    }
    local_94 = param_4[2];
    uVar23 = param_4[1];
    local_50 = (longlong)(param_5 * *(double *)(param_4 + 4) + dVar25);
    puVar18 = local_80;
  }
  local_60 = (undefined4 *)(param_1 + 300);
  uVar8 = FUN_180008ba0(param_1,*(char **)(*(longlong *)
                                            (*(longlong *)(param_1 + 0x28) + (longlong)local_88 * 8)
                                          + 8),local_60,*(undefined8 *)(param_1 + 0x120));
  if ((int)uVar8 != 0) {
    return uVar8 & 0xffffffff;
  }
  uVar24 = (uint)puVar20;
  if (((0 < (int)uVar24) && (*(int *)(param_1 + 0x1d8) < (int)uVar24)) ||
     ((uVar23 != 0 && (*(int *)(param_1 + 0x1dc) < (int)uVar23)))) {
    uVar8 = 0xffffd8f2;
    goto LAB_180009a9a;
  }
  uVar9 = FUN_18000a720(param_5);
  uVar8 = uVar9 & 0xffffffff;
  if ((int)uVar9 != 0) goto LAB_180009a9a;
  if ((param_7 & 0xffff0000) != 0) {
    return 0xffffd8f5;
  }
  puVar10 = (undefined4 *)FUN_180020230(0x228);
  if (puVar10 == (undefined4 *)0x0) {
    uVar8 = 0xffffd8f8;
    goto LAB_180009a9a;
  }
  local_64 = 0;
  local_6c = 0;
  bVar3 = false;
  local_68 = 0;
  local_70 = 0;
  *(undefined8 *)(puVar10 + 0x88) = 0;
  pvVar11 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
  *(HANDLE *)(puVar10 + 0x7e) = pvVar11;
  plVar1 = (longlong *)(puVar10 + 0x60);
  plVar2 = (longlong *)(puVar10 + 0x62);
  if (pvVar11 == (HANDLE)0x0) {
    uVar21 = 0;
LAB_180008fb0:
    uVar8 = 0xffffd8f1;
    DVar5 = GetLastError();
    FUN_180009b70(DVar5);
  }
  else {
    local_80 = param_8;
    *plVar1 = 0;
    *plVar2 = 0;
    *(undefined8 *)(puVar10 + 0x6a) = 0;
    if (param_8 == (undefined *)0x0) {
      param_8 = FUN_180008290;
      local_78 = &DAT_18002bae8;
      lVar12 = 0xa8;
    }
    else {
      local_78 = param_9;
      lVar12 = 0x48;
    }
    FUN_180006e70(puVar10,param_1 + lVar12,param_8,local_78);
    FUN_180003950((double *)(puVar10 + 0x14),param_5);
    lVar12 = FUN_180020230((uVar24 + uVar23) * 0x18);
    *plVar1 = lVar12;
    if (lVar12 == 0) {
LAB_180009968:
      uVar8 = 0xffffd8f8;
    }
    else {
      uVar9 = 0;
      uVar21 = 0;
      uVar8 = uVar9;
      if (0 < (int)uVar24) {
        do {
          lVar12 = *plVar1;
          *(undefined4 *)(lVar12 + uVar8 * 0x18) = 1;
          iVar7 = (int)uVar8;
          if (puVar4 != (undefined8 *)0x0) {
            iVar7 = *(int *)((longlong)puVar4 + uVar8 * 4);
          }
          *(int *)(lVar12 + 4 + uVar8 * 0x18) = iVar7;
          uVar6 = (int)uVar8 + 1;
          *(undefined8 *)(lVar12 + 0x10 + uVar8 * 0x18) = 0;
          *(undefined8 *)(lVar12 + 8 + uVar8 * 0x18) = 0;
          uVar8 = (ulonglong)uVar6;
        } while ((int)uVar6 < (int)uVar24);
      }
      if (0 < (int)uVar23) {
        do {
          iVar7 = (int)uVar9;
          lVar15 = (longlong)(int)(iVar7 + uVar24);
          lVar12 = *plVar1;
          *(undefined4 *)(lVar12 + lVar15 * 0x18) = 0;
          iVar22 = iVar7;
          if (puVar18 != (undefined *)0x0) {
            iVar22 = *(int *)(puVar18 + uVar9 * 4);
          }
          *(int *)(lVar12 + 4 + lVar15 * 0x18) = iVar22;
          uVar9 = (ulonglong)(iVar7 + 1U);
          *(undefined8 *)(lVar12 + 0x10 + lVar15 * 0x18) = 0;
          *(undefined8 *)(lVar12 + 8 + lVar15 * 0x18) = 0;
        } while ((int)(iVar7 + 1U) < (int)uVar23);
      }
      if (local_80 != (undefined *)0x0) {
        uVar21 = (uint)local_50;
        if ((uint)local_50 < (uint)local_58) {
          uVar21 = (uint)local_58;
        }
      }
      local_8c = FUN_180009ef0(uVar21,param_6,(longlong)local_60);
      iVar22 = uVar24 + uVar23;
      uVar13 = FUN_180006ef0(*plVar1,iVar22,local_8c,&PTR_FUN_18002b2b0);
      iVar7 = (int)uVar13;
      if ((iVar7 == 0) ||
         ((uVar21 = *(uint *)(param_1 + 0x1e8), local_8c != uVar21 &&
          (uVar13 = FUN_180006ef0(*plVar1,iVar22,uVar21,&PTR_FUN_18002b2b0), local_8c = uVar21,
          (int)uVar13 == 0)))) {
        uVar21 = 1;
        local_64 = 1;
        lVar12 = FUN_180020230(iVar22 * 0x34);
        *plVar2 = lVar12;
        if (lVar12 == 0) {
          uVar8 = 0xffffd8f8;
        }
        else {
          uVar8 = 0;
          if (0 < iVar22) {
            do {
              lVar12 = uVar8 * 0x34;
              *(undefined4 *)(lVar12 + *plVar2) = *(undefined4 *)(*plVar1 + 4 + uVar8 * 0x18);
              *(undefined4 *)(*plVar2 + 4 + lVar12) = *(undefined4 *)(*plVar1 + uVar8 * 0x18);
              uVar13 = FUN_180006ff0(*plVar2 + lVar12);
              iVar7 = (int)uVar13;
              if (iVar7 != 0) {
                uVar8 = 0xffffd8f1;
                pcVar14 = FUN_180009ad0(iVar7);
                FUN_180003c90(3,iVar7,pcVar14);
                goto LAB_180009977;
              }
              uVar6 = (int)uVar8 + 1;
              uVar8 = (ulonglong)uVar6;
            } while ((int)uVar6 < iVar22);
          }
          lVar12 = FUN_180020230(iVar22 * 0x10);
          *(longlong *)(puVar10 + 0x6a) = lVar12;
          if (lVar12 == 0) {
            uVar8 = 0xffffd8f8;
          }
          else {
            if ((int)uVar24 < 1) {
              *(undefined8 *)(puVar10 + 0x6c) = 0;
              *(undefined8 *)(puVar10 + 0x6e) = 0;
            }
            else {
              *(longlong *)(puVar10 + 0x6c) = lVar12;
              *(longlong *)(puVar10 + 0x6e) = lVar12 + (longlong)(int)uVar24 * 8;
              lVar12 = 0;
              do {
                lVar15 = lVar12 + 1;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x6c) + -8 + lVar15 * 8) =
                     *(undefined8 *)(*plVar1 + 8 + lVar12 * 0x18);
                *(undefined8 *)(*(longlong *)(puVar10 + 0x6e) + -8 + lVar15 * 8) =
                     *(undefined8 *)(*plVar1 + 0x10 + lVar12 * 0x18);
                uVar6 = (int)puVar20 - 1;
                puVar20 = (undefined *)(ulonglong)uVar6;
                lVar12 = lVar15;
              } while (uVar6 != 0);
            }
            if ((int)uVar23 < 1) {
              *(undefined8 *)(puVar10 + 0x70) = 0;
              *(undefined8 *)(puVar10 + 0x72) = 0;
            }
            else {
              *(longlong *)(puVar10 + 0x70) =
                   *(longlong *)(puVar10 + 0x6a) + (longlong)(int)(uVar24 * 2) * 8;
              *(longlong *)(puVar10 + 0x72) =
                   *(longlong *)(puVar10 + 0x6a) + (longlong)(int)(uVar24 * 2 + uVar23) * 8;
              uVar8 = 0;
              do {
                lVar12 = (longlong)(int)((int)uVar8 + uVar24);
                uVar6 = (int)uVar8 + 1;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x70) + uVar8 * 8) =
                     *(undefined8 *)(*plVar1 + 8 + lVar12 * 0x18);
                *(undefined8 *)(*(longlong *)(puVar10 + 0x72) + uVar8 * 8) =
                     *(undefined8 *)(*plVar1 + 0x10 + lVar12 * 0x18);
                uVar8 = (ulonglong)uVar6;
              } while ((int)uVar6 < (int)uVar23);
            }
            uVar6 = 0;
            if ((int)uVar24 < 1) {
              *(undefined8 *)(puVar10 + 0x74) = 0;
            }
            else {
              uVar19 = *(undefined4 *)(*plVar2 + 0x10);
              uVar13 = FUN_180008210(uVar19);
              uVar6 = (uint)uVar13;
              FUN_180009d90(uVar19,(undefined8 *)(puVar10 + 0x74),puVar10 + 0x76);
            }
            uVar17 = 0;
            if ((int)uVar23 < 1) {
              *(undefined8 *)(puVar10 + 0x78) = 0;
            }
            else {
              uVar19 = *(undefined4 *)(*plVar2 + 0x10 + (longlong)(int)uVar24 * 0x34);
              uVar13 = FUN_180008210(uVar19);
              uVar17 = (uint)uVar13;
              FUN_18000a060(uVar19,(undefined8 *)(puVar10 + 0x78),puVar10 + 0x7a);
            }
            FUN_180007060(puVar10 + 100,puVar10 + 0x65);
            if (local_80 == (undefined *)0x0) {
              lVar12 = FUN_180020230(0x1a0);
              *(longlong *)(puVar10 + 0x88) = lVar12;
              if (lVar12 == 0) {
                uVar8 = 0xffffd8f8;
              }
              else {
                if (param_6 == 0) {
                  param_6 = local_8c;
                }
                *(undefined8 *)(lVar12 + 0x20) = 0;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x88) + 0x18) = 0;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x88) + 0x30) = 0;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x88) + 0x28) = 0;
                local_80 = (undefined *)(CONCAT44(local_80._4_4_,local_90) & 0xffffffff7fffffff);
                *(undefined8 *)(*(longlong *)(puVar10 + 0x88) + 0x88) = 0;
                *(undefined8 *)(*(longlong *)(puVar10 + 0x88) + 0x80) = 0;
                **(undefined4 **)(puVar10 + 0x88) = 1;
                local_60 = (undefined4 *)(CONCAT44(local_60._4_4_,local_94) & 0xffffffff7fffffff);
                puVar10 = (undefined4 *)
                          FUN_180006290(puVar10 + 0x1a,uVar24,local_90 & 0x7fffffff,
                                        uVar6 | 0x80000000,uVar23,local_94 & 0x7fffffff,
                                        uVar17 | 0x80000000,param_5,param_7,param_6,local_8c,0,
                                        (longlong)param_8,local_78);
                uVar8 = (ulonglong)extraout_EAX;
                if (extraout_EAX == 0) {
                  local_68 = 1;
                  puVar10 = (undefined4 *)
                            FUN_180006290((uint *)(*(longlong *)(puVar10 + 0x88) + 0x90),uVar24,
                                          local_90,(uint)local_80,uVar23,local_94,(uint)local_60,
                                          param_5,3,param_6,param_6,1,0,0);
                  uVar8 = (ulonglong)extraout_EAX_00;
                  if (extraout_EAX_00 == 0) {
                    local_70 = 1;
                    if (uVar24 == 0) {
LAB_1800096dd:
                      if (uVar23 == 0) {
LAB_1800098f5:
                        *(double *)(puVar10 + 0x12) = param_5;
                        puVar10[0x5e] = local_8c;
                        *(longlong *)(puVar10 + 0x5c) = param_1;
                        puVar10[0x66] = uVar24;
                        puVar10[0x67] = uVar23;
                        *(undefined1 *)(puVar10 + 0x68) = *(undefined1 *)(param_1 + 0x1f0);
                        puVar10[0x81] = 1;
                        puVar10[0x82] = 0;
                        *(int *)(param_1 + 0x128) = local_88;
                        DAT_18002bae8 = puVar10;
                        *param_2 = puVar10;
                        return 0;
                      }
                      lVar12 = *(longlong *)(puVar10 + 0x88);
                      pvVar11 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
                      *(HANDLE *)(lVar12 + 0x18) = pvVar11;
                      if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x18) == 0) {
                        uVar8 = 0xffffd8f1;
                        DVar5 = GetLastError();
                        FUN_180009b70(DVar5);
                        goto LAB_18000996d;
                      }
                      lVar12 = *(longlong *)(puVar10 + 0x88);
                      uVar21 = 1;
                      local_6c = 1;
                      uVar13 = FUN_180020230(uVar23 * 8);
                      *(undefined8 *)(lVar12 + 0x80) = uVar13;
                      if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x80) != 0) {
                        iVar22 = (int)local_50 - puVar10[0x65];
                        iVar7 = 1;
                        if (0 < iVar22) {
                          iVar7 = iVar22;
                        }
                        iVar7 = ((iVar7 + -1 + param_6) / param_6 + 1) * param_6;
                        *(uint *)(*(longlong *)(puVar10 + 0x88) + 0x78) = iVar7 - param_6;
                        do {
                          uVar21 = uVar21 * 2;
                        } while ((int)uVar21 < iVar7);
                        iVar7 = FUN_180006280(*(longlong *)(puVar10 + 0x88) + 0x90);
                        iVar22 = FUN_180006280((longlong)(puVar10 + 0x1a));
                        *(double *)(puVar10 + 0x10) =
                             (double)(puVar10[0x65] +
                                     (uVar21 / param_6 - 1) * param_6 + iVar7 + iVar22) / param_5;
                        uVar13 = Pa_GetSampleSize(local_94);
                        lVar12 = *(longlong *)(puVar10 + 0x88);
                        uVar6 = (int)uVar13 * uVar23;
                        uVar13 = FUN_180020230(uVar6 * uVar21);
                        *(undefined8 *)(lVar12 + 0x28) = uVar13;
                        lVar12 = *(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x28);
                        if (lVar12 != 0) {
                          FUN_180006c50((uint *)(*(longlong *)(puVar10 + 0x88) + 0x38),uVar6,uVar21,
                                        lVar12);
                          goto LAB_1800098f5;
                        }
                        goto LAB_180009968;
                      }
                      uVar8 = 0xffffd8f8;
                    }
                    else {
                      lVar12 = *(longlong *)(puVar10 + 0x88);
                      pvVar11 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
                      *(HANDLE *)(lVar12 + 0x20) = pvVar11;
                      lVar12 = *(longlong *)(puVar10 + 0x88);
                      if (*(longlong *)(lVar12 + 0x20) == 0) goto LAB_180008fb0;
                      bVar3 = true;
                      uVar13 = FUN_180020230(uVar24 * 8);
                      *(undefined8 *)(lVar12 + 0x88) = uVar13;
                      if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x88) == 0) {
                        uVar8 = 0xffffd8f8;
                      }
                      else {
                        iVar7 = puVar10[100];
                        iVar16 = (int)local_58 - iVar7;
                        iVar22 = 1;
                        if (0 < iVar16) {
                          iVar22 = iVar16;
                        }
                        do {
                          uVar21 = uVar21 * 2;
                        } while ((int)uVar21 <
                                 (int)(((iVar22 + -1 + param_6) / param_6 + 1) * param_6));
                        iVar22 = FUN_180006270(*(longlong *)(puVar10 + 0x88) + 0x90);
                        iVar16 = FUN_180006270((longlong)(puVar10 + 0x1a));
                        *(double *)(puVar10 + 0xe) =
                             (double)((uVar21 / param_6 - 1) * param_6 + iVar7 + iVar22 + iVar16) /
                             param_5;
                        uVar13 = Pa_GetSampleSize(local_90);
                        lVar12 = *(longlong *)(puVar10 + 0x88);
                        uVar6 = (int)uVar13 * uVar24;
                        uVar13 = FUN_180020230(uVar6 * uVar21);
                        *(undefined8 *)(lVar12 + 0x30) = uVar13;
                        lVar12 = *(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x30);
                        if (lVar12 != 0) {
                          FUN_180006c50((uint *)(*(longlong *)(puVar10 + 0x88) + 0x58),uVar6,uVar21,
                                        lVar12);
                          bVar3 = true;
                          goto LAB_1800096dd;
                        }
                        uVar21 = 1;
                        uVar8 = 0xffffd8f8;
                        bVar3 = true;
                      }
                    }
                  }
                }
              }
            }
            else {
              puVar10 = (undefined4 *)
                        FUN_180006290(puVar10 + 0x1a,uVar24,local_90,uVar6 | 0x80000000,uVar23,
                                      local_94,uVar17 | 0x80000000,param_5,param_7,param_6,local_8c,
                                      0,(longlong)param_8,local_78);
              uVar8 = (ulonglong)extraout_EAX_01;
              if (extraout_EAX_01 == 0) {
                iVar7 = puVar10[100];
                iVar16 = FUN_180006270((longlong)(puVar10 + 0x1a));
                iVar22 = puVar10[0x65];
                *(double *)(puVar10 + 0xe) = (double)(uint)(iVar16 + iVar7) / param_5;
                iVar7 = FUN_180006280((longlong)(puVar10 + 0x1a));
                *(double *)(puVar10 + 0x10) = (double)(uint)(iVar7 + iVar22) / param_5;
                goto LAB_1800098f5;
              }
            }
          }
        }
        goto LAB_180009977;
      }
      uVar8 = 0xffffd8f1;
      pcVar14 = FUN_180009ad0(iVar7);
      FUN_180003c90(3,iVar7,pcVar14);
    }
LAB_18000996d:
    uVar21 = 1;
  }
LAB_180009977:
  if (*(longlong *)(puVar10 + 0x88) != 0) {
    if (local_70 != 0) {
      FUN_1800069c0(*(longlong *)(puVar10 + 0x88) + 0x90);
    }
    if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x28) != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x28));
    }
    if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x80) != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x80));
    }
    if (local_6c != 0) {
      CloseHandle(*(HANDLE *)(*(longlong *)(puVar10 + 0x88) + 0x18));
    }
    if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x30) != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x30));
    }
    if (*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x88) != 0) {
      FUN_180020240(*(longlong *)(*(longlong *)(puVar10 + 0x88) + 0x88));
    }
    if (bVar3) {
      CloseHandle(*(HANDLE *)(*(longlong *)(puVar10 + 0x88) + 0x20));
    }
    FUN_180020240(*(longlong *)(puVar10 + 0x88));
  }
  if (local_68 != 0) {
    FUN_1800069c0((longlong)(puVar10 + 0x1a));
  }
  if (uVar21 != 0) {
    CloseHandle(*(HANDLE *)(puVar10 + 0x7e));
  }
  if (*plVar1 != 0) {
    FUN_180020240(*plVar1);
  }
  if (*plVar2 != 0) {
    FUN_180020240(*plVar2);
  }
  if (*(longlong *)(puVar10 + 0x6a) != 0) {
    FUN_180020240(*(longlong *)(puVar10 + 0x6a));
  }
  FUN_180020240((longlong)puVar10);
  if (local_64 != 0) {
    FUN_180006f50();
  }
LAB_180009a9a:
  thunk_FUN_180006f70();
  return uVar8;
}



/* ========================================================================
   ENTRY: 180009ad0
   NAME : FUN_180009ad0
   SIG  : char * __fastcall FUN_180009ad0(int param_1)
   ======================================================================== */

char * FUN_180009ad0(int param_1)

{
  if (param_1 < 1) {
    if (param_1 == 0) {
LAB_180009b3f:
      return "Success";
    }
    switch(param_1) {
    case -1000:
      return "Hardware input or output is not present or available";
    case -999:
      return "Hardware is malfunctioning";
    case -0x3e6:
      return "Input parameter invalid";
    case -0x3e5:
      return "Hardware is in a bad mode or used in a bad mode";
    case -0x3e4:
      return "Hardware is not running when sample position is inquired";
    case -0x3e3:
      return "Sample clock or rate cannot be determined or is not present";
    case -0x3e2:
      return "Not enough memory for completing the request";
    }
  }
  else if (param_1 == 0x3f4847a0) goto LAB_180009b3f;
  return "Unknown ASIO error";
}



/* ========================================================================
   ENTRY: 180009b70
   NAME : FUN_180009b70
   SIG  : undefined __fastcall FUN_180009b70(DWORD param_1)
   ======================================================================== */

void FUN_180009b70(DWORD param_1)

{
  FUN_1800202f0(3,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180009b80
   NAME : FUN_180009b80
   SIG  : undefined8 __fastcall FUN_180009b80(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

undefined8 FUN_180009b80(longlong param_1,longlong param_2,uint param_3)

{
  double dVar1;
  int *piVar2;
  longlong lVar3;
  DWORD DVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  ulonglong uVar8;
  int local_res8 [2];
  longlong local_res10;
  uint local_res18 [2];
  longlong local_res20;
  longlong local_58 [3];
  
  piVar2 = *(int **)(param_1 + 0x220);
  uVar8 = 0;
  local_res20 = 0;
  local_58[0] = 0;
  local_res18[0] = 0;
  local_res8[0] = 0;
  if (*piVar2 == 0) {
    uVar6 = *(uint *)(param_1 + 0x68);
    dVar1 = *(double *)(param_1 + 0x48);
    if (*(int *)(param_1 + 0x208) != 0) {
      if (*(int *)(param_1 + 0x198) == 0) {
        return 0xffffd909;
      }
      uVar5 = uVar6 * 8000;
      local_res10 = param_2;
      if ((piVar2[0x37] == 0) && (lVar3 = *(longlong *)(piVar2 + 0x22), piVar2[0x2b] != 0)) {
        do {
          uVar7 = (int)uVar8 + 1;
          *(undefined8 *)(lVar3 + uVar8 * 8) = *(undefined8 *)(param_2 + uVar8 * 8);
          uVar8 = (ulonglong)uVar7;
        } while (uVar7 < (uint)piVar2[0x2b]);
      }
      do {
        uVar7 = param_3;
        if (uVar6 < param_3) {
          uVar7 = uVar6;
        }
        uVar6 = uVar7;
        uVar7 = FUN_180006b00((longlong)(piVar2 + 0x16));
        if ((int)uVar7 < (int)uVar6) {
          piVar2[2] = uVar6;
          piVar2[4] = 1;
          DVar4 = WaitForSingleObject(*(HANDLE *)(piVar2 + 8),
                                      (DWORD)(longlong)((double)uVar5 / dVar1));
          if (DVar4 == 0xffffffff) {
            DVar4 = GetLastError();
            FUN_180009b70(DVar4);
            return 0xffffd8f1;
          }
          if (DVar4 == 0x102) {
            if (*piVar2 != 0) {
              return 0xffffd901;
            }
            return 0xffffd8fd;
          }
        }
        FUN_180006b10(piVar2 + 0x16,uVar6,&local_res20,local_res18,local_58,local_res8);
        FUN_180006860((longlong)(piVar2 + 0x24),local_res18[0]);
        FUN_180006880((longlong)(piVar2 + 0x24),0,local_res20,0);
        if (local_res8[0] != 0) {
          FUN_180006750((longlong)(piVar2 + 0x24),local_res8[0]);
          FUN_180006760((longlong)(piVar2 + 0x24),0,local_58[0],0);
        }
        uVar7 = FUN_180005d50((longlong)(piVar2 + 0x24),&local_res10,uVar6);
        FUN_180006ad0((longlong)(piVar2 + 0x16),uVar7);
        param_3 = param_3 - uVar7;
        if (param_3 == 0) {
          if (piVar2[0x67] != 0) {
            piVar2[0x67] = 0;
            return 0xffffd903;
          }
          return 0;
        }
      } while( true );
    }
  }
  return 0xffffd901;
}



/* ========================================================================
   ENTRY: 180009d90
   NAME : FUN_180009d90
   SIG  : undefined __fastcall FUN_180009d90(undefined4 param_1, undefined8 * param_2, undefined4 * param_3)
   ======================================================================== */

void FUN_180009d90(undefined4 param_1,undefined8 *param_2,undefined4 *param_3)

{
  *param_3 = 0;
  *param_2 = 0;
  switch(param_1) {
  case 0:
    *param_2 = &LAB_18000a550;
    return;
  case 1:
    *param_2 = &LAB_18000a580;
    break;
  case 2:
  case 3:
    *param_2 = &LAB_18000a5b0;
    return;
  case 4:
    *param_2 = &LAB_18000a5e0;
    return;
  case 8:
    *param_2 = &LAB_18000a660;
    *param_3 = 0x10;
    return;
  case 9:
    *param_2 = &LAB_18000a660;
    *param_3 = 0xe;
    return;
  case 10:
    *param_2 = &LAB_18000a660;
    *param_3 = 0xc;
    return;
  case 0xb:
    *param_2 = &LAB_18000a660;
    *param_3 = 8;
    return;
  case 0x14:
    *param_2 = &LAB_180008670;
    return;
  case 0x18:
    *param_2 = &LAB_18000a1c0;
    *param_3 = 0x10;
    return;
  case 0x19:
    *param_2 = &LAB_18000a1c0;
    *param_3 = 0xe;
    return;
  case 0x1a:
    *param_2 = &LAB_18000a1c0;
    *param_3 = 0xc;
    return;
  case 0x1b:
    *param_2 = &LAB_18000a1c0;
    *param_3 = 8;
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180009ef0
   NAME : FUN_180009ef0
   SIG  : undefined __fastcall FUN_180009ef0(uint param_1, uint param_2, longlong param_3)
   ======================================================================== */

void FUN_180009ef0(uint param_1,uint param_2,longlong param_3)

{
  uint uVar1;
  
  if (param_2 != 0) {
    uVar1 = FUN_180009f20(param_1,param_2,param_3);
    if (uVar1 != 0) {
      return;
    }
  }
  FUN_180009fe0(param_1,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180009f20
   NAME : FUN_180009f20
   SIG  : uint __fastcall FUN_180009f20(uint param_1, uint param_2, longlong param_3)
   ======================================================================== */

uint FUN_180009f20(uint param_1,uint param_2,longlong param_3)

{
  int iVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  
  iVar1 = *(int *)(param_3 + 0xc0);
  uVar4 = 0;
  if (iVar1 == 0) {
    uVar3 = *(uint *)(param_3 + 0xbc);
    if (uVar3 % param_2 != 0) {
      return 0;
    }
  }
  else {
    uVar3 = *(uint *)(param_3 + 0xb4);
    if (iVar1 == -1) {
      do {
        if ((uVar3 % param_2 == 0) && (param_1 <= uVar3)) {
          return uVar3;
        }
        uVar2 = uVar3;
        if (uVar3 % param_2 != 0) {
          uVar2 = uVar4;
        }
        uVar4 = uVar2;
        uVar3 = uVar3 * 2;
      } while (uVar3 <= *(uint *)(param_3 + 0xb8));
      return uVar4;
    }
    while ((uVar3 % param_2 != 0 || (uVar3 < param_1))) {
      uVar2 = uVar3;
      if (uVar3 % param_2 != 0) {
        uVar2 = uVar4;
      }
      uVar4 = uVar2;
      uVar3 = uVar3 + iVar1;
      if (*(uint *)(param_3 + 0xb8) < uVar3) {
        return uVar4;
      }
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180009fe0
   NAME : FUN_180009fe0
   SIG  : ulonglong __fastcall FUN_180009fe0(uint param_1, longlong param_2)
   ======================================================================== */

ulonglong FUN_180009fe0(uint param_1,longlong param_2)

{
  uint uVar1;
  uint uVar2;
  uint uVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  
  uVar3 = *(uint *)(param_2 + 0xb4);
  uVar4 = (ulonglong)uVar3;
  if (uVar3 < param_1) {
    uVar1 = *(uint *)(param_2 + 0xb8);
    uVar5 = (ulonglong)uVar1;
    if (uVar1 <= param_1) {
LAB_18000a02a:
      return uVar5 & 0xffffffff;
    }
    uVar2 = *(uint *)(param_2 + 0xc0);
    if (uVar2 == 0) {
      return (ulonglong)*(uint *)(param_2 + 0xbc);
    }
    if (uVar2 == 0xffffffff) {
      uVar1 = FUN_180008cd0(param_1);
      if (uVar3 <= uVar1) {
        uVar3 = uVar1;
      }
      uVar4 = (ulonglong)uVar3;
      if ((uint)uVar5 < uVar3) goto LAB_18000a02a;
    }
    else {
      uVar2 = (((param_1 - 1) + uVar2) / uVar2) * uVar2;
      if (uVar3 <= uVar2) {
        uVar3 = uVar2;
      }
      uVar4 = (ulonglong)uVar3;
      if (uVar1 < uVar3) {
        uVar4 = uVar5;
      }
    }
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000a060
   NAME : FUN_18000a060
   SIG  : undefined __fastcall FUN_18000a060(undefined4 param_1, undefined8 * param_2, undefined4 * param_3)
   ======================================================================== */

void FUN_18000a060(undefined4 param_1,undefined8 *param_2,undefined4 *param_3)

{
  *param_3 = 0;
  *param_2 = 0;
  switch(param_1) {
  case 0:
    *param_2 = &LAB_18000a550;
    return;
  case 1:
    *param_2 = &LAB_18000a580;
    break;
  case 2:
  case 3:
    *param_2 = &LAB_18000a5b0;
    return;
  case 4:
    *param_2 = &LAB_1800085e0;
    return;
  case 8:
    *param_2 = &LAB_18000a200;
    *param_3 = 0x10;
    return;
  case 9:
    *param_2 = &LAB_18000a200;
    *param_3 = 0xe;
    return;
  case 10:
    *param_2 = &LAB_18000a200;
    *param_3 = 0xc;
    return;
  case 0xb:
    *param_2 = &LAB_18000a200;
    *param_3 = 8;
    return;
  case 0x14:
    *param_2 = &LAB_1800085a0;
    return;
  case 0x18:
    *param_2 = &LAB_18000a1e0;
    *param_3 = 0x10;
    return;
  case 0x19:
    *param_2 = &LAB_18000a1e0;
    *param_3 = 0xe;
    return;
  case 0x1a:
    *param_2 = &LAB_18000a1e0;
    *param_3 = 0xc;
    return;
  case 0x1b:
    *param_2 = &LAB_18000a1e0;
    *param_3 = 8;
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a230
   NAME : FUN_18000a230
   SIG  : int __fastcall FUN_18000a230(longlong param_1)
   ======================================================================== */

int FUN_18000a230(longlong param_1)

{
  undefined4 *puVar1;
  BOOL BVar2;
  DWORD DVar3;
  undefined8 uVar4;
  char *pcVar5;
  int iVar6;
  int iVar7;
  
  iVar6 = 0;
  puVar1 = *(undefined4 **)(param_1 + 0x220);
  if (0 < *(int *)(param_1 + 0x19c)) {
    FUN_18000aa10(param_1,0);
    FUN_18000aa10(param_1,1);
  }
  FUN_1800066c0(param_1 + 0x68);
  *(undefined1 *)(param_1 + 0x1ec) = 0;
  *(undefined1 *)(param_1 + 0x20c) = 0;
  *(undefined4 *)(param_1 + 0x210) = 0xffffffff;
  *(undefined4 *)(param_1 + 0x214) = 0;
  *(undefined4 *)(param_1 + 0x218) = 0;
  BVar2 = ResetEvent(*(HANDLE *)(param_1 + 0x1f8));
  if (BVar2 == 0) {
    iVar6 = -9999;
    DVar3 = GetLastError();
    FUN_180009b70(DVar3);
  }
  if (puVar1 != (undefined4 *)0x0) {
    FUN_1800066c0((longlong)(puVar1 + 0x24));
    if (*(int *)(param_1 + 0x198) != 0) {
      BVar2 = ResetEvent(*(HANDLE *)(puVar1 + 8));
      if (BVar2 == 0) {
        iVar6 = -9999;
        DVar3 = GetLastError();
        FUN_180009b70(DVar3);
      }
      FUN_180006af0((longlong)(puVar1 + 0x16));
      (**(code **)(puVar1 + 0x32))(*(undefined8 *)(puVar1 + 0x1c),1,puVar1[0x2b] * puVar1[0x16]);
    }
    if (*(int *)(param_1 + 0x19c) != 0) {
      BVar2 = ResetEvent(*(HANDLE *)(puVar1 + 6));
      if (BVar2 == 0) {
        iVar6 = -9999;
        DVar3 = GetLastError();
        FUN_180009b70(DVar3);
      }
      FUN_180006af0((longlong)(puVar1 + 0xe));
      (**(code **)(puVar1 + 0x3a))(*(undefined8 *)(puVar1 + 0x14),1,puVar1[0x34] * puVar1[0xe]);
      FUN_180006ae0((longlong)(puVar1 + 0xe),puVar1[0x1e]);
    }
    *(undefined8 *)(puVar1 + 1) = 0;
    *(undefined8 *)(puVar1 + 3) = 0;
    *(undefined8 *)(puVar1 + 0x66) = 0;
    *puVar1 = 0;
  }
  if (iVar6 == 0) {
    *(undefined4 *)(param_1 + 0x204) = 0;
    *(undefined4 *)(param_1 + 0x208) = 1;
    *(undefined1 *)(param_1 + 0x200) = 0;
    uVar4 = FUN_180007220();
    iVar7 = (int)uVar4;
    iVar6 = 0;
    if (iVar7 != 0) {
      *(undefined4 *)(param_1 + 0x204) = 1;
      *(undefined4 *)(param_1 + 0x208) = 0;
      pcVar5 = FUN_180009ad0(iVar7);
      FUN_180003c90(3,iVar7,pcVar5);
      return -9999;
    }
  }
  return iVar6;
}



/* ========================================================================
   ENTRY: 18000a420
   NAME : FUN_18000a420
   SIG  : undefined4 __fastcall FUN_18000a420(longlong param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined4 FUN_18000a420(longlong param_1)

{
  undefined4 *puVar1;
  DWORD DVar2;
  int iVar3;
  undefined8 uVar4;
  char *pcVar5;
  undefined4 uVar6;
  double dVar7;
  
  uVar6 = 0;
  puVar1 = *(undefined4 **)(param_1 + 0x220);
  if (*(int *)(param_1 + 0x208) != 0) {
    if ((puVar1 != (undefined4 *)0x0) && (uVar6 = 0, *(int *)(param_1 + 0x19c) != 0)) {
      puVar1[1] = puVar1[0xe];
      puVar1[3] = 1;
      *puVar1 = 1;
      DVar2 = WaitForSingleObject(*(HANDLE *)(puVar1 + 6),
                                  (DWORD)(longlong)
                                         ((double)(puVar1[0xe] * 2000) / *(double *)(param_1 + 0x48)
                                         ));
      if (DVar2 == 0xffffffff) {
        uVar6 = 0xffffd8f1;
        DVar2 = GetLastError();
        FUN_180009b70(DVar2);
      }
      else {
        uVar6 = 0;
        if (DVar2 == 0x102) {
          uVar6 = 0xffffd8fd;
        }
      }
    }
    dVar7 = *(double *)(param_1 + 0x40) * DAT_1800239c8;
    *(undefined1 *)(param_1 + 0x1ec) = 1;
    WaitForSingleObject(*(HANDLE *)(param_1 + 0x1f8),(DWORD)(longlong)(dVar7 * _DAT_180023cb8));
  }
  uVar4 = FUN_180007240();
  iVar3 = (int)uVar4;
  if (iVar3 == 0) {
    FUN_1800086a0(param_1);
  }
  else {
    uVar6 = 0xffffd8f1;
    pcVar5 = FUN_180009ad0(iVar3);
    FUN_180003c90(3,iVar3,pcVar5);
  }
  *(undefined4 *)(param_1 + 0x204) = 1;
  *(undefined4 *)(param_1 + 0x208) = 0;
  if ((*(char *)(param_1 + 0x200) == '\0') && (*(code **)(param_1 + 0x20) != (code *)0x0)) {
    (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
  }
  return uVar6;
}



/* ========================================================================
   ENTRY: 18000a690
   NAME : FUN_18000a690
   SIG  : undefined __fastcall FUN_18000a690(longlong param_1)
   ======================================================================== */

void FUN_18000a690(longlong param_1)

{
  undefined8 *_Memory;
  
  if (*(longlong *)(param_1 + 0x108) != 0) {
    FUN_180001280(*(longlong *)(param_1 + 0x108));
    FUN_180001230(*(longlong *)(param_1 + 0x108));
  }
  _Memory = *(undefined8 **)(param_1 + 0x118);
  if (_Memory != (undefined8 *)0x0) {
    thunk_FUN_180007580(_Memory);
    free(_Memory);
  }
  DAT_18002bad8 = 0;
  FUN_180020200(3,(int *)(param_1 + 0x110));
  FUN_180020240(param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000a710
   NAME : thunk_FUN_180006f70
   SIG  : undefined __fastcall thunk_FUN_180006f70(void)
   ======================================================================== */

void thunk_FUN_180006f70(void)

{
  if (DAT_18002bad0 != 0) {
    FUN_180007450(DAT_18002bad8);
  }
  DAT_18002bad0 = 0;
  return;
}



/* ========================================================================
   ENTRY: 18000a720
   NAME : FUN_18000a720
   SIG  : undefined8 __fastcall FUN_18000a720(double param_1)
   ======================================================================== */

undefined8 FUN_18000a720(double param_1)

{
  undefined8 uVar1;
  undefined8 uVar2;
  double local_res10 [3];
  
  uVar1 = FUN_180006eb0(SUB84(param_1,0));
  if (((int)uVar1 == 0) && (uVar1 = FUN_1800070c0(local_res10), (int)uVar1 == 0)) {
    if (local_res10[0] != param_1) {
      uVar1 = FUN_180007200(SUB84(param_1,0));
      uVar2 = 0;
      if ((int)uVar1 != 0) {
        uVar2 = 0xffffd8f3;
      }
      return uVar2;
    }
    return uVar1;
  }
  return 0xffffd8f3;
}



/* ========================================================================
   ENTRY: 18000a790
   NAME : FUN_18000a790
   SIG  : undefined8 __fastcall FUN_18000a790(longlong param_1, int * param_2, int param_3, longlong * param_4)
   ======================================================================== */

undefined8 FUN_18000a790(longlong param_1,int *param_2,int param_3,longlong *param_4)

{
  int iVar1;
  longlong lVar2;
  uint uVar3;
  ulonglong uVar4;
  
  if (param_2 != (int *)0x0) {
    if ((*param_2 != 0x18) || (param_2[2] != 1)) {
      return 0xffffd900;
    }
    if ((*(byte *)(param_2 + 3) & 1) != 0) {
      lVar2 = *(longlong *)(param_2 + 4);
      *param_4 = lVar2;
      if (lVar2 == 0) {
        return 0xffffd900;
      }
      uVar4 = 0;
      if (0 < *(int *)(param_1 + 4)) {
        do {
          iVar1 = *(int *)(lVar2 + uVar4 * 4);
          if ((iVar1 < 0) || (param_3 <= iVar1)) {
            return 0xffffd8f2;
          }
          uVar3 = (int)uVar4 + 1;
          uVar4 = (ulonglong)uVar3;
        } while ((int)uVar3 < *(int *)(param_1 + 4));
      }
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000a7f0
   NAME : FUN_18000a7f0
   SIG  : undefined8 __fastcall FUN_18000a7f0(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

undefined8 FUN_18000a7f0(longlong param_1,longlong param_2,uint param_3)

{
  double dVar1;
  int *piVar2;
  int iVar3;
  DWORD DVar4;
  uint uVar5;
  uint uVar6;
  ulonglong uVar7;
  uint uVar8;
  int local_res8 [2];
  int local_res18 [2];
  longlong local_res20;
  longlong local_48;
  longlong local_40;
  
  piVar2 = *(int **)(param_1 + 0x220);
  local_48 = 0;
  local_40 = 0;
  local_res18[0] = 0;
  local_res8[0] = 0;
  if (*piVar2 == 0) {
    uVar8 = *(uint *)(param_1 + 0x68);
    dVar1 = *(double *)(param_1 + 0x48);
    if (*(int *)(param_1 + 0x208) != 0) {
      if (*(int *)(param_1 + 0x19c) == 0) {
        return 0xffffd90a;
      }
      uVar5 = uVar8 * 8000;
      local_res20 = param_2;
      if ((piVar2[0x37] == 0) &&
         (local_res20 = *(longlong *)(piVar2 + 0x20), uVar7 = 0, piVar2[0x34] != 0)) {
        do {
          uVar6 = (int)uVar7 + 1;
          *(undefined8 *)(local_res20 + uVar7 * 8) = *(undefined8 *)(param_2 + uVar7 * 8);
          uVar7 = (ulonglong)uVar6;
        } while (uVar6 < (uint)piVar2[0x34]);
      }
      do {
        uVar6 = param_3;
        if (uVar8 < param_3) {
          uVar6 = uVar8;
        }
        uVar8 = uVar6;
        iVar3 = FUN_180006ba0(piVar2 + 0xe);
        if (iVar3 < (int)uVar8) {
          piVar2[1] = uVar8;
          piVar2[3] = 1;
          DVar4 = WaitForSingleObject(*(HANDLE *)(piVar2 + 6),
                                      (DWORD)(longlong)((double)uVar5 / dVar1));
          if (DVar4 == 0xffffffff) {
            DVar4 = GetLastError();
            FUN_180009b70(DVar4);
            return 0xffffd8f1;
          }
          if (DVar4 == 0x102) {
            if (*piVar2 != 0) {
              return 0xffffd901;
            }
            return 0xffffd8fd;
          }
        }
        FUN_180006bc0(piVar2 + 0xe,uVar8,&local_48,local_res18,&local_40,local_res8);
        FUN_1800069a0((longlong)(piVar2 + 0x24),local_res18[0]);
        FUN_1800068d0((longlong)(piVar2 + 0x24),0,local_48,0);
        if (local_res8[0] != 0) {
          FUN_180006830((longlong)(piVar2 + 0x24),local_res8[0]);
          FUN_1800067b0((longlong)(piVar2 + 0x24),0,local_40,0);
        }
        uVar6 = FUN_180005eb0((longlong)(piVar2 + 0x24),&local_res20,uVar8);
        FUN_180006ae0((longlong)(piVar2 + 0xe),uVar6);
        param_3 = param_3 - uVar6;
        if (param_3 == 0) {
          if (piVar2[0x66] != 0) {
            piVar2[0x66] = 0;
            return 0xffffd904;
          }
          return 0;
        }
      } while( true );
    }
  }
  return 0xffffd901;
}



/* ========================================================================
   ENTRY: 18000aa10
   NAME : FUN_18000aa10
   SIG  : undefined __fastcall FUN_18000aa10(longlong param_1, int param_2)
   ======================================================================== */

void FUN_18000aa10(longlong param_1,int param_2)

{
  undefined8 uVar1;
  int iVar2;
  longlong lVar3;
  
  iVar2 = 0;
  if (0 < *(int *)(param_1 + 0x19c)) {
    do {
      lVar3 = (longlong)(*(int *)(param_1 + 0x198) + iVar2);
      uVar1 = FUN_180008400(*(undefined4 *)(*(longlong *)(param_1 + 0x188) + 0x10 + lVar3 * 0x34));
      memset(*(void **)(*(longlong *)(param_1 + 0x180) + 8 + (longlong)param_2 * 8 + lVar3 * 0x18),0
             ,(ulonglong)(uint)((int)uVar1 * *(int *)(param_1 + 0x178)));
      iVar2 = iVar2 + 1;
    } while (iVar2 < *(int *)(param_1 + 0x19c));
  }
  return;
}



/* ========================================================================
   ENTRY: 18000aaa0
   NAME : PaAsio_GetAvailableBufferSizes
   SIG  : undefined __fastcall PaAsio_GetAvailableBufferSizes(int param_1, undefined4 * param_2, undefined4 * param_3, undefined4 * param_4, undefined4 * param_5)
   ======================================================================== */

void PaAsio_GetAvailableBufferSizes
               (int param_1,undefined4 *param_2,undefined4 *param_3,undefined4 *param_4,
               undefined4 *param_5)

{
  longlong lVar1;
  undefined8 uVar2;
  int local_38 [2];
  int *local_30 [2];
  
                    /* 0xaaa0  50  PaAsio_GetAvailableBufferSizes */
  uVar2 = FUN_180003c40((longlong *)local_30,3);
  if ((int)uVar2 == 0) {
    uVar2 = FUN_180003c20(local_38,param_1,local_30[0]);
    if ((int)uVar2 == 0) {
      lVar1 = *(longlong *)(*(longlong *)(local_30[0] + 10) + (longlong)local_38[0] * 8);
      *param_2 = *(undefined4 *)(lVar1 + 0x48);
      *param_3 = *(undefined4 *)(lVar1 + 0x4c);
      *param_4 = *(undefined4 *)(lVar1 + 0x50);
      *param_5 = *(undefined4 *)(lVar1 + 0x54);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ab20
   NAME : PaAsio_GetInputChannelName
   SIG  : undefined8 __fastcall PaAsio_GetInputChannelName(int param_1, int param_2, longlong * param_3)
   ======================================================================== */

undefined8 PaAsio_GetInputChannelName(int param_1,int param_2,longlong *param_3)

{
  longlong lVar1;
  undefined8 uVar2;
  int local_res20 [2];
  int *local_18 [2];
  
                    /* 0xab20  53  PaAsio_GetInputChannelName */
  uVar2 = FUN_180003c40((longlong *)local_18,3);
  if ((int)uVar2 == 0) {
    uVar2 = FUN_180003c20(local_res20,param_1,local_18[0]);
    if ((int)uVar2 == 0) {
      if ((-1 < param_2) &&
         (lVar1 = *(longlong *)(*(longlong *)(local_18[0] + 10) + (longlong)local_res20[0] * 8),
         param_2 < *(int *)(lVar1 + 0x14))) {
        *param_3 = (longlong)param_2 * 0x34 + 0x14 + *(longlong *)(lVar1 + 0x58);
        return 0;
      }
      uVar2 = 0xffffd8f2;
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000abc0
   NAME : PaAsio_GetOutputChannelName
   SIG  : undefined8 __fastcall PaAsio_GetOutputChannelName(int param_1, int param_2, longlong * param_3)
   ======================================================================== */

undefined8 PaAsio_GetOutputChannelName(int param_1,int param_2,longlong *param_3)

{
  longlong lVar1;
  undefined8 uVar2;
  int local_res20 [2];
  int *local_18 [2];
  
                    /* 0xabc0  54  PaAsio_GetOutputChannelName */
  uVar2 = FUN_180003c40((longlong *)local_18,3);
  if ((int)uVar2 == 0) {
    uVar2 = FUN_180003c20(local_res20,param_1,local_18[0]);
    if ((int)uVar2 == 0) {
      if ((-1 < param_2) &&
         (lVar1 = *(longlong *)(*(longlong *)(local_18[0] + 10) + (longlong)local_res20[0] * 8),
         param_2 < *(int *)(lVar1 + 0x18))) {
        *param_3 = (longlong)(*(int *)(lVar1 + 0x14) + param_2) * 0x34 +
                   *(longlong *)(lVar1 + 0x58) + 0x14;
        return 0;
      }
      uVar2 = 0xffffd8f2;
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000ac60
   NAME : FUN_18000ac60
   SIG  : ulonglong __fastcall FUN_18000ac60(longlong * param_1, undefined4 param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18000ac60(longlong *param_1,undefined4 param_2)

{
  longlong *plVar1;
  longlong lVar2;
  char *_Str1;
  undefined8 *_Memory;
  int iVar3;
  int iVar4;
  int extraout_EAX;
  longlong lVar5;
  ulonglong uVar6;
  undefined4 *puVar7;
  longlong *plVar8;
  HWND pHVar9;
  longlong lVar10;
  HMODULE hModule;
  INT_PTR IVar11;
  longlong *plVar12;
  ulonglong uVar13;
  int iVar14;
  longlong *plVar15;
  undefined1 auStackY_f8 [32];
  longlong local_80;
  longlong *local_78;
  longlong *local_70;
  int *local_68;
  longlong *local_60;
  char local_58 [32];
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_f8;
  lVar5 = FUN_180020230(0x1f8);
  if (lVar5 == 0) {
    return 0xffffd8f8;
  }
  local_68 = (int *)(lVar5 + 0x110);
  local_80 = lVar5;
  uVar6 = FUN_180020180(3,local_68);
  uVar13 = uVar6 & 0xffffffff;
  plVar12 = (longlong *)(lVar5 + 0x108);
  plVar1 = (longlong *)(lVar5 + 0x118);
  plVar15 = (longlong *)0x0;
  local_78 = plVar1;
  local_70 = plVar12;
  if ((int)uVar6 == 0) {
    *plVar1 = 0;
    puVar7 = FUN_1800011b0();
    *plVar12 = (longlong)puVar7;
    if (puVar7 != (undefined4 *)0x0) {
      local_60 = (longlong *)FUN_180021630(0x18);
      plVar8 = plVar15;
      if (local_60 != (longlong *)0x0) {
        plVar8 = FUN_180007260(local_60);
      }
      *plVar1 = (longlong)plVar8;
      if (*plVar1 != 0) {
        DAT_18002bad8 = *plVar1;
        *(undefined8 *)(lVar5 + 0x120) = 0;
        *(undefined4 *)(lVar5 + 0x128) = 0xffffffff;
        *param_1 = lVar5;
        *(undefined4 *)(lVar5 + 8) = 1;
        *(undefined4 *)(*param_1 + 0xc) = 3;
        *(undefined **)(*param_1 + 0x10) = &DAT_180023bc4;
        *(undefined4 *)(*param_1 + 0x18) = 0;
        pHVar9 = GetDesktopWindow();
        *(HWND *)(lVar5 + 0x120) = pHVar9;
        iVar3 = FUN_1800076a0(*plVar1);
        if (iVar3 < 1) {
LAB_18000b035:
          lVar2 = *param_1;
          if (*(int *)(lVar2 + 0x18) < 1) {
            *(undefined4 *)(lVar2 + 0x1c) = 0xffffffff;
            *(undefined4 *)(*param_1 + 0x20) = 0xffffffff;
          }
          else {
            *(undefined4 *)(lVar2 + 0x1c) = 0;
            *(undefined4 *)(*param_1 + 0x20) = 0;
          }
          *(code **)(*param_1 + 0x30) = FUN_18000a690;
          *(code **)(*param_1 + 0x38) = FUN_180008d00;
          *(code **)(*param_1 + 0x40) = FUN_180008a30;
          FUN_180006e10((undefined8 *)(lVar5 + 0x48),FUN_180008470,FUN_18000a230,FUN_18000a420,
                        FUN_180008180,&LAB_180008b90,&LAB_180008b80,FUN_1800087a0,&LAB_180008780,
                        &LAB_180006df0,&LAB_180006e00,&LAB_180006df0,&LAB_180006e00);
          FUN_180006e10((undefined8 *)(lVar5 + 0xa8),FUN_180008470,FUN_18000a230,FUN_18000a420,
                        FUN_180008180,&LAB_180008b90,&LAB_180008b80,FUN_1800087a0,&LAB_180006de0,
                        FUN_180009b80,FUN_18000a7f0,&LAB_180008790,&LAB_1800087d0);
          return 0;
        }
        plVar8 = FUN_1800086f0(lVar5,(int *)*plVar12,iVar3);
        if (plVar8 != (longlong *)0x0) {
          lVar2 = *param_1;
          lVar10 = FUN_1800012e0((int *)*plVar12,iVar3 * 8);
          *(longlong *)(lVar2 + 0x28) = lVar10;
          if ((*(longlong *)(*param_1 + 0x28) != 0) &&
             (local_78 = (longlong *)FUN_1800012e0((int *)*plVar12,iVar3 * 0x60),
             local_78 != (longlong *)0x0)) {
            hModule = LoadLibraryA("Kernel32.dll");
            DAT_18002bae0 = GetProcAddress(hModule,"IsDebuggerPresent");
            local_58[0] = '\0';
            local_58[1] = '\0';
            local_58[2] = '\0';
            local_58[3] = '\0';
            local_58[4] = '\0';
            local_58[5] = '\0';
            local_58[6] = '\0';
            local_58[7] = '\0';
            local_58[8] = '\0';
            local_58[9] = '\0';
            local_58[10] = '\0';
            local_58[0xb] = '\0';
            local_58[0xc] = '\0';
            local_58[0xd] = '\0';
            local_58[0xe] = '\0';
            local_58[0xf] = '\0';
            local_58[0x10] = '\0';
            local_58[0x11] = '\0';
            local_58[0x12] = '\0';
            local_58[0x13] = '\0';
            local_58[0x14] = '\0';
            local_58[0x15] = '\0';
            local_58[0x16] = '\0';
            local_58[0x17] = '\0';
            local_58[0x18] = '\0';
            local_58[0x19] = '\0';
            local_58[0x1a] = '\0';
            local_58[0x1b] = '\0';
            local_58[0x1c] = '\0';
            local_58[0x1d] = '\0';
            local_58[0x1e] = '\0';
            local_58[0x1f] = '\0';
            local_80 = CONCAT44(local_80._4_4_,0x20);
            RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOdrivername"
                         ,0x10002,(LPDWORD)0x0,local_58,(LPDWORD)&local_80);
            if (local_58[0] == '\0') {
              RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64",
                           "ASIOdrivername",0x10002,(LPDWORD)0x0,local_58,(LPDWORD)&local_80);
            }
            do {
              iVar14 = (int)plVar15;
              _Str1 = (char *)plVar8[iVar14];
              iVar4 = strcmp(_Str1,"ASIO DirectX Full Duplex Driver");
              if (((((iVar4 != 0) && (iVar4 = strcmp(_Str1,"ASIO Multimedia Driver"), iVar4 != 0))
                   && (iVar4 = strncmp(_Str1,"Premiere",8), iVar4 != 0)) &&
                  ((iVar4 = strncmp(_Str1,"Adobe",5), iVar4 != 0 &&
                   (iVar4 = strcmp(_Str1,local_58), iVar4 != 0)))) &&
                 ((iVar4 = strcmp(_Str1,"ASIO Avid Driver"), iVar4 != 0 &&
                  (((DAT_18002bae0 == (FARPROC)0x0 ||
                    (IVar11 = (*DAT_18002bae0)(), (int)IVar11 == 0)) ||
                   (iVar4 = strcmp((char *)plVar8[iVar14],"ASIO Digidesign Driver"), iVar4 != 0)))))
                 ) {
                plVar12 = local_78 + (longlong)*(int *)(*param_1 + 0x18) * 0xc;
                *(undefined4 *)plVar12 = 2;
                *(undefined4 *)(plVar12 + 2) = param_2;
                plVar12[1] = plVar8[iVar14];
                uVar6 = FUN_1800087e0(lVar5,(char *)plVar8[iVar14],plVar15,(longlong)plVar12,
                                      (longlong)plVar12);
                if (extraout_EAX == 0) {
                  *(ulonglong *)
                   (*(longlong *)(*param_1 + 0x28) + (longlong)*(int *)(*param_1 + 0x18) * 8) =
                       uVar6;
                  *(int *)(*param_1 + 0x18) = *(int *)(*param_1 + 0x18) + 1;
                }
              }
              plVar15 = (longlong *)(ulonglong)(iVar14 + 1U);
            } while ((int)(iVar14 + 1U) < iVar3);
            goto LAB_18000b035;
          }
        }
      }
    }
    uVar13 = 0xffffd8f8;
  }
  if (*plVar12 != 0) {
    FUN_180001280(*plVar12);
    FUN_180001230(*plVar12);
  }
  _Memory = (undefined8 *)*plVar1;
  if (_Memory != (undefined8 *)0x0) {
    thunk_FUN_180007580(_Memory);
    free(_Memory);
  }
  DAT_18002bad8 = 0;
  FUN_180020200(3,local_68);
  FUN_180020240(lVar5);
  return uVar13;
}



/* ========================================================================
   ENTRY: 18000b210
   NAME : PaAsio_ShowControlPanel
   SIG  : ulonglong __fastcall PaAsio_ShowControlPanel(int param_1, undefined8 param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong PaAsio_ShowControlPanel(int param_1,undefined8 param_2)

{
  int iVar1;
  ulonglong uVar2;
  char *pcVar3;
  undefined8 uVar4;
  ulonglong uVar5;
  undefined1 auStack_108 [32];
  int local_e8 [2];
  int *local_e0;
  int local_d8 [4];
  undefined4 local_c8;
  undefined1 local_c4 [160];
  undefined8 local_24;
  ulonglong local_18;
  
                    /* 0xb210  51  PaAsio_ShowControlPanel */
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_108;
  uVar2 = FUN_180020180(3,local_d8);
  if ((int)uVar2 != 0) {
    return uVar2;
  }
  uVar2 = FUN_180003c40((longlong *)&local_e0,3);
  uVar5 = uVar2 & 0xffffffff;
  if ((int)uVar2 == 0) {
    uVar2 = FUN_180003c20(local_e8,param_1,local_e0);
    uVar5 = uVar2 & 0xffffffff;
    if ((int)uVar2 == 0) {
      if (local_e0[0x4a] == -1) {
        uVar2 = FUN_180007340(*(undefined8 **)(local_e0 + 0x46),
                              *(char **)(*(longlong *)
                                          (*(longlong *)(local_e0 + 10) + (longlong)local_e8[0] * 8)
                                        + 8));
        if ((char)uVar2 == '\0') {
          uVar5 = 0xffffd8f1;
        }
        else {
          memset(local_c4,0,0xa0);
          local_c8 = 2;
          local_24 = param_2;
          uVar2 = FUN_1800070e0((longlong)&local_c8);
          if ((int)uVar2 == 0) {
            uVar4 = FUN_180006ed0();
            iVar1 = (int)uVar4;
            if (iVar1 != 0) {
              uVar5 = 0xffffd8f1;
              pcVar3 = FUN_180009ad0(iVar1);
              FUN_180003c90(3,iVar1,pcVar3);
              FUN_180006f70();
              goto LAB_18000b28e;
            }
            uVar2 = FUN_180006f70();
            if ((int)uVar2 == 0) {
              return uVar2;
            }
          }
          uVar5 = 0xffffd8f1;
          iVar1 = (int)uVar2;
          pcVar3 = FUN_180009ad0(iVar1);
          FUN_180003c90(3,iVar1,pcVar3);
        }
      }
      else {
        uVar5 = 0xffffd8ff;
      }
    }
  }
LAB_18000b28e:
  FUN_180020200(3,local_d8);
  return uVar5;
}



/* ========================================================================
   ENTRY: 18000b3a0
   NAME : FUN_18000b3a0
   SIG  : undefined8 __fastcall FUN_18000b3a0(longlong param_1, undefined8 param_2, undefined4 * param_3, LPCWSTR param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18000b3a0(longlong param_1,undefined8 param_2,undefined4 *param_3,LPCWSTR param_4)

{
  longlong lVar1;
  double dVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined1 auStack_68 [32];
  longlong *local_48;
  ulonglong local_40;
  undefined8 uStack_38;
  ulonglong local_30;
  
  local_30 = DAT_18002b580 ^ (ulonglong)auStack_68;
  lVar1 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*(int *)(param_1 + 0x18) * 8);
  if (param_3 == (undefined4 *)0x0) {
    *(undefined8 *)(lVar1 + 0x58) = 0;
  }
  else {
    *(undefined8 **)(lVar1 + 0x58) = (undefined8 *)(lVar1 + 0x48);
    uVar5 = *(undefined8 *)(param_3 + 2);
    *(undefined8 *)(lVar1 + 0x48) = *(undefined8 *)param_3;
    *(undefined8 *)(lVar1 + 0x50) = uVar5;
  }
  iVar3 = (*DAT_18002bb18)(param_3,&local_48,0);
  if (iVar3 != 0) {
    return 0;
  }
  uStack_38 = 0;
  local_40 = 0x10;
  iVar3 = (**(code **)(*local_48 + 0x20))(local_48,&local_40);
  if ((iVar3 != 0) || ((local_40 & 0x2000000000) != 0)) {
    (**(code **)(*local_48 + 0x10))();
    return 0;
  }
  *(int *)(lVar1 + 0x14) = uStack_38._4_4_;
  *(undefined2 *)(lVar1 + 0x78) = 0x101;
  *(undefined4 *)(lVar1 + 0x18) = 0;
  if ((param_4 != (LPCWSTR)0x0) && (uVar4 = FUN_180020a10(param_4,1), 0 < (int)uVar4)) {
    *(uint *)(lVar1 + 0x14) = uVar4;
    *(undefined1 *)(lVar1 + 0x78) = 1;
  }
  if (uStack_38._4_4_ == 2) {
    if (((uint)uStack_38 >> 0xb & 1) != 0) {
LAB_18000b489:
      uVar5 = 0x40e5888000000000;
      goto LAB_18000b527;
    }
    if (((uint)uStack_38 >> 0xf & 1) == 0) {
      if ((char)uStack_38 < '\0') {
LAB_18000b4aa:
        uVar5 = 0x40d5888000000000;
        goto LAB_18000b527;
      }
      if ((uStack_38 & 8) != 0) {
LAB_18000b4c2:
        uVar5 = 0x40c5888000000000;
        goto LAB_18000b527;
      }
      uVar4 = (uint)uStack_38 & 0x80000;
LAB_18000b4db:
      if (uVar4 != 0) {
        uVar5 = 0x40f7700000000000;
        goto LAB_18000b527;
      }
    }
  }
  else if (uStack_38._4_4_ == 1) {
    if (((uint)uStack_38 >> 10 & 1) != 0) goto LAB_18000b489;
    if (((uint)uStack_38 >> 0xe & 1) == 0) {
      if ((uStack_38 & 0x40) == 0) {
        if ((uStack_38 & 4) == 0) {
          uVar4 = (uint)uStack_38 & 0x40000;
          goto LAB_18000b4db;
        }
        goto LAB_18000b4c2;
      }
      goto LAB_18000b4aa;
    }
  }
  uVar5 = 0x40e7700000000000;
LAB_18000b527:
  *(undefined8 *)(lVar1 + 0x40) = uVar5;
  dVar2 = FUN_18000cc80();
  *(double *)(lVar1 + 0x20) = dVar2;
  *(undefined8 *)(lVar1 + 0x28) = 0;
  *(undefined8 *)(lVar1 + 0x38) = 0;
  *(double *)(lVar1 + 0x30) = dVar2 + dVar2;
  (**(code **)(*local_48 + 0x10))();
  *(undefined8 *)(lVar1 + 8) = param_2;
  if (param_3 == (undefined4 *)0x0) {
    *(undefined4 *)(param_1 + 0x1c) = *(undefined4 *)(param_1 + 0x18);
  }
  *(int *)(param_1 + 0x18) = *(int *)(param_1 + 0x18) + 1;
  return 0;
}



/* ========================================================================
   ENTRY: 18000b590
   NAME : FUN_18000b590
   SIG  : undefined8 __fastcall FUN_18000b590(longlong param_1, undefined8 param_2, longlong * param_3, LPCWSTR param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000b590(longlong param_1,undefined8 param_2,longlong *param_3,LPCWSTR param_4)

{
  double dVar1;
  longlong lVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  undefined1 auStack_d8 [32];
  longlong *local_b8;
  undefined1 local_b0 [8];
  undefined4 local_a8;
  undefined8 local_a4;
  ulonglong uStack_9c;
  undefined8 local_94;
  undefined8 uStack_8c;
  undefined8 local_84;
  undefined8 uStack_7c;
  undefined8 local_74;
  undefined8 uStack_6c;
  undefined8 local_64;
  undefined4 uStack_5c;
  undefined4 uStack_58;
  undefined4 uStack_54;
  undefined8 uStack_50;
  ulonglong local_48;
  ulonglong uVar6;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStack_d8;
  uVar6 = 0;
  lVar2 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*(int *)(param_1 + 0x18) * 8);
  if (param_3 == (longlong *)0x0) {
    *(undefined8 *)(lVar2 + 0x58) = 0;
  }
  else {
    lVar3 = param_3[1];
    *(longlong *)(lVar2 + 0x48) = *param_3;
    *(longlong *)(lVar2 + 0x50) = lVar3;
    *(longlong **)(lVar2 + 0x58) = (longlong *)(lVar2 + 0x48);
    if ((DAT_18002b340 == *param_3) && (DAT_18002b348 == param_3[1])) {
      return 0;
    }
    if ((DAT_18002b350 == *param_3) && (DAT_18002b358 == param_3[1])) {
      return 0;
    }
  }
  iVar4 = (*DAT_18002bb00)(param_3,&local_b8,0);
  if (iVar4 != 0) {
    return 0;
  }
  local_a8 = 0x60;
  local_64 = 0;
  uStack_5c = 0;
  local_a4 = 0;
  uStack_9c = 0;
  local_94 = 0;
  uStack_8c = 0;
  local_84 = 0;
  uStack_7c = 0;
  local_74 = 0;
  uStack_6c = 0;
  uStack_58 = 0;
  uStack_54 = 0;
  uStack_50 = 0;
  iVar4 = (**(code **)(*local_b8 + 0x20))();
  if ((iVar4 != 0) || ((local_a4 & 0x20) != 0)) {
    (**(code **)(*local_b8 + 0x10))();
    return 0;
  }
  *(undefined4 *)(lVar2 + 0x14) = 0;
  *(undefined1 *)(lVar2 + 0x78) = 1;
  *(uint *)(lVar2 + 0x18) = (((uint)local_a4 >> 1 & 1) != 0) + 1;
  *(bool *)(lVar2 + 0x79) = ((uint)local_a4 >> 1 & 1) == 0;
  if (param_4 == (LPCWSTR)0x0) {
    iVar4 = (**(code **)(*local_b8 + 0x40))(local_b8,local_b0);
    if (-1 < iVar4) {
      switch(local_b0[0]) {
      case 1:
      case 4:
        uVar5 = 2;
        break;
      case 2:
        uVar5 = 1;
        break;
      case 3:
      case 5:
        uVar5 = 4;
        break;
      case 6:
      case 9:
        uVar5 = 6;
        break;
      case 7:
      case 8:
        uVar5 = 8;
        break;
      default:
        goto switchD_18000b6e7_default;
      }
LAB_18000b71a:
      *(uint *)(lVar2 + 0x18) = uVar5;
      *(undefined1 *)(lVar2 + 0x79) = 1;
    }
  }
  else {
    uVar5 = FUN_180020a10(param_4,0);
    if (0 < (int)uVar5) goto LAB_18000b71a;
  }
switchD_18000b6e7_default:
  if ((local_a4 & 0x10) != 0) {
    *(double *)(lVar2 + 0x40) = (double)(uStack_9c & 0xffffffff);
    do {
      dVar1 = *(double *)(&DAT_18002b390 + uVar6 * 8);
      if (((double)local_a4._4_4_ <= dVar1) && (dVar1 <= (double)(uStack_9c & 0xffffffff))) {
        *(double *)(lVar2 + 0x40) = dVar1;
        break;
      }
      uVar5 = (int)uVar6 + 1;
      uVar6 = (ulonglong)uVar5;
    } while ((int)uVar5 < 0xd);
    goto LAB_18000b7d6;
  }
  if (local_a4._4_4_ == (uint)uStack_9c) {
    dVar1 = DAT_180023d60;
    if (local_a4._4_4_ != 0) goto LAB_18000b7c9;
  }
  else {
    if (((double)local_a4._4_4_ < DAT_1800239c8) &&
       (_DAT_180023d68 < (double)(uStack_9c & 0xffffffff))) {
      *(undefined8 *)(lVar2 + 0x40) = 0x40e7700000000000;
      goto LAB_18000b7d6;
    }
LAB_18000b7c9:
    dVar1 = (double)(uStack_9c & 0xffffffff);
  }
  *(double *)(lVar2 + 0x40) = dVar1;
LAB_18000b7d6:
  *(undefined8 *)(lVar2 + 0x20) = 0;
  *(undefined8 *)(lVar2 + 0x30) = 0;
  dVar1 = FUN_18000cc80();
  *(double *)(lVar2 + 0x28) = dVar1;
  *(double *)(lVar2 + 0x38) = dVar1 + dVar1;
  (**(code **)(*local_b8 + 0x10))();
  *(undefined8 *)(lVar2 + 8) = param_2;
  if (param_3 == (longlong *)0x0) {
    *(undefined4 *)(param_1 + 0x20) = *(undefined4 *)(param_1 + 0x18);
  }
  *(int *)(param_1 + 0x18) = *(int *)(param_1 + 0x18) + 1;
  return 0;
}



/* ========================================================================
   ENTRY: 18000b860
   NAME : FUN_18000b860
   SIG  : undefined __fastcall FUN_18000b860(uint * param_1, uint * param_2, int param_3, int param_4, uint param_5, uint param_6, double param_7, uint param_8)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000b860(uint *param_1,uint *param_2,int param_3,int param_4,uint param_5,uint param_6,
                  double param_7,uint param_8)

{
  uint uVar1;
  uint uVar2;
  int iVar3;
  uint uVar4;
  uint uVar5;
  double dVar6;
  
  uVar5 = (uint)(longlong)(param_7 * _DAT_180023d18);
  uVar1 = (uint)(longlong)(param_7 * _DAT_180023cb0);
  if (param_8 == 0) {
    if (param_6 < param_5) {
      param_6 = param_5;
    }
    uVar2 = param_6 >> 2;
    *param_2 = uVar2;
    if (uVar2 < uVar1) {
      *param_2 = uVar1;
      if (param_6 < uVar1 * 2) {
        param_6 = uVar1 * 2;
      }
      *param_1 = param_6 + uVar1;
    }
    else if (uVar5 < uVar2) {
      *param_2 = uVar5;
      if (param_6 < uVar5 + uVar1) {
        param_6 = uVar5 + uVar1;
      }
      *param_1 = param_6 + uVar5;
    }
    else {
      if (param_6 < uVar2 + uVar1) {
        param_6 = uVar2 + uVar1;
      }
      *param_1 = param_6 + uVar2;
    }
  }
  else {
    if ((param_3 == 0) || (param_4 == 0)) {
      if (param_5 < param_6) {
        param_5 = param_6;
      }
    }
    else if ((param_8 < param_6) && (param_5 < param_6 - param_8)) {
      param_5 = param_6 - param_8;
    }
    uVar2 = uVar1 + param_8;
    if (uVar1 + param_8 <= param_5) {
      uVar2 = param_5;
    }
    uVar4 = param_8 >> 2;
    *param_1 = uVar2 + param_8;
    uVar1 = uVar4;
    if (uVar4 == 0) {
      uVar1 = 1;
    }
    uVar2 = param_5 >> 4;
    if ((param_5 >> 4 < uVar1) && (uVar2 = uVar4, uVar4 == 0)) {
      uVar2 = 1;
    }
    *param_2 = uVar2;
    if (uVar5 < uVar2) {
      *param_2 = uVar5;
    }
  }
  if ((param_3 != 0) && (iVar3 = FUN_180020730(), 5 < iVar3)) {
    dVar6 = (double)*param_1;
    if ((double)*param_1 <= param_7 * _DAT_180023d10) {
      dVar6 = param_7 * _DAT_180023d10;
    }
    *param_1 = (uint)(longlong)dVar6;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b9b0
   NAME : FUN_18000b9b0
   SIG  : undefined __fastcall FUN_18000b9b0(uint param_1, uint * param_2, double param_3, uint param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000b9b0(uint param_1,uint *param_2,double param_3,uint param_4)

{
  uint uVar1;
  uint uVar2;
  uint uVar3;
  double dVar4;
  
  dVar4 = param_3 * _DAT_180023d18;
  uVar2 = param_4 >> 2;
  uVar1 = uVar2;
  if (uVar2 == 0) {
    uVar1 = 1;
  }
  uVar3 = param_1 >> 4;
  if ((param_1 >> 4 < uVar1) && (uVar3 = uVar2, uVar2 == 0)) {
    uVar3 = 1;
  }
  *param_2 = uVar3;
  uVar1 = (uint)(longlong)dVar4;
  if (uVar1 < uVar3) {
    *param_2 = uVar1;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b9f0
   NAME : FUN_18000b9f0
   SIG  : ulonglong __fastcall FUN_18000b9f0(longlong param_1)
   ======================================================================== */

ulonglong FUN_18000b9f0(longlong param_1)

{
  uint uVar1;
  ulonglong uVar2;
  uint local_res8 [2];
  void *local_res10;
  uint *puVar3;
  
  uVar2 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x68))(*(longlong **)(param_1 + 0x180),0);
  if ((int)uVar2 == 0) {
    puVar3 = local_res8;
    uVar2 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x58))
                      (*(longlong **)(param_1 + 0x180),0,*(undefined4 *)(param_1 + 0x18c),
                       &local_res10,puVar3,0,0,0);
    if ((int)uVar2 == 0) {
      memset(local_res10,0,(ulonglong)local_res8[0]);
      uVar2 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x98))
                        (*(longlong **)(param_1 + 0x180),local_res10,local_res8[0],0,
                         (ulonglong)puVar3 & 0xffffffff00000000);
      if (((int)uVar2 == 0) &&
         (uVar1 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x20))
                            (*(longlong **)(param_1 + 0x180),param_1 + 0x1a8,param_1 + 0x188),
         uVar2 = 0, uVar1 != 0)) {
        uVar2 = (ulonglong)uVar1;
      }
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000bac0
   NAME : FUN_18000bac0
   SIG  : undefined8 __fastcall FUN_18000bac0(undefined4 * param_1)
   ======================================================================== */

undefined8 FUN_18000bac0(undefined4 *param_1)

{
  CloseHandle(*(HANDLE *)(param_1 + 0x80));
  if (*(HANDLE *)(param_1 + 0x88) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x88));
  }
  if (*(longlong **)(param_1 + 0x60) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x60) + 0x90))();
    (**(code **)(**(longlong **)(param_1 + 0x60) + 0x10))();
    *(undefined8 *)(param_1 + 0x60) = 0;
  }
  if (*(longlong **)(param_1 + 0x5e) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x5e) + 0x10))();
    *(undefined8 *)(param_1 + 0x5e) = 0;
  }
  if (*(longlong **)(param_1 + 0x70) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x70) + 0x50))();
    (**(code **)(**(longlong **)(param_1 + 0x70) + 0x10))();
    *(undefined8 *)(param_1 + 0x70) = 0;
  }
  if (*(longlong **)(param_1 + 0x6e) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x6e) + 0x10))();
    *(undefined8 *)(param_1 + 0x6e) = 0;
  }
  if (*(longlong **)(param_1 + 0x5c) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x5c) + 0x10))();
    *(undefined8 *)(param_1 + 0x5c) = 0;
  }
  FUN_1800069c0((longlong)(param_1 + 0x1a));
  FUN_180006ea0(param_1);
  FUN_180020240((longlong)param_1);
  return 0;
}



/* ========================================================================
   ENTRY: 18000bbb0
   NAME : FUN_18000bbb0
   SIG  : undefined8 __fastcall FUN_18000bbb0(undefined8 * param_1, LPCWSTR param_2, undefined8 param_3, undefined8 * param_4)
   ======================================================================== */

undefined8 FUN_18000bbb0(undefined8 *param_1,LPCWSTR param_2,undefined8 param_3,undefined8 *param_4)

{
  undefined8 *puVar1;
  int iVar2;
  longlong lVar3;
  undefined8 uVar4;
  longlong lVar5;
  LPSTR pCVar6;
  
  if (*(int *)(param_4 + 2) == 0) {
    uVar4 = FUN_18000bdc0((longlong)param_4);
    if ((int)uVar4 != 0) {
      *(int *)(param_4 + 1) = (int)uVar4;
      return 0;
    }
  }
  lVar5 = (longlong)*(int *)((longlong)param_4 + 0xc);
  lVar3 = param_4[3];
  if (param_1 == (undefined8 *)0x0) {
    *(undefined8 *)(lVar3 + 0x18 + lVar5 * 0x28) = 0;
  }
  else {
    *(longlong *)(lVar3 + 0x18 + lVar5 * 0x28) = lVar3 + 8 + lVar5 * 0x28;
    uVar4 = param_1[1];
    puVar1 = (undefined8 *)(param_4[3] + 8 + (longlong)*(int *)((longlong)param_4 + 0xc) * 0x28);
    *puVar1 = *param_1;
    puVar1[1] = uVar4;
  }
  iVar2 = *(int *)((longlong)param_4 + 0xc);
  lVar3 = param_4[3];
  pCVar6 = FUN_18000bca0((int *)*param_4,param_2);
  *(LPSTR *)(lVar3 + (longlong)iVar2 * 0x28) = pCVar6;
  if (*(longlong *)(param_4[3] + (longlong)*(int *)((longlong)param_4 + 0xc) * 0x28) == 0) {
    *(undefined4 *)(param_4 + 1) = 0xffffd8f8;
    return 0;
  }
  *(undefined8 *)(param_4[3] + 0x20 + (longlong)*(int *)((longlong)param_4 + 0xc) * 0x28) = 0;
  *(int *)((longlong)param_4 + 0xc) = *(int *)((longlong)param_4 + 0xc) + 1;
  *(int *)(param_4 + 2) = *(int *)(param_4 + 2) + -1;
  return 1;
}



/* ========================================================================
   ENTRY: 18000bca0
   NAME : FUN_18000bca0
   SIG  : LPSTR __fastcall FUN_18000bca0(int * param_1, LPCWSTR param_2)
   ======================================================================== */

LPSTR FUN_18000bca0(int *param_1,LPCWSTR param_2)

{
  int iVar1;
  LPSTR lpMultiByteStr;
  LPSTR pCVar2;
  
  if (param_2 == (LPCWSTR)0x0) {
    pCVar2 = (LPSTR)FUN_1800012e0(param_1,1);
    lpMultiByteStr = (LPSTR)0x0;
    if (pCVar2 != (LPSTR)0x0) {
      *pCVar2 = '\0';
      return pCVar2;
    }
  }
  else {
    iVar1 = WideCharToMultiByte(0xfde9,0,param_2,-1,(LPSTR)0x0,0,(LPCSTR)0x0,(LPBOOL)0x0);
    lpMultiByteStr = (LPSTR)FUN_1800012e0(param_1,iVar1 + 1);
    if ((lpMultiByteStr != (LPSTR)0x0) &&
       (iVar1 = WideCharToMultiByte(0xfde9,0,param_2,-1,lpMultiByteStr,iVar1,(LPCSTR)0x0,(LPBOOL)0x0
                                   ), iVar1 == 0)) {
      lpMultiByteStr = (LPSTR)0x0;
    }
  }
  return lpMultiByteStr;
}



/* ========================================================================
   ENTRY: 18000bd70
   NAME : FUN_18000bd70
   SIG  : wchar_t * __fastcall FUN_18000bd70(int * param_1, wchar_t * param_2)
   ======================================================================== */

wchar_t * FUN_18000bd70(int *param_1,wchar_t *param_2)

{
  size_t sVar1;
  wchar_t *_Dest;
  
  sVar1 = wcslen(param_2);
  _Dest = (wchar_t *)FUN_1800012e0(param_1,(int)sVar1 * 2 + 2);
  wcscpy(_Dest,param_2);
  return _Dest;
}



/* ========================================================================
   ENTRY: 18000bdc0
   NAME : FUN_18000bdc0
   SIG  : undefined8 __fastcall FUN_18000bdc0(longlong param_1)
   ======================================================================== */

undefined8 FUN_18000bdc0(longlong param_1)

{
  undefined4 *puVar1;
  undefined4 *puVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined4 uVar5;
  HLOCAL pvVar6;
  longlong lVar7;
  int iVar8;
  
  iVar8 = *(int *)(param_1 + 0x10);
  *(int *)(param_1 + 0x10) = *(int *)(param_1 + 0xc) + iVar8 * 2;
  pvVar6 = LocalAlloc(0,(longlong)(*(int *)(param_1 + 0xc) + iVar8) * 0x50);
  if (pvVar6 == (HLOCAL)0x0) {
    return 0xffffd8f8;
  }
  iVar8 = 0;
  if (0 < *(int *)(param_1 + 0xc)) {
    do {
      lVar7 = (longlong)iVar8;
      *(undefined8 *)((longlong)pvVar6 + lVar7 * 0x28) =
           *(undefined8 *)(*(longlong *)(param_1 + 0x18) + lVar7 * 0x28);
      if (*(longlong *)(*(longlong *)(param_1 + 0x18) + 0x18 + lVar7 * 0x28) == 0) {
        *(undefined8 *)((longlong)pvVar6 + lVar7 * 0x28 + 0x18) = 0;
      }
      else {
        *(longlong *)((longlong)pvVar6 + lVar7 * 0x28 + 0x18) = (longlong)pvVar6 + lVar7 * 0x28 + 8;
        puVar2 = *(undefined4 **)(*(longlong *)(param_1 + 0x18) + 0x18 + lVar7 * 0x28);
        uVar3 = puVar2[1];
        uVar4 = puVar2[2];
        uVar5 = puVar2[3];
        puVar1 = (undefined4 *)((longlong)pvVar6 + lVar7 * 0x28 + 8);
        *puVar1 = *puVar2;
        puVar1[1] = uVar3;
        puVar1[2] = uVar4;
        puVar1[3] = uVar5;
      }
      iVar8 = iVar8 + 1;
      *(undefined8 *)((longlong)pvVar6 + lVar7 * 0x28 + 0x20) =
           *(undefined8 *)(*(longlong *)(param_1 + 0x18) + 0x20 + lVar7 * 0x28);
    } while (iVar8 < *(int *)(param_1 + 0xc));
  }
  LocalFree(*(HLOCAL *)(param_1 + 0x18));
  *(HLOCAL *)(param_1 + 0x18) = pvVar6;
  return 0;
}



/* ========================================================================
   ENTRY: 18000bea0
   NAME : FUN_18000bea0
   SIG  : undefined __fastcall FUN_18000bea0(undefined8 param_1)
   ======================================================================== */

void FUN_18000bea0(undefined8 param_1)

{
  int iVar1;
  undefined1 local_res10 [8];
  longlong *local_res18;
  longlong *local_res20;
  code *local_18;
  undefined8 local_10;
  
  iVar1 = (*DAT_18002baf8)(&DAT_18002b360,&DAT_180025d90,&local_res20);
  if (iVar1 == 0) {
    iVar1 = (**(code **)(*local_res20 + 0x18))(local_res20,0,&DAT_18002b380,&local_res18);
    if (iVar1 == 0) {
      local_18 = FUN_18000c3e0;
      local_10 = param_1;
      (**(code **)(*local_res18 + 0x18))
                (local_res18,&DAT_18002b370,8,0,0,&local_18,0x10,local_res10);
      (**(code **)(*local_res18 + 0x10))();
    }
    (**(code **)(*local_res20 + 0x10))();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000bf60
   NAME : FUN_18000bf60
   SIG  : undefined8 __fastcall FUN_18000bf60(void)
   ======================================================================== */

undefined8 FUN_18000bf60(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 18000bf80
   NAME : FUN_18000bf80
   SIG  : undefined8 __fastcall FUN_18000bf80(longlong param_1, longlong param_2, uint param_3, uint param_4, ushort param_5, undefined4 param_6, undefined4 param_7)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8
FUN_18000bf80(longlong param_1,longlong param_2,uint param_3,uint param_4,ushort param_5,
             undefined4 param_6,undefined4 param_7)

{
  int iVar1;
  undefined8 uVar2;
  undefined1 auStackY_b8 [32];
  undefined8 local_88;
  undefined4 local_80;
  int local_7c;
  undefined2 *local_78;
  undefined2 local_70 [24];
  ulonglong local_40;
  
  local_40 = DAT_18002b580 ^ (ulonglong)auStackY_b8;
  uVar2 = (*DAT_18002bb18)(*(undefined8 *)(param_2 + 0x58),param_1 + 0x1b8,0);
  local_7c = (int)uVar2;
  if (local_7c == 0) {
    local_80 = param_6;
    local_78 = local_70;
    local_88 = 0x18;
    uVar2 = FUN_1800208c0(param_3);
    FUN_180020830(local_70,(uint)param_5,param_3,(uint)uVar2,(double)param_4,param_7);
    iVar1 = (**(code **)(**(longlong **)(param_1 + 0x1b8) + 0x18))
                      (*(longlong **)(param_1 + 0x1b8),&local_88,param_1 + 0x1c0,0);
    if (iVar1 != 0) {
      uVar2 = FUN_1800208c0(param_3);
      FUN_1800207c0(local_70,(uint)param_5,param_3,(short)uVar2,(double)param_4);
      uVar2 = (**(code **)(**(longlong **)(param_1 + 0x1b8) + 0x18))
                        (*(longlong **)(param_1 + 0x1b8),&local_88,param_1 + 0x1c0,0);
      if ((int)uVar2 != 0) {
        return uVar2;
      }
    }
    *(undefined4 *)(param_1 + 0x1cc) = 0;
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000c0d0
   NAME : FUN_18000c0d0
   SIG  : ulonglong __fastcall FUN_18000c0d0(longlong param_1, longlong param_2, uint param_3, uint param_4, ushort param_5, undefined4 param_6, undefined4 param_7)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18000c0d0(longlong param_1,longlong param_2,uint param_3,uint param_4,ushort param_5,
                       undefined4 param_6,undefined4 param_7)

{
  int iVar1;
  HWND pHVar2;
  undefined8 uVar3;
  ulonglong unaff_RBX;
  undefined1 auStackY_e8 [32];
  undefined4 local_b8;
  undefined4 local_b4;
  undefined8 local_b0;
  undefined8 local_a8;
  undefined4 local_a0;
  undefined4 local_9c;
  undefined4 local_98;
  undefined4 local_94;
  undefined2 *local_90;
  undefined2 local_88 [24];
  ulonglong local_58;
  
  local_58 = DAT_18002b580 ^ (ulonglong)auStackY_e8;
  iVar1 = (*DAT_18002bb00)(*(undefined8 *)(param_2 + 0x58),param_1 + 0x170,0);
  if (iVar1 != 0) {
    return unaff_RBX;
  }
  pHVar2 = GetDesktopWindow();
  iVar1 = (**(code **)(**(longlong **)(param_1 + 0x170) + 0x30))
                    (*(longlong **)(param_1 + 0x170),pHVar2,3);
  if (iVar1 != 0) {
    return unaff_RBX;
  }
  local_b0 = 0;
  local_a8 = 0;
  local_b8 = 0x18;
  local_b4 = 1;
  iVar1 = (**(code **)(**(longlong **)(param_1 + 0x170) + 0x18))
                    (*(longlong **)(param_1 + 0x170),&local_b8,param_1 + 0x178,0);
  if (iVar1 == 0) {
    uVar3 = FUN_1800208c0(param_3);
    FUN_180020830(local_88,(uint)param_5,param_3,(uint)uVar3,(double)param_4,param_7);
    iVar1 = (**(code **)(**(longlong **)(param_1 + 0x178) + 0x70))
                      (*(longlong **)(param_1 + 0x178),local_88);
    if (iVar1 != 0) {
      uVar3 = FUN_1800208c0(param_3);
      FUN_1800207c0(local_88,(uint)param_5,param_3,(short)uVar3,(double)param_4);
      iVar1 = (**(code **)(**(longlong **)(param_1 + 0x178) + 0x70))
                        (*(longlong **)(param_1 + 0x178),local_88);
      if (iVar1 != 0) goto LAB_18000c275;
    }
    local_98 = param_6;
    local_90 = local_88;
    local_94 = 0;
    local_a0 = 0x18;
    local_9c = 0x18000;
    iVar1 = (**(code **)(**(longlong **)(param_1 + 0x170) + 0x18))
                      (*(longlong **)(param_1 + 0x170),&local_a0,param_1 + 0x180,0);
    if (iVar1 == 0) {
      return unaff_RBX;
    }
  }
LAB_18000c275:
  if (*(longlong **)(param_1 + 0x178) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x178) + 0x10))();
    *(undefined8 *)(param_1 + 0x178) = 0;
  }
  return unaff_RBX;
}



/* ========================================================================
   ENTRY: 18000c2d0
   NAME : FUN_18000c2d0
   SIG  : undefined4 __fastcall FUN_18000c2d0(undefined8 * param_1, undefined8 param_2)
   ======================================================================== */

undefined4 FUN_18000c2d0(undefined8 *param_1,undefined8 param_2)

{
  HLOCAL pvVar1;
  undefined4 uVar2;
  
  *param_1 = param_2;
  param_1[1] = 0;
  *(undefined4 *)(param_1 + 2) = 8;
  pvVar1 = LocalAlloc(0,0x140);
  param_1[3] = pvVar1;
  uVar2 = 0xffffd8f8;
  if (pvVar1 != (HLOCAL)0x0) {
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000c320
   NAME : FUN_18000c320
   SIG  : ulonglong __fastcall FUN_18000c320(longlong param_1, int * param_2, int * param_3)
   ======================================================================== */

ulonglong FUN_18000c320(longlong param_1,int *param_2,int *param_3)

{
  longlong lVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  if (param_2 != (int *)0x0) {
    if (*param_2 == -2) {
      return 0xffffd8f4;
    }
    lVar1 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_2 * 8);
    if ((*(char *)(lVar1 + 0x78) != '\0') && (*(int *)(lVar1 + 0x14) < param_2[1])) {
      return 0xffffd8f2;
    }
    uVar2 = FUN_18000dc20(param_2,*(int **)(param_2 + 6));
    if ((int)uVar2 != 0) {
      return uVar2;
    }
  }
  if (param_3 == (int *)0x0) {
    return 0;
  }
  if (*param_3 == -2) {
    return 0xffffd8f4;
  }
  lVar1 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8);
  if ((*(char *)(lVar1 + 0x79) != '\0') && (*(int *)(lVar1 + 0x18) < param_3[1])) {
    return 0xffffd8f2;
  }
  uVar2 = FUN_18000dc20(param_3,*(int **)(param_3 + 6));
  uVar3 = 0;
  if ((int)uVar2 != 0) {
    uVar3 = uVar2 & 0xffffffff;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18000c3e0
   NAME : FUN_18000c3e0
   SIG  : undefined8 __fastcall FUN_18000c3e0(longlong param_1, longlong * param_2)
   ======================================================================== */

undefined8 FUN_18000c3e0(longlong param_1,longlong *param_2)

{
  longlong *plVar1;
  wchar_t *pwVar2;
  longlong lVar3;
  uint uVar4;
  ulonglong uVar5;
  
  pwVar2 = *(wchar_t **)(param_1 + 0x28);
  if (pwVar2 != (wchar_t *)0x0) {
    if (*(int *)(param_1 + 4) == 0) {
      if (*(int *)((longlong)param_2 + 0x34) < 1) {
        return 1;
      }
      uVar5 = 0;
      while (((plVar1 = *(longlong **)(param_2[8] + 0x18 + uVar5 * 0x28), plVar1 == (longlong *)0x0
              || (*(longlong *)(param_1 + 8) != *plVar1)) ||
             (*(longlong *)(param_1 + 0x10) != plVar1[1]))) {
        uVar4 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar4;
        if (*(int *)((longlong)param_2 + 0x34) <= (int)uVar4) {
          return 1;
        }
      }
      pwVar2 = FUN_18000bd70(*(int **)(*param_2 + 0x108),pwVar2);
      lVar3 = param_2[8];
    }
    else {
      if (*(int *)(param_1 + 4) != 1) {
        return 1;
      }
      if (*(int *)((longlong)param_2 + 0x14) < 1) {
        return 1;
      }
      uVar5 = 0;
      while (((plVar1 = *(longlong **)(param_2[4] + 0x18 + uVar5 * 0x28), plVar1 == (longlong *)0x0
              || (*(longlong *)(param_1 + 8) != *plVar1)) ||
             (*(longlong *)(param_1 + 0x10) != plVar1[1]))) {
        uVar4 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar4;
        if (*(int *)((longlong)param_2 + 0x14) <= (int)uVar4) {
          return 1;
        }
      }
      pwVar2 = FUN_18000bd70(*(int **)(*param_2 + 0x108),pwVar2);
      lVar3 = param_2[4];
    }
    *(wchar_t **)(lVar3 + 0x20 + uVar5 * 0x28) = pwVar2;
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18000c500
   NAME : FUN_18000c500
   SIG  : ulonglong __fastcall FUN_18000c500(longlong param_1, undefined8 * param_2, int * param_3, int * param_4, double param_5, uint param_6, uint param_7, longlong param_8, undefined8 param_9)
   ======================================================================== */

ulonglong FUN_18000c500(longlong param_1,undefined8 *param_2,int *param_3,int *param_4,
                       double param_5,uint param_6,uint param_7,longlong param_8,undefined8 param_9)

{
  bool bVar1;
  double dVar2;
  uint extraout_EAX;
  DWORD DVar3;
  BOOL BVar4;
  DWORD extraout_EAX_00;
  ulonglong uVar5;
  undefined4 *puVar6;
  HANDLE pvVar7;
  undefined8 uVar8;
  int iVar9;
  uint uVar10;
  int *piVar11;
  longlong lVar12;
  uint uVar13;
  int *piVar14;
  uint uVar15;
  uint uVar16;
  int local_res8;
  uint local_res18;
  uint local_78;
  int local_74;
  uint local_70;
  int local_6c;
  undefined8 local_68;
  uint local_60;
  LARGE_INTEGER local_58;
  undefined8 local_50;
  
  bVar1 = false;
  uVar16 = 0;
  local_78 = 0;
  local_res8 = (int)param_1;
  if (param_3 == (int *)0x0) {
    uVar10 = 0;
    uVar13 = 0;
    local_68 = (ulonglong)local_68._4_4_ << 0x20;
    local_58.QuadPart = local_58.QuadPart & 0xffffffff00000000;
    local_6c = local_res8;
  }
  else {
    if (*param_3 == -2) {
      return 0xffffd8f4;
    }
    uVar13 = param_3[1];
    lVar12 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8);
    if ((*(char *)(lVar12 + 0x78) != '\0') && (*(int *)(lVar12 + 0x14) < (int)uVar13)) {
      return 0xffffd8f2;
    }
    piVar11 = *(int **)(param_3 + 6);
    piVar14 = param_3;
    uVar5 = FUN_18000dc20(param_3,piVar11);
    if ((int)uVar5 != 0) {
      return uVar5;
    }
    uVar10 = 0;
    local_68 = CONCAT44(local_68._4_4_,piVar14[2]);
    local_58.QuadPart = (LONGLONG)(param_5 * *(double *)(piVar14 + 4));
    if (piVar11 != (int *)0x0) {
      if ((piVar11[3] & 1U) != 0) {
        uVar10 = piVar11[4];
      }
      if ((piVar11[3] & 4U) != 0) {
        local_6c = piVar11[5];
        goto LAB_18000c5f4;
      }
    }
    uVar8 = FUN_180020750(uVar13);
    local_6c = (int)uVar8;
  }
LAB_18000c5f4:
  if (param_4 == (int *)0x0) {
    uVar15 = 0;
    local_60 = 0;
    local_50 = (ulonglong)local_50._4_4_ << 0x20;
  }
  else {
    if (*param_4 == -2) {
      return 0xffffd8f4;
    }
    uVar15 = param_4[1];
    lVar12 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_4 * 8);
    if ((*(char *)(lVar12 + 0x79) != '\0') && (*(int *)(lVar12 + 0x18) < (int)uVar15)) {
      return 0xffffd8f2;
    }
    piVar11 = *(int **)(param_4 + 6);
    uVar5 = FUN_18000dc20(param_4,piVar11);
    if ((int)uVar5 != 0) {
      return uVar5;
    }
    local_60 = param_4[2];
    uVar16 = 0;
    local_50 = (longlong)(param_5 * *(double *)(param_4 + 4));
    if (piVar11 != (int *)0x0) {
      if ((piVar11[3] & 1U) != 0) {
        uVar16 = piVar11[4];
      }
      if ((piVar11[3] & 4U) != 0) {
        local_res8 = piVar11[5];
        goto LAB_18000c6af;
      }
    }
    uVar8 = FUN_180020750(uVar15);
    local_res8 = (int)uVar8;
  }
LAB_18000c6af:
  if (((0 < (int)uVar10) && (0 < (int)uVar16)) && (uVar10 != uVar16)) {
    return 0xffffd900;
  }
  if ((param_7 & 0xffff0000) != 0) {
    return 0xffffd8f5;
  }
  local_74 = local_res8;
  puVar6 = (undefined4 *)FUN_180020230(0x238);
  if (puVar6 == (undefined4 *)0x0) {
    return 0xffffd8f8;
  }
  lVar12 = 0x48;
  if (param_8 == 0) {
    lVar12 = 0xa8;
  }
  FUN_180006e70(puVar6,lVar12 + param_1,param_8,param_9);
  puVar6[0x7d] = param_7;
  FUN_180003950((double *)(puVar6 + 0x14),param_5);
  if (param_3 == (int *)0x0) {
    local_res18 = 0;
  }
  else {
    local_res18 = FUN_1800030c0(0x5e,param_3[2]);
  }
  if (param_4 == (int *)0x0) {
    local_70 = 0;
  }
  else {
    local_70 = FUN_1800030c0(0x5e,param_4[2]);
  }
  FUN_180006290(puVar6 + 0x1a,uVar13,(uint)local_68,local_res18,uVar15,local_60,local_70,param_5,
                param_7,param_6,0,3,param_8,param_9);
  uVar5 = (ulonglong)extraout_EAX;
  if (extraout_EAX != 0) goto LAB_18000c869;
  bVar1 = true;
  pvVar7 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
  *(HANDLE *)(puVar6 + 0x80) = pvVar7;
  if (pvVar7 == (HANDLE)0x0) {
    uVar5 = 0xffffd8f8;
    goto LAB_18000c869;
  }
  pvVar7 = CreateWaitableTimerA((LPSECURITY_ATTRIBUTES)0x0,0,(LPCSTR)0x0);
  *(HANDLE *)(puVar6 + 0x88) = pvVar7;
  if (pvVar7 == (HANDLE)0x0) {
    DVar3 = GetLastError();
  }
  else {
    local_68 = (ulonglong)(param_5 + DAT_180023560);
    if (((int)uVar10 < 1) && ((int)uVar16 < 1)) {
      FUN_18000b860(puVar6 + 0x75,&local_78,(uint)(param_3 != (int *)0x0),
                    (uint)(param_4 != (int *)0x0),local_58.s.LowPart,(uint)local_50,param_5,param_6)
      ;
    }
    else {
      if ((int)uVar16 < (int)uVar10) {
        uVar16 = uVar10;
      }
      puVar6[0x75] = uVar16;
      FUN_18000b9b0(uVar16,&local_78,param_5,param_6);
    }
    uVar16 = local_78;
    *(double *)(puVar6 + 0x7a) = (double)local_78 / param_5;
    if (param_4 == (int *)0x0) {
LAB_18000ca40:
      uVar13 = (uint)local_68;
    }
    else {
      uVar8 = Pa_GetSampleSize(local_70);
      iVar9 = param_4[1] * puVar6[0x75] * (int)uVar8;
      puVar6[100] = param_4[1] * (int)uVar8;
      puVar6[99] = iVar9;
      dVar2 = DAT_1800235c8;
      if (iVar9 < 4) {
        uVar5 = 0xffffd8fa;
        goto LAB_18000c869;
      }
      if (0xfffffff < iVar9) {
        uVar5 = 0xffffd8f9;
        goto LAB_18000c869;
      }
      *(undefined8 *)(puVar6 + 0x6b) = 0;
      *(double *)(puVar6 + 0x78) = dVar2 / ((double)(uVar15 * puVar6[0x2b]) * param_5);
      BVar4 = QueryPerformanceFrequency(&local_58);
      if (BVar4 == 0) {
        *(undefined8 *)(puVar6 + 0x66) = 0;
        goto LAB_18000ca40;
      }
      uVar13 = (uint)local_68;
      *(longlong *)(puVar6 + 0x66) =
           ((int)puVar6[0x75] * local_58.QuadPart) / (longlong)(local_68 & 0xffffffff);
    }
    if (param_3 != (int *)0x0) {
      uVar8 = Pa_GetSampleSize(local_res18);
      uVar10 = param_3[1] * puVar6[0x75] * (int)uVar8;
      puVar6[0x72] = param_3[1] * (int)uVar8;
      puVar6[0x74] = uVar10;
      if (uVar10 < 4) {
        uVar5 = 0xffffd8fa;
        goto LAB_18000c869;
      }
      if (0xfffffff < uVar10) {
        uVar5 = 0xffffd8f9;
        goto LAB_18000c869;
      }
    }
    if (((param_4 == (int *)0x0) || (*(longlong *)(puVar6 + 0x60) != 0)) ||
       (puVar6 = (undefined4 *)
                 FUN_18000c0d0((longlong)puVar6,
                               *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_4 * 8)
                               ,local_70,uVar13,*(ushort *)(param_4 + 1),puVar6[99],local_74),
       DVar3 = extraout_EAX_00, extraout_EAX_00 == 0)) {
      if ((param_3 == (int *)0x0) || (*(longlong *)(puVar6 + 0x70) != 0)) {
LAB_18000cb50:
        FUN_18000d2d0((longlong)puVar6,param_6,uVar16,param_5);
        *(double *)(puVar6 + 0x12) = param_5;
        *param_2 = puVar6;
        return 0;
      }
      uVar8 = FUN_18000bf80((longlong)puVar6,
                            *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8),
                            local_res18,uVar13,*(ushort *)(param_3 + 1),puVar6[0x74],local_6c);
      DVar3 = (DWORD)uVar8;
      if (DVar3 == 0) goto LAB_18000cb50;
    }
  }
  uVar5 = 0xffffd8f1;
  FUN_180003c90(1,DVar3,"DirectSound error");
LAB_18000c869:
  if (*(HANDLE *)(puVar6 + 0x80) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(puVar6 + 0x80));
  }
  if (*(HANDLE *)(puVar6 + 0x88) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(puVar6 + 0x88));
  }
  if (*(longlong **)(puVar6 + 0x60) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(puVar6 + 0x60) + 0x90))();
    (**(code **)(**(longlong **)(puVar6 + 0x60) + 0x10))();
    *(undefined8 *)(puVar6 + 0x60) = 0;
  }
  if (*(longlong **)(puVar6 + 0x5e) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(puVar6 + 0x5e) + 0x10))();
    *(undefined8 *)(puVar6 + 0x5e) = 0;
  }
  if (*(longlong **)(puVar6 + 0x70) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(puVar6 + 0x70) + 0x50))();
    (**(code **)(**(longlong **)(puVar6 + 0x70) + 0x10))();
    *(undefined8 *)(puVar6 + 0x70) = 0;
  }
  if (*(longlong **)(puVar6 + 0x6e) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(puVar6 + 0x6e) + 0x10))();
    *(undefined8 *)(puVar6 + 0x6e) = 0;
  }
  if (*(longlong **)(puVar6 + 0x5c) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(puVar6 + 0x5c) + 0x10))();
    *(undefined8 *)(puVar6 + 0x5c) = 0;
  }
  if (bVar1) {
    FUN_1800069c0((longlong)(puVar6 + 0x1a));
  }
  FUN_180006ea0(puVar6);
  FUN_180020240((longlong)puVar6);
  return uVar5;
}



/* ========================================================================
   ENTRY: 18000cc30
   NAME : FUN_18000cc30
   SIG  : undefined8 __fastcall FUN_18000cc30(void)
   ======================================================================== */

undefined8 FUN_18000cc30(void)

{
  int iVar1;
  undefined8 local_res8;
  
  iVar1 = FUN_180020730();
  if (iVar1 < 2) {
    return DAT_180023d28;
  }
  if (iVar1 == 2) {
    return DAT_180023d38;
  }
  if (2 < iVar1) {
    return DAT_180023d20;
  }
  return local_res8;
}



/* ========================================================================
   ENTRY: 18000cc80
   NAME : FUN_18000cc80
   SIG  : double __fastcall FUN_18000cc80(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

double FUN_18000cc80(void)

{
  DWORD DVar1;
  int iVar2;
  double dVar3;
  undefined1 auStack_58 [32];
  CHAR local_38 [32];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_58;
  DVar1 = GetEnvironmentVariableA("PA_MIN_LATENCY_MSEC",local_38,0x20);
  if (DVar1 - 1 < 0x1f) {
    iVar2 = atoi(local_38);
    return (double)iVar2 * _DAT_180023cb0;
  }
  dVar3 = (double)FUN_18000cc30();
  return dVar3;
}



/* ========================================================================
   ENTRY: 18000cd00
   NAME : FUN_18000cd00
   SIG  : ulonglong __fastcall FUN_18000cd00(longlong * param_1, undefined4 param_2)
   ======================================================================== */

ulonglong FUN_18000cd00(longlong *param_1,undefined4 param_2)

{
  uint uVar1;
  longlong lVar2;
  ulonglong uVar3;
  undefined4 *puVar4;
  longlong lVar5;
  longlong lVar6;
  ulonglong uVar7;
  ulonglong uVar8;
  int iVar9;
  ulonglong uVar10;
  longlong local_88;
  undefined8 local_80;
  uint local_78;
  int local_74;
  longlong local_68;
  undefined8 local_60;
  uint local_58;
  int local_54;
  longlong local_48;
  
  FUN_18000ddb0();
  uVar10 = 0;
  local_88 = 0;
  local_68 = 0;
  local_48 = 0;
  lVar2 = FUN_180020230(0x118);
  if (lVar2 != 0) {
    uVar3 = FUN_180020180(1,(undefined4 *)(lVar2 + 0x110));
    uVar8 = uVar3 & 0xffffffff;
    if ((int)uVar3 != 0) goto LAB_18000cd49;
    puVar4 = FUN_1800011b0();
    *(undefined4 **)(lVar2 + 0x108) = puVar4;
    if (puVar4 != (undefined4 *)0x0) {
      *param_1 = lVar2;
      *(undefined4 *)(lVar2 + 8) = 1;
      *(undefined4 *)(*param_1 + 0xc) = 1;
      *(char **)(*param_1 + 0x10) = "Windows DirectSound";
      *(undefined4 *)(*param_1 + 0x18) = 0;
      *(undefined4 *)(*param_1 + 0x1c) = 0xffffffff;
      *(undefined4 *)(*param_1 + 0x20) = 0xffffffff;
      uVar1 = FUN_18000c2d0(&local_80,*(undefined8 *)(lVar2 + 0x108));
      uVar8 = (ulonglong)uVar1;
      if (uVar1 != 0) goto LAB_18000cd49;
      uVar1 = FUN_18000c2d0(&local_60,*(undefined8 *)(lVar2 + 0x108));
      uVar8 = (ulonglong)uVar1;
      if (uVar1 != 0) goto LAB_18000cd49;
      (*DAT_18002bb20)(FUN_18000bbb0,&local_80);
      (*DAT_18002bb08)(FUN_18000bbb0,&local_60);
      uVar8 = (ulonglong)local_78;
      if ((local_78 != 0) || (uVar8 = (ulonglong)local_58, local_58 != 0)) goto LAB_18000cd49;
      iVar9 = local_54 + local_74;
      if (0 < iVar9) {
        local_88 = lVar2;
        FUN_18000bea0(&local_88);
        lVar6 = *param_1;
        lVar5 = FUN_1800012e0(*(int **)(lVar2 + 0x108),iVar9 * 8);
        *(longlong *)(lVar6 + 0x28) = lVar5;
        if ((*(longlong *)(*param_1 + 0x28) == 0) ||
           (lVar6 = FUN_1800012e0(*(int **)(lVar2 + 0x108),iVar9 * 0x80), uVar3 = uVar10, lVar6 == 0
           )) goto LAB_18000cd44;
        do {
          puVar4 = (undefined4 *)(uVar3 * 0x80 + lVar6);
          uVar1 = (int)uVar3 + 1;
          *puVar4 = 2;
          puVar4[4] = param_2;
          *(undefined8 *)(puVar4 + 2) = 0;
          *(undefined4 **)(*(longlong *)(*param_1 + 0x28) + uVar3 * 8) = puVar4;
          uVar3 = (ulonglong)uVar1;
        } while ((int)uVar1 < iVar9);
        uVar3 = uVar10;
        if (0 < local_74) {
          do {
            lVar6 = (longlong)(int)uVar3;
            uVar7 = FUN_18000b3a0(lVar2,*(undefined8 *)(local_68 + lVar6 * 0x28),
                                  *(undefined4 **)(local_68 + 0x18 + lVar6 * 0x28),
                                  *(LPCWSTR *)(local_68 + 0x20 + lVar6 * 0x28));
            uVar8 = uVar7 & 0xffffffff;
            if ((int)uVar7 != 0) goto LAB_18000cd49;
            uVar1 = (int)uVar3 + 1;
            uVar3 = (ulonglong)uVar1;
          } while ((int)uVar1 < local_74);
        }
        if (0 < local_54) {
          do {
            lVar6 = (longlong)(int)uVar10;
            uVar3 = FUN_18000b590(lVar2,*(undefined8 *)(local_48 + lVar6 * 0x28),
                                  *(longlong **)(local_48 + 0x18 + lVar6 * 0x28),
                                  *(LPCWSTR *)(local_48 + 0x20 + lVar6 * 0x28));
            uVar8 = uVar3 & 0xffffffff;
            if ((int)uVar3 != 0) goto LAB_18000cd49;
            uVar1 = (int)uVar10 + 1;
            uVar10 = (ulonglong)uVar1;
          } while ((int)uVar1 < local_54);
        }
      }
      uVar1 = FUN_18000d720((longlong)&local_80);
      uVar8 = (ulonglong)uVar1;
      if (uVar1 == 0) {
        uVar1 = FUN_18000d720((longlong)&local_60);
        uVar8 = (ulonglong)uVar1;
        if (uVar1 == 0) {
          *(code **)(*param_1 + 0x30) = FUN_18000d6d0;
          *(code **)(*param_1 + 0x38) = FUN_18000c500;
          *(code **)(*param_1 + 0x40) = FUN_18000c320;
          FUN_180006e10((undefined8 *)(lVar2 + 0x48),FUN_18000bac0,FUN_18000d380,FUN_18000d5c0,
                        &LAB_18000b390,&LAB_18000c3d0,&LAB_18000c3c0,&LAB_18000bf70,&LAB_180008780,
                        &LAB_180006df0,&LAB_180006e00,&LAB_180006df0,&LAB_180006e00);
          FUN_180006e10((undefined8 *)(lVar2 + 0xa8),FUN_18000bac0,FUN_18000d380,FUN_18000d5c0,
                        &LAB_18000b390,&LAB_18000c3d0,&LAB_18000c3c0,&LAB_18000bf70,&LAB_180006de0,
                        FUN_18000bf60,FUN_18000bf60,FUN_18000bf60,FUN_18000bf60);
          return 0;
        }
      }
      goto LAB_18000cd49;
    }
  }
LAB_18000cd44:
  uVar8 = 0xffffd8f8;
LAB_18000cd49:
  FUN_18000d720((longlong)&local_80);
  FUN_18000d720((longlong)&local_60);
  FUN_18000d6d0(lVar2);
  return uVar8;
}



/* ========================================================================
   ENTRY: 18000d100
   NAME : FUN_18000d100
   SIG  : undefined8 __fastcall FUN_18000d100(LPVOID param_1)
   ======================================================================== */

undefined8 FUN_18000d100(LPVOID param_1)

{
  BOOL BVar1;
  DWORD DVar2;
  int lPeriod;
  LARGE_INTEGER local_res8;
  
  lPeriod = (int)(*(double *)((longlong)param_1 + 0x1e8) * DAT_1800239c8);
  if (lPeriod < 1) {
    lPeriod = 1;
  }
  local_res8.s.LowPart = lPeriod * 10000;
  local_res8.s.HighPart = 0;
  BVar1 = SetWaitableTimer(*(HANDLE *)((longlong)param_1 + 0x220),&local_res8,lPeriod,FUN_18000dc50,
                           param_1,0);
  if (BVar1 != 0) {
    do {
      do {
        DVar2 = WaitForSingleObjectEx(*(HANDLE *)((longlong)param_1 + 0x200),lPeriod * 10,1);
      } while (DVar2 == 0x102);
    } while (DVar2 == 0xc0);
  }
  CancelWaitableTimer(*(HANDLE *)((longlong)param_1 + 0x220));
  return 0;
}



/* ========================================================================
   ENTRY: 18000d1b0
   NAME : FUN_18000d1b0
   SIG  : undefined8 __fastcall FUN_18000d1b0(longlong param_1, int * param_2)
   ======================================================================== */

undefined8 FUN_18000d1b0(longlong param_1,int *param_2)

{
  longlong lVar1;
  undefined8 uVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int local_res8 [4];
  int local_res18 [2];
  LARGE_INTEGER local_res20;
  
  uVar2 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x20))
                    (*(longlong **)(param_1 + 0x180),local_res8,local_res18);
  if ((int)uVar2 == 0) {
    iVar4 = local_res18[0] - local_res8[0];
    if (iVar4 < 0) {
      iVar4 = iVar4 + *(int *)(param_1 + 0x18c);
    }
    iVar3 = *(int *)(param_1 + 0x18c);
    if ((*(int *)(param_1 + 0x1b0) != 0) && (*(longlong *)(param_1 + 0x198) != 0)) {
      QueryPerformanceCounter(&local_res20);
      lVar1 = *(longlong *)(param_1 + 0x1a0);
      iVar5 = local_res8[0] - *(int *)(param_1 + 0x1a8);
      ((LARGE_INTEGER *)(param_1 + 0x1a0))->QuadPart = (LONGLONG)local_res20;
      if (iVar5 < 0) {
        iVar5 = iVar5 + *(int *)(param_1 + 0x18c);
      }
      iVar3 = *(int *)(param_1 + 0x18c);
      *(int *)(param_1 + 0x1a8) = local_res8[0];
      iVar5 = ((int)(((longlong)iVar3 * (local_res20.QuadPart - lVar1)) /
                    *(longlong *)(param_1 + 0x198)) - iVar5) / iVar3;
      if (0 < iVar5) {
        local_res8[0] = local_res8[0] + iVar5 * iVar3;
      }
    }
    local_res8[0] = local_res8[0] - *(int *)(param_1 + 0x188);
    iVar5 = iVar3 + local_res8[0];
    if (-1 < local_res8[0]) {
      iVar5 = local_res8[0];
    }
    if (iVar3 - iVar4 < iVar5) {
      if (*(int *)(param_1 + 0x1b0) != 0) {
        *(int *)(param_1 + 0x1ac) = *(int *)(param_1 + 0x1ac) + 1;
      }
      *(int *)(param_1 + 0x188) = local_res18[0];
      *param_2 = iVar3 - iVar4;
      return 0;
    }
    *param_2 = iVar5;
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000d2d0
   NAME : FUN_18000d2d0
   SIG  : undefined __fastcall FUN_18000d2d0(longlong param_1, int param_2, int param_3, double param_4)
   ======================================================================== */

void FUN_18000d2d0(longlong param_1,int param_2,int param_3,double param_4)

{
  int iVar1;
  int iVar2;
  double dVar3;
  
  if (param_2 == 0) {
    param_2 = param_3;
  }
  if (*(int *)(param_1 + 0x84) == 0) {
    dVar3 = 0.0;
  }
  else {
    iVar1 = FUN_180006270(param_1 + 0x68);
    dVar3 = (double)(uint)(iVar1 + param_2) / param_4;
  }
  *(double *)(param_1 + 0x38) = dVar3;
  if (*(int *)(param_1 + 0xa8) == 0) {
    *(undefined8 *)(param_1 + 0x40) = 0;
  }
  else {
    iVar1 = *(int *)(param_1 + 0x1d4);
    iVar2 = FUN_180006280(param_1 + 0x68);
    *(double *)(param_1 + 0x40) = (double)(uint)(iVar2 + (iVar1 - param_2)) / param_4;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000d380
   NAME : FUN_18000d380
   SIG  : undefined8 __fastcall FUN_18000d380(void * param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000d380(void *param_1)

{
  longlong *plVar1;
  DWORD DVar2;
  MMRESULT MVar3;
  BOOL BVar4;
  ulonglong uVar5;
  HANDLE hThread;
  UINT uPeriod;
  timecaps_tag local_res8;
  
  *(undefined4 *)((longlong)param_1 + 0x1f8) = 0;
  FUN_1800066c0((longlong)param_1 + 0x68);
  ResetEvent(*(HANDLE *)((longlong)param_1 + 0x200));
  if (*(int *)((longlong)param_1 + 0x84) != 0) {
    plVar1 = *(longlong **)((longlong)param_1 + 0x1c0);
    DVar2 = local_res8.wPeriodMin;
    if (plVar1 != (longlong *)0x0) {
      DVar2 = (**(code **)(*plVar1 + 0x48))(plVar1,1);
    }
    if (DVar2 != 0) goto LAB_18000d534;
  }
  *(undefined8 *)((longlong)param_1 + 0x1d8) = 0;
  *(undefined4 *)((longlong)param_1 + 0x1f0) = 0;
  *(undefined4 *)((longlong)param_1 + 0x214) = 0;
  *(undefined4 *)((longlong)param_1 + 0x210) = 0;
  if (*(int *)((longlong)param_1 + 0xa8) != 0) {
    QueryPerformanceCounter((LARGE_INTEGER *)((longlong)param_1 + 0x1a0));
    *(undefined4 *)((longlong)param_1 + 0x1b4) = 0;
    uVar5 = FUN_18000b9f0((longlong)param_1);
    DVar2 = (DWORD)uVar5;
    if (DVar2 != 0) goto LAB_18000d534;
    if ((*(longlong *)((longlong)param_1 + 0x18) != 0) &&
       ((*(byte *)((longlong)param_1 + 500) & 8) != 0)) {
      *(undefined4 *)((longlong)param_1 + 0x1f0) = 0x10;
      FUN_18000d760((longlong)param_1);
      *(undefined4 *)((longlong)param_1 + 0x1f0) = 0;
    }
    plVar1 = *(longlong **)((longlong)param_1 + 0x180);
    if (plVar1 != (longlong *)0x0) {
      DVar2 = (**(code **)(*plVar1 + 0x60))(plVar1,0,0,1);
      if (DVar2 != 0) goto LAB_18000d534;
      *(undefined4 *)((longlong)param_1 + 0x1b0) = 1;
    }
  }
  if (*(longlong *)((longlong)param_1 + 0x18) != 0) {
    MVar3 = timeGetDevCaps(&local_res8,8);
    if ((MVar3 == 0) && (local_res8.wPeriodMin != 0)) {
      uPeriod = (UINT)(longlong)
                      (*(double *)((longlong)param_1 + 0x1e8) * DAT_1800239c8 * _DAT_180023d30);
      *(UINT *)((longlong)param_1 + 0x218) = uPeriod;
      if (uPeriod < local_res8.wPeriodMin) {
        *(UINT *)((longlong)param_1 + 0x218) = local_res8.wPeriodMin;
        uPeriod = local_res8.wPeriodMin;
      }
      if (local_res8.wPeriodMax < uPeriod) {
        *(UINT *)((longlong)param_1 + 0x218) = local_res8.wPeriodMax;
        uPeriod = local_res8.wPeriodMax;
      }
      MVar3 = timeBeginPeriod(uPeriod);
      if (MVar3 != 0) {
        *(undefined4 *)((longlong)param_1 + 0x218) = 0;
      }
    }
    hThread = (HANDLE)_beginthreadex((void *)0x0,0,FUN_18000d100,param_1,0,
                                     (uint *)((longlong)param_1 + 0x230));
    *(HANDLE *)((longlong)param_1 + 0x228) = hThread;
    if ((hThread == (HANDLE)0x0) || (BVar4 = SetThreadPriority(hThread,0xf), BVar4 == 0)) {
      DVar2 = GetLastError();
LAB_18000d534:
      FUN_180003c90(1,DVar2,"DirectSound error");
      if ((*(longlong **)((longlong)param_1 + 0x180) != (longlong *)0x0) &&
         (*(int *)((longlong)param_1 + 0x1b0) != 0)) {
        (**(code **)(**(longlong **)((longlong)param_1 + 0x180) + 0x90))();
      }
      *(undefined4 *)((longlong)param_1 + 0x1b0) = 0;
      if (*(HANDLE *)((longlong)param_1 + 0x228) != (HANDLE)0x0) {
        CloseHandle(*(HANDLE *)((longlong)param_1 + 0x228));
        *(undefined8 *)((longlong)param_1 + 0x228) = 0;
      }
      return 0xffffd8f1;
    }
  }
  *(undefined4 *)((longlong)param_1 + 0x20c) = 1;
  *(undefined4 *)((longlong)param_1 + 0x208) = 1;
  return 0;
}



/* ========================================================================
   ENTRY: 18000d5c0
   NAME : FUN_18000d5c0
   SIG  : undefined8 __fastcall FUN_18000d5c0(longlong param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000d5c0(longlong param_1)

{
  DWORD DVar1;
  
  if (*(longlong *)(param_1 + 0x18) != 0) {
    *(undefined4 *)(param_1 + 0x210) = 1;
    WaitForSingleObject(*(HANDLE *)(param_1 + 0x200),
                        (int)(((double)*(int *)(param_1 + 0x1d4) / *(double *)(param_1 + 0x48)) *
                             _DAT_180023d40));
  }
  if (*(HANDLE *)(param_1 + 0x228) != (HANDLE)0x0) {
    DVar1 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x228),3000);
    if (DVar1 == 0x102) {
      return 0xffffd8f1;
    }
    CloseHandle(*(HANDLE *)(param_1 + 0x228));
    *(undefined8 *)(param_1 + 0x228) = 0;
  }
  if (*(UINT *)(param_1 + 0x218) != 0) {
    timeEndPeriod(*(UINT *)(param_1 + 0x218));
    *(undefined4 *)(param_1 + 0x218) = 0;
  }
  if ((*(int *)(param_1 + 0xa8) != 0) && (*(longlong **)(param_1 + 0x180) != (longlong *)0x0)) {
    *(undefined4 *)(param_1 + 0x1b0) = 0;
    (**(code **)(**(longlong **)(param_1 + 0x180) + 0x90))();
    if (*(longlong **)(param_1 + 0x178) != (longlong *)0x0) {
      (**(code **)(**(longlong **)(param_1 + 0x178) + 0x90))();
    }
  }
  if ((*(int *)(param_1 + 0x84) != 0) && (*(longlong **)(param_1 + 0x1c0) != (longlong *)0x0)) {
    (**(code **)(**(longlong **)(param_1 + 0x1c0) + 0x50))();
  }
  *(undefined4 *)(param_1 + 0x208) = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 18000d6d0
   NAME : FUN_18000d6d0
   SIG  : undefined __fastcall FUN_18000d6d0(longlong param_1)
   ======================================================================== */

void FUN_18000d6d0(longlong param_1)

{
  if (param_1 != 0) {
    if (*(longlong *)(param_1 + 0x108) != 0) {
      FUN_180001280(*(longlong *)(param_1 + 0x108));
      FUN_180001230(*(longlong *)(param_1 + 0x108));
    }
    FUN_180020200(1,(int *)(param_1 + 0x110));
    FUN_180020240(param_1);
  }
  FUN_18000df80();
  return;
}



/* ========================================================================
   ENTRY: 18000d720
   NAME : FUN_18000d720
   SIG  : uint __fastcall FUN_18000d720(longlong param_1)
   ======================================================================== */

uint FUN_18000d720(longlong param_1)

{
  uint uVar1;
  HLOCAL pvVar2;
  
  uVar1 = 0;
  if (*(HLOCAL *)(param_1 + 0x18) != (HLOCAL)0x0) {
    pvVar2 = LocalFree(*(HLOCAL *)(param_1 + 0x18));
    *(undefined8 *)(param_1 + 0x18) = 0;
    uVar1 = -(uint)(pvVar2 != (HLOCAL)0x0) & 0xffffd8f8;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18000d760
   NAME : FUN_18000d760
   SIG  : ulonglong __fastcall FUN_18000d760(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18000d760(longlong param_1)

{
  int iVar1;
  bool bVar2;
  int iVar3;
  uint uVar4;
  undefined7 extraout_var;
  uint uVar5;
  int iVar6;
  int iVar7;
  double dVar8;
  undefined1 auStack_d8 [32];
  uint *local_b8;
  longlong *local_b0;
  uint *local_a8;
  undefined4 local_a0;
  uint local_98;
  uint local_94;
  uint local_90;
  uint local_8c;
  int local_88;
  int local_84;
  longlong local_80;
  longlong local_78;
  longlong local_70;
  longlong local_68;
  undefined1 local_60 [8];
  double local_58;
  double dStack_50;
  double local_48;
  ulonglong local_40;
  
  local_40 = DAT_18002b580 ^ (ulonglong)auStack_d8;
  iVar3 = 0;
  uVar4 = 0;
  iVar6 = 0;
  dVar8 = 0.0;
  local_48 = 0.0;
  local_88 = 0;
  local_58 = 0.0;
  dStack_50 = 0.0;
  local_68 = 0;
  local_70 = 0;
  local_8c = 0;
  local_90 = 0;
  local_78 = 0;
  local_80 = 0;
  local_94 = 0;
  local_98 = 0;
  if (*(int *)(param_1 + 0x84) != 0) {
    iVar3 = (**(code **)(**(longlong **)(param_1 + 0x1c0) + 0x20))
                      (*(longlong **)(param_1 + 0x1c0),local_60,&local_84);
    dVar8 = 0.0;
    uVar5 = uVar4;
    if (iVar3 == 0) {
      uVar5 = local_84 - *(int *)(param_1 + 0x1cc);
      if ((int)uVar5 < 0) {
        uVar5 = uVar5 + *(int *)(param_1 + 0x1d0);
      }
      dVar8 = (double)(int)uVar5 * *(double *)(param_1 + 0x1e0);
    }
    iVar3 = (int)uVar5 / *(int *)(param_1 + 0x1c8);
  }
  iVar7 = iVar3;
  if (*(int *)(param_1 + 0xa8) != 0) {
    iVar1 = *(int *)(param_1 + 0x1ac);
    FUN_18000d1b0(param_1,&local_88);
    iVar6 = local_88 / *(int *)(param_1 + 400);
    iVar7 = iVar6;
    if (*(int *)(param_1 + 0x1ac) != iVar1) {
      *(uint *)(param_1 + 0x1f0) = *(uint *)(param_1 + 0x1f0) | 4;
    }
  }
  if (((*(int *)(param_1 + 0x84) != 0) && (*(int *)(param_1 + 0xa8) != 0)) &&
     (iVar7 = iVar3, iVar6 < iVar3)) {
    iVar7 = iVar6;
  }
  if (iVar7 < 1) goto LAB_18000db12;
  FUN_1800038c0(param_1 + 0x50);
  dStack_50 = FUN_180020250();
  FUN_180005ce0(param_1 + 0x68,&local_58,*(undefined4 *)(param_1 + 0x1f0));
  *(undefined4 *)(param_1 + 0x1f0) = 0;
  uVar5 = 0;
  if (*(int *)(param_1 + 0x84) == 0) {
LAB_18000d987:
    uVar4 = uVar5;
    if (*(int *)(param_1 + 0xa8) == 0) {
LAB_18000da4b:
      uVar4 = FUN_180006010((uint *)(param_1 + 0x68),(int *)(param_1 + 0x1f8));
      *(double *)(param_1 + 0x1d8) = (double)(int)uVar4 + *(double *)(param_1 + 0x1d8);
      if (*(int *)(param_1 + 0xa8) != 0) {
        *(uint *)(param_1 + 0x188) =
             (uVar4 * *(int *)(param_1 + 400) + *(int *)(param_1 + 0x188)) %
             *(uint *)(param_1 + 0x18c);
        local_b8 = (uint *)CONCAT44(local_b8._4_4_,local_98);
        (**(code **)(**(longlong **)(param_1 + 0x180) + 0x98))
                  (*(longlong **)(param_1 + 0x180),local_78,local_94,local_80);
      }
    }
    else {
      local_a8 = &local_98;
      local_a0 = 0;
      local_b0 = &local_80;
      local_b8 = &local_94;
      local_48 = dStack_50;
      iVar3 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x58))
                        (*(longlong **)(param_1 + 0x180),*(undefined4 *)(param_1 + 0x188),
                         iVar7 * *(int *)(param_1 + 400));
      if (iVar3 == 0) {
        FUN_1800069a0(param_1 + 0x68,local_94 / *(uint *)(param_1 + 400));
        FUN_1800068d0(param_1 + 0x68,0,local_78,0);
        if (local_98 != 0) {
          FUN_180006830(param_1 + 0x68,local_98 / *(uint *)(param_1 + 400));
          FUN_1800067b0(param_1 + 0x68,0,local_80,0);
        }
        goto LAB_18000da4b;
      }
      FUN_1800066c0(param_1 + 0x68);
      *(undefined4 *)(param_1 + 0x1f8) = 1;
    }
    if (*(int *)(param_1 + 0x84) != 0) {
      *(uint *)(param_1 + 0x1cc) =
           (uVar4 * *(int *)(param_1 + 0x1c8) + *(int *)(param_1 + 0x1cc)) %
           *(uint *)(param_1 + 0x1d0);
      local_b8 = (uint *)CONCAT44(local_b8._4_4_,local_90);
      (**(code **)(**(longlong **)(param_1 + 0x1c0) + 0x58))
                (*(longlong **)(param_1 + 0x1c0),local_68,local_8c,local_70);
    }
  }
  else {
    local_a8 = &local_90;
    local_58 = dStack_50 - dVar8;
    local_a0 = 0;
    local_b0 = &local_70;
    local_b8 = &local_8c;
    iVar3 = (**(code **)(**(longlong **)(param_1 + 0x1c0) + 0x40))
                      (*(longlong **)(param_1 + 0x1c0),*(undefined4 *)(param_1 + 0x1cc),
                       iVar7 * *(int *)(param_1 + 0x1c8));
    if (iVar3 == 0) {
      uVar5 = local_8c / *(uint *)(param_1 + 0x1c8);
      FUN_180006860(param_1 + 0x68,uVar5);
      FUN_180006880(param_1 + 0x68,0,local_68,0);
      if (local_90 != 0) {
        uVar5 = local_90 / *(uint *)(param_1 + 0x1c8);
        FUN_180006750(param_1 + 0x68,uVar5);
        FUN_180006760(param_1 + 0x68,0,local_70,0);
      }
      goto LAB_18000d987;
    }
    FUN_1800066c0(param_1 + 0x68);
    *(undefined4 *)(param_1 + 0x1f8) = 1;
  }
  FUN_1800038e0((double *)(param_1 + 0x50),uVar4);
LAB_18000db12:
  if (*(int *)(param_1 + 0x1f8) == 1) {
    bVar2 = FUN_1800066b0(param_1 + 0x68);
    if ((int)CONCAT71(extraout_var,bVar2) == 0) {
      return CONCAT71(extraout_var,bVar2);
    }
  }
  return (ulonglong)*(uint *)(param_1 + 0x1f8);
}



/* ========================================================================
   ENTRY: 18000db80
   NAME : FUN_18000db80
   SIG  : undefined __fastcall FUN_18000db80(undefined8 param_1, undefined8 param_2, longlong param_3)
   ======================================================================== */

void FUN_18000db80(undefined8 param_1,undefined8 param_2,longlong param_3)

{
  ulonglong uVar1;
  
  if (param_3 == 0) {
    return;
  }
  if (*(int *)(param_3 + 0x20c) == 0) {
    return;
  }
  if (*(int *)(param_3 + 0x214) == 0) {
    if (*(int *)(param_3 + 0x210) == 0) {
      uVar1 = FUN_18000d760(param_3);
      if ((int)uVar1 == 0) {
        return;
      }
      *(undefined4 *)(param_3 + 0x210) = 1;
      return;
    }
    if ((*(int *)(param_3 + 0xa8) != 0) &&
       (FUN_18000dc70(param_3), *(int *)(param_3 + 0x1b4) < *(int *)(param_3 + 0x18c))) {
      return;
    }
  }
  if (*(code **)(param_3 + 0x20) != (code *)0x0) {
    (**(code **)(param_3 + 0x20))(*(undefined8 *)(param_3 + 0x28));
  }
  *(undefined4 *)(param_3 + 0x20c) = 0;
  SetEvent(*(HANDLE *)(param_3 + 0x200));
  return;
}



/* ========================================================================
   ENTRY: 18000dc20
   NAME : FUN_18000dc20
   SIG  : undefined8 __fastcall FUN_18000dc20(undefined8 param_1, int * param_2)
   ======================================================================== */

undefined8 FUN_18000dc20(undefined8 param_1,int *param_2)

{
  if ((param_2 != (int *)0x0) &&
     (((*param_2 != 0x18 || (param_2[2] != 2)) ||
      (((*(byte *)(param_2 + 3) & 1) != 0 && (param_2[4] == 0)))))) {
    return 0xffffd900;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000dc50
   NAME : FUN_18000dc50
   SIG  : undefined __fastcall FUN_18000dc50(longlong param_1)
   ======================================================================== */

void FUN_18000dc50(longlong param_1)

{
  FUN_18000db80(0,0,param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000dc70
   NAME : FUN_18000dc70
   SIG  : ulonglong __fastcall FUN_18000dc70(longlong param_1)
   ======================================================================== */

ulonglong FUN_18000dc70(longlong param_1)

{
  uint uVar1;
  ulonglong uVar2;
  uint local_res18 [2];
  int local_res20 [2];
  void **ppvVar3;
  uint *puVar4;
  undefined4 uVar5;
  void *local_18;
  void *local_10;
  
  local_10 = (void *)0x0;
  local_18 = (void *)0x0;
  local_res18[0] = 0;
  uVar2 = FUN_18000d1b0(param_1,local_res20);
  if ((int)uVar2 == 0) {
    if (local_res20[0] == 0) {
      return uVar2;
    }
    puVar4 = local_res18;
    uVar5 = 0;
    ppvVar3 = &local_18;
    uVar1 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x58))();
    uVar2 = (ulonglong)uVar1;
    if (uVar1 == 0) {
      memset(local_10,0,0);
      if (local_18 != (void *)0x0) {
        memset(local_18,0,(ulonglong)local_res18[0]);
      }
      *(uint *)(param_1 + 0x188) =
           (*(int *)(param_1 + 0x188) + local_res18[0]) % *(uint *)(param_1 + 0x18c);
      (**(code **)(**(longlong **)(param_1 + 0x180) + 0x98))
                (*(longlong **)(param_1 + 0x180),local_10,0,local_18,local_res18[0],ppvVar3,puVar4,
                 uVar5);
      *(int *)(param_1 + 0x1b4) = *(int *)(param_1 + 0x1b4) + local_res18[0];
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000ddb0
   NAME : FUN_18000ddb0
   SIG  : undefined __fastcall FUN_18000ddb0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000ddb0(void)

{
  DAT_18002baf0 = LoadLibraryA("dsound.dll");
  if (DAT_18002baf0 == (HMODULE)0x0) {
    GetLastError();
    DAT_18002bb00 = (FARPROC)&LAB_18000dd90;
    DAT_18002bb08 = (FARPROC)&LAB_18000dd90;
    _DAT_18002bb10 = (FARPROC)&LAB_18000dd90;
    DAT_18002bb18 = (FARPROC)&LAB_18000dd90;
    DAT_18002bb20 = (FARPROC)&LAB_18000dd90;
    _DAT_18002bb28 = (FARPROC)&LAB_18000dd90;
  }
  else {
    DAT_18002baf8 = GetProcAddress(DAT_18002baf0,"DllGetClassObject");
    if (DAT_18002baf8 == (FARPROC)0x0) {
      DAT_18002baf8 = (FARPROC)&LAB_18000dda0;
    }
    DAT_18002bb00 = GetProcAddress(DAT_18002baf0,"DirectSoundCreate");
    if (DAT_18002bb00 == (FARPROC)0x0) {
      DAT_18002bb00 = (FARPROC)&LAB_18000dd90;
    }
    DAT_18002bb08 = GetProcAddress(DAT_18002baf0,"DirectSoundEnumerateW");
    if (DAT_18002bb08 == (FARPROC)0x0) {
      DAT_18002bb08 = (FARPROC)&LAB_18000dd90;
    }
    _DAT_18002bb10 = GetProcAddress(DAT_18002baf0,"DirectSoundEnumerateA");
    if (_DAT_18002bb10 == (FARPROC)0x0) {
      _DAT_18002bb10 = (FARPROC)&LAB_18000dd90;
    }
    DAT_18002bb18 = GetProcAddress(DAT_18002baf0,"DirectSoundCaptureCreate");
    if (DAT_18002bb18 == (FARPROC)0x0) {
      DAT_18002bb18 = (FARPROC)&LAB_18000dd90;
    }
    DAT_18002bb20 = GetProcAddress(DAT_18002baf0,"DirectSoundCaptureEnumerateW");
    if (DAT_18002bb20 == (FARPROC)0x0) {
      DAT_18002bb20 = (FARPROC)&LAB_18000dd90;
    }
    _DAT_18002bb28 = GetProcAddress(DAT_18002baf0,"DirectSoundCaptureEnumerateA");
    if (_DAT_18002bb28 == (FARPROC)0x0) {
      _DAT_18002bb28 = (FARPROC)&LAB_18000dd90;
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000df80
   NAME : FUN_18000df80
   SIG  : undefined __fastcall FUN_18000df80(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000df80(void)

{
  if (DAT_18002baf0 != (HMODULE)0x0) {
    DAT_18002bb00 = 0;
    DAT_18002bb08 = 0;
    _DAT_18002bb10 = 0;
    DAT_18002bb18 = 0;
    DAT_18002bb20 = 0;
    _DAT_18002bb28 = 0;
    FreeLibrary(DAT_18002baf0);
    DAT_18002baf0 = (HMODULE)0x0;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000dfe0
   NAME : FUN_18000dfe0
   SIG  : uint __fastcall FUN_18000dfe0(uint param_1, uint param_2)
   ======================================================================== */

uint FUN_18000dfe0(uint param_1,uint param_2)

{
  if (param_2 != 0) {
    return param_1 - param_1 % param_2;
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 18000e000
   NAME : FUN_18000e000
   SIG  : uint __fastcall FUN_18000e000(uint param_1, uint param_2)
   ======================================================================== */

uint FUN_18000e000(uint param_1,uint param_2)

{
  if ((param_2 != 0) && (param_1 % param_2 != 0)) {
    return param_2 + (param_1 - param_1 % param_2);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 18000e020
   NAME : FUN_18000e020
   SIG  : undefined __fastcall FUN_18000e020(uint param_1)
   ======================================================================== */

void FUN_18000e020(uint param_1)

{
  uint uVar1;
  
  uVar1 = 1;
  do {
    uVar1 = uVar1 * 2;
  } while (uVar1 < param_1);
  return;
}



/* ========================================================================
   ENTRY: 18000e030
   NAME : FUN_18000e030
   SIG  : undefined8 __fastcall FUN_18000e030(longlong param_1)
   ======================================================================== */

undefined8 FUN_18000e030(longlong param_1)

{
  FUN_180013370(param_1);
  return 0;
}



/* ========================================================================
   ENTRY: 18000e040
   NAME : FUN_18000e040
   SIG  : undefined4 __fastcall FUN_18000e040(longlong param_1, longlong param_2, int param_3)
   ======================================================================== */

undefined4 FUN_18000e040(longlong param_1,longlong param_2,int param_3)

{
  int extraout_EAX;
  int iVar1;
  undefined8 uVar2;
  longlong lVar3;
  uint uVar4;
  longlong lVar5;
  undefined8 *puVar6;
  int iVar7;
  ulonglong uVar8;
  bool bVar9;
  uint local_res18 [2];
  int local_res20 [2];
  
  lVar5 = 0x2c8;
  if (param_3 == 0) {
    lVar5 = 0x170;
  }
  puVar6 = (undefined8 *)(lVar5 + param_2);
  uVar8 = FUN_18000e690(param_1,param_2,puVar6,param_3,local_res20);
  if (extraout_EAX < 0) {
    return local_res20[0];
  }
  iVar1 = (**(code **)(*(longlong *)*puVar6 + 0x20))((longlong *)*puVar6,local_res18);
  if (-1 < iVar1) {
    iVar7 = (int)uVar8;
    lVar5 = 0x314;
    if (iVar7 == 0) {
      lVar5 = 0x1bc;
    }
    *(uint *)(lVar5 + param_2) = local_res18[0];
    lVar5 = 0x3f0;
    if (iVar7 == 0) {
      lVar5 = 0x298;
    }
    if (*(longlong *)(lVar5 + param_2) != 0) {
      uVar2 = FUN_180012dc0((longlong)puVar6,local_res18[0]);
      if ((int)uVar2 != 0) {
        FUN_180014510((int)uVar2);
        return 0xffffd8f8;
      }
    }
    if ((iVar7 != 0) ||
       (iVar1 = (**(code **)(**(longlong **)(param_2 + 0x170) + 0x28))
                          (*(longlong **)(param_2 + 0x170),param_2 + 0x1c0), -1 < iVar1)) {
      lVar5 = 0x330;
      if (iVar7 == 0) {
        lVar5 = 0x1d8;
      }
      *(uint *)(lVar5 + param_2) = local_res18[0];
      lVar5 = 0x3d8;
      if (iVar7 == 0) {
        lVar5 = 0x280;
      }
      uVar4 = local_res18[0];
      if (*(int *)(lVar5 + param_2) == 0) {
        lVar5 = 0x3b0;
        if (iVar7 == 0) {
          lVar5 = 600;
        }
        uVar4 = *(uint *)(lVar5 + param_2);
      }
      bVar9 = iVar7 == 0;
      lVar5 = 0x3d4;
      if (bVar9) {
        lVar5 = 0x27c;
      }
      *(uint *)(lVar5 + param_2) = uVar4;
      lVar5 = 0x2e4;
      if (bVar9) {
        lVar5 = 0x18c;
      }
      lVar3 = 0x328;
      if (bVar9) {
        lVar3 = 0x1d0;
      }
      *(double *)(lVar3 + param_2) = (double)local_res18[0] / (double)*(uint *)(lVar5 + param_2);
      return 0;
    }
  }
  FUN_180014510(iVar1);
  return 0xffffd8f4;
}



/* ========================================================================
   ENTRY: 18000e210
   NAME : FUN_18000e210
   SIG  : undefined8 __fastcall FUN_18000e210(undefined8 * param_1, longlong param_2, undefined8 * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18000e210(undefined8 *param_1,longlong param_2,undefined8 *param_3)

{
  char cVar1;
  int iVar2;
  undefined8 uVar3;
  undefined7 extraout_var;
  undefined1 auStack_68 [32];
  undefined8 *local_48;
  undefined8 local_38;
  undefined4 local_30;
  undefined4 local_2c;
  ulonglong local_28;
  
  local_28 = DAT_18002b580 ^ (ulonglong)auStack_68;
  uVar3 = FUN_18000f7b0();
  local_48 = param_3;
  uVar3 = (**(code **)(*(longlong *)*param_1 + 0x18))((longlong *)*param_1,uVar3,0x17,0);
  if (-1 < (int)uVar3) {
    if ((param_2 != 0) && (cVar1 = FUN_18000f810(), 1 < (uint)CONCAT71(extraout_var,cVar1))) {
      local_2c = 0;
      local_30 = *(undefined4 *)(param_2 + 0x2c);
      local_38 = 0x10;
      if (*(int *)(param_2 + 0x30) == 1) {
        iVar2 = FUN_180020730();
        if (8 < iVar2) {
          local_2c = 1;
        }
      }
      else if ((*(int *)(param_2 + 0x30) == 2) && (iVar2 = FUN_180020730(), 9 < iVar2)) {
        local_2c = 2;
      }
      iVar2 = (**(code **)(*(longlong *)*param_3 + 0x80))((longlong *)*param_3,&local_38);
      if (iVar2 < 0) {
        FUN_180014510(iVar2);
      }
    }
    uVar3 = 0;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18000e310
   NAME : FUN_18000e310
   SIG  : undefined __fastcall FUN_18000e310(int param_1, uint param_2, undefined * param_3)
   ======================================================================== */

void FUN_18000e310(int param_1,uint param_2,undefined *param_3)

{
  uint uVar1;
  
  uVar1 = (*(code *)param_3)(param_1 * param_2);
  if (uVar1 < 0x80) {
    uVar1 = 0x80;
  }
  FUN_18000e000((uVar1 & 0xffffff80) / param_2,8);
  return;
}



/* ========================================================================
   ENTRY: 18000e350
   NAME : FUN_18000e350
   SIG  : longlong __fastcall FUN_18000e350(longlong param_1, uint param_2)
   ======================================================================== */

longlong FUN_18000e350(longlong param_1,uint param_2)

{
  longlong *plVar1;
  longlong lVar2;
  longlong *_Dst;
  uint uVar3;
  ulonglong uVar5;
  ulonglong uVar4;
  
  lVar2 = FUN_1800012e0(*(int **)(param_1 + 0x108),param_2 * 0x280);
  *(longlong *)(param_1 + 0x120) = lVar2;
  if (lVar2 == 0) {
    return 0;
  }
  uVar4 = 0;
  if (param_2 == 0) {
    return 0;
  }
  _Dst = (longlong *)FUN_1800012e0(*(int **)(param_1 + 0x108),param_2 * 8);
  *(longlong **)(param_1 + 0x28) = _Dst;
  plVar1 = (longlong *)(param_1 + 0x28);
  if (_Dst == (longlong *)0x0) {
    return 0;
  }
  if (param_2 != 0) {
    uVar5 = uVar4;
    if ((1 < param_2) && ((plVar1 < _Dst || (uVar5 = 0, _Dst + (param_2 - 1) < plVar1)))) {
      do {
        uVar3 = (int)uVar4 + 2;
        uVar4 = (ulonglong)uVar3;
      } while (uVar3 < (param_2 & 0xfffffffe));
      memset(_Dst,0,SUB168((ZEXT816(0) << 0x40 | ZEXT416(param_2 >> 1) << 4) / ZEXT816(8),0) << 3);
      uVar5 = (ulonglong)uVar3;
      if (param_2 <= uVar3) goto LAB_18000e42f;
    }
    do {
      uVar3 = (int)uVar5 + 1;
      *(undefined8 *)(*plVar1 + uVar5 * 8) = 0;
      uVar5 = (ulonglong)uVar3;
    } while (uVar3 < param_2);
  }
LAB_18000e42f:
  lVar2 = FUN_1800012e0(*(int **)(param_1 + 0x108),param_2 * 0x48);
  if (lVar2 == 0) {
    return 0;
  }
  return lVar2;
}



/* ========================================================================
   ENTRY: 18000e460
   NAME : FUN_18000e460
   SIG  : undefined __fastcall FUN_18000e460(void)
   ======================================================================== */

void FUN_18000e460(void)

{
  if (DAT_18002bb60 != (HMODULE)0x0) {
    FreeLibrary(DAT_18002bb60);
  }
  DAT_18002bb60 = (HMODULE)0x0;
  return;
}



/* ========================================================================
   ENTRY: 18000e490
   NAME : FUN_18000e490
   SIG  : ulonglong __fastcall FUN_18000e490(undefined4 * param_1)
   ======================================================================== */

ulonglong FUN_18000e490(undefined4 *param_1)

{
  int iVar1;
  ulonglong uVar2;
  
  uVar2 = 0;
  iVar1 = FUN_180010140((longlong)param_1);
  if (iVar1 != 0) {
    uVar2 = FUN_18000e030((longlong)param_1);
    uVar2 = uVar2 & 0xffffffff;
  }
  if (*(longlong **)(param_1 + 0xac) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0xac) + 0x10))();
    *(undefined8 *)(param_1 + 0xac) = 0;
  }
  if (*(longlong **)(param_1 + 0x102) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x102) + 0x10))();
    *(undefined8 *)(param_1 + 0x102) = 0;
  }
  if (*(longlong **)(param_1 + 0xb2) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0xb2) + 0x10))();
    *(undefined8 *)(param_1 + 0xb2) = 0;
  }
  if (*(longlong **)(param_1 + 0x5c) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x5c) + 0x10))();
    *(undefined8 *)(param_1 + 0x5c) = 0;
  }
  CloseHandle(*(HANDLE *)(param_1 + 0x108));
  CloseHandle(*(HANDLE *)(param_1 + 0x10a));
  FUN_1800143d0((longlong)param_1);
  free(*(void **)(param_1 + 0xa2));
  free(*(void **)(param_1 + 0xf8));
  FUN_180020240(*(longlong *)(param_1 + 0xa8));
  FUN_180020240(*(longlong *)(param_1 + 0xaa));
  FUN_180020240(*(longlong *)(param_1 + 0xfe));
  FUN_180020240(*(longlong *)(param_1 + 0x100));
  FUN_180013440(param_1 + 0x129);
  FUN_1800069c0((longlong)(param_1 + 0x1a));
  FUN_180006ea0(param_1);
  FUN_180020240((longlong)param_1);
  return uVar2;
}



/* ========================================================================
   ENTRY: 18000e5c0
   NAME : FUN_18000e5c0
   SIG  : uint __fastcall FUN_18000e5c0(longlong param_1, uint * param_2)
   ======================================================================== */

uint FUN_18000e5c0(longlong param_1,uint *param_2)

{
  uint uVar1;
  ulonglong uVar2;
  uint uVar3;
  uint uVar4;
  longlong lVar5;
  uint uVar6;
  uint uVar7;
  
  uVar2 = (ulonglong)*(uint *)(param_1 + 0x1d8) / 6;
  if (*(int *)(param_1 + 0x430) == 0) {
    lVar5 = 0x3d4;
  }
  else {
    lVar5 = 0x68;
    if (*(int *)(param_1 + 0x68) == 0) {
      lVar5 = 0x3b0;
    }
  }
  uVar4 = *(uint *)(param_1 + 0x18c);
  uVar7 = *(uint *)(lVar5 + param_1);
  lVar5 = FUN_18000fba0(uVar2,(ulonglong)uVar4);
  uVar1 = *(uint *)(param_1 + 0x2e4);
  uVar3 = (uint)lVar5;
  lVar5 = FUN_18000fba0((ulonglong)uVar7,(ulonglong)uVar1);
  if (2 < uVar3) {
    uVar3 = 2;
  }
  uVar6 = uVar7;
  uVar3 = FUN_18000fde0(param_1,uVar3,(uint)lVar5,uVar7);
  if (uVar3 == 0) {
    lVar5 = FUN_18000fbe0((ulonglong)uVar7,(ulonglong)uVar1);
    uVar7 = (uint)lVar5;
    lVar5 = FUN_18000fbe0(uVar2,(ulonglong)uVar4);
    uVar4 = FUN_18000fde0(param_1,(uint)lVar5,uVar7,uVar6);
    FUN_180013500(param_2,1,uVar4);
    uVar3 = 0;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18000e690
   NAME : FUN_18000e690
   SIG  : ulonglong __fastcall FUN_18000e690(longlong param_1, longlong param_2, undefined8 * param_3, int param_4, int * param_5)
   ======================================================================== */

ulonglong FUN_18000e690(longlong param_1,longlong param_2,undefined8 *param_3,int param_4,
                       int *param_5)

{
  undefined8 *puVar1;
  double dVar2;
  int iVar3;
  uint uVar4;
  undefined8 *puVar5;
  int *piVar6;
  uint uVar7;
  int iVar8;
  undefined8 uVar9;
  undefined1 *puVar10;
  longlong lVar11;
  uint uVar12;
  ulonglong unaff_RDI;
  code *pcVar13;
  uint local_64;
  longlong *local_60;
  longlong local_58 [4];
  
  piVar6 = param_5;
  iVar3 = *(int *)((longlong)param_3 + 0xfc);
  puVar5 = (undefined8 *)param_3[0xf];
  uVar4 = *(uint *)(param_3 + 0x1d);
  dVar2 = (double)param_3[0x1e];
  local_60 = (longlong *)0x0;
  local_58[0] = 0;
  *param_5 = -0x270c;
  if ((puVar5 == (undefined8 *)0x0) || (param_3 + 0x10 == (undefined8 *)0x0)) {
    *param_5 = -0x2704;
    return unaff_RDI;
  }
  if ((int)(longlong)dVar2 == 0) {
    *param_5 = -0x270d;
    return unaff_RDI;
  }
  uVar9 = FUN_18000e210(puVar5,(longlong)(param_3 + 0x14),&local_60);
  iVar8 = (int)uVar9;
  if (iVar8 < 0) {
LAB_18000e743:
    *piVar6 = -0x2708;
  }
  else {
    uVar9 = FUN_18000f840(local_60,param_1,(longlong)puVar5,(undefined4 *)(param_3 + 0x10),dVar2,
                          *(int *)((longlong)param_3 + 0x6c),param_3 + 3,param_4);
    if ((int)uVar9 != 0) {
      *piVar6 = (int)uVar9;
LAB_18000e79e:
      FUN_180014510(-0x7776fff8);
      goto LAB_18000ec7f;
    }
    if ((*(int *)((longlong)param_3 + 0x84) == 1) && (*(short *)((longlong)param_3 + 0x1a) == 2)) {
      iVar8 = 2;
      if (param_4 != 0) {
        iVar8 = 0;
      }
      puVar10 = FUN_18000fc20((short *)(param_3 + 3),iVar8);
      param_3[0x25] = puVar10;
      if (puVar10 == (undefined1 *)0x0) {
        *piVar6 = -0x270e;
        goto LAB_18000e79e;
      }
    }
    iVar8 = *(int *)((longlong)param_3 + 0x6c);
    if ((iVar8 == 1) ||
       ((uVar12 = *(uint *)(param_3 + 0xe), uVar12 != 0 && ((uVar12 >> 0x12 & 1) != 0)))) {
      lVar11 = FUN_180012f60((double)param_3[0x12]);
      lVar11 = FUN_180010310(lVar11,*(uint *)((longlong)param_3 + 0x1c));
      uVar12 = *(uint *)(param_3 + 0xe);
      uVar7 = (int)lVar11 + uVar4;
    }
    else {
      uVar7 = FUN_180013c50(uVar4,(double)param_3[0x12],(double)*(uint *)((longlong)param_3 + 0x1c))
      ;
    }
    param_5._0_4_ = uVar7;
    if ((param_4 != 0) && (iVar3 != 0)) {
      param_5._0_4_ = *(uint *)(param_2 + 0x1d8);
    }
    if ((uint)param_5 == 0) {
      lVar11 = FUN_180010310(puVar5[0x42],*(uint *)((longlong)param_3 + 0x1c));
      uVar12 = *(uint *)(param_3 + 0xe);
      param_5._0_4_ = (uint)lVar11;
    }
    if (((param_4 == 0) && (iVar8 == 1)) && ((uVar12 >> 0x12 & 1) == 0)) {
      param_5._0_4_ = (uint)param_5 / 6;
    }
    FUN_180013c10((longlong)param_3,(uint *)&param_5,FUN_18000dfe0);
    if (*(int *)((longlong)param_3 + 0x6c) == 0) {
      lVar11 = puVar5[0x42];
      if ((longlong)param_3[0xb] < lVar11) {
        param_3[0xb] = lVar11;
        lVar11 = FUN_180010310(lVar11,*(uint *)((longlong)param_3 + 0x1c));
        param_5._0_4_ = (uint)lVar11;
        pcVar13 = FUN_18000dfe0;
LAB_18000e904:
        FUN_180013c10((longlong)param_3,(uint *)&param_5,pcVar13);
      }
    }
    else {
      lVar11 = puVar5[0x43];
      if ((longlong)param_3[0xb] < lVar11) {
        param_3[0xb] = lVar11;
        lVar11 = FUN_180010310(lVar11,*(uint *)((longlong)param_3 + 0x1c));
        param_5._0_4_ = (uint)lVar11;
        pcVar13 = FUN_18000e000;
        goto LAB_18000e904;
      }
    }
    if (*(int *)((longlong)param_3 + 0x6c) == 1) {
      if ((*(uint *)(param_3 + 0xe) & 0x40000) == 0) {
        if (20000000 < (longlong)param_3[0xb]) {
          param_3[0xb] = 20000000;
          lVar11 = 20000000;
          goto LAB_18000e952;
        }
      }
      else if (5000000 < (longlong)param_3[0xb]) {
        param_3[0xb] = 5000000;
        lVar11 = 5000000;
LAB_18000e952:
        lVar11 = FUN_180010310(lVar11,*(uint *)((longlong)param_3 + 0x1c));
        param_5._0_4_ = (uint)lVar11;
        FUN_180013c10((longlong)param_3,(uint *)&param_5,FUN_18000dfe0);
      }
    }
    FUN_180013c70((longlong)param_3,param_4,local_58);
    puVar1 = param_3 + 3;
    iVar8 = (**(code **)(*local_60 + 0x18))
                      (local_60,*(undefined4 *)((longlong)param_3 + 0x6c),
                       *(undefined4 *)(param_3 + 0xe),param_3[0xb],local_58[0],puVar1,0);
    if (((param_4 == 0) || (iVar8 < 0)) || (*(int *)((longlong)param_3 + 0x6c) != 1)) {
LAB_18000eacc:
      while (iVar8 == -0x7ff8fff2) {
        if ((longlong)param_3[0xb] < 0xf4241) goto LAB_18000ec36;
        lVar11 = param_3[0xb] + -1000000;
        param_3[0xb] = lVar11;
        lVar11 = FUN_180010310(lVar11,*(uint *)((longlong)param_3 + 0x1c));
        param_5._0_4_ = (uint)lVar11;
        FUN_180013c10((longlong)param_3,(uint *)&param_5,FUN_18000dfe0);
        if (local_60 != (longlong *)0x0) {
          (**(code **)(*local_60 + 0x10))();
          local_60 = (longlong *)0x0;
        }
        uVar9 = FUN_18000e210(puVar5,(longlong)(param_3 + 0x14),&local_60);
        iVar8 = (int)uVar9;
        if (iVar8 < 0) goto LAB_18000e743;
        FUN_180013c70((longlong)param_3,param_4,local_58);
        iVar8 = (**(code **)(*local_60 + 0x18))
                          (local_60,*(undefined4 *)((longlong)param_3 + 0x6c),
                           *(undefined4 *)(param_3 + 0xe),param_3[0xb],local_58[0],puVar1,0);
      }
      if ((iVar8 == -0x7776ffea) || (iVar8 == -0x7776ffe7)) {
        param_3[0xb] = puVar5[0x42];
        if (local_60 != (longlong *)0x0) {
          (**(code **)(*local_60 + 0x10))();
          local_60 = (longlong *)0x0;
        }
        uVar9 = FUN_18000e210(puVar5,(longlong)(param_3 + 0x14),&local_60);
        iVar8 = (int)uVar9;
        if (iVar8 < 0) goto LAB_18000e743;
        FUN_180013c70((longlong)param_3,param_4,local_58);
        iVar8 = (**(code **)(*local_60 + 0x18))
                          (local_60,*(undefined4 *)((longlong)param_3 + 0x6c),
                           *(undefined4 *)(param_3 + 0xe),param_3[0xb],local_58[0],puVar1,0);
      }
      if (-1 < iVar8) {
        *param_3 = local_60;
        (**(code **)(*local_60 + 8))();
        lVar11 = FUN_180010310(param_3[0xb],*(uint *)((longlong)param_3 + 0x1c));
        FUN_180014370((longlong)param_3,uVar4,(uint)lVar11,iVar3,param_4);
        *piVar6 = 0;
        goto LAB_18000ec7f;
      }
LAB_18000ec36:
      *piVar6 = -0x270c;
    }
    else {
      iVar8 = (**(code **)(*local_60 + 0x20))();
      if (-1 < iVar8) {
        if ((uint)param_5 * 2 <= local_64) {
          lVar11 = FUN_180010310((longlong)param_3[0xb] /
                                 (longlong)((ulonglong)local_64 / (ulonglong)(uint)param_5),
                                 *(uint *)((longlong)param_3 + 0x1c));
          param_5._0_4_ = (uint)lVar11;
          FUN_180013c10((longlong)param_3,(uint *)&param_5,FUN_18000dfe0);
          if ((longlong)param_3[0xb] < (longlong)puVar5[0x43]) {
            param_3[0xb] = puVar5[0x43];
          }
          if (local_60 != (longlong *)0x0) {
            (**(code **)(*local_60 + 0x10))();
            local_60 = (longlong *)0x0;
          }
          uVar9 = FUN_18000e210(puVar5,(longlong)(param_3 + 0x14),&local_60);
          iVar8 = (int)uVar9;
          if (iVar8 < 0) goto LAB_18000e743;
          FUN_180013c70((longlong)param_3,param_4,local_58);
          iVar8 = (**(code **)(*local_60 + 0x18))
                            (local_60,*(undefined4 *)((longlong)param_3 + 0x6c),
                             *(undefined4 *)(param_3 + 0xe),param_3[0xb],local_58[0],puVar1,0);
        }
        goto LAB_18000eacc;
      }
      *piVar6 = -0x270c;
    }
  }
  FUN_180014510(iVar8);
LAB_18000ec7f:
  if (local_60 != (longlong *)0x0) {
    (**(code **)(*local_60 + 0x10))();
  }
  return unaff_RDI;
}



/* ========================================================================
   ENTRY: 18000ecc0
   NAME : FUN_18000ecc0
   SIG  : ulonglong __fastcall FUN_18000ecc0(longlong param_1, undefined4 param_2)
   ======================================================================== */

ulonglong FUN_18000ecc0(longlong param_1,undefined4 param_2)

{
  uint *puVar1;
  undefined8 *puVar2;
  undefined8 *puVar3;
  HRESULT HVar4;
  int iVar5;
  uint uVar6;
  uint uVar7;
  wchar_t *pwVar8;
  undefined8 uVar9;
  ulonglong uVar10;
  ulonglong uVar11;
  uint uVar12;
  ulonglong uVar14;
  uint local_res8;
  undefined4 uStackX_c;
  undefined4 local_res10;
  longlong *local_res18;
  longlong *local_res20;
  longlong local_78;
  wchar_t *local_70;
  wchar_t *local_68;
  longlong local_60;
  longlong *local_58;
  ulonglong uVar13;
  
  uVar13 = 0;
  puVar1 = (uint *)(param_1 + 0x118);
  local_70 = (wchar_t *)0x0;
  local_68 = (wchar_t *)0x0;
  local_res18 = (longlong *)0x0;
  local_res20 = (longlong *)0x0;
  if ((*puVar1 != 0) || (*(int *)(param_1 + 0x18) != 0)) {
    return 0xffffd8fe;
  }
  local_res10 = param_2;
  HVar4 = CoCreateInstance((IID *)&DAT_180023e98,(LPUNKNOWN)0x0,1,(IID *)&DAT_180023e88,&local_res20
                          );
  if (HVar4 < 0) {
    uVar11 = 0xffffd8fe;
  }
  else {
    iVar5 = (**(code **)(*local_res20 + 0x20))(local_res20,0,1,&local_res8);
    if (iVar5 == 0) {
      iVar5 = (**(code **)(*(longlong *)CONCAT44(uStackX_c,local_res8) + 0x28))
                        ((longlong *)CONCAT44(uStackX_c,local_res8),&local_70);
      (**(code **)(*(longlong *)CONCAT44(uStackX_c,local_res8) + 0x10))();
      if (iVar5 < 0) {
        uVar11 = 0xffffd8fe;
        goto LAB_18000efcf;
      }
    }
    else if (((iVar5 + 0x80000000U & 0x80000000) == 0) && (iVar5 != -0x7ff8fb70)) {
      uVar11 = 0xffffd8fe;
      goto LAB_18000efcf;
    }
    iVar5 = (**(code **)(*local_res20 + 0x20))(local_res20,1,1,&local_res8);
    if (iVar5 == 0) {
      iVar5 = (**(code **)(*(longlong *)CONCAT44(uStackX_c,local_res8) + 0x28))
                        ((longlong *)CONCAT44(uStackX_c,local_res8),&local_68);
      (**(code **)(*(longlong *)CONCAT44(uStackX_c,local_res8) + 0x10))();
      if (iVar5 < 0) {
        uVar11 = 0xffffd8fe;
        goto LAB_18000efcf;
      }
    }
    else if (((iVar5 + 0x80000000U & 0x80000000) == 0) && (iVar5 != -0x7ff8fb70)) {
      uVar11 = 0xffffd8fe;
      goto LAB_18000efcf;
    }
    iVar5 = (**(code **)(*local_res20 + 0x18))(local_res20,2,1,&local_res18);
    if (iVar5 < 0) {
      uVar11 = 0xffffd8fe;
    }
    else {
      iVar5 = (**(code **)(*local_res18 + 0x18))();
      if (iVar5 < 0) {
        uVar11 = 0xffffd8fe;
      }
      else {
        local_78 = 0;
        uVar6 = FUN_18000fac0(local_res18,0);
        local_res8 = uVar6;
        if ((*puVar1 == 0) || (local_78 = FUN_18000e350(param_1,*puVar1 + uVar6), local_78 != 0)) {
          uVar7 = 0;
          uVar14 = uVar13;
          if (*puVar1 != 0) {
            do {
              local_60 = uVar13 * 0x280;
              puVar2 = (undefined8 *)(local_78 + uVar13 * 0x48);
              local_58 = (longlong *)(*(longlong *)(param_1 + 0x120) + local_60);
              pwVar8 = (wchar_t *)FUN_18000f030((undefined4 *)puVar2,local_res10);
              uVar9 = FUN_18000f040(param_1,local_res18,(uint)uVar13,local_70,local_68,
                                    (longlong)puVar2,pwVar8);
              uVar7 = (uint)uVar14;
              if ((int)uVar9 != 0) {
                uVar10 = FUN_18000f440(param_1,(longlong)puVar2);
                uVar11 = uVar10 & 0xffffffff;
                uVar6 = local_res8;
                if ((int)uVar10 != 0) goto LAB_18000efcf;
              }
              *(undefined8 **)(*(longlong *)(param_1 + 0x28) + uVar13 * 8) = puVar2;
              *(int *)(param_1 + 0x18) = *(int *)(param_1 + 0x18) + 1;
              if ((*(int *)(*(longlong *)(param_1 + 0x120) + 0x270 + local_60) == 0) &&
                 (uVar7 < uVar6)) {
                uVar6 = *puVar1 + uVar7;
                puVar3 = (undefined8 *)(local_78 + (ulonglong)uVar6 * 0x48);
                uVar9 = FUN_18000f490(param_1,puVar3,
                                      (longlong *)
                                      ((ulonglong)uVar6 * 0x280 + *(longlong *)(param_1 + 0x120)),
                                      puVar2,local_58);
                if ((int)uVar9 == 0) {
                  uVar11 = 0xffffd8f8;
                  goto LAB_18000efcf;
                }
                uVar14 = (ulonglong)(uVar7 + 1);
                *(undefined8 **)(*(longlong *)(param_1 + 0x28) + (ulonglong)uVar6 * 8) = puVar3;
                *(int *)(param_1 + 0x18) = *(int *)(param_1 + 0x18) + 1;
              }
              uVar7 = *puVar1;
              uVar12 = (uint)uVar13 + 1;
              uVar13 = (ulonglong)uVar12;
              uVar6 = local_res8;
            } while (uVar12 < uVar7);
          }
          uVar11 = 0;
          *puVar1 = uVar7 + uVar6;
        }
        else {
          uVar11 = 0xffffd8f8;
        }
      }
    }
  }
LAB_18000efcf:
  CoTaskMemFree(local_70);
  CoTaskMemFree(local_68);
  if (local_res18 != (longlong *)0x0) {
    (**(code **)(*local_res18 + 0x10))();
    local_res18 = (longlong *)0x0;
  }
  if (local_res20 != (longlong *)0x0) {
    (**(code **)(*local_res20 + 0x10))();
  }
  return uVar11;
}



/* ========================================================================
   ENTRY: 18000f030
   NAME : FUN_18000f030
   SIG  : undefined __fastcall FUN_18000f030(undefined4 * param_1, undefined4 param_2)
   ======================================================================== */

void FUN_18000f030(undefined4 *param_1,undefined4 param_2)

{
  *param_1 = 2;
  param_1[4] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 18000f040
   NAME : FUN_18000f040
   SIG  : undefined8 __fastcall FUN_18000f040(longlong param_1, longlong * param_2, uint param_3, wchar_t * param_4, wchar_t * param_5, longlong param_6, wchar_t * param_7)
   ======================================================================== */

undefined8
FUN_18000f040(longlong param_1,longlong *param_2,uint param_3,wchar_t *param_4,wchar_t *param_5,
             longlong param_6,wchar_t *param_7)

{
  double dVar1;
  wchar_t *pwVar2;
  longlong *pv;
  wchar_t *pwVar3;
  int iVar4;
  LPSTR lpMultiByteStr;
  size_t sVar5;
  undefined8 uVar6;
  longlong *local_res8;
  undefined8 local_58;
  wchar_t *pwStack_50;
  void *local_48;
  
  pwVar2 = param_7;
  iVar4 = (**(code **)(*param_2 + 0x20))(param_2,param_3,param_7);
  if ((-1 < iVar4) &&
     (iVar4 = (**(code **)(**(longlong **)pwVar2 + 0x28))(*(longlong **)pwVar2,&param_7),
     pwVar3 = param_7, -1 < iVar4)) {
    wcsncpy(pwVar2 + 4,param_7,0xff);
    CoTaskMemFree(pwVar3);
    iVar4 = (**(code **)(**(longlong **)pwVar2 + 0x30))(*(longlong **)pwVar2,pwVar2 + 0x104);
    if ((-1 < iVar4) &&
       (iVar4 = (**(code **)(**(longlong **)pwVar2 + 0x20))(*(longlong **)pwVar2,0,&param_7),
       -1 < iVar4)) {
      local_48 = (void *)0x0;
      local_58 = 0;
      pwStack_50 = (wchar_t *)0x0;
      iVar4 = (**(code **)(*(longlong *)param_7 + 0x28))(param_7,&DAT_180023f90,&local_58);
      if (-1 < iVar4) {
        lpMultiByteStr = (LPSTR)FUN_1800012e0(*(int **)(param_1 + 0x108),0x80);
        pwVar3 = pwStack_50;
        *(LPSTR *)(param_6 + 8) = lpMultiByteStr;
        if (lpMultiByteStr == (LPSTR)0x0) {
          PropVariantClear((PROPVARIANT *)&local_58);
          return 0xffffd8f8;
        }
        if (pwStack_50 == (wchar_t *)0x0) {
          snprintf(lpMultiByteStr,0x7f,"baddev%d",(ulonglong)param_3);
        }
        else {
          sVar5 = wcslen(pwStack_50);
          WideCharToMultiByte(0xfde9,0,pwVar3,(int)sVar5,lpMultiByteStr,0x7f,(LPCSTR)0x0,(LPBOOL)0x0
                             );
        }
        PropVariantClear((PROPVARIANT *)&local_58);
        local_48 = (void *)0x0;
        local_58 = 0;
        pwStack_50 = (wchar_t *)0x0;
        iVar4 = (**(code **)(*(longlong *)param_7 + 0x28))(param_7,&DAT_180023fc0,&local_58);
        if (-1 < iVar4) {
          sVar5 = 0x28;
          if ((uint)pwStack_50 < 0x29) {
            sVar5 = (ulonglong)pwStack_50 & 0xffffffff;
          }
          memcpy(pwVar2 + 0x110,local_48,sVar5);
          PropVariantClear((PROPVARIANT *)&local_58);
          local_48 = (void *)0x0;
          local_58 = 0;
          pwStack_50 = (wchar_t *)0x0;
          iVar4 = (**(code **)(*(longlong *)param_7 + 0x28))(param_7,&DAT_180023fa8,&local_58);
          if (-1 < iVar4) {
            *(undefined4 *)(pwVar2 + 0x13a) = pwStack_50._0_4_;
            PropVariantClear((PROPVARIANT *)&local_58);
            iVar4 = (**(code **)**(undefined8 **)pwVar2)
                              (*(undefined8 **)pwVar2,&DAT_180023e78,&local_res8);
            if ((-1 < iVar4) &&
               ((**(code **)(*local_res8 + 0x18))(local_res8,pwVar2 + 0x138),
               local_res8 != (longlong *)0x0)) {
              (**(code **)(*local_res8 + 0x10))();
              local_res8 = (longlong *)0x0;
            }
            if (param_7 != (wchar_t *)0x0) {
              (**(code **)(*(longlong *)param_7 + 0x10))();
            }
            if ((param_4 != (wchar_t *)0x0) &&
               (iVar4 = wcsncmp(pwVar2 + 4,param_4,0x7f), iVar4 == 0)) {
              *(uint *)(param_1 + 0x20) = param_3;
            }
            if ((param_5 != (wchar_t *)0x0) &&
               (iVar4 = wcsncmp(pwVar2 + 4,param_5,0x7f), iVar4 == 0)) {
              *(uint *)(param_1 + 0x1c) = param_3;
            }
            uVar6 = FUN_18000e210((undefined8 *)pwVar2,0,&param_7);
            if (-1 < (int)uVar6) {
              iVar4 = (**(code **)(*(longlong *)param_7 + 0x48))
                                (param_7,pwVar2 + 0x108,pwVar2 + 0x10c);
              if (iVar4 < 0) {
                pwVar2[0x108] = L'蚠';
                pwVar2[0x109] = L'\x01';
                pwVar2[0x10a] = L'\0';
                pwVar2[0x10b] = L'\0';
                pwVar2[0x10c] = L'田';
                pwVar2[0x10d] = L'\0';
                pwVar2[0x10e] = L'\0';
                pwVar2[0x10f] = L'\0';
              }
              iVar4 = (**(code **)(*(longlong *)param_7 + 0x40))(param_7,&local_res8);
              pv = local_res8;
              if (-1 < iVar4) {
                sVar5 = (ulonglong)*(ushort *)(local_res8 + 2) + 0x12;
                if (0x28 < sVar5) {
                  sVar5 = 0x28;
                }
                memcpy(pwVar2 + 0x124,local_res8,sVar5);
                CoTaskMemFree(pv);
              }
              if (param_7 != (wchar_t *)0x0) {
                (**(code **)(*(longlong *)param_7 + 0x10))();
                param_7 = (wchar_t *)0x0;
              }
              if (iVar4 == 0) {
                *(undefined8 *)(param_6 + 0x14) = 0;
                *(double *)(param_6 + 0x40) = (double)*(uint *)(pwVar2 + 0x126);
                if (*(int *)(pwVar2 + 0x138) == 0) {
                  *(uint *)(param_6 + 0x18) = (uint)(ushort)pwVar2[0x125];
                  dVar1 = FUN_180014880(*(longlong *)(pwVar2 + 0x108));
                  *(double *)(param_6 + 0x38) = dVar1;
                  dVar1 = FUN_180014880(*(longlong *)(pwVar2 + 0x10c));
                  *(double *)(param_6 + 0x28) = dVar1;
                }
                else {
                  if (*(int *)(pwVar2 + 0x138) != 1) {
                    return 0xffffd8fe;
                  }
                  *(uint *)(param_6 + 0x14) = (uint)(ushort)pwVar2[0x125];
                  dVar1 = FUN_180014880(*(longlong *)(pwVar2 + 0x108));
                  *(double *)(param_6 + 0x30) = dVar1;
                  dVar1 = FUN_180014880(*(longlong *)(pwVar2 + 0x10c));
                  *(double *)(param_6 + 0x20) = dVar1;
                }
                return 0;
              }
              FUN_180014510(iVar4);
              return 0xffffd8fe;
            }
          }
        }
      }
    }
  }
  return 0xffffd8fe;
}



/* ========================================================================
   ENTRY: 18000f440
   NAME : FUN_18000f440
   SIG  : undefined8 __fastcall FUN_18000f440(longlong param_1, longlong param_2)
   ======================================================================== */

undefined8 FUN_18000f440(longlong param_1,longlong param_2)

{
  undefined1 *puVar1;
  
  puVar1 = *(undefined1 **)(param_2 + 8);
  if (puVar1 == (undefined1 *)0x0) {
    puVar1 = (undefined1 *)FUN_1800012e0(*(int **)(param_1 + 0x108),1);
    *(undefined1 **)(param_2 + 8) = puVar1;
    if (puVar1 == (undefined1 *)0x0) {
      return 0xffffd8f8;
    }
  }
  *puVar1 = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 18000f490
   NAME : FUN_18000f490
   SIG  : undefined8 __fastcall FUN_18000f490(longlong param_1, undefined8 * param_2, longlong * param_3, undefined8 * param_4, longlong * param_5)
   ======================================================================== */

undefined8
FUN_18000f490(longlong param_1,undefined8 *param_2,longlong *param_3,undefined8 *param_4,
             longlong *param_5)

{
  undefined8 uVar1;
  longlong lVar2;
  longlong *plVar3;
  longlong lVar4;
  
  uVar1 = param_4[1];
  *param_2 = *param_4;
  param_2[1] = uVar1;
  uVar1 = param_4[3];
  param_2[2] = param_4[2];
  param_2[3] = uVar1;
  uVar1 = param_4[5];
  param_2[4] = param_4[4];
  param_2[5] = uVar1;
  uVar1 = param_4[7];
  param_2[6] = param_4[6];
  param_2[7] = uVar1;
  param_2[8] = param_4[8];
  lVar4 = 5;
  plVar3 = param_3;
  do {
    lVar2 = param_5[1];
    *plVar3 = *param_5;
    plVar3[1] = lVar2;
    lVar2 = param_5[3];
    plVar3[2] = param_5[2];
    plVar3[3] = lVar2;
    lVar2 = param_5[5];
    plVar3[4] = param_5[4];
    plVar3[5] = lVar2;
    lVar2 = param_5[7];
    plVar3[6] = param_5[6];
    plVar3[7] = lVar2;
    lVar2 = param_5[9];
    plVar3[8] = param_5[8];
    plVar3[9] = lVar2;
    lVar2 = param_5[0xb];
    plVar3[10] = param_5[10];
    plVar3[0xb] = lVar2;
    lVar2 = param_5[0xd];
    plVar3[0xc] = param_5[0xc];
    plVar3[0xd] = lVar2;
    lVar2 = param_5[0xf];
    plVar3[0xe] = param_5[0xe];
    plVar3[0xf] = lVar2;
    lVar4 = lVar4 + -1;
    plVar3 = plVar3 + 0x10;
    param_5 = param_5 + 0x10;
  } while (lVar4 != 0);
  lVar4 = FUN_1800012e0(*(int **)(param_1 + 0x108),0x81);
  param_2[1] = lVar4;
  if (lVar4 == 0) {
    return 0;
  }
  snprintf(lVar4,0x7f,"%s [Loopback]",param_4[1]);
  if ((longlong *)*param_3 != (longlong *)0x0) {
    (**(code **)(*(longlong *)*param_3 + 8))();
  }
  *(undefined4 *)(param_3 + 0x4f) = 1;
  *(undefined4 *)((longlong)param_2 + 0x14) = *(undefined4 *)(param_4 + 3);
  param_2[6] = param_4[7];
  param_2[4] = param_4[5];
  *(undefined4 *)(param_2 + 3) = 0;
  param_2[7] = 0;
  param_2[5] = 0;
  return 1;
}



/* ========================================================================
   ENTRY: 18000f5d0
   NAME : FUN_18000f5d0
   SIG  : undefined __fastcall FUN_18000f5d0(longlong param_1, int param_2)
   ======================================================================== */

void FUN_18000f5d0(longlong param_1,int param_2)

{
  FUN_180012e30(param_1);
  if (param_2 == 1) {
                    /* WARNING: Could not recover jumptable at 0x00018000f5e7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    CoUninitialize();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000f600
   NAME : FUN_18000f600
   SIG  : ulonglong __fastcall FUN_18000f600(longlong * param_1, double param_2, undefined8 * param_3, undefined8 * param_4, int param_5)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18000f600(longlong *param_1,double param_2,undefined8 *param_3,undefined8 *param_4,
                       int param_5)

{
  uint uVar1;
  uint uVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  undefined1 auStack_b8 [32];
  undefined8 local_98;
  undefined8 uStack_90;
  undefined8 local_88;
  undefined8 uStack_80;
  undefined8 local_78;
  undefined8 uStack_70;
  undefined8 local_68;
  undefined8 uStack_60;
  undefined8 local_58;
  undefined8 uStack_50;
  undefined4 local_48;
  ulonglong local_40;
  
  local_40 = DAT_18002b580 ^ (ulonglong)auStack_b8;
  uVar4 = 1;
  if (*(int *)((longlong)param_3 + 4) == 1) {
    uStack_90 = param_3[1];
    local_88 = param_3[2];
    uStack_80 = param_3[3];
    local_98 = CONCAT44(2,(int)*param_3);
    uVar3 = FUN_180010390(&local_78,(longlong)&local_98,param_2,param_5);
    if ((int)uVar3 == 0) {
      uVar1 = (**(code **)(*param_1 + 0x38))(param_1,uVar4 & 0xffffffff,&local_78,0);
      uVar4 = (ulonglong)uVar1;
      if (uVar1 == 0) {
LAB_18000f787:
        *param_4 = local_78;
        param_4[1] = uStack_70;
        param_4[2] = local_68;
        param_4[3] = uStack_60;
        param_4[4] = local_58;
        param_4[5] = uStack_50;
        *(undefined4 *)(param_4 + 6) = local_48;
        return 0;
      }
    }
    uVar1 = 0;
    do {
      uStack_90 = CONCAT44(uStack_90._4_4_,(&DAT_180023f28)[(int)uVar1]);
      uVar3 = FUN_180010390(&local_78,(longlong)&local_98,param_2,param_5);
      if ((int)uVar3 == 0) {
        uVar2 = (**(code **)(*param_1 + 0x38))(param_1,1,&local_78,0);
        uVar4 = (ulonglong)uVar2;
        if (uVar2 == 0) goto LAB_18000f787;
      }
      uVar1 = uVar1 + 1;
    } while (uVar1 < 4);
  }
  local_98 = *param_3;
  uStack_90 = param_3[1];
  uVar1 = 0;
  local_88 = param_3[2];
  uStack_80 = param_3[3];
  do {
    uStack_90 = CONCAT44(uStack_90._4_4_,(&DAT_180023f28)[(int)uVar1]);
    uVar3 = FUN_180010390(&local_78,(longlong)&local_98,param_2,param_5);
    if ((int)uVar3 == 0) {
      uVar2 = (**(code **)(*param_1 + 0x38))(param_1,1,&local_78,0);
      uVar4 = (ulonglong)uVar2;
      if (uVar2 == 0) goto LAB_18000f787;
    }
    uVar1 = uVar1 + 1;
    if (3 < uVar1) {
      return uVar4 & 0xffffffff;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 18000f7b0
   NAME : FUN_18000f7b0
   SIG  : undefined __fastcall FUN_18000f7b0(void)
   ======================================================================== */

void FUN_18000f7b0(void)

{
  char cVar1;
  int iVar2;
  undefined7 extraout_var;
  
  if (DAT_18002bb78 == (undefined *)0x0) {
    cVar1 = FUN_18000f810();
    iVar2 = (int)CONCAT71(extraout_var,cVar1);
    if (iVar2 != 2) {
      if (iVar2 != 3) {
        DAT_18002bb78 = &DAT_180023e48;
        return;
      }
      DAT_18002bb78 = &DAT_180023e68;
      return;
    }
    DAT_18002bb78 = &DAT_180023e58;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000f810
   NAME : FUN_18000f810
   SIG  : char __fastcall FUN_18000f810(void)
   ======================================================================== */

char FUN_18000f810(void)

{
  int iVar1;
  
  iVar1 = FUN_180020730();
  if (9 < iVar1) {
    return '\x03';
  }
  return (7 < iVar1) + '\x01';
}



/* ========================================================================
   ENTRY: 18000f840
   NAME : FUN_18000f840
   SIG  : undefined8 __fastcall FUN_18000f840(longlong * param_1, longlong param_2, longlong param_3, undefined4 * param_4, double param_5, int param_6, undefined8 * param_7, int param_8)
   ======================================================================== */

/* WARNING: Type propagation algorithm not settling */

undefined8
FUN_18000f840(longlong *param_1,longlong param_2,longlong param_3,undefined4 *param_4,double param_5
             ,int param_6,undefined8 *param_7,int param_8)

{
  short sVar1;
  longlong lVar2;
  int iVar3;
  uint uVar4;
  int iVar5;
  undefined8 uVar6;
  ulonglong uVar7;
  char *pcVar8;
  short **ppsVar9;
  bool bVar10;
  short *local_res20;
  undefined8 local_88;
  uint uStack_80;
  undefined4 uStack_7c;
  undefined8 local_78;
  undefined8 uStack_70;
  
  local_78 = *(undefined8 *)(param_4 + 4);
  uStack_70 = *(undefined8 *)(param_4 + 6);
  lVar2 = *(longlong *)(param_4 + 6);
  local_res20 = (short *)0x0;
  local_88._0_4_ = *param_4;
  iVar3 = param_4[1];
  uVar4 = param_4[2];
  uStack_7c = param_4[3];
  if ((lVar2 == 0) || ((*(byte *)(lVar2 + 0xc) & 0x20) == 0)) {
    bVar10 = false;
  }
  else {
    bVar10 = true;
  }
  local_88._4_4_ = iVar3;
  uStack_80 = uVar4;
  FUN_180010390(param_7,(longlong)&local_88,param_5,0);
  iVar5 = FUN_180020730();
  if ((((6 < iVar5) && (param_6 == 0)) && (lVar2 != 0)) && ((*(byte *)(lVar2 + 0xc) & 0x40) != 0)) {
    return 0;
  }
  ppsVar9 = &local_res20;
  if (param_6 != 0) {
    ppsVar9 = (short **)0x0;
  }
  iVar5 = (**(code **)(*param_1 + 0x38))(param_1,param_6,param_7,ppsVar9);
  if (iVar5 != 0) {
    if (param_6 == 1) {
      FUN_180010390(param_7,(longlong)&local_88,param_5,1);
      iVar5 = (**(code **)(*param_1 + 0x38))(param_1,1,param_7,0);
    }
    if (iVar5 != 0) {
      if (local_res20 == (short *)0x0) {
        if ((param_6 == 1) && (!bVar10)) {
          uVar7 = FUN_18000f600(param_1,param_5,&local_88,param_7,0);
          if ((int)uVar7 == 0) {
            return 0;
          }
          uVar7 = FUN_18000f600(param_1,param_5,&local_88,param_7,1);
          iVar5 = (int)uVar7;
          if (iVar5 == 0) {
            return 0;
          }
        }
        FUN_180014510(iVar5);
      }
      else {
        uVar6 = *(undefined8 *)(local_res20 + 4);
        sVar1 = *local_res20;
        *param_7 = *(undefined8 *)local_res20;
        param_7[1] = uVar6;
        if (sVar1 == -2) {
          uVar6 = *(undefined8 *)(local_res20 + 0xc);
          param_7[2] = *(undefined8 *)(local_res20 + 8);
          param_7[3] = uVar6;
          param_7[4] = *(undefined8 *)(local_res20 + 0x10);
        }
        else {
          *(short *)(param_7 + 2) = local_res20[8];
        }
        CoTaskMemFree(local_res20);
        if ((int)(longlong)param_5 == *(int *)((longlong)param_7 + 4)) {
          if ((short)iVar3 == *(short *)((longlong)param_7 + 2)) {
            uVar6 = FUN_180011160(uVar4);
            if ((short)uVar6 != 0) {
              return 0;
            }
            return 0xffffd8f6;
          }
          if ((iVar3 == 1) && (*(short *)((longlong)param_7 + 2) == 2)) {
            return 0;
          }
          return 0xffffd8f2;
        }
      }
      return 0xffffd8f3;
    }
  }
  if ((((param_8 == 0 && param_6 == 0) && (iVar3 == 1)) && (local_res20 == (short *)0x0)) &&
     ((1 < *(ushort *)(param_3 + 0x24a) &&
      (pcVar8 = strstr(*(char **)(param_2 + 8),"Realtek"), pcVar8 != (char *)0x0)))) {
    *(undefined2 *)((longlong)param_7 + 2) = 2;
    FUN_180013640((longlong)param_7);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18000fac0
   NAME : FUN_18000fac0
   SIG  : uint __fastcall FUN_18000fac0(longlong * param_1, int param_2)
   ======================================================================== */

uint FUN_18000fac0(longlong *param_1,int param_2)

{
  int iVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  uint local_res8 [4];
  int local_res18 [2];
  longlong *local_res20;
  longlong *local_38 [2];
  
  iVar1 = (**(code **)(*param_1 + 0x18))(param_1,local_res8);
  if (iVar1 < 0) {
    return 0;
  }
  uVar3 = 0;
  uVar4 = 0;
  uVar2 = uVar3;
  if (local_res8[0] != 0) {
    do {
      iVar1 = (**(code **)(*param_1 + 0x20))(param_1,uVar3,local_38);
      if (iVar1 < 0) {
        return 0;
      }
      iVar1 = (**(code **)*local_38[0])(local_38[0],&DAT_180023e78,&local_res20);
      uVar4 = uVar2;
      if (-1 < iVar1) {
        iVar1 = (**(code **)(*local_res20 + 0x18))(local_res20,local_res18);
        if (-1 < iVar1) {
          uVar2 = uVar2 + (local_res18[0] == param_2);
        }
        uVar4 = uVar2;
        if (local_res20 != (longlong *)0x0) {
          (**(code **)(*local_res20 + 0x10))();
          local_res20 = (longlong *)0x0;
        }
      }
      if (local_38[0] != (longlong *)0x0) {
        (**(code **)(*local_38[0] + 0x10))();
        local_38[0] = (longlong *)0x0;
      }
      uVar3 = uVar3 + 1;
      uVar2 = uVar4;
    } while (uVar3 < local_res8[0]);
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 18000fba0
   NAME : FUN_18000fba0
   SIG  : longlong __fastcall FUN_18000fba0(longlong param_1, longlong param_2)
   ======================================================================== */

longlong FUN_18000fba0(longlong param_1,longlong param_2)

{
  if (param_2 == 0) {
    return 0;
  }
  return ((param_1 * 10000000) / param_2) / 10000;
}



/* ========================================================================
   ENTRY: 18000fbe0
   NAME : FUN_18000fbe0
   SIG  : longlong __fastcall FUN_18000fbe0(longlong param_1, longlong param_2)
   ======================================================================== */

longlong FUN_18000fbe0(longlong param_1,longlong param_2)

{
  if (param_2 == 0) {
    return 0;
  }
  return ((param_1 * 10000000) / param_2) / 10;
}



/* ========================================================================
   ENTRY: 18000fc20
   NAME : FUN_18000fc20
   SIG  : undefined1 * __fastcall FUN_18000fc20(short * param_1, int param_2)
   ======================================================================== */

undefined1 * FUN_18000fc20(short *param_1,int param_2)

{
  undefined8 uVar1;
  undefined1 *puVar2;
  uint uVar3;
  
  uVar1 = FUN_180013850(param_1);
  uVar3 = (uint)uVar1;
  if (param_2 == 0) {
    uVar3 = uVar3 & 0x7fffffff;
    if (uVar3 == 2) {
      return &LAB_180014000;
    }
    if (uVar3 == 4) {
      return &LAB_180013fd0;
    }
    if (uVar3 == 8) {
      puVar2 = &LAB_180013f90;
      if (param_1[7] == 0x20) {
        puVar2 = &LAB_180013fd0;
      }
      return puVar2;
    }
    if (uVar3 != 0x10) {
      puVar2 = &LAB_180014030;
      if (uVar3 != 0x40) {
        puVar2 = (undefined1 *)0x0;
      }
      return puVar2;
    }
    return &LAB_180013f60;
  }
  if (param_2 != 1) {
    if (param_2 != 2) {
      return (undefined1 *)0x0;
    }
    uVar3 = uVar3 & 0x7fffffff;
    if (uVar3 == 2) {
      return &LAB_1800141b0;
    }
    if (uVar3 == 4) {
      return &LAB_1800141b0;
    }
    if (uVar3 != 8) {
      if (uVar3 != 0x10) {
        puVar2 = (undefined1 *)0x0;
        if (uVar3 == 0x40) {
          puVar2 = &LAB_180014280;
        }
        return puVar2;
      }
      return &LAB_180014090;
    }
    puVar2 = &LAB_180014140;
    if (param_1[7] == 0x20) {
      puVar2 = &LAB_1800141b0;
    }
    return puVar2;
  }
  uVar3 = uVar3 & 0x7fffffff;
  if (uVar3 == 2) {
    return &LAB_1800141e0;
  }
  if (uVar3 == 4) {
    return &LAB_180014180;
  }
  if (uVar3 == 8) {
    puVar2 = &LAB_1800140c0;
    if (param_1[7] == 0x20) {
      puVar2 = &LAB_180014250;
    }
    return puVar2;
  }
  if (uVar3 != 0x10) {
    puVar2 = (undefined1 *)0x0;
    if (uVar3 == 0x40) {
      puVar2 = &LAB_180014220;
    }
    return puVar2;
  }
  return &LAB_180014060;
}



/* ========================================================================
   ENTRY: 18000fdc0
   NAME : FUN_18000fdc0
   SIG  : uint __fastcall FUN_18000fdc0(uint param_1)
   ======================================================================== */

uint FUN_18000fdc0(uint param_1)

{
  if ((param_1 & 0x7fffffff) == 0x10000) {
    param_1 = (int)param_1 >> 0x1f & 0x80000004U | 4;
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 18000fde0
   NAME : FUN_18000fde0
   SIG  : uint __fastcall FUN_18000fde0(longlong param_1, uint param_2, uint param_3, uint param_4)
   ======================================================================== */

uint FUN_18000fde0(longlong param_1,uint param_2,uint param_3,uint param_4)

{
  uint uVar1;
  
  uVar1 = param_3;
  if ((param_4 != 0) && (uVar1 = param_3 >> 1, 2 < *(uint *)(param_1 + 0x330) / param_4)) {
    uVar1 = param_3;
  }
  if ((param_2 != 0) && (uVar1 != 0)) {
    if (param_2 < uVar1) {
      uVar1 = param_2;
    }
    return uVar1;
  }
  if (param_2 != 0) {
    uVar1 = param_2;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18000fe30
   NAME : FUN_18000fe30
   SIG  : int __fastcall FUN_18000fe30(longlong param_1)
   ======================================================================== */

int FUN_18000fe30(longlong param_1)

{
  int iVar1;
  uint uVar2;
  int local_res8 [8];
  
  local_res8[0] = 0;
  if (*(int *)(param_1 + 0x434) == 0) {
    return -0x26ff;
  }
  if (*(longlong *)(param_1 + 0x2c0) == 0) {
    return -0x2704;
  }
  iVar1 = FUN_1800142b0(param_1,local_res8);
  if (iVar1 != 0) {
    FUN_180014510(iVar1);
    return -9999;
  }
  uVar2 = FUN_180006b00(*(longlong *)(param_1 + 0x2a0));
  return uVar2 + local_res8[0];
}



/* ========================================================================
   ENTRY: 18000fec0
   NAME : FUN_18000fec0
   SIG  : undefined4 __fastcall FUN_18000fec0(longlong param_1)
   ======================================================================== */

undefined4 FUN_18000fec0(longlong param_1)

{
  int iVar1;
  int local_res8 [8];
  
  local_res8[0] = 0;
  if (*(int *)(param_1 + 0x434) == 0) {
    return 0xffffd901;
  }
  if (*(longlong *)(param_1 + 0x418) == 0) {
    return 0xffffd8fc;
  }
  iVar1 = FUN_180014300(param_1,local_res8);
  if (iVar1 != 0) {
    FUN_180014510(iVar1);
    return 0xffffd8f1;
  }
  return local_res8[0];
}



/* ========================================================================
   ENTRY: 18000ff30
   NAME : FUN_18000ff30
   SIG  : ulonglong __fastcall FUN_18000ff30(longlong param_1, int * param_2, int * param_3, double param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18000ff30(longlong param_1,int *param_2,int *param_3,double param_4)

{
  longlong lVar1;
  longlong lVar2;
  int iVar3;
  ulonglong uVar4;
  undefined8 uVar5;
  uint uVar6;
  undefined8 *puVar7;
  undefined1 auStackY_d8 [32];
  longlong *local_98;
  undefined8 local_90 [7];
  ulonglong local_58;
  
  local_58 = DAT_18002b580 ^ (ulonglong)auStackY_d8;
  local_98 = (longlong *)0x0;
  uVar4 = FUN_180010150(param_1,param_2,param_3,param_4);
  if ((int)uVar4 != 0) {
    return uVar4;
  }
  if (param_2 != (int *)0x0) {
    uVar6 = 0;
    lVar1 = *(longlong *)(param_2 + 6);
    lVar2 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_2 * 8);
    puVar7 = (undefined8 *)((longlong)*param_2 * 0x280 + *(longlong *)(param_1 + 0x120));
    if (lVar1 != 0) {
      uVar6 = *(uint *)(lVar1 + 0xc) & 1;
    }
    uVar5 = FUN_18000e210(puVar7,lVar1,&local_98);
    iVar3 = (int)uVar5;
    if (iVar3 != 0) goto LAB_180010089;
    uVar4 = FUN_18000f840(local_98,lVar2,(longlong)puVar7,param_2,param_4,uVar6,local_90,0);
    if (local_98 != (longlong *)0x0) {
      (**(code **)(*local_98 + 0x10))();
      local_98 = (longlong *)0x0;
    }
    if ((int)uVar4 != 0) {
      return uVar4 & 0xffffffff;
    }
  }
  if (param_3 == (int *)0x0) {
    return 0;
  }
  uVar6 = 0;
  lVar1 = *(longlong *)(param_3 + 6);
  lVar2 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8);
  puVar7 = (undefined8 *)((longlong)*param_3 * 0x280 + *(longlong *)(param_1 + 0x120));
  if (lVar1 != 0) {
    uVar6 = *(uint *)(lVar1 + 0xc) & 1;
  }
  uVar5 = FUN_18000e210(puVar7,lVar1,&local_98);
  iVar3 = (int)uVar5;
  if (iVar3 == 0) {
    uVar4 = FUN_18000f840(local_98,lVar2,(longlong)puVar7,param_3,param_4,uVar6,local_90,1);
    if (local_98 != (longlong *)0x0) {
      (**(code **)(*local_98 + 0x10))();
    }
    if ((int)uVar4 == 0) {
      return 0;
    }
    return uVar4 & 0xffffffff;
  }
LAB_180010089:
  FUN_180014510(iVar3);
  return 0xffffd8f4;
}



/* ========================================================================
   ENTRY: 180010140
   NAME : FUN_180010140
   SIG  : undefined4 __fastcall FUN_180010140(longlong param_1)
   ======================================================================== */

undefined4 FUN_180010140(longlong param_1)

{
  return *(undefined4 *)(param_1 + 0x434);
}



/* ========================================================================
   ENTRY: 180010150
   NAME : FUN_180010150
   SIG  : undefined8 __fastcall FUN_180010150(longlong param_1, int * param_2, int * param_3, double param_4)
   ======================================================================== */

undefined8 FUN_180010150(longlong param_1,int *param_2,int *param_3,double param_4)

{
  int *piVar1;
  undefined8 uVar2;
  
  if (param_1 == 0) {
    return 0xffffd905;
  }
  if ((int)(longlong)param_4 == 0) {
    return 0xffffd8f3;
  }
  if (param_2 == (int *)0x0) {
    if (param_3 == (int *)0x0) {
      return 0xffffd8fe;
    }
    if (*param_3 == -2) {
      return 0xffffd8f4;
    }
    if (*(int *)(*(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8) + 0x18) <
        param_3[1]) {
      return 0xffffd8f2;
    }
    piVar1 = *(int **)(param_3 + 6);
    if (piVar1 != (int *)0x0) {
      if (*piVar1 != 0x48) {
        return 0xffffd900;
      }
      if (piVar1[2] != 1) {
        return 0xffffd900;
      }
      uVar2 = 0;
      if (piVar1[1] != 0xd) {
        uVar2 = 0xffffd900;
      }
      return uVar2;
    }
  }
  else {
    if (*param_2 == -2) {
      return 0xffffd8f4;
    }
    if (*(int *)(*(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_2 * 8) + 0x14) <
        param_2[1]) {
      return 0xffffd8f2;
    }
    piVar1 = *(int **)(param_2 + 6);
    if ((piVar1 != (int *)0x0) && (((*piVar1 != 0x48 || (piVar1[2] != 1)) || (piVar1[1] != 0xd)))) {
      return 0xffffd900;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180010220
   NAME : FUN_180010220
   SIG  : undefined4 __fastcall FUN_180010220(void)
   ======================================================================== */

undefined4 FUN_180010220(void)

{
  HMODULE hModule;
  FARPROC pFVar1;
  HANDLE pvVar2;
  INT_PTR IVar3;
  undefined4 local_res8 [8];
  
  local_res8[0] = 0;
  hModule = GetModuleHandleA("kernel32");
  pFVar1 = GetProcAddress(hModule,"IsWow64Process");
  if (pFVar1 != (FARPROC)0x0) {
    pvVar2 = GetCurrentProcess();
    IVar3 = (*pFVar1)(pvVar2,local_res8);
    if ((int)IVar3 != 0) {
      return local_res8[0];
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180010280
   NAME : FUN_180010280
   SIG  : undefined8 __fastcall FUN_180010280(uint param_1, longlong * param_2)
   ======================================================================== */

undefined8 FUN_180010280(uint param_1,longlong *param_2)

{
  longlong lVar1;
  HANDLE pvVar2;
  undefined4 local_res8 [2];
  
  local_res8[0] = 0;
  if (param_1 < 8) {
    lVar1 = (*DAT_18002bb48)(*(undefined8 *)(&DAT_18002b400 + (longlong)(int)param_1 * 8),local_res8
                            );
    if (lVar1 != 0) {
      pvVar2 = GetCurrentThread();
      GetThreadPriority(pvVar2);
      pvVar2 = GetCurrentProcess();
      GetPriorityClass(pvVar2);
      *param_2 = lVar1;
      return 0;
    }
  }
  return 0xffffd8f1;
}



/* ========================================================================
   ENTRY: 180010300
   NAME : FUN_180010300
   SIG  : undefined __fastcall FUN_180010300(void)
   ======================================================================== */

void FUN_180010300(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180010300. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*DAT_18002bb50)();
  return;
}



/* ========================================================================
   ENTRY: 180010310
   NAME : FUN_180010310
   SIG  : longlong __fastcall FUN_180010310(longlong param_1, uint param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

longlong FUN_180010310(longlong param_1,uint param_2)

{
  return (longlong)
         ((((double)param_1 * (double)param_2) / DAT_1800239c8) / _DAT_180024880 + DAT_180023560);
}



/* ========================================================================
   ENTRY: 180010350
   NAME : FUN_180010350
   SIG  : longlong __fastcall FUN_180010350(uint param_1, uint param_2)
   ======================================================================== */

longlong FUN_180010350(uint param_1,uint param_2)

{
  return (longlong)((DAT_180024888 / (double)param_2) * (double)param_1 + DAT_180023560);
}



/* ========================================================================
   ENTRY: 180010390
   NAME : FUN_180010390
   SIG  : undefined8 __fastcall FUN_180010390(undefined8 * param_1, longlong param_2, double param_3, int param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180010390(undefined8 *param_1,longlong param_2,double param_3,int param_4)

{
  bool bVar1;
  bool bVar2;
  short sVar3;
  short sVar4;
  undefined2 uVar5;
  undefined8 uVar6;
  int iVar7;
  longlong lVar8;
  undefined4 uVar9;
  undefined4 uVar10;
  undefined4 uVar11;
  undefined4 uVar12;
  
  lVar8 = *(longlong *)(param_2 + 0x18);
  bVar1 = 2 < *(int *)(param_2 + 4);
  if ((lVar8 == 0) || ((*(byte *)(lVar8 + 0xc) & 0x80) == 0)) {
    bVar2 = false;
  }
  else {
    bVar2 = true;
  }
  uVar6 = FUN_180011160(*(uint *)(param_2 + 8));
  sVar3 = (short)uVar6;
  if (sVar3 == 0) {
    return 0xffffd8f6;
  }
  iVar7 = 0;
  if ((lVar8 != 0) && (iVar7 = 0, (*(byte *)(lVar8 + 0xc) & 4) != 0)) {
    iVar7 = *(int *)(lVar8 + 0x10);
    bVar1 = true;
  }
  *param_1 = 0;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  param_1[4] = 0;
  param_1[5] = 0;
  *(undefined4 *)(param_1 + 6) = 0;
  *(undefined2 *)((longlong)param_1 + 2) = *(undefined2 *)(param_2 + 4);
  *(short *)((longlong)param_1 + 0xe) = sVar3;
  *(int *)((longlong)param_1 + 4) = (int)(longlong)param_3;
  if ((sVar3 - 8U & 0xfff7) == 0) {
    if (!bVar1) {
      *(undefined2 *)param_1 = 1;
      goto LAB_18001055e;
    }
  }
  else {
    sVar4 = 0x20;
    if (param_4 != 0) {
      sVar4 = sVar3;
    }
    *(short *)((longlong)param_1 + 0xe) = sVar4;
  }
  *(undefined2 *)param_1 = 0xfffe;
  uVar5 = 0x16;
  if (bVar2) {
    uVar5 = 0x22;
  }
  *(undefined2 *)(param_1 + 2) = uVar5;
  uVar6 = ram0x000180023f20;
  if (!bVar2) {
    uVar9 = (undefined4)DAT_180023ef8;
    uVar10 = DAT_180023ef8._4_4_;
    uVar11 = (undefined4)DAT_180023f00;
    uVar12 = DAT_180023f00._4_4_;
    if ((*(uint *)(param_2 + 8) & 0x7fffffff) == 2) {
      uVar9 = (undefined4)DAT_180023f08;
      uVar10 = DAT_180023f08._4_4_;
      uVar11 = (undefined4)DAT_180023f10;
      uVar12 = DAT_180023f10._4_4_;
    }
    *(undefined4 *)(param_1 + 3) = uVar9;
    *(undefined4 *)((longlong)param_1 + 0x1c) = uVar10;
    *(undefined4 *)(param_1 + 4) = uVar11;
    *(undefined4 *)((longlong)param_1 + 0x24) = uVar12;
  }
  else {
    param_1[3] = _DAT_180023f18;
    param_1[4] = uVar6;
    *(int *)(param_1 + 3) = *(int *)(lVar8 + 0x34) >> 0x10;
    *(undefined2 *)((longlong)param_1 + 0x1c) = *(undefined2 *)(lVar8 + 0x34);
    *(undefined4 *)(param_1 + 5) = *(undefined4 *)(lVar8 + 0x38);
    *(undefined4 *)((longlong)param_1 + 0x2c) = *(undefined4 *)(lVar8 + 0x3c);
    *(undefined4 *)(param_1 + 6) = *(undefined4 *)(lVar8 + 0x40);
  }
  *(short *)((longlong)param_1 + 0x12) = sVar3;
  if (iVar7 == 0) {
    switch(*(undefined4 *)(param_2 + 4)) {
    case 1:
      *(undefined4 *)((longlong)param_1 + 0x14) = 4;
      break;
    case 2:
      *(undefined4 *)((longlong)param_1 + 0x14) = 3;
      break;
    case 3:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0xb;
      break;
    case 4:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0x33;
      break;
    case 5:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0x3b;
      break;
    case 6:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0x60f;
      break;
    case 7:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0x70f;
      break;
    case 8:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0x63f;
      break;
    default:
      *(undefined4 *)((longlong)param_1 + 0x14) = 0;
    }
  }
  else {
    *(int *)((longlong)param_1 + 0x14) = iVar7;
  }
LAB_18001055e:
  FUN_180013640((longlong)param_1);
  return 0;
}



/* ========================================================================
   ENTRY: 1800105a0
   NAME : FUN_1800105a0
   SIG  : HRESULT __fastcall FUN_1800105a0(longlong param_1)
   ======================================================================== */

HRESULT FUN_1800105a0(longlong param_1)

{
  HRESULT HVar1;
  
  *(undefined8 *)(param_1 + 0x2b8) = 0;
  *(undefined8 *)(param_1 + 0x178) = 0;
  *(undefined8 *)(param_1 + 0x410) = 0;
  *(undefined8 *)(param_1 + 0x2d0) = 0;
  if ((*(longlong *)(param_1 + 0x170) == 0) ||
     ((HVar1 = FUN_180010660((longlong *)(param_1 + 0x170)), HVar1 == 0 &&
      (HVar1 = CoMarshalInterThreadInterfaceInStream
                         ((IID *)&DAT_180023eb8,*(LPUNKNOWN *)(param_1 + 0x2b0),
                          (LPSTREAM *)(param_1 + 0x2b8)), HVar1 == 0)))) {
    if (*(longlong *)(param_1 + 0x2c8) == 0) {
      return 0;
    }
    HVar1 = FUN_180010660((longlong *)(param_1 + 0x2c8));
    if ((HVar1 == 0) &&
       (HVar1 = CoMarshalInterThreadInterfaceInStream
                          ((IID *)&DAT_180023ea8,*(LPUNKNOWN *)(param_1 + 0x408),
                           (LPSTREAM *)(param_1 + 0x410)), HVar1 == 0)) {
      return 0;
    }
  }
  FUN_180013520(param_1);
  FUN_180012e30(param_1);
  return HVar1;
}



/* ========================================================================
   ENTRY: 180010660
   NAME : FUN_180010660
   SIG  : HRESULT __fastcall FUN_180010660(undefined8 * param_1)
   ======================================================================== */

HRESULT FUN_180010660(undefined8 *param_1)

{
  LPUNKNOWN pUnk;
  HRESULT HVar1;
  IID *riid;
  
  pUnk = (LPUNKNOWN)*param_1;
  param_1[1] = 0;
  riid = (IID *)FUN_18000f7b0();
  HVar1 = CoMarshalInterThreadInterfaceInStream(riid,pUnk,(LPSTREAM *)(param_1 + 1));
  if (HVar1 != 0) {
    FUN_1800135f0((longlong)param_1);
    FUN_180012e90((longlong)param_1);
    return HVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800106d0
   NAME : FUN_1800106d0
   SIG  : undefined __fastcall FUN_1800106d0(longlong param_1, uint param_2, int param_3)
   ======================================================================== */

void FUN_1800106d0(longlong param_1,uint param_2,int param_3)

{
  uint uVar1;
  
  if (*(code **)(param_1 + 0x4a8) != (code *)0x0) {
    uVar1 = param_2 | 1;
    if (-1 < param_3) {
      uVar1 = param_2;
    }
                    /* WARNING: Could not recover jumptable at 0x0001800106f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(param_1 + 0x4a8))(param_1,uVar1,param_3,*(undefined8 *)(param_1 + 0x4b0));
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180010700
   NAME : FUN_180010700
   SIG  : ulonglong __fastcall FUN_180010700(longlong param_1, undefined8 * param_2, int * param_3, int * param_4, double param_5, uint param_6, uint param_7, longlong param_8, undefined8 param_9)
   ======================================================================== */

ulonglong FUN_180010700(longlong param_1,undefined8 *param_2,int *param_3,int *param_4,
                       double param_5,uint param_6,uint param_7,longlong param_8,undefined8 param_9)

{
  ushort uVar1;
  uint uVar2;
  uint *puVar3;
  double dVar4;
  undefined8 uVar5;
  undefined4 uVar6;
  int iVar7;
  uint uVar8;
  uint uVar9;
  uint uVar10;
  uint extraout_EAX;
  ulonglong uVar11;
  undefined4 extraout_var;
  undefined4 *puVar12;
  longlong lVar13;
  undefined8 uVar14;
  uint uVar15;
  longlong lVar16;
  undefined4 *puVar17;
  ulonglong uVar18;
  byte *pbVar19;
  size_t sVar20;
  longlong lVar21;
  undefined8 *puVar22;
  undefined4 uVar23;
  undefined4 uVar24;
  undefined4 uVar25;
  int local_res18;
  uint local_res20;
  uint local_b8;
  uint local_b4;
  uint local_b0;
  longlong local_a8;
  uint local_a0;
  undefined4 *local_90;
  longlong local_80;
  undefined8 *local_78;
  
  lVar21 = 0;
  if ((param_3 == (int *)0x0) || (local_res18 = 1, param_4 == (int *)0x0)) {
    local_res18 = 0;
  }
  local_b4 = (uint)(param_3 != (int *)0x0);
  local_b0 = (uint)(param_4 != (int *)0x0);
  uVar11 = FUN_180010150(param_1,param_3,param_4,param_5);
  uVar18 = uVar11 & 0xffffffff;
  if ((int)uVar11 != 0) {
    uVar6 = FUN_180014810((int)uVar11);
    return CONCAT44(extraout_var,uVar6);
  }
  if ((param_7 & 0xffff0000) != 0) {
    return 0xffffd8f5;
  }
  lVar16 = lVar21;
  local_a8 = lVar21;
  if (param_3 != (int *)0x0) {
    local_a8 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8);
    lVar16 = (longlong)*param_3 * 0x280 + *(longlong *)(param_1 + 0x120);
  }
  local_80 = lVar21;
  if (param_4 != (int *)0x0) {
    local_80 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_4 * 8);
    lVar21 = (longlong)*param_4 * 0x280 + *(longlong *)(param_1 + 0x120);
  }
  puVar12 = (undefined4 *)FUN_180020230(0x4b8);
  if (puVar12 == (undefined4 *)0x0) {
    return 0xffffd8f8;
  }
  puVar17 = (undefined4 *)0x0;
  local_90 = (undefined4 *)0x0;
  puVar12[0x10d] = 0;
  puVar12[0x10e] = 1;
  puVar12[0x128] = 1;
  if (param_6 == 0) {
    uVar8 = 0;
    if (param_3 != (int *)0x0) {
      lVar13 = FUN_180010310(*(longlong *)(lVar16 + 0x210),(uint)(longlong)param_5);
      uVar8 = (uint)lVar13;
    }
    uVar11 = (ulonglong)param_5;
    if (param_4 != (int *)0x0) {
      lVar13 = FUN_180010310(*(longlong *)(lVar21 + 0x210),(uint)uVar11);
      param_6 = (uint)lVar13;
    }
    if (param_6 < uVar8) {
      param_6 = uVar8;
    }
    if (param_6 == 0) {
      param_6 = (int)((uVar11 & 0xffffffff) / 100) * 2;
    }
  }
  uVar11 = 0x48;
  local_res20 = 0x10;
  pbVar19 = &DAT_0000000c;
  if (param_3 == (int *)0x0) {
    local_a0 = (uint)puVar17;
    uVar8 = 0x10;
    local_b8 = 0x10;
  }
  else {
    local_a0 = param_3[1];
    local_b8 = FUN_18000fdc0(param_3[2]);
    puVar12[0x77] = (int)puVar17;
    puVar3 = *(uint **)(param_3 + 6);
    if (puVar3 == (uint *)0x0) {
      puVar22 = (undefined8 *)0x20;
      iVar7 = 0;
    }
    else {
      sVar20 = uVar11 & 0xffffffff;
      if (*puVar3 <= (uint)uVar11) {
        sVar20 = (size_t)*puVar3;
      }
      puVar17 = puVar12 + 0x84;
      memcpy(puVar17,puVar3,sVar20);
      *puVar17 = 0x48;
      *(undefined4 **)(puVar12 + 0x82) = puVar17;
      uVar8 = puVar12[0x87];
      puVar12[0x79] = uVar8;
      if ((uVar8 & 1) == 0) {
        iVar7 = puVar12[0x77];
      }
      else {
        puVar12[0x128] = 6;
        iVar7 = 1;
        puVar12[0x77] = 1;
      }
      if (((uVar8 & 0x10) != 0) && (puVar12[0x8e] - 1 < 7)) {
        puVar12[0x128] = puVar12[0x8e];
      }
      pbVar19 = (byte *)(puVar12 + 0x87);
      local_b4 = ~(uVar8 >> 1) & 1;
      puVar22 = (undefined8 *)(puVar12 + 0x8c);
      local_90 = puVar17;
    }
    uVar6 = 0;
    if (iVar7 == 1) {
      uVar6 = 0x40000;
    }
    puVar12[0x78] = uVar6;
    if ((((*(int *)(param_1 + 0x128) != 0) || (param_8 == 0)) ||
        ((puVar17 != (undefined4 *)0x0 && ((*pbVar19 & 8) != 0)))) || (local_res18 != 0)) {
      puVar12[0x78] = 0;
    }
    iVar7 = FUN_180020730();
    if ((((6 < iVar7) && (puVar12[0x77] == 0)) && (local_90 != (undefined4 *)0x0)) &&
       ((*pbVar19 & 0x40) != 0)) {
      puVar12[0x78] = puVar12[0x78] | 0x88000000;
    }
    if (*(int *)(lVar16 + 0x278) != 0) {
      puVar12[0x78] = puVar12[0x78] | 0x20000;
    }
    *(longlong *)(puVar12 + 0x7a) = lVar16;
    uVar14 = *(undefined8 *)(param_3 + 2);
    *(undefined8 *)(puVar12 + 0x7c) = *(undefined8 *)param_3;
    *(undefined8 *)(puVar12 + 0x7e) = uVar14;
    uVar14 = *(undefined8 *)(param_3 + 4);
    uVar5 = *(undefined8 *)(param_3 + 6);
    puVar12[0x9a] = (uint)(param_8 == 0);
    puVar12[0x9b] = local_res18;
    *(double *)(puVar12 + 0x98) = param_5;
    *(undefined8 *)(puVar12 + 0x80) = uVar14;
    *(undefined8 *)(puVar12 + 0x82) = uVar5;
    puVar12[0x96] = param_6;
    puVar12[0x9c] = *(int *)(param_1 + 0x128);
    uVar8 = FUN_18000e040(local_a8,(longlong)puVar12,0);
    uVar18 = (ulonglong)uVar8;
    if (uVar8 != 0) goto LAB_180011067;
    uVar14 = FUN_180013850((short *)(puVar12 + 0x62));
    uVar8 = FUN_1800030c0((uint)uVar14,local_b8);
    if ((local_90 != (undefined4 *)0x0) && ((*pbVar19 & 2) != 0)) {
      *(undefined8 *)(puVar12 + 0x120) = *puVar22;
      *(undefined8 *)(puVar12 + 0x122) = param_9;
    }
    iVar7 = (**(code **)(**(longlong **)(puVar12 + 0x5c) + 0x70))
                      (*(longlong **)(puVar12 + 0x5c),&DAT_180023eb8,puVar12 + 0xac);
    if (iVar7 < 0) goto LAB_180010b89;
    uVar18 = 0;
    if (puVar12[0x9a] == 1) {
      uVar9 = FUN_18000e020(((uint)puVar12[0x76] / 6) * 2);
      uVar1 = *(ushort *)(puVar12 + 0x65);
      lVar16 = *(longlong *)(puVar12 + 0xa6);
      lVar13 = FUN_180020230(0x20);
      *(longlong *)(puVar12 + 0xa8) = lVar13;
      if (lVar13 == 0) {
LAB_180010c01:
        FUN_18000e490(puVar12);
        return 0xffffd8f8;
      }
      uVar15 = (uint)(uVar1 >> (lVar16 != 0));
      lVar16 = FUN_180020230(uVar15 * uVar9);
      *(longlong *)(puVar12 + 0xaa) = lVar16;
      if (lVar16 == 0) goto LAB_180010c01;
      uVar14 = FUN_180006c50(*(uint **)(puVar12 + 0xa8),uVar15,uVar9,lVar16);
      if ((int)uVar14 != 0) {
        FUN_18000e490(puVar12);
        return 0xffffd8fe;
      }
    }
  }
  sVar20 = 0x48;
  pbVar19 = &DAT_0000000c;
  puVar17 = (undefined4 *)0x0;
  if (param_4 == (int *)0x0) {
    uVar9 = 0;
    uVar15 = 0x10;
  }
  else {
    uVar9 = param_4[1];
    local_res20 = FUN_18000fdc0(param_4[2]);
    puVar12[0xcd] = 0;
    puVar3 = *(uint **)(param_4 + 6);
    if (puVar3 == (uint *)0x0) {
      local_78 = (undefined8 *)0x18;
      iVar7 = 0;
    }
    else {
      if (*puVar3 < 0x49) {
        sVar20 = (size_t)*puVar3;
      }
      puVar17 = puVar12 + 0xda;
      memcpy(puVar17,puVar3,sVar20);
      *puVar17 = 0x48;
      *(undefined4 **)(puVar12 + 0xd8) = puVar17;
      uVar15 = puVar12[0xdd];
      puVar12[0xcf] = uVar15;
      if ((uVar15 & 1) == 0) {
        iVar7 = puVar12[0xcd];
      }
      else {
        puVar12[0x128] = 6;
        iVar7 = 1;
        puVar12[0xcd] = 1;
      }
      if (((uVar15 & 0x10) != 0) && (puVar12[0xe4] - 1 < 7)) {
        puVar12[0x128] = puVar12[0xe4];
      }
      local_78 = (undefined8 *)(puVar12 + 0xe0);
      local_b0 = ~(uVar15 >> 1) & 1;
      pbVar19 = (byte *)(puVar12 + 0xdd);
    }
    uVar6 = 0;
    if (iVar7 == 1) {
      uVar6 = 0x40000;
    }
    puVar12[0xce] = uVar6;
    if ((((*(int *)(param_1 + 0x128) != 0) || (param_8 == 0)) ||
        ((puVar17 != (undefined4 *)0x0 && ((*pbVar19 & 8) != 0)))) || (local_res18 != 0)) {
      puVar12[0xce] = 0;
    }
    iVar7 = FUN_180020730();
    if ((((6 < iVar7) && (puVar12[0xcd] == 0)) && (puVar17 != (undefined4 *)0x0)) &&
       ((*pbVar19 & 0x40) != 0)) {
      puVar12[0xce] = puVar12[0xce] | 0x88000000;
    }
    *(longlong *)(puVar12 + 0xd0) = lVar21;
    uVar14 = *(undefined8 *)(param_4 + 2);
    *(undefined8 *)(puVar12 + 0xd2) = *(undefined8 *)param_4;
    *(undefined8 *)(puVar12 + 0xd4) = uVar14;
    uVar14 = *(undefined8 *)(param_4 + 4);
    uVar5 = *(undefined8 *)(param_4 + 6);
    puVar12[0xf0] = (uint)(param_8 == 0);
    *(undefined8 *)(puVar12 + 0xd6) = uVar14;
    *(undefined8 *)(puVar12 + 0xd8) = uVar5;
    puVar12[0xf1] = local_res18;
    *(double *)(puVar12 + 0xee) = param_5;
    puVar12[0xec] = param_6;
    puVar12[0xf2] = *(int *)(param_1 + 0x128);
    uVar15 = FUN_18000e040(local_80,(longlong)puVar12,1);
    uVar18 = (ulonglong)uVar15;
    if (uVar15 != 0) goto LAB_180011067;
    uVar14 = FUN_180013850((short *)(puVar12 + 0xb8));
    uVar15 = FUN_1800030c0((uint)uVar14,local_res20);
    if ((puVar17 != (undefined4 *)0x0) && ((*pbVar19 & 2) != 0)) {
      *(undefined8 *)(puVar12 + 0x11c) = *local_78;
      *(undefined8 *)(puVar12 + 0x11e) = param_9;
    }
    iVar7 = (**(code **)(**(longlong **)(puVar12 + 0xb2) + 0x70))
                      (*(longlong **)(puVar12 + 0xb2),&DAT_180023ea8,puVar12 + 0x102);
    uVar18 = 0;
    if (iVar7 < 0) {
LAB_180010b89:
      FUN_180014510(iVar7);
      FUN_18000e490(puVar12);
      return 0xffffd8f1;
    }
  }
  if (((param_3 != (int *)0x0) && (param_4 != (int *)0x0)) &&
     ((local_90 != (undefined4 *)0x0 && (puVar17 != (undefined4 *)0x0)))) {
    if ((*(byte *)(local_90 + 3) & 8) == 0) {
      if ((puVar17[3] & 8) != 0) goto LAB_18001107c;
    }
    else if ((puVar17[3] & 8) == 0) {
LAB_18001107c:
      FUN_18000e490(puVar12);
      return 0xffffd8f5;
    }
  }
  puVar12[0x124] = (uint)(param_8 == 0);
  lVar21 = 0x48;
  if (param_8 == 0) {
    lVar21 = 0xa8;
  }
  FUN_180006e70(puVar12,lVar21 + param_1,param_8,param_9);
  FUN_180013450(puVar12 + 0x129,1);
  FUN_180003950((double *)(puVar12 + 0x14),param_5);
  lVar21 = 0x3d4;
  if (param_4 == (int *)0x0) {
    lVar21 = 0x27c;
  }
  uVar2 = *(uint *)(lVar21 + (longlong)puVar12);
  if (param_3 == (int *)0x0) {
    uVar10 = 0;
    if (((param_4 != (int *)0x0) && (uVar10 = 0, puVar12[0xf4] == 1)) &&
       ((puVar12[0xce] == 0 || (uVar10 = 0, ((uint)puVar12[0xce] >> 0x12 & 1) == 0))))
    goto LAB_180010fd7;
  }
  else {
LAB_180010fd7:
    uVar10 = 1;
  }
  uVar6 = 0;
  uVar25 = 0;
  puVar12[0x10c] = uVar10;
  if ((local_b4 != 0) || (local_b0 != 0)) {
    FUN_180006290(puVar12 + 0x1a,local_a0,local_b8,uVar8,uVar9,local_res20,uVar15,param_5,param_7,
                  param_6,uVar2,uVar10,param_8,param_9);
    uVar18 = (ulonglong)extraout_EAX;
    if (extraout_EAX != 0) {
LAB_180011067:
      FUN_18000e490(puVar12);
      return uVar18;
    }
    uVar18 = 0;
    if (local_b4 != 0) {
      uVar8 = FUN_180006270((longlong)(puVar12 + 0x1a));
      dVar4 = (double)uVar8 / param_5;
      goto LAB_1800110b1;
    }
  }
  dVar4 = 0.0;
LAB_1800110b1:
  if (param_3 == (int *)0x0) {
    uVar23 = 0;
    uVar24 = 0;
  }
  else {
    uVar23 = (undefined4)*(undefined8 *)(puVar12 + 0x74);
    uVar24 = (undefined4)((ulonglong)*(undefined8 *)(puVar12 + 0x74) >> 0x20);
  }
  *(double *)(puVar12 + 0xe) = (double)CONCAT44(uVar24,uVar23) + dVar4;
  if (local_b0 == 0) {
    dVar4 = 0.0;
  }
  else {
    uVar8 = FUN_180006280((longlong)(puVar12 + 0x1a));
    dVar4 = (double)uVar8 / param_5;
  }
  if (param_4 != (int *)0x0) {
    uVar6 = (undefined4)*(undefined8 *)(puVar12 + 0xca);
    uVar25 = (undefined4)((ulonglong)*(undefined8 *)(puVar12 + 0xca) >> 0x20);
  }
  *(double *)(puVar12 + 0x12) = param_5;
  *(double *)(puVar12 + 0x10) = (double)CONCAT44(uVar25,uVar6) + dVar4;
  *param_2 = puVar12;
  return uVar18;
}



/* ========================================================================
   ENTRY: 180011160
   NAME : FUN_180011160
   SIG  : undefined8 __fastcall FUN_180011160(uint param_1)
   ======================================================================== */

undefined8 FUN_180011160(uint param_1)

{
  undefined8 uVar1;
  uint uVar2;
  
  uVar2 = param_1 & 0x7fffffff;
  if (0x10 < uVar2) {
    if ((uVar2 - 0x20 & 0xffffffdf) != 0) {
      uVar1 = 0x18;
      if (uVar2 != 0x10000) {
        uVar1 = 0;
      }
      return uVar1;
    }
    return 8;
  }
  if (uVar2 == 0x10) {
    return 0x10;
  }
  if (uVar2 == 1) {
    return 0x40;
  }
  if ((uVar2 != 2) && (uVar2 != 4)) {
    if (uVar2 != 8) {
      return 0;
    }
    return 0x18;
  }
  return 0x20;
}



/* ========================================================================
   ENTRY: 1800111d0
   NAME : PaWasapiWinrt_SetDefaultDeviceId
   SIG  : undefined8 __fastcall PaWasapiWinrt_SetDefaultDeviceId(void)
   ======================================================================== */

undefined8 PaWasapiWinrt_SetDefaultDeviceId(void)

{
                    /* 0x111d0  67  PaWasapiWinrt_SetDefaultDeviceId
                       0x111d0  69  PaWasapiWinrt_PopulateDeviceList */
  return 0xffffd90b;
}



/* ========================================================================
   ENTRY: 1800111e0
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x0001800111e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 1800111f0
   NAME : PaWasapi_GetAudioClient
   SIG  : undefined8 __fastcall PaWasapi_GetAudioClient(longlong param_1, undefined8 * param_2, int param_3)
   ======================================================================== */

undefined8 PaWasapi_GetAudioClient(longlong param_1,undefined8 *param_2,int param_3)

{
  longlong lVar1;
  
                    /* 0x111f0  56  PaWasapi_GetAudioClient */
  if (param_1 == 0) {
    return 0xffffd8fc;
  }
  if (param_2 == (undefined8 *)0x0) {
    return 0xffffd8f1;
  }
  lVar1 = 0x2c8;
  if (param_3 != 1) {
    lVar1 = 0x170;
  }
  *param_2 = *(undefined8 *)(lVar1 + param_1);
  return 0;
}



/* ========================================================================
   ENTRY: 180011230
   NAME : PaWasapi_GetDeviceCurrentFormat
   SIG  : uint __fastcall PaWasapi_GetDeviceCurrentFormat(longlong param_1, void * param_2, uint param_3, int param_4)
   ======================================================================== */

uint PaWasapi_GetDeviceCurrentFormat(longlong param_1,void *param_2,uint param_3,int param_4)

{
  uint uVar1;
  longlong lVar2;
  
                    /* 0x11230  58  PaWasapi_GetDeviceCurrentFormat */
  if (param_1 == 0) {
    return 0xffffd8fc;
  }
  uVar1 = 0x28;
  if (param_3 < 0x28) {
    uVar1 = param_3;
  }
  lVar2 = 0x2e0;
  if (param_4 != 1) {
    lVar2 = 0x188;
  }
  memcpy(param_2,(void *)(lVar2 + param_1),(ulonglong)uVar1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180011290
   NAME : PaWasapi_GetDeviceDefaultFormat
   SIG  : ulonglong __fastcall PaWasapi_GetDeviceDefaultFormat(void * param_1, uint param_2, int param_3)
   ======================================================================== */

ulonglong PaWasapi_GetDeviceDefaultFormat(void *param_1,uint param_2,int param_3)

{
  uint uVar1;
  ulonglong uVar2;
  longlong local_res8;
  
                    /* 0x11290  59  PaWasapi_GetDeviceDefaultFormat */
  if (param_1 == (void *)0x0) {
    return 0xffffd90c;
  }
  if (param_2 == 0) {
    return 0xffffd8fa;
  }
  uVar2 = FUN_180013ec0(&local_res8,param_3);
  if ((int)uVar2 == 0) {
    uVar1 = 0x28;
    if (param_2 < 0x28) {
      uVar1 = param_2;
    }
    uVar2 = (ulonglong)uVar1;
    memcpy(param_1,(void *)(local_res8 + 0x220),(ulonglong)uVar1);
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180011310
   NAME : PaWasapi_GetDeviceMixFormat
   SIG  : ulonglong __fastcall PaWasapi_GetDeviceMixFormat(void * param_1, uint param_2, int param_3)
   ======================================================================== */

ulonglong PaWasapi_GetDeviceMixFormat(void *param_1,uint param_2,int param_3)

{
  uint uVar1;
  ulonglong uVar2;
  longlong local_res8;
  
                    /* 0x11310  60  PaWasapi_GetDeviceMixFormat */
  if (param_1 == (void *)0x0) {
    return 0xffffd90c;
  }
  if (param_2 == 0) {
    return 0xffffd8fa;
  }
  uVar2 = FUN_180013ec0(&local_res8,param_3);
  if ((int)uVar2 == 0) {
    uVar1 = 0x28;
    if (param_2 < 0x28) {
      uVar1 = param_2;
    }
    uVar2 = (ulonglong)uVar1;
    memcpy(param_1,(void *)(local_res8 + 0x248),(ulonglong)uVar1);
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180011390
   NAME : PaWasapi_GetDeviceRole
   SIG  : ulonglong __fastcall PaWasapi_GetDeviceRole(int param_1)
   ======================================================================== */

ulonglong PaWasapi_GetDeviceRole(int param_1)

{
  ulonglong uVar1;
  longlong local_res10 [3];
  
                    /* 0x11390  61  PaWasapi_GetDeviceRole */
  uVar1 = FUN_180013ec0(local_res10,param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (ulonglong)*(uint *)(local_res10[0] + 0x274);
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800113c0
   NAME : PaWasapi_GetFramesPerHostBuffer
   SIG  : undefined8 __fastcall PaWasapi_GetFramesPerHostBuffer(longlong param_1, undefined4 * param_2, undefined4 * param_3)
   ======================================================================== */

undefined8 PaWasapi_GetFramesPerHostBuffer(longlong param_1,undefined4 *param_2,undefined4 *param_3)

{
                    /* 0x113c0  64  PaWasapi_GetFramesPerHostBuffer */
  if (param_1 == 0) {
    return 0xffffd8fc;
  }
  if (param_2 != (undefined4 *)0x0) {
    *param_2 = *(undefined4 *)(param_1 + 0x1d8);
  }
  if (param_3 != (undefined4 *)0x0) {
    *param_3 = *(undefined4 *)(param_1 + 0x330);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800113f0
   NAME : PaWasapi_GetIMMDevice
   SIG  : undefined8 __fastcall PaWasapi_GetIMMDevice(int param_1, undefined8 * param_2)
   ======================================================================== */

undefined8 PaWasapi_GetIMMDevice(int param_1,undefined8 *param_2)

{
  undefined8 uVar1;
  undefined8 *local_res10 [3];
  
                    /* 0x113f0  70  PaWasapi_GetIMMDevice */
  if (param_2 == (undefined8 *)0x0) {
    return 0xffffd90c;
  }
  uVar1 = FUN_180013ec0((longlong *)local_res10,param_1);
  if ((int)uVar1 == 0) {
    uVar1 = 0;
    *param_2 = *local_res10[0];
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180011430
   NAME : PaWasapi_GetJackCount
   SIG  : undefined8 __fastcall PaWasapi_GetJackCount(int param_1, undefined4 * param_2)
   ======================================================================== */

undefined8 PaWasapi_GetJackCount(int param_1,undefined4 *param_2)

{
  int iVar1;
  undefined8 uVar2;
  undefined4 local_res10 [2];
  longlong *local_res18;
  longlong *local_res20;
  longlong *local_38;
  longlong *local_30;
  longlong *local_28;
  undefined8 *local_20;
  
                    /* 0x11430  65  PaWasapi_GetJackCount */
  local_res18 = (longlong *)0x0;
  local_res20 = (longlong *)0x0;
  local_38 = (longlong *)0x0;
  local_30 = (longlong *)0x0;
  local_28 = (longlong *)0x0;
  local_res10[0] = 0;
  if (param_2 == (undefined4 *)0x0) {
    return 0xffffd8f1;
  }
  uVar2 = FUN_180013ec0((longlong *)&local_20,param_1);
  if ((int)uVar2 == 0) {
    iVar1 = (**(code **)(*(longlong *)*local_20 + 0x18))
                      ((longlong *)*local_20,&DAT_180023ec8,1,0,&local_res18);
    if (-1 < iVar1) {
      iVar1 = (**(code **)(*local_res18 + 0x20))(local_res18,0,&local_res20);
      if (-1 < iVar1) {
        iVar1 = (**(code **)(*local_res20 + 0x40))(local_res20,&local_38);
        if (iVar1 == -0x7ff8fffd) {
          iVar1 = -0x7fffbffe;
        }
        else if (-1 < iVar1) {
          iVar1 = (**(code **)*local_38)(local_38,&DAT_180023ed8,&local_30);
          if (-1 < iVar1) {
            iVar1 = (**(code **)(*local_30 + 0x68))(local_30,1,&DAT_180023ee8,&local_28);
            if (-1 < iVar1) {
              iVar1 = (**(code **)(*local_28 + 0x18))(local_28,local_res10);
              if (-1 < iVar1) {
                *param_2 = local_res10[0];
              }
            }
          }
        }
      }
    }
    if (local_res18 != (longlong *)0x0) {
      (**(code **)(*local_res18 + 0x10))();
      local_res18 = (longlong *)0x0;
    }
    if (local_res20 != (longlong *)0x0) {
      (**(code **)(*local_res20 + 0x10))();
      local_res20 = (longlong *)0x0;
    }
    if (local_38 != (longlong *)0x0) {
      (**(code **)(*local_38 + 0x10))();
      local_38 = (longlong *)0x0;
    }
    if (local_30 != (longlong *)0x0) {
      (**(code **)(*local_30 + 0x10))();
      local_30 = (longlong *)0x0;
    }
    if (local_28 != (longlong *)0x0) {
      (**(code **)(*local_28 + 0x10))();
      local_28 = (longlong *)0x0;
    }
    FUN_180014510(iVar1);
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 1800115d0
   NAME : PaWasapi_GetJackDescription
   SIG  : undefined8 __fastcall PaWasapi_GetJackDescription(int param_1, undefined4 param_2, undefined4 * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 PaWasapi_GetJackDescription(int param_1,undefined4 param_2,undefined4 *param_3)

{
  int iVar1;
  undefined8 uVar2;
  undefined1 auStack_b8 [32];
  longlong **local_98;
  longlong *local_88;
  longlong *local_80;
  longlong *local_78;
  longlong *local_70;
  longlong *local_68;
  undefined8 *local_60;
  undefined8 local_58;
  undefined4 uStack_50;
  undefined4 uStack_4c;
  int local_48;
  undefined8 local_44;
  ulonglong local_38;
  
                    /* 0x115d0  66  PaWasapi_GetJackDescription */
  local_38 = DAT_18002b580 ^ (ulonglong)auStack_b8;
  local_88 = (longlong *)0x0;
  local_80 = (longlong *)0x0;
  local_58 = 0;
  uStack_50 = 0;
  local_78 = (longlong *)0x0;
  uStack_4c = 0;
  local_48 = 0;
  local_44 = 0;
  local_70 = (longlong *)0x0;
  local_68 = (longlong *)0x0;
  uVar2 = FUN_180013ec0((longlong *)&local_60,param_1);
  if ((int)uVar2 == 0) {
    local_98 = &local_88;
    iVar1 = (**(code **)(*(longlong *)*local_60 + 0x18))((longlong *)*local_60,&DAT_180023ec8,1,0);
    if (-1 < iVar1) {
      iVar1 = (**(code **)(*local_88 + 0x20))(local_88,0,&local_80);
      if (-1 < iVar1) {
        iVar1 = (**(code **)(*local_80 + 0x40))(local_80,&local_78);
        if (iVar1 == -0x7ff8fffd) {
          iVar1 = -0x7fffbffe;
        }
        else if (-1 < iVar1) {
          iVar1 = (**(code **)*local_78)(local_78,&DAT_180023ed8,&local_70);
          if (-1 < iVar1) {
            iVar1 = (**(code **)(*local_70 + 0x68))(local_70,1,&DAT_180023ee8,&local_68);
            if (-1 < iVar1) {
              iVar1 = (**(code **)(*local_68 + 0x20))(local_68,param_2,&local_58);
              if (-1 < iVar1) {
                *param_3 = (undefined4)local_58;
                param_3[1] = local_58._4_4_;
                uVar2 = FUN_180013d10(uStack_50);
                param_3[2] = (int)uVar2;
                uVar2 = FUN_180013da0(local_48);
                param_3[4] = (int)uVar2;
                uVar2 = FUN_180013dd0(uStack_4c);
                param_3[3] = (int)uVar2;
                param_3[6] = local_44._4_4_;
                uVar2 = FUN_180013da0((int)local_44);
                param_3[5] = (int)uVar2;
              }
            }
          }
        }
      }
    }
    if (local_88 != (longlong *)0x0) {
      (**(code **)(*local_88 + 0x10))();
      local_88 = (longlong *)0x0;
    }
    if (local_80 != (longlong *)0x0) {
      (**(code **)(*local_80 + 0x10))();
      local_80 = (longlong *)0x0;
    }
    if (local_78 != (longlong *)0x0) {
      (**(code **)(*local_78 + 0x10))();
      local_78 = (longlong *)0x0;
    }
    if (local_70 != (longlong *)0x0) {
      (**(code **)(*local_70 + 0x10))();
      local_70 = (longlong *)0x0;
    }
    if (local_68 != (longlong *)0x0) {
      (**(code **)(*local_68 + 0x10))();
      local_68 = (longlong *)0x0;
    }
    FUN_180014510(iVar1);
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 1800117d0
   NAME : FUN_1800117d0
   SIG  : ulonglong __fastcall FUN_1800117d0(longlong * param_1, undefined4 param_2)
   ======================================================================== */

ulonglong FUN_1800117d0(longlong *param_1,undefined4 param_2)

{
  int iVar1;
  undefined8 uVar2;
  longlong lVar3;
  ulonglong uVar4;
  undefined4 *puVar5;
  ulonglong uVar6;
  
  iVar1 = FUN_180020730();
  if (iVar1 < 6) {
    return 0;
  }
  uVar2 = FUN_180012f70();
  if ((int)uVar2 == 0) {
    return 0;
  }
  lVar3 = FUN_180020230(0x130);
  if (lVar3 != 0) {
    uVar4 = FUN_180020180(0xd,(undefined4 *)(lVar3 + 0x110));
    uVar6 = uVar4 & 0xffffffff;
    if ((int)uVar4 != 0) goto LAB_180011821;
    puVar5 = FUN_1800011b0();
    *(undefined4 **)(lVar3 + 0x108) = puVar5;
    if (puVar5 != (undefined4 *)0x0) {
      *param_1 = lVar3;
      *(undefined4 *)(lVar3 + 8) = 1;
      *(undefined4 *)(*param_1 + 0xc) = 0xd;
      *(char **)(*param_1 + 0x10) = "Windows WASAPI";
      *(undefined4 *)(*param_1 + 0x18) = 0;
      *(undefined4 *)(*param_1 + 0x1c) = 0xffffffff;
      *(undefined4 *)(*param_1 + 0x20) = 0xffffffff;
      *(code **)(*param_1 + 0x30) = FUN_1800134a0;
      *(code **)(*param_1 + 0x38) = FUN_180010700;
      *(code **)(*param_1 + 0x40) = FUN_18000ff30;
      uVar4 = FUN_18000ecc0(lVar3,param_2);
      uVar6 = uVar4 & 0xffffffff;
      if ((int)uVar4 == 0) {
        uVar2 = FUN_180013670();
        *(int *)(lVar3 + 0x128) = (int)uVar2;
        FUN_180013420();
        FUN_180006e10((undefined8 *)(lVar3 + 0x48),FUN_18000e490,FUN_1800130a0,FUN_18000e030,
                      FUN_18000e030,&LAB_180010210,FUN_180010140,&LAB_18000bf70,&LAB_180008780,
                      &LAB_180006df0,&LAB_180006e00,&LAB_180006df0,&LAB_180006e00);
        FUN_180006e10((undefined8 *)(lVar3 + 0xa8),FUN_18000e490,FUN_1800130a0,FUN_18000e030,
                      FUN_18000e030,&LAB_180010210,FUN_180010140,&LAB_18000bf70,&LAB_180006de0,
                      FUN_1800129a0,FUN_180013940,FUN_18000fe30,FUN_18000fec0);
        return 0;
      }
      goto LAB_180011821;
    }
  }
  uVar6 = 0xffffd8f8;
LAB_180011821:
  FUN_1800134a0(lVar3);
  return uVar6;
}



/* ========================================================================
   ENTRY: 180011a20
   NAME : FUN_180011a20
   SIG  : void * __fastcall FUN_180011a20(void * param_1, size_t param_2)
   ======================================================================== */

void * FUN_180011a20(void *param_1,size_t param_2)

{
  void *pvVar1;
  
  pvVar1 = realloc(param_1,param_2);
  if (pvVar1 == (void *)0x0) {
    free(param_1);
    pvVar1 = (void *)0x0;
  }
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180011a50
   NAME : PaWasapi_SetStreamStateHandler
   SIG  : undefined8 __fastcall PaWasapi_SetStreamStateHandler(longlong param_1, undefined8 param_2, undefined8 param_3)
   ======================================================================== */

undefined8 PaWasapi_SetStreamStateHandler(longlong param_1,undefined8 param_2,undefined8 param_3)

{
                    /* 0x11a50  68  PaWasapi_SetStreamStateHandler */
  if (param_1 == 0) {
    return 0xffffd8fc;
  }
  *(undefined8 *)(param_1 + 0x4a8) = param_2;
  *(undefined8 *)(param_1 + 0x4b0) = param_3;
  return 0;
}



/* ========================================================================
   ENTRY: 180011a70
   NAME : PaWasapi_ThreadPriorityBoost
   SIG  : undefined8 __fastcall PaWasapi_ThreadPriorityBoost(longlong * param_1, uint param_2)
   ======================================================================== */

undefined8 PaWasapi_ThreadPriorityBoost(longlong *param_1,uint param_2)

{
  undefined8 uVar1;
  longlong local_res8 [4];
  
                    /* 0x11a70  62  PaWasapi_ThreadPriorityBoost */
  if (param_1 == (longlong *)0x0) {
    return 0xffffd8f1;
  }
  uVar1 = FUN_180010280(param_2,local_res8);
  if ((int)uVar1 == 0) {
    *param_1 = local_res8[0];
    uVar1 = 0;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180011ab0
   NAME : PaWasapi_ThreadPriorityRevert
   SIG  : undefined8 __fastcall PaWasapi_ThreadPriorityRevert(longlong param_1)
   ======================================================================== */

undefined8 PaWasapi_ThreadPriorityRevert(longlong param_1)

{
                    /* 0x11ab0  63  PaWasapi_ThreadPriorityRevert */
  if (param_1 == 0) {
    return 0xffffd8f1;
  }
  FUN_180010300();
  return 0;
}



/* ========================================================================
   ENTRY: 180011ad0
   NAME : PaWasapi_UpdateDeviceList
   SIG  : undefined8 __fastcall PaWasapi_UpdateDeviceList(void)
   ======================================================================== */

undefined8 PaWasapi_UpdateDeviceList(void)

{
                    /* 0x11ad0  57  PaWasapi_UpdateDeviceList */
  return 0xffffd8fe;
}



/* ========================================================================
   ENTRY: 180011ae0
   NAME : FUN_180011ae0
   SIG  : undefined8 __fastcall FUN_180011ae0(longlong param_1, undefined4 * param_2)
   ======================================================================== */

undefined8 FUN_180011ae0(longlong param_1,undefined4 *param_2)

{
  HRESULT HVar1;
  
  HVar1 = CoInitializeEx((LPVOID)0x0,2);
  if (HVar1 != -0x7ffefefa) {
    if (HVar1 < 0) {
      return 0;
    }
    *param_2 = 1;
  }
  HVar1 = FUN_180013520(param_1);
  if (HVar1 != 0) {
    CoUninitialize();
    return 0;
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180011b40
   NAME : FUN_180011b40
   SIG  : undefined8 __fastcall FUN_180011b40(longlong param_1)
   ======================================================================== */

undefined8 FUN_180011b40(longlong param_1)

{
  longlong *plVar1;
  int iVar2;
  DWORD DVar3;
  DWORD DVar4;
  undefined8 uVar5;
  HANDLE pvVar6;
  code **ppcVar7;
  code **ppcVar8;
  BOOL bWaitAll;
  bool bVar9;
  bool bVar10;
  int local_res8 [2];
  code *local_48;
  longlong lStack_40;
  code *local_38;
  code *pcStack_30;
  code *local_28;
  code *pcStack_20;
  
  local_res8[0] = 0;
  FUN_1800106d0(param_1,2,0);
  uVar5 = FUN_180011ae0(param_1,local_res8);
  if ((int)uVar5 == 0) {
    return 0xffffd8f1;
  }
  iVar2 = 0;
  bWaitAll = 0;
  if ((((*(longlong *)(param_1 + 0x180) != 0) && (*(longlong *)(param_1 + 0x2d8) != 0)) &&
      (*(int *)(param_1 + 0x1dc) == 1)) && (bWaitAll = 0, *(int *)(param_1 + 0x334) == 1)) {
    bWaitAll = 1;
  }
  local_48 = FUN_1800136a0;
  lStack_40 = param_1;
  ppcVar7 = &local_48;
  if (*(code **)(param_1 + 0x480) != (code *)0x0) {
    ppcVar7 = (code **)(param_1 + 0x480);
  }
  ppcVar8 = &local_48;
  if (*(code **)(param_1 + 0x470) != (code *)0x0) {
    ppcVar8 = (code **)(param_1 + 0x470);
  }
  local_38 = *ppcVar7;
  pcStack_30 = ppcVar7[1];
  local_28 = *ppcVar8;
  pcStack_20 = ppcVar8[1];
  PaWasapi_ThreadPriorityBoost((longlong *)(param_1 + 0x498),*(uint *)(param_1 + 0x4a0));
  bVar9 = *(longlong *)(param_1 + 0x428) == 0;
  if (bVar9) {
    pvVar6 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
    *(HANDLE *)(param_1 + 0x428) = pvVar6;
  }
  pvVar6 = *(HANDLE *)(param_1 + 0x420);
  bVar10 = pvVar6 != (HANDLE)0x0;
  if (!bVar10) {
    pvVar6 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
    *(HANDLE *)(param_1 + 0x420) = pvVar6;
  }
  if ((*(longlong *)(param_1 + 0x428) != 0) && (pvVar6 != (HANDLE)0x0)) {
    *(undefined4 *)(param_1 + 0x434) = 1;
    SetEvent(*(HANDLE *)(param_1 + 0x450));
    plVar1 = *(longlong **)(param_1 + 0x180);
    iVar2 = 0;
    if (((plVar1 == (longlong *)0x0) ||
        (((bVar10 ||
          (iVar2 = (**(code **)(*plVar1 + 0x68))(plVar1,*(undefined8 *)(param_1 + 0x420)),
          iVar2 == 0)) &&
         (iVar2 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x50))(), iVar2 == 0)))) &&
       ((plVar1 = *(longlong **)(param_1 + 0x2d8), plVar1 == (longlong *)0x0 ||
        ((((!bVar9 ||
           (iVar2 = (**(code **)(*plVar1 + 0x68))(plVar1,*(undefined8 *)(param_1 + 0x428)),
           iVar2 == 0)) &&
          (iVar2 = FUN_180012890(param_1,(longlong)&local_38,*(int *)(param_1 + 0x3d4)), iVar2 == 0)
          ) && (iVar2 = (**(code **)(**(longlong **)(param_1 + 0x2d8) + 0x50))(), iVar2 == 0)))))) {
      FUN_1800106d0(param_1,4,0);
      DVar3 = WaitForMultipleObjects(2,(HANDLE *)(param_1 + 0x420),bWaitAll,10000);
      DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
      while (DVar4 == 0x102) {
        if (DVar3 == 0) {
          if (*(longlong *)(param_1 + 0x2c0) != 0) {
            iVar2 = FUN_180012730(param_1,&local_38);
joined_r0x000180011e03:
            if (iVar2 != 0) goto LAB_180011e44;
          }
        }
        else if (DVar3 == 1) {
          if (*(longlong *)(param_1 + 0x418) != 0) {
            iVar2 = FUN_180012890(param_1,(longlong)&local_38,*(int *)(param_1 + 0x3d4));
            goto joined_r0x000180011e03;
          }
        }
        else if (DVar3 == 0x102) break;
        DVar3 = WaitForMultipleObjects(2,(HANDLE *)(param_1 + 0x420),bWaitAll,10000);
        DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
      }
      goto LAB_180011e66;
    }
LAB_180011e44:
    FUN_180014510(iVar2);
  }
  SetEvent(*(HANDLE *)(param_1 + 0x450));
LAB_180011e66:
  FUN_180014480(param_1);
  FUN_18000f5d0(param_1,local_res8[0]);
  *(undefined4 *)(param_1 + 0x434) = 0;
  SetEvent(*(HANDLE *)(param_1 + 0x458));
  FUN_1800106d0(param_1,8,iVar2);
  return 0;
}



/* ========================================================================
   ENTRY: 180011ed0
   NAME : FUN_180011ed0
   SIG  : undefined8 __fastcall FUN_180011ed0(longlong param_1)
   ======================================================================== */

undefined8 FUN_180011ed0(longlong param_1)

{
  undefined4 uVar1;
  undefined4 uVar2;
  int iVar3;
  DWORD DVar4;
  DWORD DVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  LARGE_INTEGER *pLVar8;
  longlong *plVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  undefined4 uVar13;
  undefined4 uVar14;
  int iVar15;
  uint local_res8 [2];
  LARGE_INTEGER local_res10;
  LARGE_INTEGER local_res18;
  uint local_res20;
  int local_c8 [2];
  undefined8 local_c0;
  undefined8 local_b8;
  undefined4 local_b0 [2];
  LARGE_INTEGER local_a8;
  longlong lStack_a0;
  uint local_98;
  uint local_94;
  uint local_90;
  uint local_8c;
  LARGE_INTEGER local_88;
  longlong local_80;
  LARGE_INTEGER local_78;
  LARGE_INTEGER local_70;
  longlong local_68;
  LARGE_INTEGER local_60;
  LARGE_INTEGER LStack_58;
  
  local_c8[0] = 0;
  FUN_1800106d0(param_1,2,0);
  uVar6 = FUN_180011ae0(param_1,local_c8);
  if ((int)uVar6 == 0) {
    return 0xffffd8f1;
  }
  iVar3 = 0;
  local_res20 = FUN_18000e5c0(param_1,&local_98);
  pLVar8 = (LARGE_INTEGER *)(param_1 + 0x480);
  lStack_a0 = param_1;
  local_a8.QuadPart = (LONGLONG)FUN_1800136a0;
  if (pLVar8->QuadPart == 0) {
    pLVar8 = &local_a8;
    plVar9 = &lStack_a0;
  }
  else {
    plVar9 = (longlong *)(param_1 + 0x488);
  }
  local_78.QuadPart = *(LONGLONG *)pLVar8;
  local_70.QuadPart = *(LONGLONG *)pLVar8;
  local_80 = *plVar9;
  local_68 = *plVar9;
  pLVar8 = &local_a8;
  if (((LARGE_INTEGER *)(param_1 + 0x470))->QuadPart != 0) {
    pLVar8 = (LARGE_INTEGER *)(param_1 + 0x470);
  }
  local_60 = *pLVar8;
  LStack_58 = pLVar8[1];
  PaWasapi_ThreadPriorityBoost((longlong *)(param_1 + 0x498),*(uint *)(param_1 + 0x4a0));
  *(undefined4 *)(param_1 + 0x434) = 1;
  SetEvent(*(HANDLE *)(param_1 + 0x450));
  if (*(longlong **)(param_1 + 0x180) == (longlong *)0x0) {
LAB_180011ffd:
    if (*(longlong *)(param_1 + 0x2d8) != 0) {
      if (*(longlong *)(param_1 + 0x180) == 0) {
        local_res8[0] = 0;
        iVar3 = FUN_180014300(param_1,(int *)local_res8);
        if (iVar3 == 0) {
          if (*(int *)(param_1 + 0x430) == 0) {
            uVar12 = local_res8[0];
            if (*(uint *)(param_1 + 0x3d4) <= local_res8[0]) {
              do {
                DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
                if (DVar4 != 0x102) break;
                iVar3 = FUN_180012890(param_1,(longlong)&local_70,*(int *)(param_1 + 0x3d4));
                if (iVar3 != 0) goto LAB_1800120df;
                uVar12 = uVar12 - *(uint *)(param_1 + 0x3d4);
              } while (*(uint *)(param_1 + 0x3d4) <= uVar12);
            }
          }
          else {
            uVar12 = local_res8[0];
            if (local_res8[0] == 0) {
              uVar12 = *(uint *)(param_1 + 0x3d4);
            }
            if ((*(int *)(param_1 + 0x334) == 1) &&
               ((uint)(*(int *)(param_1 + 0x3d4) * 2) <= uVar12)) {
              uVar12 = uVar12 - *(int *)(param_1 + 0x3d4);
            }
            iVar3 = FUN_180012890(param_1,(longlong)&local_70,uVar12);
            if (iVar3 != 0) goto LAB_1800120df;
          }
        }
        else {
LAB_1800120df:
          FUN_180014510(iVar3);
        }
      }
      iVar3 = (**(code **)(**(longlong **)(param_1 + 0x2d8) + 0x50))();
      if (iVar3 != 0) goto LAB_180012696;
    }
    FUN_1800106d0(param_1,4,0);
    if ((*(longlong *)(param_1 + 0x180) == 0) || (*(longlong *)(param_1 + 0x2d8) == 0)) {
      DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
      uVar12 = local_90;
      if (DVar4 == 0x102) {
LAB_1800124d1:
        if (DAT_18002bb70 == 0) {
          DVar4 = timeGetTime();
        }
        else {
          QueryPerformanceCounter(&local_res10);
          DVar4 = (DWORD)((CONCAT44(local_res10.s.HighPart,local_res10.s.LowPart) * 1000) /
                         DAT_18002bb68);
        }
        iVar15 = 0;
        do {
          if (iVar15 == 0) {
            if (*(longlong *)(param_1 + 0x2c0) != 0) {
              iVar3 = FUN_180012730(param_1,&local_70);
              if (iVar3 != 0) goto LAB_180012696;
            }
          }
          else if ((iVar15 == 1) && (*(longlong *)(param_1 + 0x418) != 0)) {
            iVar3 = FUN_180014300(param_1,(int *)local_res8);
            if (iVar3 != 0) goto LAB_180012696;
            iVar3 = 0;
            if (*(int *)(param_1 + 0x430) == 0) {
              uVar10 = *(uint *)(param_1 + 0x3d4);
              uVar11 = local_res8[0];
              if (uVar10 <= local_res8[0]) goto LAB_180012560;
            }
            else if (local_res8[0] != 0) {
              iVar3 = FUN_180012890(param_1,(longlong)&local_70,local_res8[0]);
              if (iVar3 != 0) goto LAB_180012696;
              break;
            }
          }
          iVar15 = iVar15 + 1;
        } while (iVar15 < 2);
        goto LAB_1800125bb;
      }
    }
    else {
      DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
      if (DVar4 == 0x102) {
        local_res8[0] = local_90;
        uVar12 = local_90;
        do {
          local_res10.s.LowPart = 0;
          local_res18.s.LowPart = 0;
          local_b8 = 0;
          local_c0 = 0;
          local_b0[0] = 0;
          if (DAT_18002bb70 == 0) {
            DVar4 = timeGetTime();
          }
          else {
            QueryPerformanceCounter(&local_88);
            DVar4 = (DWORD)((local_88.QuadPart * 1000) / DAT_18002bb68);
          }
          iVar3 = FUN_180014300(param_1,(int *)&local_res18.QuadPart);
          if (iVar3 != 0) {
            FUN_180014510(iVar3);
            break;
          }
          uVar14 = 0;
          uVar13 = local_res18.s.LowPart;
          if (local_res18.s.LowPart != 0) {
            while (iVar3 = (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x18))
                                     (*(longlong **)(param_1 + 0x2c0),&local_b8,&local_res10,
                                      local_b0,0,0), uVar1 = local_res10.s.LowPart, iVar3 == 0) {
              if ((uint)uVar13 < local_res10.s.LowPart) {
                uVar14 = 0;
              }
              else {
                iVar3 = (**(code **)(**(longlong **)(param_1 + 0x418) + 0x18))
                                  (*(longlong **)(param_1 + 0x418),local_res10.s.LowPart,&local_c0);
                uVar6 = local_c0;
                uVar2 = local_res10.s.LowPart;
                if (iVar3 == 0) {
                  if (*(longlong *)(param_1 + 0x3f0) != 0) {
                    uVar7 = FUN_180012dc0(param_1 + 0x2c8,uVar1);
                    iVar3 = (int)uVar7;
                    if (iVar3 != 0) {
                      (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x20))
                                (*(longlong **)(param_1 + 0x2c0),0);
                      (**(code **)(**(longlong **)(param_1 + 0x418) + 0x20))
                                (*(longlong **)(param_1 + 0x418),0,0);
                      goto LAB_180012696;
                    }
                    local_c0 = *(undefined8 *)(param_1 + 0x3e0);
                  }
                  if (*(longlong *)(param_1 + 0x298) != 0) {
                    uVar7 = FUN_180012dc0(param_1 + 0x170,uVar2);
                    iVar3 = (int)uVar7;
                    if (iVar3 != 0) {
                      (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x20))
                                (*(longlong **)(param_1 + 0x2c0),0);
                      (**(code **)(**(longlong **)(param_1 + 0x418) + 0x20))
                                (*(longlong **)(param_1 + 0x418),0,0);
                      goto LAB_180012696;
                    }
                    (**(code **)(param_1 + 0x298))(*(undefined8 *)(param_1 + 0x288),local_b8,uVar2);
                    local_b8 = *(undefined8 *)(param_1 + 0x288);
                  }
                  (*(code *)local_78)(local_b8,uVar2,local_c0,uVar1,local_80);
                  if (*(code **)(param_1 + 0x3f0) != (code *)0x0) {
                    (**(code **)(param_1 + 0x3f0))(uVar6,*(undefined8 *)(param_1 + 0x3e0),uVar1);
                  }
                  iVar3 = (**(code **)(**(longlong **)(param_1 + 0x418) + 0x20))
                                    (*(longlong **)(param_1 + 0x418),uVar1,0);
                  if (iVar3 != 0) {
                    FUN_180014510(iVar3);
                  }
                  uVar13 = uVar13 - uVar1;
                  uVar14 = uVar2;
                }
                else if (*(int *)(param_1 + 0x334) != 0) {
                  FUN_180014510(iVar3);
                }
              }
              iVar3 = (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x20))
                                (*(longlong **)(param_1 + 0x2c0),uVar14);
              if (iVar3 != 0) goto LAB_180012387;
              uVar12 = local_res8[0];
              if ((uVar14 == 0) || (uVar13 == 0)) goto LAB_1800123a0;
            }
            uVar12 = local_res8[0];
            if (iVar3 != 0x8890001) {
LAB_180012387:
              FUN_180014510(iVar3);
              uVar12 = local_res8[0];
            }
          }
LAB_1800123a0:
          uVar10 = local_res20;
          if (local_res20 == 0) {
            uVar12 = uVar12 + 1;
            if (uVar12 == local_94) {
              uVar12 = 0;
              local_res8[0] = 0;
              uVar10 = local_8c;
            }
            else {
              uVar10 = 0;
              local_res8[0] = uVar12;
            }
          }
          if (DAT_18002bb70 == 0) {
            DVar5 = timeGetTime();
          }
          else {
            QueryPerformanceCounter(&local_a8);
            DVar5 = (DWORD)((local_a8.QuadPart * 1000) / DAT_18002bb68);
          }
          uVar11 = *(uint *)(param_1 + 0x4a4);
          DVar4 = uVar10 + (DVar4 - DVar5);
          if ((int)DVar4 < (int)uVar11) {
            DVar4 = 0;
          }
          else if (1 < (int)uVar11) {
            DVar4 = FUN_18000dfe0(DVar4,uVar11);
          }
          DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),DVar4);
        } while (DVar4 == 0x102);
      }
    }
  }
  else {
    iVar3 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x50))();
    if (iVar3 == 0) goto LAB_180011ffd;
LAB_180012696:
    FUN_180014510(iVar3);
    SetEvent(*(HANDLE *)(param_1 + 0x450));
  }
LAB_1800126b6:
  FUN_180014480(param_1);
  FUN_18000f5d0(param_1,local_c8[0]);
  *(undefined4 *)(param_1 + 0x434) = 0;
  SetEvent(*(HANDLE *)(param_1 + 0x458));
  FUN_1800106d0(param_1,8,iVar3);
  return 0;
  while( true ) {
    uVar11 = uVar11 - uVar10;
    iVar3 = 0;
    local_res8[0] = uVar11;
    if ((uVar11 < uVar10) ||
       (DVar5 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0), DVar5 != 0x102)) break;
LAB_180012560:
    iVar3 = FUN_180012890(param_1,(longlong)&local_70,uVar10);
    if (iVar3 != 0) goto LAB_180012696;
  }
LAB_1800125bb:
  uVar10 = local_res20;
  if (local_res20 == 0) {
    uVar12 = uVar12 + 1;
    if (uVar12 == local_94) {
      uVar12 = 0;
      uVar10 = local_8c;
    }
    else {
      uVar10 = 0;
    }
  }
  if (DAT_18002bb70 == 0) {
    DVar5 = timeGetTime();
  }
  else {
    QueryPerformanceCounter(&local_res18);
    DVar5 = (DWORD)((CONCAT44(local_res18.s.HighPart,local_res18.s.LowPart) * 1000) / DAT_18002bb68)
    ;
  }
  uVar11 = *(uint *)(param_1 + 0x4a4);
  DVar4 = (DVar4 - DVar5) + uVar10;
  if ((int)DVar4 < (int)uVar11) {
    DVar4 = 0;
  }
  else if (1 < (int)uVar11) {
    DVar4 = FUN_18000dfe0(DVar4,uVar11);
  }
  DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),DVar4);
  if (DVar4 != 0x102) goto LAB_1800126b6;
  goto LAB_1800124d1;
}



/* ========================================================================
   ENTRY: 180012730
   NAME : FUN_180012730
   SIG  : int __fastcall FUN_180012730(longlong param_1, undefined8 * param_2)
   ======================================================================== */

int FUN_180012730(longlong param_1,undefined8 *param_2)

{
  DWORD DVar1;
  int iVar2;
  undefined8 uVar3;
  int local_res8 [2];
  undefined1 local_res18 [8];
  undefined8 local_res20;
  
  DVar1 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
  while( true ) {
    if (DVar1 != 0x102) {
      return 0;
    }
    iVar2 = FUN_1800142b0(param_1,local_res8);
    if (iVar2 != 0) {
      return iVar2;
    }
    if (local_res8[0] == 0) {
      return 0;
    }
    iVar2 = (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x18))
                      (*(longlong **)(param_1 + 0x2c0),&local_res20,local_res8,local_res18,0,0);
    if (iVar2 != 0) break;
    if (*(longlong *)(param_1 + 0x298) != 0) {
      uVar3 = FUN_180012dc0(param_1 + 0x170,local_res8[0]);
      iVar2 = (int)uVar3;
      if (iVar2 != 0) goto LAB_180012864;
      (**(code **)(param_1 + 0x298))(*(undefined8 *)(param_1 + 0x288),local_res20,local_res8[0]);
      local_res20 = *(undefined8 *)(param_1 + 0x288);
    }
    (*(code *)*param_2)(local_res20,local_res8[0],0,0,param_2[1]);
    iVar2 = (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x20))
                      (*(longlong **)(param_1 + 0x2c0),local_res8[0]);
    if (iVar2 != 0) goto LAB_180012864;
    DVar1 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x448),0);
  }
  if (iVar2 == 0x8890001) {
    return 0;
  }
LAB_180012864:
  iVar2 = FUN_180014510(iVar2);
  return iVar2;
}



/* ========================================================================
   ENTRY: 180012890
   NAME : FUN_180012890
   SIG  : int __fastcall FUN_180012890(longlong param_1, longlong param_2, int param_3)
   ======================================================================== */

int FUN_180012890(longlong param_1,longlong param_2,int param_3)

{
  int iVar1;
  undefined8 uVar2;
  undefined8 local_res8;
  
  iVar1 = (**(code **)(**(longlong **)(param_1 + 0x418) + 0x18))
                    (*(longlong **)(param_1 + 0x418),param_3,&local_res8);
  if (iVar1 == 0) {
    if (*(longlong *)(param_1 + 0x3f0) == 0) {
      (**(code **)(param_2 + 0x10))(0,0,local_res8,param_3,*(undefined8 *)(param_2 + 0x18));
    }
    else {
      uVar2 = FUN_180012dc0(param_1 + 0x2c8,param_3);
      iVar1 = (int)uVar2;
      if (iVar1 != 0) goto LAB_1800128d1;
      (**(code **)(param_2 + 0x10))
                (0,0,*(undefined8 *)(param_1 + 0x3e0),param_3,*(undefined8 *)(param_2 + 0x18));
      (**(code **)(param_1 + 0x3f0))(local_res8,*(undefined8 *)(param_1 + 0x3e0),param_3);
    }
    iVar1 = (**(code **)(**(longlong **)(param_1 + 0x418) + 0x20))
                      (*(longlong **)(param_1 + 0x418),param_3,0);
    if (iVar1 == 0) {
      return 0;
    }
  }
  else if (iVar1 == -0x7776fffa) {
    return 0;
  }
LAB_1800128d1:
  iVar1 = FUN_180014510(iVar1);
  return iVar1;
}



/* ========================================================================
   ENTRY: 1800129a0
   NAME : FUN_1800129a0
   SIG  : uint __fastcall FUN_1800129a0(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

uint FUN_1800129a0(longlong param_1,longlong param_2,uint param_3)

{
  ushort uVar1;
  int *piVar2;
  HANDLE pvVar3;
  longlong *plVar4;
  code *pcVar5;
  longlong lVar6;
  uint uVar7;
  DWORD DVar8;
  int iVar9;
  ulonglong uVar10;
  ulonglong uVar11;
  undefined8 uVar12;
  ulonglong uVar13;
  uint uVar14;
  int iVar15;
  undefined1 *puVar16;
  longlong lVar17;
  bool bVar18;
  undefined1 auStack_d8 [48];
  uint local_a8;
  uint uStack_a4;
  uint local_a0 [2];
  longlong local_98;
  undefined4 local_90 [2];
  longlong local_88;
  longlong local_80;
  longlong local_78;
  longlong local_70;
  uint local_68;
  int local_64;
  int local_60;
  uint local_5c;
  ulonglong local_58;
  
  puVar16 = auStack_d8;
  local_58 = DAT_18002b580 ^ (ulonglong)&local_a8;
  local_70 = *(longlong *)(param_1 + 0x298);
  uVar11 = 0;
  iVar9 = 0;
  local_90[0] = 0;
  if (*(int *)(param_1 + 0x434) == 0) {
    uVar14 = 0xffffd901;
    puVar16 = auStack_d8;
  }
  else if (*(longlong *)(param_1 + 0x2c0) == 0) {
    uVar14 = 0xffffd8fc;
  }
  else {
    local_98 = param_2;
    ResetEvent(*(HANDLE *)(param_1 + 0x460));
    FUN_180013500(&local_68,1,0xfa);
    puVar16 = auStack_d8;
    if (*(int *)(param_1 + 0x90) == 0) {
      uVar10 = (ulonglong)*(uint *)(param_1 + 0x84) * 8;
      uVar13 = uVar10 + 0xf;
      if (uVar13 <= uVar10) {
        uVar13 = 0xffffffffffffff0;
      }
      lVar17 = -(uVar13 & 0xfffffffffffffff0);
      puVar16 = auStack_d8 + lVar17;
      local_98 = (longlong)&local_a8 + lVar17;
      if (local_98 == 0) {
        uVar14 = 0xffffd8f8;
        goto LAB_180012c96;
      }
      uVar13 = uVar11;
      puVar16 = auStack_d8 + lVar17;
      if (*(uint *)(param_1 + 0x84) != 0) {
        do {
          uVar14 = (int)uVar13 + 1;
          *(undefined8 *)(local_98 + uVar13 * 8) = *(undefined8 *)(param_2 + uVar13 * 8);
          uVar13 = (ulonglong)uVar14;
          puVar16 = auStack_d8 + lVar17;
        } while (uVar14 < *(uint *)(param_1 + 0x84));
      }
    }
    lVar17 = *(longlong *)(param_1 + 0x2a0);
    *(undefined8 *)(puVar16 + -8) = 0x180012ab9;
    uVar14 = FUN_180006b00(lVar17);
    local_a8 = uVar14;
    if (uVar14 != 0) {
      local_88 = 0;
      *(uint **)(puVar16 + 0x28) = &uStack_a4;
      local_80 = 0;
      if (param_3 < uVar14) {
        uVar14 = param_3;
      }
      *(longlong **)(puVar16 + 0x20) = &local_80;
      piVar2 = *(int **)(param_1 + 0x2a0);
      *(undefined8 *)(puVar16 + -8) = 0x180012afb;
      uVar7 = FUN_180006b10(piVar2,uVar14,&local_88,local_a0,*(undefined8 **)(puVar16 + 0x20),
                            *(int **)(puVar16 + 0x28));
      uVar14 = local_a0[0];
      if (local_88 != 0) {
        *(undefined8 *)(puVar16 + -8) = 0x180012b10;
        FUN_180006860(param_1 + 0x68,uVar14);
        lVar17 = local_88;
        uVar14 = *(uint *)(param_1 + 0x84);
        *(undefined8 *)(puVar16 + -8) = 0x180012b26;
        FUN_180006880(param_1 + 0x68,0,lVar17,uVar14);
        *(undefined8 *)(puVar16 + -8) = 0x180012b37;
        uVar14 = FUN_180005d50(param_1 + 0x68,&local_98,local_a0[0]);
        param_3 = param_3 - uVar14;
      }
      uVar14 = uStack_a4;
      if (local_80 != 0) {
        *(undefined8 *)(puVar16 + -8) = 0x180012b4c;
        FUN_180006860(param_1 + 0x68,uVar14);
        lVar17 = local_80;
        uVar14 = *(uint *)(param_1 + 0x84);
        *(undefined8 *)(puVar16 + -8) = 0x180012b62;
        FUN_180006880(param_1 + 0x68,0,lVar17,uVar14);
        *(undefined8 *)(puVar16 + -8) = 0x180012b73;
        uVar14 = FUN_180005d50(param_1 + 0x68,&local_98,uStack_a4);
        param_3 = param_3 - uVar14;
      }
      lVar17 = *(longlong *)(param_1 + 0x2a0);
      *(undefined8 *)(puVar16 + -8) = 0x180012b85;
      FUN_180006ad0(lVar17,uVar7);
    }
    uVar13 = 0;
    iVar15 = local_60;
    while (param_3 != 0) {
      iVar9 = (int)uVar11;
      pvVar3 = *(HANDLE *)(param_1 + 0x448);
      *(undefined8 *)(puVar16 + -8) = 0x180012bb0;
      DVar8 = WaitForSingleObject(pvVar3,(DWORD)uVar13);
      if (DVar8 != 0x102) break;
      *(undefined8 *)(puVar16 + -8) = 0x180012bc7;
      iVar9 = FUN_1800142b0(param_1,&local_a8);
      if (iVar9 != 0) {
        *(undefined8 *)(puVar16 + -8) = 0x180012db0;
        FUN_180014510(iVar9);
        uVar14 = 0xffffd8f1;
        goto LAB_180012c96;
      }
      if (local_a8 != 0) {
        plVar4 = *(longlong **)(param_1 + 0x2c0);
        *(undefined8 *)(puVar16 + 0x28) = 0;
        *(undefined8 *)(puVar16 + 0x20) = 0;
        pcVar5 = *(code **)(*plVar4 + 0x18);
        *(undefined8 *)(puVar16 + -8) = 0x180012c54;
        uVar11 = (*pcVar5)(plVar4,&local_78,&local_a8,local_90);
        uVar14 = local_a8;
        iVar9 = (int)uVar11;
        uVar11 = uVar11 & 0xffffffff;
        if (iVar9 == 0) {
          lVar17 = local_78;
          if (local_70 != 0) {
            *(undefined8 *)(puVar16 + -8) = 0x180012cc9;
            uVar12 = FUN_180012dc0(param_1 + 0x170,uVar14);
            lVar6 = local_78;
            uVar14 = local_a8;
            iVar9 = (int)uVar12;
            if (iVar9 != 0) goto LAB_180012c6b;
            lVar17 = *(longlong *)(param_1 + 0x288);
            pcVar5 = *(code **)(param_1 + 0x298);
            *(undefined8 *)(puVar16 + -8) = 0x180012ceb;
            (*pcVar5)(lVar17,lVar6,uVar14);
          }
          uVar14 = local_a8;
          *(undefined8 *)(puVar16 + -8) = 0x180012cfd;
          FUN_180006860(param_1 + 0x68,uVar14);
          uVar14 = *(uint *)(param_1 + 0x84);
          *(undefined8 *)(puVar16 + -8) = 0x180012d12;
          FUN_180006880(param_1 + 0x68,0,lVar17,uVar14);
          *(undefined8 *)(puVar16 + -8) = 0x180012d22;
          uVar14 = FUN_180005d50(param_1 + 0x68,&local_98,param_3);
          param_3 = param_3 - uVar14;
          if ((param_3 == 0) && (uVar14 < local_a8)) {
            uVar1 = *(ushort *)(param_1 + 0x194);
            iVar9 = local_a8 - uVar14;
            bVar18 = local_70 != 0;
            piVar2 = *(int **)(param_1 + 0x2a0);
            *(undefined8 *)(puVar16 + -8) = 0x180012d57;
            FUN_180006d40(piVar2,(void *)((ulonglong)((uVar1 >> bVar18) * uVar14) + lVar17),iVar9);
          }
          uVar14 = local_a8;
          plVar4 = *(longlong **)(param_1 + 0x2c0);
          pcVar5 = *(code **)(*plVar4 + 0x20);
          *(undefined8 *)(puVar16 + -8) = 0x180012d6b;
          uVar11 = (*pcVar5)(plVar4,uVar14);
          iVar9 = (int)uVar11;
          uVar11 = uVar11 & 0xffffffff;
          if (iVar9 == 0) goto LAB_180012d71;
        }
        else if (iVar9 == 0x8890001) goto LAB_180012d71;
LAB_180012c6b:
        *(undefined8 *)(puVar16 + -8) = 0x180012c80;
        FUN_180014510(iVar9);
        break;
      }
      uVar11 = 0;
      if (*(int *)(param_1 + 0x1dc) == 1) {
LAB_180012c16:
        iVar15 = iVar15 + 1;
        if (iVar15 == local_64) {
          iVar15 = 0;
          uVar13 = (ulonglong)local_5c;
        }
        else {
          uVar13 = 0;
        }
      }
      else {
        uVar14 = *(uint *)(param_1 + 0x18c);
        uVar7 = param_3;
        if (*(uint *)(param_1 + 0x1d8) <= param_3) {
          uVar7 = *(uint *)(param_1 + 0x1d8);
        }
        *(undefined8 *)(puVar16 + -8) = 0x180012bfd;
        uVar13 = FUN_18000fba0((ulonglong)uVar7,(ulonglong)uVar14);
        uVar10 = uVar13 >> 2 & 0x3fffffff;
        uVar13 = 2;
        if ((uint)uVar10 < 3) {
          uVar13 = uVar10;
        }
        if ((int)uVar13 == 0) goto LAB_180012c16;
      }
LAB_180012d71:
      iVar9 = (int)uVar11;
    }
    pvVar3 = *(HANDLE *)(param_1 + 0x460);
    *(undefined8 *)(puVar16 + -8) = 0x180012c8d;
    SetEvent(pvVar3);
    uVar14 = -(uint)(iVar9 != 0) & 0xffffd8f1;
  }
LAB_180012c96:
  *(undefined8 *)(puVar16 + -8) = 0x180012ca2;
  return uVar14;
}



/* ========================================================================
   ENTRY: 180012dc0
   NAME : FUN_180012dc0
   SIG  : undefined8 __fastcall FUN_180012dc0(longlong param_1, int param_2)
   ======================================================================== */

undefined8 FUN_180012dc0(longlong param_1,int param_2)

{
  void *pvVar1;
  uint uVar2;
  
  if (param_2 == 0) {
    param_2 = *(int *)(param_1 + 0x4c);
  }
  uVar2 = (uint)*(ushort *)(param_1 + 0x24) * param_2;
  if (*(uint *)(param_1 + 0x120) < uVar2) {
    pvVar1 = FUN_180011a20(*(void **)(param_1 + 0x118),(ulonglong)uVar2);
    *(void **)(param_1 + 0x118) = pvVar1;
    if (pvVar1 == (void *)0x0) {
      *(undefined4 *)(param_1 + 0x120) = 0;
      return 0x8007000e;
    }
    *(uint *)(param_1 + 0x120) = uVar2;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180012e30
   NAME : FUN_180012e30
   SIG  : undefined __fastcall FUN_180012e30(longlong param_1)
   ======================================================================== */

void FUN_180012e30(longlong param_1)

{
  if (*(longlong **)(param_1 + 0x2c0) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x2c0) + 0x10))();
    *(undefined8 *)(param_1 + 0x2c0) = 0;
  }
  if (*(longlong **)(param_1 + 0x418) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x418) + 0x10))();
    *(undefined8 *)(param_1 + 0x418) = 0;
  }
  FUN_180012e90(param_1 + 0x170);
  FUN_180012e90(param_1 + 0x2c8);
  return;
}



/* ========================================================================
   ENTRY: 180012e90
   NAME : FUN_180012e90
   SIG  : undefined __fastcall FUN_180012e90(longlong param_1)
   ======================================================================== */

void FUN_180012e90(longlong param_1)

{
  if (*(longlong **)(param_1 + 0x10) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(param_1 + 0x10) + 0x10))();
    *(undefined8 *)(param_1 + 0x10) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180012ec0
   NAME : FUN_180012ec0
   SIG  : undefined __fastcall FUN_180012ec0(longlong param_1)
   ======================================================================== */

void FUN_180012ec0(longlong param_1)

{
  longlong *plVar1;
  ulonglong uVar2;
  uint uVar3;
  
  uVar2 = 0;
  if (*(int *)(param_1 + 0x118) != 0) {
    do {
      plVar1 = *(longlong **)(uVar2 * 0x280 + *(longlong *)(param_1 + 0x120));
      if (plVar1 != (longlong *)0x0) {
        (**(code **)(*plVar1 + 0x10))();
        *(undefined8 *)(uVar2 * 0x280 + *(longlong *)(param_1 + 0x120)) = 0;
      }
      uVar3 = (int)uVar2 + 1;
      uVar2 = (ulonglong)uVar3;
    } while (uVar3 < *(uint *)(param_1 + 0x118));
  }
  if (*(longlong *)(param_1 + 0x108) != 0) {
    FUN_180001360(*(longlong *)(param_1 + 0x108),*(longlong *)(param_1 + 0x120));
  }
  *(undefined4 *)(param_1 + 0x118) = 0;
  *(undefined8 *)(param_1 + 0x120) = 0;
  return;
}



/* ========================================================================
   ENTRY: 180012f60
   NAME : FUN_180012f60
   SIG  : longlong __fastcall FUN_180012f60(double param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

longlong FUN_180012f60(double param_1)

{
  return (longlong)(param_1 / _DAT_180024870);
}



/* ========================================================================
   ENTRY: 180012f70
   NAME : FUN_180012f70
   SIG  : undefined8 __fastcall FUN_180012f70(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180012f70(void)

{
  DAT_18002bb60 = LoadLibraryA("avrt.dll");
  if (DAT_18002bb60 != (HMODULE)0x0) {
    DAT_18002bb30 = GetProcAddress(DAT_18002bb60,"AvRtCreateThreadOrderingGroup");
    if (DAT_18002bb30 != (FARPROC)0x0) {
      DAT_18002bb38 = GetProcAddress(DAT_18002bb60,"AvRtDeleteThreadOrderingGroup");
      if (DAT_18002bb38 != (FARPROC)0x0) {
        DAT_18002bb40 = GetProcAddress(DAT_18002bb60,"AvRtWaitOnThreadOrderingGroup");
        if (DAT_18002bb40 != (FARPROC)0x0) {
          DAT_18002bb48 = GetProcAddress(DAT_18002bb60,"AvSetMmThreadCharacteristicsA");
          if (DAT_18002bb48 != (FARPROC)0x0) {
            DAT_18002bb50 = GetProcAddress(DAT_18002bb60,"AvRevertMmThreadCharacteristics");
            if (DAT_18002bb50 != (FARPROC)0x0) {
              _DAT_18002bb58 = GetProcAddress(DAT_18002bb60,"AvSetMmThreadPriority");
              if ((((_DAT_18002bb58 != (FARPROC)0x0) && (DAT_18002bb30 != (FARPROC)0x0)) &&
                  (DAT_18002bb38 != (FARPROC)0x0)) &&
                 (((DAT_18002bb40 != (FARPROC)0x0 && (DAT_18002bb48 != (FARPROC)0x0)) &&
                  (DAT_18002bb50 != (FARPROC)0x0)))) {
                return 1;
              }
            }
          }
        }
      }
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800130a0
   NAME : FUN_1800130a0
   SIG  : undefined8 __fastcall FUN_1800130a0(void * param_1)
   ======================================================================== */

undefined8 FUN_1800130a0(void *param_1)

{
  int iVar1;
  HRESULT HVar2;
  DWORD DVar3;
  HANDLE pvVar4;
  uintptr_t uVar5;
  
  iVar1 = FUN_180010140((longlong)param_1);
  if (iVar1 != 0) {
    return 0xffffd902;
  }
  FUN_1800066c0((longlong)param_1 + 0x68);
  FUN_1800143d0((longlong)param_1);
  pvVar4 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
  *(HANDLE *)((longlong)param_1 + 0x448) = pvVar4;
  if (pvVar4 != (HANDLE)0x0) {
    *(undefined4 *)((longlong)param_1 + 0x434) = 1;
    *(undefined4 *)((longlong)param_1 + 0x438) = 0;
    if (*(int *)((longlong)param_1 + 0x490) != 0) {
      if (*(longlong *)((longlong)param_1 + 0x2c8) != 0) {
        pvVar4 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,1,(LPCSTR)0x0);
        *(HANDLE *)((longlong)param_1 + 0x468) = pvVar4;
        if (pvVar4 == (HANDLE)0x0) goto LAB_1800130f2;
      }
      if (*(longlong *)((longlong)param_1 + 0x170) != 0) {
        pvVar4 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,1,(LPCSTR)0x0);
        *(HANDLE *)((longlong)param_1 + 0x460) = pvVar4;
        if (pvVar4 == (HANDLE)0x0) goto LAB_1800130f2;
      }
      if (((*(longlong **)((longlong)param_1 + 0x170) == (longlong *)0x0) ||
          (iVar1 = (**(code **)(**(longlong **)((longlong)param_1 + 0x170) + 0x50))(), iVar1 == 0))
         && ((*(longlong **)((longlong)param_1 + 0x2c8) == (longlong *)0x0 ||
             (iVar1 = (**(code **)(**(longlong **)((longlong)param_1 + 0x2c8) + 0x50))(), iVar1 == 0
             )))) {
        *(undefined8 *)((longlong)param_1 + 0x2c0) = *(undefined8 *)((longlong)param_1 + 0x2b0);
        *(undefined8 *)((longlong)param_1 + 0x418) = *(undefined8 *)((longlong)param_1 + 0x408);
        *(undefined8 *)((longlong)param_1 + 0x180) = *(undefined8 *)((longlong)param_1 + 0x170);
        *(undefined8 *)((longlong)param_1 + 0x2d8) = *(undefined8 *)((longlong)param_1 + 0x2c8);
        return 0;
      }
      FUN_180014510(iVar1);
      FUN_18000e030((longlong)param_1);
      return 0xffffd8f1;
    }
    pvVar4 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
    *(HANDLE *)((longlong)param_1 + 0x450) = pvVar4;
    pvVar4 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
    *(HANDLE *)((longlong)param_1 + 0x458) = pvVar4;
    if ((*(longlong *)((longlong)param_1 + 0x450) != 0) && (pvVar4 != (HANDLE)0x0)) {
      HVar2 = FUN_1800105a0((longlong)param_1);
      if (HVar2 == 0) {
        if (((*(longlong *)((longlong)param_1 + 0x170) == 0) ||
            ((*(uint *)((longlong)param_1 + 0x1e0) & 0x40000) == 0)) &&
           ((*(longlong *)((longlong)param_1 + 0x2c8) == 0 ||
            ((*(uint *)((longlong)param_1 + 0x338) & 0x40000) == 0)))) {
          uVar5 = _beginthreadex((void *)0x0,0,FUN_180011ed0,param_1,0,
                                 (uint *)((longlong)param_1 + 0x43c));
          *(uintptr_t *)((longlong)param_1 + 0x440) = uVar5;
        }
        else {
          uVar5 = _beginthreadex((void *)0x0,0,FUN_180011b40,param_1,0,
                                 (uint *)((longlong)param_1 + 0x43c));
          *(uintptr_t *)((longlong)param_1 + 0x440) = uVar5;
        }
        if ((uVar5 != 0) &&
           (DVar3 = WaitForSingleObject(*(HANDLE *)((longlong)param_1 + 0x450),60000),
           DVar3 != 0x102)) {
          return 0;
        }
      }
      SetEvent(*(HANDLE *)((longlong)param_1 + 0x458));
      FUN_180013520((longlong)param_1);
      FUN_180012e30((longlong)param_1);
      FUN_18000e030((longlong)param_1);
      return 0xffffd8f1;
    }
  }
LAB_1800130f2:
  FUN_18000e030((longlong)param_1);
  return 0xffffd8f8;
}



/* ========================================================================
   ENTRY: 180013370
   NAME : FUN_180013370
   SIG  : undefined __fastcall FUN_180013370(longlong param_1)
   ======================================================================== */

void FUN_180013370(longlong param_1)

{
  if (*(int *)(param_1 + 0x490) == 0) {
    SignalObjectAndWait(*(HANDLE *)(param_1 + 0x448),*(HANDLE *)(param_1 + 0x458),0xffffffff,0);
  }
  else {
    if (*(longlong *)(param_1 + 0x2c8) != 0) {
      SignalObjectAndWait(*(HANDLE *)(param_1 + 0x448),*(HANDLE *)(param_1 + 0x468),0xffffffff,1);
    }
    if (*(longlong *)(param_1 + 0x2c8) != 0) {
      SignalObjectAndWait(*(HANDLE *)(param_1 + 0x448),*(HANDLE *)(param_1 + 0x460),0xffffffff,1);
    }
    FUN_180014480(param_1);
  }
  FUN_1800143d0(param_1);
  *(undefined4 *)(param_1 + 0x434) = 0;
  *(undefined4 *)(param_1 + 0x438) = 1;
  return;
}



/* ========================================================================
   ENTRY: 180013420
   NAME : FUN_180013420
   SIG  : undefined __fastcall FUN_180013420(void)
   ======================================================================== */

void FUN_180013420(void)

{
  DAT_18002bb70 = QueryPerformanceFrequency((LARGE_INTEGER *)&DAT_18002bb68);
  return;
}



/* ========================================================================
   ENTRY: 180013440
   NAME : FUN_180013440
   SIG  : undefined __fastcall FUN_180013440(UINT * param_1)
   ======================================================================== */

void FUN_180013440(UINT *param_1)

{
  if (*param_1 != 0) {
                    /* WARNING: Could not recover jumptable at 0x000180013446. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    timeEndPeriod(*param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180013450
   NAME : FUN_180013450
   SIG  : undefined8 __fastcall FUN_180013450(UINT * param_1, UINT param_2)
   ======================================================================== */

undefined8 FUN_180013450(UINT *param_1,UINT param_2)

{
  MMRESULT MVar1;
  timecaps_tag local_res8 [4];
  
  *param_1 = param_2;
  MVar1 = timeGetDevCaps(local_res8,8);
  if ((MVar1 == 0) && ((int)*param_1 < (int)local_res8[0].wPeriodMin)) {
    *param_1 = local_res8[0].wPeriodMin;
  }
  MVar1 = timeBeginPeriod(*param_1);
  if (MVar1 != 0) {
    *param_1 = 10;
    return 0;
  }
  return 1;
}



/* ========================================================================
   ENTRY: 1800134a0
   NAME : FUN_1800134a0
   SIG  : undefined __fastcall FUN_1800134a0(longlong param_1)
   ======================================================================== */

void FUN_1800134a0(longlong param_1)

{
  if (param_1 != 0) {
    FUN_180012ec0(param_1);
    if (*(longlong *)(param_1 + 0x108) != 0) {
      FUN_180001280(*(longlong *)(param_1 + 0x108));
      FUN_180001230(*(longlong *)(param_1 + 0x108));
    }
    FUN_180020200(0xd,(int *)(param_1 + 0x110));
    FUN_180020240(param_1);
    FUN_18000e460();
  }
  return;
}



/* ========================================================================
   ENTRY: 180013500
   NAME : FUN_180013500
   SIG  : undefined __fastcall FUN_180013500(uint * param_1, uint param_2, uint param_3)
   ======================================================================== */

void FUN_180013500(uint *param_1,uint param_2,uint param_3)

{
  param_1[2] = 0;
  param_1[3] = param_2;
  *param_1 = param_3;
  param_1[1] = (param_2 * 1000) / param_3;
  return;
}



/* ========================================================================
   ENTRY: 180013520
   NAME : FUN_180013520
   SIG  : HRESULT __fastcall FUN_180013520(longlong param_1)
   ======================================================================== */

HRESULT FUN_180013520(longlong param_1)

{
  HRESULT HVar1;
  int iVar2;
  
  *(undefined8 *)(param_1 + 0x2c0) = 0;
  *(undefined8 *)(param_1 + 0x418) = 0;
  *(undefined8 *)(param_1 + 0x180) = 0;
  *(undefined8 *)(param_1 + 0x2d8) = 0;
  iVar2 = 0;
  if (*(longlong *)(param_1 + 0x170) != 0) {
    HVar1 = FUN_1800135f0(param_1 + 0x170);
    iVar2 = 0;
    if (HVar1 != 0) {
      iVar2 = HVar1;
    }
    HVar1 = CoGetInterfaceAndReleaseStream
                      (*(LPSTREAM *)(param_1 + 0x2b8),(IID *)&DAT_180023eb8,
                       (LPVOID *)(param_1 + 0x2c0));
    *(undefined8 *)(param_1 + 0x2b8) = 0;
    if ((HVar1 != 0) && (iVar2 == 0)) {
      iVar2 = HVar1;
    }
  }
  if (*(longlong *)(param_1 + 0x2c8) != 0) {
    HVar1 = FUN_1800135f0(param_1 + 0x2c8);
    if ((HVar1 != 0) && (iVar2 == 0)) {
      iVar2 = HVar1;
    }
    HVar1 = CoGetInterfaceAndReleaseStream
                      (*(LPSTREAM *)(param_1 + 0x410),(IID *)&DAT_180023ea8,
                       (LPVOID *)(param_1 + 0x418));
    *(undefined8 *)(param_1 + 0x410) = 0;
    if ((HVar1 != 0) && (iVar2 == 0)) {
      iVar2 = HVar1;
    }
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 1800135f0
   NAME : FUN_1800135f0
   SIG  : HRESULT __fastcall FUN_1800135f0(longlong param_1)
   ======================================================================== */

HRESULT FUN_1800135f0(longlong param_1)

{
  HRESULT HVar1;
  IID *iid;
  HRESULT HVar2;
  
  *(undefined8 *)(param_1 + 0x10) = 0;
  iid = (IID *)FUN_18000f7b0();
  HVar1 = CoGetInterfaceAndReleaseStream(*(LPSTREAM *)(param_1 + 8),iid,(LPVOID *)(param_1 + 0x10));
  *(undefined8 *)(param_1 + 8) = 0;
  HVar2 = 0;
  if (HVar1 != 0) {
    HVar2 = HVar1;
  }
  return HVar2;
}



/* ========================================================================
   ENTRY: 180013640
   NAME : FUN_180013640
   SIG  : undefined __fastcall FUN_180013640(longlong param_1)
   ======================================================================== */

void FUN_180013640(longlong param_1)

{
  ushort uVar1;
  
  uVar1 = (*(ushort *)(param_1 + 0xe) >> 3) * *(short *)(param_1 + 2);
  *(ushort *)(param_1 + 0xc) = uVar1;
  *(uint *)(param_1 + 8) = (uint)uVar1 * *(int *)(param_1 + 4);
  return;
}



/* ========================================================================
   ENTRY: 180013670
   NAME : FUN_180013670
   SIG  : undefined8 __fastcall FUN_180013670(void)
   ======================================================================== */

undefined8 FUN_180013670(void)

{
  int iVar1;
  
  iVar1 = FUN_180010220();
  if (iVar1 != 0) {
    iVar1 = FUN_180020730();
    if (iVar1 == 6) {
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800136a0
   NAME : FUN_1800136a0
   SIG  : undefined __fastcall FUN_1800136a0(longlong param_1, int param_2, longlong param_3, int param_4, longlong param_5)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_1800136a0(longlong param_1,int param_2,longlong param_3,int param_4,longlong param_5)

{
  longlong *plVar1;
  int iVar2;
  uint uVar3;
  undefined1 auStack_88 [32];
  uint local_68;
  int local_64;
  double local_60;
  double dStack_58;
  double local_50;
  ulonglong local_48;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStack_88;
  local_50 = 0.0;
  local_60 = 0.0;
  dStack_58 = 0.0;
  FUN_1800038c0(param_5 + 0x50);
  dStack_58 = FUN_180020250();
  plVar1 = *(longlong **)(param_5 + 0x180);
  if (plVar1 != (longlong *)0x0) {
    iVar2 = (**(code **)(*plVar1 + 0x30))(plVar1,&local_68);
    if (iVar2 == 0) {
      local_60 = (double)local_68 / (double)*(uint *)(param_5 + 0x18c);
    }
    else {
      local_60 = *(double *)(param_5 + 0x1d0);
    }
    local_60 = dStack_58 - local_60;
  }
  plVar1 = *(longlong **)(param_5 + 0x2d8);
  if (plVar1 != (longlong *)0x0) {
    iVar2 = (**(code **)(*plVar1 + 0x30))(plVar1,&local_68);
    if (iVar2 == 0) {
      local_50 = (double)local_68 / (double)*(uint *)(param_5 + 0x2e4);
    }
    else {
      local_50 = *(double *)(param_5 + 0x328);
    }
    local_50 = local_50 + dStack_58;
  }
  FUN_180005ce0(param_5 + 0x68,&local_60,0);
  if (*(int *)(param_5 + 0x84) != 0) {
    FUN_180006860(param_5 + 0x68,param_2);
    FUN_180006880(param_5 + 0x68,0,param_1,0);
  }
  if (*(int *)(param_5 + 0xa8) != 0) {
    FUN_1800069a0(param_5 + 0x68,param_4);
    FUN_1800068d0(param_5 + 0x68,0,param_3,0);
  }
  local_64 = 0;
  uVar3 = FUN_180006010((uint *)(param_5 + 0x68),&local_64);
  FUN_1800038e0((double *)(param_5 + 0x50),uVar3);
  if (local_64 != 0) {
    SetEvent(*(HANDLE *)(param_5 + 0x448));
  }
  return;
}



/* ========================================================================
   ENTRY: 180013850
   NAME : FUN_180013850
   SIG  : undefined8 __fastcall FUN_180013850(short * param_1)
   ======================================================================== */

undefined8 FUN_180013850(short *param_1)

{
  short sVar1;
  undefined8 uVar2;
  
  sVar1 = *param_1;
  if (sVar1 == 1) {
    sVar1 = param_1[7];
    if (sVar1 == 8) {
      return 0x40;
    }
    if (sVar1 == 0x10) {
      return 0x10;
    }
    if (sVar1 == 0x18) {
      return 8;
    }
    if (sVar1 == 0x20) {
      return 4;
    }
    uVar2 = 1;
    if (sVar1 != 0x40) {
      uVar2 = 0x10000;
    }
    return uVar2;
  }
  if (sVar1 == 3) {
    return 2;
  }
  if (sVar1 == -2) {
    if ((*(longlong *)(param_1 + 0xc) == DAT_180023f08) &&
       (*(longlong *)(param_1 + 0x10) == DAT_180023f10)) {
      if (param_1[9] == 0x20) {
        return 2;
      }
    }
    else if ((*(longlong *)(param_1 + 0xc) == DAT_180023ef8) &&
            (*(longlong *)(param_1 + 0x10) == DAT_180023f00)) {
      sVar1 = param_1[7];
      if (sVar1 == 8) {
        return 0x40;
      }
      if (sVar1 == 0x10) {
        return 0x10;
      }
      if (sVar1 == 0x18) {
        return 8;
      }
      if (sVar1 == 0x20) {
        return 4;
      }
      if (sVar1 == 0x40) {
        return 1;
      }
    }
    else if ((*(longlong *)(param_1 + 0xf) == DAT_180023f1e) && (param_1[0x13] == DAT_180023f26)) {
      return 0x10;
    }
  }
  return 0x10000;
}



/* ========================================================================
   ENTRY: 180013940
   NAME : FUN_180013940
   SIG  : uint __fastcall FUN_180013940(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

uint FUN_180013940(longlong param_1,longlong param_2,uint param_3)

{
  HANDLE pvVar1;
  longlong *plVar2;
  code *pcVar3;
  longlong lVar4;
  DWORD DVar5;
  int iVar6;
  uint uVar7;
  ulonglong uVar8;
  undefined8 uVar9;
  ulonglong uVar10;
  uint uVar11;
  undefined1 *puVar12;
  int iVar13;
  longlong lVar14;
  undefined1 auStack_a8 [32];
  uint local_88 [2];
  longlong local_80;
  longlong local_78;
  longlong local_70;
  uint local_68;
  int local_64;
  int local_60;
  uint local_5c;
  ulonglong local_58;
  
  puVar12 = auStack_a8;
  local_58 = DAT_18002b580 ^ (ulonglong)local_88;
  local_78 = *(longlong *)(param_1 + 0x3f0);
  if (*(int *)(param_1 + 0x434) == 0) {
    uVar11 = 0xffffd901;
    puVar12 = auStack_a8;
  }
  else if (*(longlong *)(param_1 + 0x418) == 0) {
    uVar11 = 0xffffd8fc;
  }
  else {
    local_80 = param_2;
    ResetEvent(*(HANDLE *)(param_1 + 0x468));
    FUN_180013500(&local_68,1,500);
    puVar12 = auStack_a8;
    if (*(int *)(param_1 + 0xb4) == 0) {
      uVar8 = (ulonglong)*(uint *)(param_1 + 0xa8) * 8;
      uVar10 = uVar8 + 0xf;
      if (uVar10 <= uVar8) {
        uVar10 = 0xffffffffffffff0;
      }
      lVar14 = -(uVar10 & 0xfffffffffffffff0);
      puVar12 = auStack_a8 + lVar14;
      local_80 = (longlong)local_88 + lVar14;
      if (local_80 == 0) {
        uVar11 = 0xffffd8f8;
        goto LAB_180013be9;
      }
      uVar10 = 0;
      puVar12 = auStack_a8 + lVar14;
      if (*(uint *)(param_1 + 0xa8) != 0) {
        do {
          *(undefined8 *)(local_80 + uVar10 * 8) = *(undefined8 *)(param_2 + uVar10 * 8);
          uVar11 = (int)uVar10 + 1;
          uVar10 = (ulonglong)uVar11;
          puVar12 = auStack_a8 + lVar14;
        } while (uVar11 < *(uint *)(param_1 + 0xa8));
      }
    }
    iVar6 = 0;
    if (param_3 != 0) {
      uVar10 = 0;
      iVar13 = local_60;
      do {
        pvVar1 = *(HANDLE *)(param_1 + 0x448);
        *(undefined8 *)(puVar12 + -8) = 0x180013a60;
        DVar5 = WaitForSingleObject(pvVar1,(DWORD)uVar10);
        if (DVar5 != 0x102) break;
        *(undefined8 *)(puVar12 + -8) = 0x180013a77;
        iVar6 = FUN_180014300(param_1,(int *)local_88);
        if (iVar6 != 0) {
LAB_180013bbe:
          *(undefined8 *)(puVar12 + -8) = 0x180013bd3;
          FUN_180014510(iVar6);
          break;
        }
        if (local_88[0] == 0) {
          uVar11 = *(uint *)(param_1 + 0x2e4);
          uVar7 = param_3;
          if (*(uint *)(param_1 + 0x330) <= param_3) {
            uVar7 = *(uint *)(param_1 + 0x330);
          }
          *(undefined8 *)(puVar12 + -8) = 0x180013aa6;
          uVar10 = FUN_18000fba0((ulonglong)uVar7,(ulonglong)uVar11);
          if ((uint)uVar10 < 2) {
            iVar13 = iVar13 + 1;
            if (iVar13 == local_64) {
              uVar10 = (ulonglong)local_5c;
              iVar13 = 0;
            }
            else {
              uVar10 = 0;
            }
          }
          else {
            uVar10 = uVar10 >> 1 & 0x7fffffff;
          }
        }
        else {
          plVar2 = *(longlong **)(param_1 + 0x418);
          uVar11 = local_88[0];
          if (param_3 < local_88[0]) {
            uVar11 = param_3;
          }
          pcVar3 = *(code **)(*plVar2 + 0x18);
          local_88[0] = uVar11;
          *(undefined8 *)(puVar12 + -8) = 0x180013af0;
          iVar6 = (*pcVar3)(plVar2,uVar11,&local_70);
          if (iVar6 == 0) {
            if (local_70 != 0) {
              lVar14 = local_70;
              if (local_78 != 0) {
                *(undefined8 *)(puVar12 + -8) = 0x180013b2e;
                uVar9 = FUN_180012dc0(param_1 + 0x2c8,uVar11);
                iVar6 = (int)uVar9;
                if (iVar6 != 0) goto LAB_180013bbe;
                lVar14 = *(longlong *)(param_1 + 0x3e0);
              }
              *(undefined8 *)(puVar12 + -8) = 0x180013b46;
              FUN_1800069a0(param_1 + 0x68,uVar11);
              uVar7 = *(uint *)(param_1 + 0xa8);
              *(undefined8 *)(puVar12 + -8) = 0x180013b5b;
              FUN_1800068d0(param_1 + 0x68,0,lVar14,uVar7);
              *(undefined8 *)(puVar12 + -8) = 0x180013b6b;
              uVar7 = FUN_180005eb0(param_1 + 0x68,&local_80,param_3);
              lVar4 = local_70;
              param_3 = param_3 - uVar7;
              if (local_78 != 0) {
                pcVar3 = *(code **)(param_1 + 0x3f0);
                *(undefined8 *)(puVar12 + -8) = 0x180013b85;
                (*pcVar3)(lVar4,lVar14,uVar7);
              }
              plVar2 = *(longlong **)(param_1 + 0x418);
              pcVar3 = *(code **)(*plVar2 + 0x20);
              *(undefined8 *)(puVar12 + -8) = 0x180013b97;
              iVar6 = (*pcVar3)(plVar2,uVar11,0);
              if (iVar6 != 0) goto LAB_180013bbe;
            }
          }
          else if (iVar6 != -0x7776fffa) goto LAB_180013bbe;
        }
      } while (param_3 != 0);
    }
    pvVar1 = *(HANDLE *)(param_1 + 0x468);
    *(undefined8 *)(puVar12 + -8) = 0x180013be0;
    SetEvent(pvVar1);
    uVar11 = -(uint)(iVar6 != 0) & 0xffffd8f1;
  }
LAB_180013be9:
  *(undefined8 *)(puVar12 + -8) = 0x180013bf5;
  return uVar11;
}



/* ========================================================================
   ENTRY: 180013c10
   NAME : FUN_180013c10
   SIG  : undefined __fastcall FUN_180013c10(longlong param_1, uint * param_2, undefined * param_3)
   ======================================================================== */

void FUN_180013c10(longlong param_1,uint *param_2,undefined *param_3)

{
  uint uVar1;
  longlong lVar2;
  
  if (*(int *)(param_1 + 0x6c) == 1) {
    uVar1 = FUN_18000e310(*param_2,(uint)*(ushort *)(param_1 + 0x24),param_3);
    *param_2 = uVar1;
  }
  lVar2 = FUN_180010350(*param_2,*(uint *)(param_1 + 0x1c));
  *(longlong *)(param_1 + 0x58) = lVar2;
  return;
}



/* ========================================================================
   ENTRY: 180013c50
   NAME : FUN_180013c50
   SIG  : int __fastcall FUN_180013c50(uint param_1, double param_2, double param_3)
   ======================================================================== */

int FUN_180013c50(uint param_1,double param_2,double param_3)

{
  uint uVar1;
  
  uVar1 = (uint)(longlong)(param_2 * param_3);
  if (uVar1 < param_1) {
    uVar1 = param_1;
  }
  return uVar1 + param_1;
}



/* ========================================================================
   ENTRY: 180013c70
   NAME : FUN_180013c70
   SIG  : undefined __fastcall FUN_180013c70(longlong param_1, int param_2, longlong * param_3)
   ======================================================================== */

void FUN_180013c70(longlong param_1,int param_2,longlong *param_3)

{
  uint uVar1;
  longlong lVar2;
  longlong lVar3;
  longlong lVar4;
  
  if (*(int *)(param_1 + 0x6c) == 1) {
    lVar3 = *(longlong *)(param_1 + 0x78);
    *param_3 = *(longlong *)(param_1 + 0x58);
    if ((((*(uint *)(param_1 + 0x70) & 0x40000) == 0) && (param_2 != 0)) &&
       (*(int *)(param_1 + 0xfc) == 0)) {
      uVar1 = FUN_18000e310(*(int *)(param_1 + 0xe8),(uint)*(ushort *)(param_1 + 0x24),FUN_18000dfe0
                           );
      lVar2 = FUN_180010350(uVar1,*(uint *)(param_1 + 0x1c));
      lVar4 = *(longlong *)(param_1 + 0x58);
      if (lVar2 <= *(longlong *)(param_1 + 0x58)) {
        lVar4 = lVar2;
      }
      lVar3 = *(longlong *)(lVar3 + 0x218);
      if (lVar3 <= lVar4) {
        lVar3 = lVar4;
      }
      *param_3 = lVar3;
    }
    return;
  }
  *param_3 = 0;
  return;
}



/* ========================================================================
   ENTRY: 180013d10
   NAME : FUN_180013d10
   SIG  : undefined8 __fastcall FUN_180013d10(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180013d10(undefined4 param_1)

{
  switch(param_1) {
  default:
    return 0;
  case 1:
    return 1;
  case 2:
    return 2;
  case 3:
    return 3;
  case 4:
    return 4;
  case 5:
    return 5;
  case 6:
    return 6;
  case 7:
    return 7;
  case 8:
    return 8;
  case 9:
    return 9;
  case 10:
    return 10;
  case 0xb:
    return 0xb;
  }
}



/* ========================================================================
   ENTRY: 180013da0
   NAME : FUN_180013da0
   SIG  : undefined8 __fastcall FUN_180013da0(int param_1)
   ======================================================================== */

undefined8 FUN_180013da0(int param_1)

{
  if (param_1 != 0) {
    if (param_1 == 1) {
      return 1;
    }
    if (param_1 == 2) {
      return 2;
    }
    if (param_1 == 3) {
      return 3;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180013dd0
   NAME : FUN_180013dd0
   SIG  : undefined8 __fastcall FUN_180013dd0(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180013dd0(undefined4 param_1)

{
  switch(param_1) {
  case 1:
    return 1;
  case 2:
    return 2;
  case 3:
    return 3;
  case 4:
    return 4;
  case 5:
    return 5;
  case 6:
    return 6;
  case 7:
    return 7;
  case 8:
    return 8;
  case 9:
    return 9;
  case 10:
    return 10;
  case 0xb:
    return 0xb;
  case 0xc:
    return 0xc;
  case 0xd:
    return 0xd;
  default:
    return 0;
  }
}



/* ========================================================================
   ENTRY: 180013e80
   NAME : FUN_180013e80
   SIG  : undefined8 __fastcall FUN_180013e80(int * param_1)
   ======================================================================== */

undefined8 FUN_180013e80(int *param_1)

{
  undefined8 uVar1;
  longlong local_res10 [3];
  
  uVar1 = FUN_180003c40(local_res10,0xd);
  if ((int)uVar1 != 0) {
    if (param_1 != (int *)0x0) {
      *param_1 = (int)uVar1;
    }
    return 0;
  }
  return local_res10[0];
}



/* ========================================================================
   ENTRY: 180013ec0
   NAME : FUN_180013ec0
   SIG  : undefined8 __fastcall FUN_180013ec0(longlong * param_1, int param_2)
   ======================================================================== */

undefined8 FUN_180013ec0(longlong *param_1,int param_2)

{
  int *piVar1;
  undefined8 uVar2;
  uint local_res18 [2];
  int local_res20 [2];
  
  piVar1 = (int *)FUN_180013e80(local_res20);
  if (piVar1 == (int *)0x0) {
    return 0xffffd8f0;
  }
  uVar2 = FUN_180003c20((int *)local_res18,param_2,piVar1);
  if ((int)uVar2 == 0) {
    if ((uint)piVar1[0x46] <= local_res18[0]) {
      return 0xffffd8f4;
    }
    *param_1 = (longlong)(int)local_res18[0] * 0x280 + *(longlong *)(piVar1 + 0x48);
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 1800142b0
   NAME : FUN_1800142b0
   SIG  : int __fastcall FUN_1800142b0(longlong param_1, undefined4 * param_2)
   ======================================================================== */

int FUN_1800142b0(longlong param_1,undefined4 *param_2)

{
  int iVar1;
  
  *param_2 = 0;
  iVar1 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x30))();
  if (iVar1 != 0) {
    iVar1 = FUN_180014510(iVar1);
    return iVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180014300
   NAME : FUN_180014300
   SIG  : int __fastcall FUN_180014300(longlong param_1, int * param_2)
   ======================================================================== */

int FUN_180014300(longlong param_1,int *param_2)

{
  int iVar1;
  int iVar2;
  int local_res8 [2];
  
  iVar2 = *(int *)(param_1 + 0x330);
  *param_2 = 0;
  iVar1 = (**(code **)(**(longlong **)(param_1 + 0x2d8) + 0x30))
                    (*(longlong **)(param_1 + 0x2d8),local_res8);
  if (iVar1 != 0) {
    iVar2 = FUN_180014510(iVar1);
    return iVar2;
  }
  *param_2 = iVar2 - local_res8[0];
  return 0;
}



/* ========================================================================
   ENTRY: 180014370
   NAME : FUN_180014370
   SIG  : undefined __fastcall FUN_180014370(longlong param_1, uint param_2, uint param_3, int param_4, int param_5)
   ======================================================================== */

void FUN_180014370(longlong param_1,uint param_2,uint param_3,int param_4,int param_5)

{
  uint uVar1;
  
  if ((param_2 == 0) || (*(uint *)(param_1 + 0x108) = param_3 / param_2, param_3 / param_2 == 0)) {
    *(undefined4 *)(param_1 + 0x108) = 1;
  }
  if ((*(int *)(param_1 + 0x6c) == 1) || (param_4 != 0)) {
    uVar1 = *(uint *)(param_1 + 0x70) & 0x40000;
    if (uVar1 != 0) {
      *(undefined4 *)(param_1 + 0x110) = 1;
    }
    if ((param_4 != 0 || uVar1 != 0) || (param_5 == 0)) {
      *(undefined4 *)(param_1 + 0x108) = 1;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800143d0
   NAME : FUN_1800143d0
   SIG  : undefined __fastcall FUN_1800143d0(longlong param_1)
   ======================================================================== */

void FUN_1800143d0(longlong param_1)

{
  if (*(HANDLE *)(param_1 + 0x440) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x440));
    *(undefined8 *)(param_1 + 0x440) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x450) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x450));
    *(undefined8 *)(param_1 + 0x450) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x458) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x458));
    *(undefined8 *)(param_1 + 0x458) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x448) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x448));
    *(undefined8 *)(param_1 + 0x448) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x460) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x460));
    *(undefined8 *)(param_1 + 0x460) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x468) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x468));
    *(undefined8 *)(param_1 + 0x468) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180014480
   NAME : FUN_180014480
   SIG  : undefined __fastcall FUN_180014480(longlong param_1)
   ======================================================================== */

void FUN_180014480(longlong param_1)

{
  longlong *plVar1;
  
  if (*(int *)(param_1 + 0x490) == 0) {
    if (*(longlong **)(param_1 + 0x180) != (longlong *)0x0) {
      (**(code **)(**(longlong **)(param_1 + 0x180) + 0x58))();
    }
    plVar1 = *(longlong **)(param_1 + 0x2d8);
  }
  else {
    if (*(longlong **)(param_1 + 0x170) != (longlong *)0x0) {
      (**(code **)(**(longlong **)(param_1 + 0x170) + 0x58))();
    }
    plVar1 = *(longlong **)(param_1 + 0x2c8);
  }
  if (plVar1 != (longlong *)0x0) {
    (**(code **)(*plVar1 + 0x58))();
  }
  if (*(longlong *)(param_1 + 0x498) != 0) {
    PaWasapi_ThreadPriorityRevert(*(longlong *)(param_1 + 0x498));
    *(undefined8 *)(param_1 + 0x498) = 0;
  }
  if (*(code **)(param_1 + 0x20) != (code *)0x0) {
                    /* WARNING: Could not recover jumptable at 0x0001800144ff. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180014510
   NAME : FUN_180014510
   SIG  : int __fastcall FUN_180014510(int param_1)
   ======================================================================== */

int FUN_180014510(int param_1)

{
  char *pcVar1;
  
  if (param_1 < -0x7776fffe) {
    if (param_1 == -0x7776ffff) {
      pcVar1 = "AUDCLNT_E_NOT_INITIALIZED";
      goto LAB_180014750;
    }
    if (param_1 == -0x7fffbffd) {
      pcVar1 = "E_POINTER";
      goto LAB_180014750;
    }
    if (param_1 == -0x7ffbfe10) {
      pcVar1 = "CO_E_NOTINITIALIZED: you must call CoInitialize() before Pa_OpenStream()";
      goto LAB_180014750;
    }
    if (param_1 == -0x7ff8ffa9) {
      pcVar1 = "E_INVALIDARG";
      goto LAB_180014750;
    }
  }
  else {
    if (param_1 < 1) {
      if (param_1 == 0) {
        return 0;
      }
      switch(param_1) {
      case -0x7776fffe:
        pcVar1 = "AUDCLNT_E_ALREADY_INITIALIZED";
        break;
      case -0x7776fffd:
        pcVar1 = "AUDCLNT_E_WRONG_ENDPOINT_TYPE";
        break;
      case -0x7776fffc:
        pcVar1 = "AUDCLNT_E_DEVICE_INVALIDATED";
        break;
      case -0x7776fffb:
        pcVar1 = "AUDCLNT_E_NOT_STOPPED";
        break;
      case -0x7776fffa:
        pcVar1 = "AUDCLNT_E_BUFFER_TOO_LARGE";
        break;
      case -0x7776fff9:
        pcVar1 = "AUDCLNT_E_OUT_OF_ORDER";
        break;
      case -0x7776fff8:
        pcVar1 = "AUDCLNT_E_UNSUPPORTED_FORMAT";
        break;
      case -0x7776fff7:
        pcVar1 = "AUDCLNT_E_INVALID_SIZE";
        break;
      case -0x7776fff6:
        pcVar1 = "AUDCLNT_E_DEVICE_IN_USE";
        break;
      case -0x7776fff5:
        pcVar1 = "AUDCLNT_E_BUFFER_OPERATION_PENDING";
        break;
      case -0x7776fff4:
        pcVar1 = "AUDCLNT_E_THREAD_NOT_REGISTERED";
        break;
      default:
        goto switchD_18001459e_caseD_8889000d;
      case -0x7776fff2:
        pcVar1 = "AUDCLNT_E_EXCLUSIVE_MODE_NOT_ALLOWED";
        break;
      case -0x7776fff1:
        pcVar1 = "AUDCLNT_E_ENDPOINT_CREATE_FAILED";
        break;
      case -0x7776fff0:
        pcVar1 = "AUDCLNT_E_SERVICE_NOT_RUNNING";
        break;
      case -0x7776ffef:
        pcVar1 = "AUDCLNT_E_EVENTHANDLE_NOT_EXPECTED";
        break;
      case -0x7776ffee:
        pcVar1 = "AUDCLNT_E_EXCLUSIVE_MODE_ONLY";
        break;
      case -0x7776ffed:
        pcVar1 = "AUDCLNT_E_BUFDURATION_PERIOD_NOT_EQUAL";
        break;
      case -0x7776ffec:
        pcVar1 = "AUDCLNT_E_EVENTHANDLE_NOT_SET";
        break;
      case -0x7776ffeb:
        pcVar1 = "AUDCLNT_E_INCORRECT_BUFFER_SIZE";
        break;
      case -0x7776ffea:
        pcVar1 = "AUDCLNT_E_BUFFER_SIZE_ERROR";
        break;
      case -0x7776ffe9:
        pcVar1 = "AUDCLNT_E_CPUUSAGE_EXCEEDED";
        break;
      case -0x7776ffe8:
        pcVar1 = "AUDCLNT_E_BUFFER_ERROR";
        break;
      case -0x7776ffe7:
        pcVar1 = "AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED";
        break;
      case -0x7776ffe0:
        pcVar1 = "AUDCLNT_E_INVALID_DEVICE_PERIOD";
        break;
      case -0x7776ffdf:
        pcVar1 = "AUDCLNT_E_INVALID_STREAM_FLAG";
        break;
      case -0x7776ffde:
        pcVar1 = "AUDCLNT_E_ENDPOINT_OFFLOAD_NOT_CAPABLE";
        break;
      case -0x7776ffdd:
        pcVar1 = "AUDCLNT_E_OUT_OF_OFFLOAD_RESOURCES";
        break;
      case -0x7776ffdc:
        pcVar1 = "AUDCLNT_E_OFFLOAD_MODE_ONLY";
        break;
      case -0x7776ffdb:
        pcVar1 = "AUDCLNT_E_NONOFFLOAD_MODE_ONLY";
        break;
      case -0x7776ffda:
        pcVar1 = "AUDCLNT_E_RESOURCES_INVALIDATED";
        break;
      case -0x7776ffd9:
        pcVar1 = "AUDCLNT_E_RAW_MODE_UNSUPPORTED";
        break;
      case -0x7776ffd8:
        pcVar1 = "AUDCLNT_E_ENGINE_PERIODICITY_LOCKED";
        break;
      case -0x7776ffd7:
        pcVar1 = "AUDCLNT_E_ENGINE_FORMAT_LOCKED";
      }
      goto LAB_180014750;
    }
    if (param_1 == 0x8890001) {
      pcVar1 = "AUDCLNT_S_BUFFER_EMPTY";
      goto LAB_180014750;
    }
    if (param_1 == 0x8890002) {
      pcVar1 = "AUDCLNT_S_THREAD_ALREADY_REGISTERED";
      goto LAB_180014750;
    }
    if (param_1 == 0x8890003) {
      pcVar1 = "AUDCLNT_S_POSITION_STALLED";
      goto LAB_180014750;
    }
  }
switchD_18001459e_caseD_8889000d:
  pcVar1 = "UNKNOWN ERROR";
LAB_180014750:
  FUN_180003c90(0xd,param_1,pcVar1);
  return param_1;
}



/* ========================================================================
   ENTRY: 180014810
   NAME : FUN_180014810
   SIG  : undefined4 __fastcall FUN_180014810(undefined4 param_1)
   ======================================================================== */

undefined4 FUN_180014810(undefined4 param_1)

{
  return param_1;
}



/* ========================================================================
   ENTRY: 180014820
   NAME : snprintf
   SIG  : int __fastcall snprintf(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

/* Library Function - Single Match
    snprintf
   
   Library: Visual Studio 2019 Release */

int snprintf(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  int iVar1;
  ulonglong *puVar2;
  undefined8 local_res20;
  
  local_res20 = param_4;
  puVar2 = (ulonglong *)FUN_180003990();
  iVar1 = __stdio_common_vsprintf(*puVar2 | 1,param_1,param_2,param_3,0,&local_res20);
  if (iVar1 < 0) {
    iVar1 = -1;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 180014880
   NAME : FUN_180014880
   SIG  : double __fastcall FUN_180014880(longlong param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

double FUN_180014880(longlong param_1)

{
  return (double)param_1 * _DAT_180024870;
}



/* ========================================================================
   ENTRY: 1800148a0
   NAME : FUN_1800148a0
   SIG  : undefined8 __fastcall FUN_1800148a0(longlong param_1)
   ======================================================================== */

undefined8 FUN_1800148a0(longlong param_1)

{
  undefined8 *puVar1;
  DWORD DVar2;
  undefined8 uVar3;
  
  puVar1 = (undefined8 *)(param_1 + 0x240);
  if (*(int *)(param_1 + 0x22c) == 0) {
    uVar3 = 0;
    CloseHandle((HANDLE)*puVar1);
    *puVar1 = 0;
    *(undefined4 *)(param_1 + 0x228) = 0;
  }
  else {
    *(undefined4 *)(param_1 + 0x234) = 1;
    SetEvent(*(HANDLE *)(param_1 + 0x248));
    DVar2 = WaitForSingleObject((HANDLE)*puVar1,10000);
    if (DVar2 == 0) {
      CloseHandle((HANDLE)*puVar1);
      uVar3 = 0;
    }
    else {
      TerminateThread((HANDLE)*puVar1,0xffffffff);
      uVar3 = 0xffffd8fd;
      CloseHandle((HANDLE)*puVar1);
    }
    *puVar1 = 0;
    *(undefined4 *)(param_1 + 0x228) = 0;
    if (*(code **)(param_1 + 0x20) != (code *)0x0) {
      (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
    }
  }
  *(undefined8 *)(param_1 + 0x228) = 0;
  return uVar3;
}



/* ========================================================================
   ENTRY: 180014960
   NAME : FUN_180014960
   SIG  : longlong __fastcall FUN_180014960(int * param_1, uint * param_2, undefined4 * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

longlong FUN_180014960(int *param_1,uint *param_2,undefined4 *param_3)

{
  bool bVar1;
  int iVar2;
  BOOL BVar3;
  int iVar4;
  BOOL BVar5;
  LSTATUS LVar6;
  int iVar7;
  HDEVINFO DeviceInfoSet;
  longlong lVar8;
  undefined7 extraout_var;
  undefined7 extraout_var_00;
  undefined7 extraout_var_01;
  HKEY hKey;
  longlong *plVar9;
  ulonglong uVar10;
  uint uVar11;
  DWORD DVar12;
  uint uVar13;
  DWORD MemberIndex;
  DWORD MemberIndex_00;
  undefined1 auStackY_538 [32];
  DWORD local_4f8;
  int local_4f4;
  longlong local_4f0;
  DWORD local_4e8 [2];
  HDEVINFO local_4e0;
  int *local_4d8;
  uint *local_4d0;
  _SP_DEVINFO_DATA local_4c8;
  _SP_DEVICE_INTERFACE_DATA local_4a8;
  _SP_DEVICE_INTERFACE_DATA local_488;
  wchar_t local_468 [264];
  _SP_DEVICE_INTERFACE_DETAIL_DATA_W local_258 [66];
  ulonglong local_48;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStackY_538;
  MemberIndex = 0;
  uVar13 = 0;
  DVar12 = 0;
  *param_1 = 0;
  *param_2 = 0;
  local_258[0].cbSize = 8;
  local_4d8 = param_1;
  local_4d0 = param_2;
  DeviceInfoSet = SetupDiGetClassDevsA((GUID *)&DAT_180024950,(PCSTR)0x0,(HWND)0x0,0x12);
  if (DeviceInfoSet == (HDEVINFO)0xffffffffffffffff) {
    *param_3 = 0xffffd8f1;
    lVar8 = 0;
  }
  else {
    local_4a8.cbSize = 0x20;
    local_4a8.Reserved = 0;
    local_488.cbSize = 0x20;
    local_488.Reserved = 0;
    local_4e0 = DeviceInfoSet;
    iVar2 = SetupDiEnumDeviceInterfaces
                      (DeviceInfoSet,(PSP_DEVINFO_DATA)0x0,(GUID *)&DAT_180024950,0,&local_4a8);
    MemberIndex_00 = DVar12;
    while (iVar2 != 0) {
      BVar3 = SetupDiGetDeviceInterfaceAlias
                        (DeviceInfoSet,&local_4a8,(GUID *)&DAT_1800248f0,&local_488);
      uVar11 = uVar13;
      if (((BVar3 != 0) && (uVar11 = 0, local_488.Flags != 0)) &&
         (uVar11 = 0, (local_488.Flags & 4) == 0)) {
        uVar11 = 1;
      }
      BVar3 = SetupDiGetDeviceInterfaceAlias
                        (DeviceInfoSet,&local_4a8,(GUID *)&DAT_1800248e0,&local_488);
      if (((BVar3 != 0) && (local_488.Flags != 0)) && ((local_488.Flags & 4) == 0)) {
        uVar11 = uVar11 | 2;
      }
      local_4a8.cbSize = 0x20;
      local_4a8.Reserved = 0;
      local_488.cbSize = 0x20;
      DVar12 = DVar12 + (uVar11 == 0);
      local_488.Reserved = 0;
      MemberIndex_00 = MemberIndex_00 + 1;
      iVar2 = SetupDiEnumDeviceInterfaces
                        (DeviceInfoSet,(PSP_DEVINFO_DATA)0x0,(GUID *)&DAT_180024950,MemberIndex_00,
                         &local_4a8);
    }
    iVar2 = MemberIndex_00 - DVar12;
    lVar8 = FUN_180020230(iVar2 * 8);
    local_4f0 = lVar8;
    if (lVar8 == 0) {
      if (DeviceInfoSet != (HDEVINFO)0x0) {
        SetupDiDestroyDeviceInfoList(DeviceInfoSet);
      }
      *param_3 = 0xffffd8f8;
      lVar8 = 0;
    }
    else {
      local_4a8.cbSize = 0x20;
      local_4a8.Reserved = 0;
      local_488.Reserved = 0;
      local_4c8.Reserved = 0;
      local_488.cbSize = 0x20;
      local_4c8.cbSize = 0x20;
      iVar4 = SetupDiEnumDeviceInterfaces
                        (DeviceInfoSet,(PSP_DEVINFO_DATA)0x0,(GUID *)&DAT_180024950,0,&local_4a8);
      DVar12 = MemberIndex;
      while (iVar4 != 0) {
        uVar11 = 0;
        BVar3 = SetupDiGetDeviceInterfaceAlias
                          (DeviceInfoSet,&local_4a8,(GUID *)&DAT_1800248f0,&local_488);
        if (((BVar3 != 0) && (local_488.Flags != 0)) && (uVar11 = 0, (local_488.Flags & 4) == 0)) {
          uVar11 = 1;
        }
        BVar3 = SetupDiGetDeviceInterfaceAlias
                          (DeviceInfoSet,&local_4a8,(GUID *)&DAT_1800248e0,&local_488);
        if (((BVar3 != 0) && (local_488.Flags != 0)) && ((local_488.Flags & 4) == 0)) {
          uVar11 = uVar11 | 2;
        }
        if (uVar11 != 0) {
          BVar3 = SetupDiGetDeviceInterfaceAlias
                            (DeviceInfoSet,&local_4a8,(GUID *)&DAT_180024960,&local_488);
          BVar5 = SetupDiGetDeviceInterfaceDetailW
                            (DeviceInfoSet,&local_4a8,local_258,0x210,(PDWORD)0x0,&local_4c8);
          if (BVar5 != 0) {
            memset(local_468,0,0x208);
            local_4f4 = 0;
            local_4f8 = 0x208;
            bVar1 = FUN_180015ff0();
            if ((((int)CONCAT71(extraout_var,bVar1) == 0) ||
                (bVar1 = FUN_180016420(local_258[0].DevicePath),
                (int)CONCAT71(extraout_var_00,bVar1) == 0)) ||
               (BVar5 = SetupDiGetDeviceRegistryPropertyW
                                  (DeviceInfoSet,&local_4c8,0xd,local_4e8,(PBYTE)local_468,0x208,
                                   (PDWORD)0x0), BVar5 != 0)) {
              if ((local_468[0] == L'\0') ||
                 (bVar1 = FUN_1800163d0(local_468), (int)CONCAT71(extraout_var_01,bVar1) != 0))
              goto LAB_180014cc1;
            }
            else {
              local_468[0] = L'\0';
LAB_180014cc1:
              hKey = SetupDiOpenDeviceInterfaceRegKey(DeviceInfoSet,&local_4a8,0,1);
              if (hKey != (HKEY)0xffffffffffffffff) {
                LVar6 = RegQueryValueExW(hKey,L"FriendlyName",(LPDWORD)0x0,local_4e8,
                                         (LPBYTE)local_468,&local_4f8);
                if (LVar6 == 0) {
                  RegCloseKey(hKey);
                }
                else {
                  local_468[0] = L'\0';
                }
              }
            }
            FUN_18001b460(local_468,(ulonglong)local_4f8);
            plVar9 = FUN_180015770((BVar3 != 0) + 1,local_4c8.DevInst,local_258[0].DevicePath,
                                   local_468,&local_4f4);
            if (local_4f4 == 0) {
              iVar4 = 0;
              if (0 < (int)plVar9[0x86]) {
                uVar10 = 0;
                do {
                  lVar8 = *(longlong *)(plVar9[0x87] + uVar10 * 8);
                  if (lVar8 != 0) {
                    iVar7 = *(int *)(lVar8 + 0x10);
                    if (iVar7 == 0) {
                      iVar7 = 1;
                    }
                    iVar4 = iVar4 + iVar7;
                  }
                  uVar11 = (int)uVar10 + 1;
                  uVar10 = (ulonglong)uVar11;
                  DeviceInfoSet = local_4e0;
                } while ((int)uVar11 < (int)plVar9[0x86]);
              }
              uVar13 = uVar13 + iVar4;
              lVar8 = (longlong)(int)DVar12;
              DVar12 = DVar12 + 1;
              *(longlong **)(local_4f0 + lVar8 * 8) = plVar9;
            }
            else {
              iVar2 = iVar2 + -1;
            }
          }
        }
        lVar8 = local_4f0;
        local_4a8.cbSize = 0x20;
        MemberIndex = MemberIndex + 1;
        local_4a8.Reserved = 0;
        local_488.cbSize = 0x20;
        local_488.Reserved = 0;
        local_4c8.cbSize = 0x20;
        local_4c8.Reserved = 0;
        iVar4 = SetupDiEnumDeviceInterfaces
                          (DeviceInfoSet,(PSP_DEVINFO_DATA)0x0,(GUID *)&DAT_180024950,MemberIndex,
                           &local_4a8);
      }
      if (DeviceInfoSet != (HDEVINFO)0x0) {
        SetupDiDestroyDeviceInfoList(DeviceInfoSet);
      }
      *local_4d8 = iVar2;
      *local_4d0 = uVar13;
    }
  }
  return lVar8;
}



/* ========================================================================
   ENTRY: 180014e60
   NAME : FUN_180014e60
   SIG  : longlong __fastcall FUN_180014e60(void)
   ======================================================================== */

longlong FUN_180014e60(void)

{
  int iVar1;
  HANDLE hThread;
  longlong lVar2;
  undefined4 local_res8 [2];
  
  hThread = GetCurrentThread();
  lVar2 = 0;
  local_res8[0] = 0;
  if (((DAT_18002bb98 != (code *)0x0) &&
      (lVar2 = (*DAT_18002bb98)("Pro Audio",local_res8), lVar2 - 1U < 0xfffffffffffffffe)) &&
     (iVar1 = (*DAT_18002bba8)(lVar2,2), iVar1 != 0)) {
    return lVar2;
  }
  timeBeginPeriod(1);
  SetThreadPriority(hThread,0xf);
  return lVar2;
}



/* ========================================================================
   ENTRY: 180014ee0
   NAME : FUN_180014ee0
   SIG  : undefined8 __fastcall FUN_180014ee0(undefined4 * param_1)
   ======================================================================== */

undefined8 FUN_180014ee0(undefined4 *param_1)

{
  FUN_1800069c0((longlong)(param_1 + 0x26));
  FUN_180006ea0(param_1);
  FUN_180014f90((longlong)param_1);
  if (*(longlong *)(param_1 + 0x68) != 0) {
    FUN_180001280(*(longlong *)(param_1 + 0x68));
    FUN_180001230(*(longlong *)(param_1 + 0x68));
    *(undefined8 *)(param_1 + 0x68) = 0;
  }
  if (*(longlong **)(param_1 + 0x7a) != (longlong *)0x0) {
    FUN_180018770(*(longlong **)(param_1 + 0x7a));
  }
  if (*(longlong **)(param_1 + 0x6a) != (longlong *)0x0) {
    FUN_180018770(*(longlong **)(param_1 + 0x6a));
  }
  if (*(longlong *)(param_1 + 0x7a) != 0) {
    FUN_180015540(*(undefined8 **)(*(longlong *)(param_1 + 0x7a) + 0x220));
  }
  if (*(longlong *)(param_1 + 0x6a) != 0) {
    FUN_180015540(*(undefined8 **)(*(longlong *)(param_1 + 0x6a) + 0x220));
  }
  FUN_180020240((longlong)param_1);
  return 0;
}



/* ========================================================================
   ENTRY: 180014f90
   NAME : FUN_180014f90
   SIG  : undefined __fastcall FUN_180014f90(longlong param_1)
   ======================================================================== */

void FUN_180014f90(longlong param_1)

{
  undefined8 *puVar1;
  HANDLE pvVar2;
  ulonglong uVar3;
  uint uVar4;
  ulonglong uVar5;
  
  uVar3 = 0;
  if (*(HANDLE *)(param_1 + 0x248) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x248));
    *(undefined8 *)(param_1 + 0x248) = 0;
  }
  if (*(HANDLE *)(param_1 + 0x250) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x250));
  }
  if (*(HANDLE *)(param_1 + 600) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 600));
  }
  puVar1 = *(undefined8 **)(param_1 + 0x1a8);
  if ((((puVar1 != (undefined8 *)0x0) && (*(int *)(puVar1[0x44] + 0x418) == 2)) &&
      (*(int *)(puVar1 + 0x45) == 1)) && (*(longlong **)(param_1 + 0x1d0) != (longlong *)0x0)) {
    FUN_18001a100(puVar1,**(longlong **)(param_1 + 0x1d0));
  }
  uVar5 = uVar3;
  if (*(int *)(param_1 + 0x1c8) != 0) {
    do {
      if ((*(longlong *)(param_1 + 0x1d0) != 0) &&
         (pvVar2 = *(HANDLE *)(*(longlong *)(param_1 + 0x1d0) + uVar5 * 8), pvVar2 != (HANDLE)0x0))
      {
        CloseHandle(pvVar2);
        *(undefined8 *)(*(longlong *)(param_1 + 0x1d0) + uVar5 * 8) = 0;
      }
      uVar4 = (int)uVar5 + 1;
      uVar5 = (ulonglong)uVar4;
    } while (uVar4 < *(uint *)(param_1 + 0x1c8));
  }
  puVar1 = *(undefined8 **)(param_1 + 0x1e8);
  if (((puVar1 != (undefined8 *)0x0) && (*(int *)(puVar1[0x44] + 0x418) == 2)) &&
     ((*(int *)(puVar1 + 0x45) == 1 && (*(longlong **)(param_1 + 0x210) != (longlong *)0x0)))) {
    FUN_18001a100(puVar1,**(longlong **)(param_1 + 0x210));
  }
  if (*(int *)(param_1 + 0x208) != 0) {
    do {
      if ((*(longlong *)(param_1 + 0x210) != 0) &&
         (pvVar2 = *(HANDLE *)(*(longlong *)(param_1 + 0x210) + uVar3 * 8), pvVar2 != (HANDLE)0x0))
      {
        CloseHandle(pvVar2);
        *(undefined8 *)(*(longlong *)(param_1 + 0x210) + uVar3 * 8) = 0;
      }
      uVar4 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar4;
    } while (uVar4 < *(uint *)(param_1 + 0x208));
  }
  return;
}



/* ========================================================================
   ENTRY: 180015100
   NAME : FUN_180015100
   SIG  : undefined8 __fastcall FUN_180015100(longlong param_1, undefined8 param_2, undefined8 * param_3, int param_4)
   ======================================================================== */

undefined8 FUN_180015100(longlong param_1,undefined8 param_2,undefined8 *param_3,int param_4)

{
  undefined8 *local_res8;
  
  *(undefined4 *)(param_1 + 0x18) = 0;
  *(undefined8 *)(param_1 + 0x1c) = 0xffffffffffffffff;
  if (*(longlong *)(param_1 + 0x28) != 0) {
    local_res8 = (undefined8 *)FUN_1800012e0(*(int **)(param_1 + 0x108),0x10);
    *local_res8 = *(undefined8 *)(param_1 + 0x28);
    FUN_1800152d0(param_1,(longlong *)&local_res8,*(int *)(param_1 + 0x18));
    *(undefined8 *)(param_1 + 0x28) = 0;
  }
  if (param_3 != (undefined8 *)0x0) {
    if (0 < param_4) {
      *(undefined8 *)(param_1 + 0x28) = *param_3;
      *(undefined4 *)(param_1 + 0x1c) = *(undefined4 *)(param_3 + 1);
      *(undefined4 *)(param_1 + 0x20) = *(undefined4 *)((longlong)param_3 + 0xc);
      *(int *)(param_1 + 0x18) = param_4;
    }
    FUN_180001360(*(longlong *)(param_1 + 0x108),(longlong)param_3);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800151b0
   NAME : FUN_1800151b0
   SIG  : undefined8 __fastcall FUN_1800151b0(longlong * param_1, ushort * param_2, int param_3)
   ======================================================================== */

undefined8 FUN_1800151b0(longlong *param_1,ushort *param_2,int param_3)

{
  longlong lVar1;
  longlong lVar2;
  uint uVar3;
  longlong lVar4;
  
  uVar3 = FUN_180015e30(param_2,param_3);
  lVar4 = *param_1;
  lVar2 = 0;
  while (lVar1 = lVar4, lVar1 != 0) {
    if (*(uint *)(lVar1 + 8) == uVar3) {
      *(int *)(lVar1 + 4) = *(int *)(lVar1 + 4) + 1;
      return 0;
    }
    lVar2 = lVar1;
    lVar4 = *(longlong *)(lVar1 + 0x10);
  }
  lVar4 = FUN_1800012e0((int *)param_1[1],0x18);
  if (lVar4 != 0) {
    *(uint *)(lVar4 + 8) = uVar3;
    *(undefined4 *)(lVar4 + 4) = 1;
    if (lVar2 != 0) {
      *(longlong *)(lVar2 + 0x10) = lVar4;
    }
    if (*param_1 == 0) {
      *param_1 = lVar4;
    }
    return 0;
  }
  return 0xffffd8f8;
}



/* ========================================================================
   ENTRY: 180015260
   NAME : FUN_180015260
   SIG  : ulonglong __fastcall FUN_180015260(longlong param_1)
   ======================================================================== */

ulonglong FUN_180015260(longlong param_1)

{
  bool bVar1;
  undefined7 extraout_var;
  uint uVar2;
  ulonglong uVar3;
  undefined4 *puVar4;
  
  uVar3 = 0;
  puVar4 = &DAT_180025310;
  do {
    bVar1 = FUN_1800163c0(param_1,puVar4[uVar3]);
    if ((int)CONCAT71(extraout_var,bVar1) != 0) {
      return uVar3 & 0xffffffff;
    }
    uVar2 = (int)uVar3 + 1;
    uVar3 = (ulonglong)uVar2;
  } while ((int)uVar2 < 0xd);
  return 0xffffffff;
}



/* ========================================================================
   ENTRY: 1800152a0
   NAME : FUN_1800152a0
   SIG  : undefined __fastcall FUN_1800152a0(undefined8 * param_1)
   ======================================================================== */

void FUN_1800152a0(undefined8 *param_1)

{
  FUN_180001280(param_1[1]);
  FUN_180001230(param_1[1]);
  *param_1 = 0;
  param_1[1] = 0;
  return;
}



/* ========================================================================
   ENTRY: 1800152d0
   NAME : FUN_1800152d0
   SIG  : undefined8 __fastcall FUN_1800152d0(longlong param_1, longlong * param_2, int param_3)
   ======================================================================== */

undefined8 FUN_1800152d0(longlong param_1,longlong *param_2,int param_3)

{
  undefined8 *puVar1;
  uint uVar2;
  ulonglong uVar3;
  
  if (param_2 != (longlong *)0x0) {
    if (*param_2 != 0) {
      uVar3 = 0;
      if (0 < param_3) {
        do {
          puVar1 = *(undefined8 **)(*(longlong *)(*param_2 + uVar3 * 8) + 0x150);
          if (puVar1 != (undefined8 *)0x0) {
            FUN_180015540(puVar1);
          }
          uVar2 = (int)uVar3 + 1;
          uVar3 = (ulonglong)uVar2;
        } while ((int)uVar2 < param_3);
      }
      FUN_180001360(*(longlong *)(param_1 + 0x108),*(longlong *)*param_2);
      FUN_180001360(*(longlong *)(param_1 + 0x108),*param_2);
    }
    FUN_180001360(*(longlong *)(param_1 + 0x108),(longlong)param_2);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015370
   NAME : FUN_180015370
   SIG  : undefined __fastcall FUN_180015370(longlong param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180015370(longlong param_1)

{
  HANDLE hThread;
  
  hThread = GetCurrentThread();
  if (param_1 != 0) {
    (*DAT_18002bba8)(param_1);
                    /* WARNING: Could not recover jumptable at 0x000180015397. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (*_DAT_18002bba0)(param_1);
    return;
  }
  SetThreadPriority(hThread,0);
                    /* WARNING: Could not recover jumptable at 0x0001800153b1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  timeEndPeriod(1);
  return;
}



/* ========================================================================
   ENTRY: 1800153c0
   NAME : FUN_1800153c0
   SIG  : undefined __fastcall FUN_1800153c0(undefined2 * param_1, int param_2, int param_3)
   ======================================================================== */

void FUN_1800153c0(undefined2 *param_1,int param_2,int param_3)

{
  undefined2 uVar1;
  ulonglong uVar2;
  undefined2 *puVar3;
  int iVar4;
  
  if (param_3 != 0) {
    iVar4 = param_2 + -1;
    do {
      uVar1 = *param_1;
      param_3 = param_3 + -1;
      puVar3 = param_1 + 1;
      param_1 = puVar3;
      if (iVar4 != 0) {
        param_1 = puVar3 + iVar4;
        for (uVar2 = (ulonglong)((longlong)iVar4 * 2) / 2; uVar2 != 0; uVar2 = uVar2 - 1) {
          *puVar3 = uVar1;
          puVar3 = puVar3 + 1;
        }
      }
    } while (param_3 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 180015420
   NAME : FUN_180015420
   SIG  : undefined __fastcall FUN_180015420(undefined1 * param_1, int param_2, int param_3)
   ======================================================================== */

void FUN_180015420(undefined1 *param_1,int param_2,int param_3)

{
  undefined1 uVar1;
  undefined1 uVar2;
  undefined1 uVar3;
  int iVar4;
  undefined1 *puVar5;
  
  while (param_3 != 0) {
    uVar1 = *param_1;
    param_3 = param_3 + -1;
    uVar2 = param_1[1];
    uVar3 = param_1[2];
    puVar5 = param_1;
    iVar4 = param_2;
    while( true ) {
      iVar4 = iVar4 + -1;
      param_1 = puVar5 + 3;
      if (iVar4 == 0) break;
      *param_1 = uVar1;
      puVar5[4] = uVar2;
      puVar5[5] = uVar3;
      puVar5 = param_1;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180015470
   NAME : FUN_180015470
   SIG  : undefined __fastcall FUN_180015470(undefined4 * param_1, int param_2, int param_3)
   ======================================================================== */

void FUN_180015470(undefined4 *param_1,int param_2,int param_3)

{
  undefined4 uVar1;
  ulonglong uVar2;
  undefined4 *puVar3;
  int iVar4;
  
  if (param_3 != 0) {
    iVar4 = param_2 + -1;
    do {
      uVar1 = *param_1;
      param_3 = param_3 + -1;
      puVar3 = param_1 + 1;
      param_1 = puVar3;
      if (iVar4 != 0) {
        param_1 = puVar3 + iVar4;
        for (uVar2 = (ulonglong)((longlong)iVar4 * 4) / 4; uVar2 != 0; uVar2 = uVar2 - 1) {
          *puVar3 = uVar1;
          puVar3 = puVar3 + 1;
        }
      }
    } while (param_3 != 0);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800154e0
   NAME : FUN_1800154e0
   SIG  : undefined __fastcall FUN_1800154e0(longlong param_1)
   ======================================================================== */

void FUN_1800154e0(longlong param_1)

{
  if (param_1 != 0) {
    *(int *)(param_1 + 0x668) = *(int *)(param_1 + 0x668) + 1;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800154f0
   NAME : FUN_1800154f0
   SIG  : undefined8 * __fastcall FUN_1800154f0(longlong param_1, int param_2, short * param_3, int * param_4)
   ======================================================================== */

undefined8 * FUN_1800154f0(longlong param_1,int param_2,short *param_3,int *param_4)

{
  undefined8 *puVar1;
  int iVar2;
  undefined8 uVar3;
  undefined8 *puVar4;
  
  puVar1 = *(undefined8 **)(*(longlong *)(param_1 + 0x438) + (longlong)param_2 * 8);
  uVar3 = FUN_180019f70((longlong)puVar1,param_3);
  iVar2 = (int)uVar3;
  if (iVar2 == 0) {
    uVar3 = FUN_180018c50(puVar1);
    iVar2 = (int)uVar3;
  }
  *param_4 = iVar2;
  puVar4 = (undefined8 *)0x0;
  if (iVar2 == 0) {
    puVar4 = puVar1;
  }
  return puVar4;
}



/* ========================================================================
   ENTRY: 180015540
   NAME : FUN_180015540
   SIG  : undefined __fastcall FUN_180015540(undefined8 * param_1)
   ======================================================================== */

void FUN_180015540(undefined8 *param_1)

{
  int iVar1;
  
  if ((param_1 != (undefined8 *)0x0) &&
     (*(int *)(param_1 + 0xcd) = *(int *)(param_1 + 0xcd) + -1, *(int *)(param_1 + 0xcd) < 1)) {
    iVar1 = 0;
    if ((undefined8 *)param_1[0x88] != (undefined8 *)0x0) {
      FUN_180015540((undefined8 *)param_1[0x88]);
      param_1[0x88] = 0;
    }
    if (param_1[0x87] != 0) {
      if (0 < *(int *)(param_1 + 0x86)) {
        do {
          FUN_1800187c0(*(longlong **)(param_1[0x87] + (longlong)iVar1 * 8));
          iVar1 = iVar1 + 1;
        } while (iVar1 < *(int *)(param_1 + 0x86));
      }
      FUN_180020240(param_1[0x87]);
      param_1[0x87] = 0;
    }
    if (param_1[0xcb] != 0) {
      FUN_180020240(param_1[0xcb]);
      param_1[0xcb] = 0;
    }
    if (param_1[0xcc] != 0) {
      FUN_180020240(param_1[0xcc]);
      param_1[0xcc] = 0;
    }
    if ((HANDLE)*param_1 != (HANDLE)0x0) {
      CloseHandle((HANDLE)*param_1);
    }
    FUN_180020240((longlong)param_1);
  }
  return;
}



/* ========================================================================
   ENTRY: 180015630
   NAME : FUN_180015630
   SIG  : int __fastcall FUN_180015630(undefined8 * param_1)
   ======================================================================== */

int FUN_180015630(undefined8 *param_1)

{
  longlong lVar1;
  longlong *plVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int local_res8 [2];
  
  iVar5 = 0;
  local_res8[0] = 0;
  if ((*(int *)(param_1 + 0x83) == 0) || (param_1[0x87] != 0)) {
    return 0;
  }
  lVar1 = FUN_180020230(*(int *)(param_1 + 0x86) << 3);
  param_1[0x87] = lVar1;
  if (lVar1 == 0) {
    iVar3 = -0x2708;
  }
  else {
    iVar4 = iVar5;
    if (0 < *(int *)(param_1 + 0x86)) {
      do {
        plVar2 = FUN_180018f60(param_1,iVar4,local_res8);
        iVar3 = local_res8[0];
        if (local_res8[0] == -0x2708) goto LAB_1800156e2;
        if (plVar2 == (longlong *)0x0) {
          *(undefined8 *)(param_1[0x87] + (longlong)iVar4 * 8) = 0;
        }
        else {
          *(longlong **)(param_1[0x87] + (longlong)iVar4 * 8) = plVar2;
          *(int *)(param_1 + 0xca) = *(int *)(param_1 + 0xca) + 1;
        }
        iVar4 = iVar4 + 1;
      } while (iVar4 < *(int *)(param_1 + 0x86));
    }
    if (*(int *)(param_1 + 0xca) != 0) {
      return 0;
    }
    iVar3 = -0x2701;
  }
LAB_1800156e2:
  if (param_1[0x87] != 0) {
    if (0 < *(int *)(param_1 + 0x86)) {
      do {
        plVar2 = *(longlong **)(param_1[0x87] + (longlong)iVar5 * 8);
        if (plVar2 != (longlong *)0x0) {
          FUN_1800187c0(plVar2);
          *(undefined8 *)(param_1[0x87] + (longlong)iVar5 * 8) = 0;
        }
        iVar5 = iVar5 + 1;
      } while (iVar5 < *(int *)(param_1 + 0x86));
    }
    FUN_180020240(param_1[0x87]);
    param_1[0x87] = 0;
  }
  return iVar3;
}



/* ========================================================================
   ENTRY: 180015770
   NAME : FUN_180015770
   SIG  : longlong * __fastcall FUN_180015770(int param_1, undefined4 param_2, wchar_t * param_3, wchar_t * param_4, int * param_5)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

longlong *
FUN_180015770(int param_1,undefined4 param_2,wchar_t *param_3,wchar_t *param_4,int *param_5)

{
  int iVar1;
  longlong *plVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  undefined1 auStackY_c8 [32];
  undefined1 local_88 [16];
  undefined8 local_78;
  undefined8 uStack_70;
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_c8;
  plVar2 = (longlong *)FUN_180020230(0x670);
  if (plVar2 == (longlong *)0x0) {
    iVar1 = -0x2708;
  }
  else {
    *(int *)(plVar2 + 0x83) = param_1;
    *(undefined4 *)((longlong)plVar2 + 0x42c) = param_2;
    wcsncpy((wchar_t *)(plVar2 + 1),param_3,0x104);
    wcsncpy((wchar_t *)(plVar2 + 0x89),param_4,0x104);
    uVar3 = FUN_180015970(plVar2);
    iVar1 = (int)uVar3;
    if ((iVar1 == 0) &&
       (iVar1 = FUN_18001b650((HANDLE)*plVar2,0,(undefined8 *)&DAT_180024920,1,plVar2 + 0x86,4,
                              (LPDWORD)0x0), iVar1 == 0)) {
      uVar4 = FUN_18001b6c0((HANDLE)*plVar2,(undefined8 *)&DAT_1800248d0,2,plVar2 + 0xcb);
      iVar1 = (int)uVar4;
      if (iVar1 == 0) {
        uVar4 = FUN_18001b6c0((HANDLE)*plVar2,(undefined8 *)&DAT_1800248d0,1,plVar2 + 0xcc);
        iVar1 = (int)uVar4;
        if (iVar1 == 0) {
          iVar1 = FUN_18001b7a0((HANDLE)*plVar2,(undefined8 *)&DAT_1800248c0,0,local_88,0x48);
          if (iVar1 == 0) {
            *(undefined8 *)((longlong)plVar2 + 0x41c) = local_78;
            *(undefined8 *)((longlong)plVar2 + 0x424) = uStack_70;
          }
          if ((param_1 == 0) || (iVar1 = FUN_180015630(plVar2), iVar1 == 0)) {
            FUN_180015920(plVar2);
            *param_5 = 0;
            return plVar2;
          }
        }
      }
    }
  }
  FUN_180015540(plVar2);
  *param_5 = iVar1;
  return (longlong *)0x0;
}



/* ========================================================================
   ENTRY: 180015920
   NAME : FUN_180015920
   SIG  : undefined __fastcall FUN_180015920(undefined8 * param_1)
   ======================================================================== */

void FUN_180015920(undefined8 *param_1)

{
  int *piVar1;
  longlong *plVar2;
  
  plVar2 = (longlong *)param_1[0x88];
  if ((plVar2 != (longlong *)0x0) && (*plVar2 != 0)) {
    FUN_180015920(plVar2);
  }
  piVar1 = (int *)((longlong)param_1 + 0x654);
  *piVar1 = *piVar1 + -1;
  if ((*piVar1 == 0) && ((HANDLE)*param_1 != (HANDLE)0x0)) {
    CloseHandle((HANDLE)*param_1);
    *param_1 = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180015970
   NAME : FUN_180015970
   SIG  : undefined8 __fastcall FUN_180015970(longlong * param_1)
   ======================================================================== */

undefined8 FUN_180015970(longlong *param_1)

{
  HANDLE pvVar1;
  
  if (*param_1 == 0) {
    pvVar1 = CreateFileW((LPCWSTR)(param_1 + 1),0xc0000000,0,(LPSECURITY_ATTRIBUTES)0x0,3,0x40000080
                         ,(HANDLE)0x0);
    *param_1 = (longlong)pvVar1;
    if (pvVar1 == (HANDLE)0x0) {
      return 0xffffd8ff;
    }
  }
  *(int *)((longlong)param_1 + 0x654) = *(int *)((longlong)param_1 + 0x654) + 1;
  return 0;
}



/* ========================================================================
   ENTRY: 1800159d0
   NAME : FUN_1800159d0
   SIG  : longlong __fastcall FUN_1800159d0(int param_1, longlong param_2)
   ======================================================================== */

longlong FUN_1800159d0(int param_1,longlong param_2)

{
  longlong lVar1;
  uint uVar2;
  ulonglong uVar3;
  
  lVar1 = *(longlong *)(param_2 + 0x658);
  uVar3 = 0;
  if (*(uint *)(lVar1 + 4) != 0) {
    do {
      if ((*(int *)(lVar1 + 0x10 + uVar3 * 0x10) == -1) &&
         (*(int *)(lVar1 + 0x14 + uVar3 * 0x10) == param_1)) {
        return lVar1 + 8 + uVar3 * 0x10;
      }
      uVar2 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar2;
    } while (uVar2 < *(uint *)(lVar1 + 4));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015a20
   NAME : FUN_180015a20
   SIG  : longlong __fastcall FUN_180015a20(int param_1, longlong param_2)
   ======================================================================== */

longlong FUN_180015a20(int param_1,longlong param_2)

{
  longlong lVar1;
  uint uVar2;
  ulonglong uVar3;
  
  lVar1 = *(longlong *)(param_2 + 0x658);
  uVar3 = 0;
  if (*(uint *)(lVar1 + 4) != 0) {
    do {
      if ((*(int *)(lVar1 + 8 + uVar3 * 0x10) == -1) &&
         (*(int *)(lVar1 + 0xc + uVar3 * 0x10) == param_1)) {
        return lVar1 + 8 + uVar3 * 0x10;
      }
      uVar2 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar2;
    } while (uVar2 < *(uint *)(lVar1 + 4));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015a70
   NAME : FUN_180015a70
   SIG  : undefined4 __fastcall FUN_180015a70(int param_1, int param_2, longlong param_3, int param_4, uint * param_5, uint * param_6)
   ======================================================================== */

undefined4
FUN_180015a70(int param_1,int param_2,longlong param_3,int param_4,uint *param_5,uint *param_6)

{
  uint uVar1;
  longlong lVar2;
  uint *puVar3;
  ulonglong uVar4;
  longlong lVar5;
  int iVar6;
  longlong lVar7;
  code *pcVar8;
  
  pcVar8 = FUN_180015bc0;
  puVar3 = (uint *)0x0;
  iVar6 = 999;
  lVar5 = DAT_1800249b0;
  lVar7 = DAT_1800249b8;
  if (param_2 != 0) {
    pcVar8 = FUN_180015c20;
  }
  do {
    if (puVar3 == (uint *)0x0) {
      if (param_2 == 0) {
        puVar3 = (uint *)FUN_1800159d0(param_1,param_3);
      }
      else {
        puVar3 = (uint *)FUN_180015a20(param_1,param_3);
      }
    }
    else {
      puVar3 = (uint *)(*pcVar8)(puVar3,param_3,0xffffffff);
      lVar5 = DAT_1800249b0;
      lVar7 = DAT_1800249b8;
    }
    if (puVar3 == (uint *)0x0) {
      return 0xffffffff;
    }
    if (param_2 == 0) {
      uVar1 = *puVar3;
    }
    else {
      uVar1 = puVar3[2];
    }
    if (uVar1 == 0xffffffff) {
      lVar5 = 0xc;
      if (param_2 == 0) {
        lVar5 = 4;
      }
      return *(undefined4 *)(lVar5 + (longlong)puVar3);
    }
    lVar2 = *(longlong *)(param_3 + 0x660);
    if ((((*(int *)(lVar2 + 4) != 0) && (param_2 == 0)) && (-1 < param_4)) &&
       ((uVar4 = (ulonglong)*puVar3 << 4 | 8, *(longlong *)(uVar4 + lVar2) == lVar5 &&
        (*(longlong *)(uVar4 + 8 + lVar2) == lVar7)))) {
      puVar3 = (uint *)(*pcVar8)(puVar3,param_3,param_4);
      if (puVar3 == (uint *)0x0) {
        return 0xffffffff;
      }
      if (param_5 != (uint *)0x0) {
        *param_5 = puVar3[3];
      }
      lVar7 = DAT_1800249b8;
      lVar5 = DAT_1800249b0;
      if (param_6 != (uint *)0x0) {
        *param_6 = puVar3[2];
      }
    }
    iVar6 = iVar6 + -1;
  } while (iVar6 != 0);
  return 0xffffffff;
}



/* ========================================================================
   ENTRY: 180015bc0
   NAME : FUN_180015bc0
   SIG  : int * __fastcall FUN_180015bc0(int * param_1, longlong param_2, int param_3)
   ======================================================================== */

int * FUN_180015bc0(int *param_1,longlong param_2,int param_3)

{
  longlong lVar1;
  int *piVar2;
  uint uVar3;
  ulonglong uVar4;
  int iVar5;
  
  lVar1 = *(longlong *)(param_2 + 0x658);
  iVar5 = 0;
  uVar4 = 0;
  if (*(uint *)(lVar1 + 4) != 0) {
    do {
      piVar2 = (int *)(lVar1 + 8 + uVar4 * 0x10);
      if ((piVar2 != param_1) && (*(int *)(lVar1 + 0x10 + uVar4 * 0x10) == *param_1)) {
        if ((param_3 < 0) || (param_3 <= iVar5)) {
          return piVar2;
        }
        iVar5 = iVar5 + 1;
      }
      uVar3 = (int)uVar4 + 1;
      uVar4 = (ulonglong)uVar3;
    } while (uVar3 < *(uint *)(lVar1 + 4));
  }
  return (int *)0x0;
}



/* ========================================================================
   ENTRY: 180015c20
   NAME : FUN_180015c20
   SIG  : longlong __fastcall FUN_180015c20(longlong param_1, longlong param_2)
   ======================================================================== */

longlong FUN_180015c20(longlong param_1,longlong param_2)

{
  longlong lVar1;
  uint uVar2;
  ulonglong uVar3;
  longlong lVar4;
  
  lVar1 = *(longlong *)(param_2 + 0x658);
  if (*(uint *)(lVar1 + 4) != 0) {
    uVar3 = 0;
    do {
      lVar4 = lVar1 + 8 + uVar3 * 0x10;
      if ((lVar4 != param_1) && (*(int *)(lVar1 + 8 + uVar3 * 0x10) == *(int *)(param_1 + 8))) {
        return lVar4;
      }
      uVar2 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar2;
    } while (uVar2 < *(uint *)(lVar1 + 4));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015c80
   NAME : timeGetTime
   SIG  : DWORD __stdcall timeGetTime(void)
   ======================================================================== */

DWORD __stdcall timeGetTime(void)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180015c80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = timeGetTime();
  return DVar1;
}



/* ========================================================================
   ENTRY: 180015c90
   NAME : FUN_180015c90
   SIG  : undefined8 __fastcall FUN_180015c90(short * param_1, int param_2, longlong param_3, ulonglong param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_180015c90(short *param_1,int param_2,longlong param_3,ulonglong param_4)

{
  int iVar1;
  void *pvVar2;
  ushort uVar3;
  undefined *puVar4;
  undefined1 auStackY_f8 [32];
  ushort local_c8 [72];
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_f8;
  uVar3 = *param_1 + 0xe620;
  if (param_2 == 0) {
    if (uVar3 < 0x201) goto LAB_180015dec;
    if (uVar3 < 0x300) {
      uVar3 = 0x603;
      goto LAB_180015d03;
    }
  }
  else {
    if ((ushort)(*param_1 + 0xe31fU) < 0xff) {
      uVar3 = 0x603;
    }
    if (uVar3 < 0x201) goto LAB_180015dec;
  }
  if (uVar3 < 0x713) {
LAB_180015d03:
    local_c8[1] = 0;
    local_c8[2] = 0;
    local_c8[3] = 0;
    local_c8[4] = 0;
    local_c8[5] = 0;
    local_c8[6] = 0;
    local_c8[7] = 0;
    local_c8[8] = 0;
    local_c8[9] = 0;
    local_c8[10] = 0;
    local_c8[0xb] = 0;
    local_c8[0xc] = 0;
    local_c8[0xd] = 0;
    local_c8[0xe] = 0;
    local_c8[0xf] = 0;
    local_c8[0x10] = 0;
    local_c8[0x11] = 0;
    local_c8[0x12] = 0;
    local_c8[0x13] = 0;
    local_c8[0x14] = 0;
    local_c8[0x15] = 0;
    local_c8[0x16] = 0;
    local_c8[0x17] = 0;
    local_c8[0x18] = 0;
    local_c8[0x19] = 0;
    local_c8[0x1a] = 0;
    local_c8[0x1b] = 0;
    local_c8[0x1c] = 0;
    local_c8[0x1d] = 0;
    local_c8[0x1e] = 0;
    local_c8[0x1f] = 0;
    local_c8[0x20] = 0;
    local_c8[0x21] = 0;
    local_c8[0x22] = 0;
    local_c8[0x23] = 0;
    local_c8[0x24] = 0;
    local_c8[0x25] = 0;
    local_c8[0x26] = 0;
    local_c8[0x27] = 0;
    local_c8[0x28] = 0;
    local_c8[0x29] = 0;
    local_c8[0x2a] = 0;
    local_c8[0x2b] = 0;
    local_c8[0x2c] = 0;
    local_c8[0x2d] = 0;
    local_c8[0x2e] = 0;
    local_c8[0x2f] = 0;
    local_c8[0x30] = 0;
    local_c8[0x31] = 0;
    local_c8[0x32] = 0;
    local_c8[0x33] = 0;
    local_c8[0x34] = 0;
    local_c8[0x35] = 0;
    local_c8[0x36] = 0;
    local_c8[0x37] = 0;
    local_c8[0x38] = 0;
    local_c8[0x39] = 0;
    local_c8[0x3a] = 0;
    local_c8[0x3b] = 0;
    local_c8[0x3c] = 0;
    local_c8[0x3d] = 0;
    local_c8[0x3e] = 0;
    local_c8[0x3f] = 0;
    local_c8[0x40] = 0;
    local_c8[0] = uVar3;
    pvVar2 = bsearch(local_c8,&DAT_1800249c0,0x12,0x82,(_PtFuncCompare *)&LAB_1800183f0);
    if (pvVar2 == (void *)0x0) {
      return 0xffffd8f1;
    }
    if (((param_3 != 0) && ((int)param_4 != 0)) &&
       (iVar1 = snprintf(param_3,param_4 & 0xffffffff,&DAT_1800253ec,(longlong)pvVar2 + 2),
       (ushort)(uVar3 - 0x601) < 0xff)) {
      puVar4 = &DAT_180025400;
      if (param_2 != 0) {
        puVar4 = &DAT_1800253f4;
      }
      snprintf(param_3 + (longlong)iVar1 * 2,(ulonglong)(uint)((int)param_4 - iVar1),&DAT_180025408,
               puVar4);
    }
    return 0;
  }
LAB_180015dec:
  FUN_180018400(0xffffd8f1,"GetNameFromCategory: usbTerminalGUID = %04X ",(ulonglong)uVar3,param_4);
  return 0xffffd8f1;
}



/* ========================================================================
   ENTRY: 180015e30
   NAME : FUN_180015e30
   SIG  : uint __fastcall FUN_180015e30(ushort * param_1, int param_2)
   ======================================================================== */

uint FUN_180015e30(ushort *param_1,int param_2)

{
  ushort uVar1;
  uint uVar2;
  int iVar3;
  
  iVar3 = -0x7ee014f5;
  if (param_2 != 0) {
    iVar3 = -0x7ee36229;
  }
  uVar2 = 0;
  uVar1 = *param_1;
  while (uVar1 != 0) {
    uVar1 = *param_1;
    param_1 = param_1 + 1;
    uVar2 = uVar2 * iVar3 ^ (uint)uVar1;
    uVar1 = *param_1;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180015e70
   NAME : FUN_180015e70
   SIG  : int __fastcall FUN_180015e70(undefined8 * param_1, ushort * param_2, int param_3)
   ======================================================================== */

int FUN_180015e70(undefined8 *param_1,ushort *param_2,int param_3)

{
  int *piVar1;
  uint uVar2;
  
  uVar2 = FUN_180015e30(param_2,param_3);
  piVar1 = (int *)*param_1;
  while( true ) {
    if (piVar1 == (int *)0x0) {
      return 0;
    }
    if (piVar1[2] == uVar2) break;
    piVar1 = *(int **)(piVar1 + 4);
  }
  if ((uint)piVar1[1] < 2) {
    return 0;
  }
  *piVar1 = *piVar1 + 1;
  return *piVar1;
}



/* ========================================================================
   ENTRY: 180015ed0
   NAME : FUN_180015ed0
   SIG  : int __fastcall FUN_180015ed0(short * param_1)
   ======================================================================== */

int FUN_180015ed0(short *param_1)

{
  if (*param_1 == 1) {
    return 0x12;
  }
  return (ushort)param_1[8] + 0x12;
}



/* ========================================================================
   ENTRY: 180015ef0
   NAME : FUN_180015ef0
   SIG  : undefined8 __fastcall FUN_180015ef0(longlong * param_1, longlong param_2)
   ======================================================================== */

undefined8 FUN_180015ef0(longlong *param_1,longlong param_2)

{
  longlong lVar1;
  uint uVar2;
  undefined4 *puVar3;
  undefined8 uVar4;
  ushort *puVar5;
  ulonglong uVar6;
  int iVar7;
  
  puVar3 = FUN_1800011b0();
  param_1[1] = (longlong)puVar3;
  if (puVar3 == (undefined4 *)0x0) {
    return 0xffffd8f8;
  }
  iVar7 = 0;
  if (0 < *(int *)(param_2 + 0x430)) {
    do {
      lVar1 = *(longlong *)(*(longlong *)(param_2 + 0x438) + (longlong)iVar7 * 8);
      if (lVar1 != 0) {
        uVar6 = 0;
        while( true ) {
          uVar2 = *(uint *)(lVar1 + 0x10);
          if (*(uint *)(lVar1 + 0x10) == 0) {
            uVar2 = 1;
          }
          if (uVar2 <= (uint)uVar6) break;
          puVar5 = (ushort *)(lVar1 + 0x14);
          if (*(longlong *)(lVar1 + 8) != 0) {
            puVar5 = *(ushort **)(*(longlong *)(lVar1 + 8) + uVar6 * 8);
          }
          uVar4 = FUN_1800151b0(param_1,puVar5,(uint)(*(int *)(lVar1 + 0x268) == 2));
          if ((int)uVar4 != 0) {
            return uVar4;
          }
          uVar6 = (ulonglong)((uint)uVar6 + 1);
        }
      }
      iVar7 = iVar7 + 1;
    } while (iVar7 < *(int *)(param_2 + 0x430));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015fe0
   NAME : FUN_180015fe0
   SIG  : bool __fastcall FUN_180015fe0(longlong param_1, int param_2)
   ======================================================================== */

bool FUN_180015fe0(longlong param_1,int param_2)

{
  bool bVar1;
  
  bVar1 = false;
  if (*(int *)(param_1 + 0x44) <= param_2) {
    bVar1 = param_2 <= *(int *)(param_1 + 0x48);
  }
  return bVar1;
}



/* ========================================================================
   ENTRY: 180015ff0
   NAME : FUN_180015ff0
   SIG  : bool __fastcall FUN_180015ff0(void)
   ======================================================================== */

bool FUN_180015ff0(void)

{
  int iVar1;
  
  iVar1 = FUN_180020730();
  return iVar1 < 6;
}



/* ========================================================================
   ENTRY: 180016010
   NAME : FUN_180016010
   SIG  : ulonglong __fastcall FUN_180016010(ulonglong param_1, int * param_2, int * param_3, ulonglong param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_180016010(ulonglong param_1,int *param_2,int *param_3,ulonglong param_4)

{
  longlong lVar1;
  longlong lVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  char *pcVar6;
  uint uVar7;
  int iVar8;
  uint uVar9;
  int iVar10;
  short sVar11;
  undefined4 in_XMM3_Da;
  undefined4 in_XMM3_Db;
  undefined1 auStackY_c8 [32];
  ushort local_90;
  ushort local_8e;
  uint local_8c;
  short local_7e;
  ulonglong local_68;
  
  local_68 = DAT_18002b580 ^ (ulonglong)auStackY_c8;
  uVar9 = 2;
  if (param_2 == (int *)0x0) {
    uVar4 = 0;
    iVar10 = 0;
LAB_1800161d7:
    sVar11 = 0;
    if (param_3 == (int *)0x0) {
      iVar8 = 0;
LAB_18001637b:
      if (iVar10 != 0) {
        return uVar4;
      }
      if (iVar8 != 0) {
        return uVar4;
      }
      pcVar6 = "No input or output channels defined";
    }
    else {
      if ((param_3[2] & 0x10000U) == 0) {
        if (*param_3 != -2) {
          iVar8 = param_3[1];
          lVar1 = *(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8);
          if (*(int *)(lVar1 + 0x18) < iVar8) {
            FUN_180018400(0xffffd8f2,"Invalid output channel count",param_1,param_4);
            return 0xffffd8f2;
          }
          uVar5 = param_1;
          if (*(longlong *)(param_3 + 6) == 0) {
            lVar2 = *(longlong *)(lVar1 + 0x150);
            lVar1 = *(longlong *)
                     (*(longlong *)(lVar2 + 0x438) + (ulonglong)*(uint *)(lVar1 + 0x158) * 8);
            do {
              if ((uVar9 & *(uint *)(lVar1 + 0x27c)) != 0) {
                if (uVar9 != 0) {
                  if ((*(int *)(lVar2 + 0x418) == 2) && (uVar9 == 8)) {
                    uVar9 = 4;
                    sVar11 = 0x18;
                  }
                  uVar3 = FUN_180020750(iVar8);
                  uVar4 = FUN_1800208c0(uVar9);
                  param_4 = uVar4 & 0xffffffff;
                  param_1 = (ulonglong)uVar9;
                  FUN_180020830(&local_90,iVar8,uVar9,(uint)uVar4,
                                (double)CONCAT44(in_XMM3_Db,in_XMM3_Da),(int)uVar3);
                  if (sVar11 != 0) {
                    local_7e = sVar11;
                  }
                  uVar5 = FUN_180018da0(lVar1,&local_90);
                  uVar4 = uVar5 & 0xffffffff;
                  if ((int)uVar5 != 0) {
                    uVar4 = FUN_1800208c0(uVar9);
                    param_4 = uVar4 & 0xffffffff;
                    param_1 = (ulonglong)uVar9;
                    FUN_1800207c0(&local_90,iVar8,uVar9,(short)param_4,
                                  (double)CONCAT44(in_XMM3_Db,in_XMM3_Da));
                    if (sVar11 != 0) {
                      local_7e = sVar11;
                    }
                    uVar5 = FUN_180018da0(lVar1,&local_90);
                    uVar4 = uVar5 & 0xffffffff;
                    if ((int)uVar5 != 0) {
                      FUN_180018400((int)uVar5,"IsFormatSupported(render) failed: %u,%u,%u",
                                    (ulonglong)local_8c,(ulonglong)local_8e);
                      return uVar4;
                    }
                  }
                  goto LAB_18001637b;
                }
                break;
              }
              uVar9 = uVar9 * 2;
            } while (uVar9 < 0x41);
            FUN_180018400((int)uVar4,"IsFormatSupported(render) failed: no testformat found!",lVar2,
                          param_4);
            return 0xffffd8f1;
          }
          goto LAB_18001624f;
        }
        goto LAB_1800161fe;
      }
      pcVar6 = "IsFormatSupported: Custom output format not supported";
    }
  }
  else {
    if ((param_2[2] & 0x10000U) == 0) {
      if (*param_2 != -2) {
        iVar10 = param_2[1];
        uVar5 = *(ulonglong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_2 * 8);
        if (*(int *)(uVar5 + 0x14) < iVar10) {
          FUN_180018400(0xffffd8f2,"IsFormatSupported: Invalid input channel count",uVar5,param_4);
          return 0xffffd8f2;
        }
        if (*(longlong *)(param_2 + 6) == 0) {
          uVar7 = 2;
          lVar1 = *(longlong *)
                   (*(longlong *)(*(longlong *)(uVar5 + 0x150) + 0x438) +
                   (ulonglong)*(uint *)(uVar5 + 0x158) * 8);
          do {
            if ((uVar7 & *(uint *)(lVar1 + 0x27c)) != 0) {
              if (uVar7 != 0) {
                sVar11 = 0;
                if ((*(int *)(*(longlong *)(uVar5 + 0x150) + 0x418) == 2) && (uVar7 == 8)) {
                  uVar7 = 4;
                  sVar11 = 0x18;
                }
                uVar3 = FUN_180020750(iVar10);
                uVar4 = FUN_1800208c0(uVar7);
                param_4 = uVar4 & 0xffffffff;
                FUN_180020830(&local_90,iVar10,uVar7,(uint)uVar4,
                              (double)CONCAT44(in_XMM3_Db,in_XMM3_Da),(int)uVar3);
                if (sVar11 != 0) {
                  local_7e = sVar11;
                }
                uVar5 = FUN_180018da0(lVar1,&local_90);
                uVar4 = uVar5 & 0xffffffff;
                if ((int)uVar5 != 0) {
                  uVar4 = FUN_1800208c0(uVar7);
                  param_4 = uVar4 & 0xffffffff;
                  FUN_1800207c0(&local_90,iVar10,uVar7,(short)param_4,
                                (double)CONCAT44(in_XMM3_Db,in_XMM3_Da));
                  if (sVar11 != 0) {
                    local_7e = sVar11;
                  }
                  uVar5 = FUN_180018da0(lVar1,&local_90);
                  uVar4 = uVar5 & 0xffffffff;
                  if ((int)uVar5 != 0) {
                    FUN_180018400((int)uVar5,
                                  "IsFormatSupported(capture) failed: sr=%u,ch=%u,bits=%u",
                                  (ulonglong)local_8c,(ulonglong)local_8e);
                    return uVar4;
                  }
                }
                goto LAB_1800161d7;
              }
              break;
            }
            uVar7 = uVar7 * 2;
          } while (uVar7 < 0x41);
          FUN_180018400(0,"IsFormatSupported(capture) failed: no testformat found!",uVar5,param_4);
          return 0xffffd8f1;
        }
LAB_18001624f:
        FUN_180018400(0xffffd900,"Host API stream info not supported",uVar5,param_4);
        return 0xffffd900;
      }
LAB_1800161fe:
      FUN_180018400(0xffffd8f4,
                    "IsFormatSupported: paUseHostApiSpecificDeviceSpecification not supported",
                    param_1,param_4);
      return 0xffffd8f4;
    }
    pcVar6 = "IsFormatSupported: Custom input format not supported";
  }
  FUN_180018400(0xffffd8f6,pcVar6,param_1,param_4);
  return 0xffffd8f6;
}



/* ========================================================================
   ENTRY: 1800163c0
   NAME : FUN_1800163c0
   SIG  : bool __fastcall FUN_1800163c0(longlong param_1, int param_2)
   ======================================================================== */

bool FUN_1800163c0(longlong param_1,int param_2)

{
  bool bVar1;
  
  bVar1 = false;
  if (*(int *)(param_1 + 0x4c) <= param_2) {
    bVar1 = param_2 <= *(int *)(param_1 + 0x50);
  }
  return bVar1;
}



/* ========================================================================
   ENTRY: 1800163d0
   NAME : FUN_1800163d0
   SIG  : bool __fastcall FUN_1800163d0(wchar_t * param_1)
   ======================================================================== */

bool FUN_1800163d0(wchar_t *param_1)

{
  int iVar1;
  
  iVar1 = _wcsnicmp(param_1,L"USB Audio",10);
  return iVar1 == 0;
}



/* ========================================================================
   ENTRY: 180016420
   NAME : FUN_180016420
   SIG  : bool __fastcall FUN_180016420(wchar_t * param_1)
   ======================================================================== */

bool FUN_180016420(wchar_t *param_1)

{
  int iVar1;
  
  iVar1 = _wcsnicmp(param_1,L"\\\\?\\USB",8);
  return iVar1 == 0;
}



/* ========================================================================
   ENTRY: 180016450
   NAME : FUN_180016450
   SIG  : ulonglong __fastcall FUN_180016450(longlong param_1, undefined8 * param_2, int * param_3, uint * param_4, double param_5, uint param_6, uint param_7, short * param_8, uint * param_9)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_180016450(longlong param_1,undefined8 *param_2,int *param_3,uint *param_4,
                       double param_5,uint param_6,uint param_7,short *param_8,uint *param_9)

{
  int iVar1;
  longlong lVar2;
  double dVar3;
  uint uVar4;
  uint extraout_EAX;
  int iVar5;
  int iVar6;
  ulonglong uVar7;
  undefined4 *puVar8;
  undefined4 *puVar9;
  undefined8 uVar10;
  undefined8 *puVar11;
  HANDLE pvVar12;
  longlong lVar13;
  undefined4 *puVar14;
  char *pcVar15;
  uint uVar16;
  uint uVar17;
  short sVar18;
  uint *puVar19;
  uint *puVar20;
  short sVar21;
  uint uVar22;
  longlong lVar23;
  undefined1 auStackY_148 [32];
  uint local_d8;
  uint local_d4;
  uint local_d0;
  uint local_cc;
  uint local_c8;
  undefined4 local_c4;
  longlong local_c0;
  uint *local_b8;
  int *local_b0;
  longlong local_a8;
  uint *local_a0;
  short *local_98;
  undefined8 *local_90;
  short local_88;
  ushort local_86;
  uint local_84;
  ushort local_7c;
  short local_76;
  ulonglong local_60;
  
  local_60 = DAT_18002b580 ^ (ulonglong)auStackY_148;
  local_d8 = 0x10;
  local_98 = param_8;
  local_a0 = param_9;
  local_d0 = 0x10;
  puVar20 = param_4;
  local_c0 = param_1;
  local_b8 = param_4;
  local_b0 = param_3;
  local_90 = param_2;
  if (param_3 == (int *)0x0) {
    iVar5 = 0;
    puVar19 = (uint *)0x0;
  }
  else {
    if (*param_3 == -2) {
      FUN_180018400(0xffffd8f4,"paUseHostApiSpecificDeviceSpecification(in) not supported",param_3,
                    param_4);
      return 0xffffd8f4;
    }
    iVar5 = param_3[1];
    if (*(int *)(*(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)*param_3 * 8) + 0x14) <
        iVar5) {
      FUN_180018400(0xffffd8f2,"Invalid input channel count",param_3,param_4);
      return 0xffffd8f2;
    }
    puVar19 = (uint *)0x1;
    uVar7 = FUN_18001b520(param_3,*(int **)(param_3 + 6),1);
    if ((int)uVar7 != 0) {
      FUN_180018400((int)uVar7,"Host API stream info not supported (in)",puVar19,puVar20);
      return uVar7 & 0xffffffff;
    }
    local_d8 = param_3[2];
  }
  if (param_4 == (uint *)0x0) {
    local_d4 = 0;
  }
  else {
    uVar16 = *puVar20;
    if (uVar16 == 0xfffffffe) {
      FUN_180018400(0xffffd8f4,"paUseHostApiSpecificDeviceSpecification(out) not supported",puVar19,
                    puVar20);
      return 0xffffd8f4;
    }
    local_d4 = puVar20[1];
    puVar20 = (uint *)(ulonglong)local_d4;
    if (*(int *)(*(longlong *)(*(longlong *)(local_c0 + 0x28) + (longlong)(int)uVar16 * 8) + 0x18) <
        (int)local_d4) {
      FUN_180018400(0xffffd8f2,"Invalid output channel count",puVar19,puVar20);
      return 0xffffd8f2;
    }
    puVar19 = (uint *)0x0;
    uVar7 = FUN_18001b520(param_4,*(int **)(param_4 + 6),0);
    if ((int)uVar7 != 0) {
      FUN_180018400((int)uVar7,"Host API stream info not supported (out)",puVar19,puVar20);
      return uVar7 & 0xffffffff;
    }
    local_d0 = param_4[2];
  }
  if ((param_7 & 0xffff0000) != 0) {
    FUN_180018400(0xffffd8f5,"Invalid flag supplied",puVar19,puVar20);
    return 0xffffd8f5;
  }
  puVar8 = (undefined4 *)FUN_180020230(0x2a0);
  if (puVar8 == (undefined4 *)0x0) {
LAB_18001793a:
    uVar4 = 0xffffd8f8;
  }
  else {
    puVar9 = FUN_1800011b0();
    lVar13 = local_c0;
    *(undefined4 **)(puVar8 + 0x68) = puVar9;
    if (puVar9 == (undefined4 *)0x0) goto LAB_18001793a;
    if (param_8 == (short *)0x0) {
      pcVar15 = "Blocking API not supported yet";
      goto LAB_180016f7a;
    }
    FUN_180006e70(puVar8,local_c0 + 0x48,param_8,param_9);
    FUN_180003950((double *)(puVar8 + 0x20),param_5);
    sVar21 = 0x18;
    if (iVar5 < 1) {
      local_c8 = 0;
      *(undefined8 *)(puVar8 + 0x6a) = 0;
      puVar8[0x70] = 0;
LAB_18001695a:
      uVar16 = local_d4;
      if ((int)local_d4 < 1) {
        uVar16 = 0;
        *(undefined8 *)(puVar8 + 0x7a) = 0;
        puVar8[0x80] = 0;
LAB_180016ba2:
        uVar17 = local_d8;
        dVar3 = DAT_180025c40;
        if (local_b0 != (int *)0x0) {
          uVar4 = (uint)(longlong)(param_5 * *(double *)(local_b0 + 4) + DAT_180025c40);
          puVar8[0x6f] = uVar4;
          uVar22 = (uint)(longlong)param_5;
          if (uVar22 < uVar4) {
            puVar8[0x6f] = uVar22;
            uVar4 = uVar22;
          }
          else {
            uVar22 = *(uint *)(*(longlong *)(puVar8 + 0x6a) + 0x274);
            if (uVar4 < uVar22) {
              puVar8[0x6f] = uVar22;
              uVar4 = uVar22;
            }
          }
          if (uVar4 == 0) {
            puVar8[0x6f] = 1;
          }
          puVar8[0x72] = 2;
          if (((*(longlong *)(local_b0 + 6) != 0) &&
              (*(int *)(*(longlong *)(*(longlong *)(puVar8 + 0x6a) + 0x220) + 0x418) == 1)) &&
             (iVar5 = *(int *)(*(longlong *)(local_b0 + 6) + 0x10), iVar5 != 0)) {
            puVar8[0x72] = iVar5;
          }
        }
        if (local_b8 != (uint *)0x0) {
          uVar4 = (uint)(longlong)(param_5 * *(double *)(local_b8 + 4) + dVar3);
          puVar8[0x7f] = uVar4;
          uVar22 = (uint)(longlong)param_5;
          if (uVar22 < uVar4) {
            puVar8[0x7f] = uVar22;
            uVar4 = uVar22;
          }
          else {
            uVar22 = *(uint *)(*(longlong *)(puVar8 + 0x7a) + 0x274);
            if (uVar4 < uVar22) {
              puVar8[0x7f] = uVar22;
              uVar4 = uVar22;
            }
          }
          if (uVar4 == 0) {
            puVar8[0x7f] = 1;
          }
          puVar8[0x82] = 2;
          if (((*(longlong *)(local_b8 + 6) != 0) &&
              (*(int *)(*(longlong *)(*(longlong *)(puVar8 + 0x7a) + 0x220) + 0x418) == 1)) &&
             (iVar5 = *(int *)(*(longlong *)(local_b8 + 6) + 0x10), iVar5 != 0)) {
            puVar8[0x82] = iVar5;
          }
        }
        lVar23 = 0x1bc;
        lVar13 = 0x1bc;
        if ((uint)puVar8[0x6f] <= (uint)puVar8[0x7f]) {
          lVar13 = 0x1fc;
        }
        puVar9 = (undefined4 *)(ulonglong)local_c8;
        FUN_180006290(puVar8 + 0x26,puVar8[0xa4],local_d8,local_c8,puVar8[0xa6],local_d0,uVar16,
                      param_5,param_7,param_6,*(uint *)(lVar13 + (longlong)puVar8),1,
                      (longlong)local_98,local_a0);
        uVar4 = extraout_EAX;
        if (extraout_EAX == 0) {
          if ((int)puVar8[0xa4] < 1) {
            *(undefined8 *)(puVar8 + 0x6c) = 0;
LAB_180017012:
            if ((int)puVar8[0xa6] < 1) {
              *(undefined8 *)(puVar8 + 0x7c) = 0;
LAB_18001723c:
              *(double *)(puVar8 + 0x12) = param_5;
              pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
              *(HANDLE *)(puVar8 + 0x92) = pvVar12;
              if (pvVar12 != (HANDLE)0x0) {
                pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
                *(HANDLE *)(puVar8 + 0x94) = pvVar12;
                if (pvVar12 != (HANDLE)0x0) {
                  puVar9 = (undefined4 *)0x0;
                  pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
                  *(HANDLE *)(puVar8 + 0x96) = pvVar12;
                  if (pvVar12 != (HANDLE)0x0) {
                    if ((int)puVar8[0xa4] < 1) {
LAB_180017554:
                      if ((int)puVar8[0xa6] < 1) goto LAB_1800177cc;
                      iVar5 = puVar8[0x80];
                      iVar6 = puVar8[0x7f];
                      lVar13 = FUN_1800012e0(*(int **)(puVar8 + 0x68),puVar8[0x82] << 3);
                      *(longlong *)(puVar8 + 0x84) = lVar13;
                      if (lVar13 != 0) {
                        puVar14 = (undefined4 *)
                                  FUN_1800012e0(*(int **)(puVar8 + 0x68),puVar8[0x82] * 0x58);
                        *(undefined4 **)(puVar8 + 0x86) = puVar14;
                        if (puVar14 != (undefined4 *)0x0) {
                          iVar1 = *(int *)(*(longlong *)(*(longlong *)(puVar8 + 0x7a) + 0x220) +
                                          0x418);
                          if (iVar1 == 1) {
                            iVar5 = iVar5 * iVar6;
                            uVar7 = 0;
                            if (puVar8[0x82] != 0) {
                              do {
                                lVar13 = *(longlong *)(puVar8 + 0x86);
                                iVar6 = (int)uVar7;
                                pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
                                lVar23 = uVar7 * 0x58;
                                *(HANDLE *)(*(longlong *)(puVar8 + 0x84) + uVar7 * 8) = pvVar12;
                                *(undefined8 *)(lVar13 + 0x50 + lVar23) =
                                     *(undefined8 *)(*(longlong *)(puVar8 + 0x84) + uVar7 * 8);
                                uVar16 = iVar6 + 1;
                                uVar7 = (ulonglong)uVar16;
                                *(ulonglong *)(lVar13 + 0x28 + lVar23) =
                                     (ulonglong)(uint)(iVar6 * iVar5) + *(longlong *)(puVar8 + 0x7c)
                                ;
                                *(int *)(lVar13 + 0x20 + lVar23) = iVar5;
                                *(int *)(lVar13 + 0x24 + lVar23) = iVar5;
                                *(undefined4 *)(lVar13 + lVar23) = 0x38;
                                *(undefined4 *)(lVar13 + 0x10 + lVar23) = 1;
                                *(undefined4 *)(lVar13 + 0x14 + lVar23) = 1;
                              } while (uVar16 < (uint)puVar8[0x82]);
                            }
                            goto LAB_1800177cc;
                          }
                          if (iVar1 != 2) {
                            uVar7 = (ulonglong)
                                    *(uint *)(*(longlong *)(*(longlong *)(puVar8 + 0x6a) + 0x220) +
                                             0x418);
                            goto LAB_180016e2a;
                          }
                          puVar11 = *(undefined8 **)(puVar8 + 0x84);
                          puVar20 = (uint *)0x0;
                          puVar19 = (uint *)0x0;
                          pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
                          *puVar11 = pvVar12;
                          *(undefined8 *)(puVar14 + 10) = *(undefined8 *)(puVar8 + 0x7c);
                          puVar14[8] = puVar8[0x80] * puVar8[0x7f];
                          iVar5 = puVar8[0x80];
                          iVar6 = puVar8[0x7f];
                          *puVar14 = 0x38;
                          puVar14[4] = 1;
                          puVar14[5] = 1;
                          puVar14[9] = iVar5 * iVar6;
                          *(ulonglong *)(puVar14 + 0x20) =
                               (ulonglong)(uint)(puVar8[0x80] * puVar8[0x7f]) +
                               *(longlong *)(puVar8 + 0x7c);
                          puVar14[0x1e] = puVar8[0x80] * puVar8[0x7f];
                          iVar5 = puVar8[0x80];
                          iVar6 = puVar8[0x7f];
                          puVar14[0x16] = 0x38;
                          puVar14[0x1a] = 1;
                          puVar14[0x1b] = 1;
                          puVar14[0x1f] = iVar5 * iVar6;
                          if ((*(int *)(*(undefined8 **)(puVar8 + 0x7a) + 0x45) != 1) ||
                             (iVar5 = FUN_180019e90(*(undefined8 **)(puVar8 + 0x7a),
                                                    **(undefined8 **)(puVar8 + 0x84)), iVar5 == 0))
                          {
                            iVar5 = FUN_180019ef0(*(undefined8 **)(puVar8 + 0x7a));
                            if (iVar5 == 0) {
                              *(undefined1 **)(*(longlong *)(puVar8 + 0x7a) + 0x2a0) =
                                   &LAB_180018840;
                              uVar4 = 0;
                            }
                            else {
                              local_d8 = 0xdeadc0de;
                              *(code **)(*(longlong *)(puVar8 + 0x7a) + 0x2a0) = FUN_180018850;
                              iVar5 = (**(code **)(*(longlong *)(puVar8 + 0x7a) + 0x2a0))
                                                (*(longlong *)(puVar8 + 0x7a),&local_d8);
                              if ((iVar5 != 0) || (uVar4 = 0, local_d8 != 0)) {
                                pcVar15 = "Failed to read render position register (IOCTL)";
                                goto LAB_180016f7a;
                              }
                            }
LAB_1800177cc:
                            *(undefined8 *)(puVar8 + 0x8a) = 0;
                            *(undefined8 *)(puVar8 + 0x8c) = 0;
                            puVar8[0x99] = param_7;
                            puVar8[0x8e] = 0x100;
                            if (*(longlong *)(puVar8 + 0x6a) != 0) {
                              FUN_1800154e0(*(longlong *)(*(longlong *)(puVar8 + 0x6a) + 0x220));
                            }
                            if (*(longlong *)(puVar8 + 0x7a) != 0) {
                              FUN_1800154e0(*(longlong *)(*(longlong *)(puVar8 + 0x7a) + 0x220));
                            }
                            if (puVar8[0xa4] == 0) {
                              puVar8[0x14] = 0xffffffff;
                            }
                            else {
                              iVar5 = *local_b0;
                              lVar13 = *(longlong *)
                                        (*(longlong *)(local_c0 + 0x28) + (longlong)iVar5 * 8);
                              iVar6 = Pa_HostApiTypeIdToHostApiIndex(0xb);
                              iVar5 = Pa_HostApiDeviceIndexToDeviceIndex(iVar6,iVar5);
                              puVar8[0x14] = iVar5;
                              puVar8[0x15] = puVar8[0xa5];
                              puVar8[0x18] = 0xffffffff;
                              lVar23 = *(longlong *)(*(longlong *)(puVar8 + 0x6a) + 8);
                              if (lVar23 != 0) {
                                puVar8[0x18] = *(undefined4 *)
                                                (*(longlong *)
                                                  (lVar23 + (longlong)*(int *)(lVar13 + 0x15c) * 8)
                                                + 0x20c);
                              }
                              puVar8[0x17] = *(undefined4 *)(lVar13 + 0x160);
                              puVar8[0x16] = puVar8[0x6f];
                              puVar8[0x19] = *(undefined4 *)(*(longlong *)(puVar8 + 0x6a) + 0x228);
                            }
                            if (puVar8[0xa6] == 0) {
                              puVar8[0x1a] = 0xffffffff;
                              puVar8[0xc] = 2;
                              *local_90 = puVar8;
                            }
                            else {
                              uVar16 = *local_b8;
                              iVar5 = Pa_HostApiTypeIdToHostApiIndex(0xb);
                              iVar5 = Pa_HostApiDeviceIndexToDeviceIndex(iVar5,uVar16);
                              puVar8[0x1a] = iVar5;
                              puVar8[0x1b] = puVar8[0xa7];
                              puVar8[0x1c] = puVar8[0x7f];
                              puVar8[0x1d] = *(undefined4 *)(*(longlong *)(puVar8 + 0x7a) + 0x230);
                              puVar8[0x1f] = *(undefined4 *)(*(longlong *)(puVar8 + 0x7a) + 0x228);
                              puVar8[0xc] = 2;
                              *local_90 = puVar8;
                            }
                            goto LAB_1800179a6;
                          }
                          pcVar15 = "Failed to register rendering notification handle";
LAB_180016f7a:
                          FUN_180018400(0xffffd8f1,pcVar15,puVar19,puVar20);
                          uVar4 = 0xffffd8f1;
                          goto LAB_180017940;
                        }
                      }
                    }
                    else {
                      uVar16 = puVar8[0x6f];
                      iVar5 = puVar8[0x70];
                      if (uVar16 <= (uint)puVar8[0x7f]) {
                        lVar23 = 0x1fc;
                      }
                      uVar17 = FUN_180008cd0(*(int *)(lVar23 + (longlong)puVar8) * 2 + 0x400);
                      lVar13 = FUN_1800012e0(*(int **)(puVar8 + 0x68),puVar8[0x72] << 3);
                      *(longlong *)(puVar8 + 0x74) = lVar13;
                      if (lVar13 != 0) {
                        puVar14 = (undefined4 *)
                                  FUN_1800012e0(*(int **)(puVar8 + 0x68),puVar8[0x72] * 0x58);
                        *(undefined4 **)(puVar8 + 0x76) = puVar14;
                        if (puVar14 != (undefined4 *)0x0) {
                          uVar16 = iVar5 * uVar16;
                          uVar22 = *(uint *)(*(longlong *)(*(longlong *)(puVar8 + 0x6a) + 0x220) +
                                            0x418);
                          uVar7 = (ulonglong)uVar22;
                          if (uVar22 == 1) {
                            uVar7 = 0;
                            if (puVar8[0x72] != 0) {
                              do {
                                lVar13 = *(longlong *)(puVar8 + 0x76);
                                iVar5 = (int)uVar7;
                                pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,1,0,(LPCSTR)0x0);
                                lVar23 = uVar7 * 0x58;
                                *(HANDLE *)(*(longlong *)(puVar8 + 0x74) + uVar7 * 8) = pvVar12;
                                *(undefined8 *)(lVar13 + 0x50 + lVar23) =
                                     *(undefined8 *)(*(longlong *)(puVar8 + 0x74) + uVar7 * 8);
                                uVar22 = iVar5 + 1;
                                uVar7 = (ulonglong)uVar22;
                                *(ulonglong *)(lVar13 + 0x28 + lVar23) =
                                     (ulonglong)(iVar5 * uVar16) + *(longlong *)(puVar8 + 0x6c);
                                *(uint *)(lVar13 + 0x20 + lVar23) = uVar16;
                                *(undefined4 *)(lVar13 + 0x24 + lVar23) = 0;
                                *(undefined4 *)(lVar13 + lVar23) = 0x38;
                                *(undefined4 *)(lVar13 + 0x10 + lVar23) = 1;
                                *(undefined4 *)(lVar13 + 0x14 + lVar23) = 1;
                              } while (uVar22 < (uint)puVar8[0x72]);
                            }
                            goto LAB_18001750f;
                          }
                          if (uVar22 != 2) goto LAB_180016e2a;
                          puVar11 = *(undefined8 **)(puVar8 + 0x74);
                          puVar20 = (uint *)0x0;
                          puVar19 = (uint *)0x0;
                          pvVar12 = CreateEventA((LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCSTR)0x0);
                          *puVar11 = pvVar12;
                          *(undefined8 *)(puVar14 + 10) = *(undefined8 *)(puVar8 + 0x6c);
                          puVar14[8] = uVar16;
                          puVar14[9] = 0;
                          *puVar14 = 0x38;
                          puVar14[4] = 1;
                          puVar14[5] = 1;
                          *(ulonglong *)(puVar14 + 0x20) =
                               (ulonglong)uVar16 + *(longlong *)(puVar8 + 0x6c);
                          puVar14[0x1e] = uVar16;
                          puVar14[0x1f] = 0;
                          puVar14[0x16] = 0x38;
                          puVar14[0x1a] = 1;
                          puVar14[0x1b] = 1;
                          if ((*(int *)(*(undefined8 **)(puVar8 + 0x6a) + 0x45) == 1) &&
                             (iVar5 = FUN_180019e90(*(undefined8 **)(puVar8 + 0x6a),
                                                    **(undefined8 **)(puVar8 + 0x74)), iVar5 != 0))
                          {
                            pcVar15 = "Failed to register capture notification handle";
                            goto LAB_180016f7a;
                          }
                          iVar5 = FUN_180019ef0(*(undefined8 **)(puVar8 + 0x6a));
                          if (iVar5 == 0) {
                            *(undefined1 **)(*(longlong *)(puVar8 + 0x6a) + 0x2a0) = &LAB_180018840;
                            uVar4 = 0;
                          }
                          else {
                            local_d8 = 0xdeadc0de;
                            *(code **)(*(longlong *)(puVar8 + 0x6a) + 0x2a0) = FUN_1800188e0;
                            iVar5 = (**(code **)(*(longlong *)(puVar8 + 0x6a) + 0x2a0))
                                              (*(longlong *)(puVar8 + 0x6a),&local_d8);
                            if ((iVar5 != 0) || (uVar4 = 0, local_d8 != 0)) {
                              pcVar15 = "Failed to read capture position register (IOCTL)";
                              goto LAB_180016f7a;
                            }
                          }
LAB_18001750f:
                          puVar9 = (undefined4 *)
                                   FUN_1800012e0(*(int **)(puVar8 + 0x68),uVar17 * puVar8[0x70]);
                          *(undefined4 **)(puVar8 + 0xa2) = puVar9;
                          if (puVar9 != (undefined4 *)0x0) {
                            FUN_180006c50(puVar8 + 0x9a,puVar8[0x70],uVar17,puVar9);
                            goto LAB_180017554;
                          }
                        }
                      }
                    }
                  }
                }
              }
              goto LAB_18001793a;
            }
            uVar16 = puVar8[0x7f];
            *(double *)(puVar8 + 0x10) = (double)uVar16 / param_5;
            uVar17 = *(int *)((*(undefined8 **)(puVar8 + 0x7a))[0x44] + 0x418) - 1;
            uVar7 = (ulonglong)uVar17;
            if (uVar17 == 0) {
              iVar5 = puVar8[0x82] * puVar8[0x80] * uVar16;
              lVar13 = FUN_1800012e0(*(int **)(puVar8 + 0x68),iVar5);
              *(longlong *)(puVar8 + 0x7c) = lVar13;
              if (lVar13 != 0) {
                puVar8[0x7e] = iVar5;
                *(undefined1 **)(*(longlong *)(puVar8 + 0x7a) + 0x2a8) = &LAB_1800180d0;
                *(code **)(*(longlong *)(puVar8 + 0x7a) + 0x2b0) = FUN_180018310;
                goto LAB_18001723c;
              }
              uVar4 = 0xffffd8f8;
              FUN_180018400(0xffffd8f8,"Failed to allocate output buffer",uVar7,puVar9);
            }
            else {
              if (uVar17 == 1) {
                puVar20 = &local_d4;
                puVar19 = &local_d8;
                local_d4 = 0;
                local_d0 = 0;
                uVar16 = uVar16 * puVar8[0x80] * 2;
                local_d8 = uVar16;
                uVar7 = FUN_180018970(*(undefined8 **)(puVar8 + 0x7a),(undefined8 *)(puVar8 + 0x7c),
                                      puVar19,puVar20);
                if ((int)uVar7 != 0) {
                  pcVar15 = "Failed to get output buffer (with notification)";
                  goto LAB_180016f7a;
                }
                if (local_d8 != uVar16) {
                  puVar8[0x7f] = local_d8 / (uint)(puVar8[0x80] * 2);
                }
                lVar13 = *(longlong *)(puVar8 + 0x7a);
                puVar8[0x7e] = local_d8;
                if (*(int *)(lVar13 + 0x228) == 2) {
                  *(code **)(lVar13 + 0x2a8) = FUN_1800181d0;
                }
                else {
                  *(code **)(lVar13 + 0x2a8) = FUN_180018100;
                }
                *(code **)(*(longlong *)(puVar8 + 0x7a) + 0x2b0) = FUN_180018380;
                *(code **)(*(longlong *)(puVar8 + 0x7a) + 0x298) = _guard_check_icall;
                uVar4 = FUN_180018bb0(*(undefined8 **)(puVar8 + 0x7a),&local_d0,&local_c4,&local_c4)
                ;
                if (uVar4 == 0) {
                  *(uint *)(*(longlong *)(puVar8 + 0x7a) + 0x290) = local_d0;
                  *(double *)(puVar8 + 0x10) =
                       (double)((ulonglong)local_d0 / (ulonglong)(uint)puVar8[0x80]) / param_5 +
                       *(double *)(puVar8 + 0x10);
                }
                else {
                  *(undefined4 *)(*(longlong *)(puVar8 + 0x7a) + 0x290) = 0;
                }
                goto LAB_18001723c;
              }
              uVar4 = 0xffffd8fe;
              FUN_180018400(0xffffd8fe,"Wave type %u ??",
                            (ulonglong)
                            *(uint *)(*(longlong *)(*(longlong *)(puVar8 + 0x6a) + 0x220) + 0x418),
                            puVar9);
            }
          }
          else {
            uVar16 = puVar8[0x6f];
            *(double *)(puVar8 + 0xe) = (double)uVar16 / param_5;
            uVar17 = *(uint *)((*(undefined8 **)(puVar8 + 0x6a))[0x44] + 0x418);
            uVar7 = (ulonglong)uVar17;
            if (uVar17 == 1) {
              iVar5 = puVar8[0x72] * puVar8[0x70] * uVar16;
              lVar13 = FUN_1800012e0(*(int **)(puVar8 + 0x68),iVar5);
              *(longlong *)(puVar8 + 0x6c) = lVar13;
              if (lVar13 == 0) {
                uVar4 = 0xffffd8f8;
                FUN_180018400(0xffffd8f8,"Failed to allocate input buffer",uVar7,puVar9);
                goto LAB_180017940;
              }
              puVar8[0x6e] = iVar5;
              *(code **)(*(longlong *)(puVar8 + 0x6a) + 0x2a8) = FUN_180017da0;
              *(code **)(*(longlong *)(puVar8 + 0x6a) + 0x2b0) = FUN_180018050;
              goto LAB_180017012;
            }
            if (uVar17 == 2) {
              puVar20 = &local_d4;
              puVar19 = &local_d8;
              local_d4 = 0;
              local_d0 = 0;
              uVar16 = uVar16 * puVar8[0x70] * 2;
              local_d8 = uVar16;
              uVar7 = FUN_180018970(*(undefined8 **)(puVar8 + 0x6a),(undefined8 *)(puVar8 + 0x6c),
                                    puVar19,puVar20);
              if ((int)uVar7 != 0) {
                pcVar15 = "Failed to get input buffer (WaveRT)";
                goto LAB_180016f7a;
              }
              if (local_d8 != uVar16) {
                puVar8[0x6f] = local_d8 / (uint)(puVar8[0x70] * 2);
              }
              lVar13 = *(longlong *)(puVar8 + 0x6a);
              puVar8[0x6e] = local_d8;
              if (*(int *)(lVar13 + 0x228) == 2) {
                *(code **)(lVar13 + 0x2a8) = FUN_180017f70;
              }
              else {
                *(code **)(lVar13 + 0x2a8) = FUN_180017e20;
              }
              puVar9 = &local_c4;
              *(undefined1 **)(*(longlong *)(puVar8 + 0x6a) + 0x2b0) = &LAB_1800180b0;
              *(code **)(*(longlong *)(puVar8 + 0x6a) + 0x298) = _guard_check_icall;
              uVar4 = FUN_180018bb0(*(undefined8 **)(puVar8 + 0x6a),&local_d0,&local_c4,puVar9);
              if (uVar4 == 0) {
                *(uint *)(*(longlong *)(puVar8 + 0x6a) + 0x290) = local_d0;
                *(double *)(puVar8 + 0xe) =
                     (double)((ulonglong)local_d0 / (ulonglong)(uint)puVar8[0x70]) / param_5 +
                     *(double *)(puVar8 + 0xe);
              }
              else {
                *(undefined4 *)(*(longlong *)(puVar8 + 0x6a) + 0x290) = 0;
              }
              goto LAB_180017012;
            }
LAB_180016e2a:
            uVar4 = 0xffffd8fe;
            FUN_180018400(0xffffd8fe,"Wave type %u ??",uVar7,puVar9);
          }
        }
        else {
          FUN_180018400(extraout_EAX,
                        "PaUtil_InitializeBufferProcessor failed: ich=%u, isf=%u, hisf=%u, och=%u, osf=%u, hosf=%u, sr=%lf, flags=0x%X, fpub=%u, fphb=%u"
                        ,(ulonglong)(uint)puVar8[0xa4],(ulonglong)uVar17);
        }
      }
      else {
        lVar13 = *(longlong *)(param_4 + 6);
        uVar10 = FUN_180020750(local_d4);
        local_d4 = (uint)uVar10;
        if ((lVar13 != 0) && ((*(byte *)(lVar13 + 0xc) & 2) != 0)) {
          local_d4 = *(uint *)(lVar13 + 0x14);
        }
        local_cc = 0xffffd8f6;
        lVar13 = *(longlong *)(*(longlong *)(local_c0 + 0x28) + (longlong)(int)*param_4 * 8);
        lVar23 = *(longlong *)(lVar13 + 0x150);
        lVar13 = *(longlong *)
                  (*(longlong *)(lVar23 + 0x438) + (ulonglong)*(uint *)(lVar13 + 0x158) * 8);
        puVar8[0xa6] = uVar16;
        uVar16 = FUN_1800030c0(*(uint *)(lVar13 + 0x27c),local_d0);
        if (uVar16 == 0xffffd8f6) {
          uVar4 = 0xffffd8f1;
          FUN_180018400(0xffffd8f1,"PU_SCAF(%X,%X) failed (output)",
                        (ulonglong)*(uint *)(lVar13 + 0x27c),0xffffd8f6);
        }
        else {
          if ((*(int *)(lVar23 + 0x418) != 2) || (uVar16 != 8)) {
            sVar21 = 0;
            uVar4 = 0xffffd8f6;
            goto joined_r0x000180016a18;
          }
          uVar16 = 4;
          do {
            uVar17 = puVar8[0xa6];
            while( true ) {
              uVar10 = FUN_1800208c0(uVar16);
              FUN_180020830(&local_88,uVar17,uVar16,(uint)uVar10,param_5,local_d4);
              puVar8[0x80] = (uint)local_7c;
              if (sVar21 != 0) {
                local_76 = sVar21;
              }
              param_9 = &local_cc;
              param_8 = &local_88;
              puVar11 = FUN_1800154f0(lVar23,*(int *)(lVar13 + 0x22c),param_8,(int *)param_9);
              *(undefined8 **)(puVar8 + 0x7a) = puVar11;
              puVar8[0xa7] = uVar17;
              if (local_cc != 0) {
                if (local_cc == 0xffffd8ff) goto LAB_180016b55;
                uVar10 = FUN_1800208c0(uVar16);
                FUN_1800207c0(&local_88,uVar17,uVar16,(short)uVar10,param_5);
                if (sVar21 != 0) {
                  local_76 = sVar21;
                }
                param_9 = &local_cc;
                param_8 = &local_88;
                puVar11 = FUN_1800154f0(lVar23,*(int *)(lVar13 + 0x22c),param_8,(int *)param_9);
                *(undefined8 **)(puVar8 + 0x7a) = puVar11;
              }
              if (local_cc == 0xffffd8ff) goto LAB_180016b55;
              uVar4 = local_cc;
              if (local_cc == 0) goto LAB_180016b1c;
              uVar22 = *(uint *)(lVar13 + 0x278);
              if (uVar22 <= uVar17) break;
              uVar17 = (uVar17 & 0xfffffffe) + 2;
              if (uVar22 <= uVar17) {
                uVar17 = uVar22;
              }
            }
            uVar16 = uVar16 * 2;
joined_r0x000180016a18:
          } while (uVar16 < 0x41);
LAB_180016b1c:
          lVar13 = *(longlong *)(puVar8 + 0x7a);
          if (lVar13 != 0) {
            puVar8[0x81] = (uint)puVar8[0x80] / (uint)puVar8[0xa7];
            *(uint *)(lVar13 + 0x274) = *(uint *)(lVar13 + 0x274) / (uint)puVar8[0x80];
            goto LAB_180016ba2;
          }
          FUN_180018400(uVar4,"Failed to create render pin: sr=%u,ch=%u,bits=%u,align=%u",
                        (ulonglong)local_84,(ulonglong)local_86);
        }
      }
    }
    else {
      uVar10 = FUN_180020750(iVar5);
      uVar16 = local_d8;
      local_c4 = (undefined4)uVar10;
      local_cc = 0xffffd8f6;
      lVar13 = *(longlong *)(*(longlong *)(lVar13 + 0x28) + (longlong)*param_3 * 8);
      lVar23 = *(longlong *)(lVar13 + 0x150);
      lVar2 = *(longlong *)
               (*(longlong *)(lVar23 + 0x438) + (ulonglong)*(uint *)(lVar13 + 0x158) * 8);
      puVar8[0xa4] = iVar5;
      local_a8 = lVar13;
      local_c8 = FUN_1800030c0(*(uint *)(lVar2 + 0x27c),local_d8);
      if (local_c8 == 0xffffd8f6) {
        uVar4 = 0xffffd8f1;
        FUN_180018400(0xffffd8f1,"PU_SCAF(%X,%X) failed (input)",(ulonglong)*(uint *)(lVar2 + 0x27c)
                      ,(ulonglong)uVar16);
      }
      else {
        if ((*(int *)(lVar23 + 0x418) != 2) || (local_c8 != 8)) {
          sVar18 = 0;
          uVar4 = 0xffffd8f6;
          goto joined_r0x00018001672a;
        }
        sVar18 = 0x18;
        local_c8 = 4;
        do {
          uVar16 = puVar8[0xa4];
          while( true ) {
            uVar10 = FUN_1800208c0(local_c8);
            FUN_180020830(&local_88,uVar16,local_c8,(uint)uVar10,param_5,local_c4);
            puVar8[0x70] = (uint)local_7c;
            if (sVar18 != 0) {
              local_76 = sVar18;
            }
            param_9 = &local_cc;
            param_8 = &local_88;
            puVar11 = FUN_1800154f0(lVar23,*(int *)(lVar2 + 0x22c),param_8,(int *)param_9);
            uVar4 = local_c8;
            *(undefined8 **)(puVar8 + 0x6a) = puVar11;
            puVar8[0xa5] = uVar16;
            if (local_cc != 0) {
              if (local_cc == 0xffffd8ff) goto LAB_180016b55;
              uVar10 = FUN_1800208c0(local_c8);
              FUN_1800207c0(&local_88,uVar16,uVar4,(short)uVar10,param_5);
              if (sVar18 != 0) {
                local_76 = sVar18;
              }
              param_9 = &local_cc;
              param_8 = &local_88;
              puVar11 = FUN_1800154f0(lVar23,*(int *)(lVar2 + 0x22c),param_8,(int *)param_9);
              *(undefined8 **)(puVar8 + 0x6a) = puVar11;
            }
            if (local_cc == 0xffffd8ff) goto LAB_180016b55;
            lVar13 = local_a8;
            uVar4 = local_cc;
            if (local_cc == 0) goto LAB_18001683e;
            uVar17 = *(uint *)(lVar2 + 0x278);
            if (uVar17 <= uVar16) break;
            uVar16 = (uVar16 & 0xfffffffe) + 2;
            if (uVar17 <= uVar16) {
              uVar16 = uVar17;
            }
          }
          local_c8 = local_c8 * 2;
joined_r0x00018001672a:
        } while (local_c8 < 0x41);
LAB_18001683e:
        if (*(longlong *)(puVar8 + 0x6a) == 0) {
          FUN_180018400(uVar4,"Failed to create capture pin: sr=%u,ch=%u,bits=%u,align=%u",
                        (ulonglong)local_84,(ulonglong)local_86);
        }
        else {
          if (*(int *)(lVar13 + 0x15c) < 0) {
LAB_180016910:
            puVar8[0x71] = (uint)puVar8[0x70] / (uint)puVar8[0xa5];
            *(uint *)(*(longlong *)(puVar8 + 0x6a) + 0x274) =
                 *(uint *)(*(longlong *)(puVar8 + 0x6a) + 0x274) / (uint)puVar8[0x70];
            param_4 = local_b8;
            goto LAB_18001695a;
          }
          uVar10 = FUN_180015970(*(longlong **)(*(longlong *)(lVar2 + 0x220) + 0x440));
          uVar4 = (uint)uVar10;
          if (uVar4 == 0) {
            lVar13 = *(longlong *)
                      (*(longlong *)(lVar2 + 8) + (longlong)*(int *)(lVar13 + 0x15c) * 8);
            uVar16 = *(uint *)(lVar13 + 0x208);
            uVar7 = (ulonglong)uVar16;
            uVar4 = FUN_18001b7f0((HANDLE)**(undefined8 **)(*(longlong *)(lVar2 + 0x220) + 0x440),
                                  *(undefined4 *)(lVar13 + 0x20c),uVar16);
            FUN_180015920(*(undefined8 **)(*(longlong *)(lVar2 + 0x220) + 0x440));
            if (uVar4 == 0) goto LAB_180016910;
            FUN_180018400(uVar4,"Failed to set topology mux node",uVar7,param_9);
          }
          else {
            FUN_180018400(uVar4,"Failed to open topology filter",param_8,param_9);
          }
        }
      }
    }
  }
LAB_180017940:
  FUN_1800069c0((longlong)(puVar8 + 0x26));
  FUN_180014f90((longlong)puVar8);
  if (*(longlong *)(puVar8 + 0x68) != 0) {
    FUN_180001280(*(longlong *)(puVar8 + 0x68));
    FUN_180001230(*(longlong *)(puVar8 + 0x68));
    *(undefined8 *)(puVar8 + 0x68) = 0;
  }
  if (*(longlong **)(puVar8 + 0x7a) != (longlong *)0x0) {
    FUN_180018770(*(longlong **)(puVar8 + 0x7a));
  }
  if (*(longlong **)(puVar8 + 0x6a) != (longlong *)0x0) {
    FUN_180018770(*(longlong **)(puVar8 + 0x6a));
  }
  FUN_180020240((longlong)puVar8);
LAB_1800179a6:
  return (ulonglong)uVar4;
LAB_180016b55:
  uVar4 = local_cc;
  FUN_180018400(local_cc,"Device is occupied",param_8,param_9);
  goto LAB_180017940;
}



/* ========================================================================
   ENTRY: 1800179e0
   NAME : FUN_1800179e0
   SIG  : ulonglong __fastcall FUN_1800179e0(longlong * param_1)
   ======================================================================== */

ulonglong FUN_1800179e0(longlong *param_1)

{
  longlong lVar1;
  double dVar2;
  bool bVar3;
  uint uVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  longlong lVar7;
  uint uVar8;
  int iVar9;
  bool bVar10;
  uint local_res10 [2];
  longlong local_48 [2];
  
  uVar6 = 0;
  uVar4 = FUN_180006b00(*param_1 + 0x268);
  if ((*(int *)((longlong)param_1 + 0x24) == 0) &&
     (((int)param_1[8] != *(int *)((longlong)param_1 + 0x44) || (uVar4 != 0)))) {
    if ((*(longlong *)(*param_1 + 0x1a8) == 0) ||
       ((*(longlong *)(*param_1 + 0x1e8) == 0 ||
        (bVar3 = true, *(int *)((longlong)param_1 + 0x2c) != 0)))) {
      bVar3 = false;
    }
    FUN_1800038c0(*param_1 + 0x80);
    dVar2 = FUN_180020250();
    param_1[2] = (longlong)dVar2;
    FUN_180005ce0(*param_1 + 0x98,(double *)(param_1 + 1),(int)param_1[4]);
    lVar7 = *param_1;
    *(undefined4 *)(param_1 + 4) = 0;
    if (*(uint *)((longlong)param_1 + 0x44) == *(uint *)(param_1 + 8)) {
      bVar10 = false;
    }
    else {
      lVar1 = param_1[(ulonglong)(*(uint *)((longlong)param_1 + 0x44) & 3) * 2 + 0x11];
      FUN_1800069a0(lVar7 + 0x98,*(int *)(lVar7 + 0x1fc));
      lVar7 = *param_1;
      iVar9 = *(int *)(lVar7 + 0x298);
      if (0 < iVar9) {
        uVar8 = 0;
        do {
          FUN_180006980(lVar7 + 0x98,uVar8,
                        (ulonglong)(uVar8 * *(int *)(lVar7 + 0x204)) + *(longlong *)(lVar1 + 0x28),
                        *(undefined4 *)(lVar7 + 0x29c));
          lVar7 = *param_1;
          uVar8 = uVar8 + 1;
          iVar9 = *(int *)(lVar7 + 0x298);
        } while ((int)uVar8 < iVar9);
      }
      bVar10 = iVar9 == 1;
    }
    if ((uVar4 == 0) || ((*(int *)(lVar7 + 0x298) != 0 && ((int)uVar4 < *(int *)(lVar7 + 0x1fc)))))
    {
      uVar4 = 0;
      if ((0 < *(int *)(lVar7 + 0x298)) && (0 < *(int *)(lVar7 + 0x290))) {
        FUN_180006930(lVar7 + 0x98);
      }
    }
    else {
      local_res10[0] = 0;
      local_res10[1] = 0;
      local_48[0] = 0;
      local_48[1] = 0;
      uVar8 = uVar4;
      if ((*(int *)(lVar7 + 0x298) != 0) &&
         (uVar8 = *(uint *)(lVar7 + 0x1fc), (int)uVar4 < (int)*(uint *)(lVar7 + 0x1fc))) {
        uVar8 = uVar4;
      }
      uVar4 = FUN_180006b10((int *)(lVar7 + 0x268),uVar8,local_48,local_res10,local_48 + 1,
                            (int *)(local_res10 + 1));
      uVar5 = 0;
      do {
        if (local_res10[uVar5] == 0) break;
        (*(code *)(&PTR_FUN_180025348)[uVar5])(*param_1 + 0x98);
        lVar7 = *param_1;
        if (0 < *(int *)(lVar7 + 0x290)) {
          iVar9 = 0;
          do {
            (*(code *)(&PTR_FUN_180025358)[uVar5])
                      (lVar7 + 0x98,iVar9,
                       (ulonglong)(uint)(iVar9 * *(int *)(lVar7 + 0x1c4)) + local_48[uVar5],
                       *(undefined4 *)(lVar7 + 0x294));
            lVar7 = *param_1;
            iVar9 = iVar9 + 1;
          } while (iVar9 < *(int *)(lVar7 + 0x290));
        }
        uVar8 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar8;
      } while (uVar8 < 2);
    }
    lVar7 = *param_1;
    if ((bVar3) &&
       ((iVar9 = *(int *)(lVar7 + 0x160) + *(int *)(lVar7 + 0x15c),
        *(int *)(lVar7 + 0x144) + *(int *)(lVar7 + 0x140) != iVar9 || (iVar9 == 0)))) {
      uVar8 = 0;
    }
    else {
      uVar8 = FUN_180006010((uint *)(lVar7 + 0x98),(int *)((longlong)param_1 + 0x24));
    }
    if (bVar10) {
      lVar7 = *param_1;
      iVar9 = *(int *)(lVar7 + 0x204);
      lVar1 = param_1[(ulonglong)(*(uint *)((longlong)param_1 + 0x44) & 3) * 2 + 0x11];
      if (iVar9 == 2) {
        FUN_1800153c0(*(undefined2 **)(lVar1 + 0x28),*(int *)(lVar7 + 0x29c),*(int *)(lVar7 + 0x1fc)
                     );
      }
      else if (iVar9 == 3) {
        FUN_180015420(*(undefined1 **)(lVar1 + 0x28),*(int *)(lVar7 + 0x29c),*(int *)(lVar7 + 0x1fc)
                     );
      }
      else if (iVar9 == 4) {
        FUN_180015470(*(undefined4 **)(lVar1 + 0x28),*(int *)(lVar7 + 0x29c),*(int *)(lVar7 + 0x1fc)
                     );
      }
    }
    FUN_1800038e0((double *)(*param_1 + 0x80),uVar8);
    if (uVar4 != 0) {
      FUN_180006ad0(*param_1 + 0x268,uVar4);
    }
    if (*(int *)((longlong)param_1 + 0x44) != (int)param_1[8]) {
      if (*(int *)(*param_1 + 0x230) == 0) {
        uVar5 = (**(code **)(*(longlong *)(*param_1 + 0x1e8) + 0x2b0))(param_1);
        uVar6 = uVar5 & 0xffffffff;
        if ((int)uVar5 != 0) {
          return uVar5;
        }
      }
      *(int *)((longlong)param_1 + 0x44) = *(int *)((longlong)param_1 + 0x44) + 1;
      if (((int)param_1[6] == 0) && (*(int *)((longlong)param_1 + 0x2c) == 0)) {
        uVar5 = FUN_18001b090(param_1);
        uVar6 = uVar5 & 0xffffffff;
        if ((int)uVar5 == 0) {
          *(undefined4 *)(param_1 + 6) = 1;
        }
      }
    }
  }
  return uVar6;
}



/* ========================================================================
   ENTRY: 180017da0
   NAME : FUN_180017da0
   SIG  : undefined8 __fastcall FUN_180017da0(longlong * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 FUN_180017da0(longlong *param_1,ulonglong param_2)

{
  longlong lVar1;
  undefined8 uVar2;
  
  lVar1 = *(longlong *)(*param_1 + 0x1d8) + (param_2 & 0xffffffff) * 0x58;
  if (*(int *)(lVar1 + 0x24) == 0) {
    ResetEvent(*(HANDLE *)(lVar1 + 0x50));
    uVar2 = 0xffffffff;
  }
  else {
    uVar2 = 0;
    param_1[(ulonglong)(*(uint *)(param_1 + 7) & 3) * 2 + 9] = lVar1;
    FUN_180006d40((int *)(*param_1 + 0x268),*(void **)(lVar1 + 0x28),*(int *)(*param_1 + 0x1bc));
    *(int *)(param_1 + 7) = (int)param_1[7] + 1;
  }
  *(int *)(param_1 + 5) = (int)param_1[5] + -1;
  return uVar2;
}



/* ========================================================================
   ENTRY: 180017e20
   NAME : FUN_180017e20
   SIG  : undefined8 __fastcall FUN_180017e20(longlong * param_1)
   ======================================================================== */

undefined8 FUN_180017e20(longlong *param_1)

{
  longlong lVar1;
  longlong lVar2;
  ulonglong uVar3;
  int iVar4;
  int iVar5;
  uint uVar6;
  uint uVar7;
  ulonglong uVar8;
  uint local_res8;
  
  lVar1 = *param_1;
  lVar2 = *(longlong *)(lVar1 + 0x1a8);
  (**(code **)(lVar2 + 0x2a0))(lVar2);
  uVar8 = (ulonglong)local_res8 % (ulonglong)*(uint *)(lVar1 + 0x1b8);
  uVar6 = *(uint *)(lVar1 + 0x1c0);
  (**(code **)(lVar2 + 0x298))();
  uVar7 = *(uint *)(lVar1 + 0x1b8) - *(uint *)(lVar1 + 0x1e0);
  uVar8 = (ulonglong)(((int)uVar8 - (int)(uVar8 % (ulonglong)uVar6)) + uVar7) %
          (ulonglong)*(uint *)(lVar1 + 0x1b8);
  uVar6 = (uint)uVar8;
  if (uVar6 != 0) {
    if (uVar7 <= uVar6) {
      uVar8 = (ulonglong)uVar7;
    }
    iVar5 = (int)(uVar8 / *(uint *)(lVar1 + 0x1c0));
    iVar4 = FUN_180006d40((int *)(*param_1 + 0x268),
                          (void *)((ulonglong)*(uint *)(lVar1 + 0x1e0) +
                                  *(longlong *)(lVar1 + 0x1b0)),iVar5);
    uVar3 = (ulonglong)(*(uint *)(lVar1 + 0x1c0) * iVar4 + *(int *)(lVar1 + 0x1e0)) %
            (ulonglong)*(uint *)(lVar1 + 0x1b8);
    *(int *)(lVar1 + 0x1e0) = (int)uVar3;
    if (((uint)uVar8 < uVar6) && (iVar4 == iVar5)) {
      iVar5 = FUN_180006d40((int *)(*param_1 + 0x268),(void *)(uVar3 + *(longlong *)(lVar1 + 0x1b0))
                            ,(uVar6 - (uint)uVar8) / *(uint *)(lVar1 + 0x1c0));
      *(uint *)(lVar1 + 0x1e0) =
           (uint)(iVar5 * *(int *)(lVar1 + 0x1c0) + *(int *)(lVar1 + 0x1e0)) %
           *(uint *)(lVar1 + 0x1b8);
    }
  }
  *(int *)(param_1 + 7) = (int)param_1[7] + 1;
  *(int *)(param_1 + 5) = (int)param_1[5] + -1;
  return 0;
}



/* ========================================================================
   ENTRY: 180017f70
   NAME : FUN_180017f70
   SIG  : undefined8 __fastcall FUN_180017f70(longlong * param_1)
   ======================================================================== */

undefined8 FUN_180017f70(longlong *param_1)

{
  uint uVar1;
  longlong lVar2;
  longlong lVar3;
  ulonglong uVar4;
  int iVar5;
  int local_res8;
  
  lVar2 = *param_1;
  lVar3 = *(longlong *)(lVar2 + 0x1a8);
  (**(code **)(lVar3 + 0x2a0))(lVar3);
  uVar4 = (ulonglong)(uint)(local_res8 + *(int *)(lVar3 + 0x290)) %
          (ulonglong)*(uint *)(lVar2 + 0x1b8);
  uVar1 = *(uint *)(lVar2 + 0x1c0);
  (**(code **)(lVar3 + 0x298))();
  uVar4 = (ulonglong)
          ((*(uint *)(lVar2 + 0x1b8) - *(uint *)(lVar2 + 0x1e0)) +
          ((int)uVar4 - (int)(uVar4 % (ulonglong)uVar1))) % (ulonglong)*(uint *)(lVar2 + 0x1b8);
  if ((int)uVar4 != 0) {
    iVar5 = FUN_180006d40((int *)(*param_1 + 0x268),
                          (void *)((ulonglong)*(uint *)(lVar2 + 0x1e0) +
                                  *(longlong *)(lVar2 + 0x1b0)),
                          (int)(uVar4 / *(uint *)(lVar2 + 0x1c0)));
    *(uint *)(lVar2 + 0x1e0) =
         (uint)(iVar5 * *(int *)(lVar2 + 0x1c0) + *(int *)(lVar2 + 0x1e0)) %
         *(uint *)(lVar2 + 0x1b8);
    *(int *)(param_1 + 7) = (int)param_1[7] + 1;
    *(int *)(param_1 + 5) = (int)param_1[5] + -1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180018050
   NAME : FUN_180018050
   SIG  : undefined __fastcall FUN_180018050(longlong * param_1)
   ======================================================================== */

void FUN_180018050(longlong *param_1)

{
  DWORD *pDVar1;
  ulonglong uVar2;
  
  uVar2 = (ulonglong)(*(uint *)((longlong)param_1 + 0x3c) & 3);
  pDVar1 = (DWORD *)param_1[uVar2 * 2 + 9];
  param_1[uVar2 * 2 + 9] = 0;
  pDVar1[9] = 0;
  pDVar1[0xc] = 0;
  ResetEvent(*(HANDLE *)(pDVar1 + 0x14));
  FUN_180019e20((HANDLE)**(undefined8 **)(*param_1 + 0x1a8),pDVar1);
  *(int *)(param_1 + 5) = (int)param_1[5] + 1;
  return;
}



/* ========================================================================
   ENTRY: 180018100
   NAME : FUN_180018100
   SIG  : undefined8 __fastcall FUN_180018100(longlong * param_1)
   ======================================================================== */

undefined8 FUN_180018100(longlong *param_1)

{
  longlong lVar1;
  longlong lVar2;
  ulonglong uVar3;
  uint uVar4;
  ulonglong uVar5;
  uint uVar6;
  int local_res8;
  
  lVar1 = *param_1;
  uVar5 = (ulonglong)(*(uint *)(param_1 + 8) & 3);
  lVar2 = *(longlong *)(lVar1 + 0x1e8);
  uVar6 = *(uint *)(lVar1 + 0x1f8) >> 1;
  (**(code **)(lVar2 + 0x2a0))(lVar2);
  uVar3 = (ulonglong)(uint)(local_res8 + *(int *)(lVar2 + 0x290)) %
          (ulonglong)*(uint *)(lVar1 + 0x1f8);
  uVar4 = (uint)((uint)((int)uVar3 - (int)(uVar3 % (ulonglong)*(uint *)(lVar1 + 0x200))) < uVar6);
  if (*(int *)((longlong)param_1 + 0x2c) != 0) {
    uVar4 = *(uint *)(param_1 + 8) & 1;
  }
  param_1[uVar5 * 2 + 0x11] = (ulonglong)uVar4 * 0x58 + *(longlong *)(*param_1 + 0x218);
  *(uint *)(param_1 + uVar5 * 2 + 0x12) = uVar4 * uVar6;
  *(uint *)((longlong)param_1 + uVar5 * 0x10 + 0x94) = uVar6;
  *(int *)(param_1 + 8) = (int)param_1[8] + 1;
  *(int *)(param_1 + 5) = (int)param_1[5] + -1;
  return 0;
}



/* ========================================================================
   ENTRY: 1800181d0
   NAME : FUN_1800181d0
   SIG  : undefined8 __fastcall FUN_1800181d0(longlong * param_1)
   ======================================================================== */

undefined8 FUN_1800181d0(longlong *param_1)

{
  longlong lVar1;
  longlong lVar2;
  ulonglong uVar3;
  uint uVar4;
  uint uVar5;
  ulonglong uVar6;
  uint uVar7;
  int local_res8;
  
  lVar1 = *param_1;
  uVar6 = (ulonglong)(*(uint *)(param_1 + 8) & 3);
  lVar2 = *(longlong *)(lVar1 + 0x1e8);
  uVar7 = *(uint *)(lVar1 + 0x1f8) >> 1;
  (**(code **)(lVar2 + 0x2a0))(lVar2);
  uVar3 = (ulonglong)(uint)(local_res8 + *(int *)(lVar2 + 0x290)) %
          (ulonglong)*(uint *)(lVar1 + 0x1f8);
  uVar5 = (int)uVar3 - (int)(uVar3 % (ulonglong)*(uint *)(lVar1 + 0x200));
  if (*(int *)((longlong)param_1 + 0x2c) == 0) {
    *(int *)(lVar1 + 0x224) = *(int *)(lVar1 + 0x224) + 1;
    if (uVar7 <= ((*(uint *)(lVar1 + 0x1f8) - *(int *)(lVar1 + 0x220)) + uVar5) %
                 *(uint *)(lVar1 + 0x1f8)) {
      uVar4 = (uint)(uVar5 < uVar7);
      param_1[uVar6 * 2 + 0x11] =
           (ulonglong)(-(uint)(uVar5 < uVar7) & 0x58) + *(longlong *)(*param_1 + 0x218);
      uVar5 = uVar7;
      if (uVar4 != 0) {
        uVar5 = 0;
      }
      *(uint *)(lVar1 + 0x220) = uVar5;
      *(uint *)((longlong)param_1 + uVar6 * 0x10 + 0x94) = uVar7;
      *(uint *)(param_1 + uVar6 * 2 + 0x12) = uVar4 * uVar7;
      *(int *)(param_1 + 8) = (int)param_1[8] + 1;
      *(int *)(param_1 + 5) = (int)param_1[5] + -1;
      *(undefined4 *)(lVar1 + 0x224) = 0;
    }
  }
  else {
    uVar5 = *(uint *)(param_1 + 8);
    param_1[uVar6 * 2 + 0x11] = (ulonglong)(uVar5 & 1) * 0x58 + *(longlong *)(*param_1 + 0x218);
    *(uint *)((longlong)param_1 + uVar6 * 0x10 + 0x94) = uVar7;
    *(uint *)(param_1 + uVar6 * 2 + 0x12) = (uVar5 & 1) * uVar7;
    *(int *)(param_1 + 8) = (int)param_1[8] + 1;
    *(int *)(param_1 + 5) = (int)param_1[5] + -1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180018310
   NAME : FUN_180018310
   SIG  : undefined __fastcall FUN_180018310(longlong * param_1)
   ======================================================================== */

void FUN_180018310(longlong *param_1)

{
  DWORD *pDVar1;
  ulonglong uVar2;
  
  uVar2 = (ulonglong)(*(uint *)((longlong)param_1 + 0x44) & 3);
  pDVar1 = (DWORD *)param_1[uVar2 * 2 + 0x11];
  param_1[uVar2 * 2 + 0x11] = 0;
  ResetEvent(*(HANDLE *)(pDVar1 + 0x14));
  FUN_18001a160((HANDLE)**(undefined8 **)(*param_1 + 0x1e8),pDVar1);
  *(int *)(param_1 + 5) = (int)param_1[5] + 1;
  if (*(int *)((longlong)param_1 + 0x2c) != 0) {
    *(int *)((longlong)param_1 + 0x2c) = *(int *)((longlong)param_1 + 0x2c) + -1;
  }
  return;
}



/* ========================================================================
   ENTRY: 180018380
   NAME : FUN_180018380
   SIG  : undefined8 __fastcall FUN_180018380(longlong * param_1)
   ======================================================================== */

undefined8 FUN_180018380(longlong *param_1)

{
  longlong lVar1;
  
  lVar1 = *(longlong *)(*param_1 + 0x1e8);
  param_1[(ulonglong)(*(uint *)((longlong)param_1 + 0x44) & 3) * 2 + 0x11] = 0;
  (**(code **)(lVar1 + 0x298))();
  *(int *)(param_1 + 5) = (int)param_1[5] + 1;
  if ((*(int *)((longlong)param_1 + 0x2c) != 0) &&
     (*(int *)((longlong)param_1 + 0x2c) = *(int *)((longlong)param_1 + 0x2c) + -1,
     *(int *)((longlong)param_1 + 0x2c) != 0)) {
    SetEvent((HANDLE)**(undefined8 **)(*param_1 + 0x210));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180018400
   NAME : FUN_180018400
   SIG  : undefined __fastcall FUN_180018400(undefined4 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180018400(undefined4 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  ulonglong *puVar1;
  undefined8 local_res18;
  undefined8 local_res20;
  undefined1 auStack_458 [32];
  undefined8 local_438;
  undefined8 *local_430;
  char local_428 [1024];
  ulonglong local_28;
  
  local_28 = DAT_18002b580 ^ (ulonglong)auStack_458;
  local_res18 = param_3;
  local_res20 = param_4;
  puVar1 = (ulonglong *)FUN_180003990();
  local_438 = 0;
  local_430 = &local_res18;
  __stdio_common_vsprintf(*puVar1 | 1,local_428,0x3ff,param_2);
  FUN_180003c90(0xb,param_1,local_428);
  return;
}



/* ========================================================================
   ENTRY: 180018490
   NAME : FUN_180018490
   SIG  : ulonglong __fastcall FUN_180018490(ulonglong * param_1, uint param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong FUN_180018490(ulonglong *param_1,uint param_2)

{
  ulonglong uVar1;
  undefined4 *puVar2;
  ulonglong uVar3;
  ulonglong uVar4;
  int local_res18 [2];
  undefined8 *local_res20;
  
  local_res18[0] = 0;
  local_res20 = (undefined8 *)0x0;
  if (((DAT_18002bb80 != (HMODULE)0x0) ||
      (DAT_18002bb80 = LoadLibraryA("ksuser.dll"), uVar4 = 0, uVar1 = 0,
      DAT_18002bb80 != (HMODULE)0x0)) &&
     (DAT_18002bb88 = GetProcAddress(DAT_18002bb80,"KsCreatePin"), uVar4 = 0, uVar1 = 0,
     DAT_18002bb88 != (FARPROC)0x0)) {
    if ((DAT_18002bb90 == (HMODULE)0x0) &&
       (DAT_18002bb90 = LoadLibraryA("avrt.dll"), DAT_18002bb90 != (HMODULE)0x0)) {
      DAT_18002bb98 = GetProcAddress(DAT_18002bb90,"AvSetMmThreadCharacteristicsA");
      _DAT_18002bba0 = GetProcAddress(DAT_18002bb90,"AvRevertMmThreadCharacteristics");
      DAT_18002bba8 = GetProcAddress(DAT_18002bb90,"AvSetMmThreadPriority");
    }
    uVar1 = FUN_180020230(0x118);
    if (uVar1 != 0) {
      puVar2 = FUN_1800011b0();
      *(undefined4 **)(uVar1 + 0x108) = puVar2;
      if (puVar2 != (undefined4 *)0x0) {
        *param_1 = uVar1;
        *(undefined4 *)(uVar1 + 8) = 1;
        *(undefined4 *)(*param_1 + 0xc) = 0xb;
        *(char **)(*param_1 + 0x10) = "Windows WDM-KS";
        *(undefined4 *)(*param_1 + 0x18) = 0;
        *(undefined4 *)(*param_1 + 0x1c) = 0xffffffff;
        *(undefined4 *)(*param_1 + 0x20) = 0xffffffff;
        *(undefined8 *)(*param_1 + 0x28) = 0;
        uVar3 = FUN_18001aaa0(uVar1,param_2,&local_res20,local_res18);
        uVar4 = uVar3 & 0xffffffff;
        if ((int)uVar3 == 0) {
          FUN_180015100(uVar1,(ulonglong)param_2,local_res20,local_res18[0]);
          *(code **)(*param_1 + 0x30) = FUN_18001b380;
          *(code **)(*param_1 + 0x38) = FUN_180016450;
          *(code **)(*param_1 + 0x40) = FUN_180016010;
          FUN_180006e10((undefined8 *)(uVar1 + 0x48),FUN_180014ee0,FUN_18001b0e0,FUN_18001b2a0,
                        FUN_1800148a0,&LAB_180016410,&LAB_180016400,&LAB_18000bf70,&LAB_180015ec0,
                        &LAB_180006df0,&LAB_180006e00,&LAB_180006df0,&LAB_180006e00);
          FUN_180006e10((undefined8 *)(uVar1 + 0xa8),FUN_180014ee0,FUN_18001b0e0,FUN_18001b2a0,
                        FUN_1800148a0,&LAB_180016410,&LAB_180016400,&LAB_18000bf70,&LAB_180006de0,
                        PaWasapi_UpdateDeviceList,PaWasapi_UpdateDeviceList,FUN_18000bf60,
                        FUN_18000bf60);
          return 0;
        }
        goto LAB_180018588;
      }
    }
    uVar4 = 0xffffd8f8;
  }
LAB_180018588:
  FUN_18001b380(uVar1);
  return uVar4;
}



/* ========================================================================
   ENTRY: 180018770
   NAME : FUN_180018770
   SIG  : undefined __fastcall FUN_180018770(longlong * param_1)
   ======================================================================== */

void FUN_180018770(longlong *param_1)

{
  if ((param_1 != (longlong *)0x0) && (*param_1 != 0)) {
    FUN_18001a090(param_1,2);
    FUN_18001a090(param_1,0);
    CloseHandle((HANDLE)*param_1);
    *param_1 = 0;
    FUN_180015920((undefined8 *)param_1[0x44]);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800187c0
   NAME : FUN_1800187c0
   SIG  : undefined __fastcall FUN_1800187c0(longlong * param_1)
   ======================================================================== */

void FUN_1800187c0(longlong *param_1)

{
  uint uVar1;
  ulonglong uVar2;
  
  if (param_1 != (longlong *)0x0) {
    FUN_180018770(param_1);
    if (param_1[0x47] != 0) {
      FUN_180020240(param_1[0x47]);
    }
    if (param_1[0x4c] != 0) {
      FUN_180020240(param_1[0x4c]);
    }
    if (param_1[1] != 0) {
      uVar2 = 0;
      if ((int)param_1[2] != 0) {
        do {
          FUN_180020240(*(longlong *)(param_1[1] + uVar2 * 8));
          uVar1 = (int)uVar2 + 1;
          uVar2 = (ulonglong)uVar1;
        } while (uVar1 < *(uint *)(param_1 + 2));
      }
      FUN_180020240(param_1[1]);
    }
    FUN_180020240((longlong)param_1);
  }
  return;
}



/* ========================================================================
   ENTRY: 180018850
   NAME : FUN_180018850
   SIG  : undefined __fastcall FUN_180018850(undefined8 * param_1, undefined4 * param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180018850(undefined8 *param_1,undefined4 *param_2)

{
  undefined8 uVar1;
  undefined1 auStackY_78 [32];
  undefined8 local_38;
  undefined8 uStack_30;
  undefined4 local_28;
  undefined4 local_24;
  undefined4 local_20 [4];
  ulonglong local_10;
  
  local_10 = DAT_18002b580 ^ (ulonglong)auStackY_78;
  local_28 = 5;
  local_24 = 1;
  local_38 = _DAT_1800249a0;
  uStack_30 = _UNK_1800249a8;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_38,0x18,local_20,0x10,(LPDWORD)0x0)
  ;
  if ((int)uVar1 == 0) {
    *param_2 = local_20[0];
  }
  return;
}



/* ========================================================================
   ENTRY: 1800188e0
   NAME : FUN_1800188e0
   SIG  : undefined __fastcall FUN_1800188e0(undefined8 * param_1, undefined4 * param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800188e0(undefined8 *param_1,undefined4 *param_2)

{
  undefined8 uVar1;
  undefined1 auStackY_78 [32];
  undefined8 local_38;
  undefined8 uStack_30;
  undefined4 local_28;
  undefined4 local_24;
  undefined1 local_20 [8];
  undefined4 local_18;
  ulonglong local_10;
  
  local_10 = DAT_18002b580 ^ (ulonglong)auStackY_78;
  local_28 = 5;
  local_24 = 1;
  local_38 = _DAT_1800249a0;
  uStack_30 = _UNK_1800249a8;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_38,0x18,local_20,0x10,(LPDWORD)0x0)
  ;
  if ((int)uVar1 == 0) {
    *param_2 = local_18;
  }
  return;
}



/* ========================================================================
   ENTRY: 180018970
   NAME : FUN_180018970
   SIG  : ulonglong __fastcall FUN_180018970(undefined8 * param_1, undefined8 * param_2, uint * param_3, undefined4 * param_4)
   ======================================================================== */

ulonglong FUN_180018970(undefined8 *param_1,undefined8 *param_2,uint *param_3,undefined4 *param_4)

{
  ushort uVar1;
  ulonglong uVar2;
  uint uVar3;
  ulonglong uVar4;
  uint uVar5;
  int iVar6;
  
  iVar6 = 999;
  while( true ) {
    if ((*(int *)(param_1 + 0x45) != 2) &&
       (uVar4 = FUN_180018a60(param_1,param_2,param_3,param_4), (int)uVar4 == 0)) {
      *(undefined4 *)(param_1 + 0x45) = 1;
      return uVar4;
    }
    uVar4 = FUN_180018b10(param_1,param_2,param_3,param_4);
    if ((int)uVar4 == 0) {
      *(undefined4 *)(param_1 + 0x45) = 2;
      return uVar4;
    }
    uVar5 = *param_3;
    if ((uVar5 & 0x7f) == 0) break;
    uVar1 = *(ushort *)(param_1[0x49] + 0x4c);
    uVar3 = FUN_180005720(0x80,(uint)uVar1);
    uVar2 = ((ulonglong)uVar1 << 7) / (ulonglong)uVar3;
    uVar5 = (uVar5 - 1) + (int)uVar2;
    *param_3 = uVar5 - (int)((ulonglong)uVar5 % uVar2);
    iVar6 = iVar6 + -1;
    if (iVar6 == 0) {
      return uVar4 & 0xffffffff;
    }
  }
  return uVar4;
}



/* ========================================================================
   ENTRY: 180018a60
   NAME : FUN_180018a60
   SIG  : undefined __fastcall FUN_180018a60(undefined8 * param_1, undefined8 * param_2, undefined4 * param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180018a60(undefined8 *param_1,undefined8 *param_2,undefined4 *param_3,undefined4 *param_4)

{
  undefined8 uVar1;
  undefined8 local_48;
  undefined4 local_40;
  undefined4 local_3c;
  undefined8 local_38;
  undefined8 uStack_30;
  undefined4 local_28;
  undefined4 local_24;
  undefined8 local_20;
  undefined4 local_18;
  undefined4 local_14;
  
  local_18 = *param_3;
  local_20 = 0;
  local_14 = 2;
  local_38 = _DAT_180024990;
  uStack_30 = _UNK_180024998;
  local_28 = 5;
  local_24 = 1;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_38,0x28,&local_48,0x10,(LPDWORD)0x0
                       );
  if ((int)uVar1 == 0) {
    *param_2 = local_48;
    *param_3 = local_40;
    *param_4 = local_3c;
  }
  return;
}



/* ========================================================================
   ENTRY: 180018b10
   NAME : FUN_180018b10
   SIG  : undefined __fastcall FUN_180018b10(undefined8 * param_1, undefined8 * param_2, undefined4 * param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180018b10(undefined8 *param_1,undefined8 *param_2,undefined4 *param_3,undefined4 *param_4)

{
  undefined8 uVar1;
  undefined8 local_48;
  undefined4 local_40;
  undefined4 local_3c;
  undefined8 local_38;
  undefined8 uStack_30;
  undefined4 local_28;
  undefined4 local_24;
  undefined8 local_20;
  undefined4 local_18;
  
  local_18 = *param_3;
  local_20 = 0;
  local_28 = 1;
  local_38 = _DAT_180024990;
  uStack_30 = _UNK_180024998;
  local_24 = 1;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_38,0x28,&local_48,0x10,(LPDWORD)0x0
                       );
  if ((int)uVar1 == 0) {
    *param_2 = local_48;
    *param_3 = local_40;
    *param_4 = local_3c;
  }
  return;
}



/* ========================================================================
   ENTRY: 180018bb0
   NAME : FUN_180018bb0
   SIG  : undefined __fastcall FUN_180018bb0(undefined8 * param_1, undefined4 * param_2, undefined4 * param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180018bb0(undefined8 *param_1,undefined4 *param_2,undefined4 *param_3,undefined4 *param_4)

{
  undefined8 uVar1;
  undefined1 auStackY_88 [32];
  undefined8 local_48;
  undefined8 uStack_40;
  undefined4 local_38;
  undefined4 local_34;
  undefined4 local_30;
  undefined4 local_2c;
  undefined4 local_28;
  ulonglong local_20;
  
  local_20 = DAT_18002b580 ^ (ulonglong)auStackY_88;
  local_38 = 2;
  local_34 = 1;
  local_48 = _DAT_180024990;
  uStack_40 = _UNK_180024998;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_48,0x18,&local_30,0xc,(LPDWORD)0x0)
  ;
  if ((int)uVar1 == 0) {
    *param_2 = local_30;
    *param_3 = local_2c;
    *param_4 = local_28;
  }
  return;
}



/* ========================================================================
   ENTRY: 180018c50
   NAME : FUN_180018c50
   SIG  : undefined8 __fastcall FUN_180018c50(undefined8 * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_180018c50(undefined8 *param_1)

{
  int iVar1;
  undefined8 uVar2;
  undefined1 auStackY_d8 [32];
  undefined1 local_a8 [12];
  undefined4 local_9c;
  undefined1 local_88 [92];
  undefined4 local_2c;
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStackY_d8;
  if ((param_1 == (undefined8 *)0x0) || (param_1[0x47] == 0)) {
    uVar2 = 0xffffd8fe;
  }
  else {
    FUN_180015970((longlong *)param_1[0x44]);
    iVar1 = (*DAT_18002bb88)(*(undefined8 *)param_1[0x44],param_1[0x47],0xc0000000,param_1);
    if (iVar1 == 0) {
      if (*(int *)((undefined8 *)param_1[0x44] + 0x83) == 1) {
        iVar1 = FUN_18001b7a0((HANDLE)*param_1,(undefined8 *)&DAT_180024930,3,local_a8,0x18);
        if (iVar1 == 0) {
          *(undefined4 *)((longlong)param_1 + 0x274) = local_9c;
        }
        else {
          iVar1 = FUN_18001b7a0((HANDLE)*param_1,(undefined8 *)&DAT_180024930,6,local_88,0x70);
          if (iVar1 == 0) {
            *(undefined4 *)((longlong)param_1 + 0x274) = local_2c;
            return 0;
          }
        }
      }
      uVar2 = 0;
    }
    else {
      FUN_180015920((undefined8 *)param_1[0x44]);
      *param_1 = 0;
      if (iVar1 == 0x16) {
        uVar2 = 0xffffd8ff;
      }
      else {
        uVar2 = 0xffffd8f6;
        if (iVar1 != 0x57) {
          uVar2 = 0xffffd8f4;
        }
      }
    }
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180018da0
   NAME : FUN_180018da0
   SIG  : undefined8 __fastcall FUN_180018da0(longlong param_1, ushort * param_2)
   ======================================================================== */

undefined8 FUN_180018da0(longlong param_1,ushort *param_2)

{
  ushort uVar1;
  uint uVar2;
  longlong lVar3;
  longlong lVar4;
  longlong lVar5;
  longlong lVar6;
  bool bVar7;
  undefined7 extraout_var;
  undefined7 extraout_var_00;
  uint *puVar8;
  undefined8 uVar9;
  uint uVar10;
  longlong lVar11;
  ushort *puVar12;
  longlong lVar13;
  undefined8 local_38;
  longlong lStack_30;
  
  lVar6 = DAT_180024988;
  lVar5 = DAT_180024978;
  lVar4 = DAT_180024970;
  lVar3 = DAT_180024898;
  local_38 = CONCAT44(0x100000,(uint)*param_2);
  lStack_30 = 0x719b3800aa000080;
  uVar9 = 0xffffd8f4;
  puVar12 = (ushort *)0x0;
  if (*param_2 == 0xfffe) {
    puVar12 = param_2;
  }
  if (puVar12 != (ushort *)0x0) {
    local_38 = *(longlong *)(puVar12 + 0xc);
    lStack_30 = *(longlong *)(puVar12 + 0x10);
  }
  uVar2 = *(uint *)(*(longlong *)(param_1 + 0x260) + 4);
  if (uVar2 != 0) {
    puVar8 = *(uint **)(param_1 + 600);
    uVar10 = 0;
    lVar11 = DAT_180024948;
    lVar13 = DAT_180024940;
    do {
      if (((((*(longlong *)(puVar8 + 4) == lVar4) && (*(longlong *)(puVar8 + 6) == lVar5)) ||
           ((*(longlong *)(puVar8 + 4) == lVar13 && (*(longlong *)(puVar8 + 6) == lVar11)))) &&
          ((((*(longlong *)(puVar8 + 8) == lVar13 && (*(longlong *)(puVar8 + 10) == lVar11)) ||
            ((*(longlong *)(puVar8 + 8) == DAT_180024890 && (*(longlong *)(puVar8 + 10) == lVar3))))
           || ((*(longlong *)(puVar8 + 8) == local_38 && (*(longlong *)(puVar8 + 10) == lStack_30)))
           ))) && (((*(longlong *)(puVar8 + 0xc) == lVar13 &&
                    (*(longlong *)(puVar8 + 0xe) == lVar11)) ||
                   ((*(longlong *)(puVar8 + 0xc) == DAT_180024980 &&
                    (*(longlong *)(puVar8 + 0xe) == lVar6)))))) {
        if ((puVar8[0x10] == 0xffffffff) || ((uint)param_2[1] <= puVar8[0x10])) {
          if (puVar12 == (ushort *)0x0) {
            uVar1 = param_2[7];
          }
          else {
            uVar1 = puVar12[9];
          }
          bVar7 = FUN_180015fe0((longlong)puVar8,(uint)uVar1);
          if ((int)CONCAT71(extraout_var,bVar7) == 0) {
            uVar9 = 0xffffd8f6;
          }
          else {
            bVar7 = FUN_1800163c0((longlong)puVar8,*(int *)(param_2 + 2));
            if ((int)CONCAT71(extraout_var_00,bVar7) != 0) {
              return 0;
            }
            uVar9 = 0xffffd8f3;
          }
        }
        else {
          uVar9 = 0xffffd8f2;
        }
      }
      uVar10 = uVar10 + 1;
      puVar8 = (uint *)((longlong)puVar8 + (ulonglong)*puVar8);
    } while (uVar10 < uVar2);
    return uVar9;
  }
  return 0xffffd8f4;
}



/* ========================================================================
   ENTRY: 180018f60
   NAME : FUN_180018f60
   SIG  : longlong * __fastcall FUN_180018f60(undefined8 * param_1, int param_2, int * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

longlong * FUN_180018f60(undefined8 *param_1,int param_2,int *param_3)

{
  wchar_t *_Str;
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  short sVar4;
  longlong lVar5;
  longlong lVar6;
  longlong lVar7;
  undefined8 uVar8;
  bool bVar9;
  int iVar10;
  int iVar11;
  longlong *plVar12;
  longlong *plVar13;
  ulonglong uVar14;
  undefined7 extraout_var;
  undefined7 extraout_var_00;
  undefined7 extraout_var_01;
  undefined7 extraout_var_02;
  longlong lVar15;
  size_t sVar16;
  LPVOID pvVar17;
  longlong *plVar18;
  longlong lVar19;
  uint *puVar20;
  uint uVar21;
  undefined8 **ppuVar22;
  uint uVar23;
  longlong lVar24;
  undefined *puVar25;
  undefined8 uVar26;
  undefined *puVar27;
  longlong lVar28;
  longlong lVar29;
  longlong lVar30;
  short sVar31;
  uint uVar32;
  undefined1 auStackY_2f8 [32];
  int local_2b8;
  DWORD local_2b4;
  uint local_2b0 [2];
  longlong local_2a8;
  undefined8 *local_2a0;
  wchar_t local_298 [4];
  int local_290;
  undefined8 *local_288;
  wchar_t local_280 [2];
  wchar_t local_27c;
  int *local_278;
  short local_270 [12];
  wchar_t local_258;
  short local_256;
  ulonglong local_48;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStackY_2f8;
  puVar27 = (undefined *)0x0;
  iVar10 = *(int *)(param_1 + 0x83);
  local_2a8 = 0;
  local_2a0 = param_1;
  local_290 = param_2;
  local_278 = param_3;
  plVar12 = (longlong *)FUN_180020230(0x2b8);
  if (plVar12 != (longlong *)0x0) {
    plVar12[0x44] = (longlong)param_1;
    *(int *)((longlong)plVar12 + 0x22c) = param_2;
    *(undefined4 *)(plVar12 + 0x48) = 0x9a;
    plVar13 = (longlong *)FUN_180020230(0x9a);
    plVar12[0x47] = (longlong)plVar13;
    lVar15 = DAT_180024908;
    lVar24 = DAT_180024898;
    lVar19 = DAT_180024890;
    if (plVar13 != (longlong *)0x0) {
      *plVar13 = DAT_180024900;
      plVar13[1] = lVar15;
      lVar29 = DAT_180024918;
      lVar28 = DAT_180024910;
      *(uint *)(plVar12[0x47] + 0x10) = (uint)(iVar10 == 2);
      *(undefined4 *)(plVar12[0x47] + 0x14) = 0;
      lVar15 = plVar12[0x47];
      *(longlong *)(lVar15 + 0x18) = lVar28;
      *(longlong *)(lVar15 + 0x20) = lVar29;
      lVar29 = DAT_180024978;
      lVar28 = DAT_180024970;
      *(undefined4 *)(plVar12[0x47] + 0x28) = 0;
      *(undefined4 *)(plVar12[0x47] + 0x2c) = 0;
      *(int *)(plVar12[0x47] + 0x30) = param_2;
      *(undefined8 *)(plVar12[0x47] + 0x38) = 0;
      *(undefined4 *)(plVar12[0x47] + 0x40) = 0x40000000;
      *(undefined4 *)(plVar12[0x47] + 0x44) = 1;
      plVar12[0x49] = plVar12[0x47] + 0x48;
      *(undefined4 *)(plVar12[0x47] + 0x48) = 0x52;
      *(undefined4 *)(plVar12[0x49] + 4) = 0;
      *(undefined4 *)(plVar12[0x49] + 0xc) = 0;
      lVar15 = plVar12[0x49];
      *(longlong *)(lVar15 + 0x10) = lVar28;
      *(longlong *)(lVar15 + 0x18) = lVar29;
      uVar8 = DAT_180024988;
      uVar26 = DAT_180024980;
      lVar15 = plVar12[0x49];
      *(longlong *)(lVar15 + 0x20) = lVar19;
      *(longlong *)(lVar15 + 0x28) = lVar24;
      lVar19 = plVar12[0x49];
      *(undefined8 *)(lVar19 + 0x30) = uVar26;
      *(undefined8 *)(lVar19 + 0x38) = uVar8;
      *(undefined4 *)((longlong)plVar12 + 0x274) = 0;
      local_2b8 = FUN_18001b650((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,7,
                                plVar12 + 0x4a,4,(LPDWORD)0x0);
      if (local_2b8 == 0) {
        if (((int)plVar12[0x4a] - 1U & 0xfffffffd) == 0) {
          local_2b8 = FUN_18001b650((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,2,
                                    plVar12 + 0x4d,4,(LPDWORD)0x0);
          if (local_2b8 == 0) {
            uVar14 = FUN_18001b570((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,5,
                                   &local_2a8);
            local_2b8 = (int)uVar14;
            if (local_2b8 == 0) {
              local_2b8 = -9999;
              puVar25 = puVar27;
              if (*(uint *)(local_2a8 + 4) != 0) {
                do {
                  if (((*(longlong *)(local_2a8 + 8 + (longlong)puVar25 * 0x18) == DAT_180024900) &&
                      (*(longlong *)(local_2a8 + 0x10 + (longlong)puVar25 * 0x18) == DAT_180024908))
                     && (*(uint *)(local_2a8 + 0x18 + (longlong)puVar25 * 0x18) ==
                         (uint)(iVar10 == 2))) {
                    local_2b8 = 0;
                    FUN_180020240(local_2a8);
                    local_2a8 = 0;
                    uVar14 = FUN_18001b570((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,6,
                                           &local_2a8);
                    local_2b8 = (int)uVar14;
                    if (local_2b8 == 0) {
                      local_2b8 = -9999;
                      puVar25 = puVar27;
                      if (*(uint *)(local_2a8 + 4) != 0) goto LAB_180019270;
                    }
                    break;
                  }
                  uVar21 = (int)puVar25 + 1;
                  puVar25 = (undefined *)(ulonglong)uVar21;
                } while (uVar21 < *(uint *)(local_2a8 + 4));
              }
            }
          }
        }
        else {
          local_2b8 = -0x270c;
        }
      }
      goto LAB_180019bae;
    }
  }
  goto LAB_180019ba6;
LAB_18001928b:
  uVar21 = (int)puVar25 + 1;
  puVar25 = (undefined *)(ulonglong)uVar21;
  if (*(uint *)(local_2a8 + 4) <= uVar21) goto LAB_180019bae;
LAB_180019270:
  if (((*(longlong *)(local_2a8 + 8 + (longlong)puVar25 * 0x18) != DAT_180024910) ||
      (*(longlong *)(local_2a8 + 0x10 + (longlong)puVar25 * 0x18) != DAT_180024918)) ||
     (*(int *)(local_2a8 + 0x18 + (longlong)puVar25 * 0x18) != 0)) goto LAB_18001928b;
  local_2b8 = 0;
  FUN_180020240(local_2a8);
  plVar13 = plVar12 + 0x4c;
  uVar26 = 3;
  puVar25 = &DAT_180024920;
  local_2a8 = 0;
  uVar14 = FUN_18001b570((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,3,plVar13);
  local_2b8 = (int)uVar14;
  if (local_2b8 != 0) goto LAB_180019bae;
  uVar21 = 0xd;
  local_2b0[0] = 0xd;
  plVar12[0x4b] = *plVar13 + 8;
  local_2b8 = -9999;
  plVar12[0x4f] = 0;
  *(undefined4 *)(plVar12 + 0x50) = 0;
  if (*(int *)(*plVar13 + 4) != 0) {
    puVar20 = (uint *)plVar12[0x4b];
    local_2b4 = 0;
    lVar19 = DAT_180024970;
    lVar24 = DAT_180024978;
    lVar15 = DAT_1800248a0;
    lVar28 = DAT_1800248a8;
    lVar29 = DAT_1800248b2;
    lVar30 = DAT_180024948;
    iVar10 = DAT_1800248ba;
    sVar31 = DAT_1800248be;
    do {
      if ((((lVar29 == *(longlong *)((longlong)puVar20 + 0x22)) &&
           (iVar10 == *(int *)((longlong)puVar20 + 0x2a))) &&
          (sVar31 == *(short *)((longlong)puVar20 + 0x2e))) ||
         (((*(longlong *)(puVar20 + 8) == DAT_180024890 &&
           (*(longlong *)(puVar20 + 10) == DAT_180024898)) ||
          ((((*(longlong *)(puVar20 + 8) == lVar15 && (*(longlong *)(puVar20 + 10) == lVar28)) ||
            ((*(longlong *)(puVar20 + 8) == DAT_180024940 && (*(longlong *)(puVar20 + 10) == lVar30)
             ))) || ((uVar23 = (uint)puVar27, *(longlong *)(puVar20 + 4) == lVar19 &&
                     (*(longlong *)(puVar20 + 6) == lVar24)))))))) {
        local_2b8 = 0;
        uVar21 = puVar20[0x10];
        if (uVar21 == 0xffffffff) {
          *(undefined4 *)(plVar12 + 0x4f) = 0x100;
          lVar19 = DAT_180024970;
          lVar24 = DAT_180024978;
          lVar15 = DAT_1800248a0;
          lVar28 = DAT_1800248a8;
          lVar29 = DAT_1800248b2;
          lVar30 = DAT_180024948;
          iVar10 = DAT_1800248ba;
          sVar31 = DAT_1800248be;
        }
        else if ((int)plVar12[0x4f] < (int)uVar21) {
          *(uint *)(plVar12 + 0x4f) = uVar21;
          lVar19 = DAT_180024970;
          lVar24 = DAT_180024978;
          lVar15 = DAT_1800248a0;
          lVar28 = DAT_1800248a8;
          lVar29 = DAT_1800248b2;
          lVar30 = DAT_180024948;
          iVar10 = DAT_1800248ba;
          sVar31 = DAT_1800248be;
        }
        bVar9 = FUN_180015fe0((longlong)puVar20,8);
        if ((int)CONCAT71(extraout_var,bVar9) != 0) {
          *(uint *)((longlong)plVar12 + 0x27c) = *(uint *)((longlong)plVar12 + 0x27c) | 0x20;
          lVar19 = DAT_180024970;
          lVar24 = DAT_180024978;
          lVar15 = DAT_1800248a0;
          lVar28 = DAT_1800248a8;
          lVar29 = DAT_1800248b2;
          lVar30 = DAT_180024948;
          iVar10 = DAT_1800248ba;
          sVar31 = DAT_1800248be;
        }
        uVar21 = 0x10;
        bVar9 = FUN_180015fe0((longlong)puVar20,0x10);
        if ((int)CONCAT71(extraout_var_00,bVar9) != 0) {
          *(uint *)((longlong)plVar12 + 0x27c) = *(uint *)((longlong)plVar12 + 0x27c) | uVar21;
          lVar19 = DAT_180024970;
          lVar24 = DAT_180024978;
          lVar15 = DAT_1800248a0;
          lVar28 = DAT_1800248a8;
          lVar29 = DAT_1800248b2;
          lVar30 = DAT_180024948;
          iVar10 = DAT_1800248ba;
          sVar31 = DAT_1800248be;
        }
        bVar9 = FUN_180015fe0((longlong)puVar20,0x18);
        if ((int)CONCAT71(extraout_var_01,bVar9) != 0) {
          *(uint *)((longlong)plVar12 + 0x27c) = *(uint *)((longlong)plVar12 + 0x27c) | 8;
          lVar19 = DAT_180024970;
          lVar24 = DAT_180024978;
          lVar15 = DAT_1800248a0;
          lVar28 = DAT_1800248a8;
          lVar29 = DAT_1800248b2;
          lVar30 = DAT_180024948;
          iVar10 = DAT_1800248ba;
          sVar31 = DAT_1800248be;
        }
        bVar9 = FUN_180015fe0((longlong)puVar20,0x20);
        lVar7 = DAT_180024978;
        lVar6 = DAT_180024970;
        lVar5 = DAT_180024948;
        sVar4 = DAT_1800248be;
        iVar11 = DAT_1800248ba;
        lVar3 = DAT_1800248b2;
        lVar2 = DAT_1800248a8;
        lVar1 = DAT_1800248a0;
        if ((int)CONCAT71(extraout_var_02,bVar9) != 0) {
          lVar15 = *(longlong *)(puVar20 + 8) - lVar15;
          if (lVar15 == 0) {
            lVar15 = *(longlong *)(puVar20 + 10) - lVar28;
          }
          if (lVar15 == 0) {
            uVar21 = *(uint *)((longlong)plVar12 + 0x27c) | 2;
          }
          else {
            uVar21 = *(uint *)((longlong)plVar12 + 0x27c) | 4;
          }
          *(uint *)((longlong)plVar12 + 0x27c) = uVar21;
          lVar19 = lVar6;
          lVar24 = lVar7;
          lVar15 = lVar1;
          lVar28 = lVar2;
          lVar29 = lVar3;
          lVar30 = lVar5;
          iVar10 = iVar11;
          sVar31 = sVar4;
        }
        uVar14 = FUN_180015260((longlong)puVar20);
        uVar32 = (uint)uVar14;
        uVar21 = local_2b0[0];
        uVar23 = local_2b4;
        if ((-1 < (int)uVar32) && ((int)uVar32 < (int)local_2b0[0])) {
          uVar21 = uVar32;
          local_2b0[0] = uVar32;
        }
      }
      local_2b4 = uVar23 + 1;
      puVar27 = (undefined *)(ulonglong)local_2b4;
      puVar20 = (uint *)((longlong)puVar20 + (ulonglong)*puVar20);
      puVar25 = puVar27;
      param_1 = local_2a0;
      param_2 = local_290;
    } while (local_2b4 < *(uint *)(plVar12[0x4c] + 4));
  }
  plVar13 = plVar12 + 0x4d;
  if (local_2b8 != 0) goto LAB_180019bae;
  if (uVar21 == 0xd) {
    FUN_180018400(0xffffd8f1,"PinNew: No default sample rate found",puVar25,uVar26);
    local_2b8 = -9999;
    goto LAB_180019bae;
  }
  *(undefined4 *)(plVar12 + 0x50) = (&DAT_180025310)[(int)uVar21];
  local_2b8 = FUN_18001b650((HANDLE)*param_1,param_2,(undefined8 *)&DAT_180024920,0,
                            (LPVOID)((longlong)plVar12 + 0x26c),8,(LPDWORD)0x0);
  if (local_2b8 != 0) goto LAB_180019bae;
  if (*(int *)(param_1 + 0x83) == 2) {
    local_2b4 = 0;
    iVar10 = FUN_180019dd0(plVar12,&local_2b4);
    if (iVar10 == 0) {
      *(uint *)(plVar12 + 0x45) = 2 - (uint)(local_2b4 != 0);
    }
  }
  iVar10 = FUN_180015a70(param_2,(uint)((int)*plVar13 == 1),(longlong)param_1,-1,(uint *)0x0,
                         (uint *)0x0);
  local_298[0] = u_Input_180025468[4];
  local_298[1] = u_Input_180025468[5];
  local_280[0] = u_Output_180025478[4];
  local_280[1] = u_Output_180025478[5];
  local_27c = u_Output_180025478[6];
  local_2a0 = (undefined8 *)u_Input_180025468._0_8_;
  local_288 = (undefined8 *)u_Output_180025478._0_8_;
  if (iVar10 == -1) {
    ppuVar22 = &local_288;
    if ((int)*plVar13 != 1) {
      ppuVar22 = &local_2a0;
    }
    wcscpy((wchar_t *)((longlong)plVar12 + 0x14),(wchar_t *)ppuVar22);
    goto LAB_1800199f8;
  }
  local_2b4 = 0;
  local_2b8 = FUN_18001b650((HANDLE)*param_1,iVar10,(undefined8 *)&DAT_180024920,10,(LPVOID)0x0,0,
                            &local_2b4);
  if (local_2b8 != 0) {
    _Str = (wchar_t *)((longlong)plVar12 + 0x14);
    local_2b8 = FUN_18001b650((HANDLE)*param_1,iVar10,(undefined8 *)&DAT_180024920,0xc,_Str,0x104,
                              (LPDWORD)0x0);
    if (local_2b8 != 0) {
      local_270[0] = 0;
      local_270[1] = 0;
      local_270[2] = 0;
      local_270[3] = 0;
      local_270[4] = 0;
      local_270[5] = 0;
      local_270[6] = 0;
      local_270[7] = 0;
      local_2b8 = FUN_18001b650((HANDLE)*param_1,iVar10,(undefined8 *)&DAT_180024920,0xb,local_270,
                                0x10,(LPDWORD)0x0);
      if (local_2b8 == 0) {
        uVar26 = FUN_180015c90(local_270,(uint)((int)*plVar13 == 2),(longlong)_Str,0x104);
        local_2b8 = (int)uVar26;
      }
    }
    sVar16 = wcslen(_Str);
    if (sVar16 == 0) {
      ppuVar22 = &local_288;
      if ((int)*plVar13 != 1) {
        ppuVar22 = &local_2a0;
      }
      wcscpy(_Str,(wchar_t *)ppuVar22);
    }
    if ((int)*plVar13 == 1) {
      iVar10 = param_2;
    }
    *(int *)(plVar12 + 0x46) = iVar10;
    goto LAB_1800199f8;
  }
  pvVar17 = (LPVOID)FUN_180020230(local_2b4 + 2);
  if (pvVar17 != (LPVOID)0x0) {
    local_2b8 = FUN_18001b650((HANDLE)*param_1,iVar10,(undefined8 *)&DAT_180024920,10,pvVar17,
                              local_2b4,(LPDWORD)0x0);
    iVar10 = *(int *)((longlong)pvVar17 + 4);
    wcsncpy(&local_258,(wchar_t *)((longlong)pvVar17 + 8),0x104);
    FUN_180020240((longlong)pvVar17);
    if (local_2b8 != 0) goto LAB_180019bae;
    if (local_256 == 0x3f) {
      local_256 = 0x5c;
    }
    lVar19 = plVar12[0x44];
    if (*(longlong *)(lVar19 + 0x440) == 0) {
      puVar27 = &DAT_180025488;
      plVar18 = FUN_180015770(0,0,&local_258,L"",&local_2b8);
      *(longlong **)(lVar19 + 0x440) = plVar18;
      if (*(longlong *)(plVar12[0x44] + 0x440) == 0) {
        local_2b8 = -9999;
        FUN_180018400(0xffffd8f1,"Failed to create topology filter \'%S\'",&local_258,puVar27);
        goto LAB_180019bae;
      }
      wcsncpy((wchar_t *)(plVar12[0x44] + 0x210),&local_258,0x104);
    }
    uVar26 = FUN_180015970(*(longlong **)(plVar12[0x44] + 0x440));
    local_2b8 = (int)uVar26;
    if (local_2b8 != 0) goto LAB_1800199f8;
    if ((int)*plVar13 == 1) {
      uVar26 = 0xffffffff;
      local_270[0] = 0;
      local_270[1] = 0;
      local_270[2] = 0;
      local_270[3] = 0;
      local_270[4] = 0;
      local_270[5] = 0;
      local_270[6] = 0;
      local_270[7] = 0;
      lVar19 = *(longlong *)(plVar12[0x44] + 0x440);
      iVar11 = FUN_180015a70(iVar10,1,lVar19,-1,(uint *)0x0,(uint *)0x0);
      if (iVar11 == -1) {
        local_2b8 = -9999;
        FUN_180018400(0xffffd8f1,"Failed to get endpoint pin ID on topology filter!",lVar19,uVar26);
        goto LAB_180019bae;
      }
      local_2b8 = FUN_18001b650((HANDLE)**(undefined8 **)(plVar12[0x44] + 0x440),iVar11,
                                (undefined8 *)&DAT_180024920,0xb,local_270,0x10,(LPDWORD)0x0);
      if (local_2b8 == 0) {
        uVar26 = FUN_180015c90(local_270,(uint)((int)*plVar13 == 2),(longlong)plVar12 + 0x14,0x104);
        local_2b8 = (int)uVar26;
      }
      sVar16 = wcslen((wchar_t *)((longlong)plVar12 + 0x14));
      if (sVar16 == 0) {
        wcscpy((wchar_t *)((longlong)plVar12 + 0x14),(wchar_t *)&local_288);
      }
      *(int *)(plVar12 + 0x46) = iVar10;
      goto LAB_1800199f8;
    }
    uVar32 = 0;
    uVar23 = 0;
    uVar21 = uVar32;
    while( true ) {
      local_2b0[0] = 0xffffffff;
      iVar11 = FUN_180015a70(iVar10,0,*(longlong *)(plVar12[0x44] + 0x440),uVar21,(uint *)0x0,
                             local_2b0);
      if (iVar11 == -1) break;
      local_270[0] = 0;
      local_270[1] = 0;
      local_270[2] = 0;
      local_270[3] = 0;
      local_270[4] = 0;
      local_270[5] = 0;
      local_270[6] = 0;
      local_270[7] = 0;
      local_2b8 = FUN_18001b650((HANDLE)**(undefined8 **)(plVar12[0x44] + 0x440),iVar11,
                                (undefined8 *)&DAT_180024920,0xb,local_270,0x10,(LPDWORD)0x0);
      if (local_2b8 == 0) {
        if (local_2b0[0] == 0xffffffff) {
          local_2b8 = FUN_18001b650((HANDLE)**(undefined8 **)(plVar12[0x44] + 0x440),iVar11,
                                    (undefined8 *)&DAT_180024920,0xc,
                                    (LPVOID)((longlong)plVar12 + 0x14),0x104,(LPDWORD)0x0);
          if (local_2b8 != 0) {
            uVar26 = FUN_180015c90(local_270,1,(longlong)plVar12 + 0x14,0x104);
            local_2b8 = (int)uVar26;
          }
          break;
        }
        uVar26 = FUN_180015c90(local_270,1,0,0);
        local_2b8 = (int)uVar26;
        uVar23 = uVar23 + (local_2b8 == 0);
      }
      uVar21 = uVar21 + 1;
      if (0x3f < uVar21) break;
    }
    if (uVar23 == 0) {
      *(int *)(plVar12 + 0x46) = iVar11;
      sVar16 = wcslen((wchar_t *)((longlong)plVar12 + 0x14));
      if (sVar16 == 0) {
        wcscpy((wchar_t *)((longlong)plVar12 + 0x14),(wchar_t *)&local_2a0);
      }
      goto LAB_1800199f8;
    }
    lVar19 = FUN_180020230(uVar23 * 8);
    plVar12[1] = lVar19;
    if (lVar19 != 0) {
      *(uint *)(plVar12 + 2) = uVar23;
      uVar21 = uVar32;
      if (uVar23 == 0) goto LAB_1800199f8;
      while( true ) {
        uVar14 = (ulonglong)uVar21;
        if (*(longlong *)(plVar12[1] + uVar14 * 8) == 0) {
          uVar26 = FUN_180020230(0x214);
          *(undefined8 *)(plVar12[1] + uVar14 * 8) = uVar26;
          if (*(longlong *)(plVar12[1] + uVar14 * 8) == 0) goto LAB_180019b93;
        }
        lVar19 = *(longlong *)(plVar12[1] + uVar14 * 8);
        iVar11 = FUN_180015a70(iVar10,0,*(longlong *)(plVar12[0x44] + 0x440),uVar32,
                               (uint *)(lVar19 + 0x208),(uint *)(lVar19 + 0x20c));
        if (iVar11 == -1) break;
        local_270[0] = 0;
        local_270[1] = 0;
        local_270[2] = 0;
        local_270[3] = 0;
        local_270[4] = 0;
        local_270[5] = 0;
        local_270[6] = 0;
        local_270[7] = 0;
        *(int *)(*(longlong *)(plVar12[1] + uVar14 * 8) + 0x210) = iVar11;
        local_2b8 = FUN_18001b650((HANDLE)**(undefined8 **)(plVar12[0x44] + 0x440),iVar11,
                                  (undefined8 *)&DAT_180024920,0xb,local_270,0x10,(LPDWORD)0x0);
        if (local_2b8 == 0) {
          local_2b8 = FUN_18001b650((HANDLE)**(undefined8 **)(plVar12[0x44] + 0x440),iVar11,
                                    (undefined8 *)&DAT_180024920,0xc,
                                    *(LPVOID *)(plVar12[1] + uVar14 * 8),0x104,(LPDWORD)0x0);
          if (local_2b8 != 0) {
            uVar26 = FUN_180015c90(local_270,1,*(longlong *)(plVar12[1] + uVar14 * 8),0x104);
            local_2b8 = (int)uVar26;
            if (local_2b8 != 0) {
              wcscpy(*(wchar_t **)(plVar12[1] + uVar14 * 8),(wchar_t *)&local_2a0);
            }
          }
          uVar21 = uVar21 + 1;
        }
        uVar32 = uVar32 + 1;
        if (uVar23 <= uVar21) {
LAB_1800199f8:
          plVar13 = *(longlong **)(plVar12[0x44] + 0x440);
          if ((plVar13 != (longlong *)0x0) && (*plVar13 != 0)) {
            FUN_180015920(plVar13);
          }
          *local_278 = 0;
          return plVar12;
        }
      }
      goto LAB_180019bae;
    }
LAB_180019b93:
    FUN_180015920(*(undefined8 **)(plVar12[0x44] + 0x440));
  }
LAB_180019ba6:
  local_2b8 = -0x2708;
LAB_180019bae:
  plVar13 = *(longlong **)(plVar12[0x44] + 0x440);
  if ((plVar13 != (longlong *)0x0) && (*plVar13 != 0)) {
    FUN_180015920(plVar13);
  }
  FUN_180020240(local_2a8);
  FUN_1800187c0(plVar12);
  *local_278 = local_2b8;
  return (longlong *)0x0;
}



/* ========================================================================
   ENTRY: 180019dd0
   NAME : FUN_180019dd0
   SIG  : undefined __fastcall FUN_180019dd0(undefined8 * param_1, LPVOID param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180019dd0(undefined8 *param_1,LPVOID param_2)

{
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  
  local_28 = _DAT_180024990;
  uStack_20 = _UNK_180024998;
  local_18 = 8;
  local_14 = 1;
  FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_28,0x18,param_2,4,(LPDWORD)0x0);
  return;
}



/* ========================================================================
   ENTRY: 180019e20
   NAME : FUN_180019e20
   SIG  : undefined4 __fastcall FUN_180019e20(HANDLE param_1, DWORD * param_2)
   ======================================================================== */

undefined4 FUN_180019e20(HANDLE param_1,DWORD *param_2)

{
  BOOL BVar1;
  DWORD DVar2;
  undefined4 uVar3;
  DWORD local_res10 [6];
  
  local_res10[0] = 0;
  BVar1 = DeviceIoControl(param_1,0x2f4017,(LPVOID)0x0,0,param_2,*param_2,local_res10,
                          (LPOVERLAPPED)(param_2 + 0xe));
  if (BVar1 == 0) {
    DVar2 = GetLastError();
    uVar3 = 0xffffd8fe;
    if (DVar2 == 0x3e5) {
      uVar3 = 0;
    }
    return uVar3;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180019e90
   NAME : FUN_180019e90
   SIG  : undefined __fastcall FUN_180019e90(undefined8 * param_1, undefined8 param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180019e90(undefined8 *param_1,undefined8 param_2)

{
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  undefined8 local_10;
  
  local_18 = 6;
  local_14 = 2;
  local_28 = _DAT_180024990;
  uStack_20 = _UNK_180024998;
  local_10 = param_2;
  FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_28,0x20,&local_28,0x20,(LPDWORD)0x0);
  return;
}



/* ========================================================================
   ENTRY: 180019ef0
   NAME : FUN_180019ef0
   SIG  : undefined __fastcall FUN_180019ef0(undefined8 * param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180019ef0(undefined8 *param_1)

{
  undefined8 uVar1;
  undefined8 local_58;
  undefined8 uStack_50;
  undefined4 local_48;
  undefined4 local_44;
  undefined8 local_40;
  undefined8 local_38 [6];
  
  local_48 = 3;
  local_40 = 0;
  local_58 = _DAT_180024990;
  uStack_50 = _UNK_180024998;
  local_44 = 2;
  uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_58,0x20,local_38,0x28,(LPDWORD)0x0)
  ;
  if ((int)uVar1 == 0) {
    param_1[0x51] = local_38[0];
  }
  return;
}



/* ========================================================================
   ENTRY: 180019f70
   NAME : FUN_180019f70
   SIG  : undefined8 __fastcall FUN_180019f70(longlong param_1, short * param_2)
   ======================================================================== */

undefined8 FUN_180019f70(longlong param_1,short *param_2)

{
  int iVar1;
  uint uVar2;
  uint uVar3;
  void *_Dst;
  
  if ((param_1 != 0) && (param_2 != (short *)0x0)) {
    iVar1 = FUN_180015ed0(param_2);
    uVar3 = iVar1 + 0x88;
    if (*(uint *)(param_1 + 0x240) != uVar3) {
      _Dst = (void *)FUN_180020230(uVar3);
      if (_Dst == (void *)0x0) {
        return 0xffffd8f8;
      }
      uVar2 = *(uint *)(param_1 + 0x240);
      if (uVar3 <= *(uint *)(param_1 + 0x240)) {
        uVar2 = uVar3;
      }
      memcpy(_Dst,*(void **)(param_1 + 0x238),(ulonglong)uVar2);
      FUN_180020240(*(longlong *)(param_1 + 0x238));
      *(void **)(param_1 + 0x238) = _Dst;
      *(int **)(param_1 + 0x248) = (int *)((longlong)_Dst + 0x48);
      *(uint *)(param_1 + 0x240) = uVar3;
      *(int *)((longlong)_Dst + 0x48) = iVar1 + 0x40;
    }
    uVar3 = FUN_180015ed0(param_2);
    memcpy((void *)(*(longlong *)(param_1 + 0x248) + 0x40),param_2,(ulonglong)uVar3);
    *(uint *)(*(longlong *)(param_1 + 0x248) + 8) =
         (uint)(ushort)(((ushort)param_2[7] >> 3) * param_2[1]);
    return 0;
  }
  return 0xffffd8fe;
}



/* ========================================================================
   ENTRY: 18001a090
   NAME : FUN_18001a090
   SIG  : undefined8 __fastcall FUN_18001a090(undefined8 * param_1, undefined4 param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18001a090(undefined8 *param_1,undefined4 param_2)

{
  undefined8 uVar1;
  undefined4 local_res10 [6];
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  
  local_14 = 2;
  local_18 = 0;
  local_28 = _DAT_180024930;
  uStack_20 = _UNK_180024938;
  if ((param_1 != (undefined8 *)0x0) && ((HANDLE)*param_1 != (HANDLE)0x0)) {
    local_res10[0] = param_2;
    uVar1 = FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_28,0x18,local_res10,4,
                          (LPDWORD)0x0);
    return uVar1;
  }
  return 0xffffd8fe;
}



/* ========================================================================
   ENTRY: 18001a100
   NAME : FUN_18001a100
   SIG  : undefined __fastcall FUN_18001a100(undefined8 * param_1, longlong param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18001a100(undefined8 *param_1,longlong param_2)

{
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  longlong local_10;
  
  if (param_2 != 0) {
    local_28 = _DAT_180024990;
    uStack_20 = _UNK_180024998;
    local_18 = 7;
    local_14 = 2;
    local_10 = param_2;
    FUN_18001b850((HANDLE)*param_1,0x2f0003,(uint *)&local_28,0x20,&local_28,0x20,(LPDWORD)0x0);
  }
  return;
}



/* ========================================================================
   ENTRY: 18001a160
   NAME : FUN_18001a160
   SIG  : undefined4 __fastcall FUN_18001a160(HANDLE param_1, DWORD * param_2)
   ======================================================================== */

undefined4 FUN_18001a160(HANDLE param_1,DWORD *param_2)

{
  BOOL BVar1;
  DWORD DVar2;
  undefined4 uVar3;
  DWORD local_res10 [6];
  
  local_res10[0] = 0;
  BVar1 = DeviceIoControl(param_1,0x2f8013,(LPVOID)0x0,0,param_2,*param_2,local_res10,
                          (LPOVERLAPPED)(param_2 + 0xe));
  if (BVar1 == 0) {
    DVar2 = GetLastError();
    uVar3 = 0xffffd8fe;
    if (DVar2 == 0x3e5) {
      uVar3 = 0;
    }
    return uVar3;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001a1d0
   NAME : FUN_18001a1d0
   SIG  : ulonglong __fastcall FUN_18001a1d0(undefined8 * param_1)
   ======================================================================== */

ulonglong FUN_18001a1d0(undefined8 *param_1)

{
  ulonglong uVar1;
  
  uVar1 = FUN_18001a090(param_1,1);
  if ((int)uVar1 == 0) {
    uVar1 = FUN_18001a090(param_1,2);
    if ((int)uVar1 == 0) {
      return uVar1;
    }
  }
  FUN_18001a090(param_1,0);
  return uVar1 & 0xffffffff;
}



/* ========================================================================
   ENTRY: 18001a230
   NAME : FUN_18001a230
   SIG  : ulonglong __fastcall FUN_18001a230(longlong * param_1)
   ======================================================================== */

ulonglong FUN_18001a230(longlong *param_1)

{
  int iVar1;
  ulonglong uVar2;
  undefined4 extraout_var;
  ulonglong uVar3;
  longlong lVar4;
  uint uVar5;
  
  uVar3 = 0;
  if (*(undefined8 **)(*param_1 + 0x1a8) != (undefined8 *)0x0) {
    uVar2 = FUN_18001a1d0(*(undefined8 **)(*param_1 + 0x1a8));
    uVar3 = uVar2 & 0xffffffff;
    if ((int)uVar2 != 0) {
      return uVar2;
    }
    lVar4 = *param_1;
    if (*(int *)(*(longlong *)(*(longlong *)(lVar4 + 0x1a8) + 0x220) + 0x418) == 1) {
      if (*(int *)(lVar4 + 0x1c8) != 0) {
        uVar3 = 0;
        do {
          iVar1 = FUN_180019e20((HANDLE)**(undefined8 **)(lVar4 + 0x1a8),
                                (DWORD *)(uVar3 * 0x58 + *(longlong *)(lVar4 + 0x1d8)));
          if (iVar1 != 0) {
            return CONCAT44(extraout_var,iVar1);
          }
          uVar5 = (int)uVar3 + 1;
          uVar3 = (ulonglong)uVar5;
          *(int *)(param_1 + 5) = (int)param_1[5] + 1;
          lVar4 = *param_1;
        } while (uVar5 < *(uint *)(lVar4 + 0x1c8));
        uVar3 = 0;
      }
    }
    else {
      *(undefined4 *)(param_1 + 5) = 2;
    }
  }
  if (*(undefined8 **)(*param_1 + 0x1e8) != (undefined8 *)0x0) {
    uVar3 = FUN_18001a1d0(*(undefined8 **)(*param_1 + 0x1e8));
    uVar2 = uVar3 & 0xffffffff;
    if ((int)uVar3 == 0) {
      *(int *)((longlong)param_1 + 0x2c) =
           *(int *)((longlong)param_1 + 0x2c) + *(int *)(*param_1 + 0x208);
      *(int *)(param_1 + 5) = (int)param_1[5] + 1;
      SetEvent((HANDLE)**(undefined8 **)(*param_1 + 0x210));
      lVar4 = *param_1;
      uVar3 = uVar2;
      if ((*(int *)(*(longlong *)(*(longlong *)(lVar4 + 0x1e8) + 0x220) + 0x418) == 1) &&
         (1 < *(uint *)(lVar4 + 0x208))) {
        uVar2 = 1;
        do {
          SetEvent(*(HANDLE *)(*(longlong *)(lVar4 + 0x210) + uVar2 * 8));
          uVar5 = (int)uVar2 + 1;
          uVar2 = (ulonglong)uVar5;
          *(int *)(param_1 + 5) = (int)param_1[5] + 1;
          lVar4 = *param_1;
        } while (uVar5 < *(uint *)(lVar4 + 0x208));
      }
    }
    return uVar3;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001a3a0
   NAME : FUN_18001a3a0
   SIG  : undefined8 __fastcall FUN_18001a3a0(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001a3a0(longlong param_1)

{
  int iVar1;
  double dVar2;
  uint uVar3;
  uint uVar4;
  BOOL BVar5;
  DWORD DVar6;
  DWORD DVar7;
  DWORD DVar8;
  int iVar9;
  HANDLE *lpHandles;
  ulonglong uVar10;
  ulonglong uVar11;
  uint uVar12;
  uint uVar13;
  uint uVar14;
  double dVar15;
  double dVar16;
  undefined1 auStackY_168 [32];
  DWORD local_134;
  HANDLE local_130;
  longlong local_128;
  longlong local_120;
  longlong lStack_118;
  LARGE_INTEGER local_110;
  longlong local_108;
  undefined8 local_100;
  undefined8 local_f8;
  undefined8 uStack_f0;
  uint local_e8;
  int local_e4;
  int local_e0;
  int local_dc;
  int local_d8;
  uint local_d4;
  int local_cc;
  int local_c8;
  int local_c4;
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_168;
  local_128 = 0;
  local_130 = (HANDLE)0x0;
  local_120 = 0;
  lStack_118 = 0;
  memset(&local_e8,0,0xa8);
  dVar2 = DAT_180023560;
  *(undefined4 *)(param_1 + 0x260) = 0;
  local_100 = 0;
  local_f8 = 0;
  uStack_f0 = 0;
  dVar16 = (double)(uint)(*(int *)(param_1 + 0x1bc) * 2000) / *(double *)(param_1 + 0x48);
  dVar15 = (double)(uint)(*(int *)(param_1 + 0x1fc) * 2000) / *(double *)(param_1 + 0x48);
  if (dVar15 + dVar2 <= dVar16 + dVar2) {
    dVar15 = dVar16;
  }
  uVar3 = (int)(longlong)(dVar15 + dVar2) * 8;
  uVar12 = 100;
  if (100 < uVar3) {
    uVar12 = uVar3;
  }
  local_108 = param_1;
  local_d4 = uVar12;
  lpHandles = (HANDLE *)
              FUN_180020230((*(int *)(param_1 + 0x208) + *(int *)(param_1 + 0x1c8)) * 8 + 8);
  uVar3 = 1;
  uVar10 = 0;
  if (*(longlong *)(local_108 + 0x1a8) != 0) {
    *lpHandles = (HANDLE)**(undefined8 **)(local_108 + 0x1d0);
    uVar10 = 1;
    if ((*(int *)(*(longlong *)(*(longlong *)(local_108 + 0x1a8) + 0x220) + 0x418) == 1) &&
       (uVar11 = 1, uVar10 = 1, 1 < *(uint *)(local_108 + 0x1c8))) {
      do {
        uVar13 = (int)uVar11 + 1;
        uVar10 = (ulonglong)uVar13;
        lpHandles[uVar11] = *(HANDLE *)(*(longlong *)(local_108 + 0x1d0) + uVar11 * 8);
        uVar11 = uVar10;
      } while (uVar13 < *(uint *)(local_108 + 0x1c8));
    }
  }
  uVar13 = (uint)uVar10;
  uVar11 = uVar10;
  if (*(longlong *)(local_108 + 0x1e8) != 0) {
    uVar11 = (ulonglong)(uVar13 + 1);
    lpHandles[uVar10] = (HANDLE)**(undefined8 **)(local_108 + 0x210);
    if ((*(int *)(*(longlong *)(*(longlong *)(local_108 + 0x1e8) + 0x220) + 0x418) == 1) &&
       (uVar10 = 1, 1 < *(uint *)(local_108 + 0x208))) {
      do {
        uVar14 = (int)uVar10 + 1;
        lpHandles[uVar11] = *(HANDLE *)(*(longlong *)(local_108 + 0x210) + uVar10 * 8);
        uVar11 = (ulonglong)((int)uVar11 + 1);
        uVar10 = (ulonglong)uVar14;
      } while (uVar14 < *(uint *)(local_108 + 0x208));
    }
  }
  uVar14 = (uint)uVar11;
  lpHandles[uVar11] = *(HANDLE *)(local_108 + 0x248);
  uVar10 = FUN_18001a230(&local_108);
  iVar9 = (int)uVar10;
  if (iVar9 == 0) {
    local_128 = FUN_180014e60();
    if (*(longlong *)(local_108 + 0x1e8) == 0) {
      uVar10 = FUN_18001b090(&local_108);
      iVar9 = (int)uVar10;
      if (iVar9 != 0) goto LAB_18001a90f;
      local_d8 = 1;
    }
    uVar11 = uVar10 & 0xffffffff;
    iVar9 = (int)uVar10;
    if ((*(longlong *)(local_108 + 0x1a8) != 0) &&
       (*(int *)(*(longlong *)(local_108 + 0x1a8) + 0x228) == 2)) {
      local_120 = **(longlong **)(local_108 + 0x1d0);
      uVar4 = (uint)((ulonglong)(uint)(*(int *)(local_108 + 0x1bc) * 1000) /
                    ((longlong)*(double *)(local_108 + 0x48) & 0xffffffffU));
      if (uVar4 <= uVar12) {
        uVar12 = uVar4;
      }
    }
    if ((*(longlong *)(local_108 + 0x1e8) != 0) &&
       (*(int *)(*(longlong *)(local_108 + 0x1e8) + 0x228) == 2)) {
      lStack_118 = **(longlong **)(local_108 + 0x210);
      uVar4 = (uint)((ulonglong)(uint)(*(int *)(local_108 + 0x1fc) * 1000) /
                    ((longlong)*(double *)(local_108 + 0x48) & 0xffffffffU));
      if (uVar4 <= uVar12) {
        uVar12 = uVar4;
      }
    }
    if ((local_120 != 0) || (lStack_118 != 0)) {
      local_110.QuadPart = 0;
      if (9 < uVar12) {
        uVar3 = uVar12 / 5;
      }
      local_130 = CreateWaitableTimerA((LPSECURITY_ATTRIBUTES)0x0,0,(LPCSTR)0x0);
      if ((local_130 == (HANDLE)0x0) ||
         (BVar5 = SetWaitableTimer(local_130,&local_110,uVar3,FUN_18001b420,&local_120,0),
         BVar5 == 0)) {
        iVar9 = -9999;
        goto LAB_18001a90f;
      }
    }
    *(undefined4 *)(local_108 + 0x22c) = 1;
    *(undefined4 *)(local_108 + 0x260) = 0;
    SetEvent(*(HANDLE *)(local_108 + 0x250));
    DVar6 = timeGetTime();
    iVar1 = *(int *)(local_108 + 0x234);
    local_134 = DVar6;
    while (iVar1 == 0) {
      iVar9 = (int)uVar11;
      DVar7 = WaitForMultipleObjects(uVar14 + 1,lpHandles,0,0);
      if (DVar7 == 0xffffffff) break;
      if (DVar7 == 0x102) {
        DVar7 = WaitForMultipleObjectsEx(uVar14 + 1,lpHandles,0,0x32,1);
      }
      else if (DVar7 < uVar13) {
        iVar9 = FUN_180006ba0((int *)(local_108 + 0x268));
        if (iVar9 == 0) {
          local_e8 = local_e8 | 2;
        }
      }
      else if (((DVar7 < uVar14) && (local_dc == 0)) && (1 < (uint)(local_c8 - local_c4))) {
        local_e8 = local_e8 | 4;
      }
      DVar8 = timeGetTime();
      if (((*(longlong *)(local_108 + 0x1a8) != 0) && (local_d4 <= DVar8 - DVar6)) ||
         ((*(longlong *)(local_108 + 0x1e8) != 0 && (local_d4 <= DVar8 - local_134)))) {
        *(undefined4 *)(local_108 + 0x260) = 0xffffd8fd;
        goto LAB_18001a92c;
      }
      if ((DVar7 != 0xc0) && (DVar7 != 0x102)) {
        if (DVar7 < uVar13) {
          iVar9 = (**(code **)(*(longlong *)(local_108 + 0x1a8) + 0x2a8))(&local_108,DVar7);
          if (iVar9 == 0) {
            if (*(int *)(local_108 + 0x230) == 0) {
              iVar9 = (**(code **)(*(longlong *)(local_108 + 0x1a8) + 0x2b0))(&local_108,local_cc);
              if (iVar9 != 0) break;
              uVar11 = 0;
            }
            local_cc = local_cc + 1;
            DVar6 = DVar8;
            if (*(int *)(local_108 + 0x298) < 1) goto LAB_18001a892;
          }
          else {
LAB_18001a892:
            uVar11 = FUN_1800179e0(&local_108);
            iVar9 = (int)uVar11;
            uVar11 = uVar11 & 0xffffffff;
            DVar8 = DVar6;
            if (iVar9 != 0) break;
          }
          iVar9 = (int)uVar11;
          if ((*(int *)(local_108 + 0x230) != 0) && (local_e4 != 1)) {
            local_e4 = 1;
          }
          if ((local_e0 < 1) ||
             ((DVar6 = DVar8, *(longlong *)(local_108 + 0x1e8) == 0 && (local_e4 != 0)))) break;
        }
        else if (DVar7 < uVar14) {
          (**(code **)(*(longlong *)(local_108 + 0x1e8) + 0x2a8))(&local_108,DVar7 - uVar13);
          local_134 = DVar8;
          goto LAB_18001a892;
        }
      }
      iVar9 = (int)uVar11;
      iVar1 = *(int *)(local_108 + 0x234);
    }
    *(int *)(local_108 + 0x260) = iVar9;
  }
  else {
LAB_18001a90f:
    *(int *)(local_108 + 0x260) = iVar9;
    SetEvent(*(HANDLE *)(local_108 + 600));
  }
LAB_18001a92c:
  if (local_130 != (HANDLE)0x0) {
    CancelWaitableTimer(local_130);
    CloseHandle(local_130);
  }
  if (local_d8 != 0) {
    FUN_18001b260(&local_108);
  }
  FUN_180015370(local_128);
  if (lpHandles != (HANDLE *)0x0) {
    FUN_180020240((longlong)lpHandles);
  }
  *(undefined4 *)(local_108 + 0x22c) = 0;
  if (((*(int *)(local_108 + 0x230) == 0) && (*(int *)(local_108 + 0x234) == 0)) &&
     (*(code **)(local_108 + 0x20) != (code *)0x0)) {
    (**(code **)(local_108 + 0x20))(*(undefined8 *)(local_108 + 0x28));
  }
  *(undefined4 *)(local_108 + 0x230) = 0;
  *(undefined4 *)(local_108 + 0x234) = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 18001aa00
   NAME : FUN_18001aa00
   SIG  : undefined __fastcall FUN_18001aa00(longlong param_1)
   ======================================================================== */

void FUN_18001aa00(longlong param_1)

{
  HANDLE pvVar1;
  uint uVar2;
  ulonglong uVar3;
  
  ResetEvent(*(HANDLE *)(param_1 + 0x248));
  ResetEvent(*(HANDLE *)(param_1 + 0x250));
  ResetEvent(*(HANDLE *)(param_1 + 600));
  uVar3 = 0;
  if (*(int *)(param_1 + 0x1c8) != 0) {
    do {
      if ((*(longlong *)(param_1 + 0x1d0) != 0) &&
         (pvVar1 = *(HANDLE *)(*(longlong *)(param_1 + 0x1d0) + uVar3 * 8), pvVar1 != (HANDLE)0x0))
      {
        ResetEvent(pvVar1);
      }
      uVar2 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar2;
    } while (uVar2 < *(uint *)(param_1 + 0x1c8));
  }
  uVar3 = 0;
  if (*(int *)(param_1 + 0x208) != 0) {
    do {
      if ((*(longlong *)(param_1 + 0x210) != 0) &&
         (pvVar1 = *(HANDLE *)(*(longlong *)(param_1 + 0x210) + uVar3 * 8), pvVar1 != (HANDLE)0x0))
      {
        ResetEvent(pvVar1);
      }
      uVar2 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar2;
    } while (uVar2 < *(uint *)(param_1 + 0x208));
  }
  return;
}



/* ========================================================================
   ENTRY: 18001aaa0
   NAME : FUN_18001aaa0
   SIG  : ulonglong __fastcall FUN_18001aaa0(longlong param_1, undefined4 param_2, undefined8 * param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong FUN_18001aaa0(longlong param_1,undefined4 param_2,undefined8 *param_3,undefined4 *param_4)

{
  undefined8 uVar1;
  undefined8 uVar2;
  bool bVar3;
  MMRESULT MVar4;
  int iVar5;
  undefined4 uVar6;
  uint uVar7;
  wchar_t *_Str2;
  wchar_t *_Str2_00;
  wchar_t *pwVar8;
  longlong lVar9;
  undefined8 uVar10;
  size_t sVar11;
  undefined8 uVar12;
  undefined7 extraout_var;
  ulonglong uVar13;
  undefined4 *puVar14;
  longlong lVar15;
  int iVar16;
  undefined8 *puVar17;
  wchar_t *pwVar18;
  undefined1 auStackY_338 [32];
  uint local_2f8;
  uint local_2f4;
  int local_2f0;
  uint local_2ec;
  uint local_2e8;
  int local_2e4;
  int local_2e0;
  int local_2dc;
  wchar_t *local_2d8;
  undefined4 local_2d0;
  int local_2cc;
  uint local_2c8;
  wchar_t *local_2c0;
  longlong local_2b8;
  longlong local_2b0 [2];
  undefined8 *local_2a0;
  undefined8 *local_298;
  undefined8 *local_290;
  undefined4 *local_288;
  wchar_t local_278 [264];
  ulonglong local_68;
  
  local_68 = DAT_18002b580 ^ (ulonglong)auStackY_338;
  pwVar18 = (wchar_t *)0x0;
  local_2f8 = 0;
  local_2f0 = 0;
  local_2f4 = 0;
  local_2ec = 0;
  local_2e8 = 0;
  local_2d0 = param_2;
  local_290 = param_3;
  local_288 = param_4;
  local_2b8 = FUN_180014960(&local_2f0,&local_2f4,&local_2f8);
  uVar7 = local_2f4;
  local_2c8 = local_2f8;
  pwVar8 = pwVar18;
  if (local_2f8 == 0) {
    local_2c0 = (wchar_t *)0x0;
    local_2d8 = (wchar_t *)0x0;
    MVar4 = waveInMessage((HWAVEIN)0x0,0x80d,(DWORD_PTR)&local_2ec,0);
    _Str2 = pwVar18;
    if (MVar4 == 0) {
      _Str2 = (wchar_t *)FUN_180020230(local_2ec * 2 + 2);
      local_2c0 = _Str2;
      waveInMessage((HWAVEIN)0x0,0x80c,(DWORD_PTR)_Str2,(ulonglong)local_2ec);
    }
    MVar4 = waveOutMessage((HWAVEOUT)0x0,0x80d,(DWORD_PTR)&local_2e8,0);
    _Str2_00 = pwVar18;
    if (MVar4 == 0) {
      _Str2_00 = (wchar_t *)FUN_180020230(local_2e8 * 2 + 2);
      local_2d8 = _Str2_00;
      waveOutMessage((HWAVEOUT)0x0,0x80c,(DWORD_PTR)_Str2_00,(ulonglong)local_2e8);
    }
    uVar6 = 0;
    if ((int)uVar7 < 1) {
LAB_18001b02b:
      *local_290 = pwVar8;
      *local_288 = uVar6;
      return (ulonglong)local_2c8;
    }
    pwVar8 = (wchar_t *)FUN_1800012e0(*(int **)(param_1 + 0x108),0x10);
    if (pwVar8 != (wchar_t *)0x0) {
      pwVar8[4] = L'\xffff';
      pwVar8[5] = L'\xffff';
      pwVar8[6] = L'\xffff';
      pwVar8[7] = L'\xffff';
      lVar9 = FUN_1800012e0(*(int **)(param_1 + 0x108),uVar7 * 8);
      *(longlong *)pwVar8 = lVar9;
      if ((lVar9 != 0) &&
         (lVar9 = FUN_1800012e0(*(int **)(param_1 + 0x108),uVar7 * 0x168), lVar9 != 0)) {
        iVar16 = 0;
        if (0 < (int)uVar7) {
          do {
            lVar15 = (longlong)iVar16;
            iVar16 = iVar16 + 1;
            puVar14 = (undefined4 *)(lVar15 * 0x168 + lVar9);
            *puVar14 = 2;
            puVar14[4] = param_2;
            *(undefined8 *)(puVar14 + 2) = 0;
            *(undefined4 **)(*(longlong *)pwVar8 + lVar15 * 8) = puVar14;
          } while (iVar16 < (int)uVar7);
        }
        uVar2 = _DAT_180025c50;
        uVar1 = DAT_180025c48;
        local_2f8 = 0;
        local_2dc = 0;
        local_2cc = local_2f0;
        lVar9 = local_2b8;
        uVar6 = 0;
        if (0 < local_2f0) {
          do {
            iVar16 = local_2dc;
            local_2b0[0] = 0;
            local_2b0[1] = 0;
            puVar17 = *(undefined8 **)(lVar9 + (longlong)local_2dc * 8);
            if (puVar17 != (undefined8 *)0x0) {
              local_298 = puVar17;
              uVar10 = FUN_180015ef0(local_2b0,(longlong)puVar17);
              if ((int)uVar10 == 0) {
                if (_Str2 == (wchar_t *)0x0) {
LAB_18001acfe:
                  local_2f0 = 0;
                }
                else {
                  iVar5 = _wcsicmp((wchar_t *)(puVar17 + 1),_Str2);
                  local_2f0 = 1;
                  if (iVar5 != 0) goto LAB_18001acfe;
                }
                if ((_Str2_00 == (wchar_t *)0x0) ||
                   (iVar5 = _wcsicmp((wchar_t *)(puVar17 + 1),_Str2_00), iVar5 != 0)) {
                  local_2e4 = 0;
                }
                else {
                  local_2e4 = 1;
                }
                if (0 < *(int *)(puVar17 + 0x86)) {
                  local_2a0 = puVar17 + 0x89;
                  local_2e0 = 0;
                  do {
                    uVar13 = 0;
                    lVar9 = *(longlong *)(puVar17[0x87] + (longlong)local_2e0 * 8);
                    if (lVar9 != 0) {
                      while( true ) {
                        uVar7 = *(uint *)(lVar9 + 0x10);
                        local_2f4 = (uint)uVar13;
                        if (uVar7 == 0) {
                          uVar7 = 1;
                        }
                        if (uVar7 <= local_2f4) break;
                        iVar16 = *(int *)(lVar9 + 0x268);
                        puVar14 = *(undefined4 **)(*(longlong *)pwVar8 + (longlong)(int)pwVar18 * 8)
                        ;
                        *(undefined8 **)(puVar14 + 0x54) = puVar17;
                        *(undefined4 **)(puVar14 + 2) = puVar14 + 0x12;
                        *puVar14 = 2;
                        puVar14[4] = local_2d0;
                        puVar14[0x56] = *(undefined4 *)(lVar9 + 0x22c);
                        if (*(longlong *)(lVar9 + 8) == 0) {
                          wcsncpy(local_278,(wchar_t *)(lVar9 + 0x14),0x104);
                          puVar14[0x57] = 0xffffffff;
                          uVar6 = *(undefined4 *)(lVar9 + 0x230);
                        }
                        else {
                          pwVar18 = *(wchar_t **)(*(longlong *)(lVar9 + 8) + uVar13 * 8);
                          wcsncpy(local_278,pwVar18,0x104);
                          puVar14[0x57] = local_2f4;
                          uVar6 = *(undefined4 *)(pwVar18 + 0x108);
                        }
                        puVar14[0x58] = uVar6;
                        sVar11 = wcslen(local_278);
                        uVar7 = FUN_180015e70(local_2b0,(ushort *)local_278,(uint)(iVar16 == 2));
                        if (uVar7 != 0) {
                          iVar5 = snprintf(local_278 + sVar11,0x104 - sVar11,&DAT_180025510,
                                           (ulonglong)uVar7);
                          sVar11 = (longlong)iVar5 + sVar11;
                        }
                        snprintf(local_278 + sVar11,0x104 - sVar11,L" (%s)",local_2a0);
                        WideCharToMultiByte(0xfde9,0,local_278,-1,(LPSTR)(puVar14 + 0x12),0x104,
                                            (LPCSTR)0x0,(LPBOOL)0x0);
                        puVar17 = local_298;
                        _Str2_00 = local_2d8;
                        uVar7 = local_2f8;
                        if (iVar16 == 2) {
                          puVar14[5] = *(undefined4 *)(lVar9 + 0x278);
                          puVar14[6] = 0;
                          if (((local_2c0 == (wchar_t *)0x0) || (local_2f0 != 0)) &&
                             (*(int *)(pwVar8 + 4) == -1)) {
                            *(uint *)(pwVar8 + 4) = local_2f8;
                          }
                        }
                        else {
                          puVar14[5] = 0;
                          puVar14[6] = *(undefined4 *)(lVar9 + 0x278);
                          if (((local_2d8 == (wchar_t *)0x0) || (local_2e4 != 0)) &&
                             (*(int *)(pwVar8 + 6) == -1)) {
                            *(uint *)(pwVar8 + 6) = local_2f8;
                          }
                        }
                        if (*(int *)(local_298 + 0x83) == 1) {
                          bVar3 = FUN_180015ff0();
                          uVar10 = uVar1;
                          if ((int)CONCAT71(extraout_var,bVar3) != 0) {
                            uVar10 = uVar2;
                          }
                          uVar12 = 0x3fb5d867c3ece2a5;
                          *(undefined8 *)(puVar14 + 8) = uVar10;
                          *(undefined8 *)(puVar14 + 10) = uVar10;
LAB_18001af89:
                          *(undefined8 *)(puVar14 + 0xc) = uVar12;
                          *(undefined8 *)(puVar14 + 0xe) = uVar12;
                          *(double *)(puVar14 + 0x10) = (double)*(int *)(lVar9 + 0x280);
                        }
                        else if (*(int *)(local_298 + 0x83) == 2) {
                          *(undefined8 *)(puVar14 + 8) = 0x3f847ae147ae147b;
                          *(undefined8 *)(puVar14 + 10) = 0x3f847ae147ae147b;
                          uVar12 = 0x3fa47ae147ae147b;
                          goto LAB_18001af89;
                        }
                        FUN_1800154e0(*(longlong *)(puVar14 + 0x54));
                        local_2f8 = uVar7 + 1;
                        pwVar18 = (wchar_t *)(ulonglong)local_2f8;
                        uVar13 = (ulonglong)(local_2f4 + 1);
                      }
                    }
                    local_2e0 = local_2e0 + 1;
                    _Str2 = local_2c0;
                    iVar16 = local_2dc;
                  } while (local_2e0 < *(int *)(puVar17 + 0x86));
                }
                if (*(int *)(puVar17 + 0xcd) == 0) {
                  FUN_180015540(puVar17);
                }
              }
              FUN_1800152a0(local_2b0);
              lVar9 = local_2b8;
            }
            uVar6 = SUB84(pwVar18,0);
            local_2dc = iVar16 + 1;
          } while (local_2dc < local_2cc);
        }
        goto LAB_18001b02b;
      }
    }
  }
  uVar13 = FUN_1800152d0(param_1,(longlong *)pwVar8,uVar7);
  return uVar13;
}



/* ========================================================================
   ENTRY: 18001b080
   NAME : FUN_18001b080
   SIG  : undefined __fastcall FUN_18001b080(undefined8 * param_1)
   ======================================================================== */

void FUN_18001b080(undefined8 *param_1)

{
  FUN_18001a090(param_1,3);
  return;
}



/* ========================================================================
   ENTRY: 18001b090
   NAME : FUN_18001b090
   SIG  : ulonglong __fastcall FUN_18001b090(longlong * param_1)
   ======================================================================== */

ulonglong FUN_18001b090(longlong *param_1)

{
  uint uVar1;
  ulonglong uVar2;
  
  uVar2 = 0;
  if (*(undefined8 **)(*param_1 + 0x1a8) != (undefined8 *)0x0) {
    uVar1 = FUN_18001b080(*(undefined8 **)(*param_1 + 0x1a8));
    uVar2 = (ulonglong)uVar1;
  }
  if (*(undefined8 **)(*param_1 + 0x1e8) != (undefined8 *)0x0) {
    uVar2 = FUN_18001b080(*(undefined8 **)(*param_1 + 0x1e8));
    return uVar2;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18001b0e0
   NAME : FUN_18001b0e0
   SIG  : undefined4 __fastcall FUN_18001b0e0(void * param_1)
   ======================================================================== */

undefined4 FUN_18001b0e0(void *param_1)

{
  undefined4 uVar1;
  DWORD DVar2;
  HANDLE pvVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  
  if (*(longlong *)((longlong)param_1 + 0x240) != 0) {
    return 0xffffd902;
  }
  *(undefined8 *)((longlong)param_1 + 0x230) = 0;
  FUN_18001aa00((longlong)param_1);
  FUN_1800066c0((longlong)param_1 + 0x98);
  pvVar3 = GetCurrentProcess();
  DVar2 = GetPriorityClass(pvVar3);
  *(DWORD *)((longlong)param_1 + 0x238) = DVar2;
  pvVar3 = (HANDLE)_beginthreadex((void *)0x0,0,FUN_18001a3a0,param_1,4,(uint *)0x0);
  *(HANDLE *)((longlong)param_1 + 0x240) = pvVar3;
  if (pvVar3 == (HANDLE)0x0) {
    return 0xffffd8f8;
  }
  ResumeThread(pvVar3);
  uVar5 = 5000;
  uVar4 = 0;
  DVar2 = WaitForMultipleObjects(2,(HANDLE *)((longlong)param_1 + 0x250),0,5000);
  if (DVar2 == 0) {
    *(undefined4 *)((longlong)param_1 + 0x228) = 1;
    return 0;
  }
  if (DVar2 != 1) {
    FUN_180018400(0xffffd8fd,"Failed to start processing thread (timeout)!",uVar4,uVar5);
    return 0xffffd8fd;
  }
  uVar1 = *(undefined4 *)((longlong)param_1 + 0x260);
  WaitForSingleObject(*(HANDLE *)((longlong)param_1 + 0x240),200);
  CloseHandle(*(HANDLE *)((longlong)param_1 + 0x240));
  *(undefined8 *)((longlong)param_1 + 0x240) = 0;
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001b230
   NAME : FUN_18001b230
   SIG  : undefined8 __fastcall FUN_18001b230(undefined8 * param_1)
   ======================================================================== */

undefined8 FUN_18001b230(undefined8 *param_1)

{
  FUN_18001a090(param_1,2);
  FUN_18001a090(param_1,0);
  return 0;
}



/* ========================================================================
   ENTRY: 18001b260
   NAME : FUN_18001b260
   SIG  : undefined8 __fastcall FUN_18001b260(longlong * param_1)
   ======================================================================== */

undefined8 FUN_18001b260(longlong *param_1)

{
  if (*(undefined8 **)(*param_1 + 0x1e8) != (undefined8 *)0x0) {
    FUN_18001b230(*(undefined8 **)(*param_1 + 0x1e8));
  }
  if (*(undefined8 **)(*param_1 + 0x1a8) != (undefined8 *)0x0) {
    FUN_18001b230(*(undefined8 **)(*param_1 + 0x1a8));
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001b2a0
   NAME : FUN_18001b2a0
   SIG  : int __fastcall FUN_18001b2a0(longlong param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

int FUN_18001b2a0(longlong param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  int iVar1;
  bool bVar2;
  BOOL BVar3;
  DWORD DVar4;
  int iVar5;
  DWORD local_res8 [8];
  
  if (*(int *)(param_1 + 0x22c) == 0) {
    iVar1 = *(int *)(param_1 + 0x260);
    iVar5 = 0;
    bVar2 = false;
    if (iVar1 != 0) {
      *(undefined4 *)(param_1 + 0x260) = 0;
      iVar5 = iVar1;
      bVar2 = false;
    }
  }
  else {
    bVar2 = true;
    *(undefined4 *)(param_1 + 0x230) = 1;
    BVar3 = GetExitCodeThread(*(HANDLE *)(param_1 + 0x240),local_res8);
    if ((BVar3 == 0) || (local_res8[0] != 0x103)) {
      FUN_180018400(0xffffd8f1,"StopStream: GECT says not active, but streamActive = %d",
                    (ulonglong)*(uint *)(param_1 + 0x22c),param_4);
      iVar5 = -9999;
    }
    else {
      DVar4 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x240),0xffffffff);
      iVar5 = 0;
      if (DVar4 != 0) {
        TerminateThread(*(HANDLE *)(param_1 + 0x240),0xffffffff);
        iVar5 = -0x2703;
      }
    }
  }
  if (*(HANDLE *)(param_1 + 0x240) != (HANDLE)0x0) {
    CloseHandle(*(HANDLE *)(param_1 + 0x240));
    *(undefined8 *)(param_1 + 0x240) = 0;
  }
  *(undefined8 *)(param_1 + 0x228) = 0;
  if ((bVar2) && (*(code **)(param_1 + 0x20) != (code *)0x0)) {
    (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
  }
  return iVar5;
}



/* ========================================================================
   ENTRY: 18001b380
   NAME : FUN_18001b380
   SIG  : undefined __fastcall FUN_18001b380(longlong param_1)
   ======================================================================== */

void FUN_18001b380(longlong param_1)

{
  longlong *plVar1;
  
  if (DAT_18002bb80 != (HMODULE)0x0) {
    FreeLibrary(DAT_18002bb80);
    DAT_18002bb80 = (HMODULE)0x0;
  }
  if (DAT_18002bb90 != (HMODULE)0x0) {
    FreeLibrary(DAT_18002bb90);
    DAT_18002bb90 = (HMODULE)0x0;
  }
  if (param_1 != 0) {
    plVar1 = (longlong *)FUN_1800012e0(*(int **)(param_1 + 0x108),0x10);
    *plVar1 = *(longlong *)(param_1 + 0x28);
    FUN_1800152d0(param_1,plVar1,*(int *)(param_1 + 0x18));
    if (*(longlong *)(param_1 + 0x108) != 0) {
      FUN_180001280(*(longlong *)(param_1 + 0x108));
      FUN_180001230(*(longlong *)(param_1 + 0x108));
    }
    FUN_180020240(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18001b420
   NAME : FUN_18001b420
   SIG  : undefined __fastcall FUN_18001b420(undefined8 * param_1)
   ======================================================================== */

void FUN_18001b420(undefined8 *param_1)

{
  if ((HANDLE)*param_1 != (HANDLE)0x0) {
    SetEvent((HANDLE)*param_1);
  }
  if ((HANDLE)param_1[1] != (HANDLE)0x0) {
                    /* WARNING: Could not recover jumptable at 0x00018001b445. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    SetEvent((HANDLE)param_1[1]);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18001b460
   NAME : FUN_18001b460
   SIG  : undefined __fastcall FUN_18001b460(wchar_t * param_1, ulonglong param_2)
   ======================================================================== */

void FUN_18001b460(wchar_t *param_1,ulonglong param_2)

{
  int iVar1;
  size_t sVar2;
  wchar_t *pwVar3;
  longlong lVar4;
  wchar_t *_Str;
  
  iVar1 = iswctype(*param_1,8);
  _Str = param_1;
  while (iVar1 != 0) {
    pwVar3 = _Str + 1;
    _Str = _Str + 1;
    iVar1 = iswctype(*pwVar3,8);
  }
  sVar2 = wcslen(_Str);
  if (param_2 < sVar2) {
    sVar2 = param_2;
  }
  pwVar3 = _Str + (sVar2 - 1);
  while ((_Str < pwVar3 && (iVar1 = iswctype(*pwVar3,8), iVar1 != 0))) {
    pwVar3 = pwVar3 + -1;
  }
  lVar4 = (longlong)pwVar3 + (2 - (longlong)_Str) >> 1;
  memmove(param_1,_Str,lVar4 * 2);
  param_1[lVar4] = L'\0';
  return;
}



/* ========================================================================
   ENTRY: 18001b520
   NAME : FUN_18001b520
   SIG  : undefined8 __fastcall FUN_18001b520(undefined8 param_1, int * param_2, int param_3)
   ======================================================================== */

undefined8 FUN_18001b520(undefined8 param_1,int *param_2,int param_3)

{
  if ((param_2 != (int *)0x0) &&
     (((((*param_2 != 0x18 || (param_2[2] != 1)) || ((param_2[3] & 0xfffffffcU) != 0)) ||
       ((param_2[4] == 1 || (8 < (uint)param_2[4])))) ||
      (((param_2[3] & 2U) != 0 && ((param_3 != 0 || ((param_2[5] & 0x7ffc0000U) != 0)))))))) {
    return 0xffffd900;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001b570
   NAME : FUN_18001b570
   SIG  : ulonglong __fastcall FUN_18001b570(HANDLE param_1, undefined4 param_2, undefined8 * param_3, undefined4 param_4, undefined8 * param_5)
   ======================================================================== */

ulonglong FUN_18001b570(HANDLE param_1,undefined4 param_2,undefined8 *param_3,undefined4 param_4,
                       undefined8 *param_5)

{
  ulonglong uVar1;
  LPVOID pvVar2;
  ulonglong uVar3;
  DWORD local_res10 [2];
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  
  local_28 = *param_3;
  uStack_20 = param_3[1];
  local_res10[0] = 0;
  local_14 = 1;
  local_c = 0;
  local_18 = param_4;
  local_10 = param_2;
  uVar1 = FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x20,(LPVOID)0x0,0,local_res10);
  if ((int)uVar1 == 0) {
    pvVar2 = (LPVOID)FUN_180020230(local_res10[0]);
    *param_5 = pvVar2;
    if (pvVar2 == (LPVOID)0x0) {
      return 0xffffd8f8;
    }
    uVar3 = FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x20,pvVar2,local_res10[0],(LPDWORD)0x0
                         );
    uVar1 = uVar3 & 0xffffffff;
    if ((int)uVar3 != 0) {
      FUN_180020240((longlong)param_5);
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001b650
   NAME : FUN_18001b650
   SIG  : undefined __fastcall FUN_18001b650(HANDLE param_1, undefined4 param_2, undefined8 * param_3, undefined4 param_4, LPVOID param_5, DWORD param_6, LPDWORD param_7)
   ======================================================================== */

void FUN_18001b650(HANDLE param_1,undefined4 param_2,undefined8 *param_3,undefined4 param_4,
                  LPVOID param_5,DWORD param_6,LPDWORD param_7)

{
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  
  local_28 = *param_3;
  uStack_20 = param_3[1];
  local_14 = 1;
  local_c = 0;
  local_18 = param_4;
  local_10 = param_2;
  FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x20,param_5,param_6,param_7);
  return;
}



/* ========================================================================
   ENTRY: 18001b6c0
   NAME : FUN_18001b6c0
   SIG  : ulonglong __fastcall FUN_18001b6c0(HANDLE param_1, undefined8 * param_2, undefined4 param_3, undefined8 * param_4)
   ======================================================================== */

ulonglong FUN_18001b6c0(HANDLE param_1,undefined8 *param_2,undefined4 param_3,undefined8 *param_4)

{
  ulonglong uVar1;
  LPVOID pvVar2;
  ulonglong uVar3;
  DWORD local_res18 [4];
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  
  local_28 = *param_2;
  uStack_20 = param_2[1];
  local_res18[0] = 0;
  local_14 = 1;
  local_18 = param_3;
  uVar1 = FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x18,(LPVOID)0x0,0,local_res18);
  if ((int)uVar1 == 0) {
    pvVar2 = (LPVOID)FUN_180020230(local_res18[0]);
    *param_4 = pvVar2;
    if (pvVar2 == (LPVOID)0x0) {
      return 0xffffd8f8;
    }
    uVar3 = FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x18,pvVar2,local_res18[0],(LPDWORD)0x0
                         );
    uVar1 = uVar3 & 0xffffffff;
    if ((int)uVar3 != 0) {
      FUN_180020240((longlong)param_4);
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001b7a0
   NAME : FUN_18001b7a0
   SIG  : undefined __fastcall FUN_18001b7a0(HANDLE param_1, undefined8 * param_2, undefined4 param_3, LPVOID param_4, DWORD param_5)
   ======================================================================== */

void FUN_18001b7a0(HANDLE param_1,undefined8 *param_2,undefined4 param_3,LPVOID param_4,
                  DWORD param_5)

{
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  
  local_28 = *param_2;
  uStack_20 = param_2[1];
  local_14 = 1;
  local_18 = param_3;
  FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x18,param_4,param_5,(LPDWORD)0x0);
  return;
}



/* ========================================================================
   ENTRY: 18001b7f0
   NAME : FUN_18001b7f0
   SIG  : undefined __fastcall FUN_18001b7f0(HANDLE param_1, undefined4 param_2, undefined4 param_3)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18001b7f0(HANDLE param_1,undefined4 param_2,undefined4 param_3)

{
  undefined4 local_res18 [4];
  undefined8 local_28;
  undefined8 uStack_20;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  
  local_c = 0;
  local_28 = _DAT_1800249a0;
  uStack_20 = _UNK_1800249a8;
  local_18 = 0xc;
  local_14 = 0x10000002;
  local_res18[0] = param_3;
  local_10 = param_2;
  FUN_18001b850(param_1,0x2f0003,(uint *)&local_28,0x20,local_res18,4,(LPDWORD)0x0);
  return;
}



/* ========================================================================
   ENTRY: 18001b850
   NAME : FUN_18001b850
   SIG  : undefined8 __fastcall FUN_18001b850(HANDLE param_1, DWORD param_2, uint * param_3, DWORD param_4, LPVOID param_5, DWORD param_6, LPDWORD param_7)
   ======================================================================== */

undefined8
FUN_18001b850(HANDLE param_1,DWORD param_2,uint *param_3,DWORD param_4,LPVOID param_5,DWORD param_6,
             LPDWORD param_7)

{
  BOOL BVar1;
  DWORD DVar2;
  undefined8 uVar3;
  DWORD *lpBytesReturned;
  DWORD local_28 [4];
  
  local_28[0] = 0;
  lpBytesReturned = local_28;
  if (param_7 != (LPDWORD)0x0) {
    lpBytesReturned = param_7;
  }
  BVar1 = DeviceIoControl(param_1,param_2,param_3,param_4,param_5,param_6,lpBytesReturned,
                          (LPOVERLAPPED)0x0);
  if (BVar1 == 0) {
    DVar2 = GetLastError();
    if ((((DVar2 == 0x7a) || (DVar2 == 0xea)) && (param_2 == 0x2f0003)) && (param_6 == 0)) {
      uVar3 = 0;
    }
    else {
      FUN_180018400(0,
                    "WdmSyncIoctl: DeviceIoControl GLE = 0x%08X (prop_set = {%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}, prop_id = %u)"
                    ,(ulonglong)DVar2,(ulonglong)*param_3);
      uVar3 = 0xffffd8f1;
    }
  }
  else {
    uVar3 = 0;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001b9a0
   NAME : snprintf
   SIG  : int __fastcall snprintf(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

/* Library Function - Single Match
    snprintf
   
   Library: Visual Studio 2019 Release */

int snprintf(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  int iVar1;
  ulonglong *puVar2;
  undefined8 local_res20;
  
  local_res20 = param_4;
  puVar2 = (ulonglong *)FUN_180003990();
  iVar1 = __stdio_common_vswprintf(*puVar2 | 1,param_1,param_2,param_3,0,&local_res20);
  if (iVar1 < 0) {
    iVar1 = -1;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 18001ba00
   NAME : FUN_18001ba00
   SIG  : undefined8 __fastcall FUN_18001ba00(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18001ba00(longlong param_1)

{
  MMRESULT mmrError;
  DWORD DVar1;
  ulonglong uVar2;
  uint uVar3;
  ulonglong uVar4;
  undefined1 auStackY_358 [32];
  CHAR local_318 [256];
  WCHAR local_218 [256];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStackY_358;
  if (*(longlong *)(param_1 + 0x1e0) != 0) {
    *(undefined4 *)(param_1 + 0x20c) = 1;
    SetEvent(*(HANDLE *)(param_1 + 0x1d8));
  }
  uVar2 = 0;
  if ((*(longlong *)(param_1 + 0x1b0) != 0) && (uVar4 = uVar2, *(int *)(param_1 + 0x1b8) != 0)) {
    do {
      mmrError = waveOutReset(*(HWAVEOUT *)(*(longlong *)(param_1 + 0x1b0) + uVar4 * 8));
      if (mmrError != 0) {
        waveOutGetErrorTextW(mmrError,local_218,0x100);
        goto LAB_18001bb3d;
      }
      uVar3 = (int)uVar4 + 1;
      uVar4 = (ulonglong)uVar3;
    } while (uVar3 < *(uint *)(param_1 + 0x1b8));
  }
  if ((*(longlong *)(param_1 + 0x180) != 0) && (*(int *)(param_1 + 0x188) != 0)) {
    do {
      mmrError = waveInReset(*(HWAVEIN *)(*(longlong *)(param_1 + 0x180) + uVar2 * 8));
      if (mmrError != 0) {
        waveInGetErrorTextW(mmrError,local_218,0x100);
LAB_18001bb3d:
        WideCharToMultiByte(0xfde9,0,local_218,-1,local_318,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
        FUN_180003c90(2,mmrError,local_318);
        return 0xffffd8f1;
      }
      uVar3 = (int)uVar2 + 1;
      uVar2 = (ulonglong)uVar3;
    } while (uVar3 < *(uint *)(param_1 + 0x188));
  }
  if (*(HANDLE *)(param_1 + 0x1e0) != (HANDLE)0x0) {
    DVar1 = (DWORD)(longlong)((double)*(uint *)(param_1 + 0x210) * _DAT_180025cc0);
    if (DVar1 < 1000) {
      DVar1 = 1000;
    }
    DVar1 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x1e0),DVar1);
    if (DVar1 == 0x102) {
      return 0xffffd8fd;
    }
    CloseHandle(*(HANDLE *)(param_1 + 0x1e0));
    *(undefined8 *)(param_1 + 0x1e0) = 0;
  }
  *(undefined4 *)(param_1 + 0x200) = 1;
  *(undefined4 *)(param_1 + 0x204) = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 18001bc00
   NAME : FUN_18001bc00
   SIG  : ulonglong __fastcall FUN_18001bc00(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18001bc00(longlong param_1)

{
  uint *puVar1;
  MMRESULT mmrError;
  uint uVar2;
  ulonglong uVar3;
  uint uVar4;
  ulonglong uVar5;
  undefined1 auStackY_368 [32];
  CHAR local_328 [256];
  WCHAR local_228 [256];
  ulonglong local_28;
  
  local_28 = DAT_18002b580 ^ (ulonglong)auStackY_368;
  uVar3 = 0;
  uVar5 = uVar3;
  if (*(int *)(param_1 + 0x188) != 0) {
    do {
      puVar1 = (uint *)((ulonglong)*(uint *)(param_1 + 0x19c) * 0x30 + 0x18 +
                       *(longlong *)(*(longlong *)(param_1 + 400) + uVar5 * 8));
      *puVar1 = *puVar1 & 0xfffffffe;
      mmrError = waveInAddBuffer(*(HWAVEIN *)(*(longlong *)(param_1 + 0x180) + uVar5 * 8),
                                 (LPWAVEHDR)
                                 ((ulonglong)*(uint *)(param_1 + 0x19c) * 0x30 +
                                 *(longlong *)(*(longlong *)(param_1 + 400) + uVar5 * 8)),0x30);
      if (mmrError != 0) {
        uVar3 = 0xffffd8f1;
        waveInGetErrorTextW(mmrError,local_228,0x100);
        WideCharToMultiByte(0xfde9,0,local_228,-1,local_328,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
        FUN_180003c90(2,mmrError,local_328);
      }
      uVar4 = (int)uVar5 + 1;
      uVar5 = (ulonglong)uVar4;
    } while (uVar4 < *(uint *)(param_1 + 0x188));
  }
  uVar2 = *(int *)(param_1 + 0x19c) + 1;
  *(undefined4 *)(param_1 + 0x1a4) = 0;
  uVar4 = 0;
  if (uVar2 < *(uint *)(param_1 + 0x198)) {
    uVar4 = uVar2;
  }
  *(uint *)(param_1 + 0x19c) = uVar4;
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001bd60
   NAME : FUN_18001bd60
   SIG  : ulonglong __fastcall FUN_18001bd60(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18001bd60(longlong param_1)

{
  MMRESULT mmrError;
  uint uVar1;
  ulonglong uVar2;
  uint uVar3;
  ulonglong uVar4;
  undefined1 auStackY_368 [32];
  CHAR local_328 [256];
  WCHAR local_228 [256];
  ulonglong local_28;
  
  local_28 = DAT_18002b580 ^ (ulonglong)auStackY_368;
  uVar2 = 0;
  uVar4 = uVar2;
  if (*(int *)(param_1 + 0x1b8) != 0) {
    do {
      mmrError = waveOutWrite(*(HWAVEOUT *)(*(longlong *)(param_1 + 0x1b0) + uVar4 * 8),
                              (LPWAVEHDR)
                              ((ulonglong)*(uint *)(param_1 + 0x1cc) * 0x30 +
                              *(longlong *)(*(longlong *)(param_1 + 0x1c0) + uVar4 * 8)),0x30);
      if (mmrError != 0) {
        uVar2 = 0xffffd8f1;
        waveOutGetErrorTextW(mmrError,local_228,0x100);
        WideCharToMultiByte(0xfde9,0,local_228,-1,local_328,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
        FUN_180003c90(2,mmrError,local_328);
      }
      uVar3 = (int)uVar4 + 1;
      uVar4 = (ulonglong)uVar3;
    } while (uVar3 < *(uint *)(param_1 + 0x1b8));
  }
  uVar1 = *(int *)(param_1 + 0x1cc) + 1;
  *(undefined4 *)(param_1 + 0x1d4) = 0;
  uVar3 = 0;
  if (uVar1 < *(uint *)(param_1 + 0x1c8)) {
    uVar3 = uVar1;
  }
  *(uint *)(param_1 + 0x1cc) = uVar3;
  return uVar2;
}



/* ========================================================================
   ENTRY: 18001bea0
   NAME : FUN_18001bea0
   SIG  : undefined8 __fastcall FUN_18001bea0(longlong param_1, uint param_2, int param_3)
   ======================================================================== */

undefined8 FUN_18001bea0(longlong param_1,uint param_2,int param_3)

{
  uint uVar1;
  ulonglong uVar2;
  
  uVar2 = 0;
  if (param_2 != 0) {
    do {
      if ((*(byte *)(*(longlong *)(param_1 + uVar2 * 8) + 0x18 + (longlong)param_3 * 0x30) & 1) == 0
         ) {
        return 0;
      }
      uVar1 = (int)uVar2 + 1;
      uVar2 = (ulonglong)uVar1;
    } while (uVar1 < param_2);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18001bed0
   NAME : FUN_18001bed0
   SIG  : ulonglong __fastcall FUN_18001bed0(uint * param_1, uint * param_2, uint * param_3, uint * param_4, uint param_5, uint param_6, double param_7, longlong param_8, uint param_9, uint param_10, double param_11, longlong param_12, double param_13, uint param_14)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong FUN_18001bed0(uint *param_1,uint *param_2,uint *param_3,uint *param_4,uint param_5,
                       uint param_6,double param_7,longlong param_8,uint param_9,uint param_10,
                       double param_11,longlong param_12,double param_13,uint param_14)

{
  uint uVar1;
  double dVar2;
  double dVar3;
  longlong lVar4;
  uint uVar5;
  uint uVar6;
  ulonglong uVar7;
  ulonglong uVar8;
  undefined4 uVar9;
  undefined4 uVar10;
  
  lVar4 = param_8;
  uVar5 = param_5;
  uVar8 = 0;
  if ((int)param_5 < 1) {
    *param_1 = 0;
    *param_2 = 0;
LAB_18001bfdd:
    uVar9 = SUB84(param_13,0);
    uVar10 = (undefined4)((ulonglong)param_13 >> 0x20);
    dVar3 = param_13 * _DAT_180023d18;
    dVar2 = param_7 * param_13;
LAB_18001c001:
    uVar6 = 0;
    if ((int)param_9 < 1) {
      *param_3 = 0;
    }
    else {
      if ((param_12 != 0) && ((*(byte *)(param_12 + 0xc) & 1) != 0)) {
        if ((*(int *)(param_12 + 0x14) != 0) && (*(uint *)(param_12 + 0x10) != 0)) {
          *param_3 = *(uint *)(param_12 + 0x10);
          *param_4 = *(uint *)(param_12 + 0x14);
          if ((int)uVar5 < 1) goto LAB_18001c16b;
          uVar5 = *param_3;
          uVar6 = *param_1;
          if (uVar6 == uVar5) goto LAB_18001c16b;
          if ((lVar4 == 0) || ((*(byte *)(lVar4 + 0xc) & 1) == 0)) {
            *param_1 = uVar5;
            uVar5 = FUN_18001c510((int)(longlong)dVar2,uVar5,3);
            *param_2 = uVar5;
            goto LAB_18001c16b;
          }
          if (uVar5 <= uVar6) {
            if (uVar6 % uVar5 != 0) {
              uVar8 = 0xffffd900;
            }
            goto LAB_18001c16b;
          }
          if (uVar5 % uVar6 == 0) goto LAB_18001c16b;
        }
        goto LAB_18001c078;
      }
      uVar7 = FUN_18001c190(param_9,param_10,param_12,(int *)&param_5);
      uVar8 = uVar7 & 0xffffffff;
      if ((int)uVar7 != 0) goto LAB_18001c16b;
      uVar6 = (uint)(longlong)(param_11 * (double)CONCAT44(uVar10,uVar9));
      uVar7 = FUN_18001f0f0(uVar6,param_14,2,(uint)(longlong)dVar3,
                            (uint)(0x8000 / (longlong)(int)param_5),param_3,param_4);
      uVar8 = uVar7 & 0xffffffff;
      if (((int)uVar7 != 0) || ((int)uVar5 < 1)) goto LAB_18001c16b;
      uVar5 = *param_1;
      uVar1 = *param_3;
      if (uVar1 == uVar5) goto LAB_18001c16b;
      if (uVar1 <= uVar5) {
        *param_1 = uVar1;
        uVar5 = FUN_18001c510((int)(longlong)dVar2,uVar1,3);
        *param_2 = uVar5;
        goto LAB_18001c16b;
      }
      *param_3 = uVar5;
      uVar6 = FUN_18001c510(uVar6,*param_4,2);
    }
    *param_4 = uVar6;
  }
  else {
    uVar7 = FUN_18001c190(param_5,param_6,param_8,(int *)&param_5);
    uVar8 = uVar7 & 0xffffffff;
    if ((int)uVar7 != 0) goto LAB_18001c16b;
    if ((lVar4 == 0) || ((*(byte *)(lVar4 + 0xc) & 1) == 0)) {
      uVar9 = SUB84(param_13,0);
      uVar10 = (undefined4)((ulonglong)param_13 >> 0x20);
      dVar3 = param_13 * _DAT_180023d18;
      dVar2 = param_7 * param_13;
      uVar7 = FUN_18001f0f0((uint)(longlong)dVar2,param_14,(0 < (int)param_9) + 2,
                            (uint)(longlong)dVar3,(uint)(0x8000 / (longlong)(int)param_5),param_1,
                            param_2);
      uVar8 = uVar7 & 0xffffffff;
      if ((int)uVar7 != 0) goto LAB_18001c16b;
      goto LAB_18001c001;
    }
    if ((*(int *)(lVar4 + 0x14) != 0) && (*(uint *)(lVar4 + 0x10) != 0)) {
      *param_1 = *(uint *)(lVar4 + 0x10);
      *param_2 = *(uint *)(lVar4 + 0x14);
      goto LAB_18001bfdd;
    }
LAB_18001c078:
    uVar8 = 0xffffd900;
  }
LAB_18001c16b:
  return uVar8 & 0xffffffff;
}



/* ========================================================================
   ENTRY: 18001c190
   NAME : FUN_18001c190
   SIG  : undefined8 __fastcall FUN_18001c190(uint param_1, uint param_2, longlong param_3, int * param_4)
   ======================================================================== */

undefined8 FUN_18001c190(uint param_1,uint param_2,longlong param_3,int *param_4)

{
  uint uVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  int iVar8;
  longlong lVar9;
  int iVar10;
  uint uVar11;
  undefined8 uVar12;
  ulonglong uVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  uint uVar17;
  uint uVar18;
  uint uVar19;
  uint uVar20;
  
  uVar12 = Pa_GetSampleSize(param_2);
  if ((int)uVar12 < 0) {
    return uVar12;
  }
  if ((param_3 != 0) && ((*(byte *)(param_3 + 0xc) & 2) != 0)) {
    lVar9 = *(longlong *)(param_3 + 0x18);
    uVar1 = *(uint *)(param_3 + 0x20);
    param_1 = *(uint *)(lVar9 + 4);
    if (1 < uVar1) {
      uVar13 = 1;
      if ((7 < uVar1 - 1) && (1 < DAT_18002b540)) {
        uVar18 = param_1;
        uVar19 = param_1;
        uVar20 = param_1;
        uVar14 = param_1;
        uVar15 = param_1;
        uVar16 = param_1;
        uVar17 = param_1;
        do {
          iVar10 = (int)uVar13;
          iVar2 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 3) * 8);
          iVar3 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 2) * 8);
          iVar4 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 1) * 8);
          iVar5 = *(int *)(lVar9 + 4 + uVar13 * 8);
          iVar6 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 5) * 8);
          uVar11 = iVar10 + 8;
          uVar13 = (ulonglong)uVar11;
          iVar7 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 7) * 8);
          iVar8 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 6) * 8);
          uVar14 = (uint)((int)uVar14 < iVar5) * iVar5 | ((int)uVar14 >= iVar5) * uVar14;
          uVar15 = (uint)((int)uVar15 < iVar4) * iVar4 | ((int)uVar15 >= iVar4) * uVar15;
          uVar16 = (uint)((int)uVar16 < iVar3) * iVar3 | ((int)uVar16 >= iVar3) * uVar16;
          uVar17 = (uint)((int)uVar17 < iVar2) * iVar2 | ((int)uVar17 >= iVar2) * uVar17;
          iVar2 = *(int *)(lVar9 + 4 + (ulonglong)(iVar10 + 4) * 8);
          param_1 = (uint)((int)param_1 < iVar2) * iVar2 | ((int)param_1 >= iVar2) * param_1;
          uVar18 = (uint)((int)uVar18 < iVar6) * iVar6 | ((int)uVar18 >= iVar6) * uVar18;
          uVar19 = (uint)((int)uVar19 < iVar8) * iVar8 | ((int)uVar19 >= iVar8) * uVar19;
          uVar20 = (uint)((int)uVar20 < iVar7) * iVar7 | ((int)uVar20 >= iVar7) * uVar20;
        } while (uVar11 < uVar1 - (uVar1 - 1 & 7));
        uVar14 = ((int)uVar14 < (int)param_1) * param_1 | ((int)uVar14 >= (int)param_1) * uVar14;
        uVar15 = ((int)uVar15 < (int)uVar18) * uVar18 | ((int)uVar15 >= (int)uVar18) * uVar15;
        uVar18 = ((int)uVar16 < (int)uVar19) * uVar19 | ((int)uVar16 >= (int)uVar19) * uVar16;
        uVar19 = ((int)uVar17 < (int)uVar20) * uVar20 | ((int)uVar17 >= (int)uVar20) * uVar17;
        uVar18 = ((int)uVar14 < (int)uVar18) * uVar18 | ((int)uVar14 >= (int)uVar18) * uVar14;
        uVar19 = ((int)uVar15 < (int)uVar19) * uVar19 | ((int)uVar15 >= (int)uVar19) * uVar15;
        param_1 = ((int)uVar18 < (int)uVar19) * uVar19 | ((int)uVar18 >= (int)uVar19) * uVar18;
        if (uVar1 <= uVar11) goto LAB_18001c2d4;
      }
      do {
        uVar18 = *(uint *)(lVar9 + 4 + uVar13 * 8);
        if ((int)uVar18 <= (int)param_1) {
          uVar18 = param_1;
        }
        param_1 = uVar18;
        uVar18 = (int)uVar13 + 1;
        uVar13 = (ulonglong)uVar18;
      } while (uVar18 < uVar1);
    }
  }
LAB_18001c2d4:
  *param_4 = (int)uVar12 * param_1;
  return 0;
}



/* ========================================================================
   ENTRY: 18001c2f0
   NAME : FUN_18001c2f0
   SIG  : ulonglong __fastcall FUN_18001c2f0(longlong param_1)
   ======================================================================== */

ulonglong FUN_18001c2f0(longlong param_1)

{
  ulonglong uVar1;
  ulonglong uVar2;
  uint uVar3;
  
  uVar3 = 0;
  uVar2 = 0;
  if (*(int *)(param_1 + 0x198) != 1) {
    do {
      uVar1 = FUN_18001bc00(param_1);
      uVar2 = uVar1 & 0xffffffff;
      if ((int)uVar1 != 0) {
        return uVar1;
      }
      uVar3 = uVar3 + 1;
    } while (uVar3 < *(int *)(param_1 + 0x198) - 1U);
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 18001c340
   NAME : FUN_18001c340
   SIG  : ulonglong __fastcall FUN_18001c340(longlong param_1)
   ======================================================================== */

ulonglong FUN_18001c340(longlong param_1)

{
  int iVar1;
  longlong lVar2;
  void *_Src;
  void *_Dst;
  ulonglong uVar3;
  uint uVar4;
  uint uVar5;
  
  iVar1 = *(int *)(param_1 + 0x1cc);
  if (iVar1 == 0) {
    iVar1 = *(int *)(param_1 + 0x1c8);
  }
  uVar5 = 0;
  if (*(int *)(param_1 + 0x1c8) != 1) {
    do {
      uVar3 = 0;
      if (*(int *)(param_1 + 0x1b8) != 0) {
        do {
          lVar2 = *(longlong *)(*(longlong *)(param_1 + 0x1c0) + uVar3 * 8);
          _Src = *(void **)(lVar2 + (ulonglong)(iVar1 - 1) * 0x30);
          _Dst = *(void **)(lVar2 + (ulonglong)*(uint *)(param_1 + 0x1cc) * 0x30);
          if (_Dst != _Src) {
            memcpy(_Dst,_Src,
                   (ulonglong)*(uint *)(lVar2 + 8 + (ulonglong)*(uint *)(param_1 + 0x1cc) * 0x30));
          }
          uVar4 = (int)uVar3 + 1;
          uVar3 = (ulonglong)uVar4;
        } while (uVar4 < *(uint *)(param_1 + 0x1b8));
      }
      uVar3 = FUN_18001bd60(param_1);
    } while (((int)uVar3 == 0) &&
            (uVar5 = uVar5 + 1, uVar3 = uVar3 & 0xffffffff, uVar5 < *(int *)(param_1 + 0x1c8) - 1U))
    ;
    return uVar3;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001c430
   NAME : FUN_18001c430
   SIG  : undefined8 __fastcall FUN_18001c430(HANDLE param_1)
   ======================================================================== */

undefined8 FUN_18001c430(HANDLE param_1)

{
  BOOL BVar1;
  DWORD DVar2;
  undefined8 uVar3;
  
  uVar3 = 0;
  if (param_1 != (HANDLE)0x0) {
    uVar3 = 0;
    BVar1 = CloseHandle(param_1);
    if (BVar1 == 0) {
      DVar2 = GetLastError();
      FUN_18001dd10(DVar2);
      uVar3 = 0xffffd8f1;
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001c470
   NAME : FUN_18001c470
   SIG  : ulonglong __fastcall FUN_18001c470(undefined4 * param_1)
   ======================================================================== */

ulonglong FUN_18001c470(undefined4 *param_1)

{
  ulonglong uVar1;
  
  uVar1 = FUN_18001c430(*(HANDLE *)(param_1 + 0x76));
  if ((int)uVar1 == 0) {
    FUN_18001fca0((longlong)(param_1 + 0x6a),0);
    FUN_18001fca0((longlong)(param_1 + 0x5e),1);
    FUN_18001fb30((undefined8 *)(param_1 + 0x6a),0,0);
    FUN_18001fb30((undefined8 *)(param_1 + 0x5e),1,0);
    FUN_1800069c0((longlong)(param_1 + 0x1a));
    FUN_180006ea0(param_1);
    FUN_180020240((longlong)param_1);
    uVar1 = uVar1 & 0xffffffff;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001c510
   NAME : FUN_18001c510
   SIG  : uint __fastcall FUN_18001c510(int param_1, uint param_2, uint param_3)
   ======================================================================== */

uint FUN_18001c510(int param_1,uint param_2,uint param_3)

{
  uint uVar1;
  
  uVar1 = ((param_2 - 1) + param_1) / param_2 + 1;
  if (uVar1 < param_3) {
    uVar1 = param_3;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001c530
   NAME : FUN_18001c530
   SIG  : ulonglong __fastcall FUN_18001c530(uint param_1, uint param_2)
   ======================================================================== */

ulonglong FUN_18001c530(uint param_1,uint param_2)

{
  int iVar1;
  uint uVar2;
  uint uVar3;
  int iVar4;
  
  uVar2 = param_1;
  do {
    if (uVar2 <= param_2) {
      return (ulonglong)uVar2;
    }
    iVar4 = 0;
    iVar1 = DAT_18002b4b0;
    while ((uVar3 = uVar2, iVar1 != 0 &&
           (uVar3 = uVar2 / (uint)(&DAT_18002b4b0)[iVar4],
           uVar2 % (uint)(&DAT_18002b4b0)[iVar4] != 0))) {
      iVar4 = iVar4 + 1;
      iVar1 = (&DAT_18002b4b0)[iVar4];
    }
    uVar2 = uVar3;
  } while ((&DAT_18002b4b0)[iVar4] != 0);
  return (ulonglong)param_1 / ((ulonglong)((param_2 - 1) + param_1) / (ulonglong)param_2);
}



/* ========================================================================
   ENTRY: 18001c5c0
   NAME : FUN_18001c5c0
   SIG  : LPSTR __fastcall FUN_18001c5c0(LPSTR param_1, ulonglong param_2, LPCWSTR param_3)
   ======================================================================== */

LPSTR FUN_18001c5c0(LPSTR param_1,ulonglong param_2,LPCWSTR param_3)

{
  int iVar1;
  LPSTR pCVar2;
  
  iVar1 = 0x7fffffff;
  if (param_2 < 0x7fffffff) {
    iVar1 = (int)param_2;
  }
  iVar1 = WideCharToMultiByte(0xfde9,0,param_3,-1,param_1,iVar1,(LPCSTR)0x0,(LPBOOL)0x0);
  pCVar2 = (LPSTR)0x0;
  if (iVar1 != 0) {
    pCVar2 = param_1;
  }
  return pCVar2;
}



/* ========================================================================
   ENTRY: 18001c620
   NAME : FUN_18001c620
   SIG  : undefined8 __fastcall FUN_18001c620(undefined8 * param_1, LPSECURITY_ATTRIBUTES param_2, BOOL param_3, BOOL param_4, LPCWSTR param_5)
   ======================================================================== */

undefined8
FUN_18001c620(undefined8 *param_1,LPSECURITY_ATTRIBUTES param_2,BOOL param_3,BOOL param_4,
             LPCWSTR param_5)

{
  DWORD DVar1;
  HANDLE pvVar2;
  
  *param_1 = 0;
  pvVar2 = CreateEventW(param_2,param_3,param_4,param_5);
  *param_1 = pvVar2;
  if (pvVar2 == (HANDLE)0x0) {
    DVar1 = GetLastError();
    FUN_18001dd10(DVar1);
    return 0xffffd8f1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001c690
   NAME : FUN_18001c690
   SIG  : undefined __fastcall FUN_18001c690(longlong param_1)
   ======================================================================== */

void FUN_18001c690(longlong param_1)

{
  FUN_18001bea0(*(longlong *)(param_1 + 400),*(uint *)(param_1 + 0x188),*(int *)(param_1 + 0x19c));
  return;
}



/* ========================================================================
   ENTRY: 18001c6b0
   NAME : FUN_18001c6b0
   SIG  : undefined __fastcall FUN_18001c6b0(longlong param_1)
   ======================================================================== */

void FUN_18001c6b0(longlong param_1)

{
  FUN_18001bea0(*(longlong *)(param_1 + 0x1c0),*(uint *)(param_1 + 0x1b8),*(int *)(param_1 + 0x1cc))
  ;
  return;
}



/* ========================================================================
   ENTRY: 18001c6d0
   NAME : FUN_18001c6d0
   SIG  : undefined __fastcall FUN_18001c6d0(longlong param_1, undefined4 param_2, undefined * param_3, int param_4)
   ======================================================================== */

void FUN_18001c6d0(longlong param_1,undefined4 param_2,undefined *param_3,int param_4)

{
  double dVar1;
  undefined8 uVar2;
  uint uVar3;
  ulonglong uVar4;
  
  uVar4 = 0;
  *(undefined8 *)(param_1 + 0x40) = 0;
  do {
    dVar1 = *(double *)(&DAT_18002b440 + uVar4 * 8);
    uVar2 = FUN_18001e860(param_1,param_3,param_2,param_4,dVar1,0);
    if ((int)uVar2 == 0) {
      *(double *)(param_1 + 0x40) = dVar1;
      return;
    }
    uVar3 = (int)uVar4 + 1;
    uVar4 = (ulonglong)uVar3;
  } while ((int)uVar3 < 0xd);
  return;
}



/* ========================================================================
   ENTRY: 18001c760
   NAME : FUN_18001c760
   SIG  : int __fastcall FUN_18001c760(longlong param_1)
   ======================================================================== */

int FUN_18001c760(longlong param_1)

{
  uint uVar1;
  uint uVar2;
  int iVar3;
  uint uVar4;
  longlong lVar5;
  int iVar6;
  undefined8 uVar7;
  uint uVar8;
  uint uVar9;
  
  uVar1 = *(uint *)(param_1 + 0x24);
  uVar2 = *(uint *)(param_1 + 0x10);
  lVar5 = *(longlong *)(param_1 + 0x18);
  uVar7 = FUN_18001bea0(lVar5,uVar2,uVar1);
  if ((int)uVar7 == 0) {
    return 0;
  }
  iVar3 = *(int *)(param_1 + 0x28);
  iVar6 = -*(int *)(param_1 + 0x2c);
  uVar4 = *(uint *)(param_1 + 0x20);
  uVar9 = 0;
  if (uVar1 + 1 < uVar4) {
    uVar9 = uVar1 + 1;
  }
  while ((iVar6 = iVar3 + iVar6, uVar9 != uVar1 &&
         (uVar7 = FUN_18001bea0(lVar5,uVar2,uVar9), (int)uVar7 != 0))) {
    uVar8 = uVar9 + 1;
    uVar9 = 0;
    if (uVar8 < uVar4) {
      uVar9 = uVar8;
    }
  }
  return iVar6;
}



/* ========================================================================
   ENTRY: 18001c810
   NAME : FUN_18001c810
   SIG  : undefined __fastcall FUN_18001c810(double * param_1, double * param_2)
   ======================================================================== */

void FUN_18001c810(double *param_1,double *param_2)

{
  int iVar1;
  double dVar2;
  
  iVar1 = FUN_180020730();
  if (iVar1 < 2) {
    dVar2 = 0.2;
  }
  else if (iVar1 == 2) {
    dVar2 = 0.4;
  }
  else {
    if (iVar1 < 3) goto LAB_18001c859;
    dVar2 = 0.09;
  }
  *param_1 = dVar2;
LAB_18001c859:
  *param_2 = *param_1 + *param_1;
  return;
}



/* ========================================================================
   ENTRY: 18001c870
   NAME : FUN_18001c870
   SIG  : int __fastcall FUN_18001c870(LPCSTR param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_18001c870(LPCSTR param_1)

{
  DWORD DVar1;
  int iVar2;
  undefined1 auStack_58 [32];
  CHAR local_38 [32];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_58;
  DVar1 = GetEnvironmentVariableA(param_1,local_38,0x20);
  iVar2 = -1;
  if ((DVar1 != 0) && (DVar1 < 0x20)) {
    iVar2 = atoi(local_38);
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 18001c910
   NAME : FUN_18001c910
   SIG  : undefined __fastcall FUN_18001c910(longlong param_1)
   ======================================================================== */

void FUN_18001c910(longlong param_1)

{
  int iVar1;
  
  iVar1 = FUN_18001c870("PA_RECOMMENDED_INPUT_DEVICE");
  if (((-1 < iVar1) && (iVar1 < *(int *)(param_1 + 0x18))) &&
     (0 < *(int *)(*(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)iVar1 * 8) + 0x14))) {
    *(int *)(param_1 + 0x1c) = iVar1;
  }
  iVar1 = FUN_18001c870("PA_RECOMMENDED_OUTPUT_DEVICE");
  if (((-1 < iVar1) && (iVar1 < *(int *)(param_1 + 0x18))) &&
     (0 < *(int *)(*(longlong *)(*(longlong *)(param_1 + 0x28) + (longlong)iVar1 * 8) + 0x18))) {
    *(int *)(param_1 + 0x20) = iVar1;
  }
  return;
}



/* ========================================================================
   ENTRY: 18001c980
   NAME : FUN_18001c980
   SIG  : undefined8 __fastcall FUN_18001c980(longlong param_1, longlong param_2, uint param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001c980(longlong param_1,longlong param_2,uint param_3,undefined4 *param_4)

{
  char *pcVar1;
  char cVar2;
  bool bVar3;
  MMRESULT MVar4;
  longlong lVar5;
  LPSTR pCVar6;
  undefined8 uVar7;
  uint uVar8;
  LPSTR pCVar9;
  undefined1 auStack_a8 [32];
  tagWAVEINCAPSW local_88;
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStack_a8;
  *param_4 = 0;
  MVar4 = waveInGetDevCapsW((ulonglong)param_3,&local_88,0x50);
  if (MVar4 == 7) {
    return 0xffffd8f8;
  }
  if (MVar4 == 0) {
    lVar5 = FUN_18001ff00(local_88.szPname);
    if (param_3 == 0xffffffff) {
      pCVar6 = (LPSTR)FUN_1800012e0(*(int **)(param_1 + 0x108),(int)(lVar5 + 10U));
      if (pCVar6 == (LPSTR)0x0) {
        return 0xffffd8f8;
      }
      FUN_18001c5c0(pCVar6,lVar5 + 10U,local_88.szPname);
      pCVar9 = pCVar6 + -1;
      do {
        pcVar1 = pCVar9 + 1;
        pCVar9 = pCVar9 + 1;
      } while (*pcVar1 != '\0');
      lVar5 = 0;
      do {
        cVar2 = "? - Input"[lVar5 + 1];
        pCVar9[lVar5] = cVar2;
        lVar5 = lVar5 + 1;
      } while (cVar2 != '\0');
    }
    else {
      pCVar6 = (LPSTR)FUN_1800012e0(*(int **)(param_1 + 0x108),(int)(lVar5 + 1U));
      if (pCVar6 == (LPSTR)0x0) {
        return 0xffffd8f8;
      }
      FUN_18001c5c0(pCVar6,lVar5 + 1U,local_88.szPname);
    }
    *(LPSTR *)(param_2 + 8) = pCVar6;
    bVar3 = 0xfe < (ushort)(local_88.wChannels - 1);
    if (bVar3) {
      uVar8 = 2;
    }
    else {
      uVar8 = (uint)local_88.wChannels;
    }
    *(uint *)(param_2 + 0x14) = uVar8;
    *(bool *)(param_2 + 0x4c) = !bVar3;
    uVar7 = FUN_18001ec20(param_3,(uint *)(param_2 + 0x14));
    *(DWORD *)(param_2 + 0x48) = local_88.dwFormats;
    *(char *)(param_2 + 0x4c) = (char)uVar7;
    FUN_18001c6d0(param_2,param_3,FUN_18001e9a0,*(int *)(param_2 + 0x14));
    *param_4 = 1;
    return 0;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001cb00
   NAME : FUN_18001cb00
   SIG  : undefined8 __fastcall FUN_18001cb00(longlong param_1, longlong param_2, uint param_3, undefined4 * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001cb00(longlong param_1,longlong param_2,uint param_3,undefined4 *param_4)

{
  char *pcVar1;
  char cVar2;
  bool bVar3;
  MMRESULT MVar4;
  longlong lVar5;
  LPSTR pCVar6;
  undefined8 uVar7;
  uint uVar8;
  LPSTR pCVar9;
  undefined1 auStack_b8 [32];
  tagWAVEOUTCAPSW local_98;
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStack_b8;
  *param_4 = 0;
  MVar4 = waveOutGetDevCapsW((ulonglong)param_3,&local_98,0x54);
  if (MVar4 == 7) {
    return 0xffffd8f8;
  }
  if (MVar4 == 0) {
    lVar5 = FUN_18001ff00(local_98.szPname);
    if (param_3 == 0xffffffff) {
      pCVar6 = (LPSTR)FUN_1800012e0(*(int **)(param_1 + 0x108),(int)(lVar5 + 0xbU));
      if (pCVar6 == (LPSTR)0x0) {
        return 0xffffd8f8;
      }
      FUN_18001c5c0(pCVar6,lVar5 + 0xbU,local_98.szPname);
      pCVar9 = pCVar6 + -1;
      do {
        pcVar1 = pCVar9 + 1;
        pCVar9 = pCVar9 + 1;
      } while (*pcVar1 != '\0');
      lVar5 = 0;
      do {
        cVar2 = " - Output"[lVar5];
        pCVar9[lVar5] = cVar2;
        lVar5 = lVar5 + 1;
      } while (cVar2 != '\0');
    }
    else {
      pCVar6 = (LPSTR)FUN_1800012e0(*(int **)(param_1 + 0x108),(int)(lVar5 + 1U));
      if (pCVar6 == (LPSTR)0x0) {
        return 0xffffd8f8;
      }
      FUN_18001c5c0(pCVar6,lVar5 + 1U,local_98.szPname);
    }
    uVar8 = (uint)local_98.wChannels;
    *(LPSTR *)(param_2 + 8) = pCVar6;
    bVar3 = 0xfe < (ushort)(local_98.wChannels - 1);
    if (bVar3) {
      uVar8 = 2;
    }
    *(uint *)(param_2 + 0x18) = uVar8;
    *(bool *)(param_2 + 0x4d) = !bVar3;
    uVar7 = FUN_18001ecd0(param_3,(uint *)(param_2 + 0x18));
    if (((int)uVar7 != 0) && (*(char *)(param_2 + 0x4d) == '\0')) {
      *(undefined1 *)(param_2 + 0x4d) = 1;
    }
    *(DWORD *)(param_2 + 0x48) = local_98.dwFormats;
    FUN_18001c6d0(param_2,param_3,FUN_18001eae0,*(int *)(param_2 + 0x18));
    *param_4 = 1;
    return 0;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001cc80
   NAME : FUN_18001cc80
   SIG  : undefined __fastcall FUN_18001cc80(undefined8 * param_1)
   ======================================================================== */

void FUN_18001cc80(undefined8 *param_1)

{
  *param_1 = 0;
  param_1[1] = 0;
  *(undefined4 *)(param_1 + 2) = 0;
  param_1[3] = 0;
  *(undefined4 *)(param_1 + 4) = 0;
  return;
}



/* ========================================================================
   ENTRY: 18001cca0
   NAME : FUN_18001cca0
   SIG  : ulonglong __fastcall FUN_18001cca0(longlong param_1, DWORD_PTR * param_2, byte param_3, uint param_4, double param_5, longlong param_6, int param_7, undefined4 param_8, int param_9)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18001cca0(longlong param_1,DWORD_PTR *param_2,byte param_3,uint param_4,double param_5
                       ,longlong param_6,int param_7,undefined4 param_8,int param_9)

{
  LPHWAVEOUT phwo;
  UINT uDeviceID;
  MMRESULT mmrError;
  ulonglong uVar1;
  DWORD_PTR DVar2;
  undefined8 uVar3;
  uint uVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  int iVar7;
  undefined1 auStackY_3f8 [32];
  WAVEFORMATEX local_3a0 [3];
  CHAR local_368 [256];
  WCHAR local_268 [256];
  ulonglong local_68;
  
  local_68 = DAT_18002b580 ^ (ulonglong)auStackY_3f8;
  uVar5 = 0;
  uVar1 = FUN_18001c620(param_2,(LPSECURITY_ATTRIBUTES)0x0,0,0,(LPCWSTR)0x0);
  uVar6 = uVar1 & 0xffffffff;
  if ((int)uVar1 == 0) {
    DVar2 = FUN_180020230(param_7 * 8);
    param_2[1] = DVar2;
    if (DVar2 != 0) {
      *(int *)(param_2 + 2) = param_7;
      if (0 < param_7) {
        if (param_9 == 0) {
          do {
            iVar7 = (int)uVar5;
            uVar4 = iVar7 + 1;
            uVar5 = (ulonglong)uVar4;
            *(undefined8 *)(param_2[1] + (longlong)iVar7 * 8) = 0;
          } while ((int)uVar4 < param_7);
        }
        else {
          do {
            uVar4 = (int)uVar5 + 1;
            *(undefined8 *)(param_2[1] + uVar5 * 8) = 0;
            uVar5 = (ulonglong)uVar4;
          } while ((int)uVar4 < param_7);
        }
      }
      uVar3 = FUN_18001f0d0(param_4,param_3);
      uVar5 = 0;
      if (0 < param_7) {
        do {
          DVar2 = 0;
          uDeviceID = FUN_18001d510(param_1,*(int *)(param_6 + uVar5 * 8));
          uVar1 = DVar2 & 0xffffffff;
          if ((int)DVar2 == 0) {
            FUN_180020830(&local_3a0[0].wFormatTag,*(int *)(param_6 + 4 + uVar5 * 8),param_4,
                          (uint)uVar3,param_5,param_8);
            uVar1 = 0;
            DVar2 = uVar1;
          }
          else if ((int)DVar2 == 1) goto LAB_18001ce5b;
          while( true ) {
            phwo = (LPHWAVEOUT)(param_2[1] + uVar5 * 8);
            if (param_9 == 0) {
              mmrError = waveOutOpen(phwo,uDeviceID,local_3a0,*param_2,DVar2,0x50000);
            }
            else {
              mmrError = waveInOpen((LPHWAVEIN)phwo,uDeviceID,local_3a0,*param_2,DVar2,0x50000);
            }
            if (mmrError == 0) break;
            if ((int)uVar1 != 0) {
              if (mmrError != 2) {
                if ((mmrError == 4) || (mmrError == 6)) {
                  uVar6 = 0xffffd8ff;
                  goto LAB_18001ceb7;
                }
                if (mmrError == 7) goto LAB_18001ceb2;
              }
              uVar6 = 0xffffd8f1;
              if (param_9 == 0) {
                waveOutGetErrorTextW(mmrError,local_268,0x100);
              }
              else {
                waveInGetErrorTextW(mmrError,local_268,0x100);
              }
              WideCharToMultiByte(0xfde9,0,local_268,-1,local_368,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
              FUN_180003c90(2,mmrError,local_368);
              goto LAB_18001ceb7;
            }
            uVar1 = 1;
LAB_18001ce5b:
            FUN_1800207c0(&local_3a0[0].wFormatTag,*(int *)(param_6 + 4 + uVar5 * 8),param_4,
                          (short)uVar3,param_5);
            DVar2 = 0;
          }
          uVar4 = (int)uVar5 + 1;
          uVar5 = (ulonglong)uVar4;
        } while ((int)uVar4 < param_7);
      }
      return 0;
    }
LAB_18001ceb2:
    uVar6 = 0xffffd8f8;
  }
LAB_18001ceb7:
  FUN_18001fb30(param_2,param_9,1);
  return uVar6;
}



/* ========================================================================
   ENTRY: 18001cf90
   NAME : FUN_18001cf90
   SIG  : undefined8 __fastcall FUN_18001cf90(longlong param_1, int param_2, uint param_3, int param_4, longlong param_5, int param_6)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8
FUN_18001cf90(longlong param_1,int param_2,uint param_3,int param_4,longlong param_5,int param_6)

{
  HWAVEOUT hwo;
  int iVar1;
  MMRESULT mmrError;
  longlong lVar2;
  undefined8 uVar3;
  longlong lVar4;
  LPSTR pCVar5;
  int iVar6;
  uint uVar7;
  ulonglong uVar8;
  LPWAVEHDR pwh;
  ulonglong uVar9;
  DWORD DVar10;
  undefined1 auStackY_3b8 [32];
  int local_378;
  CHAR local_358 [256];
  WCHAR local_258 [256];
  ulonglong local_58;
  
  local_58 = DAT_18002b580 ^ (ulonglong)auStackY_3b8;
  lVar2 = FUN_180020230(*(int *)(param_1 + 0x10) << 3);
  *(longlong *)(param_1 + 0x18) = lVar2;
  if (lVar2 == 0) {
LAB_18001cff1:
    FUN_18001fca0(param_1,param_6);
    uVar3 = 0xffffd8f8;
  }
  else {
    iVar1 = *(int *)(param_1 + 0x10);
    iVar6 = 0;
    if (0 < iVar1) {
      do {
        lVar2 = (longlong)iVar6;
        iVar6 = iVar6 + 1;
        *(undefined8 *)(*(longlong *)(param_1 + 0x18) + lVar2 * 8) = 0;
        iVar1 = *(int *)(param_1 + 0x10);
      } while (iVar6 < iVar1);
    }
    *(int *)(param_1 + 0x20) = param_2;
    if (0 < iVar1) {
      local_378 = 0;
      do {
        uVar3 = Pa_GetSampleSize(param_3);
        lVar2 = (longlong)local_378 * 8;
        DVar10 = (int)uVar3 * *(int *)(param_5 + 4 + lVar2) * param_4;
        if ((int)DVar10 < 0) {
          FUN_18001fca0(param_1,param_6);
          return 0xffffd8fe;
        }
        lVar4 = FUN_180020230(param_2 * 0x30);
        if (lVar4 == 0) goto LAB_18001cff1;
        uVar9 = 0;
        uVar8 = uVar9;
        if (param_2 < 1) {
          *(longlong *)(*(longlong *)(param_1 + 0x18) + lVar2) = lVar4;
        }
        else {
          do {
            uVar7 = (int)uVar8 + 1;
            *(undefined8 *)(lVar4 + uVar8 * 0x30) = 0;
            uVar8 = (ulonglong)uVar7;
          } while ((int)uVar7 < param_2);
          *(longlong *)(*(longlong *)(param_1 + 0x18) + lVar2) = lVar4;
          do {
            pCVar5 = (LPSTR)FUN_180020230(DVar10);
            pwh = (LPWAVEHDR)((longlong)(int)uVar9 * 0x30 + lVar4);
            pwh->lpData = pCVar5;
            if (pCVar5 == (LPSTR)0x0) goto LAB_18001cff1;
            pwh->dwBufferLength = DVar10;
            pwh->dwUser = 0xffffffff;
            hwo = *(HWAVEOUT *)(*(longlong *)(param_1 + 8) + lVar2);
            if (param_6 == 0) {
              mmrError = waveOutPrepareHeader(hwo,pwh,0x30);
            }
            else {
              mmrError = waveInPrepareHeader((HWAVEIN)hwo,pwh,0x30);
            }
            if (mmrError != 0) {
              waveInGetErrorTextW(mmrError,local_258,0x100);
              WideCharToMultiByte(0xfde9,0,local_258,-1,local_358,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
              FUN_180003c90(2,mmrError,local_358);
              FUN_18001fca0(param_1,param_6);
              return 0xffffd8f1;
            }
            uVar7 = (int)uVar9 + 1;
            uVar9 = (ulonglong)uVar7;
            pwh->dwUser = (longlong)*(int *)(param_5 + 4 + lVar2);
          } while ((int)uVar7 < param_2);
        }
        local_378 = local_378 + 1;
      } while (local_378 < *(int *)(param_1 + 0x10));
    }
    uVar3 = 0;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001d230
   NAME : FUN_18001d230
   SIG  : ulonglong __fastcall FUN_18001d230(longlong param_1, int * param_2, int * param_3, double param_4)
   ======================================================================== */

ulonglong FUN_18001d230(longlong param_1,int *param_2,int *param_3,double param_4)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  undefined4 uVar4;
  ulonglong uVar5;
  undefined8 uVar6;
  ulonglong uVar7;
  byte bVar8;
  int iVar9;
  uint uVar10;
  longlong lVar11;
  longlong lVar12;
  
  if (param_2 != (int *)0x0) {
    if ((param_2[2] & 0x10000U) != 0) {
      return 0xffffd8f6;
    }
    lVar11 = (longlong)*param_2;
    iVar1 = param_2[1];
    lVar3 = *(longlong *)(param_2 + 6);
    if (*param_2 == -2) {
      if ((lVar3 != 0) && ((*(byte *)(lVar3 + 0xc) & 2) != 0)) {
        iVar9 = 0;
        uVar7 = 0;
        if (*(int *)(lVar3 + 0x20) != 0) {
          do {
            iVar2 = *(int *)(*(longlong *)(lVar3 + 0x18) + 4 + uVar7 * 8);
            if (iVar2 < 1) {
              return 0xffffd8f2;
            }
            lVar12 = (longlong)*(int *)(*(longlong *)(lVar3 + 0x18) + uVar7 * 8);
            lVar11 = *(longlong *)(*(longlong *)(param_1 + 0x28) + lVar12 * 8);
            uVar5 = FUN_18001d4c0(lVar11,iVar2);
            if ((int)uVar5 != 0) {
              return uVar5;
            }
            uVar4 = FUN_18001d510(param_1,(int)lVar12);
            uVar6 = FUN_18001e860(lVar11,FUN_18001e9a0,uVar4,iVar2,param_4,
                                  (byte)*(undefined4 *)(lVar3 + 0xc));
            if ((int)uVar6 != 0) {
              return 0xffffd8f3;
            }
            iVar9 = iVar9 + iVar2;
            uVar10 = (int)uVar7 + 1;
            uVar7 = (ulonglong)uVar10;
          } while (uVar10 < *(uint *)(lVar3 + 0x20));
        }
        if (iVar9 != iVar1) {
          return 0xffffd900;
        }
        goto LAB_18001d374;
      }
    }
    else if ((lVar3 != 0) && ((*(byte *)(lVar3 + 0xc) & 2) != 0)) {
      return 0xffffd900;
    }
    lVar12 = *(longlong *)(*(longlong *)(param_1 + 0x28) + lVar11 * 8);
    uVar7 = FUN_18001d4c0(lVar12,iVar1);
    if ((int)uVar7 != 0) {
      return uVar7;
    }
    uVar4 = FUN_18001d510(param_1,(int)lVar11);
    if (lVar3 == 0) {
      bVar8 = 0;
    }
    else {
      bVar8 = (byte)*(undefined4 *)(lVar3 + 0xc);
    }
    uVar6 = FUN_18001e860(lVar12,FUN_18001e9a0,uVar4,iVar1,param_4,bVar8);
    if ((int)uVar6 != 0) {
      return 0xffffd8f3;
    }
  }
LAB_18001d374:
  if (param_3 == (int *)0x0) {
    return 0;
  }
  if ((param_3[2] & 0x10000U) != 0) {
    return 0xffffd8f6;
  }
  lVar11 = (longlong)*param_3;
  iVar1 = param_3[1];
  lVar3 = *(longlong *)(param_3 + 6);
  if (*param_3 == -2) {
    if ((lVar3 == 0) || ((*(byte *)(lVar3 + 0xc) & 2) == 0)) {
LAB_18001d45e:
      lVar12 = *(longlong *)(*(longlong *)(param_1 + 0x28) + lVar11 * 8);
      uVar7 = FUN_18001d4e0(lVar12,iVar1);
      if ((int)uVar7 == 0) {
        uVar4 = FUN_18001d510(param_1,(int)lVar11);
        if (lVar3 == 0) {
          bVar8 = 0;
        }
        else {
          bVar8 = (byte)*(undefined4 *)(lVar3 + 0xc);
        }
        uVar6 = FUN_18001e860(lVar12,FUN_18001eae0,uVar4,iVar1,param_4,bVar8);
        return (ulonglong)(-(uint)((int)uVar6 != 0) & 0xffffd8f3);
      }
      return uVar7;
    }
    iVar9 = 0;
    uVar7 = 0;
    if (*(int *)(lVar3 + 0x20) != 0) {
      do {
        iVar2 = *(int *)(*(longlong *)(lVar3 + 0x18) + 4 + uVar7 * 8);
        if (iVar2 < 1) {
          return 0xffffd8f2;
        }
        lVar12 = (longlong)*(int *)(*(longlong *)(lVar3 + 0x18) + uVar7 * 8);
        lVar11 = *(longlong *)(*(longlong *)(param_1 + 0x28) + lVar12 * 8);
        uVar5 = FUN_18001d4e0(lVar11,iVar2);
        if ((int)uVar5 != 0) {
          return uVar5;
        }
        uVar4 = FUN_18001d510(param_1,(int)lVar12);
        uVar6 = FUN_18001e860(lVar11,FUN_18001eae0,uVar4,iVar2,param_4,
                              (byte)*(undefined4 *)(lVar3 + 0xc));
        if ((int)uVar6 != 0) {
          return 0xffffd8f3;
        }
        iVar9 = iVar9 + iVar2;
        uVar10 = (int)uVar7 + 1;
        uVar7 = (ulonglong)uVar10;
      } while (uVar10 < *(uint *)(lVar3 + 0x20));
    }
    if (iVar9 == iVar1) {
      return 0;
    }
  }
  else if ((lVar3 == 0) || ((*(byte *)(lVar3 + 0xc) & 2) == 0)) goto LAB_18001d45e;
  return 0xffffd900;
}



/* ========================================================================
   ENTRY: 18001d4c0
   NAME : FUN_18001d4c0
   SIG  : undefined8 __fastcall FUN_18001d4c0(longlong param_1, int param_2)
   ======================================================================== */

undefined8 FUN_18001d4c0(longlong param_1,int param_2)

{
  undefined8 uVar1;
  
  uVar1 = 0;
  if (((0 < param_2) && (*(char *)(param_1 + 0x4c) != '\0')) &&
     (uVar1 = 0, *(int *)(param_1 + 0x14) < param_2)) {
    uVar1 = 0xffffd8f2;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001d4e0
   NAME : FUN_18001d4e0
   SIG  : undefined8 __fastcall FUN_18001d4e0(longlong param_1, int param_2)
   ======================================================================== */

undefined8 FUN_18001d4e0(longlong param_1,int param_2)

{
  undefined8 uVar1;
  
  uVar1 = 0;
  if (((0 < param_2) && (*(char *)(param_1 + 0x4d) != '\0')) &&
     (uVar1 = 0, *(int *)(param_1 + 0x18) < param_2)) {
    uVar1 = 0xffffd8f2;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001d510
   NAME : FUN_18001d510
   SIG  : undefined4 __fastcall FUN_18001d510(longlong param_1, int param_2)
   ======================================================================== */

undefined4 FUN_18001d510(longlong param_1,int param_2)

{
  return *(undefined4 *)(*(longlong *)(param_1 + 0x118) + (longlong)param_2 * 4);
}



/* ========================================================================
   ENTRY: 18001d520
   NAME : FUN_18001d520
   SIG  : undefined8 __fastcall FUN_18001d520(longlong param_1)
   ======================================================================== */

undefined8 FUN_18001d520(longlong param_1)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  if (*(longlong *)(param_1 + 8) != 0) {
    uVar3 = 0;
    if (*(uint *)(param_1 + 0x20) != 0) {
      do {
        if (*(uint *)(param_1 + 0x10) != 0) {
          uVar2 = 0;
          do {
            if ((*(byte *)(*(longlong *)(*(longlong *)(param_1 + 0x18) + uVar2 * 8) + 0x18 +
                          uVar3 * 0x30) & 1) == 0) {
              return 0;
            }
            uVar1 = (int)uVar2 + 1;
            uVar2 = (ulonglong)uVar1;
          } while (uVar1 < *(uint *)(param_1 + 0x10));
        }
        uVar1 = (int)uVar3 + 1;
        uVar3 = (ulonglong)uVar1;
      } while (uVar1 < *(uint *)(param_1 + 0x20));
    }
  }
  return 1;
}



/* ========================================================================
   ENTRY: 18001d590
   NAME : FUN_18001d590
   SIG  : ulonglong __fastcall FUN_18001d590(int * param_1, undefined8 * param_2, int * param_3, int * param_4, double param_5, undefined4 param_6, uint param_7, longlong param_8, undefined8 param_9)
   ======================================================================== */

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */
/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong FUN_18001d590(int *param_1,undefined8 *param_2,int *param_3,int *param_4,double param_5,
                       undefined4 param_6,uint param_7,longlong param_8,undefined8 param_9)

{
  undefined4 uVar1;
  HANDLE hObject;
  double dVar2;
  int iVar3;
  int iVar4;
  undefined4 *puVar5;
  bool bVar6;
  uint uVar7;
  uint extraout_EAX;
  int iVar8;
  int iVar9;
  undefined7 extraout_var;
  ulonglong uVar10;
  undefined8 uVar11;
  undefined4 *puVar12;
  longlong lVar13;
  int *piVar14;
  int *piVar15;
  undefined1 *puVar16;
  undefined1 *puVar17;
  undefined1 *puVar18;
  ulonglong uVar19;
  undefined4 *puVar20;
  uint uVar21;
  undefined4 uVar22;
  undefined4 uVar23;
  undefined4 uVar24;
  undefined4 uVar25;
  undefined8 uStackY_160;
  undefined1 local_158 [32];
  int local_e8 [11];
  uint local_bc;
  uint local_b8 [2];
  int *local_b0;
  uint local_a8;
  uint local_a4;
  int local_a0;
  uint local_9c;
  uint local_98;
  uint local_94;
  undefined4 *local_90;
  undefined8 *local_88;
  ulonglong local_80;
  
  local_80 = DAT_18002b580 ^ (ulonglong)local_e8;
  piVar14 = (int *)0x0;
  local_a0 = 0;
  local_a4 = 0;
  local_b8[0] = 0;
  local_e8[9] = 0;
  local_e8[0]._0_1_ = 1;
  uStackY_160 = 0x18001d5f2;
  local_b0 = param_1;
  local_88 = param_2;
  bVar6 = FUN_18001dcf0();
  uVar22 = 0;
  uVar23 = 0;
  uVar21 = 0x10;
  if ((int)CONCAT71(extraout_var,bVar6) != 0) {
    uVar21 = 0x5e;
  }
  uVar19 = 0xffffffffffffff0;
  if (param_3 != (int *)0x0) {
    piVar14 = *(int **)(param_3 + 6);
    local_e8[3] = 1;
    uStackY_160 = 0x18001d640;
    uVar10 = FUN_18001feb0(param_3,piVar14,&local_a0,(undefined1 *)local_e8,local_e8 + 3);
    param_1 = local_b0;
    iVar8 = local_e8[3];
    puVar18 = local_158;
    if ((int)uVar10 != 0) goto LAB_18001dcba;
    local_a4 = local_e8[3];
    uVar10 = (ulonglong)(uint)local_e8[3] * 8 + 0xf;
    if (uVar10 <= (ulonglong)(uint)local_e8[3] * 8) {
      uVar10 = uVar19;
    }
    uStackY_160 = 0x18001d66f;
    lVar13 = -(uVar10 & 0xfffffffffffffff0);
    puVar16 = local_158 + lVar13;
    puVar17 = local_158 + lVar13;
    puVar20 = (undefined4 *)((longlong)local_e8 + lVar13);
    local_90 = puVar20;
    if (puVar20 != (undefined4 *)0x0) {
      uVar24 = (undefined4)*(undefined8 *)(param_3 + 4);
      uVar25 = (undefined4)((ulonglong)*(undefined8 *)(param_3 + 4) >> 0x20);
      local_e8[3] = param_3[1];
      local_bc = param_3[2];
      *(int *)(&stack0xfffffffffffffec8 + lVar13) = iVar8;
      *(undefined8 *)(local_158 + lVar13 + -8) = 0x18001d6b2;
      uVar10 = FUN_18001f000(param_1,param_3,(longlong)piVar14,puVar20,
                             *(uint *)(&stack0xfffffffffffffec8 + lVar13));
      puVar18 = local_158 + lVar13;
      if ((int)uVar10 != 0) goto LAB_18001dcba;
      *(undefined8 *)(local_158 + lVar13 + -8) = 0x18001d6c8;
      uVar10 = FUN_18001fdf0((longlong)param_1,(longlong)puVar20,iVar8);
      uVar7 = local_bc;
      puVar18 = local_158 + lVar13;
      if ((int)uVar10 != 0) goto LAB_18001dcba;
      *(undefined8 *)(local_158 + lVar13 + -8) = 0x18001d6db;
      local_e8[5] = FUN_1800030c0(uVar21,uVar7);
      if (iVar8 == 1) {
        if ((piVar14 == (int *)0x0) || ((*(byte *)(piVar14 + 3) & 4) == 0)) {
          uVar1 = *(undefined4 *)((longlong)local_e8 + lVar13 + 4);
          *(undefined8 *)(local_158 + lVar13 + -8) = 0x18001d71f;
          uVar11 = FUN_180020750(uVar1);
          local_e8[7] = (int)uVar11;
        }
        else {
          local_e8[7] = piVar14[9];
          puVar17 = local_158 + lVar13;
        }
        goto LAB_18001d738;
      }
      local_e8[7] = 0;
      puVar17 = local_158 + lVar13;
      goto LAB_18001d73b;
    }
LAB_18001d7a4:
    uVar10 = 0xffffd8f8;
    puVar18 = puVar16;
    goto LAB_18001dcba;
  }
  uVar24 = 0;
  uVar25 = 0;
  local_90 = (undefined4 *)0x0;
  local_e8[3] = 0;
  local_bc = 0;
  local_e8[5] = 0;
  puVar17 = local_158;
  local_e8[7] = local_e8[10];
LAB_18001d738:
LAB_18001d73b:
  piVar15 = (int *)0x0;
  uVar19 = 0xffffffffffffff0;
  if (param_4 == (int *)0x0) {
    local_e8[6] = 0;
    puVar20 = (undefined4 *)0x0;
    local_e8[4] = 0;
    local_e8[2] = 0;
    local_e8[8] = local_e8[10];
LAB_18001d855:
  }
  else {
    piVar15 = *(int **)(param_4 + 6);
    local_e8[2] = 1;
    *(int **)(puVar17 + 0x20) = local_e8 + 2;
    *(undefined8 *)(puVar17 + -8) = 0x18001d76b;
    uVar10 = FUN_18001feb0(param_4,piVar15,(int *)local_b8,(undefined1 *)local_e8,
                           *(int **)(puVar17 + 0x20));
    puVar18 = puVar17;
    if ((int)uVar10 != 0) goto LAB_18001dcba;
    local_e8[9] = local_e8[2];
    uVar10 = (ulonglong)(uint)local_e8[2] * 8 + 0xf;
    if (uVar10 <= (ulonglong)(uint)local_e8[2] * 8) {
      uVar10 = uVar19;
    }
    *(undefined8 *)(puVar17 + -8) = 0x18001d797;
    lVar13 = -(uVar10 & 0xfffffffffffffff0);
    puVar16 = puVar17 + lVar13;
    puVar20 = (undefined4 *)(puVar17 + lVar13 + 0x70);
    if (puVar20 == (undefined4 *)0x0) goto LAB_18001d7a4;
    uVar22 = (undefined4)*(undefined8 *)(param_4 + 4);
    uVar23 = (undefined4)((ulonglong)*(undefined8 *)(param_4 + 4) >> 0x20);
    local_e8[4] = param_4[1];
    uVar7 = param_4[2];
    *(int *)(puVar17 + lVar13 + 0x20) = local_e8[2];
    local_e8[2] = uVar7;
    *(undefined8 *)(puVar17 + lVar13 + -8) = 0x18001d7d4;
    uVar10 = FUN_18001f000(param_1,param_4,(longlong)piVar15,puVar20,
                           *(uint *)(puVar17 + lVar13 + 0x20));
    iVar8 = local_e8[9];
    puVar18 = puVar17 + lVar13;
    if ((int)uVar10 != 0) goto LAB_18001dcba;
    *(undefined8 *)(puVar17 + lVar13 + -8) = 0x18001d7eb;
    uVar10 = FUN_18001fe50((longlong)param_1,(longlong)puVar20,iVar8);
    iVar8 = local_e8[2];
    puVar18 = puVar17 + lVar13;
    if ((int)uVar10 != 0) goto LAB_18001dcba;
    *(undefined8 *)(puVar17 + lVar13 + -8) = 0x18001d7fe;
    local_e8[6] = FUN_1800030c0(uVar21,iVar8);
    if (local_e8[9] == 1) {
      if ((piVar15 == (int *)0x0) || ((*(byte *)(piVar15 + 3) & 4) == 0)) {
        *(undefined8 *)(puVar17 + lVar13 + -8) = 0x18001d835;
        uVar11 = FUN_180020750(*(undefined4 *)(puVar17 + lVar13 + 0x74));
        puVar17 = puVar17 + lVar13;
        local_e8[8] = (uint)uVar11;
      }
      else {
        puVar17 = puVar17 + lVar13;
        local_e8[8] = piVar15[9];
      }
      goto LAB_18001d855;
    }
    local_e8[8] = 0;
    puVar17 = puVar17 + lVar13;
  }
  iVar4 = local_e8[6];
  iVar8 = local_e8[4];
  puVar18 = puVar17;
  if ((param_7 & 0xffff0000) != 0) {
    uVar10 = 0xffffd8f5;
    goto LAB_18001dcba;
  }
  uVar21 = local_b8[0] & 0x30;
  *(undefined4 *)(puVar17 + 0x68) = param_6;
  iVar3 = local_e8[5];
  local_a8 = param_7 | 3;
  if (uVar21 == 0) {
    local_a8 = param_7;
  }
  *(double *)(puVar17 + 0x60) = param_5;
  *(int **)(puVar17 + 0x58) = piVar15;
  *(ulonglong *)(puVar17 + 0x50) = CONCAT44(uVar23,uVar22);
  *(int *)(puVar17 + 0x48) = iVar4;
  *(int *)(puVar17 + 0x40) = iVar8;
  *(int **)(puVar17 + 0x38) = piVar14;
  *(ulonglong *)(puVar17 + 0x30) = CONCAT44(uVar25,uVar24);
  *(int *)(puVar17 + 0x28) = iVar3;
  *(int *)(puVar17 + 0x20) = local_e8[3];
  *(undefined8 *)(puVar17 + -8) = 0x18001d8db;
  uVar19 = FUN_18001bed0(&local_94,(uint *)(local_e8 + 10),&local_98,&local_9c,
                         *(uint *)(puVar17 + 0x20),*(uint *)(puVar17 + 0x28),
                         *(double *)(puVar17 + 0x30),*(longlong *)(puVar17 + 0x38),
                         *(uint *)(puVar17 + 0x40),*(uint *)(puVar17 + 0x48),
                         *(double *)(puVar17 + 0x50),*(longlong *)(puVar17 + 0x58),
                         *(double *)(puVar17 + 0x60),*(uint *)(puVar17 + 0x68));
  uVar10 = uVar19 & 0xffffffff;
  if ((int)uVar19 != 0) goto LAB_18001dcba;
  *(undefined8 *)(puVar17 + -8) = 0x18001d8ef;
  puVar12 = (undefined4 *)FUN_180020230(0x218);
  if (puVar12 == (undefined4 *)0x0) {
    uVar10 = 0xffffd8f8;
    goto LAB_18001dcba;
  }
  local_e8[1] = 0;
  *(undefined8 *)(puVar17 + -8) = 0x18001d912;
  FUN_18001cc80((undefined8 *)(puVar12 + 0x5e));
  *(undefined8 *)(puVar17 + -8) = 0x18001d91e;
  FUN_18001cc80((undefined8 *)(puVar12 + 0x6a));
  *(undefined1 *)(puVar12 + 0x7b) = (undefined1)local_e8[0];
  *(undefined8 *)(puVar12 + 0x76) = 0;
  lVar13 = 0x48;
  if (param_8 == 0) {
    lVar13 = 0xa8;
  }
  *(undefined8 *)(puVar12 + 0x78) = 0;
  *(undefined8 *)(puVar17 + -8) = 0x18001d96a;
  FUN_180006e70(puVar12,lVar13 + (longlong)param_1,param_8,param_9);
  *(undefined8 *)(puVar17 + -8) = 0x18001d976;
  FUN_180003950((double *)(puVar12 + 0x14),param_5);
  uVar21 = local_bc;
  iVar8 = local_e8[3];
  if (param_3 == (int *)0x0) {
    uVar7 = local_e8[10];
    if (param_4 != (int *)0x0) {
      uVar7 = local_98;
    }
  }
  else {
    uVar7 = local_94;
    if ((param_4 != (int *)0x0) && (uVar7 = local_98, local_94 < local_98)) {
      uVar7 = local_94;
    }
  }
  *(undefined8 *)(puVar17 + 0x68) = param_9;
  iVar4 = local_e8[5];
  *(longlong *)(puVar17 + 0x60) = param_8;
  *(undefined4 *)(puVar17 + 0x58) = 0;
  *(uint *)(puVar17 + 0x50) = uVar7;
  *(undefined4 *)(puVar17 + 0x48) = param_6;
  *(uint *)(puVar17 + 0x40) = local_a8;
  iVar3 = local_e8[6];
  *(double *)(puVar17 + 0x38) = param_5;
  *(int *)(puVar17 + 0x30) = iVar3;
  *(int *)(puVar17 + 0x28) = local_e8[2];
  *(int *)(puVar17 + 0x20) = local_e8[4];
  puVar12[0x68] = local_94;
  puVar12[0x74] = local_98;
  *(undefined8 *)(puVar17 + -8) = 0x18001da0e;
  puVar12 = (undefined4 *)
            FUN_180006290(puVar12 + 0x1a,iVar8,uVar21,iVar4,*(uint *)(puVar17 + 0x20),
                          *(uint *)(puVar17 + 0x28),*(uint *)(puVar17 + 0x30),
                          *(double *)(puVar17 + 0x38),*(uint *)(puVar17 + 0x40),
                          *(uint *)(puVar17 + 0x48),*(uint *)(puVar17 + 0x50),
                          *(uint *)(puVar17 + 0x58),*(longlong *)(puVar17 + 0x60),
                          *(undefined8 *)(puVar17 + 0x68));
  uVar10 = (ulonglong)extraout_EAX;
  iVar8 = local_e8[1];
  if (extraout_EAX == 0) {
    local_e8[1] = 1;
    *(undefined8 *)(puVar17 + -8) = 0x18001da28;
    iVar8 = FUN_180006270((longlong)(puVar12 + 0x1a));
    *(double *)(puVar12 + 0xe) = (double)(iVar8 + local_94) / param_5;
    *(undefined8 *)(puVar17 + -8) = 0x18001da45;
    iVar9 = FUN_180006280((longlong)(puVar12 + 0x1a));
    iVar3 = local_a0;
    piVar14 = local_b0;
    iVar4 = local_e8[7];
    iVar8 = local_e8[5];
    *(double *)(puVar12 + 0x12) = param_5;
    *(double *)(puVar12 + 0x10) = (double)(iVar9 + (local_9c - 1) * local_98) / param_5;
    dVar2 = DAT_1800239c8;
    if (((local_a8 & 8) == 0) || (param_8 == 0)) {
      uVar22 = 0;
    }
    else {
      uVar22 = 1;
    }
    puVar12[0x5c] = uVar22;
    puVar12[0x80] = 1;
    puVar12[0x7f] =
         (int)(longlong)
              ((double)(uint)puVar12[0x1b] * *(double *)(puVar12 + 0x56) * _DAT_180023d30 * dVar2);
    puVar12[0x81] = 0;
    if (param_3 != (int *)0x0) {
      *(undefined4 *)(puVar17 + 0x40) = 1;
      *(int *)(puVar17 + 0x38) = iVar4;
      *(uint *)(puVar17 + 0x30) = local_a4;
      *(undefined4 **)(puVar17 + 0x28) = local_90;
      *(double *)(puVar17 + 0x20) = param_5;
      *(undefined8 *)(puVar17 + -8) = 0x18001db0d;
      uVar19 = FUN_18001cca0((longlong)piVar14,(DWORD_PTR *)(puVar12 + 0x5e),(byte)iVar3,iVar8,
                             *(double *)(puVar17 + 0x20),*(longlong *)(puVar17 + 0x28),
                             *(int *)(puVar17 + 0x30),*(undefined4 *)(puVar17 + 0x38),
                             *(int *)(puVar17 + 0x40));
      uVar10 = uVar19 & 0xffffffff;
      iVar8 = local_e8[1];
      if ((int)uVar19 != 0) goto LAB_18001dc45;
    }
    piVar14 = local_b0;
    uVar21 = local_b8[0];
    iVar4 = local_e8[8];
    iVar8 = local_e8[6];
    if (param_4 != (int *)0x0) {
      *(undefined4 *)(puVar17 + 0x40) = 0;
      *(int *)(puVar17 + 0x38) = iVar4;
      *(int *)(puVar17 + 0x30) = local_e8[9];
      *(undefined4 **)(puVar17 + 0x28) = puVar20;
      *(double *)(puVar17 + 0x20) = param_5;
      *(undefined8 *)(puVar17 + -8) = 0x18001db55;
      uVar19 = FUN_18001cca0((longlong)piVar14,(DWORD_PTR *)(puVar12 + 0x6a),(byte)uVar21,iVar8,
                             *(double *)(puVar17 + 0x20),*(longlong *)(puVar17 + 0x28),
                             *(int *)(puVar17 + 0x30),*(undefined4 *)(puVar17 + 0x38),
                             *(int *)(puVar17 + 0x40));
      uVar10 = uVar19 & 0xffffffff;
      iVar8 = local_e8[1];
      if ((int)uVar19 != 0) goto LAB_18001dc45;
    }
    puVar5 = local_90;
    iVar8 = local_e8[5];
    if (param_3 != (int *)0x0) {
      *(undefined4 *)(puVar17 + 0x28) = 1;
      *(undefined4 **)(puVar17 + 0x20) = puVar5;
      *(undefined8 *)(puVar17 + -8) = 0x18001db8b;
      uVar19 = FUN_18001cf90((longlong)(puVar12 + 0x5e),local_e8[10],iVar8,local_94,
                             *(longlong *)(puVar17 + 0x20),*(int *)(puVar17 + 0x28));
      uVar10 = uVar19 & 0xffffffff;
      iVar8 = local_e8[1];
      if ((int)uVar19 != 0) goto LAB_18001dc45;
    }
    iVar8 = local_e8[6];
    if (param_4 == (int *)0x0) {
      local_98 = local_94 * puVar12[0x66];
    }
    else {
      *(undefined4 *)(puVar17 + 0x28) = 0;
      *(undefined4 **)(puVar17 + 0x20) = puVar20;
      *(undefined8 *)(puVar17 + -8) = 0x18001dbbd;
      uVar19 = FUN_18001cf90((longlong)(puVar12 + 0x6a),local_9c,iVar8,local_98,
                             *(longlong *)(puVar17 + 0x20),*(int *)(puVar17 + 0x28));
      uVar10 = uVar19 & 0xffffffff;
      iVar8 = local_e8[1];
      if ((int)uVar19 != 0) goto LAB_18001dc45;
      local_98 = local_98 * puVar12[0x72];
    }
    puVar12[0x84] = (int)(longlong)(((double)local_98 * dVar2) / param_5);
    if (param_8 != 0) {
      *(undefined8 *)(puVar17 + 0x20) = 0;
      *(undefined8 *)(puVar17 + -8) = 0x18001dc23;
      uVar19 = FUN_18001c620((undefined8 *)(puVar12 + 0x76),(LPSECURITY_ATTRIBUTES)0x0,1,0,
                             *(LPCWSTR *)(puVar17 + 0x20));
      uVar10 = uVar19 & 0xffffffff;
      if ((int)uVar19 != 0) {
        iVar8 = 1;
        goto LAB_18001dc45;
      }
    }
    *local_88 = puVar12;
  }
  else {
LAB_18001dc45:
    hObject = *(HANDLE *)(puVar12 + 0x76);
    if (hObject != (HANDLE)0x0) {
      *(undefined8 *)(puVar17 + -8) = 0x18001dc53;
      CloseHandle(hObject);
    }
    *(undefined8 *)(puVar17 + -8) = 0x18001dc61;
    FUN_18001fca0((longlong)(puVar12 + 0x6a),0);
    *(undefined8 *)(puVar17 + -8) = 0x18001dc72;
    FUN_18001fca0((longlong)(puVar12 + 0x5e),1);
    *(undefined8 *)(puVar17 + -8) = 0x18001dc86;
    FUN_18001fb30((undefined8 *)(puVar12 + 0x6a),0,1);
    *(undefined8 *)(puVar17 + -8) = 0x18001dc9a;
    FUN_18001fb30((undefined8 *)(puVar12 + 0x5e),1,1);
    if (iVar8 != 0) {
      *(undefined8 *)(puVar17 + -8) = 0x18001dca8;
      FUN_1800069c0((longlong)(puVar12 + 0x1a));
    }
    *(undefined8 *)(puVar17 + -8) = 0x18001dcb0;
    FUN_180006ea0(puVar12);
    *(undefined8 *)(puVar17 + -8) = 0x18001dcb8;
    FUN_180020240((longlong)puVar12);
  }
LAB_18001dcba:
  *(undefined8 *)(puVar18 + -8) = 0x18001dcc6;
  return uVar10;
}



/* ========================================================================
   ENTRY: 18001dcf0
   NAME : FUN_18001dcf0
   SIG  : bool __fastcall FUN_18001dcf0(void)
   ======================================================================== */

bool FUN_18001dcf0(void)

{
  int iVar1;
  
  iVar1 = FUN_180020730();
  return 5 < iVar1;
}



/* ========================================================================
   ENTRY: 18001dd10
   NAME : FUN_18001dd10
   SIG  : undefined __fastcall FUN_18001dd10(DWORD param_1)
   ======================================================================== */

void FUN_18001dd10(DWORD param_1)

{
  FUN_1800202f0(2,param_1);
  return;
}



/* ========================================================================
   ENTRY: 18001dd20
   NAME : FUN_18001dd20
   SIG  : ulonglong __fastcall FUN_18001dd20(longlong * param_1, undefined4 param_2)
   ======================================================================== */

ulonglong FUN_18001dd20(longlong *param_1,undefined4 param_2)

{
  UINT UVar1;
  longlong lVar2;
  undefined4 *puVar3;
  longlong lVar4;
  longlong lVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  uint uVar8;
  int iVar9;
  uint uVar10;
  undefined4 local_res18 [2];
  int local_res20;
  undefined4 uStackX_24;
  uint local_78;
  uint local_74;
  UINT local_70;
  undefined4 local_68;
  undefined4 uStack_64;
  
  lVar2 = FUN_180020230(0x120);
  if (lVar2 == 0) {
    return 0xffffd8f8;
  }
  puVar3 = FUN_1800011b0();
  *(undefined4 **)(lVar2 + 0x108) = puVar3;
  if (puVar3 != (undefined4 *)0x0) {
    *param_1 = lVar2;
    *(undefined4 *)(lVar2 + 8) = 1;
    uVar8 = 0xffffffff;
    *(undefined4 *)(*param_1 + 0xc) = 2;
    *(undefined **)(*param_1 + 0x10) = &DAT_180025cb8;
    *(undefined4 *)(*param_1 + 0x18) = 0;
    *(undefined4 *)(*param_1 + 0x1c) = 0xffffffff;
    *(undefined4 *)(*param_1 + 0x20) = 0xffffffff;
    *(undefined8 *)(lVar2 + 0x110) = 0;
    local_res18[0] = 0;
    local_78 = 0xffffffff;
    waveInMessage((HWAVEIN)0xffffffff,0x2015,(DWORD_PTR)&local_78,(DWORD_PTR)local_res18);
    local_res18[0] = 0;
    local_74 = 0xffffffff;
    waveOutMessage((HWAVEOUT)0xffffffff,0x2015,(DWORD_PTR)&local_74,(DWORD_PTR)local_res18);
    UVar1 = waveInGetNumDevs();
    iVar9 = UVar1 + 1;
    if ((int)UVar1 < 1) {
      iVar9 = 0;
    }
    local_70 = waveOutGetNumDevs();
    if (0 < (int)local_70) {
      iVar9 = iVar9 + 1 + local_70;
    }
    if (iVar9 < 1) {
LAB_18001e05e:
      FUN_18001c910(lVar2);
      *(code **)(*param_1 + 0x30) = FUN_18001faf0;
      *(code **)(*param_1 + 0x38) = FUN_18001d590;
      *(code **)(*param_1 + 0x40) = FUN_18001d230;
      FUN_180006e10((undefined8 *)(lVar2 + 0x48),FUN_18001c470,FUN_18001f1d0,FUN_18001f740,
                    FUN_18001ba00,&LAB_18001d500,&LAB_180008b90,&LAB_18000bf70,&LAB_180008780,
                    &LAB_180006df0,&LAB_180006e00,&LAB_180006df0,&LAB_180006e00);
      FUN_180006e10((undefined8 *)(lVar2 + 0xa8),FUN_18001c470,FUN_18001f1d0,FUN_18001f740,
                    FUN_18001ba00,&LAB_18001d500,&LAB_180008b90,&LAB_18000bf70,&LAB_180006de0,
                    FUN_18001ed80,FUN_18001ff40,&LAB_18001c8d0,&LAB_18001c8f0);
      return 0;
    }
    lVar5 = *param_1;
    lVar4 = FUN_1800012e0(*(int **)(lVar2 + 0x108),iVar9 * 8);
    *(longlong *)(lVar5 + 0x28) = lVar4;
    if ((*(longlong *)(*param_1 + 0x28) != 0) &&
       (lVar5 = FUN_1800012e0(*(int **)(lVar2 + 0x108),iVar9 * 0x50), lVar5 != 0)) {
      lVar4 = FUN_1800012e0(*(int **)(lVar2 + 0x108),iVar9 * 4);
      *(longlong *)(lVar2 + 0x118) = lVar4;
      if (lVar4 != 0) {
        FUN_18001c810((double *)&local_68,(double *)&local_res20);
        iVar9 = local_res20;
        if (0 < (int)UVar1) {
          uVar10 = 0xffffffff;
          do {
            puVar3 = (undefined4 *)((longlong)*(int *)(*param_1 + 0x18) * 0x50 + lVar5);
            *puVar3 = 2;
            puVar3[4] = param_2;
            *(undefined8 *)(puVar3 + 5) = 0;
            *(undefined2 *)(puVar3 + 0x13) = 0x101;
            puVar3[8] = local_68;
            puVar3[9] = uStack_64;
            puVar3[10] = local_68;
            puVar3[0xb] = uStack_64;
            puVar3[0xc] = iVar9;
            puVar3[0xd] = uStackX_24;
            puVar3[0xe] = iVar9;
            puVar3[0xf] = uStackX_24;
            uVar6 = FUN_18001c980(lVar2,(longlong)puVar3,uVar10,&local_res20);
            uVar7 = uVar6 & 0xffffffff;
            if ((int)uVar6 != 0) goto LAB_18001e1b7;
            if (local_res20 != 0) {
              lVar4 = *param_1;
              if ((*(int *)(lVar4 + 0x1c) == -1) || (uVar10 == local_78)) {
                *(undefined4 *)(lVar4 + 0x1c) = *(undefined4 *)(lVar4 + 0x18);
              }
              *(uint *)(*(longlong *)(lVar2 + 0x118) + (longlong)*(int *)(*param_1 + 0x18) * 4) =
                   uVar10;
              *(undefined4 **)
               (*(longlong *)(*param_1 + 0x28) + (longlong)*(int *)(*param_1 + 0x18) * 8) = puVar3;
              *(int *)(lVar2 + 0x110) = *(int *)(lVar2 + 0x110) + 1;
              *(int *)(*param_1 + 0x18) = *(int *)(*param_1 + 0x18) + 1;
            }
            uVar10 = uVar10 + 1;
          } while ((int)uVar10 < (int)UVar1);
        }
        UVar1 = local_70;
        if (0 < (int)local_70) {
          do {
            puVar3 = (undefined4 *)((longlong)*(int *)(*param_1 + 0x18) * 0x50 + lVar5);
            *puVar3 = 2;
            puVar3[4] = param_2;
            *(undefined8 *)(puVar3 + 5) = 0;
            *(undefined2 *)(puVar3 + 0x13) = 0x101;
            puVar3[8] = local_68;
            puVar3[9] = uStack_64;
            puVar3[10] = local_68;
            puVar3[0xb] = uStack_64;
            puVar3[0xc] = iVar9;
            puVar3[0xd] = uStackX_24;
            puVar3[0xe] = iVar9;
            puVar3[0xf] = uStackX_24;
            uVar6 = FUN_18001cb00(lVar2,(longlong)puVar3,uVar8,&local_res20);
            uVar7 = uVar6 & 0xffffffff;
            if ((int)uVar6 != 0) goto LAB_18001e1b7;
            if (local_res20 != 0) {
              lVar4 = *param_1;
              if ((*(int *)(lVar4 + 0x20) == -1) || (uVar8 == local_74)) {
                *(undefined4 *)(lVar4 + 0x20) = *(undefined4 *)(lVar4 + 0x18);
              }
              *(uint *)(*(longlong *)(lVar2 + 0x118) + (longlong)*(int *)(*param_1 + 0x18) * 4) =
                   uVar8;
              *(undefined4 **)
               (*(longlong *)(*param_1 + 0x28) + (longlong)*(int *)(*param_1 + 0x18) * 8) = puVar3;
              *(int *)(lVar2 + 0x114) = *(int *)(lVar2 + 0x114) + 1;
              *(int *)(*param_1 + 0x18) = *(int *)(*param_1 + 0x18) + 1;
            }
            uVar8 = uVar8 + 1;
          } while ((int)uVar8 < (int)UVar1);
        }
        goto LAB_18001e05e;
      }
    }
  }
  uVar7 = 0xffffd8f8;
LAB_18001e1b7:
  if (*(longlong *)(lVar2 + 0x108) != 0) {
    FUN_180001280(*(longlong *)(lVar2 + 0x108));
    FUN_180001230(*(longlong *)(lVar2 + 0x108));
  }
  FUN_180020240(lVar2);
  return uVar7;
}



/* ========================================================================
   ENTRY: 18001e1e0
   NAME : FUN_18001e1e0
   SIG  : int __fastcall FUN_18001e1e0(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_18001e1e0(longlong param_1)

{
  HANDLE pvVar1;
  HWAVEOUT hwo;
  longlong lVar2;
  bool bVar3;
  bool bVar4;
  double dVar5;
  double dVar6;
  DWORD DVar7;
  int iVar8;
  int iVar9;
  undefined8 uVar10;
  int iVar11;
  ulonglong uVar12;
  uint uVar13;
  uint uVar14;
  uint uVar15;
  int iVar16;
  int iVar17;
  bool bVar18;
  double dVar19;
  undefined1 auStack_108 [32];
  int local_e8;
  uint local_e4;
  int local_e0;
  DWORD local_dc;
  DWORD local_d8;
  mmtime_tag local_d0;
  double local_c0;
  double dStack_b8;
  double local_b0;
  HANDLE local_a8 [3];
  ulonglong local_90;
  
  dVar6 = DAT_1800235c8;
  dVar5 = DAT_180023560;
  local_90 = DAT_18002b580 ^ (ulonglong)auStack_108;
  iVar16 = 0;
  pvVar1 = *(HANDLE *)(param_1 + 0x178);
  bVar4 = false;
  bVar3 = false;
  local_d8 = *(uint *)(param_1 + 0x210) >> 1;
  local_e8 = 0;
  if (pvVar1 != (HANDLE)0x0) {
    local_a8[0] = pvVar1;
  }
  uVar15 = (uint)(pvVar1 != (HANDLE)0x0);
  uVar12 = (ulonglong)(pvVar1 != (HANDLE)0x0);
  if (*(HANDLE *)(param_1 + 0x1a8) != (HANDLE)0x0) {
    local_a8[uVar12] = *(HANDLE *)(param_1 + 0x1a8);
    uVar15 = (pvVar1 != (HANDLE)0x0) + 1;
    uVar12 = (ulonglong)(pvVar1 != (HANDLE)0x0) + 1;
  }
  local_dc = uVar15 + 1;
  local_a8[uVar12] = *(HANDLE *)(param_1 + 0x1d8);
  uVar15 = 0;
  do {
    DVar7 = WaitForMultipleObjects(local_dc,local_a8,0,local_d8);
    if (DVar7 == 0xffffffff) {
      iVar16 = -9999;
      bVar4 = true;
      bVar3 = true;
      local_e8 = -9999;
    }
    if (*(int *)(param_1 + 0x20c) != 0) break;
    if (*(int *)(param_1 + 0x208) == 0) {
      do {
        iVar17 = -1;
        if ((*(longlong *)(param_1 + 0x180) != 0) && (iVar8 = FUN_18001c690(param_1), iVar8 != 0)) {
          uVar10 = FUN_18001d520(param_1 + 0x178);
          if ((int)uVar10 != 0) {
            uVar12 = FUN_18001c2f0(param_1);
            iVar16 = (int)uVar12;
            if (iVar16 != 0) {
              bVar4 = true;
              bVar3 = true;
            }
            uVar15 = uVar15 | 2;
            local_e8 = iVar16;
          }
          iVar17 = *(int *)(param_1 + 0x19c);
        }
        iVar8 = -1;
        if ((*(longlong *)(param_1 + 0x1b0) != 0) &&
           (iVar9 = FUN_18001c6b0(param_1), iVar8 = -1, iVar9 != 0)) {
          uVar10 = FUN_18001d520(param_1 + 0x1a8);
          if ((int)uVar10 != 0) {
            uVar12 = FUN_18001c340(param_1);
            iVar16 = (int)uVar12;
            if (iVar16 != 0) {
              bVar3 = true;
            }
            uVar15 = uVar15 | 4;
            local_e8 = iVar16;
            bVar4 = bVar3;
          }
          iVar8 = *(int *)(param_1 + 0x1cc);
        }
        if ((*(longlong *)(param_1 + 0x180) == 0) || (*(longlong *)(param_1 + 0x1b0) == 0)) {
          if (iVar17 == -1) goto LAB_18001e3e3;
        }
        else {
          if (iVar17 == -1) goto LAB_18001e7e8;
LAB_18001e3e3:
          if (iVar8 == -1) goto LAB_18001e7e8;
        }
        local_b0 = 0.0;
        local_c0 = 0.0;
        dStack_b8 = 0.0;
        if (*(undefined8 **)(param_1 + 0x1b0) != (undefined8 *)0x0) {
          hwo = (HWAVEOUT)**(undefined8 **)(param_1 + 0x1b0);
          local_d0.wType = 2;
          dVar19 = FUN_180020250();
          waveOutGetPosition(hwo,&local_d0,0xc);
          dStack_b8 = FUN_180020250();
          uVar14 = *(int *)(param_1 + 0x6c) * *(int *)(param_1 + 0x1c8);
          uVar13 = local_d0.u.ms % uVar14;
          iVar11 = *(int *)(param_1 + 0x6c) * *(int *)(param_1 + 0x1cc) + *(int *)(param_1 + 0x1d4);
          iVar9 = iVar11 - uVar13;
          if (iVar11 <= (int)uVar13) {
            iVar9 = iVar9 + uVar14;
          }
          local_b0 = (double)iVar9 * *(double *)(param_1 + 0x158) +
                     (dStack_b8 - dVar19) * dVar5 + dVar19;
        }
        FUN_1800038c0(param_1 + 0x50);
        FUN_180005ce0(param_1 + 0x68,&local_c0,uVar15);
        local_e4 = 0;
        if (*(longlong *)(param_1 + 0x180) != 0) {
          FUN_180006860(param_1 + 0x68,0);
          iVar9 = 0;
          uVar12 = 0;
          if (*(int *)(param_1 + 0x188) != 0) {
            do {
              lVar2 = *(longlong *)(*(longlong *)(param_1 + 400) + uVar12 * 8);
              uVar15 = *(uint *)((longlong)iVar17 * 0x30 + 0x10 + lVar2);
              FUN_180006880(param_1 + 0x68,iVar9,
                            (ulonglong)
                            (*(int *)(param_1 + 0x1a4) * *(int *)(param_1 + 0x88) * uVar15) +
                            *(longlong *)((longlong)iVar17 * 0x30 + lVar2),uVar15);
              iVar9 = iVar9 + uVar15;
              uVar15 = (int)uVar12 + 1;
              uVar12 = (ulonglong)uVar15;
              iVar16 = local_e8;
            } while (uVar15 < *(uint *)(param_1 + 0x188));
          }
        }
        if (*(longlong *)(param_1 + 0x1b0) != 0) {
          FUN_1800069a0(param_1 + 0x68,0);
          iVar17 = 0;
          uVar12 = 0;
          if (*(int *)(param_1 + 0x1b8) != 0) {
            do {
              lVar2 = *(longlong *)(*(longlong *)(param_1 + 0x1c0) + uVar12 * 8);
              uVar15 = *(uint *)((longlong)iVar8 * 0x30 + 0x10 + lVar2);
              FUN_1800068d0(param_1 + 0x68,iVar17,
                            (ulonglong)
                            (*(int *)(param_1 + 0x1d4) * *(int *)(param_1 + 0xac) * uVar15) +
                            *(longlong *)((longlong)iVar8 * 0x30 + lVar2),uVar15);
              iVar17 = iVar17 + uVar15;
              uVar15 = (int)uVar12 + 1;
              uVar12 = (ulonglong)uVar15;
              iVar16 = local_e8;
            } while (uVar15 < *(uint *)(param_1 + 0x1b8));
          }
        }
        local_e0 = 0;
        uVar15 = FUN_180006010((uint *)(param_1 + 0x68),&local_e0);
        *(int *)(param_1 + 0x1a4) = *(int *)(param_1 + 0x1a4) + uVar15;
        *(int *)(param_1 + 0x1d4) = *(int *)(param_1 + 0x1d4) + uVar15;
        FUN_1800038e0((double *)(param_1 + 0x50),uVar15);
        bVar18 = true;
        if (local_e0 != 0) {
          iVar16 = 0;
          local_e8 = 0;
          if (local_e0 == 2) {
            *(undefined4 *)(param_1 + 0x20c) = 1;
            bVar4 = true;
            bVar3 = bVar18;
          }
          else {
            *(undefined4 *)(param_1 + 0x208) = 1;
          }
        }
        if ((((*(longlong *)(param_1 + 0x180) != 0) && (*(int *)(param_1 + 0x208) == 0)) &&
            (*(int *)(param_1 + 0x20c) == 0)) &&
           (*(int *)(param_1 + 0x1a4) == *(int *)(param_1 + 0x1a0))) {
          uVar10 = FUN_18001d520(param_1 + 0x178);
          if ((int)uVar10 != 0) {
            uVar12 = FUN_18001c2f0(param_1);
            local_e4 = 2;
            bVar4 = bVar3;
            if ((int)uVar12 != 0) {
              bVar4 = true;
            }
          }
          uVar12 = FUN_18001bc00(param_1);
          iVar16 = (int)uVar12;
          local_e8 = iVar16;
          bVar3 = bVar4;
          if (iVar16 != 0) {
            bVar4 = true;
            bVar3 = bVar18;
          }
        }
        uVar15 = local_e4;
        if ((*(longlong *)(param_1 + 0x1b0) != 0) && (*(int *)(param_1 + 0x20c) == 0)) {
          if (*(int *)(param_1 + 0x208) != 0) {
            uVar15 = *(uint *)(param_1 + 0x1d4);
            if (uVar15 < *(uint *)(param_1 + 0x1d0)) {
              uVar13 = FUN_180006a40(param_1 + 0x68,*(uint *)(param_1 + 0x1d0) - uVar15);
              *(uint *)(param_1 + 0x1d4) = uVar13 + uVar15;
            }
          }
          uVar15 = local_e4;
          if (*(int *)(param_1 + 0x1d4) == *(int *)(param_1 + 0x1d0)) {
            uVar10 = FUN_18001d520(param_1 + 0x1a8);
            uVar12 = FUN_18001bd60(param_1);
            iVar16 = (int)uVar12;
            bVar3 = bVar4;
            if (iVar16 != 0) {
              bVar3 = true;
            }
            uVar15 = local_e4;
            local_e8 = iVar16;
            bVar4 = bVar3;
            if ((((int)uVar10 != 0) && (!bVar3)) && (*(int *)(param_1 + 0x208) == 0)) {
              uVar12 = FUN_18001c340(param_1);
              iVar16 = (int)uVar12;
              if (iVar16 != 0) {
                bVar3 = bVar18;
              }
              uVar15 = local_e4 | 4;
              local_e8 = iVar16;
              bVar4 = iVar16 != 0;
            }
          }
        }
        if (*(char *)(param_1 + 0x1ec) != '\0') {
          if ((*(int *)(param_1 + 0x208) == 0) && (*(int *)(param_1 + 0x20c) == 0)) {
            dVar19 = (double)FUN_180003940(param_1 + 0x50);
            if (dVar6 < dVar19) {
              if (*(int *)(param_1 + 0x1f0) != *(int *)(param_1 + 0x1f8)) {
                SetThreadPriority(*(HANDLE *)(param_1 + 0x1e0),*(int *)(param_1 + 0x1f8));
                *(undefined4 *)(param_1 + 0x1f0) = *(undefined4 *)(param_1 + 0x1f8);
              }
              Sleep(*(DWORD *)(param_1 + 0x1fc));
              goto LAB_18001e7b7;
            }
            iVar17 = *(int *)(param_1 + 500);
            bVar18 = *(int *)(param_1 + 0x1f0) == iVar17;
          }
          else {
            iVar17 = *(int *)(param_1 + 500);
            bVar18 = *(int *)(param_1 + 0x1f0) == iVar17;
          }
          if (!bVar18) {
            SetThreadPriority(*(HANDLE *)(param_1 + 0x1e0),iVar17);
            *(undefined4 *)(param_1 + 0x1f0) = *(undefined4 *)(param_1 + 500);
          }
        }
LAB_18001e7b7:
        if ((*(int *)(param_1 + 0x208) != 0) || (*(int *)(param_1 + 0x20c) != 0))
        goto LAB_18001e7e8;
        if (bVar3) goto LAB_18001e7fa;
      } while( true );
    }
    if ((*(longlong *)(param_1 + 0x1b0) == 0) ||
       (uVar10 = FUN_18001d520(param_1 + 0x1a8), (int)uVar10 != 0)) {
      bVar4 = true;
      bVar3 = true;
    }
LAB_18001e7e8:
  } while (!bVar3);
LAB_18001e7fa:
  *(undefined4 *)(param_1 + 0x204) = 0;
  if (*(code **)(param_1 + 0x20) != (code *)0x0) {
    (**(code **)(param_1 + 0x20))(*(undefined8 *)(param_1 + 0x28));
  }
  FUN_180003970(param_1 + 0x50);
  return iVar16;
}



/* ========================================================================
   ENTRY: 18001e860
   NAME : FUN_18001e860
   SIG  : undefined8 __fastcall FUN_18001e860(longlong param_1, undefined * param_2, undefined4 param_3, int param_4, double param_5, byte param_6)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8
FUN_18001e860(longlong param_1,undefined *param_2,undefined4 param_3,int param_4,double param_5,
             byte param_6)

{
  byte bVar1;
  uint uVar2;
  int iVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  undefined1 auStackY_a8 [32];
  undefined2 local_78 [24];
  ulonglong local_48;
  
  local_48 = DAT_18002b580 ^ (ulonglong)auStackY_a8;
  uVar4 = FUN_18001f0d0(0x10,param_6);
  uVar5 = FUN_1800208c0(0x10);
  if ((uint)uVar4 != (uint)uVar5) goto LAB_18001e924;
  if (param_5 == DAT_180023d48) {
    if (param_4 == 1) {
      bVar1 = *(byte *)(param_1 + 0x48) & 4;
    }
    else {
      if (param_4 != 2) goto LAB_18001e8da;
      bVar1 = *(byte *)(param_1 + 0x48) & 8;
    }
    if (bVar1 != 0) {
      return 0;
    }
  }
LAB_18001e8da:
  if (param_5 == DAT_180023d50) {
    if (param_4 == 1) {
      bVar1 = *(byte *)(param_1 + 0x48) & 0x40;
    }
    else {
      if (param_4 != 2) goto LAB_18001e8fc;
      bVar1 = *(byte *)(param_1 + 0x48) & 0x80;
    }
    if (bVar1 != 0) {
      return 0;
    }
  }
LAB_18001e8fc:
  if (param_5 == DAT_180023d58) {
    if (param_4 == 1) {
      uVar2 = *(uint *)(param_1 + 0x48) & 0x400;
    }
    else {
      if (param_4 != 2) goto LAB_18001e924;
      uVar2 = *(uint *)(param_1 + 0x48) & 0x800;
    }
    if (uVar2 != 0) {
      return 0;
    }
  }
LAB_18001e924:
  FUN_180020830(local_78,param_4,0x10,(uint)uVar4,param_5,0);
  iVar3 = (*(code *)param_2)(param_3,local_78);
  if (iVar3 == 0) {
    return 0;
  }
  FUN_1800207c0(local_78,param_4,0x10,(short)uVar4,param_5);
  uVar4 = (*(code *)param_2)(param_3,local_78);
  return uVar4;
}



/* ========================================================================
   ENTRY: 18001e9a0
   NAME : FUN_18001e9a0
   SIG  : undefined8 __fastcall FUN_18001e9a0(UINT param_1, LPCWAVEFORMATEX param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001e9a0(UINT param_1,LPCWAVEFORMATEX param_2)

{
  MMRESULT mmrError;
  undefined8 uVar1;
  undefined1 auStackY_358 [32];
  CHAR local_318 [256];
  WCHAR local_218 [256];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStackY_358;
  mmrError = waveInOpen((LPHWAVEIN)0x0,param_1,param_2,0,0,1);
  switch(mmrError) {
  case 0:
    uVar1 = 0;
    break;
  default:
    waveInGetErrorTextW(mmrError,local_218,0x100);
    WideCharToMultiByte(0xfde9,0,local_218,-1,local_318,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
    FUN_180003c90(2,mmrError,local_318);
    uVar1 = 0xffffd8f1;
    break;
  case 4:
  case 6:
    uVar1 = 0xffffd8ff;
    break;
  case 7:
    uVar1 = 0xffffd8f8;
    break;
  case 0x20:
    uVar1 = 0xffffd8f6;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001eae0
   NAME : FUN_18001eae0
   SIG  : undefined8 __fastcall FUN_18001eae0(UINT param_1, LPCWAVEFORMATEX param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001eae0(UINT param_1,LPCWAVEFORMATEX param_2)

{
  MMRESULT mmrError;
  undefined8 uVar1;
  undefined1 auStackY_358 [32];
  CHAR local_318 [256];
  WCHAR local_218 [256];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStackY_358;
  mmrError = waveOutOpen((LPHWAVEOUT)0x0,param_1,param_2,0,0,1);
  switch(mmrError) {
  case 0:
    uVar1 = 0;
    break;
  default:
    waveOutGetErrorTextW(mmrError,local_218,0x100);
    WideCharToMultiByte(0xfde9,0,local_218,-1,local_318,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
    FUN_180003c90(2,mmrError,local_318);
    uVar1 = 0xffffd8f1;
    break;
  case 4:
  case 6:
    uVar1 = 0xffffd8ff;
    break;
  case 7:
    uVar1 = 0xffffd8f8;
    break;
  case 0x20:
    uVar1 = 0xffffd8f6;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001ec20
   NAME : FUN_18001ec20
   SIG  : undefined8 __fastcall FUN_18001ec20(uint param_1, uint * param_2)
   ======================================================================== */

undefined8 FUN_18001ec20(uint param_1,uint *param_2)

{
  MMRESULT MVar1;
  uint uVar2;
  LPCWSTR dw1;
  undefined8 uVar3;
  uint local_res8 [2];
  
  MVar1 = waveInMessage((HWAVEIN)(ulonglong)param_1,0x80d,(DWORD_PTR)local_res8,0);
  if (MVar1 == 0) {
    dw1 = (LPCWSTR)FUN_180020230(local_res8[0]);
    if (dw1 != (LPCWSTR)0x0) {
      uVar3 = 0;
      MVar1 = waveInMessage((HWAVEIN)(ulonglong)param_1,0x80c,(DWORD_PTR)dw1,
                            (ulonglong)local_res8[0]);
      if (MVar1 == 0) {
        uVar2 = FUN_180020a10(dw1,1);
        if (0 < (int)uVar2) {
          *param_2 = uVar2;
          uVar3 = 1;
        }
      }
      FUN_180020240((longlong)dw1);
      return uVar3;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001ecd0
   NAME : FUN_18001ecd0
   SIG  : undefined8 __fastcall FUN_18001ecd0(uint param_1, uint * param_2)
   ======================================================================== */

undefined8 FUN_18001ecd0(uint param_1,uint *param_2)

{
  MMRESULT MVar1;
  uint uVar2;
  LPCWSTR dw1;
  undefined8 uVar3;
  uint local_res8 [2];
  
  MVar1 = waveOutMessage((HWAVEOUT)(ulonglong)param_1,0x80d,(DWORD_PTR)local_res8,0);
  if (MVar1 == 0) {
    dw1 = (LPCWSTR)FUN_180020230(local_res8[0]);
    if (dw1 != (LPCWSTR)0x0) {
      uVar3 = 0;
      MVar1 = waveOutMessage((HWAVEOUT)(ulonglong)param_1,0x80c,(DWORD_PTR)dw1,
                             (ulonglong)local_res8[0]);
      if (MVar1 == 0) {
        uVar2 = FUN_180020a10(dw1,0);
        if (0 < (int)uVar2) {
          *param_2 = uVar2;
          uVar3 = 1;
        }
      }
      FUN_180020240((longlong)dw1);
      return uVar3;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001ed80
   NAME : FUN_18001ed80
   SIG  : int __fastcall FUN_18001ed80(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

int FUN_18001ed80(longlong param_1,longlong param_2,uint param_3)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  HANDLE hHandle;
  int iVar4;
  int iVar5;
  DWORD DVar6;
  undefined8 uVar7;
  ulonglong uVar8;
  ulonglong uVar9;
  undefined1 *puVar10;
  longlong lVar11;
  uint uVar12;
  undefined8 *puVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  undefined1 auStack_88 [32];
  uint local_68;
  uint local_64;
  uint local_60;
  uint local_5c;
  longlong local_58;
  ulonglong local_50;
  
  puVar10 = auStack_88;
  local_50 = DAT_18002b580 ^ (ulonglong)&local_68;
  if (*(longlong *)(param_1 + 0x180) == 0) {
    iVar5 = -0x26f7;
  }
  else {
    uVar16 = *(uint *)(param_1 + 0x210);
    puVar10 = auStack_88;
    local_64 = param_3;
    local_58 = param_2;
    if (*(int *)(param_1 + 0x90) == 0) {
      uVar9 = (ulonglong)*(uint *)(param_1 + 0x84) * 8;
      uVar8 = uVar9 + 0xf;
      if (uVar8 <= uVar9) {
        uVar8 = 0xffffffffffffff0;
      }
      lVar11 = -(uVar8 & 0xfffffffffffffff0);
      puVar10 = auStack_88 + lVar11;
      local_58 = (longlong)&local_68 + lVar11;
      if (local_58 == 0) {
        iVar5 = -0x2708;
        goto LAB_18001efa0;
      }
      uVar8 = 0;
      puVar10 = auStack_88 + lVar11;
      if (*(uint *)(param_1 + 0x84) != 0) {
        do {
          *(undefined8 *)(local_58 + uVar8 * 8) = *(undefined8 *)(param_2 + uVar8 * 8);
          uVar15 = (int)uVar8 + 1;
          uVar8 = (ulonglong)uVar15;
          puVar10 = auStack_88 + lVar11;
        } while (uVar15 < *(uint *)(param_1 + 0x84));
      }
    }
    local_5c = uVar16 * 3;
    uVar16 = uVar16 >> 1;
    uVar15 = 0;
    iVar5 = 0;
    local_68 = 0;
    uVar12 = 0;
    local_60 = uVar16;
    uVar14 = local_5c;
    do {
      puVar13 = (undefined8 *)(param_1 + 0x178);
      *(undefined8 *)(puVar10 + -8) = 0x18001ee04;
      iVar4 = FUN_18001c690(param_1);
      if (iVar4 == 0) {
        hHandle = (HANDLE)*puVar13;
        *(undefined8 *)(puVar10 + -8) = 0x18001ef68;
        DVar6 = WaitForSingleObject(hHandle,uVar16);
        if (DVar6 == 0xffffffff) {
          iVar5 = -9999;
          break;
        }
        if ((DVar6 == 0x102) && (uVar12 = uVar12 + uVar16, uVar14 <= uVar12)) {
          iVar5 = -0x2703;
          break;
        }
      }
      else {
        *(undefined8 *)(puVar10 + -8) = 0x18001ee14;
        uVar7 = FUN_18001d520((longlong)puVar13);
        iVar4 = *(int *)(param_1 + 0x1a0);
        iVar1 = *(int *)(param_1 + 0x19c);
        if ((int)uVar7 != 0) {
          iVar5 = -0x26fd;
        }
        iVar2 = *(int *)(param_1 + 0x1a4);
        *(undefined8 *)(puVar10 + -8) = 0x18001ee3a;
        FUN_180006860(param_1 + 0x68,iVar4 - iVar2);
        iVar4 = 0;
        uVar8 = 0;
        if (*(int *)(param_1 + 0x188) != 0) {
          lVar11 = (longlong)iVar1 * 0x30;
          do {
            iVar1 = *(int *)(param_1 + 0x1a4);
            iVar2 = *(int *)(param_1 + 0x88);
            lVar3 = *(longlong *)(*(longlong *)(param_1 + 400) + uVar8 * 8);
            uVar16 = *(uint *)(lVar11 + 0x10 + lVar3);
            lVar3 = *(longlong *)(lVar11 + lVar3);
            *(undefined8 *)(puVar10 + -8) = 0x18001ee86;
            FUN_180006880(param_1 + 0x68,iVar4,(ulonglong)(iVar1 * iVar2 * uVar16) + lVar3,uVar16);
            iVar4 = iVar4 + uVar16;
            uVar16 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar16;
            uVar15 = local_68;
          } while (uVar16 < *(uint *)(param_1 + 0x188));
        }
        uVar16 = local_64 - uVar15;
        *(undefined8 *)(puVar10 + -8) = 0x18001eeb0;
        uVar16 = FUN_180005d50(param_1 + 0x68,&local_58,uVar16);
        *(int *)(param_1 + 0x1a4) = *(int *)(param_1 + 0x1a4) + uVar16;
        if (*(int *)(param_1 + 0x1a4) == *(int *)(param_1 + 0x1a0)) {
          *(undefined8 *)(puVar10 + -8) = 0x18001eece;
          uVar8 = FUN_18001bc00(param_1);
          iVar5 = (int)uVar8;
          if (iVar5 != 0) break;
        }
        uVar12 = 0;
        uVar15 = uVar15 + uVar16;
        uVar16 = local_60;
        local_68 = uVar15;
        uVar14 = local_5c;
      }
    } while (uVar15 < local_64);
  }
LAB_18001efa0:
  *(undefined8 *)(puVar10 + -8) = 0x18001efac;
  return iVar5;
}



/* ========================================================================
   ENTRY: 18001efc0
   NAME : FUN_18001efc0
   SIG  : undefined8 __fastcall FUN_18001efc0(HANDLE param_1)
   ======================================================================== */

undefined8 FUN_18001efc0(HANDLE param_1)

{
  BOOL BVar1;
  DWORD DVar2;
  undefined8 uVar3;
  
  uVar3 = 0;
  if (param_1 != (HANDLE)0x0) {
    uVar3 = 0;
    BVar1 = ResetEvent(param_1);
    if (BVar1 == 0) {
      DVar2 = GetLastError();
      FUN_18001dd10(DVar2);
      uVar3 = 0xffffd8f1;
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 18001f000
   NAME : FUN_18001f000
   SIG  : undefined8 __fastcall FUN_18001f000(int * param_1, undefined4 * param_2, longlong param_3, undefined4 * param_4, uint param_5)
   ======================================================================== */

undefined8
FUN_18001f000(int *param_1,undefined4 *param_2,longlong param_3,undefined4 *param_4,uint param_5)

{
  int iVar1;
  undefined8 uVar2;
  uint uVar3;
  ulonglong uVar4;
  int iVar5;
  int local_res18 [2];
  
  if ((param_3 != 0) && ((*(byte *)(param_3 + 0xc) & 2) != 0)) {
    uVar4 = 0;
    iVar5 = 0;
    if (param_5 != 0) {
      do {
        uVar2 = FUN_180003c20(local_res18,*(int *)(*(longlong *)(param_3 + 0x18) + uVar4 * 8),
                              param_1);
        if ((int)uVar2 != 0) {
          return uVar2;
        }
        param_4[uVar4 * 2] = local_res18[0];
        iVar1 = *(int *)(*(longlong *)(param_3 + 0x18) + 4 + uVar4 * 8);
        iVar5 = iVar5 + iVar1;
        param_4[uVar4 * 2 + 1] = iVar1;
        uVar3 = (int)uVar4 + 1;
        uVar4 = (ulonglong)uVar3;
      } while (uVar3 < param_5);
    }
    uVar2 = 0;
    if (iVar5 != param_2[1]) {
      uVar2 = 0xffffd8f2;
    }
    return uVar2;
  }
  *param_4 = *param_2;
  param_4[1] = param_2[1];
  return 0;
}



/* ========================================================================
   ENTRY: 18001f0d0
   NAME : FUN_18001f0d0
   SIG  : undefined8 __fastcall FUN_18001f0d0(int param_1, byte param_2)
   ======================================================================== */

undefined8 FUN_18001f0d0(int param_1,byte param_2)

{
  undefined8 uVar1;
  
  if ((param_2 & 0x10) != 0) {
    return 0x92;
  }
  if ((param_2 & 0x20) != 0) {
    return 0x164;
  }
  uVar1 = FUN_1800208c0(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 18001f0f0
   NAME : FUN_18001f0f0
   SIG  : undefined8 __fastcall FUN_18001f0f0(uint param_1, uint param_2, uint param_3, uint param_4, uint param_5, uint * param_6, uint * param_7)
   ======================================================================== */

undefined8
FUN_18001f0f0(uint param_1,uint param_2,uint param_3,uint param_4,uint param_5,uint *param_6,
             uint *param_7)

{
  uint uVar1;
  uint uVar2;
  ulonglong uVar3;
  
  if (param_2 == 0) {
    uVar3 = 0x10;
    *param_6 = 0x10;
    uVar1 = FUN_18001c510(param_1,0x10,param_3);
    *param_7 = uVar1;
  }
  else {
    if (param_5 < param_2) {
      uVar3 = FUN_18001c530(param_2,param_5);
      uVar3 = uVar3 & 0xffffffff;
      if (param_1 < param_2) {
        param_1 = param_2;
      }
    }
    else {
      uVar3 = (ulonglong)param_2;
    }
    *param_6 = (uint)uVar3;
    uVar1 = FUN_18001c510(param_1,(uint)uVar3,param_3);
    *param_7 = uVar1;
    if (*param_6 < param_2) {
      return 0;
    }
  }
  uVar1 = (uVar1 + 5) / 7;
  if (1 < uVar1) {
    if (param_5 < param_4) {
      param_4 = param_5;
    }
    uVar2 = (uint)((ulonglong)param_4 / (uVar3 & 0xffffffff));
    if (uVar1 <= uVar2) {
      uVar2 = uVar1;
    }
    uVar2 = uVar2 * (int)uVar3;
    *param_6 = uVar2;
    uVar1 = FUN_18001c510(param_1,uVar2,param_3);
    *param_7 = uVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001f1d0
   NAME : FUN_18001f1d0
   SIG  : ulonglong __fastcall FUN_18001f1d0(void * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_18001f1d0(void *param_1)

{
  uint *puVar1;
  longlong lVar2;
  MMRESULT mmrError;
  DWORD DVar3;
  BOOL BVar4;
  ulonglong uVar5;
  HANDLE hThread;
  uint uVar6;
  uint uVar7;
  ulonglong uVar8;
  int iVar9;
  undefined1 auStackY_3b8 [32];
  int local_378 [2];
  double local_370 [3];
  CHAR local_358 [256];
  WCHAR local_258 [256];
  ulonglong local_58;
  
  local_58 = DAT_18002b580 ^ (ulonglong)auStackY_3b8;
  local_370[2] = 0.0;
  local_370[0] = 0.0;
  local_370[1] = 0.0;
  FUN_1800066c0((longlong)param_1 + 0x68);
  if (*(longlong *)((longlong)param_1 + 0x180) != 0) {
    uVar5 = 0;
    if (*(int *)((longlong)param_1 + 0x198) != 0) {
      do {
        uVar8 = 0;
        if (*(int *)((longlong)param_1 + 0x188) != 0) {
          do {
            puVar1 = (uint *)(uVar5 * 0x30 + 0x18 +
                             *(longlong *)(*(longlong *)((longlong)param_1 + 400) + uVar8 * 8));
            *puVar1 = *puVar1 & 0xfffffffe;
            mmrError = waveInAddBuffer(*(HWAVEIN *)
                                        (*(longlong *)((longlong)param_1 + 0x180) + uVar8 * 8),
                                       (LPWAVEHDR)
                                       (*(longlong *)
                                         (*(longlong *)((longlong)param_1 + 400) + uVar8 * 8) +
                                       uVar5 * 0x30),0x30);
            if (mmrError != 0) {
              waveInGetErrorTextW(mmrError,local_258,0x100);
              WideCharToMultiByte(0xfde9,0,local_258,-1,local_358,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
              goto LAB_18001f45d;
            }
            uVar6 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar6;
          } while (uVar6 < *(uint *)((longlong)param_1 + 0x188));
        }
        uVar6 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar6;
      } while (uVar6 < *(uint *)((longlong)param_1 + 0x198));
    }
    *(undefined4 *)((longlong)param_1 + 0x19c) = 0;
    *(undefined4 *)((longlong)param_1 + 0x1a4) = 0;
  }
  if (*(longlong *)((longlong)param_1 + 0x1b0) != 0) {
    uVar5 = 0;
    uVar6 = 0;
    if (*(int *)((longlong)param_1 + 0x1b8) != 0) {
      do {
        mmrError = waveOutPause(*(HWAVEOUT *)(*(longlong *)((longlong)param_1 + 0x1b0) + uVar5 * 8))
        ;
        if (mmrError != 0) goto LAB_18001f48c;
        uVar6 = *(uint *)((longlong)param_1 + 0x1b8);
        uVar7 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar7;
      } while (uVar7 < uVar6);
    }
    if (*(int *)((longlong)param_1 + 0x1c8) != 0) {
      uVar5 = 0;
      do {
        if (*(int *)((longlong)param_1 + 0x170) == 0) {
          uVar8 = 0;
          if (uVar6 != 0) {
            do {
              lVar2 = *(longlong *)(*(longlong *)((longlong)param_1 + 0x1c0) + uVar8 * 8);
              memset(*(void **)(lVar2 + uVar5 * 0x30),0,
                     (ulonglong)*(uint *)(lVar2 + 8 + uVar5 * 0x30));
              uVar6 = (int)uVar8 + 1;
              uVar8 = (ulonglong)uVar6;
            } while (uVar6 < *(uint *)((longlong)param_1 + 0x1b8));
          }
        }
        else {
          *(undefined4 *)((longlong)param_1 + 0x1d4) = 0;
          do {
            FUN_180005ce0((longlong)param_1 + 0x68,local_370,
                          (*(int *)((longlong)param_1 + 0x198) != 0) + 0x10);
            if (*(int *)((longlong)param_1 + 0x198) != 0) {
              FUN_180006930((longlong)param_1 + 0x68);
            }
            FUN_1800069a0((longlong)param_1 + 0x68,0);
            iVar9 = 0;
            uVar8 = 0;
            if (*(int *)((longlong)param_1 + 0x1b8) != 0) {
              do {
                lVar2 = *(longlong *)(*(longlong *)((longlong)param_1 + 0x1c0) + uVar8 * 8);
                uVar6 = *(uint *)(uVar5 * 0x30 + 0x10 + lVar2);
                FUN_1800068d0((longlong)param_1 + 0x68,iVar9,
                              (ulonglong)
                              (*(int *)((longlong)param_1 + 0x1d4) *
                               *(int *)((longlong)param_1 + 0xac) * uVar6) +
                              *(longlong *)(uVar5 * 0x30 + lVar2),uVar6);
                iVar9 = iVar9 + uVar6;
                uVar6 = (int)uVar8 + 1;
                uVar8 = (ulonglong)uVar6;
              } while (uVar6 < *(uint *)((longlong)param_1 + 0x1b8));
            }
            local_378[0] = 0;
            iVar9 = FUN_180006010((uint *)((longlong)param_1 + 0x68),local_378);
            iVar9 = *(int *)((longlong)param_1 + 0x1d4) + iVar9;
            *(int *)((longlong)param_1 + 0x1d4) = iVar9;
          } while (iVar9 != *(int *)((longlong)param_1 + 0x1d0));
        }
        uVar8 = 0;
        uVar6 = 0;
        if (*(int *)((longlong)param_1 + 0x1b8) != 0) {
          do {
            mmrError = waveOutWrite(*(HWAVEOUT *)
                                     (*(longlong *)((longlong)param_1 + 0x1b0) + uVar8 * 8),
                                    (LPWAVEHDR)
                                    (*(longlong *)
                                      (*(longlong *)((longlong)param_1 + 0x1c0) + uVar8 * 8) +
                                    uVar5 * 0x30),0x30);
            if (mmrError != 0) {
              waveOutGetErrorTextW(mmrError,local_258,0x100);
              goto LAB_18001f4b5;
            }
            uVar6 = *(uint *)((longlong)param_1 + 0x1b8);
            uVar7 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar7;
          } while (uVar7 < uVar6);
        }
        uVar7 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar7;
      } while (uVar7 < *(uint *)((longlong)param_1 + 0x1c8));
    }
    *(undefined4 *)((longlong)param_1 + 0x1cc) = 0;
    *(undefined4 *)((longlong)param_1 + 0x1d4) = 0;
  }
  *(undefined4 *)((longlong)param_1 + 0x200) = 0;
  *(undefined4 *)((longlong)param_1 + 0x204) = 1;
  *(undefined4 *)((longlong)param_1 + 0x208) = 0;
  *(undefined4 *)((longlong)param_1 + 0x20c) = 0;
  uVar5 = FUN_18001efc0(*(HANDLE *)((longlong)param_1 + 0x178));
  if ((int)uVar5 != 0) {
    return uVar5 & 0xffffffff;
  }
  uVar5 = FUN_18001efc0(*(HANDLE *)((longlong)param_1 + 0x1a8));
  if ((int)uVar5 != 0) {
    return uVar5 & 0xffffffff;
  }
  if (*(longlong *)((longlong)param_1 + 0x18) == 0) {
LAB_18001f6ac:
    if ((*(longlong *)((longlong)param_1 + 0x180) != 0) &&
       (uVar5 = 0, *(int *)((longlong)param_1 + 0x188) != 0)) {
      do {
        mmrError = waveInStart(*(HWAVEIN *)(*(longlong *)((longlong)param_1 + 0x180) + uVar5 * 8));
        if (mmrError != 0) {
          waveInGetErrorTextW(mmrError,local_258,0x100);
          goto LAB_18001f4b5;
        }
        uVar6 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar6;
      } while (uVar6 < *(uint *)((longlong)param_1 + 0x188));
    }
    if ((*(longlong *)((longlong)param_1 + 0x1b0) != 0) &&
       (uVar5 = 0, *(int *)((longlong)param_1 + 0x1b8) != 0)) {
      do {
        mmrError = waveOutRestart(*(HWAVEOUT *)
                                   (*(longlong *)((longlong)param_1 + 0x1b0) + uVar5 * 8));
        if (mmrError != 0) {
LAB_18001f48c:
          waveOutGetErrorTextW(mmrError,local_258,0x100);
LAB_18001f4b5:
          WideCharToMultiByte(0xfde9,0,local_258,-1,local_358,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
LAB_18001f45d:
          FUN_180003c90(2,mmrError,local_358);
          return 0xffffd8f1;
        }
        uVar6 = (int)uVar5 + 1;
        uVar5 = (ulonglong)uVar6;
      } while (uVar6 < *(uint *)((longlong)param_1 + 0x1b8));
    }
    uVar5 = 0;
  }
  else {
    uVar5 = FUN_18001efc0(*(HANDLE *)((longlong)param_1 + 0x1d8));
    if ((int)uVar5 != 0) {
      return uVar5 & 0xffffffff;
    }
    hThread = (HANDLE)_beginthreadex((void *)0x0,0,FUN_18001e1e0,param_1,0,
                                     (uint *)((longlong)param_1 + 0x1e8));
    *(HANDLE *)((longlong)param_1 + 0x1e0) = hThread;
    if (hThread != (HANDLE)0x0) {
      *(undefined8 *)((longlong)param_1 + 500) = 0xf;
      BVar4 = SetThreadPriority(hThread,0xf);
      if (BVar4 != 0) {
        *(undefined4 *)((longlong)param_1 + 0x1f0) = *(undefined4 *)((longlong)param_1 + 500);
        goto LAB_18001f6ac;
      }
    }
    uVar5 = 0xffffd8f1;
    DVar3 = GetLastError();
    FUN_18001dd10(DVar3);
  }
  return uVar5;
}



/* ========================================================================
   ENTRY: 18001f740
   NAME : FUN_18001f740
   SIG  : ulonglong __fastcall FUN_18001f740(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong FUN_18001f740(longlong param_1)

{
  longlong lVar1;
  DWORD DVar2;
  DWORD DVar3;
  int iVar4;
  MMRESULT MVar5;
  undefined8 uVar6;
  ulonglong uVar7;
  uint uVar8;
  ulonglong uVar9;
  ulonglong uVar10;
  ulonglong uVar11;
  longlong lVar12;
  undefined1 auStackY_378 [32];
  CHAR local_338 [256];
  WCHAR local_238 [256];
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_378;
  if (*(longlong *)(param_1 + 0x1e0) == 0) {
    uVar7 = 0;
    uVar10 = 0;
    uVar9 = uVar10;
    if (*(longlong *)(param_1 + 0x1b0) != 0) {
      if (*(int *)(param_1 + 0x1d4) != 0) {
        iVar4 = *(int *)(param_1 + 0x1cc);
        FUN_1800069a0(param_1 + 0x68,*(int *)(param_1 + 0x1d0) - *(int *)(param_1 + 0x1d4));
        if (*(int *)(param_1 + 0x1b8) != 0) {
          lVar12 = (longlong)iVar4 * 0x30;
          uVar9 = uVar7;
          uVar11 = uVar7;
          do {
            lVar1 = *(longlong *)(*(longlong *)(param_1 + 0x1c0) + uVar11 * 8);
            uVar8 = *(uint *)(lVar12 + 0x10 + lVar1);
            FUN_1800068d0(param_1 + 0x68,(int)uVar9,
                          (ulonglong)(*(int *)(param_1 + 0x1d4) * *(int *)(param_1 + 0xac) * uVar8)
                          + *(longlong *)(lVar12 + lVar1),uVar8);
            uVar9 = (ulonglong)((int)uVar9 + uVar8);
            uVar8 = (int)uVar11 + 1;
            uVar11 = (ulonglong)uVar8;
          } while (uVar8 < *(uint *)(param_1 + 0x1b8));
        }
        FUN_180006a40(param_1 + 0x68,*(int *)(param_1 + 0x1d0) - *(int *)(param_1 + 0x1d4));
        FUN_18001bd60(param_1);
      }
      DVar3 = *(uint *)(param_1 + 0x210) / *(uint *)(param_1 + 0x1c8) + 1;
      if (DVar3 < 1000) {
        DVar3 = 1000;
      }
      uVar6 = FUN_18001d520(param_1 + 0x1a8);
      iVar4 = (int)uVar6;
      uVar11 = uVar7;
      while (((uVar9 = uVar10, iVar4 == 0 &&
              (uVar9 = uVar7, (uint)uVar11 <= *(uint *)(param_1 + 0x1c8))) &&
             (DVar2 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x1a8),DVar3), uVar9 = uVar10,
             DVar2 != 0xffffffff))) {
        uVar11 = (ulonglong)((uint)uVar11 + 1);
        uVar6 = FUN_18001d520(param_1 + 0x1a8);
        iVar4 = (int)uVar6;
      }
    }
  }
  else {
    *(undefined4 *)(param_1 + 0x208) = 1;
    DVar3 = (DWORD)(longlong)((double)*(uint *)(param_1 + 0x210) * _DAT_180025cc0);
    if (DVar3 < 1000) {
      DVar3 = 1000;
    }
    uVar9 = 0;
    DVar2 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x1e0),DVar3);
    if (DVar2 == 0x102) {
      *(undefined4 *)(param_1 + 0x20c) = 1;
      SetEvent(*(HANDLE *)(param_1 + 0x1d8));
      DVar3 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x1e0),DVar3);
      uVar9 = 0xffffd8fd;
      if (DVar3 != 0x102) {
        uVar9 = 0;
      }
    }
    CloseHandle(*(HANDLE *)(param_1 + 0x1e0));
    *(undefined8 *)(param_1 + 0x1e0) = 0;
  }
  uVar7 = 0;
  if ((*(longlong *)(param_1 + 0x1b0) != 0) && (uVar10 = uVar7, *(int *)(param_1 + 0x1b8) != 0)) {
    do {
      MVar5 = waveOutReset(*(HWAVEOUT *)(*(longlong *)(param_1 + 0x1b0) + uVar10 * 8));
      if (MVar5 != 0) {
        uVar9 = 0xffffd8f1;
        waveOutGetErrorTextW(MVar5,local_238,0x100);
        WideCharToMultiByte(0xfde9,0,local_238,-1,local_338,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
        FUN_180003c90(2,MVar5,local_338);
      }
      uVar8 = (int)uVar10 + 1;
      uVar10 = (ulonglong)uVar8;
    } while (uVar8 < *(uint *)(param_1 + 0x1b8));
  }
  if ((*(longlong *)(param_1 + 0x180) != 0) && (*(int *)(param_1 + 0x188) != 0)) {
    do {
      MVar5 = waveInReset(*(HWAVEIN *)(*(longlong *)(param_1 + 0x180) + uVar7 * 8));
      if (MVar5 != 0) {
        uVar9 = 0xffffd8f1;
        waveInGetErrorTextW(MVar5,local_238,0x100);
        WideCharToMultiByte(0xfde9,0,local_238,-1,local_338,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
        FUN_180003c90(2,MVar5,local_338);
      }
      uVar8 = (int)uVar7 + 1;
      uVar7 = (ulonglong)uVar8;
    } while (uVar8 < *(uint *)(param_1 + 0x188));
  }
  *(undefined4 *)(param_1 + 0x200) = 1;
  *(undefined4 *)(param_1 + 0x204) = 0;
  return uVar9;
}



/* ========================================================================
   ENTRY: 18001faf0
   NAME : FUN_18001faf0
   SIG  : undefined __fastcall FUN_18001faf0(longlong param_1)
   ======================================================================== */

void FUN_18001faf0(longlong param_1)

{
  if (*(longlong *)(param_1 + 0x108) != 0) {
    FUN_180001280(*(longlong *)(param_1 + 0x108));
    FUN_180001230(*(longlong *)(param_1 + 0x108));
  }
  FUN_180020240(param_1);
  return;
}



/* ========================================================================
   ENTRY: 18001fb30
   NAME : FUN_18001fb30
   SIG  : undefined8 __fastcall FUN_18001fb30(undefined8 * param_1, int param_2, int param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_18001fb30(undefined8 *param_1,int param_2,int param_3)

{
  uint uVar1;
  HWAVEOUT hwo;
  MMRESULT mmrError;
  undefined8 uVar2;
  undefined1 auStackY_378 [32];
  CHAR local_338 [256];
  WCHAR local_238 [256];
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStackY_378;
  uVar2 = 0;
  if (param_1[1] != 0) {
    uVar1 = *(uint *)(param_1 + 2);
joined_r0x00018001fb7b:
    while (uVar1 = uVar1 - 1, -1 < (int)uVar1) {
      hwo = *(HWAVEOUT *)(param_1[1] + (ulonglong)uVar1 * 8);
      if (param_2 == 0) goto LAB_18001fbad;
      if (hwo != (HWAVEOUT)0x0) {
        mmrError = waveInClose((HWAVEIN)hwo);
        goto LAB_18001fbbc;
      }
    }
    FUN_180020240(param_1[1]);
    param_1[1] = 0;
  }
  if ((HANDLE)*param_1 != (HANDLE)0x0) {
    uVar2 = FUN_18001c430((HANDLE)*param_1);
    *param_1 = 0;
  }
  return uVar2;
LAB_18001fbad:
  if (hwo != (HWAVEOUT)0x0) {
    mmrError = waveOutClose(hwo);
LAB_18001fbbc:
    if ((mmrError != 0) && (param_3 == 0)) {
      uVar2 = 0xffffd8f1;
      if (param_2 == 0) {
        waveOutGetErrorTextW(mmrError,local_238,0x100);
      }
      else {
        waveInGetErrorTextW(mmrError,local_238,0x100);
      }
      WideCharToMultiByte(0xfde9,0,local_238,-1,local_338,0x100,(LPCSTR)0x0,(LPBOOL)0x0);
      FUN_180003c90(2,mmrError,local_338);
    }
  }
  goto joined_r0x00018001fb7b;
}



/* ========================================================================
   ENTRY: 18001fca0
   NAME : FUN_18001fca0
   SIG  : undefined __fastcall FUN_18001fca0(longlong param_1, int param_2)
   ======================================================================== */

void FUN_18001fca0(longlong param_1,int param_2)

{
  longlong lVar1;
  uint uVar2;
  longlong lVar3;
  uint uVar4;
  LPWAVEHDR pwVar6;
  ulonglong uVar5;
  
  if (*(longlong *)(param_1 + 0x18) != 0) {
    uVar2 = *(uint *)(param_1 + 0x10);
    while (uVar2 = uVar2 - 1, -1 < (int)uVar2) {
      lVar1 = (ulonglong)uVar2 * 8;
      lVar3 = *(longlong *)(lVar1 + *(longlong *)(param_1 + 0x18));
      if (lVar3 != 0) {
        uVar4 = *(int *)(param_1 + 0x20) - 1;
        uVar5 = (ulonglong)uVar4;
        if (-1 < (int)uVar4) {
          if (param_2 == 0) {
            do {
              pwVar6 = (LPWAVEHDR)(uVar5 * 0x30 + lVar3);
              if (pwVar6->lpData != (LPSTR)0x0) {
                if (pwVar6->dwUser != 0xffffffff) {
                  waveOutUnprepareHeader
                            (*(HWAVEOUT *)(*(longlong *)(param_1 + 8) + lVar1),pwVar6,0x30);
                }
                FUN_180020240((longlong)pwVar6->lpData);
              }
              uVar4 = (int)uVar5 - 1;
              uVar5 = (ulonglong)uVar4;
            } while (-1 < (int)uVar4);
          }
          else {
            do {
              pwVar6 = (LPWAVEHDR)(uVar5 * 0x30 + lVar3);
              if (pwVar6->lpData != (LPSTR)0x0) {
                if (pwVar6->dwUser != 0xffffffff) {
                  waveInUnprepareHeader
                            (*(HWAVEIN *)(*(longlong *)(param_1 + 8) + lVar1),pwVar6,0x30);
                }
                FUN_180020240((longlong)pwVar6->lpData);
              }
              uVar4 = (int)uVar5 - 1;
              uVar5 = (ulonglong)uVar4;
            } while (-1 < (int)uVar4);
          }
        }
        FUN_180020240(lVar3);
      }
    }
    FUN_180020240(*(longlong *)(param_1 + 0x18));
    *(undefined8 *)(param_1 + 0x18) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 18001fdf0
   NAME : FUN_18001fdf0
   SIG  : undefined8 __fastcall FUN_18001fdf0(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

undefined8 FUN_18001fdf0(longlong param_1,longlong param_2,uint param_3)

{
  int iVar1;
  undefined8 uVar2;
  uint uVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  
  uVar4 = 0;
  uVar5 = (ulonglong)param_3;
  if (param_3 != 0) {
    do {
      iVar1 = *(int *)(param_2 + 4 + uVar4 * 8);
      if (iVar1 < 1) {
        return 0xffffd8f2;
      }
      uVar2 = FUN_18001d4c0(*(longlong *)
                             (*(longlong *)(param_1 + 0x28) +
                             (longlong)*(int *)(param_2 + uVar4 * 8) * 8),iVar1);
      if ((int)uVar2 != 0) {
        return uVar2;
      }
      uVar3 = (int)uVar4 + 1;
      uVar4 = (ulonglong)uVar3;
    } while (uVar3 < (uint)uVar5);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001fe50
   NAME : FUN_18001fe50
   SIG  : undefined8 __fastcall FUN_18001fe50(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

undefined8 FUN_18001fe50(longlong param_1,longlong param_2,uint param_3)

{
  int iVar1;
  undefined8 uVar2;
  uint uVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  
  uVar4 = 0;
  uVar5 = (ulonglong)param_3;
  if (param_3 != 0) {
    do {
      iVar1 = *(int *)(param_2 + 4 + uVar4 * 8);
      if (iVar1 < 1) {
        return 0xffffd8f2;
      }
      uVar2 = FUN_18001d4e0(*(longlong *)
                             (*(longlong *)(param_1 + 0x28) +
                             (longlong)*(int *)(param_2 + uVar4 * 8) * 8),iVar1);
      if ((int)uVar2 != 0) {
        return uVar2;
      }
      uVar3 = (int)uVar4 + 1;
      uVar4 = (ulonglong)uVar3;
    } while (uVar3 < (uint)uVar5);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001feb0
   NAME : FUN_18001feb0
   SIG  : undefined8 __fastcall FUN_18001feb0(int * param_1, int * param_2, int * param_3, undefined1 * param_4, int * param_5)
   ======================================================================== */

undefined8 FUN_18001feb0(int *param_1,int *param_2,int *param_3,undefined1 *param_4,int *param_5)

{
  if (param_2 != (int *)0x0) {
    if ((*param_2 != 0x28) || (param_2[2] != 1)) {
      return 0xffffd900;
    }
    *param_3 = param_2[3];
    if ((*(byte *)(param_2 + 3) & 8) != 0) {
      *param_4 = 0;
    }
    if ((*(byte *)(param_2 + 3) & 2) != 0) {
      if (*param_1 != -2) {
        return 0xffffd8f4;
      }
      *param_5 = param_2[8];
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 18001ff00
   NAME : FUN_18001ff00
   SIG  : longlong __fastcall FUN_18001ff00(LPCWSTR param_1)
   ======================================================================== */

longlong FUN_18001ff00(LPCWSTR param_1)

{
  int iVar1;
  
  iVar1 = WideCharToMultiByte(0xfde9,0,param_1,-1,(LPSTR)0x0,0,(LPCSTR)0x0,(LPBOOL)0x0);
  return (longlong)iVar1;
}



/* ========================================================================
   ENTRY: 18001ff40
   NAME : FUN_18001ff40
   SIG  : int __fastcall FUN_18001ff40(longlong param_1, longlong param_2, uint param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

int FUN_18001ff40(longlong param_1,longlong param_2,uint param_3)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  HANDLE hHandle;
  int iVar4;
  int iVar5;
  DWORD DVar6;
  undefined8 uVar7;
  ulonglong uVar8;
  ulonglong uVar9;
  undefined1 *puVar10;
  longlong lVar11;
  uint uVar12;
  undefined8 *puVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  undefined1 auStack_88 [32];
  uint local_68;
  uint local_64;
  uint local_60;
  uint local_5c;
  longlong local_58;
  ulonglong local_50;
  
  puVar10 = auStack_88;
  local_50 = DAT_18002b580 ^ (ulonglong)&local_68;
  if (*(longlong *)(param_1 + 0x1b0) == 0) {
    iVar5 = -0x26f6;
  }
  else {
    uVar16 = *(uint *)(param_1 + 0x210);
    puVar10 = auStack_88;
    local_64 = param_3;
    local_58 = param_2;
    if (*(int *)(param_1 + 0xb4) == 0) {
      uVar9 = (ulonglong)*(uint *)(param_1 + 0xa8) * 8;
      uVar8 = uVar9 + 0xf;
      if (uVar8 <= uVar9) {
        uVar8 = 0xffffffffffffff0;
      }
      lVar11 = -(uVar8 & 0xfffffffffffffff0);
      puVar10 = auStack_88 + lVar11;
      local_58 = (longlong)&local_68 + lVar11;
      if (local_58 == 0) {
        iVar5 = -0x2708;
        goto LAB_180020160;
      }
      uVar8 = 0;
      puVar10 = auStack_88 + lVar11;
      if (*(uint *)(param_1 + 0xa8) != 0) {
        do {
          *(undefined8 *)(local_58 + uVar8 * 8) = *(undefined8 *)(param_2 + uVar8 * 8);
          uVar15 = (int)uVar8 + 1;
          uVar8 = (ulonglong)uVar15;
          puVar10 = auStack_88 + lVar11;
        } while (uVar15 < *(uint *)(param_1 + 0xa8));
      }
    }
    local_5c = uVar16 * 3;
    uVar16 = uVar16 >> 1;
    uVar15 = 0;
    iVar5 = 0;
    local_68 = 0;
    uVar12 = 0;
    local_60 = uVar16;
    uVar14 = local_5c;
    do {
      puVar13 = (undefined8 *)(param_1 + 0x1a8);
      *(undefined8 *)(puVar10 + -8) = 0x18001ffc4;
      iVar4 = FUN_18001c6b0(param_1);
      if (iVar4 == 0) {
        hHandle = (HANDLE)*puVar13;
        *(undefined8 *)(puVar10 + -8) = 0x180020128;
        DVar6 = WaitForSingleObject(hHandle,uVar16);
        if (DVar6 == 0xffffffff) {
          iVar5 = -9999;
          break;
        }
        if ((DVar6 == 0x102) && (uVar12 = uVar12 + uVar16, uVar14 <= uVar12)) {
          iVar5 = -0x2703;
          break;
        }
      }
      else {
        *(undefined8 *)(puVar10 + -8) = 0x18001ffd4;
        uVar7 = FUN_18001d520((longlong)puVar13);
        iVar4 = *(int *)(param_1 + 0x1d0);
        iVar1 = *(int *)(param_1 + 0x1cc);
        if ((int)uVar7 != 0) {
          iVar5 = -0x26fc;
        }
        iVar2 = *(int *)(param_1 + 0x1d4);
        *(undefined8 *)(puVar10 + -8) = 0x18001fffa;
        FUN_1800069a0(param_1 + 0x68,iVar4 - iVar2);
        iVar4 = 0;
        uVar8 = 0;
        if (*(int *)(param_1 + 0x1b8) != 0) {
          lVar11 = (longlong)iVar1 * 0x30;
          do {
            iVar1 = *(int *)(param_1 + 0x1d4);
            iVar2 = *(int *)(param_1 + 0xac);
            lVar3 = *(longlong *)(*(longlong *)(param_1 + 0x1c0) + uVar8 * 8);
            uVar16 = *(uint *)(lVar11 + 0x10 + lVar3);
            lVar3 = *(longlong *)(lVar11 + lVar3);
            *(undefined8 *)(puVar10 + -8) = 0x180020046;
            FUN_1800068d0(param_1 + 0x68,iVar4,(ulonglong)(iVar1 * iVar2 * uVar16) + lVar3,uVar16);
            iVar4 = iVar4 + uVar16;
            uVar16 = (int)uVar8 + 1;
            uVar8 = (ulonglong)uVar16;
            uVar15 = local_68;
          } while (uVar16 < *(uint *)(param_1 + 0x1b8));
        }
        uVar16 = local_64 - uVar15;
        *(undefined8 *)(puVar10 + -8) = 0x180020070;
        uVar16 = FUN_180005eb0(param_1 + 0x68,&local_58,uVar16);
        *(int *)(param_1 + 0x1d4) = *(int *)(param_1 + 0x1d4) + uVar16;
        if (*(int *)(param_1 + 0x1d4) == *(int *)(param_1 + 0x1d0)) {
          *(undefined8 *)(puVar10 + -8) = 0x18002008e;
          uVar8 = FUN_18001bd60(param_1);
          iVar5 = (int)uVar8;
          if (iVar5 != 0) break;
        }
        uVar12 = 0;
        uVar15 = uVar15 + uVar16;
        uVar16 = local_60;
        local_68 = uVar15;
        uVar14 = local_5c;
      }
    } while (uVar15 < local_64);
  }
LAB_180020160:
  *(undefined8 *)(puVar10 + -8) = 0x18002016c;
  return iVar5;
}



/* ========================================================================
   ENTRY: 180020180
   NAME : FUN_180020180
   SIG  : undefined8 __fastcall FUN_180020180(undefined4 param_1, undefined4 * param_2)
   ======================================================================== */

undefined8 FUN_180020180(undefined4 param_1,undefined4 *param_2)

{
  DWORD DVar1;
  
  *param_2 = 0xf1cd;
  DVar1 = CoInitialize((LPVOID)0x0);
  if (DVar1 != 0x80010106) {
    if ((int)DVar1 < 0) {
      if (DVar1 == 0x8007000e) {
        return 0xffffd8f8;
      }
      FUN_1800202f0(param_1,DVar1);
      return 0xffffd8f1;
    }
    *param_2 = 0xb38f;
    DVar1 = GetCurrentThreadId();
    param_2[1] = DVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180020200
   NAME : FUN_180020200
   SIG  : undefined __fastcall FUN_180020200(undefined8 param_1, int * param_2)
   ======================================================================== */

void FUN_180020200(undefined8 param_1,int *param_2)

{
  DWORD DVar1;
  
  if (*param_2 == 0xb38f) {
    DVar1 = GetCurrentThreadId();
    if (param_2[1] == DVar1) {
      CoUninitialize();
      *param_2 = 0xf1cd;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180020230
   NAME : FUN_180020230
   SIG  : undefined __fastcall FUN_180020230(int param_1)
   ======================================================================== */

void FUN_180020230(int param_1)

{
                    /* WARNING: Could not recover jumptable at 0x000180020238. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  GlobalAlloc(0x40,(longlong)param_1);
  return;
}



/* ========================================================================
   ENTRY: 180020240
   NAME : FUN_180020240
   SIG  : undefined __fastcall FUN_180020240(longlong param_1)
   ======================================================================== */

void FUN_180020240(longlong param_1)

{
  if (param_1 != 0) {
                    /* WARNING: Could not recover jumptable at 0x000180020245. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    GlobalFree();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180020250
   NAME : FUN_180020250
   SIG  : double __fastcall FUN_180020250(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

double FUN_180020250(void)

{
  DWORD DVar1;
  LARGE_INTEGER local_res8 [4];
  
  if (DAT_18002bbb0 != 0) {
    QueryPerformanceCounter(local_res8);
    return (double)local_res8[0].QuadPart * _DAT_18002bbb8;
  }
  DVar1 = timeGetTime();
  return (double)DVar1 * _DAT_180023cb0;
}



/* ========================================================================
   ENTRY: 1800202a0
   NAME : FUN_1800202a0
   SIG  : undefined __fastcall FUN_1800202a0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800202a0(void)

{
  BOOL BVar1;
  LARGE_INTEGER local_res8 [4];
  
  BVar1 = QueryPerformanceFrequency(local_res8);
  if (BVar1 != 0) {
    DAT_18002bbb0 = 1;
    _DAT_18002bbb8 = DAT_1800235c8 / (double)local_res8[0].QuadPart;
    return;
  }
  DAT_18002bbb0 = 0;
  return;
}



/* ========================================================================
   ENTRY: 1800202f0
   NAME : FUN_1800202f0
   SIG  : undefined __fastcall FUN_1800202f0(undefined4 param_1, DWORD param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_1800202f0(undefined4 param_1,DWORD param_2)

{
  undefined1 auStackY_c58 [32];
  CHAR local_c18 [1024];
  WCHAR local_818 [1024];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStackY_c58;
  FormatMessageW(0x1200,(LPCVOID)0x0,param_2,0x400,local_818,0x400,(va_list *)0x0);
  WideCharToMultiByte(0xfde9,0,local_818,-1,local_c18,0x400,(LPCSTR)0x0,(LPBOOL)0x0);
  FUN_180003c90(param_1,param_2,local_c18);
  return;
}



/* ========================================================================
   ENTRY: 1800203c0
   NAME : Pa_Sleep
   SIG  : void __stdcall Pa_Sleep(DWORD dwMilliseconds)
   ======================================================================== */

void __stdcall Pa_Sleep(DWORD dwMilliseconds)

{
                    /* WARNING: Could not recover jumptable at 0x0001800203c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
                    /* 0x203c0  34  Pa_Sleep */
  Sleep(dwMilliseconds);
  return;
}



/* ========================================================================
   ENTRY: 1800203d0
   NAME : FUN_1800203d0
   SIG  : int __fastcall FUN_1800203d0(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_1800203d0(void)

{
  bool bVar1;
  HMODULE pHVar2;
  FARPROC pFVar3;
  INT_PTR IVar4;
  undefined7 extraout_var;
  undefined7 extraout_var_00;
  undefined7 extraout_var_01;
  undefined7 extraout_var_02;
  undefined7 extraout_var_03;
  int iVar5;
  undefined1 auStack_158 [32];
  undefined8 local_138;
  undefined8 local_130;
  int local_128;
  undefined2 local_124;
  undefined1 local_122 [266];
  ulonglong local_18;
  
  local_18 = DAT_18002b580 ^ (ulonglong)auStack_158;
  pHVar2 = GetModuleHandleA("ntdll");
  pFVar3 = GetProcAddress(pHVar2,"RtlGetVersion");
  iVar5 = 0;
  if (pFVar3 == (FARPROC)0x0) {
LAB_18002045e:
    pHVar2 = GetModuleHandleA("kernel32");
    pFVar3 = GetProcAddress(pHVar2,"GetVersion");
    if (pFVar3 != (FARPROC)0x0) {
      IVar4 = (*pFVar3)();
      local_128 = 2;
      local_130._0_4_ = ((uint)IVar4 & 0xffff) >> 8;
      local_138._4_4_ = (uint)(byte)IVar4;
      goto LAB_180020495;
    }
    bVar1 = FUN_1800205f0(10,0,0);
    if ((int)CONCAT71(extraout_var,bVar1) != 0) {
      return 10;
    }
    bVar1 = FUN_1800205f0(6,3,0);
    if ((int)CONCAT71(extraout_var_00,bVar1) == 0) {
      bVar1 = FUN_1800205f0(6,2,0);
      if ((int)CONCAT71(extraout_var_01,bVar1) != 0) {
        return 8;
      }
      bVar1 = FUN_1800205f0(6,1,0);
      if ((int)CONCAT71(extraout_var_02,bVar1) != 0) {
        return 7;
      }
      bVar1 = FUN_1800205f0(6,0,0);
      if ((int)CONCAT71(extraout_var_03,bVar1) != 0) {
        return 6;
      }
      return 1000;
    }
LAB_1800204f9:
    iVar5 = 9;
  }
  else {
    local_138 = 0x114;
    local_130 = 0;
    local_128 = 0;
    local_124 = 0;
    memset(local_122,0,0xfe);
    IVar4 = (*pFVar3)(&local_138);
    if (((int)IVar4 != 0) || (local_138._4_4_ == 0xffffffff)) goto LAB_18002045e;
LAB_180020495:
    switch(local_138._4_4_) {
    case 0:
    case 1:
    case 2:
    case 3:
      break;
    case 4:
      iVar5 = (local_128 == 2) + 1;
      break;
    case 5:
      if ((uint)local_130 == 0) {
        iVar5 = 3;
      }
      else if ((uint)local_130 == 1) {
        iVar5 = 4;
      }
      else {
        iVar5 = 5;
      }
      break;
    case 6:
      if ((uint)local_130 == 0) {
        return 6;
      }
      if ((uint)local_130 == 1) {
        return 7;
      }
      if ((uint)local_130 == 2) {
        return 8;
      }
      goto LAB_1800204f9;
    default:
      iVar5 = 1000;
      break;
    case 10:
      iVar5 = 10;
    }
  }
  return iVar5;
}



/* ========================================================================
   ENTRY: 1800205f0
   NAME : FUN_1800205f0
   SIG  : bool __fastcall FUN_1800205f0(undefined2 param_1, ushort param_2, undefined8 param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

bool FUN_1800205f0(undefined2 param_1,ushort param_2,undefined8 param_3)

{
  HMODULE pHVar1;
  FARPROC pFVar2;
  FARPROC pFVar3;
  INT_PTR IVar4;
  undefined2 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStack_f8 [32];
  ulonglong local_d8;
  undefined8 local_d0;
  undefined4 local_c8;
  undefined1 local_c4;
  undefined8 local_c3;
  undefined8 uStack_bb;
  undefined8 local_b3;
  undefined8 uStack_ab;
  undefined8 local_a3;
  undefined8 uStack_9b;
  undefined8 local_93;
  undefined8 uStack_8b;
  undefined8 local_83;
  undefined8 uStack_7b;
  undefined8 local_73;
  undefined8 uStack_6b;
  undefined8 local_63;
  undefined7 uStack_5b;
  undefined1 uStack_54;
  undefined7 uStack_53;
  undefined8 uStack_4c;
  undefined8 local_44;
  ulonglong local_38;
  
  local_38 = DAT_18002b580 ^ (ulonglong)auStack_f8;
  local_d8 = 0x9c;
  local_c3 = 0;
  uStack_bb = 0;
  local_d0 = 0;
  local_c8 = 0;
  uVar5 = (undefined2)param_3;
  local_c4 = 0;
  local_b3 = 0;
  uStack_ab = 0;
  local_a3 = 0;
  uStack_9b = 0;
  local_93 = 0;
  uStack_8b = 0;
  local_83 = 0;
  uStack_7b = 0;
  local_44 = 0;
  local_63 = 0;
  uStack_5b = 0;
  uStack_54 = 0;
  uStack_53 = 0;
  uStack_4c = 0;
  local_73 = 0;
  uStack_6b = 0;
  pHVar1 = GetModuleHandleA("kernel32");
  pFVar2 = GetProcAddress(pHVar1,"VerSetConditionMask");
  pHVar1 = GetModuleHandleA("kernel32");
  pFVar3 = GetProcAddress(pHVar1,"VerifyVersionInfoA");
  if ((pFVar2 == (FARPROC)0x0) || (pFVar3 == (FARPROC)0x0)) {
    bVar7 = false;
  }
  else {
    uVar6 = CONCAT71((int7)((ulonglong)param_3 >> 8),3);
    IVar4 = (*pFVar2)(0,2,uVar6);
    uVar6 = CONCAT71((int7)((ulonglong)uVar6 >> 8),3);
    IVar4 = (*pFVar2)(IVar4,1,uVar6);
    IVar4 = (*pFVar2)(IVar4,0x20,CONCAT71((int7)((ulonglong)uVar6 >> 8),3));
    local_d8 = (ulonglong)CONCAT24(param_1,(undefined4)local_d8);
    local_d0 = CONCAT44(local_d0._4_4_,(uint)param_2);
    local_44 = CONCAT62(local_44._2_6_,uVar5);
    IVar4 = (*pFVar3)(&local_d8,0x23,IVar4);
    bVar7 = (int)IVar4 != 0;
  }
  return bVar7;
}



/* ========================================================================
   ENTRY: 180020730
   NAME : FUN_180020730
   SIG  : undefined __fastcall FUN_180020730(void)
   ======================================================================== */

void FUN_180020730(void)

{
  if (DAT_18002bbc0 == 0) {
    DAT_18002bbc0 = FUN_1800203d0();
  }
  return;
}



/* ========================================================================
   ENTRY: 180020750
   NAME : FUN_180020750
   SIG  : undefined8 __fastcall FUN_180020750(undefined4 param_1)
   ======================================================================== */

undefined8 FUN_180020750(undefined4 param_1)

{
  switch(param_1) {
  case 1:
    return 4;
  case 2:
    return 3;
  case 3:
    return 7;
  case 4:
    return 0x33;
  case 5:
    return 0x37;
  case 6:
    return 0x3f;
  default:
    return 0;
  case 8:
    return 0x63f;
  }
}



/* ========================================================================
   ENTRY: 1800207c0
   NAME : FUN_1800207c0
   SIG  : undefined __fastcall FUN_1800207c0(undefined2 * param_1, int param_2, uint param_3, undefined2 param_4, double param_5)
   ======================================================================== */

void FUN_1800207c0(undefined2 *param_1,int param_2,uint param_3,undefined2 param_4,double param_5)

{
  undefined8 uVar1;
  int iVar2;
  
  uVar1 = Pa_GetSampleSize(param_3);
  *param_1 = param_4;
  param_1[7] = (short)uVar1 << 3;
  param_1[1] = (short)param_2;
  iVar2 = (int)uVar1 * param_2;
  *(int *)(param_1 + 2) = (int)(longlong)param_5;
  param_1[6] = (short)iVar2;
  param_1[8] = 0;
  *(int *)(param_1 + 4) = (int)(longlong)param_5 * iVar2;
  return;
}



/* ========================================================================
   ENTRY: 180020830
   NAME : FUN_180020830
   SIG  : undefined __fastcall FUN_180020830(undefined2 * param_1, int param_2, uint param_3, uint param_4, double param_5, undefined4 param_6)
   ======================================================================== */

void FUN_180020830(undefined2 *param_1,int param_2,uint param_3,uint param_4,double param_5,
                  undefined4 param_6)

{
  undefined8 uVar1;
  undefined4 uVar2;
  short sVar3;
  undefined8 uVar4;
  int iVar5;
  
  uVar4 = Pa_GetSampleSize(param_3);
  param_1[1] = (short)param_2;
  *param_1 = 0xfffe;
  param_1[8] = 0x16;
  uVar1 = DAT_18002b534;
  *(int *)(param_1 + 2) = (int)(longlong)param_5;
  iVar5 = (int)uVar4 * param_2;
  param_1[6] = (short)iVar5;
  sVar3 = (short)uVar4 << 3;
  param_1[7] = sVar3;
  *(int *)(param_1 + 4) = (int)(longlong)param_5 * iVar5;
  param_1[9] = sVar3;
  *(undefined4 *)(param_1 + 10) = param_6;
  *(uint *)(param_1 + 0xc) = param_4 & 0xffff;
  uVar2 = DAT_18002b53c;
  *(undefined8 *)(param_1 + 0xe) = uVar1;
  *(undefined4 *)(param_1 + 0x12) = uVar2;
  return;
}



/* ========================================================================
   ENTRY: 1800208c0
   NAME : FUN_1800208c0
   SIG  : undefined8 __fastcall FUN_1800208c0(int param_1)
   ======================================================================== */

undefined8 FUN_1800208c0(int param_1)

{
  undefined8 uVar1;
  
  uVar1 = 3;
  if (param_1 != 2) {
    uVar1 = 1;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800208e0
   NAME : FUN_1800208e0
   SIG  : undefined4 __fastcall FUN_1800208e0(HANDLE param_1)
   ======================================================================== */

undefined4 FUN_1800208e0(HANDLE param_1)

{
  undefined8 uVar1;
  undefined4 uVar2;
  undefined4 local_res10 [6];
  
  uVar1 = FUN_180020e00(param_1,0,1,local_res10,4);
  uVar2 = 0;
  if ((int)uVar1 == 0) {
    uVar2 = local_res10[0];
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180020910
   NAME : FUN_180020910
   SIG  : undefined4 __fastcall FUN_180020910(HANDLE param_1, undefined4 param_2)
   ======================================================================== */

undefined4 FUN_180020910(HANDLE param_1,undefined4 param_2)

{
  undefined8 uVar1;
  undefined4 uVar2;
  undefined4 local_res18 [4];
  
  uVar1 = FUN_180020e00(param_1,param_2,7,local_res18,4);
  uVar2 = 0;
  if ((int)uVar1 == 0) {
    uVar2 = local_res18[0];
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180020940
   NAME : FUN_180020940
   SIG  : undefined4 __fastcall FUN_180020940(HANDLE param_1, undefined4 param_2)
   ======================================================================== */

undefined4 FUN_180020940(HANDLE param_1,undefined4 param_2)

{
  undefined8 uVar1;
  undefined4 uVar2;
  undefined4 local_res18 [4];
  
  uVar1 = FUN_180020e00(param_1,param_2,2,local_res18,4);
  uVar2 = 0;
  if ((int)uVar1 == 0) {
    uVar2 = local_res18[0];
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180020970
   NAME : FUN_180020970
   SIG  : undefined8 __fastcall FUN_180020970(HANDLE param_1, undefined4 param_2, undefined4 param_3, longlong * param_4, int param_5)
   ======================================================================== */

undefined8
FUN_180020970(HANDLE param_1,undefined4 param_2,undefined4 param_3,longlong *param_4,int param_5)

{
  uint uVar1;
  undefined8 uVar2;
  ulonglong uVar3;
  longlong local_18 [2];
  
  local_18[0] = 0;
  uVar2 = FUN_180020cd0(param_1,param_2,param_3,local_18);
  if ((int)uVar2 != 0) {
    return 0;
  }
  uVar3 = 0;
  uVar2 = 0;
  if (0 < *(int *)(local_18[0] + 4)) {
    do {
      if (((*(longlong *)(local_18[0] + 8 + uVar3 * 0x18) == *param_4) &&
          (*(longlong *)(local_18[0] + 0x10 + uVar3 * 0x18) == param_4[1])) &&
         (*(int *)(local_18[0] + 0x18 + uVar3 * 0x18) == param_5)) {
        uVar2 = 1;
        break;
      }
      uVar1 = (int)uVar3 + 1;
      uVar3 = (ulonglong)uVar1;
    } while ((int)uVar1 < *(int *)(local_18[0] + 4));
  }
  FUN_180020240(local_18[0]);
  return uVar2;
}



/* ========================================================================
   ENTRY: 180020a10
   NAME : FUN_180020a10
   SIG  : uint __fastcall FUN_180020a10(LPCWSTR param_1, int param_2)
   ======================================================================== */

uint FUN_180020a10(LPCWSTR param_1,int param_2)

{
  uint uVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  HANDLE hObject;
  undefined8 uVar5;
  uint *puVar6;
  uint uVar7;
  int local_res10;
  longlong local_res20;
  
  uVar7 = 0;
  if ((param_1 != (LPCWSTR)0x0) &&
     (hObject = CreateFileW(param_1,3,0,(LPSECURITY_ATTRIBUTES)0x0,3,0,(HANDLE)0x0),
     hObject != (HANDLE)0xffffffffffffffff)) {
    iVar2 = FUN_1800208e0(hObject);
    local_res10 = 0;
    if (0 < iVar2) {
      do {
        iVar3 = FUN_180020910(hObject,local_res10);
        iVar4 = FUN_180020940(hObject,local_res10);
        if ((((iVar4 == (param_2 != 0) + 1) && ((iVar3 - 1U & 0xfffffffd) == 0)) &&
            ((uVar5 = FUN_180020970(hObject,local_res10,5,(longlong *)&DAT_180025d70,0),
             (int)uVar5 != 0 ||
             (uVar5 = FUN_180020970(hObject,local_res10,5,(longlong *)&DAT_180025d70,1),
             (int)uVar5 != 0)))) &&
           (uVar5 = FUN_180020970(hObject,local_res10,6,(longlong *)&DAT_180025d60,0),
           (int)uVar5 != 0)) {
          local_res20 = 0;
          uVar5 = FUN_180020cd0(hObject,local_res10,3,&local_res20);
          if ((int)uVar5 == 0) {
            puVar6 = (uint *)(local_res20 + 8);
            for (iVar3 = *(int *)(local_res20 + 4); iVar3 != 0; iVar3 = iVar3 + -1) {
              if (((((((DAT_180025d52 == *(longlong *)((longlong)puVar6 + 0x22)) &&
                      (DAT_180025d5a == *(int *)((longlong)puVar6 + 0x2a))) &&
                     (DAT_180025d5e == *(short *)((longlong)puVar6 + 0x2e))) ||
                    ((*(longlong *)(puVar6 + 8) == DAT_180025d40 &&
                     (*(longlong *)(puVar6 + 10) == DAT_180025d48)))) ||
                   ((*(longlong *)(puVar6 + 8) == DAT_180025d30 &&
                    (*(longlong *)(puVar6 + 10) == DAT_180025d38)))) ||
                  (((*(longlong *)(puVar6 + 4) == DAT_180025d20 &&
                    (*(longlong *)(puVar6 + 6) == DAT_180025d28)) &&
                   ((*(longlong *)(puVar6 + 8) == DAT_180024940 &&
                    (*(longlong *)(puVar6 + 10) == DAT_180024948)))))) &&
                 ((uVar1 = puVar6[0x10], uVar1 < 0xffff && ((int)uVar7 < (int)uVar1)))) {
                uVar7 = uVar1;
              }
              puVar6 = (uint *)((longlong)puVar6 + (ulonglong)*puVar6);
            }
            FUN_180020240(local_res20);
          }
        }
        local_res10 = local_res10 + 1;
      } while (local_res10 < iVar2);
    }
    CloseHandle(hObject);
    return uVar7;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180020cd0
   NAME : FUN_180020cd0
   SIG  : undefined8 __fastcall FUN_180020cd0(HANDLE param_1, undefined4 param_2, undefined4 param_3, undefined8 * param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180020cd0(HANDLE param_1,undefined4 param_2,undefined4 param_3,undefined8 *param_4)

{
  BOOL BVar1;
  DWORD DVar2;
  LPVOID lpOutBuffer;
  DWORD local_res10 [2];
  DWORD local_res18 [2];
  undefined4 local_28;
  undefined4 uStack_24;
  undefined4 uStack_20;
  undefined4 uStack_1c;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  
  uStack_1c = _UNK_180025d8c;
  uStack_20 = _UNK_180025d88;
  uStack_24 = _UNK_180025d84;
  local_28 = _DAT_180025d80;
  local_res10[0] = 0;
  local_14 = 1;
  local_c = 0;
  *param_4 = 0;
  local_18 = param_3;
  local_10 = param_2;
  BVar1 = DeviceIoControl(param_1,0x2f0003,&local_28,0x20,(LPVOID)0x0,0,local_res10,
                          (LPOVERLAPPED)0x0);
  if ((BVar1 == 0) && (DVar2 = GetLastError(), DVar2 != 0xea)) {
    return 0xffffd8f1;
  }
  lpOutBuffer = (LPVOID)FUN_180020230(local_res10[0]);
  *param_4 = lpOutBuffer;
  if (lpOutBuffer == (LPVOID)0x0) {
    return 0xffffd8f8;
  }
  BVar1 = DeviceIoControl(param_1,0x2f0003,&local_28,0x20,lpOutBuffer,local_res10[0],local_res18,
                          (LPOVERLAPPED)0x0);
  if ((BVar1 != 0) && (local_res18[0] == local_res10[0])) {
    return 0;
  }
  FUN_180020240((longlong)param_4);
  return 0xffffd8f1;
}



/* ========================================================================
   ENTRY: 180020e00
   NAME : FUN_180020e00
   SIG  : undefined8 __fastcall FUN_180020e00(HANDLE param_1, undefined4 param_2, undefined4 param_3, LPVOID param_4, DWORD param_5)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
FUN_180020e00(HANDLE param_1,undefined4 param_2,undefined4 param_3,LPVOID param_4,DWORD param_5)

{
  BOOL BVar1;
  undefined8 uVar2;
  DWORD local_res10 [6];
  undefined4 local_28;
  undefined4 uStack_24;
  undefined4 uStack_20;
  undefined4 uStack_1c;
  undefined4 local_18;
  undefined4 local_14;
  undefined4 local_10;
  undefined4 local_c;
  
  local_14 = 1;
  local_c = 0;
  local_28 = _DAT_180025d80;
  uStack_24 = _UNK_180025d84;
  uStack_20 = _UNK_180025d88;
  uStack_1c = _UNK_180025d8c;
  local_18 = param_3;
  local_10 = param_2;
  BVar1 = DeviceIoControl(param_1,0x2f0003,&local_28,0x20,param_4,param_5,local_res10,
                          (LPOVERLAPPED)0x0);
  uVar2 = 0xffffd8f1;
  if ((BVar1 != 0) && (uVar2 = 0xffffd8f1, local_res10[0] == param_5)) {
    uVar2 = 0;
  }
  return uVar2;
}



/* ========================================================================
   ENTRY: 180020e80
   NAME : FUN_180020e80
   SIG  : undefined8 __fastcall FUN_180020e80(void)
   ======================================================================== */

undefined8 FUN_180020e80(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180020e90
   NAME : PA_AbortStream
   SIG  : undefined8 __fastcall PA_AbortStream(int * param_1)
   ======================================================================== */

undefined8 PA_AbortStream(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180003d1c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x18))(param_1);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020ea0
   NAME : PA_CloseStream
   SIG  : ulonglong __fastcall PA_CloseStream(int * param_1)
   ======================================================================== */

ulonglong PA_CloseStream(int *param_1)

{
  undefined8 *puVar1;
  undefined8 uVar2;
  ulonglong uVar3;
  uint uVar4;
  
  uVar2 = FUN_180003cc0(param_1);
  uVar4 = (uint)uVar2;
  FUN_180004a10((longlong)param_1);
  if (uVar4 != 0) {
    return (ulonglong)uVar4;
  }
  puVar1 = *(undefined8 **)(param_1 + 4);
  uVar3 = (*(code *)puVar1[4])();
  if ((int)uVar3 != 1) {
    if ((int)uVar3 != 0) {
      return uVar3;
    }
    uVar3 = (*(code *)puVar1[3])(param_1);
    if ((int)uVar3 != 0) {
      return uVar3;
    }
  }
  uVar3 = (*(code *)*puVar1)(param_1);
  return uVar3;
}



/* ========================================================================
   ENTRY: 180020eb0
   NAME : PA_GetDefaultHostApi
   SIG  : int __fastcall PA_GetDefaultHostApi(void)
   ======================================================================== */

int PA_GetDefaultHostApi(void)

{
  int iVar1;
  
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  if ((DAT_18002babc < 0) || (iVar1 = DAT_18002babc, DAT_18002bab8 <= DAT_18002babc)) {
    iVar1 = -0x2702;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 180020ec0
   NAME : PA_GetDefaultInputDevice
   SIG  : undefined4 __fastcall PA_GetDefaultInputDevice(void)
   ======================================================================== */

undefined4 PA_GetDefaultInputDevice(void)

{
  int iVar1;
  
  iVar1 = Pa_GetDefaultHostApi();
  if (iVar1 < 0) {
    return 0xffffffff;
  }
  return *(undefined4 *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x1c);
}



/* ========================================================================
   ENTRY: 180020ed0
   NAME : PA_GetDefaultOutputDevice
   SIG  : undefined4 __fastcall PA_GetDefaultOutputDevice(void)
   ======================================================================== */

undefined4 PA_GetDefaultOutputDevice(void)

{
  int iVar1;
  
  iVar1 = Pa_GetDefaultHostApi();
  if (iVar1 < 0) {
    return 0xffffffff;
  }
  return *(undefined4 *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x20);
}



/* ========================================================================
   ENTRY: 180020ee0
   NAME : PA_GetDeviceCount
   SIG  : undefined4 __fastcall PA_GetDeviceCount(void)
   ======================================================================== */

undefined4 PA_GetDeviceCount(void)

{
  undefined4 uVar1;
  
                    /* 0x20ee0  40  PA_GetDeviceCount */
  uVar1 = DAT_18002bac8;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020ef0
   NAME : PA_GetDeviceInfo
   SIG  : undefined8 __fastcall PA_GetDeviceInfo(int param_1)
   ======================================================================== */

undefined8 PA_GetDeviceInfo(int param_1)

{
  int iVar1;
  int aiStackX_10 [6];
  
  iVar1 = FUN_180003aa0(param_1,aiStackX_10);
  if (iVar1 < 0) {
    return 0;
  }
  return *(undefined8 *)
          (*(longlong *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0x28) +
          (longlong)aiStackX_10[0] * 8);
}



/* ========================================================================
   ENTRY: 180020f00
   NAME : PA_GetErrorText
   SIG  : char * __fastcall PA_GetErrorText(int param_1)
   ======================================================================== */

char * PA_GetErrorText(int param_1)

{
  if (0 < param_1) {
    return "Invalid error code (value greater than zero)";
  }
  if (param_1 == 0) {
    return "Success";
  }
  switch(param_1) {
  case -10000:
    return "PortAudio not initialized";
  case -9999:
    return "Unanticipated host error";
  case -0x270e:
    return "Invalid number of channels";
  case -0x270d:
    return "Invalid sample rate";
  case -0x270c:
    return "Invalid device";
  case -0x270b:
    return "Invalid flag";
  case -0x270a:
    return "Sample format not supported";
  case -0x2709:
    return "Illegal combination of I/O devices";
  case -0x2708:
    return "Insufficient memory";
  case -0x2707:
    return "Buffer too big";
  case -0x2706:
    return "Buffer too small";
  case -0x2705:
    return "No callback routine specified";
  case -0x2704:
    return "Invalid stream pointer";
  case -0x2703:
    return "Wait timed out";
  case -0x2702:
    return "Internal PortAudio error";
  case -0x2701:
    return "Device unavailable";
  case -0x2700:
    return "Incompatible host API specific stream info";
  case -0x26ff:
    return "Stream is stopped";
  case -0x26fe:
    return "Stream is not stopped";
  case -0x26fd:
    return "Input overflowed";
  case -0x26fc:
    return "Output underflowed";
  case -0x26fb:
    return "Host API not found";
  case -0x26fa:
    return "Invalid host API";
  case -0x26f9:
    return "Can\'t read from a callback stream";
  case -0x26f8:
    return "Can\'t write to a callback stream";
  case -0x26f7:
    return "Can\'t read from an output only stream";
  case -0x26f6:
    return "Can\'t write to an input only stream";
  case -0x26f5:
    return "Incompatible stream host API";
  case -0x26f4:
    return "Bad buffer pointer";
  case -0x26f3:
    return "PortAudio can not be initialized recursively";
  default:
    return "Invalid error code";
  }
}



/* ========================================================================
   ENTRY: 180020f10
   NAME : PA_GetHostApiCount
   SIG  : undefined4 __fastcall PA_GetHostApiCount(void)
   ======================================================================== */

undefined4 PA_GetHostApiCount(void)

{
  undefined4 uVar1;
  
                    /* 0x20f10  43  PA_GetHostApiCount */
  uVar1 = DAT_18002bab8;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020f20
   NAME : PA_GetHostApiInfo
   SIG  : longlong __fastcall PA_GetHostApiInfo(int param_1)
   ======================================================================== */

longlong PA_GetHostApiInfo(int param_1)

{
  if (((DAT_18002bac0 != 0) && (-1 < param_1)) && (param_1 < DAT_18002bab8)) {
    return *(longlong *)(DAT_18002bab0 + (longlong)param_1 * 8) + 8;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180020f30
   NAME : PA_GetLastHostErrorInfo
   SIG  : undefined * __fastcall PA_GetLastHostErrorInfo(void)
   ======================================================================== */

undefined * PA_GetLastHostErrorInfo(void)

{
  return &DAT_18002b2a0;
                    /* 0x20f30  45  PA_GetLastHostErrorInfo */
}



/* ========================================================================
   ENTRY: 180020f40
   NAME : PA_GetSampleSize
   SIG  : undefined8 __fastcall PA_GetSampleSize(uint param_1)
   ======================================================================== */

undefined8 PA_GetSampleSize(uint param_1)

{
  switch(param_1 & 0x7fffffff) {
  case 1:
    return 8;
  case 2:
  case 4:
    return 4;
  default:
    return 0xffffd8f6;
  case 8:
    return 3;
  case 0x10:
    return 2;
  case 0x20:
  case 0x40:
    return 1;
  }
}



/* ========================================================================
   ENTRY: 180020f50
   NAME : PA_GetStreamCpuLoad
   SIG  : undefined8 __fastcall PA_GetStreamCpuLoad(int * param_1)
   ======================================================================== */

undefined8 PA_GetStreamCpuLoad(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004177. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x38))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020f60
   NAME : PA_GetStreamInfo
   SIG  : int * __fastcall PA_GetStreamInfo(int * param_1)
   ======================================================================== */

int * PA_GetStreamInfo(int *param_1)

{
  undefined8 uVar1;
  int *piVar2;
  
  uVar1 = FUN_180003cc0(param_1);
                    /* 0x20f60  48  PA_GetStreamInfo */
  piVar2 = (int *)0x0;
  if ((int)uVar1 == 0) {
    piVar2 = param_1 + 0xc;
  }
  return piVar2;
}



/* ========================================================================
   ENTRY: 180020f70
   NAME : PA_GetStreamReadAvailable
   SIG  : undefined8 __fastcall PA_GetStreamReadAvailable(int * param_1)
   ======================================================================== */

undefined8 PA_GetStreamReadAvailable(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800041d6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x50))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020f80
   NAME : PA_GetStreamTime
   SIG  : undefined8 __fastcall PA_GetStreamTime(int * param_1)
   ======================================================================== */

undefined8 PA_GetStreamTime(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004207. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x30))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020f90
   NAME : PA_GetStreamWriteAvailable
   SIG  : undefined8 __fastcall PA_GetStreamWriteAvailable(int * param_1)
   ======================================================================== */

undefined8 PA_GetStreamWriteAvailable(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 != 0) {
    return 0;
  }
                    /* WARNING: Could not recover jumptable at 0x000180004236. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x58))(param_1);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180020fa0
   NAME : PA_GetVersion
   SIG  : undefined8 __fastcall PA_GetVersion(void)
   ======================================================================== */

undefined8 PA_GetVersion(void)

{
  return 0x130700;
                    /* 0x20fa0  73  PA_GetVersion */
}



/* ========================================================================
   ENTRY: 180020fb0
   NAME : PA_GetVersionText
   SIG  : char * __fastcall PA_GetVersionText(void)
   ======================================================================== */

char * PA_GetVersionText(void)

{
  return "PortAudio V19.7.0-devel, revision unknown";
                    /* 0x20fb0  74  PA_GetVersionText */
}



/* ========================================================================
   ENTRY: 180020fc0
   NAME : PA_HostApiDeviceIndexToDeviceIndex
   SIG  : int __fastcall PA_HostApiDeviceIndexToDeviceIndex(int param_1, int param_2)
   ======================================================================== */

int PA_HostApiDeviceIndexToDeviceIndex(int param_1,int param_2)

{
  int *piVar1;
  
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  if ((-1 < param_1) && (param_1 < DAT_18002bab8)) {
    if ((-1 < param_2) &&
       (piVar1 = *(int **)(DAT_18002bab0 + (longlong)param_1 * 8), param_2 < piVar1[6])) {
      return *piVar1 + param_2;
    }
    return -0x270c;
  }
  return -0x26fa;
}



/* ========================================================================
   ENTRY: 180020fd0
   NAME : PA_HostApiTypeIdToHostApiIndex
   SIG  : int __fastcall PA_HostApiTypeIdToHostApiIndex(int param_1)
   ======================================================================== */

int PA_HostApiTypeIdToHostApiIndex(int param_1)

{
  int iVar1;
  
  if (DAT_18002bac0 == 0) {
    return -10000;
  }
  iVar1 = 0;
  if (0 < DAT_18002bab8) {
    do {
      if (*(int *)(*(longlong *)(DAT_18002bab0 + (longlong)iVar1 * 8) + 0xc) == param_1) {
        return iVar1;
      }
      iVar1 = iVar1 + 1;
    } while (iVar1 < DAT_18002bab8);
  }
  return -0x26fb;
}



/* ========================================================================
   ENTRY: 180020fe0
   NAME : PA_Initialize
   SIG  : int __fastcall PA_Initialize(void)
   ======================================================================== */

int PA_Initialize(void)

{
  int iVar1;
  
  if (DAT_18002bac0 != 0) {
    DAT_18002bac0 = DAT_18002bac0 + 1;
    return 0;
  }
  if (DAT_18002bac4 != 0) {
    return -0x26f3;
  }
  DAT_18002bac4 = 1;
  FUN_1800202a0();
  iVar1 = FUN_180003b00();
  if (iVar1 == 0) {
    DAT_18002bac0 = DAT_18002bac0 + 1;
  }
  DAT_18002bac4 = 0;
  return iVar1;
}



/* ========================================================================
   ENTRY: 180020ff0
   NAME : PA_IsFormatSupported
   SIG  : undefined8 __fastcall PA_IsFormatSupported(int * param_1, int * param_2, double param_3)
   ======================================================================== */

undefined8 PA_IsFormatSupported(int *param_1,int *param_2,double param_3)

{
  undefined8 uVar1;
  int *piVar2;
  int *piVar3;
  int aiStackX_20 [2];
  int aiStack_68 [2];
  longlong lStack_60;
  int iStack_58;
  int iStack_54;
  int iStack_50;
  undefined8 uStack_48;
  undefined8 uStack_40;
  int iStack_38;
  int iStack_34;
  int iStack_30;
  undefined8 uStack_28;
  undefined8 uStack_20;
  
                    /* 0x20ff0  78  PA_IsFormatSupported */
  lStack_60 = 0;
  aiStackX_20[0] = -1;
  aiStack_68[0] = -1;
  if (DAT_18002bac0 == 0) {
    uVar1 = 0xffffd8f0;
  }
  else {
    uVar1 = FUN_180004b00(param_1,param_2,param_3,0,0,0,&lStack_60,aiStackX_20,aiStack_68);
    if ((int)uVar1 == 0) {
      if (param_1 == (int *)0x0) {
        piVar2 = (int *)0x0;
      }
      else {
        piVar2 = &iStack_58;
        uStack_48 = *(undefined8 *)(param_1 + 4);
        iStack_58 = aiStackX_20[0];
        iStack_54 = param_1[1];
        iStack_50 = param_1[2];
        uStack_40 = *(undefined8 *)(param_1 + 6);
      }
      if (param_2 == (int *)0x0) {
        piVar3 = (int *)0x0;
      }
      else {
        piVar3 = &iStack_38;
        uStack_28 = *(undefined8 *)(param_2 + 4);
        iStack_38 = aiStack_68[0];
        iStack_34 = param_2[1];
        iStack_30 = param_2[2];
        uStack_20 = *(undefined8 *)(param_2 + 6);
      }
      uVar1 = (**(code **)(lStack_60 + 0x40))(lStack_60,piVar2,piVar3,SUB84(param_3,0));
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180021000
   NAME : PA_IsStreamActive
   SIG  : undefined __fastcall PA_IsStreamActive(int * param_1)
   ======================================================================== */

void PA_IsStreamActive(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800044be. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(*(longlong *)(param_1 + 4) + 0x28))(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180021010
   NAME : PA_IsStreamStopped
   SIG  : undefined __fastcall PA_IsStreamStopped(int * param_1)
   ======================================================================== */

void PA_IsStreamStopped(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800044ee. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180021020
   NAME : PA_OpenDefaultStream
   SIG  : undefined __fastcall PA_OpenDefaultStream(longlong * param_1, int param_2, int param_3, undefined4 param_4, double param_5, int param_6, undefined8 param_7, longlong param_8)
   ======================================================================== */

void PA_OpenDefaultStream
               (longlong *param_1,int param_2,int param_3,undefined4 param_4,double param_5,
               int param_6,undefined8 param_7,longlong param_8)

{
  undefined1 *puVar1;
  
                    /* 0x21020  81  PA_OpenDefaultStream */
  if (param_8 == 0) {
    DAT_18002bbc8 = param_7;
    puVar1 = &LAB_180021120;
  }
  else {
    DAT_18002bbd8 = param_7;
    puVar1 = &LAB_180021130;
  }
  Pa_OpenDefaultStream(param_1,param_2,param_3,param_4,param_5,param_6,(longlong)puVar1,param_8);
  return;
}



/* ========================================================================
   ENTRY: 180021060
   NAME : PA_OpenStream
   SIG  : undefined __fastcall PA_OpenStream(longlong * param_1, int * param_2, int * param_3, double param_4, int param_5, uint param_6, undefined8 param_7, longlong param_8)
   ======================================================================== */

void PA_OpenStream(longlong *param_1,int *param_2,int *param_3,double param_4,int param_5,
                  uint param_6,undefined8 param_7,longlong param_8)

{
  undefined1 *puVar1;
  
                    /* 0x21060  82  PA_OpenStream */
  if (param_8 == 0) {
    DAT_18002bbc8 = param_7;
    puVar1 = &LAB_180021120;
  }
  else {
    DAT_18002bbd8 = param_7;
    puVar1 = &LAB_180021130;
  }
  Pa_OpenStream(param_1,param_2,param_3,param_4,param_5,param_6,(longlong)puVar1,param_8);
  return;
}



/* ========================================================================
   ENTRY: 1800210a0
   NAME : PA_ReadStream
   SIG  : undefined8 __fastcall PA_ReadStream(int * param_1, longlong param_2, int param_3)
   ======================================================================== */

undefined8 PA_ReadStream(int *param_1,longlong param_2,int param_3)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    if (param_3 == 0) {
      return uVar1;
    }
    if (param_2 == 0) {
      return 0xffffd90c;
    }
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180004829. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x40))(param_1,param_2,param_3);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800210b0
   NAME : PA_SetStreamFinishedCallback
   SIG  : undefined __fastcall PA_SetStreamFinishedCallback(int * param_1, undefined8 param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void PA_SetStreamFinishedCallback(int *param_1,undefined8 param_2)

{
  _DAT_18002bbd0 = param_2;
                    /* 0x210b0  84  PA_SetStreamFinishedCallback */
  Pa_SetStreamFinishedCallback(param_1,&DAT_180021140);
  return;
}



/* ========================================================================
   ENTRY: 1800210d0
   NAME : PA_Sleep
   SIG  : void __stdcall PA_Sleep(DWORD dwMilliseconds)
   ======================================================================== */

void __stdcall PA_Sleep(DWORD dwMilliseconds)

{
                    /* WARNING: Could not recover jumptable at 0x0001800203c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  Sleep(dwMilliseconds);
  return;
                    /* 0x210d0  85  PA_Sleep */
}



/* ========================================================================
   ENTRY: 1800210e0
   NAME : PA_StartStream
   SIG  : undefined8 __fastcall PA_StartStream(int * param_1)
   ======================================================================== */

undefined8 PA_StartStream(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
      return 0xffffd902;
    }
    if ((int)uVar1 == 1) {
                    /* WARNING: Could not recover jumptable at 0x0001800048dc. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 8))(param_1);
      return uVar1;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800210f0
   NAME : PA_StopStream
   SIG  : undefined8 __fastcall PA_StopStream(int * param_1)
   ======================================================================== */

undefined8 PA_StopStream(int *param_1)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x00018000491c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x10))(param_1);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180021100
   NAME : PA_Terminate
   SIG  : undefined8 __fastcall PA_Terminate(void)
   ======================================================================== */

undefined8 PA_Terminate(void)

{
  if (DAT_18002bac0 != 0) {
    if (DAT_18002bac0 == 1) {
      FUN_180003a50();
      FUN_180004a90();
    }
    DAT_18002bac0 = DAT_18002bac0 + -1;
    return 0;
  }
  return 0xffffd8f0;
}



/* ========================================================================
   ENTRY: 180021110
   NAME : PA_WriteStream
   SIG  : undefined8 __fastcall PA_WriteStream(int * param_1, longlong param_2, int param_3)
   ======================================================================== */

undefined8 PA_WriteStream(int *param_1,longlong param_2,int param_3)

{
  undefined8 uVar1;
  
  uVar1 = FUN_180003cc0(param_1);
  if ((int)uVar1 == 0) {
    if (param_3 == 0) {
      return uVar1;
    }
    if (param_2 == 0) {
      return 0xffffd90c;
    }
    uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x20))(param_1);
    if ((int)uVar1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800049e9. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      uVar1 = (**(code **)(*(longlong *)(param_1 + 4) + 0x48))(param_1,param_2,param_3);
      return uVar1;
    }
    if ((int)uVar1 == 1) {
      uVar1 = 0xffffd901;
    }
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 180021150
   NAME : FUN_180021150
   SIG  : undefined8 __fastcall FUN_180021150(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180021257) */
/* WARNING: Removing unreachable block (ram,0x000180021245) */
/* WARNING: Removing unreachable block (ram,0x000180021233) */
/* WARNING: Removing unreachable block (ram,0x00018002120c) */
/* WARNING: Removing unreachable block (ram,0x000180021187) */
/* WARNING: Removing unreachable block (ram,0x000180021162) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180021150(void)

{
  int *piVar1;
  uint *puVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  ulonglong uVar6;
  byte bVar7;
  uint uVar8;
  ulonglong uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  ulonglong in_XCR0;
  
  piVar1 = (int *)cpuid_basic_info(0);
  iVar4 = *piVar1;
  puVar2 = (uint *)cpuid_Version_info(1);
  uVar5 = puVar2[3];
  if ((piVar1[2] == 0x49656e69 && piVar1[3] == 0x6c65746e) && piVar1[1] == 0x756e6547) {
    uVar8 = *puVar2 & 0xfff3ff0;
    _DAT_18002b548 = 0x8000;
    _DAT_18002b550 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_18002bbe8 = DAT_18002bbe8 | 1;
    }
  }
  uVar13 = 0;
  uVar8 = 0;
  uVar11 = 0;
  if (iVar4 < 7) {
    uVar12 = 0;
    uVar10 = 0;
  }
  else {
    piVar1 = (int *)cpuid_Extended_Feature_Enumeration_info(7);
    uVar12 = piVar1[1];
    uVar10 = piVar1[2];
    if ((uVar12 >> 9 & 1) != 0) {
      DAT_18002bbe8 = DAT_18002bbe8 | 2;
    }
    if (0 < *piVar1) {
      lVar3 = cpuid_Extended_Feature_Enumeration_info(7);
      uVar13 = *(uint *)(lVar3 + 8);
    }
    if (0x23 < iVar4) {
      lVar3 = cpuid(0x24);
      uVar8 = *(uint *)(lVar3 + 4);
    }
    uVar11 = 0;
    if (0x28 < iVar4) {
      lVar3 = cpuid(0x29);
      uVar11 = *(uint *)(lVar3 + 4);
    }
  }
  DAT_18002b540 = 1;
  DAT_18002b544 = 2;
  uVar6 = DAT_18002b558 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    DAT_18002b540 = 2;
    DAT_18002b544 = 6;
    uVar6 = DAT_18002b558 & 0xffffffffffffffee;
  }
  DAT_18002b558 = uVar6;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar6 = xinuse(0);
    uVar9 = in_XCR0 & uVar6 & 0xffffffff;
    uVar6 = DAT_18002b558;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar7 = (byte)uVar9, (bVar7 & 6) == 6)) {
      DAT_18002b540 = 3;
      uVar5 = DAT_18002b544 | 8;
      if ((uVar12 & 0x20) != 0) {
        DAT_18002b540 = 5;
        uVar5 = DAT_18002b544 | 0x28;
        uVar6 = DAT_18002b558 & 0xfffffffffffffffd;
        if (((uVar12 & 0xd0030000) == 0xd0030000) && ((bVar7 & 0xe0) == 0xe0)) {
          DAT_18002b544 = DAT_18002b544 | 0x68;
          DAT_18002b540 = 6;
          uVar5 = DAT_18002b544;
          uVar6 = DAT_18002b558 & 0xffffffffffffffd9;
        }
      }
      DAT_18002b558 = uVar6;
      DAT_18002b544 = uVar5;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_18002b558 = DAT_18002b558 & 0xfffffffffeffffff;
      }
      uVar6 = DAT_18002b558;
      if (((uVar13 >> 0x13 & 1) != 0) && ((bVar7 & 0xe0) == 0xe0)) {
        _DAT_18002bbec = uVar8 & 0xff;
        uVar6 = DAT_18002b558 & 0xfffffffffeffffd0;
        if (1 < _DAT_18002bbec) {
          uVar6 = DAT_18002b558 & 0xfffffffffeffff90;
        }
      }
    }
    DAT_18002b558 = uVar6;
    if ((((uVar13 >> 0x15 & 1) != 0) && ((uVar11 & 1) != 0)) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_18002b558 = DAT_18002b558 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180021400
   NAME : FUN_180021400
   SIG  : undefined __fastcall FUN_180021400(void)
   ======================================================================== */

void FUN_180021400(void)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(2);
  return;
}



/* ========================================================================
   ENTRY: 180021410
   NAME : FUN_180021410
   SIG  : undefined8 __fastcall FUN_180021410(undefined8 param_1, undefined8 param_2, undefined8 param_3, longlong param_4)
   ======================================================================== */

undefined8 FUN_180021410(undefined8 param_1,undefined8 param_2,undefined8 param_3,longlong param_4)

{
  FUN_180021430(param_2,param_4,*(uint **)(param_4 + 0x38));
  return 1;
}



/* ========================================================================
   ENTRY: 180021430
   NAME : FUN_180021430
   SIG  : ulonglong __fastcall FUN_180021430(undefined8 param_1, longlong param_2, uint * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_180021430(undefined8 param_1,longlong param_2,uint *param_3)

{
  byte bVar1;
  longlong lVar2;
  code *pcVar3;
  int iVar4;
  uint uVar5;
  DWORD DVar6;
  undefined8 uVar7;
  ulonglong uVar8;
  uint uVar9;
  byte bVar10;
  ulonglong uVar11;
  longlong lVar12;
  uint uVar13;
  byte *pbVar14;
  byte *pbVar15;
  undefined8 *puVar16;
  int iVar17;
  uint uVar18;
  DWORD dwMilliseconds;
  
  uVar18 = *param_3;
  pbVar14 = (byte *)((ulonglong)*(uint *)(*(longlong *)(param_2 + 0x10) + 8) +
                    *(longlong *)(param_2 + 8));
  bVar1 = pbVar14[3];
  if (2 < (*pbVar14 & 7)) {
    if ((bVar1 & 0x1f) != 0) {
      uVar11 = 0;
      pbVar14 = pbVar14 + (ulonglong)(bVar1 & 0x1f) + 4;
      uVar8 = uVar11;
      pbVar15 = pbVar14;
      if (bVar1 >> 5 != 0) {
        do {
          pbVar14 = pbVar15 + 3;
          if (*pbVar15 >> 3 != 0) {
            pbVar14 = pbVar15 + (ulonglong)(uint)(*pbVar15 >> 3) + 6;
          }
          uVar9 = (int)uVar8 + 1;
          uVar8 = (ulonglong)uVar9;
          pbVar15 = pbVar14;
        } while (uVar9 < bVar1 >> 5);
      }
      if ((bVar1 & 0x1f) != 0) {
        do {
          bVar10 = *pbVar14;
          uVar8 = (ulonglong)bVar10;
          if (((bVar10 < 4) || (uVar8 = (ulonglong)(bVar10 & 0xf), (byte)((bVar10 & 0xf) - 8) < 3))
             || (uVar8 = (ulonglong)(bVar10 & 7), (byte)((bVar10 & 7) - 4) < 4)) {
            if ((char)uVar8 == '\0') break;
            if (3 < bVar10) {
              uVar8 = (ulonglong)(bVar10 & 0xf);
              if ((2 < (byte)((bVar10 & 0xf) - 8)) &&
                 (uVar8 = (ulonglong)(bVar10 & 7), 3 < (byte)((bVar10 & 7) - 4)))
              goto LAB_180021538;
              bVar10 = (byte)uVar8;
              if (0x20 < bVar10) goto switchD_18002155c_caseD_4;
            }
          }
          else {
LAB_180021538:
            if ((bVar10 & 0x3f) != 0x20) goto switchD_18002155c_caseD_4;
            bVar10 = 0x20;
          }
                    /* WARNING (jumptable): Sanity check requires truncation of jumptable */
                    /* WARNING: Could not find normalized switch variable to match jumptable */
          switch(*(undefined1 *)((longlong)&UINT_1800215d4 + (ulonglong)bVar10)) {
          case 0:
            lVar12 = 2;
            break;
          case 1:
            lVar12 = 5;
            break;
          case 2:
            lVar12 = 3;
            break;
          case 3:
            lVar12 = 1;
            break;
          case 4:
switchD_18002155c_caseD_4:
            FUN_180021400();
            pcVar3 = (code *)swi(3);
            uVar8 = (*pcVar3)();
            return uVar8;
          case 5:
            goto switchD_18002155c_caseD_5;
          }
          pbVar14 = pbVar14 + lVar12;
          uVar9 = (int)uVar11 + 1;
          uVar11 = (ulonglong)uVar9;
        } while (uVar9 < (bVar1 & 0x1f));
      }
    }
  }
  return (longlong)(int)uVar18 & 0xfffffffffffffff8;
switchD_18002155c_caseD_5:
  do {
    *(undefined8 *)(pbVar14 + uVar8 * 8) = *(undefined8 *)(uVar8 * 8);
    uVar9 = (int)uVar8 + 1;
    uVar8 = (ulonglong)uVar9;
    pbVar14 = (byte *)IMAGE_DOS_HEADER_180000000._16_8_;
  } while (uVar9 < *(uint *)(uVar11 + 0xa8));
  uVar9 = uVar18 * 3;
  dwMilliseconds = uVar18 >> 1;
  uVar18 = 0;
  uVar5 = 0;
  IMAGE_DOS_HEADER_180000000.e_magic[0] = '\0';
  IMAGE_DOS_HEADER_180000000.e_magic[1] = '\0';
  IMAGE_DOS_HEADER_180000000.e_cblp = 0;
  uVar13 = 0;
  IMAGE_DOS_HEADER_180000000._8_4_ = dwMilliseconds;
  IMAGE_DOS_HEADER_180000000._12_4_ = uVar9;
  do {
    puVar16 = (undefined8 *)(uVar11 + 0x1a8);
    iVar4 = FUN_18001c6b0(uVar11);
    if (iVar4 == 0) {
      DVar6 = WaitForSingleObject((HANDLE)*puVar16,dwMilliseconds);
      if (DVar6 == 0xffffffff) {
        uVar5 = 0xffffd8f1;
        break;
      }
      if ((DVar6 == 0x102) && (uVar13 = uVar13 + dwMilliseconds, uVar9 <= uVar13)) {
        uVar5 = 0xffffd8fd;
        break;
      }
    }
    else {
      uVar7 = FUN_18001d520((longlong)puVar16);
      iVar4 = *(int *)(uVar11 + 0x1cc);
      if ((int)uVar7 != 0) {
        uVar5 = 0xffffd904;
      }
      FUN_1800069a0(uVar11 + 0x68,*(int *)(uVar11 + 0x1d0) - *(int *)(uVar11 + 0x1d4));
      iVar17 = 0;
      uVar8 = 0;
      if (*(int *)(uVar11 + 0x1b8) != 0) {
        lVar12 = (longlong)iVar4 * 0x30;
        do {
          lVar2 = *(longlong *)(*(longlong *)(uVar11 + 0x1c0) + uVar8 * 8);
          uVar18 = *(uint *)(lVar12 + 0x10 + lVar2);
          FUN_1800068d0(uVar11 + 0x68,iVar17,
                        (ulonglong)(*(int *)(uVar11 + 0x1d4) * *(int *)(uVar11 + 0xac) * uVar18) +
                        *(longlong *)(lVar12 + lVar2),uVar18);
          iVar17 = iVar17 + uVar18;
          uVar9 = (int)uVar8 + 1;
          uVar8 = (ulonglong)uVar9;
          uVar18 = IMAGE_DOS_HEADER_180000000._0_4_;
        } while (uVar9 < *(uint *)(uVar11 + 0x1b8));
      }
      uVar9 = FUN_180005eb0(uVar11 + 0x68,(longlong *)&IMAGE_DOS_HEADER_180000000.e_sp,
                            IMAGE_DOS_HEADER_180000000._4_4_ - uVar18);
      *(int *)(uVar11 + 0x1d4) = *(int *)(uVar11 + 0x1d4) + uVar9;
      if (*(int *)(uVar11 + 0x1d4) == *(int *)(uVar11 + 0x1d0)) {
        uVar8 = FUN_18001bd60(uVar11);
        uVar5 = (uint)uVar8;
        if (uVar5 != 0) break;
      }
      uVar13 = 0;
      uVar18 = uVar18 + uVar9;
      uVar9 = IMAGE_DOS_HEADER_180000000._12_4_;
      dwMilliseconds = IMAGE_DOS_HEADER_180000000._8_4_;
      IMAGE_DOS_HEADER_180000000._0_4_ = uVar18;
    }
  } while (uVar18 < (uint)IMAGE_DOS_HEADER_180000000._4_4_);
  return (ulonglong)uVar5;
}



/* ========================================================================
   ENTRY: 180021610
   NAME : __security_check_cookie
   SIG  : void __cdecl __security_check_cookie(uintptr_t _StackCookie)
   ======================================================================== */

/* WARNING: This is an inlined function */

void __cdecl __security_check_cookie(uintptr_t _StackCookie)

{
  if ((_StackCookie == DAT_18002b580) && ((short)(_StackCookie >> 0x30) == 0)) {
    return;
  }
  FUN_180021400();
  return;
}



/* ========================================================================
   ENTRY: 180021630
   NAME : FUN_180021630
   SIG  : undefined __fastcall FUN_180021630(size_t param_1)
   ======================================================================== */

void FUN_180021630(size_t param_1)

{
  int iVar1;
  void *pvVar2;
  
  pvVar2 = malloc(param_1);
  while( true ) {
    if (pvVar2 != (void *)0x0) {
      return;
    }
    iVar1 = _callnewh(param_1);
    if (iVar1 == 0) break;
    pvVar2 = malloc(param_1);
  }
  if (param_1 == 0xffffffffffffffff) {
                    /* WARNING: Subroutine does not return */
    FUN_180021ce0();
  }
                    /* WARNING: Subroutine does not return */
  FUN_180021cc0();
}



/* ========================================================================
   ENTRY: 180021680
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222c6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180021690
   NAME : thunk_FUN_180021630
   SIG  : undefined __fastcall thunk_FUN_180021630(size_t param_1)
   ======================================================================== */

void thunk_FUN_180021630(size_t param_1)

{
  int iVar1;
  void *pvVar2;
  
  pvVar2 = malloc(param_1);
  while( true ) {
    if (pvVar2 != (void *)0x0) {
      return;
    }
    iVar1 = _callnewh(param_1);
    if (iVar1 == 0) break;
    pvVar2 = malloc(param_1);
  }
  if (param_1 == 0xffffffffffffffff) {
                    /* WARNING: Subroutine does not return */
    FUN_180021ce0();
  }
                    /* WARNING: Subroutine does not return */
  FUN_180021cc0();
}



/* ========================================================================
   ENTRY: 1800216a0
   NAME : FUN_1800216a0
   SIG  : undefined8 __fastcall FUN_1800216a0(longlong param_1, undefined8 param_2, undefined8 param_3, longlong param_4)
   ======================================================================== */

undefined8 FUN_1800216a0(longlong param_1,undefined8 param_2,undefined8 param_3,longlong param_4)

{
  longlong lVar1;
  undefined8 uVar2;
  
  lVar1 = *(longlong *)(param_4 + 0x38);
  FUN_180021430(param_2,param_4,(uint *)(lVar1 + 4));
  if ((*(uint *)(lVar1 + 4) & ((*(byte *)(param_1 + 4) & 0x66) != 0) + 1) != 0) {
    uVar2 = __CxxFrameHandler4(param_1,param_2,param_3,param_4);
    return uVar2;
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180021720
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
   ENTRY: 180021770
   NAME : FUN_180021770
   SIG  : undefined8 * __fastcall FUN_180021770(undefined8 * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 * FUN_180021770(undefined8 *param_1,ulonglong param_2)

{
  *param_1 = type_info::vftable;
  if ((param_2 & 1) != 0) {
    free(param_1);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 1800217a0
   NAME : FUN_1800217a0
   SIG  : ulonglong __fastcall FUN_1800217a0(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_1800217a0(undefined8 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  ulonglong uVar2;
  
  if (param_2 == 0) {
    uVar2 = FUN_180021910(param_3 != 0);
    return uVar2;
  }
  if (param_2 == 1) {
    uVar2 = FUN_180021800(param_1,param_3);
    return uVar2;
  }
  if (param_2 == 2) {
    bVar1 = FUN_180021f50();
    return (ulonglong)bVar1;
  }
  if (param_2 != 3) {
    return 1;
  }
  bVar1 = FUN_180021f80();
  return (ulonglong)bVar1;
}



/* ========================================================================
   ENTRY: 180021800
   NAME : FUN_180021800
   SIG  : undefined8 __fastcall FUN_180021800(undefined8 param_1, undefined8 param_2)
   ======================================================================== */

undefined8 FUN_180021800(undefined8 param_1,undefined8 param_2)

{
  code *pcVar1;
  bool bVar2;
  undefined4 uVar3;
  int iVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  longlong *plVar7;
  undefined *puVar8;
  
  uVar5 = FUN_180022050(0);
  if ((char)uVar5 != '\0') {
    uVar5 = FUN_180021e40();
    bVar2 = true;
    if (DAT_18002bc18 != 0) {
      FUN_1800221a0(7);
      pcVar1 = (code *)swi(3);
      uVar5 = (*pcVar1)();
      return uVar5;
    }
    DAT_18002bc18 = 1;
    uVar3 = FUN_180021ed0();
    if ((char)uVar3 != '\0') {
      FUN_1800221b0();
      FUN_180021df0();
      FUN_180021e20();
      iVar4 = _initterm_e(&DAT_1800234f8,&DAT_180023500);
      if (iVar4 == 0) {
        uVar6 = FUN_180021e90();
        if ((char)uVar6 != '\0') {
          _initterm(&DAT_1800234e8,&DAT_1800234f0);
          DAT_18002bc18 = 2;
          bVar2 = false;
        }
      }
    }
    FUN_180022130((char)uVar5);
    if (!bVar2) {
      plVar7 = (longlong *)FUN_180022190();
      if (*plVar7 != 0) {
        puVar8 = FUN_180022090((longlong)plVar7);
        if ((char)puVar8 != '\0') {
          (*(code *)PTR__guard_dispatch_icall_1800234c0)(param_1,2,param_2);
        }
      }
      DAT_18002bbf0 = DAT_18002bbf0 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180021910
   NAME : FUN_180021910
   SIG  : ulonglong __fastcall FUN_180021910(byte param_1)
   ======================================================================== */

ulonglong FUN_180021910(byte param_1)

{
  code *pcVar1;
  byte bVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  
  if (DAT_18002bbf0 < 1) {
    return 0;
  }
  DAT_18002bbf0 = DAT_18002bbf0 + -1;
  uVar3 = FUN_180021e40();
  if (DAT_18002bc18 == 2) {
    FUN_180021ff0();
    FUN_180021e00();
    FUN_180022200();
    DAT_18002bc18 = 0;
    FUN_180022130((char)uVar3);
    bVar2 = FUN_180022160((ulonglong)param_1,'\0');
    FUN_180022030();
    return (ulonglong)bVar2;
  }
  FUN_1800221a0(7);
  pcVar1 = (code *)swi(3);
  uVar4 = (*pcVar1)();
  return uVar4;
}



/* ========================================================================
   ENTRY: 1800219b0
   NAME : FUN_1800219b0
   SIG  : ulonglong __fastcall FUN_1800219b0(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_1800219b0(undefined8 param_1,int param_2,longlong param_3)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  if ((param_2 == 0) && (DAT_18002bbf0 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (PTR_180025db0 == (undefined *)0x0) {
      uVar2 = 1;
    }
    else {
      uVar1 = (*(code *)PTR__guard_dispatch_icall_1800234c0)();
      uVar2 = (ulonglong)uVar1;
    }
    if ((int)uVar2 == 0) {
      return uVar2;
    }
    uVar2 = FUN_1800217a0(param_1,param_2,param_3);
    if ((int)uVar2 == 0) {
      return uVar2 & 0xffffffff;
    }
  }
  uVar2 = FUN_180020e80();
  uVar3 = uVar2 & 0xffffffff;
  if ((param_2 == 1) && ((int)uVar2 == 0)) {
    FUN_180020e80();
    FUN_180021910(param_3 != 0);
    if (PTR_180025db0 != (undefined *)0x0) {
      (*(code *)PTR__guard_dispatch_icall_1800234c0)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    uVar2 = FUN_1800217a0(param_1,param_2,param_3);
    uVar3 = uVar2 & 0xffffffff;
    if ((int)uVar2 != 0) {
      if (PTR_180025db0 == (undefined *)0x0) {
        uVar3 = 1;
      }
      else {
        uVar1 = (*(code *)PTR__guard_dispatch_icall_1800234c0)(param_1,param_2,param_3);
        uVar3 = (ulonglong)uVar1;
      }
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180021af0
   NAME : entry
   SIG  : undefined __fastcall entry(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

void entry(undefined8 param_1,int param_2,longlong param_3)

{
  if (param_2 == 1) {
    FUN_180021d30();
  }
  FUN_1800219b0(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180021b30
   NAME : FUN_180021b30
   SIG  : undefined8 * __fastcall FUN_180021b30(undefined8 * param_1, longlong param_2)
   ======================================================================== */

undefined8 * FUN_180021b30(undefined8 *param_1,longlong param_2)

{
  *param_1 = std::exception::vftable;
  param_1[1] = 0;
  param_1[2] = 0;
  __std_exception_copy(param_2 + 8);
  *param_1 = std::bad_alloc::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180021b70
   NAME : FUN_180021b70
   SIG  : undefined8 * __fastcall FUN_180021b70(undefined8 * param_1)
   ======================================================================== */

undefined8 * FUN_180021b70(undefined8 *param_1)

{
  param_1[2] = 0;
  param_1[1] = "bad allocation";
  *param_1 = std::bad_alloc::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180021ba0
   NAME : FUN_180021ba0
   SIG  : undefined8 * __fastcall FUN_180021ba0(undefined8 * param_1, longlong param_2)
   ======================================================================== */

undefined8 * FUN_180021ba0(undefined8 *param_1,longlong param_2)

{
  *param_1 = std::exception::vftable;
  param_1[1] = 0;
  param_1[2] = 0;
  __std_exception_copy(param_2 + 8);
  *param_1 = std::bad_array_new_length::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180021be0
   NAME : FUN_180021be0
   SIG  : undefined8 * __fastcall FUN_180021be0(undefined8 * param_1)
   ======================================================================== */

undefined8 * FUN_180021be0(undefined8 *param_1)

{
  param_1[2] = 0;
  param_1[1] = "bad array new length";
  *param_1 = std::bad_array_new_length::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180021c10
   NAME : exception
   SIG  : undefined __thiscall exception(exception * this, exception * param_1)
   ======================================================================== */

/* Library Function - Single Match
    public: __cdecl std::exception::exception(class std::exception const & __ptr64) __ptr64
   
   Library: Visual Studio 2019 Release */

exception * __thiscall std::exception::exception(exception *this,exception *param_1)

{
  *(undefined ***)this = vftable;
  *(undefined8 *)(this + 8) = 0;
  *(undefined8 *)(this + 0x10) = 0;
  __std_exception_copy(param_1 + 8);
  return this;
}



/* ========================================================================
   ENTRY: 180021c70
   NAME : FUN_180021c70
   SIG  : undefined8 * __fastcall FUN_180021c70(undefined8 * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 * FUN_180021c70(undefined8 *param_1,ulonglong param_2)

{
  *param_1 = std::exception::vftable;
  __std_exception_destroy(param_1 + 1);
  if ((param_2 & 1) != 0) {
    free(param_1);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 180021cc0
   NAME : FUN_180021cc0
   SIG  : noreturn undefined __fastcall FUN_180021cc0(void)
   ======================================================================== */

void FUN_180021cc0(void)

{
  undefined8 local_28 [5];
  
  FUN_180021b70(local_28);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_28,(ThrowInfo *)&DAT_180028900);
}



/* ========================================================================
   ENTRY: 180021ce0
   NAME : FUN_180021ce0
   SIG  : noreturn undefined __fastcall FUN_180021ce0(void)
   ======================================================================== */

void FUN_180021ce0(void)

{
  undefined8 local_28 [5];
  
  FUN_180021be0(local_28);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_28,(ThrowInfo *)&DAT_180028988);
}



/* ========================================================================
   ENTRY: 180021d00
   NAME : FUN_180021d00
   SIG  : char * __fastcall FUN_180021d00(longlong param_1)
   ======================================================================== */

char * FUN_180021d00(longlong param_1)

{
  char *pcVar1;
  
  pcVar1 = "Unknown exception";
  if (*(char **)(param_1 + 8) != (char *)0x0) {
    pcVar1 = *(char **)(param_1 + 8);
  }
  return pcVar1;
}



/* ========================================================================
   ENTRY: 180021d20
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222c6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180021d30
   NAME : FUN_180021d30
   SIG  : undefined __fastcall FUN_180021d30(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180021d30(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10 [3];
  _FILETIME local_18 [2];
  
  if (DAT_18002b580 != 0x2b992ddfa232) {
    _DAT_18002b5c0 = ~DAT_18002b580;
    return;
  }
  local_res8.dwLowDateTime = 0;
  local_res8.dwHighDateTime = 0;
  GetSystemTimeAsFileTime(&local_res8);
  local_18[0] = local_res8;
  DVar1 = GetCurrentThreadId();
  local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
  DVar1 = GetCurrentProcessId();
  local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
  QueryPerformanceCounter(local_res10);
  DAT_18002b580 =
       (local_res10[0].QuadPart << 0x20 ^ local_res10[0].QuadPart ^ (ulonglong)local_18[0] ^
       (ulonglong)local_18) & 0xffffffffffff;
  if (DAT_18002b580 == 0x2b992ddfa232) {
    DAT_18002b580 = 0x2b992ddfa233;
  }
  _DAT_18002b5c0 = ~DAT_18002b580;
  return;
}



/* ========================================================================
   ENTRY: 180021df0
   NAME : FUN_180021df0
   SIG  : undefined __fastcall FUN_180021df0(void)
   ======================================================================== */

void FUN_180021df0(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180021df7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_18002bc00);
  return;
}



/* ========================================================================
   ENTRY: 180021e00
   NAME : FUN_180021e00
   SIG  : undefined __fastcall FUN_180021e00(void)
   ======================================================================== */

void FUN_180021e00(void)

{
  __std_type_info_destroy_list(&DAT_18002bc00);
  return;
}



/* ========================================================================
   ENTRY: 180021e10
   NAME : FUN_180021e10
   SIG  : undefined * __fastcall FUN_180021e10(void)
   ======================================================================== */

undefined * FUN_180021e10(void)

{
  return &DAT_18002bc10;
}



/* ========================================================================
   ENTRY: 180021e20
   NAME : FUN_180021e20
   SIG  : undefined __fastcall FUN_180021e20(void)
   ======================================================================== */

void FUN_180021e20(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_180003990();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_180021e10();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 180021e40
   NAME : FUN_180021e40
   SIG  : undefined8 __fastcall FUN_180021e40(void)
   ======================================================================== */

ulonglong FUN_180021e40(void)

{
  ulonglong uVar1;
  ulonglong uVar2;
  bool bVar3;
  undefined7 extraout_var;
  ulonglong uVar4;
  
  bVar3 = FUN_180022250();
  uVar4 = CONCAT71(extraout_var,bVar3);
  if ((int)uVar4 != 0) {
    uVar1 = *(ulonglong *)((longlong)Self + 8);
    uVar4 = 0;
    LOCK();
    bVar3 = DAT_18002bc20 == 0;
    uVar2 = uVar1;
    if (!bVar3) {
      uVar4 = DAT_18002bc20;
      uVar2 = DAT_18002bc20;
    }
    DAT_18002bc20 = uVar2;
    UNLOCK();
    uVar2 = DAT_18002bc20;
    while (DAT_18002bc20 = uVar2, !bVar3) {
      if (uVar1 == uVar4) {
        return CONCAT71((int7)(uVar4 >> 8),1);
      }
      uVar4 = 0;
      LOCK();
      bVar3 = uVar2 == 0;
      DAT_18002bc20 = uVar1;
      if (!bVar3) {
        uVar4 = uVar2;
        DAT_18002bc20 = uVar2;
      }
      UNLOCK();
      uVar2 = DAT_18002bc20;
    }
  }
  return uVar4 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 180021e90
   NAME : FUN_180021e90
   SIG  : undefined8 __fastcall FUN_180021e90(void)
   ======================================================================== */

undefined8 FUN_180021e90(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  ulonglong uVar3;
  
  bVar1 = FUN_180022250();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    uVar2 = FUN_180021150();
    return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
  }
  uVar3 = FUN_180020e80();
  uVar3 = _configure_narrow_argv(uVar3 & 0xffffffff);
  if ((int)uVar3 != 0) {
    return uVar3 & 0xffffffffffffff00;
  }
  uVar2 = _initialize_narrow_environment();
  return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
}



/* ========================================================================
   ENTRY: 180021ed0
   NAME : FUN_180021ed0
   SIG  : undefined4 __fastcall FUN_180021ed0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

uint FUN_180021ed0(void)

{
  bool bVar1;
  undefined4 in_EAX;
  undefined3 extraout_var;
  uint uVar2;
  
  if (DAT_18002bc29 == '\0') {
    bVar1 = FUN_180022250();
    if (CONCAT31(extraout_var,bVar1) == 0) {
      _DAT_18002bc30 = _DAT_180025e40;
      uRam000000018002bc38 = _UNK_180025e48;
      _DAT_18002bc40 = 0xffffffffffffffff;
      _DAT_18002bc48 = _DAT_180025e40;
      uRam000000018002bc50 = _UNK_180025e48;
      _DAT_18002bc58 = 0xffffffffffffffff;
    }
    else {
      uVar2 = _initialize_onexit_table(&DAT_18002bc30);
      if (uVar2 != 0) {
LAB_180021f06:
        return uVar2 & 0xffffff00;
      }
      uVar2 = _initialize_onexit_table(&DAT_18002bc48);
      if (uVar2 != 0) goto LAB_180021f06;
    }
    in_EAX = 0;
    DAT_18002bc29 = '\x01';
  }
  return CONCAT31((int3)((uint)in_EAX >> 8),1);
}



/* ========================================================================
   ENTRY: 180021f50
   NAME : FUN_180021f50
   SIG  : undefined1 __fastcall FUN_180021f50(void)
   ======================================================================== */

undefined1 FUN_180021f50(void)

{
  char cVar1;
  
  cVar1 = FUN_180022320();
  if (cVar1 != '\0') {
    cVar1 = FUN_180022320();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_180022320();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180021f80
   NAME : FUN_180021f80
   SIG  : undefined1 __fastcall FUN_180021f80(void)
   ======================================================================== */

undefined1 FUN_180021f80(void)

{
  FUN_180022320();
  FUN_180022320();
  return 1;
}



/* ========================================================================
   ENTRY: 180021fa0
   NAME : FUN_180021fa0
   SIG  : undefined __fastcall FUN_180021fa0(undefined8 param_1, int param_2, undefined8 param_3, undefined * param_4, undefined4 param_5, undefined8 param_6)
   ======================================================================== */

void FUN_180021fa0(undefined8 param_1,int param_2,undefined8 param_3,undefined *param_4,
                  undefined4 param_5,undefined8 param_6)

{
  bool bVar1;
  undefined7 extraout_var;
  
  bVar1 = FUN_180022250();
  if (((int)CONCAT71(extraout_var,bVar1) == 0) && (param_2 == 1)) {
    (*(code *)PTR__guard_dispatch_icall_1800234c0)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 180021ff0
   NAME : FUN_180021ff0
   SIG  : undefined __fastcall FUN_180021ff0(void)
   ======================================================================== */

void FUN_180021ff0(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  
  bVar1 = FUN_180022250();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    _execute_onexit_table(&DAT_18002bc30);
    return;
  }
  uVar2 = FUN_18000bf60();
  if ((int)uVar2 == 0) {
    _cexit();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180022030
   NAME : FUN_180022030
   SIG  : undefined __fastcall FUN_180022030(void)
   ======================================================================== */

void FUN_180022030(void)

{
  FUN_180022320();
  FUN_180022320();
  return;
}



/* ========================================================================
   ENTRY: 180022050
   NAME : FUN_180022050
   SIG  : undefined8 __fastcall FUN_180022050(int param_1)
   ======================================================================== */

longlong FUN_180022050(int param_1)

{
  char cVar1;
  uint7 extraout_var;
  undefined7 extraout_var_00;
  uint7 extraout_var_01;
  uint7 uVar2;
  
  if (param_1 == 0) {
    DAT_18002bc28 = 1;
  }
  FUN_180021150();
  cVar1 = FUN_180022320();
  uVar2 = extraout_var;
  if (cVar1 != '\0') {
    cVar1 = FUN_180022320();
    if (cVar1 != '\0') {
      return CONCAT71(extraout_var_00,1);
    }
    FUN_180022320();
    uVar2 = extraout_var_01;
  }
  return (ulonglong)uVar2 << 8;
}



/* ========================================================================
   ENTRY: 180022090
   NAME : FUN_180022090
   SIG  : undefined * __fastcall FUN_180022090(longlong param_1)
   ======================================================================== */

undefined * FUN_180022090(longlong param_1)

{
  ulonglong uVar1;
  ulonglong uVar2;
  uint7 uVar3;
  longlong lVar4;
  
  uVar1 = 0x5a4d;
  if (IMAGE_DOS_HEADER_180000000.e_magic == (char  [2])0x5a4d) {
    lVar4 = (longlong)(int)IMAGE_DOS_HEADER_180000000.e_lfanew;
    if ((*(int *)(lVar4 + 0x180000000) == 0x4550) &&
       (uVar1 = 0x20b,
       *(short *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar4 + -4) == 0x20b)) {
      uVar2 = (ulonglong)*(ushort *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar4 + -8) +
              0x18 + lVar4 + 0x180000000;
      uVar1 = uVar2 + (ulonglong)*(ushort *)(IMAGE_DOS_HEADER_180000000.e_magic + lVar4 + 6) * 0x28;
      while( true ) {
        uVar3 = (uint7)(uVar2 >> 8);
        if (uVar2 == uVar1) {
          return (undefined *)((ulonglong)uVar3 << 8);
        }
        if (((ulonglong)*(uint *)(uVar2 + 0xc) <= param_1 - 0x180000000U) &&
           (param_1 - 0x180000000U < (ulonglong)(*(int *)(uVar2 + 8) + *(uint *)(uVar2 + 0xc))))
        break;
        uVar2 = uVar2 + 0x28;
      }
      if (*(int *)(uVar2 + 0x24) < 0) {
        return (undefined *)(uVar2 & 0xffffffffffffff00);
      }
      return (undefined *)CONCAT71(uVar3,1);
    }
  }
  return (undefined *)(uVar1 & 0xffffffffffffff00);
}



/* ========================================================================
   ENTRY: 180022130
   NAME : FUN_180022130
   SIG  : undefined8 __fastcall FUN_180022130(char param_1)
   ======================================================================== */

undefined8 FUN_180022130(char param_1)

{
  undefined8 uVar1;
  bool bVar2;
  undefined7 extraout_var;
  undefined8 uVar3;
  
  bVar2 = FUN_180022250();
  uVar1 = DAT_18002bc20;
  uVar3 = CONCAT71(extraout_var,bVar2);
  if (((int)uVar3 != 0) && (param_1 == '\0')) {
    LOCK();
    DAT_18002bc20 = 0;
    UNLOCK();
    uVar3 = uVar1;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180022160
   NAME : FUN_180022160
   SIG  : undefined1 __fastcall FUN_180022160(undefined8 param_1, char param_2)
   ======================================================================== */

undefined1 FUN_180022160(undefined8 param_1,char param_2)

{
  if ((DAT_18002bc28 == '\0') || (param_2 == '\0')) {
    FUN_180022320();
    FUN_180022320();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180022190
   NAME : FUN_180022190
   SIG  : undefined * __fastcall FUN_180022190(void)
   ======================================================================== */

undefined * FUN_180022190(void)

{
  return &DAT_18002bc70;
}



/* ========================================================================
   ENTRY: 1800221a0
   NAME : FUN_1800221a0
   SIG  : undefined __fastcall FUN_1800221a0(undefined4 param_1)
   ======================================================================== */

void FUN_1800221a0(undefined4 param_1)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800221b0
   NAME : FUN_1800221b0
   SIG  : undefined __fastcall FUN_1800221b0(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x0001800221d0) */
/* WARNING: Removing unreachable block (ram,0x0001800221d8) */
/* WARNING: Removing unreachable block (ram,0x0001800221de) */

void FUN_1800221b0(void)

{
  return;
}



/* ========================================================================
   ENTRY: 180022200
   NAME : FUN_180022200
   SIG  : undefined __fastcall FUN_180022200(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180022220) */
/* WARNING: Removing unreachable block (ram,0x000180022228) */
/* WARNING: Removing unreachable block (ram,0x00018002222e) */

void FUN_180022200(void)

{
  return;
}



/* ========================================================================
   ENTRY: 180022250
   NAME : FUN_180022250
   SIG  : bool __fastcall FUN_180022250(void)
   ======================================================================== */

bool FUN_180022250(void)

{
  return DAT_18002b5e0 != 0;
}



/* ========================================================================
   ENTRY: 180022260
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180022260. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180022266
   NAME : memcpy
   SIG  : void * __cdecl memcpy(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memcpy(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180022266. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memcpy(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18002226c
   NAME : __CxxFrameHandler4
   SIG  : undefined __CxxFrameHandler4(void)
   ======================================================================== */

void __CxxFrameHandler4(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018002226c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __CxxFrameHandler4();
  return;
}



/* ========================================================================
   ENTRY: 180022272
   NAME : strstr
   SIG  : char * __cdecl strstr(char * _Str, char * _SubStr)
   ======================================================================== */

char * __cdecl strstr(char *_Str,char *_SubStr)

{
  char *pcVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180022272. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pcVar1 = strstr(_Str,_SubStr);
  return pcVar1;
}



/* ========================================================================
   ENTRY: 180022278
   NAME : memmove
   SIG  : void * __cdecl memmove(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memmove(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180022278. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memmove(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180022284
   NAME : __std_exception_copy
   SIG  : undefined __std_exception_copy(void)
   ======================================================================== */

void __std_exception_copy(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180022284. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_exception_copy();
  return;
}



/* ========================================================================
   ENTRY: 18002228a
   NAME : __std_exception_destroy
   SIG  : undefined __std_exception_destroy(void)
   ======================================================================== */

void __std_exception_destroy(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018002228a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_exception_destroy();
  return;
}



/* ========================================================================
   ENTRY: 180022290
   NAME : _CxxThrowException
   SIG  : noreturn void __stdcall _CxxThrowException(void * pExceptionObject, ThrowInfo * pThrowInfo)
   ======================================================================== */

void __stdcall _CxxThrowException(void *pExceptionObject,ThrowInfo *pThrowInfo)

{
                    /* WARNING: Could not recover jumptable at 0x000180022290. Too many branches */
                    /* WARNING: Subroutine does not return */
                    /* WARNING: Treating indirect jump as call */
  _CxxThrowException(pExceptionObject,pThrowInfo);
  return;
}



/* ========================================================================
   ENTRY: 180022296
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180022296. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 18002229c
   NAME : strncpy
   SIG  : char * __cdecl strncpy(char * _Dest, char * _Source, size_t _Count)
   ======================================================================== */

char * __cdecl strncpy(char *_Dest,char *_Source,size_t _Count)

{
  char *pcVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018002229c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pcVar1 = strncpy(_Dest,_Source,_Count);
  return pcVar1;
}



/* ========================================================================
   ENTRY: 1800222a2
   NAME : strcmp
   SIG  : int __cdecl strcmp(char * _Str1, char * _Str2)
   ======================================================================== */

int __cdecl strcmp(char *_Str1,char *_Str2)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222a2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = strcmp(_Str1,_Str2);
  return iVar1;
}



/* ========================================================================
   ENTRY: 1800222a8
   NAME : strcpy
   SIG  : char * __cdecl strcpy(char * _Dest, char * _Source)
   ======================================================================== */

char * __cdecl strcpy(char *_Dest,char *_Source)

{
  char *pcVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222a8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pcVar1 = strcpy(_Dest,_Source);
  return pcVar1;
}



/* ========================================================================
   ENTRY: 1800222ae
   NAME : strlen
   SIG  : size_t __cdecl strlen(char * _Str)
   ======================================================================== */

size_t __cdecl strlen(char *_Str)

{
  size_t sVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222ae. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  sVar1 = strlen(_Str);
  return sVar1;
}



/* ========================================================================
   ENTRY: 1800222b4
   NAME : strncmp
   SIG  : int __cdecl strncmp(char * _Str1, char * _Str2, size_t _MaxCount)
   ======================================================================== */

int __cdecl strncmp(char *_Str1,char *_Str2,size_t _MaxCount)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222b4. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = strncmp(_Str1,_Str2,_MaxCount);
  return iVar1;
}



/* ========================================================================
   ENTRY: 1800222ba
   NAME : wcscpy
   SIG  : wchar_t * __cdecl wcscpy(wchar_t * _Dest, wchar_t * _Source)
   ======================================================================== */

wchar_t * __cdecl wcscpy(wchar_t *_Dest,wchar_t *_Source)

{
  wchar_t *pwVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222ba. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pwVar1 = wcscpy(_Dest,_Source);
  return pwVar1;
}



/* ========================================================================
   ENTRY: 1800222c0
   NAME : wcslen
   SIG  : size_t __cdecl wcslen(wchar_t * _Str)
   ======================================================================== */

size_t __cdecl wcslen(wchar_t *_Str)

{
  size_t sVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  sVar1 = wcslen(_Str);
  return sVar1;
}



/* ========================================================================
   ENTRY: 1800222c6
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222c6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 1800222cc
   NAME : wcsncmp
   SIG  : int __cdecl wcsncmp(wchar_t * _Str1, wchar_t * _Str2, size_t _MaxCount)
   ======================================================================== */

int __cdecl wcsncmp(wchar_t *_Str1,wchar_t *_Str2,size_t _MaxCount)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222cc. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = wcsncmp(_Str1,_Str2,_MaxCount);
  return iVar1;
}



/* ========================================================================
   ENTRY: 1800222d2
   NAME : wcsncpy
   SIG  : wchar_t * __cdecl wcsncpy(wchar_t * _Dest, wchar_t * _Source, size_t _Count)
   ======================================================================== */

wchar_t * __cdecl wcsncpy(wchar_t *_Dest,wchar_t *_Source,size_t _Count)

{
  wchar_t *pwVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222d2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pwVar1 = wcsncpy(_Dest,_Source,_Count);
  return pwVar1;
}



/* ========================================================================
   ENTRY: 1800222d8
   NAME : _callnewh
   SIG  : int __cdecl _callnewh(size_t _Size)
   ======================================================================== */

int __cdecl _callnewh(size_t _Size)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222d8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = _callnewh(_Size);
  return iVar1;
}



/* ========================================================================
   ENTRY: 1800222de
   NAME : malloc
   SIG  : void * __cdecl malloc(size_t _Size)
   ======================================================================== */

void * __cdecl malloc(size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800222de. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = malloc(_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 1800222e4
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222e4. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 1800222ea
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222ea. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 1800222f0
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 1800222f6
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222f6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 1800222fc
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800222fc. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 180022302
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180022302. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180022308
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180022308. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 18002230e
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x00018002230e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 180022320
   NAME : FUN_180022320
   SIG  : undefined1 __fastcall FUN_180022320(void)
   ======================================================================== */

undefined1 FUN_180022320(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180022340
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x000180022340. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 180022360
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x000180022360 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x000180022340. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 180022370
   NAME : FUN_180022370
   SIG  : undefined __fastcall FUN_180022370(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_180022370(undefined8 param_1,longlong param_2)

{
  free(*(void **)(param_2 + 0x98));
  return;
}



/* ========================================================================
   ENTRY: 180022390
   NAME : FUN_180022390
   SIG  : undefined8 __fastcall FUN_180022390(undefined8 param_1, longlong param_2)
   ======================================================================== */

undefined8 FUN_180022390(undefined8 param_1,longlong param_2)

{
  *(undefined8 *)(*(longlong *)(param_2 + 0x78) + 0x118) = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 1800223c0
   NAME : FUN_1800223c0
   SIG  : undefined __fastcall FUN_1800223c0(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_1800223c0(undefined8 param_1,longlong param_2)

{
  FUN_180022130(*(char *)(param_2 + 0x60));
  return;
}



/* ========================================================================
   ENTRY: 1800223e0
   NAME : FUN_1800223e0
   SIG  : undefined __fastcall FUN_1800223e0(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_1800223e0(undefined8 param_1,longlong param_2)

{
  FUN_180022130(*(char *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 1800223fa
   NAME : FUN_1800223fa
   SIG  : undefined __fastcall FUN_1800223fa(void)
   ======================================================================== */

void FUN_1800223fa(void)

{
  FUN_180022030();
  return;
}



/* ========================================================================
   ENTRY: 180022410
   NAME : FUN_180022410
   SIG  : undefined __fastcall FUN_180022410(undefined8 * param_1, longlong param_2)
   ======================================================================== */

void FUN_180022410(undefined8 *param_1,longlong param_2)

{
  FUN_180021fa0(*(undefined8 *)(param_2 + 0x60),*(int *)(param_2 + 0x68),
                *(undefined8 *)(param_2 + 0x70),FUN_1800217a0,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180022450
   NAME : FUN_180022450
   SIG  : bool __fastcall FUN_180022450(undefined8 * param_1)
   ======================================================================== */

bool FUN_180022450(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}


