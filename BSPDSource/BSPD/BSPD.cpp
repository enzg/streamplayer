// BSPD.cpp: 定义 DLL 应用程序的导出函数。
//

#include "BSPD.h"
#include <Windows.h>
#include <time.h>

_DLLEXPORT BSPDContext* BSPDCreateCtx(){
	BSPDContext *ctx = NULL;
	ctx = (BSPDContext*)malloc(sizeof(BSPDContext));
	if (ctx == NULL)
	{
		return nullptr;
	}
	memset(ctx, 0, sizeof(BSPDContext));
	ctx->pCoder = (BSPDCoder*)malloc(sizeof(BSPDContext));
	memset(ctx->pCoder, 0, sizeof(BSPDContext));
	ctx->logCallback = NULL;
	ctx->pCoder->LOGLEVEL = BSPD_LOG_ERROR;
	return ctx;
}

_DLLEXPORT int  BSPDOpen(BSPDContext *ctx, char * input, char * options)
{
	int flags = BSPD_OP_OK;

	if (ctx == NULL)
	{
		return BSPD_USE_NULL_ERROR;
	}

	size_t slen = strlen(input);

	if (input == NULL && slen <1)
	{
		bc_log(ctx,BSPD_LOG_ERROR, "use error input \n");
		return ctx->opcode = BSPD_INPUT_ERROR;
	}

	bc_log(ctx,BSPD_LOG_DEBUG, "BSPDOpen input:%S \n",input);

	ctx->inputPath = (char*)malloc(slen + 1);
	if (ctx->inputPath == NULL)
	{
		bc_log(ctx, BSPD_LOG_ERROR, "no enough memory alloc ctx->inputpath \n");
		return ctx->opcode = BSPD_NO_MEMY;
	}

	ctx->inputPath[slen]= '\0';
	memcpy(ctx->inputPath, input, slen);

    if (options)
    {
        slen = strlen(options);
        if (slen > 0)
        {
            ctx->options = (char*)malloc(slen + 1);
            if (ctx->options == NULL)
            {
                bc_log(ctx, BSPD_LOG_ERROR, "no enough memory alloc ctx->options \n");
                return ctx->opcode = BSPD_NO_MEMY;
            }
            ctx->options[slen] = '\0';
            memcpy(ctx->options, options, slen);
            flags |= bc_parse_options(ctx);
        }
        else
        {
            flags |= bc_set_default_options(ctx);
        }
    }

	flags |= bc_init_coder(ctx);

	return flags;
}

_DLLEXPORT int BSPDGetYUV(BSPDContext *bspdctx,char *ydata,char *udata,char *vdata)
{
    if (bspdctx == NULL)
    {
        return BSPD_USE_NULL_ERROR;
    }

    if (ydata == NULL || udata == NULL || vdata == NULL)
    {
        bc_log(bspdctx, BSPD_LOG_ERROR, "y u v data maybe null\n ");
        return BSPD_USE_NULL_ERROR;
    }

    if (bc_get_yuv(bspdctx) == BSPD_OP_OK)
    {

        memcpy(ydata, bspdctx->pCoder->pFrameYUV->data[0], bspdctx->ysize);
        memcpy(udata, bspdctx->pCoder->pFrameYUV->data[1], bspdctx->ysize / 4);
        memcpy(vdata, bspdctx->pCoder->pFrameYUV->data[2], bspdctx->ysize / 4);
        return BSPD_OP_OK;
    }
    return BSPD_ERRO_UNDEFINE;
}

_DLLEXPORT int BSPDClose(BSPDContext * bspdctx)
{
	bc_log(bspdctx, BSPD_LOG_DEBUG, "BSPDClose\n");
    bc_close(bspdctx);
	return BSPD_ERRO_UNDEFINE;
}


_DLLEXPORT int BSPDSetLogCallback(BSPDContext * bspdctx,BSPDLogCallback call)
{
	if (bspdctx == NULL||bspdctx->pCoder == NULL)
	{
		return BSPD_USE_NULL_ERROR;
	}
	bspdctx->logCallback = call;
	return 0;
}
