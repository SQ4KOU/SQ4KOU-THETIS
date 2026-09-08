
/* ========================================================================
   ENTRY: 180001000
   NAME : FUN_180001000
   SIG  : undefined __fastcall FUN_180001000(longlong param_1)
   ======================================================================== */

void FUN_180001000(longlong param_1)

{
  uint uVar1;
  uint uVar2;
  HANDLE pvVar3;
  bool bVar4;
  DWORD local_res8 [2];
  
  local_res8[0] = 0;
  pvVar3 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res8);
  if (pvVar3 == (HANDLE)0x0) {
    pvVar3 = GetCurrentThread();
    SetThreadPriority(pvVar3,2);
  }
  else {
    AvSetMmThreadPriority(pvVar3,AVRT_PRIORITY_CRITICAL);
  }
  uVar2 = *(uint *)(param_1 + 8);
  do {
    LOCK();
    uVar1 = *(uint *)(param_1 + 8);
    bVar4 = uVar2 == uVar1;
    if (bVar4) {
      *(uint *)(param_1 + 8) = uVar2 & 1;
      uVar1 = uVar2;
    }
    uVar2 = uVar1;
    UNLOCK();
  } while (!bVar4);
  while (uVar2 != 0) {
    WaitForMultipleObjects(*(DWORD *)(param_1 + 0x1ac),(HANDLE *)(param_1 + 0x640),1,0xffffffff);
    FUN_180001bb0(param_1);
    (**(code **)(param_1 + 0xef0))
              (*(undefined4 *)(param_1 + 4),*(undefined4 *)(param_1 + 0x90),
               *(undefined8 *)(param_1 + 0x1a0));
    uVar2 = *(uint *)(param_1 + 8);
    do {
      LOCK();
      uVar1 = *(uint *)(param_1 + 8);
      bVar4 = uVar2 == uVar1;
      if (bVar4) {
        *(uint *)(param_1 + 8) = uVar2 & 1;
        uVar1 = uVar2;
      }
      uVar2 = uVar1;
      UNLOCK();
    } while (!bVar4);
  }
  _endthread();
  return;
}



/* ========================================================================
   ENTRY: 1800010e0
   NAME : FUN_1800010e0
   SIG  : undefined __fastcall FUN_1800010e0(longlong param_1)
   ======================================================================== */

void FUN_1800010e0(longlong param_1)

{
  double dVar1;
  double dVar2;
  double dVar3;
  undefined8 uVar4;
  HANDLE pvVar5;
  int iVar6;
  longlong lVar7;
  int iVar8;
  int iVar9;
  int iVar10;
  double dVar11;
  double dVar12;
  double dVar13;
  
  iVar10 = 0;
  dVar13 = (double)*(int *)(param_1 + 0xde8);
  *(undefined8 *)(param_1 + 0xf18) = 0;
  *(undefined8 *)(param_1 + 0xf20) = 0;
  *(int *)(param_1 + 0xf28) = (int)(dVar13 * *(double *)(param_1 + 0xef8));
  *(int *)(param_1 + 0xf38) = (int)(dVar13 * *(double *)(param_1 + 0xf08));
  iVar6 = (int)(dVar13 * *(double *)(param_1 + 0xf00));
  *(int *)(param_1 + 0xf3c) = (int)(dVar13 * *(double *)(param_1 + 0xf10));
  *(int *)(param_1 + 0xf2c) = iVar6;
  uVar4 = malloc0(iVar6 * 8 + 8);
  *(undefined8 *)(param_1 + 0xf30) = uVar4;
  uVar4 = malloc0(*(int *)(param_1 + 0xf3c) * 8 + 8);
  dVar3 = DAT_180018de0;
  dVar2 = DAT_180018dc8;
  dVar1 = DAT_180018db0;
  dVar13 = 0.0;
  dVar12 = 0.0;
  *(undefined8 *)(param_1 + 0xf40) = uVar4;
  iVar6 = *(int *)(param_1 + 0xf2c);
  iVar8 = iVar10;
  if (-1 < iVar6) {
    do {
      dVar11 = cos(dVar12);
      dVar12 = dVar12 + dVar3 / (double)iVar6;
      iVar9 = iVar8 + 1;
      *(double *)(*(longlong *)(param_1 + 0xf30) + (longlong)iVar8 * 8) = (dVar2 - dVar11) * dVar1;
      iVar8 = iVar9;
    } while (iVar9 <= *(int *)(param_1 + 0xf2c));
  }
  iVar6 = *(int *)(param_1 + 0xf3c);
  if (-1 < iVar6) {
    do {
      dVar12 = cos(dVar13);
      lVar7 = (longlong)iVar10;
      dVar13 = dVar13 + dVar3 / (double)iVar6;
      iVar10 = iVar10 + 1;
      *(double *)(*(longlong *)(param_1 + 0xf40) + lVar7 * 8) = (dVar12 + dVar2) * dVar1;
    } while (iVar10 <= *(int *)(param_1 + 0xf3c));
  }
  pvVar5 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
  *(HANDLE *)(param_1 + 0xf50) = pvVar5;
  pvVar5 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
  dVar13 = DAT_180018e58;
  *(HANDLE *)(param_1 + 0xf58) = pvVar5;
  LOCK();
  *(uint *)(param_1 + 0xf48) = *(uint *)(param_1 + 0xf48) & 0xfffffffe;
  UNLOCK();
  LOCK();
  *(uint *)(param_1 + 0xf4c) = *(uint *)(param_1 + 0xf4c) & 0xfffffffe;
  UNLOCK();
  *(int *)(param_1 + 0xf64) =
       2 - (int)((*(double *)(param_1 + 0xf00) + *(double *)(param_1 + 0xef8)) * dVar13);
  *(int *)(param_1 + 0xf60) =
       2 - (int)((*(double *)(param_1 + 0xf10) + *(double *)(param_1 + 0xf08)) * dVar13);
  return;
}



/* ========================================================================
   ENTRY: 180001340
   NAME : FUN_180001340
   SIG  : int * __fastcall FUN_180001340(int param_1, int param_2, int param_3, int param_4, int param_5, int param_6, int param_7, undefined8 param_8, undefined8 param_9, longlong param_10, int param_11, undefined8 param_12, undefined8 param_13, undefined8 param_14, undefined8 param_15, undefined8 param_16)
   ======================================================================== */

int * FUN_180001340(int param_1,int param_2,int param_3,int param_4,int param_5,int param_6,
                   int param_7,undefined8 param_8,undefined8 param_9,longlong param_10,int param_11,
                   undefined8 param_12,undefined8 param_13,undefined8 param_14,undefined8 param_15,
                   undefined8 param_16)

{
  uint uVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  ulonglong uVar5;
  uint uVar6;
  int *_ArgList;
  undefined8 uVar7;
  longlong lVar8;
  HANDLE pvVar9;
  undefined8 uVar10;
  int iVar11;
  uint uVar12;
  int iVar13;
  bool bVar14;
  
  _ArgList = (int *)malloc0(0xf68);
  _ArgList[1] = param_2;
  iVar11 = 0;
  *_ArgList = param_1;
  _ArgList[0x23] = param_3;
  _ArgList[0x24] = param_4;
  _ArgList[0x25] = param_5;
  _ArgList[0x6a] = param_6;
  *(undefined8 *)(_ArgList + 0xee) = param_8;
  _ArgList[0x6c] = param_7;
  _ArgList[0x37a] = param_11;
  *(undefined8 *)(_ArgList + 0x3be) = param_13;
  *(undefined8 *)(_ArgList + 0x3c0) = param_14;
  *(undefined8 *)(_ArgList + 0x3bc) = param_12;
  *(undefined8 *)(_ArgList + 0x3c2) = param_15;
  *(undefined8 *)(_ArgList + 0x3c4) = param_16;
  _ArgList[0x26] = 0x1000;
  if (0 < _ArgList[0x25]) {
    do {
      uVar7 = malloc0(_ArgList[0x26] << 4);
      lVar8 = (longlong)iVar11;
      iVar11 = iVar11 + 1;
      *(undefined8 *)(_ArgList + lVar8 * 2 + 0x28) = uVar7;
    } while (iVar11 < _ArgList[0x25]);
  }
  uVar7 = malloc0(_ArgList[0x24] << 4);
  uVar12 = 0;
  *(undefined8 *)(_ArgList + 0x68) = uVar7;
  _ArgList[0x6b] = 0;
  if (0 < _ArgList[0x25]) {
    do {
      lVar8 = (longlong)(int)uVar12;
      (_ArgList + lVar8 * 2 + 0x6e)[0] = 0;
      (_ArgList + lVar8 * 2 + 0x6e)[1] = 0x3ff00000;
      *(undefined8 *)(_ArgList + lVar8 * 2 + 0xae) = *(undefined8 *)(_ArgList + 0xee);
      _ArgList[lVar8 + 0xf0] = 0;
      _ArgList[lVar8 + 0x110] = 0;
      _ArgList[lVar8 + 0x130] = 0;
      pvVar9 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1000,(LPCWSTR)0x0);
      *(HANDLE *)(_ArgList + lVar8 * 2 + 0x150) = pvVar9;
      InitializeCriticalSectionAndSpinCount
                ((LPCRITICAL_SECTION)(_ArgList + lVar8 * 10 + 0x1d0),0x9c4);
      uVar6 = _ArgList[0x6a];
      do {
        LOCK();
        uVar1 = _ArgList[0x6a];
        bVar14 = uVar6 == uVar1;
        if (bVar14) {
          _ArgList[0x6a] = uVar6;
          uVar1 = uVar6;
        }
        uVar6 = uVar1;
        UNLOCK();
      } while (!bVar14);
      if ((uVar6 >> (uVar12 & 0x1f) & 1) == 0) {
        LOCK();
        _ArgList[lVar8 + 3] = _ArgList[lVar8 + 3] & 0xfffffffe;
        UNLOCK();
      }
      else {
        *(undefined8 *)(_ArgList + (longlong)_ArgList[0x6b] * 2 + 400) =
             *(undefined8 *)(_ArgList + lVar8 * 2 + 0x150);
        _ArgList[0x6b] = _ArgList[0x6b] + 1;
        LOCK();
        _ArgList[lVar8 + 3] = _ArgList[lVar8 + 3] | 1;
        UNLOCK();
      }
      uVar7 = DAT_180018dc8;
      uVar12 = uVar12 + 1;
    } while ((int)uVar12 < _ArgList[0x25]);
    iVar11 = 0;
    if (0 < _ArgList[0x25]) {
      do {
        lVar8 = (longlong)iVar11;
        iVar2 = *(int *)(param_10 + lVar8 * 4);
        _ArgList[lVar8 + 0x35a] = iVar2;
        iVar3 = _ArgList[0x37a];
        iVar4 = _ArgList[0x23];
        if (iVar3 < iVar2) {
          uVar5 = (longlong)iVar2 % (longlong)iVar3;
          iVar13 = (iVar2 / iVar3) * iVar4;
        }
        else {
          iVar13 = iVar4 / (iVar3 / iVar2);
          uVar5 = (longlong)iVar4 % (longlong)(iVar3 / iVar2);
        }
        uVar10 = malloc0(iVar4 << 4,uVar5 & 0xffffffff);
        *(undefined8 *)(_ArgList + lVar8 * 2 + 0x37c) = uVar10;
        uVar10 = create_resample(iVar2 != iVar3,iVar13,0,uVar10,_ArgList[lVar8 + 0x35a],
                                 _ArgList[0x37a],0,0,uVar7);
        iVar11 = iVar11 + 1;
        *(undefined8 *)(_ArgList + lVar8 * 2 + 0x31a) = uVar10;
      } while (iVar11 < _ArgList[0x25]);
    }
  }
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(_ArgList + 0x310),0x9c4);
  FUN_1800010e0((longlong)_ArgList);
  LOCK();
  _ArgList[2] = _ArgList[2] | 1;
  UNLOCK();
  if (_ArgList[0x6b] != 0) {
    _beginthread(FUN_180001000,0,_ArgList);
  }
  if (-1 < *_ArgList) {
    (&DAT_18001e140)[param_1] = _ArgList;
  }
  return _ArgList;
}



/* ========================================================================
   ENTRY: 1800016f0
   NAME : FUN_1800016f0
   SIG  : undefined __fastcall FUN_1800016f0(void * param_1, int param_2)
   ======================================================================== */

void FUN_1800016f0(void *param_1,int param_2)

{
  int iVar1;
  longlong lVar2;
  
  if (param_1 == (void *)0x0) {
    param_1 = (void *)(&DAT_18001e140)[param_2];
  }
  LOCK();
  *(uint *)((longlong)param_1 + 8) = *(uint *)((longlong)param_1 + 8) & 0xfffffffe;
  UNLOCK();
  iVar1 = 0;
  if (0 < *(int *)((longlong)param_1 + 0x94)) {
    do {
      ReleaseSemaphore(*(HANDLE *)((longlong)param_1 + (longlong)iVar1 * 8 + 0x540),1,(LPLONG)0x0);
      iVar1 = iVar1 + 1;
    } while (iVar1 < *(int *)((longlong)param_1 + 0x94));
  }
  Sleep(2);
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)param_1 + 0xc40));
  iVar1 = 0;
  if (0 < *(int *)((longlong)param_1 + 0x94)) {
    do {
      lVar2 = (longlong)iVar1;
      destroy_resample(*(undefined8 *)((longlong)param_1 + lVar2 * 8 + 0xc68));
      _aligned_free(*(void **)((longlong)param_1 + lVar2 * 8 + 0xdf0));
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)param_1 + lVar2 * 0x28 + 0x740));
      CloseHandle(*(HANDLE *)((longlong)param_1 + lVar2 * 8 + 0x540));
      iVar1 = iVar1 + 1;
    } while (iVar1 < *(int *)((longlong)param_1 + 0x94));
  }
  _aligned_free(*(void **)((longlong)param_1 + 0x1a0));
  iVar1 = 0;
  if (0 < *(int *)((longlong)param_1 + 0x94)) {
    do {
      _aligned_free(*(void **)((longlong)param_1 + (longlong)iVar1 * 8 + 0xa0));
      iVar1 = iVar1 + 1;
    } while (iVar1 < *(int *)((longlong)param_1 + 0x94));
  }
  CloseHandle(*(HANDLE *)((longlong)param_1 + 0xf58));
  CloseHandle(*(HANDLE *)((longlong)param_1 + 0xf50));
  _aligned_free(*(void **)((longlong)param_1 + 0xf40));
  _aligned_free(*(void **)((longlong)param_1 + 0xf30));
                    /* WARNING: Could not recover jumptable at 0x000180001831. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _aligned_free(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180001840
   NAME : FUN_180001840
   SIG  : undefined __fastcall FUN_180001840(longlong param_1, int param_2, int param_3, void * param_4)
   ======================================================================== */

void FUN_180001840(longlong param_1,int param_2,int param_3,void *param_4)

{
  longlong lVar1;
  uint *puVar2;
  uint uVar3;
  int iVar4;
  int *piVar5;
  int iVar6;
  uint uVar7;
  int iVar8;
  int iVar9;
  longlong lVar10;
  bool bVar11;
  
  if (param_1 == 0) {
    param_1 = (&DAT_18001e140)[param_2];
  }
  lVar10 = (longlong)param_3;
  uVar7 = *(uint *)(param_1 + 0xc + lVar10 * 4);
  do {
    puVar2 = (uint *)(param_1 + 0xc + lVar10 * 4);
    LOCK();
    uVar3 = *puVar2;
    bVar11 = uVar7 == uVar3;
    if (bVar11) {
      *puVar2 = uVar7 & 1;
      uVar3 = uVar7;
    }
    uVar7 = uVar3;
    UNLOCK();
  } while (!bVar11);
  if (uVar7 != 0) {
    lVar1 = param_1 + lVar10 * 0x28;
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x740));
    piVar5 = *(int **)(param_1 + 0xc68 + lVar10 * 8);
    if (*piVar5 != 0) {
      *(void **)(piVar5 + 2) = param_4;
      param_4 = *(void **)(param_1 + 0xdf0 + lVar10 * 8);
      xresample(*(undefined8 *)(param_1 + 0xc68 + lVar10 * 8));
    }
    iVar8 = *(int *)(param_1 + 0x3c0 + lVar10 * 4);
    iVar4 = *(int *)(param_1 + 0x8c);
    iVar9 = *(int *)(param_1 + 0x98) - iVar8;
    iVar6 = iVar9;
    if (iVar4 <= iVar9) {
      iVar6 = iVar4;
    }
    memcpy((void *)(*(longlong *)(param_1 + 0xa0 + lVar10 * 8) + (longlong)(iVar8 * 2) * 8),param_4,
           (longlong)iVar6 << 4);
    iVar8 = iVar4 - iVar9;
    if (iVar4 <= iVar9) {
      iVar8 = 0;
    }
    memcpy(*(void **)(param_1 + 0xa0 + lVar10 * 8),
           (void *)((longlong)param_4 + (longlong)(iVar6 * 2) * 8),(longlong)iVar8 << 4);
    piVar5 = (int *)(param_1 + 0x4c0 + lVar10 * 4);
    *piVar5 = *piVar5 + *(int *)(param_1 + 0x8c);
    iVar8 = *(int *)(param_1 + 0x4c0 + lVar10 * 4);
    if (*(int *)(param_1 + 0x90) <= iVar8) {
      iVar8 = iVar8 / *(int *)(param_1 + 0x90);
      ReleaseSemaphore(*(HANDLE *)(param_1 + 0x540 + lVar10 * 8),iVar8,(LPLONG)0x0);
      piVar5 = (int *)(param_1 + 0x4c0 + lVar10 * 4);
      *piVar5 = *piVar5 - iVar8 * *(int *)(param_1 + 0x90);
    }
    piVar5 = (int *)(param_1 + 0x3c0 + lVar10 * 4);
    *piVar5 = *piVar5 + *(int *)(param_1 + 0x8c);
    iVar8 = *(int *)(param_1 + 0x3c0 + lVar10 * 4);
    if (*(int *)(param_1 + 0x98) <= iVar8) {
      *(int *)(param_1 + 0x3c0 + lVar10 * 4) = iVar8 - *(int *)(param_1 + 0x98);
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x740));
  }
  return;
}



/* ========================================================================
   ENTRY: 1800019e0
   NAME : FUN_1800019e0
   SIG  : undefined __fastcall FUN_1800019e0(longlong param_1)
   ======================================================================== */

void FUN_1800019e0(longlong param_1)

{
  int iVar1;
  longlong lVar2;
  longlong lVar3;
  int iVar4;
  int iVar5;
  
  iVar5 = *(int *)(param_1 + 0x90);
  if (0 < iVar5) {
    iVar4 = 0;
    lVar2 = *(longlong *)(param_1 + 0x1a0);
    do {
      iVar1 = *(int *)(param_1 + 0xf1c);
      lVar3 = (longlong)(iVar4 * 2);
      if (iVar1 == 0) {
        if (*(int *)(param_1 + 0xf38) < 1) {
LAB_180001b3e:
          if (*(int *)(param_1 + 0xf3c) < 1) {
            *(int *)(param_1 + 0xf24) = iVar5;
LAB_180001b7d:
            *(undefined4 *)(param_1 + 0xf1c) = 6;
          }
          else {
            *(int *)(param_1 + 0xf24) = *(int *)(param_1 + 0xf3c);
            *(undefined4 *)(param_1 + 0xf1c) = 5;
          }
        }
        else {
          *(undefined4 *)(param_1 + 0xf1c) = 4;
          *(int *)(param_1 + 0xf24) = *(int *)(param_1 + 0xf38);
        }
      }
      else if (iVar1 == 4) {
        iVar1 = *(int *)(param_1 + 0xf24);
        *(int *)(param_1 + 0xf24) = iVar1 + -1;
        if (iVar1 == 0) goto LAB_180001b3e;
      }
      else if (iVar1 == 5) {
        *(double *)(lVar2 + lVar3 * 8) =
             *(double *)
              (*(longlong *)(param_1 + 0xf40) +
              (longlong)(*(int *)(param_1 + 0xf3c) - *(int *)(param_1 + 0xf24)) * 8) *
             *(double *)(lVar2 + lVar3 * 8);
        *(double *)(lVar2 + 8 + lVar3 * 8) =
             *(double *)(lVar2 + 8 + lVar3 * 8) *
             *(double *)
              (*(longlong *)(param_1 + 0xf40) +
              (longlong)(*(int *)(param_1 + 0xf3c) - *(int *)(param_1 + 0xf24)) * 8);
        iVar5 = *(int *)(param_1 + 0xf24);
        *(int *)(param_1 + 0xf24) = iVar5 + -1;
        if (iVar5 == 0) {
          *(undefined4 *)(param_1 + 0xf24) = *(undefined4 *)(param_1 + 0x90);
          goto LAB_180001b7d;
        }
      }
      else if (iVar1 == 6) {
        *(undefined8 *)(lVar2 + lVar3 * 8) = 0;
        *(undefined8 *)(lVar2 + 8 + lVar3 * 8) = 0;
        iVar5 = *(int *)(param_1 + 0xf24);
        *(int *)(param_1 + 0xf24) = iVar5 + -1;
        if (iVar5 == 0) {
          *(undefined4 *)(param_1 + 0xf1c) = 7;
        }
      }
      else if (iVar1 == 7) {
        *(undefined8 *)(lVar2 + lVar3 * 8) = 0;
        *(undefined8 *)(lVar2 + 8 + lVar3 * 8) = 0;
        if (iVar4 == *(int *)(param_1 + 0x90) + -1) {
          *(undefined4 *)(param_1 + 0xf1c) = 0;
          LOCK();
          *(uint *)(param_1 + 0xf4c) = *(uint *)(param_1 + 0xf4c) & 0xfffffffe;
          UNLOCK();
          ReleaseSemaphore(*(HANDLE *)(param_1 + 0xf58),1,(LPLONG)0x0);
        }
      }
      iVar5 = *(int *)(param_1 + 0x90);
      iVar4 = iVar4 + 1;
    } while (iVar4 < iVar5);
  }
  return;
}



/* ========================================================================
   ENTRY: 180001bb0
   NAME : FUN_180001bb0
   SIG  : undefined __fastcall FUN_180001bb0(longlong param_1)
   ======================================================================== */

void FUN_180001bb0(longlong param_1)

{
  int iVar1;
  uint *puVar2;
  int *piVar3;
  uint uVar4;
  double dVar5;
  double dVar6;
  int iVar7;
  int iVar8;
  int iVar9;
  int iVar10;
  longlong lVar11;
  uint uVar12;
  uint uVar13;
  longlong lVar14;
  bool bVar15;
  
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  uVar12 = *(uint *)(param_1 + 8);
  do {
    LOCK();
    uVar13 = *(uint *)(param_1 + 8);
    bVar15 = uVar12 == uVar13;
    if (bVar15) {
      *(uint *)(param_1 + 8) = uVar12 & 1;
      uVar13 = uVar12;
    }
    uVar12 = uVar13;
    UNLOCK();
  } while (!bVar15);
  if (uVar12 == 0) {
    LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
                    /* WARNING: Could not recover jumptable at 0x000180001bf4. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    _endthread();
    return;
  }
  memset(*(void **)(param_1 + 0x1a0),0,(longlong)*(int *)(param_1 + 0x90) << 4);
  uVar12 = *(uint *)(param_1 + 0x1b0);
  do {
    LOCK();
    uVar13 = *(uint *)(param_1 + 0x1b0);
    bVar15 = uVar12 == uVar13;
    if (bVar15) {
      *(uint *)(param_1 + 0x1b0) = uVar12;
      uVar13 = uVar12;
    }
    uVar12 = uVar13;
    UNLOCK();
  } while (!bVar15);
  uVar13 = *(uint *)(param_1 + 0x1a8);
  do {
    LOCK();
    uVar4 = *(uint *)(param_1 + 0x1a8);
    bVar15 = uVar13 == uVar4;
    if (bVar15) {
      *(uint *)(param_1 + 0x1a8) = uVar13;
      uVar4 = uVar13;
    }
    uVar13 = uVar4;
    UNLOCK();
  } while (!bVar15);
  iVar9 = 0;
  uVar12 = uVar12 & uVar13;
  if (uVar12 != 0) {
    uVar13 = 1;
    iVar7 = iVar9;
    do {
      if ((uVar12 & uVar13) != 0) {
        lVar14 = (longlong)iVar7;
        iVar8 = *(int *)(param_1 + 0x440 + lVar14 * 4);
        iVar10 = iVar9;
        if (0 < *(int *)(param_1 + 0x90)) {
          do {
            lVar11 = (longlong)(iVar10 * 2);
            iVar10 = iVar10 + 1;
            *(double *)(*(longlong *)(param_1 + 0x1a0) + lVar11 * 8) =
                 *(double *)(*(longlong *)(param_1 + 0xa0 + lVar14 * 8) + (longlong)(iVar8 * 2) * 8)
                 * *(double *)(param_1 + 0x2b8 + lVar14 * 8) +
                 *(double *)(*(longlong *)(param_1 + 0x1a0) + lVar11 * 8);
            iVar1 = iVar8 + 1;
            *(double *)(*(longlong *)(param_1 + 0x1a0) + 8 + lVar11 * 8) =
                 *(double *)
                  (*(longlong *)(param_1 + 0xa0 + lVar14 * 8) + 8 + (longlong)(iVar8 * 2) * 8) *
                 *(double *)(param_1 + 0x2b8 + lVar14 * 8) +
                 *(double *)(*(longlong *)(param_1 + 0x1a0) + 8 + lVar11 * 8);
            iVar8 = 0;
            if (iVar1 != *(int *)(param_1 + 0x98)) {
              iVar8 = iVar1;
            }
          } while (iVar10 < *(int *)(param_1 + 0x90));
        }
        uVar12 = uVar12 & ~uVar13;
      }
      iVar7 = iVar7 + 1;
      uVar13 = uVar13 << 1 | (uint)((int)uVar13 < 0);
    } while (uVar12 != 0);
  }
  iVar7 = iVar9;
  if (0 < *(int *)(param_1 + 0x94)) {
    do {
      lVar14 = (longlong)iVar7;
      uVar12 = *(uint *)(param_1 + 0xc + lVar14 * 4);
      do {
        puVar2 = (uint *)(param_1 + 0xc + lVar14 * 4);
        LOCK();
        uVar13 = *puVar2;
        bVar15 = uVar12 == uVar13;
        if (bVar15) {
          *puVar2 = uVar12 & 1;
          uVar13 = uVar12;
        }
        uVar12 = uVar13;
        UNLOCK();
      } while (!bVar15);
      if (uVar12 != 0) {
        piVar3 = (int *)(param_1 + 0x440 + lVar14 * 4);
        *piVar3 = *piVar3 + *(int *)(param_1 + 0x90);
        iVar8 = *(int *)(param_1 + 0x440 + lVar14 * 4);
        if (*(int *)(param_1 + 0x98) <= iVar8) {
          *(int *)(param_1 + 0x440 + lVar14 * 4) = iVar8 - *(int *)(param_1 + 0x98);
        }
      }
      iVar7 = iVar7 + 1;
    } while (iVar7 < *(int *)(param_1 + 0x94));
  }
  uVar12 = *(uint *)(param_1 + 0xf48);
  do {
    LOCK();
    uVar13 = *(uint *)(param_1 + 0xf48);
    bVar15 = uVar12 == uVar13;
    if (bVar15) {
      *(uint *)(param_1 + 0xf48) = uVar12 & 1;
      uVar13 = uVar12;
    }
    uVar12 = uVar13;
    UNLOCK();
  } while (!bVar15);
  if ((uVar12 != 0) && (iVar7 = *(int *)(param_1 + 0x90), 0 < iVar7)) {
    lVar14 = *(longlong *)(param_1 + 0x1a0);
    do {
      iVar8 = *(int *)(param_1 + 0xf18);
      lVar11 = (longlong)(iVar9 * 2);
      dVar5 = *(double *)(lVar14 + lVar11 * 8);
      dVar6 = *(double *)(lVar14 + 8 + lVar11 * 8);
      if (iVar8 == 0) {
        *(undefined8 *)(lVar14 + lVar11 * 8) = 0;
        *(undefined8 *)(lVar14 + 8 + lVar11 * 8) = 0;
        if ((dVar5 != 0.0) || (dVar6 != 0.0)) {
          if (*(int *)(param_1 + 0xf28) < 1) goto LAB_180001f50;
          *(undefined4 *)(param_1 + 0xf18) = 1;
          *(int *)(param_1 + 0xf20) = *(int *)(param_1 + 0xf28);
        }
      }
      else if (iVar8 == 1) {
        *(undefined8 *)(lVar14 + lVar11 * 8) = 0;
        *(undefined8 *)(lVar14 + 8 + lVar11 * 8) = 0;
        iVar7 = *(int *)(param_1 + 0xf20);
        *(int *)(param_1 + 0xf20) = iVar7 + -1;
        if (iVar7 == 0) {
LAB_180001f50:
          if (*(int *)(param_1 + 0xf2c) < 1) goto LAB_180001eba;
          *(int *)(param_1 + 0xf20) = *(int *)(param_1 + 0xf2c);
          *(undefined4 *)(param_1 + 0xf18) = 2;
        }
      }
      else if (iVar8 == 2) {
        *(double *)(lVar14 + lVar11 * 8) =
             dVar5 * *(double *)
                      (*(longlong *)(param_1 + 0xf30) +
                      (longlong)(*(int *)(param_1 + 0xf2c) - *(int *)(param_1 + 0xf20)) * 8);
        *(double *)(lVar14 + 8 + lVar11 * 8) =
             dVar6 * *(double *)
                      (*(longlong *)(param_1 + 0xf30) +
                      (longlong)(*(int *)(param_1 + 0xf2c) - *(int *)(param_1 + 0xf20)) * 8);
        iVar7 = *(int *)(param_1 + 0xf20);
        *(int *)(param_1 + 0xf20) = iVar7 + -1;
        if (iVar7 == 0) {
LAB_180001eba:
          *(undefined4 *)(param_1 + 0xf18) = 3;
        }
      }
      else if ((iVar8 == 3) && (iVar9 == iVar7 + -1)) {
        *(undefined4 *)(param_1 + 0xf18) = 0;
        LOCK();
        *(uint *)(param_1 + 0xf48) = *(uint *)(param_1 + 0xf48) & 0xfffffffe;
        UNLOCK();
        ReleaseSemaphore(*(HANDLE *)(param_1 + 0xf50),1,(LPLONG)0x0);
      }
      iVar7 = *(int *)(param_1 + 0x90);
      iVar9 = iVar9 + 1;
    } while (iVar9 < iVar7);
  }
  uVar12 = *(uint *)(param_1 + 0xf4c);
  do {
    LOCK();
    uVar13 = *(uint *)(param_1 + 0xf4c);
    bVar15 = uVar12 == uVar13;
    if (bVar15) {
      *(uint *)(param_1 + 0xf4c) = uVar12 & 1;
      uVar13 = uVar12;
    }
    uVar12 = uVar13;
    UNLOCK();
  } while (!bVar15);
  if (uVar12 != 0) {
    FUN_1800019e0(param_1);
  }
                    /* WARNING: Could not recover jumptable at 0x000180001f29. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  return;
}



/* ========================================================================
   ENTRY: 180001fb0
   NAME : FUN_180001fb0
   SIG  : undefined __fastcall FUN_180001fb0(longlong param_1)
   ======================================================================== */

void FUN_180001fb0(longlong param_1)

{
  uint *puVar1;
  DWORD DVar2;
  longlong lVar3;
  int iVar4;
  longlong lVar5;
  
  LOCK();
  *(uint *)(param_1 + 0xf4c) = *(uint *)(param_1 + 0xf4c) | 1;
  UNLOCK();
  WaitForSingleObject(*(HANDLE *)(param_1 + 0xf58),*(DWORD *)(param_1 + 0xf60));
  LOCK();
  *(uint *)(param_1 + 0xf4c) = *(uint *)(param_1 + 0xf4c) & 0xfffffffe;
  UNLOCK();
  iVar4 = 0;
  if (0 < *(int *)(param_1 + 0x94)) {
    do {
      lVar3 = (longlong)iVar4;
      iVar4 = iVar4 + 1;
      LOCK();
      puVar1 = (uint *)(param_1 + 0xc + lVar3 * 4);
      *puVar1 = *puVar1 & 0xfffffffe;
      UNLOCK();
    } while (iVar4 < *(int *)(param_1 + 0x94));
  }
  Sleep(1);
  iVar4 = 0;
  if (0 < *(int *)(param_1 + 0x94)) {
    do {
      EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + (longlong)iVar4 * 0x28 + 0x740));
      iVar4 = iVar4 + 1;
    } while (iVar4 < *(int *)(param_1 + 0x94));
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  Sleep(0x19);
  LOCK();
  *(uint *)(param_1 + 8) = *(uint *)(param_1 + 8) & 0xfffffffe;
  UNLOCK();
  iVar4 = 0;
  if (0 < *(int *)(param_1 + 0x94)) {
    do {
      ReleaseSemaphore(*(HANDLE *)(param_1 + 0x540 + (longlong)iVar4 * 8),1,(LPLONG)0x0);
      iVar4 = iVar4 + 1;
    } while (iVar4 < *(int *)(param_1 + 0x94));
  }
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  Sleep(2);
  iVar4 = 0;
  if (0 < *(int *)(param_1 + 0x94)) {
    do {
      lVar5 = (longlong)iVar4;
      lVar3 = param_1 + lVar5 * 8;
      memset(*(void **)(lVar3 + 0xa0),0,(longlong)*(int *)(param_1 + 0x98) << 4);
      *(undefined4 *)(param_1 + 0x3c0 + lVar5 * 4) = 0;
      *(undefined4 *)(param_1 + 0x440 + lVar5 * 4) = 0;
      *(undefined4 *)(param_1 + 0x4c0 + lVar5 * 4) = 0;
      do {
        DVar2 = WaitForSingleObject(*(HANDLE *)(param_1 + 0x540 + lVar5 * 8),1);
      } while (DVar2 == 0);
      flush_resample(*(undefined8 *)(lVar3 + 0xc68));
      iVar4 = iVar4 + 1;
    } while (iVar4 < *(int *)(param_1 + 0x94));
  }
  return;
}



/* ========================================================================
   ENTRY: 180002140
   NAME : FUN_180002140
   SIG  : undefined __fastcall FUN_180002140(void * param_1)
   ======================================================================== */

void FUN_180002140(void *param_1)

{
  uint *puVar1;
  uint uVar2;
  byte bVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  
  LOCK();
  *(uint *)((longlong)param_1 + 0xf48) = *(uint *)((longlong)param_1 + 0xf48) | 1;
  UNLOCK();
  LOCK();
  *(uint *)((longlong)param_1 + 8) = *(uint *)((longlong)param_1 + 8) | 1;
  UNLOCK();
  if (*(int *)((longlong)param_1 + 0x1ac) != 0) {
    _beginthread(FUN_180001000,0,param_1);
  }
  uVar6 = *(uint *)((longlong)param_1 + 0x94);
  while (uVar6 = uVar6 - 1, -1 < (int)uVar6) {
    LeaveCriticalSection((LPCRITICAL_SECTION)((longlong)param_1 + (ulonglong)uVar6 * 0x28 + 0x740));
  }
  uVar6 = *(int *)((longlong)param_1 + 0x94) - 1;
  if (-1 < (int)uVar6) {
    bVar3 = (byte)uVar6 & 0x1f;
    uVar5 = 1 << bVar3 | 1U >> 0x20 - bVar3;
    do {
      uVar4 = *(uint *)((longlong)param_1 + 0x1a8);
      do {
        LOCK();
        uVar2 = *(uint *)((longlong)param_1 + 0x1a8);
        bVar7 = uVar4 == uVar2;
        if (bVar7) {
          *(uint *)((longlong)param_1 + 0x1a8) = uVar4;
          uVar2 = uVar4;
        }
        uVar4 = uVar2;
        UNLOCK();
      } while (!bVar7);
      if ((uVar5 & uVar4) != 0) {
        LOCK();
        puVar1 = (uint *)((longlong)param_1 + (ulonglong)uVar6 * 4 + 0xc);
        *puVar1 = *puVar1 | 1;
        UNLOCK();
      }
      uVar5 = uVar5 >> 1 | (uint)((uVar5 & 1) != 0) << 0x1f;
      uVar6 = uVar6 - 1;
    } while (-1 < (int)uVar6);
  }
                    /* WARNING: Could not recover jumptable at 0x0001800021f7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  WaitForSingleObject(*(HANDLE *)((longlong)param_1 + 0xf50),*(DWORD *)((longlong)param_1 + 0xf64));
  return;
}



/* ========================================================================
   ENTRY: 180002200
   NAME : SetAAudioMixState
   SIG  : undefined __fastcall SetAAudioMixState(void * param_1, int param_2, byte param_3, uint param_4)
   ======================================================================== */

void SetAAudioMixState(void *param_1,int param_2,byte param_3,uint param_4)

{
  uint *puVar1;
  int iVar2;
  uint uVar3;
  byte bVar4;
  int iVar5;
  uint uVar6;
  uint uVar7;
  uint uVar8;
  longlong lVar9;
  bool bVar10;
  
                    /* 0x2200  62  SetAAudioMixState */
  if (param_1 == (void *)0x0) {
    param_1 = (void *)(&DAT_18001e140)[param_2];
  }
  iVar5 = *(int *)((longlong)param_1 + 0x1a8);
  do {
    LOCK();
    iVar2 = *(int *)((longlong)param_1 + 0x1a8);
    bVar10 = iVar5 == iVar2;
    if (bVar10) {
      *(int *)((longlong)param_1 + 0x1a8) = iVar5;
      iVar2 = iVar5;
    }
    iVar5 = iVar2;
    UNLOCK();
  } while (!bVar10);
  if ((iVar5 >> (param_3 & 0x1f) & 1U) != param_4) {
    FUN_180001fb0((longlong)param_1);
    uVar6 = 1 << (param_3 & 0x1f);
    if (param_4 == 0) {
      LOCK();
      *(uint *)((longlong)param_1 + 0x1a8) = *(uint *)((longlong)param_1 + 0x1a8) & ~uVar6;
      UNLOCK();
    }
    else {
      LOCK();
      *(uint *)((longlong)param_1 + 0x1a8) = *(uint *)((longlong)param_1 + 0x1a8) | uVar6;
      UNLOCK();
    }
    uVar6 = 0;
    *(undefined4 *)((longlong)param_1 + 0x1ac) = 0;
    if (0 < *(int *)((longlong)param_1 + 0x94)) {
      do {
        lVar9 = (longlong)(int)uVar6;
        uVar8 = *(uint *)((longlong)param_1 + 0x1a8);
        do {
          LOCK();
          uVar7 = *(uint *)((longlong)param_1 + 0x1a8);
          bVar10 = uVar8 == uVar7;
          if (bVar10) {
            *(uint *)((longlong)param_1 + 0x1a8) = uVar8;
            uVar7 = uVar8;
          }
          uVar8 = uVar7;
          UNLOCK();
        } while (!bVar10);
        if ((uVar8 >> (uVar6 & 0x1f) & 1) == 0) {
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + lVar9 * 4 + 0xc);
          *puVar1 = *puVar1 & 0xfffffffe;
          UNLOCK();
        }
        else {
          *(undefined8 *)
           ((longlong)param_1 + (longlong)*(int *)((longlong)param_1 + 0x1ac) * 8 + 0x640) =
               *(undefined8 *)((longlong)param_1 + lVar9 * 8 + 0x540);
          *(int *)((longlong)param_1 + 0x1ac) = *(int *)((longlong)param_1 + 0x1ac) + 1;
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + lVar9 * 4 + 0xc);
          *puVar1 = *puVar1 | 1;
          UNLOCK();
        }
        uVar6 = uVar6 + 1;
      } while ((int)uVar6 < *(int *)((longlong)param_1 + 0x94));
    }
    LOCK();
    *(uint *)((longlong)param_1 + 0xf48) = *(uint *)((longlong)param_1 + 0xf48) | 1;
    UNLOCK();
    LOCK();
    *(uint *)((longlong)param_1 + 8) = *(uint *)((longlong)param_1 + 8) | 1;
    UNLOCK();
    if (*(int *)((longlong)param_1 + 0x1ac) != 0) {
      _beginthread(FUN_180001000,0,param_1);
    }
    uVar6 = *(uint *)((longlong)param_1 + 0x94);
    while (uVar6 = uVar6 - 1, -1 < (int)uVar6) {
      LeaveCriticalSection
                ((LPCRITICAL_SECTION)((longlong)param_1 + (ulonglong)uVar6 * 0x28 + 0x740));
    }
    uVar6 = *(int *)((longlong)param_1 + 0x94) - 1;
    if (-1 < (int)uVar6) {
      bVar4 = (byte)uVar6 & 0x1f;
      uVar8 = 1 << bVar4 | 1U >> 0x20 - bVar4;
      do {
        uVar7 = *(uint *)((longlong)param_1 + 0x1a8);
        do {
          LOCK();
          uVar3 = *(uint *)((longlong)param_1 + 0x1a8);
          bVar10 = uVar7 == uVar3;
          if (bVar10) {
            *(uint *)((longlong)param_1 + 0x1a8) = uVar7;
            uVar3 = uVar7;
          }
          uVar7 = uVar3;
          UNLOCK();
        } while (!bVar10);
        if ((uVar8 & uVar7) != 0) {
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + (ulonglong)uVar6 * 4 + 0xc);
          *puVar1 = *puVar1 | 1;
          UNLOCK();
        }
        uVar8 = uVar8 >> 1 | (uint)((uVar8 & 1) != 0) << 0x1f;
        uVar6 = uVar6 - 1;
      } while (-1 < (int)uVar6);
    }
    WaitForSingleObject(*(HANDLE *)((longlong)param_1 + 0xf50),*(DWORD *)((longlong)param_1 + 0xf64)
                       );
  }
  return;
}



/* ========================================================================
   ENTRY: 1800023c0
   NAME : SetAAudioMixStates
   SIG  : undefined __fastcall SetAAudioMixStates(void * param_1, int param_2, uint param_3, uint param_4)
   ======================================================================== */

void SetAAudioMixStates(void *param_1,int param_2,uint param_3,uint param_4)

{
  uint *puVar1;
  uint uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  int iVar6;
  uint uVar7;
  uint uVar8;
  longlong lVar9;
  bool bVar10;
  
                    /* 0x23c0  63  SetAAudioMixStates */
  if (param_1 == (void *)0x0) {
    param_1 = (void *)(&DAT_18001e140)[param_2];
  }
  uVar7 = *(uint *)((longlong)param_1 + 0x1a8);
  do {
    LOCK();
    uVar8 = *(uint *)((longlong)param_1 + 0x1a8);
    bVar10 = uVar7 == uVar8;
    if (bVar10) {
      *(uint *)((longlong)param_1 + 0x1a8) = uVar7;
      uVar8 = uVar7;
    }
    uVar7 = uVar8;
    UNLOCK();
  } while (!bVar10);
  if ((uVar7 & param_3) != (param_3 & param_4)) {
    FUN_180001fb0((longlong)param_1);
    iVar3 = *(int *)((longlong)param_1 + 0x94);
    iVar6 = 0;
    if (0 < iVar3) {
      do {
        bVar5 = (byte)iVar6;
        if (((int)param_3 >> (bVar5 & 0x1f) & 1U) != 0) {
          uVar7 = 1 << (bVar5 & 0x1f);
          if (((int)param_4 >> (bVar5 & 0x1f) & 1U) == 0) {
            LOCK();
            *(uint *)((longlong)param_1 + 0x1a8) = *(uint *)((longlong)param_1 + 0x1a8) & ~uVar7;
            UNLOCK();
          }
          else {
            LOCK();
            *(uint *)((longlong)param_1 + 0x1a8) = *(uint *)((longlong)param_1 + 0x1a8) | uVar7;
            UNLOCK();
          }
        }
        iVar3 = *(int *)((longlong)param_1 + 0x94);
        iVar6 = iVar6 + 1;
      } while (iVar6 < iVar3);
    }
    uVar7 = 0;
    *(undefined4 *)((longlong)param_1 + 0x1ac) = 0;
    if (0 < iVar3) {
      do {
        lVar9 = (longlong)(int)uVar7;
        uVar8 = *(uint *)((longlong)param_1 + 0x1a8);
        do {
          LOCK();
          uVar4 = *(uint *)((longlong)param_1 + 0x1a8);
          bVar10 = uVar8 == uVar4;
          if (bVar10) {
            *(uint *)((longlong)param_1 + 0x1a8) = uVar8;
            uVar4 = uVar8;
          }
          uVar8 = uVar4;
          UNLOCK();
        } while (!bVar10);
        if ((uVar8 >> (uVar7 & 0x1f) & 1) == 0) {
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + lVar9 * 4 + 0xc);
          *puVar1 = *puVar1 & 0xfffffffe;
          UNLOCK();
        }
        else {
          *(undefined8 *)
           ((longlong)param_1 + (longlong)*(int *)((longlong)param_1 + 0x1ac) * 8 + 0x640) =
               *(undefined8 *)((longlong)param_1 + lVar9 * 8 + 0x540);
          *(int *)((longlong)param_1 + 0x1ac) = *(int *)((longlong)param_1 + 0x1ac) + 1;
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + lVar9 * 4 + 0xc);
          *puVar1 = *puVar1 | 1;
          UNLOCK();
        }
        uVar7 = uVar7 + 1;
      } while ((int)uVar7 < *(int *)((longlong)param_1 + 0x94));
    }
    LOCK();
    *(uint *)((longlong)param_1 + 0xf48) = *(uint *)((longlong)param_1 + 0xf48) | 1;
    UNLOCK();
    LOCK();
    *(uint *)((longlong)param_1 + 8) = *(uint *)((longlong)param_1 + 8) | 1;
    UNLOCK();
    if (*(int *)((longlong)param_1 + 0x1ac) != 0) {
      _beginthread(FUN_180001000,0,param_1);
    }
    uVar7 = *(uint *)((longlong)param_1 + 0x94);
    while (uVar7 = uVar7 - 1, -1 < (int)uVar7) {
      LeaveCriticalSection
                ((LPCRITICAL_SECTION)((longlong)param_1 + (ulonglong)uVar7 * 0x28 + 0x740));
    }
    uVar7 = *(int *)((longlong)param_1 + 0x94) - 1;
    if (-1 < (int)uVar7) {
      bVar5 = (byte)uVar7 & 0x1f;
      uVar8 = 1 << bVar5 | 1U >> 0x20 - bVar5;
      do {
        uVar4 = *(uint *)((longlong)param_1 + 0x1a8);
        do {
          LOCK();
          uVar2 = *(uint *)((longlong)param_1 + 0x1a8);
          bVar10 = uVar4 == uVar2;
          if (bVar10) {
            *(uint *)((longlong)param_1 + 0x1a8) = uVar4;
            uVar2 = uVar4;
          }
          uVar4 = uVar2;
          UNLOCK();
        } while (!bVar10);
        if ((uVar8 & uVar4) != 0) {
          LOCK();
          puVar1 = (uint *)((longlong)param_1 + (ulonglong)uVar7 * 4 + 0xc);
          *puVar1 = *puVar1 | 1;
          UNLOCK();
        }
        uVar8 = uVar8 >> 1 | (uint)((uVar8 & 1) != 0) << 0x1f;
        uVar7 = uVar7 - 1;
      } while (-1 < (int)uVar7);
    }
    WaitForSingleObject(*(HANDLE *)((longlong)param_1 + 0xf50),*(DWORD *)((longlong)param_1 + 0xf64)
                       );
  }
  return;
}



/* ========================================================================
   ENTRY: 1800025b0
   NAME : SetAAudioMixWhat
   SIG  : undefined __fastcall SetAAudioMixWhat(longlong param_1, int param_2, uint param_3, int param_4)
   ======================================================================== */

void SetAAudioMixWhat(longlong param_1,int param_2,uint param_3,int param_4)

{
  byte *pbVar1;
  
                    /* 0x25b0  66  SetAAudioMixWhat */
  if (param_1 == 0) {
    param_1 = (&DAT_18001e140)[param_2];
  }
  if (param_4 != 0) {
    LOCK();
    pbVar1 = (byte *)(param_1 + 0x1b0 + ((longlong)(int)param_3 >> 3));
    *pbVar1 = *pbVar1 | '\x01' << (param_3 & 7);
    UNLOCK();
    return;
  }
  LOCK();
  pbVar1 = (byte *)(param_1 + 0x1b0 + ((longlong)(int)param_3 >> 3));
  *pbVar1 = *pbVar1 & ~('\x01' << (param_3 & 7));
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 1800025e0
   NAME : SetAAudioMixVolume
   SIG  : undefined __fastcall SetAAudioMixVolume(longlong param_1, int param_2, double param_3)
   ======================================================================== */

void SetAAudioMixVolume(longlong param_1,int param_2,double param_3)

{
  undefined1 auVar1 [16];
  undefined1 auVar2 [16];
  undefined1 auVar3 [16];
  undefined1 auVar4 [16];
  undefined1 auVar5 [16];
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  undefined1 auVar10 [16];
  undefined1 auVar11 [16];
  undefined1 auVar12 [16];
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  undefined1 auVar15 [16];
  undefined1 auVar16 [16];
  double dVar17;
  
                    /* 0x25e0  65  SetAAudioMixVolume */
  if (param_1 == 0) {
    param_1 = (&DAT_18001e140)[param_2];
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  *(double *)(param_1 + 0x3b8) = param_3;
  dVar17 = *(double *)(param_1 + 0x1c0) * param_3;
  auVar1._8_4_ = SUB84(dVar17,0);
  auVar1._0_8_ = *(double *)(param_1 + 0x1b8) * param_3;
  auVar1._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x2b8) = auVar1;
  dVar17 = *(double *)(param_1 + 0x1d0) * param_3;
  auVar2._8_4_ = SUB84(dVar17,0);
  auVar2._0_8_ = *(double *)(param_1 + 0x1c8) * param_3;
  auVar2._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x2c8) = auVar2;
  dVar17 = *(double *)(param_1 + 0x1e0) * param_3;
  auVar3._8_4_ = SUB84(dVar17,0);
  auVar3._0_8_ = *(double *)(param_1 + 0x1d8) * param_3;
  auVar3._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x2d8) = auVar3;
  dVar17 = *(double *)(param_1 + 0x1f0) * param_3;
  auVar4._8_4_ = SUB84(dVar17,0);
  auVar4._0_8_ = *(double *)(param_1 + 0x1e8) * param_3;
  auVar4._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x2e8) = auVar4;
  dVar17 = *(double *)(param_1 + 0x200) * param_3;
  auVar5._8_4_ = SUB84(dVar17,0);
  auVar5._0_8_ = *(double *)(param_1 + 0x1f8) * param_3;
  auVar5._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x2f8) = auVar5;
  dVar17 = *(double *)(param_1 + 0x210) * param_3;
  auVar6._8_4_ = SUB84(dVar17,0);
  auVar6._0_8_ = *(double *)(param_1 + 0x208) * param_3;
  auVar6._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x308) = auVar6;
  dVar17 = *(double *)(param_1 + 0x220) * param_3;
  auVar7._8_4_ = SUB84(dVar17,0);
  auVar7._0_8_ = *(double *)(param_1 + 0x218) * param_3;
  auVar7._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x318) = auVar7;
  dVar17 = *(double *)(param_1 + 0x230) * param_3;
  auVar8._8_4_ = SUB84(dVar17,0);
  auVar8._0_8_ = *(double *)(param_1 + 0x228) * param_3;
  auVar8._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x328) = auVar8;
  dVar17 = *(double *)(param_1 + 0x240) * param_3;
  auVar9._8_4_ = SUB84(dVar17,0);
  auVar9._0_8_ = *(double *)(param_1 + 0x238) * param_3;
  auVar9._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x338) = auVar9;
  dVar17 = *(double *)(param_1 + 0x250) * param_3;
  auVar10._8_4_ = SUB84(dVar17,0);
  auVar10._0_8_ = *(double *)(param_1 + 0x248) * param_3;
  auVar10._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x348) = auVar10;
  dVar17 = *(double *)(param_1 + 0x260) * param_3;
  auVar11._8_4_ = SUB84(dVar17,0);
  auVar11._0_8_ = *(double *)(param_1 + 600) * param_3;
  auVar11._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x358) = auVar11;
  dVar17 = *(double *)(param_1 + 0x270) * param_3;
  auVar12._8_4_ = SUB84(dVar17,0);
  auVar12._0_8_ = *(double *)(param_1 + 0x268) * param_3;
  auVar12._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x368) = auVar12;
  dVar17 = *(double *)(param_1 + 0x280) * param_3;
  auVar13._8_4_ = SUB84(dVar17,0);
  auVar13._0_8_ = *(double *)(param_1 + 0x278) * param_3;
  auVar13._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x378) = auVar13;
  dVar17 = *(double *)(param_1 + 0x290) * param_3;
  auVar14._8_4_ = SUB84(dVar17,0);
  auVar14._0_8_ = *(double *)(param_1 + 0x288) * param_3;
  auVar14._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x388) = auVar14;
  dVar17 = *(double *)(param_1 + 0x2a0) * param_3;
  auVar15._8_4_ = SUB84(dVar17,0);
  auVar15._0_8_ = *(double *)(param_1 + 0x298) * param_3;
  auVar15._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x398) = auVar15;
  dVar17 = *(double *)(param_1 + 0x2b0) * param_3;
  auVar16._8_4_ = SUB84(dVar17,0);
  auVar16._0_8_ = *(double *)(param_1 + 0x2a8) * param_3;
  auVar16._12_4_ = (int)((ulonglong)dVar17 >> 0x20);
  *(undefined1 (*) [16])(param_1 + 0x3a8) = auVar16;
                    /* WARNING: Could not recover jumptable at 0x000180002757. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  return;
}



/* ========================================================================
   ENTRY: 180002760
   NAME : SetAAudioMixVol
   SIG  : undefined __fastcall SetAAudioMixVol(longlong param_1, int param_2, int param_3, double param_4)
   ======================================================================== */

void SetAAudioMixVol(longlong param_1,int param_2,int param_3,double param_4)

{
                    /* 0x2760  64  SetAAudioMixVol */
  if (param_1 == 0) {
    param_1 = (&DAT_18001e140)[param_2];
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  *(double *)(param_1 + 0x1b8 + (longlong)param_3 * 8) = param_4;
  *(double *)(param_1 + 0x2b8 + (longlong)param_3 * 8) = param_4 * *(double *)(param_1 + 0x3b8);
                    /* WARNING: Could not recover jumptable at 0x0001800027d2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0xc40));
  return;
}



/* ========================================================================
   ENTRY: 1800027e0
   NAME : FUN_1800027e0
   SIG  : undefined __fastcall FUN_1800027e0(void * param_1, undefined8 param_2, undefined4 param_3)
   ======================================================================== */

void FUN_1800027e0(void *param_1,undefined8 param_2,undefined4 param_3)

{
  int iVar1;
  int iVar2;
  undefined8 uVar3;
  int iVar4;
  longlong lVar5;
  
  if (param_1 == (void *)0x0) {
    param_1 = DAT_18001e140;
  }
  FUN_180001fb0((longlong)param_1);
  iVar4 = 0;
  *(undefined4 *)((longlong)param_1 + 0x8c) = param_3;
  if (0 < *(int *)((longlong)param_1 + 0x94)) {
    do {
      iVar2 = *(int *)((longlong)param_1 + 0xde8);
      lVar5 = (longlong)iVar4;
      iVar1 = *(int *)((longlong)param_1 + lVar5 * 4 + 0xd68);
      if (iVar2 < iVar1) {
        iVar2 = (iVar1 / iVar2) * *(int *)((longlong)param_1 + 0x8c);
      }
      else {
        iVar2 = *(int *)((longlong)param_1 + 0x8c) / (iVar2 / iVar1);
      }
      *(int *)(*(longlong *)((longlong)param_1 + lVar5 * 8 + 0xc68) + 4) = iVar2;
      _aligned_free(*(void **)((longlong)param_1 + lVar5 * 8 + 0xdf0));
      uVar3 = malloc0(*(int *)((longlong)param_1 + 0x8c) << 4);
      iVar4 = iVar4 + 1;
      *(undefined8 *)((longlong)param_1 + lVar5 * 8 + 0xdf0) = uVar3;
      *(undefined8 *)(*(longlong *)((longlong)param_1 + lVar5 * 8 + 0xc68) + 0x10) = uVar3;
    } while (iVar4 < *(int *)((longlong)param_1 + 0x94));
  }
  FUN_180002140(param_1);
  return;
}



/* ========================================================================
   ENTRY: 1800028c0
   NAME : FUN_1800028c0
   SIG  : undefined __fastcall FUN_1800028c0(void * param_1, undefined8 param_2, undefined4 param_3)
   ======================================================================== */

void FUN_1800028c0(void *param_1,undefined8 param_2,undefined4 param_3)

{
  int iVar1;
  int iVar2;
  int iVar3;
  ulonglong uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  longlong lVar7;
  longlong lVar8;
  int iVar9;
  int iVar10;
  
  if (param_1 == (void *)0x0) {
    param_1 = DAT_18001e140;
  }
  FUN_180001fb0((longlong)param_1);
  *(undefined4 *)((longlong)param_1 + 0xde8) = param_3;
  uVar5 = DAT_180018dc8;
  iVar10 = 0;
  if (0 < *(int *)((longlong)param_1 + 0x94)) {
    do {
      lVar8 = (longlong)iVar10;
      destroy_resample(*(undefined8 *)((longlong)param_1 + lVar8 * 8 + 0xc68));
      _aligned_free(*(void **)((longlong)param_1 + lVar8 * 8 + 0xdf0));
      iVar1 = *(int *)((longlong)param_1 + 0xde8);
      iVar2 = *(int *)((longlong)param_1 + lVar8 * 4 + 0xd68);
      iVar3 = *(int *)((longlong)param_1 + 0x8c);
      if (iVar1 < iVar2) {
        uVar4 = (longlong)iVar2 % (longlong)iVar1;
        iVar9 = (iVar2 / iVar1) * iVar3;
      }
      else {
        iVar9 = iVar3 / (iVar1 / iVar2);
        uVar4 = (longlong)iVar3 % (longlong)(iVar1 / iVar2);
      }
      uVar6 = malloc0(iVar3 << 4,uVar4 & 0xffffffff);
      *(undefined8 *)((longlong)param_1 + lVar8 * 8 + 0xdf0) = uVar6;
      lVar7 = create_resample(iVar2 != iVar1,iVar9,0,uVar6,
                              *(undefined4 *)((longlong)param_1 + lVar8 * 4 + 0xd68),
                              *(undefined4 *)((longlong)param_1 + 0xde8),0,0,uVar5);
      iVar10 = iVar10 + 1;
      *(longlong *)((longlong)param_1 + lVar8 * 8 + 0xc68) = lVar7;
      *(undefined8 *)(lVar7 + 0x10) = *(undefined8 *)((longlong)param_1 + lVar8 * 8 + 0xdf0);
    } while (iVar10 < *(int *)((longlong)param_1 + 0x94));
  }
  FUN_180002140(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180002a30
   NAME : SetAudioMixWhat
   SIG  : undefined __fastcall SetAudioMixWhat(int param_1, int param_2, byte param_3, int param_4)
   ======================================================================== */

void SetAudioMixWhat(int param_1,int param_2,byte param_3,int param_4)

{
  longlong lVar1;
  uint uVar2;
  
                    /* 0x2a30  94  SetAudioMixWhat */
  lVar1 = *(longlong *)(&DAT_18001e160 + (longlong)param_1 * 8);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  if (param_2 == 0) {
    uVar2 = 1 << (param_3 & 0x1f);
    if (param_4 == 0) {
      uVar2 = ~uVar2 & *(uint *)(lVar1 + 0x28);
    }
    else {
      uVar2 = uVar2 | *(uint *)(lVar1 + 0x28);
    }
    *(uint *)(lVar1 + 0x28) = uVar2;
  }
  else if (param_2 == 1) {
    uVar2 = 1 << (param_3 & 0x1f);
    if (param_4 == 0) {
      *(uint *)(lVar1 + 0x2c) = ~uVar2 & *(uint *)(lVar1 + 0x2c);
    }
    else {
      *(uint *)(lVar1 + 0x2c) = uVar2 | *(uint *)(lVar1 + 0x2c);
    }
  }
                    /* WARNING: Could not recover jumptable at 0x000180002ab0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  return;
}



/* ========================================================================
   ENTRY: 180002ac0
   NAME : SetAudioMixSize
   SIG  : undefined __fastcall SetAudioMixSize(int param_1, undefined4 param_2)
   ======================================================================== */

void SetAudioMixSize(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x2ac0  91  SetAudioMixSize */
  lVar1 = *(longlong *)(&DAT_18001e160 + (longlong)param_1 * 8);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  *(undefined4 *)(lVar1 + 8) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180002b05. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  return;
}



/* ========================================================================
   ENTRY: 180002b10
   NAME : SetAudioMixVolume
   SIG  : undefined __fastcall SetAudioMixVolume(int param_1, double param_2)
   ======================================================================== */

void SetAudioMixVolume(int param_1,double param_2)

{
  longlong lVar1;
  undefined1 auVar2 [16];
  undefined1 auVar3 [16];
  undefined1 auVar4 [16];
  undefined1 auVar5 [16];
  undefined1 auVar6 [16];
  undefined1 auVar7 [16];
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  undefined1 auVar10 [16];
  undefined1 auVar11 [16];
  undefined1 auVar12 [16];
  undefined1 auVar13 [16];
  undefined1 auVar14 [16];
  undefined1 auVar15 [16];
  undefined1 auVar16 [16];
  undefined1 auVar17 [16];
  double dVar18;
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
  double dVar35;
  
                    /* 0x2b10  93  SetAudioMixVolume */
  lVar1 = *(longlong *)(&DAT_18001e160 + (longlong)param_1 * 8);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  *(double *)(lVar1 + 0x430) = param_2;
  dVar18 = *(double *)(lVar1 + 0x38) * param_2;
  dVar35 = *(double *)(lVar1 + 0x138) * param_2;
  auVar2._8_4_ = SUB84(dVar18,0);
  auVar2._0_8_ = *(double *)(lVar1 + 0x30) * param_2;
  auVar2._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x230) = auVar2;
  auVar34._8_4_ = SUB84(dVar35,0);
  auVar34._0_8_ = *(double *)(lVar1 + 0x130) * param_2;
  auVar34._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x330) = auVar34;
  dVar18 = *(double *)(lVar1 + 0x48) * param_2;
  dVar35 = *(double *)(lVar1 + 0x148) * param_2;
  auVar3._8_4_ = SUB84(dVar18,0);
  auVar3._0_8_ = *(double *)(lVar1 + 0x40) * param_2;
  auVar3._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x240) = auVar3;
  auVar19._8_4_ = SUB84(dVar35,0);
  auVar19._0_8_ = *(double *)(lVar1 + 0x140) * param_2;
  auVar19._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x340) = auVar19;
  dVar18 = *(double *)(lVar1 + 0x58) * param_2;
  dVar35 = *(double *)(lVar1 + 0x158) * param_2;
  auVar4._8_4_ = SUB84(dVar18,0);
  auVar4._0_8_ = *(double *)(lVar1 + 0x50) * param_2;
  auVar4._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x250) = auVar4;
  auVar20._8_4_ = SUB84(dVar35,0);
  auVar20._0_8_ = *(double *)(lVar1 + 0x150) * param_2;
  auVar20._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x350) = auVar20;
  dVar18 = *(double *)(lVar1 + 0x68) * param_2;
  dVar35 = *(double *)(lVar1 + 0x168) * param_2;
  auVar5._8_4_ = SUB84(dVar18,0);
  auVar5._0_8_ = *(double *)(lVar1 + 0x60) * param_2;
  auVar5._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x260) = auVar5;
  auVar21._8_4_ = SUB84(dVar35,0);
  auVar21._0_8_ = *(double *)(lVar1 + 0x160) * param_2;
  auVar21._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x360) = auVar21;
  dVar18 = *(double *)(lVar1 + 0x78) * param_2;
  dVar35 = *(double *)(lVar1 + 0x178) * param_2;
  auVar6._8_4_ = SUB84(dVar18,0);
  auVar6._0_8_ = *(double *)(lVar1 + 0x70) * param_2;
  auVar6._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x270) = auVar6;
  auVar22._8_4_ = SUB84(dVar35,0);
  auVar22._0_8_ = *(double *)(lVar1 + 0x170) * param_2;
  auVar22._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x370) = auVar22;
  dVar18 = *(double *)(lVar1 + 0x88) * param_2;
  dVar35 = *(double *)(lVar1 + 0x188) * param_2;
  auVar7._8_4_ = SUB84(dVar18,0);
  auVar7._0_8_ = *(double *)(lVar1 + 0x80) * param_2;
  auVar7._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x280) = auVar7;
  auVar23._8_4_ = SUB84(dVar35,0);
  auVar23._0_8_ = *(double *)(lVar1 + 0x180) * param_2;
  auVar23._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x380) = auVar23;
  dVar18 = *(double *)(lVar1 + 0x98) * param_2;
  dVar35 = *(double *)(lVar1 + 0x198) * param_2;
  auVar8._8_4_ = SUB84(dVar18,0);
  auVar8._0_8_ = *(double *)(lVar1 + 0x90) * param_2;
  auVar8._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x290) = auVar8;
  auVar24._8_4_ = SUB84(dVar35,0);
  auVar24._0_8_ = *(double *)(lVar1 + 400) * param_2;
  auVar24._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x390) = auVar24;
  dVar18 = *(double *)(lVar1 + 0xa8) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1a8) * param_2;
  auVar9._8_4_ = SUB84(dVar18,0);
  auVar9._0_8_ = *(double *)(lVar1 + 0xa0) * param_2;
  auVar9._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2a0) = auVar9;
  auVar25._8_4_ = SUB84(dVar35,0);
  auVar25._0_8_ = *(double *)(lVar1 + 0x1a0) * param_2;
  auVar25._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3a0) = auVar25;
  dVar18 = *(double *)(lVar1 + 0xb8) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1b8) * param_2;
  auVar10._8_4_ = SUB84(dVar18,0);
  auVar10._0_8_ = *(double *)(lVar1 + 0xb0) * param_2;
  auVar10._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2b0) = auVar10;
  auVar26._8_4_ = SUB84(dVar35,0);
  auVar26._0_8_ = *(double *)(lVar1 + 0x1b0) * param_2;
  auVar26._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3b0) = auVar26;
  dVar18 = *(double *)(lVar1 + 200) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1c8) * param_2;
  auVar11._8_4_ = SUB84(dVar18,0);
  auVar11._0_8_ = *(double *)(lVar1 + 0xc0) * param_2;
  auVar11._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2c0) = auVar11;
  auVar27._8_4_ = SUB84(dVar35,0);
  auVar27._0_8_ = *(double *)(lVar1 + 0x1c0) * param_2;
  auVar27._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3c0) = auVar27;
  dVar18 = *(double *)(lVar1 + 0xd8) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1d8) * param_2;
  auVar12._8_4_ = SUB84(dVar18,0);
  auVar12._0_8_ = *(double *)(lVar1 + 0xd0) * param_2;
  auVar12._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2d0) = auVar12;
  auVar28._8_4_ = SUB84(dVar35,0);
  auVar28._0_8_ = *(double *)(lVar1 + 0x1d0) * param_2;
  auVar28._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3d0) = auVar28;
  dVar18 = *(double *)(lVar1 + 0xe8) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1e8) * param_2;
  auVar13._8_4_ = SUB84(dVar18,0);
  auVar13._0_8_ = *(double *)(lVar1 + 0xe0) * param_2;
  auVar13._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2e0) = auVar13;
  auVar29._8_4_ = SUB84(dVar35,0);
  auVar29._0_8_ = *(double *)(lVar1 + 0x1e0) * param_2;
  auVar29._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3e0) = auVar29;
  dVar18 = *(double *)(lVar1 + 0xf8) * param_2;
  dVar35 = *(double *)(lVar1 + 0x1f8) * param_2;
  auVar14._8_4_ = SUB84(dVar18,0);
  auVar14._0_8_ = *(double *)(lVar1 + 0xf0) * param_2;
  auVar14._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x2f0) = auVar14;
  auVar30._8_4_ = SUB84(dVar35,0);
  auVar30._0_8_ = *(double *)(lVar1 + 0x1f0) * param_2;
  auVar30._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x3f0) = auVar30;
  dVar18 = *(double *)(lVar1 + 0x108) * param_2;
  dVar35 = *(double *)(lVar1 + 0x208) * param_2;
  auVar15._8_4_ = SUB84(dVar18,0);
  auVar15._0_8_ = *(double *)(lVar1 + 0x100) * param_2;
  auVar15._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x300) = auVar15;
  auVar31._8_4_ = SUB84(dVar35,0);
  auVar31._0_8_ = *(double *)(lVar1 + 0x200) * param_2;
  auVar31._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x400) = auVar31;
  dVar18 = *(double *)(lVar1 + 0x118) * param_2;
  dVar35 = *(double *)(lVar1 + 0x218) * param_2;
  auVar16._8_4_ = SUB84(dVar18,0);
  auVar16._0_8_ = *(double *)(lVar1 + 0x110) * param_2;
  auVar16._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x310) = auVar16;
  auVar32._8_4_ = SUB84(dVar35,0);
  auVar32._0_8_ = *(double *)(lVar1 + 0x210) * param_2;
  auVar32._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x410) = auVar32;
  dVar18 = *(double *)(lVar1 + 0x128) * param_2;
  dVar35 = *(double *)(lVar1 + 0x228) * param_2;
  auVar17._8_4_ = SUB84(dVar18,0);
  auVar17._0_8_ = *(double *)(lVar1 + 0x120) * param_2;
  auVar17._12_4_ = (int)((ulonglong)dVar18 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 800) = auVar17;
  auVar33._8_4_ = SUB84(dVar35,0);
  auVar33._0_8_ = *(double *)(lVar1 + 0x220) * param_2;
  auVar33._12_4_ = (int)((ulonglong)dVar35 >> 0x20);
  *(undefined1 (*) [16])(lVar1 + 0x420) = auVar33;
                    /* WARNING: Could not recover jumptable at 0x000180002d90. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  return;
}



/* ========================================================================
   ENTRY: 180002da0
   NAME : SetAudioMixVol
   SIG  : undefined __fastcall SetAudioMixVol(int param_1, int param_2, uint param_3, double param_4)
   ======================================================================== */

void SetAudioMixVol(int param_1,int param_2,uint param_3,double param_4)

{
  longlong lVar1;
  ulonglong uVar2;
  
                    /* 0x2da0  92  SetAudioMixVol */
  uVar2 = (ulonglong)param_3;
  lVar1 = *(longlong *)(&DAT_18001e160 + (longlong)param_1 * 8);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  if (param_2 == 0) {
    *(double *)(lVar1 + 0x30 + uVar2 * 8) = param_4;
    *(double *)(lVar1 + 0x230 + uVar2 * 8) = param_4 * *(double *)(lVar1 + 0x430);
  }
  else if (param_2 == 1) {
    *(double *)(lVar1 + 0x130 + uVar2 * 8) = param_4;
    *(double *)(lVar1 + 0x330 + uVar2 * 8) = param_4 * *(double *)(lVar1 + 0x430);
  }
                    /* WARNING: Could not recover jumptable at 0x000180002e21. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x438));
  return;
}



/* ========================================================================
   ENTRY: 180002e30
   NAME : FUN_180002e30
   SIG  : undefined __fastcall FUN_180002e30(void)
   ======================================================================== */

void FUN_180002e30(void)

{
  int *piVar1;
  int iVar2;
  undefined8 *puVar3;
  int *piVar4;
  uint uVar5;
  longlong lVar6;
  ulonglong uVar7;
  undefined *puVar8;
  int iVar9;
  longlong lVar10;
  int iVar11;
  
  piVar4 = DAT_1800205e0;
  puVar8 = PTR_DAT_18001e080;
  iVar11 = 0;
  puVar3 = *(undefined8 **)(DAT_1800205e0 + 0xe);
  *puVar3 = 0;
  puVar3[1] = 0;
  puVar3[2] = 0;
  puVar3[3] = 0;
  puVar3[4] = 0;
  puVar3[5] = 0;
  puVar3[6] = 0;
  puVar3[7] = 0;
  if (0 < *piVar4) {
    do {
      lVar6 = (longlong)iVar11;
      if ((piVar4[1] <= *(int *)(*(longlong *)(piVar4 + 2) + lVar6 * 4)) &&
         (*(int *)(*(longlong *)(piVar4 + 4) + lVar6 * 4) == 1)) {
        lVar10 = (longlong)(*(int *)(*(longlong *)(piVar4 + 10) + lVar6 * 4) - *(int *)(puVar8 + 4))
        ;
        *(undefined4 *)
         (*(longlong *)(*(longlong *)(piVar4 + 0x10) + lVar10 * 8) +
         (longlong)*(int *)(*(longlong *)(piVar4 + 0xe) + lVar10 * 4) * 4) =
             *(undefined4 *)(*(longlong *)(piVar4 + 0xc) + lVar6 * 4);
        *(undefined4 *)
         (*(longlong *)(*(longlong *)(piVar4 + 0x12) + lVar10 * 8) +
         (longlong)*(int *)(*(longlong *)(piVar4 + 0xe) + lVar10 * 4) * 4) =
             *(undefined4 *)(*(longlong *)(piVar4 + 2) + lVar6 * 4);
        piVar1 = (int *)(*(longlong *)(piVar4 + 0xe) + lVar10 * 4);
        *piVar1 = *piVar1 + 1;
      }
      iVar11 = iVar11 + 1;
    } while (iVar11 < *piVar4);
  }
  uVar7 = 0;
  do {
    iVar11 = *(int *)(*(longlong *)(piVar4 + 0xe) + uVar7 * 4);
    if (0 < iVar11) {
      iVar2 = *(int *)(puVar8 + 4);
      iVar9 = iVar2 + (int)uVar7;
      if (iVar9 < iVar2) {
        iVar9 = iVar9 * *(int *)(puVar8 + 0xc);
      }
      else if (iVar9 < *(int *)(puVar8 + 8) + iVar2) {
        iVar9 = iVar9 + (*(int *)(puVar8 + 0xc) + -1) * iVar2;
      }
      else {
        iVar9 = -1;
      }
      TXASetSipAllocDisps(iVar9,iVar11,*(undefined8 *)(*(longlong *)(piVar4 + 0x10) + uVar7 * 8),
                          *(undefined8 *)(*(longlong *)(piVar4 + 0x12) + uVar7 * 8));
      piVar4 = DAT_1800205e0;
      puVar8 = PTR_DAT_18001e080;
    }
    uVar5 = (int)uVar7 + 1;
    uVar7 = (ulonglong)uVar5;
  } while ((int)uVar5 < 0x10);
  return;
}



/* ========================================================================
   ENTRY: 180002f60
   NAME : alloc_analyzer
   SIG  : int __fastcall alloc_analyzer(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

int alloc_analyzer(int param_1,int param_2,undefined4 param_3)

{
  int *piVar1;
  int iVar2;
  longlong lVar3;
  int local_res20 [2];
  
                    /* 0x2f60  249  alloc_analyzer */
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x14));
  piVar1 = DAT_1800205e0;
  iVar2 = 0;
  if (**(int **)(DAT_1800205e0 + 2) != -1) {
    do {
      if (*DAT_1800205e0 <= iVar2) break;
      iVar2 = iVar2 + 1;
    } while ((*(int **)(DAT_1800205e0 + 2))[iVar2] != -1);
  }
  if (*DAT_1800205e0 <= iVar2) {
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x14));
    return -1;
  }
  lVar3 = (longlong)iVar2;
  *(int *)(*(longlong *)(DAT_1800205e0 + 4) + lVar3 * 4) = param_1;
  *(int *)(*(longlong *)(piVar1 + 6) + lVar3 * 4) = param_2;
  *(undefined4 *)(*(longlong *)(piVar1 + 8) + lVar3 * 4) = param_3;
  if (param_1 != 0) {
    if (param_1 == 1) {
      param_2 = *(int *)(PTR_DAT_18001e080 + 4) + param_2;
    }
    else if (param_1 == 2) {
      param_2 = *(int *)(PTR_DAT_18001e080 + 8) + *(int *)(PTR_DAT_18001e080 + 4) + param_2;
    }
    else {
      param_2 = -1;
    }
  }
  *(int *)(*(longlong *)(piVar1 + 10) + lVar3 * 4) = param_2;
  *(int *)(*(longlong *)(piVar1 + 2) + lVar3 * 4) = piVar1[1] + iVar2;
  XCreateAnalyzer(*(undefined4 *)(*(longlong *)(piVar1 + 2) + lVar3 * 4),local_res20,
                  *(undefined4 *)(*(longlong *)(piVar1 + 8) + lVar3 * 4),1,1,&DAT_1800176c4);
  FUN_180002e30();
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x14));
  if (-1 < local_res20[0]) {
    local_res20[0] = *(int *)(*(longlong *)(DAT_1800205e0 + 2) + lVar3 * 4);
  }
  return local_res20[0];
}



/* ========================================================================
   ENTRY: 1800030c0
   NAME : free_analyzer
   SIG  : undefined8 __fastcall free_analyzer(int param_1)
   ======================================================================== */

undefined8 free_analyzer(int param_1)

{
                    /* 0x30c0  257  free_analyzer */
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x50));
  *(undefined4 *)
   (*(longlong *)(DAT_1800205e0 + 8) + (longlong)(param_1 - *(int *)(DAT_1800205e0 + 4)) * 4) =
       0xffffffff;
  FUN_180002e30();
  DestroyAnalyzer(param_1);
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x50));
  return 0;
}



/* ========================================================================
   ENTRY: 180003120
   NAME : run_analyzer
   SIG  : undefined __fastcall run_analyzer(int param_1, undefined4 param_2)
   ======================================================================== */

void run_analyzer(int param_1,undefined4 param_2)

{
                    /* 0x3120  298  run_analyzer */
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x50));
  *(undefined4 *)
   (*(longlong *)(DAT_1800205e0 + 0x30) + (longlong)(param_1 - *(int *)(DAT_1800205e0 + 4)) * 4) =
       param_2;
  FUN_180002e30();
                    /* WARNING: Could not recover jumptable at 0x00018000316d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_1800205e0 + 0x50));
  return;
}



/* ========================================================================
   ENTRY: 180003180
   NAME : GetInboundBps
   SIG  : double __fastcall GetInboundBps(void)
   ======================================================================== */

double GetInboundBps(void)

{
  ULONGLONG UVar1;
  longlong lVar2;
  
                    /* 0x3180  31  GetInboundBps */
  UVar1 = GetTickCount64();
  LOCK();
  UNLOCK();
  LOCK();
  UNLOCK();
  LOCK();
  UNLOCK();
  if (DAT_18001fb98 == 0) {
    DAT_18001fbc8 = 0.0;
  }
  else {
    lVar2 = UVar1 - DAT_18001fb98;
    if (lVar2 < 1) {
      return DAT_18001fbc8;
    }
    DAT_18001fbc8 = ((double)(DAT_18001fbb0 - DAT_18001fbd0) * DAT_180018e18) / (double)lVar2;
  }
  LOCK();
  DAT_18001fbd0 = DAT_18001fbb0;
  UNLOCK();
  LOCK();
  DAT_18001fb98 = UVar1;
  UNLOCK();
  return DAT_18001fbc8;
}



/* ========================================================================
   ENTRY: 180003210
   NAME : GetOutboundBps
   SIG  : double __fastcall GetOutboundBps(void)
   ======================================================================== */

double GetOutboundBps(void)

{
  ULONGLONG UVar1;
  longlong lVar2;
  
                    /* 0x3210  34  GetOutboundBps */
  UVar1 = GetTickCount64();
  LOCK();
  UNLOCK();
  LOCK();
  UNLOCK();
  LOCK();
  UNLOCK();
  if (DAT_18001fba8 == 0) {
    DAT_18001fba0 = 0.0;
  }
  else {
    lVar2 = UVar1 - DAT_18001fba8;
    if (lVar2 < 1) {
      return DAT_18001fba0;
    }
    DAT_18001fba0 = ((double)(DAT_18001fbc0 - DAT_18001fbb8) * DAT_180018e18) / (double)lVar2;
  }
  LOCK();
  DAT_18001fbb8 = DAT_18001fbc0;
  UNLOCK();
  LOCK();
  DAT_18001fba8 = UVar1;
  UNLOCK();
  return DAT_18001fba0;
}



/* ========================================================================
   ENTRY: 1800032a0
   NAME : FUN_1800032a0
   SIG  : undefined * __fastcall FUN_1800032a0(void)
   ======================================================================== */

undefined * FUN_1800032a0(void)

{
  return &DAT_1800205e8;
}



/* ========================================================================
   ENTRY: 1800032b0
   NAME : FID_conflict:sprintf_s
   SIG  : int __cdecl FID_conflict:sprintf_s(wchar_t * _Dst, size_t _SizeInWords, wchar_t * _Format, ...)
   ======================================================================== */

/* Library Function - Multiple Matches With Different Base Names
    sprintf_s
    swprintf_s
   
   Libraries: Visual Studio 2017 Release, Visual Studio 2019 Release */

int __cdecl FID_conflict_sprintf_s(wchar_t *_Dst,size_t _SizeInWords,wchar_t *_Format,...)

{
  int iVar1;
  undefined8 *puVar2;
  undefined8 in_R9;
  undefined8 local_res20;
  
  local_res20 = in_R9;
  puVar2 = (undefined8 *)FUN_1800032a0();
  iVar1 = __stdio_common_vsprintf_s(*puVar2,_Dst,_SizeInWords,_Format,0,&local_res20);
  if (iVar1 < 0) {
    iVar1 = -1;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 180003310
   NAME : FUN_180003310
   SIG  : undefined __fastcall FUN_180003310(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180003310(void)

{
  undefined4 uVar1;
  undefined *puVar2;
  undefined *puVar3;
  int iVar4;
  uint uVar5;
  void *_Memory;
  undefined8 uVar6;
  undefined8 uVar7;
  HANDLE pvVar8;
  undefined4 uVar9;
  undefined1 auStack_f8 [32];
  int local_d8;
  void *local_d0;
  uint local_c8;
  undefined4 local_c4;
  undefined4 local_c0;
  int local_bc;
  wchar_t local_b8 [64];
  ulonglong local_38;
  
  puVar3 = PTR_DAT_18001e080;
  puVar2 = PTR_DAT_18001e078;
  local_38 = DAT_18001e000 ^ (ulonglong)auStack_f8;
  *(undefined4 *)(PTR_DAT_18001e078 + 0x4c) = 1;
  *(undefined4 *)(puVar2 + 0x20) = *(undefined4 *)(puVar3 + 0x2ac);
  uVar1 = *(undefined4 *)(puVar3 + 0x2a8);
  _Memory = calloc(0x20,1);
  iVar4 = getASIODriverString(_Memory);
  if (iVar4 == 0) {
    local_d8 = uVar1;
    local_d0 = _Memory;
    FID_conflict_sprintf_s
              (local_b8,0x80,
               (wchar_t *)
               "Initializing cmASIO with: \nblock size = %d\nsample rate = %d\ndriver name = \"%s\"\n\n"
               ,(ulonglong)*(uint *)(PTR_DAT_18001e078 + 0x20));
    OutputDebugStringA((LPCSTR)local_b8);
    local_bc = 0;
    iVar4 = getASIOBaseInputChannel(&local_bc);
    if (iVar4 != 0) {
      local_bc = 0;
    }
    local_c0 = 0;
    iVar4 = getASIOBaseOutputChannel(&local_c0);
    if (iVar4 != 0) {
      local_c0 = 0;
    }
    local_c4 = 0;
    iVar4 = getASIOInputMode(&local_c4);
    puVar2 = PTR_DAT_18001e078;
    if (iVar4 != 0) {
      local_c4 = 2;
    }
    *(undefined4 *)(PTR_DAT_18001e078 + 0x58) = local_c4;
    local_d0 = (void *)CONCAT44(local_d0._4_4_,local_c0);
    local_d8 = local_bc;
    uVar5 = prepareASIO(*(undefined4 *)(puVar2 + 0x20),uVar1,_Memory,FUN_1800036d0);
    FID_conflict_sprintf_s(local_b8,0x80,(wchar_t *)"prepareASIO return = %d",(ulonglong)uVar5);
    OutputDebugStringA((LPCSTR)local_b8);
    if (uVar5 == 0) {
      FID_conflict_sprintf_s
                (local_b8,0x80,(wchar_t *)"ASIO driver \"%s\" is loaded, initialized, and prepared."
                 ,_Memory);
      OutputDebugStringA((LPCSTR)local_b8);
      puVar2 = PTR_DAT_18001e078;
      *(undefined4 *)(PTR_DAT_18001e080 + 0xce4) = 1;
      *(undefined4 *)(puVar2 + 0x24) = 0;
      uVar6 = malloc0(*(int *)(puVar2 + 0x20) << 4);
      *(undefined8 *)(puVar2 + 0x18) = uVar6;
      puVar2 = PTR_DAT_18001e078;
      uVar6 = malloc0(*(int *)(PTR_DAT_18001e078 + 0x20) << 4);
      *(undefined8 *)(puVar2 + 0x10) = uVar6;
      local_c8 = 0;
      iVar4 = getASIOBlockNum(&local_c8);
      puVar2 = PTR_DAT_18001e078;
      if (iVar4 != 0) {
        local_c8 = 5;
      }
      *(uint *)(PTR_DAT_18001e078 + 0x48) = (uint)((local_c8 & 0xffff0000) != 0);
      local_c8 = local_c8 & 0xffff;
      local_d8 = *(int *)(puVar2 + 0x48);
      FID_conflict_sprintf_s(local_b8,0x80,(wchar_t *)"blockNum = %d\nlockMode = %d");
      OutputDebugStringA((LPCSTR)local_b8);
      uVar6 = DAT_180018dc8;
      uVar9 = (undefined4)DAT_180018dc8;
      local_d0 = (void *)DAT_180018dc8;
      iVar4 = *(int *)(PTR_DAT_18001e078 + 0x20);
      local_d8 = iVar4 * local_c8;
      uVar7 = create_rmatchV(iVar4,iVar4,uVar1,uVar1);
      puVar2 = PTR_DAT_18001e078;
      local_d0 = (void *)uVar6;
      *(undefined8 *)PTR_DAT_18001e078 = uVar7;
      iVar4 = *(int *)(puVar2 + 0x20);
      local_d8 = iVar4 * local_c8;
      uVar6 = create_rmatchV(iVar4,iVar4,uVar1,uVar1);
      puVar2 = PTR_DAT_18001e078;
      *(undefined8 *)(PTR_DAT_18001e078 + 8) = uVar6;
      forceRMatchVar(*(undefined8 *)puVar2,1,uVar9);
      forceRMatchVar(*(undefined8 *)(PTR_DAT_18001e078 + 8),1,uVar9);
      puVar2 = PTR_DAT_18001e078;
      pvVar8 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
      *(HANDLE *)(puVar2 + 0x28) = pvVar8;
      puVar2 = PTR_DAT_18001e078;
      pvVar8 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,1,1,(LPCWSTR)0x0);
      *(HANDLE *)(puVar2 + 0x30) = pvVar8;
      puVar2 = PTR_DAT_18001e078;
      *(undefined8 *)(PTR_DAT_18001e078 + 0x40) = 0;
      *(undefined8 *)(puVar2 + 0x38) = 0;
    }
    DAT_18001e1d0 = uVar5;
    free(_Memory);
  }
  else {
    free(_Memory);
  }
  return;
}



/* ========================================================================
   ENTRY: 180003670
   NAME : FUN_180003670
   SIG  : undefined __fastcall FUN_180003670(undefined8 param_1, int param_2, void * param_3)
   ======================================================================== */

void FUN_180003670(undefined8 param_1,int param_2,void *param_3)

{
  if (*(int *)(PTR_DAT_18001e078 + 0x24) != 0) {
    xrmatchIN(*(undefined8 *)(PTR_DAT_18001e078 + 8));
    if (*(int *)(PTR_DAT_18001e078 + 0x4c) == 0) {
      memset(param_3,0,(longlong)param_2 << 4);
      OutBound(0,param_2,param_3);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800036d0
   NAME : FUN_1800036d0
   SIG  : undefined __fastcall FUN_1800036d0(longlong param_1, longlong param_2, longlong param_3, longlong param_4)
   ======================================================================== */

void FUN_1800036d0(longlong param_1,longlong param_2,longlong param_3,longlong param_4)

{
  double dVar1;
  double dVar2;
  double dVar3;
  undefined *puVar4;
  DWORD DVar5;
  longlong lVar6;
  longlong lVar7;
  int iVar8;
  longlong lVar9;
  int iVar10;
  int iVar11;
  double dVar12;
  double dVar13;
  double dVar14;
  
  puVar4 = PTR_DAT_18001e078;
  dVar1 = DAT_180018d78;
  lVar6 = param_1;
  if ((*(int *)(PTR_DAT_18001e078 + 0x58) != 0) &&
     (lVar6 = param_2, *(int *)(PTR_DAT_18001e078 + 0x58) == 1)) {
    param_1 = param_2;
  }
  if (*(int *)(PTR_DAT_18001e078 + 0x48) == 0) {
    iVar8 = 0;
    iVar11 = 0;
    iVar10 = 0;
    if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
      do {
        lVar7 = (longlong)iVar10;
        iVar10 = iVar10 + 1;
        lVar9 = (longlong)iVar11;
        iVar11 = iVar11 + 2;
        *(double *)(*(longlong *)(puVar4 + 0x10) + lVar9 * 8) =
             (double)*(int *)(param_1 + lVar7 * 4) * dVar1;
        *(double *)(*(longlong *)(puVar4 + 0x10) + 8 + lVar9 * 8) =
             (double)*(int *)(lVar6 + lVar7 * 4) * dVar1;
      } while (iVar10 < *(int *)(puVar4 + 0x20));
    }
    xrmatchIN(*(undefined8 *)puVar4,*(undefined8 *)(puVar4 + 0x10));
    xrmatchOUT(*(undefined8 *)(PTR_DAT_18001e078 + 8),*(undefined8 *)(PTR_DAT_18001e078 + 0x18));
    puVar4 = PTR_DAT_18001e078;
    dVar3 = DAT_180018e60;
    dVar2 = DAT_180018e50;
    dVar1 = DAT_180018e48;
    iVar10 = 0;
    if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
      do {
        lVar6 = *(longlong *)(puVar4 + 0x18);
        dVar12 = *(double *)(lVar6 + (longlong)iVar8 * 8) * dVar2;
        dVar14 = *(double *)(lVar6 + 8 + (longlong)iVar8 * 8) * dVar2;
        dVar13 = dVar1;
        if ((dVar12 <= dVar1) && (dVar13 = dVar3, dVar3 <= dVar12)) {
          dVar13 = dVar12;
        }
        dVar12 = dVar1;
        if ((dVar14 <= dVar1) && (dVar12 = dVar3, dVar3 <= dVar14)) {
          dVar12 = dVar14;
        }
        lVar6 = (longlong)iVar10;
        iVar8 = iVar8 + 2;
        iVar10 = iVar10 + 1;
        *(int *)(param_3 + lVar6 * 4) = (int)dVar13;
        *(int *)(param_4 + lVar6 * 4) = (int)dVar12;
      } while (iVar10 < *(int *)(puVar4 + 0x20));
    }
  }
  else {
    DVar5 = WaitForSingleObject(*(HANDLE *)(PTR_DAT_18001e078 + 0x30),0);
    puVar4 = PTR_DAT_18001e078;
    dVar1 = DAT_180018d78;
    if (DVar5 != 0) {
      xrmatchOUT(*(undefined8 *)(PTR_DAT_18001e078 + 8),*(undefined8 *)(PTR_DAT_18001e078 + 0x18));
      puVar4 = PTR_DAT_18001e078;
      dVar3 = DAT_180018e60;
      dVar2 = DAT_180018e50;
      dVar1 = DAT_180018e48;
      iVar8 = 0;
      iVar11 = 0;
      iVar10 = 0;
      if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
        do {
          dVar12 = *(double *)(*(longlong *)(puVar4 + 0x18) + (longlong)iVar11 * 8) * dVar2;
          dVar14 = *(double *)(*(longlong *)(puVar4 + 0x18) + 8 + (longlong)iVar11 * 8) * dVar2;
          dVar13 = dVar1;
          if ((dVar12 <= dVar1) && (dVar13 = dVar3, dVar3 <= dVar12)) {
            dVar13 = dVar12;
          }
          dVar12 = dVar1;
          if ((dVar14 <= dVar1) && (dVar12 = dVar3, dVar3 <= dVar14)) {
            dVar12 = dVar14;
          }
          lVar7 = (longlong)iVar10;
          iVar11 = iVar11 + 2;
          iVar10 = iVar10 + 1;
          *(int *)(param_3 + lVar7 * 4) = (int)dVar13;
          *(int *)(param_4 + lVar7 * 4) = (int)dVar12;
        } while (iVar10 < *(int *)(puVar4 + 0x20));
      }
      DVar5 = WaitForSingleObject(*(HANDLE *)(puVar4 + 0x30),2);
      puVar4 = PTR_DAT_18001e078;
      dVar1 = DAT_180018d78;
      if (DVar5 != 0x102) {
        iVar10 = 0;
        if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
          do {
            lVar7 = (longlong)iVar10;
            iVar10 = iVar10 + 1;
            lVar9 = (longlong)iVar8;
            iVar8 = iVar8 + 2;
            *(double *)(*(longlong *)(puVar4 + 0x10) + lVar9 * 8) =
                 (double)*(int *)(param_1 + lVar7 * 4) * dVar1;
            *(double *)(*(longlong *)(puVar4 + 0x10) + 8 + lVar9 * 8) =
                 (double)*(int *)(lVar6 + lVar7 * 4) * dVar1;
          } while (iVar10 < *(int *)(puVar4 + 0x20));
        }
                    /* WARNING: Could not recover jumptable at 0x00018000399c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
        ReleaseSemaphore(*(HANDLE *)(puVar4 + 0x28),1,(LPLONG)0x0);
        return;
      }
      *(int *)(PTR_DAT_18001e078 + 0x38) = *(int *)(PTR_DAT_18001e078 + 0x38) + 1;
      return;
    }
    iVar8 = 0;
    iVar11 = 0;
    iVar10 = 0;
    if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
      do {
        lVar7 = (longlong)iVar10;
        iVar10 = iVar10 + 1;
        lVar9 = (longlong)iVar11;
        iVar11 = iVar11 + 2;
        *(double *)(*(longlong *)(puVar4 + 0x10) + lVar9 * 8) =
             (double)*(int *)(param_1 + lVar7 * 4) * dVar1;
        *(double *)(*(longlong *)(puVar4 + 0x10) + 8 + lVar9 * 8) =
             (double)*(int *)(lVar6 + lVar7 * 4) * dVar1;
      } while (iVar10 < *(int *)(puVar4 + 0x20));
    }
    ReleaseSemaphore(*(HANDLE *)(puVar4 + 0x28),1,(LPLONG)0x0);
    xrmatchOUT(*(undefined8 *)(PTR_DAT_18001e078 + 8),*(undefined8 *)(PTR_DAT_18001e078 + 0x18));
    puVar4 = PTR_DAT_18001e078;
    dVar3 = DAT_180018e60;
    dVar2 = DAT_180018e50;
    dVar1 = DAT_180018e48;
    iVar10 = 0;
    if (0 < *(int *)(PTR_DAT_18001e078 + 0x20)) {
      do {
        lVar6 = *(longlong *)(puVar4 + 0x18);
        dVar12 = *(double *)(lVar6 + (longlong)iVar8 * 8) * dVar2;
        dVar14 = *(double *)(lVar6 + 8 + (longlong)iVar8 * 8) * dVar2;
        dVar13 = dVar1;
        if ((dVar12 <= dVar1) && (dVar13 = dVar3, dVar3 <= dVar12)) {
          dVar13 = dVar12;
        }
        dVar12 = dVar1;
        if ((dVar14 <= dVar1) && (dVar12 = dVar3, dVar3 <= dVar14)) {
          dVar12 = dVar14;
        }
        lVar6 = (longlong)iVar10;
        iVar8 = iVar8 + 2;
        iVar10 = iVar10 + 1;
        *(int *)(param_3 + lVar6 * 4) = (int)dVar13;
        *(int *)(param_4 + lVar6 * 4) = (int)dVar12;
      } while (iVar10 < *(int *)(puVar4 + 0x20));
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180003ac0
   NAME : getCMAstate
   SIG  : ulonglong __fastcall getCMAstate(void)
   ======================================================================== */

ulonglong getCMAstate(void)

{
                    /* 0x3ac0  262  getCMAstate */
  if (DAT_18001e1d0 != 0) {
    return 0xffffffff;
  }
  return (ulonglong)(*(int *)(PTR_DAT_18001e080 + 0xce4) == 1);
}



/* ========================================================================
   ENTRY: 180003af0
   NAME : getCMAevents
   SIG  : undefined __fastcall getCMAevents(undefined4 * param_1, undefined8 param_2, undefined4 * param_3, undefined8 param_4)
   ======================================================================== */

void getCMAevents(undefined4 *param_1,undefined8 param_2,undefined4 *param_3,undefined8 param_4)

{
  undefined *puVar1;
  undefined4 local_38 [2];
  undefined8 local_30 [2];
  
                    /* 0x3af0  261  getCMAevents */
  if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
    local_38[0] = 0;
    local_30[0] = 0;
    getRMatchDiags(*(undefined8 *)PTR_DAT_18001e078,param_3,param_1,local_30,local_38,local_38);
    getRMatchDiags(*(undefined8 *)(PTR_DAT_18001e078 + 8),param_4,param_2,local_30,local_38,local_38
                  );
    puVar1 = PTR_DAT_18001e078;
    if (*(int *)(PTR_DAT_18001e078 + 0x48) != 0) {
      *param_1 = *(undefined4 *)(PTR_DAT_18001e078 + 0x38);
      *param_3 = *(undefined4 *)(puVar1 + 0x40);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180003bb0
   NAME : resetCMAevents
   SIG  : undefined __fastcall resetCMAevents(void)
   ======================================================================== */

void resetCMAevents(void)

{
  undefined *puVar1;
  
                    /* 0x3bb0  296  resetCMAevents */
  puVar1 = PTR_DAT_18001e078;
  if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
    *(undefined8 *)(PTR_DAT_18001e078 + 0x40) = 0;
    *(undefined8 *)(puVar1 + 0x38) = 0;
    resetRMatchDiags(*(undefined8 *)puVar1);
                    /* WARNING: Could not recover jumptable at 0x000180003bed. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    resetRMatchDiags(*(undefined8 *)(PTR_DAT_18001e078 + 8));
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180003c00
   NAME : FUN_180003c00
   SIG  : undefined __fastcall FUN_180003c00(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180003c00(void)

{
  undefined8 uVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  ulonglong uVar5;
  ulonglong uVar6;
  undefined8 uVar7;
  int iVar8;
  longlong lVar9;
  undefined *puVar10;
  int iVar11;
  longlong lVar12;
  longlong lVar13;
  int iVar14;
  double dVar15;
  undefined1 local_res8 [8];
  undefined4 uVar16;
  ulonglong uVar17;
  ulonglong uVar18;
  ulonglong uVar19;
  
  uVar6 = _UNK_180018e78;
  uVar5 = _DAT_180018e70;
  uVar4 = DAT_180018e00;
  uVar3 = DAT_180018d98;
  uVar2 = DAT_180018d90;
  uVar1 = DAT_180018d88;
  iVar11 = 0;
  if (0 < *(int *)(PTR_DAT_18001e080 + 4)) {
    do {
      lVar12 = (longlong)iVar11;
      iVar14 = 0;
      lVar13 = lVar12 * 0x40;
      puVar10 = PTR_DAT_18001e080;
      if (0 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          iVar8 = *(uint *)(puVar10 + 0xa0) << 6;
          uVar7 = malloc0((((*(uint *)(puVar10 + 0xa0) & 0x3ffffff) >> 0x19) +
                          iVar8 / 48000 + (iVar8 >> 0x1f)) * 0x10);
          puVar10 = PTR_DAT_18001e080;
          lVar9 = (longlong)iVar14;
          iVar14 = iVar14 + 1;
          *(undefined8 *)(PTR_DAT_18001e080 + lVar13 + lVar9 * 8 + 0xcf8) = uVar7;
        } while (iVar14 < *(int *)(puVar10 + 0xc));
      }
      uVar17 = uVar5;
      uVar7 = create_anb(0,*(undefined4 *)(puVar10 + lVar12 * 4 + 0x228),
                         *(undefined8 *)(puVar10 + lVar12 * 8 + 0xa8),
                         *(undefined8 *)(puVar10 + lVar12 * 8 + 0xa8),
                         (double)*(int *)(puVar10 + lVar12 * 4 + 0x1a8),uVar5,uVar5,uVar6,uVar3,
                         uVar4);
      puVar10 = PTR_DAT_18001e080;
      *(undefined8 *)(PTR_DAT_18001e080 + lVar13 + 0xd20) = uVar7;
      dVar15 = (double)*(int *)(puVar10 + lVar12 * 4 + 0x1a8);
      uVar18 = uVar5;
      uVar19 = uVar6;
      uVar7 = create_nob(0,*(undefined4 *)(puVar10 + lVar12 * 4 + 0x228),
                         *(undefined8 *)(puVar10 + lVar12 * 8 + 0xa8),
                         *(undefined8 *)(puVar10 + lVar12 * 8 + 0xa8),dVar15,
                         uVar17 & 0xffffffff00000000,uVar5,uVar6,uVar5,uVar6,uVar2,uVar3,uVar4);
      puVar10 = PTR_DAT_18001e080;
      uVar16 = (undefined4)((ulonglong)dVar15 >> 0x20);
      iVar14 = 0;
      *(undefined8 *)(PTR_DAT_18001e080 + lVar13 + 0xd28) = uVar7;
      if (0 < *(int *)(puVar10 + 0xc)) {
        do {
          iVar8 = *(int *)(puVar10 + 4);
          if (iVar11 < iVar8) {
            iVar8 = iVar11 * *(int *)(puVar10 + 0xc) + iVar14;
          }
          else if (iVar11 < *(int *)(puVar10 + 8) + iVar8) {
            iVar8 = (*(int *)(puVar10 + 0xc) + -1) * iVar8 + iVar11;
          }
          else {
            iVar8 = -1;
          }
          uVar19 = uVar19 & 0xffffffff00000000;
          uVar18 = uVar18 & 0xffffffff00000000;
          dVar15 = (double)CONCAT44((int)((ulonglong)dVar15 >> 0x20),48000);
          OpenChannel(iVar8,*(undefined4 *)(puVar10 + lVar12 * 4 + 0x228),0x1000,
                      *(undefined4 *)(puVar10 + lVar12 * 4 + 0x1a8),dVar15,
                      *(undefined4 *)(puVar10 + lVar13 + 0xcf0),uVar18,uVar19,uVar1,uVar2,0,uVar1,1)
          ;
          uVar16 = (undefined4)((ulonglong)dVar15 >> 0x20);
          iVar14 = iVar14 + 1;
          puVar10 = PTR_DAT_18001e080;
        } while (iVar14 < *(int *)(PTR_DAT_18001e080 + 0xc));
      }
      XCreateAnalyzer(iVar11,local_res8,0x40000,1,CONCAT44(uVar16,1),&DAT_1800176c4);
      iVar11 = iVar11 + 1;
    } while (iVar11 < *(int *)(PTR_DAT_18001e080 + 4));
  }
  return;
}



/* ========================================================================
   ENTRY: 180003ef0
   NAME : FUN_180003ef0
   SIG  : undefined __fastcall FUN_180003ef0(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180003ef0(void)

{
  undefined4 uVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  int *piVar6;
  undefined4 *puVar7;
  int iVar8;
  longlong lVar9;
  int iVar10;
  uint uVar11;
  ulonglong uVar12;
  undefined *puVar13;
  ulonglong uVar14;
  longlong lVar15;
  int iVar16;
  undefined1 auStackY_2c8 [32];
  undefined1 local_1e8 [16];
  undefined4 local_1d8 [64];
  ulonglong local_d8;
  
  uVar4 = _DAT_180018e90;
  uVar3 = DAT_180018da8;
  local_d8 = DAT_18001e000 ^ (ulonglong)auStackY_2c8;
  uVar14 = 0;
  iVar8 = *(int *)(PTR_DAT_18001e080 + 4);
  uVar12 = uVar14;
  if (0 < iVar8) {
    do {
      iVar10 = *(int *)(PTR_DAT_18001e080 + 0xc);
      lVar9 = (longlong)iVar10;
      if (0 < iVar10) {
        uVar1 = *(undefined4 *)(PTR_DAT_18001e080 + uVar12 * 0x40 + 0xcf0);
        puVar7 = local_1d8 + iVar10 * (int)uVar12;
        for (; lVar9 != 0; lVar9 = lVar9 + -1) {
          *puVar7 = uVar1;
          puVar7 = puVar7 + 1;
        }
      }
      uVar11 = (int)uVar12 + 1;
      uVar12 = (ulonglong)uVar11;
    } while ((int)uVar11 < iVar8);
  }
  puVar13 = PTR_DAT_18001e080;
  if (0 < *(int *)(PTR_DAT_18001e080 + 8)) {
    do {
      iVar8 = *(uint *)(puVar13 + 0xa4) << 6;
      iVar16 = (int)uVar14;
      iVar10 = *(int *)(puVar13 + 4) + iVar16;
      uVar5 = malloc0((((*(uint *)(puVar13 + 0xa4) & 0x3ffffff) >> 0x19) +
                      iVar8 / 48000 + (iVar8 >> 0x1f)) * 0x10);
      puVar13 = PTR_DAT_18001e080;
      lVar15 = (longlong)iVar16 * 0x58;
      *(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x10f8) = uVar5;
      iVar8 = *(uint *)(puVar13 + 0xa4) << 6;
      uVar5 = malloc0((((*(uint *)(puVar13 + 0xa4) & 0x3ffffff) >> 0x19) +
                      iVar8 / 48000 + (iVar8 >> 0x1f)) * 0x10);
      puVar13 = PTR_DAT_18001e080;
      *(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x1100) = uVar5;
      iVar8 = *(uint *)(puVar13 + 0xa4) << 6;
      uVar5 = malloc0((((*(uint *)(puVar13 + 0xa4) & 0x3ffffff) >> 0x19) +
                      iVar8 / 48000 + (iVar8 >> 0x1f)) * 0x10);
      puVar13 = PTR_DAT_18001e080;
      lVar9 = (longlong)iVar10;
      *(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x1108) = uVar5;
      uVar5 = uVar3;
      create_dexp(uVar14,0,*(undefined4 *)(puVar13 + lVar9 * 4 + 0x228));
      piVar6 = FUN_180001340(-1,iVar16,*(int *)(PTR_DAT_18001e080 + 0x2ac),
                             *(int *)(PTR_DAT_18001e080 + 0x2ac),
                             *(int *)(PTR_DAT_18001e080 + 0xc) * *(int *)(PTR_DAT_18001e080 + 4),0,
                             (1 << ((byte)(*(int *)(PTR_DAT_18001e080 + 0xc) *
                                          *(int *)(PTR_DAT_18001e080 + 4)) & 0x1f)) + -1,uVar4,uVar5
                             ,(longlong)local_1d8,*(int *)(PTR_DAT_18001e080 + 0x2a8),
                             SendAntiVOXData_exref,0,0,0,0);
      puVar13 = PTR_DAT_18001e080;
      *(int **)(PTR_DAT_18001e080 + lVar15 + 0x1138) = piVar6;
      iVar8 = *(int *)(puVar13 + 4);
      if (iVar10 < iVar8) {
        iVar8 = iVar10 * *(int *)(puVar13 + 0xc);
      }
      else if (iVar10 < *(int *)(puVar13 + 8) + iVar8) {
        iVar8 = (*(int *)(puVar13 + 0xc) + -1) * iVar8 + iVar10;
      }
      else {
        iVar8 = -1;
      }
      OpenChannel(iVar8,*(undefined4 *)(puVar13 + lVar9 * 4 + 0x228),0x1000,
                  *(undefined4 *)(puVar13 + lVar9 * 4 + 0x1a8));
      XCreateAnalyzer(iVar10,local_1e8,0x40000,1);
      uVar5 = *(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x10f8);
      uVar1 = *(undefined4 *)(PTR_DAT_18001e080 + lVar15 + 0x10f4);
      lVar9 = malloc0(0x90);
      *(undefined4 *)(lVar9 + 8) = uVar1;
      *(undefined8 *)(lVar9 + 0x10) = uVar5;
      *(undefined8 *)(lVar9 + 0x18) = uVar5;
      *(undefined8 *)(lVar9 + 0x20) = 0x3ff0000000000000;
      *(undefined8 *)(lVar9 + 0x28) = 0x3ff0000000000000;
      *(undefined4 *)(lVar9 + 0x30) = 0;
      *(undefined4 *)(lVar9 + 0x34) = 0x32;
      InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(lVar9 + 0x40),0x9c4);
      InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(lVar9 + 0x68),0x9c4);
      puVar13 = PTR_DAT_18001e080;
      *(longlong *)(PTR_DAT_18001e080 + lVar15 + 0x1120) = lVar9;
      uVar5 = *(undefined8 *)(puVar13 + lVar15 + 0x10f8);
      uVar5 = create_eer(0,*(undefined4 *)(puVar13 + lVar15 + 0x10f4),uVar5,uVar5);
      puVar13 = PTR_DAT_18001e080;
      uVar1 = *(undefined4 *)(PTR_DAT_18001e080 + lVar15 + 0x10f4);
      *(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x1128) = uVar5;
      uVar5 = *(undefined8 *)(puVar13 + 0xcc8);
      puVar7 = (undefined4 *)malloc0(0x28);
      puVar7[3] = 2;
      puVar7[2] = uVar1;
      *puVar7 = 0;
      puVar7[1] = 1;
      puVar7[4] = 3;
      *(undefined8 *)(puVar7 + 8) = uVar5;
      uVar5 = malloc0(puVar7[3] * puVar7[2] * 0x10);
      *(undefined8 *)(puVar7 + 6) = uVar5;
      puVar13 = PTR_DAT_18001e080;
      iVar8 = *(int *)(PTR_DAT_18001e080 + lVar15 + 0x10f4);
      iVar10 = *(int *)(PTR_DAT_18001e080 + lVar15 + 0x10f0);
      *(undefined4 **)(PTR_DAT_18001e080 + lVar15 + 0x1130) = puVar7;
      uVar5 = *(undefined8 *)(puVar13 + lVar15 + 0x10f8);
      uVar2 = *(undefined8 *)(puVar13 + lVar15 + 0x1108);
      piVar6 = (int *)malloc0(0x140);
      *piVar6 = iVar16;
      piVar6[1] = 1;
      piVar6[2] = 0;
      piVar6[3] = iVar10;
      piVar6[4] = iVar8;
      *(undefined8 *)(piVar6 + 10) = uVar5;
      *(undefined8 *)(piVar6 + 0xc) = uVar2;
      *(undefined8 *)(piVar6 + 0xe) = uVar5;
      piVar6[0x10] = 0;
      piVar6[0x12] = 0;
      piVar6[0x13] = 0x40790000;
      piVar6[0x1a] = 0x47ae147b;
      piVar6[0x1b] = 0x3f747ae1;
      piVar6[0x14] = 0;
      piVar6[0x15] = 0x3ff00000;
      piVar6[0x16] = 0;
      piVar6[0x17] = 0x3ff00000;
      piVar6[0x18] = 0x14;
      piVar6[0x19] = 0;
      InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar6 + 0x46),0x9c4);
      puVar13 = PTR_DAT_18001e080;
      uVar14 = (ulonglong)(iVar16 + 1U);
      piVar6[0x41] = 0;
      piVar6[0x42] = 0;
      piVar6[0x43] = 0;
      piVar6[0x44] = 0;
      (&DAT_1800202e0)[*piVar6] = piVar6;
    } while ((int)(iVar16 + 1U) < *(int *)(puVar13 + 8));
  }
  return;
}



/* ========================================================================
   ENTRY: 1800045a0
   NAME : FUN_1800045a0
   SIG  : undefined __fastcall FUN_1800045a0(void)
   ======================================================================== */

void FUN_1800045a0(void)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined4 uVar1;
  char cVar2;
  int *piVar3;
  undefined8 uVar4;
  HANDLE pvVar5;
  int iVar6;
  undefined *puVar7;
  int iVar8;
  int *piVar9;
  ulonglong uVar10;
  int iVar11;
  uint uVar12;
  int iVar13;
  ulonglong uVar14;
  void *_ArgList;
  undefined8 in_stack_ffffffffffffffa8;
  
  uVar14 = 0;
  puVar7 = PTR_DAT_18001e080;
  uVar10 = uVar14;
  if (0 < *(int *)PTR_DAT_18001e080) {
    do {
      iVar11 = (int)uVar10;
      _ArgList = (void *)(longlong)iVar11;
      InitializeCriticalSectionAndSpinCount
                ((LPCRITICAL_SECTION)(puVar7 + (longlong)_ArgList * 0x28 + 0x6b0),0x9c4);
      iVar6 = *(int *)(PTR_DAT_18001e080 + (longlong)_ArgList * 4 + 0x228);
      iVar8 = *(int *)(PTR_DAT_18001e080 + (longlong)_ArgList * 4 + 0x1c);
      iVar13 = (*(int *)(PTR_DAT_18001e080 + 0x9c) << 6) / 48000;
      piVar3 = (int *)malloc0(0x90);
      puVar7 = PTR_DAT_18001e080;
      *(int **)(PTR_DAT_18001e080 + (longlong)_ArgList * 8 + 0x5b0) = piVar3;
      *(int **)(puVar7 + (longlong)_ArgList * 8 + 0x4b0) = piVar3;
      *(int **)(puVar7 + (longlong)_ArgList * 8 + 0x3b0) = piVar3;
      *(int **)(puVar7 + (longlong)_ArgList * 8 + 0x2b0) = piVar3;
      *piVar3 = iVar11;
      piVar3[2] = iVar13;
      if (iVar13 <= iVar8) {
        iVar13 = iVar8;
      }
      piVar3[0xc] = 1;
      piVar3[0xb] = 1;
      piVar3[1] = iVar8;
      piVar3[3] = iVar6;
      piVar3[4] = iVar13;
      piVar3[5] = iVar13 * 3;
      uVar4 = malloc0(iVar13 * 0x30);
      piVar3[8] = 0;
      piVar3[9] = 0;
      *(undefined8 *)(piVar3 + 6) = uVar4;
      piVar3[10] = 0;
      pvVar5 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1000,(LPCWSTR)0x0);
      *(HANDLE *)(piVar3 + 0xe) = pvVar5;
      InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar3 + 0x1a),0x9c4);
      InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar3 + 0x10),0x9c4);
      _beginthread(FUN_180006f20,0,_ArgList);
      iVar6 = *(uint *)(PTR_DAT_18001e080 + 0x9c) << 6;
      uVar4 = malloc0((((*(uint *)(PTR_DAT_18001e080 + 0x9c) & 0x3ffffff) >> 0x19) +
                      iVar6 / 48000 + (iVar6 >> 0x1f)) * 0x10);
      puVar7 = PTR_DAT_18001e080;
      *(undefined8 *)(PTR_DAT_18001e080 + (longlong)_ArgList * 8 + 0xa8) = uVar4;
      uVar10 = (ulonglong)(iVar11 + 1U);
    } while ((int)(iVar11 + 1U) < *(int *)puVar7);
  }
  FUN_180003c00();
  FUN_180003ef0();
  puVar7 = PTR_DAT_18001e080;
  iVar6 = *(int *)(PTR_DAT_18001e080 + 4);
  iVar8 = *(int *)(PTR_DAT_18001e080 + 0xc);
  cVar2 = (char)iVar8;
  uVar1 = *(undefined4 *)(PTR_DAT_18001e080 + 8);
  uVar10 = uVar14;
  if (0 < iVar6) {
    do {
      iVar13 = (int)uVar10;
      if (0 < iVar8) {
        uVar10 = uVar14;
        do {
          iVar11 = (int)uVar10;
          uVar12 = iVar11 + 1;
          uVar10 = (ulonglong)uVar12;
          *(undefined4 *)(puVar7 + (longlong)(iVar8 * iVar13 + iVar11) * 4 + 0xbb0) =
               *(undefined4 *)(puVar7 + (longlong)iVar13 * 0x40 + 0xcf0);
          iVar8 = *(int *)(puVar7 + 0xc);
        } while ((int)uVar12 < iVar8);
      }
      uVar10 = (ulonglong)(iVar13 + 1U);
    } while ((int)(iVar13 + 1U) < *(int *)(puVar7 + 4));
  }
  iVar8 = *(int *)(puVar7 + 8);
  uVar10 = uVar14;
  if (0 < iVar8) {
    do {
      iVar13 = (int)uVar10;
      *(undefined4 *)
       (puVar7 + (longlong)(*(int *)(puVar7 + 0xc) * *(int *)(puVar7 + 4) + iVar13) * 4 + 0xbb0) =
           *(undefined4 *)(puVar7 + (longlong)iVar13 * 0x58 + 0x10f0);
      iVar8 = *(int *)(puVar7 + 8);
      uVar10 = (ulonglong)(iVar13 + 1U);
    } while ((int)(iVar13 + 1U) < iVar8);
  }
  FUN_180001340(0,0,*(int *)(puVar7 + 0x2ac),*(int *)(puVar7 + 0x2ac),
                *(int *)(puVar7 + 0xc) * *(int *)(puVar7 + 4) + iVar8,0,
                (1 << (cVar2 * (char)iVar6 + (char)uVar1 & 0x1fU)) + -1,DAT_180018dc8,
                in_stack_ffffffffffffffa8,(longlong)(puVar7 + 0xbb0),*(int *)(puVar7 + 0x2a8),
                *(undefined8 *)(puVar7 + 0xcc0),0,DAT_180018d88,0,DAT_180018d88);
  FUN_180003310();
  DAT_18001f840 = (undefined4 *)malloc0(0x3368);
  lpCriticalSection = (LPCRITICAL_SECTION)(DAT_18001f840 + 0xcd0);
  *DAT_18001f840 = 0;
  InitializeCriticalSectionAndSpinCount(lpCriticalSection,0x9c4);
  piVar3 = (int *)malloc0(0x78);
  DAT_1800205e0 = piVar3;
  *piVar3 = 0x20;
  piVar3[1] = 0x28;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 4) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 6) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 8) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 10) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 2) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 0xc) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x40);
  *(undefined8 *)(piVar3 + 0xe) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  *(undefined8 *)(piVar3 + 0x10) = uVar4;
  piVar3 = DAT_1800205e0;
  uVar4 = malloc0(0x80);
  piVar9 = DAT_1800205e0;
  *(undefined8 *)(piVar3 + 0x12) = uVar4;
  uVar10 = uVar14;
  do {
    uVar4 = malloc0(*piVar9 << 2);
    piVar3 = DAT_1800205e0;
    *(undefined8 *)(*(longlong *)(DAT_1800205e0 + 0x10) + uVar10 * 8) = uVar4;
    uVar4 = malloc0(*piVar3 << 2);
    piVar9 = DAT_1800205e0;
    *(undefined8 *)(*(longlong *)(DAT_1800205e0 + 0x12) + uVar10 * 8) = uVar4;
    uVar10 = uVar10 + 1;
  } while (uVar10 != 0x10);
  if (0 < *piVar9) {
    do {
      iVar6 = (int)uVar14;
      uVar14 = (ulonglong)(iVar6 + 1U);
      *(undefined4 *)(*(longlong *)(piVar9 + 2) + (longlong)iVar6 * 4) = 0xffffffff;
      *(undefined4 *)(*(longlong *)(piVar9 + 0xc) + (longlong)iVar6 * 4) = 1;
    } while ((int)(iVar6 + 1U) < *piVar9);
  }
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar9 + 0x14),0x9c4);
  *(int **)(PTR_DAT_18001e080 + 0xce8) = DAT_1800205e0;
  return;
}



/* ========================================================================
   ENTRY: 180004a60
   NAME : FUN_180004a60
   SIG  : undefined __fastcall FUN_180004a60(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180004a60(void)

{
  void *pvVar1;
  int iVar2;
  longlong lVar3;
  longlong lVar4;
  int iVar5;
  int iVar6;
  undefined1 auStack_b8 [32];
  wchar_t local_98 [64];
  ulonglong local_18;
  
  local_18 = DAT_18001e000 ^ (ulonglong)auStack_b8;
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_1800205e0 + 0x50));
  lVar3 = 0;
  do {
    _aligned_free(*(void **)(*(longlong *)((longlong)DAT_1800205e0 + 0x40) + lVar3 * 8));
    _aligned_free(*(void **)(*(longlong *)((longlong)DAT_1800205e0 + 0x48) + lVar3 * 8));
    lVar3 = lVar3 + 1;
  } while (lVar3 != 0x10);
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x48));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x40));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x38));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x30));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 8));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x28));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x20));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x18));
  _aligned_free(*(void **)((longlong)DAT_1800205e0 + 0x10));
  _aligned_free(DAT_1800205e0);
  pvVar1 = DAT_18001f840;
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_18001f840 + 0x3340));
  _aligned_free(pvVar1);
  if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
    CloseHandle(*(HANDLE *)(PTR_DAT_18001e078 + 0x28));
    CloseHandle(*(HANDLE *)(PTR_DAT_18001e078 + 0x30));
    destroy_rmatchV(*(undefined8 *)PTR_DAT_18001e078);
    destroy_rmatchV(*(undefined8 *)(PTR_DAT_18001e078 + 8));
    _aligned_free(*(void **)(PTR_DAT_18001e078 + 0x10));
    _aligned_free(*(void **)(PTR_DAT_18001e078 + 0x18));
    unloadASIO();
    FID_conflict_sprintf_s(local_98,0x80,(wchar_t *)"cmASIO has been destroyed");
    OutputDebugStringA((LPCSTR)local_98);
  }
  FUN_1800016f0((void *)0x0,0);
  iVar5 = 0;
  if (0 < *(int *)(PTR_DAT_18001e080 + 8)) {
    do {
      lVar3 = (&DAT_1800202e0)[iVar5];
      _aligned_free(*(void **)(lVar3 + 0xa8));
      _aligned_free(*(void **)(lVar3 + 200));
      DeleteCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
      lVar3 = (longlong)iVar5 * 0x58;
      pvVar1 = *(void **)(PTR_DAT_18001e080 + lVar3 + 0x1130);
      _aligned_free(*(void **)((longlong)pvVar1 + 0x18));
      _aligned_free(pvVar1);
      destroy_eer(*(undefined8 *)(PTR_DAT_18001e080 + lVar3 + 0x1128));
      pvVar1 = *(void **)(PTR_DAT_18001e080 + lVar3 + 0x1120);
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x68));
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x40));
      _aligned_free(pvVar1);
      DestroyAnalyzer(*(int *)(PTR_DAT_18001e080 + 4) + iVar5);
      iVar6 = *(int *)(PTR_DAT_18001e080 + 4);
      iVar2 = iVar6 + iVar5;
      if (iVar2 < iVar6) {
        iVar2 = iVar2 * *(int *)(PTR_DAT_18001e080 + 0xc);
      }
      else if (iVar2 < *(int *)(PTR_DAT_18001e080 + 8) + iVar6) {
        iVar2 = iVar2 + (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar6;
      }
      else {
        iVar2 = -1;
      }
      CloseChannel(iVar2);
      FUN_1800016f0(*(void **)(PTR_DAT_18001e080 + lVar3 + 0x1138),-1);
      destroy_dexp(iVar5);
      lVar4 = 0;
      do {
        _aligned_free(*(void **)(PTR_DAT_18001e080 + lVar3 + lVar4 * 8 + 0x10f8));
        lVar4 = lVar4 + 1;
      } while (lVar4 != 3);
      iVar5 = iVar5 + 1;
    } while (iVar5 < *(int *)(PTR_DAT_18001e080 + 8));
  }
  iVar5 = 0;
  if (0 < *(int *)(PTR_DAT_18001e080 + 4)) {
    do {
      DestroyAnalyzer(iVar5);
      iVar6 = 0;
      if (0 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          iVar2 = *(int *)(PTR_DAT_18001e080 + 4);
          if (iVar5 < iVar2) {
            iVar2 = iVar5 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar6;
          }
          else if (iVar5 < *(int *)(PTR_DAT_18001e080 + 8) + iVar2) {
            iVar2 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar2 + iVar5;
          }
          else {
            iVar2 = -1;
          }
          CloseChannel(iVar2);
          iVar6 = iVar6 + 1;
        } while (iVar6 < *(int *)(PTR_DAT_18001e080 + 0xc));
      }
      lVar3 = (longlong)iVar5 * 0x40;
      destroy_nob(*(undefined8 *)(PTR_DAT_18001e080 + lVar3 + 0xd28));
      destroy_anb(*(undefined8 *)(PTR_DAT_18001e080 + lVar3 + 0xd20));
      iVar6 = 0;
      if (0 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          _aligned_free(*(void **)(PTR_DAT_18001e080 + lVar3 + (longlong)iVar6 * 8 + 0xcf8));
          iVar6 = iVar6 + 1;
        } while (iVar6 < *(int *)(PTR_DAT_18001e080 + 0xc));
      }
      iVar5 = iVar5 + 1;
    } while (iVar5 < *(int *)(PTR_DAT_18001e080 + 4));
  }
  iVar5 = 0;
  if (0 < *(int *)PTR_DAT_18001e080) {
    do {
      lVar3 = (longlong)iVar5;
      DeleteCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (lVar3 * 5 + 0xd6) * 8));
      pvVar1 = *(void **)(PTR_DAT_18001e080 + lVar3 * 8 + 0x2b0);
      LOCK();
      *(uint *)((longlong)pvVar1 + 0x30) = *(uint *)((longlong)pvVar1 + 0x30) & 0xfffffffe;
      UNLOCK();
      EnterCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x68));
      EnterCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x40));
      Sleep(0x19);
      LOCK();
      *(uint *)((longlong)pvVar1 + 0x2c) = *(uint *)((longlong)pvVar1 + 0x2c) & 0xfffffffe;
      UNLOCK();
      ReleaseSemaphore(*(HANDLE *)((longlong)pvVar1 + 0x38),1,(LPLONG)0x0);
      LeaveCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x40));
      Sleep(2);
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x40));
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x68));
      CloseHandle(*(HANDLE *)((longlong)pvVar1 + 0x38));
      _aligned_free(*(void **)((longlong)pvVar1 + 0x18));
      _aligned_free(pvVar1);
      _aligned_free(*(void **)(PTR_DAT_18001e080 + lVar3 * 8 + 0xa8));
      iVar5 = iVar5 + 1;
    } while (iVar5 < *(int *)PTR_DAT_18001e080);
  }
  return;
}



/* ========================================================================
   ENTRY: 180004fa0
   NAME : xcmaster
   SIG  : undefined __fastcall xcmaster(int param_1)
   ======================================================================== */

void xcmaster(int param_1)

{
  int *piVar1;
  double *pdVar2;
  int iVar3;
  longlong lVar4;
  void *pvVar5;
  uint *puVar6;
  DWORD DVar7;
  int iVar8;
  undefined8 uVar9;
  longlong lVar10;
  longlong lVar11;
  int iVar12;
  int iVar13;
  uint uVar14;
  undefined *puVar15;
  uint uVar16;
  uint uVar17;
  longlong lVar18;
  bool bVar19;
  double dVar20;
  undefined1 local_res8 [8];
  
                    /* 0x4fa0  300  xcmaster */
  lVar11 = (longlong)param_1;
  EnterCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + lVar11 * 0x28 + 0x6b0));
  iVar12 = *(int *)(PTR_DAT_18001e080 + 4);
  if (param_1 < iVar12) {
    FUN_180011e90(param_1,0,(undefined8 *)(PTR_DAT_18001e080 + 0xa8));
    FUN_180006440(param_1,*(longlong *)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8),
                  *(int *)(PTR_DAT_18001e080 + lVar11 * 4 + 0x228));
    lVar18 = lVar11 * 0x40;
    xanb(*(undefined8 *)(PTR_DAT_18001e080 + lVar18 + 0xd20));
    xnob(*(undefined8 *)(PTR_DAT_18001e080 + lVar18 + 0xd28));
    puVar15 = PTR_DAT_18001e080;
    uVar9 = *(undefined8 *)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8);
    iVar12 = *(int *)(PTR_DAT_18001e080 + lVar18 + 0xd18);
    do {
      LOCK();
      iVar8 = *(int *)(puVar15 + lVar18 + 0xd18);
      bVar19 = iVar12 == iVar8;
      if (bVar19) {
        *(int *)(puVar15 + lVar18 + 0xd18) = iVar12;
        iVar8 = iVar12;
      }
      iVar12 = iVar8;
      UNLOCK();
    } while (!bVar19);
    Spectrum0(iVar12,param_1,0,0,uVar9);
    iVar12 = 0;
    puVar15 = PTR_DAT_18001e080;
    if (0 < **(int **)(PTR_DAT_18001e080 + 0xce8)) {
      do {
        lVar4 = *(longlong *)(puVar15 + 0xce8);
        lVar10 = (longlong)iVar12;
        iVar8 = *(int *)(*(longlong *)(lVar4 + 8) + lVar10 * 4);
        if ((-1 < iVar8) && (*(int *)(*(longlong *)(lVar4 + 0x28) + lVar10 * 4) == param_1)) {
          lVar4 = *(longlong *)(lVar4 + 0x30);
          iVar13 = *(int *)(lVar4 + lVar10 * 4);
          do {
            piVar1 = (int *)(lVar4 + lVar10 * 4);
            LOCK();
            iVar3 = *piVar1;
            bVar19 = iVar13 == iVar3;
            if (bVar19) {
              *piVar1 = iVar13;
              iVar3 = iVar13;
            }
            iVar13 = iVar3;
            UNLOCK();
          } while (!bVar19);
          if (iVar13 == 0) {
LAB_180005125:
            uVar9 = 0;
          }
          else {
            lVar4 = *(longlong *)(*(longlong *)(PTR_DAT_18001e080 + 0xce8) + 0x10);
            iVar13 = *(int *)(lVar4 + lVar10 * 4);
            do {
              piVar1 = (int *)(lVar4 + lVar10 * 4);
              LOCK();
              iVar3 = *piVar1;
              bVar19 = iVar13 == iVar3;
              if (bVar19) {
                *piVar1 = iVar13;
                iVar3 = iVar13;
              }
              iVar13 = iVar3;
              UNLOCK();
            } while (!bVar19);
            if (iVar13 != 0) goto LAB_180005125;
            uVar9 = 1;
          }
          Spectrum0(uVar9,iVar8,0,0,*(undefined8 *)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8));
          puVar15 = PTR_DAT_18001e080;
        }
        iVar12 = iVar12 + 1;
      } while (iVar12 < **(int **)(puVar15 + 0xce8));
    }
    iVar12 = 0;
    if (0 < *(int *)(puVar15 + 0xc)) {
      do {
        iVar8 = *(int *)(puVar15 + 4);
        if (param_1 < iVar8) {
          iVar8 = param_1 * *(int *)(puVar15 + 0xc) + iVar12;
        }
        else if (param_1 < *(int *)(puVar15 + 8) + iVar8) {
          iVar8 = (*(int *)(puVar15 + 0xc) + -1) * iVar8 + param_1;
        }
        else {
          iVar8 = -1;
        }
        fexchange0(iVar8,*(undefined8 *)(puVar15 + lVar11 * 8 + 0xa8),
                   *(undefined8 *)(puVar15 + lVar18 + (longlong)iVar12 * 8 + 0xcf8),local_res8);
        iVar12 = iVar12 + 1;
        puVar15 = PTR_DAT_18001e080;
      } while (iVar12 < *(int *)(PTR_DAT_18001e080 + 0xc));
    }
    FUN_180011e90(param_1,1,(undefined8 *)(puVar15 + lVar18 + 0xcf8));
    iVar12 = 0;
    if (0 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
      do {
        iVar8 = *(int *)(PTR_DAT_18001e080 + 4);
        if (param_1 < iVar8) {
          iVar8 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar12;
        }
        else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar8) {
          iVar8 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar8 + param_1;
        }
        else {
          iVar8 = -1;
        }
        FUN_180001840(0,0,iVar8,
                      *(void **)(PTR_DAT_18001e080 + lVar18 + (longlong)iVar12 * 8 + 0xcf8));
        iVar8 = 0;
        if (0 < *(int *)(PTR_DAT_18001e080 + 8)) {
          do {
            iVar13 = *(int *)(PTR_DAT_18001e080 + 4);
            if (param_1 < iVar13) {
              iVar13 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar12;
            }
            else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar13) {
              iVar13 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar13 + param_1;
            }
            else {
              iVar13 = -1;
            }
            FUN_180001840(*(longlong *)(PTR_DAT_18001e080 + (longlong)iVar8 * 0x58 + 0x1138),-1,
                          iVar13,*(void **)(PTR_DAT_18001e080 +
                                           ((longlong)iVar12 + lVar11 * 8) * 8 + 0xcf8));
            iVar8 = iVar8 + 1;
          } while (iVar8 < *(int *)(PTR_DAT_18001e080 + 8));
        }
        iVar12 = iVar12 + 1;
      } while (iVar12 < *(int *)(PTR_DAT_18001e080 + 0xc));
    }
  }
  else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar12) {
    iVar12 = param_1 - iVar12;
    if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
      pvVar5 = *(void **)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8);
      if (*(int *)(PTR_DAT_18001e078 + 0x48) == 0) {
        xrmatchOUT(*(undefined8 *)PTR_DAT_18001e078,pvVar5);
        iVar13 = 0;
        iVar8 = *(int *)(PTR_DAT_18001e078 + 0x20);
        if (0 < iVar8 * 2) {
          do {
            lVar18 = (longlong)iVar13;
            iVar13 = iVar13 + 2;
            dVar20 = *(double *)((longlong)pvVar5 + lVar18 * 8 + 8) +
                     *(double *)((longlong)pvVar5 + lVar18 * 8);
            pdVar2 = (double *)((longlong)pvVar5 + lVar18 * 8);
            *pdVar2 = dVar20;
            pdVar2[1] = dVar20;
          } while (iVar13 < iVar8 * 2);
        }
      }
      else {
        DVar7 = WaitForSingleObject(*(HANDLE *)(PTR_DAT_18001e078 + 0x28),2);
        puVar15 = PTR_DAT_18001e078;
        if (DVar7 == 0x102) {
          *(int *)(PTR_DAT_18001e078 + 0x40) = *(int *)(PTR_DAT_18001e078 + 0x40) + 1;
        }
        else {
          memcpy(pvVar5,*(void **)(PTR_DAT_18001e078 + 0x10),
                 (longlong)*(int *)(PTR_DAT_18001e078 + 0x20) << 4);
          iVar13 = 0;
          iVar8 = *(int *)(puVar15 + 0x20) * 2;
          if (0 < iVar8) {
            do {
              lVar18 = (longlong)iVar13;
              iVar13 = iVar13 + 2;
              dVar20 = *(double *)((longlong)pvVar5 + lVar18 * 8 + 8) +
                       *(double *)((longlong)pvVar5 + lVar18 * 8);
              pdVar2 = (double *)((longlong)pvVar5 + lVar18 * 8);
              *pdVar2 = dVar20;
              pdVar2[1] = dVar20;
            } while (iVar13 < iVar8);
          }
          ReleaseSemaphore(*(HANDLE *)(puVar15 + 0x30),1,(LPLONG)0x0);
        }
      }
    }
    puVar15 = PTR_DAT_18001e080;
    lVar18 = (longlong)iVar12 * 0x58;
    uVar17 = *(uint *)(PTR_DAT_18001e080 + lVar18 + 0x1140);
    do {
      LOCK();
      uVar16 = *(uint *)(puVar15 + lVar18 + 0x1140);
      bVar19 = uVar17 == uVar16;
      if (bVar19) {
        *(uint *)(puVar15 + lVar18 + 0x1140) = uVar17 & 1;
        uVar16 = uVar17;
      }
      uVar17 = uVar16;
      UNLOCK();
    } while (!bVar19);
    if (uVar17 != 0) {
      if (*(code **)(PTR_DAT_18001e080 + 0xcd8) == (code *)0x0) {
        memset(*(void **)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8),0,
               (longlong)*(int *)(PTR_DAT_18001e080 + lVar11 * 4 + 0x228) << 4);
      }
      else {
        (**(code **)(PTR_DAT_18001e080 + 0xcd8))
                  (*(undefined4 *)(PTR_DAT_18001e080 + lVar11 * 4 + 0x228));
      }
    }
    FUN_180011e90(param_1,0,(undefined8 *)(PTR_DAT_18001e080 + 0xa8));
    xdexp(iVar12);
    iVar8 = *(int *)(PTR_DAT_18001e080 + 4);
    if (param_1 < iVar8) {
      iVar8 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc);
    }
    else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar8) {
      iVar8 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar8 + param_1;
    }
    else {
      iVar8 = -1;
    }
    fexchange0(iVar8,*(undefined8 *)(PTR_DAT_18001e080 + lVar11 * 8 + 0xa8),
               *(undefined8 *)(PTR_DAT_18001e080 + lVar18 + 0x10f8),local_res8);
    FUN_180012ff0(iVar12);
    FUN_180011e90(param_1,1,(undefined8 *)(PTR_DAT_18001e080 + lVar18 + 0x10f8));
    iVar8 = *(int *)(PTR_DAT_18001e080 + 4);
    if (param_1 < iVar8) {
      iVar8 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc);
    }
    else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar8) {
      iVar8 = param_1 + (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar8;
    }
    else {
      iVar8 = -1;
    }
    FUN_180001840(0,0,iVar8,*(void **)(PTR_DAT_18001e080 + lVar18 + 0x1108));
    FUN_180014770(*(uint **)(PTR_DAT_18001e080 + lVar18 + 0x1120));
    xeer(*(undefined8 *)(PTR_DAT_18001e080 + lVar18 + 0x1128));
    puVar15 = PTR_DAT_18001e080;
    puVar6 = *(uint **)(PTR_DAT_18001e080 + ((longlong)iVar12 + 0x32) * 0x58);
    uVar17 = *puVar6;
    do {
      LOCK();
      uVar16 = *puVar6;
      bVar19 = uVar17 == uVar16;
      if (bVar19) {
        *puVar6 = uVar17 & 1;
        uVar16 = uVar17;
      }
      uVar17 = uVar16;
      UNLOCK();
    } while (!bVar19);
    uVar16 = puVar6[2];
    if (uVar17 == 0) {
      pvVar5 = *(void **)(puVar15 + lVar18 + 0x10f8);
      if (*(void **)(puVar6 + 6) != pvVar5) {
        memcpy(*(void **)(puVar6 + 6),pvVar5,(longlong)(int)uVar16 << 4);
      }
    }
    else {
      uVar17 = 0;
      iVar12 = 0;
      bVar19 = 0 < (int)uVar16;
      uVar16 = uVar17;
      if (bVar19) {
        do {
          uVar16 = puVar6[4];
          do {
            LOCK();
            uVar14 = puVar6[4];
            bVar19 = uVar16 == uVar14;
            if (bVar19) {
              puVar6[4] = uVar16;
              uVar14 = uVar16;
            }
            uVar16 = uVar14;
            UNLOCK();
          } while (!bVar19);
          if (uVar16 != 0) {
            iVar8 = 0;
            uVar14 = 1;
            do {
              if ((uVar16 & uVar14) != 0) {
                lVar4 = *(longlong *)(puVar15 + (longlong)iVar8 * 8 + lVar18 + 0x10f8);
                *(undefined8 *)(*(longlong *)(puVar6 + 6) + (longlong)(int)(uVar17 * 2) * 8) =
                     *(undefined8 *)(lVar4 + (longlong)(iVar12 * 2) * 8);
                *(undefined8 *)(*(longlong *)(puVar6 + 6) + 8 + (longlong)(int)(uVar17 * 2) * 8) =
                     *(undefined8 *)(lVar4 + 8 + (longlong)(iVar12 * 2) * 8);
                uVar16 = uVar16 & ~uVar14;
                uVar17 = uVar17 + 1;
              }
              iVar8 = iVar8 + 1;
              uVar14 = uVar14 << 1 | (uint)((int)uVar14 < 0);
            } while (uVar16 != 0);
          }
          iVar12 = iVar12 + 1;
        } while (iVar12 < (int)puVar6[2]);
        (**(code **)(puVar6 + 8))(puVar6[1],uVar17,*(undefined8 *)(puVar6 + 6));
        goto LAB_18000566e;
      }
    }
    (**(code **)(puVar6 + 8))(puVar6[1],uVar16,*(undefined8 *)(puVar6 + 6));
  }
  else {
    FUN_180011e90(param_1,0,(undefined8 *)(PTR_DAT_18001e080 + 0xa8));
  }
LAB_18000566e:
  LeaveCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (lVar11 * 5 + 0xd6) * 8));
  return;
}



/* ========================================================================
   ENTRY: 1800056a0
   NAME : SendpOutboundRx
   SIG  : undefined __fastcall SendpOutboundRx(undefined8 param_1)
   ======================================================================== */

void SendpOutboundRx(undefined8 param_1)

{
                    /* 0x56a0  58  SendpOutboundRx */
  *(undefined8 *)(PTR_DAT_18001e080 + 0xcc0) = param_1;
  *(undefined8 *)(DAT_18001e140 + 0xef0) = param_1;
  return;
}



/* ========================================================================
   ENTRY: 1800056c0
   NAME : SendpOutboundTx
   SIG  : undefined __fastcall SendpOutboundTx(undefined8 param_1)
   ======================================================================== */

void SendpOutboundTx(undefined8 param_1)

{
  undefined *puVar1;
  
                    /* 0x56c0  61  SendpOutboundTx */
  puVar1 = PTR_DAT_18001e080;
  *(undefined8 *)(PTR_DAT_18001e080 + 0xcc8) = param_1;
  *(undefined8 *)(*(longlong *)(puVar1 + 0x1130) + 0x20) = param_1;
  return;
}



/* ========================================================================
   ENTRY: 1800056e0
   NAME : SendpOutboundTCIRxIQ
   SIG  : undefined __fastcall SendpOutboundTCIRxIQ(undefined8 param_1)
   ======================================================================== */

void SendpOutboundTCIRxIQ(undefined8 param_1)

{
                    /* 0x56e0  60  SendpOutboundTCIRxIQ */
  *(undefined8 *)(PTR_DAT_18001e080 + 0xcd0) = param_1;
  return;
}



/* ========================================================================
   ENTRY: 1800056f0
   NAME : SendpInboundTCITxAudio
   SIG  : undefined __fastcall SendpInboundTCITxAudio(undefined8 param_1)
   ======================================================================== */

void SendpInboundTCITxAudio(undefined8 param_1)

{
                    /* 0x56f0  57  SendpInboundTCITxAudio */
  *(undefined8 *)(PTR_DAT_18001e080 + 0xcd8) = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180005700
   NAME : SetRXTCIRun
   SIG  : undefined __fastcall SetRXTCIRun(undefined4 param_1)
   ======================================================================== */

void SetRXTCIRun(undefined4 param_1)

{
                    /* 0x5700  203  SetRXTCIRun */
  LOCK();
  *(undefined4 *)(PTR_DAT_18001e080 + 0xce0) = param_1;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180005710
   NAME : SetTXTCIAudioRun
   SIG  : undefined __fastcall SetTXTCIAudioRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetTXTCIAudioRun(int param_1,undefined4 param_2)

{
                    /* 0x5710  225  SetTXTCIAudioRun */
  LOCK();
  *(undefined4 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1140) = param_2;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180005730
   NAME : SetRunPanadapter
   SIG  : undefined __fastcall SetRunPanadapter(int param_1, undefined4 param_2)
   ======================================================================== */

void SetRunPanadapter(int param_1,undefined4 param_2)

{
                    /* 0x5730  206  SetRunPanadapter */
  LOCK();
  *(undefined4 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x40 + 0xd18) = param_2;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180005750
   NAME : SetXcmInrate
   SIG  : undefined __fastcall SetXcmInrate(int param_1, int param_2)
   ======================================================================== */

void SetXcmInrate(int param_1,int param_2)

{
  longlong lVar1;
  longlong lVar2;
  undefined *puVar3;
  DWORD DVar4;
  int iVar5;
  int iVar6;
  void *_ArgList;
  longlong lVar7;
  longlong lVar8;
  
                    /* 0x5750  241  SetXcmInrate */
  _ArgList = (void *)(longlong)param_1;
  EnterCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (longlong)_ArgList * 0x28 + 0x6b0));
  puVar3 = PTR_DAT_18001e080;
  lVar7 = (longlong)_ArgList * 4 + 0x1a8;
  if (*(int *)(PTR_DAT_18001e080 + lVar7) != param_2) {
    *(int *)(PTR_DAT_18001e080 + lVar7) = param_2;
    iVar6 = (param_2 << 6) / 48000;
    lVar1 = (longlong)_ArgList * 4 + 0x228;
    *(int *)(puVar3 + lVar1) = iVar6;
    lVar8 = *(longlong *)(puVar3 + (longlong)_ArgList * 8 + 0x2b0);
    LOCK();
    *(uint *)(lVar8 + 0x30) = *(uint *)(lVar8 + 0x30) & 0xfffffffe;
    UNLOCK();
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar8 + 0x68));
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar8 + 0x40));
    Sleep(0x19);
    LOCK();
    *(uint *)(lVar8 + 0x2c) = *(uint *)(lVar8 + 0x2c) & 0xfffffffe;
    UNLOCK();
    ReleaseSemaphore(*(HANDLE *)(lVar8 + 0x38),1,(LPLONG)0x0);
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar8 + 0x40));
    Sleep(2);
    lVar2 = *(longlong *)(PTR_DAT_18001e080 + (longlong)_ArgList * 8 + 0x5b0);
    memset(*(void **)(lVar2 + 0x18),0,(longlong)*(int *)(lVar2 + 0x14) << 4);
    *(undefined8 *)(lVar2 + 0x20) = 0;
    *(undefined4 *)(lVar2 + 0x28) = 0;
    do {
      DVar4 = WaitForSingleObject(*(HANDLE *)(lVar2 + 0x38),1);
    } while (DVar4 == 0);
    *(int *)(lVar8 + 0xc) = iVar6;
    LOCK();
    *(uint *)(lVar8 + 0x2c) = *(uint *)(lVar8 + 0x2c) | 1;
    UNLOCK();
    _beginthread(FUN_180006f20,0,_ArgList);
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar8 + 0x68));
    LOCK();
    *(uint *)(lVar8 + 0x30) = *(uint *)(lVar8 + 0x30) | 1;
    UNLOCK();
    iVar6 = *(int *)(PTR_DAT_18001e080 + 4);
    if (param_1 < iVar6) {
      lVar8 = (longlong)_ArgList * 0x40;
      pSetRCVRANBBuffsize(*(undefined8 *)(PTR_DAT_18001e080 + lVar8 + 0xd20),
                          *(undefined4 *)(PTR_DAT_18001e080 + lVar1));
      pSetRCVRANBSamplerate(*(undefined8 *)(PTR_DAT_18001e080 + lVar8 + 0xd20),param_2);
      pSetRCVRNOBBuffsize(*(undefined8 *)(PTR_DAT_18001e080 + lVar8 + 0xd28),
                          *(undefined4 *)(PTR_DAT_18001e080 + lVar1));
      pSetRCVRNOBSamplerate(*(undefined8 *)(PTR_DAT_18001e080 + lVar8 + 0xd28),param_2);
      iVar6 = 0;
      if (0 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          iVar5 = *(int *)(PTR_DAT_18001e080 + 4);
          if (param_1 < iVar5) {
            iVar5 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar6;
          }
          else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar5) {
            iVar5 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar5 + param_1;
          }
          else {
            iVar5 = -1;
          }
          SetInputSamplerate(iVar5,param_2);
          iVar5 = *(int *)(PTR_DAT_18001e080 + 4);
          if (param_1 < iVar5) {
            iVar5 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar6;
          }
          else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar5) {
            iVar5 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar5 + param_1;
          }
          else {
            iVar5 = -1;
          }
          SetInputBuffsize(iVar5,*(undefined4 *)(PTR_DAT_18001e080 + lVar1));
          iVar6 = iVar6 + 1;
        } while (iVar6 < *(int *)(PTR_DAT_18001e080 + 0xc));
      }
      if (param_1 == 0) {
        SetSiphonInsize(0,*(undefined4 *)(PTR_DAT_18001e080 + 0x228));
      }
      SetIVACiqSizeAndRate
                (param_1,*(int *)(PTR_DAT_18001e080 + lVar1),*(int *)(PTR_DAT_18001e080 + lVar7));
    }
    else {
      iVar5 = *(int *)(PTR_DAT_18001e080 + 8);
      if (param_1 < iVar6 + iVar5) {
        if (param_1 < iVar6 + iVar5) {
          iVar5 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar6 + param_1;
        }
        else {
          iVar5 = -1;
        }
        SetInputSamplerate(iVar5,param_2);
        iVar5 = *(int *)(PTR_DAT_18001e080 + 4);
        if (param_1 < iVar5) {
          iVar5 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc);
        }
        else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar5) {
          iVar5 = param_1 + (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar5;
        }
        else {
          iVar5 = -1;
        }
        SetInputBuffsize(iVar5,*(undefined4 *)(PTR_DAT_18001e080 + lVar1));
        SetDEXPSize(param_1 - iVar6,*(undefined4 *)(PTR_DAT_18001e080 + lVar1));
        SetDEXPRate(param_1 - iVar6,(double)param_2);
        SetIVACmicSize(0,*(int *)(PTR_DAT_18001e080 + lVar1));
        SetIVACmicRate(0,param_2);
        SetIVACmicSize(1,*(int *)(PTR_DAT_18001e080 + lVar1));
        SetIVACmicRate(1,param_2);
      }
      else {
        lVar7 = (longlong)((param_1 - iVar5) - iVar6);
        pSetRCVRANBBuffsize(*(undefined8 *)(PTR_DAT_18001e088 + (lVar7 + 0x33) * 0x10),
                            *(undefined4 *)(PTR_DAT_18001e080 + lVar1));
        pSetRCVRANBSamplerate(*(undefined8 *)(PTR_DAT_18001e088 + (lVar7 + 0x33) * 0x10),param_2);
        pSetRCVRNOBBuffsize(*(undefined8 *)(PTR_DAT_18001e088 + lVar7 * 0x10 + 0x338),
                            *(undefined4 *)(PTR_DAT_18001e080 + lVar1));
        pSetRCVRNOBSamplerate(*(undefined8 *)(PTR_DAT_18001e088 + lVar7 * 0x10 + 0x338),param_2);
      }
    }
  }
                    /* WARNING: Could not recover jumptable at 0x000180005b51. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection
            ((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + ((longlong)_ArgList * 5 + 0xd6) * 8));
  return;
}



/* ========================================================================
   ENTRY: 180005b60
   NAME : SetCMAudioOutrate
   SIG  : undefined __fastcall SetCMAudioOutrate(int param_1, int param_2)
   ======================================================================== */

void SetCMAudioOutrate(int param_1,int param_2)

{
  undefined4 uVar1;
  uint uVar2;
  undefined *puVar3;
  void *pvVar4;
  undefined8 uVar5;
  ulonglong uVar6;
  
                    /* 0x5b60  97  SetCMAudioOutrate */
  EnterCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (longlong)param_1 * 0x28 + 0x6b0));
  puVar3 = PTR_DAT_18001e080;
  *(int *)(PTR_DAT_18001e080 + 0x2a8) = param_2;
  uVar2 = (param_2 << 6) / 48000;
  uVar6 = (ulonglong)uVar2;
  *(uint *)(puVar3 + 0x2ac) = uVar2;
  FUN_1800028c0((void *)0x0,uVar6,param_2);
  FUN_1800027e0((void *)0x0,uVar6,*(undefined4 *)(PTR_DAT_18001e080 + 0x2ac));
  pvVar4 = DAT_18001e140;
  uVar1 = *(undefined4 *)(PTR_DAT_18001e080 + 0x2ac);
  FUN_180001fb0((longlong)DAT_18001e140);
  *(undefined4 *)((longlong)pvVar4 + 0x90) = uVar1;
  _aligned_free(*(void **)((longlong)pvVar4 + 0x1a0));
  uVar5 = malloc0(*(int *)((longlong)pvVar4 + 0x90) << 4);
  *(undefined8 *)((longlong)pvVar4 + 0x1a0) = uVar5;
  FUN_180002140(pvVar4);
                    /* WARNING: Could not recover jumptable at 0x000180005c48. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + ((longlong)param_1 * 5 + 0xd6) * 8))
  ;
  return;
}



/* ========================================================================
   ENTRY: 180005c50
   NAME : SetRcvrChannelOutrate
   SIG  : undefined __fastcall SetRcvrChannelOutrate(int param_1, uint param_2, uint param_3)
   ======================================================================== */

void SetRcvrChannelOutrate(int param_1,uint param_2,uint param_3)

{
  undefined8 *puVar1;
  int iVar2;
  int iVar3;
  undefined4 uVar4;
  undefined *puVar5;
  int iVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  undefined *puVar9;
  int iVar10;
  ulonglong uVar11;
  longlong lVar12;
  longlong lVar13;
  void *pvVar14;
  longlong lVar15;
  int iVar16;
  
                    /* 0x5c50  205  SetRcvrChannelOutrate */
  lVar13 = (longlong)param_1;
  EnterCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (lVar13 * 5 + 0xd6) * 8));
  puVar9 = PTR_DAT_18001e080;
  iVar16 = 0;
  lVar15 = lVar13 * 0x40;
  *(int *)(PTR_DAT_18001e080 + lVar15 + 0xcf4) = (int)(param_2 << 6) / 48000;
  *(uint *)(puVar9 + lVar15 + 0xcf0) = param_2;
  uVar8 = DAT_180018dc8;
  if (0 < *(int *)(puVar9 + 0xc)) {
    do {
      iVar10 = *(int *)(puVar9 + 4);
      if (param_1 < iVar10) {
        iVar10 = param_1 * *(int *)(puVar9 + 0xc) + iVar16;
      }
      else if (param_1 < *(int *)(puVar9 + 8) + iVar10) {
        iVar10 = (*(int *)(puVar9 + 0xc) + -1) * iVar10 + param_1;
      }
      else {
        iVar10 = -1;
      }
      SetOutputSamplerate(iVar10,param_2);
      iVar10 = *(int *)(PTR_DAT_18001e080 + 4);
      if (param_1 < iVar10) {
        iVar10 = param_1 * *(int *)(PTR_DAT_18001e080 + 0xc) + iVar16;
      }
      else if (param_1 < *(int *)(PTR_DAT_18001e080 + 8) + iVar10) {
        iVar10 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar10 + param_1;
      }
      else {
        iVar10 = -1;
      }
      SetAAudioMixState((void *)0x0,0,(byte)iVar10,0);
      pvVar14 = DAT_18001e140;
      lVar12 = (longlong)iVar10;
      *(uint *)((longlong)DAT_18001e140 + lVar12 * 4 + 0xd68) = param_2;
      destroy_resample(*(undefined8 *)((longlong)pvVar14 + lVar12 * 8 + 0xc68));
      iVar2 = *(int *)((longlong)pvVar14 + 0xde8);
      iVar3 = *(int *)((longlong)pvVar14 + lVar12 * 4 + 0xd68);
      if (iVar2 < iVar3) {
        iVar6 = (iVar3 / iVar2) * *(int *)((longlong)pvVar14 + 0x8c);
      }
      else {
        iVar6 = *(int *)((longlong)pvVar14 + 0x8c) / (iVar2 / iVar3);
      }
      uVar7 = create_resample(iVar3 != iVar2,iVar6,0,
                              *(undefined8 *)((longlong)pvVar14 + lVar12 * 8 + 0xdf0),iVar3,iVar2,0,
                              0,uVar8);
      *(undefined8 *)((longlong)pvVar14 + lVar12 * 8 + 0xc68) = uVar7;
      SetAAudioMixState((void *)0x0,0,(byte)iVar10,param_3);
      iVar16 = iVar16 + 1;
      puVar9 = PTR_DAT_18001e080;
    } while (iVar16 < *(int *)(PTR_DAT_18001e080 + 0xc));
  }
  SetIVACaudioRate(param_1,param_2);
  SetIVACaudioSize(param_1,*(undefined4 *)(PTR_DAT_18001e080 + lVar15 + 0xcf4));
  uVar11 = (ulonglong)param_2;
  FUN_180013b90(param_1,param_2);
  puVar5 = PTR_DAT_18001e098;
  puVar9 = PTR_DAT_18001e080;
  if ((-1 < param_1) && (param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
    uVar4 = *(undefined4 *)(PTR_DAT_18001e080 + lVar15 + 0xcf4);
    puVar1 = (undefined8 *)(PTR_DAT_18001e098 + lVar13 * 0x30 + 8);
    *(undefined4 *)(PTR_DAT_18001e098 + lVar13 * 0x30 + 0x18) = uVar4;
    if ((void *)*puVar1 != (void *)0x0) {
      FUN_1800027e0((void *)*puVar1,uVar11,uVar4);
      uVar4 = *(undefined4 *)(puVar5 + lVar13 * 0x30 + 0x18);
      pvVar14 = *(void **)(puVar5 + lVar13 * 0x30 + 8);
      if (pvVar14 == (void *)0x0) {
        pvVar14 = DAT_18001e140;
      }
      FUN_180001fb0((longlong)pvVar14);
      *(undefined4 *)((longlong)pvVar14 + 0x90) = uVar4;
      _aligned_free(*(void **)((longlong)pvVar14 + 0x1a0));
      uVar8 = malloc0(*(int *)((longlong)pvVar14 + 0x90) << 4);
      *(undefined8 *)((longlong)pvVar14 + 0x1a0) = uVar8;
      FUN_180002140(pvVar14);
      FUN_1800139e0(param_1);
      puVar9 = PTR_DAT_18001e080;
    }
  }
                    /* WARNING: Could not recover jumptable at 0x000180005f3b. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(puVar9 + lVar13 * 0x28 + 0x6b0));
  return;
}



/* ========================================================================
   ENTRY: 180005f50
   NAME : SetXmtrChannelOutrate
   SIG  : undefined __fastcall SetXmtrChannelOutrate(int param_1, int param_2, uint param_3)
   ======================================================================== */

void SetXmtrChannelOutrate(int param_1,int param_2,uint param_3)

{
  undefined8 *puVar1;
  int iVar2;
  longlong lVar3;
  undefined8 uVar4;
  undefined *puVar5;
  int iVar6;
  int iVar7;
  undefined8 uVar8;
  undefined *puVar9;
  int iVar10;
  int iVar11;
  int iVar12;
  longlong lVar13;
  longlong lVar14;
  longlong lVar15;
  double dVar16;
  double dVar17;
  
                    /* 0x5f50  242  SetXmtrChannelOutrate */
  lVar13 = (longlong)param_1;
  iVar12 = *(int *)(PTR_DAT_18001e080 + 4);
  iVar10 = iVar12 + param_1;
  if (iVar10 < iVar12) {
    iVar12 = iVar10 * *(int *)(PTR_DAT_18001e080 + 0xc);
  }
  else if (iVar10 < *(int *)(PTR_DAT_18001e080 + 8) + iVar12) {
    iVar12 = (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar12 + iVar10;
  }
  else {
    iVar12 = -1;
  }
  iVar2 = (param_2 << 6) / 48000;
  EnterCriticalSection((LPCRITICAL_SECTION)(PTR_DAT_18001e080 + (longlong)iVar10 * 0x28 + 0x6b0));
  puVar9 = PTR_DAT_18001e080;
  lVar15 = lVar13 * 0x58;
  *(int *)(PTR_DAT_18001e080 + lVar15 + 0x10f0) = param_2;
  *(int *)(puVar9 + lVar15 + 0x10f4) = iVar2;
  iVar11 = *(int *)(puVar9 + 4);
  if (iVar10 < iVar11) {
    iVar11 = iVar10 * *(int *)(puVar9 + 0xc);
  }
  else if (iVar10 < *(int *)(puVar9 + 8) + iVar11) {
    iVar11 = iVar10 + (*(int *)(puVar9 + 0xc) + -1) * iVar11;
  }
  else {
    iVar11 = -1;
  }
  SetOutputSamplerate(iVar11);
  lVar3 = (&DAT_1800202e0)[lVar13];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
  _aligned_free(*(void **)(lVar3 + 0xa8));
  _aligned_free(*(void **)(lVar3 + 200));
  *(int *)(lVar3 + 0xc) = param_2;
  *(undefined8 *)(lVar3 + 0x70) = 0;
  dVar16 = cos(0.0);
  *(double *)(lVar3 + 0x78) = dVar16;
  dVar17 = sin(0.0);
  dVar16 = *(double *)(lVar3 + 0x48) * DAT_180018df0;
  *(double *)(lVar3 + 0x80) = dVar17;
  dVar16 = dVar16 / (double)param_2;
  *(double *)(lVar3 + 0x88) = dVar16;
  dVar17 = cos(dVar16);
  *(double *)(lVar3 + 0x90) = dVar17;
  dVar16 = sin(dVar16);
  *(double *)(lVar3 + 0x98) = dVar16;
  FUN_180012d80(lVar3);
  FUN_180012e70(lVar3);
  FUN_180012f70(lVar3);
  *(undefined4 *)(lVar3 + 0xe8) = 0;
  *(undefined4 *)(lVar3 + 0x100) = 0;
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
  lVar3 = (&DAT_1800202e0)[lVar13];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
  *(int *)(lVar3 + 0x10) = iVar2;
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
  SetAAudioMixState((void *)0x0,0,(byte)iVar12,0);
  lVar3 = DAT_18001e140;
  lVar14 = (longlong)iVar12;
  *(int *)(DAT_18001e140 + 0xd68 + lVar14 * 4) = param_2;
  destroy_resample(*(undefined8 *)(lVar3 + 0xc68 + lVar14 * 8));
  uVar4 = DAT_180018dc8;
  iVar11 = *(int *)(lVar3 + 0xde8);
  iVar7 = *(int *)(lVar3 + 0xd68 + lVar14 * 4);
  if (iVar11 < iVar7) {
    iVar6 = (iVar7 / iVar11) * *(int *)(lVar3 + 0x8c);
  }
  else {
    iVar6 = *(int *)(lVar3 + 0x8c) / (iVar11 / iVar7);
  }
  uVar8 = create_resample(iVar7 != iVar11,iVar6,0,*(undefined8 *)(lVar3 + 0xdf0 + lVar14 * 8),iVar7,
                          iVar11,0,0,DAT_180018dc8);
  *(undefined8 *)(lVar3 + 0xc68 + lVar14 * 8) = uVar8;
  SetAAudioMixState((void *)0x0,0,(byte)iVar12,param_3);
  puVar9 = PTR_DAT_18001e080;
  *(int *)(*(longlong *)(PTR_DAT_18001e080 + lVar15 + 0x1120) + 8) = iVar2;
  pSetEERSamplerate(*(undefined8 *)(puVar9 + lVar15 + 0x1128),param_2);
  pSetEERSize(*(undefined8 *)(PTR_DAT_18001e080 + lVar15 + 0x1128),iVar2);
  *(int *)(*(longlong *)(PTR_DAT_18001e080 + (lVar13 + 0x32) * 0x58) + 8) = iVar2;
  FUN_180008430(0,param_2);
  *(int *)(DAT_18001f430 + 0x2c) = iVar2;
  FUN_180008430(1,param_2);
  puVar9 = PTR_DAT_18001e080;
  iVar12 = 0;
  *(int *)(DAT_18001f438 + 0x2c) = iVar2;
  if (0 < *(int *)(puVar9 + 4)) {
    do {
      puVar5 = PTR_DAT_18001e098;
      if ((-1 < iVar12) && (iVar12 < *(int *)(puVar9 + 4))) {
        lVar13 = (longlong)iVar12 * 0x30;
        puVar1 = (undefined8 *)(PTR_DAT_18001e098 + lVar13 + 8);
        if (((void *)*puVar1 == (void *)0x0) ||
           (*(int *)(PTR_DAT_18001e098 + lVar13 + 0x14) == param_2)) {
          *(int *)(PTR_DAT_18001e098 + lVar13 + 0x14) = param_2;
        }
        else {
          *(int *)(PTR_DAT_18001e098 + lVar13 + 0x14) = param_2;
          SetAAudioMixState((void *)*puVar1,0,1,0);
          *(uint *)(puVar5 + lVar13 + 0x30) = *(uint *)(puVar5 + lVar13 + 0x30) & 0xfffffffd;
          lVar15 = *(longlong *)(puVar5 + lVar13 + 8);
          if (*(longlong *)(puVar5 + lVar13 + 8) == 0) {
            lVar15 = DAT_18001e140;
          }
          *(undefined4 *)(lVar15 + 0xd6c) = *(undefined4 *)(puVar5 + lVar13 + 0x14);
          destroy_resample(*(undefined8 *)(lVar15 + 0xc70));
          iVar11 = *(int *)(lVar15 + 0xde8);
          iVar2 = *(int *)(lVar15 + 0xd6c);
          if (iVar11 < iVar2) {
            iVar7 = (iVar2 / iVar11) * *(int *)(lVar15 + 0x8c);
          }
          else {
            iVar7 = *(int *)(lVar15 + 0x8c) / (iVar11 / iVar2);
          }
          uVar8 = create_resample(iVar2 != iVar11,iVar7,0,*(undefined8 *)(lVar15 + 0xdf8),iVar2,
                                  iVar11,0,0,uVar4);
          *(undefined8 *)(lVar15 + 0xc70) = uVar8;
          FUN_1800139e0(iVar12);
          puVar9 = PTR_DAT_18001e080;
        }
      }
      iVar12 = iVar12 + 1;
    } while (iVar12 < *(int *)(puVar9 + 4));
  }
                    /* WARNING: Could not recover jumptable at 0x0001800063c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(puVar9 + ((longlong)iVar10 * 5 + 0xd6) * 8));
  return;
}



/* ========================================================================
   ENTRY: 1800063d0
   NAME : SetAntiVOXSourceStates
   SIG  : undefined __fastcall SetAntiVOXSourceStates(int param_1, uint param_2, uint param_3)
   ======================================================================== */

void SetAntiVOXSourceStates(int param_1,uint param_2,uint param_3)

{
                    /* 0x63d0  87  SetAntiVOXSourceStates */
  SetAAudioMixStates(*(void **)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1138),-1,param_2,
                     param_3);
  return;
}



/* ========================================================================
   ENTRY: 180006400
   NAME : SetAntiVOXSourceWhat
   SIG  : undefined __fastcall SetAntiVOXSourceWhat(int param_1, uint param_2, int param_3)
   ======================================================================== */

void SetAntiVOXSourceWhat(int param_1,uint param_2,int param_3)

{
  byte *pbVar1;
  longlong lVar2;
  
                    /* 0x6400  88  SetAntiVOXSourceWhat */
  lVar2 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1138);
  if (*(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1138) == 0) {
    lVar2 = DAT_18001e138;
  }
  if (param_3 != 0) {
    LOCK();
    pbVar1 = (byte *)(lVar2 + 0x1b0 + ((longlong)(int)param_2 >> 3));
    *pbVar1 = *pbVar1 | '\x01' << (param_2 & 7);
    UNLOCK();
    return;
  }
  LOCK();
  pbVar1 = (byte *)(lVar2 + 0x1b0 + ((longlong)(int)param_2 >> 3));
  *pbVar1 = *pbVar1 & ~('\x01' << (param_2 & 7));
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180006440
   NAME : FUN_180006440
   SIG  : undefined __fastcall FUN_180006440(int param_1, longlong param_2, int param_3)
   ======================================================================== */

void FUN_180006440(int param_1,longlong param_2,int param_3)

{
  longlong lVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  longlong lVar6;
  uint uVar7;
  longlong lVar8;
  longlong *plVar9;
  int iVar10;
  
  lVar1 = (longlong)param_1 * 0x28;
  if (((*(int *)(&DAT_18001fc00 + lVar1) != 0) && (*(int *)(&DAT_18001fc04 + lVar1) != 0)) &&
     (0 < param_3)) {
    uVar7 = *(uint *)(&DAT_18001fbf0 + lVar1);
    LOCK();
    iVar5 = *(int *)(&DAT_18001fbf4 + lVar1);
    if (iVar5 == 0) {
      *(int *)(&DAT_18001fbf4 + lVar1) = 0;
      iVar5 = 0;
    }
    UNLOCK();
    iVar10 = *(int *)(&DAT_18001fbe8 + lVar1);
    iVar3 = (iVar10 - uVar7) + iVar5;
    if (iVar3 < param_3) {
      iVar3 = param_3 - iVar3;
      LOCK();
      *(int *)(&DAT_18001fbf8 + lVar1) = *(int *)(&DAT_18001fbf8 + lVar1) + iVar3;
      UNLOCK();
      LOCK();
      *(int *)(&DAT_18001fbfc + lVar1) = *(int *)(&DAT_18001fbfc + lVar1) + 1;
      UNLOCK();
      LOCK();
      *(int *)(&DAT_18001fbf4 + lVar1) = *(int *)(&DAT_18001fbf4 + lVar1) + iVar3;
      UNLOCK();
      iVar10 = *(int *)(&DAT_18001fbe8 + lVar1);
      iVar5 = iVar3 + (iVar10 - uVar7) + iVar5;
      if ((iVar5 < param_3) && (param_3 = iVar5, iVar5 < 1)) {
        return;
      }
    }
    uVar7 = *(uint *)(&DAT_18001fbec + lVar1) & uVar7;
    iVar5 = iVar10 - uVar7;
    if (param_3 < (int)(iVar10 - uVar7)) {
      iVar5 = param_3;
    }
    iVar10 = 0;
    if (3 < iVar5) {
      do {
        lVar8 = (longlong)(iVar10 * 2);
        lVar6 = (longlong)(int)((iVar10 + uVar7) * 2);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + lVar6 * 4) =
             (float)*(double *)(param_2 + lVar8 * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + 4 + lVar6 * 4) =
             (float)*(double *)(param_2 + 8 + lVar8 * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + 8 + lVar6 * 4) =
             (float)*(double *)(param_2 + 0x10 + lVar8 * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + 0xc + lVar6 * 4) =
             (float)*(double *)(param_2 + 0x18 + lVar8 * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) +
                  (longlong)(int)((iVar10 + uVar7 + 2) * 2) * 4) =
             (float)*(double *)(param_2 + (longlong)(iVar10 * 2 + 4) * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + 0x14 + lVar6 * 4) =
             (float)*(double *)(param_2 + 0x28 + lVar8 * 8);
        iVar2 = iVar10 * 2;
        iVar3 = iVar10 + uVar7 + 3;
        iVar10 = iVar10 + 4;
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + (longlong)(iVar3 * 2) * 4) =
             (float)*(double *)(param_2 + (longlong)(iVar2 + 6) * 8);
        *(float *)(*(longlong *)(&DAT_18001fbe0 + lVar1) + 0x1c + lVar6 * 4) =
             (float)*(double *)(param_2 + 0x38 + lVar8 * 8);
      } while (iVar10 < iVar5 + -3);
    }
    plVar9 = (longlong *)(&DAT_18001fbe0 + lVar1);
    for (; iVar10 < iVar5; iVar10 = iVar10 + 1) {
      lVar6 = (longlong)(int)((iVar10 + uVar7) * 2);
      *(float *)(*plVar9 + lVar6 * 4) = (float)*(double *)(param_2 + (longlong)(iVar10 * 2) * 8);
      *(float *)(*plVar9 + 4 + lVar6 * 4) =
           (float)*(double *)(param_2 + 8 + (longlong)(iVar10 * 2) * 8);
    }
    iVar10 = 0;
    iVar3 = param_3 - iVar5;
    if (3 < iVar3) {
      do {
        lVar6 = (longlong)((iVar10 + iVar5) * 2);
        lVar8 = (longlong)(iVar10 * 2);
        *(float *)(*plVar9 + lVar8 * 4) = (float)*(double *)(param_2 + lVar6 * 8);
        *(float *)(*plVar9 + 4 + lVar8 * 4) = (float)*(double *)(param_2 + 8 + lVar6 * 8);
        *(float *)(*plVar9 + 8 + lVar8 * 4) = (float)*(double *)(param_2 + 0x10 + lVar6 * 8);
        *(float *)(*plVar9 + 0xc + lVar8 * 4) = (float)*(double *)(param_2 + 0x18 + lVar6 * 8);
        *(float *)(*plVar9 + (longlong)(iVar10 * 2 + 4) * 4) =
             (float)*(double *)(param_2 + (longlong)((iVar10 + iVar5 + 2) * 2) * 8);
        *(float *)(*plVar9 + 0x14 + lVar8 * 4) = (float)*(double *)(param_2 + 0x28 + lVar6 * 8);
        iVar4 = iVar10 + iVar5 + 3;
        iVar2 = iVar10 * 2;
        iVar10 = iVar10 + 4;
        *(float *)(*plVar9 + (longlong)(iVar2 + 6) * 4) =
             (float)*(double *)(param_2 + (longlong)(iVar4 * 2) * 8);
        *(float *)(*plVar9 + 0x1c + lVar8 * 4) = (float)*(double *)(param_2 + 0x38 + lVar6 * 8);
      } while (iVar10 < iVar3 + -3);
    }
    for (; iVar10 < iVar3; iVar10 = iVar10 + 1) {
      lVar6 = (longlong)((iVar10 + iVar5) * 2);
      *(float *)(*plVar9 + (longlong)(iVar10 * 2) * 4) = (float)*(double *)(param_2 + lVar6 * 8);
      *(float *)(*plVar9 + 4 + (longlong)(iVar10 * 2) * 4) =
           (float)*(double *)(param_2 + 8 + lVar6 * 8);
    }
    LOCK();
    UNLOCK();
    LOCK();
    *(int *)(&DAT_18001fbf0 + lVar1) = *(int *)(&DAT_18001fbf0 + lVar1) + param_3;
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 1800067d0
   NAME : CM_WaterfallIQ_Init
   SIG  : undefined __fastcall CM_WaterfallIQ_Init(uint param_1, int param_2)
   ======================================================================== */

void CM_WaterfallIQ_Init(uint param_1,int param_2)

{
  longlong lVar1;
  size_t _Size;
  int iVar2;
  void *_Dst;
  int iVar3;
  
                    /* 0x67d0  6  CM_WaterfallIQ_Init */
  if (param_1 < 0x20) {
    lVar1 = (longlong)(int)param_1 * 0x28;
    if (*(int *)(&DAT_18001fc00 + lVar1) != 0) {
      if (*(void **)(&DAT_18001fbe0 + lVar1) != (void *)0x0) {
        free(*(void **)(&DAT_18001fbe0 + lVar1));
        *(undefined8 *)(&DAT_18001fbe0 + lVar1) = 0;
      }
      *(undefined8 *)(&DAT_18001fbe8 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fbf0 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fbf4 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fc00 + lVar1) = 0;
    }
    iVar3 = 1;
    iVar2 = 0x400;
    if (0x400 < param_2) {
      iVar2 = param_2;
    }
    if (1 < iVar2) {
      do {
        iVar3 = iVar3 * 2;
      } while (iVar3 < iVar2);
    }
    _Size = (longlong)(iVar3 * 2) * 4;
    _Dst = malloc(_Size);
    *(void **)(&DAT_18001fbe0 + lVar1) = _Dst;
    if (_Dst != (void *)0x0) {
      memset(_Dst,0,_Size);
      *(int *)(&DAT_18001fbe8 + lVar1) = iVar3;
      *(int *)(&DAT_18001fbec + lVar1) = iVar3 + -1;
      *(undefined4 *)(&DAT_18001fbf0 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fbf4 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fbf8 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fbfc + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fc04 + lVar1) = 0;
      *(undefined4 *)(&DAT_18001fc00 + lVar1) = 1;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800068d0
   NAME : CM_WaterfallIQ_SetEnabled
   SIG  : undefined __fastcall CM_WaterfallIQ_SetEnabled(uint param_1, int param_2)
   ======================================================================== */

void CM_WaterfallIQ_SetEnabled(uint param_1,int param_2)

{
                    /* 0x68d0  8  CM_WaterfallIQ_SetEnabled */
  if (param_1 < 0x20) {
    LOCK();
    *(uint *)(&DAT_18001fc04 + (longlong)(int)param_1 * 0x28) = (uint)(param_2 != 0);
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 180006900
   NAME : CM_WaterfallIQ_Free
   SIG  : undefined __fastcall CM_WaterfallIQ_Free(uint param_1)
   ======================================================================== */

void CM_WaterfallIQ_Free(uint param_1)

{
  longlong lVar1;
  
                    /* 0x6900  4  CM_WaterfallIQ_Free */
  if (param_1 < 0x20) {
    lVar1 = (longlong)(int)param_1 * 0x28;
    if (*(void **)(&DAT_18001fbe0 + lVar1) != (void *)0x0) {
      free(*(void **)(&DAT_18001fbe0 + lVar1));
      *(undefined8 *)(&DAT_18001fbe0 + lVar1) = 0;
    }
    *(undefined8 *)(&DAT_18001fbe8 + lVar1) = 0;
    *(undefined4 *)(&DAT_18001fbf0 + lVar1) = 0;
    *(undefined4 *)(&DAT_18001fbf4 + lVar1) = 0;
    *(undefined4 *)(&DAT_18001fc00 + lVar1) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180006970
   NAME : CM_WaterfallIQ_Available
   SIG  : int __fastcall CM_WaterfallIQ_Available(uint param_1)
   ======================================================================== */

int CM_WaterfallIQ_Available(uint param_1)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  
                    /* 0x6970  2  CM_WaterfallIQ_Available */
  if ((param_1 < 0x20) &&
     (lVar3 = (longlong)(int)param_1, *(int *)(&DAT_18001fc00 + lVar3 * 0x28) != 0)) {
    LOCK();
    iVar1 = *(int *)(&DAT_18001fbf0 + lVar3 * 0x28);
    if (iVar1 == 0) {
      *(int *)(&DAT_18001fbf0 + lVar3 * 0x28) = 0;
      iVar1 = 0;
    }
    UNLOCK();
    LOCK();
    iVar2 = *(int *)(&DAT_18001fbf4 + lVar3 * 0x28);
    if (iVar2 == 0) {
      *(int *)(&DAT_18001fbf4 + lVar3 * 0x28) = 0;
      iVar2 = 0;
    }
    UNLOCK();
    return iVar1 - iVar2;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800069b0
   NAME : CM_WaterfallIQ_Get
   SIG  : int __fastcall CM_WaterfallIQ_Get(uint param_1, longlong param_2, longlong param_3, int param_4)
   ======================================================================== */

int CM_WaterfallIQ_Get(uint param_1,longlong param_2,longlong param_3,int param_4)

{
  int iVar1;
  int iVar2;
  longlong lVar3;
  longlong lVar4;
  uint uVar5;
  int iVar6;
  longlong lVar7;
  int iVar8;
  longlong *plVar9;
  uint *puVar10;
  
                    /* 0x69b0  5  CM_WaterfallIQ_Get */
  if ((((param_1 < 0x20) && (0 < param_4)) &&
      (lVar4 = (longlong)(int)param_1 * 0x28, *(int *)(&DAT_18001fc00 + lVar4) != 0)) &&
     ((param_2 != 0 && (param_3 != 0)))) {
    LOCK();
    iVar2 = *(int *)(&DAT_18001fbf0 + lVar4);
    if (iVar2 == 0) {
      *(int *)(&DAT_18001fbf0 + lVar4) = 0;
      iVar2 = 0;
    }
    UNLOCK();
    puVar10 = (uint *)(&DAT_18001fbf4 + lVar4);
    LOCK();
    uVar5 = *puVar10;
    if (uVar5 == 0) {
      *puVar10 = 0;
      uVar5 = 0;
    }
    UNLOCK();
    iVar2 = iVar2 - uVar5;
    if (0 < iVar2) {
      if (iVar2 < param_4) {
        param_4 = iVar2;
      }
      uVar5 = *(uint *)(&DAT_18001fbec + lVar4) & uVar5;
      iVar2 = *(int *)(&DAT_18001fbe8 + lVar4) - uVar5;
      if (param_4 < (int)(*(int *)(&DAT_18001fbe8 + lVar4) - uVar5)) {
        iVar2 = param_4;
      }
      iVar8 = 0;
      if (3 < iVar2) {
        do {
          lVar7 = (longlong)iVar8;
          lVar3 = (longlong)(int)((iVar8 + uVar5) * 2);
          *(undefined4 *)(param_2 + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + lVar3 * 4);
          *(undefined4 *)(param_3 + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + 4 + lVar3 * 4);
          *(undefined4 *)(param_2 + 4 + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + 8 + lVar3 * 4);
          *(undefined4 *)(param_3 + 4 + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + 0xc + lVar3 * 4);
          *(undefined4 *)(param_2 + 8 + lVar7 * 4) =
               *(undefined4 *)
                (*(longlong *)(&DAT_18001fbe0 + lVar4) +
                (longlong)(int)((iVar8 + uVar5 + 2) * 2) * 4);
          iVar6 = iVar8 + uVar5 + 3;
          *(undefined4 *)(param_3 + 8 + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + 0x14 + lVar3 * 4);
          iVar8 = iVar8 + 4;
          *(undefined4 *)(param_2 + 0xc + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + (longlong)(iVar6 * 2) * 4);
          *(undefined4 *)(param_3 + 0xc + lVar7 * 4) =
               *(undefined4 *)(*(longlong *)(&DAT_18001fbe0 + lVar4) + 0x1c + lVar3 * 4);
        } while (iVar8 < iVar2 + -3);
      }
      plVar9 = (longlong *)(&DAT_18001fbe0 + lVar4);
      for (; iVar8 < iVar2; iVar8 = iVar8 + 1) {
        lVar4 = (longlong)(int)((iVar8 + uVar5) * 2);
        *(undefined4 *)(param_2 + (longlong)iVar8 * 4) = *(undefined4 *)(*plVar9 + lVar4 * 4);
        *(undefined4 *)(param_3 + (longlong)iVar8 * 4) = *(undefined4 *)(*plVar9 + 4 + lVar4 * 4);
      }
      iVar8 = 0;
      iVar6 = param_4 - iVar2;
      if (3 < iVar6) {
        do {
          lVar3 = (longlong)(iVar8 * 2);
          lVar4 = (longlong)(iVar8 + iVar2);
          *(undefined4 *)(param_2 + lVar4 * 4) = *(undefined4 *)(*plVar9 + lVar3 * 4);
          *(undefined4 *)(param_3 + lVar4 * 4) = *(undefined4 *)(*plVar9 + 4 + lVar3 * 4);
          *(undefined4 *)(param_2 + 4 + lVar4 * 4) = *(undefined4 *)(*plVar9 + 8 + lVar3 * 4);
          *(undefined4 *)(param_3 + 4 + lVar4 * 4) = *(undefined4 *)(*plVar9 + 0xc + lVar3 * 4);
          *(undefined4 *)(param_2 + 8 + lVar4 * 4) =
               *(undefined4 *)(*plVar9 + (longlong)(iVar8 * 2 + 4) * 4);
          iVar1 = iVar8 * 2;
          *(undefined4 *)(param_3 + 8 + lVar4 * 4) = *(undefined4 *)(*plVar9 + 0x14 + lVar3 * 4);
          iVar8 = iVar8 + 4;
          *(undefined4 *)(param_2 + 0xc + lVar4 * 4) =
               *(undefined4 *)(*plVar9 + (longlong)(iVar1 + 6) * 4);
          *(undefined4 *)(param_3 + 0xc + lVar4 * 4) = *(undefined4 *)(*plVar9 + 0x1c + lVar3 * 4);
        } while (iVar8 < iVar6 + -3);
      }
      for (; iVar8 < iVar6; iVar8 = iVar8 + 1) {
        *(undefined4 *)(param_2 + (longlong)(iVar8 + iVar2) * 4) =
             *(undefined4 *)(*plVar9 + (longlong)(iVar8 * 2) * 4);
        *(undefined4 *)(param_3 + (longlong)(iVar8 + iVar2) * 4) =
             *(undefined4 *)(*plVar9 + 4 + (longlong)(iVar8 * 2) * 4);
      }
      LOCK();
      UNLOCK();
      LOCK();
      *puVar10 = *puVar10 + param_4;
      UNLOCK();
      return param_4;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180006c50
   NAME : CM_WaterfallIQ_DroppedSamples
   SIG  : int __fastcall CM_WaterfallIQ_DroppedSamples(uint param_1)
   ======================================================================== */

int CM_WaterfallIQ_DroppedSamples(uint param_1)

{
  int iVar1;
  
                    /* 0x6c50  3  CM_WaterfallIQ_DroppedSamples */
  if ((param_1 < 0x20) && (*(int *)(&DAT_18001fc00 + (longlong)(int)param_1 * 0x28) != 0)) {
    LOCK();
    iVar1 = *(int *)(&DAT_18001fbf8 + (longlong)(int)param_1 * 0x28);
    if (iVar1 == 0) {
      *(int *)(&DAT_18001fbf8 + (longlong)(int)param_1 * 0x28) = 0;
      iVar1 = 0;
    }
    UNLOCK();
    return iVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180006c80
   NAME : CM_WaterfallIQ_ResetDropped
   SIG  : undefined __fastcall CM_WaterfallIQ_ResetDropped(uint param_1)
   ======================================================================== */

void CM_WaterfallIQ_ResetDropped(uint param_1)

{
  longlong lVar1;
  
                    /* 0x6c80  7  CM_WaterfallIQ_ResetDropped */
  if ((param_1 < 0x20) &&
     (lVar1 = (longlong)(int)param_1, *(int *)(&DAT_18001fc00 + lVar1 * 0x28) != 0)) {
    LOCK();
    *(undefined4 *)(&DAT_18001fbf8 + lVar1 * 0x28) = 0;
    UNLOCK();
    LOCK();
    *(undefined4 *)(&DAT_18001fbfc + lVar1 * 0x28) = 0;
    UNLOCK();
  }
  return;
}



/* ========================================================================
   ENTRY: 180006cb0
   NAME : FUN_180006cb0
   SIG  : undefined __fastcall FUN_180006cb0(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

void FUN_180006cb0(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  undefined8 local_res18;
  undefined8 local_res20;
  
  local_res18 = param_3;
  local_res20 = param_4;
  puVar1 = (undefined8 *)FUN_1800032a0();
  __stdio_common_vfprintf(*puVar1,param_1,param_2,0,&local_res18);
  return;
}



/* ========================================================================
   ENTRY: 180006d00
   NAME : Inbound
   SIG  : undefined __fastcall Inbound(uint param_1, int param_2, void * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void Inbound(uint param_1,int param_2,void *param_3)

{
  uint uVar1;
  longlong lVar2;
  uint uVar3;
  int iVar4;
  int iVar5;
  DWORD DVar6;
  FILE *_File;
  int iVar7;
  longlong lVar8;
  bool bVar9;
  undefined1 auStack_a8 [32];
  uint local_88;
  uint local_80;
  uint local_78;
  uint local_70;
  uint local_68;
  uint local_60;
  _SYSTEMTIME local_58;
  ulonglong local_48;
  
                    /* 0x6d00  37  Inbound */
  local_48 = DAT_18001e000 ^ (ulonglong)auStack_a8;
  lVar8 = (longlong)(int)param_1;
  lVar2 = *(longlong *)(PTR_DAT_18001e080 + lVar8 * 8 + 0x4b0);
  uVar3 = *(uint *)(lVar2 + 0x30);
  do {
    LOCK();
    uVar1 = *(uint *)(lVar2 + 0x30);
    bVar9 = uVar3 == uVar1;
    if (bVar9) {
      *(uint *)(lVar2 + 0x30) = uVar3 & 1;
      uVar1 = uVar3;
    }
    uVar3 = uVar1;
    UNLOCK();
  } while (!bVar9);
  if (uVar3 != 0) {
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x68));
    iVar7 = *(int *)(lVar2 + 0x14) - *(int *)(lVar2 + 0x20);
    iVar5 = param_2;
    if (iVar7 < param_2) {
      iVar5 = iVar7;
    }
    memcpy((void *)(*(longlong *)(lVar2 + 0x18) + (longlong)(*(int *)(lVar2 + 0x20) * 2) * 8),
           param_3,(longlong)iVar5 << 4);
    iVar4 = param_2 - iVar7;
    if (param_2 <= iVar7) {
      iVar4 = 0;
    }
    memcpy(*(void **)(lVar2 + 0x18),(void *)((longlong)(iVar5 * 2) * 8 + (longlong)param_3),
           (longlong)iVar4 << 4);
    *(int *)(lVar2 + 0x28) = *(int *)(lVar2 + 0x28) + param_2;
    if (*(int *)(lVar2 + 0xc) <= *(int *)(lVar2 + 0x28)) {
      iVar5 = *(int *)(lVar2 + 0x28) / *(int *)(lVar2 + 0xc);
      ReleaseSemaphore(*(HANDLE *)(lVar2 + 0x38),iVar5,(LPLONG)0x0);
      *(int *)(lVar2 + 0x28) = *(int *)(lVar2 + 0x28) - iVar5 * *(int *)(lVar2 + 0xc);
    }
    *(int *)(lVar2 + 0x20) = *(int *)(lVar2 + 0x20) + param_2;
    iVar5 = *(int *)(lVar2 + 0x20);
    iVar7 = *(int *)(lVar2 + 0x14);
    if (iVar7 <= iVar5) {
      iVar5 = iVar5 - iVar7;
      *(int *)(lVar2 + 0x20) = iVar5;
    }
    iVar5 = iVar5 - *(int *)(lVar2 + 0x24);
    iVar4 = iVar7 + iVar5;
    if (-1 < iVar5) {
      iVar4 = iVar5;
    }
    if ((((iVar7 * 9) / 10 < iVar4) && (param_1 < 0x40)) &&
       (DVar6 = GetTickCount(), 999 < DVar6 - *(int *)(&DAT_1800200e0 + lVar8 * 4))) {
      *(DWORD *)(&DAT_1800200e0 + lVar8 * 4) = DVar6;
      _File = fopen("net_seq_gaps.log","a");
      if (_File != (FILE *)0x0) {
        GetLocalTime(&local_58);
        local_68 = (uint)local_58.wMilliseconds;
        local_70 = (uint)local_58.wSecond;
        local_78 = (uint)local_58.wMinute;
        local_80 = (uint)local_58.wHour;
        local_88 = (uint)local_58.wDay;
        local_60 = param_1;
        FUN_180006cb0(_File,
                      "%04d-%02d-%02d %02d:%02d:%02d.%03d ring id=%d OVERRUN (DSP consumer behind)\n"
                      ,(ulonglong)local_58.wYear,(ulonglong)local_58.wMonth);
        fclose(_File);
      }
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x68));
  }
  return;
}



/* ========================================================================
   ENTRY: 180006f20
   NAME : FUN_180006f20
   SIG  : undefined __fastcall FUN_180006f20(int param_1)
   ======================================================================== */

void FUN_180006f20(int param_1)

{
  uint uVar1;
  int iVar2;
  longlong lVar3;
  longlong lVar4;
  void *_Dst;
  int iVar5;
  uint uVar6;
  HANDLE pvVar7;
  int iVar8;
  int iVar9;
  longlong lVar10;
  bool bVar11;
  DWORD local_res8 [2];
  longlong local_res10;
  
  local_res8[0] = 0;
  pvVar7 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res8);
  if (pvVar7 == (HANDLE)0x0) {
    pvVar7 = GetCurrentThread();
    SetThreadPriority(pvVar7,2);
  }
  else {
    AvSetMmThreadPriority(pvVar7,AVRT_PRIORITY_CRITICAL);
  }
  lVar10 = (longlong)param_1;
  lVar3 = *(longlong *)(PTR_DAT_18001e080 + lVar10 * 8 + 0x3b0);
  uVar6 = *(uint *)(lVar3 + 0x2c);
  do {
    LOCK();
    uVar1 = *(uint *)(lVar3 + 0x2c);
    bVar11 = uVar6 == uVar1;
    if (bVar11) {
      *(uint *)(lVar3 + 0x2c) = uVar6 & 1;
      uVar1 = uVar6;
    }
    uVar6 = uVar1;
    UNLOCK();
    local_res10 = lVar10;
  } while (!bVar11);
  while (uVar6 != 0) {
    WaitForSingleObject(*(HANDLE *)(lVar3 + 0x38),0xffffffff);
    lVar4 = *(longlong *)(PTR_DAT_18001e080 + lVar10 * 8 + 0x3b0);
    _Dst = *(void **)(PTR_DAT_18001e080 + lVar10 * 8 + 0xa8);
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar4 + 0x40));
    uVar6 = *(uint *)(lVar4 + 0x2c);
    do {
      LOCK();
      uVar1 = *(uint *)(lVar4 + 0x2c);
      bVar11 = uVar6 == uVar1;
      if (bVar11) {
        *(uint *)(lVar4 + 0x2c) = uVar6 & 1;
        uVar1 = uVar6;
      }
      uVar6 = uVar1;
      UNLOCK();
    } while (!bVar11);
    if (uVar6 == 0) {
      LeaveCriticalSection((LPCRITICAL_SECTION)(lVar4 + 0x40));
      _endthread();
    }
    else {
      iVar2 = *(int *)(lVar4 + 0xc);
      iVar9 = *(int *)(lVar4 + 0x14) - *(int *)(lVar4 + 0x24);
      iVar5 = iVar9;
      if (iVar2 <= iVar9) {
        iVar5 = iVar2;
      }
      memcpy(_Dst,(void *)(*(longlong *)(lVar4 + 0x18) + (longlong)(*(int *)(lVar4 + 0x24) * 2) * 8)
             ,(longlong)iVar5 << 4);
      iVar8 = iVar2 - iVar9;
      if (iVar2 <= iVar9) {
        iVar8 = 0;
      }
      memcpy((void *)((longlong)_Dst + (longlong)(iVar5 * 2) * 8),*(void **)(lVar4 + 0x18),
             (longlong)iVar8 << 4);
      *(int *)(lVar4 + 0x24) = *(int *)(lVar4 + 0x24) + *(int *)(lVar4 + 0xc);
      if (*(int *)(lVar4 + 0x14) <= *(int *)(lVar4 + 0x24)) {
        *(int *)(lVar4 + 0x24) = *(int *)(lVar4 + 0x24) - *(int *)(lVar4 + 0x14);
      }
      LeaveCriticalSection((LPCRITICAL_SECTION)(lVar4 + 0x40));
      lVar10 = local_res10;
    }
    xcmaster(param_1);
    uVar6 = *(uint *)(lVar3 + 0x2c);
    do {
      LOCK();
      uVar1 = *(uint *)(lVar3 + 0x2c);
      bVar11 = uVar6 == uVar1;
      if (bVar11) {
        *(uint *)(lVar3 + 0x2c) = uVar6 & 1;
        uVar1 = uVar6;
      }
      uVar6 = uVar1;
      UNLOCK();
    } while (!bVar11);
  }
  _endthread();
  return;
}



/* ========================================================================
   ENTRY: 1800070f0
   NAME : SetRadioStructure
   SIG  : undefined __fastcall SetRadioStructure(undefined4 param_1, undefined4 param_2, undefined4 param_3, undefined4 param_4, int param_5, void * param_6, void * param_7, undefined4 param_8, undefined4 param_9, undefined4 param_10)
   ======================================================================== */

void SetRadioStructure(undefined4 param_1,undefined4 param_2,undefined4 param_3,undefined4 param_4,
                      int param_5,void *param_6,void *param_7,undefined4 param_8,undefined4 param_9,
                      undefined4 param_10)

{
  undefined *puVar1;
  
                    /* 0x70f0  204  SetRadioStructure */
  puVar1 = PTR_DAT_18001e080;
  *(undefined4 *)(PTR_DAT_18001e080 + 8) = param_3;
  *(undefined4 *)puVar1 = param_1;
  *(undefined4 *)(puVar1 + 4) = param_2;
  *(undefined4 *)(puVar1 + 0xc) = param_4;
  *(int *)(puVar1 + 0x10) = param_5;
  memcpy(puVar1 + 0x14,param_6,(longlong)param_5 << 2);
  memcpy(puVar1 + 0x1c,param_7,(longlong)*(int *)puVar1 << 2);
  *(undefined4 *)(puVar1 + 0x9c) = param_8;
  *(undefined4 *)(puVar1 + 0xa0) = param_9;
  *(undefined4 *)(puVar1 + 0xa4) = param_10;
  return;
}



/* ========================================================================
   ENTRY: 180007160
   NAME : set_cmdefault_rates
   SIG  : undefined __fastcall set_cmdefault_rates(longlong param_1, int param_2, longlong param_3, longlong param_4)
   ======================================================================== */

void set_cmdefault_rates(longlong param_1,int param_2,longlong param_3,longlong param_4)

{
  int iVar1;
  undefined *puVar2;
  longlong lVar3;
  longlong lVar4;
  int iVar5;
  
                    /* 0x7160  299  set_cmdefault_rates */
  puVar2 = PTR_DAT_18001e080;
  iVar5 = 0;
  if (0 < *(int *)PTR_DAT_18001e080) {
    do {
      lVar3 = (longlong)iVar5;
      iVar5 = iVar5 + 1;
      iVar1 = *(int *)(param_1 + lVar3 * 4);
      *(int *)(puVar2 + lVar3 * 4 + 0x1a8) = iVar1;
      *(int *)(puVar2 + lVar3 * 4 + 0x228) = (iVar1 << 6) / 48000;
    } while (iVar5 < *(int *)puVar2);
  }
  *(int *)(puVar2 + 0x2a8) = param_2;
  iVar5 = 0;
  *(undefined4 *)(puVar2 + 0xce4) = 0;
  *(int *)(puVar2 + 0x2ac) = (param_2 << 6) / 48000;
  if (0 < *(int *)(puVar2 + 4)) {
    do {
      lVar3 = (longlong)iVar5;
      iVar5 = iVar5 + 1;
      lVar4 = lVar3 * 0x40;
      iVar1 = *(int *)(param_3 + lVar3 * 4);
      *(int *)(puVar2 + lVar4 + 0xcf0) = iVar1;
      *(int *)(puVar2 + lVar4 + 0xcf4) = (iVar1 << 6) / 48000;
    } while (iVar5 < *(int *)(puVar2 + 4));
  }
  iVar5 = 0;
  if (0 < *(int *)(puVar2 + 8)) {
    do {
      lVar3 = (longlong)iVar5;
      iVar5 = iVar5 + 1;
      lVar4 = lVar3 * 0x58;
      iVar1 = *(int *)(param_4 + lVar3 * 4);
      *(int *)(puVar2 + lVar4 + 0x10f0) = iVar1;
      *(int *)(puVar2 + lVar4 + 0x10f4) = (iVar1 << 6) / 48000;
    } while (iVar5 < *(int *)(puVar2 + 8));
  }
  return;
}



/* ========================================================================
   ENTRY: 180007280
   NAME : CreateRadio
   SIG  : undefined __fastcall CreateRadio(void)
   ======================================================================== */

void CreateRadio(void)

{
  undefined8 uVar1;
  
                    /* 0x7280  10  CreateRadio */
  FUN_1800045a0();
  FUN_180011a40();
  uVar1 = malloc0(*(int *)(PTR_DAT_18001e080 + 0x1c) << 4);
  *(undefined8 *)PTR_DAT_18001e090 = uVar1;
                    /* WARNING: Could not recover jumptable at 0x0001800072bf. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  create_divEXT(0,0,2,0x400);
  return;
}



/* ========================================================================
   ENTRY: 1800072d0
   NAME : DestroyRadio
   SIG  : undefined __fastcall DestroyRadio(void)
   ======================================================================== */

void DestroyRadio(void)

{
  void *_Memory;
  undefined *puVar1;
  longlong lVar2;
  undefined *puVar3;
  int iVar4;
  int iVar5;
  
                    /* 0x72d0  12  DestroyRadio */
  destroy_divEXT(0);
  _aligned_free(*(void **)PTR_DAT_18001e090);
  iVar5 = 0;
  iVar4 = iVar5;
  if (0 < *(int *)(PTR_DAT_18001e080 + 0x14)) {
    do {
      destroy_anb(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)iVar4 * 0x10 + 0x330));
      destroy_nob(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)iVar4 * 0x10 + 0x338));
      iVar4 = iVar4 + 1;
    } while (iVar4 < *(int *)(PTR_DAT_18001e080 + 0x14));
  }
  puVar3 = PTR_DAT_18001e080;
  iVar4 = iVar5;
  if (0 < *(int *)(PTR_DAT_18001e080 + 4)) {
    do {
      puVar1 = PTR_DAT_18001e098;
      if ((-1 < iVar4) && (iVar4 < *(int *)(puVar3 + 4))) {
        lVar2 = (longlong)iVar4;
        if (*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8) != (void *)0x0) {
          FUN_1800016f0(*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8),0);
          puVar3 = PTR_DAT_18001e080;
          *(undefined8 *)(puVar1 + lVar2 * 0x30 + 8) = 0;
        }
        *(undefined8 *)(puVar1 + lVar2 * 0x30 + 0x30) = 0;
      }
      iVar4 = iVar4 + 1;
    } while (iVar4 < *(int *)(puVar3 + 4));
  }
  if (0 < *(int *)(puVar3 + 4)) {
    do {
      _aligned_free(*(void **)(*(longlong *)PTR_DAT_18001e088 + (longlong)iVar5 * 8));
      _Memory = (void *)(&DAT_18001f430)[iVar5];
      _aligned_free(*(void **)((longlong)_Memory + 0x40));
      _aligned_free(*(void **)((longlong)_Memory + 0xb0));
      *(undefined8 *)((longlong)_Memory + 0xb0) = 0;
      *(undefined8 *)((longlong)_Memory + 0xa8) = 0;
      destroy_rmatchV(*(undefined8 *)((longlong)_Memory + 0x50));
      destroy_rmatchV(*(undefined8 *)((longlong)_Memory + 0x48));
      DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x158));
      free(_Memory);
      iVar5 = iVar5 + 1;
    } while (iVar5 < *(int *)(PTR_DAT_18001e080 + 4));
  }
  _aligned_free(*(void **)PTR_DAT_18001e088);
  destroy_siphonEXT(0);
  FUN_180004a60();
  return;
}



/* ========================================================================
   ENTRY: 180007470
   NAME : getbuffsize
   SIG  : int __fastcall getbuffsize(int param_1)
   ======================================================================== */

int getbuffsize(int param_1)

{
                    /* 0x7470  288  getbuffsize */
  return (param_1 << 6) / 48000;
}



/* ========================================================================
   ENTRY: 180007490
   NAME : getInputRate
   SIG  : undefined4 __fastcall getInputRate(int param_1, int param_2)
   ======================================================================== */

undefined4 getInputRate(int param_1,int param_2)

{
                    /* 0x7490  270  getInputRate */
  if (param_1 == 0) {
    return *(undefined4 *)(PTR_DAT_18001e080 + (longlong)param_2 * 4 + 0x1a8);
  }
  if (param_1 != 1) {
    if (param_1 != 2) {
      return *(undefined4 *)(PTR_DAT_18001e080 + 0x1a4);
    }
    return *(undefined4 *)
            (PTR_DAT_18001e080 +
            (longlong)(*(int *)(PTR_DAT_18001e080 + 8) + *(int *)(PTR_DAT_18001e080 + 4) + param_2)
            * 4 + 0x1a8);
  }
  return *(undefined4 *)
          (PTR_DAT_18001e080 + (longlong)(param_2 + *(int *)(PTR_DAT_18001e080 + 4)) * 4 + 0x1a8);
}



/* ========================================================================
   ENTRY: 180007500
   NAME : getChannelOutputRate
   SIG  : undefined4 __fastcall getChannelOutputRate(int param_1, int param_2)
   ======================================================================== */

undefined4 getChannelOutputRate(int param_1,int param_2)

{
  undefined4 local_res8;
  
                    /* 0x7500  263  getChannelOutputRate */
  if (param_1 == 0) {
    return *(undefined4 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xcf0);
  }
  if (param_1 == 1) {
    return *(undefined4 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x58 + 0x10f0);
  }
  return local_res8;
}



/* ========================================================================
   ENTRY: 180007540
   NAME : chid
   SIG  : int __fastcall chid(int param_1, int param_2)
   ======================================================================== */

int chid(int param_1,int param_2)

{
  int iVar1;
  
                    /* 0x7540  250  chid */
  iVar1 = *(int *)(PTR_DAT_18001e080 + 4);
  if (param_1 < iVar1) {
    return param_2 + param_1 * *(int *)(PTR_DAT_18001e080 + 0xc);
  }
  if (*(int *)(PTR_DAT_18001e080 + 8) + iVar1 <= param_1) {
    return -1;
  }
  return (*(int *)(PTR_DAT_18001e080 + 0xc) + -1) * iVar1 + param_1;
}



/* ========================================================================
   ENTRY: 180007580
   NAME : inid
   SIG  : int __fastcall inid(int param_1, int param_2)
   ======================================================================== */

int inid(int param_1,int param_2)

{
                    /* 0x7580  289  inid */
  if (param_1 == 0) {
    return param_2;
  }
  if (param_1 != 1) {
    if (param_1 != 2) {
      return -1;
    }
    return *(int *)(PTR_DAT_18001e080 + 8) + *(int *)(PTR_DAT_18001e080 + 4) + param_2;
  }
  return *(int *)(PTR_DAT_18001e080 + 4) + param_2;
}



/* ========================================================================
   ENTRY: 1800075c0
   NAME : SetILVRun
   SIG  : undefined __fastcall SetILVRun(int param_1, int param_2)
   ======================================================================== */

void SetILVRun(int param_1,int param_2)

{
  uint *puVar1;
  
                    /* 0x75c0  129  SetILVRun */
  puVar1 = *(uint **)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58);
  if (param_2 != 0) {
    LOCK();
    *puVar1 = *puVar1 | 1;
    UNLOCK();
    return;
  }
  LOCK();
  *puVar1 = *puVar1 & 0xfffffffe;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 1800075f0
   NAME : SetILVWhat
   SIG  : undefined __fastcall SetILVWhat(int param_1, uint param_2, int param_3)
   ======================================================================== */

void SetILVWhat(int param_1,uint param_2,int param_3)

{
  byte *pbVar1;
  
                    /* 0x75f0  130  SetILVWhat */
  if (param_3 != 0) {
    LOCK();
    pbVar1 = (byte *)(*(longlong *)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58) + 0x10 +
                     ((longlong)(int)param_2 >> 3));
    *pbVar1 = *pbVar1 | '\x01' << (param_2 & 7);
    UNLOCK();
    return;
  }
  LOCK();
  pbVar1 = (byte *)(*(longlong *)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58) + 0x10 +
                   ((longlong)(int)param_2 >> 3));
  *pbVar1 = *pbVar1 & ~('\x01' << (param_2 & 7));
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180007620
   NAME : SetILVInsize
   SIG  : undefined __fastcall SetILVInsize(int param_1, undefined4 param_2)
   ======================================================================== */

void SetILVInsize(int param_1,undefined4 param_2)

{
                    /* 0x7620  127  SetILVInsize */
  *(undefined4 *)(*(longlong *)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58) + 8) =
       param_2;
  return;
}



/* ========================================================================
   ENTRY: 180007640
   NAME : SetILVOutboundId
   SIG  : undefined __fastcall SetILVOutboundId(int param_1, undefined4 param_2)
   ======================================================================== */

void SetILVOutboundId(int param_1,undefined4 param_2)

{
                    /* 0x7640  128  SetILVOutboundId */
  *(undefined4 *)(*(longlong *)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58) + 4) =
       param_2;
  return;
}



/* ========================================================================
   ENTRY: 180007660
   NAME : FUN_180007660
   SIG  : undefined __fastcall FUN_180007660(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

void FUN_180007660(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 uVar1;
  undefined8 *puVar2;
  undefined8 local_res10;
  undefined8 local_res18;
  undefined8 local_res20;
  
  local_res10 = param_2;
  local_res18 = param_3;
  local_res20 = param_4;
  uVar1 = __acrt_iob_func(1);
  puVar2 = (undefined8 *)FUN_1800032a0();
  __stdio_common_vfprintf(*puVar2,uVar1,param_1,0,&local_res10);
  return;
}



/* ========================================================================
   ENTRY: 1800076c0
   NAME : FUN_1800076c0
   SIG  : undefined __fastcall FUN_1800076c0(longlong param_1)
   ======================================================================== */

void FUN_1800076c0(longlong param_1)

{
  undefined4 uVar1;
  undefined4 uVar2;
  undefined8 uVar3;
  int iVar4;
  
  iVar4 = (int)((double)(*(int *)(param_1 + 0x10) * 2) * *(double *)(param_1 + 200));
  *(int *)(param_1 + 0x58) = iVar4;
  *(int *)(param_1 + 0x5c) =
       (int)((double)(*(int *)(param_1 + 0x1c) * 2) * *(double *)(param_1 + 0xd0));
  uVar3 = create_rmatchV(*(undefined4 *)(param_1 + 0x30),*(undefined4 *)(param_1 + 0x20),
                         *(int *)(param_1 + 0x1c),*(int *)(param_1 + 0x10),iVar4,
                         *(undefined8 *)(param_1 + 0x138));
  *(undefined8 *)(param_1 + 0x48) = uVar3;
  forceRMatchVar(uVar3,*(undefined4 *)(param_1 + 0x118),*(undefined8 *)(param_1 + 0x120));
  if (*(int *)(param_1 + 4) == 0) {
    uVar1 = *(undefined4 *)(param_1 + 0x14);
    uVar2 = *(undefined4 *)(param_1 + 0x28);
  }
  else {
    uVar1 = *(undefined4 *)(param_1 + 0xc);
    uVar2 = *(undefined4 *)(param_1 + 0x24);
  }
  uVar3 = create_rmatchV(uVar2,*(undefined4 *)(param_1 + 0x30),uVar1,*(undefined4 *)(param_1 + 0x1c)
                         ,*(undefined4 *)(param_1 + 0x5c),*(undefined8 *)(param_1 + 0x140));
  *(undefined8 *)(param_1 + 0x50) = uVar3;
  forceRMatchVar(uVar3,*(undefined4 *)(param_1 + 0x128),*(undefined8 *)(param_1 + 0x130));
  iVar4 = *(uint *)(PTR_DAT_18001e080 + 0x9c) << 6;
  uVar3 = malloc0((((*(uint *)(PTR_DAT_18001e080 + 0x9c) & 0x3ffffff) >> 0x19) +
                  iVar4 / 48000 + (iVar4 >> 0x1f)) * 0x10);
  *(undefined8 *)(param_1 + 0x40) = uVar3;
  if (*(void **)(param_1 + 0xb0) != (void *)0x0) {
    _aligned_free(*(void **)(param_1 + 0xb0));
    *(undefined8 *)(param_1 + 0xb0) = 0;
    *(undefined8 *)(param_1 + 0xa8) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800077f0
   NAME : create_ivac
   SIG  : undefined __fastcall create_ivac(int param_1, undefined4 param_2, undefined8 param_3, undefined8 param_4, undefined4 param_5, undefined4 param_6, undefined4 param_7, undefined4 param_8, undefined4 param_9, undefined4 param_10, undefined4 param_11, undefined4 param_12, undefined4 param_13, undefined4 param_14)
   ======================================================================== */

void create_ivac(int param_1,undefined4 param_2,undefined8 param_3,undefined8 param_4,
                undefined4 param_5,undefined4 param_6,undefined4 param_7,undefined4 param_8,
                undefined4 param_9,undefined4 param_10,undefined4 param_11,undefined4 param_12,
                undefined4 param_13,undefined4 param_14)

{
  undefined4 *puVar1;
  int *piVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  undefined8 in_stack_ffffffffffffff88;
  int local_38;
  undefined4 local_34;
  
                    /* 0x77f0  252  create_ivac */
  uVar3 = 0x180;
  uVar4 = param_3;
  uVar5 = param_4;
  puVar1 = calloc(1,0x180);
  if (puVar1 == (undefined4 *)0x0) {
    FUN_180007660("mem failure in ivac \n",uVar3,uVar4,uVar5);
                    /* WARNING: Subroutine does not return */
    exit(1);
  }
  *puVar1 = param_2;
  puVar1[1] = (int)param_3;
  puVar1[2] = (int)param_4;
  puVar1[3] = param_5;
  puVar1[4] = param_6;
  puVar1[5] = param_7;
  puVar1[6] = param_8;
  puVar1[7] = param_9;
  puVar1[8] = param_10;
  puVar1[9] = param_11;
  puVar1[10] = param_12;
  puVar1[0xb] = param_13;
  puVar1[0xc] = param_14;
  puVar1[0x46] = 0;
  puVar1[0x4a] = 0;
  *(undefined8 *)(puVar1 + 0x52) = 0;
  puVar1[0x54] = 0;
  *(undefined8 *)(puVar1 + 0x2c) = 0;
  *(undefined8 *)(puVar1 + 0x2a) = 0;
  *(undefined8 *)(puVar1 + 0x48) = 0x3ff0000000000000;
  *(undefined8 *)(puVar1 + 0x4c) = 0x3ff0000000000000;
  *(undefined8 *)(puVar1 + 0x4e) = 0x3ff0000000000000;
  *(undefined8 *)(puVar1 + 0x50) = 0x3ff0000000000000;
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(puVar1 + 0x56),0x9c4);
  FUN_1800076c0((longlong)puVar1);
  local_38 = puVar1[5];
  local_34 = puVar1[6];
  piVar2 = FUN_180001340(-1,param_1,puVar1[10],puVar1[10],2,3,3,DAT_180018dc8,
                         in_stack_ffffffffffffff88,(longlong)&local_38,local_38,&LAB_180007c30,0,0,0
                         ,0);
  *(int **)(puVar1 + 0xe) = piVar2;
  (&DAT_18001f430)[param_1] = puVar1;
  return;
}



/* ========================================================================
   ENTRY: 1800079b0
   NAME : destroy_ivac
   SIG  : undefined __fastcall destroy_ivac(int param_1)
   ======================================================================== */

void destroy_ivac(int param_1)

{
  void *_Memory;
  
                    /* 0x79b0  254  destroy_ivac */
  _Memory = (void *)(&DAT_18001f430)[param_1];
  _aligned_free(*(void **)((longlong)_Memory + 0x40));
  _aligned_free(*(void **)((longlong)_Memory + 0xb0));
  *(undefined8 *)((longlong)_Memory + 0xb0) = 0;
  *(undefined8 *)((longlong)_Memory + 0xa8) = 0;
  destroy_rmatchV(*(undefined8 *)((longlong)_Memory + 0x50));
  destroy_rmatchV(*(undefined8 *)((longlong)_Memory + 0x48));
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x158));
                    /* WARNING: Could not recover jumptable at 0x000180007a14. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180007a20
   NAME : xvacIN
   SIG  : undefined __fastcall xvacIN(int param_1, longlong param_2, int param_3)
   ======================================================================== */

void xvacIN(int param_1,longlong param_2,int param_3)

{
  double *pdVar1;
  int *piVar2;
  undefined1 auVar3 [16];
  undefined1 auVar4 [16];
  undefined1 auVar5 [16];
  double dVar6;
  double dVar7;
  undefined1 auVar8 [16];
  undefined1 auVar9 [16];
  uint uVar10;
  uint uVar11;
  longlong lVar12;
  int iVar13;
  int iVar14;
  
                    /* 0x7a20  302  xvacIN */
  piVar2 = (int *)(&DAT_18001f430)[param_1];
  if (*piVar2 != 0) {
    if (param_3 == 0 && piVar2[0x3d] == 0) {
      xrmatchOUT();
      if (piVar2[0x3e] != 0) {
        iVar14 = piVar2[8];
        iVar13 = 0;
        if (0 < iVar14 * 2) {
          do {
            lVar12 = (longlong)iVar13;
            iVar13 = iVar13 + 2;
            dVar6 = *(double *)(param_2 + 8 + lVar12 * 8) + *(double *)(param_2 + lVar12 * 8);
            auVar3._8_4_ = SUB84(dVar6,0);
            auVar3._0_8_ = dVar6;
            auVar3._12_4_ = (int)((ulonglong)dVar6 >> 0x20);
            *(undefined1 (*) [16])(param_2 + lVar12 * 8) = auVar3;
          } while (iVar13 < iVar14 * 2);
        }
      }
      dVar6 = *(double *)(piVar2 + 0x40);
      uVar10 = piVar2[8] * 2;
      if (0 < (int)uVar10) {
        iVar14 = 0;
        if (7 < uVar10) {
          uVar11 = uVar10 & 0x80000007;
          if ((int)uVar11 < 0) {
            uVar11 = (uVar11 - 1 | 0xfffffff8) + 1;
          }
          do {
            lVar12 = (longlong)iVar14;
            iVar14 = iVar14 + 8;
            pdVar1 = (double *)(param_2 + lVar12 * 8);
            dVar7 = pdVar1[1] * dVar6;
            auVar4._8_4_ = SUB84(dVar7,0);
            auVar4._0_8_ = *pdVar1 * dVar6;
            auVar4._12_4_ = (int)((ulonglong)dVar7 >> 0x20);
            *(undefined1 (*) [16])(param_2 + lVar12 * 8) = auVar4;
            pdVar1 = (double *)(param_2 + 0x10 + lVar12 * 8);
            dVar7 = pdVar1[1] * dVar6;
            auVar8._8_4_ = SUB84(dVar7,0);
            auVar8._0_8_ = *pdVar1 * dVar6;
            auVar8._12_4_ = (int)((ulonglong)dVar7 >> 0x20);
            *(undefined1 (*) [16])(param_2 + 0x10 + lVar12 * 8) = auVar8;
            pdVar1 = (double *)(param_2 + 0x20 + lVar12 * 8);
            dVar7 = pdVar1[1] * dVar6;
            auVar5._8_4_ = SUB84(dVar7,0);
            auVar5._0_8_ = *pdVar1 * dVar6;
            auVar5._12_4_ = (int)((ulonglong)dVar7 >> 0x20);
            *(undefined1 (*) [16])(param_2 + 0x20 + lVar12 * 8) = auVar5;
            pdVar1 = (double *)(param_2 + 0x30 + lVar12 * 8);
            dVar7 = pdVar1[1] * dVar6;
            auVar9._8_4_ = SUB84(dVar7,0);
            auVar9._0_8_ = *pdVar1 * dVar6;
            auVar9._12_4_ = (int)((ulonglong)dVar7 >> 0x20);
            *(undefined1 (*) [16])(param_2 + 0x30 + lVar12 * 8) = auVar9;
          } while (iVar14 < (int)(uVar10 - uVar11));
          if ((int)uVar10 <= iVar14) {
            return;
          }
        }
        if (3 < (int)(uVar10 - iVar14)) {
          do {
            lVar12 = (longlong)iVar14;
            iVar14 = iVar14 + 4;
            *(double *)(param_2 + lVar12 * 8) = dVar6 * *(double *)(param_2 + lVar12 * 8);
            *(double *)(param_2 + 8 + lVar12 * 8) = dVar6 * *(double *)(param_2 + 8 + lVar12 * 8);
            *(double *)(param_2 + 0x10 + lVar12 * 8) =
                 dVar6 * *(double *)(param_2 + 0x10 + lVar12 * 8);
            *(double *)(param_2 + 0x18 + lVar12 * 8) =
                 dVar6 * *(double *)(param_2 + 0x18 + lVar12 * 8);
          } while (iVar14 < (int)(uVar10 - 3));
          if ((int)uVar10 <= iVar14) {
            return;
          }
        }
        do {
          lVar12 = (longlong)iVar14;
          iVar14 = iVar14 + 1;
          *(double *)(param_2 + lVar12 * 8) = dVar6 * *(double *)(param_2 + lVar12 * 8);
        } while (iVar14 < (int)uVar10);
        return;
      }
    }
    else {
      xrmatchOUT(*(undefined8 *)(piVar2 + 0x12),*(undefined8 *)(piVar2 + 0x10));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180007bc0
   NAME : xvacOUT
   SIG  : undefined __fastcall xvacOUT(int param_1, int param_2, void * param_3)
   ======================================================================== */

void xvacOUT(int param_1,int param_2,void *param_3)

{
  int *piVar1;
  
                    /* 0x7bc0  303  xvacOUT */
  piVar1 = (int *)(&DAT_18001f430)[param_1];
  if (*piVar1 != 0) {
    if (piVar1[1] == 0) {
      if (param_2 == 1) {
        FUN_180001840(*(longlong *)(piVar1 + 0xe),-1,0,param_3);
        return;
      }
      if (param_2 == 2) {
        FUN_180001840(*(longlong *)(piVar1 + 0xe),-1,1,param_3);
        return;
      }
    }
    else if (param_2 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180007c19. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      xrmatchIN(*(undefined8 *)(piVar1 + 0x14),param_3);
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180007c50
   NAME : FUN_180007c50
   SIG  : undefined8 __fastcall FUN_180007c50(longlong param_1, longlong param_2)
   ======================================================================== */

undefined8 FUN_180007c50(longlong param_1,longlong param_2)

{
  undefined4 *puVar1;
  undefined8 uVar2;
  uint uVar3;
  int *piVar4;
  void *_Dst;
  undefined4 uVar5;
  undefined4 uVar6;
  longlong lVar7;
  int iVar8;
  ulonglong uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  int in_stack_00000030;
  
  piVar4 = (int *)(&DAT_18001f430)[in_stack_00000030];
  if (*piVar4 != 0) {
    if (piVar4[0x19] == 1) {
      uVar3 = piVar4[0xc];
      uVar9 = (ulonglong)(uVar3 * 2);
      if (*(ulonglong *)(piVar4 + 0x2a) < uVar9) {
        if (*(void **)(piVar4 + 0x2c) != (void *)0x0) {
          _aligned_free(*(void **)(piVar4 + 0x2c));
          piVar4[0x2c] = 0;
          piVar4[0x2d] = 0;
          piVar4[0x2a] = 0;
          piVar4[0x2b] = 0;
        }
        lVar7 = malloc0(uVar3 << 4);
        if (lVar7 == 0) {
          return 2;
        }
        *(longlong *)(piVar4 + 0x2c) = lVar7;
        *(ulonglong *)(piVar4 + 0x2a) = uVar9;
      }
      _Dst = *(void **)(piVar4 + 0x2c);
      if (param_1 == 0) {
        memset(_Dst,0,uVar9 * 8);
      }
      else {
        lVar7 = 0;
        uVar9 = 0;
        if (3 < uVar3) {
          do {
            puVar1 = (undefined4 *)(param_1 + uVar9 * 8);
            uVar5 = *puVar1;
            uVar6 = puVar1[1];
            iVar8 = (int)uVar9;
            puVar1 = (undefined4 *)((longlong)_Dst + lVar7 * 8);
            *puVar1 = uVar5;
            puVar1[1] = uVar6;
            puVar1[2] = uVar5;
            puVar1[3] = uVar6;
            puVar1 = (undefined4 *)(param_1 + (ulonglong)(iVar8 + 1) * 8);
            uVar5 = *puVar1;
            uVar6 = puVar1[1];
            puVar1 = (undefined4 *)((longlong)_Dst + lVar7 * 8 + 0x10);
            *puVar1 = uVar5;
            puVar1[1] = uVar6;
            puVar1[2] = uVar5;
            puVar1[3] = uVar6;
            puVar1 = (undefined4 *)(param_1 + (ulonglong)(iVar8 + 2) * 8);
            uVar5 = *puVar1;
            uVar6 = puVar1[1];
            uVar9 = (ulonglong)(iVar8 + 4U);
            puVar1 = (undefined4 *)((longlong)_Dst + lVar7 * 8 + 0x20);
            *puVar1 = uVar5;
            puVar1[1] = uVar6;
            puVar1[2] = uVar5;
            puVar1[3] = uVar6;
            puVar1 = (undefined4 *)(param_1 + (ulonglong)(iVar8 + 3) * 8);
            uVar5 = *puVar1;
            uVar6 = puVar1[1];
            puVar1 = (undefined4 *)((longlong)_Dst + lVar7 * 8 + 0x30);
            *puVar1 = uVar5;
            puVar1[1] = uVar6;
            puVar1[2] = uVar5;
            puVar1[3] = uVar6;
            lVar7 = lVar7 + 8;
          } while (iVar8 + 4U < uVar3 - 3);
        }
        uVar10 = (uint)uVar9;
        while (uVar10 < uVar3) {
          puVar1 = (undefined4 *)(param_1 + uVar9 * 8);
          uVar5 = *puVar1;
          uVar6 = puVar1[1];
          lVar7 = lVar7 + 2;
          uVar10 = (int)uVar9 + 1;
          uVar9 = (ulonglong)uVar10;
          puVar1 = (undefined4 *)((longlong)_Dst + lVar7 * 8 + -0x10);
          *puVar1 = uVar5;
          puVar1[1] = uVar6;
          puVar1[2] = uVar5;
          puVar1[3] = uVar6;
        }
      }
      xrmatchIN(*(undefined8 *)(piVar4 + 0x12),_Dst);
    }
    else {
      xrmatchIN(*(undefined8 *)(piVar4 + 0x12),param_1);
    }
    xrmatchOUT(*(undefined8 *)(piVar4 + 0x14),param_2);
    if ((piVar4[1] != 0) && (piVar4[0x52] != 0)) {
      uVar3 = piVar4[0xc];
      uVar10 = 0;
      uVar9 = 1;
      uVar11 = (uint)DAT_180018ea0;
      uVar12 = (uint)((ulonglong)DAT_180018ea0 >> 0x20);
      if (3 < uVar3) {
        do {
          iVar8 = (int)uVar9;
          uVar10 = uVar10 + 4;
          uVar2 = *(undefined8 *)(param_2 + uVar9 * 8);
          *(ulonglong *)(param_2 + uVar9 * 8) =
               CONCAT44((uint)((ulonglong)uVar2 >> 0x20) ^ uVar12,(uint)uVar2 ^ uVar11);
          uVar2 = *(undefined8 *)(param_2 + (ulonglong)(iVar8 + 2) * 8);
          *(ulonglong *)(param_2 + (ulonglong)(iVar8 + 2) * 8) =
               CONCAT44((uint)((ulonglong)uVar2 >> 0x20) ^ uVar12,(uint)uVar2 ^ uVar11);
          uVar2 = *(undefined8 *)(param_2 + (ulonglong)(iVar8 + 4) * 8);
          *(ulonglong *)(param_2 + (ulonglong)(iVar8 + 4) * 8) =
               CONCAT44((uint)((ulonglong)uVar2 >> 0x20) ^ uVar12,(uint)uVar2 ^ uVar11);
          uVar2 = *(undefined8 *)(param_2 + (ulonglong)(iVar8 + 6) * 8);
          uVar9 = (ulonglong)(iVar8 + 8);
          *(ulonglong *)(param_2 + (ulonglong)(iVar8 + 6) * 8) =
               CONCAT44((uint)((ulonglong)uVar2 >> 0x20) ^ uVar12,(uint)uVar2 ^ uVar11);
        } while (uVar10 < uVar3 - 3);
      }
      for (; uVar10 < uVar3; uVar10 = uVar10 + 1) {
        uVar2 = *(undefined8 *)(param_2 + uVar9 * 8);
        *(ulonglong *)(param_2 + uVar9 * 8) =
             CONCAT44((uint)((ulonglong)uVar2 >> 0x20) ^ uVar12,(uint)uVar2 ^ uVar11);
        uVar9 = (ulonglong)((int)uVar9 + 2);
      }
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180007e90
   NAME : StartAudioIVAC
   SIG  : undefined4 __fastcall StartAudioIVAC(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined4 StartAudioIVAC(int param_1)

{
  longlong lVar1;
  int iVar2;
  int iVar3;
  int iVar4;
  undefined4 uVar5;
  longlong lVar6;
  longlong lVar7;
  undefined1 auStack_138 [32];
  undefined4 local_118;
  undefined4 local_110;
  code *local_108;
  longlong local_100;
  undefined4 local_f8;
  undefined4 local_f4;
  undefined4 local_f0;
  undefined4 local_ec;
  undefined4 local_d0;
  undefined4 local_a8;
  undefined4 local_a4;
  undefined4 local_a0;
  undefined4 local_9c;
  undefined4 local_80;
  ulonglong local_58;
  
                    /* 0x7e90  243  StartAudioIVAC */
  local_58 = DAT_18001e000 ^ (ulonglong)auStack_138;
  lVar1 = (&DAT_18001f430)[param_1];
  iVar2 = Pa_HostApiDeviceIndexToDeviceIndex
                    (*(undefined4 *)(lVar1 + 0xb8),*(undefined4 *)(lVar1 + 0xbc));
  iVar3 = Pa_HostApiDeviceIndexToDeviceIndex
                    (*(undefined4 *)(lVar1 + 0xb8),*(undefined4 *)(lVar1 + 0xc0));
  iVar4 = 2;
  if (iVar2 < 1) {
    lVar6 = 0;
  }
  else {
    lVar6 = Pa_GetDeviceInfo(iVar2);
    if ((lVar6 != 0) && (iVar4 = *(int *)(lVar6 + 0x14), 2 < iVar4)) {
      iVar4 = 2;
    }
  }
  lVar7 = 0;
  if (0 < iVar3) {
    lVar7 = Pa_GetDeviceInfo(iVar3);
  }
  *(undefined8 *)(lVar1 + 0x70) = *(undefined8 *)(lVar1 + 0xd8);
  *(int *)(lVar1 + 100) = iVar4;
  *(undefined8 *)(lVar1 + 0x90) = *(undefined8 *)(lVar1 + 0xe0);
  *(int *)(lVar1 + 0x60) = iVar2;
  *(undefined4 *)(lVar1 + 0x68) = 1;
  *(undefined8 *)(lVar1 + 0x78) = 0;
  *(int *)(lVar1 + 0x80) = iVar3;
  *(undefined4 *)(lVar1 + 0x84) = 2;
  *(undefined4 *)(lVar1 + 0x88) = 1;
  *(undefined8 *)(lVar1 + 0x98) = 0;
  if ((((lVar6 != 0) && (*(int *)(lVar1 + 0x14c) != 0)) &&
      (lVar6 = Pa_GetHostApiInfo(*(undefined4 *)(lVar6 + 0x10)), lVar6 != 0)) &&
     (*(int *)(lVar6 + 4) == 0xd)) {
    local_f8 = 0x48;
    local_f4 = 0xd;
    local_f0 = 1;
    local_ec = 0x11;
    local_d0 = 6;
    *(undefined4 **)(lVar1 + 0x78) = &local_f8;
  }
  if (((lVar7 != 0) && (*(int *)(lVar1 + 0x150) != 0)) &&
     ((lVar6 = Pa_GetHostApiInfo(*(undefined4 *)(lVar7 + 0x10)), lVar6 != 0 &&
      (*(int *)(lVar6 + 4) == 0xd)))) {
    local_a8 = 0x48;
    local_a4 = 0xd;
    local_a0 = 1;
    local_9c = 0x11;
    local_80 = 6;
    *(undefined4 **)(lVar1 + 0x98) = &local_a8;
  }
  local_108 = FUN_180007c50;
  local_118 = *(undefined4 *)(lVar1 + 0x30);
  local_110 = 0;
  local_100 = (longlong)param_1;
  iVar4 = Pa_OpenStream(lVar1 + 0xa0,lVar1 + 0x60,lVar1 + 0x80,(double)*(int *)(lVar1 + 0x1c));
  if (iVar4 == 0) {
    iVar4 = Pa_StartStream(*(undefined8 *)(lVar1 + 0xa0));
    uVar5 = 1;
    if (iVar4 != 0) {
      uVar5 = 0xffffffff;
    }
  }
  else {
    uVar5 = 0xffffffff;
  }
  return uVar5;
}



/* ========================================================================
   ENTRY: 1800080c0
   NAME : _guard_check_icall
   SIG  : undefined __fastcall _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
                    /* 0x80c0  77  SetAlex2LPFBits
                       0x80c0  78  SetAlex3HPFBits
                       0x80c0  79  SetAlex3LPFBits
                       0x80c0  80  SetAlex4HPFBits
                       0x80c0  81  SetAlex4LPFBits
                       0x80c0  143  SetIVACRBReset */
  return;
}



/* ========================================================================
   ENTRY: 1800080d0
   NAME : StopAudioIVAC
   SIG  : undefined __fastcall StopAudioIVAC(int param_1)
   ======================================================================== */

void StopAudioIVAC(int param_1)

{
                    /* 0x80d0  246  StopAudioIVAC */
                    /* WARNING: Could not recover jumptable at 0x0001800080e5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  Pa_CloseStream(*(undefined8 *)((&DAT_18001f430)[param_1] + 0xa0));
  return;
}



/* ========================================================================
   ENTRY: 1800080f0
   NAME : SetIVACrun
   SIG  : undefined __fastcall SetIVACrun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACrun(int param_1,undefined4 param_2)

{
                    /* 0x80f0  162  SetIVACrun */
  *(undefined4 *)(&DAT_18001f430)[param_1] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008110
   NAME : SetIVACiqType
   SIG  : undefined __fastcall SetIVACiqType(int param_1, int param_2)
   ======================================================================== */

void SetIVACiqType(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0x8110  153  SetIVACiqType */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 4)) {
    *(int *)(lVar1 + 4) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x00018000819d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 1800081b0
   NAME : SetIVACstereo
   SIG  : undefined __fastcall SetIVACstereo(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACstereo(int param_1,undefined4 param_2)

{
                    /* 0x81b0  164  SetIVACstereo */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 8) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 1800081d0
   NAME : SetIVACvacRate
   SIG  : undefined __fastcall SetIVACvacRate(int param_1, int param_2)
   ======================================================================== */

void SetIVACvacRate(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0x81d0  166  SetIVACvacRate */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x1c)) {
    *(int *)(lVar1 + 0x1c) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x00018000825d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008270
   NAME : SetIVACmicRate
   SIG  : undefined __fastcall SetIVACmicRate(int param_1, int param_2)
   ======================================================================== */

void SetIVACmicRate(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0x8270  154  SetIVACmicRate */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x10)) {
    *(int *)(lVar1 + 0x10) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x0001800082fd. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008310
   NAME : SetIVACaudioRate
   SIG  : undefined __fastcall SetIVACaudioRate(int param_1, int param_2)
   ======================================================================== */

void SetIVACaudioRate(int param_1,int param_2)

{
  longlong lVar1;
  int *piVar2;
  int local_res18;
  undefined4 local_res1c;
  undefined8 in_stack_ffffffffffffff98;
  
                    /* 0x8310  145  SetIVACaudioRate */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x14)) {
    *(int *)(lVar1 + 0x14) = param_2;
    FUN_1800016f0(*(void **)(lVar1 + 0x38),0);
    local_res18 = *(int *)(lVar1 + 0x14);
    local_res1c = *(undefined4 *)(lVar1 + 0x18);
    piVar2 = FUN_180001340(-1,param_1,*(int *)(lVar1 + 0x28),*(int *)(lVar1 + 0x28),2,3,3,
                           DAT_180018dc8,in_stack_ffffffffffffff98,(longlong)&local_res18,
                           local_res18,&LAB_180007c30,0,0,0,0);
    *(int **)(lVar1 + 0x38) = piVar2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x000180008420. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008430
   NAME : FUN_180008430
   SIG  : undefined __fastcall FUN_180008430(int param_1, int param_2)
   ======================================================================== */

void FUN_180008430(int param_1,int param_2)

{
  longlong lVar1;
  int *piVar2;
  int local_res18;
  undefined4 local_res1c;
  undefined8 in_stack_ffffffffffffff98;
  
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x18)) {
    *(int *)(lVar1 + 0x18) = param_2;
    FUN_1800016f0(*(void **)(lVar1 + 0x38),0);
    local_res18 = *(int *)(lVar1 + 0x14);
    local_res1c = *(undefined4 *)(lVar1 + 0x18);
    piVar2 = FUN_180001340(-1,param_1,*(int *)(lVar1 + 0x28),*(int *)(lVar1 + 0x28),2,3,3,
                           DAT_180018dc8,in_stack_ffffffffffffff98,(longlong)&local_res18,
                           local_res18,&LAB_180007c30,0,0,0,0);
    *(int **)(lVar1 + 0x38) = piVar2;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800084fd. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008510
   NAME : SetIVACvacSize
   SIG  : undefined __fastcall SetIVACvacSize(int param_1, int param_2)
   ======================================================================== */

void SetIVACvacSize(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0x8510  167  SetIVACvacSize */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x30)) {
    *(int *)(lVar1 + 0x30) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x00018000859d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 1800085b0
   NAME : SetIVACmicSize
   SIG  : undefined __fastcall SetIVACmicSize(int param_1, int param_2)
   ======================================================================== */

void SetIVACmicSize(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0x85b0  155  SetIVACmicSize */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (param_2 != *(int *)(lVar1 + 0x20)) {
    *(int *)(lVar1 + 0x20) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x00018000863d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008650
   NAME : SetIVACiqSizeAndRate
   SIG  : undefined __fastcall SetIVACiqSizeAndRate(int param_1, int param_2, int param_3)
   ======================================================================== */

void SetIVACiqSizeAndRate(int param_1,int param_2,int param_3)

{
  longlong lVar1;
  
                    /* 0x8650  152  SetIVACiqSizeAndRate */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if ((param_2 != *(int *)(lVar1 + 0x24)) || (param_3 != *(int *)(lVar1 + 0xc))) {
    *(int *)(lVar1 + 0x24) = param_2;
    *(int *)(lVar1 + 0xc) = param_3;
    if (*(int *)(lVar1 + 4) != 0) {
      _aligned_free(*(void **)(lVar1 + 0x40));
      _aligned_free(*(void **)(lVar1 + 0xb0));
      *(undefined8 *)(lVar1 + 0xb0) = 0;
      *(undefined8 *)(lVar1 + 0xa8) = 0;
      destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
      destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
      FUN_1800076c0(lVar1);
    }
  }
                    /* WARNING: Could not recover jumptable at 0x0001800086e1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 1800086f0
   NAME : SetIVACaudioSize
   SIG  : undefined __fastcall SetIVACaudioSize(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACaudioSize(int param_1,undefined4 param_2)

{
  longlong lVar1;
  int *piVar2;
  int local_res18;
  undefined4 local_res1c;
  undefined8 in_stack_ffffffffffffff98;
  
                    /* 0x86f0  146  SetIVACaudioSize */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  *(undefined4 *)(lVar1 + 0x28) = param_2;
  FUN_1800016f0(*(void **)(lVar1 + 0x38),0);
  local_res18 = *(int *)(lVar1 + 0x14);
  local_res1c = *(undefined4 *)(lVar1 + 0x18);
  piVar2 = FUN_180001340(-1,param_1,*(int *)(lVar1 + 0x28),*(int *)(lVar1 + 0x28),2,3,3,
                         DAT_180018dc8,in_stack_ffffffffffffff98,(longlong)&local_res18,local_res18,
                         &LAB_180007c30,0,0,0,0);
  *(int **)(lVar1 + 0x38) = piVar2;
  _aligned_free(*(void **)(lVar1 + 0x40));
  _aligned_free(*(void **)(lVar1 + 0xb0));
  *(undefined8 *)(lVar1 + 0xb0) = 0;
  *(undefined8 *)(lVar1 + 0xa8) = 0;
  destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
  destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
  FUN_1800076c0(lVar1);
                    /* WARNING: Could not recover jumptable at 0x0001800087f7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008800
   NAME : SetIVAChostAPIindex
   SIG  : undefined __fastcall SetIVAChostAPIindex(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVAChostAPIindex(int param_1,undefined4 param_2)

{
                    /* 0x8800  149  SetIVAChostAPIindex */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xb8) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008820
   NAME : SetIVACinputDEVindex
   SIG  : undefined __fastcall SetIVACinputDEVindex(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACinputDEVindex(int param_1,undefined4 param_2)

{
                    /* 0x8820  151  SetIVACinputDEVindex */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xbc) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008840
   NAME : SetIVACoutputDEVindex
   SIG  : undefined __fastcall SetIVACoutputDEVindex(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACoutputDEVindex(int param_1,undefined4 param_2)

{
                    /* 0x8840  160  SetIVACoutputDEVindex */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xc0) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008860
   NAME : SetIVACnumChannels
   SIG  : undefined __fastcall SetIVACnumChannels(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACnumChannels(int param_1,undefined4 param_2)

{
                    /* 0x8860  159  SetIVACnumChannels */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xc4) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008880
   NAME : SetIVACInLatency
   SIG  : undefined __fastcall SetIVACInLatency(int param_1, double param_2)
   ======================================================================== */

void SetIVACInLatency(int param_1,double param_2)

{
  longlong lVar1;
  
                    /* 0x8880  137  SetIVACInLatency */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (*(double *)(lVar1 + 200) != param_2) {
    *(double *)(lVar1 + 200) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x00018000891e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008930
   NAME : SetIVACOutLatency
   SIG  : undefined __fastcall SetIVACOutLatency(int param_1, double param_2)
   ======================================================================== */

void SetIVACOutLatency(int param_1,double param_2)

{
  longlong lVar1;
  
                    /* 0x8930  138  SetIVACOutLatency */
  lVar1 = (&DAT_18001f430)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  if (*(double *)(lVar1 + 0xd0) != param_2) {
    *(double *)(lVar1 + 0xd0) = param_2;
    _aligned_free(*(void **)(lVar1 + 0x40));
    _aligned_free(*(void **)(lVar1 + 0xb0));
    *(undefined8 *)(lVar1 + 0xb0) = 0;
    *(undefined8 *)(lVar1 + 0xa8) = 0;
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
    destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
    FUN_1800076c0(lVar1);
  }
                    /* WARNING: Could not recover jumptable at 0x0001800089ce. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 1800089e0
   NAME : SetIVACPAInLatency
   SIG  : undefined __fastcall SetIVACPAInLatency(int param_1, double param_2)
   ======================================================================== */

void SetIVACPAInLatency(int param_1,double param_2)

{
                    /* 0x89e0  139  SetIVACPAInLatency */
  if (*(double *)((&DAT_18001f430)[param_1] + 0xd8) != param_2) {
    *(double *)((&DAT_18001f430)[param_1] + 0xd8) = param_2;
  }
  return;
}



/* ========================================================================
   ENTRY: 180008a10
   NAME : SetIVACPAOutLatency
   SIG  : undefined __fastcall SetIVACPAOutLatency(int param_1, double param_2)
   ======================================================================== */

void SetIVACPAOutLatency(int param_1,double param_2)

{
                    /* 0x8a10  140  SetIVACPAOutLatency */
  if (*(double *)((&DAT_18001f430)[param_1] + 0xe0) != param_2) {
    *(double *)((&DAT_18001f430)[param_1] + 0xe0) = param_2;
  }
  return;
}



/* ========================================================================
   ENTRY: 180008a40
   NAME : SetIVACvox
   SIG  : undefined __fastcall SetIVACvox(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACvox(int param_1,undefined4 param_2)

{
                    /* 0x8a40  168  SetIVACvox */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xe8) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008a60
   NAME : SetIVACmox
   SIG  : undefined __fastcall SetIVACmox(int param_1, int param_2)
   ======================================================================== */

void SetIVACmox(int param_1,int param_2)

{
  longlong lVar1;
  longlong lVar2;
  
                    /* 0x8a60  158  SetIVACmox */
  lVar1 = (&DAT_18001f430)[param_1];
  lVar2 = *(longlong *)(lVar1 + 0x38);
  *(int *)(lVar1 + 0xec) = param_2;
  if (param_2 == 0) {
    if (*(int *)(lVar1 + 0xf0) == 0) {
      if (lVar2 == 0) {
        lVar2 = DAT_18001e140;
      }
      LOCK();
      *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffd;
      UNLOCK();
    }
    else {
      if (lVar2 == 0) {
        lVar2 = DAT_18001e140;
      }
      LOCK();
      *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 2;
      UNLOCK();
    }
    lVar2 = *(longlong *)(lVar1 + 0x38);
    if (*(longlong *)(lVar1 + 0x38) == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 1;
    UNLOCK();
    return;
  }
  if (*(int *)(lVar1 + 0xf0) != 0) {
    if (lVar2 == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffe;
    UNLOCK();
    lVar2 = *(longlong *)(lVar1 + 0x38);
    if (*(longlong *)(lVar1 + 0x38) == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 2;
    UNLOCK();
    return;
  }
  if (lVar2 == 0) {
    lVar2 = DAT_18001e140;
  }
  LOCK();
  *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffe;
  UNLOCK();
  lVar2 = *(longlong *)(lVar1 + 0x38);
  if (*(longlong *)(lVar1 + 0x38) == 0) {
    lVar2 = DAT_18001e140;
  }
  LOCK();
  *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffd;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180008b30
   NAME : SetIVACmon
   SIG  : undefined __fastcall SetIVACmon(int param_1, int param_2)
   ======================================================================== */

void SetIVACmon(int param_1,int param_2)

{
  longlong lVar1;
  longlong lVar2;
  
                    /* 0x8b30  156  SetIVACmon */
  lVar1 = (&DAT_18001f430)[param_1];
  lVar2 = *(longlong *)(lVar1 + 0x38);
  *(int *)(lVar1 + 0xf0) = param_2;
  if (*(int *)(lVar1 + 0xec) == 0) {
    if (param_2 == 0) {
      if (lVar2 == 0) {
        lVar2 = DAT_18001e140;
      }
      LOCK();
      *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffd;
      UNLOCK();
    }
    else {
      if (lVar2 == 0) {
        lVar2 = DAT_18001e140;
      }
      LOCK();
      *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 2;
      UNLOCK();
    }
    lVar2 = *(longlong *)(lVar1 + 0x38);
    if (*(longlong *)(lVar1 + 0x38) == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 1;
    UNLOCK();
    return;
  }
  if (param_2 != 0) {
    if (lVar2 == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffe;
    UNLOCK();
    lVar2 = *(longlong *)(lVar1 + 0x38);
    if (*(longlong *)(lVar1 + 0x38) == 0) {
      lVar2 = DAT_18001e140;
    }
    LOCK();
    *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) | 2;
    UNLOCK();
    return;
  }
  if (lVar2 == 0) {
    lVar2 = DAT_18001e140;
  }
  LOCK();
  *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffe;
  UNLOCK();
  lVar2 = *(longlong *)(lVar1 + 0x38);
  if (*(longlong *)(lVar1 + 0x38) == 0) {
    lVar2 = DAT_18001e140;
  }
  LOCK();
  *(uint *)(lVar2 + 0x1b0) = *(uint *)(lVar2 + 0x1b0) & 0xfffffffd;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180008c00
   NAME : SetIVACmonVol
   SIG  : undefined __fastcall SetIVACmonVol(int param_1, double param_2)
   ======================================================================== */

void SetIVACmonVol(int param_1,double param_2)

{
  longlong lVar1;
  
                    /* 0x8c00  157  SetIVACmonVol */
  lVar1 = *(longlong *)((&DAT_18001f430)[param_1] + 0x38);
  *(double *)((&DAT_18001f430)[param_1] + 0x110) = param_2;
  if (lVar1 == 0) {
    lVar1 = DAT_18001e140;
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0xc40));
  *(double *)(lVar1 + 0x1c0) = param_2;
  *(double *)(lVar1 + 0x2c0) = param_2 * *(double *)(lVar1 + 0x3b8);
                    /* WARNING: Could not recover jumptable at 0x000180008c72. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0xc40));
  return;
}



/* ========================================================================
   ENTRY: 180008c80
   NAME : SetIVACpreamp
   SIG  : undefined __fastcall SetIVACpreamp(int param_1, undefined8 param_2)
   ======================================================================== */

void SetIVACpreamp(int param_1,undefined8 param_2)

{
                    /* 0x8c80  161  SetIVACpreamp */
  *(undefined8 *)((&DAT_18001f430)[param_1] + 0x100) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008ca0
   NAME : SetIVACrxscale
   SIG  : undefined __fastcall SetIVACrxscale(int param_1, double param_2)
   ======================================================================== */

void SetIVACrxscale(int param_1,double param_2)

{
  longlong lVar1;
  
                    /* 0x8ca0  163  SetIVACrxscale */
  lVar1 = (&DAT_18001f430)[param_1];
  *(double *)(lVar1 + 0x108) = param_2;
  SetAAudioMixVolume(*(longlong *)(lVar1 + 0x38),0,param_2);
  return;
}



/* ========================================================================
   ENTRY: 180008cd0
   NAME : SetIVACbypass
   SIG  : undefined __fastcall SetIVACbypass(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACbypass(int param_1,undefined4 param_2)

{
                    /* 0x8cd0  147  SetIVACbypass */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xf4) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008cf0
   NAME : SetIVACcombine
   SIG  : undefined __fastcall SetIVACcombine(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACcombine(int param_1,undefined4 param_2)

{
                    /* 0x8cf0  148  SetIVACcombine */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0xf8) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180008d10
   NAME : getIVACdiags
   SIG  : undefined __fastcall getIVACdiags(int param_1, int param_2, undefined8 param_3, undefined8 param_4, undefined8 param_5, undefined8 param_6, undefined8 param_7)
   ======================================================================== */

void getIVACdiags(int param_1,int param_2,undefined8 param_3,undefined8 param_4,undefined8 param_5,
                 undefined8 param_6,undefined8 param_7)

{
  longlong lVar1;
  
                    /* 0x8d10  269  getIVACdiags */
  lVar1 = 0x50;
  if (param_2 != 0) {
    lVar1 = 0x48;
  }
  getRMatchDiags(*(undefined8 *)((&DAT_18001f430)[param_1] + lVar1),param_3,param_4,param_5,param_6,
                 param_7);
  return;
}



/* ========================================================================
   ENTRY: 180008d70
   NAME : forceIVACvar
   SIG  : undefined __fastcall forceIVACvar(int param_1, int param_2, undefined4 param_3, undefined8 param_4)
   ======================================================================== */

void forceIVACvar(int param_1,int param_2,undefined4 param_3,undefined8 param_4)

{
  longlong lVar1;
  longlong lVar2;
  
                    /* 0x8d70  256  forceIVACvar */
  lVar1 = (&DAT_18001f430)[param_1];
  if (param_2 == 0) {
    *(undefined8 *)(lVar1 + 0x130) = param_4;
    lVar2 = 0x50;
    *(undefined4 *)(lVar1 + 0x128) = param_3;
  }
  else {
    *(undefined8 *)(lVar1 + 0x120) = param_4;
    lVar2 = 0x48;
    *(undefined4 *)(lVar1 + 0x118) = param_3;
  }
                    /* WARNING: Could not recover jumptable at 0x000180008db6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  forceRMatchVar(*(undefined8 *)(lVar2 + lVar1),param_3,(int)param_4);
  return;
}



/* ========================================================================
   ENTRY: 180008dc0
   NAME : resetIVACdiags
   SIG  : undefined __fastcall resetIVACdiags(int param_1, int param_2)
   ======================================================================== */

void resetIVACdiags(int param_1,int param_2)

{
  undefined8 uVar1;
  longlong lVar2;
  
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  uVar1 = *(undefined8 *)((&DAT_18001f430)[param_1] + lVar2);
  EnterCriticalSection((LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158));
  resetRMatchDiags(uVar1);
                    /* WARNING: Could not recover jumptable at 0x000180008e20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180008e30
   NAME : SetIVACFeedbackGain
   SIG  : undefined __fastcall SetIVACFeedbackGain(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACFeedbackGain(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x8e30  136  SetIVACFeedbackGain */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchFeedbackGain(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180008e93. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180008ea0
   NAME : SetIVACSlewTime
   SIG  : undefined __fastcall SetIVACSlewTime(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACSlewTime(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x8ea0  144  SetIVACSlewTime */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchSlewTime1(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180008f03. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180008f10
   NAME : SetIVACPropRingMin
   SIG  : undefined __fastcall SetIVACPropRingMin(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACPropRingMin(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x8f10  142  SetIVACPropRingMin */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchPropRingMin(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180008f72. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180008f80
   NAME : SetIVACPropRingMax
   SIG  : undefined __fastcall SetIVACPropRingMax(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACPropRingMax(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x8f80  141  SetIVACPropRingMax */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchPropRingMax(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180008fe2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180008ff0
   NAME : SetIVACFFRingMin
   SIG  : undefined __fastcall SetIVACFFRingMin(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACFFRingMin(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x8ff0  135  SetIVACFFRingMin */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchFFRingMin(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180009052. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180009060
   NAME : SetIVACFFRingMax
   SIG  : undefined __fastcall SetIVACFFRingMax(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACFFRingMax(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x9060  134  SetIVACFFRingMax */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchFFRingMax(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x0001800090c2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 1800090d0
   NAME : SetIVACFFAlpha
   SIG  : undefined __fastcall SetIVACFFAlpha(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetIVACFFAlpha(int param_1,int param_2,undefined4 param_3)

{
  LPCRITICAL_SECTION lpCriticalSection;
  undefined8 uVar1;
  longlong lVar2;
  
                    /* 0x90d0  133  SetIVACFFAlpha */
  lVar2 = 0x50;
  if (param_2 != 0) {
    lVar2 = 0x48;
  }
  lpCriticalSection = (LPCRITICAL_SECTION)((&DAT_18001f430)[param_1] + 0x158);
  uVar1 = *(undefined8 *)(lVar2 + (&DAT_18001f430)[param_1]);
  EnterCriticalSection(lpCriticalSection);
  setRMatchFFAlpha(uVar1,param_3);
                    /* WARNING: Could not recover jumptable at 0x000180009133. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection(lpCriticalSection);
  return;
}



/* ========================================================================
   ENTRY: 180009140
   NAME : GetIVACControlFlag
   SIG  : undefined __fastcall GetIVACControlFlag(int param_1, int param_2, undefined8 param_3)
   ======================================================================== */

void GetIVACControlFlag(int param_1,int param_2,undefined8 param_3)

{
  longlong lVar1;
  
                    /* 0x9140  30  GetIVACControlFlag */
  lVar1 = 0x50;
  if (param_2 != 0) {
    lVar1 = 0x48;
  }
                    /* WARNING: Could not recover jumptable at 0x000180009166. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  getControlFlag(*(undefined8 *)((&DAT_18001f430)[param_1] + lVar1),param_3);
  return;
}



/* ========================================================================
   ENTRY: 180009170
   NAME : SetIVACinitialVars
   SIG  : undefined __fastcall SetIVACinitialVars(int param_1, double param_2, double param_3)
   ======================================================================== */

void SetIVACinitialVars(int param_1,double param_2,double param_3)

{
  longlong lVar1;
  bool bVar2;
  
                    /* 0x9170  150  SetIVACinitialVars */
  lVar1 = (&DAT_18001f430)[param_1];
  bVar2 = param_2 != *(double *)(lVar1 + 0x138);
  if (bVar2) {
    *(double *)(lVar1 + 0x138) = param_2;
  }
  if (param_3 == *(double *)(lVar1 + 0x140)) {
    if (!bVar2) {
      return;
    }
  }
  else {
    *(double *)(lVar1 + 0x140) = param_3;
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  _aligned_free(*(void **)(lVar1 + 0x40));
  _aligned_free(*(void **)(lVar1 + 0xb0));
  *(undefined8 *)(lVar1 + 0xb0) = 0;
  *(undefined8 *)(lVar1 + 0xa8) = 0;
  destroy_rmatchV(*(undefined8 *)(lVar1 + 0x50));
  destroy_rmatchV(*(undefined8 *)(lVar1 + 0x48));
  FUN_1800076c0(lVar1);
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x158));
  return;
}



/* ========================================================================
   ENTRY: 180009230
   NAME : SetIVACswapIQout
   SIG  : undefined __fastcall SetIVACswapIQout(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACswapIQout(int param_1,undefined4 param_2)

{
                    /* 0x9230  165  SetIVACswapIQout */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0x148) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180009250
   NAME : SetIVACExclusiveOut
   SIG  : undefined __fastcall SetIVACExclusiveOut(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACExclusiveOut(int param_1,undefined4 param_2)

{
                    /* 0x9250  132  SetIVACExclusiveOut */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0x150) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180009270
   NAME : SetIVACExclusiveIn
   SIG  : undefined __fastcall SetIVACExclusiveIn(int param_1, undefined4 param_2)
   ======================================================================== */

void SetIVACExclusiveIn(int param_1,undefined4 param_2)

{
                    /* 0x9270  131  SetIVACExclusiveIn */
  *(undefined4 *)((&DAT_18001f430)[param_1] + 0x14c) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180009290
   NAME : StartAudioNative
   SIG  : undefined8 __fastcall StartAudioNative(undefined8 param_1, undefined8 param_2, char * param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 StartAudioNative(undefined8 param_1,undefined8 param_2,char *param_3)

{
  longlong lVar1;
  longlong lVar2;
  undefined4 uVar3;
  uint uVar4;
  int iVar5;
  FILE *_File;
  HANDLE pvVar6;
  uintptr_t uVar7;
  undefined8 uVar8;
  undefined8 uVar9;
  ulonglong uVar10;
  bool bVar11;
  undefined1 auStackY_c8 [32];
  wchar_t local_98 [64];
  ulonglong local_18;
  
  iVar5 = DAT_180020568;
  lVar1 = DAT_180020530;
                    /* 0x9290  244  StartAudioNative */
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_c8;
  if (DAT_18001e1d4 != 0) {
    return 0;
  }
  DAT_18001e1d4 = 1;
  DAT_1800205c8 = 1;
  if ((DAT_1800205ac == 0) && (DAT_180020530 != 0)) {
    uVar9 = 0;
    *(uint *)(DAT_180020530 + 0x318) = (DAT_180020568 != 0) + 0x3f;
    bVar11 = iVar5 == 0;
    uVar3 = 0xee;
    if (bVar11) {
      uVar3 = 0x3f;
    }
    uVar8 = 0x7e;
    *(undefined4 *)(lVar1 + 0x35c) = uVar3;
    *(undefined4 *)(lVar1 + 0x464) = uVar3;
    *(undefined4 *)(lVar1 + 0x56c) = uVar3;
    *(undefined4 *)(lVar1 + 0x674) = uVar3;
    *(undefined4 *)(lVar1 + 0x77c) = uVar3;
    *(undefined4 *)(lVar1 + 0x884) = uVar3;
    *(undefined4 *)(lVar1 + 0x98c) = uVar3;
    *(undefined4 *)(lVar1 + 0xa94) = uVar3;
    *(undefined4 *)(lVar1 + 0xb9c) = uVar3;
    *(undefined4 *)(lVar1 + 0xca4) = uVar3;
    *(undefined4 *)(lVar1 + 0xdac) = uVar3;
    *(undefined4 *)(lVar1 + 0xeb4) = uVar3;
    uVar3 = 0xf0;
    if (bVar11) {
      uVar3 = 0x7e;
    }
    *(undefined4 *)(lVar1 + 0xfc8) = uVar3;
    *(undefined4 *)(lVar1 + 0x1014) = uVar3;
    *(undefined4 *)(lVar1 + 0x1060) = uVar3;
    uVar3 = 0x40;
    if (bVar11) {
      uVar3 = 0x7e;
    }
    *(undefined4 *)(lVar1 + 0x1064) = uVar3;
    *(undefined4 *)(lVar1 + 0x1068) = uVar3;
    FUN_180011560(0,0x7e,param_3,*(int *)(lVar1 + 0x1064));
    uVar10 = (ulonglong)*(uint *)(DAT_180020530 + 0xfc8);
    FUN_180011560(1,uVar8,param_3,*(uint *)(DAT_180020530 + 0xfc8));
    if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
      *(int *)(PTR_DAT_18001e078 + 0x4c) = DAT_180020568;
      uVar4 = asioStart();
      param_3 = "asioStart = %d";
      uVar8 = 0x80;
      uVar10 = (ulonglong)uVar4;
      FID_conflict_sprintf_s(local_98,0x80,(wchar_t *)"asioStart = %d");
      OutputDebugStringA((LPCSTR)local_98);
      *(undefined4 *)(PTR_DAT_18001e078 + 0x24) = 1;
    }
    if (DAT_180020568 == 0) {
      iVar5 = SendStartToMetis();
      lVar1 = DAT_180020530;
      if (iVar5 == 0) {
        if (DAT_180020530 != 0) {
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          *(HANDLE *)(lVar1 + 0x98) = pvVar6;
          lVar1 = DAT_180020530;
          uVar7 = _beginthreadex((void *)0x0,0,FUN_18000f950,(void *)0x0,0,(uint *)0x0);
          lVar2 = DAT_180020530;
          *(uintptr_t *)(lVar1 + 0x90) = uVar7;
          WaitForSingleObject(*(HANDLE *)(lVar2 + 0x98),0xffffffff);
          lVar1 = DAT_180020530;
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          *(HANDLE *)(lVar1 + 0xa8) = pvVar6;
          lVar1 = DAT_180020530;
          uVar7 = _beginthreadex((void *)0x0,0,FUN_180011050,(void *)0x0,0,(uint *)0x0);
          *(uintptr_t *)(lVar1 + 0xa0) = uVar7;
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          lVar1 = DAT_180020530;
          *(HANDLE *)(DAT_180020530 + 0xb0) = pvVar6;
          *(HANDLE *)(lVar1 + 0xc0) = pvVar6;
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          lVar1 = DAT_180020530;
          *(HANDLE *)(DAT_180020530 + 0xb8) = pvVar6;
          *(HANDLE *)(lVar1 + 200) = pvVar6;
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          *(HANDLE *)(lVar1 + 0xd0) = pvVar6;
          lVar1 = DAT_180020530;
          pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
          *(HANDLE *)(lVar1 + 0xd8) = pvVar6;
        }
      }
      else {
        FUN_180007660("SendStart failed ...\n",uVar8,param_3,uVar10);
        _File = (FILE *)__acrt_iob_func(1);
        fflush(_File);
        FUN_18000c3b0();
        uVar9 = 0xfffffffd;
        FUN_18000c3b0();
      }
    }
    else {
      uVar9 = 0;
      *(undefined4 *)(DAT_180020530 + 0x40) = 1;
      FUN_18000d740();
      CmdRx();
      FUN_18000e1c0();
      FUN_18000d990();
      lVar1 = DAT_180020530;
      pvVar6 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1,(LPCWSTR)0x0);
      *(HANDLE *)(lVar1 + 0x98) = pvVar6;
      lVar1 = DAT_180020530;
      uVar7 = _beginthreadex((void *)0x0,0,FUN_18000ed60,(void *)0x0,0,(uint *)0x0);
      lVar2 = DAT_180020530;
      *(uintptr_t *)(lVar1 + 0x90) = uVar7;
      WaitForSingleObject(*(HANDLE *)(lVar2 + 0x98),0xffffffff);
      lVar1 = DAT_180020530;
      uVar7 = _beginthreadex((void *)0x0,0,FUN_18000edf0,(void *)0x0,0,(uint *)0x0);
      *(uintptr_t *)(lVar1 + 0xe0) = uVar7;
    }
    return uVar9;
  }
  return 3;
}



/* ========================================================================
   ENTRY: 1800096e0
   NAME : StopAudio
   SIG  : undefined __fastcall StopAudio(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void StopAudio(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  uint uVar1;
  FILE *pFVar2;
  undefined8 uVar3;
  char *pcVar4;
  undefined1 auStack_b8 [32];
  wchar_t local_98 [64];
  ulonglong local_18;
  
                    /* 0x96e0  245  StopAudio */
  local_18 = DAT_18001e000 ^ (ulonglong)auStack_b8;
  if (DAT_18001e1d4 != 0) {
    DAT_18001e1d4 = 0;
    FUN_180007660("stop audio called\n",param_2,param_3,param_4);
    pFVar2 = (FILE *)__acrt_iob_func(1);
    fflush(pFVar2);
    if (DAT_180020514 == 0) {
      uVar3 = __acrt_iob_func(2);
      param_3 = 1;
      pcVar4 = "Warning: IOThreadStop failed with rc=%d\n";
      FUN_180006cb0(uVar3,"Warning: IOThreadStop failed with rc=%d\n",1,param_4);
    }
    else {
      pcVar4 = (char *)0xffffffff;
      DAT_180020514 = 0;
      WaitForSingleObject(*(HANDLE *)(DAT_180020530 + 0x90),0xffffffff);
      CloseHandle(*(HANDLE *)(DAT_180020530 + 0x90));
      CloseHandle(*(HANDLE *)(DAT_180020530 + 0x98));
      if (DAT_180020568 == 0) {
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xa0));
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xa8));
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xb8));
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xb0));
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xd0));
        CloseHandle(*(HANDLE *)(DAT_180020530 + 0xd8));
      }
    }
    FUN_180007660("iothread stopped\n",pcVar4,param_3,param_4);
    pFVar2 = (FILE *)__acrt_iob_func(1);
    fflush(pFVar2);
    FUN_18000c3b0();
    if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
      *(undefined4 *)(PTR_DAT_18001e078 + 0x24) = 0;
      uVar1 = asioStop();
      FID_conflict_sprintf_s(local_98,0x80,(wchar_t *)"asioStop = %d",(ulonglong)uVar1);
      OutputDebugStringA((LPCSTR)local_98);
    }
    FUN_180011650(0);
    FUN_180011650(1);
  }
  return;
}



/* ========================================================================
   ENTRY: 1800098d0
   NAME : nativeGetDotDashPTT
   SIG  : uint __fastcall nativeGetDotDashPTT(void)
   ======================================================================== */

uint nativeGetDotDashPTT(void)

{
                    /* 0x98d0  293  nativeGetDotDashPTT */
  return (*(uint *)(DAT_180020530 + 0x5c) | *(uint *)(DAT_180020530 + 0x58) |
         *(uint *)(DAT_180020530 + 0x54)) & 7;
}



/* ========================================================================
   ENTRY: 1800098f0
   NAME : getAndResetADC_Overload
   SIG  : uint __fastcall getAndResetADC_Overload(void)
   ======================================================================== */

uint getAndResetADC_Overload(void)

{
  int iVar1;
  uint uVar2;
  uint uVar3;
  longlong lVar4;
  
                    /* 0x98f0  259  getAndResetADC_Overload */
  lVar4 = DAT_180020530;
  if ((DAT_180020530 != 0) && (DAT_180020530 != -0x248)) {
    iVar1 = *(int *)(DAT_180020530 + 0x2c8);
    uVar2 = *(uint *)(DAT_180020530 + 0x290);
    uVar3 = *(uint *)(DAT_180020530 + 600);
    *(undefined4 *)(DAT_180020530 + 600) = 0;
    *(undefined4 *)(lVar4 + 0x290) = 0;
    *(undefined4 *)(lVar4 + 0x2c8) = 0;
    return (iVar1 * 2 | uVar2) * 2 | uVar3;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180009940
   NAME : getAndResetADCmaxMagnitudeAtOverload
   SIG  : undefined2 __fastcall getAndResetADCmaxMagnitudeAtOverload(uint param_1)
   ======================================================================== */

undefined2 getAndResetADCmaxMagnitudeAtOverload(uint param_1)

{
  undefined2 uVar1;
  
                    /* 0x9940  260  getAndResetADCmaxMagnitudeAtOverload */
  if (((param_1 < 3) && (DAT_180020530 != 0)) && (DAT_180020530 != -0x248)) {
    uVar1 = *(undefined2 *)((longlong)(int)param_1 * 0x38 + 0x27a + DAT_180020530);
    *(undefined2 *)((longlong)(int)param_1 * 0x38 + 0x27a + DAT_180020530) = 0;
    return uVar1;
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180009990
   NAME : getADCmaxMagnitude
   SIG  : undefined2 __fastcall getADCmaxMagnitude(uint param_1)
   ======================================================================== */

undefined2 getADCmaxMagnitude(uint param_1)

{
                    /* 0x9990  258  getADCmaxMagnitude */
  if (((param_1 < 3) && (DAT_180020530 != 0)) && (DAT_180020530 != -0x248)) {
    return *(undefined2 *)((longlong)(int)param_1 * 0x38 + 0x278 + DAT_180020530);
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800099d0
   NAME : getOOO
   SIG  : undefined __fastcall getOOO(void)
   ======================================================================== */

void getOOO(void)

{
  longlong lVar1;
  uint uVar2;
  uint uVar3;
  
                    /* 0x99d0  272  getOOO */
  lVar1 = DAT_180020530;
  uVar3 = (uint)(*(int *)(DAT_180020530 + 0x8c) != 0);
  if (*(int *)(DAT_180020530 + 0x344) != 0) {
    uVar3 = (*(int *)(DAT_180020530 + 0x8c) != 0) + 2;
  }
  uVar2 = uVar3 | 4;
  if (*(int *)(DAT_180020530 + 0x44c) == 0) {
    uVar2 = uVar3;
  }
  uVar3 = uVar2 | 8;
  if (*(int *)(DAT_180020530 + 0x554) == 0) {
    uVar3 = uVar2;
  }
  uVar2 = uVar3 | 0x10;
  if (*(int *)(DAT_180020530 + 0x65c) == 0) {
    uVar2 = uVar3;
  }
  uVar3 = uVar2 | 0x20;
  if (*(int *)(DAT_180020530 + 0x764) == 0) {
    uVar3 = uVar2;
  }
  uVar2 = uVar3 | 0x40;
  if (*(int *)(DAT_180020530 + 0x86c) == 0) {
    uVar2 = uVar3;
  }
  uVar3 = uVar2 | 0x80;
  if (*(int *)(DAT_180020530 + 0x974) == 0) {
    uVar3 = uVar2;
  }
  uVar2 = uVar3 | 0x100;
  if (*(int *)(DAT_180020530 + 0xfc0) == 0) {
    uVar2 = uVar3;
  }
  if (uVar2 != 0) {
    *(undefined4 *)(DAT_180020530 + 0x8c) = 0;
    *(undefined4 *)(lVar1 + 0x344) = 0;
    *(undefined4 *)(lVar1 + 0x44c) = 0;
    *(undefined4 *)(lVar1 + 0x554) = 0;
    *(undefined4 *)(lVar1 + 0x65c) = 0;
    *(undefined4 *)(lVar1 + 0x764) = 0;
    *(undefined4 *)(lVar1 + 0x86c) = 0;
    *(undefined4 *)(lVar1 + 0x974) = 0;
    *(undefined4 *)(lVar1 + 0xfc0) = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 180009ab0
   NAME : getSeqInDelta
   SIG  : bool __fastcall getSeqInDelta(int param_1, int param_2, undefined8 * param_3, undefined4 * param_4, undefined4 * param_5, undefined4 * param_6)
   ======================================================================== */

bool getSeqInDelta(int param_1,int param_2,undefined8 *param_3,undefined4 *param_4,
                  undefined4 *param_5,undefined4 *param_6)

{
  longlong lVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined8 uVar5;
  longlong lVar6;
  longlong lVar7;
  longlong lVar8;
  
                    /* 0x9ab0  274  getSeqInDelta */
  lVar8 = (longlong)param_2;
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x198));
  lVar6 = DAT_180020530;
  if (param_1 == 1) {
    *(undefined8 *)((lVar8 + 4) * 0x108 + DAT_180020530) =
         *(undefined8 *)(lVar8 * 0x108 + 0x408 + DAT_180020530);
  }
  lVar7 = (lVar8 + 4) * 0x108;
  lVar8 = *(longlong *)(lVar7 + lVar6);
  if (lVar8 != 0) {
    uVar5 = *(undefined8 *)(lVar8 + 0x18);
    *param_3 = *(undefined8 *)(lVar8 + 0x10);
    param_3[1] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x28);
    param_3[2] = *(undefined8 *)(lVar8 + 0x20);
    param_3[3] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x38);
    param_3[4] = *(undefined8 *)(lVar8 + 0x30);
    param_3[5] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x48);
    param_3[6] = *(undefined8 *)(lVar8 + 0x40);
    param_3[7] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x58);
    param_3[8] = *(undefined8 *)(lVar8 + 0x50);
    param_3[9] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x68);
    param_3[10] = *(undefined8 *)(lVar8 + 0x60);
    param_3[0xb] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x78);
    param_3[0xc] = *(undefined8 *)(lVar8 + 0x70);
    param_3[0xd] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x88);
    param_3[0xe] = *(undefined8 *)(lVar8 + 0x80);
    param_3[0xf] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0x98);
    param_3[0x10] = *(undefined8 *)(lVar8 + 0x90);
    param_3[0x11] = uVar5;
    uVar5 = *(undefined8 *)(lVar8 + 0xa8);
    param_3[0x12] = *(undefined8 *)(lVar8 + 0xa0);
    param_3[0x13] = uVar5;
    lVar1 = *(longlong *)(lVar7 + lVar6);
    uVar2 = *(undefined4 *)(lVar1 + 0xb4);
    uVar3 = *(undefined4 *)(lVar1 + 0xb8);
    uVar4 = *(undefined4 *)(lVar1 + 0xbc);
    *param_4 = *(undefined4 *)(lVar1 + 0xb0);
    param_4[1] = uVar2;
    param_4[2] = uVar3;
    param_4[3] = uVar4;
    *(undefined8 *)(param_4 + 4) = *(undefined8 *)(lVar1 + 0xc0);
    *param_5 = *(undefined4 *)(*(longlong *)(lVar7 + lVar6) + 200);
    *param_6 = *(undefined4 *)(*(longlong *)(lVar7 + lVar6) + 0xcc);
    *(undefined8 *)(lVar7 + lVar6) = **(undefined8 **)(lVar7 + lVar6);
  }
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar6 + 0x198));
  return lVar8 != 0;
}



/* ========================================================================
   ENTRY: 180009bf0
   NAME : GetPLLLock
   SIG  : uint __fastcall GetPLLLock(void)
   ======================================================================== */

uint GetPLLLock(void)

{
                    /* 0x9bf0  35  GetPLLLock */
  return *(uint *)(DAT_180020530 + 0x60) >> 4 & 1;
}



/* ========================================================================
   ENTRY: 180009c10
   NAME : getUserI01
   SIG  : uint __fastcall getUserI01(void)
   ======================================================================== */

uint getUserI01(void)

{
                    /* 0x9c10  279  getUserI01
                       0x9c10  284  getUserI04_p2 */
  return *(uint *)(DAT_180020530 + 0x80) & 1;
}



/* ========================================================================
   ENTRY: 180009c30
   NAME : getUserI02
   SIG  : uint __fastcall getUserI02(void)
   ======================================================================== */

uint getUserI02(void)

{
                    /* 0x9c30  280  getUserI02
                       0x9c30  285  getUserI05_p2 */
  return *(uint *)(DAT_180020530 + 0x80) >> 1 & 1;
}



/* ========================================================================
   ENTRY: 180009c50
   NAME : getUserI03
   SIG  : uint __fastcall getUserI03(void)
   ======================================================================== */

uint getUserI03(void)

{
                    /* 0x9c50  282  getUserI03
                       0x9c50  286  getUserI06_p2 */
  return *(uint *)(DAT_180020530 + 0x80) >> 2 & 1;
}



/* ========================================================================
   ENTRY: 180009c70
   NAME : getUserI04
   SIG  : uint __fastcall getUserI04(void)
   ======================================================================== */

uint getUserI04(void)

{
                    /* 0x9c70  283  getUserI04
                       0x9c70  287  getUserI08_p2 */
  return *(uint *)(DAT_180020530 + 0x80) >> 3 & 1;
}



/* ========================================================================
   ENTRY: 180009c90
   NAME : getUserI02_p2
   SIG  : uint __fastcall getUserI02_p2(void)
   ======================================================================== */

uint getUserI02_p2(void)

{
                    /* 0x9c90  281  getUserI02_p2 */
  return *(uint *)(DAT_180020530 + 0x80) >> 4 & 1;
}



/* ========================================================================
   ENTRY: 180009cb0
   NAME : getExciterPower
   SIG  : undefined4 __fastcall getExciterPower(void)
   ======================================================================== */

undefined4 getExciterPower(void)

{
                    /* 0x9cb0  265  getExciterPower */
  return *(undefined4 *)(DAT_180020530 + 4000);
}



/* ========================================================================
   ENTRY: 180009cc0
   NAME : getFwdPower
   SIG  : undefined4 __fastcall getFwdPower(void)
   ======================================================================== */

undefined4 getFwdPower(void)

{
                    /* 0x9cc0  266  getFwdPower */
  return DAT_1800205a8;
}



/* ========================================================================
   ENTRY: 180009cd0
   NAME : getRevPower
   SIG  : undefined4 __fastcall getRevPower(void)
   ======================================================================== */

undefined4 getRevPower(void)

{
                    /* 0x9cd0  273  getRevPower */
  return DAT_1800205c4;
}



/* ========================================================================
   ENTRY: 180009ce0
   NAME : getUserADC0
   SIG  : undefined4 __fastcall getUserADC0(void)
   ======================================================================== */

undefined4 getUserADC0(void)

{
                    /* 0x9ce0  275  getUserADC0 */
  return *(undefined4 *)(DAT_180020530 + 0x70);
}



/* ========================================================================
   ENTRY: 180009cf0
   NAME : getUserADC1
   SIG  : undefined4 __fastcall getUserADC1(void)
   ======================================================================== */

undefined4 getUserADC1(void)

{
                    /* 0x9cf0  276  getUserADC1 */
  return *(undefined4 *)(DAT_180020530 + 0x74);
}



/* ========================================================================
   ENTRY: 180009d00
   NAME : getUserADC2
   SIG  : undefined4 __fastcall getUserADC2(void)
   ======================================================================== */

undefined4 getUserADC2(void)

{
                    /* 0x9d00  277  getUserADC2 */
  return *(undefined4 *)(DAT_180020530 + 0x78);
}



/* ========================================================================
   ENTRY: 180009d10
   NAME : getUserADC3
   SIG  : undefined4 __fastcall getUserADC3(void)
   ======================================================================== */

undefined4 getUserADC3(void)

{
                    /* 0x9d10  278  getUserADC3 */
  return *(undefined4 *)(DAT_180020530 + 0x7c);
}



/* ========================================================================
   ENTRY: 180009d20
   NAME : getHermesDCVoltage
   SIG  : undefined4 __fastcall getHermesDCVoltage(void)
   ======================================================================== */

undefined4 getHermesDCVoltage(void)

{
                    /* 0x9d20  268  getHermesDCVoltage */
  return *(undefined4 *)(DAT_180020530 + 0x6c);
}



/* ========================================================================
   ENTRY: 180009d30
   NAME : SetPttOut
   SIG  : undefined __fastcall SetPttOut(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetPttOut(uint param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0x9d30  182  SetPttOut */
  lVar2 = DAT_180020530;
  if (((*(uint *)(DAT_180020530 + 0xf98) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, DAT_180020574 = param_1,
      *(uint *)(DAT_180020530 + 0xf98) = param_1 & 1, bVar9)) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009d70
   NAME : SetTRXrelay
   SIG  : undefined __fastcall SetTRXrelay(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetTRXrelay(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  longlong lVar3;
  SOCKET s;
  longlong lVar4;
  u_short uVar5;
  int iVar6;
  uint uVar7;
  byte bVar8;
  undefined8 uVar9;
  undefined8 uVar10;
  bool bVar11;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
  lVar4 = DAT_1800205b0;
                    /* 0x9d70  219  SetTRXrelay */
  lVar3 = DAT_180020530;
  bVar8 = *(byte *)(DAT_1800205b0 + 7);
  if ((bVar8 >> 3 & 1) == param_1) {
    return;
  }
  if (*(int *)(DAT_180020530 + 0xfb8) == 0) {
    bVar8 = ((char)param_1 << 3 ^ bVar8) & 8 ^ bVar8;
    *(byte *)(DAT_1800205b0 + 7) = bVar8;
  }
  bVar1 = *(byte *)(lVar4 + 6);
  *(byte *)(lVar4 + 6) = (bVar1 ^ bVar8 >> 1) & 4 ^ bVar1;
  bVar11 = DAT_180020598 == 0xffffffffffffffff;
  *(byte *)(DAT_1800205d0 + 6) =
       (*(byte *)(DAT_1800205d0 + 6) ^ bVar8 >> 1) & 4 ^ *(byte *)(DAT_1800205d0 + 6);
  if (bVar11) {
    return;
  }
  if (*(int *)(lVar3 + 0x48) == 0) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar6 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar6 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar6 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar6 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar6 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar6 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar6 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar6 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar6 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar2 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar2 >> 8);
  uStack_5b4 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar2 >> 8);
  uStack_5b0 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar2 >> 8);
  uStack_5ac = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar2 >> 8);
  uStack_5a8 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar2 >> 8);
  uStack_5a4 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar2 >> 8);
  uStack_5a0 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar2 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar2 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar2 >> 8);
  uStack_59c = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar2 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar2 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar2 >> 8);
  uStack_598 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar2 >> 8);
  uStack_594 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar2 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar2 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar2 >> 8);
  uStack_590 = (undefined1)uVar2;
  uVar2 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar2 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar2 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar2 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar2;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar8 = 0;
  if (DAT_18002056c == 0) {
    bVar8 = 2;
  }
  bStack_50 = bVar8 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar2 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar5 = htons((short)uVar2 + 3);
    uVar10 = 0;
    uVar9 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar5;
    sStack_5d8.sa_data[1] = (char)(uVar5 >> 8);
    sStack_5d8.sa_family = 2;
    iVar6 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar6 < 1) {
      if (iVar6 == -1) {
        uVar7 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar7,uVar9,uVar10);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar6;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009e00
   NAME : EnableEClassModulation
   SIG  : undefined __fastcall EnableEClassModulation(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void EnableEClassModulation(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0x9e00  19  EnableEClassModulation */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = (bVar1 ^ (byte)param_1) & 1 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180009e40
   NAME : SetOCBits
   SIG  : undefined __fastcall SetOCBits(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetOCBits(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0x9e40  177  SetOCBits */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 100) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 100) = param_1, bVar9))
     || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009e70
   NAME : SetOCExtraBits
   SIG  : undefined __fastcall SetOCExtraBits(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetOCExtraBits(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0x9e70  178  SetOCExtraBits */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0x68) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x68) = param_1, bVar9)
      ) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009ea0
   NAME : SetAlexAtten
   SIG  : undefined __fastcall SetAlexAtten(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetAlexAtten(uint param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0x9ea0  82  SetAlexAtten */
  if ((((DAT_180020518 != 0) ||
       (bVar5 = *(byte *)(DAT_1800205b0 + 5), ((bVar5 & 0x60) != 0) == param_1)) ||
      (bVar5 = (bVar5 ^ (char)(param_1 >> 1) << 5) & 0x20 ^ bVar5,
      bVar8 = DAT_180020598 == 0xffffffffffffffff,
      *(byte *)(DAT_1800205b0 + 5) = ((char)param_1 << 6 ^ bVar5) & 0x40 ^ bVar5, bVar8)) ||
     (*(int *)(DAT_180020530 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009f10
   NAME : SetADCDither
   SIG  : undefined __fastcall SetADCDither(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetADCDither(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  undefined1 uStack_5c4;
  byte bStack_5c3;
  byte bStack_5c2;
  byte bStack_5c1;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_58e;
  undefined1 uStack_75;
  ulonglong uStack_18;
  
                    /* 0x9f10  70  SetADCDither */
  lVar2 = DAT_180020530;
  if (*(int *)(DAT_180020530 + 0x25c) != param_1) {
    bVar8 = DAT_180020598 != 0xffffffffffffffff;
    *(int *)(DAT_180020530 + 0x2cc) = param_1;
    *(int *)(lVar2 + 0x294) = param_1;
    *(int *)(lVar2 + 0x25c) = param_1;
    if (bVar8) {
      uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
      memset(acStack_5c8,0,0x5a4);
      s = DAT_180020598;
      uStack_5c4 = *(undefined1 *)(DAT_180020530 + 0x4c);
      uStack_5b5 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x334);
      bStack_5c3 = ((*(char *)(DAT_180020530 + 0x2cc) * '\x02' | *(byte *)(DAT_180020530 + 0x294)) *
                    '\x02' | *(byte *)(DAT_180020530 + 0x25c)) & 7;
      bStack_5c2 = ((*(char *)(DAT_180020530 + 0x2d0) * '\x02' | *(byte *)(DAT_180020530 + 0x298)) *
                    '\x02' | *(byte *)(DAT_180020530 + 0x260)) & 7;
      bStack_5c1 = (((((*(char *)(DAT_180020530 + 0x95c) * '\x02' | *(byte *)(DAT_180020530 + 0x854)
                       ) * '\x02' | *(byte *)(DAT_180020530 + 0x74c)) * '\x02' |
                     *(byte *)(DAT_180020530 + 0x644)) * '\x02' | *(byte *)(DAT_180020530 + 0x53c))
                    * '\x02' | *(byte *)(DAT_180020530 + 0x434)) * '\x02' |
                   *(byte *)(DAT_180020530 + 0x32c);
      uStack_5b7 = *(undefined1 *)(DAT_180020530 + 0x324);
      uStack_5b6 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x334) >> 8);
      uStack_5b2 = *(undefined1 *)(DAT_180020530 + 0x338);
      uStack_5b1 = *(undefined1 *)(DAT_180020530 + 0x42c);
      uStack_5b0 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x43c) >> 8);
      uStack_5ac = *(undefined1 *)(DAT_180020530 + 0x440);
      uStack_5ab = *(undefined1 *)(DAT_180020530 + 0x534);
      uStack_5af = (undefined1)*(undefined4 *)(DAT_180020530 + 0x43c);
      uStack_5aa = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x544) >> 8);
      uStack_5a6 = *(undefined1 *)(DAT_180020530 + 0x548);
      uStack_5a5 = *(undefined1 *)(DAT_180020530 + 0x63c);
      uStack_5a9 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x544);
      uStack_5a4 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x64c) >> 8);
      uStack_5a0 = *(undefined1 *)(DAT_180020530 + 0x650);
      uStack_59f = *(undefined1 *)(DAT_180020530 + 0x744);
      uStack_5a3 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x64c);
      uStack_59e = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x754) >> 8);
      uStack_59a = *(undefined1 *)(DAT_180020530 + 0x758);
      uStack_599 = *(undefined1 *)(DAT_180020530 + 0x84c);
      uStack_59d = (undefined1)*(undefined4 *)(DAT_180020530 + 0x754);
      uStack_598 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x85c) >> 8);
      uStack_594 = *(undefined1 *)(DAT_180020530 + 0x860);
      uStack_593 = *(undefined1 *)(DAT_180020530 + 0x954);
      uStack_597 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x85c);
      uStack_592 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x964) >> 8);
      uStack_58e = *(undefined1 *)(DAT_180020530 + 0x968);
      uStack_75 = *(undefined1 *)(DAT_180020530 + 0x330);
      uStack_591 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x964);
      if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
        uVar1 = *(undefined4 *)(DAT_180020530 + 4);
        sStack_5d8.sa_family = 0;
        sStack_5d8.sa_data[0] = '\0';
        sStack_5d8.sa_data[1] = '\0';
        sStack_5d8.sa_data[2] = '\0';
        sStack_5d8.sa_data[3] = '\0';
        sStack_5d8.sa_data[4] = '\0';
        sStack_5d8.sa_data[5] = '\0';
        sStack_5d8.sa_data[6] = '\0';
        sStack_5d8.sa_data[7] = '\0';
        sStack_5d8.sa_data[8] = '\0';
        sStack_5d8.sa_data[9] = '\0';
        sStack_5d8.sa_data[10] = '\0';
        sStack_5d8.sa_data[0xb] = '\0';
        sStack_5d8.sa_data[0xc] = '\0';
        sStack_5d8.sa_data[0xd] = '\0';
        EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        uVar3 = htons((short)uVar1 + 1);
        uVar7 = 0;
        uVar6 = 0x5a4;
        sStack_5d8.sa_data[2] = '\0';
        sStack_5d8.sa_data[3] = '\0';
        sStack_5d8.sa_data[4] = '\0';
        sStack_5d8.sa_data[5] = '\0';
        sStack_5d8.sa_data[0] = (char)uVar3;
        sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
        sStack_5d8.sa_family = 2;
        sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
        sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
        sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
        sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
        iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
        LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        if (iVar4 < 1) {
          if (iVar4 == -1) {
            uVar5 = WSAGetLastError();
            FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
          }
        }
        else {
          LOCK();
          DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
          UNLOCK();
        }
      }
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009f40
   NAME : SetADCRandom
   SIG  : undefined __fastcall SetADCRandom(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetADCRandom(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  undefined1 uStack_5c4;
  byte bStack_5c3;
  byte bStack_5c2;
  byte bStack_5c1;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_58e;
  undefined1 uStack_75;
  ulonglong uStack_18;
  
                    /* 0x9f40  71  SetADCRandom */
  lVar2 = DAT_180020530;
  if (*(int *)(DAT_180020530 + 0x260) != param_1) {
    bVar8 = DAT_180020598 != 0xffffffffffffffff;
    *(int *)(DAT_180020530 + 0x2d0) = param_1;
    *(int *)(lVar2 + 0x298) = param_1;
    *(int *)(lVar2 + 0x260) = param_1;
    if (bVar8) {
      uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
      memset(acStack_5c8,0,0x5a4);
      s = DAT_180020598;
      uStack_5c4 = *(undefined1 *)(DAT_180020530 + 0x4c);
      uStack_5b5 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x334);
      bStack_5c3 = ((*(char *)(DAT_180020530 + 0x2cc) * '\x02' | *(byte *)(DAT_180020530 + 0x294)) *
                    '\x02' | *(byte *)(DAT_180020530 + 0x25c)) & 7;
      bStack_5c2 = ((*(char *)(DAT_180020530 + 0x2d0) * '\x02' | *(byte *)(DAT_180020530 + 0x298)) *
                    '\x02' | *(byte *)(DAT_180020530 + 0x260)) & 7;
      bStack_5c1 = (((((*(char *)(DAT_180020530 + 0x95c) * '\x02' | *(byte *)(DAT_180020530 + 0x854)
                       ) * '\x02' | *(byte *)(DAT_180020530 + 0x74c)) * '\x02' |
                     *(byte *)(DAT_180020530 + 0x644)) * '\x02' | *(byte *)(DAT_180020530 + 0x53c))
                    * '\x02' | *(byte *)(DAT_180020530 + 0x434)) * '\x02' |
                   *(byte *)(DAT_180020530 + 0x32c);
      uStack_5b7 = *(undefined1 *)(DAT_180020530 + 0x324);
      uStack_5b6 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x334) >> 8);
      uStack_5b2 = *(undefined1 *)(DAT_180020530 + 0x338);
      uStack_5b1 = *(undefined1 *)(DAT_180020530 + 0x42c);
      uStack_5b0 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x43c) >> 8);
      uStack_5ac = *(undefined1 *)(DAT_180020530 + 0x440);
      uStack_5ab = *(undefined1 *)(DAT_180020530 + 0x534);
      uStack_5af = (undefined1)*(undefined4 *)(DAT_180020530 + 0x43c);
      uStack_5aa = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x544) >> 8);
      uStack_5a6 = *(undefined1 *)(DAT_180020530 + 0x548);
      uStack_5a5 = *(undefined1 *)(DAT_180020530 + 0x63c);
      uStack_5a9 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x544);
      uStack_5a4 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x64c) >> 8);
      uStack_5a0 = *(undefined1 *)(DAT_180020530 + 0x650);
      uStack_59f = *(undefined1 *)(DAT_180020530 + 0x744);
      uStack_5a3 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x64c);
      uStack_59e = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x754) >> 8);
      uStack_59a = *(undefined1 *)(DAT_180020530 + 0x758);
      uStack_599 = *(undefined1 *)(DAT_180020530 + 0x84c);
      uStack_59d = (undefined1)*(undefined4 *)(DAT_180020530 + 0x754);
      uStack_598 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x85c) >> 8);
      uStack_594 = *(undefined1 *)(DAT_180020530 + 0x860);
      uStack_593 = *(undefined1 *)(DAT_180020530 + 0x954);
      uStack_597 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x85c);
      uStack_592 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x964) >> 8);
      uStack_58e = *(undefined1 *)(DAT_180020530 + 0x968);
      uStack_75 = *(undefined1 *)(DAT_180020530 + 0x330);
      uStack_591 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x964);
      if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
        uVar1 = *(undefined4 *)(DAT_180020530 + 4);
        sStack_5d8.sa_family = 0;
        sStack_5d8.sa_data[0] = '\0';
        sStack_5d8.sa_data[1] = '\0';
        sStack_5d8.sa_data[2] = '\0';
        sStack_5d8.sa_data[3] = '\0';
        sStack_5d8.sa_data[4] = '\0';
        sStack_5d8.sa_data[5] = '\0';
        sStack_5d8.sa_data[6] = '\0';
        sStack_5d8.sa_data[7] = '\0';
        sStack_5d8.sa_data[8] = '\0';
        sStack_5d8.sa_data[9] = '\0';
        sStack_5d8.sa_data[10] = '\0';
        sStack_5d8.sa_data[0xb] = '\0';
        sStack_5d8.sa_data[0xc] = '\0';
        sStack_5d8.sa_data[0xd] = '\0';
        EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        uVar3 = htons((short)uVar1 + 1);
        uVar7 = 0;
        uVar6 = 0x5a4;
        sStack_5d8.sa_data[2] = '\0';
        sStack_5d8.sa_data[3] = '\0';
        sStack_5d8.sa_data[4] = '\0';
        sStack_5d8.sa_data[5] = '\0';
        sStack_5d8.sa_data[0] = (char)uVar3;
        sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
        sStack_5d8.sa_family = 2;
        sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
        sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
        sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
        sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
        iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
        LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        if (iVar4 < 1) {
          if (iVar4 == -1) {
            uVar5 = WSAGetLastError();
            FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
          }
        }
        else {
          LOCK();
          DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
          UNLOCK();
        }
      }
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180009f70
   NAME : SetAntBits
   SIG  : undefined __fastcall SetAntBits(uint param_1, uint param_2, uint param_3, byte param_4, char param_5)
   ======================================================================== */

void SetAntBits(uint param_1,uint param_2,uint param_3,byte param_4,char param_5)

{
  longlong lVar1;
  longlong lVar2;
  byte bVar3;
  byte bVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  bool bVar8;
  
                    /* 0x9f70  86  SetAntBits */
  lVar1 = DAT_1800205b0;
  if (DAT_180020518 == 0) {
    *(byte *)(DAT_1800205b0 + 5) =
         (*(byte *)(DAT_1800205b0 + 5) ^ param_4 << 3) & 8 ^ *(byte *)(DAT_1800205b0 + 5);
  }
  else if ((param_1 == 1) || (param_5 != '\0')) {
    *(byte *)(DAT_1800205b0 + 5) =
         ((*(byte *)(DAT_1800205b0 + 5) ^ param_4 << 3) & 8 ^ *(byte *)(DAT_1800205b0 + 5)) & 0xbf;
  }
  else {
    *(byte *)(DAT_1800205b0 + 5) = *(byte *)(DAT_1800205b0 + 5) & 0xb7;
    *(byte *)(lVar1 + 5) = *(byte *)(lVar1 + 5) | (param_4 & 1) << 6;
  }
  *(byte *)(lVar1 + 5) = *(byte *)(lVar1 + 5) & 0xfb;
  uVar7 = param_1 & 3;
  bVar4 = 0;
  if (uVar7 == 2) {
    bVar4 = 2;
  }
  bVar3 = 0;
  if (uVar7 == 1) {
    bVar3 = 4;
  }
  *(byte *)(lVar1 + 7) = *(byte *)(lVar1 + 7) & 0xfe;
  uVar6 = param_2 & 3;
  *(byte *)(lVar1 + 5) = bVar3 | *(byte *)(lVar1 + 5) & 0xfc | bVar4 | uVar7 == 3;
  lVar2 = DAT_1800205d0;
  bVar4 = 0;
  if (uVar6 == 3) {
    bVar4 = 4;
  }
  bVar3 = 0;
  if (uVar6 == 2) {
    bVar3 = 2;
  }
  uVar5 = param_3 & 3;
  *(byte *)(lVar1 + 7) = uVar6 == 1 | *(byte *)(lVar1 + 7) & 0xf9 | bVar3 | bVar4;
  uVar7 = *(uint *)(lVar2 + 4);
  *(uint *)(lVar2 + 4) = uVar7 & 0x700ffff;
  *(uint *)(lVar2 + 4) = (uVar7 ^ *(uint *)(lVar1 + 4)) & 0x700ffff ^ *(uint *)(lVar1 + 4);
  *(byte *)(lVar2 + 7) = *(byte *)(lVar2 + 7) & 0xfe;
  bVar4 = 0;
  if (uVar5 == 3) {
    bVar4 = 4;
  }
  bVar3 = 0;
  if (uVar5 == 2) {
    bVar3 = 2;
  }
  bVar8 = DAT_180020598 != -1;
  *(byte *)(lVar2 + 7) = uVar5 == 1 | *(byte *)(lVar2 + 7) & 0xf9 | bVar3 | bVar4;
  if ((bVar8) && (*(int *)(DAT_180020530 + 0x48) != 0)) {
    FUN_18000d990();
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a0f0
   NAME : SetVFOfreq
   SIG  : undefined __fastcall SetVFOfreq(int param_1, int param_2, int param_3)
   ======================================================================== */

void SetVFOfreq(int param_1,int param_2,int param_3)

{
  longlong lVar1;
  longlong lVar2;
  bool bVar3;
  
  lVar1 = DAT_180020530;
                    /* 0xa0f0  233  SetVFOfreq */
  if (param_3 == 0) {
    if (DAT_180020530 == 0) {
      return;
    }
    lVar2 = (longlong)param_1 * 0x108;
    if (*(int *)(lVar2 + 0x328 + DAT_180020530) == param_2) {
      return;
    }
    bVar3 = DAT_180020598 == -1;
    *(int *)(lVar2 + 0x328 + DAT_180020530) = param_2;
    if (bVar3) {
      return;
    }
    if (*(int *)(lVar1 + 0x48) == 0) {
      return;
    }
  }
  else {
    if (((DAT_180020530 == 0) ||
        (lVar2 = (longlong)param_1 * 0x4c, *(int *)(lVar2 + 0xf84 + DAT_180020530) == param_2)) ||
       (bVar3 = DAT_180020598 == -1, *(int *)(lVar2 + 0xf84 + DAT_180020530) = param_2, bVar3)) {
      return;
    }
    if (*(int *)(lVar1 + 0x48) == 0) {
      return;
    }
  }
  FUN_18000d990();
  return;
}



/* ========================================================================
   ENTRY: 18000a170
   NAME : SetOutputPowerFactor
   SIG  : undefined __fastcall SetOutputPowerFactor(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetOutputPowerFactor(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa170  179  SetOutputPowerFactor */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0xf9c) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0xf9c) = param_1, bVar9
      )) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a1a0
   NAME : SetMicBoost
   SIG  : undefined __fastcall SetMicBoost(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetMicBoost(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa1a0  173  SetMicBoost */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 1 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 * '\x02' ^ bVar1) & 2 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a1e0
   NAME : SetMicXlr
   SIG  : undefined __fastcall SetMicXlr(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetMicXlr(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa1e0  176  SetMicXlr */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 5 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 << 5 ^ bVar1) & 0x20 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a220
   NAME : SetAudio24
   SIG  : undefined __fastcall SetAudio24(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetAudio24(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa220  89  SetAudio24 */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 6 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 << 6 ^ bVar1) & 0x40 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a260
   NAME : SetLineIn
   SIG  : undefined __fastcall SetLineIn(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetLineIn(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa260  170  SetLineIn */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = (bVar1 ^ (byte)param_1) & 1 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a2a0
   NAME : EnableApolloFilter
   SIG  : undefined __fastcall EnableApolloFilter(int param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void EnableApolloFilter(int param_1)

{
                    /* 0xa2a0  15  EnableApolloFilter */
  _DAT_1800205d8 = -(uint)(param_1 != 0) & 4;
  return;
}



/* ========================================================================
   ENTRY: 18000a2b0
   NAME : EnableApolloTuner
   SIG  : undefined __fastcall EnableApolloTuner(int param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void EnableApolloTuner(int param_1)

{
                    /* 0xa2b0  16  EnableApolloTuner */
  _DAT_18002057c = -(uint)(param_1 != 0) & 8;
  return;
}



/* ========================================================================
   ENTRY: 18000a2c0
   NAME : EnableApolloAutoTune
   SIG  : undefined __fastcall EnableApolloAutoTune(int param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void EnableApolloAutoTune(int param_1)

{
                    /* 0xa2c0  14  EnableApolloAutoTune */
  _DAT_180020590 = -(uint)(param_1 != 0) & 0x10;
  return;
}



/* ========================================================================
   ENTRY: 18000a2d0
   NAME : SelectApolloFilter
   SIG  : undefined __fastcall SelectApolloFilter(int param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void SelectApolloFilter(int param_1)

{
                    /* 0xa2d0  46  SelectApolloFilter */
  _DAT_18002051c = -(uint)(param_1 != 0) & 0x20;
  return;
}



/* ========================================================================
   ENTRY: 18000a2e0
   NAME : SetAlexHPFBits
   SIG  : undefined __fastcall SetAlexHPFBits(uint param_1)
   ======================================================================== */

void SetAlexHPFBits(uint param_1)

{
  longlong lVar1;
  byte bVar2;
  bool bVar3;
  
                    /* 0xa2e0  83  SetAlexHPFBits */
  lVar1 = DAT_1800205b0;
  if (DAT_180020570 != param_1) {
    bVar2 = (*(byte *)(DAT_1800205b0 + 4) ^ (char)param_1 * '\x02') & 2 ^
            *(byte *)(DAT_1800205b0 + 4);
    bVar2 = (bVar2 ^ (char)(param_1 >> 1) << 2) & 4 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 2) << 4) & 0x10 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 3) << 5) & 0x20 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 4) << 6) & 0x40 ^ bVar2;
    *(byte *)(DAT_1800205b0 + 5) =
         (*(byte *)(DAT_1800205b0 + 5) ^ (char)(param_1 >> 5) << 4) & 0x10 ^
         *(byte *)(DAT_1800205b0 + 5);
    bVar3 = DAT_180020598 != -1;
    *(byte *)(lVar1 + 4) = (bVar2 ^ (char)(param_1 >> 6) << 3) & 8 ^ bVar2;
    if ((bVar3) && (*(int *)(DAT_180020530 + 0x48) != 0)) {
      FUN_18000d990();
    }
  }
  DAT_180020570 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 18000a3c0
   NAME : DisablePA
   SIG  : undefined __fastcall DisablePA(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void DisablePA(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xa3c0  13  DisablePA */
  if ((DAT_180020530[0x3ee] != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff, DAT_180020530[0x3ee] = param_1,
     piVar2 = DAT_180020530, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a3f0
   NAME : SetAlex2HPFBits
   SIG  : undefined __fastcall SetAlex2HPFBits(uint param_1)
   ======================================================================== */

void SetAlex2HPFBits(uint param_1)

{
  longlong lVar1;
  byte bVar2;
  bool bVar3;
  
                    /* 0xa3f0  76  SetAlex2HPFBits */
  lVar1 = DAT_1800205d0;
  if (DAT_1800205b8 != param_1) {
    bVar2 = (*(byte *)(DAT_1800205d0 + 4) ^ (char)param_1 * '\x02') & 2 ^
            *(byte *)(DAT_1800205d0 + 4);
    bVar2 = (bVar2 ^ (char)(param_1 >> 1) << 2) & 4 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 2) << 4) & 0x10 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 3) << 5) & 0x20 ^ bVar2;
    bVar2 = (bVar2 ^ (char)(param_1 >> 4) << 6) & 0x40 ^ bVar2;
    *(byte *)(DAT_1800205d0 + 5) =
         (*(byte *)(DAT_1800205d0 + 5) ^ (char)(param_1 >> 5) << 4) & 0x10 ^
         *(byte *)(DAT_1800205d0 + 5);
    bVar3 = DAT_180020598 != -1;
    *(byte *)(lVar1 + 4) = (bVar2 ^ (char)(param_1 >> 6) << 3) & 8 ^ bVar2;
    if ((bVar3) && (*(int *)(DAT_180020530 + 0x48) != 0)) {
      FUN_18000d990();
    }
  }
  DAT_1800205b8 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 18000a4d0
   NAME : SetBPF2Gnd
   SIG  : undefined __fastcall SetBPF2Gnd(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetBPF2Gnd(uint param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa4d0  95  SetBPF2Gnd */
  bVar5 = *(byte *)(DAT_1800205d0 + 5);
  if ((((bVar5 & 1) == param_1) ||
      (bVar8 = DAT_180020598 == 0xffffffffffffffff,
      *(byte *)(DAT_1800205d0 + 5) = (bVar5 ^ (byte)param_1) & 1 ^ bVar5, bVar8)) ||
     (*(int *)(DAT_180020530 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a510
   NAME : SetAlexLPFBits
   SIG  : undefined __fastcall SetAlexLPFBits(uint param_1, char param_2, char param_3)
   ======================================================================== */

void SetAlexLPFBits(uint param_1,char param_2,char param_3)

{
  uint uVar1;
  bool bVar2;
  longlong lVar3;
  longlong lVar4;
  byte bVar5;
  char cVar6;
  char cVar7;
  char cVar8;
  bool bVar9;
  
  lVar4 = DAT_1800205d0;
                    /* 0xa510  84  SetAlexLPFBits */
  lVar3 = DAT_1800205b0;
  if ((param_3 == '\0') && (param_2 == '\0')) {
    cVar6 = (char)(param_1 >> 1);
    cVar7 = (char)(param_1 >> 2);
    cVar8 = (char)(param_1 >> 3);
    bVar9 = false;
  }
  else {
    cVar6 = (char)(param_1 >> 1);
    cVar7 = (char)(param_1 >> 2);
    cVar8 = (char)(param_1 >> 3);
    bVar9 = DAT_180020588 != param_1;
    if (bVar9) {
      uVar1 = *(uint *)(DAT_1800205d0 + 4);
      DAT_180020588 = param_1;
      *(uint *)(DAT_1800205d0 + 4) = uVar1 & 0x700ffff;
      *(uint *)(lVar4 + 4) = (uVar1 ^ *(uint *)(lVar3 + 4)) & 0x700ffff ^ *(uint *)(lVar3 + 4);
      bVar5 = (*(byte *)(lVar4 + 6) ^ (char)param_1 << 4) & 0x10 ^ *(byte *)(lVar4 + 6);
      bVar5 = (bVar5 ^ cVar6 << 5) & 0x20 ^ bVar5;
      *(byte *)(lVar4 + 6) = cVar8 << 7 | ((bVar5 ^ cVar7 << 6) & 0x40 ^ bVar5) & 0x7f;
      bVar5 = (*(byte *)(lVar4 + 7) ^ (char)(param_1 >> 4) << 5) & 0x20 ^ *(byte *)(lVar4 + 7);
      *(byte *)(lVar4 + 7) =
           (char)(param_1 >> 6) << 7 | ((bVar5 ^ (char)(param_1 >> 5) << 6) & 0x40 ^ bVar5) & 0x7f;
    }
    if ((param_3 == '\0') && (bVar2 = false, param_2 != '\0')) goto LAB_18000a708;
  }
  bVar2 = false;
  if (DAT_18002058c != param_1) {
    bVar5 = (*(byte *)(lVar3 + 6) ^ (char)param_1 << 4) & 0x10 ^ *(byte *)(lVar3 + 6);
    bVar5 = (bVar5 ^ cVar6 << 5) & 0x20 ^ bVar5;
    DAT_18002058c = param_1;
    *(byte *)(lVar3 + 6) = cVar8 << 7 | ((bVar5 ^ cVar7 << 6) & 0x40 ^ bVar5) & 0x7f;
    bVar5 = (*(byte *)(lVar3 + 7) ^ (char)(param_1 >> 4) << 5) & 0x20 ^ *(byte *)(lVar3 + 7);
    bVar2 = true;
    *(byte *)(lVar3 + 7) =
         (char)(param_1 >> 6) << 7 | ((bVar5 ^ (char)(param_1 >> 5) << 6) & 0x40 ^ bVar5) & 0x7f;
  }
LAB_18000a708:
  if ((((bVar9) || (bVar2)) && (DAT_180020598 != -1)) && (*(int *)(DAT_180020530 + 0x48) != 0)) {
    FUN_18000d990();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a760
   NAME : SetRX1Preamp
   SIG  : undefined __fastcall SetRX1Preamp(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetRX1Preamp(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa760  201  SetRX1Preamp */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0x33c) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x33c) = param_1, bVar9
      )) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a790
   NAME : SetRX2Preamp
   SIG  : undefined __fastcall SetRX2Preamp(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetRX2Preamp(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa790  202  SetRX2Preamp */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0x444) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x444) = param_1, bVar9
      )) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a7c0
   NAME : SetMicTipRing
   SIG  : undefined __fastcall SetMicTipRing(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetMicTipRing(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa7c0  175  SetMicTipRing */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 3 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 << 3 ^ bVar1) & 8 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a800
   NAME : SetMicBias
   SIG  : undefined __fastcall SetMicBias(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetMicBias(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa800  172  SetMicBias */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 4 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 << 4 ^ bVar1) & 0x10 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a840
   NAME : SetMicPTT
   SIG  : undefined __fastcall SetMicPTT(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetMicPTT(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa840  174  SetMicPTT */
  bVar1 = *(byte *)(DAT_180020530 + 0x314);
  if (((bVar1 >> 2 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x314) = ((char)param_1 << 2 ^ bVar1) & 4 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a880
   NAME : SetLineBoost
   SIG  : undefined __fastcall SetLineBoost(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetLineBoost(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa880  169  SetLineBoost */
  if ((*(int *)(DAT_180020530 + 0x310) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x310) = param_1,
     s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a8b0
   NAME : SetPureSignal
   SIG  : undefined __fastcall SetPureSignal(uint param_1)
   ======================================================================== */

void SetPureSignal(uint param_1)

{
                    /* 0xa8b0  183  SetPureSignal */
  if (*(uint *)(DAT_180020530 + 0x220) != param_1) {
    *(uint *)(DAT_180020530 + 0x220) = param_1 & 1;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a8d0
   NAME : SetUserOut0
   SIG  : undefined __fastcall SetUserOut0(uint param_1)
   ======================================================================== */

void SetUserOut0(uint param_1)

{
                    /* 0xa8d0  229  SetUserOut0 */
  *(uint *)(DAT_180020530 + 0x84) = param_1 & 1;
  return;
}



/* ========================================================================
   ENTRY: 18000a8f0
   NAME : SetUserOut1
   SIG  : undefined __fastcall SetUserOut1(uint param_1)
   ======================================================================== */

void SetUserOut1(uint param_1)

{
                    /* 0xa8f0  230  SetUserOut1 */
  *(uint *)(DAT_180020530 + 0x84) = (param_1 & 1) * 2;
  return;
}



/* ========================================================================
   ENTRY: 18000a910
   NAME : SetUserOut2
   SIG  : undefined __fastcall SetUserOut2(uint param_1)
   ======================================================================== */

void SetUserOut2(uint param_1)

{
                    /* 0xa910  231  SetUserOut2 */
  *(uint *)(DAT_180020530 + 0x84) = (param_1 & 1) << 2;
  return;
}



/* ========================================================================
   ENTRY: 18000a930
   NAME : SetUserOut3
   SIG  : undefined __fastcall SetUserOut3(uint param_1)
   ======================================================================== */

void SetUserOut3(uint param_1)

{
                    /* 0xa930  232  SetUserOut3 */
  *(uint *)(DAT_180020530 + 0x84) = (param_1 & 1) << 3;
  return;
}



/* ========================================================================
   ENTRY: 18000a950
   NAME : SetADC1StepAttenData
   SIG  : undefined __fastcall SetADC1StepAttenData(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetADC1StepAttenData(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa950  67  SetADC1StepAttenData */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0x24c) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x24c) = param_1, bVar9
      )) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a980
   NAME : SetADC2StepAttenData
   SIG  : undefined __fastcall SetADC2StepAttenData(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetADC2StepAttenData(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  byte bVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa980  68  SetADC2StepAttenData */
  lVar2 = DAT_180020530;
  if (((*(int *)(DAT_180020530 + 0x284) == param_1) ||
      (bVar9 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x284) = param_1, bVar9
      )) || (*(int *)(lVar2 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar4 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar4 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar4 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar4 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar4 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar4 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar4 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar4 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar4 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar6 = 0;
  if (DAT_18002056c == 0) {
    bVar6 = 2;
  }
  bStack_50 = bVar6 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((short)uVar1 + 3);
    uVar8 = 0;
    uVar7 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar3;
    sStack_5d8.sa_data[1] = (char)(uVar3 >> 8);
    sStack_5d8.sa_family = 2;
    iVar4 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a9b0
   NAME : SetADC3StepAttenData
   SIG  : undefined __fastcall SetADC3StepAttenData(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetADC3StepAttenData(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xa9b0  69  SetADC3StepAttenData */
  if ((*(int *)(DAT_180020530 + 700) == param_1) ||
     (bVar8 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 700) = param_1, bVar8))
  {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000a9e0
   NAME : ReversePaddles
   SIG  : undefined __fastcall ReversePaddles(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void ReversePaddles(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xa9e0  45  ReversePaddles */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 >> 2 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 << 2 ^ bVar1) & 4 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000aa20
   NAME : SetCWKeyerSpeed
   SIG  : undefined __fastcall SetCWKeyerSpeed(int param_1)
   ======================================================================== */

void SetCWKeyerSpeed(int param_1)

{
  longlong lVar1;
  bool bVar2;
  
                    /* 0xaa20  105  SetCWKeyerSpeed */
  if (*(int *)(DAT_180020530 + 0x2f8) != param_1) {
    bVar2 = DAT_180020598 != -1;
    *(int *)(DAT_180020530 + 0x2f8) = param_1;
    if (bVar2) {
      FUN_18000e1c0();
    }
    lVar1 = DAT_1800202e0;
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800202e0 + 0x118));
    *(int *)(lVar1 + 0x60) = param_1;
    FUN_180012f70(lVar1);
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  }
  return;
}



/* ========================================================================
   ENTRY: 18000aaa0
   NAME : SetCWKeyerMode
   SIG  : undefined __fastcall SetCWKeyerMode(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWKeyerMode(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xaaa0  104  SetCWKeyerMode */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 >> 5 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 << 5 ^ bVar1) & 0x20 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000aae0
   NAME : SetCWKeyerWeight
   SIG  : undefined __fastcall SetCWKeyerWeight(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWKeyerWeight(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xaae0  106  SetCWKeyerWeight */
  if ((*(int *)(DAT_180020530 + 0x2fc) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x2fc) = param_1,
     s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ab10
   NAME : EnableCWKeyerSpacing
   SIG  : undefined __fastcall EnableCWKeyerSpacing(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void EnableCWKeyerSpacing(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xab10  18  EnableCWKeyerSpacing */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 >> 6 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 << 6 ^ bVar1) & 0x40 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ab50
   NAME : SetCWEdgeLength
   SIG  : undefined __fastcall SetCWEdgeLength(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWEdgeLength(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xab50  101  SetCWEdgeLength */
  if ((*(int *)(DAT_180020530 + 0x308) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x308) = param_1,
     s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ab80
   NAME : SetADC_cntrl1
   SIG  : undefined __fastcall SetADC_cntrl1(uint param_1)
   ======================================================================== */

void SetADC_cntrl1(uint param_1)

{
  longlong lVar1;
  bool bVar2;
  
  lVar1 = DAT_180020530;
                    /* 0xab80  73  SetADC_cntrl1 */
  bVar2 = DAT_18002052c != param_1;
  DAT_18002052c = param_1;
  if (bVar2) {
    *(uint *)(DAT_180020530 + 0x324) = param_1 & 3;
    *(uint *)(lVar1 + 0x42c) = (int)param_1 >> 2 & 3;
    *(uint *)(lVar1 + 0x534) = (int)param_1 >> 4 & 3;
    *(uint *)(lVar1 + 0x63c) = (int)param_1 >> 6 & 3;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000abd0
   NAME : GetADC_cntrl1
   SIG  : undefined4 __fastcall GetADC_cntrl1(void)
   ======================================================================== */

undefined4 GetADC_cntrl1(void)

{
                    /* 0xabd0  23  GetADC_cntrl1 */
  return DAT_18002052c;
}



/* ========================================================================
   ENTRY: 18000abe0
   NAME : SetADC_cntrl2
   SIG  : undefined __fastcall SetADC_cntrl2(uint param_1)
   ======================================================================== */

void SetADC_cntrl2(uint param_1)

{
  longlong lVar1;
  bool bVar2;
  
  lVar1 = DAT_180020530;
                    /* 0xabe0  74  SetADC_cntrl2 */
  bVar2 = DAT_180020580 != param_1;
  DAT_180020580 = param_1;
  if (bVar2) {
    *(uint *)(DAT_180020530 + 0x744) = param_1 & 3;
    *(uint *)(lVar1 + 0x84c) = (int)param_1 >> 2 & 3;
    *(uint *)(lVar1 + 0x954) = (int)param_1 >> 4 & 3;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ac20
   NAME : GetADC_cntrl2
   SIG  : undefined4 __fastcall GetADC_cntrl2(void)
   ======================================================================== */

undefined4 GetADC_cntrl2(void)

{
                    /* 0xac20  24  GetADC_cntrl2 */
  return DAT_180020580;
}



/* ========================================================================
   ENTRY: 18000ac30
   NAME : SetADC_cntrl_P1
   SIG  : undefined __fastcall SetADC_cntrl_P1(undefined4 param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void SetADC_cntrl_P1(undefined4 param_1)

{
                    /* 0xac30  75  SetADC_cntrl_P1 */
  _DAT_180020524 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 18000ac40
   NAME : GetADC_cntrl_P1
   SIG  : undefined4 __fastcall GetADC_cntrl_P1(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined4 GetADC_cntrl_P1(void)

{
                    /* 0xac40  25  GetADC_cntrl_P1 */
  return _DAT_180020524;
}



/* ========================================================================
   ENTRY: 18000ac50
   NAME : SetTxAttenData
   SIG  : undefined __fastcall SetTxAttenData(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetTxAttenData(int param_1)

{
  undefined4 uVar1;
  longlong lVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xac50  228  SetTxAttenData */
  lVar2 = DAT_180020530;
  if (*(int *)(DAT_180020530 + 0x250) != param_1) {
    bVar8 = DAT_180020598 != 0xffffffffffffffff;
    *(int *)(DAT_180020530 + 0x250) = param_1;
    *(int *)(lVar2 + 0x288) = param_1;
    *(int *)(lVar2 + 0x2c0) = param_1;
    s = DAT_180020598;
    if (bVar8) {
      uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
      lStack_58 = (ulonglong)
                  CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                           CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                    CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                             *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
      _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                             CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                      CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                        *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                               (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
      lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
      _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                             CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                      CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)))
      ;
      uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
      lStack_40 = (ulonglong)
                  CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                           (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
      uStack_38 = 0;
      uStack_30 = 0;
      uStack_2c = 0;
      uStack_28 = 0;
      uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
      lStack_24 = (ulonglong)
                  CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                           CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                    *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
      if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
        uVar1 = *(undefined4 *)(DAT_180020530 + 4);
        sStack_68.sa_family = 0;
        sStack_68.sa_data[0] = '\0';
        sStack_68.sa_data[1] = '\0';
        sStack_68.sa_data[2] = '\0';
        sStack_68.sa_data[3] = '\0';
        sStack_68.sa_data[4] = '\0';
        sStack_68.sa_data[5] = '\0';
        sStack_68.sa_data[6] = '\0';
        sStack_68.sa_data[7] = '\0';
        sStack_68.sa_data[8] = '\0';
        sStack_68.sa_data[9] = '\0';
        sStack_68.sa_data[10] = '\0';
        sStack_68.sa_data[0xb] = '\0';
        sStack_68.sa_data[0xc] = '\0';
        sStack_68.sa_data[0xd] = '\0';
        EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        uVar3 = htons((short)uVar1 + 2);
        uVar7 = 0;
        uVar6 = 0x3c;
        sStack_68.sa_data[2] = '\0';
        sStack_68.sa_data[3] = '\0';
        sStack_68.sa_data[4] = '\0';
        sStack_68.sa_data[5] = '\0';
        sStack_68.sa_data[0] = (char)uVar3;
        sStack_68.sa_data[1] = (char)(uVar3 >> 8);
        sStack_68.sa_family = 2;
        sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
        sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
        sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
        sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
        iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
        LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
        if (iVar4 < 1) {
          if (iVar4 == -1) {
            uVar5 = WSAGetLastError();
            FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
          }
        }
        else {
          LOCK();
          DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
          UNLOCK();
        }
      }
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ac80
   NAME : EnableCWKeyer
   SIG  : undefined __fastcall EnableCWKeyer(uint param_1)
   ======================================================================== */

void EnableCWKeyer(uint param_1)

{
  byte bVar1;
  longlong lVar2;
  bool bVar3;
  
                    /* 0xac80  17  EnableCWKeyer */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if ((bVar1 >> 1 & 1) != param_1) {
    bVar3 = DAT_180020598 != -1;
    *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 * '\x02' ^ bVar1) & 2 ^ bVar1;
    if (bVar3) {
      FUN_18000e1c0();
    }
    lVar2 = DAT_1800202e0;
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800202e0 + 0x118));
    *(uint *)(lVar2 + 4) = param_1;
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x118));
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ad00
   NAME : SetCWSidetoneVolume
   SIG  : undefined __fastcall SetCWSidetoneVolume(int param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void SetCWSidetoneVolume(int param_1)

{
  longlong lVar1;
  bool bVar2;
  
                    /* 0xad00  110  SetCWSidetoneVolume */
  if (*(int *)(DAT_180020530 + 0x2f0) != param_1) {
    bVar2 = DAT_180020598 != -1;
    *(int *)(DAT_180020530 + 0x2f0) = param_1;
    if (bVar2) {
      FUN_18000e1c0();
    }
    lVar1 = DAT_1800202e0;
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800202e0 + 0x118));
    *(double *)(lVar1 + 0x50) = (double)param_1 / _DAT_180018e08;
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ad80
   NAME : SetCWPTTDelay
   SIG  : undefined __fastcall SetCWPTTDelay(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWPTTDelay(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xad80  107  SetCWPTTDelay */
  if ((*(int *)(DAT_180020530 + 0x304) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x304) = param_1,
     s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000adb0
   NAME : SetCWHangTime
   SIG  : undefined __fastcall SetCWHangTime(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWHangTime(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xadb0  102  SetCWHangTime */
  if ((*(int *)(DAT_180020530 + 0x300) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x300) = param_1,
     s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ade0
   NAME : SetCWSidetoneFreq
   SIG  : undefined __fastcall SetCWSidetoneFreq(int param_1)
   ======================================================================== */

void SetCWSidetoneFreq(int param_1)

{
  bool bVar1;
  
                    /* 0xade0  109  SetCWSidetoneFreq */
  if (*(int *)(DAT_180020530 + 0x2f4) != param_1) {
    bVar1 = DAT_180020598 != -1;
    *(int *)(DAT_180020530 + 0x2f4) = param_1;
    if (bVar1) {
      FUN_18000e1c0();
    }
    SetSidetonePitch(0,(double)param_1);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ae30
   NAME : SetEERPWMmin
   SIG  : undefined __fastcall SetEERPWMmin(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetEERPWMmin(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xae30  120  SetEERPWMmin */
  if ((DAT_180020530[0x3ed] != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff, DAT_180020530[0x3ed] = param_1,
     piVar2 = DAT_180020530, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ae60
   NAME : SetEERPWMmax
   SIG  : undefined __fastcall SetEERPWMmax(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetEERPWMmax(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xae60  119  SetEERPWMmax */
  if ((DAT_180020530[0x3ec] != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff, DAT_180020530[0x3ec] = param_1,
     piVar2 = DAT_180020530, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000ae90
   NAME : SetAudioAmpEnable
   SIG  : undefined __fastcall SetAudioAmpEnable(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetAudioAmpEnable(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xae90  90  SetAudioAmpEnable */
  if (((DAT_18002056c == param_1) || (DAT_18002056c = param_1, DAT_180020598 == 0xffffffffffffffff))
     || (*(int *)(DAT_180020530 + 0x48) == 0)) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000aec0
   NAME : SetCWSidetone
   SIG  : undefined __fastcall SetCWSidetone(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWSidetone(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xaec0  108  SetCWSidetone */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 >> 4 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 << 4 ^ bVar1) & 0x10 ^ bVar1,
     s = DAT_180020598, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000af00
   NAME : SetCWIambic
   SIG  : undefined __fastcall SetCWIambic(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWIambic(uint param_1)

{
  byte bVar1;
  undefined4 uVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xaf00  103  SetCWIambic */
  bVar1 = *(byte *)(DAT_180020530 + 0x30c);
  if (((bVar1 >> 3 & 1) != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = ((char)param_1 << 3 ^ bVar1) & 8 ^ bVar1, s = DAT_180020598,
     bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar2 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar3 = htons((short)uVar2 + 2);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000af40
   NAME : SetCWBreakIn
   SIG  : undefined __fastcall SetCWBreakIn(uint param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWBreakIn(uint param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  longlong lStack_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong lStack_48;
  longlong lStack_40;
  undefined8 uStack_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 uStack_26;
  undefined1 uStack_25;
  longlong lStack_24;
  ulonglong uStack_18;
  
                    /* 0xaf40  98  SetCWBreakIn */
  if ((*(byte *)(DAT_180020530 + 0x30c) >> 7 != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff,
     *(byte *)(DAT_180020530 + 0x30c) = *(byte *)(DAT_180020530 + 0x30c) & 0x7f | (char)param_1 << 7
     , s = DAT_180020598, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    lStack_58 = (ulonglong)
                CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                         CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                                  CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                           *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
    _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                           CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                    CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                      *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                             (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
    lStack_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
    _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                           CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                    CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
    uStack_26 = *(undefined1 *)(DAT_180020530 + 0x314);
    lStack_40 = (ulonglong)
                CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                         (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
    uStack_38 = 0;
    uStack_30 = 0;
    uStack_2c = 0;
    uStack_28 = 0;
    uStack_25 = *(undefined1 *)(DAT_180020530 + 0x310);
    lStack_24 = (ulonglong)
                CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                         CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                  *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 2);
      uVar6 = 0;
      uVar5 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar2;
      sStack_68.sa_data[1] = (char)(uVar2 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,(char *)&lStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000af80
   NAME : SetCWDash
   SIG  : undefined __fastcall SetCWDash(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWDash(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xaf80  99  SetCWDash */
  if ((*(int *)(DAT_180020530 + 0xf90) == param_1) ||
     (bVar8 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0xf90) = param_1, bVar8)
     ) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000afb0
   NAME : SetCWDot
   SIG  : undefined __fastcall SetCWDot(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCWDot(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xafb0  100  SetCWDot */
  if ((*(int *)(DAT_180020530 + 0xf94) == param_1) ||
     (bVar8 = DAT_180020598 == 0xffffffffffffffff, *(int *)(DAT_180020530 + 0xf94) = param_1, bVar8)
     ) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000afe0
   NAME : SetCWX
   SIG  : undefined __fastcall SetCWX(int param_1)
   ======================================================================== */

void SetCWX(int param_1)

{
  longlong lVar1;
  
                    /* 0xafe0  111  SetCWX */
  if (*(int *)(DAT_180020530 + 0xf8c) != param_1) {
    *(int *)(DAT_180020530 + 0xf8c) = param_1;
    lVar1 = DAT_1800202e0;
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_1800202e0 + 0x118));
    if (*(int *)(lVar1 + 0x110) == 0) {
      *(int *)(lVar1 + 0x104) = param_1;
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
    if (DAT_180020598 != -1) {
      FUN_18000d990();
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b060
   NAME : getHaveSync
   SIG  : undefined4 __fastcall getHaveSync(void)
   ======================================================================== */

undefined4 getHaveSync(void)

{
                    /* 0xb060  267  getHaveSync */
  return DAT_1800205c8;
}



/* ========================================================================
   ENTRY: 18000b070
   NAME : getControlByteIn
   SIG  : ulonglong __fastcall getControlByteIn(uint param_1)
   ======================================================================== */

ulonglong getControlByteIn(uint param_1)

{
                    /* 0xb070  264  getControlByteIn */
  if (param_1 < 5) {
    return (ulonglong)(byte)(&DAT_180020558)[(int)param_1];
  }
  return 0xffffffff;
}



/* ========================================================================
   ENTRY: 18000b090
   NAME : EnableRx
   SIG  : undefined __fastcall EnableRx(int param_1, uint param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void EnableRx(int param_1,uint param_2)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  undefined1 uStack_5c4;
  byte bStack_5c3;
  byte bStack_5c2;
  byte bStack_5c1;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_58e;
  undefined1 uStack_75;
  ulonglong uStack_18;
  
                    /* 0xb090  20  EnableRx */
  if ((*(uint *)((longlong)param_1 * 0x108 + 0x32c + DAT_180020530) != param_2) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff,
     *(uint *)((longlong)param_1 * 0x108 + 0x32c + DAT_180020530) = param_2 & 1, bVar7)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
    memset(acStack_5c8,0,0x5a4);
    s = DAT_180020598;
    uStack_5c4 = *(undefined1 *)(DAT_180020530 + 0x4c);
    uStack_5b5 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x334);
    bStack_5c3 = ((*(char *)(DAT_180020530 + 0x2cc) * '\x02' | *(byte *)(DAT_180020530 + 0x294)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x25c)) & 7;
    bStack_5c2 = ((*(char *)(DAT_180020530 + 0x2d0) * '\x02' | *(byte *)(DAT_180020530 + 0x298)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x260)) & 7;
    bStack_5c1 = (((((*(char *)(DAT_180020530 + 0x95c) * '\x02' | *(byte *)(DAT_180020530 + 0x854))
                     * '\x02' | *(byte *)(DAT_180020530 + 0x74c)) * '\x02' |
                   *(byte *)(DAT_180020530 + 0x644)) * '\x02' | *(byte *)(DAT_180020530 + 0x53c)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x434)) * '\x02' |
                 *(byte *)(DAT_180020530 + 0x32c);
    uStack_5b7 = *(undefined1 *)(DAT_180020530 + 0x324);
    uStack_5b6 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x334) >> 8);
    uStack_5b2 = *(undefined1 *)(DAT_180020530 + 0x338);
    uStack_5b1 = *(undefined1 *)(DAT_180020530 + 0x42c);
    uStack_5b0 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x43c) >> 8);
    uStack_5ac = *(undefined1 *)(DAT_180020530 + 0x440);
    uStack_5ab = *(undefined1 *)(DAT_180020530 + 0x534);
    uStack_5af = (undefined1)*(undefined4 *)(DAT_180020530 + 0x43c);
    uStack_5aa = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x544) >> 8);
    uStack_5a6 = *(undefined1 *)(DAT_180020530 + 0x548);
    uStack_5a5 = *(undefined1 *)(DAT_180020530 + 0x63c);
    uStack_5a9 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x544);
    uStack_5a4 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x64c) >> 8);
    uStack_5a0 = *(undefined1 *)(DAT_180020530 + 0x650);
    uStack_59f = *(undefined1 *)(DAT_180020530 + 0x744);
    uStack_5a3 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x64c);
    uStack_59e = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x754) >> 8);
    uStack_59a = *(undefined1 *)(DAT_180020530 + 0x758);
    uStack_599 = *(undefined1 *)(DAT_180020530 + 0x84c);
    uStack_59d = (undefined1)*(undefined4 *)(DAT_180020530 + 0x754);
    uStack_598 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x85c) >> 8);
    uStack_594 = *(undefined1 *)(DAT_180020530 + 0x860);
    uStack_593 = *(undefined1 *)(DAT_180020530 + 0x954);
    uStack_597 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x85c);
    uStack_592 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x964) >> 8);
    uStack_58e = *(undefined1 *)(DAT_180020530 + 0x968);
    uStack_75 = *(undefined1 *)(DAT_180020530 + 0x330);
    uStack_591 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x964);
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_5d8.sa_family = 0;
      sStack_5d8.sa_data[0] = '\0';
      sStack_5d8.sa_data[1] = '\0';
      sStack_5d8.sa_data[2] = '\0';
      sStack_5d8.sa_data[3] = '\0';
      sStack_5d8.sa_data[4] = '\0';
      sStack_5d8.sa_data[5] = '\0';
      sStack_5d8.sa_data[6] = '\0';
      sStack_5d8.sa_data[7] = '\0';
      sStack_5d8.sa_data[8] = '\0';
      sStack_5d8.sa_data[9] = '\0';
      sStack_5d8.sa_data[10] = '\0';
      sStack_5d8.sa_data[0xb] = '\0';
      sStack_5d8.sa_data[0xc] = '\0';
      sStack_5d8.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 1);
      uVar6 = 0;
      uVar5 = 0x5a4;
      sStack_5d8.sa_data[2] = '\0';
      sStack_5d8.sa_data[3] = '\0';
      sStack_5d8.sa_data[4] = '\0';
      sStack_5d8.sa_data[5] = '\0';
      sStack_5d8.sa_data[0] = (char)uVar2;
      sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
      sStack_5d8.sa_family = 2;
      sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b0d0
   NAME : EnableRxs
   SIG  : undefined __fastcall EnableRxs(uint param_1)
   ======================================================================== */

void EnableRxs(uint param_1)

{
  longlong lVar1;
  
                    /* 0xb0d0  22  EnableRxs */
  lVar1 = DAT_180020530;
  if (*(int *)(DAT_180020530 + 0x53c) * 4 + *(int *)(DAT_180020530 + 0x644) * 8 +
      *(int *)(DAT_180020530 + 0x434) * 2 + *(int *)(DAT_180020530 + 0x32c) != param_1) {
    *(uint *)(DAT_180020530 + 0x32c) = param_1 & 1;
    *(uint *)(lVar1 + 0x434) = (int)param_1 >> 1 & 1;
    *(uint *)(lVar1 + 0x53c) = (int)param_1 >> 2 & 1;
    *(uint *)(lVar1 + 0x644) = (int)param_1 >> 3 & 1;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b140
   NAME : EnableRxSync
   SIG  : undefined __fastcall EnableRxSync(int param_1, uint param_2)
   ======================================================================== */

void EnableRxSync(int param_1,uint param_2)

{
                    /* 0xb140  21  EnableRxSync */
  if (*(uint *)((longlong)param_1 * 0x108 + 0x330 + DAT_180020530) != param_2) {
    *(uint *)((longlong)param_1 * 0x108 + 0x330 + DAT_180020530) = param_2 & 0xff;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b170
   NAME : Protocol1DDCConfig
   SIG  : undefined __fastcall Protocol1DDCConfig(undefined8 param_1, undefined4 param_2, undefined8 param_3, undefined4 param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Protocol1DDCConfig(undefined8 param_1,undefined4 param_2,undefined8 param_3,undefined4 param_4)

{
                    /* 0xb170  44  Protocol1DDCConfig */
  _DAT_1800205c0 = param_2;
  _DAT_1800205bc = param_4;
  return;
}



/* ========================================================================
   ENTRY: 18000b180
   NAME : SetDDCRate
   SIG  : undefined __fastcall SetDDCRate(int param_1, int param_2)
   ======================================================================== */

void SetDDCRate(int param_1,int param_2)

{
  longlong lVar1;
  
                    /* 0xb180  115  SetDDCRate */
  lVar1 = (longlong)param_1;
  if (param_2 < 0x2ee01) {
    if (param_2 == 0x2ee00) {
      *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0xc0;
    }
    else if (param_2 == 0) {
      *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0;
    }
    else if (param_2 == 48000) {
      *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0x30;
    }
    else if (param_2 == 96000) {
      *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0x60;
    }
  }
  else if (param_2 == 0x5dc00) {
    *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0x180;
  }
  else if (param_2 == 0xbb800) {
    *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0x300;
  }
  else if (param_2 == 0x177000) {
    *(undefined4 *)(lVar1 * 0x108 + 0x334 + DAT_180020530) = 0x600;
  }
  if ((param_1 == 0) && (DAT_180020568 == 0)) {
    if (param_2 != 48000) {
      if (param_2 == 96000) {
        DAT_180020520 = 1;
        DAT_180020550 = 2;
        DAT_180020528 = 0;
        return;
      }
      if ((param_2 != 0x2ee00) && (param_2 == 0x5dc00)) {
        DAT_180020520 = 3;
        DAT_180020550 = 8;
        DAT_180020528 = 0;
        return;
      }
      DAT_180020520 = 2;
      DAT_180020550 = 4;
      DAT_180020528 = 0;
      return;
    }
    DAT_180020520 = 0;
    DAT_180020550 = 1;
    DAT_180020528 = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b320
   NAME : SetRxADC
   SIG  : undefined __fastcall SetRxADC(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetRxADC(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  bool bVar7;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  undefined1 uStack_5c4;
  byte bStack_5c3;
  byte bStack_5c2;
  byte bStack_5c1;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_58e;
  undefined1 uStack_75;
  ulonglong uStack_18;
  
                    /* 0xb320  207  SetRxADC */
  if ((*(int *)(DAT_180020530 + 0x4c) != param_1) &&
     (bVar7 = DAT_180020598 != 0xffffffffffffffff, *(int *)(DAT_180020530 + 0x4c) = param_1, bVar7))
  {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
    memset(acStack_5c8,0,0x5a4);
    s = DAT_180020598;
    uStack_5c4 = *(undefined1 *)(DAT_180020530 + 0x4c);
    uStack_5b5 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x334);
    bStack_5c3 = ((*(char *)(DAT_180020530 + 0x2cc) * '\x02' | *(byte *)(DAT_180020530 + 0x294)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x25c)) & 7;
    bStack_5c2 = ((*(char *)(DAT_180020530 + 0x2d0) * '\x02' | *(byte *)(DAT_180020530 + 0x298)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x260)) & 7;
    bStack_5c1 = (((((*(char *)(DAT_180020530 + 0x95c) * '\x02' | *(byte *)(DAT_180020530 + 0x854))
                     * '\x02' | *(byte *)(DAT_180020530 + 0x74c)) * '\x02' |
                   *(byte *)(DAT_180020530 + 0x644)) * '\x02' | *(byte *)(DAT_180020530 + 0x53c)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x434)) * '\x02' |
                 *(byte *)(DAT_180020530 + 0x32c);
    uStack_5b7 = *(undefined1 *)(DAT_180020530 + 0x324);
    uStack_5b6 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x334) >> 8);
    uStack_5b2 = *(undefined1 *)(DAT_180020530 + 0x338);
    uStack_5b1 = *(undefined1 *)(DAT_180020530 + 0x42c);
    uStack_5b0 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x43c) >> 8);
    uStack_5ac = *(undefined1 *)(DAT_180020530 + 0x440);
    uStack_5ab = *(undefined1 *)(DAT_180020530 + 0x534);
    uStack_5af = (undefined1)*(undefined4 *)(DAT_180020530 + 0x43c);
    uStack_5aa = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x544) >> 8);
    uStack_5a6 = *(undefined1 *)(DAT_180020530 + 0x548);
    uStack_5a5 = *(undefined1 *)(DAT_180020530 + 0x63c);
    uStack_5a9 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x544);
    uStack_5a4 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x64c) >> 8);
    uStack_5a0 = *(undefined1 *)(DAT_180020530 + 0x650);
    uStack_59f = *(undefined1 *)(DAT_180020530 + 0x744);
    uStack_5a3 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x64c);
    uStack_59e = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x754) >> 8);
    uStack_59a = *(undefined1 *)(DAT_180020530 + 0x758);
    uStack_599 = *(undefined1 *)(DAT_180020530 + 0x84c);
    uStack_59d = (undefined1)*(undefined4 *)(DAT_180020530 + 0x754);
    uStack_598 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x85c) >> 8);
    uStack_594 = *(undefined1 *)(DAT_180020530 + 0x860);
    uStack_593 = *(undefined1 *)(DAT_180020530 + 0x954);
    uStack_597 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x85c);
    uStack_592 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x964) >> 8);
    uStack_58e = *(undefined1 *)(DAT_180020530 + 0x968);
    uStack_75 = *(undefined1 *)(DAT_180020530 + 0x330);
    uStack_591 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x964);
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      uVar1 = *(undefined4 *)(DAT_180020530 + 4);
      sStack_5d8.sa_family = 0;
      sStack_5d8.sa_data[0] = '\0';
      sStack_5d8.sa_data[1] = '\0';
      sStack_5d8.sa_data[2] = '\0';
      sStack_5d8.sa_data[3] = '\0';
      sStack_5d8.sa_data[4] = '\0';
      sStack_5d8.sa_data[5] = '\0';
      sStack_5d8.sa_data[6] = '\0';
      sStack_5d8.sa_data[7] = '\0';
      sStack_5d8.sa_data[8] = '\0';
      sStack_5d8.sa_data[9] = '\0';
      sStack_5d8.sa_data[10] = '\0';
      sStack_5d8.sa_data[0xb] = '\0';
      sStack_5d8.sa_data[0xc] = '\0';
      sStack_5d8.sa_data[0xd] = '\0';
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      uVar2 = htons((short)uVar1 + 1);
      uVar6 = 0;
      uVar5 = 0x5a4;
      sStack_5d8.sa_data[2] = '\0';
      sStack_5d8.sa_data[3] = '\0';
      sStack_5d8.sa_data[4] = '\0';
      sStack_5d8.sa_data[5] = '\0';
      sStack_5d8.sa_data[0] = (char)uVar2;
      sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
      sStack_5d8.sa_family = 2;
      sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
      if (iVar3 < 1) {
        if (iVar3 == -1) {
          uVar4 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b340
   NAME : SetWBPacketsPerFrame
   SIG  : undefined __fastcall SetWBPacketsPerFrame(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetWBPacketsPerFrame(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xb340  235  SetWBPacketsPerFrame */
  bVar8 = DAT_180020598 != 0xffffffffffffffff;
  DAT_180020530[0x8d] = param_1;
  piVar2 = DAT_180020530;
  if (bVar8) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b360
   NAME : SetWBUpdateRate
   SIG  : undefined __fastcall SetWBUpdateRate(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetWBUpdateRate(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xb360  236  SetWBUpdateRate */
  bVar8 = DAT_180020598 != 0xffffffffffffffff;
  DAT_180020530[0x8c] = param_1;
  piVar2 = DAT_180020530;
  if (bVar8) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b380
   NAME : SetWBEnable
   SIG  : undefined __fastcall SetWBEnable(uint param_1, int param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetWBEnable(uint param_1,int param_2)

{
  uint uVar1;
  byte *pbVar2;
  int *piVar3;
  SOCKET s;
  u_short uVar4;
  int iVar5;
  uint uVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  bool bVar9;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xb380  234  SetWBEnable */
  if (param_2 == 0) {
    LOCK();
    pbVar2 = (byte *)((longlong)DAT_180020530 + ((longlong)(int)param_1 >> 3) + 0x238);
    *pbVar2 = *pbVar2 & ~('\x01' << (param_1 & 7));
    UNLOCK();
  }
  else {
    LOCK();
    pbVar2 = (byte *)((longlong)DAT_180020530 + ((longlong)(int)param_1 >> 3) + 0x238);
    *pbVar2 = *pbVar2 | '\x01' << (param_1 & 7);
    UNLOCK();
  }
  piVar3 = DAT_180020530;
  if (DAT_180020598 != 0xffffffffffffffff) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar5 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar5 >> 8);
    uStack_4a = (undefined1)(iVar5 + 3);
    uStack_4b = (undefined1)((uint)(iVar5 + 3) >> 8);
    uStack_48 = (undefined1)(iVar5 + 4);
    uStack_52 = (undefined1)iVar5;
    uStack_49 = (undefined1)((uint)(iVar5 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar5 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar5 + 1) >> 8);
    uStack_50 = (undefined1)(iVar5 + 1);
    uStack_4f = (undefined1)((uint)(iVar5 + 2) >> 8);
    uStack_4e = (undefined1)(iVar5 + 2);
    uStack_46 = (undefined1)(iVar5 + 10);
    uVar6 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar3[0x8e];
      bVar9 = uVar6 == uVar1;
      if (bVar9) {
        piVar3[0x8e] = uVar6 & 0xff;
        uVar1 = uVar6;
      }
      uVar6 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar9);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar6));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar5 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar4 = htons((u_short)iVar5);
      uVar8 = 0;
      uVar7 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar4;
      sStack_68.sa_data[1] = (char)(uVar4 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar5 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar5 < 1) {
        if (iVar5 == -1) {
          uVar6 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar6,uVar7,uVar8);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar5;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b3b0
   NAME : SendHighPriority
   SIG  : undefined __fastcall SendHighPriority(int param_1)
   ======================================================================== */

void SendHighPriority(int param_1)

{
  bool bVar1;
  
                    /* 0xb3b0  54  SendHighPriority */
  if (param_1 == 0) {
    *(undefined4 *)(DAT_180020530 + 0x48) = 0;
  }
  else {
    bVar1 = DAT_180020598 != -1;
    *(undefined4 *)(DAT_180020530 + 0x48) = 1;
    if (bVar1) {
      FUN_18000d990();
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b3e0
   NAME : SetWatchdogTimer
   SIG  : undefined __fastcall SetWatchdogTimer(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetWatchdogTimer(int param_1)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr sStack_68;
  char acStack_58 [6];
  undefined1 uStack_52;
  undefined1 uStack_51;
  undefined1 uStack_50;
  undefined1 uStack_4f;
  undefined1 uStack_4e;
  undefined1 uStack_4d;
  undefined1 uStack_4c;
  undefined1 uStack_4b;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  undefined1 uStack_48;
  undefined1 uStack_47;
  undefined1 uStack_46;
  undefined1 uStack_45;
  undefined1 uStack_44;
  undefined1 uStack_43;
  undefined1 uStack_42;
  undefined8 uStack_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 uStack_31;
  undefined8 uStack_29;
  int iStack_21;
  byte bStack_1d;
  ulonglong uStack_18;
  
                    /* 0xb3e0  237  SetWatchdogTimer */
  if ((DAT_180020530[0x11] != param_1) &&
     (bVar8 = DAT_180020598 != 0xffffffffffffffff, DAT_180020530[0x11] = param_1,
     piVar2 = DAT_180020530, bVar8)) {
    uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
    acStack_58[0] = '\0';
    acStack_58[1] = '\0';
    acStack_58[2] = '\0';
    acStack_58[3] = '\0';
    iVar4 = *DAT_180020530;
    acStack_58[4] = 0;
    acStack_58[5] = (char)((uint)iVar4 >> 8);
    uStack_4a = (undefined1)(iVar4 + 3);
    uStack_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
    uStack_48 = (undefined1)(iVar4 + 4);
    uStack_52 = (undefined1)iVar4;
    uStack_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
    uStack_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
    uStack_31 = 0;
    uStack_29 = 0;
    uStack_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
    uStack_50 = (undefined1)(iVar4 + 1);
    uStack_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
    uStack_4e = (undefined1)(iVar4 + 2);
    uStack_46 = (undefined1)(iVar4 + 10);
    uVar5 = DAT_180020530[0x8e];
    do {
      LOCK();
      uVar1 = piVar2[0x8e];
      bVar8 = uVar5 == uVar1;
      if (bVar8) {
        piVar2[0x8e] = uVar5 & 0xff;
        uVar1 = uVar5;
      }
      uVar5 = uVar1;
      s = DAT_180020598;
      UNLOCK();
    } while (!bVar8);
    uStack_41._0_3_ =
         CONCAT12((char)DAT_180020530[0x8a],
                  CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
    uStack_41 = (ulonglong)
                CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                                  CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                         (undefined3)uStack_41);
    _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                          CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                     CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8
                                                                    ),CONCAT11((char)DAT_180020530
                                                                                     [0x3ec],
                                                                               (char)((uint)
                                                  DAT_180020530[0x3ec] >> 8)))) << 0x10));
    iStack_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
    bStack_1d = *DAT_1800205d0 | *DAT_1800205b0;
    if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
      iVar4 = DAT_180020530[1];
      sStack_68.sa_family = 0;
      sStack_68.sa_data[0] = '\0';
      sStack_68.sa_data[1] = '\0';
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[6] = '\0';
      sStack_68.sa_data[7] = '\0';
      sStack_68.sa_data[8] = '\0';
      sStack_68.sa_data[9] = '\0';
      sStack_68.sa_data[10] = '\0';
      sStack_68.sa_data[0xb] = '\0';
      sStack_68.sa_data[0xc] = '\0';
      sStack_68.sa_data[0xd] = '\0';
      uStack_4d = acStack_58[5];
      uStack_4c = uStack_52;
      uStack_45 = uStack_51;
      uStack_44 = uStack_50;
      uStack_43 = uStack_4f;
      uStack_42 = uStack_4e;
      EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      uVar3 = htons((u_short)iVar4);
      uVar7 = 0;
      uVar6 = 0x3c;
      sStack_68.sa_data[2] = '\0';
      sStack_68.sa_data[3] = '\0';
      sStack_68.sa_data[4] = '\0';
      sStack_68.sa_data[5] = '\0';
      sStack_68.sa_data[0] = (char)uVar3;
      sStack_68.sa_data[1] = (char)(uVar3 >> 8);
      sStack_68.sa_family = 2;
      sStack_68.sa_data[2] = (undefined1)DAT_18001e1d8;
      sStack_68.sa_data[3] = DAT_18001e1d8._1_1_;
      sStack_68.sa_data[4] = DAT_18001e1d8._2_1_;
      sStack_68.sa_data[5] = DAT_18001e1d8._3_1_;
      iVar4 = sendto(s,acStack_58,0x3c,0,&sStack_68,0x10);
      LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
      if (iVar4 < 1) {
        if (iVar4 == -1) {
          uVar5 = WSAGetLastError();
          FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
        }
      }
      else {
        LOCK();
        DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
        UNLOCK();
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b400
   NAME : SetMKIIBPF
   SIG  : undefined __fastcall SetMKIIBPF(undefined4 param_1)
   ======================================================================== */

void SetMKIIBPF(undefined4 param_1)

{
                    /* 0xb400  171  SetMKIIBPF */
  DAT_180020518 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 18000b410
   NAME : SetXVTREnable
   SIG  : undefined __fastcall SetXVTREnable(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void SetXVTREnable(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xb410  240  SetXVTREnable */
  if ((_DAT_180020578 == param_1) || (_DAT_180020578 = param_1, DAT_180020598 == 0xffffffffffffffff)
     ) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b430
   NAME : ATU_Tune
   SIG  : undefined __fastcall ATU_Tune(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void ATU_Tune(int param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xb430  1  ATU_Tune */
  if ((_DAT_1800205cc == param_1) || (_DAT_1800205cc = param_1, DAT_180020598 == 0xffffffffffffffff)
     ) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | (char)_DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b450
   NAME : getLEDs
   SIG  : undefined4 __fastcall getLEDs(void)
   ======================================================================== */

undefined4 getLEDs(void)

{
                    /* 0xb450  271  getLEDs */
  return *(undefined4 *)(DAT_180020530 + 0x21c);
}



/* ========================================================================
   ENTRY: 18000b460
   NAME : LRAudioSwap
   SIG  : undefined __fastcall LRAudioSwap(int param_1)
   ======================================================================== */

void LRAudioSwap(int param_1)

{
                    /* 0xb460  39  LRAudioSwap */
  if (*(int *)(DAT_180020530 + 0x23c) != param_1) {
    *(int *)(DAT_180020530 + 0x23c) = param_1;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000b480
   NAME : create_rnet
   SIG  : undefined __fastcall create_rnet(void)
   ======================================================================== */

void create_rnet(void)

{
  undefined *puVar1;
  longlong lVar2;
  undefined4 *puVar3;
  void *pvVar4;
  undefined8 uVar5;
  uint uVar6;
  ulonglong uVar7;
  
                    /* 0xb480  253  create_rnet */
  puVar3 = malloc(0x1080);
  DAT_180020530 = puVar3;
  if (puVar3 != (undefined4 *)0x0) {
    LOCK();
    DAT_18001fbb0 = 0;
    UNLOCK();
    LOCK();
    DAT_18001fbc0 = 0;
    UNLOCK();
    LOCK();
    DAT_18001fbd0 = 0;
    UNLOCK();
    LOCK();
    DAT_18001fbb8 = 0;
    UNLOCK();
    LOCK();
    DAT_18001fb98 = 0;
    UNLOCK();
    LOCK();
    DAT_18001fba8 = 0;
    UNLOCK();
    DAT_18001fbc8 = 0;
    DAT_18001fba0 = 0;
    puVar3[1] = 0x400;
    *puVar3 = 0x401;
    pvVar4 = calloc(8,8);
    *(void **)(puVar3 + 2) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    **(undefined8 **)(DAT_180020530 + 2) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 8) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x10) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x18) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x20) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x28) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x30) = pvVar4;
    pvVar4 = calloc(0x40,0x10);
    *(void **)(*(longlong *)(DAT_180020530 + 2) + 0x38) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0xf00);
    *(void **)(puVar3 + 4) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0x2d00);
    *(void **)(puVar3 + 6) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0x5a4);
    *(void **)(puVar3 + 8) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0x5a0);
    *(void **)(puVar3 + 10) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0x2d00);
    *(void **)(puVar3 + 0xc) = pvVar4;
    puVar3 = DAT_180020530;
    pvVar4 = calloc(1,0x2d00);
    *(void **)(puVar3 + 0xe) = pvVar4;
    puVar3 = DAT_180020530;
    *(undefined8 *)(DAT_180020530 + 0x10) = 0;
    puVar3[0x12] = 1;
    puVar3[0x13] = 1;
    *(undefined8 *)(puVar3 + 0x14) = 1;
    *(undefined8 *)(puVar3 + 0x16) = 0;
    puVar3[0x18] = 0;
    *(undefined8 *)(puVar3 + 0x22) = 0;
    *(undefined1 *)(puVar3 + 0xc3) = 0;
    *(undefined8 *)(puVar3 + 0xbc) = 0;
    *(undefined8 *)(puVar3 + 0xbe) = 0;
    *(undefined8 *)(puVar3 + 0xc0) = 0;
    puVar3[0xc2] = 7;
    *(undefined1 *)(puVar3 + 0xc5) = 0;
    puVar3[0xc4] = 0;
    puVar3[0xc6] = 0x40;
    puVar3[0x89] = 0x20;
    puVar3[0x8e] = 0;
    puVar3[0x8a] = 0x200;
    puVar3[0x8b] = 0x10;
    puVar3[0x8c] = 0x46;
    puVar3[0x8d] = 0x20;
    *(undefined8 *)(puVar3 + 0x8f) = 0;
    *(undefined8 *)(puVar3 + 0x92) = 0;
    puVar3[0x94] = 0x1f;
    *(undefined8 *)(puVar3 + 0x96) = 0;
    *(undefined8 *)(puVar3 + 0x98) = 0;
    puVar3[0x9a] = 0;
    uVar5 = malloc0(0x2000);
    puVar3 = DAT_180020530;
    *(undefined8 *)(DAT_180020530 + 0x9c) = uVar5;
    *(undefined8 *)(puVar3 + 0xa0) = 1;
    puVar3[0xa2] = 0x1f;
    *(undefined8 *)(puVar3 + 0xa4) = 0;
    *(undefined8 *)(puVar3 + 0xa6) = 0;
    puVar3[0xa8] = 0;
    uVar5 = malloc0(0x2000);
    puVar3 = DAT_180020530;
    *(undefined8 *)(DAT_180020530 + 0xaa) = uVar5;
    *(undefined8 *)(puVar3 + 0xae) = 2;
    puVar3[0xb0] = 0x1f;
    *(undefined8 *)(puVar3 + 0xb2) = 0;
    *(undefined8 *)(puVar3 + 0xb4) = 0;
    puVar3[0xb6] = 0;
    uVar5 = malloc0(0x2000);
    puVar3 = DAT_180020530;
    *(undefined8 *)(DAT_180020530 + 0xb8) = uVar5;
    uVar7 = 0;
    do {
      puVar3[uVar7 * 0x42 + 200] = (int)uVar7;
      uVar6 = (int)uVar7 + 1;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xc9) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xcb) = 0;
      puVar3[uVar7 * 0x42 + 0xcd] = 0x30;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xce) = 0x18;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xd0) = 0;
      puVar3[uVar7 * 0x42 + 0xd2] = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xd4) = 0;
      puVar3[uVar7 * 0x42 + 0xd6] = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xd7) = 0xee;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xff) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xd9) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xdb) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xdd) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xdf) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xe1) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xe3) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xe5) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xe7) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xe9) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xeb) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xed) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xef) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xf1) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xf3) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xf5) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xf7) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xf9) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xfb) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0xfd) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0x102) = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0x104) = 0;
      puVar3[uVar7 * 0x42 + 0x106] = 0;
      *(undefined8 *)(puVar3 + uVar7 * 0x42 + 0x108) = 0;
      uVar7 = (ulonglong)uVar6;
    } while ((int)uVar6 < 0xc);
    *(undefined8 *)(puVar3 + 0x3e0) = 0;
    *(undefined8 *)(puVar3 + 0x3e2) = 0xc0;
    *(undefined8 *)(puVar3 + 0x3e4) = 0;
    *(undefined8 *)(puVar3 + 0x3e6) = 0;
    *(undefined8 *)(puVar3 + 0x3eb) = 0;
    *(undefined8 *)(puVar3 + 0x3ed) = 0;
    *(undefined8 *)(puVar3 + 0x3ef) = 0;
    puVar3[0x3f1] = 0;
    puVar3[0x3f2] = 0xf0;
    *(undefined8 *)(puVar3 + 0x3f3) = 1;
    *(undefined8 *)(puVar3 + 0x3f5) = 0xc0;
    *(undefined8 *)(puVar3 + 0x3f7) = 0;
    *(undefined8 *)(puVar3 + 0x3f9) = 0;
    *(undefined8 *)(puVar3 + 0x3fe) = 0;
    *(undefined8 *)(puVar3 + 0x400) = 0;
    *(undefined8 *)(puVar3 + 0x402) = 0;
    puVar3[0x404] = 0;
    puVar3[0x405] = 0xf0;
    *(undefined8 *)(puVar3 + 0x406) = 2;
    *(undefined8 *)(puVar3 + 0x408) = 0xc0;
    *(undefined8 *)(puVar3 + 0x40a) = 0;
    *(undefined8 *)(puVar3 + 0x40c) = 0;
    *(undefined8 *)(puVar3 + 0x411) = 0;
    *(undefined8 *)(puVar3 + 0x413) = 0;
    *(undefined8 *)(puVar3 + 0x415) = 0;
    puVar3[0x417] = 0;
    puVar3[0x418] = 0xf0;
    puVar3[0x419] = 0x40;
    *(undefined8 *)(puVar3 + 0x41a) = 0x40;
    puVar3[0x88] = 0;
    *(undefined8 *)(puVar3 + 0x41c) = 0;
    puVar3[0x41e] = 0;
    DAT_1800205b0 = (undefined8 *)malloc0(8);
    *DAT_1800205b0 = 1;
    DAT_1800205d0 = (undefined8 *)malloc0(8);
    puVar3 = DAT_180020530;
    *DAT_1800205d0 = 2;
    *(undefined8 *)(puVar3 + 0x24) = 0;
    *(undefined8 *)(puVar3 + 0x26) = 0;
    *(undefined8 *)(puVar3 + 0x28) = 0;
    *(undefined8 *)(puVar3 + 0x2a) = 0;
    *(undefined8 *)(puVar3 + 0x38) = 0;
    *(undefined8 *)(puVar3 + 0x3a) = 0;
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(puVar3 + 0x3e),0x9c4);
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(DAT_180020530 + 0x48),0x9c4);
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(DAT_180020530 + 0x52),0x9c4);
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c),0x9c4);
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(DAT_180020530 + 0x70),0x9c4);
    InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(DAT_180020530 + 0x66),0);
    lVar2 = DAT_18001e140;
    puVar1 = PTR_DAT_18001e080;
    if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 0) {
      *(code **)(PTR_DAT_18001e080 + 0xcc0) = OutBound;
      *(code **)(lVar2 + 0xef0) = OutBound;
    }
    else if (*(int *)(PTR_DAT_18001e080 + 0xce4) == 1) {
      *(code **)(PTR_DAT_18001e080 + 0xcc0) = FUN_180003670;
      *(code **)(lVar2 + 0xef0) = FUN_180003670;
    }
    *(code **)(puVar1 + 0xcc8) = OutBound;
    *(code **)(*(longlong *)(puVar1 + 0x1130) + 0x20) = OutBound;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000bbc0
   NAME : clearSnapshots
   SIG  : undefined __fastcall clearSnapshots(void)
   ======================================================================== */

void clearSnapshots(void)

{
  longlong lVar1;
  undefined8 *_Memory;
  longlong lVar2;
  longlong lVar3;
  longlong lVar4;
  
                    /* 0xbbc0  251  clearSnapshots */
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x198));
  lVar4 = 0;
  lVar2 = DAT_180020530;
  do {
    lVar3 = lVar4 * 0x108;
    lVar1 = *(longlong *)(lVar3 + 0x408 + lVar2);
    while (lVar1 != 0) {
      _Memory = *(undefined8 **)(lVar3 + 0x408 + lVar2);
      *(undefined8 *)(lVar3 + 0x408 + lVar2) = *_Memory;
      free(_Memory);
      lVar2 = DAT_180020530;
      lVar1 = *(longlong *)(lVar3 + 0x408 + DAT_180020530);
    }
    lVar4 = lVar4 + 1;
    *(undefined4 *)(lVar3 + 0x418 + lVar2) = 0;
    *(undefined8 *)(lVar3 + 0x410 + lVar2) = 0;
  } while (lVar4 != 0xc);
                    /* WARNING: Could not recover jumptable at 0x00018000bc5c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x198));
  return;
}



/* ========================================================================
   ENTRY: 18000bc70
   NAME : destroy_rnet
   SIG  : undefined __fastcall destroy_rnet(void)
   ======================================================================== */

void destroy_rnet(void)

{
  undefined8 *_Memory;
  void *pvVar1;
  longlong lVar2;
  longlong lVar3;
  longlong lVar4;
  
                    /* 0xbc70  255  destroy_rnet */
  lVar4 = 0;
  lVar2 = lVar4;
  do {
    _aligned_free(*(void **)(lVar2 * 0x38 + 0x270 + (longlong)DAT_180020530));
    lVar2 = lVar2 + 1;
  } while (lVar2 != 3);
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0xf8));
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0x148));
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0x170));
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0x1c0));
  if (*(void **)((longlong)DAT_180020530 + 0x28) != (void *)0x0) {
    free(*(void **)((longlong)DAT_180020530 + 0x28));
  }
  free(*(void **)((longlong)DAT_180020530 + 0x18));
  free(*(void **)((longlong)DAT_180020530 + 0x10));
  lVar2 = lVar4;
  do {
    free(*(void **)(*(longlong *)((longlong)DAT_180020530 + 8) + lVar2 * 8));
    lVar2 = lVar2 + 1;
  } while (lVar2 != 8);
  free(*(void **)((longlong)DAT_180020530 + 8));
  free(*(void **)((longlong)DAT_180020530 + 0x20));
  EnterCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0x198));
  pvVar1 = DAT_180020530;
  do {
    lVar3 = lVar4 * 0x108;
    lVar2 = *(longlong *)(lVar3 + 0x408 + (longlong)pvVar1);
    while (lVar2 != 0) {
      _Memory = *(undefined8 **)(lVar3 + 0x408 + (longlong)pvVar1);
      *(undefined8 *)(lVar3 + 0x408 + (longlong)pvVar1) = *_Memory;
      free(_Memory);
      pvVar1 = DAT_180020530;
      lVar2 = *(longlong *)(lVar3 + 0x408 + (longlong)DAT_180020530);
    }
    lVar4 = lVar4 + 1;
    *(undefined4 *)(lVar3 + 0x418 + (longlong)pvVar1) = 0;
    *(undefined8 *)(lVar3 + 0x410 + (longlong)pvVar1) = 0;
  } while (lVar4 != 0xc);
  LeaveCriticalSection((LPCRITICAL_SECTION)((longlong)pvVar1 + 0x198));
  DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020530 + 0x198));
  free(DAT_180020530);
  _aligned_free(DAT_1800205b0);
                    /* WARNING: Could not recover jumptable at 0x00018000be47. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _aligned_free(DAT_1800205d0);
  return;
}



/* ========================================================================
   ENTRY: 18000be50
   NAME : SetCATPort
   SIG  : undefined __fastcall SetCATPort(undefined4 param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void SetCATPort(undefined4 param_1)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_608 [32];
  sockaddr sStack_5d8;
  char acStack_5c8 [4];
  byte bStack_5c4;
  byte bStack_5c3;
  undefined1 uStack_5bf;
  undefined1 uStack_5be;
  undefined1 uStack_5bd;
  undefined1 uStack_5bc;
  undefined1 uStack_5bb;
  undefined1 uStack_5ba;
  undefined1 uStack_5b9;
  undefined1 uStack_5b8;
  undefined1 uStack_5b7;
  undefined1 uStack_5b6;
  undefined1 uStack_5b5;
  undefined1 uStack_5b4;
  undefined1 uStack_5b3;
  undefined1 uStack_5b2;
  undefined1 uStack_5b1;
  undefined1 uStack_5b0;
  undefined1 uStack_5af;
  undefined1 uStack_5ae;
  undefined1 uStack_5ad;
  undefined1 uStack_5ac;
  undefined1 uStack_5ab;
  undefined1 uStack_5aa;
  undefined1 uStack_5a9;
  undefined1 uStack_5a8;
  undefined1 uStack_5a7;
  undefined1 uStack_5a6;
  undefined1 uStack_5a5;
  undefined1 uStack_5a4;
  undefined1 uStack_5a3;
  undefined1 uStack_5a2;
  undefined1 uStack_5a1;
  undefined1 uStack_5a0;
  undefined1 uStack_59f;
  undefined1 uStack_59e;
  undefined1 uStack_59d;
  undefined1 uStack_59c;
  undefined1 uStack_59b;
  undefined1 uStack_59a;
  undefined1 uStack_599;
  undefined1 uStack_598;
  undefined1 uStack_597;
  undefined1 uStack_596;
  undefined1 uStack_595;
  undefined1 uStack_594;
  undefined1 uStack_593;
  undefined1 uStack_592;
  undefined1 uStack_591;
  undefined1 uStack_590;
  undefined1 uStack_47f;
  undefined1 uStack_47e;
  undefined1 uStack_47d;
  undefined1 uStack_47c;
  undefined1 uStack_46f;
  byte bStack_53;
  undefined1 uStack_52;
  undefined1 uStack_51;
  byte bStack_50;
  char cStack_4f;
  byte bStack_4e;
  byte bStack_4d;
  undefined1 uStack_34;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined1 uStack_31;
  undefined1 uStack_30;
  undefined1 uStack_2f;
  undefined1 uStack_2e;
  undefined1 uStack_2d;
  undefined1 uStack_26;
  undefined1 uStack_25;
  ulonglong uStack_18;
  
                    /* 0xbe50  96  SetCATPort */
  bVar8 = DAT_180020598 == 0xffffffffffffffff;
  *(undefined4 *)(DAT_180020530 + 0x240) = param_1;
  if (bVar8) {
    return;
  }
  uStack_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(acStack_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  bStack_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  bStack_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
                '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    uStack_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    uStack_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    uStack_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    uStack_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    uStack_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    uStack_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    uStack_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  uStack_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  uStack_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  uStack_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5b1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  uStack_5af = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5ae = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5ad = (undefined1)((uint)uVar1 >> 8);
  uStack_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  uStack_5ab = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5aa = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a9 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  uStack_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a5 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  uStack_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_5a1 = (undefined1)((uint)uVar1 >> 8);
  uStack_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  uStack_59f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_59d = (undefined1)((uint)uVar1 >> 8);
  uStack_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  uStack_59b = (undefined1)((uint)uVar1 >> 0x18);
  uStack_59a = (undefined1)((uint)uVar1 >> 0x10);
  uStack_599 = (undefined1)((uint)uVar1 >> 8);
  uStack_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  uStack_597 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_596 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_595 = (undefined1)((uint)uVar1 >> 8);
  uStack_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  uStack_593 = (undefined1)((uint)uVar1 >> 0x18);
  uStack_592 = (undefined1)((uint)uVar1 >> 0x10);
  uStack_591 = (undefined1)((uint)uVar1 >> 8);
  uStack_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  uStack_47f = (undefined1)((uint)uVar1 >> 0x18);
  uStack_47e = (undefined1)((uint)uVar1 >> 0x10);
  uStack_47d = (undefined1)((uint)uVar1 >> 8);
  uStack_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  uStack_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    bStack_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  uStack_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  uStack_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  bStack_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  cStack_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  bStack_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  bStack_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  uStack_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  uStack_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  uStack_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  uStack_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  uStack_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  uStack_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  uStack_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  uStack_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  uStack_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  uStack_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    sStack_5d8.sa_family = 0;
    sStack_5d8.sa_data[0] = '\0';
    sStack_5d8.sa_data[1] = '\0';
    sStack_5d8.sa_data[2] = '\0';
    sStack_5d8.sa_data[3] = '\0';
    sStack_5d8.sa_data[4] = '\0';
    sStack_5d8.sa_data[5] = '\0';
    sStack_5d8.sa_data[6] = '\0';
    sStack_5d8.sa_data[7] = '\0';
    sStack_5d8.sa_data[8] = '\0';
    sStack_5d8.sa_data[9] = '\0';
    sStack_5d8.sa_data[10] = '\0';
    sStack_5d8.sa_data[0xb] = '\0';
    sStack_5d8.sa_data[0xc] = '\0';
    sStack_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    sStack_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    sStack_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    sStack_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    sStack_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    sStack_5d8.sa_data[0] = (char)uVar2;
    sStack_5d8.sa_data[1] = (char)(uVar2 >> 8);
    sStack_5d8.sa_family = 2;
    iVar3 = sendto(s,acStack_5c8,0x5a4,0,&sStack_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000be70
   NAME : FUN_18000be70
   SIG  : undefined __fastcall FUN_18000be70(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

void FUN_18000be70(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 uVar1;
  undefined8 *puVar2;
  undefined8 local_res10;
  undefined8 local_res18;
  undefined8 local_res20;
  
  local_res10 = param_2;
  local_res18 = param_3;
  local_res20 = param_4;
  uVar1 = __acrt_iob_func(1);
  puVar2 = (undefined8 *)FUN_1800032a0();
  __stdio_common_vfwprintf(*puVar2,uVar1,param_1,0,&local_res10);
  return;
}



/* ========================================================================
   ENTRY: 18000bed0
   NAME : FUN_18000bed0
   SIG  : int __fastcall FUN_18000bed0(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

int FUN_18000bed0(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  int iVar1;
  ulonglong *puVar2;
  undefined8 local_res18;
  undefined8 local_res20;
  
  local_res18 = param_3;
  local_res20 = param_4;
  puVar2 = (ulonglong *)FUN_1800032a0();
  iVar1 = __stdio_common_vsprintf(*puVar2 | 1,param_1,0xffffffffffffffff,param_2,0,&local_res18);
  if (iVar1 < 0) {
    iVar1 = -1;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 18000bf30
   NAME : NotifyRxReconfig
   SIG  : undefined __fastcall NotifyRxReconfig(void)
   ======================================================================== */

void NotifyRxReconfig(void)

{
                    /* 0xbf30  42  NotifyRxReconfig */
  DAT_1800201e0 = 1;
  return;
}



/* ========================================================================
   ENTRY: 18000bf40
   NAME : DeInitMetisSockets
   SIG  : undefined __fastcall DeInitMetisSockets(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void DeInitMetisSockets(void)

{
                    /* 0xbf40  11  DeInitMetisSockets */
  closesocket(DAT_180020598);
  DAT_180020598 = 0xffffffffffffffff;
  if (DAT_18001e1dc != 0) {
    WSACleanup();
    DAT_18001e1dc = 0;
    _DAT_180020510 = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000bf80
   NAME : nativeInitMetis
   SIG  : undefined8 __fastcall nativeInitMetis(char * param_1, int param_2, char * param_3, undefined8 param_4, int param_5, undefined4 param_6, int param_7)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Type propagation algorithm not settling */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
nativeInitMetis(char *param_1,int param_2,char *param_3,undefined8 param_4,int param_5,
               undefined4 param_6,int param_7)

{
  int *piVar1;
  u_short uVar2;
  ulong uVar3;
  uint uVar4;
  int iVar5;
  FILE *pFVar6;
  char *pcVar7;
  undefined8 uVar8;
  LPWSADATA lpWSAData;
  ulonglong uVar9;
  undefined8 uVar10;
  u_long *argp;
  char *optval;
  undefined1 auStackY_228 [32];
  char local_1f8 [4];
  u_long local_1f4 [3];
  undefined8 local_1e8;
  sockaddr local_1e0;
  WSADATA local_1c8;
  ulonglong local_28;
  
                    /* 0xbf80  294  nativeInitMetis */
  piVar1 = DAT_180020530;
  local_28 = DAT_18001e000 ^ (ulonglong)auStackY_228;
  local_1f4[1] = 6;
  DAT_180020568 = param_5;
  DAT_180020584 = param_6;
  DAT_180020530[1] = param_2;
  local_1e0.sa_family = 0;
  local_1e0.sa_data[0] = '\0';
  local_1e0.sa_data[1] = '\0';
  local_1e0.sa_data[2] = '\0';
  local_1e0.sa_data[3] = '\0';
  local_1e0.sa_data[4] = '\0';
  local_1e0.sa_data[5] = '\0';
  local_1e0.sa_data[6] = '\0';
  local_1e0.sa_data[7] = '\0';
  local_1e0.sa_data[8] = '\0';
  local_1e0.sa_data[9] = '\0';
  local_1e0.sa_data[10] = '\0';
  local_1e0.sa_data[0xb] = '\0';
  local_1e0.sa_data[0xc] = '\0';
  local_1e0.sa_data[0xd] = '\0';
  if (param_5 == 1) {
    if (param_7 == 0) {
      *piVar1 = 0x401;
    }
    else {
      *piVar1 = param_2 + 1;
    }
  }
  uVar2 = htons((u_short)param_4);
  local_1e0.sa_data[0] = (char)uVar2;
  local_1e0.sa_data[1] = (char)(uVar2 >> 8);
  local_1e0.sa_family = 2;
  uVar3 = inet_addr(param_3);
  local_1e0.sa_data[2] = (char)uVar3;
  local_1e0.sa_data[3] = (char)(uVar3 >> 8);
  local_1e0.sa_data[4] = (char)(uVar3 >> 0x10);
  local_1e0.sa_data[5] = (char)(uVar3 >> 0x18);
  iVar5 = 1;
  do {
    uVar10 = 0;
    uVar8 = 2;
    DAT_180020598 = socket(2,2,0);
    if (DAT_180020598 != 0xffffffffffffffff) {
      optval = local_1f8;
      local_1f8[0] = '\0';
      local_1f8[1] = '\0';
      local_1f8[2] = ' ';
      local_1f8[3] = '\0';
      argp = (u_long *)0x1001;
      iVar5 = setsockopt(DAT_180020598,0xffff,0x1001,optval,4);
      if (iVar5 == 0) {
        optval = local_1f8;
        local_1f8[0] = '\0';
        local_1f8[1] = '\0';
        local_1f8[2] = -0x80;
        local_1f8[3] = '\0';
        argp = (u_long *)0x1002;
        iVar5 = setsockopt(DAT_180020598,0xffff,0x1002,optval,4);
        if (iVar5 == 0) {
          optval = local_1f8;
          local_1f8[0] = -0xc;
          local_1f8[1] = '\x01';
          local_1f8[2] = '\0';
          local_1f8[3] = '\0';
          argp = (u_long *)0x1006;
          iVar5 = setsockopt(DAT_180020598,0xffff,0x1006,optval,4);
          if (iVar5 == 0) {
            optval = local_1f8;
            local_1f8[0] = -0xc;
            local_1f8[1] = '\x01';
            local_1f8[2] = '\0';
            local_1f8[3] = '\0';
            argp = (u_long *)0x1005;
            iVar5 = setsockopt(DAT_180020598,0xffff,0x1005,optval,4);
            if (iVar5 == 0) {
              argp = local_1f4;
              local_1f4[0] = 1;
              iVar5 = ioctlsocket(DAT_180020598,-0x7ffb9982,argp);
              if (iVar5 == 0) {
                argp = (u_long *)0x10;
                iVar5 = bind(DAT_180020598,&local_1e0,0x10);
                if (iVar5 != -1) {
                  DAT_18001e1d8 = inet_addr(param_1);
                  pFVar6 = (FILE *)__acrt_iob_func(1);
                  fflush(pFVar6);
                  uVar3 = inet_addr(param_1);
                  if (uVar3 != 0) {
                    FUN_180007660("destination addr: 0x%08x\n",(ulonglong)uVar3,argp,optval);
                    pFVar6 = (FILE *)__acrt_iob_func(1);
                    fflush(pFVar6);
                    local_1e8 = 0xffffffffffffffff;
                    SendARP(uVar3,0,&local_1e8,local_1f4 + 1);
                    return 0;
                  }
                  closesocket(DAT_180020598);
                  if (DAT_18001e1dc != 0) {
                    WSACleanup();
                  }
                  return 0xfffffffc;
                }
                uVar4 = WSAGetLastError();
                pcVar7 = "bind failed %ld\n";
              }
              else {
                uVar4 = WSAGetLastError();
                pcVar7 = "ioctlsocket FIONBIO failed %ld\n";
              }
            }
            else {
              uVar4 = WSAGetLastError();
              pcVar7 = "setsockopt SO_SNDTIMEO failed %ld\n";
            }
          }
          else {
            uVar4 = WSAGetLastError();
            pcVar7 = "setsockopt SO_RCVTIMEO failed %ld\n";
          }
        }
        else {
          uVar4 = WSAGetLastError();
          pcVar7 = "setsockopt SO_RCVBUF failed %ld\n";
        }
      }
      else {
        uVar4 = WSAGetLastError();
        pcVar7 = "setsockopt SO_SNDBUF failed %ld\n";
      }
      FUN_180007660(pcVar7,(ulonglong)uVar4,argp,optval);
      closesocket(DAT_180020598);
LAB_18000c269:
      if (DAT_18001e1dc != 0) {
        WSACleanup();
      }
      return 0xffffffff;
    }
    uVar4 = WSAGetLastError();
    if (uVar4 != 0x276d) {
      FUN_180007660("createSocket Error: socket failed %ld\n",(ulonglong)uVar4,uVar10,param_4);
      goto LAB_18000c269;
    }
    DAT_18001e1dc = 0;
    if (iVar5 == 2) {
      FUN_180007660("socket() still returns wsa not initialised!\n",uVar8,uVar10,param_4);
      return 1;
    }
    lpWSAData = &local_1c8;
    iVar5 = WSAStartup(0x202,lpWSAData);
    if (iVar5 != 0) {
      uVar4 = WSAGetLastError();
      uVar9 = (ulonglong)uVar4;
      FUN_180007660("Failed. Error Code : %d",uVar9,uVar10,param_4);
      FUN_180007660("initWSA failed rc!\n",uVar9,uVar10,param_4);
      return 1;
    }
    _DAT_180020510 = 1;
    DAT_18001e1dc = 1;
    FUN_180007660("initWSA ok!\n",lpWSAData,uVar10,param_4);
    iVar5 = 2;
  } while( true );
}



/* ========================================================================
   ENTRY: 18000c340
   NAME : GetMetisIPAddr
   SIG  : undefined4 __fastcall GetMetisIPAddr(void)
   ======================================================================== */

undefined4 GetMetisIPAddr(void)

{
                    /* 0xc340  33  GetMetisIPAddr */
  return DAT_18001e1d8;
}



/* ========================================================================
   ENTRY: 18000c350
   NAME : GetMACAddr
   SIG  : undefined __fastcall GetMACAddr(undefined4 * param_1)
   ======================================================================== */

void GetMACAddr(undefined4 *param_1)

{
  longlong lVar1;
  
                    /* 0xc350  32  GetMACAddr */
  lVar1 = DAT_180020530;
  *param_1 = *(undefined4 *)(DAT_180020530 + 0x106c);
  *(undefined2 *)(param_1 + 1) = *(undefined2 *)(lVar1 + 0x1070);
  return;
}



/* ========================================================================
   ENTRY: 18000c370
   NAME : GetCodeVersion
   SIG  : undefined __fastcall GetCodeVersion(undefined1 * param_1)
   ======================================================================== */

void GetCodeVersion(undefined1 *param_1)

{
                    /* 0xc370  29  GetCodeVersion */
  *param_1 = *(undefined1 *)(DAT_180020530 + 0x1074);
  return;
}



/* ========================================================================
   ENTRY: 18000c390
   NAME : GetBoardID
   SIG  : undefined __fastcall GetBoardID(undefined1 * param_1)
   ======================================================================== */

void GetBoardID(undefined1 *param_1)

{
                    /* 0xc390  27  GetBoardID */
  *param_1 = *(undefined1 *)(DAT_180020530 + 0x1072);
  return;
}



/* ========================================================================
   ENTRY: 18000c3b0
   NAME : FUN_18000c3b0
   SIG  : undefined __fastcall FUN_18000c3b0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000c3b0(void)

{
  FILE *_File;
  ulonglong uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  GetLocalTime((LPSYSTEMTIME)&DAT_180020540);
  uVar3 = (ulonglong)DAT_180020548;
  uVar2 = (ulonglong)DAT_180020546;
  uVar1 = (ulonglong)DAT_180020542;
  FUN_180007660("(%02d/%02d %02d:%02d:%02d:%03d) ",uVar1,uVar2,uVar3);
  FUN_180007660("- StopReadThread()\n",uVar1,uVar2,uVar3);
  _File = (FILE *)__acrt_iob_func(1);
  fflush(_File);
  if (DAT_180020568 == 1) {
    *(undefined4 *)(DAT_180020530 + 0x40) = 0;
    FUN_18000d990();
  }
  else {
    SendStopToMetis();
  }
  closesocket(DAT_180020598);
  DAT_180020598 = 0xffffffffffffffff;
  if (DAT_18001e1dc != 0) {
    WSACleanup();
    DAT_18001e1dc = 0;
    _DAT_180020510 = 0;
  }
  return;
}



/* ========================================================================
   ENTRY: 18000c490
   NAME : FUN_18000c490
   SIG  : undefined __fastcall FUN_18000c490(undefined8 param_1, undefined4 param_2, int param_3, int param_4, undefined8 param_5)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_18000c490(undefined8 param_1,undefined4 param_2,int param_3,int param_4,undefined8 param_5)

{
  FILE *_File;
  undefined1 auStack_e8 [32];
  uint local_c8;
  uint local_c0;
  uint local_b8;
  uint local_b0;
  uint local_a8;
  undefined8 local_a0;
  undefined4 local_98;
  int local_90;
  int local_88;
  int local_80;
  undefined8 local_78;
  _SYSTEMTIME local_68;
  ulonglong local_58;
  
  local_58 = DAT_18001e000 ^ (ulonglong)auStack_e8;
  _File = fopen("net_seq_gaps.log","a");
  if (_File != (FILE *)0x0) {
    GetLocalTime(&local_68);
    local_90 = (param_4 - param_3) + -1;
    local_78 = param_5;
    local_a8 = (uint)local_68.wMilliseconds;
    local_b0 = (uint)local_68.wSecond;
    local_b8 = (uint)local_68.wMinute;
    local_c0 = (uint)local_68.wHour;
    local_c8 = (uint)local_68.wDay;
    local_a0 = param_1;
    local_98 = param_2;
    local_88 = param_3;
    local_80 = param_4;
    FUN_180006cb0(_File,"%04d-%02d-%02d %02d:%02d:%02d.%03d %s%d gap=%u last=%u this=%u %s\n",
                  (ulonglong)local_68.wYear,(ulonglong)local_68.wMonth);
    fclose(_File);
  }
  return;
}



/* ========================================================================
   ENTRY: 18000c5d0
   NAME : FUN_18000c5d0
   SIG  : uint __fastcall FUN_18000c5d0(undefined8 * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint FUN_18000c5d0(undefined8 *param_1)

{
  byte *pbVar1;
  uint uVar2;
  void *pvVar3;
  undefined8 *puVar4;
  int iVar5;
  double dVar6;
  u_short uVar7;
  int iVar8;
  int *piVar9;
  uint *puVar10;
  char *pcVar11;
  FILE *pFVar12;
  longlong lVar13;
  undefined8 *puVar14;
  int *piVar15;
  longlong lVar16;
  int iVar17;
  undefined8 uVar18;
  longlong lVar19;
  ulonglong uVar20;
  int iVar21;
  int iVar22;
  int iVar23;
  bool bVar24;
  undefined1 auStackY_658 [32];
  uint local_618;
  int local_610;
  uint local_60c;
  _SYSTEMTIME local_608;
  sockaddr local_5f8;
  char local_5e8 [4];
  undefined8 local_5e4;
  undefined8 uStack_5dc;
  undefined8 local_5d4;
  undefined8 uStack_5cc;
  undefined8 local_5c4;
  undefined8 uStack_5bc;
  undefined8 local_5b4;
  ulonglong local_38;
  
  local_38 = DAT_18001e000 ^ (ulonglong)auStackY_658;
  local_610 = 0x10;
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x52));
  uVar20 = 0;
  uVar18 = 0x5a4;
  iVar8 = recvfrom(DAT_180020598,local_5e8,0x5a4,0,&local_5f8,&local_610);
  if (iVar8 == -1) {
    piVar9 = _errno();
    iVar8 = WSAGetLastError();
    *piVar9 = iVar8;
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x52));
    piVar9 = _errno();
    if (*piVar9 == 0x2733) {
      return 0;
    }
    piVar9 = _errno();
    if (*piVar9 == 0x2738) {
      piVar9 = _errno();
      iVar8 = *piVar9;
      puVar10 = (uint *)_errno();
      uVar2 = *puVar10;
      pcVar11 = strerror(iVar8);
      FUN_180007660("Error code %d: recvfrom() : %s\n",(ulonglong)uVar2,pcVar11,uVar20);
      pFVar12 = (FILE *)__acrt_iob_func(1);
      fflush(pFVar12);
    }
    return 0xffffffff;
  }
  if (0 < iVar8) {
    LOCK();
    DAT_18001fbb0 = DAT_18001fbb0 + iVar8;
    UNLOCK();
  }
  local_618 = CONCAT31(CONCAT21(CONCAT11(local_5e8[0],local_5e8[1]),local_5e8[2]),local_5e8[3]);
  uVar7 = ntohs(local_5f8.sa_data._0_2_);
  piVar9 = DAT_180020530;
  dVar6 = DAT_180018d78;
  local_60c = (uint)uVar7;
  iVar23 = local_60c - *DAT_180020530;
  switch(iVar23) {
  case 0:
    if (iVar8 == 0x3c) {
      puVar10 = (uint *)(DAT_180020530 + 0x22);
      if ((local_618 != *puVar10 + 1) && (local_618 != 0)) {
        DAT_180020530[0x23] = DAT_180020530[0x23] + 1;
        FUN_180007660("- Rx High Priority C&C: seq error this: %i last: %i\n",(ulonglong)local_618,
                      (ulonglong)*puVar10,uVar20);
        pFVar12 = (FILE *)__acrt_iob_func(1);
        fflush(pFVar12);
      }
      piVar9 = DAT_180020530;
      DAT_180020530[0x22] = local_618;
      *param_1 = local_5e4;
      param_1[1] = uStack_5dc;
      param_1[2] = local_5d4;
      param_1[3] = uStack_5cc;
      param_1[4] = local_5c4;
      param_1[5] = uStack_5bc;
      param_1[6] = local_5b4;
    }
    break;
  case 1:
    if ((*(byte *)(DAT_180020530 + 0xc5) & 0x40) == 0) {
      bVar24 = iVar8 == 0x84;
    }
    else {
      bVar24 = iVar8 == 0xc4;
    }
    if (bVar24) {
      iVar8 = DAT_180020530[0x3ef];
      if ((local_618 != iVar8 + 1U) && (local_618 != 0)) {
        iVar23 = local_618 - iVar8;
        if (iVar23 - 2U < 8) {
          uVar20 = (ulonglong)local_618;
          if ((*(byte *)(DAT_180020530 + 0xc5) & 0x40) == 0) {
            FUN_18000c490(&DAT_180017b38,0,iVar8,local_618,"drop (legacy policy)");
          }
          else {
            FUN_18000c490(&DAT_180017b38,0,iVar8,local_618,&DAT_180017b34);
            while (iVar23 = iVar23 + -1, iVar23 != 0) {
              Inbound(*(uint *)(PTR_DAT_18001e080 + 4),DAT_180020530[0xc6],&DAT_180017ce0);
            }
          }
        }
        puVar10 = (uint *)(DAT_180020530 + 0x3ef);
        DAT_180020530[0x3f0] = DAT_180020530[0x3f0] + 1;
        FUN_180007660("- Mic samples: seq error this: %i last: %i\n",(ulonglong)local_618,
                      (ulonglong)*puVar10,uVar20);
        pFVar12 = (FILE *)__acrt_iob_func(1);
        fflush(pFVar12);
      }
      piVar9 = DAT_180020530;
      pbVar1 = (byte *)(DAT_180020530 + 0xc5);
      DAT_180020530[0x3ef] = local_618;
      memcpy(param_1,&local_5e4,(ulonglong)*pbVar1 & 0xffffffffffffffc0 | 0x80);
    }
    break;
  case 2:
  case 3:
  case 4:
  case 5:
  case 6:
  case 7:
  case 8:
  case 9:
    if (iVar8 == 0x404) {
      iVar8 = DAT_180020530[0x8a];
      iVar17 = DAT_180020530[0x8d];
      iVar22 = DAT_180020530[0x89] + -2 + iVar23;
      iVar21 = 4;
      local_608._0_8_ = SEXT48(iVar23);
      iVar23 = 0;
      lVar13 = local_608._0_8_ - 2;
      pvVar3 = *(void **)(DAT_180020530 + lVar13 * 0xe + 0x9c);
      if (3 < iVar8) {
        do {
          lVar16 = (longlong)iVar21;
          iVar21 = iVar21 + 8;
          lVar19 = (longlong)iVar23;
          iVar23 = iVar23 + 4;
          *(double *)((longlong)pvVar3 + lVar19 * 8) =
               (double)(int)((uint)CONCAT11(local_5e8[lVar16],local_5e8[lVar16 + 1]) << 0x10) *
               dVar6;
          *(double *)((longlong)pvVar3 + lVar19 * 8 + 8) =
               (double)(int)((uint)CONCAT11(local_5e8[lVar16 + 2],local_5e8[lVar16 + 3]) << 0x10) *
               dVar6;
          *(double *)((longlong)pvVar3 + lVar19 * 8 + 0x10) =
               (double)(int)((uint)CONCAT11(*(undefined1 *)((longlong)&local_5e4 + lVar16),
                                            *(undefined1 *)((longlong)&local_5e4 + lVar16 + 1)) <<
                            0x10) * dVar6;
          *(double *)((longlong)pvVar3 + lVar19 * 8 + 0x18) =
               (double)(int)((uint)CONCAT11(*(undefined1 *)((longlong)&local_5e4 + lVar16 + 2),
                                            *(undefined1 *)((longlong)&local_5e4 + lVar16 + 3)) <<
                            0x10) * dVar6;
        } while (iVar23 < iVar8 + -3);
      }
      for (; iVar23 < iVar8; iVar23 = iVar23 + 1) {
        *(double *)((longlong)pvVar3 + (longlong)iVar23 * 8) =
             (double)(int)((uint)CONCAT11(local_5e8[iVar21],local_5e8[(longlong)iVar21 + 1]) << 0x10
                          ) * dVar6;
        iVar21 = iVar21 + 2;
      }
      lVar16 = local_608._0_8_ + 9;
      if (piVar9[lVar16 * 0xe] == 0) {
        piVar9[lVar13 * 0xe + 0x99] = 0;
        if (local_618 == 0) {
          piVar9[lVar16 * 0xe] = 1;
          Spectrum(iVar22,0,0,pvVar3);
          piVar9 = DAT_180020530;
          DAT_180020530[lVar13 * 0xe + 0x99] = DAT_180020530[lVar13 * 0xe + 0x99] + 1;
        }
      }
      else if (piVar9[lVar16 * 0xe] == 1) {
        if (local_618 == piVar9[lVar13 * 0xe + 0x99]) {
          Spectrum(iVar22,0,0,pvVar3);
          piVar9 = DAT_180020530;
          if (DAT_180020530[lVar13 * 0xe + 0x99] == iVar17 + -1) {
            DAT_180020530[lVar16 * 0xe] = 0;
            piVar9[lVar13 * 0xe + 0x99] = piVar9[lVar13 * 0xe + 0x99] + 1;
            break;
          }
        }
        else {
          memset(pvVar3,0,(longlong)iVar8 << 3);
          iVar8 = piVar9[lVar13 * 0xe + 0x99];
          for (; iVar8 < iVar17; iVar8 = iVar8 + 1) {
            Spectrum(iVar22,0,0,pvVar3);
            piVar9 = DAT_180020530;
          }
          piVar9[lVar16 * 0xe] = 0;
        }
        piVar9[lVar13 * 0xe + 0x99] = piVar9[lVar13 * 0xe + 0x99] + 1;
      }
    }
    break;
  case 10:
  case 0xb:
  case 0xc:
  case 0xd:
  case 0xe:
  case 0xf:
  case 0x10:
    if (iVar8 == 0x5a4) {
      if (local_618 != 0) {
        lVar13 = (longlong)iVar23 + -10;
        EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x66));
        piVar9 = DAT_180020530;
        iVar8 = DAT_180020530[lVar13 * 0x42 + 0x100];
        DAT_180020530[lVar13 * 0x42 + (longlong)iVar8 + 0xd8] =
             (local_618 - DAT_180020530[lVar13 * 0x42 + 0xd0]) + -1;
        iVar17 = 0;
        if (iVar8 != 0x27) {
          iVar17 = iVar8 + 1;
        }
        piVar9[lVar13 * 0x42 + 0x100] = iVar17;
        LeaveCriticalSection((LPCRITICAL_SECTION)(piVar9 + 0x66));
      }
      iVar8 = DAT_180020530[(longlong)iVar23 * 0x42 + -0x1c4];
      if ((local_618 != iVar8 + 1U) && (local_618 != 0)) {
        iVar17 = local_618 - iVar8;
        if (iVar17 - 2U < 8) {
          if ((*(byte *)(DAT_180020530 + 0xc5) & 0x40) == 0) {
            FUN_18000c490(&DAT_180017b30,iVar23 + -10,iVar8,local_618,"drop (legacy policy)");
          }
          else {
            FUN_18000c490(&DAT_180017b30,iVar23 + -10,iVar8,local_618,&DAT_180017b34);
            while (iVar17 = iVar17 + -1, iVar17 != 0) {
              xrouter(0,0,iVar23 + -10,DAT_180020530[0xd7],&DAT_180017ce0);
            }
          }
        }
        lVar13 = (longlong)iVar23 + -10;
        puVar10 = (uint *)(DAT_180020530 + lVar13 * 0x42 + 0xd0);
        DAT_180020530[lVar13 * 0x42 + 0xd1] = DAT_180020530[lVar13 * 0x42 + 0xd1] + 1;
        FUN_180007660("- Rx%d I/Q: seq error this: %d last: %d\n",(ulonglong)(iVar23 - 10),
                      (ulonglong)local_618,(ulonglong)*puVar10);
        pFVar12 = (FILE *)__acrt_iob_func(1);
        fflush(pFVar12);
        iVar8 = DAT_180020530[lVar13 * 0x42 + 0xd0];
        EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x66));
        puVar14 = malloc(0xd0);
        piVar9 = DAT_180020530;
        if (puVar14 != (undefined8 *)0x0) {
          uVar18 = *(undefined8 *)(DAT_180020530 + lVar13 * 0x42 + 0xd8 + 2);
          puVar14[2] = *(undefined8 *)(DAT_180020530 + lVar13 * 0x42 + 0xd8);
          puVar14[3] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xdc + 2);
          puVar14[4] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xdc);
          puVar14[5] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe0 + 2);
          puVar14[6] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe0);
          puVar14[7] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe4 + 2);
          puVar14[8] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe4);
          puVar14[9] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe8 + 2);
          puVar14[10] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xe8);
          puVar14[0xb] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xec + 2);
          puVar14[0xc] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xec);
          puVar14[0xd] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf0 + 2);
          puVar14[0xe] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf0);
          puVar14[0xf] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf4 + 2);
          puVar14[0x10] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf4);
          puVar14[0x11] = uVar18;
          uVar18 = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf8 + 2);
          puVar14[0x12] = *(undefined8 *)(piVar9 + lVar13 * 0x42 + 0xf8);
          puVar14[0x13] = uVar18;
          piVar9 = piVar9 + lVar13 * 0x42 + 0xfc;
          iVar17 = *piVar9;
          iVar21 = piVar9[1];
          iVar22 = piVar9[2];
          iVar5 = piVar9[3];
          puVar14[1] = 0;
          *(int *)(puVar14 + 0x14) = iVar17;
          *(int *)((longlong)puVar14 + 0xa4) = iVar21;
          *(int *)(puVar14 + 0x15) = iVar22;
          *(int *)((longlong)puVar14 + 0xac) = iVar5;
          GetLocalTime(&local_608);
          FUN_18000bed0(puVar14 + 0x16,"%02d/%02d %02d:%02d:%02d:%03d",
                        (ulonglong)local_608._0_8_ >> 0x10 & 0xffff,
                        (ulonglong)local_608._0_8_ >> 0x30);
          piVar15 = DAT_180020530;
          *(uint *)(puVar14 + 0x19) = local_618;
          *(int *)((longlong)puVar14 + 0xcc) = iVar8;
          if (piVar15[lVar13 * 0x42 + 0x106] == 0x14) {
            pvVar3 = *(void **)(piVar15 + lVar13 * 0x42 + 0x104);
            puVar4 = *(undefined8 **)((longlong)pvVar3 + 8);
            *(undefined8 **)(piVar15 + lVar13 * 0x42 + 0x104) = puVar4;
            *puVar4 = 0;
            piVar9 = DAT_180020530;
            piVar15[lVar13 * 0x42 + 0x106] = piVar15[lVar13 * 0x42 + 0x106] + -1;
            if (*(void **)(piVar9 + ((longlong)iVar23 + -6) * 0x42) == pvVar3) {
              *(undefined8 *)(piVar9 + ((longlong)iVar23 + -6) * 0x42) =
                   *(undefined8 *)(piVar15 + lVar13 * 0x42 + 0x104);
            }
            free(pvVar3);
            piVar15 = DAT_180020530;
          }
          if (*(longlong *)(piVar15 + lVar13 * 0x42 + 0x102) == 0) {
            *puVar14 = 0;
            *(undefined8 **)(piVar15 + lVar13 * 0x42 + 0x104) = puVar14;
            piVar9 = piVar15;
          }
          else {
            *(undefined8 **)(*(longlong *)(piVar15 + lVar13 * 0x42 + 0x102) + 8) = puVar14;
            piVar9 = DAT_180020530;
            *puVar14 = *(undefined8 *)(piVar15 + lVar13 * 0x42 + 0x102);
          }
          *(undefined8 **)(piVar15 + lVar13 * 0x42 + 0x102) = puVar14;
          piVar15[lVar13 * 0x42 + 0x106] = piVar15[lVar13 * 0x42 + 0x106] + 1;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)(piVar9 + 0x66));
      }
      piVar9 = DAT_180020530;
      DAT_180020530[((longlong)iVar23 + -10) * 0x42 + 0xd0] = local_618;
      memcpy(param_1,(void *)((longlong)&uStack_5dc + 4),0x594);
    }
    break;
  default:
    FUN_180007660("Rcvd data on Port %d\n",(ulonglong)uVar7,uVar18,uVar20);
    piVar9 = DAT_180020530;
  }
  LeaveCriticalSection((LPCRITICAL_SECTION)(piVar9 + 0x52));
  return local_60c;
}



/* ========================================================================
   ENTRY: 18000cf90
   NAME : FUN_18000cf90
   SIG  : undefined __fastcall FUN_18000cf90(void)
   ======================================================================== */

void FUN_18000cf90(void)

{
  byte bVar1;
  byte *pbVar2;
  double dVar3;
  float fVar4;
  undefined *puVar5;
  int iVar6;
  uint uVar7;
  undefined8 uVar8;
  FILE *_File;
  undefined2 uVar9;
  ulonglong uVar10;
  longlong lVar11;
  int iVar12;
  int iVar13;
  int iVar14;
  int iVar15;
  ulonglong uVar16;
  int *piVar17;
  ulonglong uVar18;
  longlong lVar19;
  longlong lVar20;
  int *piVar21;
  float fVar22;
  float fVar23;
  undefined8 in_stack_ffffffffffffffa8;
  void *pvVar24;
  
  uVar7 = (uint)((ulonglong)in_stack_ffffffffffffffa8 >> 0x20);
  uVar8 = WSACreateEvent();
  *(undefined8 *)(DAT_180020530 + 0x7a) = uVar8;
  WSAEventSelect(DAT_180020598,uVar8,1);
  GetLocalTime((LPSYSTEMTIME)&DAT_180020540);
  uVar18 = (ulonglong)DAT_180020548;
  uVar16 = (ulonglong)DAT_180020546;
  uVar10 = (ulonglong)DAT_180020542;
  pvVar24 = (void *)((ulonglong)uVar7 << 0x20);
  FUN_180007660("(%02d/%02d %02d:%02d:%02d:%03d) ",uVar10,uVar16,uVar18);
  FUN_180007660("- ReadThreadMainLoop()\n",uVar10,uVar16,uVar18);
  _File = (FILE *)__acrt_iob_func(1);
  fflush(_File);
  fVar4 = DAT_180018d80;
  dVar3 = DAT_180018d78;
  piVar21 = DAT_180020530;
  do {
    do {
      while( true ) {
        if (DAT_180020514 == 0) {
          return;
        }
        pvVar24 = (void *)((ulonglong)pvVar24 & 0xffffffff00000000);
        uVar8 = 0xffffffff;
        if (piVar21[0x11] != 0) {
          uVar8 = 3000;
        }
        iVar6 = WSAWaitForMultipleEvents(1,piVar21 + 0x7a,0,uVar8,pvVar24);
        if ((iVar6 != -1) && (iVar6 != 0x102)) break;
        DAT_1800205c8 = 0;
        DAT_180020530[0x10] = 0;
        FUN_18000d990();
        piVar21 = DAT_180020530;
        memset(*(void **)(DAT_180020530 + 4),0,(longlong)DAT_180020530[0xd7]);
        memset(*(void **)(piVar21 + 6),0,(longlong)piVar21[0xc6]);
        Inbound(0,piVar21[0xd7],*(void **)(piVar21 + 4));
        Inbound(1,DAT_180020530[0x119],*(void **)(DAT_180020530 + 4));
        Inbound(*(uint *)(PTR_DAT_18001e080 + 4),DAT_180020530[0xc6],*(void **)(DAT_180020530 + 6));
        piVar21 = DAT_180020530;
      }
      piVar17 = DAT_180020530 + 0x7c;
      WSAEnumNetworkEvents(DAT_180020598,*(undefined8 *)(DAT_180020530 + 0x7a));
      piVar21 = DAT_180020530;
    } while ((*(byte *)(DAT_180020530 + 0x7c) & 1) == 0);
    if (DAT_180020530[0x7d] != 0) {
      FUN_180007660("FD_READ failed with error %d\n",(ulonglong)(uint)DAT_180020530[0x7d],piVar17,
                    uVar8);
      return;
    }
    iVar6 = 0;
    uVar7 = FUN_18000c5d0(*(undefined8 **)(DAT_180020530 + 8));
    lVar11 = DAT_1800202e0;
    piVar21 = DAT_180020530;
    while (DAT_1800202e0 = lVar11, DAT_180020530 = piVar21, 0 < (int)uVar7) {
      iVar12 = *piVar21;
      switch(uVar7 - iVar12) {
      case 0:
        pbVar2 = *(byte **)(piVar21 + 8);
        piVar21[0x15] = *pbVar2 & 1;
        piVar21[0x16] = *pbVar2 & 2;
        piVar21[0x17] = *pbVar2 & 4;
        bVar1 = *pbVar2;
        EnterCriticalSection((LPCRITICAL_SECTION)(lVar11 + 0x118));
        if (*(int *)(lVar11 + 0x110) == 1) {
          *(uint *)(lVar11 + 0x104) = (uint)(bVar1 >> 7);
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)(lVar11 + 0x118));
        piVar21 = DAT_180020530;
        DAT_180020530[0x18] = **(byte **)(DAT_180020530 + 8) & 0x10;
        piVar21[0x95] = piVar21[0x96];
        if ((piVar21[0x96] != 0) ||
           (iVar12 = 0, (*(byte *)(*(longlong *)(piVar21 + 8) + 1) & 1) != 0)) {
          iVar12 = 1;
        }
        piVar21[0x96] = iVar12;
        piVar21[0xa3] = piVar21[0xa4];
        if ((piVar21[0xa4] != 0) ||
           (iVar12 = 0, (*(byte *)(*(longlong *)(piVar21 + 8) + 1) & 2) != 0)) {
          iVar12 = 1;
        }
        piVar21[0xa4] = iVar12;
        piVar21[0xb1] = piVar21[0xb2];
        if ((piVar21[0xb2] != 0) ||
           (iVar12 = 0, (*(byte *)(*(longlong *)(piVar21 + 8) + 1) & 4) != 0)) {
          iVar12 = 1;
        }
        piVar21[0xb2] = iVar12;
        piVar21[1000] =
             (uint)CONCAT11(*(undefined1 *)(*(longlong *)(piVar21 + 8) + 2),
                            *(undefined1 *)(*(longlong *)(piVar21 + 8) + 3));
        uVar7 = (uint)CONCAT11(*(undefined1 *)(*(longlong *)(piVar21 + 8) + 10),
                               *(undefined1 *)(*(longlong *)(piVar21 + 8) + 0xb));
        piVar21[0x3e9] = uVar7;
        fVar22 = (float)uVar7;
        uVar7 = (uint)CONCAT11(*(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x12),
                               *(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x13));
        piVar21[0x3ea] = uVar7;
        if (fVar22 <= DAT_1800205a8) {
          fVar22 = DAT_1800205a8 * fVar4;
        }
        fVar23 = (float)uVar7;
        if (fVar23 <= DAT_1800205c4) {
          fVar23 = DAT_1800205c4 * fVar4;
        }
        DAT_1800205a8 = fVar22;
        DAT_1800205c4 = fVar23;
        *(ushort *)(piVar21 + 0x9e) =
             CONCAT11(*(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x23),
                      *(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x24));
        uVar9 = CONCAT11(*(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x25),
                         *(undefined1 *)(*(longlong *)(piVar21 + 8) + 0x26));
        *(undefined2 *)(piVar21 + 0xac) = uVar9;
        *(undefined2 *)(piVar21 + 0xba) = 0;
        if ((piVar21[0x95] == 0) && (piVar21[0x96] != 0)) {
          *(short *)((longlong)piVar21 + 0x27a) = (short)piVar21[0x9e];
        }
        if ((piVar21[0xa3] == 0) && (piVar21[0xa4] != 0)) {
          *(undefined2 *)((longlong)piVar21 + 0x2b2) = uVar9;
        }
        *(undefined2 *)((longlong)piVar21 + 0x2ea) = 0;
        lVar11 = *(longlong *)(piVar21 + 8);
        piVar21[0x1b] =
             (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x2d),*(undefined1 *)(lVar11 + 0x2e));
        piVar21[0x1f] =
             (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x2f),*(undefined1 *)(lVar11 + 0x30));
        piVar21[0x1e] =
             (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x31),*(undefined1 *)(lVar11 + 0x32));
        piVar21[0x1d] =
             (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x33),*(undefined1 *)(lVar11 + 0x34));
        puVar5 = PTR_DAT_18001e080;
        uVar7 = (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x35),*(undefined1 *)(lVar11 + 0x36));
        piVar21[0x1c] = uVar7;
        lVar11 = *(longlong *)(puVar5 + 0x1120);
        EnterCriticalSection((LPCRITICAL_SECTION)(lVar11 + 0x68));
        *(uint *)(lVar11 + 0x30) = uVar7 - 0x14;
        LeaveCriticalSection((LPCRITICAL_SECTION)(lVar11 + 0x68));
        piVar21 = DAT_180020530;
        lVar11 = *(longlong *)(DAT_180020530 + 8);
        DAT_180020530[0x20] = (uint)*(byte *)(lVar11 + 0x37);
        piVar21[0x87] =
             (uint)CONCAT11(*(undefined1 *)(lVar11 + 0x1a),*(undefined1 *)(lVar11 + 0x1b));
        break;
      case 1:
        if ((*(byte *)(piVar21 + 0xc5) & 0x40) == 0) {
          iVar12 = 0;
          iVar14 = 0;
          if (0 < piVar21[0xc6]) {
            do {
              lVar19 = (longlong)(iVar12 * 2);
              iVar12 = iVar12 + 1;
              lVar11 = (longlong)iVar14;
              iVar14 = iVar14 + 2;
              *(double *)(*(longlong *)(piVar21 + 6) + lVar19 * 8) =
                   (double)(int)((uint)CONCAT11(*(undefined1 *)(lVar11 + *(longlong *)(piVar21 + 8))
                                                ,*(undefined1 *)
                                                  (lVar11 + 1 + *(longlong *)(piVar21 + 8))) << 0x10
                                ) * dVar3;
              *(undefined8 *)(*(longlong *)(piVar21 + 6) + 8 + lVar19 * 8) =
                   *(undefined8 *)(*(longlong *)(piVar21 + 6) + lVar19 * 8);
            } while (iVar12 < piVar21[0xc6]);
          }
        }
        else {
          iVar12 = 0;
          iVar14 = 0;
          if (0 < piVar21[0xc6]) {
            do {
              lVar11 = *(longlong *)(piVar21 + 8);
              lVar20 = (longlong)(iVar12 * 2);
              iVar12 = iVar12 + 1;
              lVar19 = (longlong)iVar14;
              iVar14 = iVar14 + 3;
              *(double *)(*(longlong *)(piVar21 + 6) + lVar20 * 8) =
                   (double)(int)((uint)CONCAT21(CONCAT11(*(undefined1 *)(lVar19 + lVar11),
                                                         *(undefined1 *)(lVar19 + 1 + lVar11)),
                                                *(undefined1 *)(lVar19 + 2 + lVar11)) << 8) * dVar3;
              *(undefined8 *)(*(longlong *)(piVar21 + 6) + 8 + lVar20 * 8) =
                   *(undefined8 *)(*(longlong *)(piVar21 + 6) + lVar20 * 8);
            } while (iVar12 < piVar21[0xc6]);
          }
        }
        Inbound(*(uint *)(PTR_DAT_18001e080 + 4),piVar21[0xc6],*(void **)(piVar21 + 6));
        piVar21 = DAT_180020530;
        break;
      case 10:
      case 0xb:
      case 0xc:
      case 0xd:
      case 0xe:
      case 0xf:
      case 0x10:
        iVar14 = piVar21[0xd7];
        iVar15 = 0;
        iVar13 = 0;
        if (0 < iVar14) {
          do {
            lVar11 = *(longlong *)(piVar21 + 8);
            iVar14 = iVar15 * 2;
            lVar19 = (longlong)iVar13;
            iVar15 = iVar15 + 1;
            iVar13 = iVar13 + 6;
            *(double *)(*(longlong *)(piVar21 + 4) + (longlong)iVar14 * 8) =
                 (double)(int)((uint)CONCAT21(CONCAT11(*(undefined1 *)(lVar19 + lVar11),
                                                       *(undefined1 *)(lVar19 + 1 + lVar11)),
                                              *(undefined1 *)(lVar19 + 2 + lVar11)) << 8) * dVar3;
            lVar11 = *(longlong *)(piVar21 + 8);
            *(double *)(*(longlong *)(piVar21 + 4) + 8 + (longlong)iVar14 * 8) =
                 (double)(int)((uint)CONCAT21(CONCAT11(*(undefined1 *)(lVar19 + 3 + lVar11),
                                                       *(undefined1 *)(lVar19 + 4 + lVar11)),
                                              *(undefined1 *)(lVar19 + 5 + lVar11)) << 8) * dVar3;
            iVar14 = piVar21[0xd7];
          } while (iVar15 < iVar14);
        }
        pvVar24 = *(void **)(piVar21 + 4);
        xrouter(0,0,(uVar7 - iVar12) + -10,iVar14,pvVar24);
        piVar21 = DAT_180020530;
      }
      iVar6 = iVar6 + 1;
      if (0x1ff < iVar6) break;
      uVar7 = FUN_18000c5d0(*(undefined8 **)(piVar21 + 8));
      lVar11 = DAT_1800202e0;
      piVar21 = DAT_180020530;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 18000d740
   NAME : FUN_18000d740
   SIG  : undefined __fastcall FUN_18000d740(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_18000d740(void)

{
  uint uVar1;
  int *piVar2;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  bool bVar8;
  undefined1 auStackY_98 [32];
  sockaddr local_68;
  char local_58 [6];
  undefined1 local_52;
  undefined1 local_51;
  undefined1 local_50;
  undefined1 local_4f;
  undefined1 local_4e;
  undefined1 local_4d;
  undefined1 local_4c;
  undefined1 local_4b;
  undefined1 local_4a;
  undefined1 local_49;
  undefined1 local_48;
  undefined1 local_47;
  undefined1 local_46;
  undefined1 local_45;
  undefined1 local_44;
  undefined1 local_43;
  undefined1 local_42;
  undefined8 local_41;
  int6 iStack_39;
  undefined1 uStack_33;
  undefined1 uStack_32;
  undefined8 local_31;
  undefined8 uStack_29;
  int local_21;
  byte local_1d;
  ulonglong local_18;
  
  piVar2 = DAT_180020530;
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
  local_58[0] = '\0';
  local_58[1] = '\0';
  local_58[2] = '\0';
  local_58[3] = '\0';
  iVar4 = *DAT_180020530;
  local_58[4] = 0;
  local_58[5] = (char)((uint)iVar4 >> 8);
  local_4a = (undefined1)(iVar4 + 3);
  local_4b = (undefined1)((uint)(iVar4 + 3) >> 8);
  local_48 = (undefined1)(iVar4 + 4);
  local_52 = (undefined1)iVar4;
  local_49 = (undefined1)((uint)(iVar4 + 4) >> 8);
  local_47 = (undefined1)((uint)(iVar4 + 10) >> 8);
  local_31 = 0;
  uStack_29 = 0;
  local_51 = (undefined1)((uint)(iVar4 + 1) >> 8);
  local_50 = (undefined1)(iVar4 + 1);
  local_4f = (undefined1)((uint)(iVar4 + 2) >> 8);
  local_4e = (undefined1)(iVar4 + 2);
  local_46 = (undefined1)(iVar4 + 10);
  uVar5 = DAT_180020530[0x8e];
  do {
    LOCK();
    uVar1 = piVar2[0x8e];
    bVar8 = uVar5 == uVar1;
    if (bVar8) {
      piVar2[0x8e] = uVar5 & 0xff;
      uVar1 = uVar5;
    }
    uVar5 = uVar1;
    s = DAT_180020598;
    UNLOCK();
  } while (!bVar8);
  local_41._0_3_ =
       CONCAT12((char)DAT_180020530[0x8a],
                CONCAT11((char)((uint)DAT_180020530[0x8a] >> 8),(char)uVar5));
  local_41 = (ulonglong)
             CONCAT33(CONCAT12((char)DAT_180020530[0x8d],
                               CONCAT11((char)DAT_180020530[0x8c],(char)DAT_180020530[0x8b])),
                      (undefined3)local_41);
  _iStack_39 = CONCAT17((char)DAT_180020530[0x11],
                        CONCAT16(8,(uint6)CONCAT13((char)DAT_180020530[0x3ed],
                                                   CONCAT12((char)((uint)DAT_180020530[0x3ed] >> 8),
                                                            CONCAT11((char)DAT_180020530[0x3ec],
                                                                     (char)((uint)DAT_180020530
                                                                                  [0x3ec] >> 8))))
                                   << 0x10));
  local_21 = (uint)(DAT_180020530[0x3ee] == 0) << 0x18;
  local_1d = *DAT_1800205d0 | *DAT_1800205b0;
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    iVar4 = DAT_180020530[1];
    local_68.sa_family = 0;
    local_68.sa_data[0] = '\0';
    local_68.sa_data[1] = '\0';
    local_68.sa_data[2] = '\0';
    local_68.sa_data[3] = '\0';
    local_68.sa_data[4] = '\0';
    local_68.sa_data[5] = '\0';
    local_68.sa_data[6] = '\0';
    local_68.sa_data[7] = '\0';
    local_68.sa_data[8] = '\0';
    local_68.sa_data[9] = '\0';
    local_68.sa_data[10] = '\0';
    local_68.sa_data[0xb] = '\0';
    local_68.sa_data[0xc] = '\0';
    local_68.sa_data[0xd] = '\0';
    local_4d = local_58[5];
    local_4c = local_52;
    local_45 = local_51;
    local_44 = local_50;
    local_43 = local_4f;
    local_42 = local_4e;
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
    uVar3 = htons((u_short)iVar4);
    uVar7 = 0;
    uVar6 = 0x3c;
    local_68.sa_data[2] = '\0';
    local_68.sa_data[3] = '\0';
    local_68.sa_data[4] = '\0';
    local_68.sa_data[5] = '\0';
    local_68.sa_data[0] = (char)uVar3;
    local_68.sa_data[1] = (char)(uVar3 >> 8);
    local_68.sa_family = 2;
    local_68.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_68.sa_data[3] = DAT_18001e1d8._1_1_;
    local_68.sa_data[4] = DAT_18001e1d8._2_1_;
    local_68.sa_data[5] = DAT_18001e1d8._3_1_;
    iVar4 = sendto(s,local_58,0x3c,0,&local_68,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x5c));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000d990
   NAME : FUN_18000d990
   SIG  : undefined __fastcall FUN_18000d990(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_18000d990(void)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  undefined1 auStackY_608 [32];
  sockaddr local_5d8;
  char local_5c8 [4];
  byte local_5c4;
  byte local_5c3;
  undefined1 local_5bf;
  undefined1 local_5be;
  undefined1 local_5bd;
  undefined1 local_5bc;
  undefined1 local_5bb;
  undefined1 local_5ba;
  undefined1 local_5b9;
  undefined1 local_5b8;
  undefined1 local_5b7;
  undefined1 local_5b6;
  undefined1 local_5b5;
  undefined1 local_5b4;
  undefined1 local_5b3;
  undefined1 local_5b2;
  undefined1 local_5b1;
  undefined1 local_5b0;
  undefined1 local_5af;
  undefined1 local_5ae;
  undefined1 local_5ad;
  undefined1 local_5ac;
  undefined1 local_5ab;
  undefined1 local_5aa;
  undefined1 local_5a9;
  undefined1 local_5a8;
  undefined1 local_5a7;
  undefined1 local_5a6;
  undefined1 local_5a5;
  undefined1 local_5a4;
  undefined1 local_5a3;
  undefined1 local_5a2;
  undefined1 local_5a1;
  undefined1 local_5a0;
  undefined1 local_59f;
  undefined1 local_59e;
  undefined1 local_59d;
  undefined1 local_59c;
  undefined1 local_59b;
  undefined1 local_59a;
  undefined1 local_599;
  undefined1 local_598;
  undefined1 local_597;
  undefined1 local_596;
  undefined1 local_595;
  undefined1 local_594;
  undefined1 local_593;
  undefined1 local_592;
  undefined1 local_591;
  undefined1 local_590;
  undefined1 local_47f;
  undefined1 local_47e;
  undefined1 local_47d;
  undefined1 local_47c;
  undefined1 local_46f;
  byte local_53;
  undefined1 local_52;
  undefined1 local_51;
  byte local_50;
  char local_4f;
  byte local_4e;
  byte local_4d;
  undefined1 local_34;
  undefined1 local_33;
  undefined1 local_32;
  undefined1 local_31;
  undefined1 local_30;
  undefined1 local_2f;
  undefined1 local_2e;
  undefined1 local_2d;
  undefined1 local_26;
  undefined1 local_25;
  ulonglong local_18;
  
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(local_5c8,0,0x5a4);
  s = DAT_180020598;
  iVar3 = *(int *)(DAT_180020530 + 0xf98);
  local_5c4 = *(char *)(DAT_180020530 + 0xf98) * '\x02' | *(byte *)(DAT_180020530 + 0x40);
  local_5c3 = ((*(char *)(DAT_180020530 + 0xf90) * '\x02' | *(byte *)(DAT_180020530 + 0xf94)) *
               '\x02' | *(byte *)(DAT_180020530 + 0xf8c)) & 7;
  if ((iVar3 == 0) || (*(int *)(DAT_180020530 + 0x220) == 0)) {
    local_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x18);
    if (iVar3 != 0) goto LAB_18000da34;
LAB_18000da4c:
    local_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 0x10);
    if (iVar3 != 0) goto LAB_18000da5d;
LAB_18000da75:
    local_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x328) >> 8);
    if (iVar3 != 0) goto LAB_18000da86;
LAB_18000da9c:
    local_5bc = *(undefined1 *)(DAT_180020530 + 0x328);
    if (iVar3 != 0) goto LAB_18000daab;
LAB_18000dac3:
    local_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x18);
    if (iVar3 != 0) goto LAB_18000dad4;
LAB_18000daec:
    local_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 0x10);
    if (iVar3 != 0) goto LAB_18000dafd;
LAB_18000db15:
    local_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x430) >> 8);
    if (iVar3 != 0) goto LAB_18000db26;
  }
  else {
    local_5bf = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000da34:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da4c;
    local_5be = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000da5d:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da75;
    local_5bd = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000da86:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000da9c;
    local_5bc = *(undefined1 *)(DAT_180020530 + 0xf84);
LAB_18000daab:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000dac3;
    local_5bb = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x18);
LAB_18000dad4:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000daec;
    local_5ba = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 0x10);
LAB_18000dafd:
    if (*(int *)(DAT_180020530 + 0x220) == 0) goto LAB_18000db15;
    local_5b9 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0xf84) >> 8);
LAB_18000db26:
    if (*(int *)(DAT_180020530 + 0x220) != 0) {
      local_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0xf84);
      goto LAB_18000db3d;
    }
  }
  local_5b8 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x430);
LAB_18000db3d:
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x538);
  local_5b7 = (undefined1)((uint)uVar1 >> 0x18);
  local_5b6 = (undefined1)((uint)uVar1 >> 0x10);
  local_5b5 = (undefined1)((uint)uVar1 >> 8);
  local_5b4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x640);
  local_5b3 = (undefined1)((uint)uVar1 >> 0x18);
  local_5b2 = (undefined1)((uint)uVar1 >> 0x10);
  local_5b1 = (undefined1)((uint)uVar1 >> 8);
  local_5b0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x748);
  local_5af = (undefined1)((uint)uVar1 >> 0x18);
  local_5ae = (undefined1)((uint)uVar1 >> 0x10);
  local_5ad = (undefined1)((uint)uVar1 >> 8);
  local_5ac = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x850);
  local_5ab = (undefined1)((uint)uVar1 >> 0x18);
  local_5aa = (undefined1)((uint)uVar1 >> 0x10);
  local_5a9 = (undefined1)((uint)uVar1 >> 8);
  local_5a8 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0x958);
  local_5a7 = (undefined1)((uint)uVar1 >> 0x18);
  local_5a6 = (undefined1)((uint)uVar1 >> 0x10);
  local_5a5 = (undefined1)((uint)uVar1 >> 8);
  local_5a4 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xa60);
  local_5a3 = (undefined1)((uint)uVar1 >> 0x18);
  local_5a2 = (undefined1)((uint)uVar1 >> 0x10);
  local_5a1 = (undefined1)((uint)uVar1 >> 8);
  local_5a0 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xb68);
  local_59f = (undefined1)((uint)uVar1 >> 0x18);
  local_59e = (undefined1)((uint)uVar1 >> 0x10);
  local_59d = (undefined1)((uint)uVar1 >> 8);
  local_59c = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xc70);
  local_59b = (undefined1)((uint)uVar1 >> 0x18);
  local_59a = (undefined1)((uint)uVar1 >> 0x10);
  local_599 = (undefined1)((uint)uVar1 >> 8);
  local_598 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xd78);
  local_597 = (undefined1)((uint)uVar1 >> 0x18);
  local_596 = (undefined1)((uint)uVar1 >> 0x10);
  local_595 = (undefined1)((uint)uVar1 >> 8);
  local_594 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xe80);
  local_593 = (undefined1)((uint)uVar1 >> 0x18);
  local_592 = (undefined1)((uint)uVar1 >> 0x10);
  local_591 = (undefined1)((uint)uVar1 >> 8);
  local_590 = (undefined1)uVar1;
  uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
  local_47f = (undefined1)((uint)uVar1 >> 0x18);
  local_47e = (undefined1)((uint)uVar1 >> 0x10);
  local_47d = (undefined1)((uint)uVar1 >> 8);
  local_46f = *(undefined1 *)(DAT_180020530 + 0xf9c);
  local_47c = (undefined1)uVar1;
  if (DAT_180020584 == 0xd) {
    local_53 = *(byte *)(DAT_180020530 + 0x68) & 0xf;
  }
  local_52 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x240) >> 8);
  local_51 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x240);
  bVar5 = 0;
  if (DAT_18002056c == 0) {
    bVar5 = 2;
  }
  local_50 = bVar5 | DAT_1800205cc << 2 | DAT_180020578;
  local_4f = *(char *)(DAT_180020530 + 100) * '\x02';
  local_4e = *(byte *)(DAT_180020530 + 0x84) & 0xf;
  local_4d = *(char *)(DAT_180020530 + 0x444) * '\x02' | *(byte *)(DAT_180020530 + 0x33c);
  local_34 = *(undefined1 *)(DAT_1800205d0 + 7);
  local_33 = *(undefined1 *)(DAT_1800205d0 + 6);
  local_32 = *(undefined1 *)(DAT_1800205d0 + 5);
  local_31 = *(undefined1 *)(DAT_1800205d0 + 4);
  local_30 = *(undefined1 *)(DAT_1800205b0 + 7);
  local_2f = *(undefined1 *)(DAT_1800205b0 + 6);
  local_2e = *(undefined1 *)(DAT_1800205b0 + 5);
  local_2d = *(undefined1 *)(DAT_1800205b0 + 4);
  local_26 = *(undefined1 *)(DAT_180020530 + 0x284);
  local_25 = *(undefined1 *)(DAT_180020530 + 0x24c);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    local_5d8.sa_family = 0;
    local_5d8.sa_data[0] = '\0';
    local_5d8.sa_data[1] = '\0';
    local_5d8.sa_data[2] = '\0';
    local_5d8.sa_data[3] = '\0';
    local_5d8.sa_data[4] = '\0';
    local_5d8.sa_data[5] = '\0';
    local_5d8.sa_data[6] = '\0';
    local_5d8.sa_data[7] = '\0';
    local_5d8.sa_data[8] = '\0';
    local_5d8.sa_data[9] = '\0';
    local_5d8.sa_data[10] = '\0';
    local_5d8.sa_data[0xb] = '\0';
    local_5d8.sa_data[0xc] = '\0';
    local_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 3);
    uVar7 = 0;
    uVar6 = 0x5a4;
    local_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    local_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    local_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    local_5d8.sa_data[0] = (char)uVar2;
    local_5d8.sa_data[1] = (char)(uVar2 >> 8);
    local_5d8.sa_family = 2;
    iVar3 = sendto(s,local_5c8,0x5a4,0,&local_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar6,uVar7);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000dee0
   NAME : CmdRx
   SIG  : undefined __fastcall CmdRx(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void CmdRx(void)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  undefined1 auStackY_608 [32];
  sockaddr local_5d8;
  char local_5c8 [4];
  undefined1 local_5c4;
  byte local_5c3;
  byte local_5c2;
  byte local_5c1;
  undefined1 local_5b7;
  undefined1 local_5b6;
  undefined1 local_5b5;
  undefined1 local_5b2;
  undefined1 local_5b1;
  undefined1 local_5b0;
  undefined1 local_5af;
  undefined1 local_5ac;
  undefined1 local_5ab;
  undefined1 local_5aa;
  undefined1 local_5a9;
  undefined1 local_5a6;
  undefined1 local_5a5;
  undefined1 local_5a4;
  undefined1 local_5a3;
  undefined1 local_5a0;
  undefined1 local_59f;
  undefined1 local_59e;
  undefined1 local_59d;
  undefined1 local_59a;
  undefined1 local_599;
  undefined1 local_598;
  undefined1 local_597;
  undefined1 local_594;
  undefined1 local_593;
  undefined1 local_592;
  undefined1 local_591;
  undefined1 local_58e;
  undefined1 local_75;
  ulonglong local_18;
  
                    /* 0xdee0  9  CmdRx */
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_608;
  memset(local_5c8,0,0x5a4);
  s = DAT_180020598;
  local_5c4 = *(undefined1 *)(DAT_180020530 + 0x4c);
  local_5b5 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x334);
  local_5c3 = ((*(char *)(DAT_180020530 + 0x2cc) * '\x02' | *(byte *)(DAT_180020530 + 0x294)) *
               '\x02' | *(byte *)(DAT_180020530 + 0x25c)) & 7;
  local_5c2 = ((*(char *)(DAT_180020530 + 0x2d0) * '\x02' | *(byte *)(DAT_180020530 + 0x298)) *
               '\x02' | *(byte *)(DAT_180020530 + 0x260)) & 7;
  local_5c1 = (((((*(char *)(DAT_180020530 + 0x95c) * '\x02' | *(byte *)(DAT_180020530 + 0x854)) *
                  '\x02' | *(byte *)(DAT_180020530 + 0x74c)) * '\x02' |
                *(byte *)(DAT_180020530 + 0x644)) * '\x02' | *(byte *)(DAT_180020530 + 0x53c)) *
               '\x02' | *(byte *)(DAT_180020530 + 0x434)) * '\x02' |
              *(byte *)(DAT_180020530 + 0x32c);
  local_5b7 = *(undefined1 *)(DAT_180020530 + 0x324);
  local_5b6 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x334) >> 8);
  local_5b2 = *(undefined1 *)(DAT_180020530 + 0x338);
  local_5b1 = *(undefined1 *)(DAT_180020530 + 0x42c);
  local_5b0 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x43c) >> 8);
  local_5ac = *(undefined1 *)(DAT_180020530 + 0x440);
  local_5ab = *(undefined1 *)(DAT_180020530 + 0x534);
  local_5af = (undefined1)*(undefined4 *)(DAT_180020530 + 0x43c);
  local_5aa = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x544) >> 8);
  local_5a6 = *(undefined1 *)(DAT_180020530 + 0x548);
  local_5a5 = *(undefined1 *)(DAT_180020530 + 0x63c);
  local_5a9 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x544);
  local_5a4 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x64c) >> 8);
  local_5a0 = *(undefined1 *)(DAT_180020530 + 0x650);
  local_59f = *(undefined1 *)(DAT_180020530 + 0x744);
  local_5a3 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x64c);
  local_59e = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x754) >> 8);
  local_59a = *(undefined1 *)(DAT_180020530 + 0x758);
  local_599 = *(undefined1 *)(DAT_180020530 + 0x84c);
  local_59d = (undefined1)*(undefined4 *)(DAT_180020530 + 0x754);
  local_598 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x85c) >> 8);
  local_594 = *(undefined1 *)(DAT_180020530 + 0x860);
  local_593 = *(undefined1 *)(DAT_180020530 + 0x954);
  local_597 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x85c);
  local_592 = (undefined1)((uint)*(undefined4 *)(DAT_180020530 + 0x964) >> 8);
  local_58e = *(undefined1 *)(DAT_180020530 + 0x968);
  local_75 = *(undefined1 *)(DAT_180020530 + 0x330);
  local_591 = (undefined1)*(undefined4 *)(DAT_180020530 + 0x964);
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    local_5d8.sa_family = 0;
    local_5d8.sa_data[0] = '\0';
    local_5d8.sa_data[1] = '\0';
    local_5d8.sa_data[2] = '\0';
    local_5d8.sa_data[3] = '\0';
    local_5d8.sa_data[4] = '\0';
    local_5d8.sa_data[5] = '\0';
    local_5d8.sa_data[6] = '\0';
    local_5d8.sa_data[7] = '\0';
    local_5d8.sa_data[8] = '\0';
    local_5d8.sa_data[9] = '\0';
    local_5d8.sa_data[10] = '\0';
    local_5d8.sa_data[0xb] = '\0';
    local_5d8.sa_data[0xc] = '\0';
    local_5d8.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 1);
    uVar6 = 0;
    uVar5 = 0x5a4;
    local_5d8.sa_data[2] = '\0';
    local_5d8.sa_data[3] = '\0';
    local_5d8.sa_data[4] = '\0';
    local_5d8.sa_data[5] = '\0';
    local_5d8.sa_data[0] = (char)uVar2;
    local_5d8.sa_data[1] = (char)(uVar2 >> 8);
    local_5d8.sa_family = 2;
    local_5d8.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_5d8.sa_data[3] = DAT_18001e1d8._1_1_;
    local_5d8.sa_data[4] = DAT_18001e1d8._2_1_;
    local_5d8.sa_data[5] = DAT_18001e1d8._3_1_;
    iVar3 = sendto(s,local_5c8,0x5a4,0,&local_5d8,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e1c0
   NAME : FUN_18000e1c0
   SIG  : undefined __fastcall FUN_18000e1c0(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_18000e1c0(void)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  uint uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  undefined1 auStackY_98 [32];
  sockaddr local_68;
  longlong local_58;
  undefined1 auStack_50 [4];
  undefined1 uStack_4c;
  undefined1 uStack_4a;
  undefined1 uStack_49;
  longlong local_48;
  longlong lStack_40;
  undefined8 local_38;
  undefined4 uStack_30;
  undefined4 uStack_2c;
  undefined2 uStack_28;
  undefined1 local_26;
  undefined1 local_25;
  longlong lStack_24;
  ulonglong local_18;
  
  s = DAT_180020598;
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_98;
  local_58 = (ulonglong)
             CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x2f4) >> 8),
                      CONCAT12(*(undefined1 *)(DAT_180020530 + 0x2f0),
                               CONCAT11(*(undefined1 *)(DAT_180020530 + 0x30c),
                                        *(undefined1 *)(DAT_180020530 + 0x50)))) << 0x20;
  _auStack_50 = CONCAT14((char)*(undefined4 *)(DAT_180020530 + 0x300),
                         CONCAT13((char)((uint)*(undefined4 *)(DAT_180020530 + 0x300) >> 8),
                                  CONCAT21(CONCAT11(*(undefined1 *)(DAT_180020530 + 0x2fc),
                                                    *(undefined1 *)(DAT_180020530 + 0x2f8)),
                                           (char)*(undefined4 *)(DAT_180020530 + 0x2f4))));
  local_48 = (ulonglong)*(byte *)(DAT_180020530 + 0x308) << 8;
  _auStack_50 = CONCAT17((char)*(undefined4 *)(DAT_180020530 + 0xf88),
                         CONCAT16((char)((uint)*(undefined4 *)(DAT_180020530 + 0xf88) >> 8),
                                  CONCAT15(*(undefined1 *)(DAT_180020530 + 0x304),_auStack_50)));
  local_26 = *(undefined1 *)(DAT_180020530 + 0x314);
  lStack_40 = (ulonglong)
              CONCAT11((char)*(undefined4 *)(DAT_180020530 + 0xfac),
                       (char)((uint)*(undefined4 *)(DAT_180020530 + 0xfac) >> 8)) << 0x10;
  local_38 = 0;
  uStack_30 = 0;
  uStack_2c = 0;
  uStack_28 = 0;
  local_25 = *(undefined1 *)(DAT_180020530 + 0x310);
  lStack_24 = (ulonglong)
              CONCAT12(*(undefined1 *)(DAT_180020530 + 0x250),
                       CONCAT11(*(undefined1 *)(DAT_180020530 + 0x288),
                                *(undefined1 *)(DAT_180020530 + 0x2c0))) << 0x28;
  if ((DAT_180020598 != 0xffffffffffffffff) && (DAT_180020568 == 1)) {
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    local_68.sa_family = 0;
    local_68.sa_data[0] = '\0';
    local_68.sa_data[1] = '\0';
    local_68.sa_data[2] = '\0';
    local_68.sa_data[3] = '\0';
    local_68.sa_data[4] = '\0';
    local_68.sa_data[5] = '\0';
    local_68.sa_data[6] = '\0';
    local_68.sa_data[7] = '\0';
    local_68.sa_data[8] = '\0';
    local_68.sa_data[9] = '\0';
    local_68.sa_data[10] = '\0';
    local_68.sa_data[0xb] = '\0';
    local_68.sa_data[0xc] = '\0';
    local_68.sa_data[0xd] = '\0';
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((short)uVar1 + 2);
    uVar6 = 0;
    uVar5 = 0x3c;
    local_68.sa_data[2] = '\0';
    local_68.sa_data[3] = '\0';
    local_68.sa_data[4] = '\0';
    local_68.sa_data[5] = '\0';
    local_68.sa_data[0] = (char)uVar2;
    local_68.sa_data[1] = (char)(uVar2 >> 8);
    local_68.sa_family = 2;
    local_68.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_68.sa_data[3] = DAT_18001e1d8._1_1_;
    local_68.sa_data[4] = DAT_18001e1d8._2_1_;
    local_68.sa_data[5] = DAT_18001e1d8._3_1_;
    iVar3 = sendto(s,(char *)&local_58,0x3c,0,&local_68,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar3 < 1) {
      if (iVar3 == -1) {
        uVar4 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar4,uVar5,uVar6);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar3;
      UNLOCK();
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 18000e3a0
   NAME : FUN_18000e3a0
   SIG  : undefined __fastcall FUN_18000e3a0(int param_1, void * param_2)
   ======================================================================== */

void FUN_18000e3a0(int param_1,void *param_2)

{
  undefined8 uVar1;
  double dVar2;
  int iVar3;
  HANDLE hHandle;
  longlong lVar4;
  int iVar5;
  longlong lVar6;
  int iVar7;
  longlong lVar8;
  longlong lVar9;
  double dVar10;
  double dVar11;
  double dVar12;
  
  lVar6 = DAT_180020530;
  if (DAT_180020568 != 1) {
    if (**(int **)(PTR_DAT_18001e080 + 0x1128) == 0) {
      if (param_1 != 1) goto LAB_18000eafc;
      memcpy(*(void **)(DAT_180020530 + 0x38),param_2,0x7e0);
    }
    else {
      if (param_1 != 1) {
LAB_18000eafc:
        memcpy(*(void **)(DAT_180020530 + 0x30),param_2,0x7e0);
        ReleaseSemaphore(*(HANDLE *)(lVar6 + 0xb0),1,(LPLONG)0x0);
        hHandle = *(HANDLE *)(DAT_180020530 + 0xd8);
        goto LAB_18000eb28;
      }
      memcpy((void *)(*(longlong *)(DAT_180020530 + 0x38) + 0x1680),param_2,0x7e0);
      iVar7 = 0x2d0;
      iVar3 = DAT_18001fbd8 + 0x100;
      iVar5 = DAT_18001fbd8;
      do {
        lVar9 = (longlong)iVar7;
        lVar8 = (longlong)iVar5;
        lVar4 = (longlong)iVar3;
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 8 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x10 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 8 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x18 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x10 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x20 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x18 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x28 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x10 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x30 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x18 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x38 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x20 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x40 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x28 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x48 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x20 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x50 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x28 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x58 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x30 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x60 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x38 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x68 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x30 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x70 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x38 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x78 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x40 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x80 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x48 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x88 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x40 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x90 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x48 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x98 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x50 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xa0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x58 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xa8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x50 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xb0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x58 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xb8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x60 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xc0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x68 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 200 + lVar9 * 8);
        iVar7 = iVar7 + 0x24;
        iVar3 = iVar3 + 0x12;
        iVar5 = iVar5 + 0x12;
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x60 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xd0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x68 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xd8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x70 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xe0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x78 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xe8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x70 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xf0 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x78 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0xf8 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x80 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x100 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x88 + lVar8 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x108 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x80 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x110 + lVar9 * 8);
        *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x88 + lVar4 * 8) =
             *(undefined8 *)(*(longlong *)(lVar6 + 0x38) + 0x118 + lVar9 * 8);
      } while (iVar7 != 0x3cc);
      DAT_18001fbd8 = (DAT_18001fbd8 + 0x7e) % 0xfc;
      if (DAT_18001fbd8 != 0) {
        return;
      }
    }
    ReleaseSemaphore(*(HANDLE *)(lVar6 + 0xb8),1,(LPLONG)0x0);
    hHandle = *(HANDLE *)(DAT_180020530 + 0xd0);
LAB_18000eb28:
    WaitForSingleObject(hHandle,0xffffffff);
    return;
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0xf8));
  lVar6 = DAT_180020530;
  dVar11 = DAT_180018e40;
  dVar2 = DAT_180018db0;
  iVar5 = 0;
  iVar3 = 0;
  if (param_1 == 1) {
    iVar3 = *(int *)(DAT_180020530 + 0xfc8);
    if (0 < iVar3 * 2) {
      do {
        dVar10 = *(double *)((longlong)param_2 + (longlong)iVar5 * 8);
        dVar12 = dVar10 * dVar11;
        if (dVar10 < 0.0) {
          dVar10 = ceil(dVar12 - dVar2);
        }
        else {
          dVar10 = floor(dVar12 + dVar2);
        }
        iVar7 = (int)dVar10;
        iVar3 = iVar5 * 3;
        iVar5 = iVar5 + 1;
        lVar4 = (longlong)iVar3;
        *(char *)(lVar4 + *(longlong *)(lVar6 + 0x28)) = (char)((uint)iVar7 >> 0x10);
        *(char *)(lVar4 + 1 + *(longlong *)(lVar6 + 0x28)) = (char)((uint)iVar7 >> 8);
        *(char *)(lVar4 + 2 + *(longlong *)(lVar6 + 0x28)) = (char)iVar7;
        iVar3 = *(int *)(lVar6 + 0xfc8);
      } while (iVar5 < iVar3 * 2);
    }
  }
  else {
    if ((*(int *)(DAT_180020530 + 0x23c) != 0) &&
       (iVar7 = 0, 0 < *(int *)(DAT_180020530 + 0x1064) * 2)) {
      do {
        lVar4 = (longlong)iVar7;
        iVar7 = iVar7 + 2;
        uVar1 = *(undefined8 *)((longlong)param_2 + lVar4 * 8);
        *(undefined8 *)((longlong)param_2 + lVar4 * 8) =
             *(undefined8 *)((longlong)param_2 + lVar4 * 8 + 8);
        *(undefined8 *)((longlong)param_2 + lVar4 * 8 + 8) = uVar1;
      } while (iVar7 < *(int *)(lVar6 + 0x1064) * 2);
    }
    lVar6 = DAT_180020530;
    dVar11 = DAT_180018e10;
    dVar2 = DAT_180018dc8;
    if (DAT_1800201e0 == 0) {
      if (DAT_1800201e4 == 1) goto LAB_18000e526;
      if (DAT_1800201e4 == 2) {
        memset(param_2,0,(longlong)(*(int *)(DAT_180020530 + 0x1064) * 2) << 3);
        DAT_18001fbdc = DAT_18001fbdc + *(int *)(lVar6 + 0x1064) * 2;
        if (0x4af < DAT_18001fbdc) {
          DAT_1800201e4 = 3;
          DAT_18001fbdc = 0;
        }
      }
      else if (DAT_1800201e4 == 3) {
        iVar7 = *(int *)(DAT_180020530 + 0x1064);
        if (0 < iVar7 * 2) {
          do {
            iVar7 = DAT_18001fbdc + iVar3;
            lVar4 = (longlong)iVar3;
            iVar3 = iVar3 + 1;
            dVar12 = (double)iVar7 / dVar11;
            dVar10 = dVar2;
            if (dVar12 <= dVar2) {
              dVar10 = dVar12;
            }
            *(double *)((longlong)param_2 + lVar4 * 8) =
                 dVar10 * *(double *)((longlong)param_2 + lVar4 * 8);
            iVar7 = *(int *)(lVar6 + 0x1064);
          } while (iVar3 < iVar7 * 2);
        }
        DAT_18001fbdc = DAT_18001fbdc + iVar7 * 2;
        if (0x1df < DAT_18001fbdc) {
          DAT_1800201e4 = 0;
        }
      }
    }
    else {
      DAT_1800201e0 = 0;
      DAT_18001fbdc = 0;
      DAT_1800201e4 = 1;
LAB_18000e526:
      iVar3 = *(int *)(DAT_180020530 + 0x1064);
      iVar7 = 0;
      if (0 < iVar3 * 2) {
        do {
          iVar3 = DAT_18001fbdc + iVar7;
          lVar4 = (longlong)iVar7;
          iVar7 = iVar7 + 1;
          dVar12 = dVar2 - (double)iVar3 / dVar11;
          dVar10 = 0.0;
          if (0.0 <= dVar12) {
            dVar10 = dVar12;
          }
          *(double *)((longlong)param_2 + lVar4 * 8) =
               dVar10 * *(double *)((longlong)param_2 + lVar4 * 8);
          iVar3 = *(int *)(lVar6 + 0x1064);
        } while (iVar7 < iVar3 * 2);
      }
      DAT_18001fbdc = DAT_18001fbdc + iVar3 * 2;
      if (0x1df < DAT_18001fbdc) {
        DAT_1800201e4 = 2;
        DAT_18001fbdc = 0;
      }
    }
    dVar10 = DAT_180018e40;
    dVar11 = DAT_180018e38;
    dVar2 = DAT_180018db0;
    iVar3 = *(int *)(lVar6 + 0x1064);
    if ((*(byte *)(lVar6 + 0x314) & 0x40) == 0) {
      if (0 < iVar3 * 2) {
        do {
          dVar10 = *(double *)((longlong)param_2 + (longlong)iVar5 * 8);
          dVar12 = dVar10 * dVar11;
          if (dVar10 < 0.0) {
            dVar10 = ceil(dVar12 - dVar2);
          }
          else {
            dVar10 = floor(dVar12 + dVar2);
          }
          iVar3 = iVar5 * 2;
          iVar5 = iVar5 + 1;
          *(char *)((longlong)iVar3 + *(longlong *)(lVar6 + 0x28)) = (char)((uint)(int)dVar10 >> 8);
          *(char *)((longlong)iVar3 + 1 + *(longlong *)(lVar6 + 0x28)) = (char)(int)dVar10;
          iVar3 = *(int *)(lVar6 + 0x1064);
        } while (iVar5 < iVar3 * 2);
      }
      iVar3 = iVar3 * 4;
      goto LAB_18000e7aa;
    }
    if (0 < iVar3 * 2) {
      do {
        dVar11 = *(double *)((longlong)param_2 + (longlong)iVar5 * 8);
        dVar12 = dVar11 * dVar10;
        if (dVar11 < 0.0) {
          dVar11 = ceil(dVar12 - dVar2);
        }
        else {
          dVar11 = floor(dVar12 + dVar2);
        }
        iVar7 = (int)dVar11;
        iVar3 = iVar5 * 3;
        iVar5 = iVar5 + 1;
        lVar4 = (longlong)iVar3;
        *(char *)(lVar4 + *(longlong *)(lVar6 + 0x28)) = (char)((uint)iVar7 >> 0x10);
        *(char *)(lVar4 + 1 + *(longlong *)(lVar6 + 0x28)) = (char)((uint)iVar7 >> 8);
        *(char *)(lVar4 + 2 + *(longlong *)(lVar6 + 0x28)) = (char)iVar7;
      } while (iVar5 < *(int *)(lVar6 + 0x1064) * 2);
      iVar3 = *(int *)(lVar6 + 0x1064) * 6;
      goto LAB_18000e7aa;
    }
  }
  iVar3 = iVar3 * 6;
LAB_18000e7aa:
  FUN_18000eb40(param_1,*(void **)(lVar6 + 0x28),iVar3);
                    /* WARNING: Could not recover jumptable at 0x00018000e7db. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0xf8));
  return;
}



/* ========================================================================
   ENTRY: 18000eb40
   NAME : FUN_18000eb40
   SIG  : undefined __fastcall FUN_18000eb40(int param_1, void * param_2, int param_3)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000eb40(int param_1,void *param_2,int param_3)

{
  longlong lVar1;
  undefined1 *_Dst;
  u_short uVar2;
  undefined1 auStack_5e8 [32];
  char local_5c8;
  undefined1 local_5c7;
  undefined1 local_5c6;
  undefined1 local_5c5;
  undefined1 local_5c4 [1452];
  ulonglong local_18;
  
  lVar1 = DAT_180020530;
  local_18 = DAT_18001e000 ^ (ulonglong)auStack_5e8;
  if (param_1 == 0) {
    _Dst = local_5c4;
    local_5c8 = *(char *)(DAT_180020530 + 0x34b);
    local_5c7 = *(undefined1 *)(DAT_180020530 + 0x34a);
    local_5c6 = *(undefined1 *)(DAT_180020530 + 0x349);
    local_5c5 = *(undefined1 *)(DAT_180020530 + 0x348);
    *(int *)(DAT_180020530 + 0x348) = *(int *)(DAT_180020530 + 0x348) + 1;
    memcpy(_Dst,param_2,(longlong)param_3);
    if (DAT_180020598 == -1) {
      return;
    }
    if (DAT_180020568 != 1) {
      return;
    }
    uVar2 = (short)*(undefined4 *)(lVar1 + 4) + 4;
  }
  else {
    if (param_1 != 1) {
      return;
    }
    _Dst = local_5c4;
    _DAT_18001f4b0 = _DAT_18001f4b0 + 1;
    local_5c8 = *(char *)(DAT_180020530 + 0xfc7);
    local_5c7 = *(undefined1 *)(DAT_180020530 + 0xfc6);
    local_5c6 = *(undefined1 *)(DAT_180020530 + 0xfc5);
    local_5c5 = *(undefined1 *)(DAT_180020530 + 0xfc4);
    *(int *)(DAT_180020530 + 0xfc4) = *(int *)(DAT_180020530 + 0xfc4) + 1;
    memcpy(_Dst,param_2,(longlong)param_3);
    if (DAT_180020598 == -1) {
      return;
    }
    if (DAT_180020568 != 1) {
      return;
    }
    uVar2 = (short)*(undefined4 *)(lVar1 + 4) + 5;
  }
  FUN_18000ec70(_Dst,&local_5c8,param_3 + 4,uVar2);
  return;
}



/* ========================================================================
   ENTRY: 18000ec70
   NAME : FUN_18000ec70
   SIG  : int __fastcall FUN_18000ec70(undefined8 param_1, char * param_2, uint param_3, u_short param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_18000ec70(undefined8 param_1,char *param_2,uint param_3,u_short param_4)

{
  SOCKET s;
  u_short uVar1;
  int iVar2;
  uint uVar3;
  ulonglong uVar4;
  undefined8 uVar5;
  undefined1 auStackY_68 [32];
  sockaddr local_38;
  ulonglong local_28;
  
  s = DAT_180020598;
  local_28 = DAT_18001e000 ^ (ulonglong)auStackY_68;
  uVar4 = (ulonglong)param_3;
  local_38.sa_family = 0;
  local_38.sa_data[0] = '\0';
  local_38.sa_data[1] = '\0';
  local_38.sa_data[2] = '\0';
  local_38.sa_data[3] = '\0';
  local_38.sa_data[4] = '\0';
  local_38.sa_data[5] = '\0';
  local_38.sa_data[6] = '\0';
  local_38.sa_data[7] = '\0';
  local_38.sa_data[8] = '\0';
  local_38.sa_data[9] = '\0';
  local_38.sa_data[10] = '\0';
  local_38.sa_data[0xb] = '\0';
  local_38.sa_data[0xc] = '\0';
  local_38.sa_data[0xd] = '\0';
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
  uVar1 = htons(param_4);
  uVar5 = 0;
  local_38.sa_data[2] = '\0';
  local_38.sa_data[3] = '\0';
  local_38.sa_data[4] = '\0';
  local_38.sa_data[5] = '\0';
  local_38.sa_data[0] = (char)uVar1;
  local_38.sa_data[1] = (char)(uVar1 >> 8);
  local_38.sa_family = 2;
  local_38.sa_data[2] = (undefined1)DAT_18001e1d8;
  local_38.sa_data[3] = DAT_18001e1d8._1_1_;
  local_38.sa_data[4] = DAT_18001e1d8._2_1_;
  local_38.sa_data[5] = DAT_18001e1d8._3_1_;
  iVar2 = sendto(s,param_2,param_3,0,&local_38,0x10);
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
  if (iVar2 < 1) {
    if (iVar2 == -1) {
      uVar3 = WSAGetLastError();
      FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar3,uVar4,uVar5);
    }
  }
  else {
    LOCK();
    DAT_18001fbc0 = DAT_18001fbc0 + iVar2;
    UNLOCK();
  }
  return iVar2;
}



/* ========================================================================
   ENTRY: 18000ed60
   NAME : FUN_18000ed60
   SIG  : undefined8 __fastcall FUN_18000ed60(void)
   ======================================================================== */

undefined8 FUN_18000ed60(void)

{
  HANDLE pvVar1;
  DWORD local_res10 [6];
  
  local_res10[0] = 0;
  pvVar1 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res10);
  if (pvVar1 == (HANDLE)0x0) {
    pvVar1 = GetCurrentThread();
    SetThreadPriority(pvVar1,0xf);
  }
  else {
    AvSetMmThreadPriority(pvVar1,AVRT_PRIORITY_CRITICAL);
  }
  DAT_180020514 = 1;
  DAT_1800205ac = 1;
  ReleaseSemaphore(*(HANDLE *)(DAT_180020530 + 0x98),1,(LPLONG)0x0);
  FUN_18000cf90();
  DAT_1800205ac = 0;
  return 0;
}



/* ========================================================================
   ENTRY: 18000edf0
   NAME : FUN_18000edf0
   SIG  : undefined8 __fastcall FUN_18000edf0(void)
   ======================================================================== */

undefined8 FUN_18000edf0(void)

{
  longlong lVar1;
  DWORD DVar2;
  BOOL BVar3;
  HANDLE hTimer;
  undefined8 uVar4;
  undefined8 in_R9;
  undefined8 uVar5;
  
  uVar4 = 0;
  *(undefined8 *)(DAT_180020530 + 0xf0) = 0xffffffffff676980;
  hTimer = CreateWaitableTimerW((LPSECURITY_ATTRIBUTES)0x0,0,(LPCWSTR)0x0);
  lVar1 = DAT_180020530;
  *(HANDLE *)(DAT_180020530 + 0xe8) = hTimer;
  if (hTimer == (HANDLE)0x0) {
    DVar2 = GetLastError();
    FUN_180007660("CreateWaitableTimer failed (%d)\n",(ulonglong)DVar2,uVar4,in_R9);
    return 0;
  }
  uVar5 = 0;
  uVar4 = 500;
  BVar3 = SetWaitableTimer(hTimer,(LARGE_INTEGER *)(lVar1 + 0xf0),500,(PTIMERAPCROUTINE)0x0,
                           (LPVOID)0x0,0);
  if (BVar3 == 0) {
    DVar2 = GetLastError();
    FUN_180007660("SetWaitableTimer failed (%d)\n",(ulonglong)DVar2,uVar4,uVar5);
  }
  while (DAT_180020514 != 0) {
    WaitForSingleObject(*(HANDLE *)(DAT_180020530 + 0xe8),0xffffffff);
    if ((*(int *)(DAT_180020530 + 0x40) != 0) && (*(int *)(DAT_180020530 + 0x44) != 0)) {
      FUN_18000d740();
    }
  }
  CancelWaitableTimer(*(HANDLE *)(DAT_180020530 + 0xe8));
  CloseHandle(*(HANDLE *)(DAT_180020530 + 0xe8));
  return 0;
}



/* ========================================================================
   ENTRY: 18000ef00
   NAME : SendStartToMetis
   SIG  : int __fastcall SendStartToMetis(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int SendStartToMetis(void)

{
  undefined4 uVar1;
  SOCKET s;
  u_short uVar2;
  int iVar3;
  int iVar4;
  uint uVar5;
  undefined4 *puVar6;
  void *pvVar7;
  undefined1 *puVar8;
  int iVar9;
  int iVar10;
  undefined8 uVar11;
  undefined8 uVar12;
  undefined1 auStackY_c98 [32];
  undefined4 local_c68;
  byte local_c64;
  undefined2 local_c63;
  char local_c61;
  undefined1 local_c60 [504];
  undefined4 local_a68;
  undefined1 local_a64;
  undefined1 local_a63;
  undefined1 local_a62;
  undefined1 local_a61;
  undefined1 local_a60 [504];
  sockaddr local_868;
  char local_858 [64];
  undefined8 local_818 [250];
  ulonglong local_48;
  
                    /* 0xef00  55  SendStartToMetis */
  local_48 = DAT_18001e000 ^ (ulonglong)auStackY_c98;
  puVar6 = malloc(0x70);
  *puVar6 = 1;
  puVar6[1] = 0x400;
  puVar6[2] = 0x10;
  puVar6[3] = 5;
  pvVar7 = calloc(0x10,0x400);
  *(void **)(puVar6 + 4) = pvVar7;
  pvVar7 = calloc((longlong)(int)puVar6[2],8);
  iVar9 = 0;
  *(void **)(puVar6 + 6) = pvVar7;
  iVar3 = puVar6[2];
  iVar4 = iVar9;
  if (0 < iVar3) {
    do {
      iVar10 = iVar4 + 1;
      *(longlong *)(*(longlong *)(puVar6 + 6) + (longlong)iVar4 * 8) =
           (longlong)(iVar4 * puVar6[1]) + *(longlong *)(puVar6 + 4);
      iVar3 = puVar6[2];
      iVar4 = iVar10;
    } while (iVar10 < iVar3);
  }
  pvVar7 = calloc((longlong)iVar3,4);
  *(undefined8 *)(puVar6 + 0xb) = 0;
  *(void **)(puVar6 + 0x10) = pvVar7;
  *(undefined8 *)(puVar6 + 0xd) = 0;
  puVar6[10] = puVar6[2] + -1;
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(puVar6 + 0x12),0x9c4);
  iVar3 = DAT_18001f4c0;
  local_858[8] = '\0';
  local_858[9] = '\0';
  local_858[10] = '\0';
  local_858[0xb] = '\0';
  local_858[0xc] = '\0';
  local_858[0xd] = '\0';
  local_858[0xe] = '\0';
  local_858[0xf] = '\0';
  local_858[0] = -0x11;
  local_858[1] = -2;
  local_858[2] = '\x04';
  local_858[3] = '\x01';
  local_858[4] = '\0';
  local_858[5] = '\0';
  local_858[6] = '\0';
  local_858[7] = '\0';
  local_858[0x10] = '\0';
  local_858[0x11] = '\0';
  local_858[0x12] = '\0';
  local_858[0x13] = '\0';
  local_858[0x14] = '\0';
  local_858[0x15] = '\0';
  local_858[0x16] = '\0';
  local_858[0x17] = '\0';
  local_858[0x18] = '\0';
  local_858[0x19] = '\0';
  local_858[0x1a] = '\0';
  local_858[0x1b] = '\0';
  local_858[0x1c] = '\0';
  local_858[0x1d] = '\0';
  local_858[0x1e] = '\0';
  local_858[0x1f] = '\0';
  local_858[0x20] = '\0';
  local_858[0x21] = '\0';
  local_858[0x22] = '\0';
  local_858[0x23] = '\0';
  local_858[0x24] = '\0';
  local_858[0x25] = '\0';
  local_858[0x26] = '\0';
  local_858[0x27] = '\0';
  local_858[0x28] = '\0';
  local_858[0x29] = '\0';
  local_858[0x2a] = '\0';
  local_858[0x2b] = '\0';
  local_858[0x2c] = '\0';
  local_858[0x2d] = '\0';
  local_858[0x2e] = '\0';
  local_858[0x2f] = '\0';
  local_858[0x30] = '\0';
  local_858[0x31] = '\0';
  local_858[0x32] = '\0';
  local_858[0x33] = '\0';
  local_858[0x34] = '\0';
  local_858[0x35] = '\0';
  local_858[0x36] = '\0';
  local_858[0x37] = '\0';
  local_858[0x38] = '\0';
  local_858[0x39] = '\0';
  local_858[0x3a] = '\0';
  local_858[0x3b] = '\0';
  local_858[0x3c] = '\0';
  local_858[0x3d] = '\0';
  local_858[0x3e] = '\0';
  local_858[0x3f] = '\0';
  DAT_180020508 = puVar6;
  do {
    uVar1 = *(undefined4 *)(DAT_180020530 + 0xf84);
    memset(local_c60,0,0x1f8);
    puVar8 = local_a60;
    memset(puVar8,0,0x1f8);
    local_c64 = DAT_180020520 & 3;
    local_c68 = 0x7f7f7f;
    local_c63 = 0;
    local_c61 = (DAT_1800205bc + -1) * '\b';
    local_a64 = (undefined1)((uint)uVar1 >> 0x18);
    local_a63 = (undefined1)((uint)uVar1 >> 0x10);
    local_a62 = (undefined1)((uint)uVar1 >> 8);
    local_a68 = 0x27f7f7f;
    local_a61 = (undefined1)uVar1;
    FUN_18000f7b0(puVar8,(undefined8 *)&local_c68);
    Sleep(10);
    uVar1 = *(undefined4 *)(DAT_180020530 + 0x328);
    memset(local_c60,0,0x1f8);
    puVar8 = local_a60;
    memset(puVar8,0,0x1f8);
    local_c64 = DAT_180020520 & 3;
    local_c68 = 0x7f7f7f;
    local_c63 = 0;
    local_c61 = (DAT_1800205bc + -1) * '\b';
    local_a64 = (undefined1)((uint)uVar1 >> 0x18);
    local_a63 = (undefined1)((uint)uVar1 >> 0x10);
    local_a62 = (undefined1)((uint)uVar1 >> 8);
    local_a68 = 0x47f7f7f;
    local_a61 = (undefined1)uVar1;
    FUN_18000f7b0(puVar8,(undefined8 *)&local_c68);
    Sleep(10);
    s = DAT_180020598;
    local_868.sa_family = 0;
    local_868.sa_data[0] = '\0';
    local_868.sa_data[1] = '\0';
    local_868.sa_data[2] = '\0';
    local_868.sa_data[3] = '\0';
    local_868.sa_data[4] = '\0';
    local_868.sa_data[5] = '\0';
    local_868.sa_data[6] = '\0';
    local_868.sa_data[7] = '\0';
    local_868.sa_data[8] = '\0';
    local_868.sa_data[9] = '\0';
    local_868.sa_data[10] = '\0';
    local_868.sa_data[0xb] = '\0';
    local_868.sa_data[0xc] = '\0';
    local_868.sa_data[0xd] = '\0';
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar2 = htons((u_short)uVar1);
    local_868.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_868.sa_data[3] = DAT_18001e1d8._1_1_;
    local_868.sa_data[4] = DAT_18001e1d8._2_1_;
    local_868.sa_data[5] = DAT_18001e1d8._3_1_;
    local_868.sa_data[0] = (char)uVar2;
    local_868.sa_data[1] = (char)(uVar2 >> 8);
    local_868.sa_family = 2;
    uVar12 = 0;
    uVar11 = 0x40;
    iVar4 = sendto(s,local_858,0x40,0,&local_868,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar11,uVar12);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
    FUN_18000f420(local_818);
    if (DAT_18001f4c0 != iVar3) goto LAB_18000f26b;
    Sleep(10);
    iVar9 = iVar9 + 1;
  } while (iVar9 < 5);
  iVar4 = -1;
  if (DAT_18001f4c0 != iVar3) {
LAB_18000f26b:
    iVar4 = 0;
  }
  return iVar4;
}



/* ========================================================================
   ENTRY: 18000f290
   NAME : SendStopToMetis
   SIG  : undefined8 __fastcall SendStopToMetis(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 SendStopToMetis(void)

{
  undefined4 uVar1;
  int iVar2;
  void *_Memory;
  SOCKET s;
  u_short uVar3;
  int iVar4;
  uint uVar5;
  int iVar6;
  undefined8 uVar7;
  undefined8 uVar8;
  undefined1 auStackY_b8 [32];
  sockaddr local_88;
  char local_78 [64];
  ulonglong local_38;
  
                    /* 0xf290  56  SendStopToMetis */
  iVar2 = DAT_18001f4c0;
  local_38 = DAT_18001e000 ^ (ulonglong)auStackY_b8;
  local_78[8] = '\0';
  local_78[9] = '\0';
  local_78[10] = '\0';
  local_78[0xb] = '\0';
  local_78[0xc] = '\0';
  local_78[0xd] = '\0';
  local_78[0xe] = '\0';
  local_78[0xf] = '\0';
  local_78[0] = -0x11;
  local_78[1] = -2;
  local_78[2] = '\x04';
  local_78[3] = '\0';
  local_78[4] = '\0';
  local_78[5] = '\0';
  local_78[6] = '\0';
  local_78[7] = '\0';
  iVar6 = 0;
  local_78[0x10] = '\0';
  local_78[0x11] = '\0';
  local_78[0x12] = '\0';
  local_78[0x13] = '\0';
  local_78[0x14] = '\0';
  local_78[0x15] = '\0';
  local_78[0x16] = '\0';
  local_78[0x17] = '\0';
  local_78[0x18] = '\0';
  local_78[0x19] = '\0';
  local_78[0x1a] = '\0';
  local_78[0x1b] = '\0';
  local_78[0x1c] = '\0';
  local_78[0x1d] = '\0';
  local_78[0x1e] = '\0';
  local_78[0x1f] = '\0';
  local_78[0x20] = '\0';
  local_78[0x21] = '\0';
  local_78[0x22] = '\0';
  local_78[0x23] = '\0';
  local_78[0x24] = '\0';
  local_78[0x25] = '\0';
  local_78[0x26] = '\0';
  local_78[0x27] = '\0';
  local_78[0x28] = '\0';
  local_78[0x29] = '\0';
  local_78[0x2a] = '\0';
  local_78[0x2b] = '\0';
  local_78[0x2c] = '\0';
  local_78[0x2d] = '\0';
  local_78[0x2e] = '\0';
  local_78[0x2f] = '\0';
  local_78[0x30] = '\0';
  local_78[0x31] = '\0';
  local_78[0x32] = '\0';
  local_78[0x33] = '\0';
  local_78[0x34] = '\0';
  local_78[0x35] = '\0';
  local_78[0x36] = '\0';
  local_78[0x37] = '\0';
  local_78[0x38] = '\0';
  local_78[0x39] = '\0';
  local_78[0x3a] = '\0';
  local_78[0x3b] = '\0';
  local_78[0x3c] = '\0';
  local_78[0x3d] = '\0';
  local_78[0x3e] = '\0';
  local_78[0x3f] = '\0';
  while( true ) {
    s = DAT_180020598;
    local_88.sa_family = 0;
    local_88.sa_data[0] = '\0';
    local_88.sa_data[1] = '\0';
    local_88.sa_data[2] = '\0';
    local_88.sa_data[3] = '\0';
    local_88.sa_data[4] = '\0';
    local_88.sa_data[5] = '\0';
    local_88.sa_data[6] = '\0';
    local_88.sa_data[7] = '\0';
    local_88.sa_data[8] = '\0';
    local_88.sa_data[9] = '\0';
    local_88.sa_data[10] = '\0';
    local_88.sa_data[0xb] = '\0';
    local_88.sa_data[0xc] = '\0';
    local_88.sa_data[0xd] = '\0';
    uVar1 = *(undefined4 *)(DAT_180020530 + 4);
    EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    uVar3 = htons((u_short)uVar1);
    uVar8 = 0;
    uVar7 = 0x40;
    local_88.sa_data[2] = (undefined1)DAT_18001e1d8;
    local_88.sa_data[3] = DAT_18001e1d8._1_1_;
    local_88.sa_data[4] = DAT_18001e1d8._2_1_;
    local_88.sa_data[5] = DAT_18001e1d8._3_1_;
    local_88.sa_data[0] = (char)uVar3;
    local_88.sa_data[1] = (char)(uVar3 >> 8);
    local_88.sa_family = 2;
    iVar4 = sendto(s,local_78,0x40,0,&local_88,0x10);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
    if (iVar4 < 1) {
      if (iVar4 == -1) {
        uVar5 = WSAGetLastError();
        FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar5,uVar7,uVar8);
      }
    }
    else {
      LOCK();
      DAT_18001fbc0 = DAT_18001fbc0 + iVar4;
      UNLOCK();
    }
    Sleep(10);
    _Memory = DAT_180020508;
    if (DAT_18001f4c0 == iVar2) break;
    iVar6 = iVar6 + 1;
    if (4 < iVar6) {
      return 0xffffffff;
    }
  }
  if (DAT_180020508 != (void *)0x0) {
    DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020508 + 0x48));
    free(*(void **)((longlong)_Memory + 0x18));
    free(*(void **)((longlong)_Memory + 0x10));
    free(*(void **)((longlong)_Memory + 0x40));
    free(_Memory);
  }
  DAT_180020508 = (void *)0x0;
  return 0;
}



/* ========================================================================
   ENTRY: 18000f420
   NAME : FUN_18000f420
   SIG  : undefined8 __fastcall FUN_18000f420(undefined8 * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_18000f420(undefined8 *param_1)

{
  undefined4 uVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined4 uVar5;
  undefined4 uVar6;
  undefined4 uVar7;
  undefined4 uVar8;
  undefined8 uVar9;
  undefined8 uVar10;
  int iVar11;
  uint uVar12;
  int *piVar13;
  uint *puVar14;
  FILE *_File;
  undefined8 *puVar15;
  char *pcVar16;
  ulonglong uVar17;
  longlong lVar18;
  undefined8 uVar19;
  undefined8 uVar20;
  undefined1 auStackY_5a8 [32];
  uint local_578;
  int local_574;
  sockaddr local_570;
  char local_558;
  char local_557;
  char local_556;
  byte local_555;
  undefined1 local_554;
  undefined1 local_553;
  undefined1 local_552;
  undefined1 local_551;
  char local_550;
  char cStack_54f;
  char cStack_54e;
  char local_118 [256];
  ulonglong local_18;
  
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_5a8;
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x1c0));
  local_574 = 0x10;
  uVar20 = 0;
  uVar19 = 0x432;
  iVar11 = recvfrom(DAT_180020598,&local_558,0x432,0,&local_570,&local_574);
  uVar17 = (ulonglong)iVar11;
  if (iVar11 == -1) {
    piVar13 = _errno();
    iVar11 = WSAGetLastError();
    *piVar13 = iVar11;
    piVar13 = _errno();
    if ((*piVar13 == 0x2733) || (piVar13 = _errno(), *piVar13 == 0x2738)) {
      piVar13 = _errno();
      strerror_s(local_118,0x100,*piVar13);
      puVar14 = (uint *)_errno();
      FUN_180007660("Error code %d: recvfrom() : %s\n",(ulonglong)*puVar14,local_118,uVar20);
      _File = (FILE *)__acrt_iob_func(1);
      fflush(_File);
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x1c0));
    uVar19 = 0xffffffff;
  }
  else {
    if (0 < iVar11) {
      LOCK();
      DAT_18001fbb0 = DAT_18001fbb0 + uVar17;
      UNLOCK();
    }
    if (iVar11 == 0x408) {
      if (((local_558 == -0x11) && (local_557 == -2)) && (local_556 == '\x01')) {
        local_578 = CONCAT31(CONCAT21(CONCAT11(local_554,local_553),local_552),local_551);
        if (local_555 == 6) {
          if (((local_550 == '\x7f') && (cStack_54f == '\x7f')) && (cStack_54e == '\x7f')) {
            DAT_1800205c8 = 1;
          }
          else {
            DAT_1800205c8 = 0;
            FUN_180007660("MRD: sync error on frame %d\n",(ulonglong)local_578,uVar19,uVar20);
          }
          lVar18 = 8;
          puVar15 = param_1;
          pcVar16 = &local_550;
          do {
            uVar19 = *(undefined8 *)(pcVar16 + 8);
            uVar20 = *(undefined8 *)(pcVar16 + 0x10);
            uVar9 = *(undefined8 *)(pcVar16 + 0x18);
            *puVar15 = *(undefined8 *)pcVar16;
            puVar15[1] = uVar19;
            uVar19 = *(undefined8 *)(pcVar16 + 0x20);
            uVar10 = *(undefined8 *)(pcVar16 + 0x28);
            puVar15[2] = uVar20;
            puVar15[3] = uVar9;
            uVar20 = *(undefined8 *)(pcVar16 + 0x30);
            uVar9 = *(undefined8 *)(pcVar16 + 0x38);
            puVar15[4] = uVar19;
            puVar15[5] = uVar10;
            uVar19 = *(undefined8 *)(pcVar16 + 0x40);
            uVar10 = *(undefined8 *)(pcVar16 + 0x48);
            puVar15[6] = uVar20;
            puVar15[7] = uVar9;
            uVar20 = *(undefined8 *)(pcVar16 + 0x50);
            uVar9 = *(undefined8 *)(pcVar16 + 0x58);
            puVar15[8] = uVar19;
            puVar15[9] = uVar10;
            uVar1 = *(undefined4 *)(pcVar16 + 0x60);
            uVar2 = *(undefined4 *)(pcVar16 + 100);
            uVar3 = *(undefined4 *)(pcVar16 + 0x68);
            uVar4 = *(undefined4 *)(pcVar16 + 0x6c);
            puVar15[10] = uVar20;
            puVar15[0xb] = uVar9;
            uVar5 = *(undefined4 *)(pcVar16 + 0x70);
            uVar6 = *(undefined4 *)(pcVar16 + 0x74);
            uVar7 = *(undefined4 *)(pcVar16 + 0x78);
            uVar8 = *(undefined4 *)(pcVar16 + 0x7c);
            *(undefined4 *)(puVar15 + 0xc) = uVar1;
            *(undefined4 *)((longlong)puVar15 + 100) = uVar2;
            *(undefined4 *)(puVar15 + 0xd) = uVar3;
            *(undefined4 *)((longlong)puVar15 + 0x6c) = uVar4;
            *(undefined4 *)(puVar15 + 0xe) = uVar5;
            *(undefined4 *)((longlong)puVar15 + 0x74) = uVar6;
            *(undefined4 *)(puVar15 + 0xf) = uVar7;
            *(undefined4 *)((longlong)puVar15 + 0x7c) = uVar8;
            piVar13 = DAT_180020508;
            lVar18 = lVar18 + -1;
            puVar15 = puVar15 + 0x10;
            pcVar16 = pcVar16 + 0x80;
          } while (lVar18 != 0);
          if (*DAT_180020508 != 0) {
            EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020508 + 0x12));
            if (piVar13[0xb] == 0) {
              uVar12 = local_578;
              if (local_578 == piVar13[0xd] + 1U) {
                piVar13[0xc] = piVar13[0xc] + 1;
                if (piVar13[0xc] == piVar13[2]) {
                  piVar13[0xb] = 1;
                  piVar13[9] = local_578 - piVar13[3] & piVar13[10];
                }
              }
              else {
                piVar13[0xc] = 0;
              }
            }
            else {
              piVar13[8] = local_578 & piVar13[10];
              memcpy(*(void **)(*(longlong *)(piVar13 + 6) +
                               (longlong)(int)(local_578 & piVar13[10]) * 8),param_1,
                     (longlong)piVar13[1]);
              *(uint *)(*(longlong *)(piVar13 + 0x10) + (longlong)piVar13[8] * 4) = local_578;
              uVar12 = piVar13[9] + 1U & piVar13[10];
              piVar13[9] = uVar12;
              memcpy(param_1,*(void **)(*(longlong *)(piVar13 + 6) + (longlong)(int)uVar12 * 8),
                     (longlong)piVar13[1]);
              if (*(int *)(*(longlong *)(piVar13 + 0x10) + (longlong)piVar13[9] * 4) !=
                  piVar13[0xd] + 1) {
                piVar13[0xe] = piVar13[0xe] + 1;
              }
              uVar12 = *(uint *)(*(longlong *)(piVar13 + 0x10) + (longlong)piVar13[9] * 4);
            }
            piVar13[0xd] = uVar12;
            LeaveCriticalSection((LPCRITICAL_SECTION)(piVar13 + 0x12));
          }
          if (local_578 != DAT_18001f4c0 + 1) {
            _DAT_18001f4b8 = _DAT_18001f4b8 + 1;
          }
          DAT_18001f4c0 = local_578;
          LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x1c0));
          return 0x400;
        }
        uVar17 = (ulonglong)local_555;
        pcVar16 = "MRD: ignoring data for ep %d\n";
      }
      else {
        uVar17 = 0x408;
        pcVar16 = "MRD: ignoring right sized frame bad header! %d\n";
      }
    }
    else {
      pcVar16 = "MRD: ignoring short frame size=%d\n";
    }
    FUN_180007660(pcVar16,uVar17,uVar19,uVar20);
    LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x1c0));
    uVar19 = 0;
  }
  return uVar19;
}



/* ========================================================================
   ENTRY: 18000f7b0
   NAME : FUN_18000f7b0
   SIG  : int __fastcall FUN_18000f7b0(undefined8 param_1, undefined8 * param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_18000f7b0(undefined8 param_1,undefined8 *param_2)

{
  undefined4 uVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  SOCKET s;
  u_short uVar7;
  int iVar8;
  uint uVar9;
  undefined8 *puVar10;
  longlong lVar11;
  undefined8 uVar12;
  undefined8 uVar13;
  undefined1 auStackY_468 [32];
  sockaddr local_438;
  char local_428 [5];
  undefined1 local_423;
  undefined1 local_422;
  undefined1 local_421;
  undefined8 local_420 [129];
  ulonglong local_18;
  
  s = DAT_180020598;
  local_18 = DAT_18001e000 ^ (ulonglong)auStackY_468;
  local_428[4] = (char)((uint)DAT_180020500 >> 0x18);
  lVar11 = 8;
  local_423 = (undefined1)((uint)DAT_180020500 >> 0x10);
  local_422 = (undefined1)((uint)DAT_180020500 >> 8);
  local_421 = (undefined1)DAT_180020500;
  DAT_180020500 = DAT_180020500 + 1;
  local_428[0] = -0x11;
  local_428[1] = -2;
  local_428[2] = '\x01';
  local_428[3] = '\x02';
  puVar10 = local_420;
  do {
    uVar12 = param_2[1];
    uVar13 = param_2[2];
    uVar5 = param_2[3];
    *puVar10 = *param_2;
    puVar10[1] = uVar12;
    uVar12 = param_2[4];
    uVar6 = param_2[5];
    puVar10[2] = uVar13;
    puVar10[3] = uVar5;
    uVar13 = param_2[6];
    uVar5 = param_2[7];
    puVar10[4] = uVar12;
    puVar10[5] = uVar6;
    uVar12 = param_2[8];
    uVar6 = param_2[9];
    puVar10[6] = uVar13;
    puVar10[7] = uVar5;
    uVar13 = param_2[10];
    uVar5 = param_2[0xb];
    puVar10[8] = uVar12;
    puVar10[9] = uVar6;
    uVar12 = param_2[0xc];
    uVar6 = param_2[0xd];
    puVar10[10] = uVar13;
    puVar10[0xb] = uVar5;
    uVar1 = *(undefined4 *)(param_2 + 0xe);
    uVar2 = *(undefined4 *)((longlong)param_2 + 0x74);
    uVar3 = *(undefined4 *)(param_2 + 0xf);
    uVar4 = *(undefined4 *)((longlong)param_2 + 0x7c);
    puVar10[0xc] = uVar12;
    puVar10[0xd] = uVar6;
    *(undefined4 *)(puVar10 + 0xe) = uVar1;
    *(undefined4 *)((longlong)puVar10 + 0x74) = uVar2;
    *(undefined4 *)(puVar10 + 0xf) = uVar3;
    *(undefined4 *)((longlong)puVar10 + 0x7c) = uVar4;
    lVar11 = lVar11 + -1;
    puVar10 = puVar10 + 0x10;
    param_2 = param_2 + 0x10;
  } while (lVar11 != 0);
  local_438.sa_family = 0;
  local_438.sa_data[0] = '\0';
  local_438.sa_data[1] = '\0';
  local_438.sa_data[2] = '\0';
  local_438.sa_data[3] = '\0';
  local_438.sa_data[4] = '\0';
  local_438.sa_data[5] = '\0';
  local_438.sa_data[6] = '\0';
  local_438.sa_data[7] = '\0';
  local_438.sa_data[8] = '\0';
  local_438.sa_data[9] = '\0';
  local_438.sa_data[10] = '\0';
  local_438.sa_data[0xb] = '\0';
  local_438.sa_data[0xc] = '\0';
  local_438.sa_data[0xd] = '\0';
  uVar1 = *(undefined4 *)(DAT_180020530 + 4);
  EnterCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
  uVar7 = htons((u_short)uVar1);
  uVar13 = 0;
  uVar12 = 0x408;
  local_438.sa_data[2] = '\0';
  local_438.sa_data[3] = '\0';
  local_438.sa_data[4] = '\0';
  local_438.sa_data[5] = '\0';
  local_438.sa_data[0] = (char)uVar7;
  local_438.sa_data[1] = (char)(uVar7 >> 8);
  local_438.sa_family = 2;
  local_438.sa_data[2] = (undefined1)DAT_18001e1d8;
  local_438.sa_data[3] = DAT_18001e1d8._1_1_;
  local_438.sa_data[4] = DAT_18001e1d8._2_1_;
  local_438.sa_data[5] = DAT_18001e1d8._3_1_;
  iVar8 = sendto(s,local_428,0x408,0,&local_438,0x10);
  LeaveCriticalSection((LPCRITICAL_SECTION)(DAT_180020530 + 0x170));
  if (iVar8 < 1) {
    if (iVar8 == -1) {
      uVar9 = WSAGetLastError();
      FUN_18000be70(L"sendto failed with error:%d\n",(ulonglong)uVar9,uVar12,uVar13);
    }
  }
  else {
    LOCK();
    DAT_18001fbc0 = DAT_18001fbc0 + iVar8;
    UNLOCK();
  }
  return iVar8 + -8;
}



/* ========================================================================
   ENTRY: 18000f950
   NAME : FUN_18000f950
   SIG  : undefined8 __fastcall FUN_18000f950(void)
   ======================================================================== */

undefined8 FUN_18000f950(void)

{
  HANDLE pvVar1;
  FILE *pFVar2;
  undefined8 uVar3;
  undefined8 uVar4;
  undefined8 in_R9;
  DWORD local_res10 [6];
  
  local_res10[0] = 0;
  pvVar1 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res10);
  if (pvVar1 == (HANDLE)0x0) {
    pvVar1 = GetCurrentThread();
    SetThreadPriority(pvVar1,2);
  }
  else {
    AvSetMmThreadPriority(pvVar1,AVRT_PRIORITY_CRITICAL);
  }
  uVar4 = 0;
  uVar3 = 1;
  DAT_180020514 = 1;
  DAT_1800205ac = 1;
  ReleaseSemaphore(*(HANDLE *)(DAT_180020530 + 0x98),1,(LPLONG)0x0);
  FUN_180007660("MetisReadThread runs...\n",uVar3,uVar4,in_R9);
  pFVar2 = (FILE *)__acrt_iob_func(1);
  fflush(pFVar2);
  FUN_18000fa20();
  DAT_1800205ac = 0;
  FUN_180007660("MetisReadThread dies...\n",uVar3,uVar4,in_R9);
  pFVar2 = (FILE *)__acrt_iob_func(1);
  fflush(pFVar2);
  return 0;
}



/* ========================================================================
   ENTRY: 18000fa20
   NAME : FUN_18000fa20
   SIG  : undefined __fastcall FUN_18000fa20(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_18000fa20(void)

{
  byte bVar1;
  byte bVar2;
  byte bVar3;
  byte bVar4;
  char cVar5;
  double dVar6;
  float fVar7;
  void *_Memory;
  undefined4 uVar8;
  FILE *pFVar9;
  void *pvVar10;
  byte bVar11;
  undefined1 *puVar12;
  longlong lVar13;
  longlong lVar14;
  undefined8 *puVar15;
  ulonglong uVar16;
  longlong lVar17;
  longlong lVar18;
  int iVar19;
  char *pcVar20;
  longlong lVar21;
  int iVar22;
  int iVar23;
  undefined8 uVar24;
  ulonglong uVar25;
  int iVar26;
  undefined8 in_R9;
  ulonglong uVar27;
  longlong lVar28;
  int iVar29;
  int iVar30;
  int iVar31;
  int iVar32;
  float fVar33;
  int local_res10;
  undefined8 in_stack_fffffffffffffb88;
  uint uVar34;
  undefined4 local_458;
  byte local_454;
  undefined2 local_453;
  char local_451;
  undefined1 local_450 [504];
  undefined4 local_258;
  undefined1 local_254;
  undefined1 local_253;
  undefined1 local_252;
  undefined1 local_251;
  undefined1 local_250 [536];
  
  DAT_180020528 = 0;
  _DAT_18001f4b8 = 0;
  DAT_1800205a0 = calloc(0x400,1);
  DAT_180020560 = calloc(0x400,1);
  uVar8 = *(undefined4 *)(DAT_180020530 + 0xf84);
  memset(local_450,0,0x1f8);
  puVar12 = local_250;
  memset(puVar12,0,0x1f8);
  iVar22 = 3;
  local_454 = DAT_180020520 & 3;
  local_251 = (undefined1)uVar8;
  local_458 = 0x7f7f7f;
  local_451 = ((char)_DAT_1800205bc + -1) * '\b';
  local_254 = (undefined1)((uint)uVar8 >> 0x18);
  local_253 = (undefined1)((uint)uVar8 >> 0x10);
  iVar19 = 3;
  local_252 = (undefined1)((uint)uVar8 >> 8);
  local_453 = 0;
  local_258 = 0x27f7f7f;
  do {
    FUN_18000f7b0(puVar12,(undefined8 *)&local_458);
    iVar19 = iVar19 + -1;
  } while (iVar19 != 0);
  Sleep(10);
  uVar8 = *(undefined4 *)(DAT_180020530 + 0x328);
  memset(local_450,0,0x1f8);
  puVar12 = local_250;
  uVar24 = 0x1f8;
  memset(puVar12,0,0x1f8);
  local_454 = DAT_180020520 & 3;
  local_458 = 0x7f7f7f;
  local_453 = 0;
  local_451 = ((char)_DAT_1800205bc + -1) * '\b';
  local_254 = (undefined1)((uint)uVar8 >> 0x18);
  local_253 = (undefined1)((uint)uVar8 >> 0x10);
  local_252 = (undefined1)((uint)uVar8 >> 8);
  local_258 = 0x47f7f7f;
  local_251 = (undefined1)uVar8;
  do {
    puVar15 = (undefined8 *)&local_458;
    FUN_18000f7b0(puVar12,puVar15);
    uVar34 = (uint)((ulonglong)in_stack_fffffffffffffb88 >> 0x20);
    iVar22 = iVar22 + -1;
  } while (iVar22 != 0);
  Sleep(10);
  FUN_180007660("iot: main loop starting\n",puVar15,uVar24,in_R9);
  pFVar9 = (FILE *)__acrt_iob_func(1);
  fflush(pFVar9);
  uVar24 = WSACreateEvent();
  *(undefined8 *)(DAT_180020530 + 0x1e8) = uVar24;
  WSAEventSelect(DAT_180020598,uVar24,1);
  GetLocalTime((LPSYSTEMTIME)&DAT_180020540);
  uVar27 = (ulonglong)DAT_180020548;
  uVar25 = (ulonglong)DAT_180020546;
  uVar16 = (ulonglong)DAT_180020542;
  pvVar10 = (void *)((ulonglong)uVar34 << 0x20);
  FUN_180007660("(%02d/%02d %02d:%02d:%02d:%03d) ",uVar16,uVar25,uVar27);
  FUN_180007660("- MetisReadThreadMainLoop()\n",uVar16,uVar25,uVar27);
  pFVar9 = (FILE *)__acrt_iob_func(1);
  fflush(pFVar9);
  fVar7 = DAT_180018d80;
  dVar6 = DAT_180018d78;
  do {
    do {
      while( true ) {
        if (DAT_180020514 == 0) {
          return;
        }
        pvVar10 = (void *)((ulonglong)pvVar10 & 0xffffffff00000000);
        uVar24 = 0xffffffff;
        if (*(int *)(DAT_180020530 + 0x44) != 0) {
          uVar24 = 3000;
        }
        iVar19 = WSAWaitForMultipleEvents(1,DAT_180020530 + 0x1e8,0,uVar24,pvVar10);
        _Memory = DAT_180020508;
        if ((iVar19 != -1) && (iVar19 != 0x102)) break;
        DAT_1800205c8 = 0;
        if (DAT_180020508 != (void *)0x0) {
          DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)DAT_180020508 + 0x48));
          free(*(void **)((longlong)_Memory + 0x18));
          free(*(void **)((longlong)_Memory + 0x10));
          free(*(void **)((longlong)_Memory + 0x40));
          free(_Memory);
        }
        DAT_180020508 = (void *)0x0;
      }
      lVar28 = DAT_180020530 + 0x1f0;
      WSAEnumNetworkEvents(DAT_180020598,*(undefined8 *)(DAT_180020530 + 0x1e8));
    } while ((*(byte *)(DAT_180020530 + 0x1f0) & 1) == 0);
    if (*(uint *)(DAT_180020530 + 500) != 0) {
      FUN_180007660("FD_READ failed with error %d\n",(ulonglong)*(uint *)(DAT_180020530 + 500),
                    lVar28,uVar24);
      return;
    }
    FUN_18000f420(DAT_1800205a0);
    local_res10 = 0;
    do {
      lVar28 = DAT_180020530;
      pcVar20 = (char *)((longlong)local_res10 + (longlong)DAT_1800205a0);
      if (((*pcVar20 == '\x7f') && (pcVar20[1] == '\x7f')) && (pcVar20[2] == '\x7f')) {
        bVar1 = pcVar20[3];
        bVar2 = pcVar20[4];
        bVar3 = pcVar20[5];
        bVar4 = pcVar20[6];
        cVar5 = pcVar20[7];
        DAT_180020558 = bVar1;
        DAT_180020559 = bVar2;
        DAT_18002055a = bVar3;
        DAT_18002055b = bVar4;
        DAT_18002055c = cVar5;
        *(uint *)(DAT_180020530 + 0x54) = bVar1 & 1;
        *(undefined8 *)(lVar28 + 0x58) = 0;
        bVar11 = bVar1 & 0xf8;
        if ((bVar1 & 0xf8) == 0) {
          *(uint *)(lVar28 + 600) = (uint)((bVar2 & 1) != 0 || *(int *)(lVar28 + 600) != 0);
          *(uint *)(lVar28 + 0x80) = bVar2 >> 1 & 0xf;
          fVar33 = DAT_1800205a8;
        }
        else if (bVar11 == 8) {
          *(uint *)(lVar28 + 4000) = (uint)CONCAT11(bVar2,bVar3);
          *(uint *)(lVar28 + 0xfa4) = (uint)CONCAT11(bVar4,cVar5);
          fVar33 = (float)CONCAT11(bVar4,cVar5);
          if (fVar33 <= DAT_1800205a8) {
            fVar33 = DAT_1800205a8 * fVar7;
          }
        }
        else if (bVar11 == 0x10) {
          *(uint *)(lVar28 + 0xfa8) = (uint)CONCAT11(bVar2,bVar3);
          fVar33 = (float)CONCAT11(bVar2,bVar3);
          if (fVar33 <= DAT_1800205c4) {
            fVar33 = DAT_1800205c4 * fVar7;
          }
          DAT_1800205c4 = fVar33;
          *(uint *)(lVar28 + 0x70) = (uint)CONCAT11(bVar4,cVar5);
          fVar33 = DAT_1800205a8;
        }
        else if (bVar11 == 0x18) {
          *(uint *)(lVar28 + 0x74) = (uint)CONCAT11(bVar2,bVar3);
          *(uint *)(lVar28 + 0x6c) = (uint)CONCAT11(bVar4,cVar5);
          fVar33 = DAT_1800205a8;
        }
        else {
          fVar33 = DAT_1800205a8;
          if (bVar11 == 0x20) {
            if ((*(int *)(lVar28 + 600) == 0) && ((bVar2 & 1) == 0)) {
              uVar8 = 0;
            }
            else {
              uVar8 = 1;
            }
            *(undefined4 *)(lVar28 + 600) = uVar8;
            if ((*(int *)(lVar28 + 0x290) == 0) && ((bVar3 & 1) == 0)) {
              uVar8 = 0;
            }
            else {
              uVar8 = 1;
            }
            *(undefined4 *)(lVar28 + 0x290) = uVar8;
            if ((*(int *)(lVar28 + 0x2c8) == 0) && ((bVar4 & 1) == 0)) {
              *(undefined4 *)(lVar28 + 0x2c8) = 0;
              fVar33 = DAT_1800205a8;
            }
            else {
              *(undefined4 *)(lVar28 + 0x2c8) = 1;
              fVar33 = DAT_1800205a8;
            }
          }
        }
        DAT_1800205a8 = fVar33;
        iVar22 = _DAT_1800205bc * 6 + 2;
        iVar30 = (int)(0x1f8 / (longlong)iVar22);
        iVar19 = _DAT_1800205bc;
        if (0 < _DAT_1800205bc) {
          iVar31 = 0;
          iVar32 = 0;
          do {
            iVar29 = 0;
            if (iVar30 < 4) {
              if (0 < iVar30) {
                lVar21 = *(longlong *)(*(longlong *)(lVar28 + 8) + (longlong)iVar31 * 8);
                goto LAB_180010261;
              }
            }
            else {
              lVar21 = *(longlong *)(*(longlong *)(lVar28 + 8) + (longlong)iVar31 * 8);
              do {
                lVar28 = (longlong)(iVar29 * 2);
                iVar26 = iVar22 * iVar29 + iVar32;
                *(double *)(lVar21 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[(longlong)iVar26 + 8],
                                                           pcVar20[iVar26 + 9]),pcVar20[iVar26 + 10]
                                                 ) << 8) * dVar6;
                iVar19 = iVar22 * (iVar29 + 1) + iVar32;
                *(double *)(lVar21 + 8 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[iVar26 + 0xb],
                                                           pcVar20[iVar26 + 0xc]),
                                                  pcVar20[iVar26 + 0xd]) << 8) * dVar6;
                *(double *)(lVar21 + 0x10 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[(longlong)iVar19 + 8],
                                                           pcVar20[iVar19 + 9]),pcVar20[iVar19 + 10]
                                                 ) << 8) * dVar6;
                *(double *)(lVar21 + 0x18 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[iVar19 + 0xb],
                                                           pcVar20[iVar19 + 0xc]),
                                                  pcVar20[iVar19 + 0xd]) << 8) * dVar6;
                iVar19 = iVar22 * (iVar29 + 2) + iVar32;
                *(double *)(lVar21 + (longlong)((iVar29 + 2) * 2) * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[(longlong)iVar19 + 8],
                                                           pcVar20[iVar19 + 9]),pcVar20[iVar19 + 10]
                                                 ) << 8) * dVar6;
                iVar26 = iVar22 * (iVar29 + 3) + iVar32;
                *(double *)(lVar21 + 0x28 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[iVar19 + 0xb],
                                                           pcVar20[iVar19 + 0xc]),
                                                  pcVar20[iVar19 + 0xd]) << 8) * dVar6;
                *(double *)(lVar21 + (longlong)((iVar29 + 3) * 2) * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[(longlong)iVar26 + 8],
                                                           pcVar20[iVar26 + 9]),pcVar20[iVar26 + 10]
                                                 ) << 8) * dVar6;
                iVar29 = iVar29 + 4;
                *(double *)(lVar21 + 0x38 + lVar28 * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[iVar26 + 0xb],
                                                           pcVar20[iVar26 + 0xc]),
                                                  pcVar20[iVar26 + 0xd]) << 8) * dVar6;
                lVar28 = DAT_180020530;
                iVar19 = _DAT_1800205bc;
              } while (iVar29 < iVar30 + -3);
              for (; DAT_180020530 = lVar28, iVar29 < iVar30; iVar29 = iVar29 + 1) {
LAB_180010261:
                iVar26 = iVar22 * iVar29 + iVar32;
                *(double *)(lVar21 + (longlong)(iVar29 * 2) * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[(longlong)iVar26 + 8],
                                                           pcVar20[iVar26 + 9]),pcVar20[iVar26 + 10]
                                                 ) << 8) * dVar6;
                *(double *)(lVar21 + 8 + (longlong)(iVar29 * 2) * 8) =
                     (double)(int)((uint)CONCAT21(CONCAT11(pcVar20[iVar26 + 0xb],
                                                           pcVar20[iVar26 + 0xc]),
                                                  pcVar20[iVar26 + 0xd]) << 8) * dVar6;
                lVar28 = DAT_180020530;
              }
            }
            iVar31 = iVar31 + 1;
            iVar32 = iVar32 + 6;
          } while (iVar31 < iVar19);
        }
        if (iVar19 == 2) {
          iVar22 = iVar30 * 2;
          if (0 < iVar22) {
            iVar19 = 0;
            lVar21 = *(longlong *)(lVar28 + 0x10);
            iVar31 = 0;
            lVar13 = **(longlong **)(lVar28 + 8);
            lVar17 = (*(longlong **)(lVar28 + 8))[1];
            do {
              lVar14 = (longlong)iVar19;
              iVar19 = iVar19 + 2;
              lVar18 = (longlong)iVar31;
              iVar31 = iVar31 + 4;
              *(undefined8 *)(lVar21 + lVar18 * 8) = *(undefined8 *)(lVar13 + lVar14 * 8);
              *(undefined8 *)(lVar21 + 8 + lVar18 * 8) = *(undefined8 *)(lVar13 + 8 + lVar14 * 8);
              *(undefined8 *)(lVar21 + 0x10 + lVar18 * 8) = *(undefined8 *)(lVar17 + lVar14 * 8);
              *(undefined8 *)(lVar21 + 0x18 + lVar18 * 8) = *(undefined8 *)(lVar17 + 8 + lVar14 * 8)
              ;
            } while (iVar19 < iVar22);
          }
          pvVar10 = *(void **)(lVar28 + 0x10);
          iVar19 = 0;
LAB_18001056f:
          xrouter(0,0,iVar19,iVar22,pvVar10);
          lVar28 = DAT_180020530;
          iVar19 = _DAT_1800205bc;
        }
        else {
          iVar22 = iVar30;
          if (iVar19 == 4) {
            xrouter(0,0,0,iVar30,(void *)**(undefined8 **)(lVar28 + 8));
            lVar28 = DAT_180020530;
            iVar19 = iVar30 * 2;
            if (0 < iVar19) {
              iVar31 = 0;
              lVar21 = *(longlong *)(DAT_180020530 + 0x10);
              iVar32 = 0;
              lVar13 = *(longlong *)(*(longlong *)(DAT_180020530 + 8) + 0x10);
              lVar17 = *(longlong *)(*(longlong *)(DAT_180020530 + 8) + 0x18);
              do {
                lVar14 = (longlong)iVar31;
                iVar31 = iVar31 + 2;
                lVar18 = (longlong)iVar32;
                iVar32 = iVar32 + 4;
                *(undefined8 *)(lVar21 + lVar18 * 8) = *(undefined8 *)(lVar13 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 8 + lVar18 * 8) = *(undefined8 *)(lVar13 + 8 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 0x10 + lVar18 * 8) = *(undefined8 *)(lVar17 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 0x18 + lVar18 * 8) =
                     *(undefined8 *)(lVar17 + 8 + lVar14 * 8);
              } while (iVar31 < iVar19);
            }
            xrouter(0,0,1,iVar19,*(void **)(lVar28 + 0x10));
            iVar19 = 2;
            pvVar10 = *(void **)(*(longlong *)(DAT_180020530 + 8) + 8);
            goto LAB_18001056f;
          }
          if (iVar19 == 5) {
            iVar19 = iVar30 * 2;
            iVar31 = 0;
            if (iVar19 < 1) {
              xrouter(0,0,0,iVar19,*(void **)(lVar28 + 0x10));
              lVar28 = DAT_180020530;
            }
            else {
              iVar32 = 0;
              pvVar10 = *(void **)(lVar28 + 0x10);
              lVar21 = **(longlong **)(lVar28 + 8);
              lVar28 = (*(longlong **)(lVar28 + 8))[1];
              do {
                lVar13 = (longlong)iVar31;
                iVar31 = iVar31 + 2;
                lVar17 = (longlong)iVar32;
                iVar32 = iVar32 + 4;
                *(undefined8 *)((longlong)pvVar10 + lVar17 * 8) =
                     *(undefined8 *)(lVar21 + lVar13 * 8);
                *(undefined8 *)((longlong)pvVar10 + lVar17 * 8 + 8) =
                     *(undefined8 *)(lVar21 + 8 + lVar13 * 8);
                *(undefined8 *)((longlong)pvVar10 + lVar17 * 8 + 0x10) =
                     *(undefined8 *)(lVar28 + lVar13 * 8);
                *(undefined8 *)((longlong)pvVar10 + lVar17 * 8 + 0x18) =
                     *(undefined8 *)(lVar28 + 8 + lVar13 * 8);
              } while (iVar31 < iVar19);
              xrouter(0,0,0,iVar19,pvVar10);
              lVar28 = DAT_180020530;
              iVar31 = 0;
              iVar32 = 0;
              lVar21 = *(longlong *)(DAT_180020530 + 0x10);
              lVar13 = *(longlong *)(*(longlong *)(DAT_180020530 + 8) + 0x18);
              lVar17 = *(longlong *)(*(longlong *)(DAT_180020530 + 8) + 0x20);
              do {
                lVar14 = (longlong)iVar31;
                iVar31 = iVar31 + 2;
                lVar18 = (longlong)iVar32;
                iVar32 = iVar32 + 4;
                *(undefined8 *)(lVar21 + lVar18 * 8) = *(undefined8 *)(lVar13 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 8 + lVar18 * 8) = *(undefined8 *)(lVar13 + 8 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 0x10 + lVar18 * 8) = *(undefined8 *)(lVar17 + lVar14 * 8);
                *(undefined8 *)(lVar21 + 0x18 + lVar18 * 8) =
                     *(undefined8 *)(lVar17 + 8 + lVar14 * 8);
              } while (iVar31 < iVar19);
            }
            xrouter(0,0,1,iVar19,*(void **)(lVar28 + 0x10));
            iVar19 = 2;
            pvVar10 = *(void **)(*(longlong *)(DAT_180020530 + 8) + 0x10);
            goto LAB_18001056f;
          }
        }
        iVar22 = DAT_180020550;
        iVar29 = 0;
        iVar26 = 0;
        iVar32 = 0;
        iVar31 = DAT_180020528;
        if (iVar30 < 4) {
          if (0 < iVar30) goto LAB_18001075e;
        }
        else {
          do {
            iVar31 = iVar31 + 1;
            if (iVar31 == iVar22) {
              lVar21 = *(longlong *)(lVar28 + 0x18);
              iVar32 = iVar29 * 2;
              iVar29 = iVar29 + 1;
              iVar23 = (iVar19 * 6 + 2) * iVar26 + iVar19 * 6;
              iVar31 = 0;
              *(double *)(lVar21 + (longlong)iVar32 * 8) =
                   (double)(int)((uint)CONCAT11(pcVar20[(longlong)iVar23 + 8],pcVar20[iVar23 + 9])
                                << 0x10) * dVar6;
              *(undefined8 *)(lVar21 + 8 + (longlong)iVar32 * 8) = 0;
            }
            if (iVar31 + 1 == iVar22) {
              lVar21 = *(longlong *)(lVar28 + 0x18);
              iVar31 = 1;
              iVar32 = iVar29 * 2;
              iVar23 = (iVar19 * 6 + 2) * (iVar26 + 1) + iVar19 * 6;
              iVar29 = iVar29 + 1;
              *(double *)(lVar21 + (longlong)iVar32 * 8) =
                   (double)(int)((uint)CONCAT11(pcVar20[(longlong)iVar23 + 8],pcVar20[iVar23 + 9])
                                << 0x10) * dVar6;
              *(undefined8 *)(lVar21 + 8 + (longlong)iVar32 * 8) = 0;
            }
            else {
              iVar31 = iVar31 + 2;
            }
            if (iVar31 == iVar22) {
              lVar21 = *(longlong *)(lVar28 + 0x18);
              iVar32 = iVar29 * 2;
              iVar29 = iVar29 + 1;
              iVar23 = (iVar19 * 6 + 2) * (iVar26 + 2) + iVar19 * 6;
              iVar31 = 0;
              *(double *)(lVar21 + (longlong)iVar32 * 8) =
                   (double)(int)((uint)CONCAT11(pcVar20[(longlong)iVar23 + 8],pcVar20[iVar23 + 9])
                                << 0x10) * dVar6;
              *(undefined8 *)(lVar21 + 8 + (longlong)iVar32 * 8) = 0;
            }
            iVar31 = iVar31 + 1;
            DAT_180020528 = iVar31;
            if (iVar31 == iVar22) {
              lVar21 = *(longlong *)(lVar28 + 0x18);
              iVar31 = 0;
              DAT_180020528 = 0;
              iVar32 = iVar29 * 2;
              iVar23 = (iVar19 * 6 + 2) * (iVar26 + 3) + iVar19 * 6;
              iVar29 = iVar29 + 1;
              *(double *)(lVar21 + (longlong)iVar32 * 8) =
                   (double)(int)((uint)CONCAT11(pcVar20[(longlong)iVar23 + 8],pcVar20[iVar23 + 9])
                                << 0x10) * dVar6;
              *(undefined8 *)(lVar21 + 8 + (longlong)iVar32 * 8) = 0;
            }
            iVar26 = iVar26 + 4;
            iVar32 = iVar26;
          } while (iVar26 < iVar30 + -3);
          for (; iVar32 < iVar30; iVar32 = iVar32 + 1) {
LAB_18001075e:
            iVar31 = iVar31 + 1;
            DAT_180020528 = iVar31;
            if (iVar31 == iVar22) {
              lVar21 = *(longlong *)(lVar28 + 0x18);
              iVar31 = 0;
              DAT_180020528 = 0;
              iVar26 = iVar29 * 2;
              iVar23 = (iVar19 * 6 + 2) * iVar32 + iVar19 * 6;
              iVar29 = iVar29 + 1;
              *(double *)(lVar21 + (longlong)iVar26 * 8) =
                   (double)(int)((uint)CONCAT11(pcVar20[(longlong)iVar23 + 8],pcVar20[iVar23 + 9])
                                << 0x10) * dVar6;
              *(undefined8 *)(lVar21 + 8 + (longlong)iVar26 * 8) = 0;
            }
          }
        }
        Inbound(*(uint *)(PTR_DAT_18001e080 + 4),iVar29,*(void **)(lVar28 + 0x18));
      }
      local_res10 = local_res10 + 0x200;
    } while (local_res10 != 0x400);
  } while( true );
}



/* ========================================================================
   ENTRY: 1800108c0
   NAME : FUN_1800108c0
   SIG  : undefined __fastcall FUN_1800108c0(undefined8 * param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800108c0(undefined8 *param_1)

{
  undefined8 uVar1;
  undefined8 *puVar2;
  int iVar3;
  int iVar4;
  undefined8 *puVar5;
  undefined8 *puVar6;
  byte bVar7;
  uint uVar8;
  undefined8 *puVar9;
  undefined8 *puVar10;
  byte bVar11;
  uint uVar12;
  longlong lVar13;
  uint uVar14;
  byte bVar15;
  uint uVar16;
  uint uVar17;
  byte bVar18;
  longlong lVar19;
  int iVar20;
  bool bVar21;
  
  iVar4 = _DAT_1800205bc;
  iVar3 = DAT_180020574;
  puVar2 = DAT_180020560;
  lVar13 = DAT_180020530;
  bVar7 = DAT_180020520 & 3;
  uVar16 = 0;
  uVar14 = 0;
  uVar12 = 0;
  uVar17 = 0;
  uVar8 = (int)_DAT_180020524 >> 8;
  bVar21 = DAT_180020584 == 0xd;
  iVar20 = 0;
  do {
    lVar19 = (longlong)iVar20;
    *(undefined2 *)(lVar19 + (longlong)puVar2) = 0x7f7f;
    *(undefined1 *)(lVar19 + 2 + (longlong)puVar2) = 0x7f;
    if ((iVar3 != DAT_18001f4b4) && (DAT_18001f4b4 = iVar3, iVar4 == 2)) {
      DAT_18001f4bc = 2;
    }
    bVar18 = (byte)iVar3;
    switch(DAT_18001f4bc) {
    case 0:
      bVar15 = *(byte *)(DAT_1800205b0 + 5);
      uVar14 = (uint)(byte)(*(byte *)(lVar13 + 0x30c) & 1 | *(char *)(lVar13 + 100) * '\x02');
      uVar16 = (uint)bVar7;
      bVar11 = (((*(byte *)(lVar13 + 0x260) & 1 | bVar15 & 0xf8) * '\x02' |
                *(byte *)(lVar13 + 0x25c) & 1) * '\x02' | *(byte *)(lVar13 + 0x33c) & 1) << 2 |
               (bVar15 >> 2 & 0x10 | bVar15 & 0x20) >> 4;
      uVar12 = (uint)bVar11;
      if ((bVar15 & 1) == 0) {
        if ((bVar15 & 4) == 0) {
          if ((bVar15 & 2) != 0) {
            uVar12 = (uint)(bVar11 | 0x40);
          }
        }
        else {
          uVar12 = (uint)(bVar11 | 0x20);
        }
      }
      else {
        uVar12 = (uint)(bVar11 | 0x60);
      }
      if ((*(byte *)(DAT_1800205b0 + 7) & 4) == 0) {
        bVar15 = (*(byte *)(DAT_1800205b0 + 7) & 2 | 8) >> 1;
      }
      else {
        bVar15 = 6;
      }
      uVar17 = (uint)(byte)((DAT_1800205c0 << 4 | (char)iVar4 - 1U) << 3 | bVar15);
      break;
    case 1:
      bVar18 = bVar18 | 2;
LAB_180010a62:
      uVar17 = *(uint *)(lVar13 + 0xf84);
      goto LAB_180010a69;
    case 2:
      bVar18 = bVar18 | 4;
      if (((iVar4 == 2) && (iVar3 == 1)) && (*(int *)(lVar13 + 0x220) != 0)) goto LAB_180010a62;
LAB_180010ac5:
      uVar17 = *(uint *)(lVar13 + 0x328);
      goto LAB_180010a69;
    case 3:
      bVar18 = bVar18 | 6;
      if (iVar4 == 2) {
        if ((iVar3 == 1) && (*(int *)(lVar13 + 0x220) != 0)) goto LAB_180010a62;
      }
      else if (iVar4 == 5) goto LAB_180010ac5;
      uVar17 = *(uint *)(lVar13 + 0x430);
      goto LAB_180010a69;
    case 4:
      bVar18 = bVar18 | 0x1c;
      uVar16 = _DAT_180020524 & 0xff;
      uVar12 = (uint)(*(byte *)(lVar13 + 0x250) & 0x1f);
      uVar14 = uVar8 & 0xff;
      goto LAB_180010e08;
    case 5:
      bVar18 = bVar18 | 8;
      if (iVar4 != 5) goto LAB_180010a62;
      uVar17 = *(uint *)(lVar13 + 0x640);
LAB_180010a69:
      uVar16 = (int)uVar17 >> 0x18;
      uVar14 = (int)uVar17 >> 0x10;
      uVar12 = (int)uVar17 >> 8;
      break;
    case 6:
      bVar18 = bVar18 | 10;
      goto LAB_180010b0f;
    case 7:
      bVar18 = bVar18 | 0xc;
LAB_180010b0f:
      uVar12 = *(uint *)(lVar13 + 0xf84);
      goto LAB_180010b16;
    case 8:
      uVar12 = *(uint *)(lVar13 + 0x328);
      bVar18 = bVar18 | 0xe;
      goto LAB_180010b16;
    case 9:
      uVar12 = *(uint *)(lVar13 + 0x328);
      bVar18 = bVar18 | 0x10;
LAB_180010b16:
      uVar16 = (int)uVar12 >> 0x18;
      uVar14 = (int)uVar12 >> 0x10;
      uVar17 = uVar12 & 0xff;
      uVar12 = (int)uVar12 >> 8;
      break;
    case 10:
      bVar18 = bVar18 | 0x12;
      bVar15 = *(byte *)(DAT_1800205b0 + 4);
      uVar16 = (uint)*(byte *)(lVar13 + 0xf9c);
      uVar14 = (uint)(byte)((DAT_18002051c | DAT_180020590 | DAT_18002057c | DAT_1800205d8) & 0x3f |
                            (*(byte *)(lVar13 + 0x314) & 1 | 0x20) * '\x02' |
                           *(byte *)(lVar13 + 0x314) >> 1 & 1);
      uVar12 = (uint)(byte)(((*(char *)(lVar13 + 0xfb8) << 4 | bVar15 & 8) << 2 |
                            *(byte *)(DAT_1800205b0 + 5) & 0x10) * '\x02' |
                           (bVar15 >> 1 & 0x38 | bVar15 & 6) >> 1);
      uVar17 = (uint)((byte)(*(byte *)(DAT_1800205b0 + 6) >> 3 | *(byte *)(DAT_1800205b0 + 7) & 0xe1
                            ) >> 1);
      break;
    case 0xb:
      bVar18 = bVar18 | 0x14;
      bVar15 = *(byte *)(lVar13 + 0x33c) & 1;
      uVar14 = (uint)(byte)((*(byte *)(lVar13 + 0x220) & 1) << 6 | *(byte *)(lVar13 + 0x310) & 0x1f)
      ;
      uVar16 = (uint)(byte)(((((*(byte *)(lVar13 + 0x314) & 4) * '\x02' | bVar15) * '\x02' |
                             *(byte *)(lVar13 + 0x54c) & 1) * '\x02' | *(byte *)(lVar13 + 0x444) & 1
                            | *(byte *)(lVar13 + 0x314) & 0x18) * '\x02' | bVar15);
      uVar12 = (uint)(*(byte *)(lVar13 + 0x84) & 0xf);
      uVar17 = (uint)(*(byte *)(lVar13 + 0x24c) & 0x1f | 0x20);
      break;
    case 0xc:
      bVar18 = bVar18 | 0x16;
      if (iVar3 == 0) {
        if (DAT_180020584 == 0xf) goto LAB_180010c95;
        bVar15 = *(byte *)(lVar13 + 0x284);
      }
      else if (DAT_180020584 == 0xf) {
LAB_180010c95:
        bVar15 = *(byte *)(lVar13 + 0x284) & 0x1f;
      }
      else {
        bVar15 = 0x1f;
      }
      bVar11 = *(byte *)(lVar13 + 0x30c);
      uVar16 = (uint)(bVar15 | 0x20);
      uVar14 = (uint)(byte)((bVar11 & 4 | 2) << 4 | *(byte *)(lVar13 + 700) & 0x1f);
      if ((bVar11 & 8) == 0) {
        bVar15 = 0;
      }
      else {
        bVar15 = 0x80;
        if ((bVar11 & 0x20) == 0) {
          bVar15 = 0x40;
        }
      }
      uVar12 = (uint)(*(byte *)(lVar13 + 0x2f8) & 0x3f | bVar15);
      uVar17 = (uint)(byte)((bVar11 & 0xc0) * '\x02' | *(byte *)(lVar13 + 0x2fc) & 0x7f);
      break;
    case 0xd:
      bVar18 = bVar18 | 0x1e;
      uVar14 = (uint)*(byte *)(lVar13 + 0x2f0);
      uVar12 = (uint)*(byte *)(lVar13 + 0x304);
      uVar16 = (uint)(*(byte *)(lVar13 + 0x30c) >> 1 & 1);
      goto LAB_180010e08;
    case 0xe:
      bVar18 = bVar18 | 0x20;
      uVar14 = (uint)((byte)*(int *)(lVar13 + 0x300) & 3);
      uVar16 = *(int *)(lVar13 + 0x300) >> 2;
      uVar12 = *(int *)(lVar13 + 0x2f4) >> 4;
      uVar17 = (uint)((byte)*(int *)(lVar13 + 0x2f4) & 0xf);
      break;
    case 0xf:
      bVar18 = bVar18 | 0x22;
      uVar14 = (uint)((byte)*(int *)(lVar13 + 0xfb4) & 3);
      uVar16 = *(int *)(lVar13 + 0xfb4) >> 2;
      uVar12 = *(int *)(lVar13 + 0xfb0) >> 2;
      uVar17 = (uint)((byte)*(int *)(lVar13 + 0xfb0) & 3);
      break;
    case 0x10:
      bVar18 = bVar18 | 0x24;
      bVar15 = *(byte *)(DAT_1800205d0 + 4);
      bVar15 = ((*(byte *)(DAT_1800205d0 + 5) << 4 | bVar15 & 8) << 2 |
               *(byte *)(DAT_1800205d0 + 5) & 0x10) * '\x02' |
               (bVar15 >> 1 & 0x38 | bVar15 & 6) >> 1;
      uVar14 = (uint)(byte)((*(byte *)(lVar13 + 0x220) & 1) << 6 | DAT_180020578 & 1);
      goto LAB_180010e06;
    case 0x11:
      bVar18 = bVar18 | 0x26;
      bVar15 = *(byte *)(lVar13 + 0x68) & 0xf;
      uVar14 = 0;
LAB_180010e06:
      uVar16 = (uint)bVar15;
      uVar12 = 0;
LAB_180010e08:
      uVar17 = 0;
    }
    *(byte *)(lVar19 + 3 + (longlong)puVar2) = bVar18;
    *(char *)(lVar19 + 4 + (longlong)puVar2) = (char)uVar16;
    *(char *)(lVar19 + 5 + (longlong)puVar2) = (char)uVar14;
    *(char *)(lVar19 + 6 + (longlong)puVar2) = (char)uVar12;
    *(char *)(lVar19 + 7 + (longlong)puVar2) = (char)uVar17;
    if (DAT_18001f4bc < (int)(bVar21 + 0x10)) {
      DAT_18001f4bc = DAT_18001f4bc + 1;
    }
    else {
      DAT_18001f4bc = 0;
    }
    iVar20 = iVar20 + 0x200;
    if (iVar20 == 0x400) {
      lVar13 = 3;
      lVar19 = 3;
      puVar5 = param_1;
      puVar6 = puVar2 + 1;
      do {
        puVar10 = puVar6;
        puVar9 = puVar5;
        uVar1 = puVar9[1];
        *puVar10 = *puVar9;
        puVar10[1] = uVar1;
        uVar1 = puVar9[3];
        puVar10[2] = puVar9[2];
        puVar10[3] = uVar1;
        uVar1 = puVar9[5];
        puVar10[4] = puVar9[4];
        puVar10[5] = uVar1;
        uVar1 = puVar9[7];
        puVar10[6] = puVar9[6];
        puVar10[7] = uVar1;
        uVar1 = puVar9[9];
        puVar10[8] = puVar9[8];
        puVar10[9] = uVar1;
        uVar1 = puVar9[0xb];
        puVar10[10] = puVar9[10];
        puVar10[0xb] = uVar1;
        uVar1 = puVar9[0xd];
        puVar10[0xc] = puVar9[0xc];
        puVar10[0xd] = uVar1;
        uVar1 = puVar9[0xf];
        puVar10[0xe] = puVar9[0xe];
        puVar10[0xf] = uVar1;
        lVar19 = lVar19 + -1;
        puVar5 = puVar9 + 0x10;
        puVar6 = puVar10 + 0x10;
      } while (lVar19 != 0);
      uVar1 = puVar9[0x11];
      puVar10[0x10] = puVar9[0x10];
      puVar10[0x11] = uVar1;
      uVar1 = puVar9[0x13];
      puVar10[0x12] = puVar9[0x12];
      puVar10[0x13] = uVar1;
      uVar1 = puVar9[0x15];
      puVar10[0x14] = puVar9[0x14];
      puVar10[0x15] = uVar1;
      uVar1 = puVar9[0x17];
      puVar10[0x16] = puVar9[0x16];
      puVar10[0x17] = uVar1;
      uVar1 = puVar9[0x19];
      puVar10[0x18] = puVar9[0x18];
      puVar10[0x19] = uVar1;
      uVar1 = puVar9[0x1b];
      puVar10[0x1a] = puVar9[0x1a];
      puVar10[0x1b] = uVar1;
      uVar1 = puVar9[0x1d];
      puVar10[0x1c] = puVar9[0x1c];
      puVar10[0x1d] = uVar1;
      puVar10[0x1e] = puVar9[0x1e];
      puVar5 = param_1 + 0x3f;
      puVar6 = puVar2 + 0x41;
      do {
        puVar10 = puVar6;
        puVar9 = puVar5;
        uVar1 = puVar9[1];
        *puVar10 = *puVar9;
        puVar10[1] = uVar1;
        uVar1 = puVar9[3];
        puVar10[2] = puVar9[2];
        puVar10[3] = uVar1;
        uVar1 = puVar9[5];
        puVar10[4] = puVar9[4];
        puVar10[5] = uVar1;
        uVar1 = puVar9[7];
        puVar10[6] = puVar9[6];
        puVar10[7] = uVar1;
        uVar1 = puVar9[9];
        puVar10[8] = puVar9[8];
        puVar10[9] = uVar1;
        uVar1 = puVar9[0xb];
        puVar10[10] = puVar9[10];
        puVar10[0xb] = uVar1;
        uVar1 = puVar9[0xd];
        puVar10[0xc] = puVar9[0xc];
        puVar10[0xd] = uVar1;
        uVar1 = puVar9[0xf];
        puVar10[0xe] = puVar9[0xe];
        puVar10[0xf] = uVar1;
        lVar13 = lVar13 + -1;
        puVar5 = puVar9 + 0x10;
        puVar6 = puVar10 + 0x10;
      } while (lVar13 != 0);
      uVar1 = puVar9[0x11];
      puVar10[0x10] = puVar9[0x10];
      puVar10[0x11] = uVar1;
      uVar1 = puVar9[0x13];
      puVar10[0x12] = puVar9[0x12];
      puVar10[0x13] = uVar1;
      uVar1 = puVar9[0x15];
      puVar10[0x14] = puVar9[0x14];
      puVar10[0x15] = uVar1;
      uVar1 = puVar9[0x17];
      puVar10[0x16] = puVar9[0x16];
      puVar10[0x17] = uVar1;
      uVar1 = puVar9[0x19];
      puVar10[0x18] = puVar9[0x18];
      puVar10[0x19] = uVar1;
      uVar1 = puVar9[0x1b];
      puVar10[0x1a] = puVar9[0x1a];
      puVar10[0x1b] = uVar1;
      uVar1 = puVar9[0x1d];
      puVar10[0x1c] = puVar9[0x1c];
      puVar10[0x1d] = uVar1;
      uVar1 = puVar9[0x1e];
      puVar10[0x1e] = uVar1;
      FUN_18000f7b0(uVar1,puVar2);
      ReleaseSemaphore(*(HANDLE *)(DAT_180020530 + 0xd0),1,(LPLONG)0x0);
                    /* WARNING: Could not recover jumptable at 0x000180010ffa. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      ReleaseSemaphore(*(HANDLE *)(DAT_180020530 + 0xd8),1,(LPLONG)0x0);
      return;
    }
  } while( true );
}



/* ========================================================================
   ENTRY: 180011050
   NAME : FUN_180011050
   SIG  : undefined8 __fastcall FUN_180011050(void)
   ======================================================================== */

undefined8 FUN_180011050(void)

{
  undefined8 uVar1;
  longlong lVar2;
  double dVar3;
  double dVar4;
  longlong lVar5;
  HANDLE pvVar6;
  longlong lVar7;
  ushort uVar8;
  longlong lVar9;
  int iVar10;
  ulonglong uVar11;
  int iVar12;
  int iVar13;
  double dVar14;
  double dVar15;
  DWORD local_res10 [2];
  longlong local_68 [8];
  
  local_res10[0] = 0;
  pvVar6 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res10);
  if (pvVar6 == (HANDLE)0x0) {
    pvVar6 = GetCurrentThread();
    SetThreadPriority(pvVar6,2);
  }
  else {
    AvSetMmThreadPriority(pvVar6,AVRT_PRIORITY_CRITICAL);
  }
  dVar4 = DAT_180018e38;
  dVar3 = DAT_180018db0;
  lVar2 = *(longlong *)(DAT_180020530 + 0x30);
  local_68[1] = *(undefined8 *)(DAT_180020530 + 0x38);
  local_68[0] = lVar2;
  do {
    if (DAT_180020514 == 0) {
      return 0;
    }
    WaitForMultipleObjects(2,(HANDLE *)(DAT_180020530 + 0xc0),1,0xffffffff);
    iVar13 = DAT_180020574;
    lVar5 = DAT_180020530;
    if (**(int **)(PTR_DAT_18001e080 + 0x1128) == 0) {
LAB_18001116f:
      if (iVar13 == 0) goto LAB_180011173;
    }
    else {
      if (DAT_180020574 != 0) {
        memcpy(*(void **)(DAT_180020530 + 0x30),
               (void *)(*(longlong *)(DAT_180020530 + 0x38) + 0x800),0x7e0);
        goto LAB_18001116f;
      }
LAB_180011173:
      memset(*(void **)(lVar5 + 0x38),0,0x7e0);
    }
    lVar7 = 0;
    do {
      uVar1 = *(undefined8 *)(lVar2 + lVar7 * 8);
      *(undefined8 *)(lVar2 + lVar7 * 8) = *(undefined8 *)(lVar2 + 8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x10 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x10 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x18 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x18 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x20 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x20 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x28 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x28 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x30 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x30 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x38 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x38 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x40 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x40 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x48 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x48 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x50 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x50 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x58 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x58 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x60 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x60 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x68 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x68 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x70 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x70 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x78 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x78 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x80 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x80 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x88 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x88 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x90 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x90 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x98 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x98 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xa0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xa0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0xa8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xa8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xb0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xb0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0xb8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xb8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xc0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xc0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 200 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 200 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xd0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xd0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0xd8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xd8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xe0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xe0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0xe8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xe8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0xf0 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xf0 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0xf8 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0xf8 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x100 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x100 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x108 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x108 + lVar7 * 8) = uVar1;
      uVar1 = *(undefined8 *)(lVar2 + 0x110 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x110 + lVar7 * 8) = *(undefined8 *)(lVar2 + 0x118 + lVar7 * 8);
      *(undefined8 *)(lVar2 + 0x118 + lVar7 * 8) = uVar1;
      lVar7 = lVar7 + 0x24;
    } while (lVar7 != 0xfc);
    iVar13 = 0;
    do {
      iVar12 = iVar13 * 2;
      uVar11 = 0;
      do {
        lVar7 = local_68[uVar11];
        dVar14 = *(double *)(lVar7 + (longlong)iVar12 * 8);
        dVar15 = dVar14 * dVar4;
        if (dVar14 < 0.0) {
          dVar14 = ceil(dVar15 - dVar3);
        }
        else {
          dVar14 = floor(dVar15 + dVar3);
        }
        uVar8 = (ushort)(int)dVar14;
        iVar10 = (int)uVar11;
        if (((*(byte *)(lVar5 + 0x30c) & 2) != 0) && (iVar10 == 1)) {
          uVar8 = ((*(short *)(lVar5 + 0xf94) * 2 | *(ushort *)(lVar5 + 0xf90)) * 2 |
                  *(ushort *)(lVar5 + 0xf8c)) & 7;
        }
        lVar9 = (longlong)((iVar12 + iVar10) * 4);
        *(char *)(lVar9 + *(longlong *)(lVar5 + 0x28)) = (char)(uVar8 >> 8);
        *(char *)(lVar9 + 1 + *(longlong *)(lVar5 + 0x28)) = (char)uVar8;
        dVar14 = *(double *)(lVar7 + 8 + (longlong)iVar12 * 8);
        dVar15 = dVar14 * dVar4;
        if (dVar14 < 0.0) {
          dVar14 = ceil(dVar15 - dVar3);
        }
        else {
          dVar14 = floor(dVar15 + dVar3);
        }
        uVar8 = (ushort)(int)dVar14;
        if (((*(byte *)(lVar5 + 0x30c) & 2) != 0) && (iVar10 == 1)) {
          uVar8 = ((*(short *)(lVar5 + 0xf94) * 2 | *(ushort *)(lVar5 + 0xf90)) * 2 |
                  *(ushort *)(lVar5 + 0xf8c)) & 7;
        }
        uVar11 = (ulonglong)(iVar10 + 1U);
        *(char *)(lVar9 + 2 + *(longlong *)(lVar5 + 0x28)) = (char)(uVar8 >> 8);
        *(char *)(lVar9 + 3 + *(longlong *)(lVar5 + 0x28)) = (char)uVar8;
      } while ((int)(iVar10 + 1U) < 2);
      iVar13 = iVar13 + 1;
    } while (iVar13 < 0x7e);
    FUN_1800108c0(*(undefined8 **)(lVar5 + 0x28));
  } while( true );
}



/* ========================================================================
   ENTRY: 180011560
   NAME : FUN_180011560
   SIG  : undefined __fastcall FUN_180011560(int param_1, undefined8 param_2, undefined8 param_3, int param_4)
   ======================================================================== */

void FUN_180011560(int param_1,undefined8 param_2,undefined8 param_3,int param_4)

{
  int *piVar1;
  void *pvVar2;
  HANDLE pvVar3;
  void *_ArgList;
  
  _ArgList = (void *)(longlong)param_1;
  piVar1 = calloc(1,0x98);
  *(int **)(&DAT_180020480 + (longlong)_ArgList * 8) = piVar1;
  *(int **)(&DAT_180020400 + (longlong)_ArgList * 8) = piVar1;
  *(int **)(&DAT_180020380 + (longlong)_ArgList * 8) = piVar1;
  (&DAT_180020300)[(longlong)_ArgList] = piVar1;
  *piVar1 = param_1;
  piVar1[2] = param_4;
  if (param_4 < 0x801) {
    param_4 = 0x800;
  }
  piVar1[1] = 0x800;
  piVar1[0xc] = 1;
  piVar1[0xb] = 1;
  piVar1[3] = param_4;
  piVar1[4] = param_4 * 2;
  pvVar2 = calloc((longlong)(param_4 * 2),0x10);
  *(void **)(piVar1 + 6) = pvVar2;
  piVar1[8] = 0;
  piVar1[9] = 0;
  piVar1[10] = 0;
  pvVar3 = CreateSemaphoreW((LPSECURITY_ATTRIBUTES)0x0,0,1000,(LPCWSTR)0x0);
  *(HANDLE *)(piVar1 + 0xe) = pvVar3;
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar1 + 0x1a),0x9c4);
  InitializeCriticalSectionAndSpinCount((LPCRITICAL_SECTION)(piVar1 + 0x10),0x9c4);
  pvVar2 = calloc(0x168,0x10);
  *(void **)(piVar1 + 0x24) = pvVar2;
                    /* WARNING: Could not recover jumptable at 0x000180011649. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _beginthread(FUN_180011830,0,_ArgList);
  return;
}



/* ========================================================================
   ENTRY: 180011650
   NAME : FUN_180011650
   SIG  : undefined __fastcall FUN_180011650(int param_1)
   ======================================================================== */

void FUN_180011650(int param_1)

{
  void *_Memory;
  
  if (DAT_180020300 != 0) {
    _Memory = (void *)(&DAT_180020300)[param_1];
    LOCK();
    *(uint *)((longlong)_Memory + 0x30) = *(uint *)((longlong)_Memory + 0x30) & 0xfffffffe;
    UNLOCK();
    EnterCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x68));
    EnterCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x40));
    Sleep(0x19);
    LOCK();
    *(uint *)((longlong)_Memory + 0x2c) = *(uint *)((longlong)_Memory + 0x2c) & 0xfffffffe;
    UNLOCK();
    ReleaseSemaphore(*(HANDLE *)((longlong)_Memory + 0x38),1,(LPLONG)0x0);
    LeaveCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x40));
    Sleep(2);
    DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x40));
    DeleteCriticalSection((LPCRITICAL_SECTION)((longlong)_Memory + 0x68));
    CloseHandle(*(HANDLE *)((longlong)_Memory + 0x38));
    free(*(void **)((longlong)_Memory + 0x90));
    free(*(void **)((longlong)_Memory + 0x18));
    free(_Memory);
  }
  return;
}



/* ========================================================================
   ENTRY: 180011730
   NAME : OutBound
   SIG  : undefined __fastcall OutBound(int param_1, int param_2, void * param_3)
   ======================================================================== */

void OutBound(int param_1,int param_2,void *param_3)

{
  uint uVar1;
  longlong lVar2;
  uint uVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  bool bVar7;
  
                    /* 0x11730  43  OutBound */
  lVar2 = *(longlong *)(&DAT_180020400 + (longlong)param_1 * 8);
  uVar3 = *(uint *)(lVar2 + 0x30);
  do {
    LOCK();
    uVar1 = *(uint *)(lVar2 + 0x30);
    bVar7 = uVar3 == uVar1;
    if (bVar7) {
      *(uint *)(lVar2 + 0x30) = uVar3 & 1;
      uVar1 = uVar3;
    }
    uVar3 = uVar1;
    UNLOCK();
  } while (!bVar7);
  if (uVar3 != 0) {
    EnterCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x68));
    iVar6 = *(int *)(lVar2 + 0x10) - *(int *)(lVar2 + 0x20);
    iVar5 = param_2;
    if (iVar6 < param_2) {
      iVar5 = iVar6;
    }
    memcpy((void *)(*(longlong *)(lVar2 + 0x18) + (longlong)(*(int *)(lVar2 + 0x20) * 2) * 8),
           param_3,(longlong)iVar5 << 4);
    iVar4 = param_2 - iVar6;
    if (param_2 <= iVar6) {
      iVar4 = 0;
    }
    memcpy(*(void **)(lVar2 + 0x18),(void *)((longlong)param_3 + (longlong)(iVar5 * 2) * 8),
           (longlong)iVar4 << 4);
    *(int *)(lVar2 + 0x28) = *(int *)(lVar2 + 0x28) + param_2;
    if (*(int *)(lVar2 + 8) <= *(int *)(lVar2 + 0x28)) {
      iVar5 = *(int *)(lVar2 + 0x28) / *(int *)(lVar2 + 8);
      ReleaseSemaphore(*(HANDLE *)(lVar2 + 0x38),iVar5,(LPLONG)0x0);
      *(int *)(lVar2 + 0x28) = *(int *)(lVar2 + 0x28) - iVar5 * *(int *)(lVar2 + 8);
    }
    *(int *)(lVar2 + 0x20) = *(int *)(lVar2 + 0x20) + param_2;
    if (*(int *)(lVar2 + 0x10) <= *(int *)(lVar2 + 0x20)) {
      *(int *)(lVar2 + 0x20) = *(int *)(lVar2 + 0x20) - *(int *)(lVar2 + 0x10);
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(lVar2 + 0x68));
  }
  return;
}



/* ========================================================================
   ENTRY: 180011830
   NAME : FUN_180011830
   SIG  : undefined __fastcall FUN_180011830(int param_1)
   ======================================================================== */

void FUN_180011830(int param_1)

{
  uint uVar1;
  int iVar2;
  longlong lVar3;
  void *_Dst;
  longlong lVar4;
  int iVar5;
  uint uVar6;
  DWORD DVar7;
  HANDLE pvVar8;
  int iVar9;
  int iVar10;
  longlong lVar11;
  bool bVar12;
  DWORD local_res8 [2];
  DWORD local_res10;
  longlong local_res18;
  
  local_res8[0] = 0;
  pvVar8 = AvSetMmThreadCharacteristicsW(L"Pro Audio",local_res8);
  if (pvVar8 == (HANDLE)0x0) {
    pvVar8 = GetCurrentThread();
    SetThreadPriority(pvVar8,2);
  }
  else {
    AvSetMmThreadPriority(pvVar8,AVRT_PRIORITY_CRITICAL);
  }
  lVar11 = (longlong)param_1 * 8;
  lVar3 = *(longlong *)(&DAT_180020380 + lVar11);
  uVar6 = *(uint *)(lVar3 + 0x2c);
  do {
    LOCK();
    uVar1 = *(uint *)(lVar3 + 0x2c);
    bVar12 = uVar6 == uVar1;
    if (bVar12) {
      *(uint *)(lVar3 + 0x2c) = uVar6 & 1;
      uVar1 = uVar6;
    }
    uVar6 = uVar1;
    UNLOCK();
  } while (!bVar12);
  local_res18 = lVar11;
  if (uVar6 != 0) {
    local_res10 = 0xffffffff;
    if (param_1 == 0) {
      local_res10 = 10;
    }
    do {
      DVar7 = WaitForSingleObject(*(HANDLE *)(lVar3 + 0x38),local_res10);
      EnterCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x40));
      LeaveCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x40));
      if (DVar7 == 0) {
        _Dst = *(void **)(lVar3 + 0x90);
        lVar4 = *(longlong *)(&DAT_180020380 + lVar11);
        uVar6 = *(uint *)(lVar4 + 0x2c);
        do {
          LOCK();
          uVar1 = *(uint *)(lVar4 + 0x2c);
          bVar12 = uVar6 == uVar1;
          if (bVar12) {
            *(uint *)(lVar4 + 0x2c) = uVar6 & 1;
            uVar1 = uVar6;
          }
          uVar6 = uVar1;
          UNLOCK();
        } while (!bVar12);
        if (uVar6 == 0) {
          _endthread();
        }
        iVar2 = *(int *)(lVar4 + 8);
        iVar10 = *(int *)(lVar4 + 0x10) - *(int *)(lVar4 + 0x24);
        iVar5 = iVar10;
        if (iVar2 <= iVar10) {
          iVar5 = iVar2;
        }
        memcpy(_Dst,(void *)(*(longlong *)(lVar4 + 0x18) +
                            (longlong)(*(int *)(lVar4 + 0x24) * 2) * 8),(longlong)iVar5 << 4);
        iVar9 = iVar2 - iVar10;
        if (iVar2 <= iVar10) {
          iVar9 = 0;
        }
        memcpy((void *)((longlong)(iVar5 * 2) * 8 + (longlong)_Dst),*(void **)(lVar4 + 0x18),
               (longlong)iVar9 << 4);
        *(int *)(lVar4 + 0x24) = *(int *)(lVar4 + 0x24) + *(int *)(lVar4 + 8);
        lVar11 = local_res18;
        if (*(int *)(lVar4 + 0x10) <= *(int *)(lVar4 + 0x24)) {
          *(int *)(lVar4 + 0x24) = *(int *)(lVar4 + 0x24) - *(int *)(lVar4 + 0x10);
        }
      }
      else if (param_1 == 0) {
        memset(*(void **)(lVar3 + 0x90),0,(longlong)(*(int *)(lVar3 + 8) * 2) << 3);
      }
      FUN_18000e3a0(param_1,*(void **)(lVar3 + 0x90));
      uVar6 = *(uint *)(lVar3 + 0x2c);
      do {
        LOCK();
        uVar1 = *(uint *)(lVar3 + 0x2c);
        bVar12 = uVar6 == uVar1;
        if (bVar12) {
          *(uint *)(lVar3 + 0x2c) = uVar6 & 1;
          uVar1 = uVar6;
        }
        uVar6 = uVar1;
        UNLOCK();
      } while (!bVar12);
    } while (uVar6 != 0);
  }
  _endthread();
  return;
}



/* ========================================================================
   ENTRY: 180011a40
   NAME : FUN_180011a40
   SIG  : undefined __fastcall FUN_180011a40(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180011a40(void)

{
  double dVar1;
  int iVar2;
  int iVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  ulonglong uVar6;
  undefined8 uVar7;
  undefined *puVar8;
  undefined8 uVar9;
  int *piVar10;
  undefined8 uVar11;
  undefined *puVar12;
  longlong lVar13;
  int iVar14;
  int iVar15;
  longlong lVar16;
  uint uVar17;
  int iVar18;
  int local_res8;
  undefined4 local_resc;
  ulonglong in_stack_ffffffffffffff40;
  ulonglong uVar19;
  undefined8 in_stack_ffffffffffffff58;
  undefined4 uVar20;
  
  iVar15 = 0;
  create_siphonEXT(0,1,*(undefined4 *)(PTR_DAT_18001e080 + 0x228),0x200,0x200,
                   in_stack_ffffffffffffff40 & 0xffffffff00000000);
  (*DAT_18001f4d8)(0);
  uVar9 = malloc0(*(int *)(PTR_DAT_18001e080 + 4) << 3);
  puVar12 = PTR_DAT_18001e080;
  *(undefined8 *)PTR_DAT_18001e088 = uVar9;
  iVar14 = iVar15;
  if (0 < *(int *)(puVar12 + 4)) {
    do {
      uVar20 = (undefined4)((ulonglong)in_stack_ffffffffffffff58 >> 0x20);
      lVar16 = (longlong)iVar14;
      lVar13 = lVar16 * 0x40;
      uVar9 = malloc0(*(int *)(puVar12 + lVar13 + 0xcf4) << 4);
      *(undefined8 *)(*(longlong *)PTR_DAT_18001e088 + lVar16 * 8) = uVar9;
      (*DAT_18001f4e8)(iVar14);
      (*DAT_18001f4e0)(iVar14);
      in_stack_ffffffffffffff58 = CONCAT44(uVar20,48000);
      create_ivac(iVar14,0,0,0,*(undefined4 *)(PTR_DAT_18001e080 + lVar16 * 4 + 0x1a8),
                  *(undefined4 *)
                   (PTR_DAT_18001e080 + (longlong)*(int *)(PTR_DAT_18001e080 + 4) * 4 + 0x1a8),
                  *(undefined4 *)(PTR_DAT_18001e080 + lVar13 + 0xcf0),
                  *(undefined4 *)(PTR_DAT_18001e080 + 0x10f0),48000,
                  *(undefined4 *)
                   (PTR_DAT_18001e080 + (longlong)*(int *)(PTR_DAT_18001e080 + 4) * 4 + 0x228),
                  *(undefined4 *)(PTR_DAT_18001e080 + lVar16 * 4 + 0x228),
                  *(undefined4 *)(PTR_DAT_18001e080 + lVar13 + 0xcf4),
                  *(undefined4 *)(PTR_DAT_18001e080 + 0x10f4),0x400);
      puVar8 = PTR_DAT_18001e088;
      puVar12 = PTR_DAT_18001e080;
      iVar14 = iVar14 + 1;
      *(undefined8 *)(PTR_DAT_18001e088 + lVar16 * 0x30 + 0x40) = 0;
      *(undefined4 *)(puVar8 + lVar16 * 0x30 + 0x48) = 0;
    } while (iVar14 < *(int *)(puVar12 + 4));
  }
  uVar9 = DAT_180018dc8;
  iVar14 = iVar15;
  if (0 < *(int *)(puVar12 + 4)) {
    do {
      puVar8 = PTR_DAT_18001e098;
      lVar13 = (longlong)iVar14;
      iVar2 = *(int *)(puVar12 + lVar13 * 0x40 + 0xcf0);
      *(int *)(PTR_DAT_18001e098 + lVar13 * 0x30 + 0x10) = iVar2;
      uVar20 = *(undefined4 *)(puVar12 + 0x10f0);
      *(undefined4 *)(puVar8 + lVar13 * 0x30 + 0x14) = uVar20;
      iVar3 = *(int *)(puVar12 + lVar13 * 0x40 + 0xcf4);
      *(int *)(puVar8 + lVar13 * 0x30 + 0x18) = iVar3;
      if ((-1 < iVar14) && (iVar14 < *(int *)(puVar12 + 4))) {
        if (*(int *)(puVar8 + lVar13 * 0x30 + 0x1c) == 0) {
          iVar18 = 1;
          uVar17 = 1;
        }
        else {
          iVar18 = 2;
          uVar17 = -(uint)(*(int *)(puVar8 + lVar13 * 0x30 + 0x20) != 0) & 2;
        }
        local_res8 = iVar2;
        local_resc = uVar20;
        piVar10 = FUN_180001340(-1,iVar14,iVar3,iVar3,2,iVar18,uVar17,uVar9,
                                in_stack_ffffffffffffff58,(longlong)&local_res8,iVar2,&LAB_180013b30
                                ,0,0,0,0);
        dVar1 = *(double *)(puVar8 + lVar13 * 0x30 + 0x28);
        *(int **)(puVar8 + lVar13 * 0x30 + 8) = piVar10;
        if (piVar10 == (int *)0x0) {
          piVar10 = DAT_18001e140;
        }
        *(int *)(puVar8 + lVar13 * 0x30 + 0x30) = iVar18;
        *(uint *)(puVar8 + lVar13 * 0x30 + 0x34) = uVar17;
        EnterCriticalSection((LPCRITICAL_SECTION)(piVar10 + 0x310));
        *(double *)(piVar10 + 0x70) = dVar1;
        *(double *)(piVar10 + 0xb0) = dVar1 * *(double *)(piVar10 + 0xee);
        LeaveCriticalSection((LPCRITICAL_SECTION)(piVar10 + 0x310));
        puVar12 = PTR_DAT_18001e080;
      }
      iVar14 = iVar14 + 1;
    } while (iVar14 < *(int *)(puVar12 + 4));
  }
  uVar7 = _UNK_180018e78;
  uVar6 = _DAT_180018e70;
  uVar5 = DAT_180018e00;
  uVar4 = DAT_180018d98;
  uVar9 = DAT_180018d90;
  if (0 < *(int *)(puVar12 + 0x14)) {
    do {
      lVar16 = (longlong)(*(int *)(puVar12 + 8) + *(int *)(puVar12 + 4) + iVar15);
      uVar19 = uVar6;
      uVar11 = create_anb(0,*(undefined4 *)(puVar12 + lVar16 * 4 + 0x228),
                          *(undefined8 *)(puVar12 + lVar16 * 8 + 0xa8),
                          *(undefined8 *)(puVar12 + lVar16 * 8 + 0xa8),
                          (double)*(int *)(puVar12 + lVar16 * 4 + 0x1a8),uVar6,uVar6,uVar7,uVar4,
                          uVar5);
      puVar12 = PTR_DAT_18001e080;
      lVar13 = (longlong)iVar15;
      *(undefined8 *)(PTR_DAT_18001e088 + lVar13 * 0x10 + 0x330) = uVar11;
      uVar11 = create_nob(0,*(undefined4 *)(puVar12 + lVar16 * 4 + 0x228),
                          *(undefined8 *)(puVar12 + lVar16 * 8 + 0xa8),
                          *(undefined8 *)(puVar12 + lVar16 * 8 + 0xa8),
                          (double)*(int *)(puVar12 + lVar16 * 4 + 0x1a8),uVar19 & 0xffffffff00000000
                          ,uVar6,uVar7,uVar6,uVar7,uVar9,uVar4,uVar5);
      puVar12 = PTR_DAT_18001e080;
      iVar15 = iVar15 + 1;
      *(undefined8 *)(PTR_DAT_18001e088 + lVar13 * 0x10 + 0x338) = uVar11;
    } while (iVar15 < *(int *)(puVar12 + 0x14));
  }
  return;
}



/* ========================================================================
   ENTRY: 180011e90
   NAME : FUN_180011e90
   SIG  : undefined __fastcall FUN_180011e90(int param_1, int param_2, undefined8 * param_3)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180012162) */
/* WARNING: Removing unreachable block (ram,0x0001800126b2) */
/* WARNING: Removing unreachable block (ram,0x000180012712) */
/* WARNING: Restarted to delay deadcode elimination for space: ram */

void FUN_180011e90(int param_1,int param_2,undefined8 *param_3)

{
  int *piVar1;
  uint uVar2;
  void *pvVar3;
  longlong lVar4;
  longlong lVar5;
  uint uVar6;
  longlong lVar7;
  longlong lVar8;
  longlong lVar9;
  int iVar10;
  undefined8 uVar11;
  undefined *puVar12;
  undefined *puVar13;
  int iVar14;
  bool bVar15;
  int local_res8;
  
  puVar13 = PTR_DAT_18001e080;
  lVar8 = (longlong)param_1;
  iVar14 = *(int *)(PTR_DAT_18001e080 + 4);
  lVar9 = param_3[lVar8];
  if ((iVar14 <= param_1) && (*(int *)(PTR_DAT_18001e080 + 8) + iVar14 <= param_1)) {
    local_res8 = (param_1 - *(int *)(PTR_DAT_18001e080 + 8)) - iVar14;
  }
  if (param_1 == 0) {
    if (param_2 != 0) {
      if (param_2 != 1) {
        return;
      }
      memcpy((void *)**(undefined8 **)PTR_DAT_18001e088,(void *)*param_3,
             (longlong)*(int *)(PTR_DAT_18001e080 + 0xcf4) << 4);
      iVar14 = 1;
      piVar1 = (int *)(PTR_DAT_18001e080 + 0xc);
      puVar13 = PTR_DAT_18001e080;
      puVar12 = PTR_DAT_18001e080;
      if (1 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          puVar13 = puVar12;
          if (0 < *(int *)(puVar12 + 0xcf4) * 2) {
            iVar10 = 0;
            lVar9 = param_3[iVar14];
            do {
              lVar8 = (longlong)iVar10;
              iVar10 = iVar10 + 1;
              *(double *)(**(longlong **)PTR_DAT_18001e088 + lVar8 * 8) =
                   *(double *)(**(longlong **)PTR_DAT_18001e088 + lVar8 * 8) +
                   *(double *)(lVar9 + lVar8 * 8);
              puVar13 = PTR_DAT_18001e080;
            } while (iVar10 < *(int *)(puVar12 + 0xcf4) * 2);
          }
          iVar14 = iVar14 + 1;
          puVar12 = puVar13;
        } while (iVar14 < *piVar1);
      }
      if (*(int *)(PTR_DAT_18001e088 + 0x40) != 0) {
        (*DAT_18001f4f8)(0,**(undefined8 **)PTR_DAT_18001e088);
        puVar13 = PTR_DAT_18001e080;
      }
      if ((*DAT_18001f430 != 0) && (DAT_18001f430[1] == 0)) {
        FUN_180001840(*(longlong *)(DAT_18001f430 + 0xe),-1,0,
                      (void *)**(undefined8 **)PTR_DAT_18001e088);
        puVar13 = PTR_DAT_18001e080;
      }
      if (0 < *(int *)(puVar13 + 4)) {
        pvVar3 = (void *)**(undefined8 **)PTR_DAT_18001e088;
        uVar6 = *(uint *)(puVar13 + 0xce0);
        do {
          LOCK();
          uVar2 = *(uint *)(puVar13 + 0xce0);
          bVar15 = uVar6 == uVar2;
          if (bVar15) {
            *(uint *)(puVar13 + 0xce0) = uVar6 & 1;
            uVar2 = uVar6;
          }
          uVar6 = uVar2;
          UNLOCK();
        } while (!bVar15);
        if (((uVar6 != 0) && (*(longlong *)PTR_DAT_18001e098 != 0)) &&
           (*(longlong *)(PTR_DAT_18001e098 + 8) != 0)) {
          FUN_180001840(*(longlong *)(PTR_DAT_18001e098 + 8),-1,0,pvVar3);
        }
      }
      if (*(int *)(PTR_DAT_18001e088 + 0x48) == 0) {
        return;
      }
      (*DAT_18001f508)(0,1,**(undefined8 **)PTR_DAT_18001e088);
      return;
    }
    uVar6 = *(uint *)(PTR_DAT_18001e080 + 0xce0);
    do {
      LOCK();
      uVar2 = *(uint *)(puVar13 + 0xce0);
      bVar15 = uVar6 == uVar2;
      if (bVar15) {
        *(uint *)(puVar13 + 0xce0) = uVar6 & 1;
        uVar2 = uVar6;
      }
      uVar6 = uVar2;
      UNLOCK();
    } while (!bVar15);
    if ((uVar6 != 0) && (*(code **)(PTR_DAT_18001e080 + 0xcd0) != (code *)0x0)) {
      (**(code **)(PTR_DAT_18001e080 + 0xcd0))(0,*(undefined4 *)(PTR_DAT_18001e080 + 0x228),lVar9);
    }
    if (*(int *)(PTR_DAT_18001e088 + 0x44) != 0) {
      (*DAT_18001f500)(0,lVar9);
    }
    if (*(int *)(PTR_DAT_18001e088 + 0x48) != 0) {
      (*DAT_18001f508)(0,0,lVar9);
    }
    xsiphonEXT(0,lVar9);
    LOCK();
    UNLOCK();
    Spectrum0(DAT_18001f4f0,0,1,0,lVar9);
  }
  else {
    if (iVar14 <= param_1) {
      if (param_1 != iVar14) {
        if (param_1 == *(int *)(PTR_DAT_18001e080 + 8) + iVar14) {
          xanb(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)local_res8 + 0x33) * 0x10));
          xnob(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)local_res8 * 0x10 + 0x338));
          LOCK();
          UNLOCK();
          uVar11 = 0;
        }
        else {
          if (param_1 != *(int *)(PTR_DAT_18001e080 + 8) + iVar14 + 1) {
            return;
          }
          xanb(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)local_res8 + 0x33) * 0x10));
          xnob(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)local_res8 * 0x10 + 0x338));
          LOCK();
          UNLOCK();
          uVar11 = 2;
        }
        Spectrum0(DAT_18001f4f0,0,uVar11,0,lVar9);
        return;
      }
      if (param_2 == 0) {
        uVar6 = *(uint *)(PTR_DAT_18001e080 + 0x1140);
        do {
          LOCK();
          uVar2 = *(uint *)(puVar13 + 0x1140);
          bVar15 = uVar6 == uVar2;
          if (bVar15) {
            *(uint *)(puVar13 + 0x1140) = uVar6 & 1;
            uVar2 = uVar6;
          }
          uVar6 = uVar2;
          UNLOCK();
        } while (!bVar15);
        if (uVar6 == 0) {
          if (*(int *)(PTR_DAT_18001e088 + 0x44) != 0) {
            (*DAT_18001f500)(1,lVar9);
          }
          if (*(int *)(PTR_DAT_18001e088 + 0x74) != 0) {
            (*DAT_18001f530)(1,lVar9);
          }
          if (DAT_18001f7f0 == 0) {
            xvacIN(0,lVar9,0);
            xvacIN(1,lVar9,1);
          }
          if (DAT_18001f7f0 == 1) {
            xvacIN(1,lVar9,0);
            xvacIN(0,lVar9,1);
          }
        }
        if (*(int *)(PTR_DAT_18001e088 + 0x48) != 0) {
          (*DAT_18001f508)(1,0,lVar9);
        }
        if (*(int *)(PTR_DAT_18001e088 + 0x78) == 0) {
          return;
        }
        (*DAT_18001f538)(1,0,lVar9);
        return;
      }
      if (param_2 != 1) {
        return;
      }
      if (*(int *)(PTR_DAT_18001e088 + 0x40) != 0) {
        (*DAT_18001f4f8)(1,param_3[2]);
      }
      if ((*DAT_18001f430 != 0) && (DAT_18001f430[1] == 0)) {
        FUN_180001840(*(longlong *)(DAT_18001f430 + 0xe),-1,1,(void *)param_3[2]);
      }
      if ((*DAT_18001f438 != 0) && (DAT_18001f438[1] == 0)) {
        FUN_180001840(*(longlong *)(DAT_18001f438 + 0xe),-1,1,(void *)param_3[2]);
      }
      iVar14 = 0;
      puVar13 = PTR_DAT_18001e080;
      if (0 < *(int *)(PTR_DAT_18001e080 + 4)) {
        do {
          if ((-1 < iVar14) && (iVar14 < *(int *)(puVar13 + 4))) {
            pvVar3 = (void *)param_3[2];
            uVar6 = *(uint *)(puVar13 + 0xce0);
            do {
              LOCK();
              uVar2 = *(uint *)(puVar13 + 0xce0);
              bVar15 = uVar6 == uVar2;
              if (bVar15) {
                *(uint *)(puVar13 + 0xce0) = uVar6 & 1;
                uVar2 = uVar6;
              }
              uVar6 = uVar2;
              UNLOCK();
            } while (!bVar15);
            puVar13 = PTR_DAT_18001e080;
            if (((uVar6 != 0) && (*(longlong *)PTR_DAT_18001e098 != 0)) &&
               (*(longlong *)(PTR_DAT_18001e098 + (longlong)iVar14 * 0x30 + 8) != 0)) {
              FUN_180001840(*(longlong *)(PTR_DAT_18001e098 + (longlong)iVar14 * 0x30 + 8),-1,1,
                            pvVar3);
              puVar13 = PTR_DAT_18001e080;
            }
          }
          iVar14 = iVar14 + 1;
        } while (iVar14 < *(int *)(puVar13 + 4));
      }
      if (*(int *)(PTR_DAT_18001e088 + 0x48) != 0) {
        (*DAT_18001f508)(1,1,param_3[2]);
      }
      if (*(int *)(PTR_DAT_18001e088 + 0x78) == 0) {
        return;
      }
      (*DAT_18001f538)(1,1,param_3[2]);
      return;
    }
    if (param_2 != 0) {
      if (param_2 != 1) {
        return;
      }
      lVar9 = lVar8 * 0x40;
      memcpy(*(void **)(*(longlong *)PTR_DAT_18001e088 + lVar8 * 8),(void *)*param_3,
             (longlong)*(int *)(PTR_DAT_18001e080 + lVar9 + 0xcf4) << 4);
      iVar14 = 1;
      piVar1 = (int *)(PTR_DAT_18001e080 + 0xc);
      puVar13 = PTR_DAT_18001e080;
      puVar12 = PTR_DAT_18001e080;
      if (1 < *(int *)(PTR_DAT_18001e080 + 0xc)) {
        do {
          puVar13 = puVar12;
          if (0 < *(int *)(puVar12 + lVar9 + 0xcf4) * 2) {
            iVar10 = 0;
            lVar4 = param_3[iVar14];
            do {
              lVar7 = (longlong)iVar10;
              iVar10 = iVar10 + 1;
              lVar5 = *(longlong *)(*(longlong *)PTR_DAT_18001e088 + lVar8 * 8);
              *(double *)(lVar5 + lVar7 * 8) =
                   *(double *)(lVar5 + lVar7 * 8) + *(double *)(lVar4 + lVar7 * 8);
              puVar13 = PTR_DAT_18001e080;
            } while (iVar10 < *(int *)(puVar12 + lVar9 + 0xcf4) * 2);
          }
          iVar14 = iVar14 + 1;
          puVar12 = puVar13;
        } while (iVar14 < *piVar1);
      }
      piVar1 = (&DAT_18001f430)[lVar8];
      if ((*piVar1 != 0) && (piVar1[1] == 0)) {
        FUN_180001840(*(longlong *)(piVar1 + 0xe),-1,0,
                      *(void **)(*(longlong *)PTR_DAT_18001e088 + lVar8 * 8));
        puVar13 = PTR_DAT_18001e080;
      }
      if ((-1 < param_1) && (param_1 < *(int *)(puVar13 + 4))) {
        pvVar3 = *(void **)(*(longlong *)PTR_DAT_18001e088 + lVar8 * 8);
        uVar6 = *(uint *)(puVar13 + 0xce0);
        do {
          LOCK();
          uVar2 = *(uint *)(puVar13 + 0xce0);
          bVar15 = uVar6 == uVar2;
          if (bVar15) {
            *(uint *)(puVar13 + 0xce0) = uVar6 & 1;
            uVar2 = uVar6;
          }
          uVar6 = uVar2;
          UNLOCK();
        } while (!bVar15);
        if (((uVar6 != 0) && (*(longlong *)PTR_DAT_18001e098 != 0)) &&
           (*(longlong *)(PTR_DAT_18001e098 + lVar8 * 0x30 + 8) != 0)) {
          FUN_180001840(*(longlong *)(PTR_DAT_18001e098 + lVar8 * 0x30 + 8),-1,0,pvVar3);
        }
      }
      if (*(int *)(PTR_DAT_18001e088 + lVar8 * 0x30 + 0x48) == 0) {
        return;
      }
      (*(&DAT_18001f508)[lVar8 * 6])
                (0,1,*(undefined8 *)(*(longlong *)PTR_DAT_18001e088 + lVar8 * 8));
      return;
    }
    uVar6 = *(uint *)(PTR_DAT_18001e080 + 0xce0);
    do {
      LOCK();
      uVar2 = *(uint *)(puVar13 + 0xce0);
      bVar15 = uVar6 == uVar2;
      if (bVar15) {
        *(uint *)(puVar13 + 0xce0) = uVar6 & 1;
        uVar2 = uVar6;
      }
      uVar6 = uVar2;
      UNLOCK();
    } while (!bVar15);
    if ((uVar6 != 0) && (*(code **)(PTR_DAT_18001e080 + 0xcd0) != (code *)0x0)) {
      (**(code **)(PTR_DAT_18001e080 + 0xcd0))
                (param_1,*(undefined4 *)(PTR_DAT_18001e080 + lVar8 * 4 + 0x228),lVar9);
    }
    if (*(int *)(PTR_DAT_18001e088 + lVar8 * 0x30 + 0x44) != 0) {
      (*(&DAT_18001f500)[lVar8 * 6])(0,lVar9);
    }
    if (*(int *)(PTR_DAT_18001e088 + lVar8 * 0x30 + 0x48) != 0) {
      (*(&DAT_18001f508)[lVar8 * 6])(0,0,lVar9);
    }
  }
  piVar1 = (&DAT_18001f430)[lVar8];
  if ((*piVar1 != 0) && (piVar1[1] != 0)) {
    xrmatchIN(*(undefined8 *)(piVar1 + 0x14),lVar9);
  }
  return;
}



/* ========================================================================
   ENTRY: 180012750
   NAME : SendCBCreateScope
   SIG  : undefined __fastcall SendCBCreateScope(undefined8 param_1)
   ======================================================================== */

void SendCBCreateScope(undefined8 param_1)

{
                    /* 0x12750  47  SendCBCreateScope */
  DAT_18001f4d8 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180012760
   NAME : SendCBScope
   SIG  : undefined __fastcall SendCBScope(int param_1, undefined8 param_2)
   ======================================================================== */

void SendCBScope(int param_1,undefined8 param_2)

{
                    /* 0x12760  51  SendCBScope */
  (&DAT_18001f4f8)[(longlong)param_1 * 6] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180012780
   NAME : SetScopeRun
   SIG  : undefined __fastcall SetScopeRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetScopeRun(int param_1,undefined4 param_2)

{
                    /* 0x12780  208  SetScopeRun */
  (&DAT_18001f510)[(longlong)param_1 * 0xc] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 1800127a0
   NAME : SendCBCreateWRecord
   SIG  : undefined __fastcall SendCBCreateWRecord(undefined8 param_1)
   ======================================================================== */

void SendCBCreateWRecord(undefined8 param_1)

{
                    /* 0x127a0  49  SendCBCreateWRecord */
  DAT_18001f4e0 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 1800127b0
   NAME : SendCBWaveRecorder
   SIG  : undefined __fastcall SendCBWaveRecorder(int param_1, undefined8 param_2)
   ======================================================================== */

void SendCBWaveRecorder(int param_1,undefined8 param_2)

{
                    /* 0x127b0  53  SendCBWaveRecorder */
  (&DAT_18001f508)[(longlong)param_1 * 6] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 1800127d0
   NAME : SetWaveRecorderRun
   SIG  : undefined __fastcall SetWaveRecorderRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetWaveRecorderRun(int param_1,undefined4 param_2)

{
                    /* 0x127d0  239  SetWaveRecorderRun */
  (&DAT_18001f518)[(longlong)param_1 * 0xc] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 1800127f0
   NAME : SendCBCreateWPlay
   SIG  : undefined __fastcall SendCBCreateWPlay(undefined8 param_1)
   ======================================================================== */

void SendCBCreateWPlay(undefined8 param_1)

{
                    /* 0x127f0  48  SendCBCreateWPlay */
  DAT_18001f4e8 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180012800
   NAME : SendCBWavePlayer
   SIG  : undefined __fastcall SendCBWavePlayer(int param_1, undefined8 param_2)
   ======================================================================== */

void SendCBWavePlayer(int param_1,undefined8 param_2)

{
                    /* 0x12800  52  SendCBWavePlayer */
  (&DAT_18001f500)[(longlong)param_1 * 6] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180012820
   NAME : SetWavePlayerRun
   SIG  : undefined __fastcall SetWavePlayerRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetWavePlayerRun(int param_1,undefined4 param_2)

{
                    /* 0x12820  238  SetWavePlayerRun */
  (&DAT_18001f514)[(longlong)param_1 * 0xc] = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180012840
   NAME : SetTopPan3Run
   SIG  : undefined __fastcall SetTopPan3Run(undefined4 param_1)
   ======================================================================== */

void SetTopPan3Run(undefined4 param_1)

{
                    /* 0x12840  227  SetTopPan3Run */
  LOCK();
  DAT_18001f4f0 = param_1;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180012850
   NAME : SetTXVAC
   SIG  : undefined __fastcall SetTXVAC(int param_1, undefined4 param_2)
   ======================================================================== */

void SetTXVAC(int param_1,undefined4 param_2)

{
                    /* 0x12850  226  SetTXVAC */
  LOCK();
  (&DAT_18001f7f0)[param_1] = param_2;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180012860
   NAME : xrouter
   SIG  : undefined __fastcall xrouter(longlong param_1, int param_2, int param_3, int param_4, void * param_5)
   ======================================================================== */

void xrouter(longlong param_1,int param_2,int param_3,int param_4,void *param_5)

{
  int *piVar1;
  longlong lVar2;
  int iVar3;
  int iVar4;
  ulonglong uVar5;
  int iVar6;
  longlong lVar7;
  longlong lVar8;
  int iVar9;
  int iVar10;
  int iVar11;
  bool bVar12;
  int local_res18;
  longlong local_48 [2];
  
                    /* 0x12860  301  xrouter */
  if (param_1 == 0) {
    param_1 = (&DAT_18001f840)[param_2];
  }
  uVar5 = (ulonglong)*(uint *)(param_1 + 4);
  do {
    iVar10 = (int)uVar5;
    LOCK();
    iVar11 = *(int *)(param_1 + 4);
    bVar12 = iVar10 == iVar11;
    if (bVar12) {
      *(int *)(param_1 + 4) = iVar10;
      iVar11 = iVar10;
    }
    UNLOCK();
    uVar5 = (ulonglong)iVar11;
  } while (!bVar12);
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x3340));
  if (param_3 < *(int *)(param_1 + 8)) {
    piVar1 = (int *)(param_1 + ((longlong)param_3 + 0x184) * 4);
    iVar11 = param_4 / *piVar1;
    local_res18 = 0;
    if (0 < *(int *)(param_1 + 0xc)) {
      lVar2 = (longlong)param_3 * 0x40 + param_1 + uVar5 * 4;
      do {
        lVar7 = (longlong)local_res18 * 0x20;
        iVar10 = *(int *)(lVar2 + 0x10 + lVar7);
        if (iVar10 == 1) {
          Inbound(*(uint *)(lVar2 + 0x310 + lVar7),param_4,param_5);
        }
        else if (iVar10 == 2) {
          iVar10 = 0;
          if (0 < *piVar1) {
            do {
              iVar9 = 0;
              iVar4 = iVar10 * iVar11;
              local_48[iVar10] = param_1 + ((longlong)(iVar10 * iVar11 * 2) + 200) * 8;
              if (3 < iVar11) {
                do {
                  lVar8 = (longlong)((iVar9 + iVar4) * 2);
                  *(undefined8 *)(param_1 + 0x640 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)((iVar9 * *piVar1 + iVar10) * 2) * 8);
                  *(undefined8 *)(param_1 + 0x648 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)((iVar9 * *piVar1 + iVar10) * 2) * 8 + 8);
                  *(undefined8 *)(param_1 + 0x650 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)(((iVar9 + 1) * *piVar1 + iVar10) * 2) * 8);
                  *(undefined8 *)(param_1 + 0x658 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 +
                        (longlong)(((iVar9 + 1) * *piVar1 + iVar10) * 2) * 8 + 8);
                  *(undefined8 *)(param_1 + 0x640 + (longlong)((iVar9 + iVar4 + 2) * 2) * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)(((iVar9 + 2) * *piVar1 + iVar10) * 2) * 8);
                  iVar6 = iVar9 + 3;
                  *(undefined8 *)(param_1 + 0x668 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 +
                        (longlong)(((iVar9 + 2) * *piVar1 + iVar10) * 2) * 8 + 8);
                  iVar3 = iVar9 + iVar4 + 3;
                  iVar9 = iVar9 + 4;
                  *(undefined8 *)(param_1 + 0x640 + (longlong)(iVar3 * 2) * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)((iVar6 * *piVar1 + iVar10) * 2) * 8);
                  *(undefined8 *)(param_1 + 0x678 + lVar8 * 8) =
                       *(undefined8 *)
                        ((longlong)param_5 + (longlong)((iVar6 * *piVar1 + iVar10) * 2) * 8 + 8);
                } while (iVar9 < iVar11 + -3);
              }
              for (; iVar9 < iVar11; iVar9 = iVar9 + 1) {
                lVar8 = (longlong)((iVar9 + iVar4) * 2);
                *(undefined8 *)(param_1 + 0x640 + lVar8 * 8) =
                     *(undefined8 *)
                      ((longlong)param_5 + (longlong)((iVar9 * *piVar1 + iVar10) * 2) * 8);
                *(undefined8 *)(param_1 + 0x648 + lVar8 * 8) =
                     *(undefined8 *)
                      ((longlong)param_5 + (longlong)((iVar9 * *piVar1 + iVar10) * 2) * 8 + 8);
              }
              iVar10 = iVar10 + 1;
            } while (iVar10 < *piVar1);
          }
          InboundBlock(*(int *)(lVar2 + 0x310 + lVar7),iVar11,local_48);
        }
        local_res18 = local_res18 + 1;
      } while (local_res18 < *(int *)(param_1 + 0xc));
    }
  }
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x3340));
  return;
}



/* ========================================================================
   ENTRY: 180012b90
   NAME : LoadRouterAll
   SIG  : undefined __fastcall LoadRouterAll(longlong param_1, int param_2, int param_3, int param_4, int param_5, longlong param_6, longlong param_7, longlong param_8)
   ======================================================================== */

void LoadRouterAll(longlong param_1,int param_2,int param_3,int param_4,int param_5,longlong param_6
                  ,longlong param_7,longlong param_8)

{
  longlong lVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  longlong lVar4;
  int iVar5;
  ulonglong uVar6;
  uint uVar7;
  int iVar8;
  
                    /* 0x12b90  40  LoadRouterAll */
  if (param_1 == 0) {
    param_1 = (&DAT_18001f840)[param_2];
  }
  EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x3340));
  uVar6 = 0;
  uVar2 = uVar6;
  do {
    uVar3 = uVar2 + 1;
    lVar1 = uVar2 * 0x40;
    *(undefined8 *)(param_1 + 0x10 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x310 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x18 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x318 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x20 + lVar1) = 0;
    *(undefined8 *)(param_1 + 800 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x28 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x328 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x30 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x330 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x38 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x338 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x40 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x340 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x48 + lVar1) = 0;
    *(undefined8 *)(param_1 + 0x348 + lVar1) = 0;
    uVar2 = uVar3;
  } while (uVar3 != 0xc);
  *(undefined8 *)(param_1 + 0x610) = 0;
  *(undefined8 *)(param_1 + 0x618) = 0;
  *(undefined8 *)(param_1 + 0x620) = 0;
  *(undefined8 *)(param_1 + 0x628) = 0;
  *(undefined8 *)(param_1 + 0x630) = 0;
  *(undefined8 *)(param_1 + 0x638) = 0;
  memset((void *)(param_1 + 0x640),0,0x2d00);
  *(int *)(param_1 + 8) = param_3;
  *(int *)(param_1 + 0xc) = param_4;
  if (0 < param_3) {
    do {
      iVar8 = 0;
      iVar5 = (int)uVar6;
      if (0 < *(int *)(param_1 + 0xc)) {
        do {
          if (0 < param_5) {
            uVar2 = 0;
            lVar1 = ((longlong)iVar8 + (longlong)iVar5 * 2) * 0x20 + param_1;
            do {
              lVar4 = (longlong)((iVar5 * param_4 + iVar8) * param_5 + (int)uVar2);
              *(undefined4 *)(lVar1 + 0x10 + uVar2 * 4) = *(undefined4 *)(param_7 + lVar4 * 4);
              *(undefined4 *)(lVar1 + 0x310 + uVar2 * 4) = *(undefined4 *)(param_8 + lVar4 * 4);
              uVar7 = (int)uVar2 + 1;
              uVar2 = (ulonglong)uVar7;
            } while ((int)uVar7 < param_5);
          }
          iVar8 = iVar8 + 1;
        } while (iVar8 < *(int *)(param_1 + 0xc));
      }
      uVar6 = (ulonglong)(iVar5 + 1U);
      *(undefined4 *)(param_1 + 0x610 + (longlong)iVar5 * 4) =
           *(undefined4 *)(param_6 + (longlong)iVar5 * 4);
    } while ((int)(iVar5 + 1U) < *(int *)(param_1 + 8));
  }
                    /* WARNING: Could not recover jumptable at 0x000180012d3a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x3340));
  return;
}



/* ========================================================================
   ENTRY: 180012d50
   NAME : LoadRouterControlBit
   SIG  : undefined __fastcall LoadRouterControlBit(longlong param_1, int param_2, uint param_3, int param_4)
   ======================================================================== */

void LoadRouterControlBit(longlong param_1,int param_2,uint param_3,int param_4)

{
  byte *pbVar1;
  
                    /* 0x12d50  41  LoadRouterControlBit */
  if (param_1 == 0) {
    param_1 = (&DAT_18001f840)[param_2];
  }
  if (param_4 != 0) {
    LOCK();
    pbVar1 = (byte *)(param_1 + 4 + ((longlong)(int)param_3 >> 3));
    *pbVar1 = *pbVar1 | '\x01' << (param_3 & 7);
    UNLOCK();
    return;
  }
  LOCK();
  pbVar1 = (byte *)(param_1 + 4 + ((longlong)(int)param_3 >> 3));
  *pbVar1 = *pbVar1 & ~('\x01' << (param_3 & 7));
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180012d80
   NAME : FUN_180012d80
   SIG  : undefined __fastcall FUN_180012d80(longlong param_1)
   ======================================================================== */

void FUN_180012d80(longlong param_1)

{
  double dVar1;
  double dVar2;
  undefined8 uVar3;
  int iVar4;
  longlong lVar5;
  double dVar6;
  
  iVar4 = (int)((double)*(int *)(param_1 + 0xc) * *(double *)(param_1 + 0x68));
  *(int *)(param_1 + 0xa0) = iVar4;
  uVar3 = malloc0(iVar4 * 8 + 8);
  *(undefined8 *)(param_1 + 0xa8) = uVar3;
  dVar1 = DAT_180018de0;
  if (*(int *)(param_1 + 100) == 0) {
    *(undefined8 *)(param_1 + 0xb8) = 0;
    *(double *)(param_1 + 0xb0) = dVar1 / (double)*(int *)(param_1 + 0xa0);
    dVar2 = DAT_180018dc8;
    dVar1 = DAT_180018db0;
    if (-1 < *(int *)(param_1 + 0xa0)) {
      dVar6 = 0.0;
      iVar4 = 0;
      do {
        dVar6 = cos(dVar6);
        lVar5 = (longlong)iVar4;
        iVar4 = iVar4 + 1;
        *(double *)(*(longlong *)(param_1 + 0xa8) + lVar5 * 8) = (dVar2 - dVar6) * dVar1;
        dVar6 = *(double *)(param_1 + 0xb8) + *(double *)(param_1 + 0xb0);
        *(double *)(param_1 + 0xb8) = dVar6;
      } while (iVar4 <= *(int *)(param_1 + 0xa0));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180012e70
   NAME : FUN_180012e70
   SIG  : undefined __fastcall FUN_180012e70(longlong param_1)
   ======================================================================== */

void FUN_180012e70(longlong param_1)

{
  double dVar1;
  double dVar2;
  undefined8 uVar3;
  int iVar4;
  longlong lVar5;
  double dVar6;
  
  iVar4 = (int)((double)*(int *)(param_1 + 0xc) * *(double *)(param_1 + 0x68));
  *(int *)(param_1 + 0xc0) = iVar4;
  uVar3 = malloc0(iVar4 * 8 + 8);
  *(undefined8 *)(param_1 + 200) = uVar3;
  dVar6 = DAT_180018de0;
  if (*(int *)(param_1 + 100) == 0) {
    *(undefined8 *)(param_1 + 0xd8) = 0x400921fb54442d18;
    *(double *)(param_1 + 0xd0) = dVar6 / (double)*(int *)(param_1 + 0xc0);
    dVar2 = DAT_180018dc8;
    dVar1 = DAT_180018db0;
    if (-1 < *(int *)(param_1 + 0xc0)) {
      iVar4 = 0;
      do {
        dVar6 = cos(dVar6);
        lVar5 = (longlong)iVar4;
        iVar4 = iVar4 + 1;
        *(double *)(*(longlong *)(param_1 + 200) + lVar5 * 8) = (dVar2 - dVar6) * dVar1;
        dVar6 = *(double *)(param_1 + 0xd8) - *(double *)(param_1 + 0xd0);
        *(double *)(param_1 + 0xd8) = dVar6;
      } while (iVar4 <= *(int *)(param_1 + 0xc0));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180012f70
   NAME : FUN_180012f70
   SIG  : undefined __fastcall FUN_180012f70(longlong param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180012f70(longlong param_1)

{
  double dVar1;
  double dVar2;
  
  dVar2 = DAT_180018dd0 / (double)*(int *)(param_1 + 0x60);
  dVar1 = (double)round((dVar2 - *(double *)(param_1 + 0x68)) * (double)*(int *)(param_1 + 0xc));
  dVar2 = dVar2 * _DAT_180018dd8;
  *(int *)(param_1 + 0xe0) = (int)dVar1;
  dVar2 = (double)round((double)*(int *)(param_1 + 0xc) * (dVar2 - *(double *)(param_1 + 0x68)));
  *(int *)(param_1 + 0xe4) = (int)dVar2;
  return;
}



/* ========================================================================
   ENTRY: 180012ff0
   NAME : FUN_180012ff0
   SIG  : undefined __fastcall FUN_180012ff0(int param_1)
   ======================================================================== */

void FUN_180012ff0(int param_1)

{
  double dVar1;
  int iVar2;
  longlong lVar3;
  int iVar4;
  int iVar5;
  longlong lVar6;
  double dVar7;
  double dVar8;
  double dVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  
  lVar3 = (&DAT_1800202e0)[param_1];
  if ((*(int *)(lVar3 + 4) != 0) || (*(int *)(lVar3 + 8) != 0)) {
    dVar8 = *(double *)(lVar3 + 0x70);
    dVar7 = cos(dVar8);
    *(double *)(lVar3 + 0x78) = dVar7;
    dVar8 = sin(dVar8);
    iVar5 = 0;
    *(double *)(lVar3 + 0x80) = dVar8;
    dVar8 = DAT_180018df0;
    if (0 < *(int *)(lVar3 + 0x10)) {
      uVar12 = (uint)DAT_180018ea0;
      uVar13 = (uint)((ulonglong)DAT_180018ea0 >> 0x20);
      do {
        EnterCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
        dVar7 = *(double *)(lVar3 + 0x78);
        dVar1 = *(double *)(lVar3 + 0x80);
        uVar10 = SUB84(dVar1,0) ^ uVar12;
        uVar11 = (uint)((ulonglong)dVar1 >> 0x20) ^ uVar13;
        *(double *)(lVar3 + 0xf0) = dVar7;
        dVar9 = *(double *)(lVar3 + 0x88) + *(double *)(lVar3 + 0x70);
        *(ulonglong *)(lVar3 + 0xf8) = CONCAT44(uVar11,uVar10);
        *(double *)(lVar3 + 0x78) =
             *(double *)(lVar3 + 0x90) * dVar7 - *(double *)(lVar3 + 0x98) * dVar1;
        *(double *)(lVar3 + 0x80) =
             *(double *)(lVar3 + 0x90) * dVar1 + *(double *)(lVar3 + 0x98) * dVar7;
        *(double *)(lVar3 + 0x70) = dVar9;
        if (dVar8 <= dVar9) {
          dVar9 = dVar9 - dVar8;
          *(double *)(lVar3 + 0x70) = dVar9;
        }
        if (dVar9 < 0.0) {
          *(double *)(lVar3 + 0x70) = dVar9 + dVar8;
        }
        iVar4 = *(int *)(lVar3 + 0xe8);
        if (iVar4 == 0) {
          *(undefined8 *)(lVar3 + 0x18) = 0;
          *(undefined8 *)(lVar3 + 0x20) = 0;
          if (((*(int *)(lVar3 + 0x104) != 0) || (*(int *)(lVar3 + 0x108) != 0)) ||
             (*(int *)(lVar3 + 0x10c) != 0)) {
            *(undefined4 *)(lVar3 + 0xe8) = 1;
          }
LAB_18001329c:
          *(undefined4 *)(lVar3 + 0x100) = 0;
        }
        else if (iVar4 == 1) {
          iVar4 = *(int *)(lVar3 + 0x100);
          *(double *)(lVar3 + 0x18) =
               dVar7 * *(double *)(*(longlong *)(lVar3 + 0xa8) + (longlong)iVar4 * 8);
          dVar7 = *(double *)(*(longlong *)(lVar3 + 0xa8) + (longlong)iVar4 * 8);
          *(int *)(lVar3 + 0x100) = iVar4 + 1;
          *(double *)(lVar3 + 0x20) = (double)CONCAT44(uVar11,uVar10) * dVar7;
          if (*(int *)(lVar3 + 0xa0) < iVar4 + 1) {
            if (((*(int *)(lVar3 + 0x104) == 0) && (*(int *)(lVar3 + 0x108) == 0)) &&
               (*(int *)(lVar3 + 0x10c) == 0)) {
              *(undefined4 *)(lVar3 + 0xe8) = 3;
              goto LAB_18001329c;
            }
            *(undefined4 *)(lVar3 + 0xe8) = 2;
            if (*(int *)(lVar3 + 0x108) != 0) {
              *(undefined4 *)(lVar3 + 0x100) = *(undefined4 *)(lVar3 + 0xe0);
            }
            if (*(int *)(lVar3 + 0x10c) != 0) {
              *(undefined4 *)(lVar3 + 0x100) = *(undefined4 *)(lVar3 + 0xe4);
            }
          }
        }
        else if (iVar4 == 2) {
          *(double *)(lVar3 + 0x18) = dVar7;
          *(ulonglong *)(lVar3 + 0x20) = CONCAT44(uVar11,uVar10);
          if (((*(int *)(lVar3 + 0x100) < 1) ||
              (iVar4 = *(int *)(lVar3 + 0x100) + -1, *(int *)(lVar3 + 0x100) = iVar4, iVar4 < 1)) &&
             (*(int *)(lVar3 + 0x104) == 0)) {
            *(undefined4 *)(lVar3 + 0xe8) = 3;
            goto LAB_18001329c;
          }
        }
        else if (iVar4 == 3) {
          iVar4 = *(int *)(lVar3 + 0x100);
          *(double *)(lVar3 + 0x18) =
               dVar7 * *(double *)(*(longlong *)(lVar3 + 200) + (longlong)iVar4 * 8);
          dVar7 = *(double *)(*(longlong *)(lVar3 + 200) + (longlong)iVar4 * 8);
          *(int *)(lVar3 + 0x100) = iVar4 + 1;
          *(double *)(lVar3 + 0x20) = (double)CONCAT44(uVar11,uVar10) * dVar7;
          if (*(int *)(lVar3 + 0xc0) < iVar4 + 1) {
            *(undefined8 *)(lVar3 + 0x108) = 0;
            *(undefined4 *)(lVar3 + 0x100) = 0;
            if (*(int *)(lVar3 + 0x104) == 0) {
              *(undefined4 *)(lVar3 + 0xe8) = 0;
            }
            else {
              *(undefined4 *)(lVar3 + 0xe8) = 1;
            }
          }
        }
        iVar4 = iVar5 * 2;
        if (*(int *)(lVar3 + 4) != 0) {
          *(double *)(*(longlong *)(lVar3 + 0x30) + (longlong)iVar4 * 8) =
               *(double *)(lVar3 + 0x50) * *(double *)(lVar3 + 0x18);
          *(double *)(*(longlong *)(lVar3 + 0x30) + 8 + (longlong)iVar4 * 8) =
               *(double *)(lVar3 + 0x50) * *(double *)(lVar3 + 0x20);
        }
        if (*(int *)(lVar3 + 8) != 0) {
          iVar2 = *(int *)(lVar3 + 0x40);
          lVar6 = (longlong)iVar4;
          *(double *)(*(longlong *)(lVar3 + 0x38) + lVar6 * 8) =
               *(double *)(lVar3 + 0x58) * *(double *)(lVar3 + 0x18);
          dVar7 = *(double *)(lVar3 + 0x20);
          if (iVar2 == 0) {
            *(double *)(*(longlong *)(lVar3 + 0x38) + 8 + lVar6 * 8) =
                 *(double *)(lVar3 + 0x58) * dVar7;
          }
          else {
            *(double *)(*(longlong *)(lVar3 + 0x38) + 8 + lVar6 * 8) =
                 (double)CONCAT44((uint)((ulonglong)dVar7 >> 0x20) ^ uVar13,SUB84(dVar7,0) ^ uVar12)
                 * *(double *)(lVar3 + 0x58);
          }
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)(lVar3 + 0x118));
        iVar5 = iVar5 + 1;
      } while (iVar5 < *(int *)(lVar3 + 0x10));
    }
  }
  if (*(int *)(lVar3 + 4) == 0) {
    if (*(void **)(lVar3 + 0x30) != *(void **)(lVar3 + 0x28)) {
      memcpy(*(void **)(lVar3 + 0x30),*(void **)(lVar3 + 0x28),(longlong)*(int *)(lVar3 + 0x10) << 4
            );
    }
  }
  if (*(int *)(lVar3 + 8) == 0) {
    if (*(void **)(lVar3 + 0x38) != *(void **)(lVar3 + 0x28)) {
      memcpy(*(void **)(lVar3 + 0x38),*(void **)(lVar3 + 0x28),(longlong)*(int *)(lVar3 + 0x10) << 4
            );
      return;
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800133b0
   NAME : SetSidetoneSelectKey
   SIG  : undefined __fastcall SetSidetoneSelectKey(int param_1, undefined4 param_2)
   ======================================================================== */

void SetSidetoneSelectKey(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x133b0  213  SetSidetoneSelectKey */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 0x110) = param_2;
                    /* WARNING: Could not recover jumptable at 0x0001800133f8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013400
   NAME : keySidetone
   SIG  : undefined __fastcall keySidetone(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void keySidetone(int param_1,int param_2,undefined4 param_3)

{
  longlong lVar1;
  
                    /* 0x13400  290  keySidetone */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  if (param_2 == *(int *)(lVar1 + 0x110)) {
    *(undefined4 *)(lVar1 + 0x104) = param_3;
  }
                    /* WARNING: Could not recover jumptable at 0x000180013446. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013450
   NAME : makedotSidetone
   SIG  : undefined __fastcall makedotSidetone(int param_1)
   ======================================================================== */

void makedotSidetone(int param_1)

{
  longlong lVar1;
  
                    /* 0x13450  292  makedotSidetone */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 0x108) = 1;
                    /* WARNING: Could not recover jumptable at 0x000180013490. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 1800134a0
   NAME : makedashSidetone
   SIG  : undefined __fastcall makedashSidetone(int param_1)
   ======================================================================== */

void makedashSidetone(int param_1)

{
  longlong lVar1;
  
                    /* 0x134a0  291  makedashSidetone */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 0x10c) = 1;
                    /* WARNING: Could not recover jumptable at 0x0001800134e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 1800134f0
   NAME : SetCWtxIQpolarity
   SIG  : undefined __fastcall SetCWtxIQpolarity(int param_1, undefined4 param_2)
   ======================================================================== */

void SetCWtxIQpolarity(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x134f0  112  SetCWtxIQpolarity */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 0x40) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180013535. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013540
   NAME : SetSidetoneVolume
   SIG  : undefined __fastcall SetSidetoneVolume(int param_1, undefined8 param_2)
   ======================================================================== */

void SetSidetoneVolume(int param_1,undefined8 param_2)

{
  longlong lVar1;
  
                    /* 0x13540  214  SetSidetoneVolume */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined8 *)(lVar1 + 0x50) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180013588. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013590
   NAME : SetCWtxVolume
   SIG  : undefined __fastcall SetCWtxVolume(int param_1, double param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void SetCWtxVolume(int param_1,double param_2)

{
  longlong lVar1;
  double dVar2;
  
                    /* 0x13590  114  SetCWtxVolume */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  dVar2 = _DAT_180018dc0;
  *(double *)(lVar1 + 0x58) = param_2;
  if (dVar2 < param_2) {
    *(undefined8 *)(lVar1 + 0x58) = 0x3feff7ced916872b;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800135f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013600
   NAME : SetSidetoneWPM
   SIG  : undefined __fastcall SetSidetoneWPM(int param_1, undefined4 param_2)
   ======================================================================== */

void SetSidetoneWPM(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x13600  215  SetSidetoneWPM */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 0x60) = param_2;
  FUN_180012f70(lVar1);
                    /* WARNING: Could not recover jumptable at 0x00018001364d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013660
   NAME : SetSidetoneRun
   SIG  : undefined __fastcall SetSidetoneRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetSidetoneRun(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x13660  212  SetSidetoneRun */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 4) = param_2;
                    /* WARNING: Could not recover jumptable at 0x0001800136a5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 1800136b0
   NAME : SetCWtxRun
   SIG  : undefined __fastcall SetCWtxRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetCWtxRun(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x136b0  113  SetCWtxRun */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(undefined4 *)(lVar1 + 8) = param_2;
                    /* WARNING: Could not recover jumptable at 0x0001800136f5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013700
   NAME : SetSidetonePitch
   SIG  : undefined __fastcall SetSidetonePitch(int param_1, double param_2)
   ======================================================================== */

void SetSidetonePitch(int param_1,double param_2)

{
  longlong lVar1;
  double dVar2;
  double dVar3;
  
                    /* 0x13700  211  SetSidetonePitch */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  *(double *)(lVar1 + 0x48) = param_2;
  *(undefined8 *)(lVar1 + 0x70) = 0;
  dVar2 = cos(0.0);
  *(double *)(lVar1 + 0x78) = dVar2;
  dVar3 = sin(0.0);
  dVar2 = param_2 * DAT_180018df0;
  *(double *)(lVar1 + 0x80) = dVar3;
  dVar2 = dVar2 / (double)*(int *)(lVar1 + 0xc);
  *(double *)(lVar1 + 0x88) = dVar2;
  dVar3 = cos(dVar2);
  *(double *)(lVar1 + 0x90) = dVar3;
  dVar2 = sin(dVar2);
  *(double *)(lVar1 + 0x98) = dVar2;
                    /* WARNING: Could not recover jumptable at 0x0001800137aa. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 1800137c0
   NAME : SetSidetoneEdgetype
   SIG  : undefined __fastcall SetSidetoneEdgetype(int param_1, undefined4 param_2)
   ======================================================================== */

void SetSidetoneEdgetype(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x137c0  210  SetSidetoneEdgetype */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  _aligned_free(*(void **)(lVar1 + 0xa8));
  _aligned_free(*(void **)(lVar1 + 200));
  *(undefined4 *)(lVar1 + 100) = param_2;
  FUN_180012d80(lVar1);
  FUN_180012e70(lVar1);
                    /* WARNING: Could not recover jumptable at 0x00018001382f. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 180013840
   NAME : SetSidetoneEdgelength
   SIG  : undefined __fastcall SetSidetoneEdgelength(int param_1, undefined8 param_2)
   ======================================================================== */

void SetSidetoneEdgelength(int param_1,undefined8 param_2)

{
  longlong lVar1;
  
                    /* 0x13840  209  SetSidetoneEdgelength */
  lVar1 = (&DAT_1800202e0)[param_1];
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  _aligned_free(*(void **)(lVar1 + 0xa8));
  _aligned_free(*(void **)(lVar1 + 200));
  *(undefined8 *)(lVar1 + 0x68) = param_2;
  FUN_180012d80(lVar1);
  FUN_180012e70(lVar1);
                    /* WARNING: Could not recover jumptable at 0x0001800138b2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x118));
  return;
}



/* ========================================================================
   ENTRY: 1800138c0
   NAME : InboundBlock
   SIG  : undefined __fastcall InboundBlock(int param_1, int param_2, undefined8 * param_3)
   ======================================================================== */

void InboundBlock(int param_1,int param_2,undefined8 *param_3)

{
  int iVar1;
  undefined8 uVar2;
  undefined *puVar3;
  undefined *puVar4;
  int iVar5;
  int iVar6;
  bool bVar7;
  
                    /* 0x138c0  38  InboundBlock */
  puVar3 = PTR_DAT_18001e090;
  if (param_1 == 0) {
    xdivEXT();
    param_3 = (undefined8 *)PTR_DAT_18001e090;
  }
  else {
    if (param_1 == 1) {
      iVar6 = *(int *)(PTR_DAT_18001e080 + 4);
      if (iVar6 < *(int *)(PTR_DAT_18001e080 + 8) + iVar6) {
        iVar6 = iVar6 * *(int *)(PTR_DAT_18001e080 + 0xc);
      }
      else {
        iVar6 = -1;
      }
      iVar5 = *(int *)(PTR_DAT_18001e090 + 0xc);
      do {
        LOCK();
        iVar1 = *(int *)(puVar3 + 0xc);
        bVar7 = iVar5 == iVar1;
        if (bVar7) {
          *(int *)(puVar3 + 0xc) = iVar5;
          iVar1 = iVar5;
        }
        iVar5 = iVar1;
        puVar4 = PTR_DAT_18001e090;
        UNLOCK();
      } while (!bVar7);
      uVar2 = param_3[iVar5];
      iVar5 = *(int *)(PTR_DAT_18001e090 + 8);
      do {
        LOCK();
        iVar1 = *(int *)(puVar4 + 8);
        bVar7 = iVar5 == iVar1;
        if (bVar7) {
          *(int *)(puVar4 + 8) = iVar5;
          iVar1 = iVar5;
        }
        iVar5 = iVar1;
        UNLOCK();
      } while (!bVar7);
                    /* WARNING: Could not recover jumptable at 0x000180013983. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      pscc(iVar6,param_2,param_3[iVar5],uVar2);
      return;
    }
    if (param_1 == 2) {
      Inbound(0,param_2,(void *)*param_3);
      Inbound(1,param_2,(void *)param_3[1]);
      return;
    }
    if (param_1 != 3) {
      return;
    }
  }
  Inbound(0,param_2,(void *)*param_3);
  return;
}



/* ========================================================================
   ENTRY: 1800139c0
   NAME : SetPSTxIdx
   SIG  : undefined __fastcall SetPSTxIdx(int param_1, undefined4 param_2)
   ======================================================================== */

void SetPSTxIdx(int param_1,undefined4 param_2)

{
                    /* 0x139c0  181  SetPSTxIdx */
  LOCK();
  *(undefined4 *)(PTR_DAT_18001e090 + (longlong)param_1 * 8 + 8) = param_2;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 1800139d0
   NAME : SetPSRxIdx
   SIG  : undefined __fastcall SetPSRxIdx(int param_1, undefined4 param_2)
   ======================================================================== */

void SetPSRxIdx(int param_1,undefined4 param_2)

{
                    /* 0x139d0  180  SetPSRxIdx */
  LOCK();
  *(undefined4 *)(PTR_DAT_18001e090 + (longlong)param_1 * 8 + 0xc) = param_2;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 1800139e0
   NAME : FUN_1800139e0
   SIG  : undefined __fastcall FUN_1800139e0(int param_1)
   ======================================================================== */

void FUN_1800139e0(int param_1)

{
  undefined *puVar1;
  longlong lVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  
  puVar1 = PTR_DAT_18001e098;
  if ((-1 < param_1) && (param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
    lVar2 = (longlong)param_1;
    if (*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8) != (void *)0x0) {
      bVar7 = *(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x1c) != 0;
      if (bVar7) {
        iVar4 = 2;
        uVar5 = -(uint)(*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x20) != 0) & 2;
        uVar6 = uVar5;
      }
      else {
        uVar5 = 0;
        iVar4 = 1;
        uVar6 = 1;
      }
      if (*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x30) != iVar4) {
        SetAAudioMixState(*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8),0,0,(uint)!bVar7);
        SetAAudioMixState(*(void **)(puVar1 + lVar2 * 0x30 + 8),0,1,(uint)bVar7);
        *(int *)(puVar1 + lVar2 * 0x30 + 0x30) = iVar4;
      }
      if (*(uint *)(puVar1 + lVar2 * 0x30 + 0x34) != uVar6) {
        lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
        if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
          lVar3 = DAT_18001e140;
        }
        if (bVar7) {
          LOCK();
          *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffe;
          UNLOCK();
        }
        else {
          LOCK();
          *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 1;
          UNLOCK();
        }
        lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
        if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
          lVar3 = DAT_18001e140;
        }
        if (uVar5 == 0) {
          LOCK();
          *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffd;
          UNLOCK();
        }
        else {
          LOCK();
          *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 2;
          UNLOCK();
        }
        *(uint *)(puVar1 + lVar2 * 0x30 + 0x34) = uVar6;
      }
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180013b80
   NAME : SendpOutboundTCIRxAudio
   SIG  : undefined __fastcall SendpOutboundTCIRxAudio(undefined8 param_1)
   ======================================================================== */

void SendpOutboundTCIRxAudio(undefined8 param_1)

{
                    /* 0x13b80  59  SendpOutboundTCIRxAudio */
  *(undefined8 *)PTR_DAT_18001e098 = param_1;
  return;
}



/* ========================================================================
   ENTRY: 180013b90
   NAME : FUN_180013b90
   SIG  : undefined __fastcall FUN_180013b90(int param_1, int param_2)
   ======================================================================== */

void FUN_180013b90(int param_1,int param_2)

{
  undefined8 *puVar1;
  int iVar2;
  int iVar3;
  undefined *puVar4;
  int iVar5;
  undefined8 uVar6;
  undefined8 uVar7;
  longlong lVar8;
  longlong lVar9;
  
  puVar4 = PTR_DAT_18001e098;
  if ((-1 < param_1) && (lVar9 = (longlong)param_1, param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
    puVar1 = (undefined8 *)(PTR_DAT_18001e098 + lVar9 * 0x30 + 8);
    if (((void *)*puVar1 == (void *)0x0) ||
       (*(int *)(PTR_DAT_18001e098 + lVar9 * 0x30 + 0x10) == param_2)) {
      *(int *)(PTR_DAT_18001e098 + lVar9 * 0x30 + 0x10) = param_2;
    }
    else {
      *(int *)(PTR_DAT_18001e098 + lVar9 * 0x30 + 0x10) = param_2;
      SetAAudioMixState((void *)*puVar1,0,0,0);
      uVar7 = 0;
      SetAAudioMixState(*(void **)(puVar4 + lVar9 * 0x30 + 8),0,1,0);
      *(undefined4 *)(puVar4 + lVar9 * 0x30 + 0x30) = 0;
      FUN_1800028c0(*(void **)(puVar4 + lVar9 * 0x30 + 8),uVar7,
                    *(undefined4 *)(puVar4 + lVar9 * 0x30 + 0x10));
      lVar8 = *(longlong *)(puVar4 + lVar9 * 0x30 + 8);
      if (*(longlong *)(puVar4 + lVar9 * 0x30 + 8) == 0) {
        lVar8 = DAT_18001e140;
      }
      *(undefined4 *)(lVar8 + 0xd68) = *(undefined4 *)(puVar4 + lVar9 * 0x30 + 0x10);
      destroy_resample(*(undefined8 *)(lVar8 + 0xc68));
      uVar7 = DAT_180018dc8;
      iVar2 = *(int *)(lVar8 + 0xde8);
      iVar3 = *(int *)(lVar8 + 0xd68);
      if (iVar2 < iVar3) {
        iVar5 = (iVar3 / iVar2) * *(int *)(lVar8 + 0x8c);
      }
      else {
        iVar5 = *(int *)(lVar8 + 0x8c) / (iVar2 / iVar3);
      }
      uVar6 = create_resample(iVar3 != iVar2,iVar5,0,*(undefined8 *)(lVar8 + 0xdf0),iVar3,iVar2,0,0,
                              DAT_180018dc8);
      *(undefined8 *)(lVar8 + 0xc68) = uVar6;
      lVar8 = *(longlong *)(puVar4 + lVar9 * 0x30 + 8);
      if (*(longlong *)(puVar4 + lVar9 * 0x30 + 8) == 0) {
        lVar8 = DAT_18001e140;
      }
      *(undefined4 *)(lVar8 + 0xd6c) = *(undefined4 *)(puVar4 + lVar9 * 0x30 + 0x14);
      destroy_resample(*(undefined8 *)(lVar8 + 0xc70));
      iVar2 = *(int *)(lVar8 + 0xde8);
      iVar3 = *(int *)(lVar8 + 0xd6c);
      if (iVar2 < iVar3) {
        iVar5 = (iVar3 / iVar2) * *(int *)(lVar8 + 0x8c);
      }
      else {
        iVar5 = *(int *)(lVar8 + 0x8c) / (iVar2 / iVar3);
      }
      uVar7 = create_resample(iVar3 != iVar2,iVar5,0,*(undefined8 *)(lVar8 + 0xdf8),iVar3,iVar2,0,0,
                              uVar7);
      *(undefined8 *)(lVar8 + 0xc70) = uVar7;
      FUN_1800139e0(param_1);
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180013db0
   NAME : SetTCIRxAudioMox
   SIG  : undefined __fastcall SetTCIRxAudioMox(uint param_1, undefined4 param_2)
   ======================================================================== */

void SetTCIRxAudioMox(uint param_1,undefined4 param_2)

{
  undefined *puVar1;
  longlong lVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  
                    /* 0x13db0  218  SetTCIRxAudioMox */
  if ((param_1 < 0x10) &&
     (*(undefined4 *)(PTR_DAT_18001e098 + (longlong)(int)param_1 * 0x30 + 0x1c) = param_2,
     puVar1 = PTR_DAT_18001e098, (int)param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
    if ((-1 < (int)param_1) && ((int)param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
      lVar2 = (longlong)(int)param_1;
      if (*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8) != (void *)0x0) {
        bVar7 = *(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x1c) != 0;
        if (bVar7) {
          iVar4 = 2;
          uVar5 = -(uint)(*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x20) != 0) & 2;
          uVar6 = uVar5;
        }
        else {
          uVar5 = 0;
          iVar4 = 1;
          uVar6 = 1;
        }
        if (*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x30) != iVar4) {
          SetAAudioMixState(*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8),0,0,(uint)!bVar7);
          SetAAudioMixState(*(void **)(puVar1 + lVar2 * 0x30 + 8),0,1,(uint)bVar7);
          *(int *)(puVar1 + lVar2 * 0x30 + 0x30) = iVar4;
        }
        if (*(uint *)(puVar1 + lVar2 * 0x30 + 0x34) != uVar6) {
          lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
          if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
            lVar3 = DAT_18001e140;
          }
          if (bVar7) {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffe;
            UNLOCK();
          }
          else {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 1;
            UNLOCK();
          }
          lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
          if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
            lVar3 = DAT_18001e140;
          }
          if (uVar5 == 0) {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffd;
            UNLOCK();
          }
          else {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 2;
            UNLOCK();
          }
          *(uint *)(puVar1 + lVar2 * 0x30 + 0x34) = uVar6;
        }
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180013de0
   NAME : SetTCIRxAudioMon
   SIG  : undefined __fastcall SetTCIRxAudioMon(uint param_1, undefined4 param_2)
   ======================================================================== */

void SetTCIRxAudioMon(uint param_1,undefined4 param_2)

{
  undefined *puVar1;
  longlong lVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  
                    /* 0x13de0  216  SetTCIRxAudioMon */
  if ((param_1 < 0x10) &&
     (*(undefined4 *)(PTR_DAT_18001e098 + (longlong)(int)param_1 * 0x30 + 0x20) = param_2,
     puVar1 = PTR_DAT_18001e098, (int)param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
    if ((-1 < (int)param_1) && ((int)param_1 < *(int *)(PTR_DAT_18001e080 + 4))) {
      lVar2 = (longlong)(int)param_1;
      if (*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8) != (void *)0x0) {
        bVar7 = *(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x1c) != 0;
        if (bVar7) {
          iVar4 = 2;
          uVar5 = -(uint)(*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x20) != 0) & 2;
          uVar6 = uVar5;
        }
        else {
          uVar5 = 0;
          iVar4 = 1;
          uVar6 = 1;
        }
        if (*(int *)(PTR_DAT_18001e098 + lVar2 * 0x30 + 0x30) != iVar4) {
          SetAAudioMixState(*(void **)(PTR_DAT_18001e098 + lVar2 * 0x30 + 8),0,0,(uint)!bVar7);
          SetAAudioMixState(*(void **)(puVar1 + lVar2 * 0x30 + 8),0,1,(uint)bVar7);
          *(int *)(puVar1 + lVar2 * 0x30 + 0x30) = iVar4;
        }
        if (*(uint *)(puVar1 + lVar2 * 0x30 + 0x34) != uVar6) {
          lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
          if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
            lVar3 = DAT_18001e140;
          }
          if (bVar7) {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffe;
            UNLOCK();
          }
          else {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 1;
            UNLOCK();
          }
          lVar3 = *(longlong *)(puVar1 + lVar2 * 0x30 + 8);
          if (*(longlong *)(puVar1 + lVar2 * 0x30 + 8) == 0) {
            lVar3 = DAT_18001e140;
          }
          if (uVar5 == 0) {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) & 0xfffffffd;
            UNLOCK();
          }
          else {
            LOCK();
            *(uint *)(lVar3 + 0x1b0) = *(uint *)(lVar3 + 0x1b0) | 2;
            UNLOCK();
          }
          *(uint *)(puVar1 + lVar2 * 0x30 + 0x34) = uVar6;
        }
      }
    }
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180013e10
   NAME : SetTCIRxAudioMonVol
   SIG  : undefined __fastcall SetTCIRxAudioMonVol(uint param_1, double param_2)
   ======================================================================== */

void SetTCIRxAudioMonVol(uint param_1,double param_2)

{
  longlong lVar1;
  
                    /* 0x13e10  217  SetTCIRxAudioMonVol */
  if (param_1 < 0x10) {
    lVar1 = *(longlong *)(PTR_DAT_18001e098 + (longlong)(int)param_1 * 0x30 + 8);
    *(double *)(PTR_DAT_18001e098 + (longlong)(int)param_1 * 0x30 + 0x28) = param_2;
    if (lVar1 != 0) {
      EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0xc40));
      *(double *)(lVar1 + 0x1c0) = param_2;
      *(double *)(lVar1 + 0x2c0) = param_2 * *(double *)(lVar1 + 0x3b8);
      LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0xc40));
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 180013ea0
   NAME : print_cmbuff_parameters
   SIG  : undefined __fastcall print_cmbuff_parameters(char * param_1, uint param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

void print_cmbuff_parameters(char *param_1,uint param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  FILE *_File;
  ulonglong uVar2;
  
                    /* 0x13ea0  295  print_cmbuff_parameters */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)(int)param_2 * 8 + 0x2b0);
  _File = fopen(param_1,"a");
  FUN_180006cb0(_File,"id                 = %d\n",(ulonglong)param_2,param_4);
  FUN_180006cb0(_File,"accept             = %d\n",(ulonglong)*(uint *)(lVar1 + 0x30),param_4);
  FUN_180006cb0(_File,"max_insize         = %d\n",(ulonglong)*(uint *)(lVar1 + 4),param_4);
  uVar2 = (ulonglong)*(uint *)(lVar1 + 0xc);
  FUN_180006cb0(_File,"outsize            = %d\n",uVar2,param_4);
  FUN_180006cb0(_File,&DAT_180018d5c,uVar2,param_4);
  fflush(_File);
                    /* WARNING: Could not recover jumptable at 0x000180013f46. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  fclose(_File);
  return;
}



/* ========================================================================
   ENTRY: 180013f50
   NAME : FUN_180013f50
   SIG  : undefined __fastcall FUN_180013f50(void * param_1)
   ======================================================================== */

void FUN_180013f50(void *param_1)

{
  FILE *_File;
  
  _File = fopen("AudioFile","wb");
  fwrite(param_1,4,(longlong)DAT_18001f4c8,_File);
  fflush(_File);
  fclose(_File);
  free(DAT_180020260);
                    /* WARNING: Could not recover jumptable at 0x000180013fb5. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _endthread();
  return;
}



/* ========================================================================
   ENTRY: 180013fc0
   NAME : WriteAudio
   SIG  : undefined __fastcall WriteAudio(double param_1, int param_2, int param_3, longlong param_4, int param_5)
   ======================================================================== */

void WriteAudio(double param_1,int param_2,int param_3,longlong param_4,int param_5)

{
  double dVar1;
  int iVar2;
  double dVar3;
  int iVar4;
  void *_ArgList;
  int iVar5;
  int iVar6;
  double dVar7;
  
                    /* 0x13fc0  247  WriteAudio */
  if (DAT_18001f4cc == 0) {
    DAT_180020250 = (int)((double)param_2 * param_1);
    if (2 < param_5) {
      DAT_180020250 = (int)((double)param_2 * param_1) * 2;
    }
    DAT_180020260 = calloc((longlong)DAT_180020250,4);
    DAT_18001f4cc = 1;
  }
  _ArgList = DAT_180020260;
  iVar4 = DAT_180020250;
  dVar3 = DAT_180018e48;
  iVar6 = 0;
  iVar5 = DAT_18001f4c8;
  if (0 < param_3) {
    do {
      if (iVar5 < iVar4) {
        iVar2 = iVar5;
        if (param_5 == 0) {
          dVar7 = *(double *)(param_4 + (longlong)(iVar6 * 2) * 8);
        }
        else if (param_5 == 1) {
          dVar7 = *(double *)(param_4 + 8 + (longlong)(iVar6 * 2) * 8);
        }
        else if (param_5 == 2) {
          dVar7 = *(double *)(param_4 + (longlong)(iVar6 * 2) * 8);
          dVar1 = *(double *)(param_4 + 8 + (longlong)(iVar6 * 2) * 8);
          dVar7 = dVar1 * dVar1 + dVar7 * dVar7;
          if (dVar7 < 0.0) {
            dVar7 = sqrt(dVar7);
          }
          else {
            dVar7 = SQRT(dVar7);
          }
        }
        else {
          if (param_5 != 3) goto LAB_18001412f;
          *(int *)((longlong)_ArgList + (longlong)iVar5 * 4) =
               (int)(*(double *)(param_4 + (longlong)(iVar6 * 2) * 8) * dVar3);
          dVar7 = *(double *)(param_4 + 8 + (longlong)(iVar6 * 2) * 8);
          iVar2 = iVar5 + 1;
        }
        iVar5 = iVar2 + 1;
        DAT_18001f4c8 = iVar5;
        *(int *)((longlong)_ArgList + (longlong)iVar2 * 4) = (int)(dVar7 * dVar3);
      }
LAB_18001412f:
      iVar6 = iVar6 + 1;
    } while (iVar6 < param_3);
  }
  if ((iVar5 == iVar4) && (DAT_18001f888 == 0)) {
    DAT_18001f888 = 1;
                    /* WARNING: Could not recover jumptable at 0x000180014186. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    _beginthread(FUN_180013f50,0,_ArgList);
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800141a0
   NAME : FUN_1800141a0
   SIG  : undefined __fastcall FUN_1800141a0(void)
   ======================================================================== */

void FUN_1800141a0(void)

{
  char *pcVar1;
  char cVar2;
  undefined1 *puVar3;
  FILE *pFVar4;
  longlong lVar5;
  int _Value;
  char local_res10 [8];
  char local_18 [16];
  
  _Value = 0;
  if (0 < DAT_1800202c8) {
    do {
      builtin_strncpy(local_res10,"ddc",4);
      _itoa(_Value,local_18,10);
      puVar3 = &stack0x0000000f;
      do {
        pcVar1 = puVar3 + 1;
        puVar3 = puVar3 + 1;
      } while (*pcVar1 != '\0');
      lVar5 = 0;
      do {
        cVar2 = local_18[lVar5];
        puVar3[lVar5] = cVar2;
        lVar5 = lVar5 + 1;
      } while (cVar2 != '\0');
      pFVar4 = fopen(local_res10,"wb");
      fwrite((void *)(&DAT_180020280)[_Value],4,(longlong)DAT_1800202cc,pFVar4);
      fflush(pFVar4);
      fclose(pFVar4);
      free((void *)(&DAT_180020280)[_Value]);
      _Value = _Value + 1;
    } while (_Value < DAT_1800202c8);
  }
  pFVar4 = fopen("mic","wb");
  fwrite(DAT_1800202c0,4,(longlong)DAT_1800202d0,pFVar4);
  fflush(pFVar4);
  fclose(pFVar4);
  free(DAT_1800202c0);
                    /* WARNING: Could not recover jumptable at 0x0001800142e1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _endthread();
  return;
}



/* ========================================================================
   ENTRY: 1800142f0
   NAME : WriteCharFiles
   SIG  : undefined __fastcall WriteCharFiles(int param_1, int param_2, longlong param_3, int param_4)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void WriteCharFiles(int param_1,int param_2,longlong param_3,int param_4)

{
  longlong lVar1;
  uint uVar2;
  int iVar3;
  void *pvVar4;
  int iVar5;
  longlong lVar6;
  int iVar7;
  uint uVar8;
  int iVar9;
  ulonglong uVar10;
  int iVar11;
  ulonglong uVar12;
  int iVar13;
  longlong lVar14;
  double dVar15;
  
                    /* 0x142f0  248  WriteCharFiles */
  iVar7 = 8;
  if (DAT_18001f4c4 == 0) {
    DAT_1800201e8 = param_1 * param_2;
    if (param_2 == 48000) {
      DAT_180020244 = 1;
    }
    else if (param_2 == 96000) {
      DAT_180020244 = 2;
    }
    else if (param_2 == 0x2ee00) {
      DAT_180020244 = 4;
    }
    else if (param_2 == 0x5dc00) {
      DAT_180020244 = 8;
    }
    DAT_180020254 = DAT_1800201e8 / DAT_180020244;
    uVar12 = 0x1f8 / (longlong)(param_4 * 6 + 2);
    uVar2 = (uint)uVar12;
    uVar12 = uVar12 & 0xffffffff;
    DAT_1800201ec = uVar2;
    dVar15 = ceil((double)DAT_1800201e8 / (double)(int)uVar2);
    iVar5 = (int)dVar15;
    uVar10 = 0;
    DAT_1800201fc = iVar5;
    if (0 < param_4) {
      do {
        pvVar4 = calloc((longlong)(iVar5 * (int)uVar12),8);
        iVar5 = DAT_1800201fc;
        uVar2 = DAT_1800201ec;
        uVar12 = (ulonglong)DAT_1800201ec;
        (&DAT_180020200)[uVar10] = pvVar4;
        uVar8 = (int)uVar10 + 1;
        uVar10 = (ulonglong)uVar8;
      } while ((int)uVar8 < param_4);
    }
    DAT_180020248 = calloc((longlong)(int)(iVar5 * uVar2),4);
    DAT_18001f4c4 = 1;
    DAT_180020240 = 0;
    DAT_1800201f4 = 0;
    DAT_1800201f8 = 0;
    DAT_1800201f0 = 0;
  }
  pvVar4 = DAT_180020248;
  if (DAT_1800201f4 < DAT_1800201fc) {
    iVar5 = param_4 * 6;
    iVar3 = DAT_1800201f4 * DAT_1800201ec;
    iVar9 = 0;
    uVar2 = DAT_1800201ec;
    iVar11 = DAT_180020240;
    if (0 < (int)DAT_1800201ec) {
      do {
        uVar12 = 0;
        if (0 < param_4) {
          lVar14 = (longlong)((iVar9 + iVar3) * 2);
          iVar13 = iVar7;
          do {
            lVar1 = (&DAT_180020200)[uVar12];
            uVar8 = (int)uVar12 + 1;
            uVar12 = (ulonglong)uVar8;
            lVar6 = (longlong)iVar13;
            iVar13 = iVar13 + 6;
            *(uint *)(lVar1 + lVar14 * 4) =
                 (uint)CONCAT21(CONCAT11(*(undefined1 *)(param_3 + lVar6),
                                         *(undefined1 *)(param_3 + 1 + lVar6)),
                                *(undefined1 *)(param_3 + 2 + lVar6)) << 8;
            *(uint *)(lVar1 + 4 + lVar14 * 4) =
                 (uint)CONCAT21(CONCAT11(*(undefined1 *)(param_3 + 3 + lVar6),
                                         *(undefined1 *)(param_3 + 4 + lVar6)),
                                *(undefined1 *)(param_3 + 5 + lVar6)) << 8;
            uVar2 = DAT_1800201ec;
          } while ((int)uVar8 < param_4);
        }
        pvVar4 = DAT_180020248;
        DAT_180020240 = iVar11 + 1;
        iVar11 = DAT_180020240;
        if (DAT_180020240 == DAT_180020244) {
          lVar14 = (longlong)DAT_1800201f8;
          DAT_1800201f8 = DAT_1800201f8 + 1;
          iVar11 = 0;
          DAT_180020240 = 0;
          *(uint *)((longlong)DAT_180020248 + lVar14 * 4) =
               (uint)CONCAT11(*(undefined1 *)(param_3 + (iVar5 + iVar7)),
                              *(undefined1 *)(param_3 + 1 + (longlong)(iVar5 + iVar7))) << 0x10;
        }
        iVar9 = iVar9 + 1;
        iVar7 = iVar7 + iVar5 + 2;
      } while (iVar9 < (int)uVar2);
    }
    DAT_1800201f4 = DAT_1800201f4 + 1;
    if (DAT_1800201f4 < DAT_1800201fc) {
      iVar7 = DAT_1800201f4 * uVar2;
      iVar3 = 0;
      iVar9 = 0x208;
      if (0 < (int)uVar2) {
        do {
          uVar12 = 0;
          if (0 < param_4) {
            lVar14 = (longlong)((iVar3 + iVar7) * 2);
            iVar13 = iVar9;
            do {
              lVar1 = (&DAT_180020200)[uVar12];
              uVar8 = (int)uVar12 + 1;
              uVar12 = (ulonglong)uVar8;
              lVar6 = (longlong)iVar13;
              iVar13 = iVar13 + 6;
              *(uint *)(lVar1 + lVar14 * 4) =
                   (uint)CONCAT21(CONCAT11(*(undefined1 *)(param_3 + lVar6),
                                           *(undefined1 *)(param_3 + 1 + lVar6)),
                                  *(undefined1 *)(param_3 + 2 + lVar6)) << 8;
              *(uint *)(lVar1 + 4 + lVar14 * 4) =
                   (uint)CONCAT21(CONCAT11(*(undefined1 *)(param_3 + 3 + lVar6),
                                           *(undefined1 *)(param_3 + 4 + lVar6)),
                                  *(undefined1 *)(param_3 + 5 + lVar6)) << 8;
              uVar2 = DAT_1800201ec;
            } while ((int)uVar8 < param_4);
          }
          pvVar4 = DAT_180020248;
          DAT_180020240 = iVar11 + 1;
          iVar11 = DAT_180020240;
          if (DAT_180020240 == DAT_180020244) {
            lVar14 = (longlong)DAT_1800201f8;
            DAT_1800201f8 = DAT_1800201f8 + 1;
            iVar11 = 0;
            DAT_180020240 = 0;
            *(uint *)((longlong)DAT_180020248 + lVar14 * 4) =
                 (uint)CONCAT11(*(undefined1 *)(param_3 + (iVar5 + iVar9)),
                                *(undefined1 *)(param_3 + 1 + (longlong)(iVar5 + iVar9))) << 0x10;
          }
          iVar3 = iVar3 + 1;
          iVar9 = iVar9 + iVar5 + 2;
        } while (iVar3 < (int)uVar2);
      }
      DAT_1800201f4 = DAT_1800201f4 + 1;
      if (DAT_1800201f4 < DAT_1800201fc) {
        return;
      }
    }
  }
  if (DAT_1800201f0 == 0) {
    DAT_180020280 = DAT_180020200;
    uRam0000000180020288 = DAT_180020208;
    DAT_1800202cc = DAT_1800201e8;
    _DAT_180020290 = _DAT_180020210;
    uRam0000000180020298 = uRam0000000180020218;
    _DAT_1800202a0 = _DAT_180020220;
    uRam00000001800202a8 = uRam0000000180020228;
    _DAT_1800202b0 = _DAT_180020230;
    uRam00000001800202b8 = uRam0000000180020238;
    DAT_1800202d0 = DAT_180020254;
    DAT_1800202c0 = pvVar4;
    DAT_1800202c8 = param_4;
    _beginthread(FUN_1800141a0,0,(void *)0x0);
    DAT_1800201f0 = 1;
  }
  return;
}



/* ========================================================================
   ENTRY: 180014770
   NAME : FUN_180014770
   SIG  : undefined __fastcall FUN_180014770(uint * param_1)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180014770(uint *param_1)

{
  uint uVar1;
  int iVar2;
  double dVar3;
  uint uVar4;
  longlong lVar5;
  int iVar6;
  bool bVar7;
  double dVar8;
  
  uVar4 = *param_1;
  do {
    LOCK();
    uVar1 = *param_1;
    bVar7 = uVar4 == uVar1;
    if (bVar7) {
      *param_1 = uVar4 & 1;
      uVar1 = uVar4;
    }
    uVar4 = uVar1;
    UNLOCK();
  } while (!bVar7);
  if (uVar4 == 0) {
    if (*(void **)(param_1 + 6) != *(void **)(param_1 + 4)) {
      memcpy(*(void **)(param_1 + 6),*(void **)(param_1 + 4),(longlong)(int)param_1[2] << 4);
    }
  }
  else {
    EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x10));
    iVar6 = 0;
    if (0 < (int)param_1[2]) {
      do {
        iVar2 = iVar6 * 2;
        iVar6 = iVar6 + 1;
        lVar5 = (longlong)iVar2;
        *(double *)(*(longlong *)(param_1 + 6) + lVar5 * 8) =
             *(double *)(*(longlong *)(param_1 + 4) + lVar5 * 8) * *(double *)(param_1 + 8);
        *(double *)(*(longlong *)(param_1 + 6) + 8 + lVar5 * 8) =
             *(double *)(*(longlong *)(param_1 + 4) + 8 + lVar5 * 8) * *(double *)(param_1 + 10);
      } while (iVar6 < (int)param_1[2]);
    }
    LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x10));
  }
  uVar4 = param_1[1];
  do {
    LOCK();
    uVar1 = param_1[1];
    bVar7 = uVar4 == uVar1;
    if (bVar7) {
      param_1[1] = uVar4 & 1;
      uVar1 = uVar4;
    }
    uVar4 = uVar1;
    UNLOCK();
  } while (!bVar7);
  if (uVar4 != 0) {
    EnterCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x1a));
    uVar4 = param_1[0xc];
    LeaveCriticalSection((LPCRITICAL_SECTION)(param_1 + 0x1a));
    dVar8 = 0.0;
    if (0.0 < (double)(int)uVar4) {
      LOCK();
      param_1[0xe] = param_1[0xe] | 1;
      UNLOCK();
      dVar3 = _DAT_180018e30;
      if ((param_1[0xd] == 0x21) || (dVar3 = _DAT_180018e20, param_1[0xd] == 0x32)) {
        dVar8 = pow(DAT_180018df8,(double)(int)uVar4 / dVar3);
        dVar8 = DAT_180018dc8 / dVar8;
      }
      iVar6 = 0;
      if (0 < (int)(param_1[2] * 2)) {
        do {
          lVar5 = (longlong)iVar6;
          iVar6 = iVar6 + 1;
          *(double *)(*(longlong *)(param_1 + 6) + lVar5 * 8) =
               dVar8 * *(double *)(*(longlong *)(param_1 + 6) + lVar5 * 8);
        } while (iVar6 < (int)(param_1[2] * 2));
      }
    }
  }
  return;
}



/* ========================================================================
   ENTRY: 1800148e0
   NAME : SetTXFixedGainRun
   SIG  : undefined __fastcall SetTXFixedGainRun(int param_1, int param_2)
   ======================================================================== */

void SetTXFixedGainRun(int param_1,int param_2)

{
  uint *puVar1;
  
                    /* 0x148e0  224  SetTXFixedGainRun */
  puVar1 = *(uint **)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120);
  if (param_2 != 0) {
    LOCK();
    *puVar1 = *puVar1 | 1;
    UNLOCK();
    return;
  }
  LOCK();
  *puVar1 = *puVar1 & 0xfffffffe;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180014910
   NAME : SetTXFixedGain
   SIG  : undefined __fastcall SetTXFixedGain(int param_1, undefined8 param_2, undefined8 param_3)
   ======================================================================== */

void SetTXFixedGain(int param_1,undefined8 param_2,undefined8 param_3)

{
  longlong lVar1;
  
                    /* 0x14910  223  SetTXFixedGain */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x40));
  *(undefined8 *)(lVar1 + 0x20) = param_2;
  *(undefined8 *)(lVar1 + 0x28) = param_3;
                    /* WARNING: Could not recover jumptable at 0x00018001496c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x40));
  return;
}



/* ========================================================================
   ENTRY: 180014980
   NAME : GetAndResetAmpProtect
   SIG  : bool __fastcall GetAndResetAmpProtect(int param_1)
   ======================================================================== */

bool GetAndResetAmpProtect(int param_1)

{
  uint *puVar1;
  uint uVar2;
  
                    /* 0x14980  26  GetAndResetAmpProtect */
  LOCK();
  puVar1 = (uint *)(*(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120) + 0x38);
  uVar2 = *puVar1;
  *puVar1 = *puVar1 & 0xfffffffe;
  UNLOCK();
  return (uVar2 & 1) != 0;
}



/* ========================================================================
   ENTRY: 1800149b0
   NAME : SetAmpProtectRun
   SIG  : undefined __fastcall SetAmpProtectRun(int param_1, int param_2)
   ======================================================================== */

void SetAmpProtectRun(int param_1,int param_2)

{
  uint *puVar1;
  
                    /* 0x149b0  85  SetAmpProtectRun */
  if (param_2 != 0) {
    LOCK();
    puVar1 = (uint *)(*(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120) + 4);
    *puVar1 = *puVar1 | 1;
    UNLOCK();
    return;
  }
  LOCK();
  puVar1 = (uint *)(*(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120) + 4);
  *puVar1 = *puVar1 & 0xfffffffe;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 1800149e0
   NAME : SetADCSupply
   SIG  : undefined __fastcall SetADCSupply(int param_1, undefined4 param_2)
   ======================================================================== */

void SetADCSupply(int param_1,undefined4 param_2)

{
                    /* 0x149e0  72  SetADCSupply */
  *(undefined4 *)(*(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1120) + 0x34) =
       param_2;
  return;
}



/* ========================================================================
   ENTRY: 180014a00
   NAME : GetCMVersion
   SIG  : undefined8 __fastcall GetCMVersion(void)
   ======================================================================== */

undefined8 GetCMVersion(void)

{
                    /* 0x14a00  28  GetCMVersion */
  return 0x410;
}



/* ========================================================================
   ENTRY: 180014a10
   NAME : SendCBPushVox
   SIG  : undefined __fastcall SendCBPushVox(int param_1, undefined8 param_2)
   ======================================================================== */

void SendCBPushVox(int param_1,undefined8 param_2)

{
                    /* 0x14a10  50  SendCBPushVox */
  *(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1118) = param_2;
  return;
}



/* ========================================================================
   ENTRY: 180014a30
   NAME : SetTXAVoxRun
   SIG  : undefined __fastcall SetTXAVoxRun(int param_1, undefined4 param_2)
   ======================================================================== */

void SetTXAVoxRun(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x14a30  220  SetTXAVoxRun */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1110);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  *(undefined4 *)(lVar1 + 4) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180014a77. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  return;
}



/* ========================================================================
   ENTRY: 180014a80
   NAME : SetTXAVoxSize
   SIG  : undefined __fastcall SetTXAVoxSize(int param_1, undefined4 param_2)
   ======================================================================== */

void SetTXAVoxSize(int param_1,undefined4 param_2)

{
  longlong lVar1;
  
                    /* 0x14a80  221  SetTXAVoxSize */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1110);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  *(undefined4 *)(lVar1 + 8) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180014ac7. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  return;
}



/* ========================================================================
   ENTRY: 180014ad0
   NAME : SetTXAVoxThresh
   SIG  : undefined __fastcall SetTXAVoxThresh(int param_1, undefined8 param_2)
   ======================================================================== */

void SetTXAVoxThresh(int param_1,undefined8 param_2)

{
  longlong lVar1;
  
                    /* 0x14ad0  222  SetTXAVoxThresh */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1110);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  *(undefined8 *)(lVar1 + 0x20) = param_2;
                    /* WARNING: Could not recover jumptable at 0x000180014b1a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  return;
}



/* ========================================================================
   ENTRY: 180014b30
   NAME : GetTXAVoxPeak
   SIG  : undefined __fastcall GetTXAVoxPeak(int param_1, undefined8 * param_2)
   ======================================================================== */

void GetTXAVoxPeak(int param_1,undefined8 *param_2)

{
  longlong lVar1;
  
                    /* 0x14b30  36  GetTXAVoxPeak */
  lVar1 = *(longlong *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1110);
  EnterCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  *param_2 = *(undefined8 *)(lVar1 + 0x30);
                    /* WARNING: Could not recover jumptable at 0x000180014b7c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  LeaveCriticalSection((LPCRITICAL_SECTION)(lVar1 + 0x38));
  return;
}



/* ========================================================================
   ENTRY: 180014b90
   NAME : SetEERRun
   SIG  : undefined __fastcall SetEERRun(int param_1, int param_2)
   ======================================================================== */

void SetEERRun(int param_1,int param_2)

{
  uint *puVar1;
  
                    /* 0x14b90  123  SetEERRun */
  puVar1 = *(uint **)(PTR_DAT_18001e080 + ((longlong)param_1 + 0x32) * 0x58);
  pSetEERRun(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  if (param_2 != 0) {
    LOCK();
    *puVar1 = *puVar1 | 1;
    UNLOCK();
    return;
  }
  LOCK();
  *puVar1 = *puVar1 & 0xfffffffe;
  UNLOCK();
  return;
}



/* ========================================================================
   ENTRY: 180014bf0
   NAME : SetEERAMIQ
   SIG  : undefined __fastcall SetEERAMIQ(int param_1)
   ======================================================================== */

void SetEERAMIQ(int param_1)

{
                    /* 0x14bf0  116  SetEERAMIQ */
                    /* WARNING: Could not recover jumptable at 0x000180014c06. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERAMIQ(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014c10
   NAME : SetEERMgain
   SIG  : undefined __fastcall SetEERMgain(int param_1)
   ======================================================================== */

void SetEERMgain(int param_1)

{
                    /* 0x14c10  118  SetEERMgain */
                    /* WARNING: Could not recover jumptable at 0x000180014c26. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERMgain(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014c30
   NAME : SetEERPgain
   SIG  : undefined __fastcall SetEERPgain(int param_1)
   ======================================================================== */

void SetEERPgain(int param_1)

{
                    /* 0x14c30  122  SetEERPgain */
                    /* WARNING: Could not recover jumptable at 0x000180014c46. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERPgain(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014c50
   NAME : SetEERRunDelays
   SIG  : undefined __fastcall SetEERRunDelays(int param_1)
   ======================================================================== */

void SetEERRunDelays(int param_1)

{
                    /* 0x14c50  124  SetEERRunDelays */
                    /* WARNING: Could not recover jumptable at 0x000180014c66. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERRunDelays(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014c70
   NAME : SetEERMdelay
   SIG  : undefined __fastcall SetEERMdelay(int param_1)
   ======================================================================== */

void SetEERMdelay(int param_1)

{
                    /* 0x14c70  117  SetEERMdelay */
                    /* WARNING: Could not recover jumptable at 0x000180014c86. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERMdelay(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014c90
   NAME : SetEERPdelay
   SIG  : undefined __fastcall SetEERPdelay(int param_1)
   ======================================================================== */

void SetEERPdelay(int param_1)

{
                    /* 0x14c90  121  SetEERPdelay */
                    /* WARNING: Could not recover jumptable at 0x000180014ca6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERPdelay(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014cb0
   NAME : SetEERSize
   SIG  : undefined __fastcall SetEERSize(int param_1)
   ======================================================================== */

void SetEERSize(int param_1)

{
                    /* 0x14cb0  126  SetEERSize */
                    /* WARNING: Could not recover jumptable at 0x000180014cc6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERSize(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014cd0
   NAME : SetEERSamplerate
   SIG  : undefined __fastcall SetEERSamplerate(int param_1)
   ======================================================================== */

void SetEERSamplerate(int param_1)

{
                    /* 0x14cd0  125  SetEERSamplerate */
                    /* WARNING: Could not recover jumptable at 0x000180014ce6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetEERSamplerate(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_1 * 0x58 + 0x1128));
  return;
}



/* ========================================================================
   ENTRY: 180014cf0
   NAME : SetRCVRANBRun
   SIG  : undefined __fastcall SetRCVRANBRun(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBRun(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14cf0  188  SetRCVRANBRun */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014d40. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBRun(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014d11. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBRun(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014d20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBRun(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014d50
   NAME : SetRCVRANBBuffsize
   SIG  : undefined __fastcall SetRCVRANBBuffsize(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBBuffsize(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14d50  186  SetRCVRANBBuffsize */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014da0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBBuffsize(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),
                        param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014d71. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBBuffsize(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),
                        param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014d80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBBuffsize(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014db0
   NAME : SetRCVRANBSamplerate
   SIG  : undefined __fastcall SetRCVRANBSamplerate(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBSamplerate(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14db0  189  SetRCVRANBSamplerate */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014e00. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBSamplerate
              (*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014dd1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBSamplerate
              (*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014de0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBSamplerate(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014e10
   NAME : SetRCVRANBTau
   SIG  : undefined __fastcall SetRCVRANBTau(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBTau(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14e10  190  SetRCVRANBTau */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014e60. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBTau(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014e31. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBTau(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014e40. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBTau(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014e70
   NAME : SetRCVRANBHangtime
   SIG  : undefined __fastcall SetRCVRANBHangtime(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBHangtime(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14e70  187  SetRCVRANBHangtime */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014ec0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBHangtime(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),
                        param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014e91. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBHangtime(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),
                        param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014ea0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBHangtime(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014ed0
   NAME : SetRCVRANBAdvtime
   SIG  : undefined __fastcall SetRCVRANBAdvtime(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBAdvtime(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14ed0  184  SetRCVRANBAdvtime */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014f20. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBAdvtime(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3
                      );
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014ef1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBAdvtime(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),
                       param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014f00. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBAdvtime(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014f30
   NAME : SetRCVRANBBacktau
   SIG  : undefined __fastcall SetRCVRANBBacktau(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBBacktau(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14f30  185  SetRCVRANBBacktau */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014f80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBBacktau(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3
                      );
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014f51. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBBacktau(*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),
                       param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014f60. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBBacktau(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014f90
   NAME : SetRCVRANBThreshold
   SIG  : undefined __fastcall SetRCVRANBThreshold(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRANBThreshold(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14f90  191  SetRCVRANBThreshold */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180014fe0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBThreshold
              (*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd20),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180014fb1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRANBThreshold
              (*(undefined8 *)(PTR_DAT_18001e088 + ((longlong)param_2 + 0x33) * 0x10),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180014fc0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRANBThreshold(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180014ff0
   NAME : SetRCVRNOBRun
   SIG  : undefined __fastcall SetRCVRNOBRun(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBRun(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x14ff0  197  SetRCVRNOBRun */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015040. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBRun(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015011. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBRun(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015020. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBRun(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015050
   NAME : SetRCVRNOBMode
   SIG  : undefined __fastcall SetRCVRNOBMode(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBMode(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x15050  196  SetRCVRNOBMode */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800150a0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBMode(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015071. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBMode(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015080. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBMode(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 1800150b0
   NAME : SetRCVRNOBBuffsize
   SIG  : undefined __fastcall SetRCVRNOBBuffsize(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBBuffsize(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x150b0  194  SetRCVRNOBBuffsize */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015100. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBBuffsize(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),
                        param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x0001800150d1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBBuffsize(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),
                        param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800150e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBBuffsize(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015110
   NAME : SetRCVRNOBSamplerate
   SIG  : undefined __fastcall SetRCVRNOBSamplerate(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBSamplerate(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x15110  198  SetRCVRNOBSamplerate */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015160. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBSamplerate
              (*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015131. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBSamplerate
              (*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015140. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBSamplerate(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015170
   NAME : SetRCVRNOBTau
   SIG  : undefined __fastcall SetRCVRNOBTau(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBTau(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x15170  199  SetRCVRNOBTau */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800151c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBTau(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015191. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBTau(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800151a0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBTau(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 1800151d0
   NAME : SetRCVRNOBHangtime
   SIG  : undefined __fastcall SetRCVRNOBHangtime(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBHangtime(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x151d0  195  SetRCVRNOBHangtime */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015220. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBHangtime(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),
                        param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x0001800151f1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBHangtime(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),
                        param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015200. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBHangtime(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015230
   NAME : SetRCVRNOBAdvtime
   SIG  : undefined __fastcall SetRCVRNOBAdvtime(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBAdvtime(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x15230  192  SetRCVRNOBAdvtime */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015280. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBAdvtime(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3
                      );
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015251. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBAdvtime(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3
                      );
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015260. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBAdvtime(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015290
   NAME : SetRCVRNOBBacktau
   SIG  : undefined __fastcall SetRCVRNOBBacktau(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBBacktau(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x15290  193  SetRCVRNOBBacktau */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x0001800152e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBBacktau(*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3
                      );
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x0001800152b1. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBBacktau(*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3
                      );
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x0001800152c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBBacktau(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 1800152f0
   NAME : SetRCVRNOBThreshold
   SIG  : undefined __fastcall SetRCVRNOBThreshold(int param_1, int param_2, undefined4 param_3)
   ======================================================================== */

void SetRCVRNOBThreshold(int param_1,int param_2,undefined4 param_3)

{
  undefined8 local_res20;
  
                    /* 0x152f0  200  SetRCVRNOBThreshold */
  if (param_1 == 0) {
                    /* WARNING: Could not recover jumptable at 0x000180015340. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBThreshold
              (*(undefined8 *)(PTR_DAT_18001e080 + (longlong)param_2 * 0x40 + 0xd28),param_3);
    return;
  }
  if (param_1 == 2) {
                    /* WARNING: Could not recover jumptable at 0x000180015311. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    pSetRCVRNOBThreshold
              (*(undefined8 *)(PTR_DAT_18001e088 + (longlong)param_2 * 0x10 + 0x338),param_3);
    return;
  }
                    /* WARNING: Could not recover jumptable at 0x000180015320. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pSetRCVRNOBThreshold(local_res20,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015360
   NAME : __security_check_cookie
   SIG  : void __cdecl __security_check_cookie(uintptr_t _StackCookie)
   ======================================================================== */

/* WARNING: This is an inlined function */

void __cdecl __security_check_cookie(uintptr_t _StackCookie)

{
  if ((_StackCookie == DAT_18001e000) && ((short)(_StackCookie >> 0x30) == 0)) {
    return;
  }
  FUN_180015380();
  return;
}



/* ========================================================================
   ENTRY: 180015380
   NAME : FUN_180015380
   SIG  : undefined __fastcall FUN_180015380(void)
   ======================================================================== */

void FUN_180015380(void)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(2);
  return;
}



/* ========================================================================
   ENTRY: 180015390
   NAME : FUN_180015390
   SIG  : ulonglong __fastcall FUN_180015390(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_180015390(undefined8 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  ulonglong uVar2;
  
  if (param_2 == 0) {
    uVar2 = FUN_180015500(param_3 != 0);
    return uVar2;
  }
  if (param_2 == 1) {
    uVar2 = FUN_1800153f0(param_1,param_3);
    return uVar2;
  }
  if (param_2 == 2) {
    bVar1 = FUN_180015970();
    return (ulonglong)bVar1;
  }
  if (param_2 != 3) {
    return 1;
  }
  bVar1 = FUN_1800159a0();
  return (ulonglong)bVar1;
}



/* ========================================================================
   ENTRY: 1800153f0
   NAME : FUN_1800153f0
   SIG  : undefined8 __fastcall FUN_1800153f0(undefined8 param_1, undefined8 param_2)
   ======================================================================== */

undefined8 FUN_1800153f0(undefined8 param_1,undefined8 param_2)

{
  code *pcVar1;
  bool bVar2;
  undefined4 uVar3;
  int iVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  longlong *plVar7;
  ulonglong uVar8;
  
  uVar5 = FUN_180015a70(0);
  if ((char)uVar5 != '\0') {
    uVar5 = FUN_180015860();
    bVar2 = true;
    if (DAT_18001e0e8 != 0) {
      FUN_180015bc0(7);
      pcVar1 = (code *)swi(3);
      uVar5 = (*pcVar1)();
      return uVar5;
    }
    DAT_18001e0e8 = 1;
    uVar3 = FUN_1800158f0();
    if ((char)uVar3 != '\0') {
      FUN_180015bd0();
      FUN_180015810();
      FUN_180015840();
      iVar4 = _initterm_e(&DAT_180017658,&DAT_180017660);
      if (iVar4 == 0) {
        uVar6 = FUN_1800158b0();
        if ((char)uVar6 != '\0') {
          _initterm(&DAT_180017648,&DAT_180017650);
          DAT_18001e0e8 = 2;
          bVar2 = false;
        }
      }
    }
    FUN_180015b50((char)uVar5);
    if (!bVar2) {
      plVar7 = (longlong *)FUN_180015bb0();
      if (*plVar7 != 0) {
        uVar8 = FUN_180015ab0((longlong)plVar7);
        if ((char)uVar8 != '\0') {
          (*(code *)PTR__guard_dispatch_icall_180017620)(param_1,2,param_2);
        }
      }
      DAT_18001e0c0 = DAT_18001e0c0 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015500
   NAME : FUN_180015500
   SIG  : ulonglong __fastcall FUN_180015500(byte param_1)
   ======================================================================== */

ulonglong FUN_180015500(byte param_1)

{
  code *pcVar1;
  byte bVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  
  if (DAT_18001e0c0 < 1) {
    return 0;
  }
  DAT_18001e0c0 = DAT_18001e0c0 + -1;
  uVar3 = FUN_180015860();
  if (DAT_18001e0e8 == 2) {
    FUN_180015a10();
    FUN_180015820();
    FUN_180015c20();
    DAT_18001e0e8 = 0;
    FUN_180015b50((char)uVar3);
    bVar2 = FUN_180015b80((ulonglong)param_1,'\0');
    FUN_180015a50();
    return (ulonglong)bVar2;
  }
  FUN_180015bc0(7);
  pcVar1 = (code *)swi(3);
  uVar4 = (*pcVar1)();
  return uVar4;
}



/* ========================================================================
   ENTRY: 1800155a0
   NAME : FUN_1800155a0
   SIG  : ulonglong __fastcall FUN_1800155a0(HMODULE param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_1800155a0(HMODULE param_1,int param_2,longlong param_3)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  if ((param_2 == 0) && (DAT_18001e0c0 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (DAT_180017690 == 0) {
      uVar2 = 1;
    }
    else {
      uVar1 = (*(code *)PTR__guard_dispatch_icall_180017620)();
      uVar2 = (ulonglong)uVar1;
    }
    if ((int)uVar2 == 0) {
      return uVar2;
    }
    uVar2 = FUN_180015390(param_1,param_2,param_3);
    if ((int)uVar2 == 0) {
      return uVar2 & 0xffffffff;
    }
  }
  uVar2 = FUN_1800157e0(param_1,param_2);
  uVar3 = uVar2 & 0xffffffff;
  if ((param_2 == 1) && ((int)uVar2 == 0)) {
    FUN_1800157e0(param_1,0);
    FUN_180015500(param_3 != 0);
    if (DAT_180017690 != 0) {
      (*(code *)PTR__guard_dispatch_icall_180017620)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    uVar2 = FUN_180015390(param_1,param_2,param_3);
    uVar3 = uVar2 & 0xffffffff;
    if ((int)uVar2 != 0) {
      if (DAT_180017690 == 0) {
        uVar3 = 1;
      }
      else {
        uVar1 = (*(code *)PTR__guard_dispatch_icall_180017620)(param_1,param_2,param_3);
        uVar3 = (ulonglong)uVar1;
      }
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 1800156e0
   NAME : entry
   SIG  : undefined __fastcall entry(HMODULE param_1, int param_2, longlong param_3)
   ======================================================================== */

void entry(HMODULE param_1,int param_2,longlong param_3)

{
  if (param_2 == 1) {
    FUN_180015720();
  }
  FUN_1800155a0(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180015720
   NAME : FUN_180015720
   SIG  : undefined __fastcall FUN_180015720(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_180015720(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10 [3];
  _FILETIME local_18 [2];
  
  if (DAT_18001e000 != 0x2b992ddfa232) {
    _DAT_18001e040 = ~DAT_18001e000;
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
  DAT_18001e000 =
       (local_res10[0].QuadPart << 0x20 ^ local_res10[0].QuadPart ^ (ulonglong)local_18[0] ^
       (ulonglong)local_18) & 0xffffffffffff;
  if (DAT_18001e000 == 0x2b992ddfa232) {
    DAT_18001e000 = 0x2b992ddfa233;
  }
  _DAT_18001e040 = ~DAT_18001e000;
  return;
}



/* ========================================================================
   ENTRY: 1800157e0
   NAME : FUN_1800157e0
   SIG  : undefined8 __fastcall FUN_1800157e0(HMODULE param_1, int param_2)
   ======================================================================== */

undefined8 FUN_1800157e0(HMODULE param_1,int param_2)

{
  if ((param_2 == 1) && (DAT_180017690 == 0)) {
    DisableThreadLibraryCalls(param_1);
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180015810
   NAME : FUN_180015810
   SIG  : undefined __fastcall FUN_180015810(void)
   ======================================================================== */

void FUN_180015810(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015817. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_18001e0d0);
  return;
}



/* ========================================================================
   ENTRY: 180015820
   NAME : FUN_180015820
   SIG  : undefined __fastcall FUN_180015820(void)
   ======================================================================== */

void FUN_180015820(void)

{
  __std_type_info_destroy_list(&DAT_18001e0d0);
  return;
}



/* ========================================================================
   ENTRY: 180015830
   NAME : FUN_180015830
   SIG  : undefined * __fastcall FUN_180015830(void)
   ======================================================================== */

undefined * FUN_180015830(void)

{
  return &DAT_18001e0e0;
}



/* ========================================================================
   ENTRY: 180015840
   NAME : FUN_180015840
   SIG  : undefined __fastcall FUN_180015840(void)
   ======================================================================== */

void FUN_180015840(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_1800032a0();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_180015830();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 180015860
   NAME : FUN_180015860
   SIG  : undefined8 __fastcall FUN_180015860(void)
   ======================================================================== */

ulonglong FUN_180015860(void)

{
  ulonglong uVar1;
  ulonglong uVar2;
  bool bVar3;
  undefined7 extraout_var;
  ulonglong uVar4;
  
  bVar3 = FUN_180015f30();
  uVar4 = CONCAT71(extraout_var,bVar3);
  if ((int)uVar4 != 0) {
    uVar1 = *(ulonglong *)((longlong)Self + 8);
    uVar4 = 0;
    LOCK();
    bVar3 = DAT_18001e0f0 == 0;
    uVar2 = uVar1;
    if (!bVar3) {
      uVar4 = DAT_18001e0f0;
      uVar2 = DAT_18001e0f0;
    }
    DAT_18001e0f0 = uVar2;
    UNLOCK();
    uVar2 = DAT_18001e0f0;
    while (DAT_18001e0f0 = uVar2, !bVar3) {
      if (uVar1 == uVar4) {
        return CONCAT71((int7)(uVar4 >> 8),1);
      }
      uVar4 = 0;
      LOCK();
      bVar3 = uVar2 == 0;
      DAT_18001e0f0 = uVar1;
      if (!bVar3) {
        uVar4 = uVar2;
        DAT_18001e0f0 = uVar2;
      }
      UNLOCK();
      uVar2 = DAT_18001e0f0;
    }
  }
  return uVar4 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 1800158b0
   NAME : FUN_1800158b0
   SIG  : undefined8 __fastcall FUN_1800158b0(void)
   ======================================================================== */

undefined8 FUN_1800158b0(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  ulonglong uVar3;
  
  bVar1 = FUN_180015f30();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    uVar2 = FUN_180015c70();
    return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
  }
  uVar3 = FUN_180015f20();
  uVar3 = _configure_narrow_argv(uVar3 & 0xffffffff);
  if ((int)uVar3 != 0) {
    return uVar3 & 0xffffffffffffff00;
  }
  uVar2 = _initialize_narrow_environment();
  return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
}



/* ========================================================================
   ENTRY: 1800158f0
   NAME : FUN_1800158f0
   SIG  : undefined4 __fastcall FUN_1800158f0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

uint FUN_1800158f0(void)

{
  bool bVar1;
  undefined4 in_EAX;
  undefined3 extraout_var;
  uint uVar2;
  
  if (DAT_18001e0f9 == '\0') {
    bVar1 = FUN_180015f30();
    if (CONCAT31(extraout_var,bVar1) == 0) {
      _DAT_18001e100 = _DAT_1800176a0;
      uRam000000018001e108 = _UNK_1800176a8;
      _DAT_18001e110 = 0xffffffffffffffff;
      _DAT_18001e118 = _DAT_1800176a0;
      uRam000000018001e120 = _UNK_1800176a8;
      _DAT_18001e128 = 0xffffffffffffffff;
    }
    else {
      uVar2 = _initialize_onexit_table(&DAT_18001e100);
      if (uVar2 != 0) {
LAB_180015926:
        return uVar2 & 0xffffff00;
      }
      uVar2 = _initialize_onexit_table(&DAT_18001e118);
      if (uVar2 != 0) goto LAB_180015926;
    }
    in_EAX = 0;
    DAT_18001e0f9 = '\x01';
  }
  return CONCAT31((int3)((uint)in_EAX >> 8),1);
}



/* ========================================================================
   ENTRY: 180015970
   NAME : FUN_180015970
   SIG  : undefined1 __fastcall FUN_180015970(void)
   ======================================================================== */

undefined1 FUN_180015970(void)

{
  char cVar1;
  
  cVar1 = FUN_180015f80();
  if (cVar1 != '\0') {
    cVar1 = FUN_180015f80();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_180015f80();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800159a0
   NAME : FUN_1800159a0
   SIG  : undefined1 __fastcall FUN_1800159a0(void)
   ======================================================================== */

undefined1 FUN_1800159a0(void)

{
  FUN_180015f80();
  FUN_180015f80();
  return 1;
}



/* ========================================================================
   ENTRY: 1800159c0
   NAME : FUN_1800159c0
   SIG  : undefined __fastcall FUN_1800159c0(undefined8 param_1, int param_2, undefined8 param_3, undefined * param_4, undefined4 param_5, undefined8 param_6)
   ======================================================================== */

void FUN_1800159c0(undefined8 param_1,int param_2,undefined8 param_3,undefined *param_4,
                  undefined4 param_5,undefined8 param_6)

{
  bool bVar1;
  undefined7 extraout_var;
  
  bVar1 = FUN_180015f30();
  if (((int)CONCAT71(extraout_var,bVar1) == 0) && (param_2 == 1)) {
    (*(code *)PTR__guard_dispatch_icall_180017620)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 180015a10
   NAME : FUN_180015a10
   SIG  : undefined __fastcall FUN_180015a10(void)
   ======================================================================== */

void FUN_180015a10(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  
  bVar1 = FUN_180015f30();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    _execute_onexit_table(&DAT_18001e100);
    return;
  }
  uVar2 = FUN_180015f90();
  if ((int)uVar2 == 0) {
    _cexit();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 180015a50
   NAME : FUN_180015a50
   SIG  : undefined __fastcall FUN_180015a50(void)
   ======================================================================== */

void FUN_180015a50(void)

{
  FUN_180015f80();
  FUN_180015f80();
  return;
}



/* ========================================================================
   ENTRY: 180015a70
   NAME : FUN_180015a70
   SIG  : undefined8 __fastcall FUN_180015a70(int param_1)
   ======================================================================== */

longlong FUN_180015a70(int param_1)

{
  char cVar1;
  uint7 extraout_var;
  undefined7 extraout_var_00;
  uint7 extraout_var_01;
  uint7 uVar2;
  
  if (param_1 == 0) {
    DAT_18001e0f8 = 1;
  }
  FUN_180015c70();
  cVar1 = FUN_180015f80();
  uVar2 = extraout_var;
  if (cVar1 != '\0') {
    cVar1 = FUN_180015f80();
    if (cVar1 != '\0') {
      return CONCAT71(extraout_var_00,1);
    }
    FUN_180015f80();
    uVar2 = extraout_var_01;
  }
  return (ulonglong)uVar2 << 8;
}



/* ========================================================================
   ENTRY: 180015ab0
   NAME : FUN_180015ab0
   SIG  : ulonglong __fastcall FUN_180015ab0(longlong param_1)
   ======================================================================== */

ulonglong FUN_180015ab0(longlong param_1)

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
          return (ulonglong)uVar3 << 8;
        }
        if (((ulonglong)*(uint *)(uVar2 + 0xc) <= param_1 - 0x180000000U) &&
           (param_1 - 0x180000000U < (ulonglong)(*(int *)(uVar2 + 8) + *(uint *)(uVar2 + 0xc))))
        break;
        uVar2 = uVar2 + 0x28;
      }
      if (*(int *)(uVar2 + 0x24) < 0) {
        return uVar2 & 0xffffffffffffff00;
      }
      return CONCAT71(uVar3,1);
    }
  }
  return uVar1 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 180015b50
   NAME : FUN_180015b50
   SIG  : undefined8 __fastcall FUN_180015b50(char param_1)
   ======================================================================== */

undefined8 FUN_180015b50(char param_1)

{
  undefined8 uVar1;
  bool bVar2;
  undefined7 extraout_var;
  undefined8 uVar3;
  
  bVar2 = FUN_180015f30();
  uVar1 = DAT_18001e0f0;
  uVar3 = CONCAT71(extraout_var,bVar2);
  if (((int)uVar3 != 0) && (param_1 == '\0')) {
    LOCK();
    DAT_18001e0f0 = 0;
    UNLOCK();
    uVar3 = uVar1;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180015b80
   NAME : FUN_180015b80
   SIG  : undefined1 __fastcall FUN_180015b80(undefined8 param_1, char param_2)
   ======================================================================== */

undefined1 FUN_180015b80(undefined8 param_1,char param_2)

{
  if ((DAT_18001e0f8 == '\0') || (param_2 == '\0')) {
    FUN_180015f80();
    FUN_180015f80();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180015bb0
   NAME : FUN_180015bb0
   SIG  : undefined * __fastcall FUN_180015bb0(void)
   ======================================================================== */

undefined * FUN_180015bb0(void)

{
  return &DAT_180020258;
}



/* ========================================================================
   ENTRY: 180015bc0
   NAME : FUN_180015bc0
   SIG  : undefined __fastcall FUN_180015bc0(undefined4 param_1)
   ======================================================================== */

void FUN_180015bc0(undefined4 param_1)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180015bd0
   NAME : FUN_180015bd0
   SIG  : undefined __fastcall FUN_180015bd0(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180015bf0) */
/* WARNING: Removing unreachable block (ram,0x000180015bf8) */
/* WARNING: Removing unreachable block (ram,0x000180015bfe) */

void FUN_180015bd0(void)

{
  return;
}



/* ========================================================================
   ENTRY: 180015c20
   NAME : FUN_180015c20
   SIG  : undefined __fastcall FUN_180015c20(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180015c40) */
/* WARNING: Removing unreachable block (ram,0x000180015c48) */
/* WARNING: Removing unreachable block (ram,0x000180015c4e) */

void FUN_180015c20(void)

{
  return;
}



/* ========================================================================
   ENTRY: 180015c70
   NAME : FUN_180015c70
   SIG  : undefined8 __fastcall FUN_180015c70(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180015d77) */
/* WARNING: Removing unreachable block (ram,0x000180015d65) */
/* WARNING: Removing unreachable block (ram,0x000180015d53) */
/* WARNING: Removing unreachable block (ram,0x000180015d2c) */
/* WARNING: Removing unreachable block (ram,0x000180015ca7) */
/* WARNING: Removing unreachable block (ram,0x000180015c82) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180015c70(void)

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
    _DAT_18001e058 = 0x8000;
    _DAT_18001e060 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_18001e134 = DAT_18001e134 | 1;
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
      DAT_18001e134 = DAT_18001e134 | 2;
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
  _DAT_18001e050 = 1;
  DAT_18001e054 = 2;
  uVar6 = DAT_18001e068 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    _DAT_18001e050 = 2;
    DAT_18001e054 = 6;
    uVar6 = DAT_18001e068 & 0xffffffffffffffee;
  }
  DAT_18001e068 = uVar6;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar6 = xinuse(0);
    uVar9 = in_XCR0 & uVar6 & 0xffffffff;
    uVar6 = DAT_18001e068;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar7 = (byte)uVar9, (bVar7 & 6) == 6)) {
      _DAT_18001e050 = 3;
      uVar5 = DAT_18001e054 | 8;
      if ((uVar12 & 0x20) != 0) {
        _DAT_18001e050 = 5;
        uVar5 = DAT_18001e054 | 0x28;
        uVar6 = DAT_18001e068 & 0xfffffffffffffffd;
        if (((uVar12 & 0xd0030000) == 0xd0030000) && ((bVar7 & 0xe0) == 0xe0)) {
          DAT_18001e054 = DAT_18001e054 | 0x68;
          _DAT_18001e050 = 6;
          uVar5 = DAT_18001e054;
          uVar6 = DAT_18001e068 & 0xffffffffffffffd9;
        }
      }
      DAT_18001e068 = uVar6;
      DAT_18001e054 = uVar5;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_18001e068 = DAT_18001e068 & 0xfffffffffeffffff;
      }
      uVar6 = DAT_18001e068;
      if (((uVar13 >> 0x13 & 1) != 0) && ((bVar7 & 0xe0) == 0xe0)) {
        DAT_18001e138._0_4_ = uVar8 & 0xff;
        uVar6 = DAT_18001e068 & 0xfffffffffeffffd0;
        if (1 < (uint)DAT_18001e138) {
          uVar6 = DAT_18001e068 & 0xfffffffffeffff90;
        }
      }
    }
    DAT_18001e068 = uVar6;
    if ((((uVar13 >> 0x15 & 1) != 0) && ((uVar11 & 1) != 0)) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_18001e068 = DAT_18001e068 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180015f20
   NAME : FUN_180015f20
   SIG  : undefined8 __fastcall FUN_180015f20(void)
   ======================================================================== */

undefined8 FUN_180015f20(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180015f30
   NAME : FUN_180015f30
   SIG  : bool __fastcall FUN_180015f30(void)
   ======================================================================== */

bool FUN_180015f30(void)

{
  return DAT_18001e070 != 0;
}



/* ========================================================================
   ENTRY: 180015f46
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f46. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 180015f4c
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f4c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 180015f52
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f52. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 180015f58
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f58. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 180015f5e
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f5e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 180015f64
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f64. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 180015f6a
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f6a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180015f70
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f70. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180015f76
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180015f76. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 180015f80
   NAME : FUN_180015f80
   SIG  : undefined1 __fastcall FUN_180015f80(void)
   ======================================================================== */

undefined1 FUN_180015f80(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180015f90
   NAME : FUN_180015f90
   SIG  : undefined8 __fastcall FUN_180015f90(void)
   ======================================================================== */

undefined8 FUN_180015f90(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 180015fa0
   NAME : FUN_180015fa0
   SIG  : undefined8 __fastcall FUN_180015fa0(undefined8 param_1, undefined8 param_2, undefined8 param_3, longlong param_4)
   ======================================================================== */

undefined8 FUN_180015fa0(undefined8 param_1,undefined8 param_2,undefined8 param_3,longlong param_4)

{
  FUN_180015fc0(param_2,param_4);
  return 1;
}



/* ========================================================================
   ENTRY: 180015fc0
   NAME : FUN_180015fc0
   SIG  : undefined __fastcall FUN_180015fc0(undefined8 param_1, longlong param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180015fc0(undefined8 param_1,longlong param_2)

{
  code *pcVar1;
  byte bVar2;
  uint uVar3;
  byte bVar4;
  byte *pbVar5;
  byte *pbVar6;
  
  pbVar5 = (byte *)((ulonglong)*(uint *)(*(longlong *)(param_2 + 0x10) + 8) +
                   *(longlong *)(param_2 + 8));
  bVar2 = pbVar5[3];
  if ((*pbVar5 & 7) < 3) {
    return;
  }
  if ((bVar2 & 0x1f) == 0) {
    return;
  }
  uVar3 = 0;
  pbVar5 = pbVar5 + (ulonglong)(bVar2 & 0x1f) + 4;
  pbVar6 = pbVar5;
  if (bVar2 >> 5 != 0) {
    do {
      pbVar5 = pbVar6 + 3;
      if (*pbVar6 >> 3 != 0) {
        pbVar5 = pbVar6 + (ulonglong)(uint)(*pbVar6 >> 3) + 6;
      }
      uVar3 = uVar3 + 1;
      pbVar6 = pbVar5;
    } while (uVar3 < bVar2 >> 5);
  }
  if ((bVar2 & 0x1f) == 0) {
    return;
  }
  bVar2 = *pbVar5;
  bVar4 = bVar2;
  if (((bVar2 < 4) || (bVar4 = bVar2 & 0xf, (byte)(bVar4 - 8) < 3)) ||
     (bVar4 = bVar2 & 7, (byte)(bVar4 - 4) < 4)) {
    if (bVar4 == 0) {
      return;
    }
    if (bVar2 < 4) goto LAB_1800160d7;
    bVar4 = bVar2 & 0xf;
    if (((byte)((bVar2 & 0xf) - 8) < 3) || (bVar4 = bVar2 & 7, (byte)((bVar2 & 7) - 4) < 4)) {
      bVar2 = bVar4;
      if (bVar2 < 0x21) goto LAB_1800160d7;
      goto LAB_180016149;
    }
  }
  if ((bVar2 & 0x3f) != 0x20) {
LAB_180016149:
    FUN_180015380();
    pcVar1 = (code *)swi(3);
    (*pcVar1)();
    return;
  }
  bVar2 = 0x20;
LAB_1800160d7:
                    /* WARNING: Could not emulate address calculation at 0x0001800160e2 */
                    /* WARNING: Treating indirect jump as call */
  (*(code *)((ulonglong)*(uint *)(&DAT_180016150 + (ulonglong)(byte)(&DAT_180016164)[bVar2] * 4) +
            0x180000000))
            ((code *)((ulonglong)
                      *(uint *)(&DAT_180016150 + (ulonglong)(byte)(&DAT_180016164)[bVar2] * 4) +
                     0x180000000));
  return;
}



/* ========================================================================
   ENTRY: 180016185
   NAME : memcpy
   SIG  : void * __cdecl memcpy(void * _Dst, void * _Src, size_t _Size)
   ======================================================================== */

void * __cdecl memcpy(void *_Dst,void *_Src,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180016185. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memcpy(_Dst,_Src,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 18001618b
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018001618b. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180016191
   NAME : ceil
   SIG  : double __cdecl ceil(double _X)
   ======================================================================== */

double __cdecl ceil(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180016191. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = ceil(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 180016197
   NAME : cos
   SIG  : double __cdecl cos(double _X)
   ======================================================================== */

double __cdecl cos(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180016197. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = cos(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 18001619d
   NAME : floor
   SIG  : double __cdecl floor(double _X)
   ======================================================================== */

double __cdecl floor(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00018001619d. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = floor(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 1800161a3
   NAME : pow
   SIG  : double __cdecl pow(double _X, double _Y)
   ======================================================================== */

double __cdecl pow(double _X,double _Y)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800161a3. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = pow(_X,_Y);
  return dVar1;
}



/* ========================================================================
   ENTRY: 1800161a9
   NAME : sin
   SIG  : double __cdecl sin(double _X)
   ======================================================================== */

double __cdecl sin(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800161a9. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sin(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 1800161af
   NAME : sqrt
   SIG  : double __cdecl sqrt(double _X)
   ======================================================================== */

double __cdecl sqrt(double _X)

{
  double dVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800161af. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  dVar1 = sqrt(_X);
  return dVar1;
}



/* ========================================================================
   ENTRY: 1800161d0
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x0001800161d0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 1800161f0
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x0001800161f0 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x0001800161d0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 180016200
   NAME : FUN_180016200
   SIG  : undefined __fastcall FUN_180016200(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_180016200(undefined8 param_1,longlong param_2)

{
  FUN_180015b50(*(char *)(param_2 + 0x60));
  return;
}



/* ========================================================================
   ENTRY: 180016220
   NAME : FUN_180016220
   SIG  : undefined __fastcall FUN_180016220(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_180016220(undefined8 param_1,longlong param_2)

{
  FUN_180015b50(*(char *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 18001623a
   NAME : FUN_18001623a
   SIG  : undefined __fastcall FUN_18001623a(void)
   ======================================================================== */

void FUN_18001623a(void)

{
  FUN_180015a50();
  return;
}



/* ========================================================================
   ENTRY: 180016250
   NAME : FUN_180016250
   SIG  : undefined __fastcall FUN_180016250(undefined8 * param_1, longlong param_2)
   ======================================================================== */

void FUN_180016250(undefined8 *param_1,longlong param_2)

{
  FUN_1800159c0(*(undefined8 *)(param_2 + 0x60),*(int *)(param_2 + 0x68),
                *(undefined8 *)(param_2 + 0x70),FUN_180015390,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180016290
   NAME : FUN_180016290
   SIG  : bool __fastcall FUN_180016290(undefined8 * param_1)
   ======================================================================== */

bool FUN_180016290(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}


